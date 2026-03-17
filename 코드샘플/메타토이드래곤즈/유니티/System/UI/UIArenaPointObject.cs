using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class UIArenaPointObject : UIObject
    {
        [SerializeField] Text amountText = null;

        public override void Init()
        {
            base.Init();
        }

        public override void InitUI(eUIType targetType)
        {
            base.InitUI(targetType);
            RefreshArenaPointCount();
        }

        public override bool RefreshUI(eUIType targetType)
        {
            if (base.RefreshUI(targetType))
            {
                RefreshArenaPointCount();
            }

            return curSceneType != targetType;
        }

        public void RefreshArenaPointCount()
        {
            if (amountText != null)
            {
                amountText.text = SBFunc.CommaFromNumber(User.Instance.UserData.Arena_Point);
            }
        }
    }
}