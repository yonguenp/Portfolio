using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace SandboxNetwork
{
    public class WebViewPopup : Popup<WebViewPopupData>
    {
        [SerializeField]
        SBWebViewController webviewController = null;
        [SerializeField]
        GameObject closeBtn = null;
        public override void InitUI()
        {
            if (Data.Url == string.Empty)
                ClosePopup();
            closeBtn.SetActive(true);
            PopupManager.Instance.Top.HideEvent();
            SetWebView(Data.Url);
        }

        void SetWebView(string url = "")
        {
            if (webviewController != null)
            {
                webviewController.CloseWebView();
                webviewController.gameObject.SetActive(true);
                webviewController.OnWebView(url);
            }
        }

        public override void ClosePopup()
        {
            closeBtn.SetActive(false);
            base.ClosePopup();
        }
    }

}
