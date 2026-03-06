using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopMainItemUI : ScrollUIControllerItem
{
    [SerializeField]
    GameObject panel;

    [SerializeField]
    GameObject SoldOut;
    [SerializeField]
    UIBundleItem icon;

    [SerializeField]
    Text itemName;
    [SerializeField]
    Image priceIcon;
    [SerializeField]
    Text price;
    [SerializeField]
    Button buyButton;

    [SerializeField]
    GameObject priceTag = null;

    [SerializeField]
    Text remainTime;
    [SerializeField]
    Text limitCount;
    [SerializeField]
    GameObject bonusObject;
    [SerializeField]
    Text bonusText;

    public ShopItemGameData itemData { get; private set; }

    public override void SetActive(bool active)
    {
        panel.SetActive(active);
        if (priceTag != null)
            priceTag.SetActive(active);
    }

    public override void SetData(GameData data, ScrollItemSelectCallback cb = null)
    {
        itemData = data as ShopItemGameData;

        if (priceTag != null)
        {
            if (priceTag.transform.Find("name") != null)
                itemName = priceTag.transform.Find("name").GetComponent<Text>();
            priceIcon = priceTag.transform.Find("gold_icon").GetComponent<Image>();
            price = priceTag.transform.Find("price").GetComponent<Text>();
            buyButton = priceTag.GetComponent<Button>();
        }

        if (!itemData.IsBuyLimitValid())
        {
            cb = null;
            if (SoldOut != null)
                SoldOut.SetActive(true);

            if (buyButton != null)
                buyButton.interactable = false;
        }
        else
        {
            if (buyButton != null)
                buyButton.interactable = true;

            if (SoldOut != null)
            {
                SoldOut.SetActive(false);
                priceIcon.color = buyButton.colors.normalColor;
                price.color = buyButton.colors.normalColor;
            }


        }

        base.SetData(data, cb);

        itemName.text = itemData.GetName();
        icon.SetShopItem(itemData, () => { if (cb != null) cb.Invoke(this); });

        if (bonusObject != null)
        {
            bonusObject.SetActive(false);

            if (bonusText != null)
            {
                string bonus = itemData.GetBonusString();
                if (!string.IsNullOrEmpty(bonus))
                {
                    bonusObject.SetActive(true);
                    bonusText.text = bonus;
                }
            }
        }

        bool enable = true;
        if (remainTime != null)
        {
            GameObject timeBackPanel = remainTime.transform.parent.gameObject;
            var ret = string.Empty;
            if (itemData.endTime != DateTime.MaxValue)
            {
                timeBackPanel.SetActive(true);
                var remaindate = itemData.endTime - SBCommonLib.SBUtil.KoreanTime;
                if (remaindate.Days >= 1.0f)
                {
                    ret = StringManager.GetString("ui_day", remaindate.Days);
                }
                else if (remaindate.Hours >= 1.0f)
                {
                    ret = StringManager.GetString("ui_hour", remaindate.Hours);
                }
                else if (remaindate.Minutes >= 1.0f)
                {
                    ret = StringManager.GetString("ui_min", remaindate.Minutes);
                }
                else
                {
                    ret = StringManager.GetString("ui_second", remaindate.Seconds);
                }
            }
            else
            {
                timeBackPanel.SetActive(false);
            }

            remainTime.text = StringManager.GetString("ui_left_time", ret);
        }

        if (limitCount != null)
        {
            GameObject limitBackPanel = limitCount.transform.parent.gameObject;

            string buyType = StringManager.GetString("ui_package_buy");
            switch (itemData.buyType)
            {
                case 1:
                case 2:
                case 3:
                case 4:
                case 5:
                    buyType = StringManager.GetString("ui_store_time_" + itemData.buyType.ToString());
                    break;
                default:
                    break;
            }

            limitCount.text = buyType + " " + (itemData.buyLimit - Managers.UserData.GetMyShopHistory(itemData.GetID())) + "/" + itemData.buyLimit.ToString();
            bool blimit = Managers.UserData.GetMyShopHistory(itemData.GetID()) < itemData.buyLimit;

            if(itemData.buyLimit == 0)
                blimit = true;      //준형 :: buyLimit == 0 구매 무제한 상품

            enable = enable && blimit;

            //limitCount.transform.parent.parent.GetComponent<Image>().enable = false;
            if (!blimit)
            {
                limitBackPanel.SetActive(false);
                SetSoldOut();
            }
            else if (itemData.buyLimit == 0)
            {
                limitBackPanel.SetActive(false);
            }
            else
            {
                limitBackPanel.SetActive(true);
            }

            ShopPopup shop = (PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.SHOP_POPUP) as ShopPopup);
            limitCount.color = blimit ? shop.GetAbleColor() : shop.GetDisableColor();
        }

        ShopItem priceData = itemData.price;
        if (priceData.priceIcon != null)
            priceIcon.sprite = priceData.priceIcon;
        if (priceData.priceIcon == null)
        {
            priceIcon.color = Color.clear;
        }
        if (price != null)
        {
            if (priceData.amount > 0)
            {
                price.text = priceData.amount.ToString("N0");
                if (priceTag.transform.Find("KRW") != null)
                {
                    priceTag.transform.Find("KRW").gameObject.SetActive(false);
                    priceTag.transform.Find("KRW").GetComponent<Text>().text = "";
                }
                switch (priceData.type)
                {
                    case ASSET_TYPE.GOLD:
                        enable = enable && Managers.UserData.MyGold >= priceData.amount;
                        priceIcon.gameObject.SetActive(true);
                        break;
                    case ASSET_TYPE.DIA:
                        enable = enable && Managers.UserData.MyDia >= priceData.amount;
                        priceIcon.gameObject.SetActive(true);
                        break;
                    case ASSET_TYPE.MILEAGE:
                        enable = Managers.UserData.MyMileage >= priceData.amount;
                        priceIcon.gameObject.SetActive(true);
                        break;
                    case ASSET_TYPE.CASH:
                        if (itemData.buyLimit == 0)
                            enable = enable && true;
                        else
                            enable = enable && itemData.buyLimit > Managers.UserData.GetMyShopHistory(itemData.GetID());

                        if (priceTag.transform.Find("KRW") != null)
                        {
                            priceTag.transform.Find("KRW").gameObject.SetActive(true);
                            priceTag.transform.Find("KRW").GetComponent<Text>().text = StringManager.GetString("ui_money_type_kr");
                        }
                        price.text = price.text;
                        priceIcon.gameObject.SetActive(false);
                        break;
                    case ASSET_TYPE.ITEM:
                        enable = enable && Managers.UserData.GetMyItemCount(priceData.param) >= priceData.amount;
                        priceIcon.gameObject.SetActive(true);
                        break;
                    case ASSET_TYPE.ADVERTISEMENT:
                        string remain = "";
                        DateTime pivot = System.DateTime.MaxValue;
                        DateTime ableTime = pivot;
                        switch (itemData.GetID())
                        {
                            case 1001:
                                pivot = Managers.UserData.ADSeen_PACK1;
                                if (pivot < System.DateTime.MaxValue)
                                {
                                    ableTime = pivot.AddHours(4);//.AddDays(1);
                                    //ableTime = ableTime.AddHours((ableTime.Hour * -1) + 4).AddMinutes((ableTime.Minute * -1)).AddSeconds((ableTime.Second * -1));
                                }

                                enable = enable && pivot < System.DateTime.MaxValue && SBCommonLib.SBUtil.KoreanTime > ableTime;
                                break;
                            case 1002:
                                pivot = Managers.UserData.ADSeen_PACK2;
                                if (pivot < System.DateTime.MaxValue)
                                {
                                    ableTime = pivot.AddHours(4);//.AddDays(1);
                                    //ableTime = ableTime.AddHours((ableTime.Hour * -1) + 4).AddMinutes((ableTime.Minute * -1)).AddSeconds((ableTime.Second * -1));
                                }

                                enable = enable && pivot < System.DateTime.MaxValue && SBCommonLib.SBUtil.KoreanTime > ableTime;
                                break;
                            case 1003:
                                pivot = Managers.UserData.ADSeen_PACK3;
                                if (pivot < System.DateTime.MaxValue)
                                    ableTime = pivot.AddMonths(1);

                                enable = enable && pivot < System.DateTime.MaxValue && SBCommonLib.SBUtil.KoreanTime > ableTime;
                                break;
                            default:
                                enable = true;
                                break;
                        }

                        if (enable)
                        {
                            remain = StringManager.GetString("광고시청");
                        }
                        else
                        {
                            if (pivot == System.DateTime.MaxValue)
                            {
                                remain = StringManager.GetString("이용불가");
                            }
                            else
                            {
                                TimeSpan diff = ableTime - SBCommonLib.SBUtil.KoreanTime;
                                if (diff.TotalDays > 1.0f)
                                {
                                    remain = StringManager.GetString("ui_day", (int)diff.TotalDays);
                                }
                                else if (diff.TotalHours > 1.0f)
                                {
                                    remain = StringManager.GetString("ui_hour", (int)diff.TotalHours);
                                }
                                else if (diff.TotalMinutes > 1.0f)
                                {
                                    remain = StringManager.GetString("ui_min", (int)diff.TotalMinutes);
                                }
                                else if (diff.TotalSeconds > 1.0f)
                                {
                                    remain = StringManager.GetString("ui_second", (int)diff.TotalSeconds);
                                }

                                remain = StringManager.GetString("ui_left_time", remain);

                                if (remainTime != null)
                                {
                                    remainTime.transform.parent.gameObject.SetActive(true);

                                    remainTime.gameObject.SetActive(true);
                                    remainTime.text = diff.ToString(@"hh\:mm\:ss");

                                    Invoke("UpdateRemain", 1.0f);
                                }
                            }
                        }
                        price.text = remain;
                        priceIcon.gameObject.SetActive(true);
                        break;
                }

                price.color = enable ? (PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.SHOP_POPUP) as ShopPopup).GetAbleColor() : (PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.SHOP_POPUP) as ShopPopup).GetDisableColor();
                if (buyButton != null)
                    buyButton.interactable = enable;
            }
            else
            {
                price.text = StringManager.GetString("ui_free");
            }
        }

        if (cb != null)
        {
            if (buyButton != null)
            {
                buyButton.onClick.AddListener(() =>
                {
                    if (cb != null)
                        cb.Invoke(this);
                });
            }
        }
    }

    public void SetSoldOut()
    {
        Color disableColor = Color.gray;
        if (buyButton != null)
            buyButton.interactable = false;

        if (SoldOut != null)
        {
            SoldOut.SetActive(true);

            priceIcon.color = disableColor;
            price.color = disableColor;
        }

        foreach (MaskableGraphic child in GetComponentsInChildren<MaskableGraphic>())
        {
            if (child.transform == SoldOut.transform)
                continue;

            child.color = disableColor;
        }
    }

    void UpdateRemain()
    {
        if (remainTime == null)
            return;

        TimeSpan time = TimeSpan.MinValue;
        try
        {
            time = TimeSpan.Parse(remainTime.text);
            time = new TimeSpan(time.Hours, time.Minutes, time.Seconds - 1);
        }
        catch
        {
            remainTime.transform.parent.gameObject.SetActive(false);
            return;
        }

        if (time <= TimeSpan.MinValue)
        {
            remainTime.transform.parent.gameObject.SetActive(false);
            return;
        }

        remainTime.gameObject.SetActive(true);
        remainTime.text = time.ToString(@"hh\:mm\:ss");

        Invoke("UpdateRemain", 1.0f);
    }

}
