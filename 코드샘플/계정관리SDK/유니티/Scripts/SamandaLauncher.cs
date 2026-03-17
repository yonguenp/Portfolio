using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements;
using System.IO;
#if UNITY_IOS
using UnityEngine.SignInWithApple;
#endif
using System.Runtime.InteropServices;
using AOT;

// Alias
using ProductIdType = System.String;
using UnityEngine.Events;

[System.Serializable]
public class MessageJson
{
    public string key;
    public string value;
}
[System.Serializable]
public class ArrayJson
{
    public int[] array;
}
[System.Serializable]
public class oAuthResJson
{
    public int rs;
    public string type;
    public string token;
}

public class SamandaLauncher
{
    public enum Samanda_State { 
        NONE,
        INIT,        
        NEED_LOGIN,
        PASS_LOGIN,
        
        BANNER,
        MAIN,
        HIDE,

        CHAT_LEFT,
        CHAT_RIGHT,

        CHAT_CUSTOM,
    };

    public enum Samanda_Shorcut
    {
        PAGE_HOME,
        PAGE_NOTICE,
        PAGE_CHAT,
        PAGE_OVERLAY_CHAT,
        SCREENSHOT_SEND,
    };

    public enum Samanda_ScreenMode
    {
        NONE,
        CHAT_LEFT,
        CHAT_RIGHT,
    };

    public enum Samanda_Option
    { 
        NONE_OPTION = 0,
        
        ENABLE_CHAT         = 1 << 0,       // 1 Chat 1 : Use 0 : Not use
        ORIENTATION_MODE    = 1 << 1,       // 2 Orientation 1 : Landscape, 0 : Portrait
        BROWSER_TYPE_PREFIX = 1 << 2,       // 4 Browser Type 0 : STANDALONE, 1 : APP BASE
        BROWSER_TYPE_SUFFIX = 1 << 3,       // 8 Browser Type 0 : MOBILE, 1 : ZFB(PC)
        USE_CUSTOM_OVERLAY  = 1 << 4,       // 16 CUSTOM OVERLAY CHAT

        FULL_OPTION = int.MaxValue
    };

    public enum AccountType
    {
        SB, GG, FB, NV, KK, AP, IG, GUEST
    }

    enum Auth_Type
    {
        SB = 0, NV, GG, KK, IG, FB, AP, GE, 
    };

    

    private static SamandaLauncher instance = null;

    public static bool UseSamanda() {
        return true;
    }

    public delegate void Callback();
    public delegate void CommandCallback(string command);

    private Callback callbackJWT = null;
    private Callback callbackNetworkFail = null;
    private Callback callbackServerMaintenance = null;
    private Callback callbackLogInPass = null;
    private Callback callbackSplashDone = null;
    private Callback callbackPrepareDone = null;
    private Callback callbackLogOut = null;
    private Callback callbackSamandaHide = null;
    private CommandCallback callbackCommand = null;
    private Callback makeSamandaButtonFunction = null;

    //private WebViewObject webViewObject = null;
    private SamandaWebview _webView = null;

    private ProductIdType productId;
    private Samanda_Option optionFlag;
    private bool bRecivedToken = false;
    private bool bRecivedAccountNO = false;

    public static GameObject ButtonSamanda = null;
    public GameObject SamandaSceenCapturePrefab = null;

    private Samanda_State cur_state = Samanda_State.NONE;
    private Rect WebViewRect = new Rect(0, 0, 0, 0);
    private SystemLanguage curLanguage = SystemLanguage.English;
    private List<int> AccountLinks = new List<int>();

    // Sign in with Apple handler
#if UNITY_IOS && !UNITY_EDITOR
    private GameObject mSignInWithAppleObject = null;
#endif

    private string strCustomerUrl;
    private bool isLoadDone = false;
    public string GoogleClientId { get; set; }

    public static string CustomerServiceURL
    {
        get { return Instance.strCustomerUrl; }
        set { Instance.strCustomerUrl = value; }
    }

    // keyboard visibility event listener
    private UnityEvent m_onKeyboardVisibility = new UnityEvent();
    public static UnityEvent onKeyboardVisible
    {
        get { return Instance.m_onKeyboardVisibility; }
    }

