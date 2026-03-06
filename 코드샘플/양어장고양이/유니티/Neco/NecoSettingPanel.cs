using EasyMobile;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.UI;
public enum LANGUAGE_TYPE
{
    LANGUAGE_KOR,
    LANGUAGE_ENG,
    LANGUAGE_JPN,
    LANGUAGE_IND
}

public class NecoSettingPanel : MonoBehaviour
{
    public enum GRAPHIC_STATE
    {
        LOW,
        MEDIUM,
        HIGH
    }

    [Header("[Common]")]
    public Color enableButtonColor;
    public Color disableButtonColor;
    public Color iconDisableColor;

    [Header("[BGM Layer]")]
    public Image bgmIcon;
    public GameObject BgmButtonObject;
    bool _isBgmOn = true;
    public bool IsBgmOn
    {
        set
        {

            _isBgmOn = value;
            PlayerPrefs.SetInt("Setting_BGM", _isBgmOn ? 1 : 0);
        }
        get
        {
            _isBgmOn = PlayerPrefs.GetInt("Setting_BGM", 1) == 1;
            return _isBgmOn;
        }
    }

    [Header("[SFX Layer]")]
    public Image sfxIcon;
    public GameObject SFXButtonObject;
    bool _isSFXOn = true;
    public bool IsSFXOn
    {
        set
        {
            _isSFXOn = value;
            PlayerPrefs.SetInt("Setting_SFX", _isSFXOn ? 1 : 0);
        }
        get
        {
            _isSFXOn = PlayerPrefs.GetInt("Setting_SFX", 1) == 1;
            return _isSFXOn;
        }
    }

    [Header("[Vibration Layer]")]
    public Image vibrationIcon;
    public GameObject vibrationButtonObject;
    bool _isVibrationOn = true;
    public bool IsVibrationOn
    {
        set
        {
            _isVibrationOn = value;
            PlayerPrefs.SetInt("Setting_Vibration", _isVibrationOn ? 1 : 0);
        }
        get
        {
            _isVibrationOn = PlayerPrefs.GetInt("Setting_Vibration", 1) == 1;
            return _isVibrationOn;
        }
    }

    [Header("[Push Alarm Layer]")]
    public GameObject pushOnButtonObject;
    public GameObject pushOffButtonObject;
    bool _isPushAlarmOn = true;
    public bool IsPushAlarmOn
    {
        set
        {
            _isPushAlarmOn = value;
            PlayerPrefs.SetInt("Setting_PushAlarm", _isPushAlarmOn ? 1 : 0);
            
            if(_isPushAlarmOn)
                Notifications.GrantDataPrivacyConsent();
            else
                Notifications.RevokeDataPrivacyConsent();            
        }
        get
        {
            _isPushAlarmOn = PlayerPrefs.GetInt("Setting_PushAlarm", 1) == 1;
            return _isPushAlarmOn;
        }
    }

    [Header("[Graphic Set Layer]")]
    public GameObject graphicLowButtonObject;
    public GameObject graphicMediumButtonObject;
    public GameObject graphicHighButtonObject;
    GRAPHIC_STATE _graphicState = GRAPHIC_STATE.MEDIUM;
    public GRAPHIC_STATE GraphicState
    {
        set
        {
            _graphicState = value;
            PlayerPrefs.SetInt("Setting_GraphicSet", (int)value);
        }
        get
        {
            _graphicState = (GRAPHIC_STATE)PlayerPrefs.GetInt("Setting_GraphicSet", (int)GRAPHIC_STATE.MEDIUM);
            return _graphicState;
        }
    }

