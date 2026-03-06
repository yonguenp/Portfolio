using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public partial class SBWeb : MonoBehaviour
{
    public static string URL
    {
        get
        {
#if SB_TEST || UNITY_EDITOR
            if (GameConfig.Instance.USE_TEST_SERVER)
                return "http://sandbox-gs.mynetgear.com:52182/";

            if (GameConfig.Instance.USE_QA_SERVER)
                return "https://cmmmoweb-qa.sandbox-gs.com/";
#endif

            return "https://cmmmoweb.sandbox-gs.com/";
        }
    }

    public static string CDN_URL
    {
        get
        {
#if SB_TEST || UNITY_EDITOR
            if (GameConfig.Instance.USE_TEST_SERVER)
                return "https://d2yc60u7grhswl.cloudfront.net/";
#endif
            return GameConfig.Instance.CDN_URL;
        }
    }

    public delegate void SuccessCallback(JToken body);
    public delegate void FailCallback();

    protected static SBWeb _instance = null;
    protected static WebIndecator indecator = null;

    List<UnityWebRequest> requestList = new List<UnityWebRequest>();

    public static SBWeb Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Managers.Instance.gameObject.AddComponent<SBWeb>();
                GameObject prefab = Resources.Load("Prefabs/UI/WebIndecator") as GameObject;
                GameObject WebIndecator = GameObject.Instantiate(prefab) as GameObject;
                WebIndecator.transform.SetParent(_instance.transform);
                WebIndecator.transform.localScale = Vector3.one;
                WebIndecator.transform.localPosition = Vector3.zero;
                indecator = WebIndecator.GetComponent<WebIndecator>();
            }
            return _instance;
        }
    }


    // #if SB_TEST || UNITY_EDITOR
    //     public void SetTestWebServerURL()
    //     {
    //         URL = "http://sandbox-gs.mynetgear.com:52182/";
    //     }
    // #endif

    //     private void Awake()
    //     {
    // #if SB_TEST || UNITY_EDITOR
    //         if (GameConfig.Instance.USE_TEST_SERVEER)
    //         {
    //             SetTestWebServerURL();
    //         }
    // #endif
    //     }

    public static void SendPost(string uri,
        WWWForm param,
        SuccessCallback onSuccess = null)
    {
        Instance.StartCoroutine(Instance.SendPostCorutine(uri, param, onSuccess));
    }

    IEnumerator SendPostCorutine(string uri,
        WWWForm param,
        SuccessCallback onSuccess)
    {
        if (param == null)
            param = new WWWForm();

        if (Managers.UserData.MyUserID != 0)
            param.AddField("user_no", Managers.UserData.MyUserID.ToString());
        if (!string.IsNullOrEmpty(Managers.UserData.MyWebSessionID))
            param.AddField("session_id", Managers.UserData.MyWebSessionID);
        if (!string.IsNullOrEmpty(GameConfig.Instance.VERSION))
            param.AddField("client_version", GameConfig.Instance.VERSION);

        indecator.SetActive(true);

        using (UnityWebRequest req = UnityWebRequest.Post(URL + uri, param))
        {
            requestList.Add(req);

            req.timeout = 10;
            yield return req.SendWebRequest();

            requestList.Remove(req);

            if (req.result != UnityWebRequest.Result.ConnectionError && req.result != UnityWebRequest.Result.ProtocolError)
            {
                if (requestList.Count == 0)
                    indecator.SetActive(false);

                if (onSuccess != null)
                {
                    try
                    {
                        string body = req.downloadHandler.text;
                        onSuccess(JToken.Parse(body));
                    }
                    catch
                    {
                        SBDebug.LogError("====== Response Error :" + uri + ": req :" + req.downloadHandler.text);
                        string body = req.downloadHandler.text;
                        onSuccess(JToken.Parse(body));
                    };
                }
            }
            else
            {
                indecator.OnNetworkError(uri, param, onSuccess);
            }
        }
    }

    public void SetIAPProcessing(bool procesing)
    {
        indecator.SetActive(procesing);
    }
}
