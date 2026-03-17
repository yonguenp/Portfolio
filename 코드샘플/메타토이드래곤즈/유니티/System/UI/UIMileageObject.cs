using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class UIMileageObject : UIObject
    {
        [SerializeField] Text amountText = null;

        public override void Init()
        {
            base.Init();
        }

        public override void InitUI(eUIType targetType)
        {
            base.InitUI(targetType);
            RefreshMileageCount();
        }

        public override bool RefreshUI(eUIType targetType)
        {
            if (base.RefreshUI(targetType))
            {
                RefreshMileageCount();
            }

            return curSceneType != targetType;
        }

        public void RefreshMileageCount()
        {
            if (amountText != null)
            {
                amountText.text = SBFunc.CommaFromNumber(User.Instance.UserData.Mileage);
            }
        }
    }
}