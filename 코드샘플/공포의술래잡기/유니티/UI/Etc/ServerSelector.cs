using SandboxPlatform.SAMANDA;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ServerSelector : MonoBehaviour
{
    [SerializeField]
    string ip = string.Empty;
    [SerializeField]
    short port = 4319;
    [SerializeField]
    bool useTestWebServer = false;
    [SerializeField]
    bool useQAWebServer = false;

    Color originColor;

    private void Awake()
    {
        originColor = GetComponent<Image>().color;
    }

    public void OnClick()
    {
        if (ip == string.Empty)
        {
            SBDebug.LogError("ip를 입력해주세요.");
            return;
        }

#if SB_TEST || UNITY_EDITOR
        if (useTestWebServer)
        {
            GameConfig.Instance.SetTestServer();
        }
        if(useQAWebServer)
        {
            GameConfig.Instance.SetQAServer();
        }
#endif
        
       ServerInfo.Instance.IP = ip;
       ServerInfo.Instance.PORT = port;

        SceneManager.LoadScene("Start");
    }

    public void OnClickWithDummy()
    {
#if UNITY_EDITOR || SB_TEST
        GameConfig.Instance.SetDummy(!GameConfig.Instance.USE_DUMMY);
        GetComponent<Image>().color = (GameConfig.Instance.USE_DUMMY) ? Color.green : originColor;
#endif
    }

    public void OnToggleChaser(bool chaser)
    {
#if UNITY_EDITOR || SB_TEST
        GameConfig.Instance.SetDummyChaserPlay(GetComponentInChildren<Toggle>().isOn);
#endif
    }

    public void ClearAccount()
    {
        PlayerPrefs.DeleteKey("account_token");
        PlayerPrefs.DeleteKey("account_ano");
        PlayerPrefs.DeleteKey("UseLocalData");

        PlayerPrefs.DeleteKey("GuestAccount");
        PlayerPrefs.DeleteKey("GuestToken");
    }

    public void ClearAllData()
    {
        PlayerPrefs.DeleteAll();
        Caching.ClearCache();
    }
}