    public static ProductIdType GetPID() { return Instance != null ? Instance.productId : "-"; }
    public static bool Initialize(ProductIdType pid,
        string clientId,
        Samanda_Option option = Samanda_Option.FULL_OPTION,
        SystemLanguage language = SystemLanguage.English)
    {
        SamandaLauncher self = SamandaLauncher.Instance;
        if (null == self)
        {
            return false;
        }

        self.curLanguage = language;
        self.productId = pid;
        self.optionFlag = option;
        self.GoogleClientId = clientId;

        self.makeSamandaButtonFunction = () => {
            if (ButtonSamanda == null)
            {
                Debug.Log("ButtonSamanda Init #1");
                GameObject prefabs = Resources.Load("Prefabs/SAMANDA") as GameObject;

                if (prefabs)
                {
                    Debug.Log("ButtonSamanda Init #2");
                    ButtonSamanda = UnityEngine.Object.Instantiate(prefabs);
                    ButtonSamanda.name = "SAMANDA BUTTON";

                    if ((instance.optionFlag & Samanda_Option.ENABLE_CHAT) == 0)
                    {
                        Debug.Log("ButtonSamanda Init #3");
                        Transform shortcut = ButtonSamanda.transform.Find("Shortcut");
                        shortcut.Find("ChatBtn").gameObject.SetActive(false);
                        shortcut.Find("OverlayChatBtn").gameObject.SetActive(false);
                        shortcut.Find("ScreenShotBtn").gameObject.SetActive(false);
                    }

                    Debug.Log("ButtonSamanda Init #4");
                    GameObject canvas = GameObject.Find("SAMANDA_CANVAS");
                    if (canvas)
                    {
                        Debug.Log("ButtonSamanda Init #5");
                        ButtonSamanda.transform.SetParent(canvas.transform);
                        ButtonSamanda.transform.localScale = Vector3.one * 0.5f;
                        ButtonSamanda.SetActive(false);

                        ButtonSamanda.transform.localPosition = new Vector2(Screen.width / 2, Screen.height / 2) * -1.0f + new Vector2(50, 50);
                        Debug.Log("Added Samanda Button!");
                    }
                }
            }
        };

#if UNITY_IOS && !UNITY_EDITOR
        //Debug.Log("Calling SamandaNativeInit");
        SamandaNativeInit();
#endif

        return true;
    }

    private static SamandaLauncher Instance
    {
        get
        {
            if (null == instance)
            {
                instance = new SamandaLauncher();
            }
            return instance;
        }
    }

    void Start()
    {
        
    }

    void OnDestroy()
    {
        if (ButtonSamanda)
            ButtonSamanda.GetComponent<SamandaButton>().SamandaDestroy();
    }
    
    public static void SetLanguage(SystemLanguage language)
    {
        if (null == instance)
        {
            return;
        }

        int languageCode = (int)language;
        instance._webView.EvaluateJS($"window.Samanda.setLanguageCode('{languageCode}')");

        instance.curLanguage = language;
    }

    public static SystemLanguage GetLanguage()
    {
        if (null == instance)
        {
            return SystemLanguage.English;
        }

        return instance.curLanguage;
    }

    public static bool StartSamanda(Callback cb = null)
    {
        if(!SamandaLauncher.UseSamanda())
        {
            if(cb != null)
                cb();

            return false;
        }

        if (null == instance) {
            // TODO:: throw exception
            return false;
        }

        if (cb != null)
            instance.callbackSplashDone = cb;

        if (null != instance._webView)
        {
            instance._webView.EvaluateJS("cc.director.loadScene(\"EmptyScene.firers\")");
            return false;
        }

        //if (instance.webViewObject != null)
        //    return false;

#if UNITY_EDITOR_WIN || UNITY_EDITOR_OSX || (UNITY_STANDALONE_WIN && !UNITY_EDITOR)
        instance._webView = new SamandaWebviewDesktop(instance);
#elif (UNITY_ANDROID && !UNITY_EDITOR) || (UNITY_IOS && !UNITY_EDITOR)
        instance._webView = new SamandaWebviewMobile(instance);
#endif

        return instance._webView.InitializeWebview(instance.productId, instance.optionFlag);
    }

    public void OnURLOpen(string url)
    {
        Application.OpenURL(url);
    }

