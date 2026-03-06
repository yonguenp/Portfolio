using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class FriendPointItemClone : ArenaPointItemClone
    {
        protected override void SetAvailPurchase()
        {
            isAvailPurchase = false;

            switch (currentGoodsData.PRICE.GoodType)
            {
                case eGoodType.FRIENDLY_POINT:
                    isAvailPurchase = currentGoodsData.PRICE.Amount <= User.Instance.UserData.Friendly_Point;
                    break;
            }
        }

        public override void ClickItemProcess()
        {
            PopupManager.ForceUpdate<FriendPointShopPopup>();
        }
        public override void OnClickItem()
        {
            ShopBuyPopup newPopup = PopupManager.OpenPopup<ShopBuyPopup>(new ShopBuyPopupData(currentGoodsData));
            newPopup?.SetPopupData(() =>
            {
                ClickItemProcess();
            },true);
        }
    }
}
