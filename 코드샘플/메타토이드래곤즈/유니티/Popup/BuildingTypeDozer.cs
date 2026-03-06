using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class BuildingTypeDozer : Popup<PopupData>
    {
        /** before Level */
        [SerializeField]
        Text labelBefLevel = null;

        /** after Level */
        [SerializeField]
        Text labelAfLevel = null;

        /** before Product Req Time */
        [SerializeField]
        Text labelBefReqProductTime = null;

        /** after Product Req Time */
        [SerializeField]
        Text labelAfReqProductTime = null;

        /** before Product */
        [SerializeField]
        Text labelBefBatProduct = null;

        /** after Product */
        [SerializeField]
        Text labelAfBatProduct = null;

        /** before Product */
        [SerializeField]
        Text labelBefGoldProduct = null;

        /** after Product */
        [SerializeField]
        Text labelAfGoldProduct = null;

        /** before Product */
        [SerializeField]
        Text labelBefGemstoneProduct = null;

        /** after Product */
        [SerializeField]
        Text labelAfGemstoneProduct = null;

        /** 재료 부모 노드 */
        [SerializeField]
        GameObject materialParentNode = null;

        /** 소요시간 라벨 */
        [SerializeField]
        Text labelReqTime = null;

        /** 재화 라벨 */
        [SerializeField]
        Text labelCost = null;

        [SerializeField]
        Text labelBatPropertyTitle = null;

        [SerializeField]
        Text labelGoldPropertyTitle = null;

        [SerializeField]
        Text labelGemstonePropertyTitle = null;

        [SerializeField]
        Button btnOK = null;

        private int buildingTag;

        int sufficientMaterialCount = 0;
        bool isSufficientGold = false;
        bool isSufficientMaterial = false;
        private bool isNetworkState = false;

        BuildingLevelData levelData = null;
        /**
         * 
         * @param title building_base 의 index
         * @param body 건물의 고유번호, 최초 건설시 공백
         */
        public void setMessage(string title, string body)
        {
            SetDefaultData(title, body);
            RefreshNeedItem();
        }

        void SetDefaultData(string title, string body)
        {
            buildingTag = int.Parse(body);
            isNetworkState = false;
            BuildInfo buildingInstance = User.Instance.GetUserBuildingInfoByTag(buildingTag);
            DozerAutoProductInfo curBuildingInfo = new DozerAutoProductInfo(buildingInstance.Level);
            DozerAutoProductInfo newBuildingInfo = new DozerAutoProductInfo(buildingInstance.Level + 1);

            BuildingLevelData levelInfo = BuildingLevelData.GetDataByGroupAndLevel(title, buildingInstance.Level);
            levelData = levelInfo;

            labelBefLevel.text = string.Format(StringData.GetStringByIndex(100000056), curBuildingInfo.Level);
            labelAfLevel.text = string.Format(StringData.GetStringByIndex(100000056), newBuildingInfo.Level);

            labelBefBatProduct.text = string.Format(StringData.GetStringByIndex(100000241), curBuildingInfo.Item.ProductItem.Amount);
            labelAfBatProduct.text = string.Format(StringData.GetStringByIndex(100000241), newBuildingInfo.Item.ProductItem.Amount);
            labelBefGoldProduct.text = string.Format(StringData.GetStringByIndex(100000241), curBuildingInfo.Gold.ProductItem.Amount); // 100000254
            labelAfGoldProduct.text = string.Format(StringData.GetStringByIndex(100000241), newBuildingInfo.Gold.ProductItem.Amount); // 100000254
            //labelBefGemstoneProduct.text = string.Format(StringData.GetStringByIndex(100000241), curBuildingInfo.Gemstone?.ProductItem.Amount);
            //labelAfGemstoneProduct.text = string.Format(StringData.GetStringByIndex(100000241), newBuildingInfo.Gemstone?.ProductItem.Amount);

            labelBefReqProductTime.text = SBFunc.TimeString(curBuildingInfo.Item.MAX_TIME);
            labelAfReqProductTime.text = SBFunc.TimeString(newBuildingInfo.Item.MAX_TIME);

            if (labelBatPropertyTitle != null)
                labelBatPropertyTitle.text = string.Format(StringData.GetStringByIndex(100000948), curBuildingInfo.Item.TERM / 60);
            if (labelGoldPropertyTitle != null)
                labelGoldPropertyTitle.text = string.Format(StringData.GetStringByIndex(100000110), curBuildingInfo.Gold.TERM / 60);
            //if (labelGemstonePropertyTitle != null)
            //    labelGemstonePropertyTitle.text = string.Format(StringData.GetStringByIndex(100000110), curBuildingInfo.Gemstone?.TERM / 60);

            labelReqTime.text = SBFunc.TimeString(levelData.UPGRADE_TIME);
        }

        void RefreshNeedItem()
        {
            SBFunc.RemoveAllChildrens(materialParentNode.transform);

            sufficientMaterialCount = 0;

            for (var i = 0; i < levelData.NEED_ITEM.Count; i++)
            {
                var clone = Instantiate(ResourceManager.GetResource<GameObject>(eResourcePath.PrefabClonePath, "itemPrefab"), materialParentNode.transform);
                clone.GetComponent<ItemFrame>().setFrameRecipeInfo(levelData.NEED_ITEM[i].ItemNo, levelData.NEED_ITEM[i].Amount);
                if (clone.GetComponent<ItemFrame>().IsSufficientAmount)
                {
                    sufficientMaterialCount++;
                }
            }

            //업그레이드 시간, 재료 수량 추가
            if (levelData.COST_NUM > 0)
            {
                //Cost 타입에 따라 Cost 아이콘 변경
                labelCost.text = levelData.COST_NUM.ToString();

                isSufficientGold = User.Instance.GOLD >= levelData.COST_NUM;
                isSufficientMaterial = levelData.NEED_ITEM.Count == sufficientMaterialCount;

                var totalSufficient = isSufficientGold && isSufficientMaterial;
                SetColorCostLabel();
                if (btnOK != null)
                    btnOK.SetButtonSpriteState(totalSufficient);
            }
        }


        void SetColorCostLabel()
        {
            if(labelCost != null)
                labelCost.color = isSufficientGold ? new Color(255, 255, 255) : new Color(255, 0, 0);
        }

        public void onClickLevelUp()
        {
            if(!isSufficientGold)
            {
                ToastManager.On(100000646);
                return;
            }

            if(!isSufficientMaterial)
            {
                if (levelData == null)
                    return;
                var needItemList = SBFunc.GetNeedItemList(levelData.NEED_ITEM);
                if (needItemList.Count <= 0)
                {
                    ToastManager.On(100000646);
                    return;
                }
                
                ProductsBuyNowPopup.OpenPopup(needItemList, () => 
                {
                    RefreshNeedItem();
                });

                return;
            }

            WWWForm data = new WWWForm();
            data.AddField("tag", buildingTag);
            if (isNetworkState)
            {
                return;
            }
            isNetworkState = true;
            NetworkManager.Send("building/levelup", data, (jsonObj) =>
            {
                isNetworkState = false;
                if (jsonObj.ContainsKey("rs") && (eApiResCode)jsonObj["rs"].Value<int>() == eApiResCode.OK)
                {
                    PopupManager.ForceUpdate<LandMarkPopup>();
                }
                else if (jsonObj.ContainsKey("rs") && (eApiResCode)jsonObj["rs"].Value<int>() == eApiResCode.BUILDING_QUEUE_NOT_EMPTY)
                {
                    SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100000555));
                }

                ClosePopup();
                Town.Instance.RefreshMap();
            },
            (string arg) =>
            {
                isNetworkState = false;
            });
        }

        public override void InitUI() { }

        public override void ClosePopup()
        {
            SBFunc.RemoveAllChildrens(materialParentNode.transform);
            base.ClosePopup();
        }
    }
}
