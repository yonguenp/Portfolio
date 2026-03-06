using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class neco_event_xmas_gacha : game_data
{
    public override GameDataManager.DATA_TYPE GetDataType() { return GameDataManager.DATA_TYPE.NECO_EVENT_XMAS_GACHA; }

    static public List<neco_event_xmas_gacha> GetNecoGachaList()
    {
        List<game_data> necoData = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.NECO_EVENT_XMAS_GACHA);
        if (necoData == null)
        {
            return null;
        }

        List<neco_event_xmas_gacha> dataList = new List<neco_event_xmas_gacha>();

        foreach (neco_event_xmas_gacha gachaData in necoData)
        {
            dataList.Add(gachaData);
        }

        return dataList;
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
    private uint necoEventGroup_ID = 0;
    public uint GetNecoEventGroup_ID()
    {
        if (necoEventGroup_ID == 0)
        {
            object obj;
            if (data.TryGetValue("group_id", out obj))
            {
                necoEventGroup_ID = (uint)obj;
            }
        }

        return necoEventGroup_ID;
    }

    [NonSerialized]
    private uint necoEventWeight = 0;
    public uint GetNecoEventWeight()
    {
        if (necoEventWeight == 0)
        {
            object obj;
            if (data.TryGetValue("weight", out obj))
            {
                necoEventWeight = (uint)obj;
            }
        }

        return necoEventWeight;
    }

    [NonSerialized]
    private string necoEventRewtype;
    public string GetNecoEventRew_type()
    {
        if (necoEventRewtype == null)
        {
            object obj;
            if (data.TryGetValue("rew_type", out obj))
            {
                necoEventRewtype = (string)obj;
            }
        }

        return necoEventRewtype;
    }

    [NonSerialized]
    private uint necoEventRewID = 0;
    public uint GetNecoEventRewID()
    {
        if (necoEventRewID == 0)
        {
            object obj;
            if (data.TryGetValue("rew_id", out obj))
            {
                necoEventRewID = (uint)obj;
            }
        }

        return necoEventRewID;
    }

    [NonSerialized]
    private uint necoEventRewCount = 0;
    public uint GetNecoEventRewCount()
    {
        if (necoEventRewCount == 0)
        {
            object obj;
            if (data.TryGetValue("rew_cnt", out obj))
            {
                necoEventRewCount = (uint)obj;
            }
        }

        return necoEventRewCount;
    }

    [NonSerialized]
    private string necoEventDesc;
    public string GetNecoEventDesc()
    {
        if (necoEventDesc == null)
        {
            object obj;
            if (data.TryGetValue("rew_cnt", out obj))
            {
                necoEventDesc = (string)obj;
            }
        }

        return necoEventDesc;
    }
}
