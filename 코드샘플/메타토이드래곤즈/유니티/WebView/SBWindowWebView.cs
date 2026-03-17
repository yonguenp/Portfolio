using System;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR_WIN || (UNITY_STANDALONE_WIN && !UNITY_EDITOR) || (UNITY_STANDALONE_OSX && !UNITY_EDITOR)
using ZenFulcrum.EmbeddedBrowser;
#endif
/// <summary>
/// 윈도우 버전 웹뷰 세팅 - 에디터용
/// </summary>
namespace SandboxNetwork
{
#if UNITY_EDITOR_WIN || (UNITY_STANDALONE_WIN && !UNITY_EDITOR) || (UNITY_STANDALONE_OSX && !UNITY_EDITOR)
    public class SBWindowWebView : SBWebView, INewWindowHandler
    {
        Browser _webViewInstance;/* = SampleWebView.Instance.webViewObject as GameObject;*/
        GameObject parentObject = null;
        
        Action<string> webViewCallback = null;
        public override bool InitializeWebview(string url, GameObject parent, Action<string> cb = null, Action<string> started = null)
        {
            if (parent == null)
            {
                Debug.LogError("set prefab target parent -- parent is null");
                return false;
            }

            if (targetPrefab == null)
            {
                Debug.LogError("set prefab -- prefab is null");
                return false;
            }

            parentObject = parent;

            GameObject _webViewObject = UnityEngine.Object.Instantiate(targetPrefab, parent.transform);
            _webViewObject.name = "SBWebBrowser";
            _webViewObject.transform.localScale = Vector3.one;

            if (_webViewObject == null)
                return false;

            _webViewInstance = _webViewObject.GetComponent<Browser>();
            _webViewInstance.SetNewWindowHandler(Browser.NewWindowAction.NewBrowser, this);

            _webViewInstance.CustomRegisterFunction("Unity", OnJSCallback);

            _webViewInstance.LoadURL(url, true);
            
            OnFullScreen();
            
            SetMargins(/*0, 0, 0, Screen.height*/);

            webViewCallback = cb;
            _webViewInstance.WhenReady(() => {
                _webViewInstance.Resize(Screen.width, Screen.height);
            });
            return true;
        }
        private void OnJSCallback(JSONNode args)
        {
            try
            {
                if (args.Count <= 0)
                {
                    Debug.LogError("JSCall Empty Data.");
                    return;
                }

                string data = args[0];

                webViewCallback?.Invoke(data);
            }
            catch
            {
                Debug.LogError(args);
            }
        }
        public void SetMargins(/*int left, int top, int right, int bottom*/)
        {
            RectTransform rtf = _webViewInstance.gameObject.GetComponent<RectTransform>();
            var parentSize = parentObject.GetComponent<RectTransform>().rect;
            rtf.sizeDelta = new Vector2(parentSize.width , parentSize.height);

            //Vector2 canvasSize = (GetComponentInParent<Canvas>().transform as RectTransform).sizeDelta;
            //rtf.sizeDelta = new Vector2(canvasSize.x - (right + left), canvasSize.y - (bottom + top));
        }

        public void SetVisibility(bool visible)
        {
            if (null == _webViewInstance)
            {
                throw new Exception("WebView instance not exists.");
            }

            if (_webViewInstance.gameObject != null)
            {
                RawImage rt = _webViewInstance.gameObject.GetComponent<RawImage>();
                if (rt)
                {
                    rt.enabled = visible;
                }
                _webViewInstance.EnableInput = visible;
            }
        }

        public bool GetVisibility()
        {
            if (_webViewInstance != null)
            {
                if (_webViewInstance.gameObject != null)
                {
                    RawImage rt = _webViewInstance.gameObject.GetComponent<RawImage>();
                    if (rt)
                    {
                        return rt.enabled;
                    }
                }
            }
            return false;
        }

        public void OnFullScreen()
        {
            RectTransform rtf = _webViewInstance.gameObject.GetComponent<RectTransform>();

            //device 해상도 변경
            rtf.sizeDelta = new Vector2(Screen.width, Screen.height);
            rtf.localPosition = Vector3.zero;
        }

        public override void EvaluateJS(string method)
        {
            _webViewInstance.EvalJS(method).Done();
        }

        //private void OnJSCallback_(JSONNode args)
        //{
        //    try
        //    {
        //        var sample = args[0];

        //        string data = args[0];
        //        MessageJson json = JsonUtility.FromJson<MessageJson>(data);
        //        OnJSCallback(json);
        //    }
        //    catch
        //    {
        //        Debug.LogError(args);
        //    }
        //}

        public override void CloseBrowser()
        {
            if (_webViewInstance != null)
            {
                _webViewInstance.gameObject.SetActive(false);
                Destroy(_webViewInstance.gameObject);
            }

            _webViewInstance = null;
        }

        public Browser CreateBrowser(Browser parent)
        {
            if (targetPrefab == null)
            {
                Debug.LogError("set prefab -- prefab is null");
                return null;
            }

            GameObject obj = UnityEngine.Object.Instantiate(targetPrefab, _webViewInstance.transform);
            obj.name = "SBWebSubBrowser";
            Button button = obj.GetComponentInChildren<Button>();
            if (button)
            {
                button.onClick.AddListener(() =>
                {
                    GameObject.Destroy(obj);
                });
            }
            RectTransform rtf = _webViewInstance.gameObject.GetComponent<RectTransform>();
            RectTransform new_rtf = obj.gameObject.GetComponent<RectTransform>();

            new_rtf.sizeDelta = rtf.sizeDelta;
            new_rtf.localPosition = rtf.localPosition;

            return obj.GetComponent<Browser>();
        }

        public override void ClearCache()
        {
            //
        }
    }
#else
    public class SBWindowWebView : SBWebView
    {
        public override bool InitializeWebview(string url, GameObject parent, Action<string> cb = null, Action<string> started = null)
        {
            Debug.LogError("error");
            return false;
        }
        public override void EvaluateJS(string method)
        {
            Debug.LogError("error");
        }
        public override void CloseBrowser()
        {
            Debug.LogError("error");
        }
        public override void ClearCache()
        {
            Debug.LogError("error");
        }
    }
#endif //UNITY_EDITOR_WIN || UNITY_EDITOR_OSX || (UNITY_STANDALONE_WIN && !UNITY_EDITOR) || (UNITY_STANDALONE_OSX && !UNITY_EDITOR)
}

// EOF