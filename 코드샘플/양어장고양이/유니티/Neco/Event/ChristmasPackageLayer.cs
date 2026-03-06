using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChristmasPackageLayer : MonoBehaviour
{
    public List<ShopPackageInfo> packages;
    neco_shop shopData;

    private void OnEnable()
    {
        Init();
    }

    void Init()
    {
        packages[0].SetPackageInfoData(neco_shop.GetNecoShopData((uint)60), null);
        packages[1].SetPackageInfoData(neco_shop.GetNecoShopData((uint)59), null);
        packages[2].SetPackageInfoData(neco_shop.GetNecoShopData((uint)58), null);
        packages[3].SetPackageInfoData(neco_shop.GetNecoShopData((uint)57), null);

        shopData = neco_shop.GetNecoShopData((uint)56);
    }

    public void OnClickTrypurchaseByCatnip()
    {
        users user = GameDataManager.Instance.GetUserData();
        if (user == null)
            return;
        object obj;
        uint catnip = 0;
        if (user.data.TryGetValue("catnip", out obj))
        {
            catnip = (uint)obj;
        }

        uint userCatLeaf = catnip;
        uint needCatLeaf = shopData.GetNecoShopPrice();
        NecoCanvas.GetPopupCanvas().OnShopPurchaseCountPopupShow(false, shopData, null, userCatLeaf / needCatLeaf, TryPurchase);
    }

    void TryPurchase(uint count)
    {
        string product = shopData.GetIAPConstants();
        if (string.IsNullOrEmpty(product))
        {
            WWWForm data = new WWWForm();
            data.AddField("api", "shop");
            data.AddField("op", 1);
            data.AddField("list", 0);
            data.AddField("prod", shopData.GetNecoShopID().ToString());
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
                                SuccessPurchase(null);
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

        //IAPManager.GetInstance().TryPurchase(shopData.GetNecoShopID(), product, SuccessPurchase, FailPurchase);
    }

    public void SuccessPurchase(JArray responseArr)
    {
        NecoCanvas.GetPopupCanvas().OnPopupClose(NecoPopupCanvas.POPUP_TYPE.SHOP_BUY_COUNT_POPUP);

        if (shopData.GetNecoShopPriceType() == "cash")
        {
            foreach (JObject row in responseArr)
            {
                string uri = row.GetValue("uri").ToString();
                if (uri == "iap")
                {
                    if (row.ContainsKey("rew"))
                    {
                        NecoCanvas.GetPopupCanvas().OnRewardPopupShow(LocalizeData.GetText("LOCALIZE_361"), LocalizeData.GetText("LOCALIZE_363"), "iap", responseArr);
                        return;
                    }
                }
            }

            NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(LocalizeData.GetText("LOCALIZE_361"), LocalizeData.GetText("LOCALIZE_362"));
        }
        else
            NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(LocalizeData.GetText("LOCALIZE_361"), LocalizeData.GetText("LOCALIZE_363"));

        Invoke("RefreshResource", 0.1f);
    }

    public void FailPurchase(JArray responseArr)
    {
        NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(LocalizeData.GetText("LOCALIZE_364"), LocalizeData.GetText("LOCALIZE_365"));
    }

    void RefreshResource()
    {
        NecoCanvas.GetPopupCanvas().RefreshTopUILayer(TOP_UI_PANEL_TYPE.RESOURCE);
    }
}
