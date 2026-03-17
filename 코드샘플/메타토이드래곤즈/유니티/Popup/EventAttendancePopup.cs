using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class EventAttendancePopupData : PopupData
    {
        public EventScheduleData Data { get; private set; } = null;
        public EventAttendancePopupData(EventScheduleData data)
        {
            Data = data;
        }
    }
    /// <summary>
    /// 이벤트 용 출석 체크 (기본 일주일)
    /// </summary>
    public class EventAttendancePopup : AttendancePopup
    {
        [SerializeField] Image iconImage = null;
        [SerializeField] Image BGImage = null;
        [SerializeField] Image popupImage = null;

        [SerializeField] Sprite defaultImage = null;
        [SerializeField] Sprite defaultPopupImage = null;


        bool isFirstInitSprite = false;
        EventScheduleData ScheduleData { get { return ((EventAttendancePopupData)Data)?.Data; } }
        EventAttendanceData AttendanceData {
            get {
                if (ScheduleData == null)
                    return null;

                switch (ScheduleData.TYPE)
                {
                    case eActionType.EVENT_ATTENDANCE:
                        return (EventAttendanceData)ScheduleData.EventBaseData;
                    case eActionType.EVENT_DICE:
                        return ((EventDiceBaseData)ScheduleData.EventBaseData).AttendanceData;
                    case eActionType.EVENT_LUCKY_BAG:
                        return ((EventLuckyBagBaseData)ScheduleData.EventBaseData).AttendanceData;
                }

                return null;
            } 
        } 
        #region OpenPopup
        public static EventAttendancePopup OpenPopup(EventScheduleData data, System.Action action = null)
        {
            var popup = PopupManager.OpenPopup<EventAttendancePopup>(new EventAttendancePopupData(data));
            if (popup != null)
            {
                popup.SetExitCallback(action);
            }

            return popup;
        }
        public static bool CheckEventAttendance(System.Action action = null)
        {
            foreach (var data in User.Instance.EventData.GetActiveEvents(false))
            {
                if (data.TYPE == eActionType.EVENT_ATTENDANCE || data.TYPE == eActionType.EVENT_DICE
                    || data.TYPE == eActionType.EVENT_LUCKY_BAG)
                {
                    bool availableReward = User.Instance.EventData.IsNeedUpdate(data);//받기 가능한 시간인가
                    if (availableReward)
                    {
                        WWWForm form = new WWWForm();
                        form.AddField("event_id", data.KEY);

                        NetworkManager.Send("event/eventcheck", form, (JObject jsonData) =>
                        {
                            if (SBFunc.IsJTokenCheck(jsonData) && (int)eApiResCode.OK == jsonData["rs"].Value<int>())
                            {
                                User.Instance.EventData.SetData(data, jsonData);

                                if(jsonData.ContainsKey("event"))
                                {
                                    if (jsonData["event"].Type == JTokenType.Array)
                                    {
                                        JArray eventData = (JArray)jsonData["event"];
                                        if (eventData.Count > 3)
                                        {
                                            if (eventData[3].Value<bool>())
                                                OpenPopup(data, action);
                                        }
                                    }
                                    else
                                    {
                                        OpenPopup(data, action);
                                    }
                                }
                                
                            }
                        });

                        return true;
                    }
                }
            }

            return false;
        }

        public static void RequestEventAttendance()//startLoading Scene 에서 미리 데이터 불러옴.
        {
            foreach (var data in User.Instance.EventData.GetActiveEvents(false))
            {
                if (data.TYPE == eActionType.EVENT_ATTENDANCE || data.TYPE == eActionType.EVENT_DICE
                    || data.TYPE == eActionType.EVENT_LUCKY_BAG)
                {
                    WWWForm form = new WWWForm();
                    form.AddField("event_id", data.KEY);

                    NetworkManager.Send("event/eventlist", form, (JObject jsonData) =>
                    {
                        if (SBFunc.IsJTokenCheck(jsonData) && (int)eApiResCode.OK == jsonData["rs"].Value<int>())
                        {
                            if (SBFunc.IsJArray(jsonData["event"]))
                            {
                                User.Instance.EventData.SetData(data, jsonData);//이전 정보(이전에 출첵 완료 정보)
                            }
                        }
                    });
                }
            }
        }

        #endregion
        [SerializeField] private Text RewardDesc = null;//이벤트 노티 기간용 라벨
        [SerializeField] private Text TimeText = null;//이벤트 노티 기간용 라벨

        public override void InitUI()
        {
            InitTableUI();
            base.InitUI();
            SetRemainText();
        }

        void InitTableUI()
        {
            if (iconImage == null || BGImage == null || popupImage == null)
                return;

            if (ScheduleData == null)
                return;

            var resourceData = EventAttendanceResourceData.Get(int.Parse(ScheduleData.KEY));
            if (resourceData != null)
            {
                var icon = resourceData.ICON_SPRITE;
                iconImage.sprite = icon == null ? defaultImage : icon;
                iconImage.SetNativeSize();

                var bg = resourceData.BG_SPRITE;
                BGImage.sprite = bg == null ? defaultImage : bg;

                var popup = resourceData.POPUP_SPRITE;
                popupImage.sprite = popup == null ? defaultPopupImage : popup;

                if (TimeText != null)
                    TimeText.color = resourceData.DESC_COLOR;

                if (RewardDesc != null)
                    RewardDesc.color = resourceData.REWARD_DESC_HEX;
            }
            else
            {
                iconImage.sprite = defaultImage;
                BGImage.sprite = defaultImage;
                popupImage.sprite = defaultPopupImage;

                if (TimeText != null)
                    TimeText.color = new Color(56f / 255f, 70f / 255f, 117f / 255f);

                if (RewardDesc != null)
                    RewardDesc.color = Color.white;
            }
        }

        protected override IEnumerator OpenAnimation()
        {
            yield return OpenPopupAnim();

            if (clones == null)
                yield break;

            var day = AttendanceData.AttendanceDay - 1;
            if (clones.Count <= day || day < 0)
                yield break;

            if (clones[day] == null)
                yield break;

            clones[day].StartAnim();
        }

        protected override void SetAttendenceFlag()
        {
            if (AttendanceData == null)
                return;

            IsAttendance = AttendanceData.IsAttendance;
            AttendanceData.CheckAttendance();
        }

        protected override void InitializeTitle()//타이틀 고정이어도 될것같긴함.
        {
            if (titleText != null && AttendanceData != null)
                titleText.text = AttendanceData.GetEventTitleString();
        }
        protected override void InitializeClone()
        {
            var group = EventRewardData.GetGroup(AttendanceData.GetEventKey());//현재 진행중 데이터의 키값
            if (group != null)
            {
                for (int i = 0, count = group.Count; i < count; ++i)
                {
                    var data = group[i];
                    if (data == null)
                        continue;

                    if (clones.Count <= i)
                        break;

                    int slotColorIndex = 0;
                    if (data.RARITY >= 0 && boxSprites.Length > data.RARITY)
                        slotColorIndex = data.RARITY;
                        
                    clones[i].SetData(AttendanceData, data, boxSprites[slotColorIndex], IsAttendance);
                }
            }
        }

        void SetRemainText()//시간 안씀. 단순 종료 기간 표기용.
        {
            if(TimeText != null)
                TimeText.text = AttendanceData.GetEventEndDateTimeString();
        }

        public override void ClosePopup()
        {
            base.ClosePopup();
        }

        public override void OnClickDimd()
        {
            base.OnClickDimd();
        }
    }
}
