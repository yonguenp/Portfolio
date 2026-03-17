using OneStore.Alc;
using OneStore.Purchasing;
using System.Collections.Generic;
using SandboxNetwork;
using UnityEngine;

public class OneStoreManager
#if ONESTORE
    : ILicenseCheckCallback, IPurchaseCallback
#endif
{
    public delegate void CallbackPurchase(PurchaseData prod);
#if ONESTORE
    const string LICENSE_KEY = "MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQCg7HdHQaSqXV7R/0HF/Gdpl0Yna8ZdnbHFS4TGIKmcA7hUd7RKvO/XDlpLp5DAZgXGXQnr8TQgvbm+7HoN+rJ2+Sjtj7Sqk7ipcXGQ7Bcb9wP1DzuvXuiwm5+/7nk58gaxQ3xB8cB9XJqj8d7mV6SDNd+G6qoE2eF9tlQ3+ONWnQIDAQAB";
#else
    const string LICENSE_KEY = "";
#endif
    public static OneStoreManager Instance { get; private set; } = null;
    #region LICENSE
#if ONESTORE
    private OneStoreAppLicenseCheckerImpl LicenseClient { get; set; } = null;
#endif
    public static void InitializeLicense()
    {
#if ONESTORE
        if (Instance == null)
            Instance = new OneStoreManager();

        Instance.InitializeLicenseQuery();
#endif
    }
    private void InitializeLicenseQuery()
    {
#if ONESTORE
        if (LicenseClient == null)
        {
            LicenseClient = new OneStoreAppLicenseCheckerImpl(LICENSE_KEY);
            LicenseClient.Initialize(Instance);
            LicenseClient.QueryLicense();
        }
#endif
    }
    public void OnGranted(string license, string signature)
    {
#if DEBUG
        Debug.Log(SBFunc.StrBuilder("OneStore ALC OnGranted !!!!!! -> ", license, " !!!!! ", signature));
#endif
    }
#if ONESTORE
    public string GetStoreCode()
    {
        if(PurchaseClient != null)
        {
            return PurchaseClient.StoreCode;
        }

        return "";
    }
#endif
    public void OnDenied()
    {
#if DEBUG
        Debug.LogError("OneStore ALC OnDenied !!!!!!");
#endif
        SystemLoadingPopup.Instance.SetCallBack(AppExit);
        SystemLoadingPopup.Instance.SetExitCallback(AppExit);
        SystemLoadingPopup.Instance.InitPopup();
        SystemLoadingPopup.Instance.SetMessage(StringData.GetStringByStrKey("알림"), StringData.GetStringByIndex(100002656), StringData.GetStringByStrKey("확인"));
        SystemLoadingPopup.Instance.SetButtonState(true, false, false);
    }

    public void OnError(int code, string message)
    {
#if DEBUG
        Debug.LogError(SBFunc.StrBuilder("OneStore ALC OnError !!!!!! -> ", code, " | ", message));
#endif
        SystemLoadingPopup.Instance.SetCallBack(AppExit);
        SystemLoadingPopup.Instance.SetExitCallback(AppExit);
        SystemLoadingPopup.Instance.InitPopup();
        SystemLoadingPopup.Instance.SetMessage(StringData.GetStringByStrKey("알림"), StringData.GetStringByIndex(100002674), StringData.GetStringByStrKey("확인"));
        SystemLoadingPopup.Instance.SetButtonState(true, false, false);
    }
    private void AppExit()
    {
        SBFunc.Quit();
    }
    #endregion
    #region IAP
#if ONESTORE
    private PurchaseClientImpl PurchaseClient { get; set; } = null;
    private Dictionary<string, ProductDetail> Products { get; set; } = null;
#endif
    private CallbackPurchase SuccessCallBack { get; set; } = null;
    private VoidDelegate FailCallBack { get; set; } = null;
    public static void InitializeIAP()
    {
#if ONESTORE
        if (Instance == null)
            Instance = new OneStoreManager();

        if(Instance.PurchaseClient == null)
        {
            List<string> temp = new();
            var table = TableManager.GetTable<ShopSKUTable>();
            if (table != null)
            {
                var list = table.GetAllList();
                for (int i = 0, count = list.Count; i < count; ++i)
                {
                    var data = list[i];
                    if (data == null)
                        continue;

                    temp.Add(data.SKU);
#if DEBUG
                    Debug.Log(SBFunc.StrBuilder("InitializeIAP -> ", data.SKU));
#endif
                }
            }

            Instance.InitializeIAP(temp);
        }
#endif
    }
    private void InitializeIAP(List<string> items)
    {
#if ONESTORE
#if DEBUG
        Debug.Log(SBFunc.StrBuilder("InitializeIAP -> ", string.Join(",", items.ToArray())));
#endif
        if (PurchaseClient == null)
        {
            PurchaseClient = new PurchaseClientImpl(LICENSE_KEY);
            PurchaseClient.Initialize(this);
            PurchaseClient.QueryProductDetails(items.AsReadOnly(), ProductType.ALL);
        }
        SuccessCallBack = null;
        FailCallBack = null;
#endif
    }

    #region IPurchaseCallback
#if ONESTORE
    public void OnAcknowledgeFailed(IapResult iapResult)
    {
#if DEBUG
        Debug.LogError(SBFunc.StrBuilder("OneStore OnAcknowledgeFailed !!!!!! -> ", iapResult.Code, " | ", iapResult.Message));
#endif
    }

    public void OnAcknowledgeSucceeded(PurchaseData purchase, ProductType type)
    {
    }

    public void OnConsumeFailed(IapResult iapResult)
    {
#if DEBUG
        Debug.LogError(SBFunc.StrBuilder("OneStore OnConsumeFailed !!!!!! -> ", iapResult.Code, " | ", iapResult.Message));
#endif
    }

    public void OnConsumeSucceeded(PurchaseData purchase)
    {
    }

    public void OnManageRecurringProduct(IapResult iapResult, PurchaseData purchase, RecurringAction action)
    {
    }

    public void OnNeedLogin()
    {
    }

    public void OnNeedUpdate()
    {
    }

    /// <summary>
    /// 상품정보 로드하기 실패
    /// </summary>
    /// <param name="iapResult">뭔가 이상이 있으므로 처리필요</param>
    public void OnProductDetailsFailed(IapResult iapResult)
    {
#if DEBUG
        Debug.LogError(SBFunc.StrBuilder("OneStore OnProductDetailsFailed !!!!!! -> ", iapResult.Code, " | ", iapResult.Message));
#endif
    }

    /// <summary>
    /// 상품정보 로드하기 성공
    /// </summary>
    /// <param name="productDetails">상품들</param>
    public void OnProductDetailsSucceeded(List<ProductDetail> productDetails)
    {
        if (Products == null)
            Products = new();
        else
            Products.Clear();

        for (int i = 0; i < productDetails.Count; i++)
        {
            var productDetail = productDetails[i];
            if (productDetail == null)
                continue;

#if DEBUG
            Debug.Log(SBFunc.StrBuilder("OneStore OnProductDetailsSucceeded ProductID -> ", productDetail.productId));
#endif
            if (!Products.TryAdd(productDetail.productId, productDetail))
            {
#if DEBUG
                Debug.LogError(SBFunc.StrBuilder("OneStore OnProductDetailsSucceeded ProductID 중복 !!!!!! -> ", productDetail.productId));
#endif
            }
        }
    }

    public void OnPurchaseFailed(IapResult iapResult)
    {
#if DEBUG
        Debug.LogError(SBFunc.StrBuilder("OneStore OnPurchaseFailed !!!!!! -> ", iapResult.Code, " | ", iapResult.Message));
#endif
        FailCallBack?.Invoke();
        SuccessCallBack = null;
        FailCallBack = null;
    }
    /// <summary> 결제 성공시 영수증을 서버로 보내서 검증 및 보상을 받아야 함. </summary>
    /// <param name="purchases">성공 데이터</param>
    public void OnPurchaseSucceeded(List<PurchaseData> purchases)
    {
        if (purchases == null)
            return;

        var isSuccess = false;
        for(int i = 0, count = purchases.Count; i < count; ++i)
        {
            var purchase = purchases[i];
            if (purchase == null)
                continue;

            if (purchase.PurchaseState == (int)eIapState.PURCHASED)
            {
                SuccessCallBack?.Invoke(purchase);
                isSuccess = true;
            }
        }

        if (false == isSuccess)
            FailCallBack();

        SuccessCallBack = null;
        FailCallBack = null;
    }

    public void OnSetupFailed(IapResult iapResult)
    {
    }

    public ProductDetail GetProductDetail(string sku)
    {
        if (false == Products.TryGetValue(sku, out var productDetail))
            return null;

        return productDetail;
    }
#endif
    public void Purchase(string sku, CallbackPurchase success, SandboxNetwork.VoidDelegate fail = null)
    {
#if ONESTORE
#if DEBUG
        Debug.Log(SBFunc.StrBuilder("OneStore Purchase -> ", sku));
#endif
        var productDetail = GetProductDetail(sku);
        if (null == productDetail)
        {
#if DEBUG
            Debug.LogError(SBFunc.StrBuilder("OneStore PurchaseFalse ProductDetail Null -> ", sku));
#endif
            fail?.Invoke();
            return;
        }

        if (success != null)
            SuccessCallBack = success;
        else
            SuccessCallBack = null;

        if (fail != null)
            FailCallBack = fail;
        else
            FailCallBack = null;

        var productType = ProductType.Get(productDetail.type);
        var purchaseFlowParams = new PurchaseFlowParams.Builder();
        purchaseFlowParams.SetProductId(productDetail.productId);
        purchaseFlowParams.SetProductType(productType);
        PurchaseClient.Purchase(purchaseFlowParams.Build());
#endif
    }
    public void PendingPurchase(CallbackPurchase success, SandboxNetwork.VoidDelegate fail = null)
    {
#if ONESTORE
#if DEBUG
        Debug.Log(SBFunc.StrBuilder("OneStore PendingPurchase"));
#endif
        if (success != null)
            SuccessCallBack = success;
        else
            SuccessCallBack = null;

        if (fail != null)
            FailCallBack = fail;
        else
            FailCallBack = null;

        PurchaseClient.QueryPurchases(ProductType.INAPP);
#endif
    }
    #endregion
    #endregion
}

[System.Serializable]
public class OneStoreReceipt
{
    public OneStoreReceipt(string store, string transactionID, string payload)
    {
        Store = store;
        Payload = payload;
    }
#pragma warning disable 649
    public string Store;
    public string TransactionID;
    public string Payload;
#pragma warning restore 649
}