using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventScheduleData : GameData
{
    public int uid { get; private set; }
    public int use { get; private set; }
    public DateTime start_time { get; private set; }
    public DateTime end_time { get; private set; }
    public int event_type { get; private set; }
    public int use_background { get; private set; }

    public Sprite event_icon { get; private set; } = null;
    public DateTime ui_duration { get; private set; }

    public Sprite background_deco_path { get; private set; }
    public float attendance_bg_pos_x { get; private set; }
    public float attendance_bg_pos_y { get; private set; }

    public override void SetValue(Dictionary<string, string> data)
    {
        base.SetValue(data);

        uid = Int(data["uid"]);
        use = Int(data["use"]);
        start_time = DateTime.Parse(data["start_time"]);
        end_time = DateTime.Parse(data["end_time"]);
        event_type = Int(data["event_type"]);
        use_background = Int(data["use_background"]);

        if (!string.IsNullOrEmpty(data["event_icon"]))
            event_icon = Managers.Resource.LoadAssetsBundle<Sprite>(data["event_icon"]);
        if (!string.IsNullOrEmpty(data["ui_duration"]))
            ui_duration = DateTime.Parse(data["ui_duration"]);

        if (!string.IsNullOrEmpty(data["background_deco_path"]))
            background_deco_path = Managers.Resource.LoadAssetsBundle<Sprite>(data["background_deco_path"]);
        attendance_bg_pos_x = float.Parse(data["attendance_bg_pos_x"]) * 0.01f;
        attendance_bg_pos_y = float.Parse(data["attendance_bg_pos_y"]) * 0.01f;

    }

    public bool IsEventEnable()
    {
        bool ret = start_time < SBCommonLib.SBUtil.KoreanTime && end_time > SBCommonLib.SBUtil.KoreanTime;
        if (!ret)
            return false;

        switch (event_type)
        {
            case 3:
                if (AttendanceGameData.GetAttendanceDayCount(uid) < Managers.UserData.GetAttendanceDay(uid))
                    return false;
                break;
        }

        return true;
    }

    public void ShowAttendance(Popup.CloseCallback cb = null)
    {
        switch (uid)
        {
            case 2002:
                PopupCanvas.Instance.ShowPopup(PopupCanvas.POPUP_TYPE.ATTENDANCEMONTH_POPUP, cb);
                break;
            case 2001:
                PopupCanvas.Instance.ShowPopup(PopupCanvas.POPUP_TYPE.CHRISTMASATTENDANCE_POPUP, cb);
                break;
            default:
                PopupCanvas.Instance.ShowAttandancePopup(uid, cb);
                break;
        }
    }
}

public class EventBingoRewardData : GameData
{
    public int uid { get; private set; }
    public int group_uid { get; private set; }
    public int bingo_num { get; private set; }
    public int type { get; private set; }
    public int reward_num { get; private set; }
    public int rates { get; private set; }
    public int goods_id { get; private set; }

    public override void SetValue(Dictionary<string, string> data)
    {
        base.SetValue(data);

        uid = Int(data["uid"]);
        group_uid = Int(data["group_uid"]);
        bingo_num = Int(data["bingo_num"]);
        type = Int(data["type"]);
        reward_num = Int(data["reward_num"]);
        rates = Int(data["rates"]);
        goods_id = Int(data["goods_id"]);
    }
}

public class EventBingoInfoData : GameData
{
    public int uid { get; private set; }
    public int group_uid { get; private set; }
    public int event_item_uid { get; private set; }
    public int event_item_value { get; private set; }
    public int event_limit_num { get; private set; }

    public override void SetValue(Dictionary<string, string> data)
    {
        base.SetValue(data);

        uid = Int(data["uid"]);
        group_uid = Int(data["group_uid"]);
        event_item_uid = Int(data["event_item_uid"]);
        event_item_value = Int(data["event_item_value"]);
        event_limit_num = Int(data["event_limit_num"]);
    }
}
