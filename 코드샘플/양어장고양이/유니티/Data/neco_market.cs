using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class neco_market : game_data
{
    public override GameDataManager.DATA_TYPE GetDataType() { return GameDataManager.DATA_TYPE.NECO_MARKET; }

    static public neco_market GetNecoMarketDataByID(uint itemID)
    {
        List<game_data> necoData = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.NECO_MARKET);
        if (necoData == null)
        {
            return null;
        }

        object obj;
        foreach (neco_market marketData in necoData)
        {
            if (marketData.data.TryGetValue("item_id", out obj))
            {
                if (itemID == (uint)obj)
                {
                    return marketData;
                }
            }
        }

        return null;
    }

    // temp - 임시 특별 품목 수령 -> 추후 서버측에서 받아와야함
    static public List<neco_market> GetNecoMarketSpecialItem(string type)
    {
        List<game_data> necoData = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.NECO_MARKET);
        if (necoData == null)
        {
            return null;
        }

        List<neco_market> marketList = new List<neco_market>();
        
        object obj;
        foreach (neco_market marketData in necoData)
        {
            if (marketData.data.TryGetValue("market_type", out obj))
            {
                if (type == (string)obj)
                {
                    marketList.Add(marketData);
                }
            }
        }

        return marketList;
    }

    [NonSerialized]
    private uint necoMarketID = 0;
    public uint GetNecoMarketID()
    {
        if (necoMarketID == 0)
        {
            object obj;
            if (data.TryGetValue("id", out obj))
            {
                necoMarketID = (uint)obj;
            }
        }

        return necoMarketID;
    }

    [NonSerialized]
    private uint necoMarketItemID = 0;
    public uint GetNecoMarketItemID()
    {
        if (necoMarketItemID == 0)
        {
            object obj;
            if (data.TryGetValue("item_id", out obj))
            {
                necoMarketItemID = (uint)obj;
            }
        }

        return necoMarketItemID;
    }

    [NonSerialized]
    private uint necoMarketPrice = 0;
    public uint GetNecoMarketPrice()
    {
        if (necoMarketPrice == 0)
        {
            object obj;
            if (data.TryGetValue("price", out obj))
            {
                necoMarketPrice = (uint)obj;
            }
        }

        return necoMarketPrice;
    }

    [NonSerialized]
    private string necoMarketType = "";
    public string GetNecoMarketType()
    {
        if (necoMarketType == "")
        {
            object obj;
            if (data.TryGetValue("market_type", out obj))
            {
                necoMarketType = (string)obj;
            }
        }

        return necoMarketType;
    }
}
