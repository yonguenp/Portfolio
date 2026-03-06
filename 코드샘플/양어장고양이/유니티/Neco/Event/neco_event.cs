using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class neco_event
{
    public enum EVENT_TYPE
    { 
        UNKOWN = 0,
        CHUSEOK = 1,
        HALLOWEEN = 2,
        CHRISTMAS = 3,
    };

    private uint event_id;
    protected uint start_time;
    protected uint end_time;


    public static neco_event CreateEvent(JObject data)
    {
        neco_event ret;
        uint eid = data["eid"].Value<uint>();
        switch ((EVENT_TYPE)eid)
        {
            case EVENT_TYPE.CHUSEOK://2021 추석이벤트
                ret = new chuseok_event();
                break;
            case EVENT_TYPE.HALLOWEEN://2021 할로윈 이벤트
                ret = new halloween_event();
                break;
            case EVENT_TYPE.CHRISTMAS://2021 크리스마스 이벤트
                ret = new christmas_event();
                break;
            default:
                ret = new neco_event();
                break;
        }

        ret.SetEventData(data);

        return ret;
    }
    
    public virtual void SetEventData(JObject data)
    {
        event_id = data["eid"].Value<uint>();
        start_time = data["start"].Value<uint>();
        end_time = data["end"].Value<uint>();
    }

    public bool IsEventTime()
    {
        return NecoCanvas.GetCurTime() > start_time && NecoCanvas.GetCurTime() < end_time;
    }

    public uint GetEventID()
    {
        return event_id;
    }

    public uint GetEventStartTime()
    {
        return start_time;
    }

    public uint GetEventEndTime()
    {
        return end_time;
    }
}

public class chuseok_event : neco_event
{
    private uint midtime;
    

    public class chuseok_marble_data
    {
        public uint back_dice_max;
        public uint front_dice_max;
        public uint touch_dice_max;

        public uint back_dice_chance;
        public uint front_dice_chance;
        public uint touch_dice_chance;

        public uint curSelectCat;
        public uint curDiceCount;
        public uint curMapPos;

        public List<uint> mapType = new List<uint>(50);
    };
    private chuseok_marble_data marble_data = new chuseok_marble_data();

    public class chuseok_attendance
    {
        public uint totalAttendanceDays;
        public bool enableAttendance;
    };
    private chuseok_attendance attendance_data = new chuseok_attendance();

    public class chuseok_shop_data
    {
        public Dictionary<uint, int> saleDic = new Dictionary<uint, int>();
    }
    private chuseok_shop_data shop_data = new chuseok_shop_data();

    public override void SetEventData(JObject data)
    {
        base.SetEventData(data);

        if (data.ContainsKey("info"))
        {
            JObject info = (JObject)data["info"];

            midtime = info["midtime"].Value<uint>();
            end_time = info["end"].Value<uint>();
            attendance_data.enableAttendance = info["att"].Value<uint>() > 0;
            if(info.ContainsKey("dice_ltd"))
            {
                SetDiceChanceInfo((JObject)info["dice_ltd"]);
            }

            if (info.ContainsKey("dice_max"))
            {
                SetMaxDiceInfo((JObject)info["dice_max"]);
            }
        }
    }

    void SetDiceChanceInfo(JObject data)
    {
        marble_data.back_dice_chance = data["back"].Value<uint>();
        marble_data.front_dice_chance = data["front"].Value<uint>();
        marble_data.touch_dice_chance = data["touch"].Value<uint>();
    }

    void SetMaxDiceInfo(JObject data)
    {
        marble_data.back_dice_max = data["back"].Value<uint>();
        marble_data.front_dice_max = data["front"].Value<uint>();
        marble_data.touch_dice_max = data["touch"].Value<uint>();
    }

    public void SetMarbleData(JObject data, int op)
    {
        if (op == 10)
        {
            JObject info = (JObject)data["info"];

            marble_data.curMapPos = info["pos"].Value<uint>();
            marble_data.curSelectCat = info["neco"].Value<uint>();
            marble_data.mapType.Clear();

            string str = info["map"].Value<string>();
            for (int i = 0; i < str.Length; i++)
            {
                marble_data.mapType.Add(uint.Parse(str[i].ToString()));
            }
            if (info.ContainsKey("dice_ltd"))
            {
                SetDiceChanceInfo((JObject)info["dice_ltd"]);
            }

            if (info.ContainsKey("dice_max"))
            {
                SetMaxDiceInfo((JObject)info["dice_max"]);
            }
        }
        if (op == 11)
        {
            JToken resultCode = data["rs"];
            if (resultCode == null || resultCode.Type != JTokenType.Integer)
                return;

            int rs = resultCode.Value<int>();
            if (rs != 0)
                return;

            marble_data.curMapPos = data["pos"].Value<uint>();
            marble_data.curSelectCat = data["neco"].Value<uint>();
            marble_data.mapType.Clear();

            string str = data["map"].Value<string>();
            for (int i = 0; i < str.Length; i++)
            {
                marble_data.mapType.Add(uint.Parse(str[i].ToString()));
            }
        }
        if (op == 12)
        {
            marble_data.curMapPos = data["pos"].Value<uint>();
        }

    }

