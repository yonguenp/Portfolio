using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.UI;

public class UIProgressBar : MonoBehaviour
{
    [SerializeField] Image bar = null;
    [SerializeField] Text text = null;
    [SerializeField] bool usingFillAmount = true;
    [SerializeField] float maxWidth = 0;

    public void SetValue(float ratio)
    {
        if (bar != null)
        {
            if(usingFillAmount)
                bar.fillAmount = ratio;
            else
            {
                var value = maxWidth * ratio;
                bar.rectTransform.sizeDelta = new Vector2(value, bar.rectTransform.sizeDelta.y);
            }
        }
    }

    public void SetText(string value)
    {
        if (text != null)
            text.text = value;
    }

    public void BlinkBar(Action completeCallback)
    {
        var targetColor = new Color(bar.color.r, bar.color.g, bar.color.b, 0.5f);
        bar.DOColor(targetColor, 0.5f)
            .SetLoops(4, LoopType.Yoyo)
            .OnComplete(() => { completeCallback?.Invoke(); });
    }
}
