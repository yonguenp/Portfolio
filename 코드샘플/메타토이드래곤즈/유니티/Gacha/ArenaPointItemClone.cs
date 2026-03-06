using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class ArenaPointItemClone : MonoBehaviour
    {
        [SerializeField] Button itemButton = null;
        [SerializeField] GameObject blockLayer = null;

        [Header("[Item Info]")]
        [SerializeField] Image itemIcon = null;
        [SerializeField] Image itemIconBG = null;
        [SerializeField] Text itemNameText = null;
        [SerializeField] Text itemAmountText = null;

        [Header("[Price Info]")]
        [SerializeField] Image priceIcon = null;
        [SerializeField] Text priceText = null;
        [SerializeField] Text buyCountText = null;



        [SerializeField] Color normalPriceTextColor;

        protected ShopGoodsData currentGoodsData = null;
        ShopGoodsState currentGoodsStateData = null;

        Asset rewardAssetData = null;

        protected bool isAvailPurchase = false;

        public void InitItemClone(ShopGoodsData goodsData)
        {
            if (goodsData == null) return;

            currentGoodsData = goodsData;
            currentGoodsStateData = ShopManager.Instance.GetGoodsState(int.Parse(currentGoodsData.KEY));    // 데이터 갱신

            if (currentGoodsData.REWARDS != null && currentGoodsData.REWARDS.Count > 0)
            {
                rewardAssetData = currentGoodsData.REWARDS[0];
            }
            if (itemIconBG != null)
            {
                if (itemIconBG && rewardAssetData?.BaseData?.ASSET_TYPE == eGoodType.EQUIPMENT)
                {
                    itemIconBG.sprite = ResourceManager.GetResource<Sprite>(eResourcePath.PartsIconPath, SBFunc.StrBuilder("bggrade_board_", rewardAssetData.BaseData.GRADE));
                    itemIconBG.gameObject.SetActive(true);
                }
                else
                {
                    itemIconBG.gameObject.SetActive(false);
                }
            }
            
            itemIcon.sprite = currentGoodsData.SPRITE;
            itemNameText.text = string.Empty;
            itemAmountText.text = rewardAssetData.Amount == 1 ? string.Empty : rewardAssetData.Amount.ToString();

            priceText.text = currentGoodsData.PRICE.Amount.ToString();

            RefreshItemClone();
        }

        public void RefreshItemClone()
        {
            SetAvailPurchase();

           // priceIcon.color = isAvailPurchase ? Color.white : Color.gray;
            priceText.color = isAvailPurchase ? normalPriceTextColor : Color.red;

            // 유저 데이터 관련 세팅
            if (currentGoodsStateData != null)
            {
                buyCountText.text = $"{currentGoodsStateData.PurchasedLimit - currentGoodsStateData.PurchasedCount}/{currentGoodsStateData.PurchasedLimit}";
                blockLayer.SetActive(!currentGoodsStateData.IS_VALIDE);
            }
        }

        protected virtual void SetAvailPurchase()
        {
            isAvailPurchase = false;

            switch (currentGoodsData.PRICE.GoodType)
            {
                case eGoodType.ARENA_POINT:
                    isAvailPurchase = currentGoodsData.PRICE.Amount <= User.Instance.UserData.Arena_Point;
                    break;
            }
        }

        public virtual void OnClickItem()
        {
            ShopBuyPopup newPopup = PopupManager.OpenPopup<ShopBuyPopup>(new ShopBuyPopupData(currentGoodsData));
            newPopup?.SetPopupData(() =>
            {
                ClickItemProcess();
            });
        }

        public virtual void ClickItemProcess()
        {
            PopupManager.ForceUpdate<ArenaPointShopPopup>();
        }

        public void OnItemTooltip()
        {
            OnClickItem();
            return;

            if (currentGoodsData != null && currentGoodsData.REWARDS.Count > 0)
                ItemToolTip.OnItemToolTip(currentGoodsData, itemIcon.gameObject);
        }
    }
}