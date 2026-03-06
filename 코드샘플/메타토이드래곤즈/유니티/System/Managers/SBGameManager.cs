using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json.Linq;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using SQLite4Unity3d;

namespace SandboxNetwork
{
    public class SBGameManager
    {
        //EnterPlayModeOptions에서 static class 는 직접 초기화를 시켜야함.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void InitPlayMode()
        {
            if (instance != null)
            {
                instance = null;
            }
        }

        public static string ClientVersion { get { return "0.0.1"; } }
        private static SBGameManager instance = null;
        public static SBGameManager Instance
        {
            get
            {
                if (instance == null)
                    instance = new SBGameManager();

                return instance;
            }
        }
        private Dictionary<Type, IManagerBase> managers = null;
        private List<IManagerBase> updateManagers = null;
        private Dictionary<string, bool> isLoadeds = null;

        public float TimeScale { get; private set; } = 1f;
        public void SetFixedDeltaTime(bool fixedD)
        {
            IsFixedDelta = fixedD;
        }
        public bool IsFixedDelta { get; private set; } = false;
        public float DTime
        {
            get
            {
                if(IsFixedDelta)
                    return Time.fixedDeltaTime;
                else
                    return Time.deltaTime * TimeScale; 
            }
        }

        public GameObject GameObject
        {
            get { return Game.Instance.gameObject; }
        }

        private GamePreference gamePrefData = null;
        public GamePreference GamePrefData
        {
            get
            {
                if (gamePrefData == null)
                    gamePrefData = new GamePreference();
                return gamePrefData;
            }
        }

        #region 로그인 관련
        public string UserAccessToken
        {
            get { return PlayerPrefs.GetString("user_token", ""); /*Debug.Log("CHECK UserAccessToken GET --> " + PlayerPrefs.GetString("user_access_token", ""));*/ }
            set { PlayerPrefs.SetString("user_token", value); /*Debug.Log("CHECK UserAccessToken SET --> " + value);*/ }
        }

        public string UserNickname
        {
            get { return PlayerPrefs.GetString(GetPrefStringByServer("user_nick"), string.Empty); }
            set { PlayerPrefs.SetString(GetPrefStringByServer("user_nick"), value); }
        }

        public string PrevGuestAccessToken
        {
            get { return PlayerPrefs.GetString(GetPrefStringByServer("user_guest_token"), string.Empty); }
            set { PlayerPrefs.SetString(GetPrefStringByServer("user_guest_token"), value); }
        }

        public static int CurServerTag
        {
            get { return PlayerPrefs.GetInt("user_server_tag", 1); }
            set { PlayerPrefs.SetInt("user_server_tag", value); }
        }

        public static string GetPrefStringByServer(string key)
        {
            switch(CurServerTag)
            {
                case 1: // live 서버는 원상태 그대로 유지
                    return key;

                default:
                    return key + "_" + CurServerTag;
            }
        }
        public bool IsForceLogout { get; set; } = false;    // 강제 로그아웃인지 체크용 (로그인 메뉴를 보여줄지 여부 등에 사용)
#endregion
        public void Init()
        {
            if (gamePrefData == null)
                gamePrefData = new GamePreference();

            if (managers == null)
                managers = new Dictionary<Type, IManagerBase>();
            else
                managers.Clear();

            if (updateManagers == null)
                updateManagers = new List<IManagerBase>();
            else
                updateManagers.Clear();

            if (isLoadeds == null)
                isLoadeds = new Dictionary<string, bool>();
            else
                isLoadeds.Clear();

#region Manager 세팅
            instance.AddManager(typeof(NetworkManager), NetworkManager.Instance, true);
            instance.AddManager(typeof(TimeManager), TimeManager.Instance, true);
            instance.AddManager(typeof(TableManager), TableManager.Instance, false);
            instance.AddManager(typeof(ResourceManager), ResourceManager.Instance, false);
            instance.AddManager(typeof(PopupManager), PopupManager.Instance, false);
            instance.AddManager(typeof(UIManager), UIManager.Instance, false);
            instance.AddManager(typeof(QuestManager), QuestManager.Instance, false);            
            instance.AddManager(typeof(CollectionAchievementManager), CollectionAchievementManager.Instance, false);
            instance.AddManager(typeof(MiningManager), MiningManager.Instance, false);
            instance.AddManager(typeof(MagicShowcaseManager), MagicShowcaseManager.Instance, false);
            instance.AddManager(typeof(GuildManager), GuildManager.Instance, false);
#endregion
#if ONESTORE
			OneStoreManager.InitializeLicense();
#endif
            SBGameData.GetCompactLocalDatas(true);
        }