    public void SetSamandaState(string str_state)
    {
        Samanda_State state = Samanda_State.NONE;
        switch (str_state)
        {
            case "splash_done":
                if (callbackSplashDone != null)
                {
                    Callback tmp = callbackSplashDone;
                    callbackSplashDone = null;
                    tmp.Invoke();
                }
                isLoadDone = true;
                break;
            case "init":
                state = Samanda_State.INIT;
                break;
            case "need_login":
                state = Samanda_State.NEED_LOGIN;
                break;
            case "pass_login":
                state = Samanda_State.PASS_LOGIN;
                break;
            case "banner":
                state = Samanda_State.BANNER;
                break;
            case "main":
                state = Samanda_State.MAIN;
                break;
            case "hide":
                state = Samanda_State.HIDE;
                break;
            case "chat_left":
                state = Samanda_State.CHAT_LEFT;
                break;
            case "chat_right":
                state = Samanda_State.CHAT_RIGHT;
                break;
            case "chat_custom":
                state = Samanda_State.CHAT_CUSTOM;
                break;
        }

        SetSamandaState(state);
    }

    public void OnReqOAuth(string value)
    {
        Debug.Log("OnReqOAuth");
        if (Int32.TryParse(value, out int type)) {
            Auth_Type eType = (Auth_Type)type;
            switch (eType)
            {
                case Auth_Type.GG:
#if UNITY_ANDROID && !UNITY_EDITOR
                    {
                        // Google sign in handler
                        string pkg = "com.sandbox.gs.samanda";
                        string cls = "GoogleSignInHandlerActivity";
                        AndroidJavaObject samandaSignInHandler = new AndroidJavaObject($"{pkg}.{cls}");

                        // UnityPlayer class
                        Debug.Log("Calling onReqoAuth");

                        AndroidJavaClass UnityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                        AndroidJavaObject currentActivity = UnityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

                        // call java method
                        samandaSignInHandler.CallStatic("setClientId", GoogleClientId);
                        samandaSignInHandler.CallStatic("onReqOAuth", currentActivity);
                        return;
                    }
#elif UNITY_IOS && !UNITY_EDITOR
                    {
                        //UnityStartGoogleSignInIOS();
                        SamandaNativeoAuthGoogle(SamandaUnityoAuthCallback);
                        return;
                    }
#elif UNITY_EDITOR_WIN || UNITY_EDITOR_OSX || (UNITY_STANDALONE_WIN && !UNITY_EDITOR) || (UNITY_STANDALONE_OSX && !UNITY_EDITOR)
                    {
                        OAuthApp.MainWindow win = new OAuthApp.MainWindow();
                        win.button_Click();
                        return;
                    }
#else
                    Debug.Log("Not in android, but it called with GG");
#endif
                    break;

                case Auth_Type.AP:
                    {
#if UNITY_IOS && !UNITY_EDITOR
                        //Debug.Log("Apple oAuth request. unity -> iOS");
                        //SamandaNativeoAuthApple(SamandaUnityoAuthCallback);
                        StartSignInWithApple();
                        return;
#endif
                    }
                    break;

                default:
                    Debug.Log("Unswitched oAuth type: " + type);
                    break;
            } // switch (eType)
        } else {
            Debug.Log("Cannot parse oAuth type from string " + value);
        }

        _webView.EvaluateJS("window.Samanda.onOAuthJavaResponse('{\"rs\":1}')");
    }
    
#if UNITY_IOS && !UNITY_EDITOR
    SignInWithApple GetSignInWithApple()
    {
        if (null == mSignInWithAppleObject)
        {
            mSignInWithAppleObject = new GameObject();
            mSignInWithAppleObject.AddComponent<SignInWithApple>();
        }

        return mSignInWithAppleObject.GetComponent<SignInWithApple>();
    }

    private void StartSignInWithApple()
    {
        var siwa = GetSignInWithApple();
        siwa.Login(OnSignInWithAppleLogin);
    }

    public void OnSignInWithAppleLogin(SignInWithApple.CallbackArgs args)
    {
        if (args.error == null)
        {
            Debug.Log("SignInWithApple Succeeded");
            SendSignInWithAppleResult(0, args.userInfo.idToken);
        }
        else
        { 
            Debug.Log("SignInWithApple Failed with : " + args.error);
            SendSignInWithAppleResult(1, "");
        }
    }

