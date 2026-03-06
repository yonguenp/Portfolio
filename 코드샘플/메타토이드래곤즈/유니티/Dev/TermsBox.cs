using SandboxNetwork;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TermsBox : MonoBehaviour
{
    [Header("Terms")]
    [SerializeField]
    Toggle termsToggle_1 = null;
    [SerializeField]
    Toggle termsToggle_2 = null;

    [SerializeField]
    private Button signBtn = null;

    [Space(10)]
    [SerializeField]
    private GameObject systemPopup;


    LoginData signupData = null;
    NetworkManager.SuccessCallback OnSignDataCallback = null;

    private void OnEnable()
    {
        systemPopup.SetActive(false);
        signBtn.interactable = true;
    }

    public void SetActive(bool active, LoginData data = null, NetworkManager.SuccessCallback onSignData = null)
    {
        gameObject.SetActive(active);
        signupData = data;
        if(onSignData != null)
            OnSignDataCallback = onSignData;
    }

    public void OnSignUp()
    {
        if (termsToggle_1 == null || termsToggle_2 == null) return;

        // 약관 이용동의 체크
        if (termsToggle_1.isOn == false || termsToggle_2.isOn == false)
        {
            SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100002681), StringData.GetStringByIndex(100000199), StringData.GetStringByIndex(100000200));
            return;
        }

        if (signBtn != null)
        {
            signBtn.interactable = false;
            //loginBtn.SetInteractable(false);                
            signBtn.SetButtonSpriteState(false);
        }

        var data = new WWWForm();
        if (signupData == null || signupData.loginType == eAuthAccount.NONE || signupData.loginType == eAuthAccount.GUEST)
        {
            //data.AddField("type", 0);
            data.AddField("id_tok", "");
            data.AddField("a_type", (int)eAuthAccount.GUEST);
            data.AddField("is_pc",
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
             1
#else
			 0
#endif
             );
        }
        else
        {
            data.AddField("id_tok", signupData.jwtToken);
            data.AddField("a_type", (int)signupData.loginType);
            data.AddField("is_pc",
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
             1
#else
			 0
#endif
             );
        }

#if UNITY_ANDROID || UNITY_IOS
        if (Application.genuineCheckAvailable)
        {
            if (!Application.genuine)
            {
                Application.Quit();
            }
        }
#endif

        NetworkManager.Send("auth/signup", data, (Newtonsoft.Json.Linq.JObject res) => {
            AppsFlyerSDK.AppsFlyer.sendEvent("create_pid", null);

            if (OnSignDataCallback != null)
                OnSignDataCallback.Invoke(res);
        });

        SetActive(false);
    }


    public void CheckSignUpState()
    {
        if (termsToggle_1 == null || termsToggle_2 == null) return;

        // 이용약관 체크
        bool termsCheck = (termsToggle_1.isOn && termsToggle_2.isOn);
        termsToggle_1.graphic.gameObject.SetActive(termsToggle_1.isOn);
        termsToggle_2.graphic.gameObject.SetActive(termsToggle_2.isOn);

        signBtn.SetButtonSpriteState(termsCheck);
    }
}
