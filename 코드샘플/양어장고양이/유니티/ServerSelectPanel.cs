using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ServerSelectPanel : MonoBehaviour
{
    private void Awake()
    {
#if !UNITY_EDITOR
        SceneManager.LoadScene("Splash");
#endif

        string s16 = "0123456789abcdef";
        string s1024 = "";
        for (int j = 0; j < 64; j++)
            s1024 += s16;
        string s1024x1024 = "";
        for (int i = 0; i < 1024; i++)
            s1024x1024 += s1024;

        // try to save the string (it will fail in a webplayer build)
        try
        {
            PlayerPrefs.SetString("fail", s1024x1024);
        }
        // handle the error
        catch (System.Exception err)
        {
            Debug.Log("Got: " + err);
        }

        SetCurLanguageText();
    }

    public void OnClickDevServerButton()
    {
        SamandaStarter.LiveServer = false;

        SceneManager.LoadScene("Splash");
    }

    public void OnClickLiveServerButton()
    {
        SamandaStarter.LiveServer = true;

        SceneManager.LoadScene("Splash");
    }

    public void OnClickLiveServerIAPButton()
    {
        string ipAddr = "52.79.142.217/";
        NetworkManager.BASE_URL = ipAddr;
        NetworkManager.GAMESERVER_URL = ipAddr + "v5/";
        NetworkManager.CHAT_URL = ipAddr + "openchat";
        NetworkManager.PERSONAL_CHAT_URL = ipAddr + "personalchat";
        NetworkManager.DONATION_URL = ipAddr + "patron_list/list";

        OnClickLiveServerButton();
    }

    public Text curLanguage;

    public void SetCurLanguageText()
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

        switch (languageType)
        {
            case LANGUAGE_TYPE.LANGUAGE_KOR:
                curLanguage.text = "한국어";
                break;
            case LANGUAGE_TYPE.LANGUAGE_ENG:
                curLanguage.text = "영어";
                break;
            case LANGUAGE_TYPE.LANGUAGE_JPN:
                curLanguage.text = "일본어";
                break;
            case LANGUAGE_TYPE.LANGUAGE_IND:
                curLanguage.text = "인도네시아어";
                break;
        }
    }

    public void OnClickLanguageChange(int id)
    {
        PlayerPrefs.SetInt("Setting_Language", id);
        SetCurLanguageText();
    }
}
