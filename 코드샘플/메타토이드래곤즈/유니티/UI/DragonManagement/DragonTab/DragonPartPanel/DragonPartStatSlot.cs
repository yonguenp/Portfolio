using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class DragonPartStatSlot : MonoBehaviour
    {
        [SerializeField]
        Text titleLabel = null;

        [SerializeField]
        Text typeLabel = null;//타입 명시 라벨

        [SerializeField]
        Text valueLabel = null;//값 명시 라벨
        
        [SerializeField]
        Image bg = null;

        [SerializeField]
        GameObject LockBG = null;

        public void SetData(string typeStr ,float value ,bool isPercentCheck , bool _isLock = false, bool _addPlusCharacter = false)
        {
            if (typeLabel == null || valueLabel == null)
            {
                return;
            }

            typeLabel.text = typeStr;
            var modifyValue = isPercentCheck ? SBFunc.CommaFromNumber(Math.Round(value, 2)) : Math.Round(value, 2).ToString();//%표기는 소수점 두째자리까지
            valueLabel.text = isPercentCheck ? string.Format("{0}%", modifyValue) : string.Format("{0}", modifyValue);

            if (_addPlusCharacter)
                valueLabel.text = SBFunc.StrBuilder("+", valueLabel.text);

            if (LockBG != null)
            {
                LockBG.SetActive(_isLock);
                if (_isLock )
                {
                    typeLabel.text = "";
                    valueLabel.text = "";
                }
            }    
        }

        public void SetData(string typeStr, string valueStr)//한번에 넣기
        {
            if (typeLabel == null || valueLabel == null)
            {
                return;
            }

            typeLabel.text = typeStr;
            valueLabel.text = valueStr;
        }

        public void SetColor(Color _color)
        {
            typeLabel.color = _color;
            valueLabel.color = _color;
        }

        public void SetBG(Color _color)
        {
            if (bg != null)
                bg.color = _color;
        }

        public void SetTitle(string _title)
        {
            if (titleLabel != null)
                titleLabel.text = _title;
        }
    }
}
