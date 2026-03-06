using Google.Impl;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using static SQLite4Unity3d.SQLite3;
namespace SandboxNetwork
{
    public class ChampionBattleLobby : MonoBehaviour
    {
        [SerializeField]
        private ChampionBattleLobbyMainInfoController ChampionBattleLobbyMainInfoController;
        [SerializeField]
        ChampionLeagueTable LeagueTable;

        [Header("LEFT LAYER")]
        [SerializeField]
        private ChampionTabController ChampionTabController = null;
        [SerializeField]
        private GameObject normalLayer;
        [SerializeField]
        private GameObject playerLayer;

        private ChampionInfo CurChampInfo { get { return ChampionManager.Instance.CurChampionInfo; } }
        private void Start()
        {
            if (ChampionBattleLobbyMainInfoController == null) 
                return;

            PopupTopUIRefreshEvent.Hide();
            UIManager.Instance.InitUI(eUIType.ChampionBattle);
            UIManager.Instance.RefreshUI(eUIType.ChampionBattle);
            ChampionBattleLobbyMainInfoController.Init();
            
            ChampionManager.Instance.SetRefreshUICallback(() =>
            {
                RefreshUI();
            });

            init();
        }

        public void init()
        {
            //TODO 챔피언대전 
            //좌측 UI 세팅
            //참가자이면 팀세팅 UI, 관전자이면 응원하기 관련 UI를 보여주자

            ChampionTabController.Init(CurChampInfo.AmIParticipant, CurChampInfo.CurState >= ChampionInfo.ROUND_STATE.ROUND_OF_16);

            CheckTeamSetting();
        }

        private void CheckTeamSetting()
        {
            if (!CurChampInfo.AmIParticipant)
                return;
            if (CurChampInfo.CurState < ChampionInfo.ROUND_STATE.ROUND_OF_16)
                return;

            
            int remain = 0;
            remain = TimeManager.GetTimeCompare(ChampionManager.Instance.CurChampionInfo.GetContentsTime(ChampionInfo.CONTENTS_TIME.DEF_TEAM_SET));
            if (remain > 0)
            {
                var attack_team = CurChampInfo.MyInfo.GetChampionBattleFomation(ParticipantData.eTournamentTeamType.DEFFENCE);
                if (attack_team.DeckCount <= 0)
                {
                    SystemPopup.OnSystemPopup(StringData.GetStringByStrKey("알림"), StringData.GetStringFormatByStrKey("팀세팅이동경고", StringData.GetStringByStrKey("방어팀")), () =>
                    {
                        LoadingManager.Instance.EffectiveSceneLoad("ChampionBattleSetting", eSceneEffectType.CloudAnimation);
                    },
                    () => { });
                    return;
                }
            }

            remain = TimeManager.GetTimeCompare(ChampionManager.Instance.CurChampionInfo.GetContentsTime(ChampionInfo.CONTENTS_TIME.ATK_TEAM_SET));
            if(remain > 0)
            {
                var attack_team = CurChampInfo.MyInfo.GetChampionBattleFomation(ParticipantData.eTournamentTeamType.ATTACK);
                if (attack_team.DeckCount <= 0)
                {
                    SystemPopup.OnSystemPopup(StringData.GetStringByStrKey("경고"), StringData.GetStringFormatByStrKey("팀세팅이동경고", StringData.GetStringByStrKey("공격팀")), () =>
                    {
                        LoadingManager.Instance.EffectiveSceneLoad("ChampionBattleSetting", eSceneEffectType.CloudAnimation);
                    },
                    () => { });
                    return;
                }
            }

            remain = TimeManager.GetTimeCompare(ChampionManager.Instance.CurChampionInfo.GetContentsTime(ChampionInfo.CONTENTS_TIME.HIDDEN_TEAM_SET));
            if (remain > 0)
            {
                var attack_team = CurChampInfo.MyInfo.GetChampionBattleFomation(ParticipantData.eTournamentTeamType.HIDDEN);
                if (attack_team.DeckCount <= 0)
                {
                    SystemPopup.OnSystemPopup(StringData.GetStringByStrKey("알림"), StringData.GetStringFormatByStrKey("팀세팅이동경고", StringData.GetStringByStrKey("히든팀")), () =>
                    {
                        LoadingManager.Instance.EffectiveSceneLoad("ChampionBattleSetting", eSceneEffectType.CloudAnimation);
                    },
                    () => { });
                    return;
                }
            }
        }


        private void OnEnable()
        {
            CheckStart();
        }

        void CheckStart()
        {

        }

        public void RefreshUI()
        {
            UIManager.Instance.RefreshUI(eUIType.ChampionBattle);
        }

        public void OnClickBackToTown()
        {
            //LoadingManager.ImmediatelySceneLoad("Town", true, eUIType.Town);
            LoadingManager.Instance.EffectiveSceneLoad("Town", eSceneEffectType.CloudAnimation, UIManager.RefreshUICoroutine(eUIType.Town));
        }
        public void OnClickMockBattle()
        {
            //ChampionManager.Instance.SendArenaTest();
        }

