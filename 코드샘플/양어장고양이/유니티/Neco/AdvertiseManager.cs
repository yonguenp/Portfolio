using UnityEngine;
using EasyMobile;
using UnityEngine.Purchasing;
using Newtonsoft.Json.Linq;

public class AdvertiseManager : MonoBehaviour
{
    public delegate void Callback();

    static private AdvertiseManager _sharedInstance;
    public static AdvertiseManager GetInstance()
    {
        return _sharedInstance;
    }

    Callback okCallback;
    Callback failCallback;

    void OnEnable()
    {
        _sharedInstance = this;
        Advertising.RewardedAdCompleted += RewardedAdCompletedHandler;
        Advertising.RewardedAdSkipped += RewardedAdSkippedHandler;
        Advertising.RewardedInterstitialAdCompleted += RewardedAdInterstitialCompletedHandler;
        Advertising.RewardedInterstitialAdSkipped += RewardedAdInterstitialSkippedHandler;
#if !UNITY_EDITOR
        if (Advertising.IsInitialized())
            IsAdvertiseReady();
#endif
    }

    // Unsubscribe events
    void OnDisable()
    {
        if (_sharedInstance == this)
            _sharedInstance = null;

        Advertising.RewardedAdCompleted -= RewardedAdCompletedHandler;
        Advertising.RewardedAdSkipped -= RewardedAdSkippedHandler;
    }

    public bool IsAdvertiseReady()
    {
        bool ret = false;
        if (!Advertising.IsRewardedInterstitialAdReady(RewardedInterstitialAdNetwork.AdMob, AdPlacement.Default))
            Advertising.LoadRewardedInterstitialAd(RewardedInterstitialAdNetwork.AdMob, AdPlacement.Default);
        else
            ret = true;

        if (!Advertising.IsRewardedAdReady(RewardedAdNetwork.TapJoy, AdPlacement.Default))
            Advertising.LoadRewardedAd(RewardedAdNetwork.TapJoy, AdPlacement.Default);
        else
            ret = true;

        if (!Advertising.IsRewardedAdReady(RewardedAdNetwork.UnityAds, AdPlacement.Default))
            Advertising.LoadRewardedAd(RewardedAdNetwork.UnityAds, AdPlacement.Default);
        else
            ret = true;

        return ret;
    }

    public void TryADWithCallback(Callback okCB, Callback failCB)
    {
        okCallback = null;
        failCallback = null;

        okCallback = okCB;
        failCallback = failCB;

        if (IsAdvertiseReady())
        {
            if (Advertising.IsRewardedInterstitialAdReady(RewardedInterstitialAdNetwork.AdMob, AdPlacement.Default))
            {
                Debug.Log("Try ShowRewardedInterstitialAd - admob");
                Advertising.ShowRewardedInterstitialAd(RewardedInterstitialAdNetwork.AdMob, AdPlacement.Default);
                return;
            }
            else if (Advertising.IsRewardedAdReady(RewardedAdNetwork.TapJoy, AdPlacement.Default))
            {
                Debug.Log("Try ShowRewardedAd - tapjoy");
                Advertising.ShowRewardedAd(RewardedAdNetwork.TapJoy, AdPlacement.Default);
                return;
            }
            else if (Advertising.IsRewardedAdReady(RewardedAdNetwork.UnityAds, AdPlacement.Default))
            {
                Debug.Log("Try ShowRewardedAd - unity");
                Advertising.ShowRewardedAd(RewardedAdNetwork.UnityAds, AdPlacement.Default);
                return;
            }
        }

        failCallback?.Invoke();
        okCallback = null;
        failCallback = null;
    }

    void RewardedAdCompletedHandler(RewardedAdNetwork network, AdPlacement location)
    {   
        okCallback?.Invoke();
        okCallback = null;
        failCallback = null;
    }

    // Event handler called when a rewarded ad has been skipped
    void RewardedAdSkippedHandler(RewardedAdNetwork network, AdPlacement location)
    {
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
}
