using UnityEngine;
using EasyMobile;
#if ONESTORE
using OneStore.Purchasing;
#endif
#if EM_UIAP
using UnityEngine.Purchasing;
#endif
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using SandboxNetwork;

//탭조이 쓸건지 정책정하기
//#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
//using TapjoyUnity; 
//#endif

public class IAPManager : SBPersistentSingleton<IAPManager>
{
    public delegate void CallbackWithData(JToken response);

    int curProductID = 0;
    CallbackWithData okCallback;
    CallbackWithData failCallback;

#if EM_UIAP
    void OnEnable()
    {
        InAppPurchasing.PurchaseCompleted += PurchaseCompletedHandler;
        InAppPurchasing.PurchaseFailed += PurchaseFailedHandler;
    }

    // Unsubscribe events
    void OnDisable()
    {
        InAppPurchasing.PurchaseCompleted -= PurchaseCompletedHandler;
        InAppPurchasing.PurchaseFailed -= PurchaseFailedHandler;
    }
#endif

    public void CheckPendingProducts()
    {
#if ONESTORE
#elif UNITY_IOS
        InAppPurchasing.RestorePurchases();
#endif
        okCallback = null;
        failCallback = null;
        curProductID = 0;

        OnPendingProducts();
    }

    public void CheckPlayPointProduct()
    {
#if EM_UIAP
        if (InAppPurchasing.StoreController != null)
        {
            foreach (Product product in InAppPurchasing.StoreController.products.all)
            {
                if (product.hasReceipt)
                {
                    if (product.definition != null)
                    {
                        if (product.definition.storeSpecificId.Split('.').Length > 1)
                        {
                            PurchaseToServer(product);
                        }
                    }
                }
            }
        }
#endif
    }

