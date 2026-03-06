using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class neco_object_slots : game_data
{
    public override GameDataManager.DATA_TYPE GetDataType() { return GameDataManager.DATA_TYPE.NECO_OBJECT_SLOTS; }

    static public neco_object_slots GetNecoObjectSlotData(uint objectID, uint catID)
    {
        List<game_data> necoDataList = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.NECO_OBJECT_SLOTS);
        if (necoDataList == null)
        {
            return null;
        }

        object obj;
        foreach (neco_object_slots necoData in necoDataList)
        {
            if (necoData.data.TryGetValue("object_id", out obj))
            {
                if (objectID == (uint)obj)
                {
                    if (necoData.data.TryGetValue("neco_id", out obj))
                    {
                        if (catID == (uint)obj)
                        {
                            return necoData;
                        }
                    }
                }
            }
        }

        return null;
    }

    [NonSerialized]
    private uint necoSlot = 0;
    public uint GetNecoSlot()
    {
        if (necoSlot == 0)
        {
            object obj;
            if (data.TryGetValue("slot", out obj))
            {
                necoSlot = (uint)obj;
            }
        }

        return necoSlot;
    }
}
