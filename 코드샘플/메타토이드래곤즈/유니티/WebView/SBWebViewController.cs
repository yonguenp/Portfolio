using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 웹뷰를 처리 하기 전에 해당 접근 디바이스의 종류를 먼저 판단함. 그리고 하위 객체에다가 생성 요청 추가.
/// 웹뷰 호출을 위한 중간 컨트롤러 역할.
/// 
/// (주의)webViewImage ->(생성 기준 부모) 해당 오브젝트는 무조건 '스트레치 모드 - 피벗(0.5,0.5)' 를 사용 할 것.
/// 
/// </summary>
/// 

namespace SandboxNetwork
{
    public class SBWebViewController : MonoBehaviour
    {
        [SerializeField] RectTransform webViewImage;
        [SerializeField] GameObject webviewBG;

        [SerializeField] string defaultURL;

        [SerializeField]
        SBWindowWebView windowWebView = null;

        [SerializeField]
        SBMobileWebView mobileWebview = null;
                
        SBWebView webView = null;
        bool isMobile = false;

        private void OnDisable()
        {
            CloseWebView();
        }

        public void Update()
        {
#if UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.Escape))
                CloseWebView();
#endif
        }
        public void OnWebView(string url = "", Action<string> cb = null, Action<string> started = null)
        {
            if (string.IsNullOrEmpty(url))
                url = defaultURL;

            isMobile = NetworkManager.Instance.IsMobile;
            
            InitWebView(url, cb, started);
        }

        void InitWebView(string url, Action<string> cb = null, Action<string> started = null)
        {
            webviewBG.SetActive(true);
            if (!isMobile)
            {
#if (UNITY_STANDALONE_WIN && !UNITY_EDITOR) || UNITY_EDITOR_WIN
                webView = windowWebView;
#else
                Debug.LogError("Error : not import webview");
#endif
            }
            else
            {
                webView = mobileWebview;
            }

            if (webView != null)
                webView.InitializeWebview(url, webViewImage.gameObject, cb, started);
        }

        public void EvaluateJS(string method)
        {
            if(webView != null)
                webView.EvaluateJS(method);
        }

        public void CloseWebView()
        {
            if (webView != null)
            {
                webView.CloseBrowser();
                webView = null;
            }

            if (webviewBG != null)
                webviewBG.SetActive(false);
        }

        public void ClearCache()
        {
            if(webView != null)
                webView.ClearCache();
        }
    }
}

