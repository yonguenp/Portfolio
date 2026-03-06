using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class GachaMileageItemClone : MonoBehaviour
    {
        [SerializeField] Button itemButton = null;
        [SerializeField] Image itemIcon = null;
        [SerializeField] Image itemIconBG = null;
        [SerializeField] Text itemAmountText = null;

        [Header("[Price Info]")]
        [SerializeField] Image priceIcon = null;
        [SerializeField] Text priceText = null;

        [SerializeField] Sprite normalPriceSprite = null;
        [SerializeField] Sprite grayPriceSprite = null;

        [SerializeField] Color normalPriceTextColor;

        ShopGoodsData currentGoodsData = null;
        ItemGroupData currentItemGroupData = null;

        bool isAvailPurchase = false;

        public void InitItemClone(ShopGoodsData goodsData)
        {
            if (goodsData == null) return;

            currentGoodsData = goodsData;

            List<ItemGroupData> ItemGroupDataList = ItemGroupData.Get(currentGoodsData.REWARD_ID);
            if (ItemGroupDataList != null && ItemGroupDataList.Count > 0)
            {
                currentItemGroupData = ItemGroupDataList[0];
            }

            if (itemIconBG != null)
            {
                if (itemIconBG && currentItemGroupData.Reward?.BaseData?.ASSET_TYPE == eGoodType.EQUIPMENT)
                {
                    itemIconBG.sprite = ResourceManager.GetResource<Sprite>(eResourcePath.PartsIconPath, SBFunc.StrBuilder("bggrade_board_", currentItemGroupData.Reward.BaseData.GRADE));
                    itemIconBG.gameObject.SetActive(true);
                }
                else
                {
                    itemIconBG.gameObject.SetActive(false);
                }
            }

            itemIcon.sprite = currentGoodsData.SPRITE;
            itemAmountText.text = currentItemGroupData.Reward.Amount == 1 ? string.Empty : currentItemGroupData.Reward.Amount.ToString();

            priceText.text = currentGoodsData.PRICE.Amount.ToString();

            RefreshItemClone();
        }

        public void RefreshItemClone()
        {
            isAvailPurchase = false;
            switch (currentGoodsData.PRICE.GoodType)
            {
                case eGoodType.MILEAGE:
                    isAvailPurchase = (currentGoodsData.PRICE.Amount <= User.Instance.UserData.Mileage);
                    break;
                case eGoodType.ITEM:
                    if (currentGoodsData.PRICE.ItemNo == 10000011)
                        isAvailPurchase = (currentGoodsData.PRICE.Amount <= User.Instance.UserData.Mileage);
                    break;
            }

            priceText.color = isAvailPurchase ? normalPriceTextColor : Color.gray;
        }

        public void OnClickMileageItem()
        {
            ShopBuyPopup newPopup = PopupManager.OpenPopup<ShopBuyPopup>(new ShopBuyPopupData(currentGoodsData));
            newPopup?.SetPopupData(() => 
            {   
                PopupManager.ForceUpdate<GachaMileageShopPopup>();
            });
        }
    }
}