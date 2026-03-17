using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class PassiveTableClone : MonoBehaviour
    {
        [SerializeField]
        Text gradeText = null;
        [SerializeField]
        Text probabilityText = null;

        [SerializeField]
        Image backImg = null;

        public float GachaMaxRate { get; private set; } = 0;

        SkillPassiveRateData currentRateData = null;
        List<SkillPassiveRateData> resultRateData = null;
        
        List<GachaTableSubClone> subTables = null;
        int grade = 0;
        bool subOpen = false;
        PassiveTablePopup parent = null;

        public void InitClone(SkillPassiveRateData rateData, List<GachaTableSubClone> subs, PassiveTablePopup p)
        {
            currentRateData = rateData;

            if (gradeText != null)
            {
                gradeText.text = rateData.NAME;
            }

            parent = p;
            subTables = subs;

            SetSubTable(false);
            //probabilityText.text = (rateData.RATE / (float)SBDefine.Million).ToString("P2");
        }

        public void UpdateMaxRate(float maxRate)
        {
            if (probabilityText != null)
            {
                probabilityText.gameObject.SetActive(currentRateData.RATE != maxRate);

                var rate = currentRateData.RATE / maxRate;
                rate *= SBDefine.MILLION;
                rate = Mathf.Floor(rate * SBDefine.THOUSAND) / SBDefine.THOUSAND; // 소수점 4째짜리이하 버림
                rate = Mathf.Ceil(rate * SBDefine.BASE_FLOAT) / SBDefine.BASE_FLOAT; // 소수점 3째짜리 올림
                rate /= SBDefine.MILLION;
                probabilityText.text = rate.ToString("P2");

                int subTotal = 0;
                foreach (var sub in subTables)
                {
                    if(sub.currentPassiveSkillRateData != null)
                    {
                        subTotal += sub.currentPassiveSkillRateData.RATE;
                    }
                }

                foreach (var sub in subTables)
                {
                    sub.UpdateMaxRate(rate, subTotal);
                }
            }
        }

        public void OnToggleTable()
        {
            SetSubTable(!subOpen);
        }

        public void SetSubTable(bool enable)
        {
            if (enable)
            {
                parent.OnSubTableClear();
            }

            subOpen = enable;

            foreach (var sub in subTables)
            {
                sub.SetActive(enable);
            }

            parent.CancelInvoke("CheckScrollSize");
            parent.Invoke("CheckScrollSize", 0.1f);
        }
    }
}


