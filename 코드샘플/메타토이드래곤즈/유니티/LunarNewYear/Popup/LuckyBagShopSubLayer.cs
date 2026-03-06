using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class LuckyBagShopSubLayer : EventShopSubLayer
    {
        protected override void BuyCallbackProcess()
        {
            base.BuyCallbackProcess();
            LuckyBagUIEvent.RefreshTabReddot();
        }

        public void OnClickMoveTab()
        {
            LuckyBagEventPopup.MoveTabForce(new TabTypePopupData(0, 0));
        }
    }
}

