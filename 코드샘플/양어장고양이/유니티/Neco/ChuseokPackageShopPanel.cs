using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChuseokPackageShopPanel : MonoBehaviour
{
    [Header("[Common Info]")]
    public GameObject withdrawalLayerObject;
    public GameObject withdrawalScrollObject;
    public GameObject dimmedLayerObject;
    public Image purchaseButtonImage;

    public Color originButtonColor;
    public Color dimmedButtonColor;

    [Header("[Package Info]")]
    public Image packageImage;

    public Text packageGuideText;
    public Text packageCountText;
    public Text packagePriceText;

    neco_shop curShopData = null;

    ChuseokUI rootParentPanel;

    public void OnClickPurchaseButton()
    {
        // 패키지 구매 버튼 클릭 시 처리
        if (neco_data.Instance.GetPurchaseCount(curShopData.GetNecoShopID()) >= curShopData.GetNecoShopPurchaseLimit())
        {
            NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(LocalizeData.GetText("LOCALIZE_374"), LocalizeData.GetText("LOCALIZE_375"));
            return;
        }

        ConfirmPopupData popupData = SetConfirmPopupData();

        NecoCanvas.GetPopupCanvas().OnSystemConfirmPopupShow(popupData, CONFIRM_POPUP_TYPE.COMMON_WITHDRAWAL, TryPurchase);
    }

    public void OnClickWithdrawalInfoPopup()
    {
        if (withdrawalLayerObject == null || withdrawalScrollObject == null) { return; }

        withdrawalScrollObject.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;

        withdrawalLayerObject.SetActive(!withdrawalLayerObject.activeInHierarchy);

        LayoutRebuilder.ForceRebuildLayoutImmediate(withdrawalScrollObject.GetComponent<RectTransform>());
    }

    public void InitPakcageShopPanel(ChuseokUI rootPanel)
    {
        rootParentPanel = rootPanel;

        SetPackageInfoData();
    }

    void SetPackageInfoData()
    {
        ClearData();

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

                            curShopData = neco_shop.GetNecoShopData(44);
                            if (curShopData != null)
                            {
                                //packageImage.sprite = curShopData.GetNecoShopIcon();
                                packageImage.sprite = Resources.Load<Sprite>("Sprites/Neco/Ui/Image_chuseok_package"); //임시 처리

                                packageGuideText.text = LocalizeData.GetText("LOCALIZE_487");

                                uint currentPurchaseCount = neco_data.Instance.GetPurchaseCount(curShopData.GetNecoShopID());
                                uint limitPurchaseCount = curShopData.GetNecoShopPurchaseLimit();
                                packageCountText.text = string.Format("({0}/{1})", currentPurchaseCount, limitPurchaseCount);
                                
                                packagePriceText.text = string.Format("\\ {0}", curShopData.GetNecoShopPrice().ToString("n0"));

                                dimmedLayerObject.SetActive(currentPurchaseCount >= limitPurchaseCount);

                                purchaseButtonImage.color = currentPurchaseCount < limitPurchaseCount ? originButtonColor : dimmedButtonColor;
                            }
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

    void TryPurchase()
    {
        string product = curShopData.GetIAPConstants();
        if (string.IsNullOrEmpty(product))
        {
            FailPurchase(null);
            return;
        }

        IAPManager.GetInstance().TryPurchase(curShopData.GetNecoShopID(), product, SuccessPurchase, FailPurchase);
    }

    public void SuccessPurchase(JArray responseArr)
    {
        NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(LocalizeData.GetText("LOCALIZE_361"), LocalizeData.GetText("LOCALIZE_362"));

        Refresh();
    }

    public void FailPurchase(JArray responseArr)
    {
        NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(LocalizeData.GetText("LOCALIZE_364"), LocalizeData.GetText("LOCALIZE_365"));

        Refresh();
    }

    void Refresh()
    {
        // 구매 후 갱신처리 필요한 부분 처리
        SetPackageInfoData();
    }

    ConfirmPopupData SetConfirmPopupData()
    {
        ConfirmPopupData popupData = new ConfirmPopupData();

        popupData.titleText = LocalizeData.GetText("LOCALIZE_376");
        popupData.titleMessageText = LocalizeData.GetText("LOCALIZE_377");

        popupData.messageText_1 = curShopData.GetNecoShopName();
        popupData.messageText_2 = curShopData.GetNecoShopDetail();

        popupData.amountText = string.Format("\\ {0}", curShopData.GetNecoShopPrice().ToString("n0"));

        return popupData;
    }

    void ClearData()
    {
        dimmedLayerObject.SetActive(false);

        withdrawalLayerObject.SetActive(false);
    }
}