    public void SetAttendanceData(JObject data)
    {
        JObject info = (JObject)data["info"];

        attendance_data.totalAttendanceDays = info["checked"].Value<uint>();
        attendance_data.enableAttendance = info["today"].Value<uint>() > 0;

        if (info.ContainsKey("dice_ltd"))
        {
            SetDiceChanceInfo((JObject)info["dice_ltd"]);
        }

        if (info.ContainsKey("dice_max"))
        {
            SetMaxDiceInfo((JObject)info["dice_max"]);
        }
    }

    public void SetShopData(JObject data)
    {
        if (data.ContainsKey("stock"))
        {
            JObject shopItem = (JObject)data["stock"];
            foreach (JProperty property in shopItem.Properties())
            {
                uint prodid = uint.Parse(property.Name);
                int stock = property.Value.Value<int>();
                shop_data.saleDic[prodid] = stock;
            }
        }
    }

    public chuseok_marble_data GetMarbleData()
    {
        return marble_data;
    }

    public chuseok_attendance GetAttendanceData()
    {
        return attendance_data;
    }

    public chuseok_shop_data GetShopData()
    {
        return shop_data;
    }

    public bool IsRealEventTime()
    {
        return NecoCanvas.GetCurTime() > start_time && NecoCanvas.GetCurTime() < midtime;
    }

    public bool IsEnableMarble()
    {
        return IsRealEventTime();
    }

    public bool IsEnablePackage()
    {
        return IsRealEventTime();
    }

    public bool IsEnableShop()
    {
        return IsEventTime();
    }

    public bool IsEnableAttendance()
    {
        return IsRealEventTime();
    }

    public string GetEventTimeString()
    {
        DateTime startTime = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(start_time);
        DateTime endTime = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(end_time);

        return startTime.Month + "." + startTime.Day + " ~ " + endTime.Month + "." + endTime.Day;
    }

    public string GetRealEventTimeString()
    {
        DateTime startTime = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(start_time);
        DateTime endTime = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(midtime);

        return startTime.Month + "." + startTime.Day + " ~ " + endTime.Month + "." + endTime.Day;
    }
}


public class halloween_event : neco_event
{
    public class halloween_attendance
    {
        public uint totalAttendanceDays;
        public bool enableAttendance;
    };
    private halloween_attendance attendance_data = new halloween_attendance();

    public override void SetEventData(JObject data)
    {
        base.SetEventData(data);

        if (data.ContainsKey("info"))
        {
            JObject info = (JObject)data["info"];

            end_time = info["end"].Value<uint>();
            attendance_data.enableAttendance = info["att"].Value<uint>() > 0;            
        }
    }

    public void SetAttendanceData(JObject data)
    {
        JObject info = (JObject)data["info"];

        attendance_data.totalAttendanceDays = info["checked"].Value<uint>();
        attendance_data.enableAttendance = info["today"].Value<uint>() > 0;
    }

    public halloween_attendance GetAttendanceData()
    {
        return attendance_data;
    }

    public bool IsEnableAttendance()
    {
        return IsEventTime();
    }

    public string GetEventTimeString()
    {
        DateTime startTime = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(start_time);
        DateTime endTime = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(end_time);

        return startTime.Month + "." + startTime.Day + " ~ " + endTime.Month + "." + endTime.Day;
    }
}

public class christmas_event : neco_event
{
    public class christmas_attendance
    {
        public uint totalAttendanceDays;
        public bool enableAttendance;
    };
    private christmas_attendance attendance_data = new christmas_attendance();

    public override void SetEventData(JObject data)
    {
        base.SetEventData(data);

        if (data.ContainsKey("info"))
        {
            JObject info = (JObject)data["info"];

            end_time = info["end"].Value<uint>();
            attendance_data.enableAttendance = info["att"].Value<uint>() > 0;
        }
    }

    public void SetAttendanceData(JObject data)
    {
        JObject info = (JObject)data["info"];

        attendance_data.totalAttendanceDays = info["checked"].Value<uint>();
        attendance_data.enableAttendance = info["today"].Value<uint>() > 0;
    }

    public christmas_attendance GetAttendanceData()
    {
        return attendance_data;
    }

    public bool IsEnableAttendance()
    {
        return IsEventTime();
    }

    public string GetEventTimeString()
    {
        DateTime startTime = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(start_time);
        DateTime endTime = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(end_time);

        return startTime.Month + "." + startTime.Day + " ~ " + endTime.Month + "." + endTime.Day;
    }
}