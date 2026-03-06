using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IAPObjectHelpPopup : MonoBehaviour
{
    public Text DescText;

    public ShopPackageInfo MovableCatHousePackage;
    public ShopPackageInfo CatnipFarmPackage;

    int curType = -1;
    public void OnClose()
    {
        NecoCanvas.GetPopupCanvas().OnPopupClose(NecoPopupCanvas.POPUP_TYPE.IAP_OBJECT_HELP_POPUP);
    }

    private void OnEnable()
    {

    }

    public void RefreshLayer()
    {
        Invoke("RefreshUI", 0.1f);
    }

    public void RefreshUI()
    {
        SetUIType(curType);
    }

    public void SetUIType(int type)
    {
        curType = type;
        switch (type)
        {
            case 0:
                MovableCatHousePackage.gameObject.SetActive(true);
                MovableCatHousePackage.SetPackageInfoData(neco_shop.GetNecoShopData(1), null, this);
                CatnipFarmPackage.gameObject.SetActive(false);
#if UNITY_IOS
                DescText.text = LocalizeData.GetText("이동식캣하설명_FORIOS");
#else
                DescText.text = LocalizeData.GetText("이동식캣하설명");
#endif
                break;
            case 1:
                MovableCatHousePackage.gameObject.SetActive(false);                
                CatnipFarmPackage.gameObject.SetActive(true);
                CatnipFarmPackage.SetPackageInfoData(neco_shop.GetNecoShopData(52), null, this);
#if UNITY_IOS
                DescText.text = LocalizeData.GetText("캣닙급식소설명_FORIOS");
#else
                DescText.text = LocalizeData.GetText("캣닙급식소설명");
#endif
                break;
        }

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
                            MovableCatHousePackage.SetPackageInfoData(neco_shop.GetNecoShopData(1), null, this);
                            CatnipFarmPackage.SetPackageInfoData(neco_shop.GetNecoShopData(52), null, this);
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

    public void OnOpenShop()
    {
        OnClose();
        NecoCanvas.GetPopupCanvas().OnShopListPopupShow(NecoShopPanel.SHOP_CATEGORY.PACKAGE);
    }
}