        public void OnClickReplayBattle()
        {
            //ChampionManager.Instance.SendArenaTest();
        }

        public void OnClickJoinBtn()
        {
            //string title = StringData.GetStringByStrKey("");
            //string contentGuide = StringData.GetStringByStrKey("");
            SystemPopup.OnSystemPopup(StringData.GetStringByStrKey("알림"), StringData.GetStringByStrKey("챔피언텍스트1"), StringData.GetStringByStrKey("확인"), StringData.GetStringByStrKey("취소"),
                () => { },
                () => { }
                );
        }
        public void OnClickJoinFailBtn()
        {
            //string title = StringData.GetStringByStrKey("");
            //string contentGuide = StringData.GetStringByStrKey("");
            SystemPopup.OnSystemPopup(StringData.GetStringByStrKey("알림"), StringData.GetStringByStrKey("챔피언텍스트2"), StringData.GetStringByStrKey("확인"), "",
                () => { }
                );
        }
        public void OnClickAlreadyJoinBtn()
        {
            //string title = StringData.GetStringByStrKey("");
            //string contentGuide = StringData.GetStringByStrKey("");
            SystemPopup.OnSystemPopup(StringData.GetStringByStrKey("알림"), StringData.GetStringByStrKey("챔피언텍스트3"), StringData.GetStringByStrKey("확인"), "",
                () => { }
                );
        }

        public void OnClickPracticeModeBtn()
        {
            //TODO 제공된 연습전투횟수를 초과하면 젬스톤으로 진행가능


            //string title = StringData.GetStringByStrKey("");
            //string contentGuide = StringData.GetStringByStrKey("");
            //int price = 480;

            ePriceDataFlag priceFlag = ePriceDataFlag.CloseBtn | ePriceDataFlag.ContentBG | ePriceDataFlag.GemStone;
            PricePopup.OpenPopup(StringData.GetStringByStrKey("알림"), "", StringData.GetStringByStrKey("챔피언텍스트4"), 480, priceFlag,
            () =>
            {
                //TODO 젬스톤사용으로 연습경기진행
                Debug.Log("연습모드 버튼 클릭");
            });

        }

        public void OnClickPracticeModeBtn2()
        {
            //TODO 제공된 연습전투
            int useCnt = 3;
            int givenCnt = 3;

            SystemPopup.OnSystemPopup(StringData.GetStringByStrKey("알림"), StringData.GetStringFormatByStrKey("챔피언텍스트5", useCnt, givenCnt), StringData.GetStringByStrKey("확인"), "",
            () =>
            {

            });

        }

        public void OnClickBetBtn()
        {
            //TODO 응원하기
            
        }

        private void OnDisable()
        {

        }
        public void OnClickSelectDragonBtn()
        {
            ChampionBattleDragonSelectPopup.OpenPopup(0);
        }
        public void OnClickTeamSetting()
        {
            //if (CurChampInfo.CurState < ChampionInfo.ROUND_STATE.ROUND_OF_16)
            //{
            //    SystemPopup.OnSystemPopup(StringData.GetStringByStrKey("안내"), StringData.GetStringByStrKey("팀설정이용불가안내"));
            //}
            //else
            {
                LoadingManager.Instance.EffectiveSceneLoad("ChampionBattleSetting", eSceneEffectType.CloudAnimation);
            }
        }

        public void OnStatisticPopup()
        {
            //PopupManager.OpenPopup<ChampionBattleStatisticPopup>();
        }

        public void ShowWinnerPopup()
        {
            ChampionManager.Instance.CurChampionInfo.ReqHallOfFame(() =>
            {
                PopupManager.OpenPopup<ChampionWinnerPopup>();
            });
        }

        public void GotoShop()
        {
            if (!User.Instance.ENABLE_P2E)
            {
                SystemPopup.OnSystemPopup(StringData.GetStringByStrKey("안내"), "응원봉상점이용불가안내");
            }

            DAppManager.Instance.OpenDAppChampionPage();
        }
        public void ShowBetResultPopup()
        {
            if (CurChampInfo.CurState < ChampionInfo.ROUND_STATE.ROUND_OF_16)
            {
                SystemPopup.OnSystemPopup(StringData.GetStringByStrKey("안내"), StringData.GetStringByStrKey("배팅로그이용불가안내"));
            }
            else
            {
                ChampionBetResultPopup.OpenPopup();
            }
            
        }

        public void ShowSurpportPopup()
        {
            if (CurChampInfo.CurState < ChampionInfo.ROUND_STATE.ROUND_OF_16)
            {
                SystemPopup.OnSystemPopup(StringData.GetStringByStrKey("안내"), StringData.GetStringByStrKey("배팅로그이용불가안내"));
            }
            else
            {
                ChampionSurpportPopup.OpenPopup(ChampionSurpportInfo.eSurpportType.NONE);
            }

        }
    }

}