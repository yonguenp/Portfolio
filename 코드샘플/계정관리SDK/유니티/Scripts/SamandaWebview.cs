using System;
using UnityEngine;
#if UNITY_EDITOR_WIN || UNITY_EDITOR_OSX || (UNITY_STANDALONE_WIN && !UNITY_EDITOR)
using ZenFulcrum.EmbeddedBrowser;
#endif


using ProductIdType = System.String;

public abstract class SamandaWebview
{
    [System.Serializable]
    public class MessageJson
    {
        public string key;
        public string value;
    }

    // Component of webview
#if UNITY_EDITOR_WIN || UNITY_EDITOR_OSX || (UNITY_STANDALONE_WIN && !UNITY_EDITOR)
    protected Browser _webViewInstance = null;
#elif (UNITY_ANDROID && !UNITY_EDITOR) || (UNITY_IOS && !UNITY_EDITOR)
    protected WebViewObject _webViewInstance = null;
#endif

    protected SamandaLauncher _owner = null;

    public delegate void WebViewCallback(string arg);
    protected WebViewCallback _webViewCallback = (string _) => {};

    protected string _sdkUrl;

    // Dynamic하게 생성
    public abstract bool InitializeWebview(ProductIdType pid, SamandaLauncher.Samanda_Option option);

    // js -> unity callback
    //public virtual void SetCallback(WebViewCallback callback) =>
    //    _webViewCallback = callback;

    // 4방향 margin 설정
    public abstract void SetMargins(int left, int top, int right, int bottom);

    public abstract void OnFullScreen();

    // set visible
    public abstract void SetVisibility(bool visible);

    public abstract bool GetVisibility();    

    public abstract void EvaluateJS(string method);

    public void OnJSCallback(MessageJson msg)
    {
        Debug.Log("On Samanda Command : " + msg.key);

        switch (msg.key)
        {
            case "tok":
                _owner.SetAccountToken(msg.value);
                //Debug.Log("responsed account_token is " + PlayerPrefs.GetString("account_token", "-"));
                break;
            case "ano":
                _owner.SetAccountNo(msg.value);
                //Debug.Log("responsed account_no is " + PlayerPrefs.GetString("account_no", "-"));
                break;
            case "nic":
                Debug.Log(PlayerPrefs.GetString("account_nick"));
                _owner.SetAccountNickName(msg.value);
                //Debug.Log("responsed nick is " + PlayerPrefs.GetString("account_nick", "-"));
                break;
            case "logout":                                
                SamandaLauncher.OnLogoutCall();
                break;
            case "state":
                _owner.SetSamandaState(msg.value);
                break;
            case "url":
                _owner.OnURLOpen(msg.value);
                break;
            case "oAuth":
                _owner.OnReqOAuth(msg.value);
                break;
            case "toast":
                _owner.ShowToast(msg.value);
                break;
            case "screenshot":
                SamandaLauncher.ClickScreenShot();
                break;
            case "chatprofile":
                //_owner.OnChatProfile(msg.value);
                break;
            case "chatroomtype":
                //_owner.OnChatRoom(msg.value);
                break;
            case "chat":
                //_owner.OnChatMessage(msg.value);
                break;
            case "chat_image":
                //_owner.OnChatImage(msg.value);
                break;
            case "jwt":
                _owner.OnJWT(msg.value);
                break;
            case "links":
                _owner.OnLinks(msg.value);
                break;
            case "hide":
                SamandaLauncher.OnHideScreen();
                break;
            case "Termsofuse":
                if (GetVisibility() == false)
                    SetVisibility(true);
                break;
            case "NickSetBox":
                if (GetVisibility() == false)
                    SetVisibility(true);
                break;
            case "auth":
                _owner.OnAuthCommand(msg.value);
                break;
            case "server":
                _owner.OnServerEvent(msg.value);
                break;
            case "cs":
                SamandaLauncher.OpenCustomerSupportPage();
                break;
            case "cid":
                _owner.GoogleClientId = msg.value;
                break;
            case "command":
                _owner.OnSamandaCommand(msg.value);
                break;
        }
    }

    public void OnError(/* need error type?*/)
    {
        _owner.OnNetworkErrorCallback();
    }
}
