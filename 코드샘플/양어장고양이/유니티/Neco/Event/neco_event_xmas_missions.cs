using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class neco_event_xmas_missions : game_data
{
    public override GameDataManager.DATA_TYPE GetDataType() { return GameDataManager.DATA_TYPE.NECO_EVENT_XMAS_ATTENDANCE; }

    static public List<neco_event_xmas_missions> GetNecoAttendanceList()
    {
        List<game_data> necoData = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.NECO_EVENT_XMAS_ATTENDANCE);
        if (necoData == null)
        {
            return null;
        }

        List<neco_event_xmas_missions> attendanceList = new List<neco_event_xmas_missions>();

        foreach (neco_event_xmas_missions attendanceData in necoData)
        {
            attendanceList.Add(attendanceData);
        }

        return attendanceList;
    }

    [NonSerialized]
    private uint necoEventID = 0;
    public uint GetNecoEventID()
    {
        if (necoEventID == 0)
        {
            object obj;
            if (data.TryGetValue("id", out obj))
            {
                necoEventID = (uint)obj;
            }
        }

        return necoEventID;
    }

    [NonSerialized]
    private string necoEventTriggerType;
    public string GetNecoEventTriggerType()
    {
        if (necoEventTriggerType == null)
        {
            object obj;
            if (data.TryGetValue("desc", out obj))
            {
                necoEventTriggerType = (string)obj;
            }
        }

        return necoEventTriggerType;
    }

    [NonSerialized]
    private uint necoEventNeedCount = 0;
    public uint GetNecoEventNeedCount()
    {
        if (necoEventNeedCount == 0)
        {
            object obj;
            if (data.TryGetValue("count", out obj))
            {
                necoEventNeedCount = (uint)obj;
            }
        }

        return necoEventNeedCount;
    }

    [NonSerialized]
    private string necoEventRewardType;
    public string GetNecoEventRewardType()
    {
        if (necoEventRewardType == null)
        {
            object obj;
            if (data.TryGetValue("reward_type", out obj))
            {
                necoEventRewardType = (string)obj;
            }
        }

        return necoEventRewardType;
    }

    [NonSerialized]
    private uint necoEventRewardID = 0;
    public uint GetNecoEventRewardID()
    {
        if (necoEventRewardID == 0)
        {
            object obj;
            if (data.TryGetValue("reward_id", out obj))
            {
                necoEventRewardID = (uint)obj;
            }
        }

        return necoEventRewardID;
    }

    [NonSerialized]
    private uint necoEventRewardCount = 0;
    public uint GetNecoEventRewardCount()
    {
        if (necoEventRewardCount == 0)
        {
            object obj;
            if (data.TryGetValue("reward_count", out obj))
            {
                necoEventRewardCount = (uint)obj;
            }
        }

        return necoEventRewardCount;
    }
}
