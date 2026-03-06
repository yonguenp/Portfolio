using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SandboxPlatform.SAMANDA;
using Firebase.Auth;
using Google;
using System.Threading.Tasks;
using System;
#if UNITY_IOS && !UNITY_EDITOR
using OAuthApp;
using UnityEngine.SignInWithApple;
#endif

public class SAMANDA_IOS : MonoBehaviour
{
    public string appleIdToken;
    private string rawNonce = null;

    private bool isInit = false;
    private bool isLogin = false;

    private GoogleSignInConfiguration configuration;


    protected static SAMANDA_IOS _instance;
    public static SAMANDA_IOS Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<SAMANDA_IOS>();
                if (_instance == null)
                {
                    GameObject obj = new GameObject();
                    obj.name = typeof(SAMANDA_IOS).Name;
                    _instance = obj.AddComponent<SAMANDA_IOS>();
                }
            }
            return _instance;
        }
    }
    private GameObject mSignInWithAppleObject;

#if UNITY_IOS && !UNITY_EDITOR
    SignInWithApple GetSignInWithApple()
    {
        if (null == mSignInWithAppleObject)
        {
            mSignInWithAppleObject = new GameObject();
            mSignInWithAppleObject.AddComponent<SignInWithApple>();
        }

        return mSignInWithAppleObject.GetComponent<SignInWithApple>();
    }

    private void StartSignInWithApple()
    {
        var siwa = GetSignInWithApple();
        siwa.Login(OnSignInWithAppleLogin);
    }

    public void OnSignInWithAppleLogin(SignInWithApple.CallbackArgs args)
    {
        if (args.error == null)
        {
            Debug.Log("SignInWithApple Succeeded");
            SendSignInWithAppleResult(0, args.userInfo.idToken);

            TryFriebase();
        }
        else
        { 
            Debug.Log("SignInWithApple Failed with : " + args.error);
            SendSignInWithAppleResult(1, "");
        }
    }

    private void SendSignInWithAppleResult(int error, string token)
    {
        oAuthSendForSAMANDA data = new oAuthSendForSAMANDA();
        data.rs = error.ToString();
        data.type = AUTH_TYPE.AP;
        data.token = token;
        appleIdToken = token;
        SAMANDA.Instance.OnOAuthJavaResponse(data);
    }
#endif
    /// <summary>
    /// IOS 에서 로그인 신청
    /// </summary>
    public void SAMANDAWithApple(Action<bool> callback = null)
    {
#if UNITY_IOS && !UNITY_EDITOR
        //Debug.Log("Apple oAuth request. unity -> iOS");
        //SamandaNativeoAuthApple(SamandaUnityoAuthCallback);
        StartSignInWithApple();
#endif
        
        if (isLogin)
            callback?.Invoke(true);
        else
            callback?.Invoke(false);
    }
    /// <summary>
    /// 구글에서 IOS로 연결 
    /// </summary>
    public void GoogleToApple()
    {
#if UNITY_IOS && !UNITY_EDITOR
        //UnityStartGoogleSignInIOS();
        //SamandaNativeoAuthGoogle(SamandaUnityoAuthCallback);
#endif

    }
    public void AppleToGoogle(Action<bool> callback = null)
    {
        Init();
        OnSignIn();

        if (isLogin)
            callback?.Invoke(true);
        else
            callback?.Invoke(false);
    }

    /// <summary>
    /// ios 에서 Apple 로그인 인증
    /// </summary>
    /// <returns></returns>
    IEnumerator TryFriebase()
    {
        if (string.IsNullOrEmpty(string.Empty))
        {
            yield return null;
        }
        FirebaseAuth auth = FirebaseAuth.DefaultInstance;

        Credential credential =
            OAuthProvider.GetCredential("apple.com", appleIdToken, rawNonce, null);
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
            isLogin = true;
        });
    }


    #region IOSTOGoogle Login
    public void Init()
    {
        if (isInit)
            return;

        configuration = new GoogleSignInConfiguration
        {
            RequestIdToken = true
        };
        isInit = true;
    }
    public void OnSignIn()
    {
        GoogleSignIn.Configuration = configuration;
        GoogleSignIn.Configuration.UseGameSignIn = false;
        GoogleSignIn.Configuration.RequestIdToken = true;
        Debug.Log("Calling SignIn");

        GoogleSignIn.DefaultInstance.SignIn().ContinueWith(
          OnAuthenticationFinished);
    }

    public void OnSignOut()
    {
        Debug.Log("Calling SignOut");
        GoogleSignIn.DefaultInstance.SignOut();

        FirebaseAuth auth = FirebaseAuth.DefaultInstance;
        auth.SignOut();
    }

    public void OnDisconnect()
    {
        Debug.Log("Calling Disconnect");
        GoogleSignIn.DefaultInstance.Disconnect();
        isLogin = false;
    }

    internal void OnAuthenticationFinished(Task<GoogleSignInUser> task)
    {
        if (task.IsFaulted)
        {
            using (IEnumerator<System.Exception> enumerator =
                    task.Exception.InnerExceptions.GetEnumerator())
            {
                if (enumerator.MoveNext())
                {
                    GoogleSignIn.SignInException error =
                            (GoogleSignIn.SignInException)enumerator.Current;
                    Debug.Log("Got Error: " + error.Status + " " + error.Message);
                }
                else
                {
                    Debug.Log("Got Unexpected Exception?!?" + task.Exception);
                }
            }
        }
        else if (task.IsCanceled)
        {
            Debug.Log("Canceled");
        }
        else
        {
            Debug.Log("Welcome: " + task.Result.DisplayName + "!");
            isLogin = true;
        }
    }

    public void OnSignInSilently()
    {
        GoogleSignIn.Configuration = configuration;
        GoogleSignIn.Configuration.UseGameSignIn = false;
        GoogleSignIn.Configuration.RequestIdToken = true;
        Debug.Log("Calling SignIn Silently");

        GoogleSignIn.DefaultInstance.SignInSilently()
              .ContinueWith(OnAuthenticationFinished);
    }

    public void OnGamesSignIn()
    {
        GoogleSignIn.Configuration = configuration;
        GoogleSignIn.Configuration.UseGameSignIn = true;
        GoogleSignIn.Configuration.RequestIdToken = false;

        Debug.Log("Calling Games SignIn");

        GoogleSignIn.DefaultInstance.SignIn().ContinueWith(
          OnAuthenticationFinished);
    }
    #endregion
}
