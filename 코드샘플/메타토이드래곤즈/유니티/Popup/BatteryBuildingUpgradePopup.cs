using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class BatteryBuildingUpgradePopup : BuildingUpgradePopup
    {   
        public Text currentTimeText = null;
        public Text nextTimeText = null;

        public Text currentAmountText = null;
        public Text nextAmountText = null;

        ProductAutoData currentProductData = null;
        ProductAutoData nextProductData = null;

        public override void ForceUpdate(BuildingPopupData data)
        {
            base.DataRefresh(data);
            Refresh();
        }

        protected override void InitProduct()
        {
            currentProductData = (ProductAutoData)Data.Product;//ProductAutoData.GetProductDataByGropuAndLevel(Data.OpenData.BUILDING, Data.Level);
            nextProductData = (ProductAutoData)Data.NextProduct;//ProductAutoData.GetProductDataByGropuAndLevel(Data.OpenData.BUILDING, Data.Level + 1);

            currentAmountText.text = string.Format(StringData.GetStringByIndex(100000241), (currentProductData.MAX_TIME / currentProductData.TERM) * currentProductData.ProductItem.Amount);
            nextAmountText.text = string.Format(StringData.GetStringByIndex(100000241), (nextProductData.MAX_TIME / nextProductData.TERM) * nextProductData.ProductItem.Amount);

            currentTimeText.text = SBFunc.TimeString(currentProductData.MAX_TIME);
            nextTimeText.text = SBFunc.TimeString(nextProductData.MAX_TIME);
        }

        void Refresh()
        {

        }

    }
}