        public void BackToLoginScene(bool logout = false)
        {
            TutorialManager.tutorialManagement.EndTutorialEvent(logout);

            SceneManager.sceneLoaded += BackToLgoinSceneCallback;

            SceneManager.LoadScene("Start");

            IsForceLogout = logout;
        }
        private void BackToLgoinSceneCallback(Scene scene, LoadSceneMode loadSceneMode)
        {
            SceneManager.sceneLoaded -= BackToLgoinSceneCallback;

            UIObjectEvent.Event(UIObjectEvent.eEvent.EVENT_HIDE, UIObjectEvent.eUITarget.ALL);

            User.Instance.ClearUserData(true);

            ReloadManagerData();
        }
        // 모든 데이터 다시 로드
        public void ReloadManagerData()
        {
            Init();
        }
        /// <summary> Hash를 체크하여 갱신할 테이블을 갱신 </summary>
        /// <param name="state">게이지 표시 용도</param>
        public IEnumerator GameDataSyncAndLoad(DownloadState state)
        {
            var skipTable = new List<string>()
            {
                "reward_group",
                "part_option",
                "event_candidate",
                "announcement",
                "skill_exp",
                "pass_info",
                "pet_merge_list",
                "bot_group",
                "bot_dragon_list",
                "skill_projectile",
                "tutorial_layer_base",
                "tutorial_base",
                "server_notice",
                "server_option",

                "string",
                "string_kor",
                "string_eng",
                "string_prt",
                "string_jpn",
                "string_chs",
                "string_cht",

                "mail_string",
                "mail_kor",
                "mail_eng",
                "mail_prt",
                "mail_jpn",
                "mail_chs",
                "mail_cht",

                "script_string",
                "script_kor",
                "script_eng",
                "script_prt",
                "script_jpn",
                "script_chs",
                "script_cht",
            };

            switch (GamePreference.Instance.GameLanguage)
            {
                case SystemLanguage.Korean:
                    skipTable.Remove("string_kor");
                    skipTable.Remove("mail_kor");
                    skipTable.Remove("script_kor");
                    break;
                case SystemLanguage.Japanese:
                    skipTable.Remove("string_jpn");
                    skipTable.Remove("mail_jpn");
                    skipTable.Remove("script_jpn");
                    break;
                case SystemLanguage.ChineseSimplified:
                    skipTable.Remove("string_chs");
                    skipTable.Remove("mail_chs");
                    skipTable.Remove("script_chs");
                    break;
                case SystemLanguage.ChineseTraditional:
                    skipTable.Remove("string_cht");
                    skipTable.Remove("mail_cht");
                    skipTable.Remove("script_cht");
                    break;
                case SystemLanguage.Portuguese:
                    skipTable.Remove("string_prt");
                    skipTable.Remove("mail_prt");
                    skipTable.Remove("script_prt");
                    break;
                default:
                    skipTable.Remove("string_eng");
                    skipTable.Remove("mail_eng");
                    skipTable.Remove("script_eng");
                    break;
            }

            DataBase.VersionCheck();

            using (UnityWebRequest req = UnityWebRequest.Post(NetworkManager.WEB_SERVER + "data/filehash", new WWWForm()))
            {
                req.timeout = 10;
                var reqAsync = req.SendWebRequest();
                while (false == reqAsync.isDone)
                {
                    yield return SBDefine.GetWaitForEndOfFrame();
                }
                if (req.result != UnityWebRequest.Result.Success)
                {
                    yield return DataSyncFailed();
                }

                JObject root = null;
                if (!string.IsNullOrEmpty(req.downloadHandler.text))
                    root = JObject.Parse(req.downloadHandler.text);

                if (!root.ContainsKey("rs") || root["rs"].Value<int>() != 0)
                {
                    Debug.LogError("오류류류류류");
                    yield return DataSyncFailed();
                }

                if (!SBFunc.IsJObject(root["hashes"]))
                {
                    Debug.LogError("오류류류류류");
                    yield return DataSyncFailed();
                }

                JObject hashes = (JObject)root["hashes"];
                List<JProperty> ie = hashes.Properties().ToList();
                List<string> needSyncTables = new List<string>();
                //해쉬 비교
                var localHashs = DataBase.GetHash();
                for (int i = 0; i < ie.Count; i++)
                {
                    if (skipTable.Contains(ie[i].Name))
                        continue;

                    if (localHashs != null)
                    {
                        var hash = localHashs.Find((cur) => cur.UNIQUE_KEY == ie[i].Name);
                        if (hash != null)
                        {
                            if (hash.HASH.CompareTo(ie[i].Value.Value<string>()) == 0)
                                continue;
                        }
                    }
                    needSyncTables.Add(ie[i].Name);
                }

                //해쉬가 일치하면 로컬 데이터 그대로 사용
                if (needSyncTables.Count == 0)
                {
                    Debug.Log("로컬 기획 데이터 사용");
                }
                else //서버해쉬와 로컬 해쉬가 불일치 = 서버 데이터 수용
                {
                    Debug.Log("서버 기획 데이터 사용");
                    yield return DataSync(state, needSyncTables);
                }
            }

            yield return DBPreload(state);
            state?.Invoke(1, 1); //"데이터싱크완료"
            SettingEvent.RefreshString();
            yield break;
        }

