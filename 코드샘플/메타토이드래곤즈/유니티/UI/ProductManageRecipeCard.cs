using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class ProductManageRecipeCard : MonoBehaviour
    {
        [SerializeField] ItemFrame productItemFrame = null;
        [SerializeField] Button recipeFrameButton = null;

        [Header("Auto Product")]
        [SerializeField] Button autoProductButton = null;
        [SerializeField] Text autoProductText = null;

        [Header("Block Layer")]
        [SerializeField] GameObject blockLayerObject = null;
        [SerializeField] Text blockLayerText = null;

        ProductManageSlot parentSlot = null;

        public ProductData CurrentProductData { get; private set; } = null;
        public ProductAutoData CurrentProductAutoData { get; private set; } = null;

        BuildInfo buildInfoData = null;
        bool isNetworkState = false;

        public delegate void func(JObject _jobject);
        func successCallback = null;

        public void InitRecipeCard(ProductData data, BuildInfo buildInfo, ProductManageSlot parent, func _successCallback = null)
        {
            CurrentProductData = data;
            buildInfoData = buildInfo;
            parentSlot = parent;
            successCallback = _successCallback;

            //productItemFrame.SetFrameProductInfo(CurrentProductData.ProductItem.ItemNo, CurrentProductData.ICON, CurrentProductData.ProductItem.Amount);

            int userItemCnt = User.Instance.GetItemCount(CurrentProductData.ProductItem.ItemNo);
            productItemFrame.SetFrameProductInfo(CurrentProductData.ProductItem.ItemNo, CurrentProductData.ICON_SPRITE, userItemCnt);

            RefreshAutoProductState();
            isNetworkState = false;

            // 생산 가능 여부 처리
            bool availProduce = buildInfoData.Level >= CurrentProductData.BUILDING_LEVEL;

            if (availProduce == false)  // 생산 불가할 경우 문구 세팅
            {
                blockLayerText.text = string.Format(StringData.GetStringByIndex(100000130), "", CurrentProductData.BUILDING_LEVEL);
            }

            autoProductButton.gameObject.SetActive(availProduce);
            blockLayerObject.SetActive(!availProduce);
        }

        public void InitAutoRecipeCard(ProductAutoData data, BuildInfo buildInfo, ProductManageSlot parent, func _successCallback = null)
        {
            CurrentProductAutoData = data;
            buildInfoData = buildInfo;
            parentSlot = parent;
            successCallback = _successCallback;

            productItemFrame.SetFrameProductInfo(CurrentProductAutoData.ProductItem.ItemNo, CurrentProductAutoData.ProductItem.BaseData.ICON_SPRITE, CurrentProductAutoData.ProductItem.Amount);

            recipeFrameButton.interactable = false;
            autoProductButton.gameObject.SetActive(false);
        }

        public void OnClickProduct(bool isAll = false)
        {
            if (isNetworkState) return;
            //가용 슬릇 체크
            ProducesBuilding pBuilding = User.Instance.GetProduces(buildInfoData.Tag);
            List<ProducesRecipe> queueList = pBuilding != null ? pBuilding.Items : null;
            int usageSlot = 0;

            for (int i = 0; i < pBuilding.Slot; i++)
            {
                if (queueList.Count <= i || // [1]
                    queueList[i].State == eProducesState.Complete ||    // [2]
                    (queueList[i].State == eProducesState.Ing && queueList[i].ProductionExp <= TimeManager.GetTime()))  // [3]
                {
                    // [1] 최대 슬릇을 채우지 않았거나,
                    // [2] 완료된 슬릇이 있거나,
                    // [3] 통신 후 완료로 찍힐 예상 생산완료 슬릇이 있거나
                    // 가용 슬릇으로 인정
                    ++usageSlot;
                }
            }

            if (usageSlot == 0)
            {
                ToastManager.On(100000818);
                return;
            }


            if (isAll)
            {  // 롱 터치일 경우 처리
                List<Asset> itemList = new List<Asset>();
                if (queueList != null)
                {
                    for (int i = 0; i < queueList.Count; ++i)
                    {
                        if ((int)queueList[i].State == 3)
                        {
                            itemList.Add(new Asset(CurrentProductData.ProductItem));
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                if (User.Instance.CheckInventoryGetItem(itemList))
                {
                    SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100002077),
                        () => {
                            PopupManager.OpenPopup<InventoryPopup>();
                        },
                        () => {   //나가기
                        },
                        () => {  //나가기
                        }
                    );
                    return;
                }
            }

            // 골드 보유량 확인
            if (User.Instance.GOLD < CurrentProductData.NEED_GOLD)
            {
                ToastManager.On(100000104);
                return;
            }

            // 재료 보유량 확인
            if (CurrentProductData.CheckRecipeNeedItem() == false)
            {
                var requestCount = isAll ? usageSlot : 1;
                if (!isSufficientItemByCount(requestCount))//가용 생산 슬롯 사이즈 - usageSlot // 가용 슬롯과 다른지 확인해봐야함.
                {
                    var availableCount = GetAvailableProductCount();//현재 내가 생산 가능한 만큼의 슬롯
                    if (availableCount < usageSlot)//슬롯이 여유로움
                    {
                        var remainSlotCount = usageSlot - availableCount;
                        if (remainSlotCount > requestCount)//요청 갯수가 더 적으면 요청갯수로 세팅
                            remainSlotCount = requestCount;

                        var needItemList = SBFunc.GetNeedItemList(CurrentProductData.NEED_ITEM, remainSlotCount,true);//즉시생산 팝업에 필요한 재료 계산
                        if (!IsSufficientItem())//애초부터 재료가 없는 상태의 요청 - 서버 처리 필요없이 클라에서 바로 팝업 요청 추가
                        {
                            ProductsBuyNowPopup.OpenPopup(needItemList, () => {
                                RequestEnqueueItem(isAll);//다 사면 재시도
                            },false, (index)=> {
                                parentSlot?.RefreshSlotByBuilding(CurrentProductData);
                            });
                        }
                        else
                        {
                            RequestEnqueueItem(isAll, () =>
                            {
                                //현재의 인벤토리갯수를 체크하기 때문에 서버쪽 처리 완료 된후에 팝업 생성
                                ProductsBuyNowPopup.OpenPopup(needItemList, () => {
                                    RequestEnqueueItem(isAll);//다 사면 재시도
                                }, false, (index) => {
                                    parentSlot?.RefreshSlotByBuilding(CurrentProductData);
                                });
                            });//서버쪽에서 가용슬롯 만큼 넣어줌
                        }
                        return;
                    }
                }
            }

            RequestEnqueueItem(isAll);//낱개 & 전체 요청

        }
        void RequestEnqueueItem(bool isAll, VoidDelegate _callback = null)
        {
            //생산 시도, 대기열이 꽉차거나 재료가 부족한지 검사
            WWWForm param = new WWWForm();
            param.AddField("tag", buildInfoData.Tag);
            param.AddField("recipe", CurrentProductData.KEY);
            param.AddField("all", isAll ? 1 : 0);
            isNetworkState = true;
            NetworkManager.Send("produce/enqueue", param, (jsonObj) =>
            {
                //PopupManager.ForceUpdate<MainPopup>();
                if (SBFunc.IsJTokenCheck(jsonObj["rs"]))
                {
                    if ((eApiResCode)jsonObj["rs"].Value<int>() == eApiResCode.INVENTORY_FULL)
                    {
                        //이거 넣으면 가방 터짐
                        SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100002077),
                            () => {
                                PopupManager.OpenPopup<InventoryPopup>();
                            },
                            () => {   //나가기

                            },
                            () => {  //나가기

                            }
                        );
                        return;
                    }
                    else if ((eApiResCode)jsonObj["rs"].Value<int>() == eApiResCode.PRODUCE_SLOT_FULL)
                    {
                        ToastManager.On(100000818);
                    }
                    else
                    {
                        parentSlot?.RefreshSlotByBuilding(CurrentProductData);

                        successCallback?.Invoke(jsonObj);

                        if (_callback != null)
                            _callback();
                    }
                }
                isNetworkState = false;
            }, (string arg) =>
            {
                isNetworkState = false;
            });
        }
        void RefreshAutoProductState()
        {
            if (autoProductButton == null || autoProductText == null) return;

            ProducesBuilding currentProduceBuilding = User.Instance.GetProduces(buildInfoData.Tag);

            if (currentProduceBuilding != null)
            {
                int remainCount = currentProduceBuilding.GetRemainProductQueueSize();

                autoProductButton.SetInteractable(remainCount > 0);
                autoProductText.text = SBFunc.StrBuilder("x ", remainCount);
            }
        }
        /// <summary>
        /// 현재 요구 갯수만큼 생산이 가능한 컨디션인가
        /// </summary>
        /// <param name="_count"></param>
        /// <returns></returns>
        bool isSufficientItemByCount(int _count)
        {
            if (CurrentProductData == null)
                return false;

            var needItemList = CurrentProductData.NEED_ITEM;
            if (needItemList == null || needItemList.Count <= 0)
                return false;

            int sufficientCount = 0;
            foreach (var needItem in needItemList)
            {
                if (needItem == null)
                    continue;

                var designData = ItemBaseData.Get(needItem.ItemNo);
                if (designData == null)
                    continue;

                if (designData.USE == false || designData.BUY == 0)
                    continue;

                var needItemNo = needItem.ItemNo;
                var needOnceAmount = needItem.Amount;
                var totalRequireCount = needOnceAmount * _count;
                var curItemAmount = User.Instance.GetItemCount(needItemNo);

                if (totalRequireCount <= curItemAmount)
                    sufficientCount++;
            }

            return needItemList.Count == sufficientCount;
        }
        int GetAvailableProductCount()//현재 아이템 생산 가능한 최대 갯수
        {
            if (CurrentProductData == null)
                return 0;

            var needItemList = CurrentProductData.NEED_ITEM;
            if (needItemList == null || needItemList.Count <= 0)
                return 0;

            List<int> itemCheckCountList = new List<int>();
            foreach (var needItem in needItemList)
            {
                if (needItem == null)
                    continue;

                var designData = ItemBaseData.Get(needItem.ItemNo);
                if (designData == null)
                    continue;

                if (designData.USE == false || designData.BUY == 0)
                    continue;

                var needItemNo = needItem.ItemNo;
                var needOnceAmount = needItem.Amount;
                var curItemAmount = User.Instance.GetItemCount(needItemNo);

                itemCheckCountList.Add(curItemAmount / needOnceAmount);
            }

            return itemCheckCountList.Min();
        }

        bool IsSufficientItem()//초기 재료 카운트
        {
            if (CurrentProductData == null)
                return false;

            var needItemList = CurrentProductData.NEED_ITEM;
            if (needItemList == null || needItemList.Count <= 0)
                return false;

            int sufficientCount = 0;
            foreach (var needItem in needItemList)
            {
                if (needItem == null)
                    continue;

                var designData = ItemBaseData.Get(needItem.ItemNo);
                if (designData == null)
                    continue;

                var needItemNo = needItem.ItemNo;
                var needOnceAmount = needItem.Amount;
                var curItemAmount = User.Instance.GetItemCount(needItemNo);

                if (curItemAmount >= needOnceAmount)
                    sufficientCount++;
            }

            return sufficientCount == needItemList.Count;
        }
    }
}