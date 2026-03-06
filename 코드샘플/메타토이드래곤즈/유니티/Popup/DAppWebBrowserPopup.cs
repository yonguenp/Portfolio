using SandboxNetwork;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DAppWebBrowserPopup : Popup<PopupData>
{
    [SerializeField]
    SBWebViewController webviewController = null;
    ScreenOrientation orientation;
    public static DAppWebBrowserPopup OnWebView(string sid, Newtonsoft.Json.Linq.JObject param = null, Action<string> cb = null, Action closeCallback = null, Action<string> started = null)
    {   
        DAppWebBrowserPopup popup = PopupManager.OpenPopup<DAppWebBrowserPopup>();
        popup.SetExitCallback(closeCallback);
        popup.SetURL(GetURL(sid, param), cb, started);

        return popup;
    }

    public static DAppWebBrowserPopup OnArtBlockWebView(string sid)
    {
        DAppWebBrowserPopup popup = PopupManager.OpenPopup<DAppWebBrowserPopup>();
        popup.SetExitCallback(null);
        popup.SetURL(GetURL(sid, null, "artblock"));

        return popup;
    }

    public static string GetURL(string sid = "", Newtonsoft.Json.Linq.JObject param = null, string subUrl = "")
    {
        string url = "https://webview.metatoydragonz.io";
                
        if (!string.IsNullOrEmpty(subUrl))
            url += "/" + subUrl;

        url += string.Format("?width={0}&height={1}&safe_area={2}&user_no={3}&user_name={4}&champion={5}&web2={6}", Screen.width, Screen.height, Screen.safeArea.ToString(), NetworkManager.Instance.UserNo, NetworkManager.Instance.NickName, GameConfigTable.IsChampionActive() ? 1 : 0, !User.Instance.ENABLE_P2E ? 1 : 0);

        if(!string.IsNullOrEmpty(sid))
        {
            url += "&temp_sid=" + sid;
        }

        url += "&is_dev=" + (NetworkManager.IsLiveServer ? "0" :
#if SB_TEST
        NetworkManager.IsQAServer ? "2" :
#endif
        "1");

        url += "&server_tag=" + SBGameManager.CurServerTag;

        if (param != null)
        {
            foreach (var val in param.Properties())
            {
                var key = val.Name;
                var value = val.Value;

                url += "&" + key + "=" + value;
            }
        }

        bool bOnWeb3Event = false;
        foreach (var eventData in User.Instance.EventData.GetActiveEvents(false))
        {
            if(eventData.TYPE == eActionType.OPEN_EVENT || eventData.TYPE == eActionType.UNIONRAID_RANKING || eventData.TYPE == eActionType.CHAMPIONEVENT_RANKING)
            {
                int remain = User.Instance.EventData.GetRemainTime(eventData);
                if (remain > 0)
                {
                    bOnWeb3Event = true;
                    break;
                }
            }
        }

        if (bOnWeb3Event)
            url += "&web3event=1";

        return url;
    }
    public void SetURL(string url, Action<string> cb = null, Action<string> started = null)
    {
        orientation = Screen.orientation;
        //if (UICanvas.Instance != null)
        //{
        //    RectTransform canvasRect = UICanvas.Instance.GetCanvasRectTransform();
        //    WebviewArea.localPosition = new Vector3(250.0f * 0.5f, 0.0f, 0.0f);
        //    WebviewArea.sizeDelta = canvasRect.sizeDelta - new Vector2(250.0f, 0.0f);
        //}
        webviewController.OnWebView(url, cb, started);
    }

    public void EvaluateJS(string method)
    {
        webviewController.EvaluateJS(method);
    }

    public override void InitUI()
    {
        UICanvas.Instance.StartBackgroundBlurEffect();
    }

    public override void BackButton()
    {
        ClosePopup();
    }

    public override void ClosePopup()
    {
        webviewController.CloseWebView();
        base.ClosePopup();

        UICanvas.Instance.EndBackgroundBlurEffect();
    }

    public void ClearCache()
    {
        webviewController.ClearCache();
    }

    private void OnRectTransformDimensionsChange()
    {
        // Check for an Orientation Change
        ScreenOrientation curOri = Screen.orientation;
        switch (curOri)
        {
            case ScreenOrientation.Unknown: // Ignore
            {
                break;
            }
            default:
            {
                if (orientation != curOri)
                {
                    orientation = curOri;
                    DAppManager.Instance.OnScreenChanged();
                }
                break;
            }
        }
    }
}
