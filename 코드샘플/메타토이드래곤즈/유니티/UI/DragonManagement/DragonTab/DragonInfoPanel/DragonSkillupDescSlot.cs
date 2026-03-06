using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class DragonSkillupDescSlot : MonoBehaviour
    {
        [SerializeField]
        Color currentValueColor = new Color();
        [SerializeField]
        Color currentEffectColor = new Color();

        [SerializeField]
        Text currentValue = null;
        [SerializeField]
        Text currentEffectString = null;
        [SerializeField]
        Text nextValue = null;
        [SerializeField]
        Text nextEffectString = null;

        SkillEffectData data = null;

        int skillMaxLevel = -1;

        public void SetData(SkillEffectData _data, int _currentLevel, int _targetLevel)
        {
            if (_data == null)
                return;

            data = _data;

            if (skillMaxLevel < 0)
                skillMaxLevel = GameConfigTable.GetSkillLevelMax();

            SetValue(true, _currentLevel);
            SetValue(false, _targetLevel);
            SetEffectDesc();
        }

        public void SetData(string _effectString , float _currentValue , float _nextValue, bool _isPercent = false)
        {
            string currentValueString = Math.Round(_currentValue, 2).ToString();
            string nextValueString = Math.Round(_nextValue, 2).ToString();

            currentEffectString.text = string.Format(_effectString, "<color=#8AEBFA>" + currentValueString + "</color>");
            nextEffectString.text = string.Format(_effectString, "<color=#A2FF50>" + nextValueString + "</color>");

            //bool isEmtpyValue = _currentValue <= 0f;
            //currentEffectString.gameObject.SetActive(!isEmtpyValue);

            currentValue.gameObject.SetActive(false);
            nextValue.gameObject.SetActive(false);
        }

        void SetValue(bool _isCurrent, int _targetLevel)
        {
            //string valueStr = Math.Round(data.GetVALUE(_targetLevel), 2) + TryIntParseString(data._DESC2);
            //if (_isCurrent)
            //    currentValue.text = valueStr;
            //else
            //    nextValue.text = valueStr;

            if(_targetLevel >= skillMaxLevel && !_isCurrent)
            {
                nextValue.text = "MAX";
                nextEffectString.gameObject.SetActive(false);
            }
        }

        void SetEffectDesc()
        {
            //currentEffectString.text = TryIntParseString(data._DESC1);
            //nextEffectString.text = TryIntParseString(data._DESC1);
        }

        string TryIntParseString(string _checker)
        {
            if (int.TryParse(_checker, out int result))
            {
                if (result == 0)
                    return "";
                else
                    return StringData.GetStringByIndex(result);
            }
            else
                return StringData.GetStringByStrKey(_checker);
        }
    }
}
