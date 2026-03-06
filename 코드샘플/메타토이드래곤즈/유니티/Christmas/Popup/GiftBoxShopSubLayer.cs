using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    /// <summary>
    /// 선물상자 서브탭 -> 상점 내용 그대로 뿌려주는 레이어
    /// </summary>
    public class GiftBoxShopSubLayer : EventShopSubLayer
    {
        protected override void BuyCallbackProcess()
        {
            base.BuyCallbackProcess();
            DiceUIEvent.RefreshTabReddot();//변경된 상자 기준 레드닷 갱신
        }
    }
}