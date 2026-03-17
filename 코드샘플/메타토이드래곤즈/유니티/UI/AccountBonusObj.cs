using SandboxNetwork;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AccountBonusObj : MonoBehaviour
{
    [SerializeField] Text statText = null;
    [SerializeField] Text valueText = null;
    public eStatusType StatType { get; private set; }
    public eStatusValueType StatValueType { get; private set; }
    private float value = 0;

    private StatTypeData statTypeData = null;
    public void InitUI(eStatusType _type, eStatusValueType valueType, float _value)
    {
        StatType = _type;
        StatValueType = valueType;
        value = _value;

        statTypeData = StatTypeData.Get(StatType);

        SetDescText();
        SetValueText(_value);
    }
    void SetDescText()
    {
        if (statText != null && statTypeData != null)
        {
            statText.text = StatValueType == eStatusValueType.PERCENT ? statTypeData.PERCENT_DESC : statTypeData.VALUE_DESC;
        }
    }

    void SetValueText(float _value)
    {
        if (valueText != null && statTypeData != null)
        {
            value = _value;
            var valuePostFix = StatValueType switch
            {
                eStatusValueType.PERCENT => "%",
                eStatusValueType.VALUE => "",
                _ => ""
            };

            valueText.text = SBFunc.StrBuilder("+", _value, valuePostFix);
        }
    }
}
