using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using UnityEngine;


namespace SandboxPlatform.SAMANDA
{
    public class SAMANDA : MonoBehaviour
    {
        public const string BASE_URL = "https://sandbox-gs.mynetgear.com/";
        [SerializeField] private string GOOGLECLIENT_ID = "";
        [SerializeField] private string PRODUCT_ID = "";

        [Header("[Splash]")]
        [SerializeField]
        SAMANDA_Splash Splash;

        [Header("[UI Prefab]")]
        [SerializeField]
        GameObject UIPrefab;

        [SerializeField]
        SAMANDA_UI UI = null;

        [Header("Login Option Check")]
        public bool _guestLogin;
        public bool _googleLogin;
        public bool _appleLogin;

        //게스트 모드 시 자동 게스트 접속
        public bool onlyGuestMode;

        #region Instance
        protected static SAMANDA _instance = null;
        public static SAMANDA Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<SAMANDA>();
                    if (_instance == null)
                    {
                        GameObject obj = new GameObject();
                        obj.name = typeof(SAMANDA).Name;
                        _instance = obj.AddComponent<SAMANDA>();
                    }
                }
                return _instance;
            }
        }
        public static bool hasInstance()
        {
            return _instance;
        }
        public static void ClearInstance()
        {
            if (hasInstance())
            {
                DestroyImmediate(_instance.gameObject);
                _instance = null;
            }
        }
        #endregion
        #region ACCOUNT_INFO
        public string ACCOUNT_TOK
        {
            get { return PlayerPrefs.GetString("account_token", ""); }
            private set { PlayerPrefs.SetString("account_token", value); }
        }
        public string ACCOUNT_ANO
        {
            get { return PlayerPrefs.GetString("account_ano", ""); }
            private set { PlayerPrefs.SetString("account_ano", value); }
        }

        public AUTH_TYPE ACCOUNT_TYPE
        {
            get { return (AUTH_TYPE)PlayerPrefs.GetInt("account_type", -1); }
            private set { PlayerPrefs.SetInt("account_type", (int)value); }
        }

        public string ACCOUNT_JWT
        {
            get { return PlayerPrefs.GetString("account_jwt", ""); }
            private set { PlayerPrefs.SetString("account_jwt", value); }
        }


        string TERM_VER = "";
        List<AUTH_TYPE> AUTH_LINK = new List<AUTH_TYPE>();

        #endregion

        public LOGIN_STATE curLoginState = LOGIN_STATE.UNKNOWN;
        public delegate void LoginCallback(string jwt);

        LoginCallback LoginCB = null;
        System.Action LogoutCB = null;

        protected void Awake()
        {
            if (!Application.isPlaying)
            {
                return;
            }
            _instance = this;
            if (UI == null)
            {
                if (UIPrefab != null)
                {
                    GameObject uiObject = GameObject.Instantiate(UIPrefab, transform);
                    uiObject.transform.SetAsFirstSibling();
                    uiObject.transform.localPosition = Vector3.zero;
                    uiObject.transform.localScale = Vector3.one;

                    UI = uiObject.GetComponent<SAMANDA_UI>();

                    Splash.transform.SetAsLastSibling();
                }
                else
                {
                    Debug.LogError("오류");
                }
            }

            DontDestroyOnLoad(this.gameObject);
        }

        public void StartSamanda(string product_id, LoginCallback loginCB, System.Action logoutCB)
        {
            PRODUCT_ID = product_id;

            UI.SetActive(false);

#if !UNITY_EDITOR//에디터에서는 Splash 생략
            //Splash.OnSplash();
#endif
            if (string.IsNullOrEmpty(ACCOUNT_JWT) && !onlyGuestMode)
            {
                curLoginState = LOGIN_STATE.NO_ACCOUNT_INFO;
                RefreshState();
            }
            else if (onlyGuestMode && string.IsNullOrEmpty(ACCOUNT_JWT))
            {
                OnGuestLogin();
            }
            else
            {
                WWWForm param = new WWWForm();
                param.AddField("pid", PRODUCT_ID);
                param.AddField("ano", ACCOUNT_ANO);
                param.AddField("tok", ACCOUNT_TOK);
                param.AddField("plt", 2);

                WebRequester.Instance.SendPost(BASE_URL + "auth/signin", param, ResponseAccountInfo);
            }

            LoginCB = loginCB;
            LogoutCB = logoutCB;
        }

        void ResponseAccountInfo(JToken response)
        {
            ClearAccount();

            JObject data = (JObject)response;

            if (data.ContainsKey("rs"))
            {
                curLoginState = (LOGIN_STATE)data["rs"].Value<int>();
            }

            SetAccountInfo(data);
            RefreshState();
        }

        public void OnSplashDone()
        {
            RefreshState();
        }

        void SetAccountInfo(JObject data)
        {
            if (data.ContainsKey("tok"))
            {
                ACCOUNT_TOK = data["tok"].Value<string>();
            }

            if (data.ContainsKey("ano"))
            {
                ACCOUNT_ANO = data["ano"].Value<string>();
            }

            if (data.ContainsKey("typ"))
            {
                ACCOUNT_TYPE = (AUTH_TYPE)data["typ"].Value<int>();
            }

            if (data.ContainsKey("terms_ver"))
            {
                TERM_VER = data["terms_ver"].Value<string>();
            }

            if (data.ContainsKey("jwt"))
            {
                ACCOUNT_JWT = data["jwt"].Value<string>();
            }

            if (data.ContainsKey("acc"))
            {
                JObject accData = (JObject)data["acc"];
                if (accData != null && accData.ContainsKey("links"))
                {
                    JArray links = (JArray)accData["links"];
                    foreach (JToken link in links)
                    {
                        AUTH_LINK.Add((AUTH_TYPE)link.Value<int>());
                    }
                }
            }
        }

        void ClearAccount()
        {
            ACCOUNT_TOK = "-";
            ACCOUNT_ANO = "-";
            TERM_VER = "";
            ACCOUNT_TYPE = AUTH_TYPE.NONE;
            ACCOUNT_JWT = "";

            AUTH_LINK.Clear();
        }

        public void LoginDone()
        {
            UI.SetActive(false);
            LoginCB?.Invoke(ACCOUNT_JWT);
        }

        public void TermsofuseDone()
        {

            WWWForm param = new WWWForm();
            param.AddField("pid", PRODUCT_ID);
            param.AddField("ano", ACCOUNT_ANO);
            param.AddField("tok", ACCOUNT_TOK);
            param.AddField("ver", TERM_VER);
            param.AddField("act", 1);
            //WebRequester.Instance.SendPost(BASE_URL + "auth/signup", wwwParam, onSuccess =>
            //{
            //    ClearAccount();
            //    JObject res = (JObject)onSuccess;
            //    SetAccountInfo(res);

            //    curLoginState = LOGIN_STATE.LOGIN_DONE;
            //    RefreshState();
            //});

            WebRequester.Instance.SendPost(BASE_URL + "auth/terms", param, (response) =>
            {
                if (response["rs"].Value<int>() == 0)
                {
                    curLoginState = LOGIN_STATE.LOGIN_DONE;
                    RefreshState();
                }
            });
        }

        public void OnAccountUI()
        {
            if (curLoginState == LOGIN_STATE.LOGIN_DONE)
            {
                UI.SetUIState(curLoginState);
            }
        }

        public void RefreshState()
        {
            switch (curLoginState)
            {
                case LOGIN_STATE.UNKNOWN:
                    break;
                case LOGIN_STATE.VALID_ACCOUNT:
                    curLoginState = LOGIN_STATE.LOGIN_DONE;
                    RefreshState();
                    return;
                case LOGIN_STATE.NO_ACCOUNT_INFO:
                    break;
                case LOGIN_STATE.TERMSOFUSE:
                    break;
                case LOGIN_STATE.LOGIN_DONE:
                    if (string.IsNullOrEmpty(TERM_VER) && ACCOUNT_TYPE != AUTH_TYPE.GE)
                    {
                        curLoginState = LOGIN_STATE.TERMSOFUSE;
                        RefreshState();
                        return;
                    }
                    else
                    {
                        LoginDone();
                        return;
                    }
                default://error
                    curLoginState = LOGIN_STATE.NO_ACCOUNT_INFO;
                    break;
            }

            UI.SetUIState(curLoginState);
        }

