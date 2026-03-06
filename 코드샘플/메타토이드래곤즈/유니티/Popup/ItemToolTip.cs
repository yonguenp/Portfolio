using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class ItemToolTip : ToolTip
    {

        

        public static ItemToolTip OnItemToolTip(ShopGoodsData data, GameObject targetObject)
        {
            string itemName = data.Name;
            string itemDESC = data.Desc;
            
            Vector2 screenPos = Camera.main.WorldToScreenPoint(targetObject.transform.position);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(UICanvas.Instance.GetComponent<Canvas>().GetComponent<RectTransform>(), screenPos, Camera.main, out Vector2 localPos);
            var parent = PopupManager.Instance.Beacon;
            var beaconScale = parent.transform.localScale;
            bool reverseFlag = !(localPos.x < 800 * beaconScale.x);//640 기준 일단 하드코딩(1280/2) -> 2배로 늘림
            bool upDownFlag = !(localPos.y < 360 * beaconScale.y);
            ItemFrame frame = null;
            frame = targetObject.GetComponent<ItemFrame>();
            var tooltip = PopupManager.OpenPopup<ItemToolTip>(new TooltipPopupData(new ItemToolTipData(StringData.GetStringByStrKey(itemName), StringData.GetStringByStrKey(itemDESC), targetObject, reverseFlag, upDownFlag, eToolTipDataFlag.Default, frame)));
            tooltip.SetRateBtn(data.REWARDS[0].ItemNo);
            return tooltip;
        }

        public static ItemToolTip OnItemToolTip(int itemNo, GameObject targetObject)
        {
            var itemData =ItemBaseData.Get(itemNo);
            string itemName = itemData.NAME;
            string itemDESC = itemData.DESC;

            Vector2 screenPos = Camera.main.WorldToScreenPoint(targetObject.transform.position);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(UICanvas.Instance.GetComponent<Canvas>().GetComponent<RectTransform>(), screenPos, Camera.main, out Vector2 localPos);
            var parent = PopupManager.Instance.Beacon;
            var beaconScale = parent.transform.localScale;
            bool reverseFlag = !(localPos.x < 800 * beaconScale.x);//640 기준 일단 하드코딩(1280/2) -> 2배로 늘림
            bool upDownFlag = !(localPos.y < 360 * beaconScale.y);
            ItemFrame frame = null;
            frame = targetObject.GetComponent<ItemFrame>();
            var tooltip = PopupManager.OpenPopup<ItemToolTip>(new TooltipPopupData(new ItemToolTipData(StringData.GetStringByStrKey(itemName), StringData.GetStringByStrKey(itemDESC), targetObject, reverseFlag, upDownFlag, eToolTipDataFlag.Default, frame)));
            tooltip.SetRateBtn(itemNo);
            return tooltip;

        }

        // 해당 재료 관련 건물 및 씬으로 이동
        public void OnClickMoveButton()
        {
            if (parentObject == null || itemFrameData == null || itemData == null) return;

            string buildingIndex = ProductData.GetBuildingGroupByProductItem(itemData.KEY);
            if (buildingIndex == "" && itemData.KIND == eItemKind.EXP)
            {
                buildingIndex = SBFunc.BATTERY_TYPE_ITEM_KEY;
            }

            SBFunc.MoveScene(() => {
                SBFunc.RequestBuildingPopup(buildingIndex);//건물 건설 팝업 요청
            });
        }

        public override void ClosePopup()
        {
            if (Data == null)
            {
                return;
            }

            var itemToolTipData = Data.TipData as ItemToolTipData;
            if (itemToolTipData != null && itemToolTipData.Frame != null)
            {
                itemToolTipData.Frame.setFrameNormal();
            }
            else if (parentObject != null)
            {
                var itemData = parentObject.GetComponent<ItemFrame>();
                if (itemData != null)
                    itemData.setFrameNormal();
            }
            base.ClosePopup();
        }
    }
}