        /// <summary> 갱신할 테이블 리스트를 가지고 LocalDB갱신 </summary>
        /// <param name="state">게이지 표시 용도</param>
        /// <param name="tables">갱신 할 테이블 이름 리스트</param>
        public IEnumerator DataSync(DownloadState state, List<string> tables)
        {
            using var connection = DataBase.Get();
            for (int i = 0, count = tables.Count; i < count; ++i)
            {
                var tableName = tables[i];
                var mapping = connection.GetTableInfo(tableName);
                if (mapping == null || mapping.Count < 1)
                {
                    Type type = Type.GetType("SandboxNetwork.DB" + char.ToUpper(tableName[0]) + tableName.Substring(1));
                    if (type != null && connection.CreateTable(type) > 0)
                        mapping = connection.GetTableInfo(tableName);
                }

                if (mapping == null || mapping.Count < 1)
                {
                    SBFunc.Log("######## TableMapping FAIL => ", tableName, " #########");
                    continue;
                }

                int retryRemain = 3;
                while (retryRemain > 0)
                {
                    state?.Invoke(i, count * 2); //"데이터싱크중", 

                    SBFunc.Log("Sync Table : ", tableName);
                    WWWForm param = new WWWForm();
                    param.AddField("table", tableName);

                    using (UnityWebRequest req = UnityWebRequest.Post(NetworkManager.WEB_SERVER + "data/getfile", param))
                    {
                        req.timeout = 10;
                        yield return req.SendWebRequest();

                        JObject root = null;
                        if (!string.IsNullOrEmpty(req.downloadHandler.text))
                        {
                            try
                            {
                                root = JObject.Parse(req.downloadHandler.text);
                            }
                            catch
                            {
                                Debug.LogError("데이터 파싱 실패");
                            }
                        }

                        if (root == null || !root.ContainsKey("rs") || root["rs"].Value<int>() != 0)
                        {
                            retryRemain--;
                            Debug.LogError("오류류류류류");

                            if (retryRemain <= 0)
                            {
                                yield return DataSyncFailed();
                            }
                            else
                            {
                                yield return new WaitForSeconds(0.5f);
                                continue;
                            }
                        }

                        bool isColumnChanged = false;
                        bool initKey = false;
                        List<string> columns = new();
                        List<string> values = new();
                        List<int> skipIndex = new();
                        foreach (var it in root["data"])
                        {
                            JObject dp = null;
                            if (it.Type == JTokenType.Property)
                                dp = (JObject)((JProperty)it).Value;
                            else if (it.Type == JTokenType.Object)
                                dp = (JObject)it;

                            if (dp == null)
                                continue;

                            var index = 0;
                            foreach (var val in dp.Properties().ToArray())
                            {
                                if (skipIndex.Contains(index))
                                {
                                    index++;
                                    continue;
                                }

                                if (!initKey)
                                {
                                    if (val.Name.Contains("//"))
                                    {
                                        skipIndex.Add(index);
                                        index++;
                                        continue;
                                    }

                                    var column = mapping.Find(data => data.Name == val.Name);
                                    if (column == null)
                                    {
                                        //skipIndex.Add(index);
                                        //index++;
                                        SBFunc.Log("######## ", "TableName => ", tableName, " Column FAIL => ", val.Name, " #########");
                                        //continue;
                                        isColumnChanged = true;
                                    }

                                    columns.Add(val.Name);
                                }

                                switch (val.Value.Type)
                                {
                                    case JTokenType.Object:
                                    case JTokenType.Array:
                                    case JTokenType.Property:
                                        values.Add(val.Value.ToString());
                                        break;
                                    case JTokenType.Boolean:
                                        values.Add(val.Value.Value<bool>() ? "1" : "0");
                                        break;
                                    case JTokenType.Float:
                                    case JTokenType.Integer:
                                    {
                                        var value = val.Value.Value<string>();
                                        if (value == string.Empty)
                                            values.Add("0");
                                        else
                                            values.Add(value);
                                    }
                                    break;
                                    case JTokenType.Date:
                                    case JTokenType.String:
                                    default:
                                        values.Add(val.Value.Value<string>());
                                        break;
                                }
                                index++;
                            }
                            initKey = true;
                        }

                        if (isColumnChanged)
                        {
                            Type type = Type.GetType("SandboxNetwork.DB" + char.ToUpper(tableName[0]) + tableName.Substring(1));                            
                            if (type != null)
                            {
                                var query = string.Format("drop table if exists \"{0}\"", tableName);
                                connection.Execute(query);
                                connection.CreateTable(type);

                                Debug.LogWarning("Column 업데이트 성공!");
                                mapping = connection.GetTableInfo(tableName);
                            }
                            else
                            {
                                retryRemain--;
                                Debug.LogError("Column 업데이트 실패!");

                                if (retryRemain <= 0)
                                {
                                    yield return DataSyncFailed();
                                }
                                else
                                {
                                    yield return new WaitForSeconds(0.5f);
                                    continue;
                                }
                            }
                        }
                        else
                        {
                            connection.DeleteTable(tableName);
                        }

                        try
                        {
                            connection.Insert(tableName, columns, values);
                        }
                        catch
                        {
                            Debug.LogError("데이터 DB 동기화 실패 --" + tableName);
                        }

                        if (mapping.Count == columns.Count)
                            connection.InsertHash(tableName, root["hash"].Value<string>());
                    }

                    yield return new WaitForEndOfFrame();
                    break;
                }
            }

            connection.Destory();
        }
        IEnumerator DataSyncFailed()
        {
            Debug.Log("데이터 동기화 실패");
            SystemLoadingPopup systemPopup = SystemLoadingPopup.Instance;
            if (systemPopup != null)
            {
                //data sync error에 대한 이벤트 통계를 위하여 추가
                LoginManager.Instance.SetFirebaseEvent("data_sync_error");
                systemPopup.SetMessage(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100002676), StringData.GetStringByIndex(100000199));
                systemPopup.SetCallBack(() =>
                {
                    Instance.BackToLoginScene(false);
                });
            }

