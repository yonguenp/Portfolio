using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class UIMagnetObject : UIObject
    {
        [SerializeField]
        private Text AmountLabel = null;

        public override void InitUI(eUIType targetType)
        {
            //base.InitUI(targetType);
            RefreshCount();
        }
        public override bool RefreshUI(eUIType targetType)
        {
            if (base.RefreshUI(targetType))
            {
                RefreshCount();
            }
            return curSceneType != targetType;
        }
       
        public void RefreshCount()
        {
            if (AmountLabel != null)
            {
                var magnetAmount = User.Instance.UserData.Magnet;
                if(magnetAmount<= 0)
                {
                    AmountLabel.text = "0";
                    return;
                }

                AmountLabel.text = SBFunc.CommaFromNumber(magnetAmount);
            }
        }
        public override void ShowEvent()
        {
            gameObject.SetActive(User.Instance.ENABLE_P2E);
        }
    }
}
