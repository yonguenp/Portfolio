using System.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.Networking;
using UnityEngine;
using System.Security.Cryptography;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class AssetInfo
    {
        public string Name;
        public int Size;
        public string Hash;

        public override string ToString()
        {
            return string.Format("{0} | {1} | {2}", Name, Size, Hash);
        }

        public static AssetInfo ParseFromString(string rawData)
        {
            if (string.IsNullOrEmpty(rawData))
            {
                return null;
            }

            rawData = rawData.Trim();
            string[] assetFileInfo = rawData.Split('|');
            if (assetFileInfo.Length != 3)
            {
                return null;
            }

            string assetFileName = assetFileInfo[0].Trim();
            string assetFileSize = assetFileInfo[1].Trim();
            string assetFileHash = assetFileInfo[2].Trim();

            return new AssetInfo
            {
                Name = assetFileName,
                Size = Convert.ToInt32(assetFileSize),
                Hash = assetFileHash
            };
        }
    }

    public class AssetBundleManager : IManagerBase
    {
        public static bool isDownload = false;

        static readonly string[] suffixes =
        { "Bytes", "KB", "MB", "GB", "TB", "PB" };

        static readonly int DOWNLOAD_LIMIT = 3;
        static readonly int TRY_LIMIT = 2;

        public static bool UseBundleAssetEditor
        {
            get
            {
                return PlayerPrefs.GetInt(ClientConstants.DevAssetBundleUse, 0) == 1;
            }

            set
            {
                PlayerPrefs.SetInt(ClientConstants.DevAssetBundleUse, value ? 1 : 0);
            }
        }

        public static bool IsBundleAssetsDownload = false;
        private static Dictionary<string, bool> UseBundleAssets = new ();

        public static string CDN_URL
        {
            get
            {
                return NetworkManager.CDN;
            }
        }

        public static string FormatSize(int bytes)
        {
            int counter = 0;
            decimal number = (decimal)bytes;

            while (Math.Round(number / 1024) >= 1)
            {
                number = number / 1024;
                ++counter;
            }

            return string.Format("{0:n1} {1}", number, suffixes[counter]);
        }
        public static AssetInfo[] ParseAssetInfo(string text)
        {
            try
            {
                List<AssetInfo> result = new List<AssetInfo>();

                text = text.Trim();
                text = text.Trim('\uFEFF');     // UTF-8 BOM 헤더 삭제 

                using (StringReader sr = new StringReader(text))
                {
                    while (true)
                    {
                        string rawData = sr.ReadLine();
                        AssetInfo assetInfo = AssetInfo.ParseFromString(rawData);

                        if (assetInfo != null)
                        {
                            result.Add(assetInfo);
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                return result.ToArray();
            }
            catch
            {
                return null;
            }
        }

        public static bool IsBundle(string name)
        {
            if (UseBundleAssets.TryGetValue(name.ToLower(), out var value))
                return value;

            return false;
        }

        public static bool IsBundle(eResourcePath type)
        {
            if (UseBundleAssets.TryGetValue(type.GetBundleName(), out var value))
                return value;

            return false;
        }

        public static bool IsBundle(string[] name)
        {
            if (name == null || name.Length < 2)
                return false;

            if (IsBundle(SBFunc.StrBuilder(name[0], SBDefine.BUNDLE_EXTENSION)) || IsBundle(SBFunc.StrBuilder(name[1], SBDefine.BUNDLE_EXTENSION)))
                return true;

            return false;
        }

        private static string GetCDNAddress()
        {
            return SBFunc.StrBuilder(CDN_URL, "assetbundle/", GamePreference.Instance.RESOURCE_BUNDLE_VERSION, "/", ClientConstants.PLATFORM);
        }

        public static IEnumerator AssetInfoSyncCoroutine(DownloadState state)
        {
            string assetinfoPath = SBFunc.StrBuilder(ClientConstants.LocalBundleInfoPath, "assets");
            TextAsset includeAssetInfoFile = Resources.Load<TextAsset>(assetinfoPath);
            bool isAssetInfo = includeAssetInfoFile != null;
            string oldAssetInfoFilePath = SBFunc.StrBuilder(ClientConstants.AssetBundleDownloadPath, ClientConstants.AssetBundleListFileName);
            //string assetListUrl = SBFunc.StrBuilder(GetCDNAddress(), ClientConstants.AssetBundleBuildMetaFile);
            string assetListUrl = SBFunc.StrBuilder(GetCDNAddress(), ClientConstants.AssetBundleListFileName);
            bool needDownload = false;
            IsBundleAssetsDownload = false;

            if (isAssetInfo)
            {
                //빌드에 포함된 assets.txt 정보 파일과 비교
                AssetInfo[] includeAssetInfos = ParseAssetInfo(includeAssetInfoFile.text);
                if (File.Exists(oldAssetInfoFilePath))
                {
                    var file = File.ReadAllText(oldAssetInfoFilePath);
                    if (file != null)
                        includeAssetInfos = ParseAssetInfo(file);
                }
                AssetInfo[] newAssetInfos = null;

                using (UnityWebRequest www = UnityWebRequest.Get(assetListUrl))
                {
                    yield return www.SendWebRequest();
                    if (www.result == UnityWebRequest.Result.Success)
                    {
                        newAssetInfos = ParseAssetInfo(www.downloadHandler.text);
                        bool isCheck = false;

                        foreach (AssetInfo newAsset in newAssetInfos)
                        {
                            AssetInfo oldAsset = includeAssetInfos?.FirstOrDefault(x => x.Name == newAsset.Name);

                            if (oldAsset != null)
                            {
                                // 파일 사이즈와 해시가 전부 같지 않으면 다운로드가 필요하다
                                isCheck = !(oldAsset.Size == newAsset.Size && oldAsset.Hash == newAsset.Hash);

                                if (isCheck)
                                {
                                    needDownload = true;
                                }
                            }
                            else
                            {
                                needDownload = true;
                            }

                            if (needDownload)
                            {
                                Debug.LogWarning("노후된 IncludeAssetInfo, 다운로드 시도로 이동");
                                break;
                            }
                        }
                    }
                    else
                    {
                        //CDN 통신 오류 => 종료
                        SystemLoadingPopup systemPopup = SystemLoadingPopup.Instance;
                        if (systemPopup != null)
                        {
                            systemPopup.SetMessage(StringData.GetStringByIndex(100000618), StringData.GetStringByIndex(100002656), StringData.GetStringByIndex(100002421));
                            systemPopup.SetCallBack(() =>
                            {
                                Quit();
                            });
                        }
                        yield return new WaitUntil(() => false);
                    }
                }
            }
            else
            {
                Debug.LogWarning("Assets 정보가 없음 다운로드 시도");
                needDownload = true;
            }

            //다운로드 필요시 다운로드 진행
            IsBundleAssetsDownload = needDownload;
            if(false == IsBundleAssetsDownload)
            {
                if (false == isAssetInfo)
                    yield break;

                AssetInfo[] defaultIncludeAssetInfos = ParseAssetInfo(includeAssetInfoFile.text);
                AssetInfo[] curIncludeAssetInfos = null;
                if (File.Exists(oldAssetInfoFilePath))
                {
                    var file = File.ReadAllText(oldAssetInfoFilePath);
                    if (file != null)
                        curIncludeAssetInfos = ParseAssetInfo(file);
                }
                if (curIncludeAssetInfos == null)
                    yield break;

                int totalSize = InitializeIsBundle(defaultIncludeAssetInfos, curIncludeAssetInfos);
                yield return CacheBundle(curIncludeAssetInfos, 0, totalSize, state);
            }

            //이상 없다면 Resources 자원 사용
            yield break;
        }

        public static IEnumerator AssetBundleFileSyncCoroutine(DownloadState LoadingStateShow)
        {
            string oldAssetInfoFilePath = SBFunc.StrBuilder(ClientConstants.AssetBundleDownloadPath, ClientConstants.AssetBundleListFileName);
            string assetinfoPath = SBFunc.StrBuilder(ClientConstants.LocalBundleInfoPath, "assets");
            TextAsset includeAssetInfoFile = Resources.Load<TextAsset>(assetinfoPath);
            string serverAddress = GetCDNAddress();
            Debug.LogWarning(SBFunc.StrBuilder("Download Folder -> ", serverAddress));

            //string assetListUrl = SBFunc.StrBuilder(serverAddress, ClientConstants.AssetBundleBuildMetaFile);
            string assetListUrl = SBFunc.StrBuilder(serverAddress, ClientConstants.AssetBundleListFileName);
            string[] loadindString = ResourceManager.GetLoadingMessageFromText();

            bool isNeedToDownload = false;

            AssetInfo[] newAssetInfos = null;
            AssetInfo[] oldAssetInfos = null;
            //AssetInfo[] oldAssetInfos = ParseAssetInfo(includeAssetInfoFile.text);//여기오면 다 다운로드 받아야함
            //if (File.Exists(oldAssetInfoFilePath))
            //{
            //    oldAssetInfos = ParseAssetInfo(File.ReadAllText(oldAssetInfoFilePath));
            //}

            if (!Directory.Exists(ClientConstants.AssetBundleDownloadPath))
            {
                Directory.CreateDirectory(ClientConstants.AssetBundleDownloadPath);
            }

            int totalSize = 0;
            int newSize = 0;
            using (UnityWebRequest www = UnityWebRequest.Get(assetListUrl))
            {
                yield return www.SendWebRequest();
                if (www.result == UnityWebRequest.Result.Success)
                {
                    List<AssetInfo> downloadInfo = new List<AssetInfo>();
                    newAssetInfos = ParseAssetInfo(www.downloadHandler.text);

                    foreach (AssetInfo newAsset in newAssetInfos)
                    {
                        //AssetInfo oldAsset = oldAssetInfos?.FirstOrDefault(x => x.Name == newAsset.Name);

                        //if (oldAsset != null)
                        //{
                        //    // 파일 사이즈와 해시가 전부 같지 않으면 다운로드가 필요하다
                        //    isNeedToDownload = !(oldAsset.Size == newAsset.Size && oldAsset.Hash == newAsset.Hash);

                        //    if (isNeedToDownload)
                        //    {
                        //        downloadInfo.Add(newAsset);
                        //        totalSize += newAsset.Size;
                        //    }
                        //}
                        //else
                        //{
                        //    downloadInfo.Add(newAsset);
                        //    totalSize += newAsset.Size;
                        //}
                        //한번에 모두 받아야 메타파일 오류가 생기지 않음.
                        downloadInfo.Add(newAsset);
                        totalSize += newAsset.Size;
                    }

                    if (totalSize > 0)
                    {
                        SystemLoadingPopup systemPopup = SystemLoadingPopup.Instance;
                        if (systemPopup != null)
                        {
                            systemPopup.SetMessage(StringData.GetStringByIndex(100000248), StringData.GetStringFormatByIndex(100002657, FormatSize(totalSize)), StringData.GetStringByIndex(100000199), StringData.GetStringByIndex(100000200));
                            systemPopup.SetCallBack(() =>
                            {
                                isDownload = true;
                            },
                            () =>
                            {
                                Quit();
                            });
                        }

                        yield return new WaitUntil(() => isDownload);

                        // 확실한 피드백을 위해 다운로드 시도 횟수 제한과 다시시도 횟수를 따로둠

                        int downloadLimit = 0;
                        int tryLimit = 0;
                        // 아래에서 Cacha하는 것도 포함하기 위해 2배(다운로드 반절, 메모리 올리기 반절)
                        totalSize *= 2;

                        foreach (AssetInfo newAsset in downloadInfo)
                        {
                            isNeedToDownload = true;
                            var oldName = SBFunc.StrBuilder(ClientConstants.AssetBundleDownloadPath, newAsset.Name);
                            //다운로드 받을 파일이 존재하면 삭제
                            if (File.Exists(oldName))
                                File.Delete(oldName);

                            if (isNeedToDownload)
                            {
                                string assetUrl = serverAddress + newAsset.Name;
                                string prevHash = newAsset.Hash;

                                while (isNeedToDownload)
                                {
                                    isDownload = false;
                                    if (tryLimit >= TRY_LIMIT)
                                    {
                                        systemPopup.SetMessage(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100002658), StringData.GetStringByIndex(100002659), StringData.GetStringByIndex(100002662));
                                        systemPopup.SetCallBack(() =>
                                        {
                                            AssetFileRemove();
                                            LoadingManager.Instance.EffectiveSceneLoad("Start");
                                        },
                                        () =>
                                        {
                                            isDownload = true;
                                            Quit();
                                        });

                                        yield return new WaitUntil(() => isDownload);
                                    }
                                    else if (downloadLimit >= DOWNLOAD_LIMIT)
                                    {
                                        systemPopup.SetMessage(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100002660), StringData.GetStringByIndex(100000199), StringData.GetStringByIndex(100000200));
                                        systemPopup.SetCallBack(() =>
                                        {
                                            isDownload = true;
                                            tryLimit += 1;
                                            downloadLimit = 0;
                                        }, Quit);

                                        yield return new WaitUntil(() => isDownload);
                                    }

                                    UnityWebRequest wwwAsset = UnityWebRequest.Get(assetUrl);
                                    wwwAsset.SendWebRequest();
                                    while (false == wwwAsset.isDone)
                                    {
                                        LoadingStateShow(newSize + Mathf.FloorToInt(newAsset.Size * wwwAsset.downloadProgress), totalSize);
                                        yield return null;
                                    }

                                    if (wwwAsset.result == UnityWebRequest.Result.Success)
                                    {
                                        MD5 md5 = MD5.Create();
                                        string fileHash = BitConverter.ToString(md5.ComputeHash(wwwAsset.downloadHandler.data)).Replace("-", "").ToLower();

                                        //다르다면 무한다운로드를 시도할 것

                                        if (fileHash == prevHash)
                                        {
                                            newSize += newAsset.Size;
                                            LoadingStateShow(newSize, totalSize);
                                            Debug.Log(SBFunc.StrBuilder("Asset downloaded : ", ClientConstants.AssetBundleDownloadPath, newAsset.Name));
                                            isNeedToDownload = false;
                                            File.WriteAllBytes(SBFunc.StrBuilder(ClientConstants.AssetBundleDownloadPath, newAsset.Name), wwwAsset.downloadHandler.data);
                                        }
                                        else
                                        {
                                            downloadLimit += 1;
                                            Debug.Log(SBFunc.StrBuilder("Asset download fail : ", ClientConstants.AssetBundleDownloadPath, newAsset.Name));
                                            Debug.Log(string.Format("byte is {0}", Convert.ToBase64String(wwwAsset.downloadHandler.data)));
                                        }
                                    }
                                    else
                                    {
                                        isDownload = false;
                                        systemPopup.SetMessage(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100002661), StringData.GetStringByIndex(100000199), StringData.GetStringByIndex(100000200));
                                        systemPopup.SetCallBack(() =>
                                        {
                                            isDownload = true;
                                            downloadLimit = 0;
                                            tryLimit += 1;
                                        }, Quit);

                                        Debug.LogError(wwwAsset.responseCode);
                                        Debug.LogError(string.Format("Failed to load assetbundle : {0} / {1}", assetUrl, wwwAsset.result));
                                        yield return new WaitUntil(() => isDownload);
                                    }
                                }
                            }
                        }

                        File.WriteAllBytes(oldAssetInfoFilePath, www.downloadHandler.data);
                    }
                }
                else
                {
                    Debug.LogError(string.Format("Failed to load asset list file : {0} / {1}", assetListUrl, www.result));
                    SystemLoadingPopup systemPopup = SystemLoadingPopup.Instance;
                    if (systemPopup != null)
                    {
                        systemPopup.SetMessage(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100002663), StringData.GetStringByIndex(100002662));
                        systemPopup.SetCallBack(Quit);
                    }
                }
            }
            //Download Done?

            if (!isNeedToDownload)
            {
                AssetInfo[] includeAssetInfos = ParseAssetInfo(includeAssetInfoFile.text);
                if(includeAssetInfos != null)
                {
                    totalSize = InitializeIsBundle(includeAssetInfos, newAssetInfos);
                    yield return CacheBundle(newAssetInfos, totalSize, totalSize * 2, LoadingStateShow);
                }
            }
        }
        public static int InitializeIsBundle(AssetInfo[] includeAssetInfos, AssetInfo[] newAssetInfos)
        {
            int totalSize = 0;
            UseBundleAssets.Clear();
            foreach (var info in includeAssetInfos)
            {
                if (null == info)
                    continue;

                AssetInfo newAsset = newAssetInfos?.FirstOrDefault(x => x.Name == info.Name);
                if (null == newAsset)
                {
                    totalSize += info.Size;
                    continue;
                }
                else
                {
                    totalSize += newAsset.Size;
                }

                //if (info.Size == newAsset.Size && info.Hash == newAsset.Hash)
                //    continue;

                UseBundleAssets.Add(info.Name, true);
            }
            return totalSize;
        }
        private static IEnumerator CacheBundle(AssetInfo[] newAssetInfos, int newSize, int totalSize, DownloadState LoadingStateShow)
        {
            IEnumerator ie = newAssetInfos.GetEnumerator();
            while (ie.MoveNext())
            {
                AssetInfo asset = (AssetInfo)ie.Current;
                if (IsBundle(asset.Name))
                {
                    yield return ResourceManager.CacheAsset(asset.Name, asset, newSize, totalSize, LoadingStateShow);
                    newSize += asset.Size;
                }
            }
            yield break;
        }

        public static void AssetFileRemove()
        {
            var oldAssetInfoFilePath = SBFunc.StrBuilder(ClientConstants.AssetBundleDownloadPath, ClientConstants.AssetBundleListFileName);
            if (File.Exists(oldAssetInfoFilePath))
            {
                AssetInfo[] oldAssetInfos = ParseAssetInfo(File.ReadAllText(oldAssetInfoFilePath));

                foreach (var item in oldAssetInfos)
                {
                    var file3d = SBFunc.StrBuilder(ClientConstants.AssetBundleDownloadPath, item.Name);
                    File.Delete(file3d);
                }
                File.Delete(oldAssetInfoFilePath);


                return;
            }
        }

        private static void Quit()
        {
            SBFunc.Quit();
        }

        public void Initialize() { }

        public void Update(float dt) { }
    }

    public class CDNManager : IManagerBase
    {
        public static string CDN_URL { get { return NetworkManager.CDN; } }
        public static string BannerIdendifier { get { return "banner/"; } }
        static string NFTIdendifier { get { return "nft/"; } }

        static Queue<string> ResourceQueue = new Queue<string>();

        static Dictionary<string, Sprite> sprites = null;
        static Dictionary<string, Sprite> Sprites
        {
            get
            {
                if (sprites == null)
                {
                    LoadDefault();
                }

                return sprites;
            }
        }

        static List<UnityEngine.Networking.UnityWebRequest> reqPool = new List<UnityWebRequest>();

        public static void LoadDefault()
        {
            sprites = new Dictionary<string, Sprite>();
            reqPool.Clear();

            //LoadDefaultAnnouncement();
            //LoadDefaultEvent();
            //LoadDefaultStore();
            //LoadDefaultGacha();
        }

        public static IEnumerator LoadDefaultAnnouncement(DownloadState state)
        {
            string folder = SBFunc.StrBuilder("AssetBundle/Images/announcement/", LanguageData.LanguageFolder, "/");
            yield return SBFunc.DownloadStateEvent(state, 0, 20, 100);
            var arr = Resources.LoadAll<Sprite>(folder);
            foreach (var sp in arr)
            {
                Sprites[BannerIdendifier + "announcement/" + folder + sp.name + ".png"] = sp;
            }
            yield break;
        }

        public static IEnumerator LoadDefaultEvent(DownloadState state)
        {
            string folder = SBFunc.StrBuilder("AssetBundle/Images/event/", LanguageData.LanguageFolder, "/");
            yield return SBFunc.DownloadStateEvent(state, 20, 40, 100);
            var arr = Resources.LoadAll<Sprite>(folder);
            foreach (var sp in arr)
            {
                Sprites[BannerIdendifier + "event/" + folder + sp.name + ".png"] = sp;
            }
            yield break;
        }
        public static IEnumerator LoadDefaultStore(DownloadState state)
        {
            string folder = SBFunc.StrBuilder("AssetBundle/Images/store/", LanguageData.LanguageFolder, "/");
            yield return SBFunc.DownloadStateEvent(state, 40, 60, 100);
            var arr = Resources.LoadAll<Sprite>(folder);
            foreach (var sp in arr)
            {
                Sprites[BannerIdendifier + "store/" + folder + sp.name + ".png"] = sp;
            }
            yield break;
        }
        public static IEnumerator LoadDefaultGacha(DownloadState state)
        {
            yield return SBFunc.DownloadStateEvent(state, 60, 80, 100);
            var arr = Resources.LoadAll<Sprite>("AssetBundle/Images/gacha/");
            foreach (var sp in arr)
            {
                Sprites[BannerIdendifier + "gacha/" + sp.name + ".png"] = sp;
            }
            yield break;
        }

        public static IEnumerator LoadDefaultEventAttendance(DownloadState state)
        {
            yield return SBFunc.DownloadStateEvent(state, 80, 100, 100);
            var arr = Resources.LoadAll<Sprite>("AssetBundle/Images/event_attendance/");
            foreach (var sp in arr)
            {
                Sprites[BannerIdendifier + "event/attendance/" + sp.name + ".png"] = sp;
            }
            yield break;
        }

        public static Sprite LoadBanner(string resource_path)
        {
            resource_path = BannerIdendifier + resource_path;

            if (Sprites.ContainsKey(resource_path))
            {
                if (Sprites[resource_path] != null)
                {
                    return Sprites[resource_path];
                }
            }

            LoadResource(resource_path);
            return null;
        }

        public static void SetBanner(string resource_path, Image target, Action action = null, Action fail = null)
        {
            resource_path = BannerIdendifier + resource_path;

            if (Sprites.ContainsKey(resource_path))
            {
                if (Sprites[resource_path] != null)
                {
                    target.sprite = Sprites[resource_path];
                    action?.Invoke();
                    return;
                }
            }

            LoadResourceAndSet(resource_path, target, action, fail);
        }

        public static void TrySetBannerCatchDefault(string resource, string path, Image target, Action action = null, Action fail = null)
        {
            SetBanner(SBFunc.GetResourceNameByLang(resource, path), target, action, fail != null ? fail  : (LanguageData.LanguageFolder == "en" ? null : ()=> {
                SetBanner(SBFunc.GetResourceNameByLang(resource, path, "en"), target, action, fail);
            }));
        }

        public static void SetNFTSprite(int id, Image target)
        {
            string resource_path = NFTIdendifier + id.ToString() + ".jpg";

            if (Sprites.ContainsKey(resource_path))
            {
                if (Sprites[resource_path] != null)
                {
                    target.sprite = Sprites[resource_path];
                    return;
                }
            }

            LoadResourceAndSet(resource_path, target);
        }

        private static void LoadResourceAndSet(string resource_path, Image target, Action action = null, Action fail = null)
        {
            if (string.IsNullOrEmpty(resource_path))
            {
                Debug.LogError("resource_path is null or empty");
                return;
            }
            if (resource_path.EndsWith("/"))
            {
                Debug.LogError("resource's target is empty ( path is end by '/' )");
                return;
            }

            if (Game.Instance != null)
            {
                Game.Instance.StartCoroutine(ResourceSyncAndSet(resource_path, target, action, fail));
            }
            else
            {
                Debug.LogError("Game instance is null");
            }
        }

        static IEnumerator ResourceSyncAndSet(string resource_path, Image target, Action action = null, Action fail = null)
        {
            yield return ResourceSync(resource_path);

            if (target == null)
            {
                fail?.Invoke();
                yield break;
            }

            if (!Sprites.ContainsKey(resource_path))
            {
                fail?.Invoke();
                yield break;
            }

            while (Sprites[resource_path] == null)//혹시 여러개 들어왔을수도 있음
            {
                yield return new WaitForEndOfFrame();
            }

            target.sprite = Sprites[resource_path];

            action?.Invoke();
        }

        private static void LoadResource(string resource_path)
        {
            if (string.IsNullOrEmpty(resource_path))
            {
                Debug.LogError("resource_path is null or empty");
                return;
            }
            if (resource_path.EndsWith("/"))
            {
                Debug.LogError("resource's target is empty ( path is end by '/' )");
                return;
            }

            if (Game.Instance != null)
            {
                Game.Instance.StartCoroutine(ResourceSync(resource_path));
            }
            else
            {
                Debug.LogError("Game instance is null");
            }
        }
        public static IEnumerator ResourceSync(string resource_path)
        {
            if (!Sprites.ContainsKey(resource_path))
            {
                var ResourcePath = SBFunc.StrBuilder(ClientConstants.AssetBundleDownloadPath, resource_path);
                if (File.Exists(ResourcePath))
                {
                    Texture2D texture = new Texture2D(0, 0);
                    texture.LoadImage(File.ReadAllBytes(ResourcePath));
                    
                    texture.filterMode = FilterMode.Bilinear; // 또는 Trilinear
                    texture.anisoLevel = 4; // 원한다면 향상된 필터링

                    Rect rect = new Rect(0, 0, texture.width, texture.height);
                    if (Sprites.ContainsKey(resource_path))
                    {
                        Sprites[resource_path] = Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f));
                    }
                    else
                    {
                        Sprites.Add(resource_path, Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f)));
                    }
                }
                else
                {
                    while (reqPool.Count > 5)
                    {
                        yield return new WaitForEndOfFrame();
                    }

                    UnityWebRequest wr = UnityWebRequestTexture.GetTexture(CDN_URL + resource_path);
                    reqPool.Add(wr);

                    DownloadHandlerTexture texDI = new DownloadHandlerTexture(true);
                    wr.downloadHandler = texDI;

                    yield return wr.SendWebRequest();

                    if (wr.result != UnityWebRequest.Result.Success)
                    {
                        Sprites.Remove(resource_path);
                        Debug.LogError("이벤트 배너 다운로드 오류!!!!!!!!!!!!!!! : " + resource_path);
                    }
                    else
                    {
                        if (!Directory.Exists(ClientConstants.AssetBundleDownloadPath))
                        {
                            Directory.CreateDirectory(ClientConstants.AssetBundleDownloadPath);
                        }

                        string[] folders = resource_path.Split('/');
                        string folder = "";
                        for (int i = 0; i < folders.Length - 1; i++)
                        {
                            folder += folders[i] + "/";
                            var folderPath = SBFunc.StrBuilder(ClientConstants.AssetBundleDownloadPath, folder);
                            if (!Directory.Exists(folderPath))
                            {
                                Directory.CreateDirectory(folderPath);
                            }
                        }

                        var fileStream = new FileStream(ResourcePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                        fileStream.Write(texDI.data, 0, texDI.data.Length);
                        fileStream.Close();

                        Rect rect = new Rect(0, 0, texDI.texture.width, texDI.texture.height);
                        Sprites[resource_path] = Sprite.Create(texDI.texture, rect, new Vector2(0.5f, 0.5f));

                        texDI.texture.filterMode = FilterMode.Bilinear; // 또는 Trilinear
                        texDI.texture.anisoLevel = 4; // 원한다면 향상된 필터링
                    }

                    reqPool.Remove(wr);
                }
            }
        }
        public static IEnumerator CDNResourcePreLoadDefault(DownloadState state)
        {
            state?.Invoke(0, 100); //"디폴트이미지로드",
            yield return LoadDefaultAnnouncement(state);
            var unloaded = Resources.UnloadUnusedAssets();
            while (false == unloaded.isDone)
            {
                yield return SBDefine.GetWaitForEndOfFrame();
            }

            yield return LoadDefaultEvent(state);
            unloaded = Resources.UnloadUnusedAssets();
            while (false == unloaded.isDone)
            {
                yield return SBDefine.GetWaitForEndOfFrame();
            }

            yield return LoadDefaultStore(state);
            unloaded = Resources.UnloadUnusedAssets();
            while (false == unloaded.isDone)
            {
                yield return SBDefine.GetWaitForEndOfFrame();
            }

            yield return LoadDefaultGacha(state);
            unloaded = Resources.UnloadUnusedAssets();
            while (false == unloaded.isDone)
            {
                yield return SBDefine.GetWaitForEndOfFrame();
            }

            yield return LoadDefaultEventAttendance(state);
            unloaded = Resources.UnloadUnusedAssets();
            while (false == unloaded.isDone)
            {
                yield return SBDefine.GetWaitForEndOfFrame();
            }
            state?.Invoke(100, 100); //"디폴트이미지로드"
        }
        public static IEnumerator CDNResourcePreLoad(DownloadState state)
        {
            int count = ResourceQueue.Count - 1;
            int index = 0;

#if UNITY_IOS
            int needs = 0;
            //todo : iOS 검수를 위해 가라로 이미지 사이즈 측정해서 팝업 띄워주자.
            System.Random rand = new System.Random(200);
            foreach (var res in ResourceQueue)
            {
                if (!Sprites.ContainsKey(res))
                {
                    var ResourcePath = SBFunc.StrBuilder(ClientConstants.AssetBundleDownloadPath, res);
                    if (!File.Exists(ResourcePath))
                    {
                        needs += rand.Next(50, 280) * 1024;
                    }
                }
            }

            
            if (needs > 0)
            {
                bool confirm = false;
                SystemLoadingPopup systemPopup = SystemLoadingPopup.Instance;
                if (systemPopup != null)
                {
                    systemPopup.SetMessage(StringData.GetStringByIndex(100000248), StringData.GetStringFormatByIndex(100002657, AssetBundleManager.FormatSize(needs)), StringData.GetStringByIndex(100000199), StringData.GetStringByIndex(100000200));
                    systemPopup.SetCallBack(() =>
                    {
                        confirm = true;
                    },
                    () =>
                    {
                        SBFunc.Quit();
                    });
                }
                yield return new WaitUntil(() => confirm);
            }
#endif

            AppsFlyerSDK.AppsFlyer.sendEvent("cdn_download_start", new Dictionary<string, string>());
            float p = 0.0f;
            state?.Invoke(index, count); //"이미지로드"
            while (ResourceQueue.Any())
            {
                float c = index / count;
                if (p < 0.25f && c > 0.25f)
                {
                    AppsFlyerSDK.AppsFlyer.sendEvent("cdn_download_25p", new Dictionary<string, string>());
                }
                if (p < 0.5f && c > 0.5f)
                {
                    AppsFlyerSDK.AppsFlyer.sendEvent("cdn_download_50p", new Dictionary<string, string>());
                }
                if (p < 0.75f && c > 0.75f)
                {
                    AppsFlyerSDK.AppsFlyer.sendEvent("cdn_download_75p", new Dictionary<string, string>());
                }

                state?.Invoke(index++, count); //"이미지로드"
                yield return ResourceSync(ResourceQueue.Dequeue());
            }

            AppsFlyerSDK.AppsFlyer.sendEvent("cdn_download_finished", new Dictionary<string, string>());

            yield return Resources.UnloadUnusedAssets();
            yield break;
        }

        public static void AddCDNResourceQueue(string path)
        {
            ResourceQueue.Enqueue(BannerIdendifier + path);
        }

        public void Initialize() { }

        public void Update(float dt) { }
    }
}