#if UNITY_EDITOR_WIN || UNITY_EDITOR_OSX || (UNITY_STANDALONE_WIN && !UNITY_EDITOR) || (UNITY_STANDALONE_OSX && !UNITY_EDITOR) || UNITY_IOS

        public void OnOAuthJavaResponse(OAuthApp.oAuthSendForSAMANDA data)
        {
            switch (int.Parse(data.rs))
            {
                case 0:
                    {
                        ACCOUNT_TYPE = data.type;

                        WWWForm param = new WWWForm();
                        param.AddField("pid", PRODUCT_ID);
                        param.AddField("typ", (int)data.type);
                        param.AddField("tok", data.token);
                        param.AddField("plt", 2);

                        WebRequester.Instance.SendPost(BASE_URL + "auth/signin", param, (response) =>
                        {
                            JObject res = (JObject)response;

                            if (res["rs"].Value<int>() == 1)
                            {
                                WWWForm signupParam = new WWWForm();
                                signupParam.AddField("pid", PRODUCT_ID);
                                signupParam.AddField("typ", (int)data.type);
                                signupParam.AddField("id_tok", data.token);
                                signupParam.AddField("plt", 2);

                                //사용자 계정 가입시 약관동의 
                                //wwwParam = signupParam;
                                //curLoginState = LOGIN_STATE.TERMSOFUSE;
                                //RefreshState();
                                WebRequester.Instance.SendPost(BASE_URL + "auth/signup", signupParam, OnSignData);
                            }
                            else
                            {
                                OnSignData(res);
                                //SAMANDA_ANDROID.Instance.WebGoogleLogin(data.token);
                            }
                        });
                    }
                    break;
                case 1:
                    {

                    }
                    break;
                case 12501:
                    // user cancelled				
                    return;

                case 12502:
                    // SIGN_IN_CURRENTLY_IN_PROGRESS - ignore
                    return;

                case 12500:
                    // SIGN_IN_FAILED				
                    return;

                default:
                    break;
            }
        }