    void OnPendingProducts()
    {
#if ONESTORE
        OneStoreManager.Instance.PendingPurchase(PurchaseToServer, PurchaseFail);
#elif EM_UIAP
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

#if EM_UIAP
    void PurchaseToServer(PurchaseEventArgs args)
    {
        Product prod = args.purchasedProduct;
        PurchaseToServer(prod);
    }

    void PurchaseToServer(Product prod)
    {
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

        NetworkManager.Send("shop/iap", data, (res) =>
        {
            JToken resultCode = res["rs"];
            if (resultCode != null && resultCode.Type == JTokenType.Integer)
            {
                int rs = resultCode.Value<int>();
                if (rs == (int)eApiResCode.OK)
                {
                    if (okCallback == null)
                        Debug.Log("ok call back is null");
                    okCallback?.Invoke(res);

                    InAppPurchasing.ConfirmPendingPurchase(prod, true);

                    if (SBFunc.IsJTokenType(res["prod"], JTokenType.Integer))
                    {
                        var data = ShopSKUData.Get(res["prod"].Value<int>());
                        if (data != null)
                        {
                            AppsFlyerSDK.AppsFlyer.sendEvent(data.SKU, new Dictionary<string, string>());
                        }
                    }

                    SendPurchaseEvent(prod);
                    return;
                }
                else
                {
                    failCallback?.Invoke(res);
                    failCallback = null;

                    NetworkManager.SetIAPProcessing(false);
                }
            }
        },
        (fail)=> { NetworkManager.SetIAPProcessing(false); });
    }

    void PurchaseCompletedHandler(IAPProduct product)
    {
        Product prod = InAppPurchasing.StoreController.products.WithID(product.Id);
        
        NetworkManager.SetIAPProcessing(false);

        //ToastManager.On("결제 완료");
        //ShopItemGameData data = ShopItemGameData.GetShopData((int)curProductID);
        //if (data != null)
        //{
        //    //if (curProductID > 0)
        //    //{
        //    //    com.adjust.sdk.AdjustEvent ae = new com.adjust.sdk.AdjustEvent("c26vi7");
        //    //    ae.setRevenue(data.price.amount, "KRW");
        //    //    com.adjust.sdk.Adjust.trackEvent(ae);
        //    //}

        //    if (data.SubScriptionItems.ContainsKey(1))
        //    {
        //        //todo 구독형 상품
        //        //NetworkManager.RequestSubscribeReward(data.GetID());
        //    }

        //    if (curProductID != 999999999)
        //    {
        //        //배틀패스
        //        //if(data.rewards.Count > 0)
        //            //(PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.MAIL_POPUP) as MailPopup).SetNewFlag(true);
        //    }
        ////}
    }

    // Failed purchase handler
    void PurchaseFailedHandler(IAPProduct product, string failureReason)
    {
        failCallback?.Invoke(null);
        failCallback = null;
        ToastManager.On(StringData.GetStringByStrKey("구매불가"));
        NetworkManager.SetIAPProcessing(false);
    }
#endif

        public void TrySubscribe(int prodID, eShopIAPCheckType subscribeProcessType, CallbackWithData ok, CallbackWithData fail)
    {
#if !UNITY_EDITOR && UNITY_STANDALONE_WIN
        fail?.Invoke(null);
        ToastManager.On(StringData.GetStringByStrKey("윈도우결제미지원"));
        return;
#endif
        okCallback = ok;
        failCallback = fail;
        curProductID = prodID;

        WWWForm data = new WWWForm();
        data.AddField("api", "iap");
        data.AddField("op", (int)subscribeProcessType);
        data.AddField("prod", curProductID.ToString());

        NetworkManager.Send("shop/iap", data, (root) => {

            JToken resultCode = root["rs"];
            if (resultCode != null && resultCode.Type == JTokenType.Integer)
            {
                int rs = resultCode.Value<int>();
                if (rs == (int)eApiResCode.OK)
                {
                    var data = ShopSKUData.Get(prodID);
                    if (data == null)
                    {
                        failCallback?.Invoke(root);
                        failCallback = null;
                        return;
                    }

                    if (subscribeProcessType == eShopIAPCheckType.Buy)
                    {
                        NetworkManager.SetIAPProcessing(true);

                        InAppPurchasing.Purchase(data.SKU);

                    }
                    else if(subscribeProcessType == eShopIAPCheckType.SubscribeDailyReward)
                    {
                        ShopManager.Instance.UpdateSubscribeGoods((JObject)root["subs"]);
                        if (okCallback == null)
                            Debug.Log("ok call back is null");
                        okCallback?.Invoke(root);
                    }
                }
                else
                {
                    failCallback?.Invoke(root);
                    failCallback = null;
                }
            }
        },
        (fail) => { NetworkManager.SetIAPProcessing(false); });
    }

    public void TryPurchase(int prodID, CallbackWithData ok, CallbackWithData fail)
    {
#if !UNITY_EDITOR && UNITY_STANDALONE_WIN
        fail?.Invoke(null);
        ToastManager.On(StringData.GetStringByStrKey("윈도우결제미지원"));
        return;
#endif
#if DEBUG
        Debug.Log(SBFunc.StrBuilder("TryPurchase -> ", prodID));
#endif
        okCallback = ok;
        failCallback = fail;
        curProductID = prodID;

        WWWForm data = new WWWForm();
        data.AddField("api", "iap");
        data.AddField("op", 1);
        data.AddField("prod", curProductID.ToString());

        NetworkManager.Send("shop/iap", data, (root) => {

            JToken resultCode = root["rs"];
            if (resultCode != null && resultCode.Type == JTokenType.Integer)
            {
                int rs = resultCode.Value<int>();
                if (rs == (int)eApiResCode.OK)
                {
                    var data = ShopSKUData.Get(prodID);
                    if(data == null)
                    {
                        failCallback?.Invoke(root);
                        failCallback = null;
#if DEBUG
                        Debug.Log(SBFunc.StrBuilder("ShopSKUData -> null"));
#endif
                        return;
                    }


                    NetworkManager.SetIAPProcessing(true);
#if ONESTORE
#if DEBUG
                    Debug.Log(data.SKU);
#endif
                    OneStoreManager.Instance.Purchase(data.SKU, PurchaseToServer, PurchaseFail);
#elif EM_UIAP
                    InAppPurchasing.RegisterPrePurchaseProcessDelegate((args) => {
                        PurchaseToServer(args);
                        return InAppPurchasing.PrePurchaseProcessResult.Suspend;
                    });

                    InAppPurchasing.Purchase(data.SKU);
#endif
                }
                else
                {
                    failCallback?.Invoke(root);
                    failCallback = null;
                }
            }
        },
        (fail) => { NetworkManager.SetIAPProcessing(false); });
    }

    #region ONESTORE
#if ONESTORE
    private void PurchaseToServer(PurchaseData prod)
    {
        WWWForm data = new WWWForm();
        data.AddField("api", "iap");
        data.AddField("op", 2);
        data.AddField("mkt", 3);
        if (curProductID != 0)
        {
            data.AddField("prod", curProductID.ToString());
        }

        var receipt = JsonUtility.ToJson(new OneStoreReceipt("OneStore", prod.PurchaseId, prod.JsonReceipt));
#if DEBUG
        Debug.Log(prod.PurchaseId);
        Debug.Log(JsonUtility.ToJson(prod.JsonReceipt));
        Debug.Log(receipt);
#endif
        data.AddField("receipt", receipt);

        NetworkManager.Send("shop/iap", data, (res) =>
        {
            JToken resultCode = res["rs"];
            if (resultCode != null && resultCode.Type == JTokenType.Integer)
            {
                int rs = resultCode.Value<int>();
                if (rs == (int)eApiResCode.OK)
                {
                    if (okCallback == null)
                        Debug.Log("ok call back is null");
                    okCallback?.Invoke(res);

                    NetworkManager.SetIAPProcessing(false);
                    SendPurchaseEvent(prod);
                    return;
                }
                else
                {
                    failCallback?.Invoke(res);
                    failCallback = null;

                    NetworkManager.SetIAPProcessing(false);
                }
            }
        });
    }
    private void PurchaseFail()
    {
        failCallback?.Invoke(null);
        failCallback = null;
        ToastManager.On(StringData.GetStringByStrKey("구매불가"));
        NetworkManager.SetIAPProcessing(false);
    }
#endif
    #endregion
#if EM_UIAP
    public void SendPurchaseEvent(Product productInfo)
    {
        if (productInfo != null)
        {
            //com.adjust.sdk.AdjustEvent adjustEvent = new com.adjust.sdk.AdjustEvent("7yrokn");
            //adjustEvent.setRevenue((double)productInfo.metadata.localizedPrice, productInfo.metadata.isoCurrencyCode);
            //com.adjust.sdk.Adjust.trackEvent(adjustEvent);

            Dictionary<string, string> purchaseEvent = new Dictionary<string, string>();
            purchaseEvent.Add("os",
#if UNITY_ANDROID
    "google"
#elif UNITY_IOS
    "apple"
#else
    "unkown"
#endif
                );
            purchaseEvent.Add(AFInAppEvents.RECEIPT_ID, curProductID.ToString());
            purchaseEvent.Add(AFInAppEvents.CURRENCY, productInfo.metadata.isoCurrencyCode);
            purchaseEvent.Add(AFInAppEvents.REVENUE, productInfo.metadata.localizedPrice.ToString());
            purchaseEvent.Add(AFInAppEvents.QUANTITY, "1");
            purchaseEvent.Add(AFInAppEvents.CONTENT_TYPE, "category_a");

            AppsFlyerSDK.AppsFlyer.sendEvent(AFInAppEvents.PURCHASE, purchaseEvent);
        }
    }
#endif

#if ONESTORE
    public void SendPurchaseEvent(OneStore.Purchasing.PurchaseData prod)
    {
        OneStore.Purchasing.ProductDetail productInfo = OneStoreManager.Instance.GetProductDetail(prod.ProductId);
        if (productInfo != null)
        {
            //com.adjust.sdk.AdjustEvent adjustEvent = new com.adjust.sdk.AdjustEvent("7yrokn");
            //adjustEvent.setRevenue(double.Parse(productInfo.price), productInfo.priceCurrencyCode);
            //com.adjust.sdk.Adjust.trackEvent(adjustEvent);
        }
    }
#endif
        }