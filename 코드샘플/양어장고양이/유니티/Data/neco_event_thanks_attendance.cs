using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class neco_event_thanks_attendance : game_data
{
    public override GameDataManager.DATA_TYPE GetDataType() { return GameDataManager.DATA_TYPE.NECO_EVENT_THANKS_ATTENDANCE; }

    static public List<neco_event_thanks_attendance> GetNecoAttendanceList()
    {
        List<game_data> necoData = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.NECO_EVENT_THANKS_ATTENDANCE);
        if (necoData == null)
        {
            return null;
        }

        List<neco_event_thanks_attendance> attendanceList = new List<neco_event_thanks_attendance>();

        foreach (neco_event_thanks_attendance attendanceData in necoData)
        {
            if (attendanceData.GetNecoEventAttendanceItemID() != 143)   // 주사위는 제외함
            {
                attendanceList.Add(attendanceData);
            }
        }

        return attendanceList;
    }

    [NonSerialized]
    private uint necoEventAttendanceID = 0;
    public uint GetNecoEventAttendanceID()
    {
        if (necoEventAttendanceID == 0)
        {
            object obj;
            if (data.TryGetValue("id", out obj))
            {
                necoEventAttendanceID = (uint)obj;
            }
        }

        return necoEventAttendanceID;
    }

    [NonSerialized]
    private uint necoEventAttendanceDay = 0;
    public uint GetNecoEventAttendanceDay()
    {
        if (necoEventAttendanceDay == 0)
        {
            object obj;
            if (data.TryGetValue("day", out obj))
            {
                necoEventAttendanceDay = (uint)obj;
            }
        }

        return necoEventAttendanceDay;
    }

    [NonSerialized]
    private string necoEventAttendanceItemType;
    public string GetNecoEventAttendanceItemType()
    {
        if (necoEventAttendanceItemType == null)
        {
            object obj;
            if (data.TryGetValue("item_type", out obj))
            {
                necoEventAttendanceItemType = (string)obj;
            }
        }

        return necoEventAttendanceItemType;
    }

    [NonSerialized]
    private uint necoEventAttendanceItemID = 0;
    public uint GetNecoEventAttendanceItemID()
    {
        if (necoEventAttendanceItemID == 0)
        {
            object obj;
            if (data.TryGetValue("item_id", out obj))
            {
                necoEventAttendanceItemID = (uint)obj;
            }
        }

        return necoEventAttendanceItemID;
    }

    [NonSerialized]
    private uint necoEventAttendanceCount = 0;
    public uint GetNecoEventAttendanceCount()
    {
        if (necoEventAttendanceCount == 0)
        {
            object obj;
            if (data.TryGetValue("count", out obj))
            {
                necoEventAttendanceCount = (uint)obj;
            }
        }

        return necoEventAttendanceCount;
    }

    [NonSerialized]
    RewardData reward = null;
    public RewardData GetReward()
    {
        if (reward == null)
        {
            object obj;
            if (data.TryGetValue("item_type", out obj))
            {
                reward = new RewardData();
                switch ((string)obj)
                {
                    case "gold":
                        if (data.TryGetValue("count", out obj))
                        {
                            reward.gold = (uint)obj;
                        }
                        break;
                    case "dia":
                        if (data.TryGetValue("count", out obj))
                        {
                            reward.catnip = (uint)obj;
                        }
                        break;
                    case "item":
                        if (data.TryGetValue("item_id", out obj))
                        {
                            reward.itemData = items.GetItem((uint)obj);
                            if (data.TryGetValue("count", out obj))
                            {
                                reward.count = (uint)obj;

                            }
                        }
                        break;
                }

            }
        }

        return reward;
    }
}
