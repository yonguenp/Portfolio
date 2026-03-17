using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopPointItemInfo : MonoBehaviour
{
    [Header("[Point Item Object]")]
    public GameObject dimmedLayer;
    public Image pointItemIcon;
    public Text pointItemTitle;
    public Text pointItemSubTitle;

    public Image priceIcon;
    public Text priceAmountText;

    [Header("[Layout List]")]
    public RectTransform pointItemlayoutRect;

    neco_shop curShopData;
    neco_package curPackageData;

    NecoShopPanel rootParentPanel;

    uint userPoint;
    uint needPoint;

    public void OnClickPointItem()
    {
        if (rootParentPanel.GetUserResource(NecoShopPanel.SHOP_RESOURCE_TYPE.POINT) < curShopData.GetNecoShopPrice())
        {
            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("LOCALIZE_378"));
            return;
        }

        ConfirmPopupData popupData = SetConfirmPopupData();

        NecoCanvas.GetPopupCanvas().OnSystemConfirmPopupShow(popupData, CONFIRM_POPUP_TYPE.COMMON, TryPurchase);
    }

    public void SetPointItemInfoData(neco_shop shopData, NecoShopPanel parentPanel)
    {
        if (shopData == null) { return; }

        curShopData = shopData;
        rootParentPanel = parentPanel;

        curPackageData = neco_package.GetNecoPackageByID(curShopData.GetNecoShopID());

        if (curPackageData == null) { return; }

        ClearData();

        // 포인트 품목 데이터 세팅
        userPoint = rootParentPanel.GetUserResource(NecoShopPanel.SHOP_RESOURCE_TYPE.POINT);
        needPoint = curShopData.GetNecoShopPrice();

        pointItemIcon.sprite = curShopData.GetNecoShopIcon();

        GameObject icon = Instantiate(Resources.Load<GameObject>("Prefabs/Neco/UI/notice_item_chance"), pointItemIcon.transform);
        icon.transform.localScale = Vector3.one;
        icon.GetComponent<RectTransform>().localPosition = new Vector3(64 - 12, 64 - 12, 0);
        icon.GetComponent<Button>().onClick.AddListener(() => {
            NecoCanvas.GetPopupCanvas().ShowRandomBoxInfo(curPackageData.GetNecoPackageItemID());
        });

        pointItemTitle.text = curShopData.GetNecoShopName();
        pointItemSubTitle.text = curPackageData.GetNecoPackageDesc();

        priceIcon.sprite = curShopData.GetNecoShopPriceIcon();
        
        priceAmountText.text = curShopData.GetNecoShopPrice().ToString("n0");
        priceAmountText.color = userPoint >= curShopData.GetNecoShopPrice() ? Color.white : new Color(0.8039216f, 0.2666667f, 0.2666667f, 1.0f);
        if (priceAmountText.GetComponent<Outline>() != null)
        {
            priceAmountText.GetComponent<Outline>().enabled = userPoint >= needPoint;
        }

        if (pointItemlayoutRect != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(pointItemlayoutRect);
        }
    }

    void TryPurchase()
    {
        WWWForm data = new WWWForm();
        data.AddField("api", "shop");
        data.AddField("op", 1);

        data.AddField("list", 0);
        data.AddField("prod", curShopData.GetNecoShopID().ToString());

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
                            NecoCanvas.GetPopupCanvas().OnPopupClose(NecoPopupCanvas.POPUP_TYPE.SHOP_BUY_COUNT_POPUP);

                            SuccessPurchase();
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

                            rootParentPanel.RefreshLayer();
                        }
                    }
                }
            }
        });
    }

    void SuccessPurchase()
    {
        NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(LocalizeData.GetText("LOCALIZE_361"), LocalizeData.GetText("LOCALIZE_363"));

        if (rootParentPanel == null) { return; }

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

        popupData.titleText = LocalizeData.GetText("LOCALIZE_379");
        popupData.titleMessageText = LocalizeData.GetText("LOCALIZE_380");

        popupData.messageText_1 = curShopData.GetNecoShopName();
        popupData.messageText_2 = curShopData.GetNecoShopDetail();

        return popupData;
    }

    void ClearData()
    {
        dimmedLayer.SetActive(false);
    }
}
