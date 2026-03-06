using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class catnip_farm : game_data
{
    public override GameDataManager.DATA_TYPE GetDataType() { return GameDataManager.DATA_TYPE.CATNIP_FARM; }

    static public catnip_farm GetCatnipFarmData(uint catnipfarmLevel, uint objectID, uint objectLevel)
    {
        List<game_data> necoData = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.CATNIP_FARM);
        if (necoData == null)
        {
            return null;
        }

        foreach (catnip_farm data in necoData)
        {
            if(data.CheckData(catnipfarmLevel, objectID, objectLevel))
                return data;
        }

        return null;
    }

    public bool CheckData(uint catnipfarmLevel, uint objectID, uint objectLevel)
    {
        return GetFarmLevel() == catnipfarmLevel && GetObjectID() == objectID && GetObjectLevel() == objectLevel;
    }

    [NonSerialized]
    private uint farmLevel = 0;
    public uint GetFarmLevel()
    {
        if (farmLevel == 0)
        {
            object obj;
            if (data.TryGetValue("catnipfarm_level", out obj))
            {
                farmLevel = (uint)obj;
            }
        }

        return farmLevel;
    }

    [NonSerialized]
    private uint objectID = 0;
    public uint GetObjectID()
    {
        if (objectID == 0)
        {
            object obj;
            if (data.TryGetValue("object_id", out obj))
            {
                objectID = (uint)obj;
            }
        }

        return objectID;
    }

    [NonSerialized]
    private uint objectLevel = 0;
    public uint GetObjectLevel()
    {
        if (objectLevel == 0)
        {
            object obj;
            if (data.TryGetValue("object_level", out obj))
            {
                objectLevel = (uint)obj;
            }
        }

        return objectLevel;
    }

    [NonSerialized]
    private uint value = 0;
    public uint GetFarmValue()
    {
        if (value == 0)
        {
            object obj;
            if (data.TryGetValue("value", out obj))
            {
                value = (uint)obj;
            }
        }

        return value;
    }
}
