using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class UIGuildPointObject : UIObject
    {
        [SerializeField] Text amountText = null;


        public override void InitUI(eUIType targetType)
        {
            //base.InitUI(targetType);
            RefreshGuildPointCount();
        }

        public override bool RefreshUI(eUIType targetType)
        {
            if (base.RefreshUI(targetType))
            {
                RefreshGuildPointCount();
            }

            return curSceneType != targetType;
        }

        public void RefreshGuildPointCount()
        {
            if (amountText != null)
            {
                amountText.text = SBFunc.CommaFromNumber(User.Instance.UserData.Guild_Point);
            }
        }
    }
}