    private void SendSignInWithAppleResult(int error, string token)
    {
        string param = "window.Samanda.onOAuthJavaResponse('{\"rs\":"
            + error + ",\"type\":\"AP\",\"token\":\"" + token + "\"}')";
        //Debug.Log("param: " + param);
        _webView.EvaluateJS(param);
    }
#endif

    static public void EvaluteJS(string command)
    {
        Debug.Log(command);
        instance._webView.EvaluateJS(command);
    }

    public void ShowToast(string message)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject unityAct = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

        if (null != unityAct)
        {
            AndroidJavaClass toastClass = new AndroidJavaClass("android.widget.Toast");
            unityAct.Call("runOnUiThread", new AndroidJavaRunnable(() =>
            {
                AndroidJavaObject toast = toastClass.CallStatic<AndroidJavaObject>("makeText", unityAct, message, 0);
                toast.Call("show");
            }));
        }
#endif
    }

    public void SetSamandaState(Samanda_State state)
    {
        Debug.Log("[SAMANDA] SetSamandaState : " + state.ToString());

        if (cur_state == state)
        {
            if (Samanda_State.HIDE == state)
            {
                OnHideScreen();
            }
            return;
        }

        switch(state)
        {
            case Samanda_State.INIT:
                OnFullScreen();
                break;
            case Samanda_State.NEED_LOGIN:
                OnHideScreen();
                break;
            case Samanda_State.PASS_LOGIN:
                OnHideScreen();
                break;
            case Samanda_State.BANNER:
                if (cur_state == Samanda_State.NEED_LOGIN)
                {
                    if(callbackLogInPass != null)
                    {
                        Callback tmp = callbackLogInPass;
                        callbackLogInPass = null;
                        tmp.Invoke();
                    }
                }                
                break;
            case Samanda_State.MAIN:
                break;
            case Samanda_State.HIDE:
                if (callbackPrepareDone != null)
                {
                    Callback tmp = callbackPrepareDone;
                    callbackPrepareDone = null;
                    tmp.Invoke();
                }

                OnHideScreen();
                break;
            case Samanda_State.CHAT_RIGHT:
                OnScreenModeChange(Samanda_ScreenMode.CHAT_RIGHT);
                break;
            case Samanda_State.CHAT_LEFT:
                OnScreenModeChange(Samanda_ScreenMode.CHAT_LEFT);
                break;
            case Samanda_State.CHAT_CUSTOM:
                
                break;
        }

        cur_state = state;
    }

    public static void OnSamandaButton()
    {
        Instance.cur_state = Samanda_State.MAIN;
        OnFullScreen();
    }

    public static bool OnBackKeyPressed()
    {
        if (null == instance || null == instance._webView || false == instance._webView.GetVisibility())
        {
            return true;
        }

        instance._webView.EvaluateJS("window.Samanda.onBackKeyPressed()");

        return false;
    }

    public void SetAccountToken(string value)
    {
        bRecivedToken = true;
        PlayerPrefs.SetString("account_token", value);
    }

    public void SetAccountNo(string value)
    {
        bRecivedAccountNO = true;
        PlayerPrefs.SetString("account_no", value);
    }

    public void SetAccountNickName(string value)
    {
        PlayerPrefs.SetString("account_nick", value);
    }

    public static string GetAccountToken()
    {
        return PlayerPrefs.GetString("account_token", "-");
    }

    public static string GetAccountNo()
    {
        return PlayerPrefs.GetString("account_no", "-");
    }

    public static string GetAccountNickName()
    {
        return PlayerPrefs.GetString("account_nick", "-");
    }

    public void ClearAccountToken()
    {
        bRecivedToken = false;
        bRecivedAccountNO = false;
        PlayerPrefs.DeleteKey("account_token");
        PlayerPrefs.DeleteKey("account_no");
        PlayerPrefs.DeleteKey("account_nick");
        PlayerPrefs.DeleteKey("json_web_token");
    }

    public void ClearSamandaUI()
    {
        if(ButtonSamanda != null)
            UnityEngine.Object.Destroy(ButtonSamanda);
        
        ButtonSamanda = null;        
    }

    public static void OnAccountCheckAndPlay(Callback pass_callback, Callback logoutCallback = null)
    {
        if (null == instance) {
            if(!SamandaLauncher.UseSamanda())
                pass_callback();

            return;
        }
        Debug.Log("[SAMANDA] OnAccountCheckAndPlay : " + instance.cur_state.ToString());


        if (instance.cur_state == Samanda_State.NEED_LOGIN)
        {
            LANGUAGE_TYPE defaultLanguage = LANGUAGE_TYPE.LANGUAGE_KOR;
            switch (Application.systemLanguage)
            {
                case SystemLanguage.Korean:
                    defaultLanguage = LANGUAGE_TYPE.LANGUAGE_KOR;
                    break;
                case SystemLanguage.Indonesian:
                    defaultLanguage = LANGUAGE_TYPE.LANGUAGE_IND;
                    break;
                case SystemLanguage.Japanese:
                    defaultLanguage = LANGUAGE_TYPE.LANGUAGE_JPN;
                    break;
                case SystemLanguage.English:
                default:
                    defaultLanguage = LANGUAGE_TYPE.LANGUAGE_ENG;
                    break;
            }
            LANGUAGE_TYPE languageType = (LANGUAGE_TYPE)PlayerPrefs.GetInt("Setting_Language", (int)defaultLanguage);

            instance.ClearAccountToken();
            Debug.Log("PlayerPrefs.DeleteAll Samanda Launcher Line :: 642");

            PlayerPrefs.SetInt("Setting_Language", (int)languageType);

            instance.callbackLogInPass = pass_callback;            
            OnFullScreen();
            instance._webView.EvaluateJS("window.Samanda.onTryLogin()");
        }
        else
        {
            pass_callback();
        }

        if(logoutCallback != null)
        {
            SetLogoutCallback(logoutCallback);
        }
    }

    public void InvokeJSFunction(string functionCode)
    {

    }

    public static void OnSamandaShortcut(Samanda_Shorcut type = Samanda_Shorcut.PAGE_HOME, bool ScreenControl = true)
    {
        if (instance != null)
            instance._webView.EvaluateJS("window.Samanda.onSamandaShortcut(" + ((int)type).ToString() + ")");

        switch (type)
        {
            case Samanda_Shorcut.PAGE_HOME:
            case Samanda_Shorcut.PAGE_NOTICE:
            case Samanda_Shorcut.PAGE_CHAT:
                if (instance != null)
                {
                    Instance.SetSamandaState(Samanda_State.MAIN);
                }

                if(ScreenControl)
                    OnFullScreen();
                break;
            case Samanda_Shorcut.PAGE_OVERLAY_CHAT:
                if (ButtonSamanda)
                    ButtonSamanda.SetActive(false);
                //if (instance != null)
                //    Instance.SetSamandaState(Samanda_State.CHAT_LEFT);
                break;
            case Samanda_Shorcut.SCREENSHOT_SEND:
                ClickScreenShot();
                break;
        }
    }
    
    public static void OnGamePlayBeforeBannerAndNotice(Callback callback = null)
    {
        Debug.Log("ButtonSamanda Init #0");
        if (null == instance) {
            return;
        }

        instance.makeSamandaButtonFunction?.Invoke();

        instance.callbackPrepareDone = callback;
        OnFullScreen();
    }

    public static void OnFullScreen()
    {
        Debug.Log("[SAMANDA] OnFullScreen");
        if (null == instance) {
            Debug.Log("[SAMANDA] Error : instance or WebviewObject is NULL!!");
            return;
        }

        if (null != instance._webView)
        {
            instance._webView.SetVisibility(true);
            instance._webView.SetMargins((int)instance.WebViewRect.x, (int)instance.WebViewRect.height, (int)instance.WebViewRect.width, (int)instance.WebViewRect.y);

            if (ButtonSamanda != null)
                ButtonSamanda.SetActive(false);
        }
    }
    public static void OnHideScreen()
    {
        Debug.Log("[SAMANDA] OnHideScreen");
        if (null == instance) {
            Debug.Log("[SAMANDA] Error : instance or WebviewObject is NULL!!");
            return;
        }

        if (null != instance._webView)
        {
            instance._webView.SetVisibility(false);

            if (ButtonSamanda != null && !ButtonSamanda.activeSelf)
            {
                ButtonSamanda.SetActive(true);
            }
        }

        instance.callbackSamandaHide?.Invoke();
    }
    public static void OnScreenModeChange(Samanda_ScreenMode mode)
    {
        Debug.Log("[SAMANDA] OnChatScreenMode : " + Screen.width + "X" + Screen.height);
        if (null == instance || instance._webView == null) {
            Debug.Log("[SAMANDA] Error : instance or WebviewObject is NULL!!");
            return;
        }

        instance._webView.SetVisibility(true);
        switch(mode)
        {
            case Samanda_ScreenMode.NONE:
                instance._webView.SetMargins((int)instance.WebViewRect.x, (int)instance.WebViewRect.height, (int)instance.WebViewRect.width, (int)instance.WebViewRect.y);
                break;
            case Samanda_ScreenMode.CHAT_LEFT:
                instance._webView.SetMargins(0, 0, (int)(Screen.width - (467 * ((float)Screen.width / 1280))), 0);
                break;
            case Samanda_ScreenMode.CHAT_RIGHT:
                instance._webView.SetMargins((int)(Screen.width - (467 * ((float)Screen.width / 1280))), 0, 0, 0);
                break;
        }
    }

    public static void ClickScreenShot()
    {
        if (instance != null && instance.SamandaSceenCapturePrefab)
        {
            GameObject obj = UnityEngine.Object.Instantiate(instance.SamandaSceenCapturePrefab);
            obj.name = "SAMANDA CAPTURE";
            GameObject canvas = GameObject.Find("SAMANDA_CANVAS");
            if (canvas)
            {
                obj.transform.SetParent(canvas.transform);
                RectTransform canvasRectTransform = canvas.transform.GetComponent<RectTransform>();
                RectTransform objRectTransform = obj.transform.GetComponent<RectTransform>();


                objRectTransform.localScale = Vector3.one;
                objRectTransform.localPosition = Vector3.zero;
                objRectTransform.sizeDelta = canvasRectTransform.sizeDelta;
            }

            SamandaCapture capture = obj.GetComponent<SamandaCapture>();
            capture.OnCaptureCheck(instance);
        }

        if(ButtonSamanda)
        {
            Transform shortcutTransform = ButtonSamanda.transform.Find("Shortcut");
            if (shortcutTransform)
            {
                Transform ScreenShotBtnTransform = shortcutTransform.Find("ScreenShotBtn");
                if (ScreenShotBtnTransform)
                {
                    ScreenShotDelay delayComponent = ScreenShotBtnTransform.GetComponent<ScreenShotDelay>();
                    if (delayComponent)
                    {
                        delayComponent.OnShotEvent();
                    }
                }
            }
        }
    }

    public void OnSendScreenCapture(string b64, string b64Thumb)
    {
        // toast
        if (null != instance && null != instance._webView)
        {
            instance.ShowToast("스크린샷을 채팅으로 전송합니다.");
            instance._webView.EvaluateJS($"window.Samanda.onScreenshotCaptured('{b64}','{b64Thumb}');");
        }
        else
        {
            //팝업 
        }
    }

    public static void OnLogout()
    {
        if (instance != null)
        {
            instance.ClearAccountToken();
            instance.ClearSamandaUI();
            instance.cur_state = Samanda_State.NEED_LOGIN;

            GoogleAuthLogOut();

            instance._webView.EvaluateJS("location.reload()");
        }
    }

    public static void OnLogoutCall()
    {
        if (instance == null)
            return;

        instance.SetSamandaState(SamandaLauncher.Samanda_State.NEED_LOGIN);
        OnLogout();
        instance.callbackLogOut?.Invoke();
    }

    public static void GoogleAuthLogOut()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        // Google sign in handler
        string pkg = "com.sandbox.gs.samanda";
        string cls = "GoogleSignInHandlerActivity";
        AndroidJavaObject samandaSignInHandler = new AndroidJavaObject($"{pkg}.{cls}");

        // UnityPlayer class
        Debug.Log("Calling onReqSignOut");
        AndroidJavaClass UnityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject currentActivity = UnityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

        // call java method
        samandaSignInHandler.CallStatic("setClientId", instance.GoogleClientId);
        samandaSignInHandler.CallStatic("onReqSignOut", currentActivity);
