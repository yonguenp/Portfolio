using Spine.Unity;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ResourceManager
{
    Dictionary<string, AssetBundle> cachedAsset = new Dictionary<string, AssetBundle>();

    public T Load<T>(string path) where T : Object
    {
        if (typeof(T) == typeof(GameObject))
        {
            string name = path;
            int index = name.LastIndexOf('/');
            if (index >= 0)
                name = name.Substring(index + 1);

            GameObject go = Managers.Pool.GetOriginal(name);
            if (go != null)
                return go as T;
        }

        return Resources.Load<T>(path);
    }

    public T LoadAssetsBundle<T>(string path) where T : Object
    {
        if (typeof(T) == typeof(GameObject))
        {
            string name = path;
            int index = name.LastIndexOf('/');
            if (index >= 0)
                name = name.Substring(index + 1);

            GameObject go = Managers.Pool.GetOriginal(name);
            if (go != null)
                return go as T;
        }
        //#if !UNITY_EDITOR && !UNITY_STANDALONE_WIN
        if (typeof(T) == typeof(SkeletonDataAsset))
            return LoadSkeletonDataAssetFromBundle(path) as T;
        if (typeof(T) == typeof(AudioClip))
            return LoadAudioClipAssetFromBundle(path) as T;
        if (typeof(T) == typeof(Sprite))
            return LoadUISpriteAssetFromBundle(path) as T;
        //#endif

        return Resources.Load<T>(path);
    }


    public GameObject InstantiateFromBundle(string path, Transform parent = null)
    {
        path = path.ToLower();
        var key = path.Split('/')[0];
        string[] splitStr = path.Split('/');
        var targetResource = string.Empty;
        if (key == "effect")
        {
            if (splitStr.Length > 2)
                targetResource = splitStr[2];
        }
        else
        {
            if(splitStr.Length > 1)
                targetResource = splitStr[1];
        }

        if (string.IsNullOrEmpty(key))
            return null;

        if (string.IsNullOrEmpty(targetResource))
            return null;

        var assetPath = Path.Combine(ClientConstants.AssetBundleDownloadPath, key + ".unity3d");
        if (File.Exists(assetPath))
        {
            if (!cachedAsset.ContainsKey(key))
            {
                var loadedBundle = AssetBundle.LoadFromFile(assetPath);
                cachedAsset.Add(key, loadedBundle);
            }
            var asset = cachedAsset[key];
            var go = asset.LoadAsset<GameObject>(targetResource);
            if (go == null)
            {
                SBDebug.LogError($"{targetResource} is Null");
                return null;
            }
            SBDebug.Log($"Load from Assetbundle : {path}");
            return GameObject.Instantiate(go, parent);
        }
        else
        {
            //SBDebug.LogError($"No assetbundle : {key}");
            return Instantiate(path, parent, true);
        }
    }

    public SkeletonDataAsset LoadSkeletonDataAssetFromBundle(string path)
    {
        path = path.ToLower();
        var key = path.Split('/')[2];
        var targetResource = path.Split('/')[3];
        var assetPath = Path.Combine(ClientConstants.AssetBundleDownloadPath, key + ".unity3d");
        if (File.Exists(assetPath))
        {
            if (!cachedAsset.ContainsKey(key))
            {
                var loadedBundle = AssetBundle.LoadFromFile(assetPath);
                cachedAsset.Add(key, loadedBundle);
            }
            var asset = cachedAsset[key];
            var go = asset.LoadAsset<SkeletonDataAsset>(targetResource);
            //SBDebug.Log($"Load from Assetbundle : {path}");
            if (go == null)
            {
                //SBDebug.LogWarning($"Load from LocalAssets : {path}");
                go = Resources.Load<SkeletonDataAsset>(path);
            }
            return go;
        }
        else
        {
            //SBDebug.Log(string.Format("번들에셋에 데이터가 없습니다. + {0}", targetResource));
            return Resources.Load<SkeletonDataAsset>(path);
        }
    }

    public AudioClip LoadAudioClipAssetFromBundle(string path)
    {
        path = path.ToLower();
        if (path.Split('/').Length < 4)
        {
            return Resources.Load<AudioClip>(path);
        }
        var key = path.Split('/')[1];
        var targetResource = path.Split('/')[3];
        var assetPath = Path.Combine(ClientConstants.AssetBundleDownloadPath, key + ".unity3d");
        if (File.Exists(assetPath))
        {
            if (!cachedAsset.ContainsKey(key))
            {
                var loadedBundle = AssetBundle.LoadFromFile(assetPath);
                cachedAsset.Add(key, loadedBundle);
            }
            var asset = cachedAsset[key];
            var go = asset.LoadAsset<AudioClip>(targetResource);
            //SBDebug.Log($"Load from Assetbundle : {path}");
            if (go == null)
            {
                //SBDebug.LogWarning($"Load from LocalAssets : {path}");
                go = Resources.Load<AudioClip>(path);
            }
            return go;
        }
        else
        {
            //SBDebug.Log(string.Format("번들에셋에 데이터가 없습니다. + {0}", targetResource));
            return Resources.Load<AudioClip>(path);
        }
    }
    public Sprite LoadUISpriteAssetFromBundle(string path)
    {
        path = path.ToLower();
        var key = path.Split('/')[2];
        var targetResource = path.Split('/')[3];
        var assetPath = Path.Combine(ClientConstants.AssetBundleDownloadPath, key + ".unity3d");
        if (File.Exists(assetPath))
        {
            if (!cachedAsset.ContainsKey(key))
            {
                var loadedBundle = AssetBundle.LoadFromFile(assetPath);
                cachedAsset.Add(key, loadedBundle);
            }
            var asset = cachedAsset[key];
            var go = asset.LoadAsset<Sprite>(targetResource);
            //SBDebug.Log($"Load from Assetbundle : {path}");
            if (go == null)
            {
                //SBDebug.LogWarning($"Load from LocalAssets : {path}");
                go = Resources.Load<Sprite>(path);
            }
            return go;
        }
        else
        {
            //SBDebug.Log(string.Format("번들에셋에 데이터가 없습니다. + {0}", targetResource));
            return Resources.Load<Sprite>(path);
        }
    }


    public GameObject Instantiate(string path, Transform parent = null, bool noAsset = false)
    {
#if ASSET_DEV
        if (!noAsset)
        {
            return InstantiateFromBundle(path, null);
        }
#endif
        GameObject original = Load<GameObject>($"Prefabs/{path}");
        if (original == null)
        {
            SBDebug.Log($"Failed to load prefab : {path}");
            return null;
        }

        if (original.GetComponent<Poolable>() != null)
            return Managers.Pool.Pop(original, parent).gameObject;

        GameObject go = Object.Instantiate(original, parent);
        go.name = original.name;
        return go;
    }

    public void Destroy(GameObject go, float delay = 0)
    {
        if (go == null)
            return;

        Poolable poolable = go.GetComponent<Poolable>();
        if (poolable != null)
        {
            Managers.Pool.Push(poolable);
            return;
        }

        Object.Destroy(go, delay);
    }

    public void ClearAsset()
    {
        AssetBundle.UnloadAllAssetBundles(true);
        cachedAsset.Clear();
    }
}
