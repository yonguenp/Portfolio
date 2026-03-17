using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public struct LuckyBagUIEvent
    {
        public enum eLuckyBagUIType
        {
            REFRESH_UI,             //화면 갱신
            REFRESH_TAB_REDDOT,     //레드닷 갱신

        }
        private static LuckyBagUIEvent obj;
        public eLuckyBagUIType type;

        public static void RefreshUI()
        {
            obj.type = eLuckyBagUIType.REFRESH_UI;
            EventManager.TriggerEvent(obj);
        }
        public static void RefreshTabReddot()
        {
            obj.type = eLuckyBagUIType.REFRESH_TAB_REDDOT;
            EventManager.TriggerEvent(obj);
        }
    }
    public class LuckyBagEventPopup : Popup<TabTypePopupData>, EventListener<LuckyBagUIEvent>
    {
        const int LUCKY_BAG_ITEM_NO = 20000008;

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
        EventLuckyBagBaseData eventData = null;

        bool isFirstInit = false;

        #region OpenPopup
        public static LuckyBagEventPopup OpenPopup(int tab, int subTab = -1)
        {
            return OpenPopup(new TabTypePopupData(tab, subTab));
        }
        public static LuckyBagEventPopup OpenPopup(TabTypePopupData data)
        {
            if (data == null)
                return null;

            return PopupManager.OpenPopup<LuckyBagEventPopup>(data);
        }
        public static void MoveTabForce(TabTypePopupData data, bool _force = true)
        {
            var popup = PopupManager.GetPopup<LuckyBagEventPopup>();
            var isOpenPopup = PopupManager.IsPopupOpening(popup);
            if (isOpenPopup)
                popup.MoveTab(data, _force);
        }

        /// <summary>
        /// open popup 시도할 때 하고 성공하면 오픈으로 연결.
        /// </summary>
        public static bool RequestEventData(VoidDelegate success = null, VoidDelegate fail = null, bool uiTime = true)//startLoading Scene 에서 미리 데이터 불러옴.
        {
            foreach (var data in User.Instance.EventData.GetActiveEvents(uiTime))
            {
                if (data.TYPE == eActionType.EVENT_LUCKY_BAG)
                {
                    WWWForm form = new WWWForm();
                    form.AddField("event_id", data.KEY);
                    form.AddField("op", (int)eLuckyBagEventState.REQUEST_INFO);
                    NetworkManager.Send("event/newyear2024", form, (JObject jsonData) =>
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
        public static EventLuckyBagBaseData GetEventData()
        {
            var holidayPopup = PopupManager.GetPopup<LuckyBagEventPopup>();
            if (holidayPopup == null)
                return null;

            return holidayPopup.eventData;
        }
        public static EventScheduleData GetScheduleData()
        {
            var holidayPopup = PopupManager.GetPopup<LuckyBagEventPopup>();
            if (holidayPopup == null)
                return null;

            return holidayPopup.scheduleData;
        }
        public static bool GetTotalReddotCondition()
        {
            return GetReddotCondition();
        }
        public static bool GetEventItemReddotCondition()
        {
            return IsAvailableEventItem();
        }
        public static bool GetEventQuestReddotCondition()
        {
            return IsAvailableRewardEventQuest();
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

            SetTitleImage();
            SetEventData();//팝업 열때 한번만
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
            PopupManager.Instance.Top.SetMagnetUI(User.Instance.ENABLE_P2E);//p2e 가능 지역 일때 표시
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

        public void SetEventData()
        {
            if (isFirstInit)
                return;

            var eventDataList = EventScheduleData.GetEventTypeData(eActionType.EVENT_LUCKY_BAG, true);
            if (eventDataList == null || eventDataList.Count <= 0)
                return;

            if (eventDataList.Count > 1)
                Debug.LogError("Data Table Error HOLIDAY TYPE Count : " + eventDataList.Count);

            scheduleData = eventDataList[0];
            eventData = eventDataList[0].EventBaseData as EventLuckyBagBaseData;

            SetScheduleText();

            isFirstInit = true;
        }

        void SetScheduleText()
        {
            if (eventScheduleText != null && scheduleData != null)
                eventScheduleText.text = StringData.GetStringByStrKey("복주머니이벤트기간");
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

        public void OnClickShowAttendancePopup()
        {
            if (eventData == null)
                return;

            var isPeriod = eventData.IsEventPeriod(false);
            if (!isPeriod)
            {
                ToastManager.On(StringData.GetStringByStrKey("이벤트종료안내2"));
                return;
            }

            EventAttendancePopup.OpenPopup(scheduleData);
        }

        public static bool GetReddotCondition()
        {
            return IsAvailableEventItem() || IsAvailableRewardEventQuest();
        }
        static bool IsAvailableRewardEventQuest()
        {
            int eventKey = -1;
            var eventData = LuckyBagEventPopup.GetEventData();
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

            if (_isSubQuest)//하위 퀘스트 조건만 체크
            {
                var isShowAD = QuestCondition.IsAdvertiseQuestType(_quest.GetSingleTriggerData().TYPE) && !User.Instance.ADVERTISEMENT_PASS;
                if (isShowAD)//questTrigger가 광고 타입 && 광고제거 없으면
                    return false;
            }

            if (_quest.IsQuestClear())//진행 중인 퀘스트만 체크
                return true;

            return false;
        }

        /// <summary>
        /// 복주머니가 하나라도 있으면
        /// </summary>
        /// <returns></returns>
        static bool IsAvailableEventItem()
        {
            return User.Instance.GetItem(LUCKY_BAG_ITEM_NO).Amount > 0;
        }

        public void OnClickHelpBtn()
        {
            var curTabLayer = tabController.GetCurrentTabLayer();
            if (curTabLayer == null)
                return;

            var subLayerIndex = curTabLayer.SubLayerIndex;
            if (subLayerIndex < 0)
                subLayerIndex = 0;

            PopupManager.OpenPopup<LuckyBagEventHelpPopup>(new HelpPopupData(tabController.CurTab + 1, subLayerIndex + 1));
        }

        public override void ClosePopup()
        {
            base.ClosePopup();

            UIObjectEvent.Event(UIObjectEvent.eEvent.EVENT_SHOW, UIObjectEvent.eUITarget.ALL);
            SetSubCamTextureOff();
            tabController.InitTabIndex();

            UICanvas.Instance.EndBackgroundBlurEffect();
        }

        public void OnEvent(LuckyBagUIEvent eventType)
        {
            switch (eventType.type)
            {
                case LuckyBagUIEvent.eLuckyBagUIType.REFRESH_TAB_REDDOT:
                    tabController.RefreshReddot();
                    break;
            }
        }
    }
}

