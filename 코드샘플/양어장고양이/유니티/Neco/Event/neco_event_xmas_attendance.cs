using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class neco_event_xmas_attendance : game_data
{
    public override GameDataManager.DATA_TYPE GetDataType() { return GameDataManager.DATA_TYPE.NECO_EVENT_XMAS_ATTENDANCE; }

    static public List<neco_event_xmas_attendance> GetNecoAttendanceList()
    {
        List<game_data> necoData = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.NECO_EVENT_XMAS_ATTENDANCE);
        if (necoData == null)
        {
            return null;
        }

        List<neco_event_xmas_attendance> attendanceList = new List<neco_event_xmas_attendance>();

        foreach (neco_event_xmas_attendance attendanceData in necoData)
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
    private string necoEventDesc;
    public string GetNecoEventDesc()
    {
        if (necoEventDesc == null)
        {
            object obj;
            if (data.TryGetValue("desc", out obj))
            {
                necoEventDesc = (string)obj;
            }
        }

        return necoEventDesc;
    }

    [NonSerialized]
    private uint necoEventDay = 0;
    public uint GetNecoEventShopDay()
    {
        if (necoEventDay == 0)
        {
            object obj;
            if (data.TryGetValue("day", out obj))
            {
                necoEventDay = (uint)obj;
            }
        }

        return necoEventDay;
    }

    [NonSerialized]
    private string necoEventItemType;
    public string GetNecoEventItemType()
    {
        if (necoEventItemType == null)
        {
            object obj;
            if (data.TryGetValue("item_type", out obj))
            {
                necoEventItemType = (string)obj;
            }
        }

        return necoEventItemType;
    }

    [NonSerialized]
    private uint necoEventItemID = 0;
    public uint GetNecoEventItemID()
    {
        if (necoEventItemID == 0)
        {
            object obj;
            if (data.TryGetValue("item_id", out obj))
            {
                necoEventItemID = (uint)obj;
            }
        }

        return necoEventItemID;
    }

    [NonSerialized]
    private uint necoEventItemCount = 0;
    public uint GetNecoEventItemCount()
    {
        if (necoEventItemCount == 0)
        {
            object obj;
            if (data.TryGetValue("count", out obj))
            {
                necoEventItemCount = (uint)obj;
            }
        }

        return necoEventItemCount;
    }
}