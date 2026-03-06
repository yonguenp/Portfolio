using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class neco_object_durability : game_data
{
    public override GameDataManager.DATA_TYPE GetDataType() { return GameDataManager.DATA_TYPE.NECO_OBJECT_DURABILITY; }

    static public neco_object_durability GetObjectDurability(uint id)
    {
        List<game_data> necoDataList = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.NECO_OBJECT_DURABILITY);
        if (necoDataList == null)
        {
            return null;
        }

        object obj;
        foreach (neco_object_durability necoData in necoDataList)
        {
            if (necoData.data.TryGetValue("id", out obj))
            {
                if(id == 25)
                {
                    return GetObjectDurability(20);
                }
                if (id == (uint)obj)
                {
                    return necoData;
                }
            }
        }

        return null;
    }

    public uint GetNecoDurabilityByLevel(uint level)
    {
        uint durability = 0;
        string findStr = string.Format("lv{0}", level);

        object obj;
        if (data.TryGetValue(findStr, out obj))
        {
            durability = (uint)obj;
        }
        

        return durability;
    }
}
