using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork 
{
    public class ProductManagePopup : Popup<PopupData>
    {
        const int SCROLL_LOCK_COUNT = 4;

        [Header("Common")]
        [SerializeField] ScrollRect productSlotScroll = null;
        [SerializeField] TableView productSlotTableView = null;
        [SerializeField] GameObject productManageContent = null;

        List<ITableData> produceBuildingList = new List<ITableData>();
        List<ProductManageSlot> productManageSlotList = new List<ProductManageSlot>();

        bool isFirst = false;
        private bool isNetworkState = false;
        // 모두 채우기 관련
        private eProduceOptionFilter produceOption;
        // TableView로 사용으로 인하여 직접 모든 슬롯 및 조건 검사 후 API 전송
        List<ProducesBuilding> buildingList = new List<ProducesBuilding>();

        public eProduceOptionFilter ProduceOption
        {
            get 
            {
                produceOption = (eProduceOptionFilter)PlayerPrefs.GetInt("produce_option", 0);
                return produceOption; 
            }
            set 
            {
                PlayerPrefs.SetInt("produce_option", (int)value);
                produceOption = value; 
            }
        }


        public override void InitUI()
        {
            //ClearPopup();

            if (isFirst == false)
            {
                productSlotTableView?.OnStart();
                isFirst = true;
            }
            isNetworkState = false;
            buildingList = User.Instance.GetAllProducesList(true);
            buildingList = buildingList.OrderByDescending(building => User.Instance.GetUserBuildingInfoByTag(building.Tag).State).ThenBy(building => building.OpenData.BaseData.TYPE).ThenBy(building => building.OpenData.BaseData.BUILDING_ID).ToList();  // 실제 UI 슬롯 오더와 동일하게 처리해야하므로 정렬


            CreateProductManageSlot();

            UIObjectEvent.Event(UIObjectEvent.eEvent.EVENT_HIDE, UIObjectEvent.eUITarget.ALL);

            UICanvas.Instance.StartBackgroundBlurEffect();
        }

        void CreateProductManageSlot()
        {
            if (productSlotScroll == null) return;
            if (productSlotTableView == null) return;

            produceBuildingList.Clear();
            productManageSlotList.Clear();

            produceBuildingList = new List<ITableData>(buildingList);

            productSlotTableView.SetDelegate(
                new TableViewDelegate(produceBuildingList, (GameObject node, ITableData item) => {
                    if (node == null) return;
                    var slotInfo = node.GetComponent<ProductManageSlot>();
                    if (slotInfo == null) return;
                    slotInfo.InitSlot((ProducesBuilding)item, this);
                    productManageSlotList.Add(slotInfo);
                }));

            productSlotTableView.ReLoad();

            productSlotScroll.enabled = produceBuildingList.Count >= SCROLL_LOCK_COUNT;
        }

        void RefreshSlot()
        {
            foreach (ProductManageSlot slot in productManageSlotList)
            {
                slot.RefreshSlot();
            }
        }

        public void RefreshSlotByBuilding(List<string> buildingNameList)
        {
            if (buildingNameList == null || buildingNameList.Count <= 0) return;

            foreach (ProductManageSlot slot in productManageSlotList)
            {
                if (buildingNameList.Contains(slot.ProducesData.OpenData.BUILDING))
                {
                    slot.RefreshSlot();
                }
            }
        }

        void ClearPopup()
        {
            SBFunc.RemoveAllChildrens(productManageContent.transform);

            produceBuildingList.Clear();
            productManageSlotList.Clear();
            buildingList.Clear();
        }

        // todo - 1. 현재 배터리는 제외하고 수령중 /  2. 일괄 수령 시 획득 연출 결정 (현재 보상팝업)  
        public void OnClickGetAllProductButton()
        {
            List<Asset> ItemList = new List<Asset>();

            List<ProducesBuilding> buildingList = User.Instance.GetAllProducesList(true);

            // 수령 가능한 생산품목 리스트 확인
            foreach (ProducesBuilding building in buildingList)
            {
                if (building.Items == null || building.Items.Count <= 0) continue;

                string buildingGroup = BuildingOpenData.GetWithTag(building.Tag).BUILDING;

                foreach (ProducesRecipe productItem in building.Items)
                {
                    if (productItem.State == eProducesState.Complete ||
                        (productItem.State == eProducesState.Ing && productItem.ProductionExp <= TimeManager.GetTime()))
                    {
                        ProductData itemInfo = ProductData.GetProductDataByGroupAndKey(buildingGroup, productItem.RecipeID);
                        ItemList.Add(new Asset(itemInfo.ProductItem));
                    }
                }
            }

            // 수령 가능한 아이템이 없을 경우
            if (ItemList.Count <= 0)
            {
                ToastManager.On(100000812);
                return;
            }

            // 수령 가능한 생산품목 대비 인벤토리 체크
            if (User.Instance.CheckInventoryGetItem(ItemList))
            {
                SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100002077), StringData.GetStringByIndex(100000414), "",
                    () => {
                        //PopupManager.GetPopup<MainPopup>().changeTab(4);
                        PopupManager.OpenPopup<InventoryPopup>();
                    }
                );
                return;
            }

            // 생산 품목 일괄 수령
            WWWForm data = new WWWForm();
            data.AddField("tag", 0);    // 모두받기 태그 : 0
            if (isNetworkState)
            {
                return;
            }
            isNetworkState = true;
            NetworkManager.SendWithCAPTCHA("produce/harvest", data, (jsonObj) =>
            {
                isNetworkState = false;
                if (SBFunc.IsJTokenCheck(jsonObj["rs"]))
                {
                    switch (jsonObj["rs"].Value<int>())
                    {
                        case (int)eApiResCode.OK:
                            RefreshSlot();
                            if (jsonObj.ContainsKey("rewards"))
                            {
                                var rewardList = SBFunc.ConvertSystemRewardDataList(JArray.FromObject(jsonObj["rewards"]));

                                //InventoryIncomeEvent.Send(rewardList, Data.BuildingTag);
                                SystemRewardPopup.OpenPopup(rewardList);        // 일단은 보상팝업으로 대체
                            }
                            break;

                        case (int)eApiResCode.INVENTORY_FULL:
                            SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100002077), StringData.GetStringByIndex(100000414), "",
                                () => 
                                {
                                    PopupManager.OpenPopup<InventoryPopup>();
                                }
                            );
                            break;
                        case (int)eApiResCode.NOTHING_TO_HARVEST:
                            ToastManager.On(100000812);
                            break;
                    }
                }
            }, (string arg) =>
            {
                isNetworkState = false;
            });
        }

        public void OnClickProduceAllProductButton()
        {
            ProductManageProduceOptionData popupData = new ProductManageProduceOptionData(this);
            PopupManager.OpenPopup<ProductManageProduceOptionPopup>(popupData);
        }

        public void ProduceAllProduct()
        {
            // 실제 소모되는 아이템 및 재화 계산
            int totalNeedGold = 0;
            Dictionary<int, int> totalNeedItemDic = new Dictionary<int, int>();

            List<string> resultProduceStringList = new List<string>();
            Dictionary<int, int> resultProduce = new Dictionary<int, int>();

            List<ProducesBuilding> candidateBuildings = new List<ProducesBuilding>(buildingList);
            List<ProducesBuilding> buildings = new List<ProducesBuilding>(candidateBuildings);

            // 생산 결과물 인벤토리 체크
            List<Asset> itemList = new List<Asset>();
            foreach (var building in buildings)
            {
                string buildingGroup = BuildingOpenData.GetWithTag(building.Tag).BUILDING;
                //가용 슬릇 체크
                ProducesBuilding pBuilding = User.Instance.GetProduces(building.Tag);
                if (pBuilding == null || 0 >= pBuilding.Slot || pBuilding.Items == null)
                {
                    candidateBuildings.Remove(building);
                    continue;
                }

                List<ProducesRecipe> queueList = pBuilding.Items;
                
                if (queueList != null)
                {
                    for (int i = 0; i < queueList.Count; ++i)
                    {
                        if (queueList[i].State == eProducesState.Complete)
                        {
                            ProductData itemInfo = ProductData.GetProductDataByGroupAndKey(buildingGroup, queueList[i].RecipeID);
                            if (itemInfo != null)
                            {
                                itemList.Add(new Asset(itemInfo.ProductItem));
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                bool hasEmpty = false;
                for (int index = 0; index < pBuilding.Slot; index++)
                {
                    hasEmpty = queueList.Count <= index;
                    if (!hasEmpty)
                    {
                        if (queueList[index].State == eProducesState.Complete || (queueList[index].State == eProducesState.Ing && queueList[index].ProductionExp <= TimeManager.GetTime()))
                            hasEmpty = true;

                    }

                    if (hasEmpty)
                        break;
                }

                if(!hasEmpty)
                {
                    candidateBuildings.Remove(building);
                }
            }

            if (User.Instance.CheckInventoryGetItem(itemList))
            {
                ToastManager.On(100000640);
                return;
            }

            Dictionary<int, int> itemAmount = new Dictionary<int, int>();
            foreach(var asset in itemList)
            {
                if(asset.GoodType == eGoodType.ITEM)
                {
                    if (!itemAmount.ContainsKey(asset.ItemNo))
                        itemAmount.Add(asset.ItemNo, 0);

                    itemAmount[asset.ItemNo] += asset.Amount;
                }
            }
            int building_index = 0;            
            bool needmore = false;
            while (candidateBuildings.Count > 0)
            {
                ProducesBuilding building = buildings[building_index++];
                BuildingOpenData openData = BuildingOpenData.GetWithTag(building.Tag);
                BuildInfo buildInfo = User.Instance.GetUserBuildingInfoByTag(building.Tag);
                if (openData == null || buildInfo == null)
                {
                    candidateBuildings.Remove(building);
                    continue;
                }

                List<ProductData> filterProductDataList = ProductData.GetProductDataForProduceListByOption(ProduceOption, openData.BUILDING, buildInfo.Level);
                if (filterProductDataList == null || filterProductDataList.Count <= 0)
                {
                    candidateBuildings.Remove(building);
                    continue;
                }

                int product_index = 0;
                while (filterProductDataList.Count > product_index)
                {
                    ProductData filterProductData = filterProductDataList[product_index];

                    if (User.Instance.GOLD < totalNeedGold + filterProductData.NEED_GOLD)
                    {
                        ToastManager.On(100000104);
                        product_index++;
                        continue;
                    }

                    totalNeedGold += filterProductData.NEED_GOLD;

                    // 재료 보유량 확인
                    if (filterProductData.NEED_ITEM.Count > 0)
                    {
                        int sufficientCount = 0;
                        foreach (var needItem in filterProductData.NEED_ITEM)
                        {
                            int need = (totalNeedItemDic.ContainsKey(needItem.ItemNo) ? totalNeedItemDic[needItem.ItemNo] : 0) + needItem.Amount;
                            int has = User.Instance.GetItemCount(needItem.ItemNo) + (itemAmount.ContainsKey(needItem.ItemNo) ? itemAmount[needItem.ItemNo] : 0);
                            if (has >= need)
                            {
                                sufficientCount++;

                                if (totalNeedItemDic.ContainsKey(needItem.ItemNo))
                                {
                                    totalNeedItemDic[needItem.ItemNo] += need;
                                }
                                else
                                {
                                    totalNeedItemDic.Add(needItem.ItemNo, need);
                                }
                            }
                        }

                        if (sufficientCount != filterProductData.NEED_ITEM.Count)
                        {
                            needmore = true;
                            product_index++;
                            continue;
                        }
                    }

                    string produceString = $"{building.Tag}:{filterProductData.KEY}";
                    resultProduceStringList.Add(produceString);
                    //queue 넣기 성공함.
                    break;
                }

                candidateBuildings.Remove(building);
            }

            // 생산 가능한 품목이 있는지 체크
            if (resultProduceStringList.Count <= 0)
            {
                if (needmore)
                {
                    ToastManager.On(100002249);
                }
                else
                {
                    ToastManager.On(100000818);
                }
                return;
            }
            
            string resultString = string.Join(",", resultProduceStringList);

            //생산 시도, 대기열이 꽉차거나 재료가 부족한지 검사
            WWWForm param = new WWWForm();
            param.AddField("produce", resultString);
            if (isNetworkState)
            {
                return;
            }
            isNetworkState = true;
            NetworkManager.Send("produce/enqueuefull", param, (jsonObj) =>
            {
                isNetworkState = false;
                //PopupManager.ForceUpdate<MainPopup>();
                if (SBFunc.IsJTokenCheck(jsonObj["rs"]))
                {
                    switch ((eApiResCode)jsonObj["rs"].Value<int>())
                    {
                        case eApiResCode.OK:
                            if (jsonObj.ContainsKey("rewards"))
                            {
                                var rewardList = SBFunc.ConvertSystemRewardDataList(JArray.FromObject(jsonObj["rewards"]));

                                InventoryIncomeEvent.Send(rewardList, -1);
                            }
                            break;
                        case eApiResCode.INVENTORY_FULL:
                            // 기존 팝업 버전
                            //SystemPopup.OnSystemPopup(StringData.GetString(100000248, "알림"), StringData.GetString(100002077, "가방이 부족하네요?"),
                            //    () => {
                            //        PopupManager.OpenPopup<InventoryPopup>();
                            //    },
                            //    () => {   //나가기

                            //},
                            //    () => {  //나가기

                            //}
                            //);
                            //return;

                            // 현재는 토스트 처리
                            ToastManager.On(100000640);

                            break;
                        case eApiResCode.PRODUCE_SLOT_FULL:
                            ToastManager.On(100000818);
                            break;
                        case eApiResCode.COST_SHORT:
                            SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100002249));
                            break;

                    }
                }

                RefreshSlot();  // 채우기 실패/성공 슬롯이 섞여있으므로 전체 갱신 진행
            }, (string arg) =>
            {
                isNetworkState = false;
            });

            // Slot을 TableView를 이용하여 생성하면 아래 방식을 사용할 수 없으므로 주석처리
            //switch (ProduceOption)
            //{
            //    case eProduceOptionFilter.PRODUCT_TIME_SHORT:
            //    case eProduceOptionFilter.PRODUCT_TIME_LONG:

            //        foreach (ProductManageSlot slot in productManageSlotList)
            //        {
            //            slot.SetAllProductByProduceOption(ProduceOption);
            //        }

            //        break;
            //}
        }

        public override void ClosePopup()
        {
            UICanvas.Instance.EndBackgroundBlurEffect();

            UIObjectEvent.Event(UIObjectEvent.eEvent.EVENT_SHOW, UIObjectEvent.eUITarget.ALL);
            base.ClosePopup();

            // 레드닷 갱신처리 - 액션마다 처리하면 오히려 너무 잦아지므로 일단은 팝업 닫힐 때 갱신처리
            UIManager.Instance.MainPopupUI.RequestUpdateProductReddot();
        }
    }
}