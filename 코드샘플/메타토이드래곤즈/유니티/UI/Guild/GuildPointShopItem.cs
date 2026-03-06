using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork { 
    public class GuildPointShopItem : MonoBehaviour
    {
        [SerializeField] Button itemButton = null;
        [SerializeField] GameObject blockLayer = null;

        [Header("[Item Info]")]
        [SerializeField] Image itemIcon = null;
        [SerializeField] Image itemIconBG = null;
        [SerializeField] Text itemNameText = null;
        [SerializeField] Text itemAmountText = null;

        [Header("[Price Info]")]
        [SerializeField] GameObject priceLayer = null;
        [SerializeField] Image priceIcon = null;
        [SerializeField] Text priceText = null;
        [SerializeField] Text buyCountText = null;
        [SerializeField] GameObject buyCountLayer = null;

        [Header("[Lv Condition]")]
        [SerializeField] GameObject lvNeedLayer = null;
        [SerializeField] Text lvNeedText = null;

        [Space()]
        [SerializeField] Color normalPriceTextColor;
        protected ShopGoodsData currentGoodsData = null;
        ShopGoodsState currentGoodsStateData = null;

        Asset rewardAssetData = null;

        protected bool isAvailPurchase = false;

        bool IsLvEnough = false;

        public void InitItemClone(ShopGoodsData goodsData,int guildLv)
        {
            if (goodsData == null) return;

            currentGoodsData = goodsData;
            
            currentGoodsStateData = ShopManager.Instance.GetGoodsState(int.Parse(currentGoodsData.KEY));    // 데이터 갱신
            priceIcon.sprite = SBFunc.GetGoodTypeIcon(goodsData.PRICE.GoodType);
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

            if (rewardAssetData.GoodType == eGoodType.CHARACTER)
            {
                string thumbnail = CharBaseData.Get(rewardAssetData.ItemNo).THUMBNAIL;
                itemIcon.sprite = ResourceManager.GetResource<Sprite>(eResourcePath.CharIconPath, thumbnail);
            }
            else
                itemIcon.sprite = SBFunc.GetGoodTypeIcon(rewardAssetData.GoodType, rewardAssetData.ItemNo);
            

            itemNameText.text = string.Empty;
            if (rewardAssetData.Amount > 1)
                itemAmountText.text = rewardAssetData.Amount.ToString();
            else
                itemAmountText.text = string.Empty;

            priceText.text = currentGoodsData.PRICE.Amount.ToString();
            lvNeedText.text = StringData.GetStringFormatByStrKey("guild_desc:92", currentGoodsData.LEVEL);
            IsLvEnough = guildLv >= currentGoodsData.LEVEL;

            if (IsLvEnough)
            {
                priceLayer.SetActive(true);
                lvNeedLayer.SetActive(false);
            }
            else
            {
                priceLayer.SetActive(false);
                lvNeedLayer.SetActive(true);
            }

            RefreshItemClone();
        }

        public void RefreshItemClone()
        {
            SetAvailPurchase();

            priceText.color = isAvailPurchase ? normalPriceTextColor : Color.red;

            string text = string.Empty;
            bool onBlock = false;
            bool isUnlimited = false;
            // 유저 데이터 관련 세팅
            if (currentGoodsStateData != null)
            {
                if(currentGoodsStateData.PurchasedLimit > 0)
                {
                    string strKey = string.Empty;
                    switch (currentGoodsData.BUY_TYPE)
                    {
                        case eBuyLimitType.UNLIMIT:
                        case eBuyLimitType.SEASON:                            
                            strKey = string.Empty;
                            break;
                        case eBuyLimitType.WEEK:
                            strKey = "buy_limit_weekly";
                            break;
                        case eBuyLimitType.MONTH:
                            strKey = "buy_limit_monthly";
                            break;
                        case eBuyLimitType.DAILY:
                            strKey = "buy_limit_daily";
                            break;
                        case eBuyLimitType.ACCOUNT:
                            strKey = "buy_limit_account";
                            break;
                    }
                    text = currentGoodsData.BUY_LIMIT > 0 ? (StringData.GetStringFormatByStrKey(strKey, currentGoodsData.BUY_LIMIT) + "\n") : string.Empty;

                    int remainCnt = currentGoodsStateData.PurchasedLimit - currentGoodsStateData.PurchasedCount;
                    text += $"{remainCnt}/{currentGoodsStateData.PurchasedLimit}";
                    onBlock = (!currentGoodsStateData.IS_VALIDE || remainCnt == 0);
                }
                else
                {
                    isUnlimited = true;
                    onBlock = !currentGoodsStateData.IS_VALIDE;
                }


                buyCountText.text = text;
                blockLayer.SetActive(onBlock);
                buyCountLayer.SetActive(!isUnlimited);
            }
        }

        protected virtual void SetAvailPurchase()
        {
           isAvailPurchase = currentGoodsData.PRICE.Amount <= User.Instance.UserData.Guild_Point;
        }

        public virtual void OnClickItem()
        {
            if (IsLvEnough == false)
                return;
            ShopBuyPopup newPopup = PopupManager.OpenPopup<ShopBuyPopup>(new ShopBuyPopupData(currentGoodsData));
            newPopup?.SetPopupData(() =>
            {
                ClickItemProcess();
            });
        }

        public virtual void ClickItemProcess()
        {
            PopupManager.ForceUpdate<GuildShopPopup>();
        }

        public void OnItemTooltip()
        {
            if (currentGoodsData == null && currentGoodsData.REWARDS.Count <= 0)
                return;

            var item = currentGoodsData.REWARDS[0];
            if(item.GoodType != eGoodType.CHARACTER)
                ToolTip.OnToolTip(item, itemIcon.gameObject);
            else
            {
                var guildPopup = PopupManager.GetPopup<GuildInfoPopup>();
                if (guildPopup == null)
                    return;

                guildPopup.SetModless(false);

                var popup = DragonManagePopup.OpenPopup(0, 1);
                PopupManager.GetPopup<DragonManagePopup>().ForceCloseFlag = true;
                popup.SetExitCallback(() => {
                    guildPopup.SetModless(true);
                });
                popup.CurDragonTag = item.ItemNo;
                popup.ClearDragonInfoList();

                popup.DragonInfoList.AddRange(new List<int>() { item.ItemNo });
                popup.ForceUpdate();
            }
        }

        public void OnClickLevelNeed()
        {
            ToastManager.On(StringData.GetStringFormatByStrKey("guild_desc:92", currentGoodsData.LEVEL));
        }
    }
}