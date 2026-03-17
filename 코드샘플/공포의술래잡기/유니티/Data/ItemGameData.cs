using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemGameData : GameData
{
    public enum ITEM_TYPE
    {
        EXP_POTION = 1,
        CHAR_PIECE = 2,
        ENCHANT_ITEM = 3,
        EMOTICON = 4,
        SOULSTONE_ITEM = 5,
        CHAR_TICKET = 6,
        RANDOM_BOX = 7,
        EVENT_ITEM = 8,
        EQUIP_ITEM = 13,
        BUFF_ITEM = 14,
        NICK_CHANGE = 15,
        SELECTABLE_ITEM = 16,
    }
    public ITEM_TYPE type { get; private set; }
    public int value { get; private set; }
    public Sprite sprite { get; private set; }
    public int grade { get; private set; }

    public override void SetValue(Dictionary<string, string> data)
    {
        base.SetValue(data);

        type = (ITEM_TYPE)Int(data["type"]);
        value = Int(data["value"]);

        sprite = null;

        if (data.ContainsKey("resource") && !string.IsNullOrEmpty(data["resource"]))
        {
            string path = "AssetsBundle/Texture/Icon/";
            if (type == ITEM_TYPE.EMOTICON)
            {
                path = "AssetsBundle/Texture/Icon_emoticon/";
            }
            else if (type == ITEM_TYPE.RANDOM_BOX)
            {
                path = "AssetsBundle/Texture/RandomBox/";
            }
            else if (type == ITEM_TYPE.CHAR_PIECE)
            {
                path = "AssetsBundle/Texture/Icon_card/";
            }

            sprite = Managers.Resource.LoadAssetsBundle<Sprite>(path + data["resource"]);
            if (sprite == null)
                sprite = Managers.Resource.LoadAssetsBundle<Sprite>("Texture/UI/Lobby/" + data["resource"]);
        }

        if (sprite == null)
        {
            sprite = Managers.Resource.LoadAssetsBundle<Sprite>("AssetsBundle/Texture/Icon/icon_loading");
        }
        grade = Int(data["grade"]);
    }

    static public ItemGameData GetItemData(int id)
    {
        GameData data = GameDataManager.Instance.GetData(GameDataManager.DATA_TYPE.item, id);
        if (data != null)
        {
            return data as ItemGameData;
        }

        return null;
    }

    static public Sprite GetItemIcon(int id)
    {
        ItemGameData data = GetItemData(id);
        if (data == null)
        {
            return Managers.Resource.LoadAssetsBundle<Sprite>("AssetsBundle/Texture/Icon/icon_loading");
        }

        return data.sprite;
    }
    static public int GetItemGrade(int id)
    {
        ItemGameData data = GetItemData(id);
        if (data == null)
        {
            return 0;
        }
        return data.grade;
    }
}

public class EquipConfig : GameData
{
    public string key { get; private set; }
    public int value { get; private set; }

    public static Dictionary<string, int> Config { get; private set; } = new Dictionary<string, int>();
    public override void SetValue(Dictionary<string, string> data)
    {
        base.SetValue(data);

        key = data["key"];
        value = Int(data["value"]);

        Config[key] = value;
    }

    static public Dictionary<string, int> GetConfigDic()
    {
        return Config;
    }
}

public class EquipInfo : GameData
{
    public int group_id { get; private set; }
    public int grade { get; private set; }
    public string sp_effect_resource { get; private set; }
    public string sp_effect_resource_front { get; private set; }
    public ItemGameData itemData { get; private set; }
    public override void SetValue(Dictionary<string, string> data)
    {
        base.SetValue(data);

        group_id = Int(data["group_id"]);
        grade = Int(data["grade"]);

        sp_effect_resource = data["sp_effect_resource"];

        if (data.ContainsKey("sp_effect_resource_front"))
            sp_effect_resource_front = data["sp_effect_resource_front"];
        else
            sp_effect_resource_front = "";
        itemData = ItemGameData.GetItemData(GetID());
    }

