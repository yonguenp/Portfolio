using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class UIMiningIconObject : UIObject
    {
        [SerializeField] Image lockImage = null;
        public override void InitUI(eUIType targetType)
        {
            base.InitUI(targetType);

            RefreshUI(targetType);
        }

        public override bool RefreshUI(eUIType targetType)
        {
            bool active = GameConfigTable.WEB3_MENU_OPEN_ON_KOREAN || User.Instance.ENABLE_P2E;

            gameObject.SetActive(active);
            
            if(active)
                RefreshLockState();
            
            return active;
        }

        public void RefreshLockState()
        {
            if (lockImage != null)
                lockImage.gameObject.SetActive(!MiningManager.Instance.IsAvailableMiningUIOpen());
        }
    }
}
