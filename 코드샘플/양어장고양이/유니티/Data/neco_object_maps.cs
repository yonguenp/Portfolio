using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class neco_object_maps : game_data
{
    public override GameDataManager.DATA_TYPE GetDataType() { return GameDataManager.DATA_TYPE.NECO_OBJECT_MAPS; }

    static public neco_object_maps GetNecoObjectMapData(uint objectID)
    {
        List<game_data> necoDataList = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.NECO_OBJECT_MAPS);
        if (necoDataList == null)
        {
            return null;
        }

        object obj;
        foreach (neco_object_maps necoData in necoDataList)
        {
            if (necoData.data.TryGetValue("object_id", out obj))
            {
                if (objectID == (uint)obj)
                {
                    return necoData;
                }
            }
        }

        return null;
    }

    static public List<uint> GetSpotIDListInMap(uint mapID)
    {
        List<uint> ret = new List<uint>();
        List<game_data> necoDataList = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.NECO_OBJECT_MAPS);
        if (necoDataList == null)
        {
            return ret;
        }

        object obj;
        foreach (neco_object_maps necoData in necoDataList)
        {
            if(necoData.GetNecoMapID() == mapID)
                ret.Add(necoData.GetNecoObjectID());
        }

        return ret;
    }

    [NonSerialized]
    private uint necoMapID = 0;
    public uint GetNecoMapID()
    {
        if (necoMapID == 0)
        {
            object obj;
            if (data.TryGetValue("map_id", out obj))
            {
                necoMapID = (uint)obj;
            }
        }

        return necoMapID;
    }

    [NonSerialized]
    private uint objectID = 0;
    public uint GetNecoObjectID()
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
}
