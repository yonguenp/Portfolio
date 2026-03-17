using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class neco_event_thanks_shop : game_data
{
    public override GameDataManager.DATA_TYPE GetDataType() { return GameDataManager.DATA_TYPE.NECO_EVENT_THANKS_SHOP; }

    [NonSerialized]
    private uint necoEventShopID = 0;
    public uint GetNecoEventShopID()
    {
        if (necoEventShopID == 0)
        {
            object obj;
            if (data.TryGetValue("id", out obj))
            {
                necoEventShopID = (uint)obj;
            }
        }

        return necoEventShopID;
    }

    [NonSerialized]
    private uint necoEventShopPrice = 0;
    public uint GetNecoEventShopPrice()
    {
        if (necoEventShopPrice == 0)
        {
            object obj;
            if (data.TryGetValue("price", out obj))
            {
                necoEventShopPrice = (uint)obj;
            }
        }

        return necoEventShopPrice;
    }

    [NonSerialized]
    private string necoEventShopItemType;
    public string GetNecoEventShopItemType()
    {
        if (necoEventShopItemType == null)
        {
            object obj;
            if (data.TryGetValue("item_type", out obj))
            {
                necoEventShopItemType = (string)obj;
            }
        }

        return necoEventShopItemType;
    }

    [NonSerialized]
    private uint necoEventShopItemID = 0;
    public uint GetNecoEventShopItemID()
    {
        if (necoEventShopItemID == 0)
        {
            object obj;
            if (data.TryGetValue("item_id", out obj))
            {
                necoEventShopItemID = (uint)obj;
            }
        }

        return necoEventShopItemID;
    }

    [NonSerialized]
    private uint necoEventShopCount = 0;
    public uint GetNecoEventShopCount()
    {
        if (necoEventShopCount == 0)
        {
            object obj;
            if (data.TryGetValue("count", out obj))
            {
                necoEventShopCount = (uint)obj;
            }
        }

        return necoEventShopCount;
    }

    [NonSerialized]
    private uint necoEventShopLimit = 0;
    public uint GetNecoEventShopLimit()
    {
        if (necoEventShopLimit == 0)
        {
            object obj;
            if (data.TryGetValue("limit", out obj))
            {
                necoEventShopLimit = (uint)obj;
            }
        }

        return necoEventShopLimit;
    }
}