#endif
    }

    public static void SetSamandaButtonMakeFunction(Callback callback)
    {
        if (instance != null)
            instance.makeSamandaButtonFunction = callback;
    }

    public static void ResizeWebViewRect(Rect rect)
    {
        if (instance != null)
        {            
            instance.WebViewRect = rect;
        }
    }

    public static Samanda_State GetState()
    {
        if(instance != null)
            return instance.cur_state;
        
        return Samanda_State.NONE;
    }

    public static void OnJWTCheck(Callback callback)
    {
        if (instance != null)
        {
            PlayerPrefs.DeleteKey("json_web_token");
            instance._webView.EvaluateJS("window.Samanda.RequestJWT()");
            instance.callbackJWT = callback;
        }
    }

    public void OnJWT(string jwt)
    {        
        PlayerPrefs.DeleteKey("json_web_token");
        if (jwt != "error")
        {
            PlayerPrefs.SetString("json_web_token", jwt);
        }

        if(callbackJWT != null)
            callbackJWT.Invoke();
        
        callbackJWT = null;
    }

    public static void SetSamandaCapturePrefab(GameObject obj)
    {
        if(instance != null)
            instance.SamandaSceenCapturePrefab = obj;
    }

    public void OnLinks(string links)
    {
        string newJson = "{ \"array\": " + links + "}";
        ArrayJson linksVal = JsonUtility.FromJson<ArrayJson>(newJson);
        AccountLinks.Clear();
        foreach(int val in linksVal.array)
        {
            AccountLinks.Add(val);
        }
    }

    static public List<int> GetAccountLinks()
    {
        if(instance != null)
            return instance.AccountLinks;

        return null;
    }

    public static int GetRelativeKeyboardHeight(RectTransform rectTransform, bool includeInput)
    {
        int keyboardHeight = GetKeyboardHeight(includeInput);
        float screenToRectRatio = Screen.height / rectTransform.rect.height;
        float keyboardHeightRelativeToRect = keyboardHeight / screenToRectRatio;

        return (int)keyboardHeightRelativeToRect;
    }

    public static int GetKeyboardHeight(bool includeInput)
    {
#if UNITY_EDITOR
        return 0;
#elif UNITY_ANDROID
        using (AndroidJavaClass unityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            AndroidJavaObject unityPlayer = unityClass.GetStatic<AndroidJavaObject>("currentActivity").Get<AndroidJavaObject>("mUnityPlayer");
            AndroidJavaObject view = unityPlayer.Call<AndroidJavaObject>("getView");
            
            if (view == null)
                return 0;
            var decorHeight = 0;
            if (includeInput)
            {
                AndroidJavaObject dialog = unityPlayer.Get<AndroidJavaObject>("mSoftInputDialog");
                if(dialog != null)
                {
                    AndroidJavaObject decorView = dialog.Call<AndroidJavaObject>("getWindow").Call<AndroidJavaObject>("getDecorView");
                    if (decorView != null)
                        decorHeight = decorView.Call<int>("getHeight");
                }
            }
            using (AndroidJavaObject rect = new AndroidJavaObject("android.graphics.Rect"))
            {
                view.Call("getWindowVisibleDisplayFrame", rect);
                return Screen.height - rect.Call<int>("height") + decorHeight;
            }
        }
#elif UNITY_IOS
        return (int)TouchScreenKeyboard.area.height;
#else
        return 0;
#endif
    }

    public static void SendSamandaCommand(string command)
    {
        if(instance != null)
            instance._webView.EvaluateJS(command);
    }

    public void OnNetworkErrorCallback()
    {
        callbackNetworkFail?.Invoke();        
    }

    public void OnAuthCommand(string command)
    {
        switch(command)
        {
            case "cleargoogle":
                GoogleAuthLogOut();
                break;

            default:
                break;
        }
    }

    public void OnServerEvent(string command)
    {
        switch (command)
        {
            case "maintenance":
                callbackServerMaintenance?.Invoke();                
                break;

            default:
                break;
        }
    }

    /**
     * 인터페이스는 모두 static으로 만듦
     */
    public static void SetLogoutCallback(Callback cb)
    {
        if (null != instance)
            instance.callbackLogOut = cb;
    }

    public static void SetOnHideCallback(Callback cb)
    {
        if (null != instance)
        {
            instance.callbackSamandaHide = cb;
        }
    }

    public static void SetNetworkErrorCallback(Callback cb)
    {
        if (null == instance)
        {
            instance = SamandaLauncher.Instance;            
        }

        instance.callbackNetworkFail = cb;
    }

    public static void SetServerMaintenanceCallback(Callback cb)
    {
        if (null == instance)
        {
            instance = SamandaLauncher.Instance;
        }

        instance.callbackServerMaintenance = cb;
    }

    public static void OpenCustomerSupportPage()
    {
        OpenCustomerSupportPage(GameDataManager.Instance.GetVersion(),
            NetworkManager.GetInstance().UserNo.ToString());
    }
    public static void OpenCustomerSupportPage(string clientVersion, string userId = "")
    {
        if (null == instance)
        {
            Debug.LogError("SamandaLauncher instance not initialized.");
            return;
        }

        Dictionary<string, string> values = new Dictionary<string, string>()
        {
            { "device", SystemInfo.deviceModel },
            { "osver", SystemInfo.operatingSystem },
            { "pid", GetPID() },
            { "cver", clientVersion },
            { "ano", GetAccountNo() },
            { "uno", userId }
        };

        var builder = new UriBuilder(CustomerServiceURL);

        foreach (KeyValuePair<string, string> entity in values)
        {
            string queryToAppend = String.Format("{0}={1}", entity.Key, entity.Value);
            
            if (builder.Query != null && builder.Query.Length > 1)
            {
                builder.Query = builder.Query.Substring(1) + "&" + queryToAppend;
            }
            else
            {
                builder.Query = queryToAppend;
            }
        }

        Application.OpenURL(builder.Uri.AbsoluteUri);
    }

    static public bool IsSamandaReady()
    {
        switch (Instance.cur_state)
        {
            case Samanda_State.NONE:
                return instance.isLoadDone;
            case Samanda_State.HIDE:
                return instance.isLoadDone;
            case Samanda_State.INIT:
                return true;
            case Samanda_State.NEED_LOGIN:
                return true;
            case Samanda_State.PASS_LOGIN:
                return true;
            case Samanda_State.BANNER:
                return true;
            case Samanda_State.MAIN:
                return true;
        }

        return false;
    }

    static public void OnSamandaPageLoad()
    {
        instance.isLoadDone = true;
    }

    // iOS native link
    //#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
