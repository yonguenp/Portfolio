using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class neco_booster : game_data
{
    public override GameDataManager.DATA_TYPE GetDataType() { return GameDataManager.DATA_TYPE.NECO_BOOSTER; }


    static public neco_booster GetBoosterData(uint index)
    {
        List<game_data> necoData = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.NECO_BOOSTER);
        if (necoData == null)
        {
            return null;
        }

        object obj;
        foreach (neco_booster Data in necoData)
        {
            if (index == Data.GetBoosterID())
                return Data;
        }

        return null;
    }

    [NonSerialized]
    private uint boosterID = 0;
    public uint GetBoosterID()
    {
        if (boosterID == 0)
        {
            object obj;
            if (data.TryGetValue("id", out obj))
            {
                boosterID = (uint)obj;
            }
        }

        return boosterID;
    }

    [NonSerialized]
    private string price_type = "";
    public string GetPriceType()
    {
        if(string.IsNullOrEmpty(price_type))
        {
            object obj;
            if (data.TryGetValue("price_type", out obj))
            {
                price_type = (string)obj;
            }
        }

        return price_type;
    }

    [NonSerialized]
    private uint price = 0;
    public uint GetPrice()
    {
        if (price == 0)
        {
            object obj;
            if (data.TryGetValue("price", out obj))
            {
                price = (uint)obj;
            }
        }

        return price;
    }

    [NonSerialized]
    private uint effect = 0;
    public uint GetEffect()
    {
        if (effect == 0)
        {
            object obj;
            if (data.TryGetValue("effect", out obj))
            {
                effect = (uint)obj;
            }
        }

        return effect;
    }

    [NonSerialized]
    private uint effect_time = 0;
    public uint GetEffectTime()
    {
        if (effect_time == 0)
        {
            object obj;
            if (data.TryGetValue("effect_time", out obj))
            {
                effect_time = (uint)obj;
            }
        }

        return effect_time;
    }
}
