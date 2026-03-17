using UnityEngine;
using EasyMobile;
using UnityEngine.Purchasing;
using Newtonsoft.Json.Linq;
using com.adjust.sdk;

public class AdvertiseManager : MonoBehaviour
{
    public delegate void Callback();

    Callback okCallback;
    Callback failCallback;

    void OnEnable()
    {
        Advertising.RewardedAdCompleted += RewardedAdCompletedHandler;
        Advertising.RewardedAdSkipped += RewardedAdSkippedHandler;

#if !UNITY_EDITOR && !UNITY_STANDALONE_WIN 
        if (Advertising.IsInitialized())
            IsAdvertiseReady();
#endif
        IronSourceEvents.onImpressionDataReadyEvent += ImpressionDataReadyEvent;
    }

    // Unsubscribe events
    void OnDisable()
    {
        Advertising.RewardedAdCompleted -= RewardedAdCompletedHandler;
        Advertising.RewardedAdSkipped -= RewardedAdSkippedHandler;

        IronSourceEvents.onImpressionDataReadyEvent -= ImpressionDataReadyEvent;
    }

    public bool IsAdvertiseReady()
    {
        bool ret = false;
        if (!Advertising.IsRewardedAdReady(RewardedAdNetwork.IronSource, AdPlacement.Default))
            Advertising.LoadRewardedAd(RewardedAdNetwork.IronSource, AdPlacement.Default);
        else
            ret = true;

#if UNITY_EDITOR || UNITY_STANDALONE_WIN
        if (!Advertising.IsRewardedAdReady(RewardedAdNetwork.AdMob, AdPlacement.Default))
            Advertising.LoadRewardedAd(RewardedAdNetwork.AdMob, AdPlacement.Default);
        else
            ret = true;
#endif

        //if (!Advertising.IsRewardedAdReady(RewardedAdNetwork.UnityAds, AdPlacement.Default))
        //    Advertising.LoadRewardedAd(RewardedAdNetwork.UnityAds, AdPlacement.Default);
        //else
        //    ret = true;

        return ret;
    }

    public void TryADWithCallback(Callback okCB, Callback failCB)
    {
        okCallback = null;
        failCallback = null;

        okCallback = okCB;
        failCallback = failCB;

#if !UNITY_EDITOR && UNITY_ANDROID
        var unityPlayerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        var currentActivity = unityPlayerClass.GetStatic<AndroidJavaObject>("currentActivity");
        var packageManager = currentActivity.Call<AndroidJavaObject>("getPackageManager");
        var isPC = packageManager.Call<bool>("hasSystemFeature", "com.google.android.play.feature.HPE_EXPERIENCE");
        if (isPC)
        {
            failCallback?.Invoke();
            okCallback = null;
            failCallback = null;

            PopupCanvas.Instance.ShowFadeText("광고미지원플랫폼");
            return;
        }
#endif

        if (IsAdvertiseReady())
        {
            if (Advertising.IsRewardedAdReady(RewardedAdNetwork.IronSource, AdPlacement.Default))
            {
                Managers.Sound.BGMStop();
                Debug.Log("Try ShowRewardedInterstitialAd - IronSource");
                Advertising.ShowRewardedAd(RewardedAdNetwork.IronSource, AdPlacement.Default);

                return;
            }
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
            else if (Advertising.IsRewardedAdReady(RewardedAdNetwork.AdMob, AdPlacement.Default))
            {
                Managers.Sound.BGMStop();
                Debug.Log("Try ShowRewardedAd - AdMob");
                Advertising.ShowRewardedAd(RewardedAdNetwork.AdMob, AdPlacement.Default);
                return;
            }
#endif
            //else if (Advertising.IsRewardedAdReady(RewardedAdNetwork.UnityAds, AdPlacement.Default))
            //{
            //    Debug.Log("Try ShowRewardedAd - unity");
            //    Advertising.ShowRewardedAd(RewardedAdNetwork.UnityAds, AdPlacement.Default);
            //    return;
            //}
        }

        failCallback?.Invoke();
        okCallback = null;
        failCallback = null;
    }

    void RewardedAdCompletedHandler(RewardedAdNetwork network, AdPlacement location)
    {
        Managers.Sound.BGMPlay();
        okCallback?.Invoke();
        okCallback = null;
        failCallback = null;
    }

    // Event handler called when a rewarded ad has been skipped
    void RewardedAdSkippedHandler(RewardedAdNetwork network, AdPlacement location)
    {
        Managers.Sound.BGMPlay();
        failCallback?.Invoke();
        okCallback = null;
        failCallback = null;
    }

    void RewardedAdInterstitialCompletedHandler(RewardedInterstitialAdNetwork network, AdPlacement location)
    {
        okCallback?.Invoke();
        okCallback = null;
        failCallback = null;
    }

    // Event handler called when a rewarded ad has been skipped
    void RewardedAdInterstitialSkippedHandler(RewardedInterstitialAdNetwork network, AdPlacement location)
    {
        failCallback?.Invoke();
        okCallback = null;
        failCallback = null;
    }

    private void ImpressionDataReadyEvent(IronSourceImpressionData impressionData)
    {
        AdjustAdRevenue adjustAdRevenue = new AdjustAdRevenue(AdjustConfig.AdjustAdRevenueSourceIronSource);
        adjustAdRevenue.setRevenue((double)impressionData.revenue, "USD");
        // optional fields
        adjustAdRevenue.setAdRevenueNetwork(impressionData.adNetwork);
        adjustAdRevenue.setAdRevenueUnit(impressionData.adUnit);
        adjustAdRevenue.setAdRevenuePlacement(impressionData.placement);
        // track Adjust ad revenue
        Adjust.trackAdRevenue(adjustAdRevenue);
    }
}
