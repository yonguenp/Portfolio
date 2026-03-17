using Spine.Unity;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ShopPopup : Popup
{
    public static class TabType
    {
        public const int RANDOM = 1;
        public const int MILEAGUE = 5;
        public const int ASSETS = 6;
    }

    [SerializeField]
    ScrollUIController MenuUI;
    [SerializeField]
    ShopMainScrollUI MainScroll;

    [SerializeField]
    UITop UITop;

    [SerializeField]
    SkeletonGraphic shopCharacter;

    [SerializeField] Color ableColor;
    [SerializeField] Color disableColor;
    [SerializeField] AudioClip shopBGM;

    public ShopMenuGameData curMenuItemData { get; private set; }

    public override void Open(CloseCallback cb = null)
    {
        base.Open(cb);

        shopCharacter.startingAnimation = "f_idle_0";
        shopCharacter.startingLoop = true;
        shopCharacter.Initialize(true);
        shopBGM = Managers.Resource.LoadAssetsBundle<AudioClip>("AssetsBundle/Sounds/bgm/BGM_SHOP");
        Managers.Sound.Play(shopBGM, Sound.Bgm);
    }
    public override void Close()
    {
        curMenuItemData = null;
        base.Close();
        Managers.Scene.CurrentScene.StartBackgroundMusic(false);
    }
    public override void RefreshUI()
    {
        List<GameData> menus = ShopMenuGameData.GetShopMenuListWithSort();
        shopCharacter.gameObject.SetActive(menus.Count <= 4);

        MenuUI.SetScrollList(menus, OnMenuItem);

        if (curMenuItemData == null)
            curMenuItemData = menus[0] as ShopMenuGameData;

        UITop.SetDisplayAsset(curMenuItemData.assetType);
        MainScroll.RefreshUI();
        MainScroll.OnTopPosition();

        foreach (Transform child in MenuUI.ScrollContainer)
        {
            child.GetComponent<ShopMenuItem>().SetFocus(curMenuItemData);
        }

    }

    public void SetMenu(int type)
    {
        List<GameData> menus = ShopMenuGameData.GetShopMenuListWithSort();
        if (menus.Count <= 0)
            return;

        ShopMenuGameData tab = null;
        foreach (ShopMenuGameData item in menus)
        {
            if (item.GetID() == type)
            {
                tab = item;
                break;
            }
        }

        if (type == 0)
            tab = menus[0] as ShopMenuGameData;

        OnMenuItem(tab);
    }

    public void OnMenuItem(ShopMenuGameData shopMenuData)
    {
        if (shopMenuData == null)
            return;

        List<GameData> items = null;
        switch (shopMenuData.pageType)
        {
            case ShopMenuGameData.PAGE_TYPE.COLUMN_3:
                items = ShopItemGameData.GetShopItemWithSort(shopMenuData.GetID());
                if (items == null || items.Count == 0)
                {
                    PopupCanvas.Instance.ShowPopup(PopupCanvas.POPUP_TYPE.DEVLOPING_NOTICE_POPUP);
                    return;
                }
                break;
            case ShopMenuGameData.PAGE_TYPE.COLUMN_4:
                items = ShopItemGameData.GetShopItemWithSort(shopMenuData.GetID());
                if (items == null || items.Count == 0)
                {
                    PopupCanvas.Instance.ShowPopup(PopupCanvas.POPUP_TYPE.DEVLOPING_NOTICE_POPUP);
                    return;
                }
                break;
            case ShopMenuGameData.PAGE_TYPE.RANDOM:
                break;
            case ShopMenuGameData.PAGE_TYPE.PACKAGE_COLUMN_3:
                items = ShopItemGameData.GetShopItemWithSort(shopMenuData.GetID());
                if (items.Count == 0)
                {
                    PopupCanvas.Instance.ShowPopup(PopupCanvas.POPUP_TYPE.DEVLOPING_NOTICE_POPUP);
                }
                break;
        }

        curMenuItemData = shopMenuData;

        RefreshUI();
    }
    public void OnMenuItem(ScrollUIControllerItem caller)
    {
        ShopMenuItem menu = (caller as ShopMenuItem);

        if (curMenuItemData == menu.shopMenuData)
            return;

        OnMenuItem(menu.shopMenuData);
    }

    public Color GetAbleColor()
    {
        return ableColor;
    }
    public Color GetDisableColor()
    {
        return disableColor;
    }

    public bool RefreshCheck()
    {
        if (MainScroll.GetResreshPopup().activeInHierarchy)
        {
            MainScroll.OnRefreshClear();
            return true;
        }
        return false;
    }
}
