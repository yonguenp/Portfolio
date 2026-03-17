using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SandboxPlatform.SAMANDA;
using Newtonsoft.Json.Linq;
using UnityEngine.Networking;
using DG.Tweening;
using Spine.Unity;
using System;
using SBCommonLib;
using EasyMobile;

public class StartScene : BaseScene
{
    [SerializeField]
    GameObject LobbyButton;

    [SerializeField]
    string SAMANDA_ProductID = "WTSCAm";
    [SerializeField]
    Text VersionText;
    [SerializeField]
    Text BranchText;
    [SerializeField]
    Text StateText;
    [SerializeField]
    GameObject NickNamePopup;
    [SerializeField]
    InputField NickField;
    [SerializeField]
    TitleAnimation TitleAnimation;

    string account_jwt = "";
    Coroutine loginProcess = null;

    protected override void Init()
    {

    }

    private void Start()
    {
        Managers.GameServer.ClearDisconnectCallback();
        Managers.GameServer.Disconnect();
        Managers.Network.ClearDisconnectCallback();
        Managers.Network.Disconnect();

        TitleAnimation.SetTitle();
        Game.ClearInstance();
        Managers.Instance.ClearSingleton();
        PopupCanvas.Instance.ClearAll();

        NickNamePopup.SetActive(false);

        if (loginProcess != null)
        {
            StopCoroutine(loginProcess);
            loginProcess = null;
        }

        SAMANDA.Instance.StartSamanda(SAMANDA_ProductID, LoginDone, LogOut, SplashDone);

        VersionText.text = GameConfig.Instance.VERSION;
        BranchText.text = GameConfig.Instance.BUILD_BRANCH;

        SBDebug.Log(SystemInfo.graphicsDeviceVersion);
    }

    public void SplashDone()
    {
        if (loginProcess != null)
        {
            StopCoroutine(loginProcess);
            loginProcess = null;
        }

        loginProcess = StartCoroutine(LoginProcess());

        StartBackgroundMusic();
        TitleAnimation.OnTitleAnimation();
    }
    public override void Clear()
    {
    }

    public void LoginDone(string JWT)
    {
        account_jwt = JWT;
        SBDebug.Log("SAMANDA JWT : " + account_jwt);

#if UNITY_EDITOR
        string savedJWT = PlayerPrefs.GetString("JWT_FOR_EDITOR");
        List<string> jwtArray = new List<string>(savedJWT.Split(','));

        string curToken = SAMANDA.Instance.ACCOUNT_TOK;

        for (int i = 0; i < jwtArray.Count; i++)
        {
            if (string.IsNullOrEmpty(jwtArray[i]))
            {
                jwtArray[i] = string.Empty;
                continue;
            }

            string[] info = jwtArray[i].Split('/');
            if (info.Length != 3)
            {
                jwtArray[i] = string.Empty;
                continue;
            }

            if (info[1] == curToken)
            {
                jwtArray[i] = string.Empty;
            }
        }

        while (jwtArray.Contains(string.Empty))
        {
            jwtArray.Remove(string.Empty);
        }

        string jwtInfo = (GameConfig.Instance.USE_TEST_SERVER ? "TEST" : GameConfig.Instance.USE_TEST_SERVER ? "QA" : "LIVE") + "/" + SAMANDA.Instance.ACCOUNT_TOK + "/" + SAMANDA.Instance.ACCOUNT_ANO;
        jwtArray.Add(jwtInfo);

        jwtArray.Reverse();
        PlayerPrefs.SetString("JWT_FOR_EDITOR", string.Join(",", jwtArray));
#endif        
    }

