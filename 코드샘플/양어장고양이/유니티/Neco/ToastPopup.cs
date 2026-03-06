using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToastPopup : MonoBehaviour
{
    const float TWEEN_TIME = 0.2f;

    public Image backgroundImage;
    public Text messageText;

    float notifyTime = 1.0f;

    Coroutine showToastCoroutine = null;

    private void OnEnable()
    {
        showToastCoroutine = StartCoroutine(ShowToastPopup(notifyTime));

        StartDOTweenAnim();
    }

    public void OnClickCloseButton()
    {
        CloseToastPopup();
    }

    public void SetToastPopup(string msg, float time = 1.0f)
    {
        if (messageText == null) { return; }

        messageText.text = msg;
        notifyTime = time;
    }

    IEnumerator ShowToastPopup(float time)
    {
        yield return new WaitForSeconds(time - TWEEN_TIME);

        EndDOTweenAnim();
    }

    void StartDOTweenAnim()
    {
        Sequence openAnim = DOTween.Sequence();
        openAnim.Append(backgroundImage.DOFade(0.8f, TWEEN_TIME));
        openAnim.Join(messageText.DOFade(1.0f, TWEEN_TIME));

        openAnim.Restart();
    }

    void EndDOTweenAnim()
    {
        Sequence openAnim = DOTween.Sequence();
        openAnim.Append(backgroundImage.DOFade(0, TWEEN_TIME));
        openAnim.Join(messageText.DOFade(0, TWEEN_TIME)).OnComplete(CloseToastPopup);
    }

    void CloseToastPopup()
    {
        if (showToastCoroutine != null)
        {
            StopCoroutine(showToastCoroutine);
        }

        Color originColor = backgroundImage.color;
        Color originTextColor = messageText.color;

        backgroundImage.color = new Color(originColor.r, originColor.g, originColor.b, 0);
        messageText.color = new Color(originTextColor.r, originTextColor.g, originTextColor.b, 0);

        NecoCanvas.GetPopupCanvas().OnPopupClose(NecoPopupCanvas.POPUP_TYPE.TOAST_POPUP);
    }
}
