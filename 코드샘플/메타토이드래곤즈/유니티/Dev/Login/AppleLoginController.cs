using System.Text;
using UnityEngine;

namespace SandboxNetwork
{
#if UNITY_ANDROID && !UNITY_EDITOR

    using Firebase.Auth;
    using Firebase.Extensions;

    public partial class AppleLoginController : AppleLogin
    {
        public void OnAppleLogin()
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

            RequestAppleLoginToken();
        }

        void RequestAppleLoginToken()
        {
            isProceedLogin = true;

            loginResultData = new LoginData();

            FederatedOAuthProviderData providerData = new FederatedOAuthProviderData();
            providerData.ProviderId = "apple.com";

            FederatedOAuthProvider provider = new FederatedOAuthProvider();
            provider.SetProviderData(providerData);

            LoginManager.Instance.Auth.SignInWithProviderAsync(provider).ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled)
                {
                    Debug.LogError("SignInWithProviderAsync was canceled.");
                    return;
                }
                if (task.IsFaulted)
                {
                    Debug.LogError("SignInWithProviderAsync encountered an error: " +
                      task.Exception);
                    return;
                }

                CreateJWTTokenAndSignIn();
                //DisplaySignInResult(task.Result, 1);
            });
        }

        void CreateJWTTokenAndSignIn()
        {
            FirebaseUser user = LoginManager.Instance.Auth.CurrentUser;
            user.TokenAsync(true).ContinueWith(task =>
            {
                if (task.IsCanceled)
                {
                    Debug.LogError("TokenAsync was canceled.");
                    return;
                }

                if (task.IsFaulted)
                {
                    Debug.LogError("TokenAsync encountered an error: " + task.Exception);
                    return;
                }

                loginResultData.jwtToken = task.Result;
                //Debug.Log("CLIENT TOKEN ==> " + loginResultData.jwtToken);

                OnAppleSignIn();
            });
        }

        //void DisplaySignInResult(SignInResult result, int indentLevel)
        //{
        //    Debug.Log("-------DisplaySignInResult--------");
        //    string indent = new string(' ', indentLevel * 2);
        //    var metadata = result.Meta;
        //    if (metadata != null)
        //    {
        //        Debug.Log(string.Format("{0}Created: {1}", indent, metadata.CreationTimestamp));
        //        Debug.Log(string.Format("{0}Last Sign-in: {1}", indent, metadata.LastSignInTimestamp));
        //    }
        //    var info = result.Info;
        //    if (info != null)
        //    {
        //        Debug.Log(string.Format("{0}Additional User Info:", indent));
        //        Debug.Log(string.Format("{0}  User Name: {1}", indent, info.UserName));
        //        Debug.Log(string.Format("{0}  Provider ID: {1}", indent, info.ProviderId));
        //        Debug.Log(string.Format("{0}  Credential Provider: {1}", indent, info.UpdatedCredential.Provider));
        //    }
        //}
    }

#elif UNITY_IOS && !UNITY_EDITOR

    using AppleAuth;
    using AppleAuth.Enums;
    using AppleAuth.Interfaces;
    using AppleAuth.Extensions;

    public partial class AppleLoginController : AppleLogin
    {
        public void OnAppleLogin()
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
            Debug.Log("Apple - OnAppleLogin");
            RequestAppleLoginToken();
        }

        void RequestAppleLoginToken()
        {
            loginResultData = new LoginData();

            var loginArgs = new AppleAuthLoginArgs(LoginOptions.IncludeEmail | LoginOptions.IncludeFullName);
            LoginManager.Instance.AppleAuthManager_Instance.LoginWithAppleId(
                loginArgs,
                credential =>
                {
                    string identityToken = "";
                    // Obtained credential, cast it to IAppleIDCredential
                    var appleIdCredential = credential as IAppleIDCredential;
                    if (appleIdCredential != null)
                    {
                        identityToken = Encoding.UTF8.GetString(
                                        appleIdCredential.IdentityToken,
                                        0,
                                        appleIdCredential.IdentityToken.Length);


                    }
                    Debug.Log($"Apple - RequestAppleLoginToken - check - {identityToken}");
                    if (string.IsNullOrEmpty(identityToken))
                    {
                        return;
                    }

                    loginResultData.jwtToken = identityToken;
                    Debug.Log($"Apple - RequestAppleLoginToken - check ok - {identityToken}");
                    OnAppleSignIn();
                },
                error =>
                {
                    // Something went wrong
                    var authorizationErrorCode = error.GetAuthorizationErrorCode();
                    Debug.Log("Apple - RequestAppleLoginToken ERROR");
                });
        }

        private void Update()
        {
            if (LoginManager.Instance.AppleAuthManager_Instance != null)
            {
                LoginManager.Instance.AppleAuthManager_Instance.Update();
            }
        }
    }
#else
    public partial class AppleLoginController : AppleLogin
    {
        public void OnAppleLogin() 
        {
            SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100002672), StringData.GetStringByIndex(100000199), StringData.GetStringByIndex(100000200));
        }

        //public ePlatformLoginProcessState OnGoogleLogin()
        //{
        //    OAuthApp.MainWindow win = new OAuthApp.MainWindow();
        //    win.button_Click(OnGoogleSignIn);
        //}
    }
#endif
}