    public void OnNickCheck()
    {
        WWWForm param = new WWWForm();
        string nick = NickField.text;
        NickField.text = "";

        if (string.IsNullOrEmpty(nick))
        {
            return;
        }

        if (Crosstales.BWF.BWFManager.Instance.Contains(nick))
        {
            PopupCanvas.Instance.ShowFadeText("닉네임_문구4");
            return;
        }

        param.AddField("nick", nick);

        SBWeb.SendPost("account/check-nick", param, (response) =>
        {
            JObject res = (JObject)response;
            switch (res["rs"].Value<int>())
            {
                case 0:
                    PopupCanvas.Instance.ShowFadeText(StringManager.GetString("닉네임_문구1"));
                    break;
                case 203:
                    PopupCanvas.Instance.ShowFadeText(StringManager.GetString("닉네임_문구2"));
                    break;
                case 204:
                    PopupCanvas.Instance.ShowFadeText(StringManager.GetString("닉네임_문구3"));
                    break;
                case 205:
                    PopupCanvas.Instance.ShowFadeText(StringManager.GetString("닉네임_문구4"));
                    break;
                default:
                    PopupCanvas.Instance.ShowFadeText(StringManager.GetString("로그인스탭_20"));
                    break;
            }
        });
    }
    public void OnCreateAccount()
    {
        WWWForm param = new WWWForm();
        string nick = NickField.text;
        NickField.text = "";

        if (string.IsNullOrEmpty(nick))
        {
            return;
        }

        if (Crosstales.BWF.BWFManager.Instance.Contains(nick))
        {
            PopupCanvas.Instance.ShowFadeText(StringManager.GetString("닉네임_문구4"));
            return;
        }

        param.AddField("nick", nick);
        param.AddField("jwt", account_jwt);


        //BadWordManager.Instance.ReplaceAll(NickField.text);
        SBWeb.SendPost("account/signup", param, (response) =>
        {
            JObject res = (JObject)response;
            switch (res["rs"].Value<int>())
            {
                case 0:
                    {
                        NickNamePopup.SetActive(false);
                        OnUserData(res);

                        com.adjust.sdk.AdjustEvent adjustEvent = new com.adjust.sdk.AdjustEvent("t2w1af");
                        com.adjust.sdk.Adjust.trackEvent(adjustEvent);

                        Managers.UserData.RANK_SEANSON_ID = 0;

                        PopupCanvas.Instance.ShowFadeText(StringManager.GetString("계정생성_1"));
                    }
                    break;
                case 203:
                    PopupCanvas.Instance.ShowFadeText(StringManager.GetString("닉네임_문구2"));
                    break;
                case 204:
                    PopupCanvas.Instance.ShowFadeText(StringManager.GetString("닉네임_문구3"));
                    break;
                case 205:
                    PopupCanvas.Instance.ShowFadeText(StringManager.GetString("닉네임_문구4"));
                    break;
                default:
                    PopupCanvas.Instance.ShowFadeText(StringManager.GetString("로그인스탭_20"));
                    break;
            }
        });
    }

    void OnUserData(JObject userData)
    {
        Managers.UserData.OnUserData(userData);
    }

    static void LogOut()
    {
        GameManager.Instance.ClearSingleton();
        Managers.Instance.ClearSingleton();

        UnityEngine.SceneManagement.SceneManager.LoadScene("Start");
    }

    public void OnEnterLobby()
    {
        if (!GameConfig.Instance.IS_VERSION_VALIDATE)
        {
            PopupCanvas.Instance.ShowConfirmPopup(StringManager.GetString("업데이트필요"), StringManager.GetString("ui_update"), StringManager.GetString("ui_notice_check"), () =>
            {
                Application.OpenURL(
#if UNITY_IOS
                        GameConfig.Instance.APPLE_STORE_URL
#else
                        GameConfig.Instance.GOOGLE_STORE_URL
#endif
                        );
            }, () =>
            {
                SAMANDA.Instance.UI.SetUIState(LOGIN_STATE.MAIN_OPEN);
            });
            return;
        }

        if (Managers.Network.IsAlive())
        {
            com.adjust.sdk.AdjustEvent adjustEvent = new com.adjust.sdk.AdjustEvent("r1ysk1");
            com.adjust.sdk.Adjust.trackEvent(adjustEvent);
            Managers.Scene.LoadScene(SceneType.Lobby);
        }
        else
        {
            if (!Managers.Network.IsAlive())
            {
                SAMANDA.Instance.UI.SetMainCloseCallback(() =>
                {
                    Application.OpenURL("https://game.naver.com/lounge/Moris_Nightmare_Hide_and_seek_Multi/home");
                    Application.Quit();
                });
                SAMANDA.Instance.UI.SetUIState(LOGIN_STATE.MAIN_OPEN);
            }
        }
    }

