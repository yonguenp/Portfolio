using UnityEngine;
using Newtonsoft.Json.Linq;
using SandboxNetwork;
using System.Collections.Generic;
using Unity.Services.LevelPlay;

public class AdvertiseManager : SBPersistentSingleton<AdvertiseManager>
{
    public delegate void CallbackWithLog(string log);
    public delegate void Callback();

    CallbackWithLog okCallback;
    Callback failCallback;

    LevelPlayRewardedAd RewardedAD = null;

    string Log = string.Empty;
    void OnEnable()
    {
        IsAdvertiseReady();
    }

    // Unsubscribe events
    void OnDisable()
    {

    }


    public bool IsAdvertiseReady()
    {
        if (User.Instance.ADVERTISEMENT_PASS)
            return true;

        bool ret = false;
        
        if (RewardedAD == null)
        {
#if UNITY_IOS
            RewardedAD = new LevelPlayRewardedAd("msmjw38x1rg836cv");
#else
            RewardedAD = new LevelPlayRewardedAd("oxk3medazf81qzgw");
#endif
            RewardedAD.OnAdDisplayFailed += RewardedAdSkippedHandler;
            RewardedAD.OnAdRewarded += RewardedAdCompletedHandler;
            RewardedAD.OnAdLoaded += ImpressionDataReadyEvent;            
        }

        if (!RewardedAD.IsAdReady())
            RewardedAD.LoadAd();
        else
            ret = true;

        return ret;
    }

    public void TryADWithPopup(CallbackWithLog okCB, Callback failCB, AdvRemoveBuyGuidePopup.Callback popupCloseCB = null)
    {
        var param = new Dictionary<string, string>();
        param.Add("param", User.Instance.ADVERTISEMENT_PASS ? "2" : "1");
        AppsFlyerSDK.AppsFlyer.sendEvent("try_ad", param);

        if (User.Instance.ADVERTISEMENT_PASS)
        {
            okCB?.Invoke("");
            return;
        }

        PopupManager.OpenPopup<AdvRemoveBuyGuidePopup>().SetCallBack(()=> {
            TryADWithCallback(okCB, failCB);
        }, popupCloseCB);
    }


    public void TryADWithCallback(CallbackWithLog okCB, Callback failCB)
    {
        okCallback = null;
        failCallback = null;

        okCallback = okCB;
        failCallback = failCB;

        AdvProcess();
    }


    void AdvProcess()
    {
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

            ToastManager.On(StringData.GetStringByStrKey("광고미지원플랫폼"));
            return;
        }
#endif

        if (IsAdvertiseReady()
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
            && false
#endif
            )
        {
            SoundManager.Instance.StopBGM();
            Debug.Log("Try ShowRewardedInterstitialAd - IronSource");
            RewardedAD.ShowAd();
            Log = "s:" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            return;
        }

#if UNITY_EDITOR || UNITY_STANDALONE_WIN
        //window에서는 광고 봣다고 치고
        ToastManager.On("광고 봣다고 치겠음");

        okCallback?.Invoke("");
        okCallback = null;
        failCallback = null;
        return;
#endif

        failCallback?.Invoke();
        okCallback = null;
        failCallback = null;
    }

    void RewardedAdCompletedHandler(LevelPlayAdInfo info, LevelPlayReward reward)
    {
        SoundManager.Instance.PlayBGM();
        Log += ",e:" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        okCallback?.Invoke(Log);
        okCallback = null;
        failCallback = null;


        var param = new Dictionary<string, string>();

        AppsFlyerSDK.AppsFlyer.sendEvent("reward_ad", param);

        if (RewardedAD != null)
            RewardedAD.LoadAd();
    }

    // Event handler called when a rewarded ad has been skipped
    void RewardedAdSkippedHandler(LevelPlayAdInfo info, LevelPlayAdError err)
    {
        Debug.LogError(err.ErrorMessage);

        SoundManager.Instance.PlayBGM();
        failCallback?.Invoke();
        okCallback = null;
        failCallback = null;
    }


    private void ImpressionDataReadyEvent(LevelPlayAdInfo impressionData)
    {
        if (impressionData != null)
        {
            //for adjust
            //com.adjust.sdk.AdjustAdRevenue adjustAdRevenue = new com.adjust.sdk.AdjustAdRevenue(com.adjust.sdk.AdjustConfig.AdjustAdRevenueSourceIronSource);

            //if (impressionData.revenue == null || impressionData.revenue < 0)
            //    return;

            //adjustAdRevenue.setRevenue((double)impressionData.revenue, "USD");
            //// optional fields
            //adjustAdRevenue.setAdRevenueNetwork(impressionData.adNetwork);
            //adjustAdRevenue.setAdRevenueUnit(impressionData.adUnit);
            //adjustAdRevenue.setAdRevenuePlacement(impressionData.instanceName);

            //// track Adjust ad revenue
            //com.adjust.sdk.Adjust.trackAdRevenue(adjustAdRevenue);

            Firebase.Analytics.Parameter[] AdParameters = {
                new Firebase.Analytics.Parameter("ad_platform", "ironSource"),
                new Firebase.Analytics.Parameter("ad_source", impressionData.AdNetwork),
                new Firebase.Analytics.Parameter("ad_unit_name", impressionData.AdUnitId),
                new Firebase.Analytics.Parameter("ad_format", impressionData.AdUnitName),
                new Firebase.Analytics.Parameter("currency","USD"),
                new Firebase.Analytics.Parameter("value", impressionData.Revenue.Value)
            };

            if (LoginManager.Instance != null)
                LoginManager.Instance.SetFirebaseEvent("custom_ad_impression", AdParameters);
        }
    }

    //public void ShowAdvertisePopup(string titleText, Callback successAdvCB, Callback failAdvCB = null, Callback CloseCB = null)
    //{
    //    if(IsAdvertiseReady())
    //    {
    //        Debug.Log("광고 로드 완료 상태");
    //    }
    //    else
    //    {
    //        Debug.Log("광고가 로드되지 않아 요청");
    //    }

    //    var popup = PopupManager.OpenPopup<SystemPopup>();
    //    popup.SetMessage(titleText, StringData.GetStringFormatByStrKey("광고 확인 팝업 문구"));
    //    popup.SetCallBack(() =>
    //    {
    //        popup.SetActive(false);
    //        TryADWithPopup(
    //            ("") => {
    //                successAdvCB?.Invoke();
    //                IsAdvertiseReady();
    //            }
    //            , ()=>
    //            {
    //                failAdvCB?.Invoke();
    //            });
    //    }, () =>
    //    {
    //        popup.ClosePopup();
    //        CloseCB?.Invoke();
            
    //    }, () =>
    //    {
    //        popup.ClosePopup();
    //        CloseCB?.Invoke();

    //    });

    //}
}

