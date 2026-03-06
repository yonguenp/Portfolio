using EasyMobile;
using SandboxPlatform.SAMANDA;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionPopup : Popup
{
    [Header("메인")]
    [SerializeField] List<GameObject> tabLists = new List<GameObject>();
    [SerializeField] List<Button> tabButtonLists = new List<Button>();
    [SerializeField] Text subTitle;
    [SerializeField] Color tabSelectedColor;

    [Header("버튼")]
    [SerializeField]
    Toggle bgmToggle;
    [SerializeField]
    Toggle sfxToggle;
    [SerializeField]
    Toggle vibrationToggle;

    [SerializeField]
    Toggle floatingToggle;
    [SerializeField]
    Toggle dynamicToggle;

    [SerializeField]
    Toggle friendToggle;
    [SerializeField]
    Toggle duoToggle;
    //[SerializeField]
    //Toggle pushToggle;

    [SerializeField]
    SAMANDA_Account account;

    [SerializeField]
    Dropdown languageDropdown;
    [SerializeField]
    Text versionText;
    [SerializeField]
    Text freeDia;
    [SerializeField]
    Text cashDia;
    [SerializeField]
    Text userNo;

    [SerializeField]
    GameObject couponButton;
    
    enum OptionTap
    {
        NONE = -1,
        SOUND = 0,
        CONTROLL = 1,
        INFORMATION = 2,
        COMMUNITY = 3,

    }

    enum LanguageIndex
    {
        KOR = 0,
        ENG = 1,
        CHI = 2,
        JAP = 3,
    }


    private void Start()
    {
        
    }

    public override void Open(CloseCallback cb = null)
    {
        base.Open(cb);
        Init();
    }
    public override void Close()
    {
        base.Close();
    }
    public override void RefreshUI()
    {
    }

    private void Init()
    {
        TabButton(0);

        bgmToggle.isOn = GameConfig.Instance.OPTION_BGM;
        sfxToggle.isOn = GameConfig.Instance.OPTION_SFX;
        vibrationToggle.isOn = GameConfig.Instance.OPTION_VIBRATION;
        friendToggle.isOn = GameConfig.Instance.FRIEND_RECOMMEND;
        duoToggle.isOn = GameConfig.Instance.DUO_ENABLE;
        //pushToggle.isOn = Notifications.DataPrivacyConsent == ConsentStatus.Granted && Firebase.Messaging.FirebaseMessaging.TokenRegistrationOnInitEnabled;

        if (GameConfig.Instance.OPTION_JOYSTICK == JoystickType.Dynamic)
        {
            dynamicToggle.isOn = true;
        }
        else
            floatingToggle.isOn = true;

        languageDropdown.options.Clear();

        languageDropdown.options.Add(new Dropdown.OptionData("한국어"));
        //languageDropdown.options.Add((new Dropdown.OptionData("English"));
        //languageDropdown.options.Add((new Dropdown.OptionData("Chinese"));
        //languageDropdown.options.Add((new Dropdown.OptionData("Japanese"));

        languageDropdown.value = (int)GetCurLanguageIndex();
        versionText.text = GameConfig.Instance.VERSION;

        freeDia.text = Managers.UserData.GetFreeDia().ToString();
        cashDia.text = Managers.UserData.GetCashDia().ToString();
        userNo.text = Managers.UserData.MyUserID.ToString();

        couponButton.SetActive(GameConfig.Instance.IS_COUPON_VALIDATE);
    }

    public void TabButton(int num)
    {
        foreach (var item in tabLists)
        {
            item.SetActive(false);

            tabButtonLists[tabLists.IndexOf(item)].image.color = Color.white;
        }

        tabLists[num].SetActive(true);
        tabButtonLists[num].image.color = tabSelectedColor;


        string ConvertEnumToString(OptionTap type)
        {
            switch (type)
            {
                case OptionTap.NONE:
                    return String.Empty;
                case OptionTap.SOUND:
                    return StringManager.GetString("옵션_사운드");
                case OptionTap.CONTROLL:
                    return StringManager.GetString("옵션_조작");
                case OptionTap.INFORMATION:
                    return StringManager.GetString("옵션_게임정보");
                case OptionTap.COMMUNITY:
                    return StringManager.GetString("옵션_커뮤니티");
                default:
                    return "";
            }
        }
        subTitle.text = ConvertEnumToString((OptionTap)num);
    }

    public void ChangeFriendRecommendFlag()
    {
        bool isFlag = friendToggle.isOn;
        Managers.FriendData.SetFriendRecommend(isFlag, recommend =>
        {
            GameConfig.Instance.FRIEND_RECOMMEND = recommend;
            //friendToggle.isOn = recommend;
        });
    }

    public void ChangeOptionValue(string type)
    {
        switch (type)
        {
            case "sfx":
                GameConfig.Instance.OPTION_SFX = sfxToggle.isOn;
                Managers.Sound.RefreshConfig();
                break;
            case "bgm":
                GameConfig.Instance.OPTION_BGM = bgmToggle.isOn;
                Managers.Sound.RefreshConfig();
                break;
            case "vibration":
                GameConfig.Instance.OPTION_VIBRATION = vibrationToggle.isOn;
                break;
            case "fixed":
                {
                    GameConfig.Instance.OPTION_JOYSTICK = JoystickType.Fixed;
                    floatingToggle.targetGraphic.GetComponent<Image>().sprite = floatingToggle.isOn ? Resources.Load<Sprite>("Texture/UI/Lobby/btn_pad_02") : Resources.Load<Sprite>("Texture/UI/Lobby/btn_pad_01");
                    dynamicToggle.targetGraphic.GetComponent<Image>().sprite = dynamicToggle.isOn ? Resources.Load<Sprite>("Texture/UI/Lobby/btn_pad_04") : Resources.Load<Sprite>("Texture/UI/Lobby/btn_pad_03");
                }
                break;
            case "dynamic":
                {
                    GameConfig.Instance.OPTION_JOYSTICK = JoystickType.Dynamic;
                    floatingToggle.targetGraphic.GetComponent<Image>().sprite = floatingToggle.isOn ? Resources.Load<Sprite>("Texture/UI/Lobby/btn_pad_02") : Resources.Load<Sprite>("Texture/UI/Lobby/btn_pad_01");
                    dynamicToggle.targetGraphic.GetComponent<Image>().sprite = dynamicToggle.isOn ? Resources.Load<Sprite>("Texture/UI/Lobby/btn_pad_04") : Resources.Load<Sprite>("Texture/UI/Lobby/btn_pad_03");
                }
                break;
            case "friend":
                {

                }
                break;
            case "duo":
                {
                    GameConfig.Instance.DUO_ENABLE = duoToggle.isOn;
                }
                break;
            //case "push":
            //    {
            //        if (pushToggle.isOn)
            //        {
            //            Notifications.GrantDataPrivacyConsent();
            //            Firebase.Messaging.FirebaseMessaging.TokenRegistrationOnInitEnabled = true;
            //        }
            //        else
            //        {
            //            Notifications.RevokeDataPrivacyConsent();
            //            Firebase.Messaging.FirebaseMessaging.TokenRegistrationOnInitEnabled = false;
            //            Firebase.Messaging.FirebaseMessaging.DeleteTokenAsync();
            //        }
            //    }
            //    break;
            default:
                print("변경하려는 값이 없습니다.");
                break;
        }
        PlayerPrefs.Save();
    }

    public void OnButton(int num)
    {
        if (PopupCanvas.Instance.IsOpeningPopup(PopupCanvas.POPUP_TYPE.MATCH_INFO_POPUP))
        {
            PopupCanvas.Instance.ShowFadeText("매치대기중사용불가");
            return;
        }

        switch (num)
        {
            case 1:
                {
                    string url = GameConfig.Instance.SUPPORT_URL;
                    string paramString = "";

                    long[] tfID =
                    {
                        900011725766, //닉네임 / 닉네임
                        5354068699161, //회원 번호 / 유저넘버
                    };

                    for (int i = 0; i < tfID.Length; i++)
                    {
                        if (i == 0)
                        {
                            if (url.Contains("?"))
                                paramString += "&";
                            else
                                paramString += "?";
                        }
                        else
                            paramString += "&";

                        paramString += "tf_" + tfID[i].ToString() + "=";
                        switch (i)
                        {
                            case 0:
                                paramString += UnityEngine.Networking.UnityWebRequest.EscapeURL(Managers.UserData.MyName);
                                break;
                            case 1:
                                paramString += Managers.UserData.MyUserID;
                                break;
                        }
                    }

                    Application.OpenURL(url + paramString);
                }
                break;
            case 2://계정 탈퇴
                {
                    bool linked = SAMANDA.Instance.GetLinkedAuth().Contains(AUTH_TYPE.AP) || SAMANDA.Instance.GetLinkedAuth().Contains(AUTH_TYPE.GG);
                    PopupCanvas.Instance.ShowConfirmPopup(linked ? "계정삭제" : "계정삭제_게스트", () =>
                    {
                        if (!linked)
                        {
                            PlayerPrefs.SetString("GuestAccount", "");
                            PlayerPrefs.SetString("GuestToken", "");
                        }

                        WWWForm param = new WWWForm();

                        SBWeb.SendPost("account/delete", param, (response) =>
                        {
                            SAMANDA.Instance.OnLogout();
                        });
                    });
                }
                break;
            case 3://coupon
                {
                    string url = GameConfig.Instance.COUPON_URL;
                    string paramString = "";

                    string[] paramsKey =
                    {
                        "user_no"
                    };

                    for (int i = 0; i < paramsKey.Length; i++)
                    {
                        if (i == 0)
                        {
                            if (url.Contains("?"))
                                paramString += "&";
                            else
                                paramString += "?";
                        }
                        else
                            paramString += "&";

                        paramString += paramsKey[i].ToString() + "=";
                        switch (i)
                        {
                            case 0:
                                paramString += Managers.UserData.MyUserID;
                                break;
                        }
                    }

                    SAMANDA.Instance.OnWebView(url + paramString);

                }
                break;

            default:
                PopupCanvas.Instance.ShowFadeText(StringManager.GetString("msg_develop"));
                break;
        }
    }

    public void OnLanguageChanged()
    {
        SystemLanguage curLang = GameConfig.Instance.OPTION_LANGUAGE;
        SystemLanguage targetLang = SystemLanguage.Unknown;
        LanguageIndex index = (LanguageIndex)languageDropdown.value;
        switch (index)
        {
            case LanguageIndex.KOR:
                targetLang = SystemLanguage.Korean;
                break;
            case LanguageIndex.ENG:
                targetLang = SystemLanguage.English;
                break;
            case LanguageIndex.CHI:
                targetLang = SystemLanguage.Chinese;
                break;
            case LanguageIndex.JAP:
                targetLang = SystemLanguage.Japanese;
                break;
        }

        if (curLang != targetLang)
        {
            PopupCanvas.Instance.ShowConfirmPopup("옵션_언어변경경고", StringManager.GetString("button_check"), StringManager.GetString("button_cancel"), () =>
            {
                StringManager.Instance.ChangeLanguage(targetLang);
                UnityEngine.SceneManagement.SceneManager.LoadScene("Start");
            },
            () =>
            {
                languageDropdown.value = (int)GetCurLanguageIndex();
            });
        }
    }

    LanguageIndex GetCurLanguageIndex()
    {
        SystemLanguage curLang = GameConfig.Instance.OPTION_LANGUAGE;
        switch (curLang)
        {
            case SystemLanguage.Korean:
                return LanguageIndex.KOR;
            case SystemLanguage.English:
                return LanguageIndex.ENG;
            case SystemLanguage.Chinese:
                return LanguageIndex.CHI;
            case SystemLanguage.Japanese:
                return LanguageIndex.JAP;
        }

        return LanguageIndex.KOR;
    }

    public void OnLogoutButton()
    {
        if (PopupCanvas.Instance.IsOpeningPopup(PopupCanvas.POPUP_TYPE.MATCH_INFO_POPUP))
        {
            PopupCanvas.Instance.ShowFadeText("매치대기중사용불가");
            return;
        }

        bool linked = SAMANDA.Instance.GetLinkedAuth().Contains(AUTH_TYPE.AP) || SAMANDA.Instance.GetLinkedAuth().Contains(AUTH_TYPE.GG);

        PopupCanvas.Instance.ShowConfirmPopup(StringManager.GetString(linked ? "로그아웃" : "로그아웃_게스트"), () =>
        {
            SAMANDA.Instance.OnLogout();
        });
    }

    public void OnLinkAccount(int type)
    {
        if (PopupCanvas.Instance.IsOpeningPopup(PopupCanvas.POPUP_TYPE.MATCH_INFO_POPUP))
        {
            PopupCanvas.Instance.ShowFadeText("매치대기중사용불가");
            return;
        }

        account.OnAuthLinkButton(type);
    }
}