    IEnumerator LoginProcess()
    {
        StateText.text = "";
        yield return StartCoroutine(VersionCheck());

#if UNITY_IOS
        com.adjust.sdk.Adjust.requestTrackingAuthorizationWithCompletionHandler((status) =>
        {
            switch (status)
            {
                case 0:
                    // ATTrackingManagerAuthorizationStatusNotDetermined case
                    break;
                case 1:
                    // ATTrackingManagerAuthorizationStatusRestricted case
                    break;
                case 2:
                    // ATTrackingManagerAuthorizationStatusDenied case
                    break;
                case 3:
                    // ATTrackingManagerAuthorizationStatusAuthorized case
                    break;
            }
        });
#endif

#if !UNITY_EDITOR
        if (Notifications.DataPrivacyConsent == ConsentStatus.Unknown)
        {
            Notifications.GrantDataPrivacyConsent();
            Firebase.Messaging.FirebaseMessaging.TokenRegistrationOnInitEnabled = true;
        }        
#endif


        while (LOGIN_STATE.LOGIN_DONE != SAMANDA.Instance.GetLoginState())
        {
            string msg = "";
            switch (SAMANDA.Instance.GetLoginState())
            {
                case LOGIN_STATE.UNKNOWN:
                    msg = "로그인스탭_1";
                    break;
                case LOGIN_STATE.NO_ACCOUNT_INFO:
                    msg = "로그인스탭_2";
                    break;
                case LOGIN_STATE.VALID_ACCOUNT:
                    msg = "로그인스탭_3";
                    break;
                case LOGIN_STATE.TERMSOFUSE:
                    msg = "로그인스탭_4";
                    break;
            }
            StateText.text = StringManager.GetString(msg);

            yield return new WaitForEndOfFrame();
        }

        com.adjust.sdk.AdjustEvent adjustEvent = new com.adjust.sdk.AdjustEvent("atje3g");
        com.adjust.sdk.Adjust.trackEvent(adjustEvent);

        adjustEvent = new com.adjust.sdk.AdjustEvent("7g5jgh");
        com.adjust.sdk.Adjust.trackEvent(adjustEvent);

        StateText.text = StringManager.GetString("로그인스탭_5");
        yield return new WaitForEndOfFrame();

        while (string.IsNullOrEmpty(account_jwt))
        {
            yield return new WaitForSeconds(0.1f);
        }

        StateText.text = StringManager.GetString("로그인스탭_6");
        yield return new WaitForSeconds(0.1f);

        //---------------------------------------------------------------
        SBDebug.Log("에센번들 업데이트 시작 @@@@@@@@@@@@@@@@@@@");
        yield return StartCoroutine(AssetBundleManager.AssetBundleFileSyncCoroutine());
        SBDebug.Log("에센번들 업데이트 종료 @@@@@@@@@@@@@@@@@@@");
        yield return new WaitForEndOfFrame();

        yield return StartCoroutine(DataFileSync());
        // config와 string을 먼저 load 해준다.
        Managers.Data.Init();

        StateText.text = StringManager.GetString("resource_update");
        yield return new WaitForSeconds(0.1f);

        StateText.text = StringManager.GetString("로그인스탭_7");
        yield return new WaitForEndOfFrame();

        yield return StartCoroutine(Managers.Data.InitAsync());

        StateText.text = StringManager.GetString("로그인스탭_8");
        yield return new WaitForEndOfFrame();

        yield return StartCoroutine(MapDataFileSync());

        yield return new WaitForEndOfFrame();
        SignInGame();

        while (string.IsNullOrEmpty(Managers.UserData.MyWebSessionID))
        {
            yield return new WaitForEndOfFrame();
        }

        if (GameConfig.Instance.IS_INSPECTION)
        {
            StateText.text = StringManager.GetString("로그인스탭_17");

            //cancel -> 공식라운지 보내기 ok -> 앱종료
            PopupCanvas.Instance.ShowConfirmPopup(StringManager.GetString("점검중_팝업안내"), oktext: StringManager.GetString("button_check"), canceltext: StringManager.GetString("ui_notice_check"), okcb: () =>
            {
                Application.Quit();
            }, cancelcb: () =>
            {
                Application.OpenURL("https://game.naver.com/lounge/Moris_Nightmare_Hide_and_seek_Multi/home");
                Application.Quit();
            });

            //SAMANDA.Instance.UI.SetMainCloseCallback(() => {
            //    Application.OpenURL("https://game.naver.com/lounge/Moris_Nightmare_Hide_and_seek_Multi/home");
            //    Application.Quit();
            //});
            //SAMANDA.Instance.UI.SetUIState(LOGIN_STATE.MAIN_OPEN);

            while (true)
            {
                yield return new WaitForEndOfFrame();
            }
        }

        StateText.text = StringManager.GetString("로그인스탭_9");
        yield return new WaitForEndOfFrame();

        float limitTime = 5.0f;
        while (Managers.Network.IsAlive() && limitTime > 0.0f)
        {
            limitTime -= Time.deltaTime;
            StateText.text = StringManager.GetString("네트워크초기화중");
            yield return new WaitForEndOfFrame();
        }

        limitTime = 5.0f;
        while (Managers.GameServer.IsAlive() && limitTime > 0.0f)
        {
            limitTime -= Time.deltaTime;
            StateText.text = StringManager.GetString("네트워크초기화중");
            yield return new WaitForEndOfFrame();
        }

        Managers.Network.OnConnect(ServerInfo.Instance.IP, ServerInfo.Instance.PORT, OnConnected, Managers.Instance.Disconnected);

        float timeCheck = 10.0f;
        while (string.IsNullOrEmpty(Managers.UserData.MySocketSessionID) && GameConfig.Instance.IS_VERSION_VALIDATE)
        {
            timeCheck -= Time.deltaTime;

            if (timeCheck < 0)
            {
                SAMANDA.Instance.UI.SetMainCloseCallback(() =>
                {
                    Application.OpenURL("https://game.naver.com/lounge/Moris_Nightmare_Hide_and_seek_Multi/home");
                    Application.Quit();
                });
                SAMANDA.Instance.UI.SetUIState(LOGIN_STATE.MAIN_OPEN);

                StateText.text = StringManager.GetString("로그인스탭_20");
                while (true)
                {
                    yield return new WaitForEndOfFrame();
                }
            }

            yield return new WaitForEndOfFrame();
        }

        StateText.text = StringManager.GetString("로그인스탭_10");
        yield return new WaitForEndOfFrame();

        StateText.text = StringManager.GetString("로그인스탭_11");
        yield return new WaitForEndOfFrame();

        //StateText.text = StringManager.GetString("로그인스탭_23");
        //yield return StartCoroutine(BannerImageResourceSync());

        StateText.text = "";

        while (!TitleAnimation.titleAnimDone)
        {
            yield return new WaitForEndOfFrame();
        }

        if (!Managers.PlayData.IsResume)
            OnEnterLobby();
    }

