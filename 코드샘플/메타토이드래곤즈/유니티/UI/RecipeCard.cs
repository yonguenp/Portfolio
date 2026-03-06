using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class RecipeCard : MonoBehaviour
    {
        [SerializeField] ItemFrame productItemFrame = null;
        [SerializeField] GameObject materialNodeParent = null;
        [SerializeField] Text productNameText = null;
        [SerializeField] Text productReqTimeText = null;
        [SerializeField] GameObject blockNode = null;
        [SerializeField] Text blockText = null;
        [SerializeField] Button btnProduction = null;
        [SerializeField] Button btnAutoProduction = null;

        private int buildingTAG;
        private List<int> receipeIDList = new List<int>();

        ProductData curProductData = null;
        public delegate void func(JObject _jobject);
        private func successCallback = null;
        private VoidDelegate refreshUICallback = null;

        bool isSufficientItem = false;
        int itemCompleteCount = 0;

        private List<ItemFrame> MaterialItemList = new List<ItemFrame>();

        bool isNetworkState = false;

        public void Init(ProductData data, int targetBuildingTAG, bool canProduct, func _successCallback, int itemCompleteCount, VoidDelegate _refreshUICallback = null)
        {
            receipeIDList.Clear();
            //receipeID를 기준으로 레시피 테이블에서 해당 레시피에 필요한 정보들을 가져와 세팅
            curProductData = data;
            if (curProductData == null)
            {
                Debug.LogError("RecipeCard.init.receipeInfo == null");
                Destroy(gameObject);
                return;
            }
            isNetworkState = false;
            buildingTAG = targetBuildingTAG;
            receipeIDList.Add(data.KEY);

            if(_successCallback != null)
                successCallback = _successCallback;

            refreshUICallback = _refreshUICallback;


            productItemFrame.SetFrameProductInfo(curProductData.ProductItem.ItemNo, curProductData.ICON_SPRITE, curProductData.ProductItem.Amount, false);
            //productItemFrame.SetFrameItemInfo(receipeInfo.ProductItem.ItemNo, receipeInfo.ProductItem.ItemCount, 1);
            
            string itemName = curProductData.ProductItem.BaseData.NAME;
            if (curProductData.ProductItem.Amount <= 1)
            {
                productNameText.text = itemName;
            }
            else
            {
                productNameText.text = itemName + " x " + curProductData.ProductItem.Amount;
            }

            productReqTimeText.text = SBFunc.TimeString(curProductData.PRODUCT_TIME);

            if (!canProduct)
            {
                blockText.text = string.Format(StringData.GetStringByIndex(100000130), "", curProductData.BUILDING_LEVEL);
            }

            blockNode.gameObject.SetActive(!canProduct);
            SetMaterialItem(canProduct);//재료 리스트 세팅
            SetVisibleAutoProduct(true);
            //일괄 생산 라벨 갱신
            SetItemCompleteCount(itemCompleteCount);
            RefreshCountAutoProduct();
        }

        void AllOffMaterialItem()
        {
            if(MaterialItemList == null)
            {
                MaterialItemList = new List<ItemFrame>();
            }
            foreach(ItemFrame itemFrame in MaterialItemList)
            {
                itemFrame.gameObject.SetActive(false);
            }
        }

        void SetMaterialItem(bool _canProduct)
        {
            GameObject clone = ResourceManager.GetResource<GameObject>(eResourcePath.PrefabClonePath, "itemPrefab");
            if (clone == null)
                return;
            AllOffMaterialItem();
            if (MaterialItemList.Count == 0)  // 0번째 인덱스 골드 표기용
            {
                GameObject obj = Instantiate(clone, materialNodeParent.transform);
                MaterialItemList.Add(obj.GetComponent<ItemFrame>());
            }
            if (curProductData.NEED_GOLD > 0)//골드가 들어가는지? 재료가 들어가는지?
            {
                MaterialItemList[0].gameObject.SetActive(true);
                MaterialItemList[0].setFrameCashInfo((int)eGoodType.GOLD, curProductData.NEED_GOLD, _canProduct);
                //MaterialItemList[0].SetItemBgOff();
            }
            else
            {
                MaterialItemList[0].gameObject.SetActive(false);
            }

            int resultCount = 0;
            for (int i = 0; i < curProductData.needitemLength; i++)
            {
                if(i+1 >= MaterialItemList.Count) { 
                    GameObject obj = Instantiate(clone, materialNodeParent.transform);
                    var itemFrame = obj.GetComponent<ItemFrame>();
                    MaterialItemList.Add(itemFrame.GetComponent<ItemFrame>());
                }
                MaterialItemList[i + 1].gameObject.SetActive(true);
                MaterialItemList[i + 1].setFrameRecipeInfo(curProductData.NEED_ITEM[i].ItemNo, curProductData.NEED_ITEM[i].Amount);
               // MaterialItemList[i + 1].SetItemBgOff();
                resultCount += MaterialItemList[i + 1].IsSufficientAmount ? 1 : 0;
                isSufficientItem = curProductData.needitemLength == resultCount;
            }
        }

        public void InitEnergyType(ProductAutoData data)
        {
            //receipeID를 기준으로 레시피 테이블에서 해당 레시피에 필요한 정보들을 가져와 세팅
            ItemBaseData itemInfo = data.ProductItem.BaseData;

            if (itemInfo == null)
            {
                Debug.LogError("RecipeCard.initEnergyType() => itemInfo == null");
                Destroy(gameObject);
                return;
            }

            productItemFrame.SetFrameItemInfo(data.ProductItem.ItemNo, data.ProductItem.Amount, 1);
            //productItemFrame.SetItemBgOff();
            productNameText.text = itemInfo.NAME;
            productReqTimeText.text = SBFunc.TimeString(data.TERM);
            btnProduction.gameObject.GetComponentInChildren<LocalizeString>().SetIndex(100000071);
            btnProduction.SetInteractable(false);
            btnProduction.GetComponent<Image>().sprite = btnProduction.spriteState.disabledSprite;

			blockNode.SetActive(false);

            SetVisibleAutoProduct(false);
        }

        public void OnClickProduct(int isAll = 0)
        {
            if (isNetworkState) return;
			//가용 슬릇 체크
			ProducesBuilding pBuilding = User.Instance.GetProduces(buildingTAG);
			List<ProducesRecipe> queueList = pBuilding != null ? pBuilding.Items : null;
			int usageSlot = 0;
			
			for(int i = 0; i < pBuilding.Slot; i++)
			{
				if(	queueList.Count <= i ||	// [1]
					queueList[i].State == eProducesState.Complete ||	// [2]
					(queueList[i].State == eProducesState.Ing && queueList[i].ProductionExp <= TimeManager.GetTime()))	// [3]
				{
					// [1] 최대 슬릇을 채우지 않았거나,
					// [2] 완료된 슬릇이 있거나,
					// [3] 통신 후 완료로 찍힐 예상 생산완료 슬릇이 있거나
					// 가용 슬릇으로 인정
					++usageSlot;
				}
			}

			if(usageSlot == 0)
			{
				ToastManager.On(100000818);
				return;
			}

            if (isAll != 0)
            {  // 롱 터치일 경우 처리
                List<Asset> itemList = new List<Asset>();
                if (queueList != null)
                {
                    for (int i = 0; i < queueList.Count; ++i)
                    {
                        if ((int)queueList[i].State == 3)
                        {
                            itemList.Add(new Asset(curProductData.ProductItem));
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
            if (User.Instance.GOLD < curProductData.NEED_GOLD)
            {
                ToastManager.On(100000104);
                return;
            }

            // 재료 보유량 확인
            if (curProductData.needitemLength > 0)
            {
                var requestCount = isAll == 1 ? GetRemainProductQueueSize() : 1;
                if (!isSufficientItemByCount(requestCount))//가용 생산 슬롯 사이즈 - usageSlot // 가용 슬롯과 다른지 확인해봐야함.
                {
                    var availableCount = GetAvailableProductCount();//현재 내가 생산 가능한 만큼의 슬롯
                    if (availableCount < usageSlot)//슬롯이 여유로움
                    {
                        var remainSlotCount = usageSlot - availableCount;
                        if (remainSlotCount > requestCount)//요청 갯수가 더 적으면 요청갯수로 세팅
                            remainSlotCount = requestCount;

                        var needItemList = SBFunc.GetNeedItemList(curProductData.NEED_ITEM, remainSlotCount, true);//즉시생산 팝업에 필요한 재료 계산
                        if(!isSufficientItem)//애초부터 재료가 없는 상태의 요청 - 서버 처리 필요없이 클라에서 바로 팝업 요청 추가
                        {
                            ProductsBuyNowPopup.OpenPopup(needItemList, () => {
                                RequestEnqueueItem(isAll);//다 사면 재시도
                            }, false, (index) => {
                                if (refreshUICallback != null)
                                    refreshUICallback();
                            });
                        }
                        else
                        {
                            //현재의 인벤토리갯수를 체크하기 때문에 서버쪽 처리 완료 된후에 팝업 생성
                            RequestEnqueueItem(isAll, () =>
                            {
                                ProductsBuyNowPopup.OpenPopup(needItemList, () => {
                                    RequestEnqueueItem(isAll);//다 사면 재시도
                                }, false, (index) => {
                                    if (refreshUICallback != null)
                                        refreshUICallback();
                                });
                            });//서버쪽에서 가용슬롯 만큼 넣어줌
                        }
                        return;
                    }
                }
            }

            RequestEnqueueItem(isAll);//낱개 & 전체 요청
        }

        void RequestEnqueueItem(int isAll ,  VoidDelegate _callback = null)
        {
            //생산 시도, 대기열이 꽉차거나 재료가 부족한지 검사
            WWWForm param = new WWWForm();
            param.AddField("tag", buildingTAG);
            param.AddField("recipe", receipeIDList[0]);
            param.AddField("all", isAll);
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
                        if (successCallback != null)//외부
                            successCallback(jsonObj);

                        if (_callback != null)//내부
                            _callback();
                    }
                }
                isNetworkState = false;
            }, (string arg) =>
            {
                isNetworkState = false;
            });
        }

        /// <summary>
        /// 현재 요구 갯수만큼 생산이 가능한 컨디션인가
        /// </summary>
        /// <param name="_count"></param>
        /// <returns></returns>
        bool isSufficientItemByCount(int _count)
        {
            if (curProductData == null)
                return false;

            var needItemList = curProductData.NEED_ITEM;
            if (needItemList == null || needItemList.Count <= 0)
                return false;

            int sufficientCount = 0;
            foreach(var needItem in needItemList)
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
            if (curProductData == null)
                return 0;

            var needItemList = curProductData.NEED_ITEM;
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
        

        public void SetVisibleAutoProduct(bool _isVisible)//일괄 생산 버튼 state 갱신
        {
            if (btnAutoProduction != null)
                btnAutoProduction.gameObject.SetActive(_isVisible);
        }

        public void SetItemCompleteCount(int _count)
        {
            itemCompleteCount = _count;
        }

        public int GetRemainProductQueueSize()//일괄 넣기 추가 슬롯
        {
            int queueLength = User.Instance.GetProduces(buildingTAG) != null ? User.Instance.GetProduces(buildingTAG).Slot : 4;
            List<ProducesRecipe> queueList = User.Instance.GetProduces(buildingTAG) != null ? User.Instance.GetProduces(buildingTAG).Items : null;
            int queueListCount = queueList == null ? 0 : queueList.Count;

            return queueLength - queueListCount + itemCompleteCount;//빈슬롯 + 제작 완료한 아이템 갯수
        }
        public void RefreshCountAutoProduct()
        {
            if (btnAutoProduction == null)
                return;

            btnAutoProduction.GetComponentInChildren<Text>().text = SBFunc.StrBuilder("x", GetRemainProductQueueSize());
            btnAutoProduction.SetInteractable(GetRemainProductQueueSize() > 0);
        }

        public void SetActiveDimmedBlock(bool visible,string blockString = "")
        {
            if (blockNode.activeInHierarchy == visible)
            {
                return;
            }

            blockNode.gameObject.SetActive(visible);

            Transform lockSpriteNode = SBFunc.GetChildrensByName(blockNode.transform, new string[] { "lock" });
            if (lockSpriteNode != null)
            {
                lockSpriteNode.position = new Vector3(0, 0, 0);
            }

            blockText.text = blockString;
        }
    }
}

