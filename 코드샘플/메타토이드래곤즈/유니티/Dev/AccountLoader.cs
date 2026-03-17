using Newtonsoft.Json.Linq;
using SandboxNetwork;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AccountLoader : MonoBehaviour
{
    [SerializeField] Dev dev = null;
    [SerializeField] GameObject Item = null;

    static public AccountLoader Instance { get; private set; } = null;

    void OnEnable()
    {
        Instance = this;
        LoadAccountHistory();
    }

    private void OnDisable()
    {
        Instance = null;
    }

    static public string AccountHistory {
        get {
            if (NetworkManager.IsLiveServer)
                return PlayerPrefs.GetString(SBGameManager.GetPrefStringByServer("GuestInfo_live"), "");
            else
            {
#if SB_TEST
                if(NetworkManager.IsQAServer)
                    return PlayerPrefs.GetString(SBGameManager.GetPrefStringByServer("GuestInfo_qa"), "");
#endif
                return PlayerPrefs.GetString(SBGameManager.GetPrefStringByServer("GuestInfo_dev"), "");
            }
        }

        set {
            if (NetworkManager.IsLiveServer)
                PlayerPrefs.SetString(SBGameManager.GetPrefStringByServer("GuestInfo_live"), value);
            else
            {
#if SB_TEST
                if (NetworkManager.IsQAServer)
                    PlayerPrefs.SetString(SBGameManager.GetPrefStringByServer("GuestInfo_qa"), value);
#endif
                PlayerPrefs.SetString(SBGameManager.GetPrefStringByServer("GuestInfo_dev"), value);
            }
        }
    }

    static public void CheckValidAccount()
    {
        
    }
    
    public void CacheClear()
    {
        AccountHistory = "";
        LoadAccountHistory();
    }

    public void Refresh()
    {
        LoadAccountHistory();
    }

    void LoadAccountHistory()
    {
        Item.SetActive(false);
        foreach (Transform child in Item.transform.parent)
        {
            if(child == Item.transform)
            {
                continue;
            }

            Destroy(child.gameObject);
        }


        string history = AccountLoader.AccountHistory;
        
        if (string.IsNullOrEmpty(history))
        {
            return;
        }

        JArray array = JArray.Parse(history);

        if (array.Count == 0)
        {
            return;
        }

        Item.SetActive(true);
        foreach (JObject info in array)
        {
            if (SBFunc.IsJTokenCheck(info["token_bin"]))
            {
                string nick = "UNKNOWN";

                if(SBFunc.IsJTokenCheck(info["user_base"]))
                {
                    JToken user_base = info["user_base"];
                    if(SBFunc.IsJTokenCheck(user_base["nick"]))
                    {
                        nick = user_base["nick"].Value<string>();
                    }
                }

                GameObject clone = Instantiate(Item.gameObject, Item.transform.parent);
                clone.name = info["token_bin"].Value<string>();

                clone.GetComponentInChildren<Text>().text = nick;
                clone.GetComponent<Button>().onClick.AddListener(() =>
                {
                    SBGameManager.Instance.UserAccessToken = info["token_bin"].Value<string>();
                    SBGameManager.Instance.UserNickname = nick;

                    dev.RefreshAccount();
                });

                clone.transform.Find("Del").GetComponent<Button>().onClick.AddListener(() =>
                {
                    string guestInfo = AccountLoader.AccountHistory;
                    JArray array = new JArray();
                    if (!string.IsNullOrEmpty(guestInfo))
                        array = JArray.Parse(guestInfo);

                    foreach (JObject info in array)
                    {
                        if (SBFunc.IsJTokenCheck(info["token_bin"]))
                        {
                            if (info["token_bin"].Value<string>() == clone.name)
                            {
                                array.Remove(info);
                                Destroy(clone);
                                break;
                            }
                        }
                    }

                    AccountLoader.AccountHistory = array.ToString();

                    dev.RefreshAccount();
                });
            }
                
        }

        Item.SetActive(false);

    }
}
