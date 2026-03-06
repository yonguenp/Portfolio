using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    /// <summary>
    /// 업적 & 콜렉션 효과 클론 - 터치 시 표시 on/off 기능 - 토글
    /// </summary>
    public class CollectionAchievementEffectDataClone : MonoBehaviour
    {
        [SerializeField] Image colorChangeTarget = null;
        [SerializeField] Text statText = null;
        [SerializeField] Text valueText = null;

        [SerializeField] Color defaultColor = new Color();
        [SerializeField] Color selectedColor = new Color();

        [SerializeField] Color achievementValueColor = new Color();
        [SerializeField] Color collectionValueColor = new Color();

        private StatTypeData statTypeData = null;
        private bool isSelected = false;
        public eStatusType StatType { get; private set; }
        public eStatusValueType StatValueType { get; private set; }
        private float value = 0;

        eCollectionAchievementType currentTabType = eCollectionAchievementType.NONE;
        public void InitUI(eStatusType _type, eStatusValueType valueType, float _value, eCollectionAchievementType _tabType)//초기화 및 UI 세팅
        {
            StatType = _type;
            StatValueType = valueType;
            value = _value;
            currentTabType = _tabType;
            statTypeData = StatTypeData.Get(StatType);

            SetDescText();
            SetValueText(_value);

            RefreshUI(false);
        }

        void SetDescText()
        {
            if(statText != null && statTypeData != null)
            {
                statText.text = StatValueType == eStatusValueType.PERCENT ? statTypeData.PERCENT_DESC : statTypeData.VALUE_DESC;
            }
        }

        void SetValueText(float _value)
        {
            if(valueText != null && statTypeData != null)
            {
                value = _value;
                var valuePostFix = StatValueType switch
                {
                    eStatusValueType.PERCENT => "%",
                    eStatusValueType.VALUE => "",
                    _ => ""
                };

                valueText.text = SBFunc.StrBuilder("+", _value, valuePostFix);
                valueText.color = currentTabType == eCollectionAchievementType.COLLECTION ? collectionValueColor : achievementValueColor;
            }
        }

        public void RefreshUI(bool _isSelected, int value = -1)
        {
            isSelected = _isSelected;

            if(value > 0)
                SetValueText(value);

            SetColor(_isSelected);
        }
        void SetColor(bool _isSelected)
        {
            if (colorChangeTarget != null)
                colorChangeTarget.color = _isSelected ? selectedColor : defaultColor;
        }

        /// <summary>
        /// 버튼 누르면 CollectionAchievementEffectScrollview 이벤트 TOUCH_EFFECT 쏘고, 
        /// CollectionAchievementEffectScrollview 에서 Refresh 치고, SEND_FILTER_SCROLLVIEW 쏨.
        /// </summary>
        public void OnClickTouchEffect()//효과 버튼 누르면 
        {
            RefreshUI(!isSelected);
            CollectionAchievementUIEvent.TouchEffectUI(StatType, StatValueType, isSelected);
        }
    }
}
