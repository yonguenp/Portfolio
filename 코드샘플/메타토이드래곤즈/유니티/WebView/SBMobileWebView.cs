/*
* Copyright (C) 2012 GREE, Inc.
* 
* This software is provided 'as-is', without any express or implied
* warranty.  In no event will the authors be held liable for any damages
* arising from the use of this software.
* 
* Permission is granted to anyone to use this software for any purpose,
* including commercial applications, and to alter it and redistribute it
* freely, subject to the following restrictions:
* 
* 1. The origin of this software must not be misrepresented; you must not
*    claim that you wrote the original software. If you use this software
*    in a product, an acknowledgment in the product documentation would be
*    appreciated but is not required.
* 2. Altered source versions must be plainly marked as such, and must not be
*    misrepresented as being the original software.
* 3. This notice may not be removed or altered from any source distribution.
*/

using System;
using System.Collections;
using UnityEngine;
#if UNITY_2018_4_OR_NEWER
using UnityEngine.Networking;
#endif
using UnityEngine.UI;

/// <summary>
/// 모바일 버전 웹뷰 세팅
/// </summary>
namespace SandboxNetwork
{
    public class SBMobileWebView : SBWebView
    {
        string Url;
        WebViewObject webViewObject = null;
        Vector2[] spos = null;
        public override bool InitializeWebview(string url, GameObject parent, Action<string> cb = null, Action<string> started_cb = null)
        {
            if(parent == null)
            {
                Debug.LogError("set prefab target parent -- parent is null");
                return false;
            }

            if(targetPrefab == null)
            {
                Debug.LogError("set prefab -- prefab is null");
                return false;
            }
            if (webViewObject == null)
            {
                GameObject _webViewObject = UnityEngine.Object.Instantiate(targetPrefab, parent.transform);
                _webViewObject.name = "SBWebBrowser";
                _webViewObject.transform.localScale = Vector3.one;

                if (_webViewObject == null)
                    return false;

                webViewObject = _webViewObject.GetComponent<WebViewObject>();
            }
            
            webViewObject.Init(cb,
#if UNITY_IOS
                enableWKWebView: true,
#endif
                transparent: true,
                zoom : false,
                started: started_cb,
                httpErr: OnError
            );

            SetMargins(parent);

            ActiveWebView(url);

            return true;
        }

        public void ActiveWebView(string url)
        {
            Debug.Log("url : " + url);

            webViewObject.LoadURL(url);
            webViewObject.SetVisibility(true);
        }

        public void SetMargins(GameObject _targetParent)
        {
            if(webViewObject == null)
            {
                Debug.LogError("webviewObject is null");
                return;
            }

            if (_targetParent == null)
            {
                Debug.LogError("targetParent is null");
                return;
            }

            const int canvas_width = 2560;
            const int canvas_height = 1440;

            var parentRect = _targetParent.GetComponent<RectTransform>();
            float Left = parentRect.offsetMin.x;
            float Bottom = parentRect.offsetMin.y;
            float Right = parentRect.offsetMax.x;
            float Top = parentRect.offsetMax.y;

            int[] margins = new int[4];
            Vector3[] corners = new Vector3[4];
            (transform as RectTransform).GetWorldCorners(corners);

            if (spos == null)
            {
                var cameraComp = UICanvas.Instance.GetCamera();

                spos = new Vector2[4];
                spos[0] = cameraComp.WorldToScreenPoint(corners[0]); /*하단*/
                spos[1] = cameraComp.WorldToScreenPoint(corners[1]); /*왼쪽 위*/
                spos[2] = cameraComp.WorldToScreenPoint(corners[2]); /*오른쪽 위*/
                spos[3] = cameraComp.WorldToScreenPoint(corners[3]); /*우하*/
            }

            Debug.Log("Screen.width : " + Screen.width + "Screen.with : " + Screen.height);
            Debug.Log("spos 0 : " + spos[0] + " spos 1 : " + spos[1] + " spos 2 : " + spos[2] + " spos 3 : " + spos[3]);

            /*  주의사항: Right, Top의 경우는 Unity Inspector에서는 양수값으로 표현되지만 실제로는 음수값이 양수로 보여짐.
                Left, Bottom의 경우에는 "Screen.width - 원하는 크기값"이고 Right, Top은 그 반대임.*/

            float modifyLeft = Left * Screen.width / canvas_width;
            float modifyRight = Right * Screen.width / canvas_width;
            float modifyTop = Top * Screen.height / canvas_height;
            float modifyBottom = Bottom * Screen.height / canvas_height;

            margins[0] = (int)spos[0].x + (int)modifyLeft;
            margins[1] = (int)(Screen.height - spos[2].y - modifyTop);
            margins[2] = (int)(Screen.width - spos[2].x - modifyRight);
            margins[3] = (int)spos[0].y + (int)modifyBottom;

            Debug.Log("margins 0 : " + margins[0] + " margins 1 : " + margins[1] + " margins 2 : " + margins[2] + " margins 3 : " + margins[3]);

            webViewObject.SetMargins(margins[0], margins[1], margins[2], margins[3]);
        }

