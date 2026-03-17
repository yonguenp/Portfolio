using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class neco_subscribe : game_data
{
    public override GameDataManager.DATA_TYPE GetDataType() { return GameDataManager.DATA_TYPE.NECO_SUBSCRIBE; }

    static public List<neco_subscribe> GetNecoSubscribeListByID(uint subscribeID)
    {
        List<game_data> necoData = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.NECO_SUBSCRIBE);
        if (necoData == null)
        {
            return null;
        }

        List<neco_subscribe> subscribeList = new List<neco_subscribe>();

        object obj;
        foreach (neco_subscribe subData in necoData)
        {
            if (subData.data.TryGetValue("subscribe_id", out obj))
            {
                if (subscribeID == (uint)obj)
                {
                    subscribeList.Add(subData);
                }
            }
        }

        return subscribeList;
    }

    [NonSerialized]
    private uint necoSubscribeID = 0;
    public uint GetNecoSubcribeID()
    {
        if (necoSubscribeID == 0)
        {
            object obj;
            if (data.TryGetValue("subscribe_id", out obj))
            {
                necoSubscribeID = (uint)obj;
            }
        }

        return necoSubscribeID;
    }

    [NonSerialized]
    private string necoSubscribeDesc = "";
    public string GetNecoSubscribeDesc()
    {
        if (necoSubscribeDesc == "")
        {
            object obj;
            if (data.TryGetValue("subscribe_desc", out obj))
            {
                necoSubscribeDesc = (string)obj;
            }
        }

        return necoSubscribeDesc;
    }

    [NonSerialized]
    private string necoSubscribeItemType = "";
    public string GetNecoSubscribeItemType()
    {
        if (necoSubscribeItemType == "")
        {
            object obj;
            if (data.TryGetValue("item_type", out obj))
            {
                necoSubscribeItemType = (string)obj;
            }
        }

        return necoSubscribeItemType;
    }

    [NonSerialized]
    private uint necoSubscribeItemID = 0;
    public uint GetNecoSubcribeItemID()
    {
        if (necoSubscribeItemID == 0)
        {
            object obj;
            if (data.TryGetValue("item_id", out obj))
            {
                necoSubscribeItemID = (uint)obj;
            }
        }

        return necoSubscribeItemID;
    }

    [NonSerialized]
    private uint necoSubscribeItemCount = 0;
    public uint GetNecoSubcribeItemCount()
    {
        if (necoSubscribeItemCount == 0)
        {
            object obj;
            if (data.TryGetValue("count", out obj))
            {
                necoSubscribeItemCount = (uint)obj;
            }
        }

        return necoSubscribeItemCount;
    }
}
