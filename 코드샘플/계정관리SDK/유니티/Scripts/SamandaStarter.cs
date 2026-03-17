#define USE_SAMANDA_RENEWL

using Firebase.DynamicLinks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SamandaStarter : MonoBehaviour
{
    public bool Auto_run = true;
    public string Product_ID = "";
    public bool UseChat = true;
    public bool UseLandscape = true;
    public bool UseCustomOverlayChat = true;
    public GameObject SandboxSplash;
    public string GoogleOAuthClientId = "";
    
    public delegate void Callback();
    private Callback callbackSamandaStartDone = null;
    private Callback OnBackKeyDown = null;

    static public bool LiveServer = true;

    public void SetSamandaStartCallback(Callback cb)
    {
        callbackSamandaStartDone = cb;
    }

    public void SetBackKeyCallback(Callback cb)
    {
        OnBackKeyDown = cb;
    }

    void Start()
    {
        if (
#if UNITY_EDITOR
            Debug.isDebugBuild && !LiveServer
#else
            GameDataManager.Instance.UseTestServer() //결제테스트
#endif
            )
        {
            NetworkManager.BASE_URL = "https://sandbox-gs.mynetgear.com/";
            NetworkManager.DOWNLOAD_URL = NetworkManager.BASE_URL + "hahaha/";   
            NetworkManager.SAMANDA_URL = "https://sandbox-gs.mynetgear.com/sdk4";
            NetworkManager.GAMESERVER_URL = NetworkManager.BASE_URL + "hahaha/v5/";
            NetworkManager.CUSTOMER_SERVICE_URL = "https://sandboxnetwork.zendesk.com/hc/ko";
            NetworkManager.DONATION_URL = "https://sandbox-gs.com/patron_list/list";
            NetworkManager.CHAT_URL = NetworkManager.BASE_URL + "openchat";
            NetworkManager.PERSONAL_CHAT_URL = NetworkManager.BASE_URL + "personalchat";
            NetworkManager.DONATION_URL = NetworkManager.BASE_URL + "patron_list/list";
        }


        if (Auto_run)
            RunSamanda();

        Firebase.DynamicLinks.DynamicLinks.DynamicLinkReceived += OnDynamicLink;
    }

    void RunSamanda()
    {
        //int contentSeq = PlayerPrefs.GetInt("contents", 0);
        //if(contentSeq <= (int)neco_data.PrologueSeq.튜토리얼완료)
        //    PlayerPrefs.DeleteAll();

        //if (callbackSamandaStartDone == null)
        //{
        //    Debug.LogError("not set callbackSamandaStartDone!!!!!!!!!!!!!");

        //    return;
        //}

        if (Application.internetReachability != NetworkReachability.NotReachable)
        {
            SamandaLauncher.SetServerMaintenanceCallback(null);
            SamandaLauncher.SetNetworkErrorCallback(null);

            SamandaLauncher.Samanda_Option option = SamandaLauncher.Samanda_Option.FULL_OPTION;

            if (UseChat == false)
                option &= ~SamandaLauncher.Samanda_Option.ENABLE_CHAT;

            if (UseLandscape == false)
                option &= ~SamandaLauncher.Samanda_Option.ORIENTATION_MODE;

            if (UseCustomOverlayChat == false)
                option &= ~SamandaLauncher.Samanda_Option.USE_CUSTOM_OVERLAY;

            SamandaLauncher.Initialize(Product_ID, GoogleOAuthClientId, option, SystemLanguage.Korean);

            SamandaLauncher.StartSamanda(() =>
            {
                SamandaLauncher.SetNetworkErrorCallback(null);

                callbackSamandaStartDone?.Invoke();
            });

            SamandaLauncher.SetSamandaButtonMakeFunction(() =>
            {

            });

            SamandaLauncher.CustomerServiceURL = NetworkManager.CUSTOMER_SERVICE_URL;

            SamandaLauncher.OnHideScreen();
        }

        SandboxSplash.SetActive(true);
    }

    public void OnWebLoadedCheck()
    {
        
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            OnBackKey();
        }
    }

    void OnBackKey()
    {
        if (SceneManager.GetActiveScene().name == "Splash")
            return;

        if (!SamandaLauncher.OnBackKeyPressed())//사만다가 떠있는 상태라면
            return;

        if(OnBackKeyDown != null)
            OnBackKeyDown();
    }

    public void OnNetworkError()
    {
        CancelInvoke("OnNetworkError");

        callbackSamandaStartDone = null;

        SamandaLauncher.SetOnHideCallback(null);
        SamandaLauncher.OnHideScreen();

        SandboxSplash.SetActive(true);
    }

    void OnDynamicLink(object sender, EventArgs args)
    {
        var dynamicLinkEventArgs = args as ReceivedDynamicLinkEventArgs;
        Debug.LogFormat("Received dynamic link {0}",
                        dynamicLinkEventArgs.ReceivedDynamicLink.Url.OriginalString);
    }

}
