#if UNITY_EDITOR_WIN || UNITY_EDITOR_OSX || (UNITY_STANDALONE_WIN && !UNITY_EDITOR) || (UNITY_STANDALONE_OSX && !UNITY_EDITOR)               
using OAuthApp;
#endif
using SandboxPlatform.SAMANDA;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SAMANDA_Login : MonoBehaviour
{
    public void SetUI(bool enable)
    {
        var instance = SAMANDA.Instance;

        if (transform.Find("GoogleButton") != null && 
            transform.Find("AppleButton") != null && 
            transform.Find("GuestButton") != null)
        {
            transform.Find("GoogleButton").gameObject.SetActive(instance._googleLogin);
            transform.Find("AppleButton").gameObject.SetActive(instance._appleLogin);
            transform.Find("GuestButton").gameObject.SetActive(instance._guestLogin);
        }
        //#if !UNITY_IOS
        //        transform.Find("AppleButton").gameObject.SetActive(false);
        //#endif
        SetActive(enable);
    }

    public void SetActive(bool enable)
    {
        gameObject.SetActive(enable);
    }

    #region LoginButton Event
    public void OnLoginButton(int type)
    {
        switch (type)
        {
            case 0:
                SAMANDA.Instance.OnGuestLogin();
                break;
            case 1:
                SAMANDA.Instance.OnReqOAuth(AUTH_TYPE.GG); ;
                break;
            case 2:
                SAMANDA.Instance.OnReqOAuth(AUTH_TYPE.AP);
                break;
        }
    }
    #endregion
}
