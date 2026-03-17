using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace SandboxNetwork
{
    public class ShopBadgeObject : MonoBehaviour
    {
        [SerializeField]
        GameObject reddot = null;

        [SerializeField]
        Image badgeImg = null;

        [SerializeField]
        Text NameText = null;

        private int key = 0;
        public void Init(int goodsKey)
        {
            key = goodsKey;
           
            var goods = ShopManager.Instance.GetGoodsState(key);
            bool personalConditionSatisfied = ShopManager.Instance.PrivateGoods.ContainsKey(key);
            reddot.SetActive(goods.RemainGoodsCount > 0 && personalConditionSatisfied);
            
            CDNManager.SetBanner("store/" + goods.BaseData.BANNER.ICON_RESOURCE, badgeImg);

            NameText.text = PersonalGoodsData.Get(key).NAME_STRING;
        }

        public void OnClickBadge()
        {
            var popup = PopupManager.OpenPopup<ConditionalBuyPopup>(new ConditionBuyData(key));
            popup.SetRewardCallBack(() => {
                PopupManager.GetPopup<ShopPopup>().RefreshCurrentMenu();
                reddot.SetActive(false);
                popup.ClosePopup();
            });
        }
    }
}

