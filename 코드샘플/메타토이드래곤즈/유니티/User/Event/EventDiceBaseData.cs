using Newtonsoft.Json.Linq;
using SandboxNetwork;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct LOTTO_WINNER_INFO
{
    public long update_time;    //timeStamp 인지 물어보기
    public long user_no;        //userNo
    public string nick;         //userName

    public void Init()
    {
        update_time = 0;
        user_no = 0;
        nick = "";
    }
}

public class EventDiceBaseData : EventBaseData
{
    const int DICE_ITEM_NO = 20000009;
    const int BOX1_ITEM_NO = 20000010;
    const int BOX2_ITEM_NO = 20000011;
    const int BOX3_ITEM_NO = 20000012;

    /// <summary>
    /// gacha_type key 값
    /// </summary>
    int BOX1_GACHA_TYPE_KEY {
        get
        {
            return Convert.ToInt32(GameConfigData.Get("2023_HOLIDAY_BOX1").VALUE);
        }
    }
    int BOX2_GACHA_TYPE_KEY
    {
        get
        {
            return Convert.ToInt32(GameConfigData.Get("2023_HOLIDAY_BOX2").VALUE);
        }
    }

    int BOX3_GACHA_TYPE_KEY
    {
        get
        {
            return Convert.ToInt32(GameConfigData.Get("2023_HOLIDAY_BOX3").VALUE);
        }
    }

    public EventAttendanceData AttendanceData { get; private set; } = null;
    public EventDiceBaseData(EventScheduleData data) : base(data) {
        AttendanceData = new EventAttendanceData(data);
    }

    JObject curInfo = null;
    JObject lottoInfo = null;

    public int BoardIndex { get { return curInfo == null || !curInfo.ContainsKey("board_index") ? 0 : curInfo["board_index"].Value<int>(); } }
    public int DiceUsed { get { return curInfo == null || !curInfo.ContainsKey("dice_used") ? 0 : curInfo["dice_used"].Value<int>(); } }
    public int Box1Used { get { return curInfo == null || !curInfo.ContainsKey("box1_used") ? 0 : curInfo["box1_used"].Value<int>(); } }
    public int Box2Used { get { return curInfo == null || !curInfo.ContainsKey("box2_used") ? 0 : curInfo["box2_used"].Value<int>(); } }
    public int Box3Used { get { return curInfo == null || !curInfo.ContainsKey("box3_used") ? 0 : curInfo["box3_used"].Value<int>(); } }
    public LOTTO_WINNER_INFO LottoWinnerInfo 
    { 
        get 
        {
            var info = new LOTTO_WINNER_INFO();
            info.Init();

            if (lottoInfo == null)
                return info;

            if(SBFunc.IsJTokenType(lottoInfo, JTokenType.Object))
            {
                if(lottoInfo.ContainsKey("update_time"))
                    info.update_time = lottoInfo["update_time"].Value<long>();

                if (info.update_time <= 0)//있을 수 없는 일
                    return info;

                if (lottoInfo.ContainsKey("nick"))
                    info.nick = lottoInfo["nick"].Value<string>();
                if(lottoInfo.ContainsKey("user_no"))
                    info.user_no = lottoInfo["user_no"].Value<long>();
            }

            return info; 
        } 
    }

    public override void Clear()
    {
        curInfo = null;
        lottoInfo = null;
        AttendanceData.Clear();
    }

    public override void SetData(JObject jsonData)
    {
        if((int)eApiResCode.OK == jsonData["rs"].Value<int>())
        {
            if (jsonData.ContainsKey("event") && SBFunc.IsJArray(jsonData["event"]))
                AttendanceData.SetData(jsonData);

            if (jsonData.ContainsKey("info"))
                curInfo = (JObject)jsonData["info"];

            if (jsonData.ContainsKey("last_lottery"))
                lottoInfo = (JObject)jsonData["last_lottery"];
            else
                lottoInfo = null;
        }
    }

    public void OnDice(int count, NetworkManager.SuccessCallback cb = null, NetworkManager.FailCallback fail = null)
    {
        WWWForm form = new WWWForm();
        form.AddField("event_id", scheduleData.KEY);
        form.AddField("use_item", DICE_ITEM_NO);

        var isMulti = count > 1;

        form.AddField("use_count", count);

        NetworkManager.Send("event/dice", form, (JObject jsonData) =>
        {
            if (SBFunc.IsJTokenCheck(jsonData) && (int)eApiResCode.OK == jsonData["rs"].Value<int>())
            {
                if (isMulti)
                {
                    jsonData.Add("isMulti", isMulti);
                    jsonData.Add("use_count", count);
                }

                SetData(jsonData);
                cb?.Invoke(jsonData);
                //InventoryNoticeCheck(jsonData);//연출 끝나고 표시하는 상황이 있어서, 밖에서 사용으로 변경
            }
        }, (failData)=> {
            fail?.Invoke(failData);
        });
    }

