using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;
using DG.Tweening;
using Coffee.UIEffects;
using UnityEngine.Video;
using EasyMobile;

public class intro_manager : MonoBehaviour
{
    public GameObject videoPlayer;
    public GameObject[] FootEffect;
    public AudioSource btnSound;
    public AudioSource backMusic;

    public GameObject txt_title;
    public GameObject txt_title_en;
    public Button btn_start;
    public GameObject loading;
    public Text btnText;
    Coroutine coroutine = null;

    bool bShowMovieMap = false;
    public GameObject DebugTestButton;
    public Dropdown DebugSeqTestDropDown;
    public GameObject DataClearButton;
    public UITransitionEffect IntroStartEffect;
    public GameObject GameExitPopup;
    public GameObject TestDonePopup;
    public GameObject SamanadaErrorPopup;
    public GameObject AccountErrorPopup;
    public GameObject JWTErrorPopup;
    public GameObject ErrorMsgPopup;
    public Text Version;
    private void Awake()
    {
        Version.text = GameDataManager.Instance.GetVersionWithFlag();

        RenderTexture target = videoPlayer.GetComponent<VideoPlayer>().targetTexture;
        if (target)
        {
            RenderTexture rt = RenderTexture.active;
            RenderTexture.active = target;
            GL.Clear(true, true, Color.clear);
            RenderTexture.active = rt;
        }

        bool mute = PlayerPrefs.GetInt("Setting_BGM", 1) == 0;
        //float volume = (float)PlayerPrefs.GetInt("Config_BV", 9) / 9;

        if (mute)
        {
            backMusic.mute = mute;
            backMusic.Stop();
        }
        else
        {
            //backMusic.volume = volume;
        }

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

        SystemLanguage targetLanguage = SystemLanguage.English;

        switch (languageType)
        {
            case LANGUAGE_TYPE.LANGUAGE_KOR:
                targetLanguage = SystemLanguage.Korean;
                break;
            case LANGUAGE_TYPE.LANGUAGE_ENG:
                targetLanguage = SystemLanguage.English;
                break;
            case LANGUAGE_TYPE.LANGUAGE_JPN:
                targetLanguage = SystemLanguage.Japanese;
                break;
            case LANGUAGE_TYPE.LANGUAGE_IND:
                targetLanguage = SystemLanguage.Indonesian;
                break;
        }
        LocalizeData.instance.SetLanguage(targetLanguage);

        GameObject canvas = GameObject.Find("SAMANDA_CANVAS");
        if (canvas)
        {
            canvas.GetComponent<SamandaStarter>().SetBackKeyCallback(() => {
                GameExitPopup.SetActive(true);
            });
        }

        if(PlayerPrefs.GetInt("Setting_PushAlarm", 1) == 1)
        {
            Notifications.GrantDataPrivacyConsent();
        }
        else
        {
            Notifications.RevokeDataPrivacyConsent();
        }

        Firebase.Messaging.FirebaseMessaging.SubscribeAsync("/topics/allusers");
    }

    void Start()
    {
        if (IntroStartEffect != null)
        {
            IntroStartEffect.effectFactor = 1.0f;
            IntroStartEffect.Hide();
            Destroy(IntroStartEffect.gameObject, 1.0f);
            IntroStartEffect = null;
        }

        float wr = 1.0f;// Screen.width / 720;
        float hr = 1.0f;// Screen.height / 1280;

        SamandaLauncher.ResizeWebViewRect(new Rect(0 * wr, 0 * hr, 0 * wr, 0 * hr));

        btn_start.interactable = false;
        btnText.gameObject.SetActive(false);

        loading.SetActive(false);

        txt_title.SetActive(false);
        txt_title_en.SetActive(false);

        coroutine = StartCoroutine(FadeTextToFullAlpha());
        
        if (DebugTestButton != null)
            DebugTestButton.SetActive(false);

        if(DebugSeqTestDropDown != null)
        {
            DebugSeqTestDropDown.options.Clear();

            for (neco_data.PrologueSeq seq = neco_data.PrologueSeq.챕터1시작; seq <= neco_data.PrologueSeq.PROLOGUE_SEQ_DONE; seq++)
            {
                DebugSeqTestDropDown.options.Add(new Dropdown.OptionData(seq.ToString()));
            }
        }
    }

