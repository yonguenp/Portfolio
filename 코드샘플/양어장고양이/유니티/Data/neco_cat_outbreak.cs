using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class neco_cat_outbreak : game_data
{
    public override GameDataManager.DATA_TYPE GetDataType() { return GameDataManager.DATA_TYPE.NECO_CAT_OUTBREAK; }

    static public List<neco_cat_outbreak> GetCatObjectIDList(uint catID)
    {
        List<game_data> necoDataList = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.NECO_CAT_OUTBREAK);
        if (necoDataList == null)
        {
            return null;
        }

        List<neco_cat_outbreak> result_list = new List<neco_cat_outbreak>();

        object obj;
        foreach (neco_cat_outbreak necoData in necoDataList)
        {
            if (necoData.data.TryGetValue("cat_id", out obj))
            {
                if (catID == (uint)obj)
                {
                    if (result_list.Contains(necoData) == false)
                    {
                        result_list.Add(necoData);
                    }
                }
            }
        }

        return result_list;
    }

    static public neco_cat_outbreak GetOutbreak(uint id)
    {
        List<game_data> necoDataList = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.NECO_CAT_OUTBREAK);
        if (necoDataList == null)
        {
            return null;
        }

        foreach (neco_cat_outbreak necoData in necoDataList)
        {
            if (necoData.GetNecoOutBreakID() == id)
                return necoData;
        }

        return null;
    }

    [NonSerialized]
    private uint outbreakID = 0;
    public uint GetNecoOutBreakID()
    {
        if (outbreakID == 0)
        {
            object obj;
            if (data.TryGetValue("id", out obj))
            {
                outbreakID = (uint)obj;
            }
        }

        return outbreakID;
    }


    [NonSerialized]
    private uint necoOutBreakObjectID = 0;
    public uint GetNecoOutBreakObjectID()
    {
        if (necoOutBreakObjectID == 0)
        {
            object obj;
            if (data.TryGetValue("object_id", out obj))
            {
                necoOutBreakObjectID = (uint)obj;
            }
        }

        return necoOutBreakObjectID;
    }

    [NonSerialized]
    private neco_cat.CAT_SUDDEN_STATE sudden_type = neco_cat.CAT_SUDDEN_STATE.NONE;
    public neco_cat.CAT_SUDDEN_STATE GetNecoOutBreakType()
    {
        if (sudden_type == neco_cat.CAT_SUDDEN_STATE.NONE)
        {
            object obj;
            if (data.TryGetValue("type", out obj))
            {
                switch((string)obj)
                {
                    case "touch":
                        sudden_type = neco_cat.CAT_SUDDEN_STATE.TOUCH;
                        break;
                    case "photo":
                        sudden_type = neco_cat.CAT_SUDDEN_STATE.PHOTO;
                        break;
                    case "observe":
                        sudden_type = neco_cat.CAT_SUDDEN_STATE.WATCH;
                        break;
                }
            }
        }

        return sudden_type;
    }

    [NonSerialized]
    private float rate = 0.0f;
    public int GetRate()
    {
        if (rate == 0.0f)
        {
            object obj;
            if (data.TryGetValue("rate", out obj))
            {
                rate = (float)obj;
            }
        }

        return (int)(rate * 1000.0f);
    }

    [NonSerialized]
    private uint target = 0;
    public uint GetTarget()
    {
        if (target == 0)
        {
            object obj;
            if (data.TryGetValue("target", out obj))
            {
                target = (uint)obj;
            }
        }

        return target;
    }
}


[Serializable]
public class neco_photo_pool_data : game_data
{
    public override GameDataManager.DATA_TYPE GetDataType() { return GameDataManager.DATA_TYPE.NECO_PHOTO_POOL_DATA; }

    static public List<uint> GetPoolList(uint poolID)
    {
        List<game_data> necoDataList = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.NECO_PHOTO_POOL_DATA);
        if (necoDataList == null)
        {
            return null;
        }

        List<uint> ret = new List<uint>();
        foreach (neco_photo_pool_data necoData in necoDataList)
        {
            if (necoData.GetNecoPoolID() == poolID)
                ret.Add(necoData.GetPhotoID());
        }

        return ret;
    }

    [NonSerialized]
    private uint poolID = 0;
    public uint GetNecoPoolID()
    {
        if (poolID == 0)
        {
            object obj;
            if (data.TryGetValue("pool_id", out obj))
            {
                poolID = (uint)obj;
            }
        }

        return poolID;
    }

    [NonSerialized]
    private uint cardID = 0;

    public uint GetPhotoID()
    {
        if (cardID == 0)
        {
            object obj;
            if (data.TryGetValue("card_id", out obj))
            {
                cardID = (uint)obj;
            }
        }

        return cardID;
    }
}