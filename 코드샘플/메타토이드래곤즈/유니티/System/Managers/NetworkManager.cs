using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;
using UnityEngine.Networking;
using System;

namespace SandboxNetwork
{
    public class ServerInfo
    {
        enum Flag
        {
            NONE = 0,
            NEW = 1,
            EVENT = 2,
        }

        public int TAG { get; private set; }
        public string NAME {
            get
            {
                return StringData.GetStringByStrKey("server_name:" + TAG)
#if SB_TEST
                    + (NetworkManager.IsLiveServer ? "" : "-dev")
#endif
                    ;
            }
        }
        public string DESC { get { return StringData.GetStringByStrKey("server_desc:" + TAG); } }

        public bool NewFlag { get; private set; } = false;
        public bool EventFlag { get; private set; } = false;
        public ServerInfo(JObject info)
        {
            if (info.ContainsKey("tag"))
                TAG = info["tag"].Value<int>();
        }

        public ServerInfo(int tag, int flag)
        {
            TAG = tag;
            NewFlag = (flag & (int)Flag.NEW) > 0;
            EventFlag = (flag & (int)Flag.EVENT) > 0;
        }
    }
    public class NetworkManager : IManagerBase
    {
        
        public Dictionary<int, ServerInfo> ServerInfo { get; private set; } = new Dictionary<int, ServerInfo>();
        public bool SetServerInfo(JArray server_info, JArray server_flag)
        {
            ServerInfo.Clear();

            for (int i = 0; i < server_info.Count; i++)
            {
                JObject jobject = (JObject)server_info[i];
                if (jobject != null)
                {
                    //var info = new ServerInfo(jobject);
                    //if (info != null)
                    //{
                    //    ServerInfo.Add(info.TAG, info);
                    //}
                }
            }

            if (ServerInfo.Count <= 0)
                return false;

            if (!ServerInfo.ContainsKey(SBGameManager.CurServerTag))
            {
                SBGameManager.CurServerTag = 0;
            }    

            return true;
        }

        public void SetServerInfo(int tag, ServerInfo info)
        {
            if(!ServerInfo.ContainsKey(tag))
            {
                ServerInfo.Add(tag, info);
            }
            else
            {
                ServerInfo[tag] = info;
            }
        }

        public void SetServerInfo()
        {

        }

        
        const string DEV_CDN = "https://d1zh71njdecog6.cloudfront.net/";
        const string DEV_WEB_SERVER = "https://saga-api.meta-toy.world/dev/api/";
        public const string DEV_CHAT_SERVER = "MTDZ-SAGA-DEV-BACK-SOCK-922da8c7cc79122c.elb.ap-northeast-2.amazonaws.com";
        public const int DEV_SERVER_PORT = 3000;

        const string QA_CDN = "https://d1zh71njdecog6.cloudfront.net/";
        const string QA_WEB_SERVER = "https://saga-api.meta-toy.world/qa/api/";
        public const string QA_CHAT_SERVER = "MTDZ-SAGA-QA-BACK-SOCK-01870d75de30dc7f.elb.ap-northeast-2.amazonaws.com";
        public const int QA_SERVER_PORT = 3000;


        const string LIVE_CDN = "https://d2efgqatv3752r.cloudfront.net/";
        const string LIVE_WEB_SERVER = "https://saga-api.meta-toy.world/main/api/";
        public const string LIVE_CHAT_SERVER = "MTDZ-SAGA-MAIN-BACK-SOCK-fa2cd8f40e5c306e.elb.ap-northeast-2.amazonaws.com";
        public const int LIVE_SERVER_PORT = 3000;


        private static bool _isLiveServer =
#if SB_TEST
                    PlayerPrefs.GetInt("IS_LIVE_SERVER", 0) > 0;
#else
                    //PlayerPrefs.GetInt("IS_LIVE_SERVER", 1) > 0;
                    true;
#endif


#if SB_TEST
        public static bool IsQAServer { get { return false; } set { } }//{ get { return PlayerPrefs.GetInt("IS_QA_SERVER", 0) > 0; } set { PlayerPrefs.SetInt("IS_QA_SERVER", value ? 1: 0); } }
#endif
        public static bool IsLiveServer
        {
            get
            {
#if SB_TEST
                return _isLiveServer;
#else
                return true;
#endif
            }
 
            set
            {
                PlayerPrefs.SetInt("IS_LIVE_SERVER", value ? 1 : 0);
                _isLiveServer = value;
            }
        }

        static public string WEB_SERVER
        {
            get
            {
                if (!IsLiveServer)
                {
#if SB_TEST
                    if (IsQAServer)
                        return QA_WEB_SERVER;
#endif
                    return DEV_WEB_SERVER;
                }

                if (Instance == null)
                    return "";

                return LIVE_WEB_SERVER;
            }
        }

        static public string CHAT_SERVER
        {
            get
            {
                if (!IsLiveServer)
                {
#if SB_TEST
                    if (IsQAServer)
                        return QA_CHAT_SERVER;
#endif
                    return DEV_CHAT_SERVER;
                }

                if (Instance == null)
                    return "";

                return LIVE_CHAT_SERVER;
            }
        }

        static public int SERVER_PORT
        {
            get
            {
                if (!IsLiveServer)
                {
#if SB_TEST
                    if (IsQAServer)
                        return QA_SERVER_PORT;
#endif
                    return DEV_SERVER_PORT;
                }

                return LIVE_SERVER_PORT;
            }
        }

