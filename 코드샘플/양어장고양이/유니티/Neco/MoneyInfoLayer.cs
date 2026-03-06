using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MoneyInfoLayer : MonoBehaviour
{
    const float TWEEN_TIME = 0.6f;

    public Image bgImage;
    public Image curIcon;
    public Text curMoney;

    private void OnEnable()
    {
        Invoke("SetUserMoneyData", 0.1f);
    }

    public void RefreshGoldData()
    {
        Invoke("SetUserMoneyData", 0.1f);
    }

    public void DoFadeTween(bool isOpen)
    {
        if (bgImage == null || curIcon == null || curMoney == null) { return; }

        if (isOpen)
        {
            Sequence layerOpenAnim = DOTween.Sequence();
            layerOpenAnim.Append(bgImage.DOFade(0.58f, TWEEN_TIME));
            layerOpenAnim.Join(curIcon.DOFade(1.0f, TWEEN_TIME));
            layerOpenAnim.Join(curMoney.DOFade(1.0f, TWEEN_TIME));

            layerOpenAnim.Restart();
        }
        else
        {
            Sequence layerCloseAnim = DOTween.Sequence();
            layerCloseAnim.Append(bgImage.DOFade(0, TWEEN_TIME));
            layerCloseAnim.Join(curIcon.DOFade(0, TWEEN_TIME));
            layerCloseAnim.Join(curMoney.DOFade(0, TWEEN_TIME));

            layerCloseAnim.Restart();
        }
    }

    public void DoMoneyAppearTween(bool isOpen)
    {
        if (bgImage == null || curIcon == null || curMoney == null) { return; }

        // 팝업에서 사용
        DoFadeTween(isOpen);

        if (isOpen)
        {
            Sequence layerOpenAnim = DOTween.Sequence();
            layerOpenAnim.Append(gameObject.GetComponent<RectTransform>().DOAnchorPosY(-60, TWEEN_TIME).SetEase(Ease.InOutSine));

            layerOpenAnim.Restart();
        }
        else
        {
            Sequence layerCloseAnim = DOTween.Sequence();
            layerCloseAnim.Append(gameObject.GetComponent<RectTransform>().DOAnchorPosY(-10, TWEEN_TIME).SetEase(Ease.InOutSine));

            layerCloseAnim.Restart();
        }
    }

    void SetUserMoneyData()
    {
        users user = GameDataManager.Instance.GetUserData();
        if (user == null)
            return;

        object obj;
        uint money = 0;
        if (user.data.TryGetValue("gold", out obj))
        {
            money = (uint)obj;
        }

        curMoney.text = money.ToString("n0");
    }
}
