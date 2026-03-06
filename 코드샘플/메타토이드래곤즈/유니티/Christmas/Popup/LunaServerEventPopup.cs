using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class LunaServerEventPopup : Popup<TabTypePopupData>
    {

        public int[] BurningServerQuest { get; private set; } = new int[] { 5800016, 5800017, 5800018, 5800019, 5800020, 5800021, 5800022, 5800023, 5800024, 5800025, 5800026, 5800027, 5800028, 5800029, 5800030 };
        public int[] DragonTrainingQuest { get; private set; } = new int[] { 5800031, 5800032, 5800033, 5800034, 5800035, 5800036, 5800037, 5800038 };
        public int[] GemReinforceQuest { get; private set; } = new int[] { 5800039, 5800040, 5800041 };

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

        bool isFirstInit = false;
        public const int EVENT_KEY = 800004;
        #region OpenPopup
        public static LunaServerEventPopup OpenPopup(int tab, int subTab = -1)
        {
            return OpenPopup(new TabTypePopupData(tab, subTab));
        }
        public static LunaServerEventPopup OpenPopup(TabTypePopupData data)
        {
            if (data == null)
                return null;

            return PopupManager.OpenPopup<LunaServerEventPopup>(data);
        }
        public static void MoveTabForce(TabTypePopupData data, bool _force = true)
        {
            var popup = PopupManager.GetPopup<LunaServerEventPopup>();
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
        }

        private void OnDisable()
        {

        }

        public static bool GetLunaQuestReddotCondition(int index)
        {
            return IsAvailableRewardLunaQuest(index);
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

            if (Data != null && Data.SubIndex != -1)
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

            var HolidayTypeDataList = EventScheduleData.GetEventTypeData(eActionType.LUNASERVER_OPEN_EVENT, true);
            if (HolidayTypeDataList == null || HolidayTypeDataList.Count <= 0)
                return;

            scheduleData = HolidayTypeDataList[0];

            SetScheduleText();

            isFirstInit = true;
        }

        void SetScheduleText()
        {
            if (eventScheduleText != null && scheduleData != null)
            {
                //var startString = SBFunc.TimeCustomString(scheduleData.START_TIME, 3);
                //var endString = SBFunc.TimeCustomString(scheduleData.UI_END_TIME, 3);
                eventScheduleText.text = StringData.GetStringByStrKey("루나서버오픈이벤트기간");//string.Format("{0} ~ {1}", startString, endString);
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

        
        void SetEventScheduleTextNodeVisible(int tabIndex)
        {
            if (tabIndex == 2)
                eventScheduleNode.SetActive(false);
            else
                eventScheduleNode.SetActive(true);
        }

        static bool IsAvailableRewardLunaQuest(int index)
        {
            int eventKey = EVENT_KEY;

            var eventProceedList = QuestManager.Instance.GetProceedQuestDataByType(eQuestType.EVENT, eventKey);
            var completeList = QuestManager.Instance.GetCompleteQuestDataByType(eQuestType.EVENT, eventKey);

            eventProceedList.AddRange(completeList);
            eventProceedList.Sort((d1, d2) => d1.ID - d2.ID);//quest_base Key 오름차순


            if (eventProceedList == null || eventProceedList.Count <= 0)
                return false;

            LunaServerEventPopup popup = PopupManager.GetPopup<LunaServerEventPopup>();
            if (popup == null)
                return false;

            List<int> curQeustArray = null;
            switch(index)
            {
                case 0:
                    curQeustArray = popup.BurningServerQuest.ToList();
                    break;
                case 1:
                    curQeustArray = popup.DragonTrainingQuest.ToList();
                    break;
                case 2:
                    curQeustArray = popup.GemReinforceQuest.ToList();
                    break;
                case -1:
                    curQeustArray = new List<int>();
                    curQeustArray.AddRange(popup.BurningServerQuest.ToList());
                    curQeustArray.AddRange(popup.DragonTrainingQuest.ToList());
                    curQeustArray.AddRange(popup.GemReinforceQuest.ToList());
                    break;
                default:
                    return false;
            }

            foreach (var quest in eventProceedList)
            {
                if (!curQeustArray.Contains(quest.ID))
                    continue;
                if (IsGetRewardCondition(quest))
                {                    
                    return true;
                }
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

        public int GetCurTab()
        {
            return tabController.CurTab;
        }
    }
}
