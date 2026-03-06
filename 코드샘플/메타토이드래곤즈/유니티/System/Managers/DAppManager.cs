using Newtonsoft.Json.Linq;
using SandboxNetwork;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if USE_WEMIX
using Wemade.Wemix;
using Wemade.Wemix.Consts;
using Wemade.Wemix.Settings;
#endif
public class DAppManager : IManagerBase
{

    enum Env {
        DEV,
        STAGE,
        LIVE
    }

    Env curEnv = Env.STAGE;

    int WEMIX_MAINNET {
        get{
            switch(curEnv)
            {
                case Env.DEV:
                    return 1112;
                case Env.STAGE:
                    return 1112;
                case Env.LIVE:
                default:
                    return 1111;
            }
        }
    }//live 1111, dev,stage 1112


    private static DAppManager instance = null;
    DAppWebBrowserPopup popup = null;

    public static DAppManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new DAppManager();
            }
            return instance;
        }
    }

    public void Initialize()
    {

    }
    public void Update(float dt)
    {

    }

    public void ClearDApp()
    {
        if (popup != null)
        {
            popup.ClearCache();
        }

        CloseDAppWebView();

        popup = null;
#if USE_WEMIX
        if (Wemix.IsInitialized && Wemix.IsConnected)
        {
            Wemix.Disconnect((error) =>
            {
                if (error == null)
                {
                    Debug.LogError("wallet_disconnect");
                }
                else
                {
                    Debug.LogError("wallet_disconnect_fail");
                }
            });
        }
#endif
    }
    public JObject MakeParam()
    {
        JObject param = new JObject();
        //param.Add("server_tag", SBGameManager.CurServerTag);

        return param;
    }
    public void OpenDAppNoticePage(int page_key = 0, Action _closeCallback = null)
    {
        JObject param = MakeParam();

        param.Add("openpage", "notice");//이벤트 페이지 호출 파라미터
        param.Add("openkey", page_key);

        OpenDAppWebView(param, _closeCallback);
    }
    public void OpenDAppEventPage(int page_key = 0, Action _closeCallback = null)
    {
        JObject param = MakeParam();

        param.Add("openpage", "event");//이벤트 페이지 호출 파라미터
        param.Add("openkey", page_key);

        OpenDAppWebView(param, _closeCallback);
    }

    public void OpenDAppChampionPage(Action _closeCallback = null)
    {
        JObject param = MakeParam();

        param.Add("openpage", "champion");//이벤트 페이지 호출 파라미터

        OpenDAppWebView(param, _closeCallback);
    }

    public void OpenDAppWithEquipment(int tag, Action _closeCallback = null)
    {
        var partData = User.Instance.PartData.GetPart(tag);
        if (partData == null)
            return;
        
        if (!GameConfigTable.GEM_TO_NFT)
        {
            SystemPopup.OnSystemPopup(StringData.GetStringByStrKey("안내"), StringData.GetStringByStrKey("추후제공예정"));
            return;
        }

        if (partData.LinkDragonTag > 0)
        {
            SystemPopup.OnSystemPopup(StringData.GetStringByStrKey("안내"), StringData.GetStringByStrKey("착용장비변환불가"));
            return;
        }

        SystemPopup.OnSystemPopup(StringData.GetStringByStrKey("안내"), StringData.GetStringByStrKey("장비NFT변환안내"), StringData.GetStringByStrKey("확인"), StringData.GetStringByStrKey("취소"), () =>
        {
            WWWForm param = new WWWForm();
            param.AddField("e_tag", tag.ToString());
            NetworkManager.Send("part/exportnft", param, (data) => { _closeCallback?.Invoke(); }, (fail)=> {
                JObject root = null;
                if (!string.IsNullOrEmpty(fail))
                    root = JObject.Parse(fail);
                if (root != null && root.ContainsKey("rs"))
                {
                    switch ((eApiResCode)root["rs"].Value<int>())
                    {
                        case eApiResCode.PART_LOCKED:
                            SystemPopup.OnSystemPopup(StringData.GetStringByStrKey("안내"), StringData.GetStringByStrKey("잠금장비변환불가"));
                            break;
                        case eApiResCode.PART_EQUIPPED:
                            SystemPopup.OnSystemPopup(StringData.GetStringByStrKey("안내"), StringData.GetStringByStrKey("착용장비변환불가"));
                            break;
                    }
                }
            });
        },
        () => { });
    }
    public void OpenDAppWithItem(int itemNo, int amount, Action _closeCallback = null)
    {
        if (!GameConfigTable.SKILLCUBE_TO_NFT)
        {
            SystemPopup.OnSystemPopup(StringData.GetStringByStrKey("안내"), StringData.GetStringByStrKey("추후제공예정"));
            return;
        }

        SystemPopup.OnSystemPopup(StringData.GetStringByStrKey("안내"), StringData.GetStringByStrKey("큐브NFT변환안내"), StringData.GetStringByStrKey("확인"), StringData.GetStringByStrKey("취소"), () =>
        {
            WWWForm param = new WWWForm();
            param.AddField("ino", itemNo.ToString());
            param.AddField("count", amount.ToString());
            NetworkManager.Send("item/exportnft", param, (data) => { _closeCallback?.Invoke(); });
        },
        () => { });        
    }
    public void OpenDAppWebView(JObject param = null, Action _closeCallback = null, Action<string> _startCallback = null)
    {
        CloseDAppWebView();

        NetworkManager.Send("auth/reqtempsid", null, (res) => {           
            if (res.ContainsKey("rs"))
            {
                if (res["rs"].Value<int>() == 0)
                {
                    if (res.ContainsKey("temp_sid"))
                    {
                        AppsFlyerSDK.AppsFlyer.sendEvent("visit_dapp", new Dictionary<string, string>());

                        popup = DAppWebBrowserPopup.OnWebView(res["temp_sid"].ToString(), param, OnRecvDAppMessage, _closeCallback, _startCallback);

                        return;
                    }
                }
            }

            ToastManager.On(StringData.GetStringByStrKey("DAPP인증오류"));
        });        
    }

    public void OpenDAppArtBlock()
    {
        CloseDAppWebView();

        NetworkManager.Send("auth/reqtempsid", null, (res) => {            
            if (res.ContainsKey("rs"))
            {
                if (res["rs"].Value<int>() == 0)
                {
                    if (res.ContainsKey("temp_sid"))
                    {
                        popup = DAppWebBrowserPopup.OnArtBlockWebView(res["temp_sid"].ToString());

                        return;
                    }
                }
            }

            ToastManager.On(StringData.GetStringByStrKey("DAPP인증오류"));
        });
    }
    
    public void OnScreenChanged()
    {
        JObject json = MakeParam();
        json["width"] = Screen.width;
        json["height"] = Screen.height;
        json["safe_area"] = Screen.safeArea.ToString();

        OnDAppSendMessage("screen_info", json.ToString());
    }
    public void CloseDAppWebView()
    {
        if (popup != null)
            popup.ClosePopup();
    }

    void OnDAppSendMessage(string key, string param)
    {
        JObject json = MakeParam();
        json["key"] = key;
        json["param"] = param;

        OnDAppSendMessage(json.ToString());
    }

    void OnDAppSendMessage(string jsonvalue)
    {
        if(popup != null)
            popup.EvaluateJS("window.DApp.call(" + jsonvalue + ")");
    }


    void OnRecvDAppMessage(string msg)
    {
        if (string.IsNullOrEmpty(msg))
        {
            Debug.LogError("DApp Message is empty.");
            return;
        }


        string key = msg;
        JObject param = new JObject();
        try
        {
            JObject json = JObject.Parse(msg);
            if(json.ContainsKey("key"))
            {
                key = json["key"].Value<string>();

                if(key == "push")//푸시는 특수하게 처리
                {
                    if (json["param"].Type == JTokenType.Array)
                    {
                        param.Add("push", json["param"]);
                    }
                } 
                else if(json.ContainsKey("param"))
                {
                    if (json["param"].Type == JTokenType.String)
                        param = JObject.Parse(json["param"].Value<string>());
                    else
                        param = (JObject)json["param"];
                }
            }
        }
        catch
        {
            Debug.LogError("JSON Parse error : " + msg);
        }

        switch (key)
        {
            case "is_connect":
#if USE_WEMIX
                OnDAppSendMessage("is_connect", Wemix.IsConnected.ToString());
#endif
                break;
            case "connect":
            case "get_address":
                OnWemixWalletEvent((adress) => { OnDAppSendMessage("connect", adress); });
                break;
            case "force_connect":
                WalletForcedConnect((adress) => { OnDAppSendMessage("connect", adress); });
                break;
            case "sign_tx":
            {
                string tx = "";
                if (param.ContainsKey("tx"))
                    tx = param["tx"].Value<string>();
#if USE_WEMIX
                if (Wemix.IsInitialized && Wemix.IsConnected)
                {
                    if(string.IsNullOrEmpty(tx))
                    {
                        ToastManager.On("dapp_param_error");
                        return;
                    }

                    ExecuteSignTransaction(tx);
                }
                else
#endif
                ToastManager.On("wemix_not_conneted");
            }
            break;
            case "disconnect":
                ClearDApp();
                break;
            case "sign_msg":
            {
                string hash = "";
                if (param.ContainsKey("hash"))
                    hash = param["hash"].Value<string>();
#if USE_WEMIX
                if (Wemix.IsInitialized && Wemix.IsConnected)
                {
                    if (string.IsNullOrEmpty(hash))
                    {
                        ToastManager.On("dapp_param_error");
                        return;
                    }

                    ExecuteWemixSign(hash);
                }
                else
#endif
                    ToastManager.On("wemix_not_conneted");
            }
            break;
            case "push":
            {
                //Debug.LogError("dapp push : " + param);
                NetworkManager.Instance.PushResponse(param);
            }
            break;
            case "close":
                CloseDAppWebView();
                break;
            case "log":
                Debug.LogError("log : " + param);
                break;
            case "openexternal":
            {
                Debug.LogError("dapp open external : " + param);
                if(param.ContainsKey("url"))
                    Application.OpenURL(param["url"].Value<string>());
            }break;
            case "notice_btn_click":
            {
                if (param.ContainsKey("action") && param.ContainsKey("param"))
                {
                    eActionType actionType = (eActionType)param["action"].Value<int>();

                    SBFunc.InvokeCustomAction(actionType, param["param"].Value<string>());
                }
            }
            break;
            case "mail_check":
            {
                if (!ReddotManager.IsOn(eReddotEvent.POST_MAIL))
                {
                    var postPopup = PopupManager.GetPopup<PostListPopup>();
                    if (postPopup != null)
                        postPopup.PostForceUpdate();
                }
            }break;
            case "set_time_value":
            {
                if (param.ContainsKey("today_alarm_clicked"))
                {
                    if (param["today_alarm_clicked"].Value<bool>())
                    {
                        SBFunc.SetTimeValue("Announcement");
                    }
                    else
                    {
                        CacheUserData.DeleteKey("Announcement");
                    }
                }
            }
            break;
        }
    }

    public void OnWemixWalletEvent(Action<string> callback)
    {
#if USE_WEMIX
        if (!Wemix.IsInitialized)
        {
            string wemixHost = "";
            string webUrl = "";

            switch (curEnv)
            {
                case Env.DEV:
                    webUrl = "https://dev-relay.wemixnetwork.com";
                    wemixHost = "https://dev-oauth.wemixnetwork.com";
                    break;
                case Env.STAGE:
                    webUrl = "https://stg-relay.wemixnetwork.com";
                    wemixHost = "https://stg-oauth.wemixnetwork.com";
                    break;
                    case Env.LIVE:
                    webUrl = "https://relay.wemixnetwork.com";
                    wemixHost = "https://oauth.wemixnetwork.com";

                    break;
            }

            //Debug.Log("webUrl : " + webUrl);
            //Debug.Log("wemixHost : " + wemixHost);

            WemixRequest.WemixConfig config = new WemixRequest.WemixConfig(wemixHost, webUrl, true, true, false);
            config.addSupportWallet(WemixRequest.WalletType.PLAY_WALLET);
            config.addSupportWallet(WemixRequest.WalletType.WEMIX_PLAY_APP);

            Wemix.Initialize(
                config,
                (error) =>
                {
                    if (error == null)
                    {
                        switch (GamePreference.Instance.GameLanguage)
                        {
                            case SystemLanguage.Japanese:
                                Wemix.SetLanguage(WemixLanguage.JAPANESE);
                                break;
                            case SystemLanguage.English:
                                Wemix.SetLanguage(WemixLanguage.ENGLISH);
                                break;
                            case SystemLanguage.Korean:
                                Wemix.SetLanguage(WemixLanguage.KOREAN);
                                break;
                            case SystemLanguage.Portuguese:
                                Wemix.SetLanguage(WemixLanguage.PORTUGUESE);
                                break;
                            case SystemLanguage.ChineseSimplified:
                                Wemix.SetLanguage(WemixLanguage.CHINESE_SIMPLIFIED);
                                break;
                            case SystemLanguage.ChineseTraditional:
                                Wemix.SetLanguage(WemixLanguage.CHINESE_TRADITIONAL);
                                break;
                            default:
                                Wemix.SetLanguage(WemixLanguage.ENGLISH);
                                break;
                        }

                        Debug.Log("[Unity] Successly Initialize");

                        WalletConnect(callback);
                    }
                    else
                    {
                        Debug.Log("[Unity] Initialize retun error");
                        ToastManager.On("wallet_connect_error");
                    }
                }
            );
        }
        else
        {
            if (Wemix.IsConnected)
            {
                callback?.Invoke(Wemix.Address);
            }
            else
            {
                WalletConnect(callback);
            }
        }
#endif
    }

    private void WalletConnect(Action<string> callback)
    {
#if USE_WEMIX
        Debug.Log("WEMIX_MAINNET : " + WEMIX_MAINNET);

        Wemix.Connect(WEMIX_MAINNET, (data, error) =>
            {
                if (error == null)
                {
                    callback?.Invoke(data.address);
                }
                else
                {
                    Debug.Log("[Unity] Connect retun error :" + error.code + ", " + error.message);
                    ToastManager.On("wallet_connect_error");
                }
            }
        );
#endif
    }

    private void WalletForcedConnect(Action<string> callback)
    {
#if USE_WEMIX
        Wemix.ForcedConnect(WEMIX_MAINNET, (data, error) =>
            {
                if (error == null)
                {
                    callback?.Invoke(data.address);
                }
                else
                {
                    Debug.Log("[Unity] ForcedConnect retun error");
                    ToastManager.On("wallet_connect_error");
                }
            }
        );
#endif
    }

    void OnApplicationFocus(bool hasFocus)
    {
        if(hasFocus)
            OnDAppSendMessage("application_focused");
    }

    private void ExecuteWemixSign(string hash)
    {
        Debug.Log($"[WEMIX SDK] hash:\n{hash}");
#if USE_WEMIX
        Wemix.SignMessage(hash, (result, error) =>
        {
            if (error == null)
            {
                Debug.Log($"Signed\nsignature: {result.signature}\nrawTx: {result.rawTx}");
                ToastManager.On("wallet_sign");

                JObject data = MakeParam();
                data["signature"] = result.signature;
                data["rawtx"] = result.rawTx;

                OnDAppSendMessage("sign_msg", data.ToString());
            }
            else
            {
                Debug.Log($"Error\ncode: {error.code}\nmessage: {error.message}");
                ToastManager.On("wallet_connect_error");
                OnDAppSendMessage("sign_msg_error", error.message);
            }
        });
#endif
    }
    public void ExecuteSignTransaction(string txData)
    {
#if USE_WEMIX
        WemixRequest.TransactionData tx_data = JsonUtility.FromJson<WemixRequest.TransactionData>(txData);
        Wemix.SignTransaction(
            tx_data,
            (result, error) =>
            {
                if (error == null)
                {
                    Debug.Log(result);
                    Debug.Log("requestSignedBlock('" + Wemix.Address + "', '" + tx_data.hash + "', '" + result.signature + "')");

                    JObject data = MakeParam();
                    data["adress"] = Wemix.Address;
                    data["hash"] = tx_data.hash;
                    data["signature"] = result.signature;
                    data["txdata"] = txData;

                    OnDAppSendMessage("sign_tx", data.ToString());
                }
                else
                {
                    Debug.Log("failed");
                    ToastManager.On("wallet_connect_error");

                    OnDAppSendMessage("sign_tx_error ", error.code.ToString());
                }
            }
        );
#endif
    }

}
