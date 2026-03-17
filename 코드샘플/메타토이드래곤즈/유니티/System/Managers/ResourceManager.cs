using Spine.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;
using System.IO;
using System.Linq;

namespace SandboxNetwork
{
    public class ResourceManager : IManagerBase
    {
		private static readonly string KEY_POPUP = "popup";

		private static Dictionary<string, AssetBundle> cachedAsset = new Dictionary<string, AssetBundle>();
        public static Dictionary<string, string> AssetGuidDic = new Dictionary<string, string>();
        public static ResourceManager instance = null;
        public static ResourceManager Instance
        {
            get
            {
                if(instance == null)
                {
                    instance = new ResourceManager();
                }
                return instance;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		static void InitPlayMode()
		{
			instance = null;
		}

		public void Initialize() { }

		#region Load One Router
		public static T GetResource<T>(eResourcePath eType, string name) where T : Object
		{
#if UNITY_EDITOR
			if (AssetBundleManager.UseBundleAssetEditor)
#endif
			{
				if (AssetBundleManager.IsBundle(eType.GetBundleName()))
					return LoadAsset<T>(eType, name);
			}

			return LoadResources<T>(eType, name);
		}

		#endregion
		#region Load One
		public static T LoadAsset<T>(eResourcePath eType, string name) where T : Object
		{
			string bundleName = eType.GetBundleName();
			string key = eType.GetBundleKey();
			var assetPath = Path.Combine(ClientConstants.AssetBundleDownloadPath, bundleName);
			if (File.Exists(assetPath))
			{
				if (!cachedAsset.ContainsKey(key) || cachedAsset[key] == null)
				{
					AssetBundle loadedBundle = AssetBundle.LoadFromFile(assetPath);
					cachedAsset[key] = loadedBundle;
				}

				AssetBundle asset = cachedAsset[key];
				var go = asset.LoadAsset<T>(SBFunc.StrBuilder(name, SBDefine.GetTypeExtensionStr(typeof(T))));
				if (go == null)
				{
					go = LoadResources<T>(eType, name);
				}
				return go;
			}
			else
			{
				return LoadResources<T>(eType, name);
			}
		}
		private static T LoadResources<T>(eResourcePath eType, string name) where T : Object
		{
			var path = SBDefine.ResourcePath(eType, name);
			T go = Resources.Load<T>(SBFunc.StrBuilder("AssetBundle/", path));
			if (go == null)
			{
				go = Resources.Load<T>(path);
			}
			return go;
		}
		#endregion
		#region Load All Router

		public static T[] GetResources<T>(string path) where T : Object
		{
			return LoadAllAssetsBundle<T>(path);
		}

		public static T[] LoadAllAssetsBundle<T>(string path) where T : Object
		{
			if (AssetBundleManager.IsBundle(path.Split("/")))
			{
				if (typeof(T) == typeof(GameObject))
					return LoadAllAsset(path) as T[];
				if (typeof(T) == typeof(SkeletonDataAsset))
					return LoadAllSkeletonData(path) as T[];
				if (typeof(T) == typeof(AudioClip))
					return LoadAllAudioClip(path) as T[];
				if (typeof(T) == typeof(Sprite))
					return LoadAllUISprite(path) as T[];

				return Resources.LoadAll<T>(path);
			}

			T[] go = Resources.LoadAll<T>("AssetBundle/" + path);

			if (go == null)
			{
				go = Resources.LoadAll<T>(path);
			}

			return go;
		}

		#endregion
		#region Load All 

		public static Sprite[] LoadAllUISprite(string path)
        {
            path = path.ToLower();
            var key = path.Split('/')[1];

            var assetPath = Path.Combine(ClientConstants.AssetBundleDownloadPath, SBDefine.GetBundleExtension(key));
            if (File.Exists(assetPath))
            {
                if (!cachedAsset.ContainsKey(key) || cachedAsset[key] == null)
                {
                    var loadedBundle = AssetBundle.LoadFromFile(assetPath);
                    cachedAsset[key] = loadedBundle;
                }
                var asset = cachedAsset[key];
                var go = asset.LoadAllAssets<Sprite>();
                Debug.Log($"Load from Assetbundle : {path}");
                if (go == null)
                {
                    Debug.LogWarning($"Load from LocalAssets : {path}");
                    go = Resources.LoadAll<Sprite>(path);
                }
                return go;
            }
            else
            {
                Debug.Log(string.Format("번들에셋에 데이터가 없습니다. + {0}", path));
                return Resources.LoadAll<Sprite>(path);
            }
        }

        public static AudioClip[] LoadAllAudioClip(string path)
        {
			path = path.ToLower();
			string[] pathSplit = path.Split('/');
			string bundleName = pathSplit[0];
			string assetPath = Path.Combine(ClientConstants.AssetBundleDownloadPath, SBDefine.GetBundleExtension(bundleName));

            if (File.Exists(assetPath))
            {
                if (!cachedAsset.ContainsKey(bundleName) || cachedAsset[bundleName] ==null)
                {
                    var loadedBundle = AssetBundle.LoadFromFile(assetPath);
                    cachedAsset[bundleName] = loadedBundle;
                }
                var asset = cachedAsset[bundleName];
                var go = asset.LoadAllAssets<AudioClip>();
                
                if (go == null)
                {
                    go = Resources.LoadAll<AudioClip>(path);
                }
                return go;
            }
            else
            {
                return Resources.LoadAll<AudioClip>("AssetBundle/" + path);
			}
        }


        public static SkeletonDataAsset[] LoadAllSkeletonData(string path)
        {
            path = path.ToLower();
            var key = path.Split('/')[1];
            var assetPath = Path.Combine(ClientConstants.AssetBundleDownloadPath, SBDefine.GetBundleExtension(key));
            
			if (File.Exists(assetPath))
            {
                if (!cachedAsset.ContainsKey(key) || cachedAsset[key] == null)
                {
                    var loadedBundle = AssetBundle.LoadFromFile(assetPath);
                    cachedAsset[key] = loadedBundle;
                }
                var asset = cachedAsset[key];
                var go = asset.LoadAllAssets<SkeletonDataAsset>();

                if (go == null)
                {
                    go = Resources.LoadAll<SkeletonDataAsset>(path);
                }
                return go;
            }
            else
            {
                return Resources.LoadAll<SkeletonDataAsset>(path);
            }
        }

        public static GameObject[] LoadAllAsset(string path)
        {
            path = path.ToLower();                      // ex) Prefab/Tutorial
            var key = path.Split('/')[1];               //Tutorial
            var assetPath = Path.Combine(ClientConstants.AssetBundleDownloadPath, SBDefine.GetBundleExtension(key));

            if (File.Exists(assetPath))
            {
                if (!cachedAsset.ContainsKey(key) || cachedAsset[key] == null)
                {
                    AssetBundle loadedBundle = AssetBundle.LoadFromFile(assetPath);
                    cachedAsset[key] = loadedBundle;
                }

                AssetBundle asset = cachedAsset[key];
                var go = asset.LoadAllAssets<GameObject>();

                if (go == null)
                {
                    go = Resources.LoadAll<GameObject>(path);
                }
                return go;
            }
            else
            {
                return Resources.LoadAll<GameObject>(path);
            }
        }

		#endregion
		#region Load Scene
		public static string LoadSceneFromAssetBundle(string path)
        {
			path = path.ToLower();                      // ex) Prefab/Tutorial/101.prefab	Prefab/Dragon/legendary
			string[] pathSplit = path.Split('/');
			string bundleName = pathSplit[0];
			var key = pathSplit[1];               //Tutorial
			var targetResource = pathSplit[pathSplit.Length - 1];   //Prefab 이름
			var assetPath = Path.Combine(ClientConstants.AssetBundleDownloadPath, SBDefine.GetBundleExtension(bundleName));

            string loadScenePath = "";
            if (File.Exists(assetPath))
            {
                if (!cachedAsset.ContainsKey(bundleName) || cachedAsset[bundleName] == null)
                {
                    AssetBundle loadedBundle = AssetBundle.LoadFromFile(assetPath);
                    cachedAsset[bundleName] = loadedBundle;
                }

                AssetBundle asset = cachedAsset[bundleName];
                string[] scenes = asset.GetAllScenePaths();
                
                foreach (string sname in scenes)
                {
                    if (sname.Contains(targetResource,StringComparison.OrdinalIgnoreCase))
                    {
                        loadScenePath = sname;
                    }
                }
            }
            return loadScenePath;
            // 에셋 번들 내에 존재하는 씬의 경로를 모두 가져오기
        }
		#endregion
		public static IEnumerator LoadAssetAsync(eResourcePath eType, string fileName, DownloadState state)
		{
			string assetName = eType.GetBundleName();
			string key = assetName.Split(".")[0];
			string assetPath = Path.Combine(ClientConstants.AssetBundleDownloadPath, assetName);
			if (File.Exists(assetPath) && AssetBundleManager.IsBundle(eType))
			{
				if (!cachedAsset.ContainsKey(key) || cachedAsset[key] == null)
				{
					AssetBundleCreateRequest cRequest = AssetBundle.LoadFromFileAsync(assetPath);
					while (false == cRequest.isDone)
					{
						state?.Invoke(Mathf.FloorToInt(cRequest.progress * 100), 100);
						yield return SBDefine.GetWaitForEndOfFrame();
					}
					if (cRequest.assetBundle != null)
						cachedAsset[key] = cRequest.assetBundle;
				}
			}
			else
			{
				var cRequest = Resources.LoadAsync(SBFunc.StrBuilder("AssetBundle/", SBDefine.ResourcePath(eType, fileName)));
				while (false == cRequest.isDone)
				{
					state?.Invoke(Mathf.FloorToInt(cRequest.progress * 100), 100);
					yield return SBDefine.GetWaitForEndOfFrame();
				}
			}
			yield break;
		}
		public static AsyncOperation LoadAssetAsync(eResourcePath eType, string fileName)
		{
			string assetName = eType.GetBundleName();
			string key = assetName.Split(".")[0];
			string assetPath = Path.Combine(ClientConstants.AssetBundleDownloadPath, assetName);
			if (File.Exists(assetPath) && AssetBundleManager.IsBundle(eType))
			{
				if (!cachedAsset.ContainsKey(key) || cachedAsset[key] == null)
				{
					return AssetBundle.LoadFromFileAsync(assetPath);
				}
				return null;
			}
			else
			{
				return Resources.LoadAsync(SBFunc.StrBuilder("AssetBundle/", SBDefine.ResourcePath(eType, fileName)));
			}
		}
		#region Preload
		public static IEnumerator CacheAsset(string fullpath, AssetInfo asset, int curSize, int totalSize, DownloadState LoadingStateShow)
		{
			string[] splits = fullpath.ToLower().Split('/');
			string assetName = splits[splits.Length - 1];
			string key = splits[splits.Length - 1].Split('.')[0];
			string assetPath = Path.Combine(ClientConstants.AssetBundleDownloadPath, assetName);

			if (File.Exists(assetPath))
			{
				//받아온 Asset는 로드 우선
				//if (!cachedAsset.ContainsKey(key) || cachedAsset[key] == null)
				//{
				//	AssetBundleCreateRequest loadedBundle = AssetBundle.LoadFromFileAsync(assetPath);
				//	while(false == loadedBundle.isDone)
				//	{
				//		LoadingStateShow(curSize + Mathf.FloorToInt(asset.Size * loadedBundle.progress), totalSize);
				//		yield return SBDefine.GetWaitForEndOfFrame();
				//	}
				//	cachedAsset[key] = loadedBundle.assetBundle;
				//}
				AssetBundleCreateRequest loadedBundle = AssetBundle.LoadFromFileAsync(assetPath);
				while (false == loadedBundle.isDone)
				{
					LoadingStateShow(curSize + Mathf.FloorToInt(asset.Size * loadedBundle.progress), totalSize);
					yield return SBDefine.GetWaitForEndOfFrame();
				}
				if (cachedAsset.ContainsKey(key) && cachedAsset[key] != null)
                {
					cachedAsset[key].UnloadAsync(true);
					cachedAsset[key] = null;
				}
				cachedAsset[key] = loadedBundle.assetBundle;
			}
		}
		public static List<AssetBundleRequest> PopupPreload()
		{
			// ex) popup/accelerationmainpopup.prefab
			if (cachedAsset[KEY_POPUP] != null)
			{
				List<string> names = cachedAsset[KEY_POPUP].GetAllAssetNames().ToList();
				List<AssetBundleRequest> abr = new List<AssetBundleRequest>();

				foreach (string name in names)
				{
					abr.Add(cachedAsset[KEY_POPUP].LoadAssetAsync(name));
				}

				return abr;
			}
			return null;
		}
		public static AssetBundleRequest LoadAllBundleAsync<T>(string bundleKey)
		{
			bundleKey = bundleKey.ToLower();
			if (cachedAsset.ContainsKey(bundleKey) && cachedAsset[bundleKey] != null)
			{
				return cachedAsset[bundleKey].LoadAllAssetsAsync<T>();
			}

			return null;
		}
		#endregion
		public static string[] GetLoadingMessageFromText()
		{
			string[] messages = { 
				"로딩가십1",
				"로딩가십2",
				"로딩가십3",
				"로딩가십4",
				"로딩가십5",
				"로딩가십6",
				"로딩가십7",
				"로딩가십8",
				"로딩가십9",
				"로딩가십10",
				"로딩가십11",
			};

			return messages;
		}
		public bool HasCachedAsset(string key)
		{
			return cachedAsset.ContainsKey(key) && cachedAsset[key] != null;
		}
		public static IEnumerator LoadAsyncPaths(Dictionary<eResourcePath, List<string>> targets)
		{
			if (targets == null)
				yield break;

			var reqDic = new Dictionary<AsyncOperation, bool>();

			var targetCount = 0;
			var it = targets.GetEnumerator();
			while (it.MoveNext())
            {
				if (it.Current.Value == null)
					continue;

				for(int i = 0, count= it.Current.Value.Count; i < count; ++i)
				{
					var key = LoadAssetAsync(it.Current.Key, it.Current.Value[i]);
					if (key == null)
						continue;

					reqDic.Add(key, false);
					targetCount++;
				}
            }
			var wait = SBDefine.GetWaitForSeconds(0.1f);

			var keys = new List<AsyncOperation>(reqDic.Keys);
			var keysCount = keys.Count;
			var pathCount = 0;
			while(pathCount < targetCount)
			{
				yield return wait;

				for(int i = 0; i < keysCount; ++i)
                {
					var key = keys[i];
					if (key == null)
						continue;

					if (!reqDic[key] && key.isDone)
					{
						reqDic[key] = true;
						pathCount++;
					}
				}
			}

			yield break;
        }
		public void Update(float dt) {}
    }

}
