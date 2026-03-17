using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopMenuItem : ScrollUIControllerItem
{
    [SerializeField]
    Image panel;
    [SerializeField]
    Text MenuName;
    [SerializeField]
    ShopMenuAlert redDot;
    public ShopMenuGameData shopMenuData { get; private set; }
    public override void SetData(GameData data, ScrollItemSelectCallback cb = null)
    {
        base.SetData(data, cb);

        shopMenuData = data as ShopMenuGameData;

        MenuName.text = shopMenuData.GetName();
        if (shopMenuData.shop_type == 1)
        {
            redDot.Init(shopMenuData.GetID());
            redDot.RefreshUI();
        }
    }

    public void SetFocus(ShopMenuGameData focusItem)
    {
        if (shopMenuData != null && shopMenuData.shop_type == 1)
        {
            if (focusItem == shopMenuData)
            {
                panel.sprite = Resources.Load<Sprite>("Texture/UI/Shop/convenience_store_tab_01");
            }
            else
            {
                panel.sprite = Resources.Load<Sprite>("Texture/UI/Shop/convenience_store_tab_02");
            }
        }
        else if (shopMenuData != null && shopMenuData.shop_type == 2)
        {
            if (focusItem == shopMenuData)
            {
                panel.sprite = Resources.Load<Sprite>("Texture/UI/Shop/convenience_store_tab_02");
                ColorUtility.TryParseHtmlString("#5ad9c9", out Color color);
                panel.color = color;
            }
            else
            {
                panel.sprite = Resources.Load<Sprite>("Texture/UI/Shop/convenience_store_tab_02");
                panel.color = Color.white;
            }

        }
    }
}
