using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class WorldBossLobby : MonoBehaviour, EventListener<DragonChangedEvent> 
    {
        [Header("enterButton")]
        [SerializeField] Button battleEnterBtn = null;
        
        [SerializeField] Text battleAbleCntText = null;
        [SerializeField] GameObject battleAdIcon = null;
        [SerializeField] Text advTimerText1 = null;
        [SerializeField] TimeObject advTimeObj = null;

        [Header("TopLayer")]
        [SerializeField] GameObject enterCntObj = null;
        [SerializeField] Text availableEnterCntText = null;
        [SerializeField] GameObject advIcon = null;
        [SerializeField] Text advTimerText2 = null;

        [SerializeField] Text totalBattlePointText = null;

        
        [SerializeField] List<WorldBossDeckInfoSlot> deckList = new List<WorldBossDeckInfoSlot>();

        [Header("setVisible Node")]
        [SerializeField] GameObject dragonTeamNode = null;      //덱 팀세팅 노드 (보스 목록에서 선택시 켜짐)
        [SerializeField] WorldBossLobbyBossPortraitSlot bossPortrait = null;
        [SerializeField] WorldBossLobbyBossDetailInfo bossDescNode = null;        //보스 상세 설명 노드 (보상 , 공략법 등)
        [SerializeField] Button bossSelectButton = null;
        [SerializeField] GameObject teamSettingBtnNode = null;

        [SerializeField] private Text worldBossTimeLabel = null;

        [SerializeField] Button rankingButton = null;

        [Header("mapViewCheat")]
        [SerializeField] GameObject cheatMapviewObject = null;

        private TimeObject worldBossTimeObject = null;
        private int freeEnterCnt { get { return WorldBossManager.Instance.WorldBossEnterCount; } }
        private int advEnterCnt { get { return AdvertisementData.Get(SBDefine.AD_RAID_BOSS_KEY).LIMIT; } }
        private int curTicketCnt { get { return WorldBossManager.Instance.WorldBossProgressData.WorldBossPlayCount; } }

        bool isAdvWaitEnd = true;

        bool isTeamSettingVisibleFlag = false;

        private void OnEnable()
        {
            EventManager.AddListener(this);
        }
        private void OnDisable()
        {
            EventManager.RemoveListener(this);
        }

        void Start()
        {
            PopupTopUIRefreshEvent.Hide();
            UIManager.Instance.InitUI(eUIType.WorldBoss);
            UIManager.Instance.RefreshUI(eUIType.WorldBoss);
            UIManager.Instance.MainUI.SetTownButtonCallBack(() => {

                if (isTeamSettingVisibleFlag)
                {
                    SetWorldBossListData(true);
                    return;
                }

                WorldBossManager.Instance.UISelectBossKey = -1;

                LoadingManager.Instance.EffectiveSceneLoad("Town", eSceneEffectType.BlackBackground, UIManager.RefreshUICoroutine(eUIType.Town));
            });

            if (worldBossTimeLabel != null)
            {
                worldBossTimeObject = worldBossTimeLabel.GetComponent<TimeObject>();
                if (worldBossTimeObject == null)
                    worldBossTimeObject = worldBossTimeLabel.gameObject.AddComponent<TimeEnable>();

                if (worldBossTimeObject != null)
                    worldBossTimeObject.Refresh = RefreshRemainTime;
            }

#if UNITY_EDITOR
            if (cheatMapviewObject != null)
                cheatMapviewObject.SetActive(true);
#else
            if (cheatMapviewObject != null)
                cheatMapviewObject.SetActive(false);
#endif
            SetDeckList();
            SetAdvWaitTime();
            SetDungeonInfo();
            SetBattlePoint();
            SetBossList();
        }
        private void RefreshRemainTime()
        {
            if (worldBossTimeLabel == null)
                return;

            int time = TimeManager.GetContentResetTime();
            var timeStr = SBFunc.TimeString((int)time);
            worldBossTimeLabel.text = StringData.GetStringFormatByStrKey("남음", timeStr);
            //LayoutRebuilder.ForceRebuildLayoutImmediate(worldBossTimeLabel);
        }
        //변경 전투력 계산
        void SetBattlePoint(bool _forceRefresh = false)
        {
            int totalBp = 0;
            WorldBossFormationData curData = User.Instance.PrefData.WorldBossFormationData;

            if (curData.HasFormation())
            {
                foreach(var list in curData.TeamFormation)
                {
                    if (list == null || list.Count <= 0)
                        continue;

                    foreach(var dragonID in list)
                    {
                        if (dragonID <= 0)
                            continue;

                        var dragonData = User.Instance.DragonData.GetDragon(dragonID);
                        if (dragonData == null)
                            continue;

                        if(_forceRefresh)
                            dragonData.RefreshALLStatus();
                        totalBp += dragonData.GetTotalINF();
                    }
                }
            }

            if (totalBattlePointText != null)
                totalBattlePointText.text = SBFunc.CommaFromNumber(totalBp);
        }

        public void SetBossList()
        {
            var isSelected = WorldBossManager.Instance.UISelectBossKey < 0;
            SetWorldBossListData(isSelected);//선택이 됐으면 false;
        }
        
        /// <summary>
        /// worldBossManager에서 덱 세팅 가져오는 것 연결하기
        /// </summary>
        public void SetDeckList()
        {
            if (deckList == null || deckList.Count <= 0)
                return;

            SetSlotInfo();
        }

        void SetSlotInfo()
        {
            for(int i = 0; i< deckList.Count; i++)
            {
                var slotData = deckList[i];
                if (slotData == null)
                    continue;

                slotData.InitDeckSlot(i);

                var deckInfo = User.Instance.PrefData.GetWorldBossFormation(i);

                slotData.SetDataSlot(i, deckInfo);
            }
        }
        /// <summary>
        /// true 면 보스리스트 데이터 세팅 & 자동 선택된 보스 설명
        /// false 면 팀세팅 노드
        /// </summary>
        /// <param name="_isBossList"></param>
        void SetWorldBossListData(bool _isBossList)
        {
            if (dragonTeamNode != null)
                dragonTeamNode.SetActive(!_isBossList);
            if (rankingButton != null)
                rankingButton.gameObject.SetActive(_isBossList);
            if (bossPortrait != null)
                bossPortrait.gameObject.SetActive(!_isBossList);
            if(battleEnterBtn != null)
                battleEnterBtn.gameObject.SetActive(!_isBossList);
            if (teamSettingBtnNode != null)
                teamSettingBtnNode.SetActive(!_isBossList);
            if (bossDescNode != null)
                bossDescNode.gameObject.SetActive(_isBossList);
            if(bossSelectButton != null)
            {
                bossSelectButton.gameObject.SetActive(_isBossList);
                if (_isBossList)
                    bossSelectButton.SetButtonSpriteState(IsBossEnterCondition());//보스 입장 가능 컨디션
            }

            if (_isBossList)//보스 상세정보 세팅 , 보스 스크롤 갱신
                RefreshBossDescNode();
            else// 보스 초상화 갱신
            {
                if (bossPortrait != null)
                    bossPortrait.SetData();
            }

            isTeamSettingVisibleFlag = !_isBossList;
        }

        void RefreshBossDescNode()
        {
            var todayBossDic = WorldBossManager.Instance.WorldBossProgressData.TodayBossStageDataInfo;
            if (todayBossDic == null || todayBossDic.Count <= 0)
                return;

            var bossList = todayBossDic.Values.ToList();
            var curSelectKey = WorldBossManager.Instance.UISelectBossKey;
            WorldBossStageDataInfo targetInfo = null;
            if(curSelectKey < 0)//선택 안한 상태면 첫번째 보스 자동
            {
                targetInfo = bossList[0];
                WorldBossManager.Instance.UISelectBossKey = targetInfo.RaidBossKey;
            }
            else
            {
                targetInfo = bossList.Find(element => element.RaidBossKey == curSelectKey);
            }

            if (bossDescNode != null && targetInfo != null)
                bossDescNode.SetData(targetInfo);
        }

        public void OnClickWorldBossSlot(int _monsterKey)
        {
            WorldBossManager.Instance.UISelectBossKey = _monsterKey;//현재 요청한 monsterKey 세팅
            SetWorldBossListData(true);
        }

        public void OnClickBossSelect()
        {
            if (!IsBossEnterCondition())
            {
                SystemPopup.OnSystemPopup(StringData.GetStringByStrKey("알림"), StringData.GetStringByStrKey("보스레이드_입장불가_배틀캐슬"), true, false, false);
                return;
            }

            if (WorldBossManager.Instance.UISelectBossKey < 0)
                return;

            SetWorldBossListData(false);
        }

        public void OnClickArrow(bool _isRight)
        {
            var todayBossDic = WorldBossManager.Instance.WorldBossProgressData.TodayBossStageDataInfo;
            if (todayBossDic == null || todayBossDic.Count <= 0)
                return;

            var bossList = todayBossDic.Values.ToList();
            var bossListCount = todayBossDic.Count;

            var currentKey = WorldBossManager.Instance.UISelectBossKey;
            if (currentKey < 0)
                return;

            var currentIndex = bossList.FindIndex(element => element.RaidBossKey == currentKey);
            if (currentIndex < 0)
                return;

            var targetIndex = 0;
            if(_isRight)
            {
                targetIndex = currentIndex + 1;
                if (targetIndex >= bossListCount)
                    targetIndex = 0;
            }
            else
            {
                targetIndex = currentIndex - 1;
                if (targetIndex < 0)
                    targetIndex = bossListCount - 1;
            }

            if (targetIndex == currentIndex)
                return;

            WorldBossManager.Instance.UISelectBossKey = bossList[targetIndex].RaidBossKey;
            SetWorldBossListData(true);
        }

        /// <summary>
        /// 현재 구성된 덱 전체 삭제 - 전체 해제 기능 빼버림 (빈 덱상태를 서버에 보내면 안됨.)
        /// </summary>
        //public void OnClickAllReleaseDeck()
        //{
        //    ToastManager.On("임시 - 서버에 드래곤 리스트 전체 0 던지면 안되서 일단 기능 막음");
        //    return;
        //    SystemPopup.OnSystemPopup(StringData.GetStringByStrKey("알림"), StringData.GetStringByStrKey("월드보스전체해제알림문구"), () =>
        //    {
        //        RequestAllReleaseDeck();
        //    },
        //    () =>
        //    {
        //    },
        //    () =>
        //    {
        //    }, true, true, false);
        //}

        /// <summary>
        /// 덱 전체 해제 하는 api 따로 만들어야함.
        /// </summary>
        //void RequestAllReleaseDeck()
        //{
        //    var getTeamParam = User.Instance.PrefData.WorldBossFormationData.GetWorldBossEmptyFormation();

        //    WWWForm param = new WWWForm();
        //    param.AddField("teams", JsonConvert.SerializeObject(getTeamParam));
        //    NetworkManager.Send("raid/setteams", param, (JObject jsonData) =>
        //    {
        //        if (jsonData["err"] != null && (int)jsonData["err"] == 0 && (int)jsonData["rs"] == 0)
        //        {
        //            User.Instance.PrefData.WorldBossFormationData.AllClearFormationData(false);
        //            SetDeckList();
        //            SetBattlePoint();
        //            ToastManager.On(StringData.GetStringByStrKey("덱전체해제알림"));
        //        }
        //        else if (jsonData["err"] != null)
        //        {
        //            var errorValue = (eApiResCode)((int)jsonData["err"]);
        //            switch (errorValue)
        //            {
        //                case eApiResCode.DATA_ERROR:
        //                    ToastManager.On(StringData.GetStringByStrKey("서버요청실패"));
        //                    break;
        //            }
        //        }
        //    }, (string arg) =>
        //    {

        //    });
        //}

        public void OnClickAutoSuggestion()
        {
            SystemPopup.OnSystemPopup(StringData.GetStringByStrKey(("battle_popup_team_recommend")), StringData.GetStringByStrKey("boss_raid_auto_party"),
                () => { RequestSuggetionDeckProcess(); }, () => { }, () => { });
        }
        /// <summary>
        /// 덱 전체 대상 추천조합 세팅
        /// 0번 인덱스는 공1 덱
        /// 1번 인덱스는 공2 덱
        /// 2번 인덱스는 방1 덱
        /// 3번 인덱스는 방2 덱
        /// </summary>
        void RequestSuggetionDeckProcess()
        {
            var autoLogic = new WorldBossAutoDeckLogic();
            var suggestList = autoLogic.GetTotalAutoMergeNewVersion();

            //totalDeck setting
            for (int i = 0; i < deckList.Count; i++)
            {
                var emptyList = new List<int>() { 0, 0, 0, 0, 0, 0 };
                if(suggestList.Count <= i)
                {
                    suggestList.Add(emptyList);
                    continue;
                }

                var suggestIDList = suggestList[i];
                if (suggestIDList.Count < 6)
                {
                    var remainCount = 6 - suggestIDList.Count;
                    if (remainCount > 0)
                    {
                        for (int k = 0; k < remainCount; k++)
                            suggestIDList.Add(0);
                    }
                }
            }

            //각 결과 리스트에서 변경점만 서버에 요청 & 변경점에서 빈덱이 포함되어있을 경우 로직 안타게 추가
            //ex) 기존 1마리 이상 포함이었지만, 자동 배치로직으로 인해 덱이 빈상태면 저장 자체를 안하게.
            Dictionary<int, List<int>> tempDiffDic = new Dictionary<int, List<int>>();
            for (int i = 0; i < suggestList.Count; i++)
            {
                var currentDeckData = User.Instance.PrefData.WorldBossFormationData.GetFormationData(i);
                var suggestDeck = suggestList[i];

                var serverDeckEmpty = User.Instance.PrefData.WorldBossFormationData.IsEmpty(currentDeckData);
                var suggestDeckEmpty = User.Instance.PrefData.WorldBossFormationData.IsEmpty(suggestDeck);

                if (serverDeckEmpty && suggestDeckEmpty)//둘다 비어있으면 고려안함.
                    continue;

                if (!serverDeckEmpty && suggestDeckEmpty)//비어있지 않았는데, 비어있게 되면 저장 안하게.
                {
                    ToastManager.On(StringData.GetStringByStrKey("boss_raid_empty_party"));
                    return;
                }

                if(tempDiffDic.ContainsKey(i))
                {
                    Debug.LogError("suggest data already contain");//있을 수 없는 일
                    continue;
                }

                tempDiffDic.Add(i, suggestDeck.ToList());
            }

            var getTeamParam = User.Instance.PrefData.WorldBossFormationData.GetWorldBossFormation(tempDiffDic);
            if (getTeamParam == null)
                return;
            WWWForm param = new WWWForm();
            param.AddField("teams", JsonConvert.SerializeObject(getTeamParam));
            NetworkManager.Send("raid/setteams", param, (JObject jsonData) =>
            {
                if (jsonData["err"] != null && (int)jsonData["err"] == 0 && (int)jsonData["rs"] == 0)
                {
                    User.Instance.PrefData.WorldBossFormationData.SetAutoFormationData(tempDiffDic);
                    SetDeckList();
                    SetBattlePoint();
                    ToastManager.On(StringData.GetStringByStrKey("추천덱등록알림"));
                }
                else if (jsonData["err"] != null)
                {
                    var errorValue = (eApiResCode)((int)jsonData["err"]);
                    switch (errorValue)
                    {
                        case eApiResCode.DATA_ERROR:
                            ToastManager.On(StringData.GetStringByStrKey("서버요청실패"));
                            break;
                    }
                }
            }, (string arg) =>
            {

            });

        }

        void SetAdvWaitTime()
        {
            if (advTimeObj.Refresh == null)
            {
                var lastestDate = ShopManager.Instance.GetAdvertiseState(SBDefine.AD_RAID_BOSS_KEY).LAST_VIEWDATE;
                var term = AdvertisementData.Get(SBDefine.AD_RAID_BOSS_KEY).TERM;
                int advWaitEndTime = TimeManager.GetTimeStamp(lastestDate) + term;
                if (TimeManager.GetTimeCompare(advWaitEndTime) <= 0)
                {
                    isAdvWaitEnd = true;
                    advTimeObj.Refresh = null;
                    advTimerText1.text = advTimerText2.text = string.Empty;
                }
                else
                {
                    isAdvWaitEnd = false;
                    advTimeObj.Refresh = () =>
                    {
                        advTimerText1.text = advTimerText2.text = SBFunc.TimeStringMinute(TimeManager.GetTimeCompare(advWaitEndTime));
                        if (TimeManager.GetTimeCompare(advWaitEndTime) <= 0)
                        {
                            advTimeObj.Refresh = null;
                            isAdvWaitEnd = true;
                            advTimerText1.text = advTimerText2.text = string.Empty;
                            SetDungeonInfo();
                        }
                    };
                }
            }
        }
        void SetDungeonInfo()
        {
            
            int maxEnterCnt = freeEnterCnt + advEnterCnt;

            battleEnterBtn.SetButtonSpriteState(true);
            if (curTicketCnt < freeEnterCnt)
            {
                advIcon.SetActive(false);
                battleAdIcon.SetActive(false);
                battleAbleCntText.text = availableEnterCntText.text = string.Format("{0}/{1}", freeEnterCnt - curTicketCnt, freeEnterCnt);
                advTimerText1.text = advTimerText2.text = string.Empty;
            }
            else if (curTicketCnt < maxEnterCnt)
            {
                advIcon.SetActive(true);
                battleAdIcon.SetActive(true);
                battleAbleCntText.text = availableEnterCntText.text = string.Format("x{0}", maxEnterCnt - curTicketCnt);
                battleEnterBtn.SetInteractable(isAdvWaitEnd);
                battleEnterBtn.SetButtonSpriteState(isAdvWaitEnd);
            }
            else
            {
                advIcon.SetActive(true);
                battleAdIcon.SetActive(true);
                battleAbleCntText.text = availableEnterCntText.text = string.Format("<color=#FF0000>x{0}</color>", maxEnterCnt - curTicketCnt);
                battleEnterBtn.SetButtonSpriteState(false);
                advTimerText1.text = advTimerText2.text = string.Empty;
            }
        }

        public void OnClickWorldBossBattleStart()
        {
            WorldBossFormationData curData = User.Instance.PrefData.WorldBossFormationData;
            if (!curData.HasFormation())
            {
                ToastManager.On(StringData.GetStringByStrKey("boss_raid_setting_yet"));
                return;
            }

            int adCount = 0;
            var adInfo = AdvertisementData.Get(SBDefine.AD_RAID_BOSS_KEY);
            if (adInfo != null)
                adCount = adInfo.LIMIT;
            
            var allCount = freeEnterCnt + adCount;
            if (curTicketCnt < freeEnterCnt)
                WorldBossBattleStart();
            else if ((curTicketCnt - freeEnterCnt) < adCount)
            {
                battleEnterBtn.interactable = false;
                isAdvWaitEnd = true;
                AdvertiseManager.Instance.TryADWithPopup((log) => {
                    WorldBossBattleStart(true, log);
                    battleEnterBtn.interactable = true;
                    isAdvWaitEnd = false;
                }, () =>
                {
                    ToastManager.On(StringData.GetStringByStrKey("광고실패"));
                    battleEnterBtn.interactable = true;
                    isAdvWaitEnd = false;
                }, () =>
                {
                    battleEnterBtn.interactable = true;
                    isAdvWaitEnd = false;
                });
            }
            else
            {
                ToastManager.On(100002103);//일일 최대 입장 횟수를 모두 사용하였습니다.
                return;
            }
        }
        private void WorldBossBattleStart(bool isAd = false, string log = "")
        {
            //일단 선택
            var stageInfo = WorldBossManager.Instance.WorldBossProgressData.GetStageDataByMonsterKey(WorldBossManager.Instance.UISelectBossKey);
            WorldBossManager.Instance.WorldBossProgressData.SetCurStageData(stageInfo);

            WorldBossManager.Instance.WorldBossProgressData.IsToday(()=> {
                WorldBossManager.Instance.RequestBattleStart(isAd, () => {
                    WorldBossManager.Instance.UISelectBossKey = -1;
                }, log);
            }, ()=> { });
        }

        public void OnClickEditTeam()
        {
            if (deckList == null || deckList.Count <= 0)
                return;

            deckList[0].OnClickEditDeck();
        }

        public void OnEvent(DragonChangedEvent eventType)
        {
            SetDeckList();
            SetBattlePoint(true);
        }

        public void OnClickRankingButton()
        {
            PopupManager.OpenPopup<WorldBossRankingPopup>();
        }

        public void OnClickHelpButton()
        {
            PopupManager.OpenPopup<WorldBossInfoPopup>();
        }

        bool IsBossEnterCondition()
        {
            return WorldBossManager.Instance.IsWorldBossEnterCondition();
        }

        int GetCurrentDeckDragonCount()
        {
            return User.Instance.PrefData.WorldBossFormationData.GetTotalFormationDragonCount();
        }


        public void OnClickCheatDropDown(Dropdown dropdown)
        {
            //Debug.Log("set value : " + dropdown.value);
            var changeValue = dropdown.value;
            if (changeValue > 0)
                WorldBossManager.Instance.CheckIntFlag = changeValue;
        }
    }
}