    private void OnDestroy()
    {
        if (coroutine != null)
            StopCoroutine(coroutine);
    }
    // Update is called once per frame
    void Update()
    {
        float limit = (videoPlayer.GetComponent<RectTransform>().sizeDelta.x - gameObject.GetComponent<RectTransform>().sizeDelta.x) * 0.5f;
        if(limit < 0)
        {
            limit = (gameObject.GetComponent<RectTransform>().sizeDelta.x - videoPlayer.GetComponent<RectTransform>().sizeDelta.x) * 0.5f;
        }

        Vector3 dir = Vector3.zero;
        dir.x = (Input.acceleration.x * Time.deltaTime * 0.1f) * limit * -1.0f;
        dir = videoPlayer.transform.localPosition + dir;

        dir.x = Mathf.Min(dir.x, limit);
        dir.x = Mathf.Max(dir.x, limit * -1.0f);

        videoPlayer.transform.localPosition = dir;
    }

    public IEnumerator FadeTextToFullAlpha() // 알파값 0에서 1로 전환
    {
        yield return new WaitForSeconds(1.0f);
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

        switch (languageType)
        {
            case LANGUAGE_TYPE.LANGUAGE_KOR:
                txt_title.SetActive(true);
                btnText.text = "누르면 고양이를 만날 수 있어요";
                break;
            default:
                txt_title_en.SetActive(true);
                btnText.text = "Click to meet the cat";
                break;
        }

        btnText.gameObject.SetActive(true);
        btnText.color = new Color(btnText.color.r, btnText.color.g, btnText.color.b, 0);

        yield return new WaitForSeconds(1.0f);

        while (btnText.color.a < 1.0f)
        {
            btnText.color = new Color(btnText.color.r, btnText.color.g, btnText.color.b, btnText.color.a + (Time.deltaTime / 1.0f));
            yield return null;
        }
        btnText.color = new Color(btnText.color.r, btnText.color.g, btnText.color.b, 1.0f);

        btn_start.interactable = true;

        bool buttonFadeIn = false;

        Image foot1 = FootEffect[0].GetComponent<Image>();
        float alpha1 = 0.0f;
        bool foot1FadeIn = true;
        Image foot2 = FootEffect[1].GetComponent<Image>();
        float alpha2 = -0.5f;
        bool foot2FadeIn = true;

        while (true)
        {
            float delta = Time.deltaTime;

            if (buttonFadeIn)
            {
                btnText.color = new Color(btnText.color.r, btnText.color.g, btnText.color.b, btnText.color.a + (delta * 0.5f));
                if(btnText.color.a >= 1.0f)
                {
                    buttonFadeIn = false;
                }
            }
            else
            {
                btnText.color = new Color(btnText.color.r, btnText.color.g, btnText.color.b, btnText.color.a - (delta * 0.5f));
                if (btnText.color.a <= 0.4f)
                {
                    buttonFadeIn = true;
                }
            }

            if(foot1FadeIn)
            {
                alpha1 += delta * 1.1f;
                Color color = Color.white;
                color.a = alpha1;
                foot1.color = color;
                if(alpha1 > 1.0f)
                {
                    foot1FadeIn = false;
                }
            }
            else
            {
                alpha1 -= delta * 1.1f;
                Color color = Color.white;
                color.a = alpha1;
                foot1.color = color;
                if (alpha1 < 0.5f)
                {
                    foot1FadeIn = true;
                }
            }

            if (foot2FadeIn)
            {
                alpha2 += delta * 1.1f;
                Color color = Color.white;
                color.a = alpha2;
                foot2.color = color;
                if (alpha2 > 1.0f)
                {
                    foot2FadeIn = false;
                }
            }
            else
            {
                alpha2 -= delta * 1.1f;
                Color color = Color.white;
                color.a = alpha2;
                foot2.color = color;
                if (alpha2 < 0.5f)
                {
                    foot2FadeIn = true;
                }
            }

            yield return new WaitForEndOfFrame();
        }
    }