        public IEnumerator Init()
        {
            //webViewObject = (new GameObject("WebViewObject")).AddComponent<WebViewObject>();
            webViewObject.Init(
                cb: (msg) =>
                {
                    Debug.Log(string.Format("CallFromJS[{0}]", msg));
                //status.text = msg;
                //status.GetComponent<Animation>().Play();
            },
                err: (msg) =>
                {
                    Debug.Log(string.Format("CallOnError[{0}]", msg));
                //status.text = msg;
                //status.GetComponent<Animation>().Play();
            },
                httpErr: (msg) =>
                {
                    Debug.Log(string.Format("CallOnHttpError[{0}]", msg));
                //status.text = msg;
                //status.GetComponent<Animation>().Play();
            },
                started: (msg) =>
                {
                    Debug.Log(string.Format("CallOnStarted[{0}]", msg));
                },
                hooked: (msg) =>
                {
                    Debug.Log(string.Format("CallOnHooked[{0}]", msg));
                },
                ld: (msg) =>
                {
                    Debug.Log(string.Format("CallOnLoaded[{0}]", msg));
#if UNITY_EDITOR_OSX || (!UNITY_ANDROID && !UNITY_WEBPLAYER && !UNITY_WEBGL)
                // NOTE: depending on the situation, you might prefer
                // the 'iframe' approach.
                // cf. https://github.com/gree/unity-webview/issues/189
#if true
                webViewObject.EvaluateJS(@"
                  if (window && window.webkit && window.webkit.messageHandlers && window.webkit.messageHandlers.unityControl) {
                    window.Unity = {
                      call: function(msg) {
                        window.webkit.messageHandlers.unityControl.postMessage(msg);
                      }
                    }
                  } else {
                    window.Unity = {
                      call: function(msg) {
                        window.location = 'unity:' + msg;
                      }
                    }
                  }
                ");
#else
                webViewObject.EvaluateJS(@"
                  if (window && window.webkit && window.webkit.messageHandlers && window.webkit.messageHandlers.unityControl) {
                    window.Unity = {
                      call: function(msg) {
                        window.webkit.messageHandlers.unityControl.postMessage(msg);
                      }
                    }
                  } else {
                    window.Unity = {
                      call: function(msg) {
                        var iframe = document.createElement('IFRAME');
                        iframe.setAttribute('src', 'unity:' + msg);
                        document.documentElement.appendChild(iframe);
                        iframe.parentNode.removeChild(iframe);
                        iframe = null;
                      }
                    }
                  }
                ");
#endif
#elif UNITY_WEBPLAYER || UNITY_WEBGL
                webViewObject.EvaluateJS(
                    "window.Unity = {" +
                    "   call:function(msg) {" +
                    "       parent.unityWebView.sendMessage('WebViewObject', msg)" +
                    "   }" +
                    "};");
#endif
                webViewObject.EvaluateJS(@"Unity.call('ua=' + navigator.userAgent)");
                }
                //transparent: false,
                //zoom: true,
                //ua: "custom user agent string",
                //// android
                //androidForceDarkMode: 0,  // 0: follow system setting, 1: force dark off, 2: force dark on
                //// ios
                //enableWKWebView: true,
                //wkContentMode: 0,  // 0: recommended, 1: mobile, 2: desktop
                //wkAllowsLinkPreview: true,
                //// editor
                //separated: false
                );
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
        webViewObject.bitmapRefreshCycle = 1;
#endif
            // cf. https://github.com/gree/unity-webview/pull/512
            // Added alertDialogEnabled flag to enable/disable alert/confirm/prompt dialogs. by KojiNakamaru · Pull Request #512 · gree/unity-webview
            //webViewObject.SetAlertDialogEnabled(false);

            // cf. https://github.com/gree/unity-webview/pull/728
            //webViewObject.SetCameraAccess(true);
            //webViewObject.SetMicrophoneAccess(true);

            // cf. https://github.com/gree/unity-webview/pull/550
            // introduced SetURLPattern(..., hookPattern). by KojiNakamaru · Pull Request #550 · gree/unity-webview
            //webViewObject.SetURLPattern("", "^https://.*youtube.com", "^https://.*google.com");

            // cf. https://github.com/gree/unity-webview/pull/570
            // Add BASIC authentication feature (Android and iOS with WKWebView only) by takeh1k0 · Pull Request #570 · gree/unity-webview
            //webViewObject.SetBasicAuthInfo("id", "password");

            //webViewObject.SetScrollbarsVisibility(true);

            //webViewObject.SetMargins(5, 100, 5, Screen.height / 4);
            //webViewObject.SetTextZoom(100);  // android only. cf. https://stackoverflow.com/questions/21647641/android-webview-set-font-size-system-default/47017410#47017410
            webViewObject.SetVisibility(true);

#if !UNITY_WEBPLAYER && !UNITY_WEBGL
            if (Url.StartsWith("http"))
            {
                webViewObject.LoadURL(Url.Replace(" ", "%20"));
            }
            else
            {
                var exts = new string[]{
                ".jpg",
                ".js",
                ".html"  // should be last
            };
                foreach (var ext in exts)
                {
                    var url = Url.Replace(".html", ext);
                    var src = System.IO.Path.Combine(Application.streamingAssetsPath, url);
                    var dst = System.IO.Path.Combine(Application.persistentDataPath, url);
                    byte[] result = null;
                    if (src.Contains("://"))
                    {  // for Android
#if UNITY_2018_4_OR_NEWER
                        // NOTE: a more complete code that utilizes UnityWebRequest can be found in https://github.com/gree/unity-webview/commit/2a07e82f760a8495aa3a77a23453f384869caba7#diff-4379160fa4c2a287f414c07eb10ee36d
                        var unityWebRequest = UnityWebRequest.Get(src);
                        yield return unityWebRequest.SendWebRequest();
                        result = unityWebRequest.downloadHandler.data;
#else
                    var www = new WWW(src);
                    yield return www;
                    result = www.bytes;
#endif
                    }
                    else
                    {
                        result = System.IO.File.ReadAllBytes(src);
                    }
                    System.IO.File.WriteAllBytes(dst, result);
                    if (ext == ".html")
                    {
                        webViewObject.LoadURL("file://" + dst.Replace(" ", "%20"));
                        break;
                    }
                }
            }
#else
        if (Url.StartsWith("http")) {
            webViewObject.LoadURL(Url.Replace(" ", "%20"));
        } else {
            webViewObject.LoadURL("StreamingAssets/" + Url.Replace(" ", "%20"));
        }
#endif
            yield break;
        }

        public override void EvaluateJS(string method)
        {
            webViewObject.EvaluateJS(method);
        }

        public override void CloseBrowser()
        {
            if (webViewObject != null)
            {
                webViewObject.gameObject.SetActive(false);
                Destroy(webViewObject.gameObject);
            }

            webViewObject = null;
        }

        public void OnError(string err)
        {
            Debug.LogError(err);
        }

        public override void ClearCache()
        {
            webViewObject.ClearCache(true);
        }
    }
}
