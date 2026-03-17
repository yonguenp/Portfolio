using SandboxPlatform.SAMANDA;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NickNamePopup : MonoBehaviour
{
    [SerializeField]
    StartScene startScene;
    [SerializeField]
    GameObject guestPanel;
    [SerializeField]
    GameObject termsPanel;
    [SerializeField]
    Toggle serviceTermsofuse;
    [SerializeField]
    Toggle privacyTermsofuse;

    [SerializeField]
    Button okButton;

    private void OnEnable()
    {
        bool bGuest = false;
        if (SAMANDA.Instance.ACCOUNT_TYPE == AUTH_TYPE.NONE || SAMANDA.Instance.ACCOUNT_TYPE == AUTH_TYPE.GE)
        {
            bGuest = true;
        }

        guestPanel.SetActive(bGuest);
        termsPanel.SetActive(true);
        okButton.interactable = false;
        serviceTermsofuse.isOn = false;
        privacyTermsofuse.isOn = false;
    }

    public void ServiceButton()
    {
        Application.OpenURL(SAMANDA.Instance.base_Terms_URL + SAMANDA.Instance.service_URL);
    }

    public void PrivacyButton()
    {
        Application.OpenURL(SAMANDA.Instance.base_Terms_URL + SAMANDA.Instance.privacy_URL);
    }

    public void OnToggle()
    {
        okButton.interactable = serviceTermsofuse.isOn && privacyTermsofuse.isOn;

        okButton.GetComponentInChildren<Text>().color = okButton.interactable ? Color.white : Color.gray;
    }

    public void OnOK()
    {
        startScene.OnCreateAccount();
    }
}
