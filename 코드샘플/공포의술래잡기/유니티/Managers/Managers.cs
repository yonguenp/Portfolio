using SandboxPlatform.SAMANDA;
using SBCommonLib;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Managers : PersistentSingleton<Managers>
{
    #region Contents
    ObjectManager _obj = new ObjectManager();
    EffectManager _effect = new EffectManager();
    ChatManager _chat = new ChatManager();
    ClanChatManager _clanchat = new ClanChatManager();

    public static ObjectManager Object { get { return Instance._obj; } }
    public static EffectManager Effect { get { return Instance._effect; } }
    public static ChatManager Chat { get { return Instance._chat; } }
    public static ClanChatManager ClanCaht { get { return Instance._clanchat; } }
    #endregion

    #region Core
    PoolManager _pool = new PoolManager();
    SoundManager _sound = new SoundManager();
    ResourceManager _resource = new ResourceManager();
    SceneManagerEx _scene = new SceneManagerEx();
    NetworkManager _network = new NetworkManager();
    IAPManager _iap = null;
    AdvertiseManager _ads = null;
    TimeManager _time = new TimeManager();

    public static PoolManager Pool { get { return Instance._pool; } }
    public static ResourceManager Resource { get { return Instance._resource; } }
    public static SceneManagerEx Scene { get { return Instance._scene; } }
    public static SoundManager Sound { get { return Instance._sound; } }
    public static NetworkManager Network { get { return Instance._network; } }
    public static GameServerManager GameServer { get { return Network.GameServer; } }
    public static TimeManager TimeManager { get { return Instance._time; } }
    public static IAPManager IAP { 
        get {
            if (Instance._iap == null)
            {
                Instance._iap = Instance.GetComponent<IAPManager>();
                if (Instance._iap == null)
                {
                    Instance._iap = Instance.gameObject.AddComponent<IAPManager>();
                }
            }
            return Instance._iap;
        } 
    }

    public static AdvertiseManager ADS 
    { 
        get { 
            if(Instance._ads == null)
            {
                Instance._ads = Instance.GetComponent<AdvertiseManager>();
                if(Instance._ads == null)
                {
                    Instance._ads = Instance.gameObject.AddComponent<AdvertiseManager>();
                }
            }
            return Instance._ads; 
        } 
    }

    
    #endregion

    #region Data
    GameDataManager _tableData = new GameDataManager();
    PlayDataManager _playData = new PlayDataManager();
    UserDataManager _userData = new UserDataManager();
    FriendsManager _fridendData = new FriendsManager();

    public static GameDataManager Data { get { return Instance._tableData; } }
    public static PlayDataManager PlayData { get { return Instance._playData; } }
    public static UserDataManager UserData { get { return Instance._userData; } }
    public static FriendsManager FriendData { get { return Instance._fridendData; } }
    #endregion

    #region Debug

    #endregion

    protected override void Awake()
    {
        base.Awake();
        Init();
    }

    void Update()
    {
        if (_network != null)
        {
            _network.Update();
        }

        if (Input.GetKeyUp(KeyCode.PageDown))
        {
            Managers.Sound.Mute();
        }
        else if (Input.GetKeyUp(KeyCode.PageUp))
        {
            Managers.Sound.UnMute();
        }

#if UNITY_EDITOR || SB_TEST
        if (Input.GetKeyDown(KeyCode.F1))
            Disconnected();
        else if(Input.GetKeyDown(KeyCode.F2))
            UnityEngine.SceneManagement.SceneManager.LoadScene("Start");
#endif
    }

    static void Init()
    {
        Resource.ClearAsset();
        
        Pool.Init();
        Sound.Init();
        Data.TempInit();
    }

    public static void Clear()
    {
        Scene.Clear();
        Pool.Clear();
    }

    #region Network
    
    Coroutine SessionAliveChecker = null;
    public void RunAlivePingNotify()
    {
        if (SessionAliveChecker != null)
            StopCoroutine(SessionAliveChecker);

        SessionAliveChecker = StartCoroutine(AliveCoroutine());
    }

    IEnumerator AliveCoroutine()
    {
        NetworkReachability networkType = Application.internetReachability;
        bool connected = true;
        while (connected)
        {
            if (Network.IsAlive())
            {
                Network.SendAlivePingNotify();
                SBDebug.Log("SendAlivePingNotify!");
            }
            else
            {
                SBDebug.LogWarning("Failed SendAlivePingNotify");
            }

            float waitSeconds = 60.0f;
            while (waitSeconds > 0 && connected)
            {
                waitSeconds -= Time.deltaTime;

                if (Application.internetReachability != networkType || Application.internetReachability == NetworkReachability.NotReachable)
                {
                    Disconnected(1);
                    connected = false;
                }
                else
                {
                    yield return new WaitForEndOfFrame();
                }
            }
        }
    }

    public void FailGameServerConnect()
    {
        Disconnected(1);
    }

    public void Disconnected()
    {
        Disconnected(0);
    }

    public void Disconnected(int reason = 0)
    {
        if (LoadingUI.IsLoading)
            LoadingUI.ClearLoadingUI();

        PopupCanvas.Instance.ClearAll();

        switch (reason)
        {
            case 0:
                PopupCanvas.Instance.ShowMessagePopup("서버연결종료", () =>
                {
                    SAMANDA.Instance.UI.SetMainCloseCallback(() => 
                    { 
                        UnityEngine.SceneManagement.SceneManager.LoadScene("Start"); 
                    });

                    SAMANDA.Instance.UI.SetUIState(LOGIN_STATE.MAIN_OPEN);
                });
                
                break;
            case 1:
                PopupCanvas.Instance.ShowMessagePopup("네트워크불안정", () =>
                {
                    UnityEngine.SceneManagement.SceneManager.LoadScene("Start");
                });
                break;
        }        
    }

    public void Dodge()
    {
        Network.DisconnectDodge();

        if (LoadingUI.IsLoading)
            LoadingUI.ClearLoadingUI();

        PopupCanvas.Instance.ClearAll();
        //todo : Disconnect 이유를 알려줘야되지않을까
        PopupCanvas.Instance.ShowMessagePopup(StringManager.GetString("닷지메시지"), () =>
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("Start");
        });
    }
    #endregion


    float blurTime = 0;
    public void OnApplicationFocus(bool isFocus)
    {
        if (!isFocus)
        {
            blurTime = Time.time;
        }
        else
        {
            if (blurTime != 0)
            {
                if (Time.time - blurTime > 1800.0f)
                {
                    if (Network.IsAlive())
                        Network.Disconnect();
                    else
                        Network.OnDisconnected();
                }
            }

            blurTime = 0;
        }

    }
}
