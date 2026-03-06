using SBCommonLib;
using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExchangePopup : Popup
{
    [SerializeField] ScrollUIController MenuUI;
    [SerializeField] SkeletonGraphic exchangeCharacter;
    [SerializeField] ShopMainScrollUI MainScroll;

    [SerializeField] UITop UITop;

    [SerializeField] Color ableColor;
    [SerializeField] Color disableColor;

    [SerializeField] GameObject TypeA;
    [SerializeField] GameObject TypeB;
    [SerializeField] GameObject TypeC;
    [SerializeField] GameObject event_tble;

    [SerializeField] GameObject event_assets;

    [SerializeField] AudioClip exchangeUI_BGM;

    public ExchangeUI ExchangeUI;
    public ExchangeUI ExchangeUIGold;

    public ShopMenuGameData curMenuItemData { get; private set; }

    public List<UIbundleEquip> exchange_equipList = new List<UIbundleEquip>();

    public override void Open(CloseCallback cb = null)
    {
        base.Open(cb);

        if (exchangeCharacter != null)
        {
            exchangeCharacter.startingAnimation = "f_idle_0";
            exchangeCharacter.startingLoop = true;
            exchangeCharacter.Initialize(true);
        }

        exchangeUI_BGM = Managers.Resource.LoadAssetsBundle<AudioClip>("AssetsBundle/Sounds/bgm/BGM_TRADE");
        if (exchangeUI_BGM != null)
            Managers.Sound.Play(exchangeUI_BGM, Sound.Bgm);

        if (curMenuItemData != null)
        {
            ShopMenuGameData curMenu = curMenuItemData;
            curMenuItemData = null;

            OnMenuItem(curMenu);
        }
    }

    public void SetMenu(int type)
    {
        List<GameData> menus = ShopMenuGameData.GetTraderMenuListWithSort();
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

    public override void Close()
    {
        ExchangeUI.SlotAllClear();
        base.Close();
        Managers.Scene.CurrentScene.StartBackgroundMusic(false);
        exchange_equipList.Clear();
    }
    public override void RefreshUI()
    {
        base.RefreshUI();

        if (TypeA.activeInHierarchy)
        {
            TypeA.SetActive(true);
            TypeB.SetActive(false);
            TypeC.SetActive(false);
        }
        List<GameData> menus = ShopMenuGameData.GetTraderMenuListWithSort();

        exchangeCharacter.gameObject.SetActive(menus.Count <= 4);

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
        if (curMenuItemData != null && curMenuItemData.pageType != ShopMenuGameData.PAGE_TYPE.EXCHANGE_EVENT)
        {
            event_tble.SetActive(false);
            event_assets.SetActive(false);
        }
    }
    public void OnMenuItem(ScrollUIControllerItem caller)
    {
        ShopMenuItem menu = (caller as ShopMenuItem);

        OnMenuItem(menu.shopMenuData);
    }

    public void OnMenuItem(ShopMenuGameData shopMenuData)
    {
        if (curMenuItemData == shopMenuData)
            return;

        if (shopMenuData.pageType == ShopMenuGameData.PAGE_TYPE.SOULCARD_TRADE)
        {
            TypeA.SetActive(false);
            TypeB.SetActive(true);
            TypeC.SetActive(false);
            TypeB.GetComponent<ExchangeUI>().Init();
        }
        else if(shopMenuData.pageType == ShopMenuGameData.PAGE_TYPE.EQUIPMENT_TRADE)
        {
            TypeA.SetActive(false);
            TypeB.SetActive(false);
            TypeC.SetActive(true);
            TypeC.GetComponent<ExchangeUI>().Init();
        }
        else
        {
            TypeA.SetActive(true);
            TypeB.SetActive(false);
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
                case ShopMenuGameData.PAGE_TYPE.EXCHANGE_EVENT:
                    items = ShopItemGameData.GetShopItemWithSort(shopMenuData.GetID());

                    event_tble.SetActive(true);
                    event_tble.transform.Find("Event_Range_Text").GetComponent<Timer>().InitTime(shopMenuData.end_event, () => { RefreshUI(); });

                    if (items.Count > 0)
                    {
                        List<ShopItem> shopItems = new List<ShopItem>();
                        foreach (ShopItemGameData item in items)
                        {
                            if (item.price.type == ASSET_TYPE.ITEM)
                            {
                                if (shopItems.Count > 0)
                                {
                                    if (shopItems[0].priceIcon != item.price.priceIcon)
                                        shopItems.Add(item.price);
                                }
                                else
                                    shopItems.Add(item.price);
                            }
                        }

                        if (shopItems.Count == 1)
                        {
                            event_assets.SetActive(true);
                            event_assets.GetComponent<ExchangeAsset>().SetSlot(shopItems[0].priceIcon, shopItems[0].param);
                        }
                        else
                            event_assets.gameObject.SetActive(false);

                    }
                    if (items == null || items.Count == 0)
                    {
                        PopupCanvas.Instance.ShowPopup(PopupCanvas.POPUP_TYPE.DEVLOPING_NOTICE_POPUP);
                        return;
                    }
                    break;
            }
        }
        curMenuItemData = shopMenuData;

        RefreshUI();
    }
    public Color GetAbleColor()
    {
        return ableColor;
    }
    public Color GetEnableColor()
    {
        return disableColor;
    }

}
