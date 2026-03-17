using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class MiningDrillRepairPopup : Popup<PopupData>
    {
        [SerializeField] ItemFrame item = null;
        [SerializeField] Button repairButton = null;

        MineDrillData drillTableData = null;

        public override void InitUI()
        {
            //현재 레벨에 따른 드릴 수리 요구량 구하기
            var drillData = MiningManager.Instance.GetUserDrillData();
            if (drillData == null)
            {
                Debug.LogError("MineDrillData is null , target Level : " + drillData.LEVEL);
                ClosePopup();
                return;
            }

            drillTableData = drillData;
            SetRepairItem();
            RefreshRepairButton();
        }

        void RefreshRepairButton()
        {
            if (repairButton != null)
                repairButton.SetButtonSpriteState(IsRepairCondition());
        }

        void SetRepairItem()
        {
            var targetItem = drillTableData.REPAIR_COST_ITEM;
            if (item != null)
                item.setFrameRecipeInfo(targetItem.ItemNo, targetItem.Amount);
        }

        bool IsRepairCondition()
        {
            if (item == null || item.GetItemID() <= 0)
                return false;

            return item.IsSufficientAmount;
        }

        /// <summary>
        /// 아이템 부족 노티 팝업으로 이동
        /// </summary>
        public void OnClickRepairItem()
        {
            if (IsRepairCondition())
                SendRepairDrill();
            else
            {
                ClosePopup();
                PopupManager.OpenPopup<MiningDrillRepairInsufficiencyPopup>();//아이템 부족 팝업(바로가기 기능추가) 오픈
            }
        }

        //드릴 수리 요청
        void SendRepairDrill()
        {
            WWWForm data = new WWWForm();
            NetworkManager.Send("mine/repair", data, (jsonData) =>
            {
                JToken resultResponse = jsonData["rs"];
                if (resultResponse != null && resultResponse.Type == JTokenType.Integer)
                {
                    int rs = resultResponse.Value<int>();
                    switch ((eApiResCode)rs)
                    {
                        case eApiResCode.OK:
                            MiningPopupEvent.RequestDrillRepair(jsonData);// 갱신 요청

                            ClosePopup();
                            break;
                    }
                }
            });
        }
    }
}


