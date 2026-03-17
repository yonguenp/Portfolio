using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopMainScrollUI : ScrollUIController
{
    [Serializable]
    public class FixedColumnItem
    {
        public RectTransform ColumnContainer;
        public GameObject RowContainer;
        public ShopMainItemUI[] ColumnItem;
        public GameObject[] priceTag;
    }

    [SerializeField]
    FixedColumnItem Column3;
    [SerializeField]
    FixedColumnItem Column4;
    [SerializeField]
    FixedColumnItem RandomShop;
    [SerializeField]
    FixedColumnItem PackageShop_Column3;

    [SerializeField]
    ShopTitle Title;

    [SerializeField]
    ShopPopup ShopPopup;
    [SerializeField]
    ExchangePopup exchangePopup;

    [SerializeField]
    GameObject RefershPopup;
    [SerializeField]
    Text RefreshCostText;

    FixedColumnItem curTargetUI = null;
    int contentCount = 0;
    List<GameObject> rowList = new List<GameObject>();
    int refreshCount = 10;
    public override void Clear()
    {
        if (curTargetUI != null)
        {
            if (curTargetUI.RowContainer != null)
                curTargetUI.RowContainer.SetActive(false);

            if (curTargetUI.ColumnContainer != null)
            {
                foreach (Transform child in curTargetUI.ColumnContainer)
                {
                    if (curTargetUI.RowContainer.transform != child)
                    {
                        Destroy(child.gameObject);
                    }
                }
            }
        }

        if (Column3.ColumnContainer != null)
            Column3.ColumnContainer.gameObject.SetActive(false);
        if (Column4.ColumnContainer != null)
            Column4.ColumnContainer.gameObject.SetActive(false);
        if (RandomShop.ColumnContainer != null)
            RandomShop.ColumnContainer.gameObject.SetActive(false);
        if (PackageShop_Column3.ColumnContainer != null)
            PackageShop_Column3.ColumnContainer.gameObject.SetActive(false);

        contentCount = 0;
        rowList.Clear();
        OnRefreshClear();
    }

    protected override ScrollUIControllerItem AddItem()
    {
        int index = contentCount % curTargetUI.ColumnItem.Length;
        if (index == 0)
        {
            curTargetUI.RowContainer.SetActive(true);

            GameObject row = Instantiate(curTargetUI.RowContainer);
            row.transform.SetParent(curTargetUI.ColumnContainer);
            row.transform.localPosition = Vector3.zero;
            row.transform.localScale = Vector3.one;

            foreach (Transform child in row.transform.Find("Row").transform)
            {
                child.GetComponent<ShopMainItemUI>().SetActive(false);
                child.gameObject.SetActive(true);
            }
            curTargetUI.RowContainer.SetActive(false);

            rowList.Add(row.transform.Find("Row").gameObject);
        }

        GameObject item = rowList[rowList.Count - 1].transform.GetChild(index).gameObject;
        ShopMainItemUI ret = item.GetComponent<ShopMainItemUI>();
        ret.SetActive(true);
        contentCount++;

        return ret;
    }

    public void RefreshUI()
    {
        ShopMenuGameData popupMenuItem = null;
        if (ShopPopup != null)
            popupMenuItem = ShopPopup.curMenuItemData;
        else if (exchangePopup != null)
            popupMenuItem = exchangePopup.curMenuItemData;

        int menuID = popupMenuItem.GetID();
        List<GameData> items = new List<GameData>();

        bool userTimer = false;
        switch (popupMenuItem.pageType)
        {
            case ShopMenuGameData.PAGE_TYPE.COLUMN_3:
                SetColumn3Page();
                items = ShopItemGameData.GetShopItemWithSort(menuID);
                if (items.Count == 0)
                {
                    PopupCanvas.Instance.ShowPopup(PopupCanvas.POPUP_TYPE.DEVLOPING_NOTICE_POPUP);
                }
                break;
            case ShopMenuGameData.PAGE_TYPE.EXCHANGE_EVENT:
            case ShopMenuGameData.PAGE_TYPE.COLUMN_4:
                SetColumn4Page();
                items = ShopItemGameData.GetShopItemWithSort(menuID);
                if (items.Count == 0)
                {
                    PopupCanvas.Instance.ShowPopup(PopupCanvas.POPUP_TYPE.DEVLOPING_NOTICE_POPUP);
                }
                break;
            case ShopMenuGameData.PAGE_TYPE.RANDOM:
                SetRandomPage(menuID);
                userTimer = true;
                break;
            case ShopMenuGameData.PAGE_TYPE.PACKAGE_COLUMN_3:
                SetPackageColumn3Page();
                items = ShopItemGameData.GetShopItemWithSort(menuID);
                if (items.Count == 0)
                {
                    PopupCanvas.Instance.ShowPopup(PopupCanvas.POPUP_TYPE.DEVLOPING_NOTICE_POPUP);
                }
                break;

        }

        if (ShopPopup != null)
            Title.SetTitle(popupMenuItem.GetName(), userTimer);

        if (curTargetUI != null)
        {
            GetScroll().content = curTargetUI.ColumnContainer;

            SetScrollList(items, OnShopItem);
            curTargetUI.ColumnContainer.gameObject.SetActive(true);
        }
    }

    public void OnTopPosition()
    {
        GetScroll().verticalNormalizedPosition = 1.0f;
    }

    private void SetColumn3Page()
    {
        curTargetUI = Column3;
    }
    private void SetColumn4Page()
    {
        curTargetUI = Column4;
    }
    private void SetRandomPage(int menuID)
    {
        curTargetUI = RandomShop;

        OnRandomShopItemWithSort(menuID);
    }
    private void SetPackageColumn3Page()
    {
        curTargetUI = PackageShop_Column3;
    }

    private void SetTimer(long timer, int count)
    {
        long diff = (timer + GameConfig.Instance.RANDOM_SHOP_REFRESH_TIME) - SBCommonLib.SBUtil.GetCurrentSecTimestamp();

        if (diff < 0)
        {
            PopupCanvas.Instance.ShowFadeText("오류발생");
            return;
        }
        refreshCount = count;
        Title.SetRefreshCount(count);
        Title.SetTimer((int)diff, TimeDone);
    }

    public void TimeDone()
    {
        if (ShopPopup == null || ShopPopup.curMenuItemData == null)
            return;

        OnRandomShopItemWithSort(ShopPopup.curMenuItemData.GetID());
    }

    public void OnRandomShopItemWithSort(int menuID)
    {
        Clear();

        Title.SetTitle("", true);
        curTargetUI = RandomShop;

        SBWeb.GetRandomShopItems(menuID, OnRandomItemData);
    }

    public void OnRandomItemData(JToken res)
    {
        JToken response = res["random_list"];

        List<GameData> items = new List<GameData>();
        List<string> goodsList = new List<string>();
        List<string> purchaseList = new List<string>();
        if (response["list"] != null)
        {
            string strlist = response["list"].Value<string>();
            goodsList.AddRange(strlist.Split(','));
        }

        if (response["purchase_list"] != null)
        {
            string strlist = response["purchase_list"].Value<string>();
            purchaseList.AddRange(strlist.Split(','));
        }

        List<GameData> list = GameDataManager.Instance.GetData(GameDataManager.DATA_TYPE.shop_goods);
        List<GameData> limitOverItems = new List<GameData>();

        foreach (string goods_id in goodsList)
        {
            ShopItemGameData data = ShopItemGameData.GetShopData(int.Parse(goods_id));
            if (data != null)
            {
                if (purchaseList.Contains(data.GetID().ToString()))
                {
                    purchaseList.Remove(data.GetID().ToString());
                    limitOverItems.Add(data);
                }
                else
                {
                    items.Add(data);
                }
            }
        }

        items.Sort((a, b) =>
        {
            return ((ShopItemGameData)a).priority.CompareTo(((ShopItemGameData)b).priority);
        });

        limitOverItems.Sort((a, b) =>
        {
            return ((ShopItemGameData)a).priority.CompareTo(((ShopItemGameData)b).priority);
        });

        GetScroll().content = curTargetUI.ColumnContainer;

        SetScrollList(items, OnRandomShopItem);
        curTargetUI.ColumnContainer.gameObject.SetActive(true);

        foreach (GameData limitOverItem in limitOverItems)
        {
            ShopMainItemUI item = AddItem(limitOverItem, null) as ShopMainItemUI;
            if (item != null)
            {
                item.SetSoldOut();
            }
        }

        JToken refresh_data = res["random_refresh"];
        int refreshCount = refresh_data.Value<int>();

        SetTimer(response["updated"].Value<long>(), refreshCount);
    }

    public void OnShopItem(ScrollUIControllerItem caller)
    {
        ShopMainItemUI menu = (caller as ShopMainItemUI);

        ShopItem price = menu.itemData.price;
        switch (price.type)
        {
            case ASSET_TYPE.GOLD:
                if (Managers.UserData.MyGold < price.amount)
                {
                    PopupCanvas.Instance.ShowFadeText("골드부족");
                    return;
                }
                break;
            case ASSET_TYPE.DIA:
                if (Managers.UserData.MyDia < price.amount)
                {
                    PopupCanvas.Instance.ShowFadeText("다이아부족");
                    return;
                }
                break;
            case ASSET_TYPE.CASH:
                PopupCanvas.Instance.ShowBuyPopup(menu.itemData, (cnt) =>
                {
                    Managers.IAP.TryPurchase((uint)menu.itemData.GetID(), ShopPackageGameData.GetIAPConstants(menu.itemData.GetID()), (responseArr) =>
                    {
                        PopupCanvas.Instance.ShowFadeText("결제성공");
                        Managers.UserData.UpdateMyShopInfo(menu.itemData.GetID(), cnt);
                        ShopPopup.RefreshUI();
                    }, (responseArr) =>
                    {
                        PopupCanvas.Instance.ShowFadeText("결제실패");
                    });
                });
                return;
            case ASSET_TYPE.ITEM:
                if(Managers.UserData.GetMyItemCount(price.param) < price.amount)
                {
                    PopupCanvas.Instance.ShowFadeText("msg_change_itemfail");
                    return;

                }
                break;
            case ASSET_TYPE.ADVERTISEMENT:
                if (PopupCanvas.Instance.IsOpeningPopup(PopupCanvas.POPUP_TYPE.MATCH_INFO_POPUP))
                {
                    PopupCanvas.Instance.ShowFadeText("매치대기중사용불가");
                    return;
                }

                if (!Managers.ADS.IsAdvertiseReady())
                {
                    PopupCanvas.Instance.ShowFadeText("광고로드실패");
                    return;
                }
                break;
            case ASSET_TYPE.MILEAGE:
                if (Managers.UserData.MyMileage < price.amount)
                {
                    PopupCanvas.Instance.ShowFadeText("마일리지부족");
                    return;
                }
                break;
        }

        PopupCanvas.Instance.ShowBuyPopup(menu.itemData, (count) =>
        {
            if (count < 0)
                return;

            switch (menu.itemData.price.type)
            {
                case ASSET_TYPE.ADVERTISEMENT:
                    Managers.ADS.TryADWithCallback(() =>
                    {
                        SBWeb.OnBuy(menu.itemData.GetID(), count, (rewards) =>
                        {
                            PopupCanvas.Instance.ShowBuyResult(rewards);
                            Managers.UserData.UpdateMyShopInfo(menu.itemData.GetID(), count);
                            RefreshUI();
                        });
                    }, () =>
                    {
                        PopupCanvas.Instance.ShowFadeText("광고취소");
                    });
                    break;
                default:
                    SBWeb.OnBuy(menu.itemData.GetID(), count, (rewards) =>
                    {
                        PopupCanvas.Instance.ShowBuyResult(rewards);
                        Managers.UserData.UpdateMyShopInfo(menu.itemData.GetID(), count);
                        RefreshUI();
                    });
                    break;
            }
        });
    }

    public void OnRandomShopItem(ScrollUIControllerItem caller)
    {
        ShopMainItemUI menu = (caller as ShopMainItemUI);

        foreach (var reward in menu.itemData.rewards)
        {
            switch ((ASSET_TYPE)reward.goods_type)
            {
                case ASSET_TYPE.ITEM:
                    var targetItem = reward.targetItem;
                    if(targetItem != null)
                    {
                        if(targetItem.type == ItemGameData.ITEM_TYPE.EQUIP_ITEM)
                        {
                            if (EquipConfig.Config.ContainsKey("equip_max") && Managers.UserData.MyEquips.Count > EquipConfig.Config["equip_max"])
                            {
                                PopupCanvas.Instance.ShowMessagePopup("소지장비갯수제한");
                                return;
                            }
                        }
                    }                    
                    break;
                case ASSET_TYPE.EQUIPMENT:
                    if (EquipConfig.Config.ContainsKey("equip_max") && Managers.UserData.MyEquips.Count > EquipConfig.Config["equip_max"])
                    {
                        PopupCanvas.Instance.ShowMessagePopup("소지장비갯수제한");
                        return;
                    }
                    break;
                default:
                    break;
            }
        }

        ShopItem price = menu.itemData.price;
        switch (price.type)
        {
            case ASSET_TYPE.GOLD:
                if (Managers.UserData.MyGold < price.amount)
                {
                    PopupCanvas.Instance.ShowFadeText("골드부족");
                    return;
                }
                break;
            case ASSET_TYPE.DIA:
                if (Managers.UserData.MyDia < price.amount)
                {
                    PopupCanvas.Instance.ShowFadeText("다이아부족");
                    return;
                }
                break;
            case ASSET_TYPE.MILEAGE:
                if (Managers.UserData.MyMileage < price.amount)
                {
                    PopupCanvas.Instance.ShowFadeText("마일리지부족");
                    return;
                }
                break;
            case ASSET_TYPE.ITEM:
                if (Managers.UserData.GetMyItemCount(price.param) < price.amount)
                {
                    PopupCanvas.Instance.ShowFadeText("msg_change_itemfail");
                    return;

                }
                break;
            case ASSET_TYPE.ADVERTISEMENT:
                if (PopupCanvas.Instance.IsOpeningPopup(PopupCanvas.POPUP_TYPE.MATCH_INFO_POPUP))
                {
                    PopupCanvas.Instance.ShowFadeText("매치대기중사용불가");
                    return;
                }

                if (!Managers.ADS.IsAdvertiseReady())
                {
                    PopupCanvas.Instance.ShowFadeText("광고로드실패");
                    return;
                }
                break;
        }

        PopupCanvas.Instance.ShowBuyPopup(menu.itemData, (count) =>
        {
            switch (menu.itemData.price.type)
            {
                case ASSET_TYPE.ADVERTISEMENT:
                    Managers.ADS.TryADWithCallback(() =>
                    {
                        SBWeb.OnBuyRandomItem(menu.itemData.GetID(), (rewards) =>
                        {
                            PopupCanvas.Instance.ShowBuyResult(rewards);
                            RefreshUI();
                        });
                    }, () =>
                    {
                        PopupCanvas.Instance.ShowFadeText("광고취소");
                    });
                    break;
                default:
                    SBWeb.OnBuyRandomItem(menu.itemData.GetID(), (rewards) =>
                    {
                        PopupCanvas.Instance.ShowBuyResult(rewards);
                        RefreshUI();
                    });
                    break;
            }
        });
    }

    public void OnRefreshButton()
    {
        int diaCost = Mathf.Min(refreshCount * 10, GameConfig.Instance.RANDOM_SHOP_REFRESH_PRICE);
        if (refreshCount > 0)
        {
            if (diaCost > Managers.UserData.MyDia)
            {
                PopupCanvas.Instance.ShowFadeText("다이아부족");
                return;
            }
        }
        else
        {
            if (PopupCanvas.Instance.IsOpeningPopup(PopupCanvas.POPUP_TYPE.MATCH_INFO_POPUP))
            {
                PopupCanvas.Instance.ShowFadeText("매치대기중사용불가");
                return;
            }

            //if (!Managers.ADS.IsAdvertiseReady())
            //{
            //    PopupCanvas.Instance.ShowFadeText("광고로드실패");
            //    return;
            //}

            //Managers.ADS.TryADWithCallback(OnRefreshRandomShop, ()=> { PopupCanvas.Instance.ShowFadeText("광고취소"); });
            //return;
        }

        RefershPopup.SetActive(true);
        RefreshCostText.text = diaCost.ToString();
    }

    public void OnRefreshRandomShop()
    {
        SBWeb.RefreshRandomShopItems(ShopPopup.curMenuItemData.GetID(), OnRandomItemData);
    }

    public void OnRefreshClear()
    {
        if (RefershPopup != null)
            RefershPopup.SetActive(false);
    }

    public GameObject GetResreshPopup()
    {
        return RefershPopup;
    }
}