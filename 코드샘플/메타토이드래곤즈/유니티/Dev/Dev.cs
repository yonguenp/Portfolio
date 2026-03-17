using Coffee.UIEffects;
using Newtonsoft.Json.Linq;
using SandboxNetwork;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Dev : MonoBehaviour
{
    [SerializeField]
    private Text textServerChoice;
    [SerializeField]
    private Text textServerType;

    [SerializeField]
    private GameObject btnUseBundle;

    [SerializeField]
    Text Nick;

    [SerializeField]
    Text Token;

    [SerializeField]
    GameObject AccountFindPanel;

    [SerializeField]
    InputField AccountNick;

    [SerializeField]
    UIGradient bg;

    [SerializeField]
    AccountLoader guest;

    [SerializeField]
    Toggle SplashFlagToggle = null;
    [SerializeField]
    Dropdown LanguageDropDown = null;

    [SerializeField]
    GameObject GetAllDragonButton = null;
    [SerializeField]
    GameObject UserStateButton = null;
    [SerializeField]
    GameObject UserStateToggles = null;
    [SerializeField]
    Toggle[] UserState = null;

    [SerializeField]
    Toggle DmgShowToggle = null;
    [SerializeField]
    Toggle DmgLogToggle = null;

    string GetServerName(int tag)
    {
        switch (tag)
        {
            case 0:
                return "테스트 서버";
            case 1:
                return "엔젤 서버";
            case 2:
                return "원더 서버";
            case 3:
                return "루나 서버";
        }

        return "오류";
    }

    void Start()
    {
        UserStateToggles.SetActive(false);
#if SB_TEST || UNITY_EDITOR
        AccountFindPanel.SetActive(true);
        GetAllDragonButton.SetActive(true);
        UserStateButton.SetActive(true);
        DmgShowToggle.gameObject.SetActive(true);
#else
        AccountFindPanel.SetActive(false);
        GetAllDragonButton.SetActive(false);
        UserStateButton.SetActive(false);
        DmgShowToggle.gameObject.SetActive(false);
#endif

        btnUseBundle.GetComponentInChildren<Text>().text = AssetBundleManager.UseBundleAssetEditor ? "번들 리소스 사용 중" : "내부 리소스 사용 중";
        textServerChoice.text = NetworkManager.IsLiveServer ? "LIVE 서버" :
#if SB_TEST
            NetworkManager.IsQAServer ? "QA 서버" :
#endif
        "DEV 서버";
        //textServerType.transform.parent.gameObject.SetActive(NetworkManager.IsLiveServer);
        switch (SBGameManager.CurServerTag)
        {
            case 0:
                if (NetworkManager.IsLiveServer)
                {
                    textServerType.text = GetServerName(1);
                    SBGameManager.CurServerTag = 1;
                }
                else
                {
                    textServerType.text = GetServerName(0);
                }
                break;
            case 1:
                textServerType.text = GetServerName(1);
                break;
            case 2:
                textServerType.text = GetServerName(2);
                break;
            case 3:
                textServerType.text = GetServerName(3);
                break;
            default:
                textServerType.text = "미선택";
                break;
        }

        switch (GamePreference.Instance.GameLanguage)
        {
            case SystemLanguage.Korean:
                LanguageDropDown.value = 0;
                break;
            case SystemLanguage.English:
                LanguageDropDown.value = 1;
                break;
            case SystemLanguage.Japanese:
                LanguageDropDown.value = 2;
                break;
            case SystemLanguage.Portuguese:
                LanguageDropDown.value = 3;
                break;
            default:
                LanguageDropDown.value = 0;
                break;
        }

        RefreshAccount();

        StartCoroutine(BGAction());
    }

    private void Update()
    {
        NetworkManager.Instance.Update(Time.deltaTime);
    }
    public void OnClickServerChoice()
    {
#if SB_TEST
        if (NetworkManager.IsLiveServer)
        {
            NetworkManager.IsLiveServer = false;
            //NetworkManager.IsQAServer = false;
        }
        else
        {
            NetworkManager.IsLiveServer = true;

            //if (NetworkManager.IsQAServer)
            //{
            //    NetworkManager.IsLiveServer = true;
            //    NetworkManager.IsQAServer = false;
            //}
            //else
            //{
            //    NetworkManager.IsLiveServer = false;
            //    NetworkManager.IsQAServer = true;
            //}
        }
#else
        NetworkManager.IsLiveServer = !NetworkManager.IsLiveServer;
#endif

        textServerChoice.GetComponentInChildren<Text>().text = NetworkManager.IsLiveServer ? "LIVE 서버" :
#if SB_TEST
            NetworkManager.IsQAServer ? "QA 서버" :
#endif
            "DEV 서버";

        //textServerType.transform.parent.gameObject.SetActive(NetworkManager.IsLiveServer);
        switch (SBGameManager.CurServerTag)
        {
            case 0:
                if (NetworkManager.IsLiveServer)
                {
                    textServerType.text = GetServerName(1);
                    SBGameManager.CurServerTag = 1;
                }
                else
                {
                    textServerType.text = GetServerName(0);
                }
                break;
            case 1:
                textServerType.text = GetServerName(1);
                break;
            case 2:
                textServerType.text = GetServerName(2);
                break;
            case 3:
                textServerType.text = GetServerName(3);
                break;
            default:
                textServerType.text = "미선택";
                break;
        }

        LogOut();
        guest.Refresh();
        DataBase.Destroy();
    }

    public void OnClickServerType()
    {
        SBGameManager.CurServerTag = (SBGameManager.CurServerTag + 1) % 4;

        if (NetworkManager.IsLiveServer && SBGameManager.CurServerTag == 0)
            SBGameManager.CurServerTag = 1;

        switch (SBGameManager.CurServerTag)
        {
            case 0:
                textServerType.text = GetServerName(0);
                break;
            case 1:
                textServerType.text = GetServerName(1);
                break;
            case 2:
                textServerType.text = GetServerName(2);
                break;
            case 3:
                textServerType.text = GetServerName(3);
                break;
            default:
                textServerType.text = "미선택";
                break;
        }

        LogOut();
        guest.Refresh();
    }

    public void OnClickToggleBundleUse()
    {
        AssetBundleManager.UseBundleAssetEditor = !AssetBundleManager.UseBundleAssetEditor;
        btnUseBundle.GetComponentInChildren<Text>().text = AssetBundleManager.UseBundleAssetEditor ? "번들 리소스 사용 중" : "내부 리소스 사용 중"; //누구인가       
    }

    public void OnClickInitializeInnerDB()
    {
        DataBase.DropTable();
        Debug.Log("DB 초기화 완료 - 로그인해주세요.");
    }


    public void LogOut()
    {
        SBGameManager.Instance.UserAccessToken = "";
        SBGameManager.Instance.UserNickname = "";

        RefreshAccount();
    }

    public void RefreshAccount()
    {
        string tok = SBGameManager.Instance.UserAccessToken;
        if (string.IsNullOrEmpty(tok))
        {
            Nick.text = "기록 없음";
            Token.text = "";
        }
        else
        {
            Nick.text = SBGameManager.Instance.UserNickname;
            Token.text = tok;
        }
    }

    public void GameStart()
    {
#if UNITY_EDITOR

        if (User.Instance.UserData.USE_EDITOR_STATE)
        {
            int state = 0;
            for (int i = 0; i < UserState.Length; i++)
            {
                if (UserState[i].gameObject.activeInHierarchy && UserState[i].isOn)
                    state += 1 << (i);
            }

            User.Instance.UserData.SetUserStateForDebug(state);
        }

        if (DmgLogToggle != null && DmgLogToggle.isOn)
        {
            PlayerPrefs.SetInt("DmgLogOn", 1);
        }
        else
        {
            PlayerPrefs.SetInt("DmgLogOn", 0);
        }
        if (DmgShowToggle != null && DmgShowToggle.isOn)
        {
            PlayerPrefs.SetInt("DmgShowOn", 1);
        }
        else
        {
            PlayerPrefs.SetInt("DmgShowOn", 0);
        }

#endif

        //switch (LanguageDropDown.value)
        //{
        //    case 0:
        //        GamePreference.Instance.GameLanguage = SystemLanguage.Korean;
        //        break;
        //    case 1:
        //        GamePreference.Instance.GameLanguage = SystemLanguage.English;
        //        break;
        //    case 2:
        //        GamePreference.Instance.GameLanguage = SystemLanguage.Japanese;
        //        break;
        //    case 3:
        //        GamePreference.Instance.GameLanguage = SystemLanguage.Portuguese;
        //        break;
        //}

        if (SplashFlagToggle.isOn)
            SplashStart();
        else
            SceneManager.LoadScene("Start");
    }

    public void OnToBeHolder()
    {
        StartCoroutine(SendBecomeHolder());
    }

    IEnumerator SendBecomeHolder()
    {
        WWWForm param = new WWWForm();
        param.AddField("token", SBGameManager.Instance.UserAccessToken);
        param.AddField("op", 1);
        param.AddField("social_type", 1);

        using (UnityWebRequest req = UnityWebRequest.Post(NetworkManager.WEB_SERVER + "comDapps/actionfromdapp", param))
        {
            req.timeout = 10;
            yield return req.SendWebRequest();

            Debug.LogError("홀더가 되었다.");
        }
    }
    public void SplashStart()
    {
        SceneManager.LoadScene("Splash");
    }

    IEnumerator BGAction()
    {
        //눈알을 조지자
        bool down0 = true;
        bool down1 = true;
        bool down2 = true;
        while (true)
        {
            if (down0)
            {
                if (bg.rotation > -180)
                    bg.rotation -= SBFunc.RandomValue;
                else
                    down0 = false;
            }
            else
            {
                if (bg.rotation < 180)
                    bg.rotation += SBFunc.RandomValue;
                else
                    down0 = true;
            }

            var offset = bg.offset2;
            if (down1)
            {
                if (offset.x > -1f)
                    offset.x -= Time.deltaTime * SBFunc.RandomValue * 0.1f;
                else
                    down1 = false;
            }
            else
            {
                if (offset.x < 1f)
                    offset.x += Time.deltaTime * SBFunc.RandomValue * 0.1f;
                else
                    down1 = true;
            }

            if (down2)
            {
                if (offset.y > -1f)
                    offset.y -= Time.deltaTime * SBFunc.RandomValue * 0.1f;
                else
                    down2 = false;
            }
            else
            {
                if (offset.y < 1f)
                    offset.y += Time.deltaTime * SBFunc.RandomValue * 0.1f;
                else
                    down2 = true;
            }

            bg.offset2 = offset;

            yield return SBDefine.GetWaitForEndOfFrame();
        }
    }
#if SB_TEST || UNITY_EDITOR
    public void LoadAccountInfo()
    {
        string nick = AccountNick.text;
        if (string.IsNullOrEmpty(nick))
        {
            Debug.LogError("오류 발생");
            return;
        }

        var data = new WWWForm();
        data.AddField("nick", nick);

        AccountNick.text = "";

        var network = NetworkManager.Instance;
        network.Initialize();
                
        NetworkManager.Send("dev/msgsign", data, (jObject) => {
            switch (jObject["rs"].Value<int>())
            {
                case (int)eApiResCode.OK: //유저 데이터 채우기 및 씬 이동
                    if (!jObject.ContainsKey("token_bin"))
                    {
                        Debug.LogError("유저 찾을수 없음");
                        return;
                    }
                    string binToken = jObject["token_bin"].Value<string>();

                    if (!string.IsNullOrEmpty(binToken))
                    {
                        SBGameManager.Instance.UserAccessToken = binToken;
                        SBGameManager.Instance.UserNickname = nick;

                        RefreshAccount();
                        return;
                    }
                    break;
            }
            Debug.LogError("오류 발생");
        });
    }
#endif


#if UNITY_EDITOR
    string hex2bin(string hexdata)
    {
        if (hexdata == null)
            return "";
        if (hexdata.Length % 2 != 0)
            return "";

        byte[] bytes = new byte[hexdata.Length / 2];
        for (int i = 0; i < hexdata.Length; i += 2)
            bytes[i / 2] = (byte)(HexValue(hexdata[i]) * 0x10
            + HexValue(hexdata[i + 1]));
        return Encoding.GetEncoding(1252).GetString(bytes);
    }
    int HexValue(char c)
    {
        int ch = (int)c;
        if (ch >= (int)'0' && ch <= (int)'9')
            return ch - (int)'0';
        if (ch >= (int)'a' && ch <= (int)'f')
            return ch - (int)'a' + 10;
        if (ch >= (int)'A' && ch <= (int)'F')
            return ch - (int)'A' + 10;

        return 0;
    }

    public void GetAllDragon()
    {
        string nick = SBGameManager.Instance.UserNickname;
        if (string.IsNullOrEmpty(nick))
            return;

        var data = new WWWForm();
        data.AddField("nick", nick);

        var network = NetworkManager.Instance;
        network.Initialize();

        NetworkManager.Send("dev/msgsign", data, (jObject) => {
            switch (jObject["rs"].Value<int>())
            {
                case (int)eApiResCode.OK: //유저 데이터 채우기 및 씬 이동
                    string binToken = jObject["token_bin"].Value<string>();

                    if (!string.IsNullOrEmpty(binToken))
                    {
                        SBGameManager.Instance.UserAccessToken = binToken;
                        SBGameManager.Instance.UserNickname = nick;

                        RefreshAccount();

                        var data = new WWWForm();
                        User.Instance.UserAccountData.Set(jObject);
                        data.AddField("uno", jObject["uno"].Value<string>());
#if UNITY_EDITOR
                        System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo("git.exe");

                        startInfo.UseShellExecute = false;
                        startInfo.WorkingDirectory = Application.dataPath;
                        startInfo.RedirectStandardInput = true;
                        startInfo.RedirectStandardOutput = true;
                        startInfo.Arguments = "rev-parse --abbrev-ref HEAD";

                        System.Diagnostics.Process process = new System.Diagnostics.Process();
                        process.StartInfo = startInfo;
                        process.Start();

                        string BRANCH_NAME = process.StandardOutput.ReadLine().ToLower();
                        Debug.Log("Current branch name : " + BRANCH_NAME);

                        data.AddField("branch", BRANCH_NAME);

#endif//UNITY_EDITOR
                        NetworkManager.Send("dev/getalldragon", data, (jObject) =>
                        {
                            switch (jObject["rs"].Value<int>())
                            {
                                case (int)eApiResCode.OK:
                                    Debug.LogError("적용 완료");
                                    break;
                            }
                        });
                        return;
                    }
                    break;
            }
            Debug.LogError("오류 발생");
        });
    }

    public void OnBattleSimulator()
    {
        StartCoroutine(DataLoad(() => {
            SceneManager.LoadScene("Simulator_battle_start");
        }));
    }
    public void OnSpineSimulator()
    {
        StartCoroutine(DataLoad(() => {
            SceneManager.LoadScene("spine_Tool");
        }));
    }
    public void OnScrpitSimulator()
    {
        SceneManager.LoadScene("script_tool");
    }

    public void OnUserStateToggleShow()
    {
        User.Instance.UserData.USE_EDITOR_STATE = !User.Instance.UserData.USE_EDITOR_STATE;
        UserStateToggles.SetActive(User.Instance.UserData.USE_EDITOR_STATE);
    }

    IEnumerator DataLoad(System.Action cb)
    {
        var gameController = new GameObject();
        gameController.AddComponent<Game>();
        DontDestroyOnLoad(gameController);

        yield return SBGameManager.Instance.GameDataSyncAndLoad(null);

        cb?.Invoke();
    }
#endif

#if UNITY_EDITOR_WIN
    [ContextMenu("AutoTableGenerator")]
    void AutoTableGenerator()
    {
        string def = "\n";
        System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(Application.dataPath + "/Resources/Data");
        foreach (System.IO.FileInfo file in di.GetFiles())
        {
            if (file.Extension == ".csv")
            {
                def += "\n";
                var tableName = file.Name.Replace(file.Extension, "");
                TextAsset dAsset = Resources.Load<TextAsset>(SBFunc.StrBuilder("Data/" + tableName));
                using (StringReader sr = new(dAsset.text))
                {
                    string[] splitType = sr.ReadLine().Split(",");
                    string[] splitColumn = sr.ReadLine().Split(",");

                    var typeLen = Mathf.Min(splitType.Length, splitColumn.Length);
                    Dictionary<string, string> columns = new();
                    for (int i = 0; i < typeLen; ++i)
                    {
                        var typestring = GetTypeString(splitType[i]);
                        if (string.IsNullOrEmpty(typestring))
                            continue;
                        columns.Add(splitColumn[i], typestring);
                    }

                    if(columns.Count > 0)
                    {
                        string className = "DB" + char.ToUpper(tableName[0]) + tableName.Substring(1);
                        
                        def += "[Table(\"" + tableName + "\")]\npublic class " + className + " : DBData\n{\n";
                        foreach(var col in columns)
                        {
                            if (col.Key.Contains("//"))
                                continue;

                            if (col.Key == "KEY")
                                continue;

                            def += "    public " + col.Value + " " + col.Key + " { get; set; }";
                            
                            var defulatVal = GetDefaultValue(col.Value);
                            if (string.IsNullOrEmpty(defulatVal) == false)
                            {
                                def += " = " + defulatVal + ";";
                            }

                            def += "\n";
                        }
                        def += "}\n";
                    }
                }
            }
        }

        Debug.LogError(def);

        var output = "Assets/Resources/Data/gen.txt";
        if (false == File.Exists(output))
        {
            File.CreateText(output);
        }

        StreamWriter sw = new StreamWriter(output);
        sw.Write(def);
        sw.Close();

        Debug.LogError("생성완료");
    }

    public static string GetTypeString(string data)
    {
        if (data.CompareTo("int") == 0 || data.CompareTo("string") == 0 || data.CompareTo("float") == 0)
        {
            return data;
        }

        if (data.CompareTo("string") == 0)
            return "string";

        return null;
    }

    public static string GetDefaultValue(string strType)
    {
        switch(strType)
        {
            case "int":
                return "-1";
            case "string":
                return "";
            case "float":
                return "0.0f";
        }

        return null;
    }
#endif
}