        public string cdn_url = "";
        static public string CDN
        {
            get
            {
                if (string.IsNullOrEmpty(Instance.cdn_url))
                {
                    if (!IsLiveServer)
                    {
#if SB_TEST
                        if (IsQAServer)
                            return QA_CDN;
#endif
                        return DEV_CDN;
                    }

                    return LIVE_CDN;
                }

                return Instance.cdn_url;
            }
            set
            {
                Instance.cdn_url = value;
            }
        }

        public static int ServerTag
        {
            get
            {
                return SBGameManager.CurServerTag;
            }
        }

        public static string ServerName
        {
            get
            {
                string ret = "";
                if (IsLiveServer)
                {
                    if (Instance.ServerInfo.ContainsKey(ServerTag))
                    {
                        return Instance.ServerInfo[ServerTag].NAME;
                    }

                    return "Unknown";
                }
                else
                {
                    ret = "Dev";
                }

                if (User.Instance.ENABLE_P2E)
                    ret += ".Global";

                return ret;
            }
        }

#if UNITY_EDITOR
        public string BRANCH_NAME { get { return GameConfigTable.Instance.BRANCH_NAME; } }
#endif
        public delegate void SuccessCallback(JObject body);
        public delegate void FailCallback(string body);
        public struct NetworkQueue : IEquatable<NetworkQueue>
        {
            public string url;
            public WWWForm data;
            public SuccessCallback onSuccess;
            public FailCallback onFail;
            public bool useIndecator;
            public NetworkQueue(string url, WWWForm data, SuccessCallback onSuccess, FailCallback onFail, bool indecator = true)
            {
                this.url = url;
                this.data = data;
                this.onSuccess = onSuccess;
                this.onFail = onFail;
                this.useIndecator = indecator;
            }

            public bool Equals(NetworkQueue other)//GenericType Object Contain비교 용도.
            {
                if (url != other.url)
                    return false;

                if (data.data.Length != other.data.data.Length)
                    return false;

                for (int i = 0, count = data.data.Length; i < count; ++i)
                {
                    if (data.data[i] != other.data.data[i])
                        return false;
                }

                return true;
            }
        }

        private static NetworkManager instance = null;
        public static NetworkManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new NetworkManager();
                }

