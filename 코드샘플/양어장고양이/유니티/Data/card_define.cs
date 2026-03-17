using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class card_define : game_data
{
    public static card_define GetCardDefine(uint id)
    {
        List<game_data> necoData = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.CARD_DEFINE);
        foreach (card_define data in necoData)
        {
            if (data != null && data.GetCardID() == id)
            {
                return data;
            }
        }

        return null;
    }

    [NonSerialized]
    private uint cardID = 0;
    public uint GetCardID()
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

    public uint GetResourceType()
    {
        object obj;
        if (data.TryGetValue("resource_type", out obj))
        {
            return (uint)obj;
        }

        return 0;
    }

    [NonSerialized]
    private string resource_path = "";
    public string GetResourcePath()
    {
        if (string.IsNullOrEmpty(resource_path))
        {
            object obj;
            if (data.TryGetValue("resource_path", out obj))
            {
                resource_path = (string)obj;
            }
        }
        return resource_path;
    }

    [NonSerialized]
    private Sprite sprite = null;
    public Sprite GetSprite()
    {
        if (sprite == null)
        {
            sprite = Resources.Load<Sprite>(GetResourcePath());
        }
        return sprite;
    }

    [NonSerialized]
    private Sprite icon = null;
    public Sprite GetIcon()
    {
        if (icon == null)
        {
            object obj;
            if (data.TryGetValue("cover_img", out obj))
            {
                icon = Resources.Load<Sprite>((string)obj);
            }
            if (icon == null)
            {
                icon = Resources.Load<Sprite>("Sprites/icon/card_default");
            }
        }

        return icon;
    }

    public override GameDataManager.DATA_TYPE GetDataType() { return GameDataManager.DATA_TYPE.CARD_DEFINE; }
}
