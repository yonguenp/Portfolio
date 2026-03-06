using UnityEngine;
using EasyMobile;
using UnityEngine.Purchasing;
using Newtonsoft.Json.Linq;
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
using TapjoyUnity;
#endif

public class IAPManager : MonoBehaviour
{
    public delegate void CallbackWithData(JArray rowArray);

    static private IAPManager _sharedInstance;
    public static IAPManager GetInstance()
    {
        return _sharedInstance;
    }

    uint curProductID = 0;
    CallbackWithData okCallback;
    CallbackWithData failCallback;

    void OnEnable()
    {
        _sharedInstance = this;
        InAppPurchasing.PurchaseCompleted += PurchaseCompletedHandler;
        InAppPurchasing.PurchaseFailed += PurchaseFailedHandler;

    }

    // Unsubscribe events
    void OnDisable()
    {
        if(_sharedInstance == this)
            _sharedInstance = null;

        InAppPurchasing.PurchaseCompleted -= PurchaseCompletedHandler;
        InAppPurchasing.PurchaseFailed -= PurchaseFailedHandler;
    }

    public void CheckPendingProducts()
    {
#if UNITY_IOS
        InAppPurchasing.RestorePurchases();
#endif
        okCallback = null;
        failCallback = null;
        curProductID = 0;
#if EM_UIAP
        if (InAppPurchasing.StoreController != null)
        {
            foreach (Product product in InAppPurchasing.StoreController.products.all)
            {
                if (product.hasReceipt)
                {
                    PurchaseToServer(product);
                }
            }
        }
#endif
    }

    void PurchaseToServer(PurchaseEventArgs args)
    {
        Product prod = args.purchasedProduct;
        PurchaseToServer(prod);
    }

    void PurchaseToServer(Product prod)
    {
#if EM_UIAP
        string receipt = prod.receipt;

        WWWForm data = new WWWForm();
        data.AddField("api", "iap");
        data.AddField("op", 2);

#if UNITY_ANDROID
        data.AddField("mkt", 1);
#endif
#if UNITY_IOS
        data.AddField("mkt", 2);
#endif
        if (curProductID != 0)
        {
            data.AddField("prod", curProductID.ToString());
        }

        data.AddField("receipt", receipt);

        NetworkManager.GetInstance().SendApiRequest("iap", 2, data, (res) =>
        {
            JObject root = JObject.Parse(res);
            JToken apiToken = root["api"];
            if (null == apiToken || apiToken.Type != JTokenType.Array
                || !apiToken.HasValues)
            {
                failCallback?.Invoke(null);
                NecoCanvas.GetPopupCanvas().OnPopupClose(NecoPopupCanvas.POPUP_TYPE.IAP_WAITER);
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
                            okCallback?.Invoke(apiArr);
                            if (row.ContainsKey("first"))
                            {
                                neco_data.Instance.SetBenefit(row["first"].Value<uint>() > 0);
                            }

                            InAppPurchasing.ConfirmPendingPurchase(prod, true);

                            return;
                        }
                        else
                        {
                            failCallback?.Invoke(apiArr);
                            NecoCanvas.GetPopupCanvas().OnPopupClose(NecoPopupCanvas.POPUP_TYPE.IAP_WAITER);
                        }
                    }
                }
            }
        });
#endif
    }

    void PurchaseCompletedHandler(IAPProduct product)
    {
#if EM_UIAP
        Product prod = InAppPurchasing.StoreController.products.WithID(product.Id);
#if UNITY_ANDROID && !UNITY_EDITOR
        string receipt = prod.receipt;

        JObject receiptJson = JObject.Parse(receipt);
        if (receiptJson != null && receiptJson.ContainsKey("Payload"))
        {
            JObject payload = JObject.Parse(receiptJson["Payload"].Value<string>());
            if (payload != null && payload.ContainsKey("skuDetails"))
            {
                string skuDetails = payload["skuDetails"].Value<string>();
                Tapjoy.TrackPurchaseInGooglePlayStore(skuDetails, null, null, null);
            }
        }            
#endif

#if UNITY_IOS && !UNITY_EDITOR
        Tapjoy.TrackPurchaseInAppleAppStore(product.Id, prod.metadata.isoCurrencyCode, decimal.ToDouble(prod.metadata.localizedPrice), null);
#endif
        NecoCanvas.GetPopupCanvas().OnPopupClose(NecoPopupCanvas.POPUP_TYPE.IAP_WAITER);
#endif
    }

    // Failed purchase handler
    void PurchaseFailedHandler(IAPProduct product, string failureReason)
    {
        failCallback?.Invoke(null);
        NecoCanvas.GetPopupCanvas().OnPopupClose(NecoPopupCanvas.POPUP_TYPE.IAP_WAITER);
    }

    public void TryPurchase(uint prodID, string iap, CallbackWithData ok, CallbackWithData fail)
    {
        okCallback = ok;
        failCallback = fail;
        curProductID = prodID;

        WWWForm data = new WWWForm();
        data.AddField("api", "iap");
        data.AddField("op", 1);
        data.AddField("prod", curProductID.ToString());

        NetworkManager.GetInstance().SendApiRequest("iap", 1, data, (res) => {            
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
                            NecoCanvas.GetPopupCanvas().OnCheckLimitProduct();
                            NecoCanvas.GetPopupCanvas().OnPopupShow(NecoPopupCanvas.POPUP_TYPE.IAP_WAITER);

#if EM_UIAP
                            InAppPurchasing.RegisterPrePurchaseProcessDelegate((args) => {                                
                                PurchaseToServer(args);
                                return InAppPurchasing.PrePurchaseProcessResult.Suspend;
                            });

                            InAppPurchasing.Purchase(iap);          
#endif
                        }
                        else
                        {
                            failCallback?.Invoke(apiArr);
                        }
                    }
                }
            }
        });
    }

}
