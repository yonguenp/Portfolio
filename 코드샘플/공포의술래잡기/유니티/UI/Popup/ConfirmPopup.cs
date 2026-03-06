using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ConfirmPopup : MessagePopup
{
    [SerializeField]
    Text okButtonText;

    CloseCallback okCallback;
    public void OnConfirm()
    {
        okCallback?.Invoke();

        CloseForce();
    }

    public void OnCancel()
    {
        Close();
    }

    public void ShowConfirm(string text, string ok, string cancel, CloseCallback okCB = null)
    {
        ShowMessage(text, cancel);
        okButtonText.text = StringManager.GetString(ok);

        okCallback = okCB;
    }
}
