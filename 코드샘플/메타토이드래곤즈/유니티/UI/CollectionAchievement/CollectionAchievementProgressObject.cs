using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    /// <summary>
    /// 프로그레스바 세팅 하는 오브젝트 - 외부 세팅은 CollectionAchievementProgressController에서 값 던지기
    /// </summary>
    public class CollectionAchievementProgressObject : MonoBehaviour
    {
        [SerializeField] Slider slider = null;
        [SerializeField] Text countLabelText = null;
        [SerializeField] Text percentText = null;
        
        public void SetData(int currentValue, int maxValue)
        {
            var value = (float)currentValue / (float)maxValue;
            if (slider != null)
                slider.value = value;

            if(percentText != null)
                percentText.text = SBFunc.CommaFromNumber(Math.Round(value * 100, 2)) + "%";

            if (countLabelText != null)
                countLabelText.text = SBFunc.StrBuilder(currentValue, " / ", maxValue);
        }
    }
}
