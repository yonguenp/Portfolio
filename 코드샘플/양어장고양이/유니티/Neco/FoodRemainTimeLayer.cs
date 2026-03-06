using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FoodRemainTimeLayer : MonoBehaviour
{
    public Text remainTimeText;

    public RectTransform layoutObject;

    public void SetRemainTime(uint remainTime)
    {
        uint minute = remainTime / 60;
        uint second = remainTime % 60;

        if (minute < 1)
        {
            remainTimeText.text = string.Format(LocalizeData.GetText("LOCALIZE_211"), second);
        }
        else
        {
            remainTimeText.text = string.Format(LocalizeData.GetText("LOCALIZE_212"), minute, second);
        }

        if (layoutObject != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(layoutObject);
        }
    }
}
