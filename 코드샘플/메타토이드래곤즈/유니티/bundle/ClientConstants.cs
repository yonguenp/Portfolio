using UnityEngine;

namespace SandboxNetwork
{
    public class ClientConstants
    {
        // 클라이언트에서만 사용하는 여러 상수 / 전역 변수를 보관한다. region별로 묶어서 관리한다.

        #region ASSETBUNDLE
        public const string AssetBundleBuildPathFormat = "AssetBundles/{0}/";
        public const string AssetBundleBuildMetaFile = "asset_meta.txt";
        public const string AssetBundleListFileName = "assets.txt";
        public const string DevAssetBundleUse = "DEV_BUNDLE_USE";

#if UNITY_ANDROID
#if ONESTORE
		public const string CurrentPlatform = "OneStore";
#else
        public const string CurrentPlatform = "PlayStore";
#endif
#elif UNITY_IOS
        public const string CurrentPlatform = "iOS";
#else
#if UNITY_EDITOR
		public const string CurrentPlatform = "Dev";
#else
		public const string CurrentPlatform = "Windows";
#endif
#endif
#if UNITY_STANDALONE_WIN || UNITY_EDITOR || UNITY_EDITOR_WIN
        public static readonly string PLATFORM = "win/";
#elif UNITY_ANDROID
        public static readonly string PLATFORM = "android/";
#elif UNITY_IOS
        public static readonly string PLATFORM = "ios/";
#endif
#if UNITY_EDITOR
        public static string LocalBundlePath = Application.dataPath + "/assetinfo/" + PLATFORM;
#else
        public static string LocalBundlePath = Application.dataPath + "/assetinfo/" + PLATFORM;
#endif
#if UNITY_EDITOR
        public static string LocalBundleInfoPath = "assetinfo/" + PLATFORM;
#else
        public static string LocalBundleInfoPath = "assetinfo/" + PLATFORM;
#endif
#if UNITY_EDITOR
        public static string AssetBundleDownloadPath = "DownloadedAssets/";
#else
        public static string AssetBundleDownloadPath = Application.persistentDataPath + "/AssetBundles/";
#endif
        #endregion
    }
}
