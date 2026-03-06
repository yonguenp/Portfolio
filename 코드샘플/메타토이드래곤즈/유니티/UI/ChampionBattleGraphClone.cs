using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace SandboxNetwork { 
    public class ChampionBattleGraphClone : MonoBehaviour
    {
        [SerializeField]
        Slider graphSlider = null;
        [SerializeField]
        Image FillImg = null;
        [SerializeField]
        Text graphAmountText = null;

        Tween graphTween = null;
        public void SetData(long curValue, long maxValue, Color color)
        {          
            gameObject.SetActive(true);
            graphSlider.maxValue = maxValue;
            graphSlider.value = 0;
            if (graphTween != null)
                graphTween.Kill();
            if(curValue != -1)
            {
                graphTween = graphSlider.DOValue(curValue, 0.5f).SetEase(Ease.InOutQuad);
                graphAmountText.text = curValue.ToString();
            }
            else
            {
                graphTween = graphSlider.DOValue(maxValue, 0.5f).SetEase(Ease.InOutQuad);
                graphAmountText.text = maxValue.ToString();
            }
            
            FillImg.color = color;
        }

        private void OnDisable()
        {
            if (graphTween != null)
                graphTween.Kill();
        }
    }
}