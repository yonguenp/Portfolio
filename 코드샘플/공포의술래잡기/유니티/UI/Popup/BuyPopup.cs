using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuyPopup : Popup
{
    [SerializeField]
    Text ShopItemName;
    [SerializeField]
    UIBundleItem bundle;
    [SerializeField]
    Text NeedPrice;
    [SerializeField]
    Image NeedPriceIcon;
    [SerializeField]
    Slider slider;
    [SerializeField]
    Text countText;
    [SerializeField]
    Text descText;

    [SerializeField]
    GameObject typeA;
    [SerializeField]
    GameObject typeB;
    [SerializeField]
    GameObject krwObj;

    [SerializeField]
    Text title;

    ShopItemGameData shopItem = null;
    public delegate void BuyCallback(int count);
    protected BuyCallback buyCallback = null;

    public void Init(ShopItemGameData s, BuyCallback cb, int type)
    {
        if(PopupCanvas.Instance.IsOpeningPopup(PopupCanvas.POPUP_TYPE.EXCHANGE_POPUP))
        {
            title.text = StringManager.GetString("ui_exchange_title");
        }
        else
        {
            title.text = StringManager.GetString("ui_buy_title");
        }

        shopItem = s;
        buyCallback = cb;
        ShopItemName.text = shopItem.GetName();

        NeedPrice.gameObject.SetActive(true);

        foreach(Transform child in bundle.transform.parent)
        {
            if(child != bundle.transform)
                Destroy(child.gameObject);
        }
        bundle.gameObject.SetActive(true);        
        foreach(ShopPackageGameData reward in shopItem.rewards)
        {
            GameObject multiItemRow = Instantiate(bundle.gameObject);
            multiItemRow.transform.SetParent(bundle.transform.parent);
            multiItemRow.transform.localPosition = Vector3.zero;
            multiItemRow.transform.localScale = Vector3.one;

            UIBundleItem item = multiItemRow.GetComponent<UIBundleItem>();
            item.SetReward(reward);
        }
        bundle.gameObject.SetActive(false);

        NeedPriceIcon.gameObject.SetActive(shopItem.price.priceIcon != null);
        NeedPriceIcon.sprite = shopItem.price.priceIcon;

        NeedPrice.text = shopItem.price.amount.ToString("N0");
        bool enable = true;
        if (shopItem.price.type == ASSET_TYPE.ADVERTISEMENT)
        {
            string remain = "";
            DateTime pivot = System.DateTime.MaxValue;
            DateTime ableTime = pivot;
            switch (shopItem.GetID())
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
                    if (diff.TotalDays >= 1.0f)
                    {
                        remain = StringManager.GetString("ui_day", (int)diff.TotalDays);
                    }
                    else if (diff.TotalHours >= 1.0f)
                    {
                        remain = StringManager.GetString("ui_hour", (int)diff.TotalHours);
                    }
                    else if (diff.TotalMinutes >= 1.0f)
                    {
                        remain = StringManager.GetString("ui_min", (int)diff.TotalMinutes);
                    }
                    else if (diff.TotalSeconds >= 1.0f)
                    {
                        remain = StringManager.GetString("ui_second", (int)diff.TotalSeconds);
                    }

                    remain = StringManager.GetString("ui_left_time", remain);
                }

                buyCallback = (res) =>
                {
                    PopupCanvas.Instance.ShowFadeText("이용불가");
                };
            }

            NeedPrice.text = remain;
        }

        NeedPrice.color = enable ? (PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.SHOP_POPUP) as ShopPopup).GetAbleColor() : (PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.SHOP_POPUP) as ShopPopup).GetDisableColor();
        //캐쉬타입
        krwObj.SetActive(type == 1);

        if (type == 0)
        {
            typeB.SetActive(false);
            slider.minValue = 1;
            slider.value = 1;
            krwObj.SetActive(false);

            if (shopItem.price.type == ASSET_TYPE.CASH)
            {
                slider.maxValue = 1;
                krwObj.SetActive(true);
                NeedPrice.text = NeedPrice.text;
            }
            else if (shopItem.buyLimit == 0)
            {
                if (shopItem.price.amount > 0)
                {
                    switch (shopItem.price.type)
                    {
                        case ASSET_TYPE.GOLD:
                            slider.maxValue = Managers.UserData.MyGold / shopItem.price.amount; break;
                        case ASSET_TYPE.DIA:
                            slider.maxValue = Managers.UserData.MyDia / shopItem.price.amount; break;
                        case ASSET_TYPE.MILEAGE:
                            slider.maxValue = Managers.UserData.MyMileage / shopItem.price.amount; break;
                        case ASSET_TYPE.ITEM:
                            slider.maxValue = Managers.UserData.GetMyItemCount(shopItem.price.param) / shopItem.price.amount; break;
                        default:
                            slider.maxValue = 1;
                            break;
                    }
                }
                else
                {
                    slider.maxValue = 100;
                }
            }
            else
            {
                int buyLimit = 100;
                if (shopItem.buyLimit > 0)
                {
                    buyLimit = shopItem.buyLimit;
                }

                int buyCount = Managers.UserData.GetMyShopHistory(shopItem.GetID());
                buyLimit = buyLimit - buyCount;

                if (shopItem.price.amount > 0)
                {
                    switch (shopItem.price.type)
                    {
                        case ASSET_TYPE.GOLD:
                            buyLimit = Mathf.Min(buyLimit, Managers.UserData.MyGold / shopItem.price.amount); break;
                        case ASSET_TYPE.DIA:
                            buyLimit = Mathf.Min(buyLimit, Managers.UserData.MyDia / shopItem.price.amount); break;
                        case ASSET_TYPE.MILEAGE:
                            buyLimit = Mathf.Min(buyLimit, Managers.UserData.MyMileage / shopItem.price.amount); break;
                        case ASSET_TYPE.ITEM:
                            buyLimit = Mathf.Min(buyLimit, Managers.UserData.GetMyItemCount(shopItem.price.param) / shopItem.price.amount); break;
                        default:
                            slider.maxValue = 1;
                            break;
                    }
                }

                slider.maxValue = buyLimit;
            }

            typeA.SetActive(slider.maxValue > 1);
            countText.text = ((int)(slider.value)).ToString() + "/" + ((int)slider.maxValue).ToString();
        }
        else
        {
            typeA.SetActive(false);
            typeB.SetActive(true);

            descText.text = s.GetDesc();
        }
    }

    public override void Close()
    {
        base.Close();
    }

    public void OnBuy()
    {
        buyCallback?.Invoke((int)slider.value);

        CloseForce();
    }


    public void OnPlus()
    {
        slider.value += 1;

        if (slider.value > slider.maxValue)
            slider.value = slider.maxValue;
        OnSlide();
    }

    public void OnMinus()
    {
        slider.value -= 1;

        if (slider.value < slider.minValue)
            slider.value = slider.minValue;

        OnSlide();
    }


    public void OnSlide()
    {
        if (shopItem == null)
            return;
        if (shopItem.price.type == ASSET_TYPE.ADVERTISEMENT)
            return;

        int count = (int)(slider.value);
        countText.text = count.ToString() + "/" + ((int)slider.maxValue).ToString();
        NeedPrice.text = (count * shopItem.price.amount).ToString();
    }
}