    public void OpenBox1(NetworkManager.SuccessCallback cb = null, NetworkManager.FailCallback fail = null, int _requestCount = 1)
    {
        WWWForm form = new WWWForm();
        form.AddField("event_id", scheduleData.KEY);
        form.AddField("use_item", BOX1_ITEM_NO);
        form.AddField("use_count", _requestCount);

        NetworkManager.Send("event/dice", form, (JObject jsonData) =>
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

    public void OpenBox2(NetworkManager.SuccessCallback cb = null, NetworkManager.FailCallback fail = null, int _requestCount = 1)
    {
        WWWForm form = new WWWForm();
        form.AddField("event_id", scheduleData.KEY);
        form.AddField("use_item", BOX2_ITEM_NO);
        form.AddField("use_count", _requestCount);

        NetworkManager.Send("event/dice", form, (JObject jsonData) =>
        {
            if (SBFunc.IsJTokenCheck(jsonData) && (int)eApiResCode.OK == jsonData["rs"].Value<int>())
            {
                SetData(jsonData);
                cb?.Invoke(jsonData);
                InventoryNoticeCheck(jsonData);

                if (jsonData.ContainsKey("lottery") && jsonData["lottery"].Value<int>() == 1)//로또 체크
                {
                    SetLottoWinnerProcess();
                }
            }
        }, (failData) => {
            fail?.Invoke(failData);
        });
    }

    public void OpenBox3(NetworkManager.SuccessCallback cb = null, NetworkManager.FailCallback fail = null, int _requestCount = 1)
    {
        WWWForm form = new WWWForm();
        form.AddField("event_id", scheduleData.KEY);
        form.AddField("use_item", BOX3_ITEM_NO);
        form.AddField("use_count", _requestCount);

        NetworkManager.Send("event/dice", form, (JObject jsonData) =>
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
    /*
     * CUMULATIVE_EVENT = 1;
     * DAILY_EVENT = 2;
     * PVP_EVENT = 3;
     */
    public void RequestEventRankingPage(int page = 1, NetworkManager.SuccessCallback cb = null, NetworkManager.FailCallback fail = null)
    {
        WWWForm form = new WWWForm();
        switch(page)
        {
            case 1:
                form.AddField("event_key", (int)eOpenEventRankingPage.DICE_HOLIDAY);
                break;
            case 2:
                form.AddField("event_key", (int)eOpenEventRankingPage.DICE_HOLIDAY);
                form.AddField("is_daily", 1);
                break;
            case 3:
                form.AddField("event_key", (int)eOpenEventRankingPage.CHAMPIONSCUP);
                break;
        }

        NetworkManager.Send("event/ranking", form, (JObject jsonData) =>
        {
            if (SBFunc.IsJTokenCheck(jsonData) && (int)eApiResCode.OK == jsonData["rs"].Value<int>())
            {
                cb?.Invoke(jsonData);
            }
        }, (failData) => {
            fail?.Invoke(failData);
        });
    }

    public bool IsEventPeriod(bool _isUITime = true)
    {
        return User.Instance.EventData.IsEventPeriod(scheduleData, _isUITime);
    }

    public override bool IsNeedUpdate()
    {
        return AttendanceData.IsNeedUpdate();
    }
    public InventoryItem GetDiceItem()
    {
        return User.Instance.GetItem(DICE_ITEM_NO);
    }

    public InventoryItem GetBoxItemByIndex(int _index)
    {
        switch (_index)
        {
            case 1:
                return User.Instance.GetItem(BOX1_ITEM_NO);
            case 2:
                return User.Instance.GetItem(BOX2_ITEM_NO);
            case 3:
                return User.Instance.GetItem(BOX3_ITEM_NO);
        }

        return null;
    }
    public int GetGachaTypeKeyByIndex(int _index)
    {
        switch(_index)
        {
            case 1:
                return BOX1_GACHA_TYPE_KEY;
            case 2:
                return BOX2_GACHA_TYPE_KEY;
            case 3:
                return BOX3_GACHA_TYPE_KEY;
        }

        return -1;
    }

    /// <summary>
    /// 인벤 풀일 때 push - api "mail_sended" 가 날아오면 toast
    /// </summary>
    void InventoryNoticeCheck(JObject _jsonData)
    {
        if(IsMailSended(_jsonData))
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
    /// <summary>
    /// 현재 주사위 던질 수 있는 최대 카운트 갯수 - 인벤 기준 max보다 크면 max갯수로, 아니면 1개씩
    /// </summary>
    /// <returns></returns>
    int GetAvailableDiceCount()
    {
        var maxCount = Convert.ToInt32(GameConfigData.Get("2023_HOLIDAY_DICE_USE_MAX").VALUE);
        var invenDiceItem = GetDiceItem();

        if (invenDiceItem.Amount <= 0)//앞에서 선처리 해서 오진 않음
            return 0;

        if (maxCount > invenDiceItem.Amount)
            return invenDiceItem.Amount;
        else
            return maxCount;
    }

    /// <summary>
    /// 당첨자 플래그 확인 & 내가 당첨자라면 서버에 요청
    /// </summary>
    void SetLottoWinnerProcess()
    {
        var uno = User.Instance.UserAccountData.UserNumber;
        var userID = User.Instance.UserData.UserNick;
        var uPortraitNo = User.Instance.UserData.UserPortrait;
        var portraitType = ePortraitEtcType.RAID;
        var portraitValue = User.Instance.UserData.UserPortraitFrameInfo.GetValue(portraitType);

        var toastText = StringData.GetStringFormatByStrKey("시스템전체알림99", User.Instance.UserData.UserNick);
        ChatDataInfo data = new(eChatCommentType.SystemMsg, uno, userID, uPortraitNo, 0, "GB글로리", SBFunc.GetDateTimeToTimeStamp(), 0,
                SBFunc.GetNowDateTimeToTimeStamp(), toastText, (int)portraitType ,portraitValue);

        ChatManager.Instance.SendAchieveSystemMessage(eAchieveSystemMessageType.EVENT_LOTTO_WINNER, userID);//사이드 토스트
        ToastManager.OnSystem(data.Comment);//토스트
    }
}
