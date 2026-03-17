using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class neco_box : game_data
{
    public override GameDataManager.DATA_TYPE GetDataType() { return GameDataManager.DATA_TYPE.NECO_BOX; }

    static public List<neco_box> GetNecoBoxListByBoxID(uint boxID)
    {
        List<game_data> necoData = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.NECO_BOX);
        if (necoData == null)
        {
            return null;
        }

        List<neco_box> boxList = new List<neco_box>();

        object obj;
        foreach (neco_box boxData in necoData)
        {
            if (boxData.data.TryGetValue("box_id", out obj))
            {
                if (boxID == (uint)obj)
                {
                    boxList.Add(boxData);
                }
            }
        }

        return boxList;
    }

    [NonSerialized]
    private uint necoBoxID = 0;
    public uint GetNecoBoxID()
    {
        if (necoBoxID == 0)
        {
            object obj;
            if (data.TryGetValue("id", out obj))
            {
                necoBoxID = (uint)obj;
            }
        }

        return necoBoxID;
    }

    [NonSerialized]
    private string necoBoxDesc = "";
    public string GetNecoBoxDesc()
    {
        if (necoBoxDesc == "")
        {
            object obj;
            if (data.TryGetValue("desc", out obj))
            {
                necoBoxDesc = (string)obj;
            }
        }

        return necoBoxDesc;
    }

    [NonSerialized]
    private string necoBoxRewardType = "";
    public string GetNecoBoxRewardType()
    {
        if (necoBoxRewardType == "")
        {
            object obj;
            if (data.TryGetValue("reward_type", out obj))
            {
                necoBoxRewardType = (string)obj;
            }
        }

        return necoBoxRewardType;
    }

    [NonSerialized]
    private uint necoBoxRewardID = 0;
    public uint GetNecoRewardID()
    {
        if (necoBoxRewardID == 0)
        {
            object obj;
            if (data.TryGetValue("reward_id", out obj))
            {
                necoBoxRewardID = (uint)obj;
            }
        }

        return necoBoxRewardID;
    }

    [NonSerialized]
    private uint necoBoxRewardCount = 0;
    public uint GetNecoRewardCount()
    {
        if (necoBoxRewardCount == 0)
        {
            object obj;
            if (data.TryGetValue("reward_count", out obj))
            {
                necoBoxRewardCount = (uint)obj;
            }
        }

        return necoBoxRewardCount;
    }
}
