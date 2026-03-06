using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SystemPopupPanel : MonoBehaviour
{
    public delegate void Callback();

    public Text titleText;
    public Text guideText;

    Callback closeCallback = null;

    public void OnClickOkButton()
    {
        NecoCanvas.GetPopupCanvas().OnPopupClose(NecoPopupCanvas.POPUP_TYPE.MESSAGE_POPUP);
        closeCallback?.Invoke();
    }

    public void SetSystemPopupMsg(string title, string guide, Callback _closeCallback = null)
    {
        titleText.text = title;
        guideText.text = guide;

        closeCallback = _closeCallback;
    }

    private void OnDisable()
    {
        titleText.text = string.Empty;
        guideText.text = string.Empty;
    }
}
