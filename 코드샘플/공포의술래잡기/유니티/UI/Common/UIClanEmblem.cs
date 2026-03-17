using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIClanEmblem : MonoBehaviour
{
    [SerializeField] Image Background;
    [SerializeField] Image Icon;

    public ClanEmblemData curData { get; private set; } = null;
    public void Init(int index)
    {
        GameData data = GameDataManager.Instance.GetData(GameDataManager.DATA_TYPE.clan_emblem, index, true);
        if(data == null)
            data = GameDataManager.Instance.GetData(GameDataManager.DATA_TYPE.clan_emblem, 1);
        Init((ClanEmblemData)data);
    }

    public void Init(ClanEmblemData data)
    {
        curData = data;

        if (curData == null)
            return;

        if (Background != null)
            Background.color = curData.bgColor;

        if (Icon != null)
        {
            Icon.color = curData.iconColor;
            Icon.sprite = curData.sprite;
        }
    }
}


public class ClanEmblemData : GameData
{
    public enum EMBLEM_TYPE { 
        UNKNOWN = 0,
        NORMAL = 1,
        CHECK_ITEM = 2,
    };

    public Sprite sprite { get; private set; } = null;
    public EMBLEM_TYPE type { get; private set; }
    public int param { get; private set; }
    public Color bgColor { get; private set; }
    public Color iconColor { get; private set; }
    public override void SetValue(Dictionary<string, string> data)
    {
        base.SetValue(data);
        
        type = EMBLEM_TYPE.NORMAL;
        if (data.ContainsKey("type"))
        {
            type = (EMBLEM_TYPE)Int(data["type"]);
        }

        param = 0;
        if (data.ContainsKey("param"))
        {
            param = Int(data["param"]);
        }

        if (data.ContainsKey("resource") && !string.IsNullOrEmpty(data["resource"]))
        {
            string path = "AssetsBundle/Texture/" + data["resource"];
            sprite = Managers.Resource.LoadAssetsBundle<Sprite>(path);
        }

        if (sprite == null)
        {
            switch(type)
            {
                case EMBLEM_TYPE.CHECK_ITEM:
                    ItemGameData item = ItemGameData.GetItemData(param);
                    if(item != null)
                    {
                        sprite = item.sprite;
                    }
                    break;
            }

            if(sprite == null)
            {
                sprite = Managers.Resource.LoadAssetsBundle<Sprite>("AssetsBundle/Texture/Icon/icon_loading");
            }
        }

        Color bg = Color.white;
        //bg.r = Int(data["background_r"]) / 255.0f;
        //bg.g = Int(data["background_g"]) / 255.0f;
        //bg.b = Int(data["background_b"]) / 255.0f;

        Color icon = Color.white;
        //icon.r = Int(data["icon_r"]) / 255.0f;
        //icon.g = Int(data["icon_g"]) / 255.0f;
        //icon.b = Int(data["icon_b"]) / 255.0f;

        bgColor = bg;
        iconColor = icon;
    }
}