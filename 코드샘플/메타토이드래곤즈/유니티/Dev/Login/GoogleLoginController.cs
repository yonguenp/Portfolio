using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
#if UNITY_ANDROID && !UNITY_EDITOR

    using Google;
    public partial class GoogleLoginController : GoogleLogin
    {
        const string WEB_CLIENT_ID = "405423334723-46glrq8gjjkt7vbod0rbkqtre6q52qo6.apps.googleusercontent.com";

        public override void OnGoogleLogin()
        {
            if (LoginManager.Instance.isGoogleLoginInitialized == false)
            {
                // init이 완료되지 않음
                return;
            }

            if (isProceedLogin)
            {
                // 현재 로그인 시도중
                return;
            }

            RequestGoogleLoginToken(WEB_CLIENT_ID);
        }

        public override void ClearGoogleInstance()
        {
            GoogleSignIn.DefaultInstance.SignOut();
        }
    }

#elif UNITY_IOS && !UNITY_EDITOR

    using Google;
    public partial class GoogleLoginController : GoogleLogin
    {
        const string WEB_CLIENT_ID = "405423334723-46glrq8gjjkt7vbod0rbkqtre6q52qo6.apps.googleusercontent.com";

        public override void OnGoogleLogin()
        {
            if (LoginManager.Instance.isAppleLoginInitialized == false)
            {
                // init이 완료되지 않음
                return;
            }

            if (isProceedLogin)
            {
                // 현재 로그인 시도중
                return;
            }

            RequestGoogleLoginToken(WEB_CLIENT_ID);
        }

        public override void ClearGoogleInstance()
        {
            GoogleSignIn.DefaultInstance.SignOut();
        }
    }
#else
    public partial class GoogleLoginController : GoogleLogin
    {
        public override void OnGoogleLogin() 
        {
            //SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100002672), StringData.GetStringByIndex(100000199), StringData.GetStringByIndex(100000200));
            isProceedLogin = true;
            OAuthApp.MainWindow win = new OAuthApp.MainWindow();
            win.button_Click(OnGoogleSignIn);
        }

        public override void ClearGoogleInstance()
        {
            //GoogleSignIn.DefaultInstance.SignOut();
        }

        //public ePlatformLoginProcessState OnGoogleLogin()
        //{
        //    OAuthApp.MainWindow win = new OAuthApp.MainWindow();
        //    win.button_Click(OnGoogleSignIn);
        //}
    }
#endif
}