    public void LoadGame()
    {
        btn_start.gameObject.SetActive(false);
        loading.SetActive(true);

        if (!SamandaLauncher.IsSamandaReady() || Application.internetReachability == NetworkReachability.NotReachable)
        { 
            SamanadaErrorPopup.SetActive(true);
            return;
        }

        bool mute = PlayerPrefs.GetInt("Setting_SFX", 1) == 0;
        //float volume = (float)PlayerPrefs.GetInt("Config_EV", 9) / 9;
        
        if (!mute)
        {
            //btnSound.volume = volume;
            btnSound.Play();
        }
        GameDataManager.Instance.LoadGameData();

        if (SamandaLauncher.UseSamanda())
        {
            SamandaLauncher.SetOnHideCallback(() => {
                btn_start?.gameObject.SetActive(true);
                loading?.SetActive(false);
            });
            
            SamandaLauncher.OnAccountCheckAndPlay(() =>
            {
                SamandaLauncher.SetOnHideCallback(null);
                SamandaLauncher.OnHideScreen();
                SamandaLauncher.OnJWTCheck(()=> {
                    NetworkManager.SendJWTRequest(SyncGameData, OnErrorJWTCheck, 1);
                });

#if UNITY_ANDROID
                GameServices.ManagedInit(); // 구글 플레이 게임초기화
#endif
            }, Logout);
        }
        else
        {
            SamandaLauncher.SetOnHideCallback(null);
            StartCoroutine(DownloadLanguageCSVWithSceneChange());                      
        }

        btn_start.interactable = false;
        Invoke("ButtonReset", 1.0f);
    }

    public void ButtonReset()
    {
        btn_start.interactable = true;
    }

    public void SyncGameData()
    {
        string last_data_sync = PlayerPrefs.GetString("last_data_sync", "0");
        WWWForm data = new WWWForm();
        data.AddField("last_data_sync", last_data_sync);

        NetworkManager.GetInstance().SendApiRequest("data", 1, data, (res)=> { LoginDone(); }, (error)=> { OnNetworkError(); }, false);
    }

    public void LoginDone()
    {
        if(bShowMovieMap)
        {
            SceneManager.LoadScene("ClipMap");
            return;
        }

#if UNITY_EDITOR
        if (DebugTestButton != null)
        {
            btn_start.interactable = false;
            DebugTestButton.SetActive(true);
            if (DebugSeqTestDropDown != null)
            {
                DebugSeqTestDropDown.value = (int)((uint)GameDataManager.Instance.GetUserData().data["contents"]);
            }
            return;
        }
#endif
        StartCoroutine(DownloadLanguageCSVWithSceneChange());
    }

    public void OnErrorAccountCheck()
    {
        AccountErrorPopup.SetActive(true);
    }
    public void OnRetryAccountCheck()
    {
        AccountErrorPopup.SetActive(false);
        SamandaLauncher.OnAccountCheckAndPlay(() =>
        {
            btn_start.gameObject.SetActive(false);
            SamandaLauncher.OnHideScreen();
            SamandaLauncher.OnJWTCheck(() => {
                NetworkManager.SendJWTRequest(SyncGameData, OnErrorJWTCheck, 1);
            });

#if UNITY_ANDROID
            GameServices.ManagedInit(); // 구글 플레이 게임초기화
#endif
        }, OnErrorAccountCheck);
    }

    public void OnErrorJWTCheck()
    {
        JWTErrorPopup.SetActive(true);
    }

    public void OnRetryJWTCheck()
    {
        JWTErrorPopup.SetActive(false);
        SamandaLauncher.OnJWTCheck(() => {
            NetworkManager.SendJWTRequest(SyncGameData, OnErrorJWTCheck, 1);
        });
    }

    public void OnNetworkError()
    {
        SamanadaErrorPopup.SetActive(true);
    }

    public void Logout()
    {
        GameDataManager.Instance.SetUserData(new users());
        
        GameObject[] roots = SceneManager.GetActiveScene().GetRootGameObjects();
        foreach (GameObject root in roots)
        {
            if (root.GetComponent<Canvas>() != null)
            {
                root.SetActive(false);
            }
        }

        SceneLoader.Instance.LoadScene("Intro");
    }

    public void OnDebugMovieMapButton()
    {
        bShowMovieMap = true;
        LoadGame();
    }

    public void DataReset()
    {
        SamandaLauncher.OnLogout();
    }

    public void OnExitCancel()
    {
        GameExitPopup.SetActive(false);
    }

