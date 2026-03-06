using UnityEngine;

public class ClientConstants
{
    // 클라이언트에서만 사용하는 여러 상수 / 전역 변수를 보관한다. region별로 묶어서 관리한다.

#region ASSETBUNDLE
    public const string AssetBundleBuildPathFormat = "AssetBundles/{0}/";
    public const string AssetBundleBuildMetaFile = "asset_meta.txt";
    public const string AssetBundleListFileName = "assets.txt";
    
#if UNITY_ANDROID
    public const string CurrentPlatform = "Android";
#elif UNITY_IOS 
    public const string CurrentPlatform = "iOS";
#else
    public const string CurrentPlatform = "StandaloneWindows";
#endif
    // static 선언, 글로벌하게 쓸 변수지만 상수가 아닌 것들
    public static string AssetBundleDownloadURLFormat = SBWeb.CDN_URL;
#if UNITY_EDITOR
    public static string AssetBundleDownloadPath = "DownloadedAssets/";
#else
    public static string AssetBundleDownloadPath = Application.persistentDataPath + "/DownloadedAssets/";
#endif
    #endregion
}
