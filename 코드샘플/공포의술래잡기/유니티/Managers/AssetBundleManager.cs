using System.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.Networking;
using UnityEditor;
using UnityEngine;

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
            return null;

        rawData = rawData.Trim();
        var assetFileInfo = rawData.Split('|');
        if (assetFileInfo.Length != 3)
            return null;

        var assetFileName = assetFileInfo[0].Trim();
        var assetFileSize = assetFileInfo[1].Trim();
        var assetFileHash = assetFileInfo[2].Trim();

        return new AssetInfo
        {
            Name = assetFileName,
            Size = Convert.ToInt32(assetFileSize),
            Hash = assetFileHash
        };
    }
}

public class AssetBundleManager
{
    public static bool isDownload = false;

    static readonly string[] suffixes =
    { "Bytes", "KB", "MB", "GB", "TB", "PB" };

    public static string FormatSize(int bytes)
    {
        int counter = 0;
        decimal number = (decimal)bytes;
        while (Math.Round(number / 1024) >= 1)
        {
            number = number / 1024;
            counter++;
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
                    var rawData = sr.ReadLine();
                    var assetInfo = AssetInfo.ParseFromString(rawData);
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

    public static IEnumerator AssetBundleFileSyncCoroutine()
    {
        var oldAssetInfoFilePath = ClientConstants.AssetBundleDownloadPath + ClientConstants.AssetBundleListFileName;
        AssetInfo[] oldAssetInfos = null;
        if (File.Exists(oldAssetInfoFilePath))
        {
            oldAssetInfos = ParseAssetInfo(File.ReadAllText(oldAssetInfoFilePath));
        }
        //서버 주소 변경
        //var serverAddress = string.Format(ClientConstants.AssetBundleDownloadURLFormat, ClientConstants.CurrentPlatform);
        var serverAddress = SBWeb.CDN_URL + "assetbundle/" + GameConfig.Instance.RESOURCE_BUNDLE_VERSION + "/";
#if UNITY_STANDALONE_WIN || UNITY_EDITOR || UNITY_EDITOR_WIN
        serverAddress = serverAddress + "win/";
#elif UNITY_ANDROID
        serverAddress = serverAddress + "android/";
#elif UNITY_IOS
        serverAddress = serverAddress + "ios/";
#endif
        SBDebug.LogWarning("Download Folder -> " + serverAddress);
        var assetListUrl = serverAddress + ClientConstants.AssetBundleListFileName;

        if (!Directory.Exists(ClientConstants.AssetBundleDownloadPath))
        {
            Directory.CreateDirectory(ClientConstants.AssetBundleDownloadPath);
        }

        using (UnityWebRequest www = UnityWebRequest.Get(assetListUrl))
        {
            yield return www.SendWebRequest();
            if (www.result == UnityWebRequest.Result.Success)
            {
                var newAssetInfos = ParseAssetInfo(www.downloadHandler.text);

                //준형 :: 다운로드 필요한지 체크 필요

                bool isNeedToDownload = true;
                bool isCheck = false;
                int totalsize = 0;
                foreach (var newAsset in newAssetInfos)
                {
                    var oldAsset = oldAssetInfos?.FirstOrDefault(x => x.Name == newAsset.Name);
                    if (oldAsset != null)
                    {
                        // 파일 사이즈와 해시가 전부 같지 않으면 다운로드가 필요하다
                        isNeedToDownload = !(oldAsset.Size == newAsset.Size && oldAsset.Hash == newAsset.Hash);
                        if (!isNeedToDownload)
                            totalsize += newAsset.Size;
                        if (isNeedToDownload)
                            isCheck = true;
                    }
                    if (oldAsset == null)
                    {
                        totalsize += newAsset.Size;
                        isCheck = true;
                    }

                }
                if (isCheck)
                {
                    PopupCanvas.Instance.ShowConfirmPopup(StringManager.GetString("ui_resource_download", FormatSize(totalsize)), StringManager.GetString("button_check"), StringManager.GetString("button_cancel")
                            , () =>
                            {
                                isDownload = true;
                            }
                            , () =>
                            {
#if !UNITY_EDITOR
                                    Application.Quit();
#elif UNITY_EDITOR
                                UnityEditor.EditorApplication.isPlaying = false;
#endif

                            }
                        );
                    yield return new WaitUntil(() => isDownload);
                }

                foreach (var newAsset in newAssetInfos)
                {
                    var scene = Managers.Scene.CurrentScene as StartScene;
                    if (scene != null)
                        scene.SetStateString(StringManager.GetString("로그인스탭_16", newAsset.Name.Split('.')[0]));

                    isNeedToDownload = true;

                    var oldAsset = oldAssetInfos?.FirstOrDefault(x => x.Name == newAsset.Name);
                    if (oldAsset != null)
                    {
                        // 파일 사이즈와 해시가 전부 같지 않으면 다운로드가 필요하다
                        isNeedToDownload = !(oldAsset.Size == newAsset.Size && oldAsset.Hash == newAsset.Hash);
                    }
                    if (isNeedToDownload)
                    {
                        var assetUrl = serverAddress + newAsset.Name;
                        var prevHash = newAsset.Hash;

                        while (isNeedToDownload)
                        {
                            UnityWebRequest wwwAsset = UnityWebRequest.Get(assetUrl);
                            yield return wwwAsset.SendWebRequest();
                            if (wwwAsset.result == UnityWebRequest.Result.Success)
                            {
                                var md5 = System.Security.Cryptography.MD5.Create();
                                string fileHash = BitConverter.ToString(md5.ComputeHash(wwwAsset.downloadHandler.data)).Replace("-", "").ToLower();

                                if (fileHash == prevHash)
                                {
                                    isNeedToDownload = false;
                                    File.WriteAllBytes(ClientConstants.AssetBundleDownloadPath + newAsset.Name, wwwAsset.downloadHandler.data);
                                }

                                SBDebug.Log("Asset downloaded : " + ClientConstants.AssetBundleDownloadPath + newAsset.Name);
                            }
                            else
                            {
                                SBDebug.LogError(wwwAsset.responseCode);
                                SBDebug.LogError(string.Format("Failed to load assetbundle : {0} / {1}", assetUrl, wwwAsset.result));
                            }

                            if (isNeedToDownload)
                            {
                                isDownload = false;
                                PopupCanvas.Instance.ShowConfirmPopup("네트워크불안정", StringManager.GetString("button_check"), StringManager.GetString("button_cancel")
                                    , () =>
                                    {
                                        isDownload = true;
                                    }
                                    , () =>
                                    {
#if !UNITY_EDITOR
                                                Application.Quit();
#elif UNITY_EDITOR
                                            UnityEditor.EditorApplication.isPlaying = false;
#endif

                                        }
                                );
                                yield return new WaitUntil(() => isDownload);
                            }
                        }
                    }
                }

                File.WriteAllBytes(oldAssetInfoFilePath, www.downloadHandler.data);
            }
            else
            {
                SBDebug.LogError(string.Format("Failed to load asset list file : {0} / {1}", assetListUrl, www.result));
            }
        }
    }

    public static void AssetFileRemove()
    {
        var oldAssetInfoFilePath = ClientConstants.AssetBundleDownloadPath + ClientConstants.AssetBundleListFileName;

        if (File.Exists(oldAssetInfoFilePath))
        {
            AssetInfo[] oldAssetInfos = null;
            oldAssetInfos = ParseAssetInfo(File.ReadAllText(oldAssetInfoFilePath));
            foreach (var item in oldAssetInfos)
            {
                var file3d = ClientConstants.AssetBundleDownloadPath + item.Name;


                File.Delete(file3d);
            }
            File.Delete(oldAssetInfoFilePath);

            SBDebug.Log("에셋번들 삭제 완료");
            return;
        }
        SBDebug.Log("에셋번들 삭제 실패");
    }
}