            while (true)
            {
                yield return new WaitForEndOfFrame();
            }
        }
        // DataBase의 테이블 Preload
        private IEnumerator DBPreload(DownloadState state)
        {
            TableManager.Instance.Preload();
            //정기적인 기획데이터 시스템화
            TableManager.GetTable<PassInfoTable>().SetTable(null);
            yield break;
        }
        public bool IsLoaded()
        {
            return isLoadeds != null;
        }
        public void Update(float dt)
        {
            if (updateManagers == null)
                return;

            var count = updateManagers.Count;
            for (var i = 0; i < count; ++i)
            {
                updateManagers[i]?.Update(dt * TimeScale);
            }
        }
        public bool AddManager(Type type, IManagerBase target, bool isUpdate = false)
        {
            if (managers == null || updateManagers == null)
                return false;

            if (managers.ContainsKey(type)) //매니저 중복
                return false;

            target.Initialize();

            managers.Add(type, target);
            if (isUpdate)
                updateManagers.Add(target);

            return true;
        }
        public bool ContainManager(Type type)
        {
            if (managers == null || updateManagers == null)
                return false;

            return managers.ContainsKey(type);
        }
        public bool DelManager(Type type, IManagerBase target)
        {
            if (managers == null || updateManagers == null)
                return false;

            bool isRemove = managers.Remove(type);
            if (isRemove) //매니저 중복
                updateManagers.Remove(target);

            return isRemove;
        }
        public static T GetManager<T>() where T : class, IManagerBase
        {
            var type = typeof(T);

            if (Instance.managers.ContainsKey(type) && Instance.managers[type] is T)
                return Instance.managers[type] as T;

            return null;
        }
        public void SetResolution()
        {
            int setWidth = 1280;
            int setHeight = 720;

            int deviceWidth = Screen.width;
            int deviceHeight = Screen.height;

            Screen.SetResolution(setWidth, (int)(((float)deviceHeight / deviceWidth) * setWidth), true);

            if ((float)setWidth / setHeight < (float)deviceWidth / deviceHeight)
            {
                float newWidth = ((float)setWidth / setHeight) / ((float)deviceWidth / deviceHeight);
                Camera.main.rect = new Rect((1f - newWidth) / 2f, 0f, newWidth, 1f);
            }
            else // 게임의 해상도 비가 더 큰 경우
            {
                float newHeight = ((float)deviceWidth / deviceHeight) / ((float)setWidth / setHeight);
                Camera.main.rect = new Rect(0f, (1f - newHeight) / 2f, 1f, newHeight);
            }
        }
        public void SetCamera(Camera camera)
        {
            if (camera == null)
                return;

            int setWidth = 1280;
            int setHeight = 720;

            int deviceWidth = Screen.width;
            int deviceHeight = Screen.height;

            if ((float)setWidth / setHeight < (float)deviceWidth / deviceHeight)
            {
                float newWidth = ((float)setWidth / setHeight) / ((float)deviceWidth / deviceHeight);
                camera.rect = new Rect((1f - newWidth) / 2f, 0f, newWidth, 1f);
            }
            else // 게임의 해상도 비가 더 큰 경우
            {
                float newHeight = ((float)deviceWidth / deviceHeight) / ((float)setWidth / setHeight);
                camera.rect = new Rect(0f, (1f - newHeight) / 2f, 1f, newHeight);
            }
        }
        public static SystemLanguage ConvertSystemLangByStringLang(string _stringLanguage)
        {
            switch (_stringLanguage)
            {
                case "kor":
                case "KOR":
                case "ko-KR":
                case "kr":
                    return SystemLanguage.Korean;
                case "ENG":
                case "eng":
                case "en":
                    return SystemLanguage.English;
                case "JPN":
                case "jpn":
                case "jp":
                    return SystemLanguage.Japanese;
                case "prt":
                case "pt":
                    return SystemLanguage.Portuguese;
                case "chs":
                case "cs":
                    return SystemLanguage.ChineseSimplified;
                case "cht":
                case "ct":
                    return SystemLanguage.ChineseTraditional;
                default:
                    return SystemLanguage.English;
            }
        }
        public string ConvertStringLangBySystemLang(bool _isUpper = false)
        {
            string stringLang;
            switch (gamePrefData.GameLanguage)
            {
                case SystemLanguage.Korean:
                    stringLang = "kor";
                    break;
                case SystemLanguage.English:
                    stringLang = "eng";
                    break;
                case SystemLanguage.Japanese:
                    stringLang = "jpn";
                    break;
                case SystemLanguage.Portuguese:
                    stringLang = "prt";
                    break;
                case SystemLanguage.ChineseSimplified:
                    stringLang = "chs";
                    break;
                case SystemLanguage.ChineseTraditional:
                    stringLang = "cht";
                    break;
                default:
                    stringLang = "eng";
                    break;
            }

            if (_isUpper)
                stringLang = stringLang.ToUpper();
            return stringLang;
        }
#region 미사용
        //private JObject GetHash()
        //{
        //    var stringHashes = PlayerPrefs.GetString("hashes", "");
        //    if (stringHashes == "")
        //    {
        //        return null;
        //    }
        //    return JObject.Parse(stringHashes);
        //}
        //private void SetHash(JToken token)
        //{
        //    if (!SBFunc.IsJObject(token))
        //        return;

