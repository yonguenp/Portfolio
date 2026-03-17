#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
using System;
using UnityEngine;
using Newtonsoft.Json.Linq;

using ProductIdType = System.String;

public class SamandaWebviewMobile : SamandaWebview
{
    public SamandaWebviewMobile(SamandaLauncher owner) => _owner = owner;

    public override bool InitializeWebview(ProductIdType pid, SamandaLauncher.Samanda_Option option)
    {
        if (null != _webViewInstance)
        {
            return false;
        }

        GameObject canvas = GameObject.Find("SAMANDA_CANVAS");            
        if (canvas)
        {
            UnityEngine.Object.DontDestroyOnLoad(canvas);
        }

        _webViewInstance = (new GameObject("WebViewObject")).AddComponent<WebViewObject>();
        _webViewInstance.Init(
            cb: OnJSCallback_,
            httpErr:OnErrorWebviewMobile,
            ld: OnLoaded,
#if UNITY_IOS
            enableWKWebView: true,
#endif
            transparent: true
        );

        string token = SamandaLauncher.GetAccountToken();
        string accountNo = SamandaLauncher.GetAccountNo();

        option |= SamandaLauncher.Samanda_Option.BROWSER_TYPE_PREFIX;
        option &= ~SamandaLauncher.Samanda_Option.BROWSER_TYPE_SUFFIX;

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

        int browserType = 0;
#if UNITY_ANDROID
        browserType = 1;
#elif UNITY_IOS
        browserType = 3;
#endif        
        int orientationType = ((optionflag & (int)SamandaLauncher.Samanda_Option.ORIENTATION_MODE) > 0) ? 1 : 0;

        string url = NetworkManager.SAMANDA_URL;
        string version = GameDataManager.Instance.GetVersion();

        string sdkUrl = url + $"/index?pid={pid}&tok={token}&ano={accountNo}&plf=2&opt={optionflag}&language={languageCode}&browser={browserType}&ort={orientationType}&ver={version}";
        
        _webViewInstance.LoadURL(sdkUrl);

        //_owner.ClearAccountToken();
        OnFullScreen();

        return true;
    }

    public void OnErrorWebviewMobile(string err)
    {
        OnError();
    }

    public void OnLoaded(string json)
    {
        SamandaLauncher.OnSamandaPageLoad();
    }

    public override void SetMargins(int left, int top, int right, int bottom)
    {
        if(null == _webViewInstance)
        {
            throw new Exception("WebView instance not exists.");
        }

        _webViewInstance.SetMargins(left, top, right, bottom);
    }

    public override void SetVisibility(bool visible)
    {
        Debug.Log("[SAMANDA : mobile] SetVisibility : " + visible);
        if (null == _webViewInstance)
        {
            throw new Exception("WebView instance not exists.");
        }

        _webViewInstance.SetVisibility(visible);
    }

    public override bool GetVisibility()
    {
        if(_webViewInstance != null)
        {
            return _webViewInstance.GetVisibility();
        }
        return false;
    }

    public override void OnFullScreen()
    {
        SetVisibility(true);
        SetMargins(0, 0, 0, 0);
    }

    private void OnJSCallback_(string msg)
    {
        MessageJson json = JsonUtility.FromJson<MessageJson>(msg);
        OnJSCallback(json);
    }

    public override void EvaluateJS(string method)
    {
        _webViewInstance.EvaluateJS(method);
    }
}

#endif // #if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR

// EOF
