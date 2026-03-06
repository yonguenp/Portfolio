using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    /// <summary>
    /// 복주머니(구정이벤트) 관련 이벤트 데이터
    /// </summary>
    /// 

    public enum eLuckyBagEventState
    {
        REQUEST_INFO,
        REQUEST_BAG_REINFORCE,
        REQUEST_GET_REWARD,
        REQUEST_RANKING,
    }


    public class EventLuckyBagBaseData : EventBaseData
    {
        const int LUCKY_BAG_ITEM_NO = 20000008;

        public EventAttendanceData AttendanceData { get; private set; } = null;
        public EventLuckyBagBaseData(EventScheduleData data) : base(data)
        {
            AttendanceData = new EventAttendanceData(data);
        }

        JObject curInfo = null;//복주머니 관련 데이터 세팅

        //클라쪽 전체 투입량 안보여주기로 결정해서 안씀.
        /// <summary>
        /// 복주머니 투입량
        /// </summary>
        //public int InputCount { get { return curInfo == null || !curInfo.ContainsKey("input_count") ? 0 : curInfo["input_count"].Value<int>(); } }
        /// <summary>
        /// 복주머니 잔여갯수
        /// </summary>
        public int RemainCount { get { return curInfo == null || !curInfo.ContainsKey("remain") ? 0 : curInfo["remain"].Value<int>(); } }
        /// <summary>
        /// 복주머니 강화 단계
        /// </summary>
        public int ReinforceStep { get { return curInfo == null || !curInfo.ContainsKey("step") ? 0 : curInfo["step"].Value<int>(); } }
        /// <summary>
        /// 복주머니 누적 점수
        /// </summary>
        public int LuckyBagPoint { get { return curInfo == null || !curInfo.ContainsKey("point") ? 0 : curInfo["point"].Value<int>(); } }
        

        public override void SetData(JObject jsonData)
        {
            if ((int)eApiResCode.OK == jsonData["rs"].Value<int>())
            {
                if (jsonData.ContainsKey("event") && SBFunc.IsJArray(jsonData["event"]))
                    AttendanceData.SetData(jsonData);

                if (jsonData.ContainsKey("info"))
                    curInfo = (JObject)jsonData["info"];
            }
        }

        public void RequestToServer(eLuckyBagEventState _state,NetworkManager.SuccessCallback cb = null, NetworkManager.FailCallback fail = null, int _paramIndex = 1)
        {
            switch(_state)
            {
                case eLuckyBagEventState.REQUEST_INFO:
                    break;
                case eLuckyBagEventState.REQUEST_BAG_REINFORCE:
                    RequestReinforceLuckyBag(_state, _paramIndex, cb, fail);
                    break;
                case eLuckyBagEventState.REQUEST_GET_REWARD://0강 일때 보상 요청 하는 기능 추가로 갯수 추가
                    RequestGetRewardLuckyBag(_state, _paramIndex, cb, fail);
                    break;
                case eLuckyBagEventState.REQUEST_RANKING:
                    RequestEventRankingPage(_state, _paramIndex, cb, fail);
                    break;
            }
        }
        /// <summary>
        /// 복주머니 오픈 api
        /// </summary>
        /// <param name="cb"></param>
        /// <param name="fail"></param>
        /// <param name="_requestCount"></param>
        void RequestReinforceLuckyBag(eLuckyBagEventState _state,int _inputCount = 1, NetworkManager.SuccessCallback cb = null, NetworkManager.FailCallback fail = null)
        {
            WWWForm form = new WWWForm();
            form.AddField("op", (int)_state);
            form.AddField("event_id", GetScheduleDataKey());
            form.AddField("use_count", _inputCount);
            NetworkManager.Send("event/newyear2024", form, (JObject jsonData) =>
            {
                if (SBFunc.IsJTokenCheck(jsonData) && (int)eApiResCode.OK == jsonData["rs"].Value<int>())
                {
                    var prevCount = RemainCount;//직전 remainCount 임시 저장
                    SetData(jsonData);

                    var failCount = prevCount - RemainCount;//직전 카운트 - 현재 남은 카운트 = 실패 갯수

                    if(RemainCount == 0)
                    {
                        ToastManager.On(StringData.GetStringByStrKey("복주머니강화실패문구"));
                    }

                    jsonData.Add("fail", failCount);
                    cb?.Invoke(jsonData);

                    if(jsonData.ContainsKey("info") && jsonData["info"].Type == JTokenType.Object)
                    {
                        JObject info = (JObject)jsonData["info"];
                        if (info.ContainsKey("step"))
                        {
                            if (info["step"].Value<int>() >= 10)
                            {
                                var userID = User.Instance.UserData.UserNick;
                                ChatManager.Instance.SendAchieveSystemMessage(eAchieveSystemMessageType.EVENT_POCKET_LEVEL10, userID);//사이드 토스트
                            }
                        }
                    }
                }
            }, (failData) => {
                fail?.Invoke(failData);
            });
        }
        /// <summary>
        /// 복주머니 강화 보상 받기
        /// </summary>
        /// <param name="cb"></param>
        /// <param name="fail"></param>
        void RequestGetRewardLuckyBag(eLuckyBagEventState _state,int _inputCount,  NetworkManager.SuccessCallback cb = null, NetworkManager.FailCallback fail = null)
        {
            WWWForm form = new WWWForm();
            form.AddField("op", (int)_state);
            form.AddField("event_id", GetScheduleDataKey());

            if(ReinforceStep == 0)
                form.AddField("use_count", _inputCount);

            NetworkManager.Send("event/newyear2024", form, (JObject jsonData) =>
            {
                if (SBFunc.IsJTokenCheck(jsonData) && (int)eApiResCode.OK == jsonData["rs"].Value<int>())
                {
                    SetData(jsonData);
                    cb?.Invoke(jsonData);
                    InventoryNoticeCheck(jsonData);
                }
            }, (failData) => {
                fail?.Invoke(failData);
            });
        }

        void RequestEventRankingPage(eLuckyBagEventState _state, int page = 1, NetworkManager.SuccessCallback cb = null, NetworkManager.FailCallback fail = null)
        {
            //api 제작 대기
            return;

            WWWForm form = new WWWForm();
            form.AddField("page", page);

            NetworkManager.Send("event/event_ranking", form, (JObject jsonData) =>
            {
                if (SBFunc.IsJTokenCheck(jsonData) && (int)eApiResCode.OK == jsonData["rs"].Value<int>())
                {
                    cb?.Invoke(jsonData);
                }
            }, (failData) => {
                fail?.Invoke(failData);
            });
        }

        /// <summary>
        /// 인벤 풀일 때 push - api "mail_sended" 가 날아오면 toast
        /// </summary>
        void InventoryNoticeCheck(JObject _jsonData)
        {
            if (IsMailSended(_jsonData))
                ToastManager.On(StringData.GetStringByStrKey("보상아이템우편발송"));
        }

        public bool IsMailSended(JObject _jsonData)
        {
            if (_jsonData != null && _jsonData.ContainsKey("push"))
            {
                if (SBFunc.IsJArray(_jsonData["push"]))
                {
                    JArray pushArray = (JArray)_jsonData["push"];
                    if (pushArray == null)
                        return false;

                    var arrayCount = pushArray.Count;
                    for (var i = 0; i < arrayCount; ++i)
                    {
                        JObject jObject = (JObject)pushArray[i];

                        if (!SBFunc.IsJTokenCheck(jObject["api"]))
                            continue;

                        switch (jObject["api"].Value<string>())
                        {
                            case "mail_sended":
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }
        public bool IsEventPeriod(bool _isUITime = true)
        {
            return User.Instance.EventData.IsEventPeriod(scheduleData, _isUITime);
        }

        public override bool IsNeedUpdate()
        {
            return AttendanceData.IsNeedUpdate();
        }

        public InventoryItem GetEventItem()
        {
            return User.Instance.GetItem(LUCKY_BAG_ITEM_NO);
        }

        public override void Clear()
        {
            AttendanceData.Clear();
        }
    }
}