    static public EquipInfo GetEquipData(int id)
    {
        GameData data = GameDataManager.Instance.GetData(GameDataManager.DATA_TYPE.equipment_info, id);
        if (data != null)
        {
            return data as EquipInfo;
        }

        return null;
    }
}

public class EquipLevel : GameData
{
    public int group_id { get; private set; }
    public int level { get; private set; }
    public int max_exp { get; private set; }

    public List<EquipLevelEffect> equipLevelEffects = new List<EquipLevelEffect>();

    public override void SetValue(Dictionary<string, string> data)
    {
        base.SetValue(data);

        group_id = Int(data["group_id"]);
        level = Int(data["level"]);
        max_exp = Int(data["max_exp"]);

        equipLevelEffects.Clear();
        for (int i = 1; i < 5; i++)
        {
            EquipLevelEffect levelEffect = new EquipLevelEffect();
            string[] type = new string[4];
            type[0] = $"effect_type_{i}";
            type[1] = $"effect_calc_{i}";
            type[2] = $"effect_text_type_{i}";
            type[3] = $"effect_value_{i}";

            levelEffect.SetValue(Int(data[type[0]]), Int(data[type[1]]), Int(data[type[2]]), Int(data[type[3]]));
            equipLevelEffects.Add(levelEffect);
        }
    }
}

public class EquipLevelEffect
{
    public int effect_type { get; private set; }
    public int effect_calc { get; private set; }
    public int effect_text_type { get; private set; }
    public int effect_value { get; private set; }

    public void SetValue(int type, int calc, int text_type, int value)
    {
        effect_type = type;
        effect_calc = calc;
        effect_text_type = text_type;
        effect_value = value;
    }
}

public class EquipReinforce : GameData
{
    public int reinforce_next_equip { get; private set; }
    public int price_type { get; private set; }
    public int price_param { get; private set; }
    public int price_amount { get; private set; }

    public override void SetValue(Dictionary<string, string> data)
    {
        base.SetValue(data);

        reinforce_next_equip = Int(data["reinforce_next_equip"]);
        price_type = Int(data["price_type"]);
        price_param = Int(data["price_param"]);
        price_amount = Int(data["price_amount"]);
    }
}

public class EmoticonItemData : GameData
{
    public int group_uid { get; private set; }
    public int emoticon_grade { get; private set; }

    public ItemGameData itemData { get; private set; }
    public Sprite sprite
    {
        get
        {
            if (itemData != null)
                return itemData.sprite;

            return null;
        }
    }
    public override void SetValue(Dictionary<string, string> data)
    {
        base.SetValue(data);

        group_uid = Int(data["group_uid"]);
        emoticon_grade = Int(data["emoticon_grade"]);

        itemData = ItemGameData.GetItemData(GetID());
    }
}

public class CollectionBuff : GameData
{
    public enum BuffType
    {
        BUFF_HP = 1,
        BUFF_ATK = 2,
        BUFF_GOLD = 3,
        BUFF_ITEM = 4,
    }

    public BuffType type { get; private set; }
    public int value { get; private set; }
    

    public override void SetValue(Dictionary<string, string> data)
    {
        base.SetValue(data);

        type = (BuffType)Int(data["buff_type"]);

        switch(type)
        {
            case BuffType.BUFF_HP:
                value = (int)(Int(data["buff_value"]) * 0.001f);
                break;
            case BuffType.BUFF_ATK:
                value = (int)(Int(data["buff_value"]) * 0.001f);
                break;
            case BuffType.BUFF_GOLD:
                value = (int)(Int(data["buff_value"]) * 0.1f);
                break;
            case BuffType.BUFF_ITEM:
                value = (int)(Int(data["buff_value"]) * 0.1f);
                break;
        }        
    }

    public string GetValueString(int amount = 1)
    {
        switch (type)
        {
            case BuffType.BUFF_HP:
                return (value * amount).ToString();
            case BuffType.BUFF_ATK:
                return (value * amount).ToString();
            case BuffType.BUFF_GOLD:
                return (value * amount).ToString() + "%";
            case BuffType.BUFF_ITEM:
                return (value * amount).ToString() + "%";
        }

        return "0";
    }
}