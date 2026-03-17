using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Newtonsoft.Json.Linq;
using DG.Tweening;

namespace SandboxNetwork
{
    

    public class DailyDungeonLobby : MonoBehaviour
    {
        [SerializeField]
        private TimeObject advTimeObj = null;

        [SerializeField]
        private Button battleEnterBtn = null;
        [SerializeField]
        private Text battleAbleCntText = null;
        [SerializeField]
        private GameObject battleAdIcon =null;
        [SerializeField]
        private Text advTimerText1 = null;

        [Header("level layer")]
        [SerializeField]
        private GameObject levelLayer;
        [SerializeField]
        private ScrollRect levelScroll;
        [SerializeField]
        private RectTransform talkingDragonRect;
        [SerializeField]
        private Text talkingDragonText;
        [SerializeField]
        private Transform dragonParentTr;
        [SerializeField]
        private RectTransform dragonBubbleRect;

        [Header("TopLayer")]
        [SerializeField]
        private GameObject dailyWeekRewardObj = null;
        [SerializeField]
        private GameObject dailyEnterCntObj = null;
        [SerializeField]
        private Text dungeonNameText = null;
        [SerializeField]
        private Text availableEnterCntText = null;
        [SerializeField]
        private GameObject advIcon = null;
        [SerializeField]
        private Text advTimerText2 = null;

        [Header("daily dungeon node")]
        [SerializeField]
        private ScrollRect dungeonObjScrollRect = null;
        [SerializeField]
        List<DailyDungeonSelectObj> dungeonObjs = new List<DailyDungeonSelectObj>();
        [SerializeField]
        private HorizontalLayoutGroup dungeonLayout;
        [SerializeField]
        private Vector3 selectPos;
        [SerializeField]
        private float selectScaleValue = 1.4f;


        [Header("daily reward")]
        [SerializeField]
        private GameObject dailyRewardPopup = null;
        [SerializeField]
        private DailyRewardObject[] dailyRewards = null;

        [Header("Stage")]
        [SerializeField]
        private Transform stageParent = null;
        [SerializeField]
        private GameObject dailyStageObj = null;

        [SerializeField] private Text dailyDungeonTimeLabel = null;

        private TimeEnable dailyTimeEnable = null;
        private List<DailyDungeonStageObj> dailyDungeonStageObjs = new List<DailyDungeonStageObj>();
        private eDailyType curDay = eDailyType.None;
        private int selectIndex = -1;
        private int selectWorld = -1;
        private int selectStage = -1;

        private int freeEnterCnt { get { return GameConfigTable.GetConfigIntValue("DAY_DUNGEON_CLEAR_COUNT"); } }
        private int advEnterCnt { get { return AdvertisementData.Get(SBDefine.AD_DAILY_KEY).LIMIT; } }

        private float prevCamSize;

        private Coroutine cor;

        bool isAdvWaitEnd = true;
        void Start()
        {
            curDay = DailyManager.Instance.GetDaily();
            InitDailyRewardUI();

            RefreshUI();
            
            

            // 개요 - 말하는 드래곤 위치를 배경의 특정 위치로 고정하면서 배경의 우상단에 철근이 어디까지 나와야 하는 배경 형태인데
            // 이 배경을 적용하기 위해서 orthograpicSize가 5.5 로 됨
            // 하지만 UICavas 의 orthograpicSize는 더 작기 때문에 itemToolTip을 켰을때 위치가 엇나감
            // 이를 해결하기 위해 이 씬에서만 orthograpic 사이즈를 동일하게 맞추어줌
            prevCamSize = UIManager.Instance.UICamera.orthographicSize;
            UIManager.Instance.UICamera.orthographicSize = Camera.main.orthographicSize;

            if (dailyDungeonTimeLabel != null)
            {
                dailyTimeEnable = dailyDungeonTimeLabel.GetComponent<TimeEnable>();
                if (dailyTimeEnable == null)
                    dailyTimeEnable = dailyDungeonTimeLabel.gameObject.AddComponent<TimeEnable>();

                if (dailyTimeEnable != null)
                    dailyTimeEnable.Refresh = RefreshDailyTime;
            }
            
        }

