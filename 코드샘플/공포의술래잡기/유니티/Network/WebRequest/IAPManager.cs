using UnityEngine;
using EasyMobile;
using UnityEngine.Purchasing;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

//탭조이 쓸건지 정책정하기
//#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
//using TapjoyUnity; 
//#endif

public class IAPManager : MonoBehaviour
{
    public delegate void CallbackWithData(JToken response);

    uint curProductID = 0;
    CallbackWithData okCallback;
    CallbackWithData failCallback;
    bool bInit = false;
    void OnEnable()
    {
        bInit = true;
        InAppPurchasing.PurchaseCompleted += PurchaseCompletedHandler;
        InAppPurchasing.PurchaseFailed += PurchaseFailedHandler;
    }

    // Unsubscribe events
    void OnDisable()
    {
        if (bInit)
        {
            InAppPurchasing.PurchaseCompleted -= PurchaseCompletedHandler;
            InAppPurchasing.PurchaseFailed -= PurchaseFailedHandler;
        }
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

        SBWeb.SendPost("shop/iap", data, (res) =>
        {
            JToken resultCode = res["rs"];
            if (resultCode != null && resultCode.Type == JTokenType.Integer)
            {
                int rs = resultCode.Value<int>();
                if (rs == 0)
                {
                    okCallback?.Invoke(res);
                    okCallback = null;

                    InAppPurchasing.ConfirmPendingPurchase(prod, true);

                    return;
                }
                else
                {
                    failCallback?.Invoke(res);
                    failCallback = null;

                    SBWeb.Instance.SetIAPProcessing(false);
                }
            }
        });
#endif
    }

    void PurchaseCompletedHandler(IAPProduct product)
    {
#if EM_UIAP
        Product prod = InAppPurchasing.StoreController.products.WithID(product.Id);
        
        SBWeb.Instance.SetIAPProcessing(false);

        ShopItemGameData data = ShopItemGameData.GetShopData((int)curProductID);
        if (data != null)
        {
            if (curProductID > 0)
            {
                com.adjust.sdk.AdjustEvent ae = new com.adjust.sdk.AdjustEvent("c26vi7");
                ae.setRevenue(data.price.amount, "KRW");
                com.adjust.sdk.Adjust.trackEvent(ae);
            }

            if (data.SubScriptionItems.ContainsKey(1))
            {
                SBWeb.RequestSubscribeReward(data.GetID());
            }

            if (curProductID != 999999999)
            {
                if(data.rewards.Count > 0)
                    (PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.MAIL_POPUP) as MailPopup).SetNewFlag(true);
            }
        }
#endif
    }

    // Failed purchase handler
    void PurchaseFailedHandler(IAPProduct product, string failureReason)
    {
        failCallback?.Invoke(null);
        failCallback = null;

        SBWeb.Instance.SetIAPProcessing(false);
    }

    public void TryPurchase(uint prodID, string iap, CallbackWithData ok, CallbackWithData fail)
    {
#if !UNITY_EDITOR && UNITY_STANDALONE_WIN && !SB_TEST
        fail?.Invoke(null);
        PopupCanvas.Instance.ShowFadeText("윈도우결제미지원");
        return;
#endif
        okCallback = ok;
        failCallback = fail;
        curProductID = prodID;

        WWWForm data = new WWWForm();
        data.AddField("api", "iap");
        data.AddField("op", 1);
        data.AddField("prod", curProductID.ToString());

        SBWeb.SendPost("shop/iap", data, (root) => {

            JToken resultCode = root["rs"];
            if (resultCode != null && resultCode.Type == JTokenType.Integer)
            {
                int rs = resultCode.Value<int>();
                if (rs == 0)
                {
                    SBWeb.Instance.SetIAPProcessing(true);

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
                    failCallback?.Invoke(root);
                    failCallback = null;
                }
            }
        });
    }

}