        //    JObject jsonData = (JObject)token;

        //    var hashes = GetHash();
        //    if (hashes != null)
        //    {
        //        var properties = jsonData.Properties();
        //        foreach (JProperty it in properties)
        //        {
        //            if (hashes.ContainsKey(it.Name))
        //                hashes.Remove(it.Name);

        //            hashes.Add(it.Name, it.Value);
        //        }

        //        PlayerPrefs.SetString("hashes", hashes.ToString());
        //    }
        //    else
        //    {
        //        PlayerPrefs.SetString("hashes", jsonData.ToString());
        //    }
        //}
        //private void CompareHashDesignData(JObject root)
        //{
        //    List<GameData> localDatas = SBGameData.ParseGameData();

        //    if (localDatas.Count == 0)
        //    {
        //        //PlayerPref에 세팅된 기획 데이터가 없는가?
        //        //로컬 로드
        //        localDatas = SBGameData.GetAllLocalDatas();
        //    }

        //    if (!SBFunc.IsJObject(root["hashes"]))
        //    {
        //        //DataSyncFailed();
        //    }

        //    JObject hashes = (JObject)root["hashes"];
        //    List<JProperty> ie = hashes.Properties().ToList();
        //    List<string> needSyncTables = new List<string>();
        //    //해쉬 비교
        //    //서버 해쉬수가 더 많음 => 불일치
        //    List<string> localNames = SBGameData.Extract(localDatas, ExtractOption.KEY);
        //    List<string> localHashes = SBGameData.Extract(localDatas, ExtractOption.HASH);
        //    int index = 0;

