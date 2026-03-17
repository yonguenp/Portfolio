using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    /// <summary>
    /// 현재 적용 중인 부스터 아이템의 상태를 나타냄 - 갯수는 필요없음/ 오로지 시간으로 조건 체크
    /// </summary>
    public enum BoostUIType
    {
        NONE,
        PERCENT,
        VALUE,
        EVENT_SCHEDULE,
    }
    public class MineBoostStateUISlot : MonoBehaviour
    {
        [SerializeField] Image itemIcon = null;
        [SerializeField] Text valueText = null;
        [SerializeField] Color disableColor = new Color();
        [SerializeField] Color enableColor = new Color();
        [SerializeField] Text timeText = null;
        [SerializeField] TimeEnable timeObject = null;

        public BoostUIType boostType = BoostUIType.NONE;

        MineBoosterItem targetItem = null;
        public void SetBoost(MineBoosterItem _item)
        {
            if (_item == null)//|| _item.Amount <= 0
            {
                SetDisableState();
                return;
            }

            targetItem = _item;

            SetUI();
            SetBoostTime();
        }

        public void SetDisableState()
        {
            if(timeObject != null)
                timeObject.Refresh = null;

            if (timeText != null)
                timeText.text = "--:--:--";

            if (valueText != null)
            {
                valueText.color = disableColor;
                valueText.text = boostType == BoostUIType.VALUE ? "+0" : "+0%";
            }

            if (itemIcon != null)
                itemIcon.gameObject.SetActive(false);
        }


        void SetUI()
        {
            if (targetItem == null)
                return;

            if (itemIcon != null)
            {
                itemIcon.sprite = targetItem.BaseData.ICON_SPRITE;
                itemIcon.gameObject.SetActive(true);
            }
            if (valueText != null)
            {
                string resultString;
                if(boostType == BoostUIType.VALUE)
                {
                    var currentValue = targetItem.BoostTableData.VALUE / SBDefine.Day;//(초 단위 연산)
                    var currentProductData = MiningManager.Instance.GetProductData();
                    float productTime = 0;
                    if (currentProductData != null)
                        productTime = currentProductData.PRODUCT_TIME;

                    var resultValue = MathF.Ceiling(currentValue * productTime);
                    resultString = SBFunc.StrBuilder("+", productTime == 0 ? 0 : resultValue);
                }
                else
                    resultString = targetItem.BoostTableData.VALUE_DESC;

                valueText.text = resultString;
                valueText.color = enableColor;
            }
        }

        void SetBoostTime()
        {
            if (targetItem == null) return;
            if (timeObject == null) return;
            if (timeText == null) return;
            if (targetItem.BoostTableData == null) return;

            //현재 부스트 상태의 아이템 리스트 또는 인덱스 참조해야함 (만료 시간 체크용도)
            if (targetItem.ExpireTime <= 0)
            {
                SetDisableState();
                return;
            }

            int endTime = targetItem.ExpireTime;
            int expireTimeCheck = TimeManager.GetTimeCompare(endTime);
            if(expireTimeCheck > 0)
            {
                timeObject.Refresh = () =>
                {
                    int remainTime = TimeManager.GetTimeCompare(endTime);
                    if (remainTime >= 0)
                        timeText.text = SBFunc.TimeString(remainTime);
                    else
                    {
                        SetDisableState();

                        timeObject.Refresh = null;
                    }
                };
            }
            else
            {
                SetDisableState();
                timeObject.Refresh = null;
            }
        }
    }
}
