#if UNITY_ANDROID && !UNITY_EDITOR
using GooglePlayGames.BasicApi;
using GooglePlayGames;
#endif
using Firebase.Auth;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SAMANDA_ANDROID : MonoBehaviour
{
    private FirebaseAuth auth;
    private string authCode;
    private bool isInit = false;

    public string googleIdToken = null;
    public string googleAccessToken = null;

    protected static SAMANDA_ANDROID _instance;
    public static SAMANDA_ANDROID Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<SAMANDA_ANDROID>();
            }
            return _instance;
        }
    }
    private void Init()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
            PlayGamesClientConfiguration config = new PlayGamesClientConfiguration.Builder()
            .AddOauthScope("profile")
            .RequestServerAuthCode(false)
            .RequestIdToken()
            .Build();

            PlayGamesPlatform.InitializeInstance(config);
            PlayGamesPlatform.DebugLogEnabled = true;
            PlayGamesPlatform.Activate();
            Debug.Log("PlayGamesPlatform Connect!!!");
#endif
        auth = FirebaseAuth.DefaultInstance;
        isInit = true;
    }

    public void Login(Action<int> callback = null)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (!isInit)
        {
            Debug.Log("Already inti!!");
            Init();
        }

        if (!Social.localUser.authenticated)
        // 로그인 되어 있지 않다면 
        {
            Social.localUser.Authenticate((success, error) =>
            {
                if (success) // 성공하면 
                {
                    googleIdToken = ((PlayGamesPlatform)Social.Active).GetIdToken();
                    MonoBehaviour.print(googleIdToken);
                    callback?.Invoke(0);
                    Debug.Log($"google game service Success");
                    authCode = PlayGamesPlatform.Instance.GetServerAuthCode();
                    TryFirebaseLogin();
                }
                else // 실패하면 
                {
                    callback?.Invoke(1);
                    Debug.Log($"google game service Fail, Error :{error}");
                }
            });
        }
        else
        {
            googleIdToken = ((PlayGamesPlatform)Social.Active).GetIdToken();
            callback?.Invoke(2);
            Debug.Log("Already LogIn");
        }
#endif
    }
    public void Logout(Action<bool> callback = null)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (Social.localUser.authenticated)// 로그인 되어 있다면 
        {
            PlayGamesPlatform.Instance.SignOut(); // Google 로그아웃 
            callback?.Invoke(true);
            Debug.Log("LogOutComplete");

            //Firebase LogOut
            auth.SignOut();
            isInit = false;
        }
        else
        {
            callback?.Invoke(false);
            Debug.Log("Did not LogIn");
        }
#endif
    }

    public void TryFirebaseLogin()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        FirebaseAuth auth = FirebaseAuth.DefaultInstance;
        Credential credential = PlayGamesAuthProvider.GetCredential(authCode);
        auth.SignInWithCredentialAsync(credential).ContinueWith(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogError("SignInWithCredentialAsync was canceled.");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError("SignInWithCredentialAsync encountered an error: " + task.Exception);
                return;
            }

            FirebaseUser newUser = task.Result;
            Debug.LogFormat("User signed in successfully: {0} ({1})",
                newUser.DisplayName, newUser.UserId);
        });


        /*
         사용자가 처음으로 로그인하면 신규 사용자 계정이 생성되어 사용자의 Play 게임 ID에 연결됩니다. 이 신규 계정은 Firebase 프로젝트에 저장되며 프로젝트의 모든 앱에서 사용자 식별에 사용할 수 있습니다.
         게임에서 Firebase.Auth.FirebaseUser 객체로부터 사용자의 Firebase UID를 가져올 수 있습니다.
        */
        FirebaseUser user = auth.CurrentUser;
        if (user != null)
        {
            string playerName = user.DisplayName;

            // The user's Id, unique to the Firebase project.
            // Do NOT use this value to authenticate with your backend server, if you
            // have one; use User.TokenAsync() instead.
            string uid = user.UserId;
        }
#endif
    }


    public void TryFirebaseAppleLogin(Action<bool> callback = null)
    {
        if (!isInit)
        {
            Debug.Log("Already inti!!");
            Init();
        }

        FederatedOAuthProviderData providerData = new FederatedOAuthProviderData();
        providerData.ProviderId = "apple.com";

        providerData.CustomParameters = new Dictionary<string, string>();
        providerData.CustomParameters.Add("language", "ko");

        // Construct a FederatedOAuthProvider for use in Auth methods.
        FederatedOAuthProvider provider = new FederatedOAuthProvider();
        provider.SetProviderData(providerData);

        auth.SignInWithProviderAsync(provider).ContinueWith(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogError("SignInWithProviderAsync was canceled.");
                callback?.Invoke(false);
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError("SignInWithProviderAsync encountered an error: " +
                  task.Exception);

                callback?.Invoke(false);
                return;
            }

            SignInResult signInResult = task.Result;
            FirebaseUser user = signInResult.User;
            Debug.LogFormat("User signed in successfully: {0} ({1})",
                user.DisplayName, user.UserId);

            callback?.Invoke(true);
        });
    }

    public void WebGoogleLogin(string id_token)
    {
        googleAccessToken = id_token;
        auth = FirebaseAuth.DefaultInstance;

        Credential credential =
        GoogleAuthProvider.GetCredential(googleIdToken, googleAccessToken);
        auth.SignInWithCredentialAsync(credential).ContinueWith(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogError("SignInWithCredentialAsync was canceled.");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError("SignInWithCredentialAsync encountered an error: " + task.Exception);
                return;
            }

            FirebaseUser newUser = task.Result;
            Debug.LogFormat("User signed in successfully: {0} ({1})",
                newUser.DisplayName, newUser.UserId);


            FirebaseUser user = auth.CurrentUser;
            if (user != null)
            {
                string name = user.DisplayName;
                string email = user.Email;
                System.Uri photo_url = user.PhotoUrl;
                // The user's Id, unique to the Firebase project.
                // Do NOT use this value to authenticate with your backend server, if you
                // have one; use User.TokenAsync() instead.
                string uid = user.UserId;
            }
        });

    }
}
