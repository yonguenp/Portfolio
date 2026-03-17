using System;
using System.Collections.Generic;

[Serializable]
public class user_items : game_data
{
    public override GameDataManager.DATA_TYPE GetDataType() { return GameDataManager.DATA_TYPE.USER_ITEMS; }

    static public uint GetUserItemAmount(uint itemID)
    {
        List<game_data> user_items = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.USER_ITEMS);
        if (user_items == null)
            return 0;

        foreach (game_data data in user_items)
        {
            user_items userItem = (user_items)data;
            uint userItem_id = userItem.GetItemID();

            if (itemID == userItem_id)
            {
                return userItem.GetAmount();
            }
        }

        return 0;
    }

    [NonSerialized]
    private uint itemID = 0;

    public uint GetItemID()
    {
        if (itemID == 0)
        {
            object obj;
            if (data.TryGetValue("item_id", out obj))
            {
                itemID = (uint)obj;
            }
        }

        return itemID;
    }

    public uint GetAmount()
    {
        uint count = 0;
        object obj;
        if(data.TryGetValue("get_amount", out obj))
        {
            uint get = (uint)obj;
            if (data.TryGetValue("used_amount", out obj))
            {
                if(get >= (uint)obj)
                    count = get - (uint)obj;
            }
        }

        return count;
    }
}

