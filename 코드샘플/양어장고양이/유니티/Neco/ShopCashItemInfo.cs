using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopCashItemInfo : MonoBehaviour
{
    [Header("[Cat Leaf Item Object]")]
    public GameObject catLeafItemLayerObject;
    public GameObject catLeafItemDimmedObject;
    public Image catLeafItemIcon;
    public Text catLeafItemName;
    public Text catLeafItemDescText;

    [Space(15)]
    public Image catLeafItemPriceIcon;
    public Text catLeafItemPriceAmount;
    public Color catLeafPriceOriginColor;
    public Color catLeafPriceAlertColor;

    [Header("[Cat Leaf Buy Object]")]
    public GameObject catLeafBuyLayerObject;
    public GameObject catLeafBuyDimmedObject;
    public Image catLeafBuyIcon;
    public Text catLeafBuyName;
    public Text catLeafBuyAmountText;

    //public Image catLeafBuyPriceIcon;
    public Text catLeafBuyPriceAmount;

    [Header("[Layout List]")]
    public RectTransform catLeafItemlayoutRect;

    neco_shop curShopData;
    neco_package curPackageData;
    bool isCatLeafItem = false;

    uint userCatLeaf;
    uint needCatLeaf;

    NecoShopPanel rootParentPanel;

    public void OnClickCatLeafItem()
    {
        // 구매 제한 상품 체크
        if (curShopData.GetNecoShopPurchaseType() == "daily")
        {
            if (neco_data.Instance.GetPurchaseCount(curShopData.GetNecoShopID()) >= curShopData.GetNecoShopPurchaseLimit())
            {
                NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(LocalizeData.GetText("LOCALIZE_374"), LocalizeData.GetText("LOCALIZE_375"));
                return;
            }
        }

        ConfirmPopupData popupData = SetConfirmPopupData();

        if (isCatLeafItem && userCatLeaf < needCatLeaf)
        {    
            if (curShopData.GetNecoShopPriceType() == "point")
            {
                NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("LOCALIZE_378"));
            }
            else
            {
                NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("LOCALIZE_203"));
            }
            return;
        }

        if (curShopData.GetNecoShopID() == 8 || curShopData.GetNecoShopID() == 50)
        {
            NecoCanvas.GetPopupCanvas().OnShopPurchaseCountPopupShow(false, curShopData, null, userCatLeaf / needCatLeaf, TryPurchase);
        }
        else
        {
            CONFIRM_POPUP_TYPE popupType = isCatLeafItem ? CONFIRM_POPUP_TYPE.COMMON : CONFIRM_POPUP_TYPE.COMMON_WITHDRAWAL;
            NecoCanvas.GetPopupCanvas().OnSystemConfirmPopupShow(popupData, popupType, TryPurchase);
        }
            
    }

    public void SetCatLeafItemData(neco_shop shopData, NecoShopPanel parentPanel)
    {
        if (shopData == null) { return; }

        ClearData();

        curShopData = shopData;
        rootParentPanel = parentPanel;
        isCatLeafItem = true;

        curPackageData = neco_package.GetNecoPackageByID(curShopData.GetNecoShopID());

        if (curPackageData == null) { return; }

        // 상점 아이템 데이터 세팅
        userCatLeaf = rootParentPanel.GetUserResource(NecoShopPanel.SHOP_RESOURCE_TYPE.CAT_LEAF);
        if (curShopData.GetNecoShopPriceType() == "point")
            userCatLeaf = rootParentPanel.GetUserResource(NecoShopPanel.SHOP_RESOURCE_TYPE.POINT);
        needCatLeaf = curShopData.GetNecoShopPrice();

        catLeafItemLayerObject.SetActive(true);

        catLeafItemIcon.sprite = curShopData.GetNecoShopIcon();
        catLeafItemName.text = curShopData.GetNecoShopName();
        catLeafItemDescText.text = curPackageData.GetNecoPackageDesc();

        if(curShopData.GetNecoShopPurchaseType() == "daily")
        {
            uint cur = neco_data.Instance.GetPurchaseCount(curShopData.GetNecoShopID());
            uint max = curShopData.GetNecoShopPurchaseLimit();
            catLeafItemDescText.text = LocalizeData.GetText("daily_limit") + "(" + cur.ToString() + "/" + max.ToString() + ")";

            catLeafItemDimmedObject.SetActive(cur == max);
        }

        catLeafItemPriceIcon.sprite = curShopData.GetNecoShopPriceIcon();
        catLeafItemPriceAmount.text = curShopData.GetNecoShopPrice().ToString("n0");
        catLeafItemPriceAmount.color = userCatLeaf >= needCatLeaf ? catLeafPriceOriginColor : catLeafPriceAlertColor;
        if (catLeafItemPriceAmount.GetComponent<Outline>() != null)
        {
            catLeafItemPriceAmount.GetComponent<Outline>().enabled = userCatLeaf >= needCatLeaf;
        }

        if (catLeafItemlayoutRect != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(catLeafItemlayoutRect);
        }
    }

    public void SetCatLeafBuyData(neco_shop shopData, NecoShopPanel parentPanel)
    {
        if (shopData == null) { return; }

        ClearData();

        curShopData = shopData;
        rootParentPanel = parentPanel;
        isCatLeafItem = false;

        curPackageData = neco_package.GetNecoPackageByID(curShopData.GetNecoShopID());

        if (curPackageData == null) { return; }

        catLeafBuyLayerObject.SetActive(true);

        catLeafBuyIcon.sprite = curShopData.GetNecoShopIcon();
        catLeafBuyName.text = curShopData.GetNecoShopName();
        catLeafBuyAmountText.text = curPackageData.GetNecoPackageDesc();

        catLeafBuyPriceAmount.text = string.Format("\\ {0}", curShopData.GetNecoShopPrice().ToString("n0"));
    }

    void TryPurchase()
    {
        TryPurchase(1);
    }

    void TryPurchase(uint count)
    {
        string product = curShopData.GetIAPConstants();
        if (string.IsNullOrEmpty(product))
        {
            WWWForm data = new WWWForm();
            data.AddField("api", "shop");
            data.AddField("op", 1);
            data.AddField("list", 0);
            data.AddField("prod", curShopData.GetNecoShopID().ToString());
            data.AddField("cnt", count.ToString());

            NetworkManager.GetInstance().SendApiRequest("shop", 1, data, (response) =>
            {
                JObject root = JObject.Parse(response);
                JToken apiToken = root["api"];
                if (null == apiToken || apiToken.Type != JTokenType.Array
                    || !apiToken.HasValues)
                {
                    return;
                }

                JArray apiArr = (JArray)apiToken;
                foreach (JObject row in apiArr)
                {
                    string uri = row.GetValue("uri").ToString();
                    if (uri == "shop")
                    {
                        JToken resultCode = row["rs"];

                        if (resultCode != null && resultCode.Type == JTokenType.Integer)
                        {
                            int rs = resultCode.Value<int>();
                            if (rs == 0)
                            {
                                if(curShopData.GetNecoShopPurchaseType() == "daily")
                                {
                                    RefreshDailyInfo();
                                    NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(LocalizeData.GetText("LOCALIZE_361"), LocalizeData.GetText("LOCALIZE_362"));
                                }
                                else
                                {
                                    SuccessPurchase(null);
                                }
                            }
                            else
                            {
                                string msg = rs.ToString();
                                switch (rs)
                                {
                                    case 1: msg = LocalizeData.GetText("LOCALIZE_355"); break;
                                    case 2: msg = LocalizeData.GetText("LOCALIZE_356"); break;
                                    case 3: msg = LocalizeData.GetText("LOCALIZE_357"); break;
                                    case 4: msg = LocalizeData.GetText("LOCALIZE_358"); break;
                                    case 5: msg = LocalizeData.GetText("LOCALIZE_359"); break;
                                }

                                NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(LocalizeData.GetText("LOCALIZE_360"), msg);                      
                            }
                        }
                    }
                }
            });
            return;
        }

        IAPManager.GetInstance().TryPurchase(curShopData.GetNecoShopID(), product, SuccessPurchase, FailPurchase);
    }

    public void RefreshDailyInfo()
    {
        WWWForm data = new WWWForm();
        data.AddField("api", "iap");
        data.AddField("op", 3);

        NetworkManager.GetInstance().SendApiRequest("iap", 3, data, (res) =>
        {
            JObject root = JObject.Parse(res);
            JToken apiToken = root["api"];
            if (null == apiToken || apiToken.Type != JTokenType.Array
                || !apiToken.HasValues)
            {
                return;
            }

            JArray apiArr = (JArray)apiToken;
            foreach (JObject row in apiArr)
            {
                string uri = row.GetValue("uri").ToString();
                if (uri == "iap")
                {
                    JToken resultCode = row["rs"];
                    if (resultCode != null && resultCode.Type == JTokenType.Integer)
                    {
                        int rs = resultCode.Value<int>();
                        if (rs == 0)
                        {
                            neco_data.Instance.SetPurchaseHistory((JObject)row["cnt"]);
                            rootParentPanel.RefreshLayer();
                        }
                        else
                        {
                            string msg = rs.ToString();
                            switch (rs)
                            {
                                case 1: msg = LocalizeData.GetText("LOCALIZE_199"); break;
                                case 2: msg = LocalizeData.GetText("LOCALIZE_507"); break;
                                case 3: msg = LocalizeData.GetText("LOCALIZE_375"); break;
                                case 4: msg = LocalizeData.GetText("LOCALIZE_199"); break;
                                case 5: msg = LocalizeData.GetText("LOCALIZE_199"); break;
                                case 6: msg = LocalizeData.GetText("LOCALIZE_333"); break;
                            }

                            NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(LocalizeData.GetText("LOCALIZE_360"), msg);
                        }
                    }
                }
            }
        });
    }

    public void SuccessPurchase(JArray responseArr)
    {
        NecoCanvas.GetPopupCanvas().OnPopupClose(NecoPopupCanvas.POPUP_TYPE.SHOP_BUY_COUNT_POPUP);

        if (curShopData.GetNecoShopPriceType() == "cash")
        {
            foreach (JObject row in responseArr)
            {
                string uri = row.GetValue("uri").ToString();
                if (uri == "iap")
                {
                    if (row.ContainsKey("rew"))
                    {
                        NecoCanvas.GetPopupCanvas().OnRewardPopupShow(LocalizeData.GetText("LOCALIZE_361"), LocalizeData.GetText("LOCALIZE_363"), "iap", responseArr, () =>{
                            if (rootParentPanel != null)
                            {
                                rootParentPanel.RefreshLayer();
                            }
                            else
                            {
                                NecoCanvas.GetPopupCanvas().RefreshPopupData(NecoPopupCanvas.POPUP_REFRESH_TYPE.SHOP_CARD);
                            }
                        });

                        return;
                    }
                }
            }

            NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(LocalizeData.GetText("LOCALIZE_361"), LocalizeData.GetText("LOCALIZE_362"));
        }
        else
            NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(LocalizeData.GetText("LOCALIZE_361"), LocalizeData.GetText("LOCALIZE_363"));

        if (rootParentPanel != null)
        {
            rootParentPanel.RefreshLayer();
        }
        else
        {
            NecoCanvas.GetPopupCanvas().RefreshPopupData(NecoPopupCanvas.POPUP_REFRESH_TYPE.SHOP_CARD);
        }
    }

    public void FailPurchase(JArray responseArr)
    {
        NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(LocalizeData.GetText("LOCALIZE_364"), LocalizeData.GetText("LOCALIZE_365"));

        if (rootParentPanel)
            rootParentPanel.RefreshLayer();
    }

    public void RefreshData()
    {
        if (rootParentPanel == null) { return; }

        rootParentPanel.RefreshLayer();
    }

    ConfirmPopupData SetConfirmPopupData()
    {
        ConfirmPopupData popupData = new ConfirmPopupData();

        if (curShopData.GetNecoShopPriceType() == "point")
        {
            popupData.titleText = LocalizeData.GetText("LOCALIZE_379");
            popupData.titleMessageText = LocalizeData.GetText("LOCALIZE_380");

            popupData.contentsSprite = curShopData.GetNecoShopIcon();
            popupData.contentsNameText = curShopData.GetNecoShopName();

            if (curShopData.GetNecoShopGoodsType() == "package")
            {
                popupData.messageText_2 = curShopData.GetNecoShopDetail();
            }

            popupData.amountIcon = curShopData.GetNecoShopPriceIcon();
            popupData.amountText = curShopData.GetNecoShopPrice().ToString("n0");
        }
        else if (isCatLeafItem)
        {
            popupData.titleText = LocalizeData.GetText("LOCALIZE_366");
            popupData.titleMessageText = LocalizeData.GetText("LOCALIZE_367");

            popupData.contentsSprite = curShopData.GetNecoShopIcon();
            popupData.contentsNameText = curShopData.GetNecoShopName();

            if (curShopData.GetNecoShopGoodsType() == "package")
            {
                popupData.messageText_2 = curShopData.GetNecoShopDetail();
            }

            popupData.amountIcon = curShopData.GetNecoShopPriceIcon();
            popupData.amountText = curShopData.GetNecoShopPrice().ToString("n0");
        }
        else
        {
            popupData.titleText = LocalizeData.GetText("LOCALIZE_368");
            popupData.titleMessageText = LocalizeData.GetText("LOCALIZE_369");

            popupData.contentsSprite = curShopData.GetNecoShopIcon();
            popupData.contentsNameText = curShopData.GetNecoShopName();

            popupData.amountText = string.Format("\\ {0}", curShopData.GetNecoShopPrice().ToString("n0"));
        }

        return popupData;
    }

    void ClearData()
    {
        catLeafItemLayerObject.SetActive(false);
        catLeafItemDimmedObject.SetActive(false);

        catLeafBuyLayerObject.SetActive(false);
        catLeafBuyDimmedObject.SetActive(false);
    }
}