    [Header("[Languate Layer]")]
    public Dropdown languageDropdown;
    public Sprite[] languageSprites;
    LANGUAGE_TYPE _languageType = LANGUAGE_TYPE.LANGUAGE_KOR;
    public LANGUAGE_TYPE LanguageType
    {
        set
        {
            _languageType = value;
            PlayerPrefs.SetInt("Setting_Language", (int)value);

            SystemLanguage lang = Application.systemLanguage;
            switch (_languageType)
            {
                case LANGUAGE_TYPE.LANGUAGE_KOR:
                    lang = SystemLanguage.Korean;
                    SamandaLauncher.SetLanguage(SystemLanguage.Korean);
                    break;
                case LANGUAGE_TYPE.LANGUAGE_ENG:
                    lang = SystemLanguage.English;
                    SamandaLauncher.SetLanguage(SystemLanguage.English);
                    break;
                case LANGUAGE_TYPE.LANGUAGE_JPN:
                    lang = SystemLanguage.Japanese;
                    SamandaLauncher.SetLanguage(SystemLanguage.English);
                    break;
                case LANGUAGE_TYPE.LANGUAGE_IND:
                    lang = SystemLanguage.Indonesian;
                    SamandaLauncher.SetLanguage(SystemLanguage.English);
                    break;
                default:
                    SamandaLauncher.SetLanguage(SystemLanguage.English);
                    break;
            }

            WWWForm data = new WWWForm();
            data.AddField("api", "user");
            data.AddField("op", 9);
            data.AddField("lang", (int)lang);
            NetworkManager.GetInstance().SendPostSimple("user", 9, data);
        }
        get
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

            _languageType = (LANGUAGE_TYPE)PlayerPrefs.GetInt("Setting_Language", (int)defaultLanguage);
            return _languageType;
        }
    }

    [Header("[User Info Layer]")]
    public Text UIDText;
    public Text nicknameText;
    public Text versionText;
    public Image levelIcon;

    [Header("[Coupon]")]
    public GameObject couponBtn;
    public GameObject couponPop;
    public GameObject couponCloseBtn;
    public GameObject revokeBtn;
    public InputField inputCode;
    public SystemConfirmItemListPanel couponRewardPop;
    string originCode = "";

    public GameObject SettingBgPop;
    public GameObject closeBtn;
    public bool activeCouponPop = false;

    public void OnClickSettingButton()
    {
        NecoCanvas.GetPopupCanvas().OnPopupShow(NecoPopupCanvas.POPUP_TYPE.SETTING_POPUP);
    }

    public void OnClickCloseButton()
    {
        if (activeCouponPop)
            OnClickCouponCloseButton();

        NecoCanvas.GetPopupCanvas().OnPopupClose(NecoPopupCanvas.POPUP_TYPE.SETTING_POPUP);
    }

    public void OnClickCouponOpenButton()
    {
        couponPop.SetActive(true);
        couponCloseBtn.SetActive(true);

        SettingBgPop.SetActive(false);
        closeBtn.SetActive(false);
        activeCouponPop = true;
        inputCode.text = "";
    }

    public void OnClickCouponCloseButton()
    {
        couponRewardPop.gameObject.SetActive(false);
        couponPop.SetActive(false);
        couponCloseBtn.SetActive(false);

        SettingBgPop.SetActive(true);
        closeBtn.SetActive(true);
        activeCouponPop = false;
        originCode = "";
    }
    private void OnEnable()
    {
        // 저장된 setting 상태 로드
        InitSettingState();
        InitLanguageList();

        SetUserInfo();
    }

    #region BGM Layer

    public void OnClickBgmButton()
    {
        // bgm 기능 처리
        bool isBgmOn = IsBgmOn = !IsBgmOn;

        //if (isBgmOn)
        //{
        //    AudioManager.GetInstance().ResumeBackgroundAudio();
        //}
        //else
        //{
        //    AudioManager.GetInstance().PauseBackgroundAudio();
        //}

        AudioManager.GetInstance().SetBGMMixerControl(isBgmOn);

        bgmIcon.color = isBgmOn ? Color.white : iconDisableColor;
        BgmButtonObject.GetComponentInChildren<Image>().color = isBgmOn ? enableButtonColor : disableButtonColor;
    }

    #endregion

    #region SFX Layer

    public void OnClickSFXButton()
    {
        // sfx 기능 처리
        bool isSFXOn = IsSFXOn = !IsSFXOn;

        AudioManager.GetInstance().SetSFXMixerControl(isSFXOn);

        //GameObject[] roots = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
        //foreach (GameObject root in roots)
        //{
        //    if (root.GetComponent<Canvas>() != null)
        //    {
        //        AudioSource[] audios = root.GetComponentsInChildren<AudioSource>(true);
        //        foreach (AudioSource audio in audios)
        //        {
        //            audio.mute = !isSFXOn;
        //        }
        //    }
        //}

        sfxIcon.color = isSFXOn ? Color.white : iconDisableColor;
        SFXButtonObject.GetComponentInChildren<Image>().color = isSFXOn ? enableButtonColor : disableButtonColor;
    }

    #endregion

    #region Vibration Layer

    public void OnClickVibrationButton()
    {
        // vibration 기능 처리
        bool isVibrationOn = IsVibrationOn = !IsVibrationOn;

        vibrationIcon.color = isVibrationOn ? Color.white : iconDisableColor;
        vibrationButtonObject.GetComponentInChildren<Image>().color = isVibrationOn ? enableButtonColor : disableButtonColor;
    }

    #endregion

    #region Push Alarm Layer

    public void OnClickPushAlarmButton()
    {
        // push alarm 기능 처리
        bool isPushAlarm = IsPushAlarmOn = !IsPushAlarmOn;

        // push alarm 제어
        //...

        pushOnButtonObject.GetComponentInChildren<Image>().color = isPushAlarm ? enableButtonColor : disableButtonColor;
        pushOffButtonObject.GetComponentInChildren<Image>().color = isPushAlarm ? disableButtonColor : enableButtonColor;
    }

    #endregion

    #region Graphic Set Layer

    public void OnClickGraphicSetButton(int state)
    {
        // graphic set 기능 처리
        GraphicState = (GRAPHIC_STATE)state;

        SuperBlur.CompactBlur blur = Camera.main.gameObject.GetComponent<SuperBlur.CompactBlur>();

        // 버튼 상태 리셋
        graphicLowButtonObject.GetComponentInChildren<Image>().color = disableButtonColor;
        graphicMediumButtonObject.GetComponentInChildren<Image>().color = disableButtonColor;
        graphicHighButtonObject.GetComponentInChildren<Image>().color = disableButtonColor;

        switch (GraphicState)
        {
            case GRAPHIC_STATE.LOW:
                if (blur != null) { blur.RefreshCycleTime = 10.0f; }

                graphicLowButtonObject.GetComponentInChildren<Image>().color = enableButtonColor;
                break;
            case GRAPHIC_STATE.MEDIUM:
                if (blur != null) { blur.RefreshCycleTime = 1.0f; }

                graphicMediumButtonObject.GetComponentInChildren<Image>().color = enableButtonColor;
                break;
            case GRAPHIC_STATE.HIGH:
                if (blur != null) { blur.RefreshCycleTime = 0; }

                graphicHighButtonObject.GetComponentInChildren<Image>().color = enableButtonColor;
                break;
        }
    }

    #endregion

    #region Language Layer

    public void OnClickLanguageButton()
    {

    }

    public void OnSelectLanguage()
    {
        if (languageDropdown == null) { return; }

        LanguageType = (LANGUAGE_TYPE)languageDropdown.value;

        SystemLanguage targetLanguage = SystemLanguage.Korean;

        switch (LanguageType)
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

        if (LocalizeData.instance.isLoadDone == true)
        {
            GameObject[] rootObjects = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
            foreach (GameObject root in rootObjects)
            {
                if (root.GetComponent<Canvas>() != null)
                {
                    TextLocalize[] localize = root.GetComponentsInChildren<TextLocalize>(true);
                    foreach (TextLocalize localText in localize)
                    {
                        localText.SetText();
                    }
                }
            }
        }

        // 언어 설정 후 추가 처리
        SetUserInfo();
    }

    void InitLanguageList()
    {
        languageDropdown.ClearOptions();

        List<Dropdown.OptionData> options = new List<Dropdown.OptionData>();

        //foreach (LANGUAGE_TYPE language in Enum.GetValues(typeof(LANGUAGE_TYPE)))
        //{
        //    options.Add(new Dropdown.OptionData(LocalizeData.GetText(language.ToString())));
        //}

        foreach (Sprite language in languageSprites)
        {
            options.Add(new Dropdown.OptionData(language));
        }

        languageDropdown.AddOptions(options);

        languageDropdown.value = (int)LanguageType;
    }

    #endregion

    #region User Info Layer

    void SetUserInfo()
    {
        // 유저 정보 세팅
        UIDText.text = string.Format("{0}: {1}", LocalizeData.GetText("LOCALIZE_388"), SamandaLauncher.GetAccountNo());
        nicknameText.text = string.Format("{0}: {1}", LocalizeData.GetText("LOCALIZE_389"), SamandaLauncher.GetAccountNickName());
        versionText.text = string.Format("{0} {1}", LocalizeData.GetText("LOCALIZE_390"), GameDataManager.Instance.GetVersionWithFlag());

        levelIcon.gameObject.SetActive(false);
        users user = GameDataManager.Instance.GetUserData();
        if (user != null)
        {            
            object obj;
            if (user.data.TryGetValue("profileId", out obj))
            {
                Sprite[] sprites = Resources.LoadAll<Sprite>("Sprites/Neco/Ui/Idcard");
                uint index = (uint)obj;
                if (index > 0)
                {
                    if (sprites.Length > index - 1)
                    {
                        levelIcon.sprite = sprites[index - 1];
                        levelIcon.gameObject.SetActive(true);
                    }
                }
            }       
        }
    }

    #endregion

    #region Logout Layer

    public void OnClickLogoutButton()
    {
        if (IsLinkedAccount() || IsGuestAccount())
        {
            ConfirmPopupData popupData = SetConfirmPopupData();

            NecoCanvas.GetPopupCanvas().OnSystemConfirmPopupShow(popupData, CONFIRM_POPUP_TYPE.COMMON, () => {

                SamandaLauncher.SendSamandaCommand("window.Samanda.setLogout()");
                SamandaLauncher.OnLogoutCall();
            });

            return;
        }

        // 로그아웃할 계정이 없는 경우
        NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(LocalizeData.GetText("LOCALIZE_278"), LocalizeData.GetText("LOCALIZE_344"));
    }

    bool IsLinkedAccount()
    {
        List<int> accountLink = SamandaLauncher.GetAccountLinks();
        if (accountLink.Count < 1) { return false; }

        foreach (SamandaLauncher.AccountType account in Enum.GetValues(typeof(SamandaLauncher.AccountType)))
        {
            if (account != SamandaLauncher.AccountType.GUEST)
            {
                // 무언가 로그인 된 계정이 있을 경우
                if (accountLink.Contains((int)account))
                {
                    return true;
                }
            }
        }

        return false;
    }

    bool IsGuestAccount()
    {
        List<int> accountLink = SamandaLauncher.GetAccountLinks();
        if (accountLink.Count != 1) { return false; }

        return (SamandaLauncher.AccountType)accountLink[0] == SamandaLauncher.AccountType.GUEST;
    }

    #endregion

    #region Service Center Layer

    public void OnClickServiceCenterButton()
    {
        SamandaLauncher.OpenCustomerSupportPage();
    }

    #endregion

    #region SAMANDA Layer

    public void OnClickSAMANDAButton()
    {
        SamandaLauncher.OnSamandaShortcut();
    }

    #endregion


    void InitSettingState()
    {
        // 사운드 & 이펙트
        bgmIcon.color = IsBgmOn ? Color.white : iconDisableColor;
        BgmButtonObject.GetComponentInChildren<Image>().color = IsBgmOn ? enableButtonColor : disableButtonColor;
        sfxIcon.color = IsSFXOn ? Color.white : iconDisableColor;
        SFXButtonObject.GetComponentInChildren<Image>().color = IsSFXOn ? enableButtonColor : disableButtonColor;
        vibrationIcon.color = IsVibrationOn ? Color.white : iconDisableColor;
        vibrationButtonObject.GetComponentInChildren<Image>().color = IsVibrationOn ? enableButtonColor : disableButtonColor;

        // 푸쉬 알림
        pushOnButtonObject.GetComponentInChildren<Image>().color = IsPushAlarmOn ? enableButtonColor : disableButtonColor;
        pushOffButtonObject.GetComponentInChildren<Image>().color = IsPushAlarmOn ? disableButtonColor : enableButtonColor;

        // 그래픽 세팅
        graphicLowButtonObject.GetComponentInChildren<Image>().color = disableButtonColor;
        graphicMediumButtonObject.GetComponentInChildren<Image>().color = disableButtonColor;
        graphicHighButtonObject.GetComponentInChildren<Image>().color = disableButtonColor;

        //게스트도 off 해줌
        if (!IsLinkedAccount())
            revokeBtn.SetActive(false);
#if UNITY_IOS            
            //IOS 쿠폰버튼 비활성
            couponBtn.SetActive(false);
#elif UNITY_ANDROID || UNITY_EDITOR 
        revokeBtn.SetActive(false);
#endif

        switch (GraphicState)
        {
            case GRAPHIC_STATE.LOW:
                graphicLowButtonObject.GetComponentInChildren<Image>().color = enableButtonColor;
                break;
            case GRAPHIC_STATE.MEDIUM:
                graphicMediumButtonObject.GetComponentInChildren<Image>().color = enableButtonColor;
                break;
            case GRAPHIC_STATE.HIGH:
                graphicHighButtonObject.GetComponentInChildren<Image>().color = enableButtonColor;
                break;
        }
    }

    public void onTrySecession()
    {
        ConfirmPopupData popupData = new ConfirmPopupData();
        popupData.titleText = LocalizeData.GetText("secession");
        popupData.titleMessageText = LocalizeData.GetText("realscecssion");
        popupData.messageText_2 = LocalizeData.GetText("secessionmessage");

        NecoCanvas.GetPopupCanvas().OnSystemConfirmPopupShow(popupData, CONFIRM_POPUP_TYPE.COMMON, () => 
        {
            Secession();
        });
    }

    public void Secession()
    {
        WWWForm sparam = new WWWForm();
        sparam.AddField("api", "auth");
        sparam.AddField("op", 6);

        Debug.Log("token : " + SamandaLauncher.GetAccountToken());

        NetworkManager.GetInstance().SendApiRequest("auth", 6,
            sparam,
            (string body) =>
            {
                Debug.Log("SAMANDA TEST" + body);
                // if isError
                ResponseRoot objRoot = JsonUtility.FromJson<ResponseRoot>(body);
                if (null == objRoot || (int)eResponseCode.OK != objRoot.rs)
                {
                    Debug.Log("SAMANDA FAIL1");
                    NetworkManager.GenericApiFail(objRoot.rs);
                    return;
                }

                NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(LocalizeData.GetText("secession"), LocalizeData.GetText("secessionquit"), () =>
                {
                    SamandaLauncher.OnLogoutCall();
                    Application.Quit();
                });
            },
            (eResponseCode code) => 
            {
                Debug.Log("code : " + code);
            }
            , false);
    }

    ConfirmPopupData SetConfirmPopupData()
    {
        ConfirmPopupData popupData = new ConfirmPopupData();
        popupData.titleText = LocalizeData.GetText("LOCALIZE_387");
        popupData.titleMessageText = LocalizeData.GetText("LOCALIZE_398");

        if (IsGuestAccount())
        {
            popupData.messageText_2 = LocalizeData.GetText("LOCALIZE_459");
        }
        else
        {
            popupData.messageText_2 = LocalizeData.GetText("LOCALIZE_399");
        }
        return popupData;
    }

    public void onClickCouponBtn()
    {
        string couponeCode = inputCode.text;
        string msgTitle = LocalizeData.GetText("LOCALIZE_330");

        if (couponeCode.Length < 4)
        {
            NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(msgTitle, LocalizeData.GetText("쿠폰미입력"));
            return;
        }

        //api 통신 후 정보 받아옴
        //정상 rs일때 쿠폰 rewardList 팝업창에 아이템 뿌려줌

        //테스트코드
        //couponRewardPop.Show();

        WWWForm data = new WWWForm();
        data.AddField("uri", "coupon");
        data.AddField("code", couponeCode);
        data.AddField("op", 1);

        NetworkManager.GetInstance().SendApiRequest("coupon", 1, data, (response) =>
        {
            JObject root = JObject.Parse(response);
            JToken apiToken = root["api"];
            if (null == apiToken || apiToken.Type != JTokenType.Array || !apiToken.HasValues)
            {
                return;
            }

            JArray apiArr = (JArray)apiToken;
            foreach (JObject row in apiArr)
            {
                string uri = row.GetValue("uri").ToString();
                if (uri == "coupon")
                {
                    JToken resultCode = row["rs"];
                    if (resultCode != null && resultCode.Type == JTokenType.Integer)
                    {
                        int rs = resultCode.Value<int>();
                        JObject rewardObj = (JObject)row["rew"];

                        switch (rs)
                        {
                            case 0: //조회 정상 //아이템 리스트로 구조화
                                if(rewardObj == null)
                                {
                                    //아이템 정보 오류
                                    Debug.Log("Coupon :: 아이템 정보 오류");
                                    break;
                                }
                                List<RewardData> rewardItems = new List<RewardData>();
                                RewardData reward = null;
                                JToken token;

                                if (rewardObj.TryGetValue("gold", out token))
                                {
                                    reward = new RewardData();
                                    reward.gold = token.Value<uint>();
                                    rewardItems.Add(reward);
                                }

                                if (rewardObj.TryGetValue("catnip", out token))
                                {
                                    reward = new RewardData();
                                    reward.catnip = token.Value<uint>();
                                    rewardItems.Add(reward);
                                }

                                if (rewardObj.TryGetValue("point", out token))
                                {
                                    reward = new RewardData();
                                    reward.point = token.Value<uint>();
                                    rewardItems.Add(reward);
                                }

                                if(rewardObj.TryGetValue("item", out token))
                                {
                                    foreach (JObject item in (JArray)token)
                                    {
                                        if (item != null)
                                        {
                                            reward = new RewardData();
                                            reward.itemData = items.GetItem(item["id"].Value<uint>());
                                            reward.count = item["amount"].Value<uint>();
                                            rewardItems.Add(reward);
                                        }
                                    }
                                }
                                
                                couponRewardPop.Show("쿠폰보상팝업타이틀", "쿠폰보상팝업부제", rewardItems, GetCouponReward, null);
                                break;

                            case 1: //서버 에러 - 입력코드 형식, api 서버 장애
                                NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(msgTitle, LocalizeData.GetText("SERVER_ERROR"));
                                break;

                            case 2: //없는 코드
                                NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(msgTitle, LocalizeData.GetText("쿠폰코드틀림"));
                                break;

                            case 3: //이미 사용한 코드
                                NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(msgTitle, LocalizeData.GetText("쿠폰이미사용"));
                                break;

                            case 4: //만료된 코드
                                NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(msgTitle, LocalizeData.GetText("쿠폰만료"));
                                break;
                        }

                        //couponRewardPop.Show();
                    }
                }
            }
        });
    }

    public void OnEditCodeString()
    {
        inputCode.text = originCode;
        inputCode.characterLimit = 16;

        EventSystem.current.SetSelectedGameObject(inputCode.gameObject, null);
        inputCode.OnPointerClick(new PointerEventData(EventSystem.current));
    }

    public void OnEndEditCodeString(string code)
    {
        inputCode.text = inputCode.text.ToUpper();
        originCode = inputCode.text;
        inputCode.characterLimit = 19;
        if(inputCode.text.Length > 4)
        {
            inputCode.text = inputCode.text.Insert(4, "-");
        }

        if (inputCode.text.Length > 9)
        {
            inputCode.text = inputCode.text.Insert(9, "-");
        }

        if (inputCode.text.Length > 14)
        {
            inputCode.text = inputCode.text.Insert(14, "-");
        }
    }

    void GetCouponReward()
    {
        string couponeCode = inputCode.text;

        WWWForm data = new WWWForm();
        data.AddField("uri", "coupon");
        data.AddField("code", couponeCode);
        data.AddField("op", 2);

        NetworkManager.GetInstance().SendApiRequest("coupon", 2, data, (response) =>
        {
            JObject root = JObject.Parse(response);
            JToken apiToken = root["api"];
            if (null == apiToken || apiToken.Type != JTokenType.Array || !apiToken.HasValues)
            {
                return;
            }

            JArray apiArr = (JArray)apiToken;
            foreach (JObject row in apiArr)
            {
                string uri = row.GetValue("uri").ToString();
                if (uri == "coupon")
                {
                    JToken resultCode = row["rs"];
                    if (resultCode != null && resultCode.Type == JTokenType.Integer)
                    {
                        int rs = resultCode.Value<int>();
                        JObject rewardObj = (JObject)row["rew"];
                        string msgTitle = LocalizeData.GetText("LOCALIZE_330");

                        switch (rs)
                        {
                            case 0: //받기 정상
                                Debug.Log("쿠폰 아이템 받음");
                                NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(msgTitle, LocalizeData.GetText("쿠폰코드정상"));
                                break;

                            case 1: //서버 에러 - api 서버 장애
                                NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(msgTitle, LocalizeData.GetText("SERVER_ERROR"));
                                break;

                            case 2: //없는 코드
                                NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(msgTitle, LocalizeData.GetText("쿠폰코드틀림"));
                                break;

                            case 3: //이미 사용한 코드
                                NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(msgTitle, LocalizeData.GetText("쿠폰이미사용"));
                                break;

                            case 4: //만료된 코드
                                NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(msgTitle, LocalizeData.GetText("쿠폰만료"));
                                break;
                        }
                    }
                }
            }
        });
    }

    public void CopyUIDToClipboard()
    {
        UniClipboard.SetText(SamandaLauncher.GetAccountNo());
        NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("UID복사메세지"));
    }
}
