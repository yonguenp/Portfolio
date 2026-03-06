using System.Collections;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace SandboxNetwork
{
    public class StartLoading : MonoBehaviour, EventListener<LoginEvent>
    {
        [SerializeField] private GameObject TitleLogo = null;
        [SerializeField] private GameObject Background = null;

        [SerializeField] private Text loadingInfoText = null;
        [SerializeField] private float fadeInTime = 0.5f;
        [SerializeField] private float fadeOutTime = 1.2f;
        [SerializeField] private Slider loadingSlider = null;
        [SerializeField] private SystemLoadingPopup SystemPopup = null;
        [SerializeField] private SystemPreopenPopup PreopenPopup = null;
        [SerializeField] private Text labelClientVer = null;
        [SerializeField] private Text labelBundleVer = null;

        [SerializeField]
        private GameObject btnBundleReset;
        [SerializeField]
        private GameObject btnLogout;
        [SerializeField]
        private GameObject btnServerChange;

        [SerializeField] private GameObject busyPanel = null;
        [SerializeField] private GameObject loginMenu = null;
        [SerializeField] private TermsBox termsBox = null;
        [SerializeField] private NickNameBox nickBox = null;
        [SerializeField] private GameObject loginApple = null;
        [SerializeField] private GameObject loginGoogle = null;
        [SerializeField] private GameObject loginIMX = null;
        [SerializeField] private GameObject loginGuest = null;

        [SerializeField] private TitleSpine titleSpine = null;
        [SerializeField] private ServerSelectPopup ServerSelectPopup = null;


        bool loading = false; //로딩 중인지.
        bool enterPossible;
        bool enterClicked;
        bool userInit = false;

        GoogleLoginController googleLogin = null;
        AppleLoginController appleLogin = null;
        IMXLoginController imxLogin = null;

        LoginData currentLoginData = null;

        Coroutine curLoadCoroutine = null;

        Dictionary<int, string> ServerState = null;

        public enum LoadingState
        {
            LOADMIN = 0,

            BUNDLESYNC = 0,
            DATASYNC,
            CDN_DEFAULT_LOAD,
            CDN_LOAD,
            USER_DATA_LOAD,
            BUNDLECHECK,
            SOUNDLOAD,
            PRELOAD,
            SCENE_LOAD,

            LOADMAX = SCENE_LOAD,
        }
        LoadingState curLoadingState = LoadingState.LOADMIN;
        private void Awake()
        {
            if (SoundManager.Instance != null)
                SoundManager.Instance.Initialize();

            Resources.UnloadUnusedAssets();
        }
        private void OnEnable()
        {
            EventManager.AddListener(this);
            DAppManager.Instance.ClearDApp();
        }

        private void OnDisable()
        {
            EventManager.RemoveListener(this);
        }

        public void OnEvent(LoginEvent eventType)
        {
            switch (eventType.loginData.loginResultState)
            {
                case eLoginResult.OK_NEW_ACCOUNT:
                    UpdateLoginData(eventType.loginData);
                    OnSignUp();
                    break;
                case eLoginResult.OK_HAS_ACCOUNT:
                    UpdateLoginData(eventType.loginData);
                    OnSignIn();
                    break;
                default:
                    OnLogOut();
                    break;
            }
        }
        void UpdateLoginData(LoginData newData)
        {
            currentLoginData = newData;

            SBGameManager.Instance.UserNickname = currentLoginData.nick;
            SBGameManager.Instance.UserAccessToken = currentLoginData.binToken;
        }
        void Start()
        {
            currentLoginData = new LoginData();

            var audio = GetComponent<AudioSource>();
            audio.volume = GamePreference.Instance.GetBgmVolume();
            GetComponent<AudioSource>().Play();
            LoadingComponentInit();

            //if(SystemPopup != null) 일부로 null 검사 안함 (등록안되있으면 터지게)
            {
                SystemPopup.InitInstance();
                PreopenPopup.InitInstance();
            }

            loginMenu.SetActive(false);
            ServerSelectPopup.SetActive(false);
            SetNickBox(false);
            btnLogout.SetActive(false);
            btnServerChange.SetActive(false);

#if UNITY_EDITOR
            btnBundleReset.SetActive(false);
#endif

            // 플랫폼 로그인 Init
            // Login Controller는 Mono 상속 / Update를 써야하기때문에 Addcomponent로 인스턴스를 생성해줘야지 new로 하면안됨.
            googleLogin = gameObject.GetComponent<GoogleLoginController>();
            appleLogin = gameObject.GetComponent<AppleLoginController>();
            imxLogin = gameObject.GetComponent<IMXLoginController>();
            if (googleLogin == null)
                googleLogin = gameObject.AddComponent<GoogleLoginController>();
            if (appleLogin == null)
                appleLogin = gameObject.AddComponent<AppleLoginController>();
            if (imxLogin == null)
                imxLogin = gameObject.AddComponent<IMXLoginController>();


            UIManager.Instance.InitUI(eUIType.None);
            LoginManager.Instance.InitializeLogin();

            WebServerCheck();

            DOTween.SetTweensCapacity(3125, 312);
        }

        void InitUI()
        {
            bool loginMenuCheck = SBGameManager.Instance.IsForceLogout || string.IsNullOrWhiteSpace(SBGameManager.Instance.UserAccessToken);
            loginMenu.SetActive(loginMenuCheck);

#if UNITY_EDITOR
            btnBundleReset.SetActive(true);
#endif
        }
        void SetNickBox(bool show, LoginData data = null)
        {
            //TitleLogo.SetActive(!show);
            nickBox.SetActive(show, data);

            //foreach(Transform child in Background.transform)
            //         {
            //	child.DOKill();
            //	child.DOLocalMoveY(show ? 70f : 0f, 1f);
            //}
        }

        void SetTermBox(bool show, LoginData data, NetworkManager.SuccessCallback cb)
        {
            termsBox.SetActive(show, data, cb);
        }

        private void OnDestroy()
        {
            Graphic[] gComps = loadingInfoText.GetComponents<Graphic>();
            Outline[] oComps = loadingInfoText.GetComponents<Outline>();

            for (int i = 0; i < gComps.Length; i++)
            {
                gComps[i].DOKill();
            }
            for (int i = 0; i < oComps.Length; i++)
            {
                oComps[i].DOKill();
            }
        }
        public void OnClickResetBundle()
        {
            SystemPopup.SetMessage(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100002665), StringData.GetStringByIndex(100000199), StringData.GetStringByIndex(100000200));
            SystemPopup.SetCallBack(
                () =>
                {
                    AssetBundleManager.AssetFileRemove();
                },
                () => { }
            );
        }
        private void LoadingComponentInit()
        {
            curLoadingState = LoadingState.LOADMIN;

            if (loadingSlider != null)
            {
                loadingSlider.value = 0;
            }

            loadingSlider.gameObject.SetActive(false);
            loadingInfoText.text = "";
            loadingInfoText.fontSize = 36;

            loading = false;
            enterPossible = false;
            enterClicked = false;
            labelClientVer.text = "ClientVer. " + GamePreference.Instance.VERSION + "/" + ClientConstants.CurrentPlatform;

            StopCoroutine(TouchMessage());

            Graphic[] gComps = loadingInfoText.GetComponents<Graphic>();
            Outline[] oComps = loadingInfoText.GetComponents<Outline>();

            for (int i = 0; i < gComps.Length; i++)
            {
                gComps[i].DOKill();
                gComps[i].color = Color.white;
            }
            for (int i = 0; i < oComps.Length; i++)
            {
                oComps[i].DOKill();
                oComps[i].effectColor = Color.black;
            }
        }
        void WebServerCheck()
        {
            StartCoroutine(NetworkConnectionCheck());
        }
        IEnumerator NetworkConnectionCheck()
        {
            float time = 10.0f;
            while (time > 0 && Application.internetReachability == NetworkReachability.NotReachable)
            {
                time -= Time.deltaTime;
                loadingInfoText.text = StringData.GetStringByStrKey("네트워크상태확인중");
                yield return new WaitForEndOfFrame();
            }

            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                loadingInfoText.text = StringData.GetStringByStrKey("네트워크연결실패");
                SystemPopup.SetMessage(StringData.GetStringByIndex(100000248), StringData.GetStringByStrKey("errorcode_3"), StringData.GetStringByIndex(100000199));
                SystemPopup.SetCallBack(() =>
                {
#if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
#else
						Application.Quit();
#endif
                });

                while (true)
                {
                    yield return new WaitForEndOfFrame();
                }
            }

            NetworkManager manager = NetworkManager.Instance;
            if (manager == null)
            {
                loadingInfoText.text = StringData.GetStringByStrKey("네트워크연결실패");
                OnFailServerInit(null);
                while (true)
                {
                    yield return new WaitForEndOfFrame();
                }
            }

            if (titleSpine != null)
                yield return titleSpine.StartCO();

            InitUI();

            SBGameManager.Instance.AddManager(typeof(ChatManager), ChatManager.Instance, true);

            NetworkManager.Send("auth/init", null, OnSuccessServerInit, OnFailServerInit);
        }

        public void OnServerChange()
        {
            LoadingComponentInit();
            OnSignIn();
        }

        void OnSuccessServerInit(JObject jObject)
        {
            SystemLoadingPopup systemPopup = SystemLoadingPopup.Instance;
            if (SBFunc.IsJTokenType(jObject["rs"], JTokenType.Integer))
            {
                switch (jObject["rs"].Value<int>())
                {
                    case (int)eApiResCode.OK: //유저 데이터 채우기 및 씬 이동
                    {
                        if (jObject.ContainsKey("bundle_version"))
                        {
                            GamePreference.Instance.SetBundleVersion(jObject["bundle_version"].Value<int>());
                            labelBundleVer.text = "ResourceVer. " + GamePreference.Instance.RESOURCE_BUNDLE_VERSION;
                        }

                        if (SBFunc.IsJTokenType(jObject["client_db_version"], JTokenType.Integer))
                        {
                            DataBase.SetVersion(jObject["client_db_version"].Value<int>());
                        }

                        if (jObject.ContainsKey("cdn_url"))
                        {
                            NetworkManager.SetCDNURL(jObject["cdn_url"].Value<string>());
                        }

                        AccountLoader.CheckValidAccount();

                        if (jObject.ContainsKey("login_system"))
                        {
                            byte login_flag = jObject["login_system"].Value<byte>();
                            PlayerPrefs.SetInt("login_flag", login_flag);
                        }
                        
                        if (string.IsNullOrWhiteSpace(SBGameManager.Instance.UserAccessToken) || SBGameManager.Instance.IsForceLogout) // 토큰이 없거나 강제로 로그아웃된 경우
                        {
                            if (jObject.ContainsKey("login_system"))
                            {
                                byte login_flag = jObject["login_system"].Value<byte>();

                                byte login_guest = 1 << 0;  //1
                                byte login_google = 1 << 1; //2
                                byte login_apple = 1 << 2;  //4
                                byte login_imx = 1 << 3;  //8

                                loginGuest.SetActive((login_flag & login_guest) > 0);
                                loginApple.SetActive((login_flag & login_apple) > 0);
                                loginGoogle.SetActive((login_flag & login_google) > 0);
                                loginIMX.SetActive(
#if UNITY_EDITOR
                                    true ||
#endif
                                (login_flag & login_imx) > 0);
                            }
                            else
                            {
                                loginGuest.SetActive(true);
                                loginApple.SetActive(true);
                                loginGoogle.SetActive(true);
                                loginIMX.SetActive(true);
                            }
                        }
                        else // 저장된 토큰이 존재하고 강제 로그아웃이 아닌 경우
                        {
                            OnAutoLogin();
                        }

                        JArray server_flag = null;
                        if (SBFunc.IsJTokenType(jObject["server_flag_list"], JTokenType.Array))
                        {
                            server_flag = (JArray)jObject["server_flag_list"];
                        }

                        if (SBFunc.IsJTokenType(jObject["server_tag_list"], JTokenType.Array))
                        {
                            var servers = (JArray)jObject["server_tag_list"];
                            for (int i = 0; i < servers.Count; i++)
                            {
                                int tag = servers[i].Value<int>();
                                int flag = 0;
                                if (server_flag != null && server_flag.Count > i)
                                {
                                    flag = server_flag[i].Value<int>();
                                }

                                NetworkManager.Instance.SetServerInfo(tag, new ServerInfo(
                                    tag,
                                    flag
                                ));
                            }
                        }

                    }
                    return;

                    case (int)eApiResCode.VERSION_ERROR:
                    {
                        //todo 버전 미스매치 팝업
                        if (systemPopup != null)
                        {
                            systemPopup.SetMessage(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100002669), StringData.GetStringByIndex(100000199));
                            systemPopup.SetButtonState(true, false, false);
                            systemPopup.SetCallBack(() =>
                            {
                                Application.OpenURL(GameConfigTable.GetStoreURL());
                                SBFunc.Quit();
                            });
                        }
                    }
                    return;

                    case (int)eApiResCode.SERVICE_UNAVAILABLE:
                        if (systemPopup != null)
                        {
                            string title = jObject["title"].Value<string>();
                            string msg = jObject["msg"].Value<string>();
                            string btn = jObject["btn"].Value<string>();
                            string url = jObject["url"].Value<string>();

                            systemPopup.SetMessage(title, msg, btn);
                            systemPopup.SetButtonState(true, false, false);
                            systemPopup.SetCallBack(() =>
                            {
                                if (!string.IsNullOrEmpty(url))
                                {
                                    Application.OpenURL(url);
                                }

                                SBFunc.Quit();
                            });
                        }

                        return;
                    default:
                        break;
                }
            }

            //todo 서버점검 팝업			
            if (systemPopup != null)
            {
                systemPopup.SetMessage(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100005029), StringData.GetStringByIndex(100000199));
                systemPopup.SetCallBack(() =>
                {
                    SBFunc.Quit();
                });
            }
        }
        void OnFailServerInit(string reason)
        {
            SystemLoadingPopup systemPopup = SystemLoadingPopup.Instance;
            if (systemPopup != null)
            {
                systemPopup.SetMessage(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100005029), StringData.GetStringByIndex(100000199));
                systemPopup.SetCallBack(() =>
                {
                    SBFunc.Quit();
                });
            }
        }
        public void Init()
        {
            btnBundleReset.SetActive(false);
            btnServerChange.SetActive(false);
            btnLogout.SetActive(false);
            loginMenu.SetActive(false);
            ServerSelectPopup.SetActive(false);
            SetNickBox(false);

            SBGameManager.Instance.IsForceLogout = false;
            curLoadCoroutine = StartCoroutine(LoadingDataShow());
        }
        IEnumerator LoadingDataShow()
        {
            loadingSlider.gameObject.SetActive(true);
            LoadingMessage(0, 1, LoadingState.BUNDLESYNC); //"게임시작준비"
#if DEBUG
            if (AssetBundleManager.UseBundleAssetEditor)
#else
			if (true)
#endif
            {
                yield return AssetBundleManager.AssetInfoSyncCoroutine(LoadingMessage);

                if (AssetBundleManager.IsBundleAssetsDownload)
                {
                    LoadingMessage(0, 1, LoadingState.BUNDLESYNC);
                    yield return AssetBundleManager.AssetBundleFileSyncCoroutine(LoadingMessage);
                }
                else
                {
                    Debug.Log("리소스가 최신상태, AssetBundle 다운로드 받지 않음");
                }
            }
            else
            {
                Debug.Log("Editor 내부 리소스 사용, AssetBundle 다운로드 받지 않음");
            }

            LoadingMessage(0, 1, LoadingState.DATASYNC); //"데이터싱크시작"
            yield return SBGameManager.Instance.GameDataSyncAndLoad(LoadingMessage);

            yield return Resources.UnloadUnusedAssets();

            LoadingMessage(0, 1, LoadingState.CDN_DEFAULT_LOAD); //"CDN 디폴트 로딩 시작"
            yield return CDNManager.CDNResourcePreLoadDefault(LoadingMessage);
            LoadingMessage(0, 1, LoadingState.CDN_LOAD); //"CDN 로딩 시작"
            yield return CDNManager.CDNResourcePreLoad(LoadingMessage);

            //유저 데이터를 불러왔으므로, 최초 가입 유저라면 이곳에서 약관동의를 추가할 수 있음

            LoadingMessage(0, 1, LoadingState.USER_DATA_LOAD); //"유저 데이터 로딩"
            yield return RequestUserData(LoadingMessage);

            LoadingMessage(0, 1, LoadingState.BUNDLECHECK); //"리소스검증"
            yield return UICanvasLoad(LoadingMessage);

            RequestConnectChatServer(); //유저 정보 업데이트 이후 채팅서버 커넥트

            LoadingMessage(0, 1, LoadingState.SCENE_LOAD); // 씬 로드
            yield return LoadScene(LoadingMessage);

            yield return Resources.UnloadUnusedAssets();
        }

        //팝업 프리팹, 사운드 매니져, UICanvas등 미리 Instantiate가 필요한 단계 필요시 이어서 추가
        private IEnumerator UICanvasLoad(DownloadState state)
        {
            SBGameManager.Instance.AddManager(typeof(SoundManager), SoundManager.Instance, false);

            GamePreference.Instance.InitGamePrefData();

            if (UICanvas.Instance == null)
            {
                GameObject uicanvasPrefab = null;
#if DEBUG
                if (false == AssetBundleManager.UseBundleAssetEditor)
                {
                    string uiCanvasPath = SBFunc.StrBuilder("AssetBundle/", SBDefine.ResourcePath(eResourcePath.StaticPrefabUIPath, "UICanvas"));
                    var Request = Resources.LoadAsync(uiCanvasPath);
                    while (false == Request.isDone)
                    {
                        state.Invoke(Mathf.FloorToInt(Request.progress * 100), 100);
                        yield return SBDefine.GetWaitForEndOfFrame();
                    }

                    uicanvasPrefab = (GameObject)Request.asset;
                }
                else
                {
                    yield return ResourceManager.LoadAssetAsync(eResourcePath.StaticPrefabUIPath, "UICanvas", state);
                    uicanvasPrefab = ResourceManager.GetResource<GameObject>(eResourcePath.StaticPrefabUIPath, "UICanvas");
                }
#else
            yield return ResourceManager.LoadAssetAsync(eResourcePath.StaticPrefabUIPath, "UICanvas", state);
            uicanvasPrefab = ResourceManager.GetResource<GameObject>(eResourcePath.StaticPrefabUIPath, "UICanvas");
#endif
                if (uicanvasPrefab != null)
                    Instantiate(uicanvasPrefab);
            }

            LoadingMessage(100, 100, LoadingState.BUNDLECHECK);
            var unloaded = Resources.UnloadUnusedAssets();
            while (false == unloaded.isDone)
            {
                yield return SBDefine.GetWaitForEndOfFrame();
            }

            UIManager.Instance.InitUI(eUIType.None);
        }
        public void RequestConnectChatServer()
        {
            ChatManager.Instance.InitConnectChatServer();
        }
        IEnumerator RequestUserData(DownloadState state)
        {
            var data = new WWWForm();
            data.AddField("token_bin", SBGameManager.Instance.UserAccessToken);

#if !UNITY_EDITOR
            data.AddField("genuineCheckAvailable", Application.genuineCheckAvailable ? 1 : 0);
            data.AddField("genuine", Application.genuine ? 1 : 0);
#endif

            state?.Invoke(0, 30);
            User.Instance.ClearUserData(); // 이유 : rs가 0 이 아니면 유저 데이터 세팅을 타지 않음. 그럼 이전 유저 데이터가 남아 있을 수 있음

            yield return NetworkManager.Instance.SendCorutine(new NetworkManager.NetworkQueue("user/select", data, SetUserData, null));
            yield return SBFunc.DownloadStateEvent(state, 0, 10, 30);

#if UNITY_ANDROID
        if (Application.genuineCheckAvailable)
        {
            if (!Application.genuine)
            {
                Application.Quit();
            }
        }
#endif

            while ((SystemPopup != null && SystemPopup.IsOpening()) || (PreopenPopup != null && PreopenPopup.IsOpening()))
            {
                yield return SBDefine.GetWaitForEndOfFrame();
            }

            // data check
            long userNo = User.Instance.UserAccountData.UserNumber;
            string userNick = User.Instance.UserData.UserNick;


            // 유저 데이터가 잘못 들어왔을 때
            int serverTryCount = 0;
            while (userNo < 1 || userNick == string.Empty)
            {
                if (serverTryCount >= 3) // 3번 시도 했으나 안되면 서버로부터 요청 안하도록 함
                    break;

                yield return NetworkManager.Instance.SendCorutine(new NetworkManager.NetworkQueue("user/select", data, SetUserData, null));
                ++serverTryCount;
                userNo = User.Instance.UserAccountData.UserNumber;
                userNick = User.Instance.UserData.UserNick;
            }

            while ((SystemPopup != null && SystemPopup.IsOpening()) || (PreopenPopup != null && PreopenPopup.IsOpening()))
            {
                yield return SBDefine.GetWaitForEndOfFrame();
            }

            // 서버로부터 유저 기본 정보 못받은 상태라면
            if (userNo < 1 || userNick == string.Empty)
            {
                // 네트워크가 불안정 합니다 다시 시도하시겠습니까?
                //network error에 대한 이벤트 통계를 위하여 추가
                LoginManager.Instance.SetFirebaseEvent("ud_response_error");
                bool IsStopCurCor = true;
                if (curLoadCoroutine != null)
                {
                    StopCoroutine(curLoadCoroutine);
                    curLoadCoroutine = null;
                }
                SystemPopup.SetMessage(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100002661), StringData.GetStringByIndex(100000199));
                SystemPopup.SetCallBack(
                    () =>
                    {
                        curLoadCoroutine = StartCoroutine(LoadingDataShow());
                        IsStopCurCor = false;
                    },
                    () =>
                    {
                        IsStopCurCor = true;
#if UNITY_EDITOR
                        UnityEditor.EditorApplication.isPlaying = false;
#else
						Application.Quit();
#endif
                    },
                    () =>
                    {
                        IsStopCurCor = true;
#if UNITY_EDITOR
                        UnityEditor.EditorApplication.isPlaying = false;
#else
						Application.Quit();
#endif
                    });

                // 다음 코루틴을 절대 못타게 막아둠
                while (IsStopCurCor)
                {
                    yield return null;
                }
                yield break;
            }
            else
            {
                Dictionary<string, string> param = new Dictionary<string, string>();
                param.Add("server_tag", NetworkManager.ServerTag.ToString());
                param.Add("enable_p2e", (User.Instance.ENABLE_P2E ? 1 : 0).ToString());

                bool Authorized = true;

#if UNITY_IOS
                if (Unity.Advertisement.IosSupport.ATTrackingStatusBinding.GetAuthorizationTrackingStatus() != Unity.Advertisement.IosSupport.ATTrackingStatusBinding.AuthorizationTrackingStatus.AUTHORIZED)
                {
                    Authorized  = false;
                }
#endif
                if (PlayerPrefs.GetInt("EEA_USER", (int)EasyMobile.EEARegionStatus.Unknown) == (int)EasyMobile.EEARegionStatus.InEEA)
                {
                    //동의했던 놈인가
                    int prev_authorized = PlayerPrefs.GetInt("EEA_USER_" + userNo.ToString(), 0);
                    if (prev_authorized != 0) 
                    {
                        switch(prev_authorized)
                        {
                            case 1:
                            {   
                                //ok
                                AppsFlyerSDK.AppsFlyerConsent consent = AppsFlyerSDK.AppsFlyerConsent.ForGDPRUser(true, true);
                                AppsFlyerSDK.AppsFlyer.setConsentData(consent);                                
                            }
                            break;
                            case 2:
                            {
                                //no
                                AppsFlyerSDK.AppsFlyerConsent consent = AppsFlyerSDK.AppsFlyerConsent.ForGDPRUser(false, false);
                                AppsFlyerSDK.AppsFlyer.setConsentData(consent);
                            }
                            break;
                        }
                    }
                    else
                    {
                        if (Authorized)
                        {
                            SystemPopup.SetMessage(StringData.GetStringByIndex(100000248), "This app uses your device advertising ID for analytics and personalized advertising.", "Accept", "Reject");
                            SystemPopup.SetCallBack(
                            () =>
                            {
                                //ok
                                AppsFlyerSDK.AppsFlyerConsent consent = AppsFlyerSDK.AppsFlyerConsent.ForGDPRUser(true, true);
                                AppsFlyerSDK.AppsFlyer.setConsentData(consent);
                                PlayerPrefs.SetInt("EEA_USER_" + userNo.ToString(), 1);
                            },
                            () =>
                            {
                                //no
                                AppsFlyerSDK.AppsFlyerConsent consent = AppsFlyerSDK.AppsFlyerConsent.ForGDPRUser(false, false);
                                AppsFlyerSDK.AppsFlyer.setConsentData(consent);
                                PlayerPrefs.SetInt("EEA_USER_" + userNo.ToString(), 2);
                            });

                            while ((SystemPopup != null && SystemPopup.IsOpening()) || (PreopenPopup != null && PreopenPopup.IsOpening()))
                            {
                                yield return SBDefine.GetWaitForEndOfFrame();
                            }
                        }
                        else
                        {
                            AppsFlyerSDK.AppsFlyerConsent consent = AppsFlyerSDK.AppsFlyerConsent.ForGDPRUser(false, false);
                            AppsFlyerSDK.AppsFlyer.setConsentData(consent);
                        }
                    }

                    try
                    {
                        AppsFlyerSDK.AppsFlyer.startSDK();
                    }
                    catch
                    {
                        Debug.LogError("AppsFlyer start Failed");
                    }
                }

                AppsFlyerSDK.AppsFlyer.setCustomerUserId(userNo.ToString());
                AppsFlyerSDK.AppsFlyer.sendEvent("login", param);

                if (GuildManager.Instance.GuildWorkAble)
                    GuildManager.Instance.ReqGuildRanking();

                yield return SBFunc.DownloadStateEvent(state, 10, 20, 30);
                //yield return BattlePassManager.Instance.RefreshPassData(eBattlePassType.HOLDER);
                yield return SBFunc.DownloadStateEvent(state, 20, 30, 30);
                yield break;
            }
        }

        void OnServerList(JObject res)
        {
            User.Instance.UserAccountData.Set(res);
            UpdateUserLoginPrefData(res);

            if (User.Instance.UserAccountData.AuthAccountType == eAuthAccount.GUEST)
                SBGameManager.Instance.PrevGuestAccessToken = SBGameManager.Instance.UserAccessToken;

            ServerState = new Dictionary<int, string>();
            if (res.ContainsKey("server_info") && res["server_info"].Type == JTokenType.Object)
            {
                foreach (var state in ((JObject)res["server_info"]).Properties())
                {
                    JObject obj = (JObject)state.Value;
                    bool use = obj.ContainsKey("use") ? obj["use"].Value<bool>() : false;
                    if (use && obj.ContainsKey("nick"))
                    {
                        ServerState.Add(int.Parse(state.Name), obj["nick"].Value<string>());
                    }
                }
            }

            ShowServerList();
        }

        void ShowServerList()
        {
            ServerSelectPopup.SetActive(true, ServerState, OnSelect, OnCreate);
        }

        void SetUserData(JObject jObject)
        {
            switch (jObject["rs"].Value<int>())
            {
                case (int)eApiResCode.OK: //유저 데이터 채우기 및 씬 이동
                {
                    User.Instance.SetBase(jObject);
                    TableManager.GetTable<ScriptTriggerTable>().OrganizeTrigger();

#if UNITY_EDITOR
                    string guestInfo = AccountLoader.AccountHistory;
                    JArray array = new JArray();
                    if (!string.IsNullOrEmpty(guestInfo))
                        array = JArray.Parse(guestInfo);

                    foreach (JObject info in array)
                    {
                        if (SBFunc.IsJTokenCheck(info["uno"]) && SBFunc.IsJTokenCheck(jObject["uno"]) && info["uno"].Value<long>() == jObject["uno"].Value<long>())
                        {
                            array.Remove(info);
                            break;
                        }
                    }

                    jObject.Add("token_bin", SBGameManager.Instance.UserAccessToken);
                    array.Add(jObject);

                    AccountLoader.AccountHistory = array.ToString();
#endif

                    User.Instance.RefreshDataToInfo();
                    UserDB.VersionCheck(User.Instance.UserAccountData.UserNumber);

                    ChatManager.Instance.LoadDBData();
                    ChatManager.Instance.LoadDBGuildData(User.Instance.UserData.LastGuildNo);

                    UpdateUserLoginPrefData(jObject);

                    userInit = true;

                    LoginManager.Instance.SetFirebaseEvent(Firebase.Analytics.FirebaseAnalytics.EventLogin);

                    labelClientVer.text = "ClientVer. " + GamePreference.Instance.VERSION + "/" + ClientConstants.CurrentPlatform + "-" + NetworkManager.ServerName;
                }
                break;
                case (int)eApiResCode.ACCOUNT_IS_BANNED:
                    if (curLoadCoroutine != null)
                    {
                        StopCoroutine(curLoadCoroutine);
                        curLoadCoroutine = null;
                    }

                    SystemPopup.SetMessage(StringData.GetStringByIndex(100000614), StringData.GetStringByStrKey("account_banned"));
                    SystemPopup.SetCallBack(() =>
                    {
                        curLoadCoroutine = StartCoroutine(LoadingDataShow());
                    });
                    break;
                case (int)eApiResCode.ACCOUNT_IS_DELETED:
                    if (curLoadCoroutine != null)
                    {
                        StopCoroutine(curLoadCoroutine);
                        curLoadCoroutine = null;
                    }

                    SystemPopup.SetMessage(StringData.GetStringByIndex(100000614), StringData.GetStringByStrKey("account_deleted"));
                    SystemPopup.SetCallBack(() =>
                    {
                        curLoadCoroutine = StartCoroutine(LoadingDataShow());
                    });
                    break;
            }
        }
        void LoadingMessage(int gage, int maxGage, LoadingState state)
        {
            if (loadingSlider == null)
                return;

            loadingSlider.value = 0f;

            string text = "";
            switch (state)
            {
                case LoadingState.BUNDLESYNC:
                    text = StringData.GetStringByStrKey("로딩가십1");
                    break;
                case LoadingState.DATASYNC:
                    text = StringData.GetStringByStrKey("로딩가십2");
                    break;
                case LoadingState.CDN_DEFAULT_LOAD:
                    text = StringData.GetStringByStrKey("로딩가십3");
                    break;
                case LoadingState.CDN_LOAD:
                    text = StringData.GetStringByStrKey("로딩가십4");
                    break;
                case LoadingState.USER_DATA_LOAD:
                    text = StringData.GetStringByStrKey("로딩가십5");
                    break;
                case LoadingState.BUNDLECHECK:
                    text = StringData.GetStringByStrKey("로딩가십6");
                    break;
                case LoadingState.SOUNDLOAD:
                    text = StringData.GetStringByStrKey("로딩가십7");
                    break;
                case LoadingState.PRELOAD:
                    text = StringData.GetStringByStrKey("로딩가십8");
                    break;
                case LoadingState.SCENE_LOAD:
                    text = StringData.GetStringByStrKey("로딩가십9");
                    break;
            }

            loadingInfoText.text = text + " (" + (int)state + "/" + (int)LoadingState.LOADMAX + ")";
            curLoadingState = state;
            LoadingMessage(gage, maxGage);
        }
        void LoadingMessage(int gage, int maxGage)
        {
            if (loadingSlider == null)
                return;

            loadingSlider.value = gage;
            loadingSlider.maxValue = maxGage;
        }
        IEnumerator LoadScene(DownloadState state)  //씬로드 및 글자 표시
        {
            var popup = PopupManager.GetPopup<PostListPopup>();
            state?.Invoke(0, 10);
            if (popup != null)
            {
                popup.GetPostList();
            }

            //이벤트(출첵)데이터 사전 요청
            EventAttendancePopup.RequestEventAttendance();

            SceneManager.sceneLoaded -= LoadStartSceneEnd;
            SceneManager.sceneLoaded += LoadStartSceneEnd;
            AsyncOperation op = SceneManager.LoadSceneAsync("Town");
            op.allowSceneActivation = false;
            loading = true;

            while (!op.isDone)
            {
                if (op.progress >= 0.9f)
                {
                    enterPossible = true;

                    if (loading)
                    {
                        StartCoroutine(TouchMessage());
                    }
                    loading = false;
                }
                else
                {
                    state?.Invoke(Mathf.RoundToInt(op.progress * 10), 10);
                }

                if (enterClicked)
                {
                    op.allowSceneActivation = true;
                }

                yield return null;
            }
        }
        IEnumerator TouchMessage()
        {
            loadingInfoText.text = "Touch to Start";
            loadingInfoText.fontSize = 50;

            loadingSlider.gameObject.SetActive(false);
            btnLogout.SetActive(true);

            btnServerChange.SetActive(true);

            Graphic[] gComps = loadingInfoText.GetComponents<Graphic>();
            Outline[] oComps = loadingInfoText.GetComponents<Outline>();

            while (!enterClicked)
            {
                for (int i = 0; i < gComps.Length; i++)
                {
                    gComps[i].DOColor(new Color(1, 1, 1, 1), 0.5f);
                }
                for (int i = 0; i < oComps.Length; i++)
                {
                    oComps[i].DOColor(new Color(0, 0, 0, 1), 0.5f);
                }

                yield return SBDefine.GetWaitForSeconds(0.5f);

                for (int i = 0; i < gComps.Length; i++)
                {
                    gComps[i].DOColor(new Color(1, 1, 1, 0), 0.5f);
                }
                for (int i = 0; i < oComps.Length; i++)
                {
                    oComps[i].DOColor(new Color(0, 0, 0, 0), 0.5f);
                }

                yield return SBDefine.GetWaitForSeconds(0.5f);
            }

        }
        public void OnClickStart()
        {
            if (false == enterPossible)
                return;

            NetworkManager.Send("system/start", null, (data) =>
            {
                StopCoroutine(TouchMessage());
                GameStart();
            });
        }
        private void GameStart()
        {
            LoadingManager.Instance.CloudAnimation(fadeInTime, fadeOutTime, () =>
            {
                SystemMessage.SetLoadEnd();
                enterClicked = enterPossible;
            });
        }
        private void LoadStartSceneEnd(Scene scene, LoadSceneMode loadSceneMode)
        {
            if (scene.name == "Town")
            {
                loading = false;

                SceneManager.sceneLoaded -= LoadStartSceneEnd;
                LoadingManager.Instance.initSceneStack();
                LoadingManager.Instance.OnPostStartLoading();
            }
        }
        public void OnAppleLogin()
        {
            //loginMenu.SetActive(false);

            appleLogin.OnAppleLogin();

            //SandboxNetwork.SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100002671), StringData.GetStringByStrKey("개발중입니다."), () => { loginMenu.SetActive(true); });
        }
        public void OnGoogleLogin()
        {
            //loginMenu.SetActive(false);

            googleLogin.OnGoogleLogin();
        }

        public void OnIMXLogin()
        {
            imxLogin.OnIMXLogin(()=> {
                loginMenu.SetActive(false);
                busyPanel.SetActive(true);
            },
            ()=> {
                loginMenu.SetActive(true);
                busyPanel.SetActive(false);
            });
        }
        public void OnGuestLogin()
        {
            if (!string.IsNullOrEmpty(SBGameManager.Instance.PrevGuestAccessToken))
            {
                SystemPopup.SetMessage(StringData.GetStringByIndex(100000248), StringData.GetStringByStrKey("신규게스트계정생성알림"), StringData.GetStringByIndex(100000199), StringData.GetStringByStrKey("초기화"));
                SystemPopup.SetCallBack(
                    () =>
                    {
                        SBGameManager.Instance.UserAccessToken = SBGameManager.Instance.PrevGuestAccessToken;
                        OnSignIn();
                    },
                    () =>
                    {
                        OnSignUp();
                    }
                );
                return;
            }

            OnSignUp();
        }
        public void OnAutoLogin()
        {
            var data = new WWWForm();
            data.AddField("token_bin", SBGameManager.Instance.UserAccessToken);

            if (ServerState == null)
            {
                ServerState = new Dictionary<int, string>();
                NetworkManager.Send("auth/signin", data, (res) =>
                {
                    if (res != null)
                    {
                        if (res.ContainsKey("server_info") && res["server_info"].Type == JTokenType.Object)
                        {
                            foreach (var state in ((JObject)res["server_info"]).Properties())
                            {
                                JObject obj = (JObject)state.Value;
                                bool use = obj.ContainsKey("use") ? obj["use"].Value<bool>() : false;
                                if (use && obj.ContainsKey("nick"))
                                {
                                    ServerState.Add(int.Parse(state.Name), obj["nick"].Value<string>());
                                }
                            }
                        }

                        User.Instance.UserAccountData.Set(res);
                        
                        if (ServerState.ContainsKey(SBGameManager.CurServerTag))
                        {
                            OnLogin(SBGameManager.Instance.UserAccessToken);
                        }
                        else
                        {
                            ShowServerList();
                        }
                    }
                });
            }
            else
            {
                if (ServerState.ContainsKey(SBGameManager.CurServerTag))
                {
                    OnLogin(SBGameManager.Instance.UserAccessToken);
                }
                else
                {
                    ShowServerList();
                }
            }
        }
        private void OnLogin(string accessToken)
        {
            loginMenu.SetActive(false);

            // 액세스 토큰 여부 확인
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                SetNickBox(true);
            }
            else // 저장된 토큰이 있을 경우 로그인 자동 진행
            {
                Init();
            }
        }
        private void OnSignUp()
        {
            loginMenu.SetActive(false);
            SetTermBox(true, currentLoginData, OnServerList);
        }

        private void OnSignIn()
        {
            loginMenu.SetActive(false);

            if (ServerState == null)
            {
                var data = new WWWForm();
                data.AddField("token_bin", SBGameManager.Instance.UserAccessToken);

                NetworkManager.Send("auth/signin", data, OnServerList);
            }
            else
            {
                ShowServerList();
            }
        }

        private void OnCreate()
        {
            ServerState = null;
            SetNickBox(true, currentLoginData);
        }

        public void OnSelect()
        {
            if (curLoadCoroutine != null)
            {
                //이미 다 로드 마친다음에 서버 바꾸던지 하면 강제 재시작

                SceneManager.LoadScene("Start");
            }
            else
            {
                Init();
            }
        }

        public void OnLogOut()
        {
            User.Instance.UserAccountData.Clear();
            SBGameManager.Instance.UserAccessToken = "";
            SBGameManager.Instance.UserNickname = "";

            SceneManager.LoadScene("Start");
        }
        public void UpdateUserLoginPrefData(JObject jObject)
        {
            if (SBFunc.IsJTokenCheck(jObject["nick"]))
            {
                SBGameManager.Instance.UserNickname = jObject["nick"].Value<string>();
            }
            else
            {
                if (SBFunc.IsJObject(jObject["user_base"]))
                {
                    JObject ub = (JObject)jObject["user_base"];
                    if (SBFunc.IsJTokenCheck(ub["nick"]))
                    {
                        SBGameManager.Instance.UserNickname = ub["nick"].Value<string>();
                    }
                }
            }

            if (SBFunc.IsJTokenCheck(jObject["token_bin"]))
            {
                SBGameManager.Instance.UserAccessToken = jObject["token_bin"].Value<string>();
            }
        }
    }
}