        void TutorialCheck()
        {
            if (TutorialManager.tutorialManagement.IsFinishedTutorial(TutorialDefine.DailyDungeon)==false)
            {
                if(TutorialManager.tutorialManagement.IsPlayingTutorialByGroup(TutorialDefine.DailyDungeon) ==false)
                    TutorialManager.tutorialManagement.StartTutorial((int)TutorialDefine.DailyDungeon);
            }
        }

        private void OnDestroy()
        {
            if(UIManager.Instance.UICamera != null)
            {
                UIManager.Instance.UICamera.orthographicSize = prevCamSize;
            }
        }

        void RefreshUI()
        {
            if(selectIndex <0)
            {
                UIManager.Instance.MainUI.ReleaseTownButtonCallBack();
                SetAdvWaitTime();
                SetLayerState();
                SetDungeonInfo();
                SetDungeonObj();
                SetDailyRewardUI();
            }
            else
            {
                UIManager.Instance.MainUI.SetTownButtonCallBack(DungeonResumeProcess);
                SetLayerState(false);
                SetStageLayer();
                SetDragonTalk();
                SetSeletedStage(selectStage);
            }

            ScenarioManager.Instance.OnEventCheckFirstDailyDungeon(()=>TutorialCheck());
        }


        void SetAdvWaitTime()
        {
            if(advTimeObj.Refresh == null) {
                
                var lastestDate = ShopManager.Instance.GetAdvertiseState(SBDefine.AD_DAILY_KEY).LAST_VIEWDATE;
                var term = AdvertisementData.Get(SBDefine.AD_DAILY_KEY).TERM;
                int advWaitEndTime = TimeManager.GetTimeStamp(lastestDate) + term;
                if (TimeManager.GetTimeCompare(advWaitEndTime) <=0)
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

        void SetLayerState(bool isDefault = true)
        {
            if(dungeonLayout !=null)
                dungeonLayout.enabled = isDefault;
            
            UIManager.Instance.InitUI(isDefault ? eUIType.None: eUIType.Daily);
            UIManager.Instance.RefreshUI(isDefault ? eUIType.None : eUIType.Daily);
            UIManager.Instance.MainUI.OnOffExitBtn(true);
            dailyEnterCntObj.SetActive(isDefault);
            dailyWeekRewardObj.SetActive(isDefault);
            talkingDragonRect.gameObject.SetActive(!isDefault);
            levelLayer.SetActive(!isDefault);
        }


        #region 던전 선택 전 UI 세팅


        void SetDungeonInfo()
        {
            dungeonNameText.text = StringData.GetStringByStrKey("요일던전 타이틀");
            int ticketCnt = StageManager.Instance.DailyDungeonProgressData.DailyDungeonTicketCount;  // 0부터 시작 해서 클리어 할때마다 1씩 증가
            int maxEnterCnt = freeEnterCnt + advEnterCnt;

            battleEnterBtn.SetButtonSpriteState(true);
            if (ticketCnt < freeEnterCnt)
            {
                advIcon.SetActive(false);
                battleAdIcon.SetActive(false);
                battleAbleCntText.text = availableEnterCntText.text = string.Format("{0}/{1}", freeEnterCnt - ticketCnt, freeEnterCnt);
                advTimerText1.text = advTimerText2.text = string.Empty;
            }
            else if(ticketCnt < maxEnterCnt)
            {
                advIcon.SetActive(true);
                battleAdIcon.SetActive(true);
                battleAbleCntText.text = availableEnterCntText.text = string.Format("x{0}", maxEnterCnt-ticketCnt);
                battleEnterBtn.SetInteractable(isAdvWaitEnd);
                battleEnterBtn.SetButtonSpriteState(isAdvWaitEnd);
            }
            else
            {
                advIcon.SetActive(true);
                battleAdIcon.SetActive(true);
                battleAbleCntText.text = availableEnterCntText.text = string.Format("<color=#FF0000>x{0}</color>", maxEnterCnt - ticketCnt);
                battleEnterBtn.SetButtonSpriteState(false);
                advTimerText1.text = advTimerText2.text = string.Empty;
            }
        }

        void SetDungeonObj()
        {
            if(dungeonObjs.Count> 0)
            {
                foreach (var dungeonNode in dungeonObjs)
                {
                    dungeonNode.transform.localScale = Vector3.one;
                    dungeonNode.gameObject.SetActive(false);
                }
            }
            
            List<int> worldIndexes = StageManager.Instance.DailyDungeonProgressData.TodayWorldIndex;
            if (worldIndexes == null)
                return;
            int availAbleDungeonCount = worldIndexes.Count;
            dungeonObjScrollRect.enabled = false;
            for (int i = 0; i < availAbleDungeonCount; ++i)
            {
                if (dungeonObjs.Count <= i)
                    continue;

                dungeonObjs[i].gameObject.SetActive(true);
                StageManager.Instance.DailyDungeonProgressData.GetDailyDungeonInfoData(worldIndexes[i]);
                dungeonObjs[i].Init(curDay, OnClickDungeonSelect, i);

            }
            if (availAbleDungeonCount > 3) // 오늘 플레이 할 수 있는 던전이 4개 이상인 테스트 환경이라면
            {
                if (cor == null)
                {
                    cor = StartCoroutine(SetDungeonScrollOn());  // 스파인과 스크롤렉이 스파인을 그리는 과정에서  
                                                                 // 스파인 문제로 오류가 나니깐 스파인이 다 그려지고 난 뒤 스크롤렉 설정
                }
            }
            else
            {
                dungeonObjScrollRect.enabled = false;
            }
        }
        IEnumerator SetDungeonScrollOn()
        {
            yield return null;
            dungeonObjScrollRect.enabled = true;
            dungeonObjScrollRect.horizontalNormalizedPosition = 0;
            cor = null;
        }




        #endregion

        #region 던전 선택 후 UI 세팅


        void SetDragonTalk()
        {
            RectTransform rect = dragonParentTr.GetComponent<RectTransform>();

           
            var sizeSafeX = Screen.safeArea.width;
            var sizeX = Screen.width;
            var sizeY = Screen.height;
            var fixedSizeX = rect.rect.width * (sizeSafeX/sizeX); // 세이프 에어리어 비율 계산
            dragonBubbleRect.sizeDelta = new Vector2(fixedSizeX / 2f - 550, 0);
            var data = DailyStageData.GetByWorld(selectWorld);
            talkingDragonText.text = StringData.GetStringByStrKey(data.STAGE_DESC);
            var textRect = talkingDragonText.GetComponent<RectTransform>();
            LayoutRebuilder.ForceRebuildLayoutImmediate(textRect);
            LayoutRebuilder.ForceRebuildLayoutImmediate(textRect.parent.GetComponent<RectTransform>());
            
            //talkingDragonText.transform.parent.gameObject.SetActive(false);
        }

        void SetStageLayer()
        {
            List<int> worldIndexes = StageManager.Instance.DailyDungeonProgressData.TodayWorldIndex;
            var curData = StageBaseData.GetByWorld(worldIndexes[selectIndex]);
            var cleared = StageManager.Instance.DailyDungeonProgressData.GetDailyProgressData(worldIndexes[selectIndex]);
            dailyStageObj.SetActive(false);
            bool isCreateLvObj = false;
            for (int i =0; i< curData.Count; ++i)
            {
                if(dailyDungeonStageObjs.Count <= i)
                {
                    var obj =Instantiate(dailyStageObj, stageParent);
                    obj.SetActive(true);
                    dailyDungeonStageObjs.Add(obj.GetComponent<DailyDungeonStageObj>());
                    isCreateLvObj = true;
                }
                bool isLock = i > 0;
                if (i > 0 && (cleared.Count > i - 1))
                {
                    isLock = cleared[i - 1] == 0;
                }
                dailyDungeonStageObjs[i].Init(i + 1, isLock, SetCurrentStageReward);
            }
            if(isCreateLvObj)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(stageParent.GetComponent<RectTransform>());
            }
        }

        void SetSeletedStage(int selectedStage)
        {
            foreach( var stage in dailyDungeonStageObjs)
            {
                stage.SetColor(false);
            }
            dailyDungeonStageObjs[selectedStage-1].SetColor(true);
            levelScroll.FocusOnItem(dailyDungeonStageObjs[selectedStage - 1].GetComponent<RectTransform>(), 0.2f);
        }

        public void SetCurrentStageReward(int stage=-1)
        {
            if(stage > 0) { 
                selectStage = stage;
            }
            dungeonObjs[selectIndex].SetItemInfoLayer(selectWorld,selectStage); //  보상 세팅
            SetSeletedStage(stage);
        }

        #endregion



        #region 요일던전 던전선택

        public void OnClickDungeonSelect(int index)
        {
            IsToday(
            () => {
                DungeonSelectProcess(index);
            }, RefreshUI);
        }

        void DungeonSelectProcess(int index)
        {
            selectIndex = index;
            dungeonLayout.enabled = false;

            // 연출 영역
            var todayDungeonList = StageManager.Instance.DailyDungeonProgressData.TodayWorldIndex;

            Sequence sequence = DOTween.Sequence();
            sequence.Append(dungeonObjs[index].transform.DOLocalMove(selectPos, 0.3f));
            for (int i = 0; i < dungeonObjs.Count; ++i)
            {
                if (i == selectIndex) continue;
                if (i < selectIndex)
                {
                    sequence.Join(dungeonObjs[i].transform.DOLocalMoveX(-Screen.width, 0.3f));
                }
                else
                {
                    sequence.Join(dungeonObjs[i].transform.DOLocalMoveX(Screen.width, 0.3f));
                }

            }
            sequence.Append(dungeonObjs[index].transform.DOScale(Vector3.one * selectScaleValue, 0.2f));
            sequence.AppendCallback(() =>
            {
                selectWorld = todayDungeonList[index];
                selectStage = StageManager.Instance.DailyDungeonProgressData.GetLastestStage(selectWorld);
                dungeonObjs[index].SetItemInfoLayer(selectWorld,selectStage);
                for (int i = 0; i < dungeonObjs.Count; ++i)
                {
                    if (i == selectIndex) continue;
                    dungeonObjs[i].gameObject.SetActive(false);
                    dungeonObjs[i].transform.localPosition = selectPos;
                    dungeonObjs[i].transform.localScale = Vector3.one * selectScaleValue;
                }

                RefreshUI();
            });
        }

        void DungeonResumeProcess()
        {
            if (selectIndex < 0)
                return;

            //int index = selectIndex;
            selectIndex = -1;
            RefreshUI();
            //Sequence sequence = DOTween.Sequence();
            //sequence.Append(dungeonObjs[index].transform.DOLocalMove(selectPos, 0.5f));
            //sequence.Append(dungeonObjs[index].transform.DOScale(Vector3.one, 0.5f));
            //sequence.AppendCallback(() =>
            //{
            //    selectWorld = -1;
            //    selectStage = -1;
            //    dungeonObjs[index].Init(curDay, index);
            //    for (int i = 0; i < dungeonObjs.Length; ++i)
            //    {
            //        dungeonObjs[i].gameObject.SetActive(true);
            //        dungeonObjs[i].transform.localPosition = selectPos;
            //        dungeonObjs[i].transform.localScale = Vector3.one * selectScaleValue;
            //    }
            //    dungeonLayout.enabled = true;
            //    RefreshUI();
            //});
        }

        public void OnClickChangeDungeonArrow()
        {
            int maxCnt = StageManager.Instance.DailyDungeonProgressData.TodayWorldIndex.Count;
            selectIndex = (selectIndex + 1) % maxCnt;
            foreach(var  dungeonObj in dungeonObjs)
            {
                dungeonObj.gameObject.SetActive(false);
            }
            dungeonObjs[selectIndex].gameObject.SetActive(true);
            selectWorld = StageManager.Instance.DailyDungeonProgressData.TodayWorldIndex[selectIndex];
            selectStage = StageManager.Instance.DailyDungeonProgressData.GetLastestStage(selectWorld);
            dungeonObjs[selectIndex].SetItemInfoLayer(selectWorld, selectStage);
            SetSeletedStage(selectStage);
            RefreshUI();
        }


        #endregion

        #region 요일던전 보상 팝업 

        void InitDailyRewardUI() // 보상 세팅
        {
            dailyRewardPopup.SetActive(false);
            for (int i=0; i< dailyRewards.Length;++i)
            {
                dailyRewards[i].Init((eDailyType)i +1,false);
            }
        }
        void SetDailyRewardUI() // 현재 날짜 표시 세팅
        {
            foreach (var dailyReward in dailyRewards)
            {
                dailyReward.SetSelectState(false);
            }
            int dayIndex = (int)curDay -1;
            dailyRewards[dayIndex].SetSelectState(true);
        }
        
        public void OnClickDailyRewardUI()
        {
            IsToday(
                () => {
                    dailyRewardPopup.SetActive(true);
                    UIManager.Instance.MainUI.OnOffExitBtn(false);
                }, RefreshUI);
        }

        public void OnClickCloseDailyReward()
        {
            dailyRewardPopup.SetActive(false);
            UIManager.Instance.MainUI.OnOffExitBtn(true);
        }

        #endregion



        
        void IsToday(VoidDelegate matchDayCallBack, VoidDelegate dismatchDayCallBack) 
        {
            var day = DailyManager.Instance.GetDaily(); // 서버로부터 받은 날짜

            if (day == curDay)
            {
                matchDayCallBack?.Invoke();
            }
            else
            {

                SystemPopup.OnSystemPopup(StringData.GetStringByStrKey("알림"),StringData.GetStringByStrKey("요일던전 초기화 문구"),
                    () =>
                    {
                        NetworkManager.Send("daily/dailyinfo", null, (JObject jsonData) =>
                        {
                            if (jsonData["err"] != null && jsonData["rs"] != null && (int)jsonData["err"] == 0 && (int)jsonData["rs"] == 0)
                            {
                                var data = jsonData["daily_info"];
                                if (data != null)
                                {
                                    if (SBFunc.IsJTokenType(data["world"], JTokenType.Array))
                                        StageManager.Instance.DailyDungeonProgressData.SetTodayWorldIndex(data["world"]);

                                    if (SBFunc.IsJTokenType(data["battle_count"], JTokenType.Integer))
                                        StageManager.Instance.DailyDungeonProgressData.SetDailyDungeonTicketCount(data["battle_count"]);
                                }
                                var logData = jsonData["daily_log"];
                                if (SBFunc.IsJArray(logData))
                                {
                                    StageManager.Instance.SetDailyDungeonProgress((JArray)logData);
                                }
                                curDay = DailyManager.Instance.GetDaily();
                            }
                            dismatchDayCallBack?.Invoke();
                        }, (string log) =>
                        {
                            Debug.Log(log);
                            dismatchDayCallBack?.Invoke();
                        });
                    }
                    );
                selectIndex = -1;
            }

        }

        public void OnClickBattleReady()
        {
            int ticketCnt = StageManager.Instance.DailyDungeonProgressData.DailyDungeonTicketCount;  // 0부터 시작 해서 클리어 할때마다 1씩 증가
            int maxEnterCnt = freeEnterCnt + advEnterCnt;
            if (ticketCnt < maxEnterCnt)
            {
                IsToday(
                () => {
                    PopupManager.OpenPopup<DailyReadyPopup>(new DailyReadyData(curDay, selectWorld, selectStage, 1));
                }, RefreshUI);
            }
            else
            {
                ToastManager.On(100002103);
            }
            
        }

        public void OnDragonClick()
        {
            //talkingDragonText.transform.parent.gameObject.SetActive(!talkingDragonText.transform.parent.gameObject.activeInHierarchy);
        }
        private void RefreshDailyTime()
        {
            if (dailyDungeonTimeLabel == null)
                return;

            int time = TimeManager.GetContentResetTime();
            if (time > 0)
                dailyDungeonTimeLabel.text = TimeText(time);
            else
                dailyDungeonTimeLabel.text = TimeText(0);
        }
        private string TimeText(int time)
        {
            return StringData.GetStringFormatByIndex(100002626, SBFunc.TimeString(time));
        }
    }
}