    void OnConnected()
    {
        try
        {
            Managers.Network.SendInit(Managers.UserData.MyUserID, Managers.UserData.MyWebSessionID);
        }
        catch (System.Exception e)
        {
            SBDebug.LogException(e);
        }
    }
    IEnumerator VersionCheck()
    {
        StateText.text = StringManager.GetString("버전체크중");
        yield return new WaitForEndOfFrame();

        WWWForm param = new WWWForm();

        int marketPlatform = 0;
        switch (Application.platform)
        {
            case RuntimePlatform.Android:
                marketPlatform = 1;
                break;
            case RuntimePlatform.IPhonePlayer:
                marketPlatform = 2;
                break;
            case RuntimePlatform.WindowsPlayer:
            case RuntimePlatform.WindowsEditor:
                marketPlatform = 9;
                break;
        }

        param.AddField("platform", marketPlatform);
        param.AddField("version", GameConfig.Instance.VERSION);

        using (UnityWebRequest req = UnityWebRequest.Post(SBWeb.URL + "config/config", param))
        {
            req.timeout = 10;

            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.Success)
            {
                string body = req.downloadHandler.text;
                JObject response = (JObject)JToken.Parse(body);

                if (response.ContainsKey("rs"))
                {
                    int rs = response["rs"].Value<int>();
                    if (rs != 0)
                    {
                        switch (rs)
                        {
                            case 13:
                                SAMANDA.Instance.UI.SetActive(false);
                                StateText.text = StringManager.GetString("업데이트필요");
                                PopupCanvas.Instance.ShowConfirmPopup(StringManager.GetString("업데이트필요"), StringManager.GetString("ui_update"), StringManager.GetString("ui_notice_check"), () =>
                                {
                                    Application.OpenURL(
#if UNITY_IOS
                                        GameConfig.Instance.APPLE_STORE_URL
#else
                                        GameConfig.Instance.GOOGLE_STORE_URL
#endif
                                    );
                                }, () =>
                                {
                                    SAMANDA.Instance.UI.SetUIState(LOGIN_STATE.MAIN_OPEN);
                                });
                                break;
                            default:
                                StateText.text = StringManager.GetString("로그인스탭_19");
                                SBDebug.LogError("오류!!!!!!!!!!!!!!!");
                                break;
                        }

                        while (true)
                        {
                            yield return new WaitForEndOfFrame();
                        }
                    }
                    else
                    {
                        if(response.ContainsKey("inspection"))
                        {
                            PopupCanvas.Instance.ShowConfirmPopup(response["inspection"].Value<string>(), oktext: StringManager.GetString("button_check"), canceltext: StringManager.GetString("ui_notice_check"), okcb: () =>
                            {
                                Application.Quit();
                            }, cancelcb: () =>
                            {
                                Application.OpenURL("https://game.naver.com/lounge/Moris_Nightmare_Hide_and_seek_Multi/board/11");
                                Application.Quit();
                            });

                            while (true)
                            {
                                yield return new WaitForEndOfFrame();
                            }
                        }
                        if (response.ContainsKey("bundle_version"))
                        {
                            GameConfig.Instance.SetBundleVersion(response["bundle_version"].Value<int>());
                        }
                        if (response.ContainsKey("cdn_url"))
                        {
                            GameConfig.Instance.SetCDNURL(response["cdn_url"].Value<string>());
                        }

                        StateText.text = StringManager.GetString("버전체크완료");
                        yield return new WaitForEndOfFrame();
                    }
                }
            }
            else
            {
#if !UNITY_EDITOR
                StateText.text = StringManager.GetString("로그인스탭_19");
                SBDebug.LogError("오류!!!!!!!!!!!!!!!");
                while (true)
                {
                    yield return new WaitForEndOfFrame();
                }
#endif
            }
        }
    }
    IEnumerator DataFileSync()
    {
        StateText.text = StringManager.GetString("로그인스탭_12");
        yield return new WaitForEndOfFrame();

        List<string> fileList = new List<string>();
        for (GameDataManager.DATA_TYPE type = GameDataManager.DATA_TYPE.DATA_MIN + 1; type < GameDataManager.DATA_TYPE.DATA_COUNT; type++)
        {
            fileList.Add(type.ToString());
        }

        WWWForm files = new WWWForm();
        files.AddField("files", string.Join(",", fileList));

        StateText.text = StringManager.GetString("로그인스탭_13");
        yield return new WaitForEndOfFrame();

        using (UnityWebRequest req = UnityWebRequest.Post(SBWeb.URL + "assets/filechecker", files))
        {
            req.timeout = 10;

            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.Success)
            {
                StateText.text = StringManager.GetString("로그인스탭_14");
                yield return new WaitForEndOfFrame();

                fileList.Clear();
                string body = req.downloadHandler.text;
                JObject response = (JObject)JToken.Parse(body);

                if (response.ContainsKey("rs"))
                {
                    if (response["rs"].Value<int>() != 0)
                    {
                        SBDebug.LogWarning("Sync data error : " + response["rs"].Value<int>());
                    }
                }

                for (GameDataManager.DATA_TYPE type = GameDataManager.DATA_TYPE.DATA_MIN + 1; type < GameDataManager.DATA_TYPE.DATA_COUNT; type++)
                {
                    string md5 = GameDataManager.GetMD5hashData(type);

                    if (!response.ContainsKey(type.ToString()))
                    {
                        if (GameDataManager.Instance.UseLocalData())
                            continue;

                        StateText.text = StringManager.GetString("로그인스탭_17", type.ToString());
                        SBDebug.LogError("오류!!!!!!!!!!!!!!!" + type.ToString());
                        while (true)
                        {
                            yield return new WaitForEndOfFrame();
                        }
                    }

                    if (md5 != response[type.ToString()].Value<string>())
                        fileList.Add(type.ToString());
                }

                if (fileList.Count > 0)
                {
                    StateText.text = StringManager.GetString("로그인스탭_15");
                    yield return new WaitForEndOfFrame();

                    int index = 1;
                    foreach (string file in fileList)
                    {
                        StateText.text = StringManager.GetString("로그인스탭_16", string.Format("{0}/{1} : ", index, fileList.Count) + file);
                        yield return new WaitForEndOfFrame();

                        string url = SBWeb.URL + "assets/" + file + ".csv";
                        UnityWebRequest www = UnityWebRequest.Get(url);

                        yield return www.SendWebRequest();

                        if (req.result != UnityWebRequest.Result.Success)
                        {
                            StateText.text = StringManager.GetString("로그인스탭_17", string.Format("{0}/{1} : ", index, fileList.Count) + file);
                            SBDebug.LogError("오류!!!!!!!!!!!!!!!");
                            while (true)
                            {
                                yield return new WaitForEndOfFrame();
                            }
                        }

                        GameDataManager.SetSaveGameData(file, www.downloadHandler.text);
                        index++;
                    }
                }


                StateText.text = StringManager.GetString("로그인스탭_18");
                yield return new WaitForEndOfFrame();
            }
            else
            {
                StateText.text = StringManager.GetString("로그인스탭_19");
                SBDebug.LogError("오류!!!!!!!!!!!!!!!");
                while (true)
                {
                    yield return new WaitForEndOfFrame();
                }
            }
        }
    }

    IEnumerator MapDataFileSync()
    {
        StateText.text = StringManager.GetString("맵데이터싱크1");
        yield return new WaitForEndOfFrame();

        List<string> fileList = MapTypeGameData.GetMapFileList();
        WWWForm files = new WWWForm();
        files.AddField("files", string.Join(",", fileList));

        StateText.text = StringManager.GetString("맵데이터싱크2");
        yield return new WaitForEndOfFrame();

        using (UnityWebRequest req = UnityWebRequest.Post(SBWeb.URL + "assets/mapfilechecker", files))
        {
            req.timeout = 10;

            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.Success)
            {
                StateText.text = StringManager.GetString("맵데이터싱크3");
                yield return new WaitForEndOfFrame();

                List<string> checkList = new List<string>();
                string body = req.downloadHandler.text;
                JObject response = (JObject)JToken.Parse(body);

                if (response.ContainsKey("rs"))
                {
                    if (response["rs"].Value<int>() != 0)
                    {
                        SBDebug.LogWarning("Sync data error : " + response["rs"].Value<int>());
                    }
                }

                for (int i = 0; i < fileList.Count; i++)
                {
                    string md5 = GameDataManager.GetMD5hashData(fileList[i]);

                    if (!response.ContainsKey(fileList[i]))
                    {
                        StateText.text = StringManager.GetString("로그인스탭_17", fileList[i]);
                        SBDebug.LogError("오류!!!!!!!!!!!!!!!" + fileList[i]);
                        while (true)
                        {
                            yield return new WaitForEndOfFrame();
                        }
                    }

                    if (md5 != response[fileList[i].ToString()].Value<string>())
                        checkList.Add(fileList[i]);
                }

                if (checkList.Count > 0)
                {
                    StateText.text = StringManager.GetString("맵데이터싱크4");
                    yield return new WaitForEndOfFrame();

                    int index = 1;
                    foreach (string file in checkList)
                    {
                        StateText.text = StringManager.GetString("맵데이터싱크5", string.Format("{0}/{1} : ", index, checkList.Count) + file);
                        yield return new WaitForEndOfFrame();

                        string url = SBWeb.URL + "assets/" + file + ".json";
                        UnityWebRequest www = UnityWebRequest.Get(url);

                        yield return www.SendWebRequest();

                        if (req.result != UnityWebRequest.Result.Success)
                        {
                            StateText.text = StringManager.GetString("맵데이터싱크5", string.Format("{0}/{1} : ", index, checkList.Count) + file);
                            SBDebug.LogError("오류!!!!!!!!!!!!!!!");
                            while (true)
                            {
                                yield return new WaitForEndOfFrame();
                            }
                        }

                        GameDataManager.SetSaveGameData(file, www.downloadHandler.text);
                        index++;
                    }
                }


                StateText.text = StringManager.GetString("맵데이터싱크6");
                yield return new WaitForEndOfFrame();
            }
            else
            {
                StateText.text = StringManager.GetString("로그인스탭_19");
                SBDebug.LogError("오류!!!!!!!!!!!!!!!");
                while (true)
                {
                    yield return new WaitForEndOfFrame();
                }
            }
        }
    }

    //IEnumerator BannerImageResourceSync()
    //{

    //}

    void SignInGame(int trycount = 0)
    {
        trycount++;
        if (trycount > 3)
        {
            OnServerError();
            return;
        }

        WWWForm jwt = new WWWForm();
        jwt.AddField("jwt", account_jwt);
        jwt.AddField("version", GameConfig.Instance.VERSION);

        SBWeb.SendPost("account/signin", jwt, (response) =>
        {
            JObject res = (JObject)response;
            switch (res["rs"].Value<int>())
            {
                case 0:
                    StateText.text = StringManager.GetString("로그인완료");
                    OnUserData(res);
                    break;
                case 206://no account
                    StateText.text = StringManager.GetString("계정생성필요");
                    NickNamePopup.SetActive(true);
                    com.adjust.sdk.AdjustEvent adjustEvent = new com.adjust.sdk.AdjustEvent("f3blu6");
                    com.adjust.sdk.Adjust.trackEvent(adjustEvent);
                    break;
                case 102://no account
                    StateText.text = StringManager.GetString("REMOVED_ACCOUNT");
                    break;
                case 103: //ban account 
                    StateText.text = StringManager.GetString("BANNED_ACCOUNT");
                    break;
                case 13://버전 업데이트 필요
                    StateText.text = StringManager.GetString("업데이트필요");
                    PopupCanvas.Instance.ShowConfirmPopup(StringManager.GetString("업데이트필요"), StringManager.GetString("ui_update"), StringManager.GetString("ui_notice_check"), () =>
                    {
                        Application.OpenURL(
#if UNITY_IOS
                                GameConfig.Instance.APPLE_STORE_URL
#else
                                GameConfig.Instance.GOOGLE_STORE_URL
#endif
                            );
                    }, () =>
                    {
                        SAMANDA.Instance.UI.SetUIState(LOGIN_STATE.MAIN_OPEN);
                    });
                    break;
                default:
#if false
                    if (res.ContainsKey("msg"))
                        StateText.text = res["msg"].Value<string>();
                    else
                        StateText.text = StringManager.GetString("로그인스탭_20");
                    break;
#else

                    //ServerError
                    StateText.text = StringManager.GetString("로그인스탭_21", trycount);
                    SignInGame(trycount);
                    break;
#endif
            }
        });
    }

    public void SetStateString(string state)
    {
        StateText.text = state;
    }

    void OnServerError()
    {
        if (loginProcess != null)
        {
            StopCoroutine(loginProcess);
            loginProcess = null;
        }

        StateText.text = StringManager.GetString("로그인스탭_22");
    }

}