    public void OnExit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit(); // 어플리케이션 종료
#endif
    }

    public void OnDebugTest(int type)
    {
#if UNITY_EDITOR
        switch (type)
        {
            case 0:
                neco_data.SetPrologueSeq(neco_data.PrologueSeq.PROLOGUE_SEQ_DONE, true);
                break;
            case 1:
                neco_data.SetPrologueSeq(neco_data.PrologueSeq.챕터1시작, true);
                break;
            case 2:
                neco_data.SetPrologueSeq(neco_data.PrologueSeq.챕터2시작, true);
                break;
            case 3:
                neco_data.SetPrologueSeq(neco_data.PrologueSeq.챕터3시작, true);
                break;
            case 4:
                //neco_data.SetPrologueSeq(neco_data.PrologueSeq.챕터4시작, true);
                //break;
            case 5:
                neco_data.SetPrologueSeq(neco_data.PrologueSeq.챕터5시작, true);
                break;
        }
#endif
        StartCoroutine(DownloadLanguageCSVWithSceneChange());
    }

    public void OnDebugTestWithSeq()
    {
#if UNITY_EDITOR
        neco_data.SetPrologueSeq((neco_data.PrologueSeq)DebugSeqTestDropDown.value, true);
#endif
        StartCoroutine(DownloadLanguageCSVWithSceneChange());
    }

    public void OnDebugMinigame()
    {
#if UNITY_EDITOR
        SceneManager.LoadScene("MiniGameTest");
#endif
    }

    IEnumerator DownloadLanguageCSVWithSceneChange()
    {
        int SavedVersion = int.Parse(GameDataManager.Instance.GetVersionCode());

        int string_ver = PlayerPrefs.GetInt("string_ver", 0);
        if(string_ver < SavedVersion)
        {
            string_ver = SavedVersion;
            PlayerPrefs.SetInt("string_ver", SavedVersion);

            PlayerPrefs.SetString("string", "");
        }

        string server_ver = game_config.GetConfig("string_ver");
        
        int ver = 0;
        if (!string.IsNullOrEmpty(server_ver))
        {
            ver = int.Parse(server_ver);            
        }

        if (ver > string_ver)
        {
            string url = NetworkManager.DOWNLOAD_URL + game_config.GetConfig("string");

            UnityWebRequest www = UnityWebRequest.Get(url);
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
                RetryGameStart();
            }
            else
            {
                PlayerPrefs.SetInt("string_ver", ver);
                PlayerPrefs.SetString("string", www.downloadHandler.text);

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

                SystemLanguage targetLanguage = SystemLanguage.English;

                switch (languageType)
                {
                    case LANGUAGE_TYPE.LANGUAGE_KOR:
                        targetLanguage = SystemLanguage.Korean;
                        break;
                    case LANGUAGE_TYPE.LANGUAGE_ENG:
                        targetLanguage = SystemLanguage.English;
                        break;
                    case LANGUAGE_TYPE.LANGUAGE_JPN:
                        targetLanguage = SystemLanguage.Japanese;
                        break;
                    case LANGUAGE_TYPE.LANGUAGE_IND:
                        targetLanguage = SystemLanguage.Indonesian;
                        break;
                }

                LocalizeData.instance.SetDownloadData(www.downloadHandler.text, targetLanguage);

                SceneLoader.Instance.LoadScene("NecoPrologue");
            }
        }
        else
        {
            string data = PlayerPrefs.GetString("string");
            if (string.IsNullOrEmpty(data))
                data = Resources.Load<TextAsset>("Data/string").text;

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

            SystemLanguage targetLanguage = SystemLanguage.English;

            switch (languageType)
            {
                case LANGUAGE_TYPE.LANGUAGE_KOR:
                    targetLanguage = SystemLanguage.Korean;
                    break;
                case LANGUAGE_TYPE.LANGUAGE_ENG:
                    targetLanguage = SystemLanguage.English;
                    break;
                case LANGUAGE_TYPE.LANGUAGE_JPN:
                    targetLanguage = SystemLanguage.Japanese;
                    break;
                case LANGUAGE_TYPE.LANGUAGE_IND:
                    targetLanguage = SystemLanguage.Indonesian;
                    break;
            }

            LocalizeData.instance.SetDownloadData(data, targetLanguage);

            SceneLoader.Instance.LoadScene("NecoPrologue");
        }
    }

    public void RetryGameStart()
    {
        GameObject[] roots = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
        foreach (GameObject root in roots)
        {
            Destroy(root);
        }

        SceneManager.LoadScene("Splash");
    }
    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit(); // 어플리케이션 종료
#endif
    }

    public void OnAccountError(int rs)
    {
        Transform tt = ErrorMsgPopup.transform.Find("Text");
        if (tt == null)
            return;
        Text txt = tt.GetComponent<Text>();
        if (txt == null)
            return;
        string msg = "";
        switch(rs)
        {
            case 5:
                msg = LocalizeData.GetText("정지계정");
                break;
            case 6:
                msg = LocalizeData.GetText("삭제계정");
                break;
            default:
                msg = LocalizeData.GetText("기타계정오류");
                break;
        }
        txt.text = msg;
        
        ErrorMsgPopup.SetActive(true);
    }
}
