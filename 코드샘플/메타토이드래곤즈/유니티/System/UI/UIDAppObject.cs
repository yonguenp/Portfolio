using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class UIDAppObject : UIObject
    {
        public override void InitUI(eUIType targetType)
        {
            base.InitUI(targetType);

            RefreshUI(targetType);
        }

        public override bool RefreshUI(eUIType targetType)
        {
            bool active = GameConfigTable.WEB3_MENU_OPEN_ON_KOREAN || User.Instance.ENABLE_P2E;

            gameObject.SetActive(active);
            
            return active;
        }
    }
}
