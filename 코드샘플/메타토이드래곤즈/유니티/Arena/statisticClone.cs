using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace SandboxNetwork { 
    public class statisticClone : MonoBehaviour
    {
        [SerializeField] private Slider atkSlider = null;
        [SerializeField] private Text atkText = null;
        [SerializeField] private Text atkPercentText = null;
        [SerializeField] private Slider defSlider = null;
        [SerializeField] private Text defText = null;
        [SerializeField] private Text defPercentText = null;
        [SerializeField] private Slider defTrueSlider = null;
        [SerializeField] private Text defTrueText = null;

        int atkDmg;
        int defDmg;
        int defTrueDmg;
        int atkTotalDmg;
        int defTotalDmg;
        public void setAtkData(int dmg, float percentage,int totalDmg)
        {
            if (atkSlider == null || atkText == null || atkPercentText ==null) return;
            if(totalDmg != 0) { 
                atkSlider.maxValue = Mathf.Max(totalDmg,1);
            }
            else
            {
                atkSlider.maxValue = 100;
            }
            atkTotalDmg = totalDmg;
            atkSlider.value = Mathf.Max(dmg, totalDmg/100f,1);
            atkDmg = dmg;
            atkText.text = SBFunc.CommaFromNumber(dmg);
            atkPercentText.text = string.Format("{0:P2}", float.IsNaN(percentage) ? 0 : percentage);

        }        

        public void setDefData(int dmg, int trueDmg, float percentage, int totalDmg)
        {
            if (defSlider == null || defText == null || defPercentText ==null) return;
            if(totalDmg != 0) {
                defSlider.maxValue = Mathf.Max(totalDmg, 1);
                defTrueSlider.maxValue = Mathf.Max(totalDmg, 1);
            }
            else
            {
                defSlider.maxValue = 100f;
                defTrueSlider.maxValue = 100;
            }
            defTotalDmg = totalDmg;
            defSlider.value = Mathf.Max(dmg, totalDmg / 100f,1);
            defTrueSlider.value = Mathf.Max(trueDmg, totalDmg / 100f,1);
            defDmg = dmg;
            defTrueDmg = trueDmg;
            defText.text = SBFunc.CommaFromNumber(dmg);
            defTrueText.text = string.Format("({0})", SBFunc.CommaFromNumber(trueDmg));
            defPercentText.text = string.Format("{0:P2}",float.IsNaN(percentage)?0: percentage);
        }

        public void sliderTweenStart()  // 데미지를 안넣었더라도 아주 조금은 차있어야 함. 
                                        // 유니티에서 slider max value랑 value랑 차이가 너무 크면 0이랑 같은 상태로 됨 -> mathf.max( totalDamage/100, damage);
        {                               // 토탈 데미지가 0인 경우에도 살짝은 차있어야 함, 그래서 mathf.max에 1을 넣어 비교
            if (atkSlider != null) {
                atkSlider.value = 0;
                atkSlider.DOValue(Mathf.Max(atkTotalDmg / 100f, atkDmg,1), 0.3f); 
            }
            if (defTrueSlider != null && defSlider != null)
            {
                defSlider.value = 0;
                defSlider.DOValue(Mathf.Max(defTotalDmg / 100f, defDmg,1), 0.3f);
                defTrueSlider.value = 0;
                defTrueSlider.DOValue(Mathf.Max(defTotalDmg/100f, defTrueDmg,1), 0.3f);
            }
        }
    }
}