                return instance;
            }
        }

        private int platform = 0;
        public string SessionID { get { return User.Instance.UserAccountData.SessionToken; } }
        public string UserNo { get { return User.Instance.UserAccountData.UserNumber.ToString(); } }
        public string NickName { get { return User.Instance.UserData.UserNick; } }
        public string Language { get { return SBGameManager.Instance.ConvertStringLangBySystemLang(); } }
        public int Platform
        {
            get
            {
                if (platform == 0)
                {
                    switch (Application.platform)
                    {
                        case RuntimePlatform.Android:
                            platform = 1;
                            break;
                        case RuntimePlatform.IPhonePlayer:
                            platform = 2;
                            break;
                        case RuntimePlatform.WindowsPlayer:
                        case RuntimePlatform.WindowsEditor:
                            platform = 9;
                            break;
                    }
                }

                return platform;
            }
        }
        public string Version { get { return GamePreference.Instance.VERSION; } }
        public int UserState { get { return User.Instance.UserData.State; } }
        public WebIndecator Indecator
        {
            get
            {
                if (UICanvas.Instance == null)
                    return null;

                return UICanvas.Instance.Indecator;
            }
        }

        public GameObject Dim
        {
            get
            {
                if (UICanvas.Instance == null)
                    return null;

                return UICanvas.Instance.DIM;
            }
        }

        public bool IsMobile { get { return platform != 9; } }

        public bool IsWait { get { return NetworkCoroutine != null; } }
        private Queue<NetworkQueue> queue = new Queue<NetworkQueue>();
        private Coroutine NetworkCoroutine = null;

        public void Initialize()
        {
            if (queue == null)
                queue = new();
            else
                queue.Clear();
        }

        public static void SendWithCAPTCHA(string url, WWWForm data, SuccessCallback onSuccess, FailCallback onFail = null, bool useIndecator = true)
        {
            if (CAPTCHAPopup.NeedCheck())
            {
                CAPTCHAPopup.OpenPopup(() =>
                {
                    Send(url, data, onSuccess, onFail = null, useIndecator = true);
                });
            }
            else
            {
                Send(url, data, onSuccess, onFail = null, useIndecator = true);
            }
        }

        public static void Send(string url, WWWForm data, SuccessCallback onSuccess, FailCallback onFail = null, bool useIndecator = true)
        {
            if (instance == null)
            {
                return;
            }
            if (data == null)
            {
                data = new WWWForm();
            }

            var req = new NetworkQueue(url, data, onSuccess, onFail, useIndecator);
            if (!Instance.queue.Contains(req))
                Instance.queue.Enqueue(req);
        }

        public static void Retry(NetworkQueue network)
        {
            if (Instance.NetworkCoroutine != null)
                Instance.Indecator.StopCoroutine(Instance.NetworkCoroutine);

            Instance.NetworkCoroutine = null;
            Instance.SendHttp(network);
        }

        private void SendHttp(NetworkQueue network)
        {
            if (Indecator == null)//login 스텝에서는 아직 uiCanvas 생성전 
            {
                if (Game.Instance != null)
                {
                    Game.Instance.StartCoroutine(SendCorutine(network));
                }
                else if (AccountLoader.Instance != null)
                {
                    AccountLoader.Instance.StartCoroutine(SendCorutine(network));
                }
                return;
            }

            NetworkCoroutine = Indecator.SendCorutine(network, SendCorutine(network));
        }

        public IEnumerator SendCorutine(NetworkQueue network)
        {
            Indecator?.OnIndecator(network);

            float time = 3.0f;
            while (time > 0 && Application.internetReachability == NetworkReachability.NotReachable)
            {
                time -= Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }

            var url = network.url;
            var data = network.data;


            if (User.Instance.UserAccountData.UserNumber > 0)
                data.AddField("uno", UserNo); //유저번호

            if (false == string.IsNullOrEmpty(NickName))
                data.AddField("nick", NickName);

            data.AddField("server_tag", ServerTag);
            data.AddField("sid", SessionID); //세션 아이디
            data.AddField("lang", Language); //언어 타입
            data.AddField("os", Platform);
            if (!string.IsNullOrEmpty(Version))
                data.AddField("version", Version);
            data.AddField("ustate", UserState);
            data.AddField("ts", TimeManager.Instance.client_ts);

#if UNITY_EDITOR
            if (!string.IsNullOrEmpty(BRANCH_NAME))
                data.AddField("branch", BRANCH_NAME);
#endif//UNITY_EDITOR

#if ONESTORE
            data.AddField("store_code", OneStoreManager.Instance.GetStoreCode());
#endif
            var onSuccess = network.onSuccess;
            var onFail = network.onFail;

            using (UnityWebRequest req = UnityWebRequest.Post(WEB_SERVER + url, data))
            {
                req.timeout = 10;

#if UNITY_EDITOR
                System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
                stopwatch.Start();
#endif
                yield return req.SendWebRequest();

#if UNITY_EDITOR
                Debug.LogWarning("network   " + url + "  elapsed : " + stopwatch.ElapsedMilliseconds * 0.001f + "sec,  packet size :   " + System.Text.Encoding.Default.GetByteCount(req.downloadHandler.text) + "bytes.");
#endif
                //서버 진행 걸어야함. 로딩창도 끄고
                switch (req.result)
                {
                    case UnityWebRequest.Result.Success:
                        try
                        {
                            JObject root = null;
                            if (!string.IsNullOrEmpty(req.downloadHandler.text))
                                root = JObject.Parse(req.downloadHandler.text);
                            if (root == null)
                            {
                                Debug.LogError(req.result.ToString() + url + req.downloadHandler.text);
                                break;
                            }
                            else
                            {
                                Debug.Log(url + "    " + req.downloadHandler.text);
                                if (root.ContainsKey("rs"))
                                {
                                    int rs = root["rs"].Value<int>();
                                    switch (rs)
                                    {
                                        case 0:
                                            break;
                                        case (int)eApiResCode.NOTHING_TO_HARVEST:
                                            ToastManager.On(100000812);
                                            break;
                                        case (int)eApiResCode.SERVICE_UNAVAILABLE:
                                        {
                                            string title = SBFunc.Replace(root["title"].Value<string>());
                                            string msg = SBFunc.Replace(root["msg"].Value<string>());
                                            string btn = SBFunc.Replace(root["btn"].Value<string>());
                                            string redirect = root["url"].Value<string>();

                                            int popup_type = 0;
                                            if (root.ContainsKey("popup_type"))
                                                popup_type = root["popup_type"].Value<int>();

                                            if (root.ContainsKey("btn2"))
                                            {
                                                string btn2 = SBFunc.Replace(root["btn2"].Value<string>());
                                                string redirect2 = root["url2"].Value<string>();

                                                SystemPopup.OnSystemPopup(popup_type, title, msg, btn, btn2, () =>
                                                {
                                                    if (!string.IsNullOrEmpty(redirect))
                                                    {
                                                        Application.OpenURL(redirect);
                                                    }
                                                }, () =>
                                                {
                                                    if (!string.IsNullOrEmpty(redirect2))
                                                    {
                                                        Application.OpenURL(redirect2);
                                                    }
                                                }, SBFunc.Quit);
                                            }
                                            else
                                            {
                                                SystemPopup.OnSystemPopup(popup_type, title, msg, btn, "").SetCallBack(() =>
                                                {
                                                    if (!string.IsNullOrEmpty(redirect))
                                                    {
                                                        Application.OpenURL(redirect);
                                                    }

                                                    SBFunc.Quit();
                                                });
                                            }

                                            NetworkCoroutine = null;
                                            if (network.useIndecator)
                                            {
                                                Indecator?.OffIndecator(network);
                                                Debug.LogError("Indecator Close " + network.url);
                                            }

                                            yield break;
                                        }
                                        case (int)eApiResCode.GUILD_DATA_ERROR:
                                        case (int)eApiResCode.GUILD_DUPLICATE_NAME:
                                        case (int)eApiResCode.GUILD_ALREADY_REQ_JOIN:
                                        case (int)eApiResCode.GUILD_NO_REQ_JOIN:
                                        case (int)eApiResCode.GUILD_REQ_CANCEL_FAIL:
                                        case (int)eApiResCode.GUILD_REQ_DENY_FAIL:
                                        case (int)eApiResCode.GUILD_NO_AUTH:
                                        case (int)eApiResCode.GUILD_UNABLE_JOIN:
                                        case (int)eApiResCode.GUILD_MEMBER_FULL:
                                        case (int)eApiResCode.GUILD_ALREADY_BELONG:
                                        case (int)eApiResCode.GUILD_CANNOT_JOIN_YET:
                                        case (int)eApiResCode.GUILD_NO_GUILD_YOU_CAN_JOIN:
                                        case (int)eApiResCode.GUILD_INVALID_OPEN_CONDITION:
                                        case (int)eApiResCode.GUILD_CHANGE_GUILD_NAME_FAIL:
                                        case (int)eApiResCode.GUILD_CHANGE_EMBLEM_FAIL:
                                        case (int)eApiResCode.GUILD_NO_CHANGE_GRADE:
                                        case (int)eApiResCode.GUILD_NOT_CAN_BE_CHANGE:
                                        case (int)eApiResCode.GUILD_INVALID_CHANGE_MEMBER_TYPE_CONDITION:
                                        case (int)eApiResCode.GUILD_CANNOT_LEAVE:
                                        case (int)eApiResCode.GUILD_CANNOT_EXPEL:
                                        case (int)eApiResCode.GUILD_CANNOT_CLOSE_AGAIN:
                                        case (int)eApiResCode.GUILD_CANNOT_DONATE_YET:
                                        case (int)eApiResCode.GUILD_DONATION_COUNT_IS_FULL:
                                        case (int)eApiResCode.GUILD_CANNOT_ATTENDENCE_YET:
                                            break;

                                        case (int)eApiResCode.TOURNAMENT_BET_LIMIT_OVER:
                                        {
                                            int remain = 0;
                                            if(root.ContainsKey("remain") && root["remain"].Type == JTokenType.Integer)
                                                remain = root["remain"].Value<int>();

                                            SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringFormatByStrKey("챔피언오류:1917", SBFunc.CommaFromNumber(remain)), true, false, false);
                                        }
                                        break;

                                        case (int)eApiResCode.SESSIONID_NOT_MATCH:
                                        {
                                            OnSessionNotMatch();
                                            NetworkCoroutine = null;
                                            if (network.useIndecator)
                                            {
                                                Indecator?.OffIndecator(network);
                                                Debug.LogError("Indecator Close" + network.url);
                                            }
                                            yield break;
                                        }
                                        break;
                                        default:
                                        {
                                            if (onFail == null)
                                            {
                                                //data sync error에 대한 이벤트 통계를 위하여 추가
                                                LoginManager.Instance.SetFirebaseEvent(url + "_error_" + rs.ToString());

                                                string msg = StringData.GetStringByIndex(100000634) + string.Format("\nerrorCode ::{0}", root["rs"]);
                                                if (StringData.IsContainStrKey("errorcode_" + rs.ToString()))
                                                    msg = StringData.GetStringByStrKey("errorcode_" + rs.ToString());

                                                SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), msg, true, false, false);
                                            }
                                            else
                                            {
                                                onFail.Invoke(req.downloadHandler.text);
                                            }
                                        }
                                        break;
                                    }

                                } 
                            }

                            if (SBFunc.IsJTokenType(root["err"], JTokenType.Integer))
                            {
                                switch (root["err"].Value<int>())
                                {
                                    case (int)eApiResCode.OK:
                                    {
                                        TimeRefresh(root);
                                        switch (url)
                                        {
                                            /// 푸시가 없어야하는데 Landmark에서 강제로 뿌리게 되는 경우
                                            /// 서버 처리가 어려우므로 클라에서 스킵
                                            case "auth/init":
                                            case "auth/signin":
                                            case "auth/signup":
                                            case "user/create":
                                            case "user/select":
                                                PushSessionID(root);
                                                break;
                                            default:
                                                PushResponse(root);
                                                break;
                                        }

                                        onSuccess?.Invoke(root);
                                    }
                                    break;
                                    case (int)eApiResCode.SESSIONID_NOT_MATCH:
                                    {
                                        OnSessionNotMatch();
                                    }
                                    break;
                                    default:
                                    {
                                        if (onFail != null)
                                            onFail.Invoke(req.downloadHandler.text);
                                        else
                                        {
                                            OnError(root);
                                        }
                                    }
                                    break;
                                }
                            }
                            else
                            {
                                onSuccess(root);
                            }
                        }
                        catch (Exception exception)
                        {
                            Debug.LogError(url);
                            Debug.LogError(req.result.ToString());
                            Debug.LogError(req.downloadHandler.text);
                            Debug.LogError("errrorrorroror!!!!!! : " + exception.Message);
                        }
                        break;
                    default:
                        Debug.LogWarning(req.error);
                        Debug.LogError(req.result.ToString() + " " + url + " " + req.downloadHandler.text);

                        if (onFail == null)
                            Indecator?.OnNetworkError(network);
                        else
                            onFail.Invoke(req.downloadHandler.text);
                        break;
                }
            }

            yield return SBDefine.GetWaitForEndOfFrame();

            NetworkCoroutine = null;

            Indecator?.OffIndecator(network);
        }

        public void OnError(JObject root)
        {
            int err = root["err"].Value<int>();

            string msg = StringData.GetStringByIndex(100000634) + string.Format("\nerrorCode ::{0}", err);
            if (StringData.IsContainStrKey("errorcode_" + err.ToString()))
                msg = StringData.GetStringByStrKey("errorcode_" + err.ToString());

            SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), msg, true, false, false);

            eApiResCode res = (eApiResCode)err;
            Debug.Log("error => " + res.ErrorString());
        }

        void OnSessionNotMatch()
        {
            SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100002674),
                                            () =>
                                            {
                                                UIManager.Instance.InitUI(eUIType.None);
                                                LoadingManager.Instance.LoadStartScene();
                                                PopupManager.AllClosePopup();
                                            },
                                            null,
                                            () =>
                                            {
                                                UIManager.Instance.InitUI(eUIType.None);
                                                LoadingManager.Instance.LoadStartScene();
                                                PopupManager.AllClosePopup();
                                            },
                                        true, false, false);
        }
        public void Update(float dt)
        {
            if (NetworkCoroutine != null)
                return;

            if (queue == null || queue.Count < 1)
                return;

            SendHttp(queue.Dequeue()); //발송 큐 걸기
        }

        public void TimeRefresh(JObject root)
        {
            if (root == null)
                return;

            if (!SBFunc.IsJTokenType(root["ts"], JTokenType.Integer))
                return;

            TimeManager.TimeRefresh(root["ts"].Value<int>());
        }

        public void PushSessionID(JObject jsonData)
        {
            if (jsonData == null)
            {
                return;
            }
            if (SBFunc.IsJArray(jsonData["push"]))
            {
                JArray pushArray = (JArray)jsonData["push"];
                if (pushArray == null)
                    return;

                var arrayCount = pushArray.Count;
                for (var i = 0; i < arrayCount; ++i)
                {
                    JObject jObject = (JObject)pushArray[i];

                    if (!SBFunc.IsJTokenCheck(jObject["api"]))
                        continue;

                    switch (jObject["api"].Value<string>())
                    {
                        case "session_id":
                        {
                            User.Instance.UserAccountData.Set(jObject);
                            return;
                        }
                        default: continue;
                    }
                }
            }
        }
        public void PushResponse(JObject jsonData)
        {
            if (jsonData == null)
            {
                return;
            }
            if (SBFunc.IsJArray(jsonData["push"]))
            {
                JArray pushArray = (JArray)jsonData["push"];
                if (pushArray == null)
                    return;

                var arrayCount = pushArray.Count;
                for (var i = 0; i < arrayCount; ++i)
                {
                    JObject jObject = (JObject)pushArray[i];

                    if (!SBFunc.IsJTokenCheck(jObject["api"]))
                        continue;

                    switch (jObject["api"].Value<string>())
                    {
                        case "session_id":
                        {
                            User.Instance.UserAccountData.Set(jObject);
                        }
                        break;
                        case "exp_update":
                        case "energy_update":
                        case "gemstone_update":
                        case "gold_update":
                        case "guild_point_update":
                        {
                            User.Instance.UserData.Set(jObject);
                            switch (jObject["api"].Value<string>())
                            {
                                case "exp_update":
                                    UserStatusEvent.RefreshExp(jObject["exp"].Value<int>(), jObject["level"].Value<int>());
                                    break;
                                case "energy_update":
                                    UserStatusEvent.RefreshEnergy(jObject["energy"].Value<int>(), jObject["energy_tick"].Value<int>());
                                    break;
                                case "gemstone_update":
                                    UserStatusEvent.RefreshGemStone(jObject["gemstone"].Value<int>());
                                    break;
                                case "guild_point_update":
                                    UserStatusEvent.RefreshGuildPoint();
                                    break;
                                case "gold_update":
                                    UserStatusEvent.RefreshGold(jObject["gold"].Value<int>());


                                    // 레드닷 갱신
                                    UIManager.Instance.MainPopupUI.RequestUpdateTownReddot();
                                    UIManager.Instance.MainPopupUI.RequestUpdateConstructionReddot();
                                    break;
                            }
                        }
                        break;
                        case "guild_exp_update":
                        {
                            if (jObject.ContainsKey("guild_exp"))
                            {
                                GuildManager.Instance.UpdateExp(jObject["guild_exp"].Value<int>());
                            }
                        }
                        break;
                        case "portrait_update":
                        {
                            if (jObject.ContainsKey("data"))
                                User.Instance.UserData.UpdatePortraitInfo(jObject["data"]);

                        }
                        break;

                        case "item_update":
                        {
                            if (SBFunc.IsJArray(jObject["data"]))
                            {
                                var array = (JArray)jObject["data"];
                                if (array == null)
                                    continue;

                                bool isGet = false;
                                bool isUse = false;
                                var count = array.Count;
                                for (var k = 0; k < count; k++)
                                {
                                    var itemData = (JArray)array[k];

                                    var itemNo = itemData[0].Value<int>();
                                    var itemCount = itemData[1].Value<int>();

                                    if (!isGet)
                                        isGet = User.Instance.GetItemCount(itemNo) < itemCount;
                                    if (!isUse)
                                        isUse = User.Instance.GetItemCount(itemNo) > itemCount;

                                    User.Instance.UpdateItem(itemNo, itemCount);
                                }

                                ItemFrameEvent.ItemUpdate();
                                if (isGet)
                                {
                                    UIObjectEvent.Event(UIObjectEvent.eEvent.ITEM_GET, UIObjectEvent.eUITarget.LB);

                                    // 레드닷 갱신
                                    UIManager.Instance.MainPopupUI.RequestUpdateTownReddot();
                                    UIManager.Instance.MainPopupUI.RequestUpdateConstructionReddot();
                                }

                                if (isUse)
                                    UIObjectEvent.Event(UIObjectEvent.eEvent.ITEM_USE);
                            }
                        }
                        break;
                        case "building_update":
                        {
                            if (SBFunc.IsJTokenCheck(jObject["tag"]) && SBFunc.IsJTokenCheck(jObject["state"]) && SBFunc.IsJTokenCheck(jObject["level"]))
                            {
                                int time = 0;
                                if (SBFunc.IsJTokenCheck(jObject["construct_exp"]))
                                {
                                    time = jObject["construct_exp"].Value<int>();
                                }
                                User.Instance.UpdateBuilding(jObject["tag"].Value<int>(), jObject["state"].Value<int>(), jObject["level"].Value<int>(), time);
                            }

                            //var map = SBGameManager.GetManager<MapManager>();
                            //if (map != null)
                            //{
                            //mapmanger 업데이트 부분 누락
                            //map.UpdateBuilding(jObject);
                            //}
                        }
                        break;
                        case "exterior_update":
                        {
                            User.Instance.UpdateGrid(jObject);
                            User.Instance.RefreshDataToInfo();
                            //var map = SBGameManager.GetManager<MapManager>();
                            //if (map != null)
                            //{
                            //mapmanger 업데이트 부분 누락
                            //map.UpdateFloor();
                            //}
                        }
                        break;
                        case "produce_update":
                        {
                            User.Instance.UpdateProduces(jObject);
                        }
                        break;
                        case "landmark_update":
                        {
                            User.Instance.PushUpdateLandmark(jObject);

                        }
                        break;
                        case "dragon_update":
                        {
                            Debug.Log("dragon update!");
                            if (SBFunc.IsJArray(jObject["data"]))
                            {
                                var array = (JArray)jObject["data"];
                                if (array == null)
                                    continue;

                                var count = array.Count;
                                for (var k = 0; k < count; k++)
                                {
                                    var dragonData = JObject.FromObject(array[k]);
                                    var dragonTag = dragonData["dragon_id"].Value<int>();

                                    var getdragonTempData = User.Instance.DragonData.GetDragon(dragonTag);
                                    UserDragon userdragonTempData;
                                    if (getdragonTempData != null)
                                        userdragonTempData = getdragonTempData;
                                    else
                                        userdragonTempData = new();

                                    userdragonTempData.SetJsonData(dragonTag, dragonData);
                                    User.Instance.DragonData.AddUserDragon(dragonTag, userdragonTempData);
                                }
                            }
                        }
                        break;
                        case "dragon_exp_update":
                        {
                            if (SBFunc.IsJArray(jObject["data"]))
                            {
                                var array = (JArray)jObject["data"];
                                if (array == null)
                                    continue;

                                var count = array.Count;
                                for (var k = 0; k < count; k++)
                                {
                                    var dragonData = JObject.FromObject(array[k]);
                                    var dragonTag = dragonData["id"].Value<int>();
                                    var dragonEXP = dragonData["exp"].Value<int>();
                                    var dragonLevel = dragonData["lvl"].Value<int>();

                                    var getdragonTempData = User.Instance.DragonData.GetDragon(dragonTag);
                                    var userdragonTempData = new UserDragon();
                                    int prevLevel = getdragonTempData.Level;
                                    userdragonTempData = getdragonTempData;
                                    userdragonTempData.SetLevel(dragonLevel);
                                    userdragonTempData.SetExp(dragonEXP);
                                    if (prevLevel != dragonLevel)
                                        userdragonTempData.RefreshALLStatus();

                                    User.Instance.DragonData.AddUserDragon(dragonTag, userdragonTempData);
                                }
                            }
                        }
                        break;
                        case "part_update":
                        {
                            if (SBFunc.IsJArray(jObject["data"]))
                            {
                                var array = (JArray)jObject["data"];
                                if (array == null)
                                    continue;

                                var count = array.Count;
                                for (var k = 0; k < count; k++)
                                {
                                    var element = JObject.FromObject(array[k]);
                                    if (element != null)
                                    {
                                        User.Instance.PartData.AddUserPart(element);
                                    }
                                    else
                                    {
                                        //업데이트
                                    }
                                }
                                //SBLog(array);
                            }
                        }
                        break;
                        case "quest_update":
                        {
                            if (SBFunc.IsJTokenCheck(jObject["data"]))
                            {
                                QuestManager.Instance.ProgressUpdate(jObject["data"]);
                                QuestEvent.Event(QuestEvent.eEvent.QUEST_UPDATE);
                            }
                        }
                        break;
                        case "tutorial_update":
                        {
                            if (SBFunc.IsJObject(jObject["data"]))
                                TutorialManagement.SetTutorialData((JObject)jObject["data"]);
                        }
                        break;
                        case "adventure_update":
                        {
                            if (SBFunc.IsJTokenCheck(jObject["world"]) && SBFunc.IsJTokenCheck(jObject["diff"]))
                            {
                                StageManager.Instance.AdventureProgressData.SetWorldInfoData(jObject["world"].Value<int>(), jObject["diff"].Value<int>(), jObject);
                            }
                            else if (SBFunc.IsJArray(jObject["info"]))
                            {
                                var array = (JArray)jObject["info"];
                                if (array == null)
                                    continue;

                                var count = array.Count;
                                for (var k = 0; k < count; k++)
                                {
                                    var data = JObject.FromObject(array[k]);
                                    var worldIndex = data["world"].Value<int>();
                                    var worldDiff = data["diff"].Value<int>();
                                    StageManager.Instance.AdventureProgressData.SetWorldInfoData(worldIndex, worldDiff, data);
                                }
                            }
                        }
                        break;
                        case "part_removed":
                        {
                            if (SBFunc.IsJArray(jObject["tags"]))
                            {
                                var array = (JArray)jObject["tags"];
                                if (array == null)
                                    continue;

                                var count = array.Count;
                                for (var k = 0; k < count; k++)
                                {
                                    User.Instance.PartData.DeleteUserPart(array[k].Value<int>());
                                }
                            }
                        }
                        break;
                        case "dragon_card_new":
                        case "dragon_card_del":
                        {
                            User.Instance.DragonCards.Set(jObject);
                        }
                        break;
                        case "pet_update":
                        {
                            if (SBFunc.IsJArray(jObject["data"]))
                            {
                                var array = (JArray)jObject["data"];
                                if (array == null)
                                    continue;

                                var arraysCount = array.Count;
                                for (var k = 0; k < arraysCount; k++)
                                {
                                    if (array[k] == null)
                                        continue;

                                    User.Instance.PetData.RefreshUserPet(array[k]);
                                }
                            }
                        }
                        break;
                        case "pet_removed":
                        {
                            if (SBFunc.IsJArray(jObject["tags"]))
                            {
                                var array = (JArray)jObject["tags"];
                                if (array == null)
                                    continue;

                                var count = array.Count;
                                for (var k = 0; k < count; k++)
                                {
                                    User.Instance.PetData.DeleteUserPet(array[k].Value<int>());
                                }
                            }
                        }
                        break;
                        case "daily_update":
                        {
                            if (SBFunc.IsJTokenType(jObject["world"], JTokenType.Array))
                                StageManager.Instance.DailyDungeonProgressData.SetTodayWorldIndex(jObject["world"]);

                            if (SBFunc.IsJTokenType(jObject["battle_count"], JTokenType.Integer))
                                StageManager.Instance.DailyDungeonProgressData.SetDailyDungeonTicketCount(jObject["battle_count"]);

                            if (SBFunc.IsJTokenType(jObject["gem_use"], JTokenType.Integer))
                                StageManager.Instance.DailyDungeonProgressData.SetDailyDungeonGemRemainCount(jObject["gem_use"]);

                            if (SBFunc.IsJArray(jObject["info"]))
                            {
                                var array = (JArray)jObject["info"];
                                var arrayLength = array.Count;

                                for (var k = 0; k < arrayLength; k++)
                                {
                                    var data = JObject.FromObject(array[k]);
                                    if (data == null)
                                        continue;

                                    StageManager.Instance.DailyDungeonProgressData.SetWorldData(data);
                                }
                            }
                        }
                        break;
                        case "raid_update":
                        {
                            if (jObject.ContainsKey("battle_count"))
                                WorldBossManager.Instance.WorldBossProgressData.SetWorldBossTicketCount(jObject["battle_count"]);



                        }
                        break;
                        case "arena_match_update":
                        {
                            ArenaManager.Instance.pushArenaMatchList((JArray)jObject["data"]);
                        }
                        break;
                        case "arena_ticket_update":
                        {
                            ArenaManager.Instance.UserArenaData.SetUserArenaTicket(jObject);
                            ItemFrameEvent.ItemUpdate();
                        }
                        break;
                        case "attendance_update":
                        {
                            if (SBFunc.IsJArray(jObject["data"]))
                                User.Instance.Attendance.SetData((JArray)jObject["data"]);
                        }
                        break;
                        //case "event_attendance_update":
                        //{
                        //    if (SBFunc.IsJArray(jObject["data"]))
                        //        User.Instance.Attendance.SetData((JArray)jObject["data"]);
                        //}
                        //break;
                        case "mileage_update":
                        {
                            User.Instance.UserData.UpdateMileage(jObject["mileage"].Value<int>());
                            UserStatusEvent.RefreshMileage();
                        }
                        break;
                        case "magnet_update":
                        {
                            User.Instance.UserData.UpdateMagnet(jObject["magnet"].Value<int>());
                            UserStatusEvent.RefreshMagnet();
                        }
                        break;
                        case "magnite_update":
                        {
                            User.Instance.UserData.UpdateMagnite(jObject["magnite"].Value<int>());
                            UserStatusEvent.RefreshMagnite();
                        }
                        break;
                        case "guild_magnet_update":
                        {
                            GuildManager.Instance.MyGuildInfo.UpdateMagnet(jObject["magnet"].Value<long>());
                        }
                        break;
                        case "guild_magnite_update":
                        {
                            GuildManager.Instance.MyGuildInfo.UpdateMagnite(jObject["magnite"].Value<long>());
                        }
                        break;
                        case "champ_oracle_update":
                        {
                            User.Instance.UserData.UpdateOracle(jObject["balance"].Value<int>());
                            UserStatusEvent.RefreshOracle();
                        }
                        break;
                        case "arena_point_update":
                        {
                            User.Instance.UserData.UpdateArenaPoint(jObject["arena_point"].Value<int>());
                            UserStatusEvent.RefreshArenaPoint();
                        }
                        break;
                        case "friend_update"://친구 하트 보내기 & 받기에 대한 업데이트
                        {
                            if (jObject.ContainsKey("send_point"))//보내기 성공한 유저의 넘버가 있음.
                            {
                                if (SBFunc.IsJArray(jObject["send_point"]))
                                {
                                    var jArray = JArray.FromObject(jObject["send_point"]);
                                    var sendUserList = jArray.ToObject<List<long>>();
                                    var friendList = FriendManager.FriendIdList;
                                    for (int j = 0, jCount = sendUserList.Count; j < jCount; ++j)
                                    {
                                        if (false == friendList.TryGetValue(sendUserList[j], out var target))
                                            continue;

                                        target.SetSendPoint(1);
                                        FriendManager.Instance.TodaySendCount++;
                                    }
                                }
                            }
                            if (jObject.ContainsKey("recv_point"))//받기에 성공 (나에게 보내준)유저의 넘버가 있음.
                            {
                                if (SBFunc.IsJArray(jObject["recv_point"]))
                                {
                                    var jArray = JArray.FromObject(jObject["recv_point"]);
                                    var recvUserList = jArray.ToObject<List<long>>();
                                    var friendList = FriendManager.FriendIdList;
                                    for (int j = 0, jCount = recvUserList.Count; j < jCount; ++j)
                                    {
                                        if (false == friendList.TryGetValue(recvUserList[j], out var target))
                                            continue;

                                        target.SetRecvPoint(2);
                                    }
                                }
                            }
                            if (jObject.ContainsKey("new_friends_info"))
                            {
                                if (SBFunc.IsJArray(jObject["new_friends_info"]))
                                {
                                    var jArray = JArray.FromObject(jObject["new_friends_info"]);
                                    for (int j = 0, jCount = jArray.Count; j < jCount; ++j)
                                    {
                                        FriendManager.Instance.AddList(jArray[j]);
                                    }
                                    FriendEvent.SendRefersh(new List<FriendUserData>(FriendManager.FriendIdList.Values));
                                }
                            }
                        }
                        break;
                        case "friendly_point_update"://친구 포인트(하트) 업데이트
                        {
                            User.Instance.UserData.UpdateFriendlyPoint(jObject["friendly_point"].Value<int>());
                            UserStatusEvent.RefreshFriendPoint();
                        }
                        break;
                        case "collection_update":
                        {
                            CollectionAchievementManager.Instance.ProgressUpdate(jObject, eCollectionAchievementType.COLLECTION);
                        }
                        break;
                        case "achievement_update":
                        {
                            CollectionAchievementManager.Instance.ProgressUpdate(jObject, eCollectionAchievementType.ACHIEVEMENT);
                        }
                        break;
                        case "extra_stat_update"://업적 & 콜렉션으로 변경되는 스탯 버프값
                        {
                            User.Instance.UserData.ExtraStatBuff.UpdateUserBuffData(jObject);
                            User.Instance.DragonData.RefreshALLDragonStat();//버프 업데이트 이후, 드래곤스탯 업데이트
                        }
                        break;
                        case "magic_showcase_update"://업적 & 콜렉션으로 변경되는 스탯 버프값
                        {
                            MagicShowcaseManager.Instance.ProgrssUpdate(jObject["data"]);
                        }
                        break;
                        case "arena_shop":
                        case "friendly_point_shop"://아레나 상점과 , 우정포인트 상점 방식이 같아서 케이스만 추가
                        {
                            int index = -1;
                            int count = 0;
                            if (jObject.ContainsKey("key"))
                                index = jObject["key"].Value<int>();
                            if (jObject.ContainsKey("c"))
                                count = jObject["c"].Value<int>();
                            if (index > 0)
                                User.Instance.UpdatePurchased(index, count);
                        }
                        break;
                        case "advertise":
                        {
                            int index = -1;
                            int view = 0;
                            if (jObject.ContainsKey("key"))
                                index = jObject["key"].Value<int>();
                            if (jObject.ContainsKey("c"))
                                view = jObject["c"].Value<int>();
                            if (index > 0)
                                User.Instance.UpdateAdvertisement(index, view);
                        }
                        break;
                        case "iap_purchase":
                        {
                            int index = -1;
                            int count = 0;
                            if (jObject.ContainsKey("key"))
                                index = jObject["key"].Value<int>();
                            if (jObject.ContainsKey("c"))
                                count = jObject["c"].Value<int>();
                            if (index > 0)
                                User.Instance.UpdatePurchased(index, count);
                        }
                        break;
                        case "iap_personal_on":
                        {
                            int index = -1;
                            long stamp = 0;
                            if (jObject.ContainsKey("key"))
                                index = jObject["key"].Value<int>();
                            if (jObject.ContainsKey("c"))
                                stamp = jObject["c"].Value<long>();
                            if (index > 0)
                                User.Instance.UpdatePrivateGoods(index, stamp);
                        }
                        break;
                        case "user_state":
                        {
                            if (jObject.ContainsKey("c"))
                                User.Instance.UserData.UpdateUserState(jObject["c"].Value<int>());
                        }
                        break;
                        case "mail_sended":
                        {
                            ReddotManager.Set(eReddotEvent.POST_MAIL, true);
                        }
                        break;
                        case "mine_item_update"://광산 - 전용 부스터 아이템 업데이트 push
                        {
                            if (SBFunc.IsJArray(jObject["data"]))
                            {
                                var array = (JArray)jObject["data"];
                                if (array == null)
                                    continue;

                                var count = array.Count;
                                for (var k = 0; k < count; k++)
                                {
                                    var itemData = (JArray)array[k];

                                    if (itemData.Count != 3)
                                    {
                                        Debug.LogError(">>>>>mine_item_update >>> item Data format error " + itemData);
                                        continue;
                                    }

                                    var itemNo = itemData[0].Value<int>();
                                    var itemCount = itemData[1].Value<int>();
                                    var itemExpireTime = itemData[2].Value<int>();

                                    MiningManager.Instance.UpdateItem(itemNo, itemCount, itemExpireTime);
                                }
                            }
                        }
                        break;
                        case "captcha":
                        {
                            CAPTCHAPopup.SetCAPTCHA();
                        }
                        break;
                    }
                }
            }
        }

        static public void SetIAPProcessing(bool procesing)
        {
            Instance.Dim.SetActive(procesing);
        }

        static public void SetCDNURL(string url)
        {
            CDN = url;
        } 

        public IEnumerator GetServerInfoCoroutine()
        {
            int retry = 3;
            JArray server_info = null;
            while (server_info == null)
            {
                using (UnityEngine.Networking.UnityWebRequest req = UnityEngine.Networking.UnityWebRequest.Post("https://saga-api.meta-toy.world/main/api/system/serverinfo", new WWWForm()))
                {
                    req.timeout = 3;
                    yield return req.SendWebRequest();

                    if (req.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
                    {
                        JObject root = null;
                        if (!string.IsNullOrEmpty(req.downloadHandler.text))
                            root = JObject.Parse(req.downloadHandler.text);

                        if (root == null)
                        {
                            Debug.LogError("서버 목록 갱신 실패");
                        }
                        else
                        {
                            if (root.ContainsKey("rs"))
                            {
                                if (root["rs"].Value<int>() == (int)eApiResCode.OK)
                                {
                                    if (root.ContainsKey("server_info"))
                                    {
                                        server_info = (JArray)root["server_info"];                                        
                                    }                                    
                                }
                            }
                        }

                        if (server_info == null)
                        {
                            retry--;
                            if (retry > 0)
                                yield return new WaitForSeconds(1.0f);
                            else
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

                                while (true)
                                {
                                    yield return new WaitForSeconds(1.0f);
                                }
                            }
                        }
                    }
                }
            }

            NetworkCoroutine = null;
        }
    }
}