        //    for (int i = 0; i < ie.Count; i++)
        //    {
        //        index = localNames.IndexOf(ie[i].Name);

        //        if (index < 0 || localHashes[index].CompareTo(ie[i].Value.Value<string>()) != 0)
        //        {
        //            //서버해쉬와 로컬 해쉬가 불일치
        //            needSyncTables.Add(ie[i].Name);
        //        }
        //    }

        //    //해쉬가 일치하면 로컬 데이터 그대로 사용
        //    if (needSyncTables.Count == 0)
        //    {
        //        Debug.Log("로컬 기획 데이터 사용");
        //        SBGameData.SaveDesignData(localDatas);
        //    }
        //    else //서버해쉬와 로컬 해쉬가 불일치 = 서버 데이터 수용
        //    {
        //        Debug.Log("서버 기획 데이터 사용");

        //        //NetworkManager.Send("data/getall", null, GetAllDataB, DataSyncFailed);
        //    }
        //}
        //private JObject GetData()
        //{
        //    var datas = SBGameData.ParseGameData();
        //    if (datas.Count == 0)
        //        return null;

        //    JObject ret = new JObject();
        //    for (int i = 0; i < datas.Count; i++)
        //    {
        //        JProperty property = new JProperty(datas[i].name, datas[i].data);

        //        ret.Add(property.Name, property.Value);
        //    }

        //    return ret;
        //}
#endregion
    }
}
