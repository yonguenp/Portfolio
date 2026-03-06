using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class neco_gift_basket : game_data
{
    public override GameDataManager.DATA_TYPE GetDataType() { return GameDataManager.DATA_TYPE.NECO_GIFT_BASKET; }

    static public List<uint> GetLevelUPNewItems(uint targetLevel)
    {
        List<uint> ret = new List<uint>();

        List<game_data> necoData = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.NECO_GIFT_BASKET);

        List<uint> checker = new List<uint>();
        object obj;
        foreach (neco_gift_basket data in necoData)
        {
            if (data.data.TryGetValue("level", out obj))
            {
                if (targetLevel > (uint)obj)
                {
                    if (data.data.TryGetValue("item_id", out obj))
                    {
                        if(!checker.Contains((uint)obj))
                            checker.Add((uint)obj);
                    }
                }
            }
        }

        foreach (neco_gift_basket data in necoData)
        {
            if (data.data.TryGetValue("level", out obj))
            {
                if (targetLevel == (uint)obj)
                {
                    if (data.data.TryGetValue("item_id", out obj))
                    {
                        if (!checker.Contains((uint)obj))
                            ret.Add((uint)obj);
                    }
                }
            }
        }

        return ret;
    }
}
