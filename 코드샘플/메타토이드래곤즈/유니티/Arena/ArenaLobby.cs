using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace SandboxNetwork
{
    public class ArenaLobby : MonoBehaviour
    {
        [SerializeField]
        private ArenaLobbyMainInfoController ArenaLobbyMainInfoController;
        [SerializeField]
        private ArenaLobbyBattleInfoController ArenaLobbyBattleInfoController;

        private void Start()
        {
            if (ArenaLobbyMainInfoController == null) return;
            if (ArenaLobbyBattleInfoController == null) return;
            PopupTopUIRefreshEvent.Hide();
            UIManager.Instance.InitUI(eUIType.Arena);
            UIManager.Instance.RefreshUI(eUIType.Arena);
            ArenaLobbyMainInfoController.Init();
            ArenaLobbyBattleInfoController.Init();
            //PopupManager.OpenPopup<ArenaRankChangePopup>(new ArenaRankChangePopupData(1900, 2001)).Init();



            ArenaManager.Instance.SetRefreshUICallback(() =>
            {
                RefreshUI();
            });
        }


        private void OnEnable()
        {
            CheckStart();
        }

        void CheckStart()
        {
            if (ArenaManager.Instance.IsRankUp())
            {
                CheckRankUp(CheckStart);
                ArenaManager.Instance.UserArenaData.LastUserGrade = ArenaManager.Instance.UserArenaData.SeasonGrade;
                return;
            }

            ArenaManager.Instance.UserArenaData.LastUserGrade = ArenaManager.Instance.UserArenaData.SeasonGrade;

            int last_season = CacheUserData.GetInt("last_season", 0);
            if (ArenaManager.Instance.UserArenaData.season_id != last_season && ArenaManager.Instance.UserArenaData.season_type != ArenaBaseData.SeasonType.PreSeason)
            {
                PopupManager.OpenPopup<ArenaSeasonOpeningPopup>().SetExitCallback(CheckStart);
                CacheUserData.SetInt("last_season", ArenaManager.Instance.UserArenaData.season_id);
                return;
            }

            if (IsArenaDefDeckEmpty())
            {
                ScenarioManager.Instance.OnEventCheckFirstArena(() => TutorialCheck());
            }
            else
            {
                TutorialCheck();
            }
        }

        void TutorialCheck()
        {
            if(TutorialManager.tutorialManagement.IsFinishedTutorial(TutorialDefine.Arena) ==false)
            {
                TutorialManager.tutorialManagement.SetButtonObjectDic((int)TutorialDefine.Arena, 22, () =>
                {
                    ArenaLobbyBattleInfoController.OnClickTabButton(3);
                });
                TutorialManager.tutorialManagement.SetButtonObjectDic((int)TutorialDefine.Arena, 27, () =>
                {
                    ArenaLobbyBattleInfoController.OnClickTabButton(4);
                });
                TutorialManager.tutorialManagement.SetButtonObjectDic((int)TutorialDefine.Arena, 30, () =>
                {
                    ArenaLobbyBattleInfoController.OnClickTabButton(1);
                });

                if (TutorialManager.tutorialManagement.IsPlayingTutorialByGroup(TutorialDefine.Arena) ==false && IsArenaDefDeckEmpty())
                    TutorialManager.tutorialManagement.StartTutorial((int)TutorialDefine.Arena);
                else
                    TutorialManager.tutorialManagement.NextTutorialStart();

                TutorialManager.tutorialManagement.SetCurrentTutorialEndCallBack(()=> { // 부정행위 예외처리
                    if (IsArenaDefDeckEmpty())
                        DeckSettingNotification();
                });
            }
            else // 부정행위 예외처리
            {
                if (IsArenaDefDeckEmpty())
                    DeckSettingNotification(); 
            }
        }
        public void CheckRankUp(Action cb)
        {
            var lastUserGrade = ArenaManager.Instance.UserArenaData.LastUserGrade;
            var currentGrade = ArenaManager.Instance.UserArenaData.SeasonGrade;
            var popup = PopupManager.OpenPopup<ArenaRankChangePopup>(new ArenaRankChangePopupData((int)currentGrade, (int)lastUserGrade));
            popup.Init();
            popup.SetExitCallback(cb);
        }

        void DeckSettingNotification()
        {
            Debug.Log("방어팀 설정 필요");

            SystemPopup.OnSystemPopup(StringData.GetStringByStrKey("알림"), StringData.GetStringByStrKey("방어팀드래곤필요"), () =>
            {
                ArenaManager.Instance.SetArenaTeamModeData(false);
                LoadingManager.Instance.EffectiveSceneLoad("ArenaTeamSetting", eSceneEffectType.CloudAnimation);
            },
            () =>
            {
                LoadingManager.Instance.EffectiveSceneLoad("Town", eSceneEffectType.CloudAnimation, UIManager.RefreshUICoroutine(eUIType.Town));
            },
            () =>
            {
                LoadingManager.Instance.EffectiveSceneLoad("Town", eSceneEffectType.CloudAnimation, UIManager.RefreshUICoroutine(eUIType.Town));
            }, true, true, false);
        }


        public void RefreshUI()
        {
            if (ArenaLobbyMainInfoController != null)
            {
                ArenaLobbyMainInfoController.RefreshMainInfoLayer();
            }
            if (ArenaLobbyBattleInfoController != null)
            {
                ArenaLobbyBattleInfoController.Refresh();
            }
            UIManager.Instance.RefreshUI(eUIType.Arena);
        }

        public void OnClickBackToTown()
        {
            //LoadingManager.ImmediatelySceneLoad("Town", true, eUIType.Town);
            LoadingManager.Instance.EffectiveSceneLoad("Town", eSceneEffectType.CloudAnimation, UIManager.RefreshUICoroutine(eUIType.Town));
        }
        public void OnClickMockBattle()
        {
            ArenaManager.Instance.SendArenaTest();
        }
        private void OnDisable()
        {
            ArenaManager.Instance.ClearTimeObject();
        }

        bool IsArenaDefDeckEmpty()
        {
            if(User.Instance.PrefData.ArenaFormationData.TeamFormationDEF == null || User.Instance.PrefData.ArenaFormationData.TeamFormationDEF.Count <= 0)
            {
                return true;
            }

            var arenaFirstpreset = User.Instance.PrefData.ArenaFormationData.TeamFormationDEF[0];
            if(arenaFirstpreset == null || arenaFirstpreset.Count <= 0)
            {
                return true;
            }

            var arenaFirstpresetCount = arenaFirstpreset.Count;
            var emptyCheckIndex = 0;
            var checkValue = 0;
            for (var i = 0; i < arenaFirstpresetCount; i++)
            {
                var arenaValue = arenaFirstpreset[i];

                if(checkValue == arenaValue)
                {
                    emptyCheckIndex++;
                }
            }

            return emptyCheckIndex == arenaFirstpresetCount;
        }
        
        public void OnClickInfoIcon()
        {
            PopupManager.OpenPopup<ArenaInfoPopup>();
        }
    }
}