#if UNITY_IOS && !UNITY_EDITOR
    // oAuth response callback type
    public delegate void SamandaNativeoAuthDelegateType(int error, string token);

    // if needed
    [DllImport("__Internal")]
    private static extern void SamandaNativeInit();

    // request signin with apple
    [DllImport("__Internal")]
    private static extern void SamandaNativeoAuthApple(SamandaNativeoAuthDelegateType callback);

    // request GoogleSignIn
    [DllImport("__Internal")]
    private static extern void SamandaNativeoAuthGoogle(SamandaNativeoAuthDelegateType callback);

#if UNITY_IOS
    // callback function of signin with apple
    [MonoPInvokeCallback(typeof(SamandaNativeoAuthDelegateType))]
    private static void SamandaUnityoAuthCallback(int error, string token)
    {
        Debug.Log("SamandaUnityoAuthCallback\nerror: " + error + " / token: " + token);
        Instance._webView.EvaluateJS(
            "window.Samanda.onOAuthJavaResponse('{\"type\":\"AP\", \"rs\":" + error + ", \"token\":\"" + token + "\"}')"
        );
    }
#elif UNITY_ANDROID
    // callback function of GoogleSignIn
    //[MonoPInvokeCallback(typeof(SamandaNativeoAuthDelegateType))]
    //private static void SamandaUnityoAuthCallback(int error, string token)
    //{
    //    Debug.Log("SamandaUnityoAuthCallback\nerror: " + error + " / token: " + token);
    //    Instance._webView.EvaluateJS(
    //        "window.Samanda.onOAuthJavaResponse('{\"type\":\"GG\", \"rs\":" + error + ", \"token\":\"" + token + "\"}')"
    //    );
    //}
#endif // #if UNITY_IOS #elif UNITY_ANDROID
#endif
    public static void SetCommandCallback(CommandCallback callback)
    {
        Instance.callbackCommand = callback;
    }

    public void OnSamandaCommand(string command)
    {
        callbackCommand?.Invoke(command);
    }
}
