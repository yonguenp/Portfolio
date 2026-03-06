using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class neco_fish_trap_rate : game_data
{
    public override GameDataManager.DATA_TYPE GetDataType() { return GameDataManager.DATA_TYPE.NECO_FISH_TRAP_RATE; }

    static public List<neco_fish_trap_rate> GetFishListByLevel(uint level)
    {
        List<game_data> necoData = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.NECO_FISH_TRAP_RATE);
        if (necoData == null)
        {
            return null;
        }

        List<neco_fish_trap_rate> fish_list = new List<neco_fish_trap_rate>();

        object obj;
        foreach (neco_fish_trap_rate trapData in necoData)
        {
            if (trapData.data.TryGetValue("level", out obj))
            {
                if ((uint)obj == level) 
                {
                    fish_list.Add(trapData);
                }
            }
        }

        return fish_list;
    }

    static public List<neco_fish_trap_rate> GetNewFishListByLevel(uint level)
    {
        List<game_data> necoData = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.NECO_FISH_TRAP_RATE);
        if (necoData == null || level >= 10)
        {
            return null;
        }

        List<neco_fish_trap_rate> result_list = new List<neco_fish_trap_rate>();
        List<neco_fish_trap_rate> fish_list = new List<neco_fish_trap_rate>();

        object obj;
        foreach (neco_fish_trap_rate trapData in necoData)
        {
            if (trapData.data.TryGetValue("level", out obj))
            {
                if ((uint)obj == level)
                {
                    fish_list.Add(trapData);
                }

                if ((uint)obj == level + 1)
                {
                    result_list.Add(trapData);
                }
            }
        }

        foreach (neco_fish_trap_rate oldfish in fish_list)
        {
            result_list.RemoveAll(x => x.GetNecoFishID() == oldfish.GetNecoFishID());
        }
        

        return result_list;
    }

    [NonSerialized]
    private uint necoFishID = 0;
    public uint GetNecoFishID()
    {
        if (necoFishID == 0)
        {
            object obj;
            if (data.TryGetValue("fish_id", out obj))
            {
                necoFishID = (uint)obj;
            }
        }

        return necoFishID;
    }
}
