using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SandboxPlatform.SAMANDA;


public class SAMANDA_Account : MonoBehaviour
{
    [SerializeField]
    Button linkGoogleButton;
    [SerializeField]
    Text linkGoogleStatusText;
    [SerializeField]
    Button linkAppleButton;
    [SerializeField]
    Text linkAppleStatusText;

    [SerializeField]
    Text GusetAccountNotice;

    public void SetUI(bool enable)
    {
        SetActive(enable);
        linkGoogleButton.gameObject.SetActive(SAMANDA.Instance._googleLogin);
        linkAppleButton.gameObject.SetActive(SAMANDA.Instance._appleLogin);

        if (enable)
        {
            //게스트 경고문 ON/OFF
            GusetAccountNotice.gameObject.SetActive(!SetLinkButtonUI(linkGoogleButton, linkAppleStatusText, AUTH_TYPE.GG) 
                && !SetLinkButtonUI(linkAppleButton, linkAppleStatusText, AUTH_TYPE.AP));
        }

        SetLinkButtonUI(linkGoogleButton, linkGoogleStatusText, AUTH_TYPE.GG);
        SetLinkButtonUI(linkAppleButton, linkAppleStatusText, AUTH_TYPE.AP);
    }

    bool SetLinkButtonUI(Button button, Text text, AUTH_TYPE type)
    {
        bool linked = SAMANDA.Instance.GetLinkedAuth().Contains(type);

        button.interactable = !linked;
        if (linked)
            text.text = "연동 완료";
        else
        {
//#if !UNITY_IOS
//            if(type == AUTH_TYPE.AP)
//            {
//                button.gameObject.SetActive(false);
//            }
//#endif
            text.text = "계정 연동";
        }

        return linked;
    }

    public void SetActive(bool enable)
    {
        gameObject.SetActive(enable);
    }

    public void OnAuthLinkButton(int type)
    {
        AUTH_TYPE authType = (AUTH_TYPE)type;
#if UNITY_IOS
        if(authType == AUTH_TYPE.AP)
        {
            return;
        }
#endif

        SAMANDA.Instance.OnReqOAuth(authType);
    }

    public void OnLogoutButton()
    {
        SAMANDA.Instance.OnLogout();
    }
}
