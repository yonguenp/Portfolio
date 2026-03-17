using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public struct eventScheduleDesc
    {
        public bool isShowNode;
        public bool isShowDescText;
        public bool isShowInfoButton;

        public eventScheduleDesc(bool _node, bool _descText, bool _infoButton)
        {
            isShowNode = _node;
            isShowDescText = _descText;
            isShowInfoButton = _infoButton;
        }
    }
    
    public struct DiceUIEvent
    {
        public enum eDiceUI
        {
            REFRESH_UI,             //화면 갱신
            REFRESH_GOODS,          //재화 갱신
            REFRESH_TAB_REDDOT,     //레드닷 갱신
            REFRESH_SCHEDULE_INFO,  //이벤트 기간 라벨, 인포버튼 잡다한 것

            OPEN_BOX,               //상자 오픈
        }
        private static DiceUIEvent obj;
        public eDiceUI type;

        public int boxSlotIndex;
        public int boxCount;

        public eventScheduleDesc descInfo;

        public static void RefreshUI()
        {
            obj.type = eDiceUI.REFRESH_UI;
            EventManager.TriggerEvent(obj);
        }

        public static void RefreshGoods()//param 세팅하지말고 Event2023Holiday -> box1~3 데이터 받아서 쓰면 될듯.
        {
            obj.type = eDiceUI.REFRESH_GOODS;
            EventManager.TriggerEvent(obj);
        }
        public static void RefreshTabReddot()
        {
            obj.type = eDiceUI.REFRESH_TAB_REDDOT;
            EventManager.TriggerEvent(obj);
        }
        /// <summary>
        /// 각 탭 (서브탭) 별로 이벤트 날짜 보이고, 인포버튼 따로 해달라고 해서 처리
        /// </summary>
        public static void RefreshDescInfo(eventScheduleDesc _infoData)
        {
            obj.type = eDiceUI.REFRESH_SCHEDULE_INFO;

            obj.descInfo = new eventScheduleDesc();
            obj.descInfo.isShowNode = _infoData.isShowNode;
            obj.descInfo.isShowDescText = _infoData.isShowDescText;
            obj.descInfo.isShowInfoButton = _infoData.isShowInfoButton;

            EventManager.TriggerEvent(obj);
        }
        /// <summary>
        /// 상자 오픈 
        /// </summary>
        /// <param name="_slotIndex"></param>//상자 UI 슬롯 넘버
        public static void RequestOpenBox(int _slotIndex, int count = 1)
        {
            obj.type = eDiceUI.OPEN_BOX;
            obj.boxCount = count;
            obj.boxSlotIndex = _slotIndex;
            EventManager.TriggerEvent(obj);
        }
    }

    public class DiceEventPopup : Popup<TabTypePopupData>, EventListener<DiceUIEvent>
    {
        /// <summary>
        /// 아이템 인덱스 정의 이유 : 서버 데이터를 요청하기 전에 레드닷 체크용도(상자 및 주사위 갯수 체크)
        /// 이것 이외에 다른 정보가 필요하다면 holidayData 데이터에 의존해야함.
        /// </summary>
        const int DICE_ITEM_NO = 20000009;
        const int BOX1_ITEM_NO = 20000010;
        const int BOX2_ITEM_NO = 20000011;
        const int BOX3_ITEM_NO = 20000012;

        [SerializeField]
        Button eventInfoButton = null;

        [SerializeField]
        Text eventScheduleText = null;

        [SerializeField]
        GameObject eventScheduleNode = null;

        [SerializeField]
        TabController tabController = null;

        [SerializeField]
        Image titleIconImage = null;

        [SerializeField]
        List<Sprite> titleIconList = new List<Sprite>();

        [SerializeField]
        GameObject eaterNode = null;

        EventScheduleData scheduleData = null;
        EventDiceBaseData holidayData = null;

        bool isFirstInit = false;

        #region OpenPopup
        public static DiceEventPopup OpenPopup(int tab, int subTab = -1)
        {
            return OpenPopup(new TabTypePopupData(tab, subTab));
        }
        public static DiceEventPopup OpenPopup(TabTypePopupData data)
        {
            if (data == null)
                return null;

            return PopupManager.OpenPopup<DiceEventPopup>(data);
        }
        public static void MoveTabForce(TabTypePopupData data, bool _force = true)
        {
            var popup = PopupManager.GetPopup<DiceEventPopup>();
            var isOpenPopup = PopupManager.IsPopupOpening(popup);
            if (isOpenPopup)
                popup.MoveTab(data, _force);
        }

        /// <summary>
        /// to do - UI 열때 갱신하는 것으로 수정
        /// open popup 시도할 때 하고 성공하면 오픈으로 연결.
        /// </summary>
        public static bool RequestEventHolidayData(VoidDelegate success = null, VoidDelegate fail = null, bool uiTime = true)//startLoading Scene 에서 미리 데이터 불러옴.
        {
            foreach (var data in User.Instance.EventData.GetActiveEvents(uiTime))
            {
                if (data.TYPE == eActionType.EVENT_DICE)
                {
                    WWWForm form = new WWWForm();
                    form.AddField("event_id", data.KEY);

                    NetworkManager.Send("event/dice", form, (JObject jsonData) =>
                    {
                        if (SBFunc.IsJTokenCheck(jsonData) && (int)eApiResCode.OK == jsonData["rs"].Value<int>())
                        {
                            User.Instance.EventData.SetData(data, jsonData);

                            success?.Invoke();
                        }
                        else
                            fail?.Invoke();
                    }, (failString) => {
                        fail?.Invoke();
                    });

                    return true;
                }
            }

            return false;
        }
        #endregion

        private void OnEnable()
        {
            EventManager.AddListener(this);
        }

        private void OnDisable()
        {
            PopupManager.Instance.Top.SetMagnetUI(false);
            EventManager.RemoveListener(this);
        }

        public static EventDiceBaseData GetHolidayData()
        {
            var holidayPopup = PopupManager.GetPopup<DiceEventPopup>();
            if (holidayPopup == null)
                return null;

            return holidayPopup.holidayData;
        }
        public static EventScheduleData GetScheduleData()
        {
            var holidayPopup = PopupManager.GetPopup<DiceEventPopup>();
            if (holidayPopup == null)
                return null;

            return holidayPopup.scheduleData;
        }
        public static bool GetTotalReddotCondition()
        {
            return GetReddotCondition();
        }
        public static bool GetDiceBoardReddotCondition()
        {
            return IsAvailableDiceItem();
        }
        public static bool GetDiceQuestReddotCondition()
        {
            return IsAvailableRewardDiceQuest();
        }

        public static bool GetBoxReddotCondition()
        {
            return IsAvailableBoxItem();
        }
        void SetSubCamTextureOn()
        {
            Town.Instance?.SetSubCamState(true);
            UICanvas.Instance.StartBackgroundBlurEffect();
        }

        void SetSubCamTextureOff()
        {
            Town.Instance?.SetSubCamState(false);
            UICanvas.Instance.EndBackgroundBlurEffect();
        }
        public override void InitUI()
        {
            if (tabController == null)
                return;
            
            UIObjectEvent.Event(UIObjectEvent.eEvent.EVENT_HIDE, UIObjectEvent.eUITarget.ALL);

            tabController.GetComponent<DiceEventTabController>().SetTabChangeCallBack(TabControllerCallback);
            SetTitleImage();
            SetHolidayData();//팝업 열때 한번만
            SetTopUI();
            InitTabController();
            InitEaterNode();
            SetSubCamTextureOn();

            UICanvas.Instance.StartBackgroundBlurEffect();
        }
        void SetTopUI()
        {
            PopupManager.Instance.Top.SetDiaUI(true);
            PopupManager.Instance.Top.SetGoldUI(true);
            PopupManager.Instance.Top.SetMagnetUI(false);//p2e 가능 지역 일때 표시
            PopupManager.Instance.Top.SetStaminaUI(false);
            PopupManager.Instance.Top.SetArenaTicketUI(false);
            PopupManager.Instance.Top.SetMileageUI(false);
            PopupManager.Instance.Top.SetArenaPointUI(false);
            PopupManager.Instance.Top.SetFriendPointUI(false);
        }
        void InitTabController()
        {
            int tabIndex;
            int subIndex = 0;
            if (Data == null)
            {
                tabIndex = 0;
            }
            else
            {
                tabIndex = Data.TabIndex;
            }
            if (tabIndex < 0)
            {
                tabIndex = 0;
            }
            if (Data.SubIndex != -1)
                subIndex = Data.SubIndex;

            tabController.InitTab(tabIndex, new TabTypePopupData(tabIndex, subIndex));
        }

        void InitEaterNode()
        {
            if (eaterNode != null)
                eaterNode.SetActive(false);
        }

        public override void ForceUpdate(TabTypePopupData data)
        {
            base.DataRefresh(data);
            tabController.RefreshTab();
        }

        public void MoveTab(TabTypePopupData data, bool _force = false)
        {
            if (data == null)
            {
                return;
            }

            int tabIndex = data.TabIndex;
            int subIndex = 0;

            if (data.SubIndex != -1)
                subIndex = data.SubIndex;

            if (_force)
                tabController.InitTabIndex();

            if (tabIndex >= 0)
            {
                tabController.ChangeTab(tabIndex, new TabTypePopupData(tabIndex, subIndex));
            }
        }

        public void SetHolidayData()
        {
            if (isFirstInit)
                return;

            var HolidayTypeDataList = EventScheduleData.GetEventTypeData(eActionType.EVENT_DICE, true);
            if (HolidayTypeDataList == null || HolidayTypeDataList.Count <= 0)
                return;

            if (HolidayTypeDataList.Count > 1)
                Debug.LogError("Data Table Error HOLIDAY TYPE Count : " + HolidayTypeDataList.Count);

            scheduleData = HolidayTypeDataList[0];
            holidayData = HolidayTypeDataList[0].EventBaseData as EventDiceBaseData;

            SetScheduleText();

            isFirstInit = true;
        }

        void SetScheduleText()
        {
            if (eventScheduleText != null && scheduleData != null)
            {
                //var startString = SBFunc.TimeCustomString(scheduleData.START_TIME, 3);
                //var endString = SBFunc.TimeCustomString(scheduleData.UI_END_TIME, 3);
                eventScheduleText.text = StringData.GetStringByStrKey("2023크리스마스이벤트기간");//string.Format("{0} ~ {1}", startString, endString);
            }
        }

        void SetTitleImage()
        {
            var index = 0;
            var language = GamePreference.Instance.GameLanguage;
            switch (language)
            {
                case SystemLanguage.Korean:
                    index = 1;
                    break;

            }

            if (titleIconImage != null && index < titleIconList.Count && index >= 0)
                titleIconImage.sprite = titleIconList[index];
        }

        void TabControllerCallback(int _curTabIndex)
        {
            //SetEventScheduleTextNodeVisible(_curTabIndex);
            ShowAttendancePopup(_curTabIndex);
        }
        
        void ShowAttendancePopup(int _curTabIndex)
        {
            if(_curTabIndex == 3)
            {
                var prevTabIndex = tabController.CurTab;
                var prevTabLayer = tabController.GetTabLayer(prevTabIndex);
                var prevSubIndex = prevTabLayer == null ? 0 : prevTabLayer.SubLayerIndex;

                EventAttendancePopup.OpenPopup(scheduleData,()=> {
                    tabController.ChangeTab(prevTabIndex, new TabTypePopupData(prevTabIndex, prevSubIndex));
                });
            }
        }

        public void OnClickShowAttendancePopup()
        {
            if (holidayData == null)
                return;

            var isPeriod = holidayData.IsEventPeriod(false);
            if (!isPeriod)
            {
                ToastManager.On(StringData.GetStringByStrKey("이벤트종료안내2"));
                return;
            }

            EventAttendancePopup.OpenPopup(scheduleData);
        }

        void SetEventScheduleTextNodeVisible(int tabIndex)
        {
            if (tabIndex == 2)
                eventScheduleNode.SetActive(false);
            else
                eventScheduleNode.SetActive(true);
        }

        /// <summary>
        /// 주사위 게임판 - (tabIndex = 0 , subIndex = 0) - 주사위 갯수가 1개 이상일 때
        /// 주사위 퀘스트 - 주사위 퀘스트 보상 받을 수 있는 컨디션일 때
        /// 내 선물 상자 - (tabIndex = 1 , subIndex = 0) - 보유 선물상자 (종류 무관) 가 1개 이상일 때
        /// </summary>
        public static bool GetReddotCondition()
        {
            return IsAvailableBoxItem() || IsAvailableDiceItem() || IsAvailableRewardDiceQuest();
        }

        static bool IsAvailableRewardDiceQuest()
        {
            int eventKey = -1;
            var eventData = DiceEventPopup.GetHolidayData();
            if (eventData != null)
                eventKey = eventData.GetScheduleDataKey();

            var eventProceedList = QuestManager.Instance.GetProceedQuestDataByType(eQuestType.EVENT, eventKey);
            var completeList = QuestManager.Instance.GetCompleteQuestDataByType(eQuestType.EVENT, eventKey);

            eventProceedList.AddRange(completeList);
            eventProceedList.Sort((d1, d2) => d1.ID - d2.ID);//quest_base Key 오름차순


            if (eventProceedList == null || eventProceedList.Count <= 0)
                return false;

            foreach (var quest in eventProceedList)
            {
                if (IsGetRewardCondition(quest))
                    return true;
            }
            return false;
        }
        static bool IsGetRewardCondition(Quest _quest, bool _isSubQuest = true)//보상을 받을 수 있는 상태인가?
        {
            if (_quest == null)
                return false;

            if (_quest.State == eQuestState.TERMINATE || _quest.State == eQuestState.PROCESS_DONE)//이미 완료한 퀘스트
                return false;

            if (_isSubQuest)//하위 퀘스트 조건만 체크 - 대장 퀘스트는 앞단에서 선체크 해줌.
            {
                var singleData = _quest.GetSingleTriggerData();
                if (singleData != null)
                {
                    var isShowAD = QuestCondition.IsAdvertiseQuestType(singleData.TYPE) && !User.Instance.ADVERTISEMENT_PASS;
                    if (isShowAD)//questTrigger가 광고 타입 && 광고제거 없으면
                        return false;
                }
            }

            if (_quest.IsQuestClear())//진행 중인 퀘스트만 체크
                return true;

            return false;
        }

        static bool IsAvailableDiceItem()
        {
            return User.Instance.GetItem(DICE_ITEM_NO).Amount > 0;
        }

        static bool IsAvailableBoxItem()
        {
            List<InventoryItem> tempList = new List<InventoryItem>() {
            User.Instance.GetItem(BOX1_ITEM_NO),
            User.Instance.GetItem(BOX2_ITEM_NO),
            User.Instance.GetItem(BOX3_ITEM_NO)
            };

            int totalCount = 0;
            foreach (var item in tempList)
            {
                if (item == null)
                    continue;
                totalCount += item.Amount;
            }

            return totalCount > 0;
        }

        public void OnClickHelpBtn()
        {
            var curTabLayer = tabController.GetCurrentTabLayer();
            if (curTabLayer == null)
                return;

            var subLayerIndex = curTabLayer.SubLayerIndex;
            if (subLayerIndex < 0)
                subLayerIndex = 0;

            PopupManager.OpenPopup<DiceEventHelpPopup>(new HelpPopupData(tabController.CurTab + 1, subLayerIndex + 1));
        }

        void SetDescDataInfo(eventScheduleDesc _info)
        {
            //if (eventScheduleNode != null)
            //    eventScheduleNode.SetActive(_info.isShowNode);
            //if (eventScheduleText != null)
            //    eventScheduleText.gameObject.SetActive(_info.isShowDescText);
            if (eventInfoButton != null)
                eventInfoButton.gameObject.SetActive(_info.isShowInfoButton);
        }

        public override void ClosePopup()
        {
            base.ClosePopup();

            UIObjectEvent.Event(UIObjectEvent.eEvent.EVENT_SHOW, UIObjectEvent.eUITarget.ALL);
            SetSubCamTextureOff();
            tabController.InitTabIndex();

            UICanvas.Instance.EndBackgroundBlurEffect();
        }

        public void OnEvent(DiceUIEvent eventType)
        {
            switch(eventType.type)
            {
                case DiceUIEvent.eDiceUI.REFRESH_TAB_REDDOT:
                    tabController.RefreshReddot();
                    break;
                case DiceUIEvent.eDiceUI.REFRESH_SCHEDULE_INFO:
                    SetDescDataInfo(eventType.descInfo);
                    break;
            }    
        }
    }
}
