#if (UNITY_STANDALONE_WIN && !UNITY_EDITOR) || (UNITY_STANDALONE_OSX && !UNITY_EDITOR) || UNITY_EDITOR_WIN || UNITY_EDITOR_OSX
using Newtonsoft.Json.Linq;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using ZenFulcrum.EmbeddedBrowser;

using ProductIdType = System.String;

public sealed class SamandaWebviewDesktop : SamandaWebview, INewWindowHandler
{
    public SamandaWebviewDesktop(SamandaLauncher owner) => _owner = owner;

    public override bool InitializeWebview(ProductIdType pid, SamandaLauncher.Samanda_Option option)
    {
        if (null != _webViewInstance)
        {
            return false;
        }

        GameObject prefabs = Resources.Load("Prefabs/SAMANDA_WIN_BROWSER") as GameObject;
        GameObject webViewObject = null;
        if (prefabs)
        {
            GameObject obj = UnityEngine.Object.Instantiate(prefabs);
            obj.name = "SAMANDA WEBVIEW";
            GameObject canvas = GameObject.Find("SAMANDA_CANVAS");
            
            if (canvas)
            {
                obj.transform.SetParent(canvas.transform);
                webViewObject = obj;

                UnityEngine.Object.DontDestroyOnLoad(canvas);
            }
        }


        if (webViewObject == null)
            return false;

        _webViewInstance = webViewObject.GetComponent<Browser>();
        _webViewInstance.SetNewWindowHandler(Browser.NewWindowAction.NewBrowser, this);

        _webViewInstance.RegisterFunction("WinWebCall", OnJSCallback_);

        string token = SamandaLauncher.GetAccountToken();
        string accountNo = SamandaLauncher.GetAccountNo();

        option |= SamandaLauncher.Samanda_Option.BROWSER_TYPE_PREFIX;
        option |= SamandaLauncher.Samanda_Option.BROWSER_TYPE_SUFFIX;

        int optionflag = (int)option;

        LANGUAGE_TYPE defaultLanguage = LANGUAGE_TYPE.LANGUAGE_KOR;
        switch (Application.systemLanguage)
        {
            case SystemLanguage.Korean:
                defaultLanguage = LANGUAGE_TYPE.LANGUAGE_KOR;
                break;
            case SystemLanguage.English:
            default:
                defaultLanguage = LANGUAGE_TYPE.LANGUAGE_ENG;
                break;
        }
        LANGUAGE_TYPE languageType = (LANGUAGE_TYPE)PlayerPrefs.GetInt("Setting_Language", (int)defaultLanguage);

        SystemLanguage targetLanguage = SystemLanguage.Korean;

        switch (languageType)
        {
            case LANGUAGE_TYPE.LANGUAGE_KOR:
                targetLanguage = SystemLanguage.Korean;
                break;
            case LANGUAGE_TYPE.LANGUAGE_ENG:
            default:
                targetLanguage = SystemLanguage.English;
                break;
        }

        int languageCode = (int)targetLanguage;

        int browserType = 2;
        int orientationType = ((optionflag & (int)SamandaLauncher.Samanda_Option.ORIENTATION_MODE) > 0) ? 1 : 0;

        string url = NetworkManager.SAMANDA_URL;
        string version = GameDataManager.Instance.GetVersion();

        string sdkUrl = url + $"/index?pid={pid}&tok={token}&ano={accountNo}&plf=2&opt={optionflag}&language={languageCode}&browser={browserType}&ort={orientationType}&ver={version}";

        _webViewInstance.LoadURL(sdkUrl, true);
        _webViewInstance.onLoad += (json) => {            
            JToken jobj = JToken.Parse(json.AsJSON);
            if(jobj.Type == JTokenType.Object)
            {
                JObject api = (JObject)jobj;
                if(null != api["status"])
                {
                    int code = api.Value<int>("status");
                    if(200 != code)
                    {
                        OnError();
                    }
                }                
            }
            SamandaLauncher.OnSamandaPageLoad();
        };
        //_owner.ClearAccountToken();
        OnFullScreen();

        return true;
    }

    public override void SetMargins(int left, int top, int right, int bottom)
    {
        RectTransform rtf = _webViewInstance.gameObject.GetComponent<RectTransform>();

        rtf.sizeDelta = new Vector2(Screen.width - (right + left), Screen.height - (bottom + top));
        rtf.localPosition = new Vector2(left, bottom / 2);
    }

    public override void SetVisibility(bool visible)
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

    public override bool GetVisibility()
    {
        if(_webViewInstance != null)
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

    public override void OnFullScreen()
    {
        RectTransform rtf = _webViewInstance.gameObject.GetComponent<RectTransform>();
        RectTransform canvasrtf = GameObject.Find("SAMANDA_CANVAS").GetComponent<RectTransform>();

        rtf.sizeDelta = canvasrtf.sizeDelta;
        rtf.localPosition = Vector3.zero;
    }

    public override void EvaluateJS(string method)
    {
        _webViewInstance.EvalJS(method).Done();
    }

    private void OnJSCallback_(JSONNode args)
    {
        try
        {
            var sample = args[0];

            string data = args[0];
            MessageJson json = JsonUtility.FromJson<MessageJson>(data);
            OnJSCallback(json);
        }
        catch {            
            Debug.LogError(args);
        }
    }

    public void CloseBrowser()
    {

    }

    public Browser CreateBrowser(Browser parent)
    {
        GameObject prefabs = Resources.Load("Prefabs/SAMANDA_WIN_SUBBROWER") as GameObject;

        GameObject obj = UnityEngine.Object.Instantiate(prefabs);
        obj.name = "SAMANDA SUB WEBVIEW";
        obj.transform.SetParent(_webViewInstance.transform);
        Button button = obj.GetComponentInChildren<Button>();
        if(button)
        {
            button.onClick.AddListener(() => {
                GameObject.Destroy(obj);
            });
        }
        RectTransform rtf = _webViewInstance.gameObject.GetComponent<RectTransform>();
        RectTransform new_rtf = obj.gameObject.GetComponent<RectTransform>();

        new_rtf.sizeDelta = rtf.sizeDelta;
        new_rtf.localPosition = rtf.localPosition;

        return obj.GetComponent<Browser>();
    }
}
#endif //UNITY_EDITOR_WIN || UNITY_EDITOR_OSX || (UNITY_STANDALONE_WIN && !UNITY_EDITOR) || (UNITY_STANDALONE_OSX && !UNITY_EDITOR)

// EOF
