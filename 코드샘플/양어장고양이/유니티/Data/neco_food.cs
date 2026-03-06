using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class neco_food : game_data
{
    public override GameDataManager.DATA_TYPE GetDataType() { return GameDataManager.DATA_TYPE.NECO_FOOD; }

    static public neco_food GetFoodInfo(uint foodID)
    {
        List<game_data> necoData = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.NECO_FOOD);
        if (necoData != null)
        {
            foreach (neco_food data in necoData)
            {
                if (data != null && data.GetFoodID() == foodID)
                {
                    return data;
                }
            }
        }

        return null;
    }

    static public Sprite GetFoodImage(uint foodID)
    {
        List<game_data> necoData = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.NECO_FOOD);
        if (necoData != null)
        {
            foreach (neco_food data in necoData)
            {
                if (data != null && data.GetFoodID() == foodID)
                {
                    return data.GetFoodImage();
                }
            }
        }

        return null;
    }

    static public uint GetFoodDuration(uint foodID)
    {
        List<game_data> necoData = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.NECO_FOOD);
        if (necoData != null)
        {
            foreach (neco_food data in necoData)
            {
                if (data != null && data.GetFoodID() == foodID)
                {
                    return data.GetFoodDuration();
                }
            }
        }

        return 0;
    }

    [NonSerialized]
    uint id = 0;

    [NonSerialized]
    Sprite foodImage = null;

    [NonSerialized]
    uint duration = 0;

    [NonSerialized]
    uint weight = 0;

    public uint GetFoodID()
    {
        if(id == 0)
        {
            object obj;
            if (data.TryGetValue("id", out obj))
            {
                id = (uint)obj;
            }
        }

        return id;
    }

    public Sprite GetFoodImage()
    {
        if(foodImage == null)
        {
            object obj;
            if(data.TryGetValue("resource_img", out obj))
            {
                foodImage = Resources.Load<Sprite>((string)obj);
            }
        }

        return foodImage;
    }

    public uint GetFoodDuration()
    {
        if (duration == 0)
        {
            object obj;
            if (data.TryGetValue("duration", out obj))
            {
                duration = (uint)obj;
            }
        }

        return duration;
    }

    public uint GetWeight()
    {
        if(weight == 0)
        {
            object obj;
            if (data.TryGetValue("weight", out obj))
            {
                weight = (uint)obj;
            }
        }

        return weight;
    }
}
