using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class NetworkManager
{
    public static string BASE_URL = "https://ffcat.sandbox-gs.com/";
    public static string DOWNLOAD_URL = BASE_URL;
    public static string SAMANDA_URL = "https://samanda.sandbox-gs.com/sdk-ffcat";
    public static string GAMESERVER_URL = BASE_URL + "v5/";
    public static string CUSTOMER_SERVICE_URL = "https://sandboxnetwork.zendesk.com/hc/ko";

    public static string CHAT_URL = "https://chat-ffcat.sandbox-gs.com/openchat";
    public static string PERSONAL_CHAT_URL = "https://chat-ffcat.sandbox-gs.com/personalchat";
    public static string DONATION_URL = BASE_URL + "patron_list/list";

    // callbacks for generic post sender
    public delegate void SuccessCallback(string body);
    public delegate void FailCallback();
    public delegate void Callback();

    // api callbacks
    public delegate void ApiSuccessCallback(string body);
    public delegate void ApiFailCallback(eResponseCode err);

    public delegate bool ApiStandardCallback(JObject jsonRow);

    // static instance
    private static NetworkManager _sharedInstance = null;
    public static NetworkManager GetInstance()
    {
        if (null == _sharedInstance)
        {
            _sharedInstance = new NetworkManager();
        }
        return _sharedInstance;
    }

    // registered callbacks
    private Dictionary<string, ApiStandardCallback> _apiHandlers;

    private Callback JWT_Success = null;
    private Callback JWT_Fail = null;

    // session token
    private string _sessionToken;
    private Coroutine NetworkCheckerCoroutine = null;

    public string SessionToken
    {
        get { return _sessionToken; }
        set { _sessionToken = value; }
    }

    // account no
    private long _userNo;
    public long UserNo
    {
        get { return _userNo; }
        set { _userNo = value; }
    }

    private int _serverTime;
    public Callback TimerReset = null;
    public int ServerTime
    {
        get { return _serverTime; }
        set { _serverTime = value; TimerReset?.Invoke(); }
    }
    private NetworkManager()
    {
        // initialize callback dictionary
        _apiHandlers = new Dictionary<string, ApiStandardCallback>()
        {
            { "auth", OnResponseAuth },
            { "data", OnResponseData },
            { "user", OnResponseUser },
            { "inter", OnResponseInter },
            { "card", OnResponseCard },
            { "item", OnResponseItem },
            { "collection", OnResponseCollection },
            { "friend", OnResponseFriend },
            { "adventure", OnAdventure },
            { "cat", OnResponseCat },
            { "plant", OnResponsePlant },
            { "neco", OnResponseNeco },
            { "object", OnResponseNecoObject },
            { "upgrade", OnResponseUpgrade },
            { "mission", OnResponseMission },
            { "shop", OnResponseShop },
            { "post", OnResponsePost },
            { "iap", OnResponseIAP },
            { "gacha", OnResponseGacha },
            { "event", OnResponseEvent },
        };

        SessionToken = "-";
        UserNo = 0;
    }

    public void SendPost(string uri,
        WWWForm param,
        SuccessCallback onSuccess,
        FailCallback onFail = null,
        bool bRetry = true)
    {
        GameObject canvas = GameObject.Find("SAMANDA_CANVAS");
        if (canvas)
        {
            canvas.GetComponent<SamandaStarter>().StartCoroutine(SendPostCorutine(uri, param, onSuccess, onFail, bRetry));
        }
        else
        {
#if UNITY_EDITOR
            Debug.LogWarning(LocalizeData.GetText("CONNECTION_ERROR"));
#else
            Debug.LogError(LocalizeData.GetText("CONNECTION_ERROR"));
#endif
        }
    }

    public void SendPostSimple(string uri,
        int opCode,
        WWWForm param,
        SuccessCallback onSuccess = null)
    {
        GameObject canvas = GameObject.Find("SAMANDA_CANVAS");
        if (canvas)
        {
            canvas.GetComponent<SamandaStarter>().StartCoroutine(SendSimplePostCorutine(uri, opCode, param, onSuccess));
        }
        else
        {
#if UNITY_EDITOR
            Debug.LogWarning(LocalizeData.GetText("CONNECTION_ERROR"));
#else
            Debug.LogError(LocalizeData.GetText("CONNECTION_ERROR"));
#endif
        }
    }

    public IEnumerator SendSimplePostCorutine(string uri,
        int opCode,
        WWWForm param,
        SuccessCallback onSuccess)
    {
        param.AddField("tk", SessionToken);
        param.AddField("un", UserNo.ToString());
        param.AddField("op", opCode);
        param.AddField("ver", GameDataManager.Instance.GetVersion());

        uri = NetworkManager.GAMESERVER_URL + uri;
        using (UnityWebRequest req = UnityWebRequest.Post(uri, param))
        {
            req.timeout = 30;
            yield return req.SendWebRequest();

            if (onSuccess != null)
            {
                if (!req.isNetworkError && !req.isHttpError)
                {
                    string body = req.downloadHandler.text;
                    ResponseRoot objRoot = JsonUtility.FromJson<ResponseRoot>(body);
                    if (null == objRoot || (int)eResponseCode.OK != objRoot.rs)
                    {
                        if (objRoot.rs == eResponseCode.DUPLICATED_LOGIN)
                            GenericApiFail(objRoot.rs);
                    }
                    else
                    {
                        TimeSyncCheck(objRoot.ts);
                        onSuccess(body);
                    }
                }
            }
        }
    }

    public IEnumerator SendPostCorutine(string uri,
        WWWForm param,
        SuccessCallback onSuccess,
        FailCallback onFail = null,
        bool bRetry = true)
    {
        NecoCanvas.GetPopupCanvas()?.OnServerWait();

        using (UnityWebRequest req = UnityWebRequest.Post(uri, param))
        {
            req.timeout = 30;
            yield return req.SendWebRequest();

            if (!req.isNetworkError && !req.isHttpError)
            {
                NecoCanvas.GetPopupCanvas()?.OnPopupClose(NecoPopupCanvas.POPUP_TYPE.SERVER_WAIT_POPUP);

                string body = req.downloadHandler.text;
                onSuccess(body);
            }
            else
            {
                if (bRetry)
                {
                    NecoCanvas.GetPopupCanvas()?.OnServerRetry(uri, param, onSuccess, onFail);
                }
                else
                {
                    Debug.Log(req.error);
                    onFail?.Invoke();
                    GenericFail();
                }
            }

            GameObject canvas = GameObject.Find("SAMANDA_CANVAS");
            if (canvas)
            {
                if (NetworkCheckerCoroutine != null)
                    canvas.GetComponent<SamandaStarter>().StopCoroutine(NetworkCheckerCoroutine);

                NetworkCheckerCoroutine = canvas.GetComponent<SamandaStarter>().StartCoroutine(NetworkHeartBeatChecker());
            }
        }
    }

    public IEnumerator NetworkHeartBeatChecker(bool waitTIme = true)
    {
        if (waitTIme)
        {
            yield return new WaitForSeconds(15 * 60);
        }

        bool CheckSuccessed = false;
        do
        {
            string uri = NetworkManager.BASE_URL + "system/heartbeat";

            WWWForm param = new WWWForm();

            int browserType = 0;
#if UNITY_ANDROID
            browserType = 1;
#elif UNITY_IOS
            browserType = 3;
#endif      
            param.AddField("ano", SamandaLauncher.GetAccountNo());
            param.AddField("pid", SamandaLauncher.GetPID());
            param.AddField("bt", browserType);

            using (UnityWebRequest req = UnityWebRequest.Post(uri, param))
            {
                req.timeout = 30;
                yield return req.SendWebRequest();

                if (!req.isNetworkError && !req.isHttpError)
                {
                    try
                    {
                        string body = req.downloadHandler.text;
                        ResponseRoot objRoot = JsonUtility.FromJson<ResponseRoot>(body);
                        TimeSyncCheck(objRoot.ts);

                        CheckSuccessed = true;
                    }
                    catch
                    {
                        Debug.Log("heartbeat error");
                    }

                }
            }

            yield return new WaitForSeconds(5 * 60);

        } while (CheckSuccessed == false);
        //todo api

    }

    public void GenericFail()
    {
        Debug.Log("NetworkManager Response Failed!");
    }

    public void SendApiRequest(string uri,
        int opCode,
        WWWForm param,
        ApiSuccessCallback onSuccess = null,
        ApiFailCallback onFail = null,
        bool bRetry = true)
    {
        // 기본값 설정
        param.AddField("tk", SessionToken);
        param.AddField("un", UserNo.ToString());
        param.AddField("op", opCode);
        param.AddField("ver", GameDataManager.Instance.GetVersion());

        Debug.Log(string.Format("Api Call : {0}", Encoding.ASCII.GetString(param.data)));

        SendPost(NetworkManager.GAMESERVER_URL + uri,
            param,
            (string body) =>
            {
                Debug.Log(body);
                // if isError
                ResponseRoot objRoot = JsonUtility.FromJson<ResponseRoot>(body);
                if (null == objRoot || (int)eResponseCode.OK != objRoot.rs)
                {
                    onFail?.Invoke(objRoot.rs);
                    GenericApiFail(objRoot.rs);
                    return;
                }

                TimeSyncCheck(objRoot.ts);
                onSuccess?.Invoke(body);

                HandleApiList(body);
            },
            () =>
            {
                onFail?.Invoke(eResponseCode.SERVER_ERROR);
                GenericApiFail(eResponseCode.SERVER_ERROR);
            },
        bRetry);
    }

    public static void HandleApiList(string body)
    {
        JObject root = JObject.Parse(body);

        JToken apiToken = root["api"];
        if (null == apiToken || apiToken.Type != JTokenType.Array
            || !apiToken.HasValues)
        {
            return;
        }

        bool needSave = false;
        JArray apiArr = (JArray)apiToken;
        foreach (JObject row in apiArr)
        {
            string uri = row.GetValue("uri").ToString();
            if (HandleApiRow(uri, row))
                needSave = true;
        }

        if (needSave)
            GameDataManager.Instance.SaveGameData();
    }

    public static bool HandleApiRow(string uri, JObject row)
    {
        NetworkManager instance = GetInstance();
        if (instance._apiHandlers.ContainsKey(uri) && instance._apiHandlers[uri] != null)
        {
            return instance._apiHandlers[uri](row);
        }

        return false;
    }

    public static void GenericApiFail(eResponseCode err)
    {
        switch (err)
        {
            case eResponseCode.SERVER_MAINTENANCE:
            case eResponseCode.DUPLICATED_LOGIN:
            case eResponseCode.LOGGED_OUT_BY_SERVER:
                if (NecoCanvas.GetPopupCanvas() != null)
                {
                    NecoCanvas.GetPopupCanvas().OnPopupClose();
                    NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(LocalizeData.GetText("error"), LocalizeData.GetText("ACCOUNT_ERROR"), () =>
                    {
                        if (err == eResponseCode.LOGGED_OUT_BY_SERVER)
                            SamandaLauncher.OnLogout();

                        SceneLoader.Instance.LoadScene("Intro");
                    });
                }
                break;

            case eResponseCode.VERSION_TOO_LOW:
                if (NecoCanvas.GetPopupCanvas() != null)
                {
                    NecoCanvas.GetPopupCanvas().OnPopupClose();
                    NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(LocalizeData.GetText("error"), LocalizeData.GetText("version_check"), () =>
                    {
                        Application.Quit(); // 어플리케이션 종료
                    });
                }
                else
                {
                    GameObject canvas = GameObject.Find("Canvas");
                    if (canvas)
                    {
                        intro_manager mgr = canvas.GetComponent<intro_manager>();
                        if (mgr)
                        {
                            GameObject popup = mgr.TestDonePopup;
                            if (popup)
                            {
                                popup.SetActive(true);
                            }
                        }
                    }
                }
                break;

            case eResponseCode.CANNOT_CONNECT:
            case eResponseCode.SERVER_ERROR:
            case eResponseCode.SERVER_TOO_BUSY:
            case eResponseCode.SQL_ERROR:
            case eResponseCode.REDIS_ERROR:
            case eResponseCode.SCRIPT_NOT_FOUND:
            default:
                if (NecoCanvas.GetPopupCanvas() != null)
                {
                    NecoCanvas.GetPopupCanvas().OnPopupClose();
                    NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(LocalizeData.GetText("error"), LocalizeData.GetText("SERVER_ERROR"), null);
                }
                else
                {
                    Debug.Log(string.Format("Err with out ErrPopup : {0}", (int)err));
                }
                break;
        }
    }

    public bool OnResponseCollection(JObject jObject)
    {
        bool needSave = false;
        JToken resultCode = jObject["rs"];
        if (resultCode == null || resultCode.Type != JTokenType.Integer)
            return false;

        Debug.Log(jObject.ToString());

        int rs = resultCode.Value<int>();
        if (rs != 0)
            return false;

        JToken opCode = jObject["op"];
        if (opCode != null && opCode.Type == JTokenType.Integer)
        {
            int op = opCode.Value<int>();

            switch (op)
            {
                case 1:
                    {
                        uint id = jObject["id"].Value<uint>();
                        JToken rew = jObject["rew"];
                        if (rew != null)
                        {
                            List<KeyValuePair<string, uint>> rewList = new List<KeyValuePair<string, uint>>();
                            if (!GameDataManager.Instance.GetUserData().data.ContainsKey("collection_rew"))
                            {
                                GameDataManager.Instance.GetUserData().data.Add("collection_rew", new List<KeyValuePair<uint, List<KeyValuePair<string, uint>>>>());
                            }

                            ((List<KeyValuePair<uint, List<KeyValuePair<string, uint>>>>)(GameDataManager.Instance.GetUserData().data["collection_rew"])).Add(new KeyValuePair<uint, List<KeyValuePair<string, uint>>>(id, rewList));

                            JObject row = (JObject)rew;
                            foreach (JProperty property in row.Properties())
                            {
                                if (property.Value.Type == JTokenType.Array)
                                {
                                    JArray ar = (JArray)property.Value;
                                    foreach (JToken val in ar)
                                    {
                                        if (val.Type == JTokenType.Object)
                                        {
                                            for (int i = 0; i < val["amount"].Value<uint>(); i++)
                                            {
                                                rewList.Add(new KeyValuePair<string, uint>(property.Name, val["id"].Value<uint>()));
                                            }
                                        }
                                        else
                                        {
                                            rewList.Add(new KeyValuePair<string, uint>(property.Name, val.Value<uint>()));
                                        }
                                    }
                                }
                                else
                                    rewList.Add(new KeyValuePair<string, uint>(property.Name, property.Value.Value<uint>()));
                            }
                        }
                    }
                    break;
                case 101: //OpCollection::COLLECTION_REWARDED
                    {
                        JToken id = jObject["id"];
                        List<game_data> user_collection_list = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.USER_COLLECTION);
                        if (user_collection_list == null)
                        {
                            user_collection_list = GameDataManager.Instance.GetEmptyGameDataWithType(GameDataManager.DATA_TYPE.USER_COLLECTION);
                        }

                        user_collection newData = new user_collection();
                        newData.data.Add("collection_id", id.Value<uint>());
                        user_collection_list.Add(newData);

                        needSave = true;
                    }
                    break;
                default:
                    break;
            }
        }

        return needSave;
    }

    public bool OnResponseItem(JObject jObject)
    {
        bool needSave = false;

        JToken opCode = jObject["op"];
        if (opCode != null && opCode.Type == JTokenType.Integer)
        {
            int op = opCode.Value<int>();

            switch (op)
            {
                case 101: //OpItem::ITEM_AMOUNT_UPDATE
                    {
                        List<game_data> data_list = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.USER_ITEMS);

                        if (data_list == null)
                        {
                            data_list = GameDataManager.Instance.GetEmptyGameDataWithType(GameDataManager.DATA_TYPE.USER_ITEMS);
                        }

                        user_items userItem = null;
                        uint itemID = 0;

                        if (jObject.ContainsKey("after"))
                        {
                            jObject = (JObject)jObject["after"];
                            itemID = jObject["id"].Value<uint>();
                        }

                        object obj;
                        foreach (game_data data in data_list)
                        {
                            if (data.data.TryGetValue("item_id", out obj))
                            {
                                if ((uint)obj == itemID)
                                {
                                    userItem = (user_items)data;
                                    break;
                                }
                            }
                        }

                        bool isNew = false;
                        if (userItem == null)
                        {
                            userItem = (user_items)game_data.CreateGameData(GameDataManager.DATA_TYPE.USER_ITEMS);
                            userItem.data["item_id"] = itemID;
                            isNew = true;

                            if (itemID == 129)
                            {
                                PlayerPrefs.SetInt("AUTO_DISPENSER", 1);

                                NecoCanvas.GetGameCanvas().GetCurMapController()?.RefreshFoodTruck();
                            }
                        }

                        uint get = jObject["get_amount"].Value<uint>();
                        uint used = jObject["used_amount"].Value<uint>();

                        userItem.data["get_amount"] = get;
                        userItem.data["used_amount"] = used;

                        userItem.data["get_time"] = jObject["get_time"].Value<uint>();

                        needSave = true;

                        if (isNew)
                        {
                            data_list.Add(userItem);
                        }
                    }
                    break;

                case 102: //OpItem::FOOD_AMOUNT_UPDATE
                    break;
                default:
                    break;
            }
        }

        UINotifiedManager.UpdateData("item");

        return needSave;
    }

    public bool OnResponseCard(JObject jObject)
    {
        bool needSave = false;

        JToken opCode = jObject["op"];
        if (opCode != null && opCode.Type == JTokenType.Integer)
        {
            int op = opCode.Value<int>();

            switch (op)
            {
                case 1:
                    {
                        JToken rs = jObject["rs"];
                        if (rs != null && rs.Type == JTokenType.Integer && rs.Value<int>() == 0)
                        {
                            JToken uidToken = jObject["uid"];
                            uint uid = uidToken.Value<uint>();
                            List<game_data> data_list = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.USER_CARD);
                            foreach (game_data data in data_list)
                            {
                                if (((user_card)data).GetCardUniqueID() == uid)
                                {
                                    data.data["memo"] = jObject["memo"].Value<string>();

                                    needSave = true;
                                }
                            }
                        }
                    }
                    break;
                case 2:
                    {
                        JToken rs = jObject["rs"];
                        if (rs != null && rs.Type == JTokenType.Integer && rs.Value<int>() == 0)
                        {
                            JArray arr = (JArray)jObject["uid"];
                            foreach (JToken val in arr)
                            {
                                uint uid = val.Value<uint>();

                                List<game_data> data_list = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.USER_CARD);
                                List<game_data> removeList = new List<game_data>();
                                foreach (game_data data in data_list)
                                {
                                    if (((user_card)data).GetCardUniqueID() == uid)
                                    {
                                        removeList.Add(data);
                                    }
                                }

                                foreach (game_data data in removeList)
                                {
                                    data_list.Remove(data);
                                    needSave = true;
                                }
                            }
                        }
                    }
                    break;
                case 3:
                    {
                        JToken rs = jObject["rs"];
                        if (rs != null && rs.Type == JTokenType.Integer && rs.Value<int>() == 0)
                        {
                            GameDataManager.Instance.GetUserData().data["max_cards"] = jObject["max"].Value<uint>();
                        }
                    }
                    break;
                case 101: //OpCard::CARD_OBTAIN 
                    {
                        List<game_data> data_list = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.USER_CARD);

                        if (data_list == null)
                        {
                            data_list = GameDataManager.Instance.GetEmptyGameDataWithType(GameDataManager.DATA_TYPE.USER_CARD);
                        }

                        user_card userCard = (user_card)game_data.CreateGameData(GameDataManager.DATA_TYPE.USER_CARD);
                        userCard.data["card_id"] = jObject["id"].Value<uint>();
                        userCard.data["card_uid"] = jObject["uid"].Value<uint>();
                        userCard.data["rect"] = jObject["rct"].Value<string>();
                        userCard.data["memo"] = jObject["memo"].Value<string>();
                        userCard.data["get_time"] = jObject["get_time"].Value<uint>();
                        needSave = true;
                        data_list.Add(userCard);
                    }
                    break;

                case 102: //OpCard::CARD_LOST 
                    {
                        List<game_data> data_list = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.USER_CARD);

                        if (data_list == null)
                        {
                            data_list = GameDataManager.Instance.GetEmptyGameDataWithType(GameDataManager.DATA_TYPE.USER_CARD);
                        }

                        JArray arr = (JArray)jObject["uid"];
                        foreach (JToken val in arr)
                        {
                            uint uid = val.Value<uint>();
                            List<game_data> removeList = new List<game_data>();
                            foreach (game_data data in data_list)
                            {
                                if (((user_card)data).GetCardUniqueID() == uid)
                                {
                                    removeList.Add(data);
                                }
                            }

                            foreach (game_data data in removeList)
                            {
                                data_list.Remove(data);
                                needSave = true;
                            }
                        }
                    }
                    break;

                case 103: //OpCard::ALBUM_UPDATE 
                    List<game_data> user_album = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.ALBUM);
                    JObject album = (JObject)jObject["album"];
                    if (album != null)
                    {
                        object obj;
                        foreach (JProperty property in album.Properties())
                        {
                            uint id = uint.Parse(property.Name);

                            bool isUpdate = false;
                            foreach (game_data ua in user_album)
                            {
                                if (ua.data.TryGetValue("card_id", out obj))
                                {
                                    if (id == (uint)obj)
                                    {
                                        ua.data["flag"] = property.Value.Value<uint>();
                                        isUpdate = true;
                                    }
                                }
                            }

                            if (isUpdate == false)
                            {
                                album _a = new album();
                                _a.data["card_id"] = id;
                                _a.data["flag"] = property.Value.Value<uint>();
                                user_album.Add(_a);
                            }
                        }
                    }
                    needSave = true;
                    break;

                default:
                    break;
            }
        }

        return needSave;
    }

    public bool OnResponseInter(JObject jObject)
    {
        bool needSave = false;

        Debug.Log(jObject.ToString());

        JToken opCode = jObject["op"];
        if (opCode != null && opCode.Type == JTokenType.Integer)
        {
            int op = opCode.Value<int>();
            switch (op)
            {
                case 101: //new touch
                    user_inter_touch newTouch = (user_inter_touch)game_data.CreateGameData(GameDataManager.DATA_TYPE.USER_INTER_TOUCH);
                    newTouch.data.Add("touch_id", jObject["id"].Value<uint>());
                    newTouch.data.Add("get_time", jObject["get_time"].Value<uint>());
                    newTouch.data.Add("last_run_day", jObject["last_run_day"].Value<uint>());
                    newTouch.data.Add("today_run_count", jObject["today_run_count"].Value<uint>());
                    GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.USER_INTER_TOUCH).Add(newTouch);
                    needSave = true;
                    break;

                case 102: //new play
                    user_inter_play newPlay = (user_inter_play)game_data.CreateGameData(GameDataManager.DATA_TYPE.USER_INTER_PLAY);
                    newPlay.data.Add("play_id", jObject["id"].Value<uint>());
                    newPlay.data.Add("get_time", jObject["get_time"].Value<uint>());
                    newPlay.data.Add("last_run_day", jObject["last_run_day"].Value<uint>());
                    newPlay.data.Add("today_run_count", jObject["today_run_count"].Value<uint>());
                    GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.USER_INTER_TOUCH).Add(newPlay);
                    needSave = true;
                    break;

                case 103: //OpInter::IDLE_STATE
                    game_data data = GameDataManager.Instance.GetUserData();
                    data.data["is_active"] = jObject["is_active"].Value<uint>();
                    data.data["last_reward_time"] = jObject["last_reward"].Value<uint>();
                    data.data["rate"] = jObject["rate"].Value<double>();
                    data.data["tick_cycle"] = jObject["tick_cycle"].Value<double>();
                    data.data["next_tick"] = jObject["next_tick"].Value<uint>();

                    needSave = true;
                    break;

                default:
                    break;
            }
        }

        return needSave;
    }

    public bool OnResponseUser(JObject jObject)
    {
        bool needSave = false;
        JToken opCode = jObject["op"];
        if (opCode != null && opCode.Type == JTokenType.Integer)
        {
            int op = opCode.Value<int>();
            switch (op)
            {
                case 1: //exp
                    GameDataManager.Instance.GetUserData().data["exp"] = jObject["after"].Value<uint>();
                    needSave = true;
                    break;

                case 2: /// level up
                    uint after = jObject["after"].Value<uint>();
                    GameDataManager.Instance.GetUserData().data["level"] = after;
                    needSave = true;

                    if (jObject.ContainsKey("rew"))
                    {
                        JToken rew = jObject["rew"];
                        if (rew != null)
                        {
                            List<KeyValuePair<string, uint>> rewList = new List<KeyValuePair<string, uint>>();
                            if (!GameDataManager.Instance.GetUserData().data.ContainsKey("rew"))
                            {
                                GameDataManager.Instance.GetUserData().data.Add("rew", new List<KeyValuePair<uint, List<KeyValuePair<string, uint>>>>());
                            }

                            ((List<KeyValuePair<uint, List<KeyValuePair<string, uint>>>>)(GameDataManager.Instance.GetUserData().data["rew"])).Add(new KeyValuePair<uint, List<KeyValuePair<string, uint>>>(after, rewList));

                            JObject row = (JObject)rew;
                            foreach (JProperty property in row.Properties())
                            {
                                if (property.Value.Type == JTokenType.Array)
                                {
                                    JArray ar = (JArray)property.Value;
                                    foreach (JToken val in ar)
                                    {
                                        if (val.Type == JTokenType.Object)
                                        {
                                            for (int i = 0; i < val["amount"].Value<uint>(); i++)
                                            {
                                                rewList.Add(new KeyValuePair<string, uint>(property.Name, val["id"].Value<uint>()));
                                            }
                                        }
                                        else
                                        {
                                            rewList.Add(new KeyValuePair<string, uint>(property.Name, val.Value<uint>()));
                                        }
                                    }
                                }
                                else
                                    rewList.Add(new KeyValuePair<string, uint>(property.Name, property.Value.Value<uint>()));
                            }
                        }
                    }

                    break;

                case 3: // gold
                    GameDataManager.Instance.GetUserData().data["gold"] = jObject["after"].Value<uint>();
                    needSave = true;

                    UINotifiedManager.UpdateData("gold");
                    break;

                case 4: //unlock content
                    GameDataManager.Instance.GetUserData().data["contents"] = jObject["val"].Value<uint>();
                    ContentLocker.UpdateUnlockSeq();
                    needSave = true;
                    break;

                case 5: //fullness update
                    GameDataManager.Instance.GetUserData().data["fullness"] = jObject["val"].Value<uint>();
                    needSave = true;
                    break;

                case 6://ready cat 별도 클라이언트구현
                    break;
                case 7: //catnip update
                    GameDataManager.Instance.GetUserData().data["catnip"] = jObject["after"].Value<uint>();
                    needSave = true;
                    break;

                case 8: //fullness update
                    GameDataManager.Instance.GetUserData().data["point"] = jObject["after"].Value<uint>();
                    needSave = true;
                    break;

                case 101: //프로필 이미지 변경내용
                    string[] profileList = jObject["list"].ToString().Split(',');

                    List<uint> list = new List<uint>();
                    foreach (JToken token in profileList)
                    {
                        list.Add(token.Value<uint>());
                    }
                    GameDataManager.Instance.GetUserData().data["profileList"] = list.ToArray();
                    needSave = true;
                    break;
            }
        }

        return needSave;
    }

    public bool OnResponseFriend(JObject jObject)
    {
        JToken opCode = jObject["op"];
        if (opCode != null && opCode.Type == JTokenType.Integer)
        {
            int op = opCode.Value<int>();
            switch (op)
            {
                case 1:
                    {
                        List<UserProfile> friends = FriendsManager.Instance.GetFriendList();
                        FriendsManager.Instance.DeleteUserProfiles(friends);
                        JArray users = (JArray)jObject["list"];
                        foreach (JToken user in users)
                        {
                            JObject userObject = (JObject)user;
                            UserProfile newUser = FriendsManager.Instance.UpdateUserProfile(userObject, UserProfile.FriendType.FRIEND);
                            foreach (UserProfile u in friends)
                            {
                                if (u.uno == newUser.uno)
                                {
                                    if (newUser.last_update < u.last_update)
                                        newUser.last_update = u.last_update;
                                    newUser.lastMessage = u.lastMessage;
                                }
                            }
                        }
                    }
                    break;
                case 2:
                    {
                        List<UserProfile> sents = FriendsManager.Instance.GetSentList();
                        FriendsManager.Instance.DeleteUserProfiles(sents);
                        JArray users = (JArray)jObject["list"];
                        foreach (JToken user in users)
                        {
                            JObject userObject = (JObject)user;
                            FriendsManager.Instance.UpdateUserProfile(userObject, UserProfile.FriendType.SENT);
                        }
                    }
                    break;
                case 3:
                    {
                        List<UserProfile> takens = FriendsManager.Instance.GetTakenList();
                        FriendsManager.Instance.DeleteUserProfiles(takens);
                        JArray users = (JArray)jObject["list"];
                        foreach (JToken user in users)
                        {
                            JObject userObject = (JObject)user;
                            FriendsManager.Instance.UpdateUserProfile(userObject, UserProfile.FriendType.TAKEN);
                        }
                    }
                    break;
                case 12: //FRIEND_LIST_UPDATE 
                    {
                        uint nf = 0;
                        uint rv = 0;
                        JToken friend = jObject["friend"];
                        if (friend != null)
                        {
                            if (friend.Value<uint>() > 0)
                            {
                                nf = friend.Value<uint>();
                            }
                        }
                        JToken rcv = jObject["rcv"];
                        if (rcv != null)
                        {
                            if (rcv.Value<uint>() > 0)
                            {
                                rv = rcv.Value<uint>();
                            }
                        }

                        FriendsManager.Instance.SetNewFriendCount(nf);
                        FriendsManager.Instance.SetNewRecivedCount(rv);
                    }
                    break;
                case 13:
                    {
                        List<UserProfile> recommands = FriendsManager.Instance.GetRecommandList();
                        FriendsManager.Instance.DeleteUserProfiles(recommands);
                        JArray users = (JArray)jObject["list"];
                        foreach (JToken user in users)
                        {
                            JObject userObject = (JObject)user;
                            FriendsManager.Instance.UpdateUserProfile(userObject, UserProfile.FriendType.RECOMMAND);
                        }
                    }
                    break;
                case 14:
                    {
                        List<UserProfile> blocked = FriendsManager.Instance.GetBlockedList();
                        FriendsManager.Instance.DeleteUserProfiles(blocked);
                        JArray users = (JArray)jObject["list"];
                        foreach (JToken user in users)
                        {
                            JObject userObject = (JObject)user;
                            FriendsManager.Instance.UpdateUserProfile(userObject, UserProfile.FriendType.BLOCKED);
                        }
                    }
                    break;
            }
        }

        return false;
    }

    public bool OnAdventure(JObject jObject)
    {
        bool bSave = false;
        JToken opCode = jObject["op"];
        if (opCode != null && opCode.Type == JTokenType.Integer)
        {
            int op = opCode.Value<int>();
            switch (op)
            {
                case 1:
                    {

                    }
                    break;
                case 2:
                    {

                    }
                    break;
                case 3:
                    {

                    }
                    break;
                case 101: //OpAdventure::ADVENTURE_STATE_UPDATE 
                    {

                    }
                    break;
                case 102: //OpAdventure::STAGE_PROGRESS_UPDATE  
                    {
                        uint stageNo = jObject["stage"].Value<uint>();
                        object obj;
                        List<game_data> user_statges = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.USER_STAGE);
                        if (user_statges != null)
                        {
                            bool isNew = true;
                            int flag = jObject["progress"].Value<int>();
                            foreach (game_data stage in user_statges)
                            {
                                if (stage.data.TryGetValue("stage_id", out obj))
                                {
                                    if (stageNo == (uint)obj)
                                    {
                                        stage.data["flag"] = flag;
                                        bSave = true;

                                        isNew = false;
                                    }
                                }
                            }

                            if (isNew)
                            {
                                user_stage newData = new user_stage();
                                newData.data.Add("stage_id", stageNo);
                                newData.data.Add("flag", flag);
                                user_statges.Add(newData);
                            }
                        }
                    }
                    break;
                case 103: //OpAdventure::LOCATION_UPDATE   
                    {
                        uint locationNo = jObject["loc"].Value<uint>();
                        if (jObject["state"].Value<uint>() > 0)
                        {
                            users userData = GameDataManager.Instance.GetUserData();
                            ((List<uint>)userData.data["unlocked_area"]).Add(locationNo);
                        }
                    }
                    break;
            }
        }

        return bSave;
    }

    public bool OnResponseCat(JObject jObject)
    {
        bool bSave = false;

        JToken opCode = jObject["op"];
        if (opCode != null && opCode.Type == JTokenType.Integer)
        {
            int op = opCode.Value<int>();
            switch (op)
            {
                case 3: //OpCat::OPEN_ACTION
                    {
                        JToken resultCode = jObject["rs"];
                        if (resultCode == null || resultCode.Type != JTokenType.Integer)
                            return false;

                        int rs = resultCode.Value<int>();
                        if (rs != 0)
                            return false;

                        uint cat_id = jObject["cat"].Value<uint>();
                        user_cats cat = user_cats.GetUserCatData(cat_id);
                        cat.data["act"] = jObject["act"].Value<uint>();

                        bSave = true;
                    }
                    break;
                case 101: //OpCat::OBTAIN                
                    {
                        object obj;
                        List<game_data> user_cats = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.USER_CATS);
                        if (user_cats != null)
                        {
                            bool isNew = true;
                            uint id = jObject["id"].Value<uint>();
                            foreach (game_data user_cat in user_cats)
                            {
                                if (user_cat.data.TryGetValue("id", out obj))
                                {
                                    if (id == (uint)obj)
                                    {
                                        user_cat.data["exp"] = jObject["exp"].Value<uint>();
                                        user_cat.data["lvl"] = jObject["lvl"].Value<uint>();
                                        user_cat.data["full"] = jObject["full"].Value<uint>();
                                        user_cat.data["aff"] = jObject["aff"].Value<uint>();
                                        user_cat.data["act"] = jObject["act"].Value<uint>();

                                        isNew = false;
                                    }
                                }
                            }

                            if (isNew)
                            {
                                user_cats newData = new user_cats();
                                newData.data["id"] = id;
                                newData.data["exp"] = jObject["exp"].Value<uint>();
                                newData.data["lvl"] = jObject["lvl"].Value<uint>();
                                newData.data["full"] = jObject["full"].Value<uint>();
                                newData.data["aff"] = jObject["aff"].Value<uint>();
                                newData.data["act"] = jObject["act"].Value<uint>();
                                user_cats.Add(newData);
                            }

                            bSave = true;
                        }
                    }
                    break;
                case 102: //OpCat::EXP_UPDATE 
                    {
                        uint cat_id = jObject["id"].Value<uint>();
                        user_cats cat = user_cats.GetUserCatData(cat_id);
                        cat.data["exp"] = jObject["after"].Value<uint>();

                        bSave = true;
                    }
                    break;
                case 103: //OpCat::LEVEL_UPDATE  
                    {
                        uint cat_id = jObject["id"].Value<uint>();
                        user_cats cat = user_cats.GetUserCatData(cat_id);
                        cat.data["lvl"] = jObject["after"].Value<uint>();

                        bSave = true;
                    }
                    break;
                case 104: //OpCat::FULLNESS_UPDATE  
                    {
                        uint cat_id = jObject["id"].Value<uint>();
                        user_cats cat = user_cats.GetUserCatData(cat_id);
                        cat.data["full"] = jObject["val"].Value<uint>();

                        bSave = true;
                    }
                    break;
                case 105: //OpCat::AFFECTION_UPDATE 
                    {
                        uint cat_id = jObject["id"].Value<uint>();
                        user_cats cat = user_cats.GetUserCatData(cat_id);
                        cat.data["aff"] = jObject["val"].Value<uint>();

                        bSave = true;
                    }
                    break;
            }
        }

        return bSave;
    }

    public bool OnResponsePlant(JObject jObject)
    {
        bool bSave = false;


        JToken opCode = jObject["op"];
        if (opCode != null && opCode.Type == JTokenType.Integer)
        {
            int op = opCode.Value<int>();
            switch (op)
            {
                case 101: //OpPlant::STATE_UPDATE 
                    {
                        neco_data.Instance.UpdatePlant(jObject);
                    }
                    break;
            }
        }

        return bSave;
    }

    public bool OnResponseNeco(JObject jObject)
    {
        bool bSave = false;

        JToken opCode = jObject["op"];
        if (opCode != null && opCode.Type == JTokenType.Integer)
        {
            int op = opCode.Value<int>();
            switch (op)
            {
                case 101:
                    {
                        if (jObject.ContainsKey("neco"))
                        {
                            JArray array = (JArray)jObject["neco"];

                            foreach (JObject data in array)
                            {
                                neco_user_cat.UpdateUserCatInfo(data);
                            }
                        }
                    }
                    break;
                case 102:
                    {
                        if (jObject.ContainsKey("new"))
                        {
                            JObject data = (JObject)jObject["new"];
                            neco_user_cat.UpdateUserCatInfo(data);
                        }
                    }
                    break;
            }
        }

        return bSave;
    }

    public bool OnResponseNecoObject(JObject jObject)
    {
        bool bSave = false;

        JToken opCode = jObject["op"];
        if (opCode != null && opCode.Type == JTokenType.Integer)
        {
            int op = opCode.Value<int>();
            switch (op)
            {
                case 2:
                    {
                        neco_data.Instance.Clear();
                    }
                    break;
                case 3:
                    {
                        neco_data.Instance.Clear();
                        if (jObject.ContainsKey("gifts"))
                        {
                            JArray array = (JArray)jObject["gifts"];
                            foreach (JObject gift in array)
                            {
                                items item = items.GetItem(gift["id"].Value<uint>());
                                if (item != null)
                                {
                                    neco_data.neco_gift_info info = new neco_data.neco_gift_info();
                                    info.item = item;
                                    info.count = gift["amount"].Value<uint>();
                                    neco_data.Instance.AddGift(info);
                                }
                            }
                        }
                    }
                    break;
                case 101:
                    {
                        if (jObject.ContainsKey("gift_boost"))
                        {
                            neco_data.BOOST_TYPE type = neco_data.BOOST_TYPE.NONE;
                            if (jObject.ContainsKey("gift_btype"))
                            {
                                if (jObject["gift_btype"].Value<uint>() == 1)
                                    type = neco_data.BOOST_TYPE.CATNIP_BOOST;
                                else
                                    type = neco_data.BOOST_TYPE.AD_BOOST;
                            }
                            neco_data.Instance.SetGiftBoostTime(jObject["gift_boost"].Value<uint>(), type);
                        }

                        if (jObject.ContainsKey("object_tick"))
                        {
                            neco_data.Instance.SetNextObjectUpdate(jObject["object_tick"].Value<uint>());
                        }

                        if (jObject.ContainsKey("objects"))
                        {
                            JArray array = (JArray)jObject["objects"];

                            foreach (JObject spotInfo in array)
                            {
                                uint id = spotInfo["id"].Value<uint>();

                                neco_spot spot = neco_spot.GetNecoSpot(id);
                                if (spot == null)
                                {
                                    spot = neco_spot.AddNecoSpot(id);

                                }

                                spot.UpdateData(spotInfo);
                            }
                        }

                        NecoCanvas.GetGameCanvas()?.RunCatSystem();
                    }
                    break;
                case 102:
                    {
                        //List<game_data> necoData = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.NECO_SPOT);
                        //if (necoData != null)
                        //{
                        //    foreach (neco_spot data in necoData)
                        //    {
                        //        if (data != null)
                        //        {
                        //            if (data.GetSpotType() != neco_spot.SPOT_TYPE.FOOD_SPOT)
                        //            {
                        //                data.RefreshItem();
                        //            }
                        //        }
                        //    }
                        //}
                    }
                    break;
                case 103:
                    {
                        if (jObject.ContainsKey("rew"))
                        {
                            uint cat = jObject["nid"].Value<uint>();
                            JObject income = (JObject)jObject["rew"];
                            if (income.ContainsKey("gold"))
                            {
                                NecoCanvas.GetUICanvas()?.OnCatVisitCoin(cat, income["gold"].Value<uint>());
                            }
                        }
                    }
                    break;
            }
        }

        return bSave;
    }

    public bool OnResponseUpgrade(JObject jObject)
    {
        bool bSave = false;

        JToken opCode = jObject["op"];
        if (opCode != null && opCode.Type == JTokenType.Integer)
        {
            int op = opCode.Value<int>();
            switch (op)
            {
                case 1:
                    {
                        if (jObject.ContainsKey("lvl") && jObject.ContainsKey("what"))
                        {
                            uint level = jObject["lvl"].Value<uint>();
                            uint target = jObject["what"].Value<uint>();

                            switch ((SUPPLY_UI_TYPE)target)
                            {
                                case SUPPLY_UI_TYPE.CAT_GIFT:
                                    neco_data.Instance.SetGiftBasketLevel(level);
                                    UINotifiedManager.UpdateData("upgradegift");
                                    break;
                                case SUPPLY_UI_TYPE.FISH_FARM:
                                    neco_data.Instance.SetFishfarmLevel(level);
                                    UINotifiedManager.UpdateData("upgradefarm");
                                    break;
                                case SUPPLY_UI_TYPE.FISH_TRAP:
                                    neco_data.Instance.SetFishtrapLevel(level);
                                    UINotifiedManager.UpdateData("upgradetrap");
                                    break;
                                case SUPPLY_UI_TYPE.WORKBENCH:
                                    neco_data.Instance.SetCraftRecipeLevel(level);
                                    UINotifiedManager.UpdateData("upgradecraft");
                                    break;
                                case SUPPLY_UI_TYPE.COUNTERTOP:
                                    neco_data.Instance.SetCookRecipeLevel(level);
                                    UINotifiedManager.UpdateData("upgradecook");
                                    break;

                            }
                        }
                    }
                    break;
            }
        }
        return bSave;
    }

    public bool OnResponseMission(JObject jObject)
    {
        bool bSave = false;

        JToken opCode = jObject["op"];
        if (opCode != null && opCode.Type == JTokenType.Integer)
        {
            int op = opCode.Value<int>();
            switch (op)
            {
                case 1:
                    {
                        JToken resultCode = jObject["rs"];
                        if (resultCode == null || resultCode.Type != JTokenType.Integer)
                            return false;

                        int rs = resultCode.Value<int>();
                        if (rs != 0)
                            return false;


                        neco_data.Instance.GetPassData().UpdatePassData(jObject);
                    }
                    break;
                case 2:
                    {
                        JToken resultCode = jObject["rs"];
                        if (resultCode == null || resultCode.Type != JTokenType.Integer)
                            return false;

                        int rs = resultCode.Value<int>();
                        if (rs != 0)
                            return false;


                        neco_data.Instance.GetPassData().UpdatePassRewarded(jObject);
                    }
                    break;
                case 4:
                    {
                        neco_data.Instance.GetPassData().UpdateMissionData(jObject);
                    }
                    break;
                case 101:
                    {
                        neco_data.Instance.GetPassData().UpdatePassExp(jObject);
                    }
                    break;
                case 102:
                    {
                        neco_data.Instance.GetPassData().UpdateMissionData(jObject);
                    }
                    break;
            }
        }
        return bSave;
    }

    public bool OnResponseShop(JObject jObject)
    {
        bool bSave = false;

        JToken opCode = jObject["op"];
        if (opCode != null && opCode.Type == JTokenType.Integer)
        {
            int op = opCode.Value<int>();
            switch (op)
            {
                case 1:
                    {
                        if (jObject.ContainsKey("stock"))
                        {
                            if (jObject.ContainsKey("prod"))
                            {
                                uint prodid = jObject["prod"].Value<uint>();
                                uint stock = jObject["stock"].Value<uint>();


                                neco_market_data data = neco_data.Instance.GetMarketData();
                                if (data.saleFish.ContainsKey(prodid))
                                {
                                    data.saleFish[prodid] = stock;
                                }

                                if (data.saleHardware.ContainsKey(prodid))
                                {
                                    data.saleHardware[prodid] = stock;
                                }
                            }
                        }
                    }
                    break;
                case 2:
                    {
                        neco_market_data data = neco_data.Instance.GetMarketData();
                        data.saleFish.Clear();
                        data.saleHardware.Clear();
                        data.fishRefresh = 0;
                        data.hardwareRefresh = 0;

                        if (jObject.ContainsKey("fish"))
                        {
                            JObject fish = (JObject)jObject["fish"];
                            foreach (JProperty property in fish.Properties())
                            {
                                uint prodid = uint.Parse(property.Name);
                                uint stock = property.Value.Value<uint>();
                                data.saleFish[prodid] = stock;
                            }
                        }

                        if (jObject.ContainsKey("hardware"))
                        {
                            JObject hardware = (JObject)jObject["hardware"];
                            foreach (JProperty property in hardware.Properties())
                            {
                                uint prodid = uint.Parse(property.Name);
                                uint stock = property.Value.Value<uint>();
                                data.saleHardware[prodid] = stock;
                            }
                        }

                        if (jObject.ContainsKey("fish_refresh"))
                        {
                            data.fishRefresh = jObject["fish_refresh"].Value<uint>();
                        }

                        if (jObject.ContainsKey("hardware_refresh"))
                        {
                            data.hardwareRefresh = jObject["hardware_refresh"].Value<uint>();
                        }

                        if (jObject.ContainsKey("fish_ad"))
                        {
                            data.fishADCount = jObject["fish_ad"].Value<uint>() <= 5 ? 5 - jObject["fish_ad"].Value<uint>() : 0;
                        }

                        if (jObject.ContainsKey("hardware_ad"))
                        {
                            data.hardwareADCount = jObject["hardware_ad"].Value<uint>() <= 5 ? 5 - jObject["hardware_ad"].Value<uint>() : 0;
                        }
                    }
                    break;
                case 3:
                    {
                        neco_market_data data = neco_data.Instance.GetMarketData();

                        if (jObject.ContainsKey("fish"))
                        {
                            data.saleFish.Clear();
                            JObject fish = (JObject)jObject["fish"];
                            foreach (JProperty property in fish.Properties())
                            {
                                uint prodid = uint.Parse(property.Name);
                                uint stock = property.Value.Value<uint>();
                                data.saleFish[prodid] = stock;
                            }
                        }

                        if (jObject.ContainsKey("hardware"))
                        {
                            data.saleHardware.Clear();
                            JObject hardware = (JObject)jObject["hardware"];
                            foreach (JProperty property in hardware.Properties())
                            {
                                uint prodid = uint.Parse(property.Name);
                                uint stock = property.Value.Value<uint>();
                                data.saleHardware[prodid] = stock;
                            }
                        }

                        if (jObject.ContainsKey("fish_refresh"))
                        {
                            data.fishRefresh = jObject["fish_refresh"].Value<uint>();
                        }

                        if (jObject.ContainsKey("hardware_refresh"))
                        {
                            data.hardwareRefresh = jObject["hardware_refresh"].Value<uint>();
                        }

                        if (jObject.ContainsKey("fish_ad"))
                        {
                            data.fishADCount = jObject["fish_ad"].Value<uint>() <= 5 ? 5 - jObject["fish_ad"].Value<uint>() : 0;
                        }

                        if (jObject.ContainsKey("hardware_ad"))
                        {
                            data.hardwareADCount = jObject["hardware_ad"].Value<uint>() <= 5 ? 5 - jObject["hardware_ad"].Value<uint>() : 0;
                        }
                    }
                    break;
                case 4:
                    {

                    }
                    break;
                case 101:
                    {

                    }
                    break;
                case 102:
                    {

                    }
                    break;
            }
        }
        return bSave;
    }

    public bool OnResponsePost(JObject jObject)
    {
        bool bSave = false;

        JToken opCode = jObject["op"];
        if (opCode != null && opCode.Type == JTokenType.Integer)
        {
            int op = opCode.Value<int>();
            switch (op)
            {
                case 101:
                    {
                        JToken resultTime = jObject["ts"];
                        if (resultTime == null || resultTime.Type != JTokenType.Integer)
                            return false;

                        uint ts = resultTime.Value<uint>();
                        if (ts > 0)
                        {
                            neco_data.Instance.SetNewPostTimestamp(ts);
                            NecoCanvas.GetUICanvas()?.RefreshTopMenuRedDot();
                        }
                    }
                    break;
            }
        }
        return bSave;
    }

    public bool OnResponseIAP(JObject jObject)
    {
        bool bSave = false;

        JToken opCode = jObject["op"];
        if (opCode != null && opCode.Type == JTokenType.Integer)
        {
            int op = opCode.Value<int>();
            switch (op)
            {
                case 6:
                    {
                        neco_data.Instance.ClearSubscribe();
                        if (jObject.TryGetValue("subs", out JToken jtk))
                        {
                            if (jtk.Type == JTokenType.Array)
                            {
                                JArray subs = (JArray)jtk;
                                foreach (JObject s in subs)
                                {
                                    neco_data.Instance.SetSubscribe(s);
                                }
                            }
                        }
                    }
                    break;
                case 101:
                    {
                        JToken prod = jObject["prod"];
                        if (prod == null || prod.Type != JTokenType.Integer)
                            return false;

                        uint id = prod.Value<uint>();
                        neco_data.Instance.UpdateTimeSale(id, jObject["expire"].Value<uint>());

                        switch (id)
                        {
                            case 34:
                            case 36:
                            case 37:
                            case 42:
                            case 43:
                            case 45:
                                break;
                            case 24:
                            default:
                                return bSave;
                        }

                        NecoCanvas.GetPopupCanvas().OnCheckLimitProduct(id);
                    }
                    break;
                case 102:
                    {
                        if (jObject.TryGetValue("subs", out JToken jtk))
                        {
                            if (jtk.Type == JTokenType.Array)
                            {
                                JArray subs = (JArray)jtk;
                                foreach (JObject s in subs)
                                {
                                    neco_data.Instance.UpdateSubscribe(s);
                                }
                            }
                        }
                    }
                    break;
            }
        }

        return bSave;
    }

    public bool OnResponseGacha(JObject jObject)
    {
        bool bSave = false;

        JToken opCode = jObject["op"];
        if (opCode != null && opCode.Type == JTokenType.Integer)
        {
            int op = opCode.Value<int>();
            switch (op)
            {
                case 1:
                    {
                        JToken ad = jObject["ad"];
                        if (ad == null || ad.Type != JTokenType.Integer)
                            return false;

                        uint count = ad.Value<uint>();
                        neco_data.Instance.SetPhotoADCount(count <= 5 ? 5 - count : 0);
                    }
                    break;
                case 2:
                    {
                        JToken ad = jObject["ad"];
                        if (ad == null || ad.Type != JTokenType.Integer)
                            return false;

                        uint count = ad.Value<uint>();
                        neco_data.Instance.SetPhotoADCount(count <= 5 ? 5 - count : 0);
                    }
                    break;
            }
        }

        return bSave;
    }

    public bool OnResponseEvent(JObject jObject)
    {
        bool bSave = false;

        JToken eventID = jObject["eid"];
        if (eventID != null && eventID.Type == JTokenType.Integer)
        {
            int eid = eventID.Value<int>();
            switch (eid)
            {
                case 1:
                    return OnResponseChuseok(jObject);
                case 2:
                    return OnResponseHalloween(jObject);
            }
        }
        

        return bSave;
    }

    public bool OnResponseChuseok(JObject jObject)
    {
        bool bSave = false;

        chuseok_event eventData = null;
        foreach(neco_event evt in neco_data.Instance.GetEvents())
        {
            if ((neco_event.EVENT_TYPE)evt.GetEventID() == neco_event.EVENT_TYPE.CHUSEOK)
                eventData = (chuseok_event)evt;
        }

        if (eventData == null)
            return false;

        JToken opCode = jObject["op"];
        if (opCode != null && opCode.Type == JTokenType.Integer)
        {
            int op = opCode.Value<int>();
            switch (op)
            {
                case 1:
                    {
                        eventData.SetEventData(jObject);
                    }
                    break;
                case 10:
                case 11:
                case 12:
                    {
                        eventData.SetMarbleData(jObject, op);
                    }
                    break;
                case 30:
                    {
                        eventData.SetShopData(jObject);
                    }
                    break;
                case 40:
                    {
                        eventData.SetAttendanceData(jObject);
                    }
                    break;
            }
        }

        return bSave;
    }

    public bool OnResponseHalloween(JObject jObject)
    {
        bool bSave = false;

        halloween_event eventData = null;
        foreach (neco_event evt in neco_data.Instance.GetEvents())
        {
            if ((neco_event.EVENT_TYPE)evt.GetEventID() == neco_event.EVENT_TYPE.HALLOWEEN)
                eventData = (halloween_event)evt;
        }

        if (eventData == null)
            return false;

        JToken opCode = jObject["op"];
        if (opCode != null && opCode.Type == JTokenType.Integer)
        {
            int op = opCode.Value<int>();
            switch (op)
            {
                case 1:
                    {
                        eventData.SetEventData(jObject);
                    }
                    break;                
                case 40:
                    {
                        eventData.SetAttendanceData(jObject);
                    }
                    break;
            }
        }

        return bSave;
    }

    public bool OnResponseData(JObject jObject)
    {
        JToken resultCode = jObject["rs"];
        if (resultCode == null || resultCode.Type != JTokenType.Integer)
            return false;

        Debug.Log(jObject.ToString());

        int rs = resultCode.Value<int>();
        if (rs != 0)
            return false;

        JToken data_array = jObject["data"];
        if (data_array == null || data_array.Type != JTokenType.Array)
            return false;

        JArray array = (JArray)data_array;
        foreach (JObject row in array)
        {
            JObject head = (JObject)row["head"];
            JArray body = (JArray)row["body"];

            string name = head.GetValue("name").ToString();
            JArray keys = (JArray)head.GetValue("col");
            JArray valType = (JArray)head.GetValue("col_type");
            GameDataManager.Instance.SetGameDataArray(name, keys, valType, body);
        }

        return true;
    }

    public bool OnResponseAuth(JObject jObject)
    {
        JToken opCode = jObject["op"];
        JToken resultCode = jObject["rs"];
        if (opCode != null && opCode.Type == JTokenType.Integer)
        {
            int op = opCode.Value<int>();
            int rs = resultCode.Value<int>();
            if (resultCode == null || resultCode.Type != JTokenType.Integer)
                return false;

            if (op == 1 || op == 2)
            {
                if (rs == 4)
                {
                    SendJWTRequest(JWT_Success, JWT_Fail, 2);
                }
                else if (rs == 0)
                {
                    SendJWTRequest(JWT_Success, JWT_Fail, 3);
                }
                return false;
            }

            if (op == 102 && rs == 0)
            {
                users data = new users();
                try
                {
                    JObject userObject = (JObject)jObject["char"];
                    UserNo = long.Parse(userObject["un"].ToString());
                    data.data.Add("user_id", UserNo);
                    data.data.Add("nick", userObject["nick"].ToString());
                    data.data.Add("catnip", userObject["catnip"].Value<uint>());
                    data.data.Add("point", userObject["point"].Value<uint>());
                    data.data.Add("reg_date_time", userObject["reg_date"].Value<uint>());
                    if (userObject.ContainsKey("lvl"))
                        data.data.Add("level", userObject["lvl"].Value<uint>());
                    //data.data.Add("exp", userObject["exp"].Value<uint>());
                    //data.data.Add("last_reward_time", userObject["last_reward"].Value<uint>());
                    //data.data.Add("buff_amount", userObject["buff_amount"].Value<uint>());
                    //data.data.Add("buff_speed", userObject["buff_speed"].Value<uint>());
                    //data.data.Add("buff_temp", userObject["buff_temp"].Value<uint>());
                    //data.data.Add("happiness", userObject["happiness"].Value<uint>());
                    //data.data.Add("fullness", userObject["fullness"].Value<uint>());
                    data.data.Add("contents", userObject["contents"].Value<uint>());
                    data.data.Add("gold", userObject["gold"].Value<uint>());
                    data.data.Add("max_cards", userObject["max_cards"].Value<uint>());

                    if(userObject.ContainsKey("chat_profile"))
                    {
                        JObject chatObject = (JObject)userObject["chat_profile"];
                        data.data.Add("profileId", chatObject["cur"].Value<uint>());
                        if(chatObject["list"].ToString().Length > 0)
                        {
                            string[] profileList = chatObject["list"].ToString().Split(',');

                            List<uint> list = new List<uint>();
                            foreach (JToken token in profileList)
                            {
                                list.Add(token.Value<uint>());
                            }
                            data.data.Add("profileList", list.ToArray());
                        }   
                        else
                        {
                            int[] list = { 0 };
                            data.data.Add("profileList", list);
                        }
                    }                    

                    JArray collection = (JArray)userObject["collection"];
                    if (collection != null)
                    {
                        JArray keys = new JArray();
                        keys.Add("collection_id");
                        JArray valType = new JArray();
                        valType.Add("i");
                        JArray body = new JArray();
                        foreach (JToken clt in collection)
                        {
                            JArray value = new JArray();
                            value.Add(clt.Value<uint>());
                            body.Add(value);
                        }
                        GameDataManager.Instance.SetGameDataArray("user_collection", keys, valType, body);
                    }

                    JArray touch = (JArray)userObject["touch"];
                    if (touch != null)
                    {
                        JArray keys = new JArray();
                        keys.Add("touch_id");
                        keys.Add("last_run_day");
                        keys.Add("today_run_count");
                        keys.Add("get_time");
                        JArray valType = new JArray();
                        valType.Add("i");
                        valType.Add("i");
                        valType.Add("i");
                        valType.Add("i");
                        JArray body = new JArray();
                        foreach (JObject touch_info in touch)
                        {
                            JArray value = new JArray();
                            value.Add(touch_info["id"].Value<uint>());
                            value.Add(touch_info["last_run_day"].Value<uint>());
                            value.Add(touch_info["today_run_count"].Value<uint>());
                            value.Add(touch_info["get_time"].Value<uint>());
                            body.Add(value);
                        }
                        GameDataManager.Instance.SetGameDataArray("user_inter_touch", keys, valType, body);
                    }

                    JArray play = (JArray)userObject["play"];
                    if (play != null)
                    {
                        JArray keys = new JArray();
                        keys.Add("play_id");
                        keys.Add("last_run_day");
                        keys.Add("today_run_count");
                        keys.Add("get_time");
                        JArray valType = new JArray();
                        valType.Add("i");
                        valType.Add("i");
                        valType.Add("i");
                        valType.Add("i");
                        JArray body = new JArray();
                        foreach (JObject play_info in play)
                        {
                            JArray value = new JArray();
                            value.Add(play_info["id"].Value<uint>());
                            value.Add(play_info["last_run_day"].Value<uint>());
                            value.Add(play_info["today_run_count"].Value<uint>());
                            value.Add(play_info["get_time"].Value<uint>());
                            body.Add(value);
                        }
                        GameDataManager.Instance.SetGameDataArray("user_inter_play", keys, valType, body);
                    }

                    JArray card = (JArray)userObject["card"];
                    if (card != null)
                    {
                        JArray keys = new JArray();
                        keys.Add("card_id");
                        keys.Add("card_uid");
                        keys.Add("rect");
                        keys.Add("memo");
                        keys.Add("get_time");
                        JArray valType = new JArray();
                        valType.Add("i");
                        valType.Add("i");
                        valType.Add("s");
                        valType.Add("s");
                        valType.Add("i");
                        JArray body = new JArray();
                        foreach (JObject card_info in card)
                        {
                            JArray value = new JArray();
                            value.Add(card_info["id"].Value<uint>());
                            value.Add(card_info["uid"].Value<uint>());
                            value.Add(card_info["rct"].Value<string>());
                            value.Add(card_info["memo"].Value<string>());
                            value.Add(card_info["get_time"].Value<uint>());
                            body.Add(value);
                        }
                        GameDataManager.Instance.SetGameDataArray("user_card", keys, valType, body);
                    }

                    JToken jtk;
                    if (userObject.TryGetValue("album", out jtk))
                    {
                        if (jtk.Type == JTokenType.Object)
                        {
                            JObject album = (JObject)jtk;
                            JArray keys = new JArray();
                            keys.Add("card_id");
                            keys.Add("flag");
                            JArray valType = new JArray();
                            valType.Add("i");
                            valType.Add("i");
                            JArray body = new JArray();
                            foreach (JProperty property in album.Properties())
                            {
                                JArray value = new JArray();
                                value.Add(uint.Parse(property.Name));
                                value.Add(property.Value.Value<uint>());
                                body.Add(value);
                            }
                            GameDataManager.Instance.SetGameDataArray("album", keys, valType, body);
                        }
                    }

                    JArray item = (JArray)userObject["item"];
                    if (item != null)
                    {
                        JArray keys = new JArray();
                        keys.Add("item_id");
                        keys.Add("get_amount");
                        keys.Add("used_amount");
                        keys.Add("get_time");

                        JArray valType = new JArray();
                        valType.Add("i");
                        valType.Add("i");
                        valType.Add("i");
                        valType.Add("i");

                        JArray body = new JArray();
                        foreach (JObject item_info in item)
                        {
                            uint get = item_info["get_amount"].Value<uint>();
                            uint used = item_info["used_amount"].Value<uint>();

                            JArray value = new JArray();
                            value.Add(item_info["id"].Value<uint>());
                            value.Add(get);
                            value.Add(used);
                            value.Add(item_info["get_time"].Value<uint>());

                            body.Add(value);
                        }
                        GameDataManager.Instance.SetGameDataArray("user_items", keys, valType, body);
                    }

                    JArray food = (JArray)userObject["food"];
                    if (food != null)
                    {
                        JArray keys = new JArray();
                        keys.Add("food_id");
                        keys.Add("food_amount");
                        JArray valType = new JArray();
                        valType.Add("i");
                        valType.Add("i");
                        JArray body = new JArray();
                        foreach (JObject food_info in food)
                        {
                            JArray value = new JArray();
                            value.Add(food_info["id"].Value<uint>());
                            value.Add(food_info["amount"].Value<uint>());

                            body.Add(value);
                        }
                        GameDataManager.Instance.SetGameDataArray("user_food", keys, valType, body);
                    }

                    JObject idle = (JObject)userObject["idle"];
                    if (idle != null)
                    {
                        data.data.Add("is_active", idle["is_active"].Value<uint>());
                        data.data.Add("last_reward_time", idle["last_reward"].Value<uint>());
                        data.data.Add("rate", idle["rate"].Value<double>());
                        data.data.Add("tick_cycle", idle["tick_cycle"].Value<double>());
                        data.data.Add("next_tick", idle["next_tick"].Value<uint>());
                    }

                    JToken unlocked_area = userObject["unlocked_area"];
                    if (unlocked_area != null)
                    {
                        List<uint> unlockedArea = new List<uint>();
                        JArray array = (JArray)unlocked_area;
                        for (int i = 0; i < array.Count; i++)
                        {
                            unlockedArea.Add(array[i].Value<uint>());
                        }

                        data.data["unlocked_area"] = unlockedArea;

                        JToken location_rew = userObject["location_rew"];
                        if (location_rew != null)
                        {
                            Dictionary<uint, List<uint>> locationDic = new Dictionary<uint, List<uint>>();
                            JObject locRew = (JObject)location_rew;
                            for (int i = 0; i < unlockedArea.Count; i++)
                            {
                                List<uint> locationList = new List<uint>();
                                locationDic.Add(unlockedArea[i], locationList);

                                if (locRew.ContainsKey(unlockedArea[i].ToString()))
                                {
                                    JArray arr = (JArray)locRew[unlockedArea[i].ToString()];
                                    for (int j = 0; j < arr.Count; j++)
                                    {
                                        locationList.Add(arr[j].Value<uint>());
                                    }
                                }
                            }

                            data.data["location_rew"] = locationDic;
                        }
                    }

                    //stages
                    {
                        JArray keys = new JArray();
                        keys.Add("stage_id");
                        keys.Add("flag");
                        JArray valType = new JArray();
                        valType.Add("i");
                        valType.Add("si");
                        JArray body = new JArray();
                        if (userObject.TryGetValue("stages", out jtk))
                        {
                            if (jtk.Type == JTokenType.Object)
                            {
                                JObject stage = (JObject)jtk;

                                foreach (JProperty property in stage.Properties())
                                {
                                    JArray value = new JArray();
                                    value.Add(uint.Parse(property.Name));
                                    value.Add(property.Value.Value<int>());
                                    body.Add(value);
                                }
                            }
                        }

                        GameDataManager.Instance.SetGameDataArray("user_stage", keys, valType, body);
                    }

                    //cats
                    {
                        JArray keys = new JArray();
                        keys.Add("id");
                        keys.Add("exp");
                        keys.Add("lvl");
                        keys.Add("full");
                        keys.Add("aff");
                        keys.Add("act");
                        JArray valType = new JArray();
                        valType.Add("i");
                        valType.Add("i");
                        valType.Add("i");
                        valType.Add("i");
                        valType.Add("i");
                        valType.Add("i");
                        JArray body = new JArray();
                        if (userObject.TryGetValue("cats", out jtk))
                        {
                            if (jtk.Type == JTokenType.Array)
                            {
                                JArray cats = (JArray)jtk;
                                foreach (JObject cat in cats)
                                {
                                    JArray value = new JArray();
                                    value.Add(cat["id"].Value<uint>());
                                    value.Add(cat["exp"].Value<uint>());
                                    value.Add(cat["lvl"].Value<uint>());
                                    value.Add(cat["full"].Value<uint>());
                                    value.Add(cat["aff"].Value<uint>());
                                    value.Add(cat["act"].Value<uint>());
                                    body.Add(value);
                                }
                            }
                        }

                        GameDataManager.Instance.SetGameDataArray("user_cats", keys, valType, body);
                    }

                    //neco_user_cat
                    {
                        JArray keys = new JArray();
                        keys.Add("id");
                        keys.Add("obtain");
                        keys.Add("memory");
                        keys.Add("visits");
                        keys.Add("fav_food");
                        keys.Add("fav_toy");
                        keys.Add("rewards");
                        keys.Add("map");
                        keys.Add("state");

                        JArray valType = new JArray();
                        valType.Add("i");
                        valType.Add("i");
                        valType.Add("i");
                        valType.Add("i");
                        valType.Add("i");
                        valType.Add("i");
                        valType.Add("s");
                        valType.Add("i");
                        valType.Add("i");

                        JArray body = new JArray();

                        GameDataManager.Instance.SetGameDataArray("neco_user_cat", keys, valType, body);

                        if (userObject.TryGetValue("neco", out jtk))
                        {
                            if (jtk.Type == JTokenType.Array)
                            {
                                JArray nuc = (JArray)jtk;
                                foreach (JObject neco in nuc)
                                {
                                    neco_user_cat.UpdateUserCatInfo(neco);
                                }
                            }
                        }
                    }

                    //neco_object(spot)
                    //neco_object_map  에 넣어놔야 검색됨
                    {
                        List<game_data> neco_spots = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.NECO_SPOT);
                        if (neco_spots != null)
                        {
                            neco_spots.Clear();
                        }

                        if (userObject.TryGetValue("objects", out jtk))
                        {
                            if (jtk.Type == JTokenType.Array)
                            {
                                JArray spots = (JArray)jtk;
                                foreach (JObject spotData in spots)
                                {
                                    neco_spot spot = neco_spot.AddNecoSpot(spotData["id"].Value<uint>());
                                    spot.UpdateData(spotData);
                                }
                            }
                        }
                    }

                    neco_data.Instance.Reset();
                    if (userObject.TryGetValue("object_tick", out jtk))
                    {
                        neco_data.Instance.SetNextObjectUpdate(userObject["object_tick"].Value<uint>());
                    }
                    if (userObject.TryGetValue("gift_basket", out jtk))
                    {
                        neco_data.Instance.SetGiftBasketLevel(userObject["gift_basket"].Value<uint>());
                    }
                    if (userObject.TryGetValue("gift_boost", out jtk))
                    {
                        neco_data.BOOST_TYPE type = neco_data.BOOST_TYPE.NONE;
                        if (jObject.ContainsKey("gift_btype"))
                        {
                            if (jObject["gift_btype"].Value<uint>() == 1)
                                type = neco_data.BOOST_TYPE.CATNIP_BOOST;
                            else
                                type = neco_data.BOOST_TYPE.AD_BOOST;
                        }

                        neco_data.Instance.SetGiftBoostTime(userObject["gift_boost"].Value<uint>(), type);
                    }

                    if (userObject.TryGetValue("farm_full", out jtk))
                    {
                        neco_data.Instance.SetGoldFullTick(userObject["farm_full"].Value<uint>());
                    }
                    if (userObject.TryGetValue("trap_state", out jtk))
                    {
                        neco_data.Instance.SetFishState(userObject["trap_state"].Value<uint>());
                    }

                    if (userObject.TryGetValue("workbench", out jtk))
                    {
                        neco_data.Instance.SetCraftRecipeLevel(userObject["workbench"].Value<uint>());
                    }
                    if (userObject.TryGetValue("countertop", out jtk))
                    {
                        neco_data.Instance.SetCookRecipeLevel(userObject["countertop"].Value<uint>());
                    }

                    uint passAlarm = 0;
                    if (userObject.TryGetValue("pass_rew", out jtk))
                    {
                        passAlarm = userObject["pass_rew"].Value<uint>();
                    }
                    neco_data.Instance.GetPassData().SetPassAlarm(passAlarm > 0);

                    if (userObject.TryGetValue("mission_rew", out jtk))
                    {
                        passAlarm = userObject["mission_rew"].Value<uint>();
                    }
                    neco_data.Instance.GetPassData().SetDailyAlarm((passAlarm & 1) > 0);
                    neco_data.Instance.GetPassData().SetSeasonAlarm((passAlarm & 2) > 0);

                    if (userObject.TryGetValue("iap", out jtk))
                    {
                        JObject iap = (JObject)jtk;
                        if (iap.ContainsKey("limit"))
                        {
                            neco_data.Instance.SetTimeSale((JObject)iap["limit"]);
                        }
                        neco_data.Instance.SetBenefit(false);
                        if (iap.ContainsKey("first"))
                        {
                            neco_data.Instance.SetBenefit(iap["first"].Value<uint>() > 0);
                        }

                        neco_data.Instance.ClearSubscribe();
                        if (iap.ContainsKey("subs"))
                        {
                            if (iap.TryGetValue("subs", out jtk))
                            {
                                if (jtk.Type == JTokenType.Array)
                                {
                                    JArray subs = (JArray)jtk;
                                    foreach (JObject s in subs)
                                    {
                                        neco_data.Instance.SetSubscribe(s);
                                    }
                                }
                            }
                        }
                    }

                    if (userObject.TryGetValue("events", out jtk))
                    {
                        JArray events = (JArray)jtk;
                        neco_data.Instance.SetEventData(events);
                    }

                    //plants
                    {
                        if (userObject.TryGetValue("plants", out jtk))
                        {
                            if (jtk.Type == JTokenType.Array)
                            {
                                JArray plants = (JArray)jtk;
                                foreach (JObject plant in plants)
                                {
                                    neco_data.Instance.UpdatePlant(plant);
                                }
                            }
                        }
                    }

                    uint ready_cat = 0;
                    if (userObject.ContainsKey("ready_cat"))
                    {
                        ready_cat = userObject["ready_cat"].Value<uint>();
                    }
                    neco_data.Instance.InitReadyCat(ready_cat);
                    //neco_data.Instance.FishingData.Baits = userObject["baits"].Value<uint>();
                }
                catch (Exception e)
                {
                    Debug.LogError("!!!!!!!!" + e.Message);
                    Debug.LogError(e.InnerException);
                    Debug.LogError(e.StackTrace);
                    Debug.LogError(e.Data);
                }


                //only use client side 
                object obj;
                if (GameDataManager.Instance.GetUserData() != null && GameDataManager.Instance.GetUserData().data.TryGetValue("background_cardid", out obj))
                {
                    try
                    {
                        data.data.Add("background_cardid", (uint)obj);
                    }
                    catch
                    {
                        data.data["background_cardid"] = 0;
                    }
                }

                GameDataManager.Instance.SetUserData(data);
                return true;
            }

            if (op == 3 || op == 4)
            {
                if (rs != 0)
                {
                    GameObject canvas = GameObject.Find("Canvas");
                    if (canvas)
                    {
                        intro_manager mgr = canvas.GetComponent<intro_manager>();
                        if (mgr)
                        {
                            mgr.OnAccountError(rs);
                        }
                    }
                }
            }

            if (op == 101 && rs == 0)
            {
                SessionToken = jObject["tk"].ToString();

                JWT_Success?.Invoke();
                JWT_Success = null;
                JWT_Fail = null;
                return false;
            }
        }

        return false;
    }

    public static void SendJWTRequest(Callback success, Callback fail, int opcode)
    {
        NetworkManager instance = GetInstance();
        instance.JWT_Success = success;
        instance.JWT_Fail = fail;

        string json_web_token = PlayerPrefs.GetString("json_web_token");
        if (string.IsNullOrEmpty(json_web_token))
        {
            instance.JWT_Fail();
            instance.JWT_Success = null;
            instance.JWT_Fail = null;
            return;
        }

        WWWForm data = new WWWForm();
        data.AddField("jwt", json_web_token);
        data.AddField("op", opcode);
        data.AddField("ver", GameDataManager.Instance.GetVersion());

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

        data.AddField("lang", (int)targetLanguage);
        data.AddField("ver", GameDataManager.Instance.GetVersion());

        instance.SendPost(NetworkManager.GAMESERVER_URL + "auth",
            data,
            (string body) =>
            {
                Debug.Log(body.ToString());
                // if isError
                try
                {
                    ResponseRoot objRoot = JsonUtility.FromJson<ResponseRoot>(body);
                    if (null == objRoot || (int)eResponseCode.OK != objRoot.rs)
                    {
                        GenericApiFail(objRoot.rs);
                        return;
                    }
                    instance.ServerTime = objRoot.ts;
                }
                catch (System.Exception e)
                {
                    instance.JWT_Fail?.Invoke();
                    return;
                }

                HandleApiList(body);
            },
            () =>
            {
                instance.JWT_Fail?.Invoke();
            }, false
        );
    }

    private void TimeSyncCheck(int time)
    {
        if (ServerTime >= time + 10)
        {
            Debug.LogError("Weird Time data!");
        }

        ServerTime = time;
    }
}
