using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class GachaTableClone : MonoBehaviour
    {
        [SerializeField]
        Text gradeText = null;
        [SerializeField]
        Text probabilityText = null;

        [SerializeField]
        Image backImg = null;

        public float GachaMaxRate { get; private set; } = 0;

        GachaRateData currentRateData = null;
        List<GachaRateData> resultRateData = null;
        List<GachaTableSubClone> subTables = null;
        int grade = 0;
        bool subOpen = false;
        GachaTablePopup parent = null;
        float CurWeight = 0;

        public void InitClone(GachaRateData rateData, List<GachaTableSubClone> subs, GachaTablePopup p, float weight)
        {
            CurWeight = weight;
            currentRateData = rateData;

            if (gradeText != null)
            {
                //gradeText.text = StringData.GetStringFormatByStrKey("gacha_table_rate", SBFunc.GetGradeConvertString(2));
                gradeText.text = rateData.Name;
            }

            parent = p;
            subTables = subs;
            //backImg.color = rateData.Color;

            SetSubTable(false);
            if(rateData.group_id==401) //럭키박스용 예외처리
                gameObject.SetActive(false);
        }
        
        public void UpdateMaxRate(float maxRate)
        {
            if (probabilityText != null)
            {
                probabilityText.gameObject.SetActive(CurWeight != maxRate);

                var rate = CurWeight / maxRate;                
                probabilityText.text = SBFunc.StrBuilder((rate * 100f - 0.005f/*소수점 내림*/).ToString("F2"), "%").Replace(".00", "");

                int subTotal = 0;
                foreach (var sub in subTables)
                {
                    if (sub.currentRateData != null)
                    {
                        subTotal += sub.currentRateData.weight;
                    }
                    if (sub.currentItemGroupData != null)
                    {
                        subTotal += sub.currentItemGroupData.ITEM_RATE;
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