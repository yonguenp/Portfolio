using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class fishing_reward : game_data
{
    public override GameDataManager.DATA_TYPE GetDataType() { return GameDataManager.DATA_TYPE.FISHING_REWARD; }

    static public fishing_reward[] GetFishingReward(uint week, uint fish_type)
    {
        fishing_reward[] ret = new fishing_reward[3];

        List<game_data> necoData = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.FISHING_REWARD);
        if (necoData == null)
        {
            return ret;
        }

        foreach (fishing_reward data in necoData)
        {
            if(data.GetWeek() == 1 && data.GetFishType() == fish_type)
            {
                uint index = data.GetRank() - 1;
                if (ret.Length > index)
                    ret[index] = data;
            }
        }

        return ret;
    }

    [NonSerialized]
    private uint week = 0;
    public uint GetWeek()
    {
        if (week == 0)
        {
            object obj;
            if (data.TryGetValue("week", out obj))
            {
                week = (uint)obj;
            }
        }

        return week;
    }

    [NonSerialized]
    private uint fish_type = 0;
    public uint GetFishType()
    {
        if (fish_type == 0)
        {
            object obj;
            if (data.TryGetValue("fish_type", out obj))
            {
                fish_type = (uint)obj;
            }
        }

        return fish_type;
    }

    [NonSerialized]
    private uint rank = 0;
    public uint GetRank()
    {
        if (rank == 0)
        {
            object obj;
            if (data.TryGetValue("rank", out obj))
            {
                rank = (uint)obj;
            }
        }

        return rank;
    }

    [NonSerialized]
    private items item = null;
    public items GetRewardItem()
    {
        if (item == null)
        {
            object obj;
            if (data.TryGetValue("item_id", out obj))
            {
                item = items.GetItem((uint)obj);
            }
        }

        return item;
    }

    [NonSerialized]
    private uint count = 0;
    public uint GetRewardCount()
    {
        if (count == 0)
        {
            object obj;
            if (data.TryGetValue("item_count", out obj))
            {
                count = (uint)obj;
            }
        }

        return count;
    }
}
