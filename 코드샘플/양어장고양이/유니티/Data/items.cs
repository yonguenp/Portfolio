using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class items : game_data
{
    static public items GetItem(uint itemID)
    {
        List<game_data> itemlist = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.ITEMS);
        if (itemlist == null)
            return null;

        foreach (items data in itemlist)
        {            
            if (data.GetItemID() == itemID)
            {
                return data;
            }
        }

        return null;
    }

    static public void ClearLocalizeData()
    {
        List<game_data> necoData = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.ITEMS);
        if (necoData == null)
        {
            return;
        }

        foreach (items data in necoData)
        {
            data.itemName = "";
            data.desc = "";
        }
    }

    public override GameDataManager.DATA_TYPE GetDataType() { return GameDataManager.DATA_TYPE.ITEMS; }

    [NonSerialized]
    uint itemID = 0;

    public uint GetItemID()
    {
        if(itemID == 0)
        {
            object obj;
            if (data.TryGetValue("item_id", out obj))
            {
                itemID = (uint)obj;
            }
        }

        return itemID;
    }

    [NonSerialized]
    string itemName = "";

    public string GetItemName()
    {
        if (itemName == "")
        {
            itemName = LocalizeData.GetText("items:name_kr:" + GetItemID().ToString());
        }

        return itemName;
    }

    [NonSerialized]
    string desc = "";

    public string GetItemDesc()
    {
        if (desc == "")
        {
            desc = LocalizeData.GetText("items:item_desc_kr:" + GetItemID().ToString());
        }

        return desc;
    }

    [NonSerialized]
    string item_type = "";

    public string GetItemType()
    {
        if (string.IsNullOrEmpty(item_type))
        {
            object obj;
            if (data.TryGetValue("item_type", out obj))
            {
                item_type = (string)obj;
            }
        }

        return item_type;
    }

    [NonSerialized]
    Sprite itemIcon = null;

    public Sprite GetItemIcon()
    {
        if (itemIcon == null)
        {
            object obj;
            if (data.TryGetValue("icon_img", out obj))
            {
                itemIcon = Resources.Load<Sprite>((string)obj);
            }
        }

        return itemIcon;
    }
    
}