#endif

        void OnSignData(JToken response)
        {
            ClearAccount();
            JObject res = (JObject)response;

            SetAccountInfo(res);
            if (curLoginState == LOGIN_STATE.LOGIN_DONE)
            {
                UI.SetUIState(LOGIN_STATE.LOGIN_DONE);//계정연동 UI 갱신
            }
            else
            {
                curLoginState = LOGIN_STATE.LOGIN_DONE;
                RefreshState();
            }

            RefreshState();
        }

        public void OnPlatFormLogin(AUTH_TYPE type)
        {
            ACCOUNT_TYPE = type;

            WWWForm param = new WWWForm();
            param.AddField("pid", PRODUCT_ID);
            param.AddField("typ", (int)type);
            if(type == AUTH_TYPE.GG)
                param.AddField("tok", SAMANDA_ANDROID.Instance.googleIdToken);
            else if(type == AUTH_TYPE.AP)
                param.AddField("tok", SAMANDA_IOS.Instance.appleIdToken);
            param.AddField("plt", 2);

            WebRequester.Instance.SendPost(BASE_URL + "auth/signin", param, (response) =>
            {
                JObject res = (JObject)response;

                if (res["rs"].Value<int>() == 1)
                {
                    WWWForm signupParam = new WWWForm();
                    signupParam.AddField("pid", PRODUCT_ID);
                    signupParam.AddField("typ", (int)type);
                    if (type == AUTH_TYPE.GG)
                        signupParam.AddField("id_tok", SAMANDA_ANDROID.Instance.googleIdToken);
                    else if (type == AUTH_TYPE.AP)
                        signupParam.AddField("id_tok", SAMANDA_IOS.Instance.appleIdToken);
                    signupParam.AddField("plt", 2);
                    signupParam.AddField("plt", 2);

                    //사용자 계정 가입시 약관동의 
                    WebRequester.Instance.SendPost(BASE_URL + "auth/signup", signupParam, OnSignData);
                }
                else
                {
                    OnSignData(res);
                }
            });
        }


        public string GetGoogleClientID()
        {
            return GOOGLECLIENT_ID;
        }

        public void OnGuestLogin()
        {
            ACCOUNT_TYPE = AUTH_TYPE.GE;

            WWWForm param = new WWWForm();
            param.AddField("pid", PRODUCT_ID);
            param.AddField("typ", (int)ACCOUNT_TYPE);
            param.AddField("plt", 2);

            WebRequester.Instance.SendPost(BASE_URL + "auth/signup", param, (response) =>
            {
                ClearAccount();

                JObject data = (JObject)response;

                SetAccountInfo(data);

                curLoginState = LOGIN_STATE.LOGIN_DONE;
                RefreshState();
            });
        }

        public List<AUTH_TYPE> GetLinkedAuth()
        {
            return AUTH_LINK;
        }

        public void OnLogout()
        {
            System.Action logoutCB = LogoutCB;
            curLoginState = LOGIN_STATE.UNKNOWN;
            //LoginCB = null;
            //LogoutCB = null;
            ClearAccount();

            UI.SetActive(false);

            logoutCB?.Invoke();
        }

        public void OnReqOAuth(AUTH_TYPE eType)
        {
            var instance = DeviceManager.Instance;
            switch (eType)
            {
                #region GG
                case AUTH_TYPE.GG:
                    //Google에서 Google 로그인
                    if (instance.deviceEnvironment == DeviceEnvironment.Google_Mobile)
                    {
                        //// Google sign in handler
                        //string pkg = "com.sandboxgame.samandaDemo";
                        //string cls = "GoogleSignInHandlerActivity";
                        //AndroidJavaObject samandaSignInHandler = new AndroidJavaObject($"{pkg}.{cls}");

                        //// UnityPlayer class
                        //Debug.Log("Calling onReqoAuth");

                        //AndroidJavaClass UnityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                        //AndroidJavaObject currentActivity = UnityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

                        //// call java method
                        //samandaSignInHandler.CallStatic("setClientId", SAMANDA.Instance.GetGoogleClientID());
                        //samandaSignInHandler.CallStatic("onReqOAuth", currentActivity);

                        SAMANDA_ANDROID.Instance.Login(callback =>
                        {
                            ACCOUNT_TYPE = AUTH_TYPE.GG;

                            switch (callback)
                            {
                                case 0:
                                    //if (string.IsNullOrEmpty(TERM_VER))
                                    //{
                                    //    curLoginState = LOGIN_STATE.TERMSOFUSE;
                                    //}
                                    //else
                                    //{
                                    //    curLoginState = LOGIN_STATE.LOGIN_DONE;
                                    //}
                                    OnPlatFormLogin(ACCOUNT_TYPE);
                                    break;
                                case 1:
                                    curLoginState = LOGIN_STATE.NO_ACCOUNT_INFO;
                                    break;
                                default:
                                    break;
                            }
                            RefreshState();
                        });
                        return;
                    }
                    //ios에서 Google 로그인
                    else if (instance.deviceEnvironment == DeviceEnvironment.IOS_Mobile)
                    {
                        ////UnityStartGoogleSignInIOS();
                        //SamandaNativeoAuthGoogle(SamandaUnityoAuthCallback);
                        SAMANDA_IOS.Instance.AppleToGoogle(success =>
                        {
                            ACCOUNT_TYPE = AUTH_TYPE.AP;

                            if (success)
                            {
                                OnPlatFormLogin(ACCOUNT_TYPE);
                            }
                            else
                            {
                                curLoginState = LOGIN_STATE.NO_ACCOUNT_INFO;
                            }
                            RefreshState();
                        });
                        return;
                    }
                    else if (instance.deviceEnvironment == DeviceEnvironment.None_Mobile)
                    {
#if UNITY_EDITOR_WIN || UNITY_EDITOR_OSX || (UNITY_STANDALONE_WIN && !UNITY_EDITOR) || (UNITY_STANDALONE_OSX && !UNITY_EDITOR)
                        OAuthApp.MainWindow win = new OAuthApp.MainWindow();
                        win.button_Click();
#endif
                        return;
                    }
                    else
                        Debug.Log("Not in android, but it called with GG");
                    break;

                #endregion

                #region AP
                case AUTH_TYPE.AP:
                    {
                        //ios 에서 Apple 로그인
                        if (instance.deviceEnvironment == DeviceEnvironment.IOS_Mobile)
                        {
                            SAMANDA_IOS.Instance.SAMANDAWithApple(success =>
                            {
                                if (success)
                                {
                                    curLoginState = LOGIN_STATE.LOGIN_DONE;
                                }
                                else
                                {
                                    curLoginState = LOGIN_STATE.NO_ACCOUNT_INFO;
                                }
                                RefreshState();

                            });
                            //Debug.Log("Apple oAuth request. unity -> iOS");
                            //SamandaNativeoAuthApple(SamandaUnityoAuthCallback);
                            //StartSignInWithApple();
                            return;
                        }
                        //Google 에서 Apple 로그인
                        else if (instance.deviceEnvironment == DeviceEnvironment.Google_Mobile)
                        {
                            ACCOUNT_TYPE = AUTH_TYPE.AP;

                            SAMANDA_ANDROID.Instance.TryFirebaseAppleLogin(success =>
                            {
                                if (success)
                                {
                                    OnPlatFormLogin(ACCOUNT_TYPE);
                                }
                                else
                                {
                                    curLoginState = LOGIN_STATE.NO_ACCOUNT_INFO;
                                }
                                RefreshState();
                            });
                            return;
                        }
                        else if (instance.deviceEnvironment == DeviceEnvironment.None_Mobile)
                        {
                            //string url = BASE_URL + "auth/aauth_t?pid=" + PRODUCT_ID;
                            Debug.Log("기능개발중입니다.");
                            return;
                        }
                        else
                            Debug.Log("Not in android, but it called with GG");
                    }
                    break;
                #endregion

                default:
                    Debug.Log("Unswitched oAuth type: " + eType);
                    break;
            }
        }
        public LOGIN_STATE GetLoginState()
        {
            return curLoginState;
        }
    }

}