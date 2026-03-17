using SandboxPlatform.SAMANDA;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SAMANDA_Termsofuse : MonoBehaviour
{
    [SerializeField]
    Toggle info;
    [SerializeField]
    Toggle service;
    [SerializeField]
    Button okButton;

    public void SetUI(bool enable)
    {
        SetActive(enable);

        okButton.interactable = false;
        info.isOn = false;
        service.isOn = false;
    }

    public void SetActive(bool enable)
    {
        gameObject.SetActive(enable);
    }

    public void OnToggle()
    {
        okButton.interactable = info.isOn && service.isOn;
    }

    public void OnOK()
    {
        SAMANDA.Instance.TermsofuseDone();
    }
}
