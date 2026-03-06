using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class BuildingMineUpgradePopup : BuildingUpgradePopup
    {
        public Text currentTimeText = null;
        public Text nextTimeText = null;

        public Text currentAmountText = null;
        public Text nextAmountText = null;

        public Text currentDurabilityAmountText = null;
        public Text nextDurabilityAmountText = null;

        public Text townConditionText = null;


        ProductData currentProductData = null;
        ProductData nextProductData = null;

        MineDrillData currentDrillData = null;
        MineDrillData nextDrillData = null;

        public override void ForceUpdate(BuildingPopupData data)
        {
            base.DataRefresh(data);
            Refresh();
        }

        protected override void InitProduct()
        {
            var mineInfo = MiningManager.Instance.MineBuildingInfo;
            if (mineInfo == null)
                return;

            var curLevel = mineInfo.Level;
            var nextLevel = mineInfo.Level + 1;

            currentProductData = ProductData.GetProductDataByGroupAndLevel(MiningManager.MINE_BUILDING_GROUP_KEY, curLevel);
            nextProductData = ProductData.GetProductDataByGroupAndLevel(MiningManager.MINE_BUILDING_GROUP_KEY, nextLevel);

            currentDrillData = MineDrillData.GetMineDrillDataByLevel(curLevel);
            nextDrillData = MineDrillData.GetMineDrillDataByLevel(nextLevel);

            currentAmountText.text = string.Format(StringData.GetStringByIndex(100000241), currentProductData == null ? 0 : currentProductData.ProductItem.Amount);//{0}개
            nextAmountText.text = string.Format(StringData.GetStringByIndex(100000241), nextProductData == null ? 0 : nextProductData.ProductItem.Amount);

            currentTimeText.text = currentProductData == null ? "00:00:00" : SBFunc.TimeString(currentProductData.PRODUCT_TIME);
            nextTimeText.text = nextProductData == null ? "00:00:00" : SBFunc.TimeString(nextProductData.PRODUCT_TIME);

            currentDurabilityAmountText.text = currentDrillData == null ? "0" : currentDrillData.MINE_DURABILITY.ToString();
            nextDurabilityAmountText.text = nextDrillData == null ? "0" : nextDrillData.MINE_DURABILITY.ToString();
        }

        public override void SetTownCondition()
        {
            base.SetTownCondition();

            if (townConditionText != null)
            {
                townConditionText.text = StringData.GetStringFormatByStrKey("광산업그레이드조건", Data.LevelData.NEED_AREA_LEVEL);
                townConditionText.color = isSufficientTown ? Color.black : Color.red;
            }
        }

        void Refresh()
        {

        }
    }
}

