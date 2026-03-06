using DG.Tweening;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class ProductManageSlot : MonoBehaviour
    {
        [SerializeField] GameObject blockLayerObject = null;
        [SerializeField] GameObject getItemAnimLayer = null;

        [Header("Product Item Layer")]
        [SerializeField] ScrollRect productItemScroll = null;
        [SerializeField] Text buildingNameText = null;
        [SerializeField] Text buildingLevelText = null;
        [SerializeField] GameObject productItemContentsObject = null;

        [SerializeField] GameObject remainTimeLayerObject = null;
        [SerializeField] Text remainTimeText = null;
        [SerializeField] TimeObject remainTimeObject = null;


        [Header("Product All Accel")]
        [SerializeField] Text remainSumTimeText = null;
        [SerializeField] TimeObject remainSumTimeObject = null;
        [SerializeField] Button ProductAllAccelBtn = null;

        const int ITEM_SCROLL_LOCK_COUNT = 10;

        [Header("Product Recipe Layer")]
        [SerializeField] ScrollRect productRecipeScroll = null;
        [SerializeField] GameObject productRecipeContentsObject = null;

        const int RECIPE_SCROLL_LOCK_COUNT = 5;

        // 슬롯추가
        GameObject addSlotObj = null;

        // 데이터
        ProductManagePopup parentPopup = null;

        public ProducesBuilding ProducesData { get; private set; } = null;
        BuildInfo buildInfo = null;

        BuildingBaseData buildingBaseData = null;
        BuildingOpenData buildingOpenData = null;

        List<ProductData> recipeDataList = new List<ProductData>();
        List<ProductAutoData> autoRecipeDataList = new List<ProductAutoData>();

        List<RecipeFrame> currentProductQueue = new List<RecipeFrame>();

        List<ProductManageRecipeCard> recipeCardList = new List<ProductManageRecipeCard>();

        bool isNetworkState = false;

        public int SlotAllEndTime { get; private set; } = 0;
        public bool IsProducting { get; private set; } = false;

        public void InitSlot(ProducesBuilding data, ProductManagePopup parent)
        {
            ProducesData = data;
            parentPopup = parent;

            buildInfo = User.Instance.GetUserBuildingInfoByTag(ProducesData.Tag);
           
            buildingOpenData = BuildingOpenData.GetWithTag(ProducesData.Tag);
            buildingBaseData = buildingOpenData.BaseData;

            ClearSlot();

            SetProductItemLayer();
            SetProductRecipeLayer();

            // 현재 운용할 수 없는 건물 상태에 대한 처리
            blockLayerObject.SetActive(buildInfo.State != eBuildingState.NORMAL);
            isNetworkState = false;
        }

        // 생산 완료 시 갱신
        public void RefreshSlot()
        {
            ClearSlot();

            SetProductItemLayer();
            SetProductRecipeLayer();
        }

        // 생산 취소 시 갱신
        public void RefreshSlotByBuilding(ProductData productData)
        {
            List<string> resultList = GetRecipeBuildingGroupList(productData);
            parentPopup?.RefreshSlotByBuilding(resultList);
        }

        public void OnClickMoveToBuilding()
        {
            PopupManager.ClosePopup<ProductManagePopup>();  // 팝업을 유지하고 싶으면 주석처리
            ProductPopup.OpenPopup(ProducesData.Tag);
        }

        // 한번에 채우기 관련 기능
        public void SetAllProductByProduceOption(eProduceOptionFilter currentOption)
        {
            if (recipeCardList.Count <= 0) return;

            ProductData filterProductData = ProductData.GetProductDataForProduceByOption(currentOption, buildingBaseData.KEY, buildInfo.Level);
            if (filterProductData == null) return;

            ProductManageRecipeCard optionCardData = recipeCardList.Find(card => card.CurrentProductData.KEY == filterProductData.KEY);

            switch (currentOption) 
            {
                case eProduceOptionFilter.PRODUCT_TIME_SHORT:
                    optionCardData.OnClickProduct(true);
                    break;
                case eProduceOptionFilter.PRODUCT_TIME_LONG:
                    optionCardData.OnClickProduct(true);
                    break;
            }
        }

        void SetProductItemLayer()
        {
            buildingNameText.text = StringData.GetStringByStrKey(buildingBaseData._NAME);
            buildingLevelText.text = string.Format(StringData.GetStringByIndex(100000056), buildInfo.Level);

            LayoutRebuilder.ForceRebuildLayoutImmediate(buildingNameText.GetComponent<RectTransform>());

            List<ProducesRecipe> queueList = ProducesData.Items;
            int queueLength = ProducesData.Slot;

            // 생산 큐 리스트 정렬
            if (queueList != null && queueList.Count > 0)
            {
                queueList.Sort((a, b) => b.State - a.State);
            }
            
            IsProducting = false;
            SlotAllEndTime = 0;

            int producingCount = 0; // 현재 생산중인 품목 갯수
            
            int localCheck = 0;
            int localTimeCheck = 0;
            for (int i = 0; i < queueLength; ++i)
            {
                GameObject itemClone = Instantiate(ResourceManager.GetResource<GameObject>(eResourcePath.PrefabClonePath, "recipeTimer"));
                itemClone.transform.SetParent(productItemContentsObject.transform);
                itemClone.transform.localScale = Vector3.one;
                RecipeFrame itemFrame = itemClone.GetComponent<RecipeFrame>();
                itemFrame.OnClickCallback = null;

                if (i < queueList.Count)    // 생산 아이템이 있는 경우
                {
                    if (buildingBaseData.KEY == "exp_battery")
                    {
                        ProductAutoData autoItemInfo = ProductAutoData.GetProductDataByGropuAndLevel(buildingBaseData.KEY, buildInfo.Level);
                        itemFrame.SetReceipeIcon(i, autoItemInfo, localCheck, ProducesData.Tag, RefreshSlot);
                        if (TimeManager.GetTimeCompare(localCheck) > 0 && TimeManager.GetTimeCompare(localCheck) <= autoItemInfo.TERM)
                        {
                            itemFrame.TimerStart();
                        }

                        itemClone.GetComponent<Button>().interactable = false;
                    }
                    else
                    {
                        ProductData itemInfo = ProductData.GetProductDataByGroupAndKey(buildingBaseData.KEY, queueList[i].RecipeID);
                        itemFrame.OnClickCallback = ProductHarvestProcess;
                        currentProductQueue.Add(itemFrame);

                        if (queueList[i].State == eProducesState.Complete)
                        {
                            itemFrame.SetReceipeIcon(i, itemInfo, -1, ProducesData.Tag, RefreshSlot);
                        }
                        else
                        {
                            IsProducting = true;
                            localCheck += itemInfo.PRODUCT_TIME;
                            if (queueList[i].State == eProducesState.Ing)//생산중
                            {
                                localCheck = queueList[i].ProductionExp;
                            }

                            itemFrame.SetReceipeIcon(i, itemInfo, localCheck, ProducesData.Tag, RefreshSlot);
                            if (TimeManager.GetTimeCompare(localCheck) > 0 && TimeManager.GetTimeCompare(localCheck) <= itemInfo.PRODUCT_TIME)
                            {
                                itemFrame.TimerStart();
                                localTimeCheck = localCheck;

                                // 남은 생산 시간 텍스트 관련
                                if (remainTimeObject != null && localCheck - TimeManager.GetTime() > 0)
                                {
                                    remainTimeObject.Refresh = delegate
                                    {
                                        int remainTime = TimeManager.GetTimeCompare(localTimeCheck);
                                        remainTimeText.text = SBFunc.TimeString(remainTime);

                                        if (remainTime <= 0)
                                        {
                                            remainTimeObject.Refresh = null;
                                        }
                                    };
                                }
                            }

                            // 실제 생산 슬롯 계산
                            if (localCheck > TimeManager.GetTime())
                            {
                                producingCount++;
                            }
                        }
                    }
                }
                else // 생산 아이템이 없는 경우 (빈 슬롯)
                {
                    itemFrame.SetFrameBlank();
                }
            }


            SlotAllEndTime = localCheck;
            if(remainSumTimeObject != null && SlotAllEndTime - TimeManager.GetTime() > 0)
            {
                if (IsProducting)
                {
                    ProductAllAccelBtn.SetButtonSpriteState(true);
                    ProductAllAccelBtn.interactable = true;
                }

                remainSumTimeObject.Refresh = delegate
                {
                    int remainTime = TimeManager.GetTimeCompare(SlotAllEndTime);
                    remainSumTimeText.text = SBFunc.TimeString(remainTime);

                    if (remainTime <= 0)
                    {
                        remainSumTimeObject.Refresh = null;

                        IsProducting = false;

                        ProductAllAccelBtn.SetButtonSpriteState(false);
                        ProductAllAccelBtn.interactable = false;
                    }
                };
            }
            else
            {
                ProductAllAccelBtn.SetButtonSpriteState(false);
                ProductAllAccelBtn.interactable = false;
            }


            // 슬롯 추가 관련
            if (buildingBaseData.KEY != "exp_battery")
            {
                if (addSlotObj != null)
                {
                    addSlotObj.GetComponent<EmptySlotFrame>()?.RefreshSlot(ProducesData, buildingBaseData);
                }
                else
                {
                    addSlotObj = Instantiate(ResourceManager.GetResource<GameObject>(eResourcePath.PrefabClonePath, "EmptySlot"));
                    addSlotObj.transform.SetParent(productItemContentsObject.transform);
                    addSlotObj.transform.localScale = Vector3.one;
                    EmptySlotFrame slotFrame = addSlotObj.GetComponent<EmptySlotFrame>();

                    slotFrame?.InitSlot(ProducesData, buildingBaseData, RefreshSlot);
                }
            }

            // 생산 시간 텍스트 on/off
            //remainTimeLayerObject?.SetActive(producingCount > 0);
            //일부 생산 시간 끄기 - WJ - 전체 즉시 완료 처리 되면서 안씀
            remainTimeLayerObject?.SetActive(false);

            //addSlotObj.gameObject.SetActive(buildingBaseData.KEY != "exp_battery");   // 23.05 - 배터리는 생산관리 제외

            // 스크롤 잠금 상태 제어
            productItemScroll.enabled = queueLength >= ITEM_SCROLL_LOCK_COUNT;
        }

        void SetProductRecipeLayer()
        {
            if (buildingBaseData.KEY == "exp_battery")
            {
                autoRecipeDataList = ProductAutoData.GetListByGroupAndLevel(buildingBaseData.KEY, buildInfo.Level);
                autoRecipeDataList.ForEach(autorRecipe => 
                {
                    GameObject newRecipeCard = Instantiate(ResourceManager.GetResource<GameObject>(eResourcePath.PrefabClonePath, "ProductManageRecipeCard"), productRecipeContentsObject.transform);

                    ProductManageRecipeCard recipeCardComponent = newRecipeCard.GetComponent<ProductManageRecipeCard>();
                    if (recipeCardComponent != null)
                    {
                        recipeCardComponent.InitAutoRecipeCard(autorRecipe, buildInfo, this);
                        recipeCardList.Add(recipeCardComponent);
                    }
                });
            }
            else
            {
                recipeDataList = ProductData.GetProductListByGroup(buildingBaseData.KEY);
                recipeDataList.ForEach(recipe =>
                {
                    GameObject newRecipeCard = Instantiate(ResourceManager.GetResource<GameObject>(eResourcePath.PrefabClonePath, "ProductManageRecipeCard"), productRecipeContentsObject.transform);

                    ProductManageRecipeCard recipeCardComponent = newRecipeCard.GetComponent<ProductManageRecipeCard>();
                    if (recipeCardComponent != null)
                    {
                        recipeCardComponent.InitRecipeCard(recipe, buildInfo, this, AfterEnqueueProduct);
                        recipeCardList.Add(recipeCardComponent);
                    }
                });
            }

            // 스크롤 잠금 상태 제어
            productRecipeScroll.enabled = (autoRecipeDataList.Count >= RECIPE_SCROLL_LOCK_COUNT || recipeDataList.Count >= RECIPE_SCROLL_LOCK_COUNT);
        }

        // 생산품 획득 시 콜백
        void ProductHarvestProcess(GameObject target, int amount)
        {
            if (amount > 0 && target != null)
                RewardText(target, amount);

            // 자신의 슬롯 갱신
            RefreshSlot();

            // 다른 슬롯 갱신 요청
            if (target != null)
            {
                RecipeFrame frame = target.GetComponent<RecipeFrame>();
                if (frame != null)
                {
                    RefreshSlotByBuilding(frame.RecipeProductData);
                }
            }
        }

        // 재료 아이템의 생산 건물 리스트를 반환 (갱신이 필요한 슬롯 판별 시 사용)
        List<string> GetRecipeBuildingGroupList(ProductData productData)
        {
            List<string> resultList = new List<string>();

            resultList.Add(productData.BUILDING_GROUP); // 일단 자신의 물품은 포함

            foreach (Asset item in productData.NEED_ITEM)
            {
                string buildingGroup = ProductData.GetBuildingGroupByProductItem(item.ItemNo);

                if (resultList.Contains(buildingGroup)) continue;

                resultList.Add(buildingGroup);
            }

            return resultList;
        }

        void RewardText(GameObject target, int amount)
        {
            if (getItemAnimLayer == null) { return; }

            // 트윈개체 생성
            GameObject getItemAnimPrefab = ResourceManager.GetResource<GameObject>(eResourcePath.PrefabClonePath, "GetItemAmountAnimObject");
            GameObject getItemAnimObject = Instantiate(getItemAnimPrefab, getItemAnimLayer.transform);
            //Vector2 targetRectPos = SBFunc.WorldToUICanvasPosition(target.transform.position);
            //getItemAnimObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(targetRectPos.x, targetRectPos.y);
            getItemAnimObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(-500, 20);
            getItemAnimObject.transform.localScale = Vector3.one * 0.7f;

            // 데이터 및 트윈 설정
            RectTransform animRectTransform = getItemAnimObject.GetComponent<RectTransform>();
            CanvasGroup animCanvasGroup = getItemAnimObject.GetComponent<CanvasGroup>();
            Text animText = getItemAnimObject.GetComponentInChildren<Text>();

            animText.text = string.Format("+{0}", amount);
            getItemAnimObject.SetActive(true);

            animCanvasGroup.alpha = 1;
            animRectTransform.DOAnchorPosY(50.0f, 1.5f).SetRelative().SetEase(Ease.OutQuad);
            animCanvasGroup.DOFade(0, 1.5f).SetEase(Ease.OutQuad).OnComplete(() => { Destroy(getItemAnimObject); });

            // 사운드 플레이
            CancelInvoke("PlayItemGetSound");
            Invoke("PlayItemGetSound", 0.01f);
        }
        void PlayItemGetSound()
        {
            CancelInvoke("PlayItemGetSound");
            SoundManager.Instance.PlaySFX("FX_ITEM_GET1");
        }

        //채우기가 생기면 없앨 것
        public void AfterEnqueueProduct(JObject jsonObj)
        {
            if (SBFunc.IsJTokenCheck(jsonObj["rewards"]) && currentProductQueue.Count > 0)
            {
                for (int i = 0; i < jsonObj["rewards"].Count(); i++)
                {
                    RewardText(currentProductQueue[i].gameObject, currentProductQueue[i].Amount);
                }

                //    var rewardList = SBFunc.ConvertSystemRewardDataList(JArray.FromObject(jsonObj["rewards"]));
                //    InventoryIncomeEvent.Send(rewardList, ProducesData.Tag);
            }

            if (currentProductQueue != null && currentProductQueue.Count > 0 && SBFunc.IsJTokenCheck(jsonObj["rs"]))
            {
                bool? check = currentProductQueue[0]?.sprChecker?.gameObject.activeInHierarchy;
                if (jsonObj["rs"].Value<int>() == 15 && check.Value)
                {
                    WWWForm data = new WWWForm();
                    data.AddField("tag", ProducesData.Tag);
                    data.AddField("slot", 0);
                    if (isNetworkState)
                    {
                        return;
                    }
                    isNetworkState = true;
                    NetworkManager.SendWithCAPTCHA("produce/harvest", data, (jsonObj) =>
                    {
                        isNetworkState = false;
                        switch (jsonObj["rs"].Value<int>())
                        {
                            case 14:
                                SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100002078));
                                break;
                        }
                        if (SBFunc.IsJTokenCheck(jsonObj["rewards"]))
                        {
                            JArray itemArr = (JArray)jsonObj["rewards"];
                            List<Asset> rewards = new();
                            for (int i = 0; i < itemArr.Count; i++)
                            {
                                rewards.Add(new Asset(itemArr[i]));
                            }
                            if (false == User.Instance.Inventory.CanItems(rewards))
                            {
                                SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100002078));
                            }
                        }
                        RefreshSlot();
                        return;
                    }, (string arg) =>
                    {
                        isNetworkState = false;
                    });
                }
            }

            RefreshSlot();
        }

        /// <summary>
        /// 전체 가속을 누를 때, 현재 생산 중 아이템이 1개라면 (대기큐에 걸린게 없다면) 일반 가속과 동일하게 처리요청
        /// </summary>
        /// <returns></returns>
        int GetSingleProductingIndex()
        {
            List<ProducesRecipe> queueList = ProducesData.Items;
            int queueLength = ProducesData.Slot;

            // 생산 큐 리스트 정렬
            if (queueList != null && queueList.Count > 0)
            {
                queueList.Sort((a, b) => b.State - a.State);
            }

            int productIndex = -1;
            int producingCount = 0; // 현재 생산중인 품목 갯수
            for (int i = 0; i < queueLength; ++i)
            {
                if (i < queueList.Count)    // 생산 아이템이 있는 경우
                {
                    if (buildingBaseData.KEY == "exp_battery")
                        continue;
                    else
                    {
                        if (queueList[i].State == eProducesState.Complete)
                            continue;
                        else
                        {
                            if (queueList[i].State == eProducesState.Ing)//생산중인것이 있는지만 체크
                            {
                                var localCheck = queueList[i].ProductionExp;
                                if (localCheck > TimeManager.GetTime())
                                {
                                    producingCount++;
                                    productIndex = i;
                                }
                            }
                            else
                                producingCount++;//idle 상태
                        }
                    }
                }
            }

            if (producingCount == 1)
                return productIndex;
            else
                return -1;
        }

        public void OnClickAllAccel()
        {
            if (SlotAllEndTime - TimeManager.GetTime() < 0)
                return;

            var checkSingleFrameIndex = GetSingleProductingIndex();
            if (checkSingleFrameIndex >= 0 && currentProductQueue.Count > checkSingleFrameIndex)
            {
                currentProductQueue[checkSingleFrameIndex].OnClick();
                return;
            }

            AccelerationMainPopup.OpenPopup(eAccelerationType.JOB, ProducesData.Tag, SlotAllEndTime - TimeManager.GetTime(), SlotAllEndTime, -1 ,() =>
            {
                RefreshSlot();
            },
            () =>
            {
                RefreshSlot();
            },true);
        }

        void ClearSlot()
        {
            addSlotObj = null;
            remainTimeObject.Refresh = null;
            productRecipeScroll.horizontalNormalizedPosition = 0.0f;
            currentProductQueue.Clear();
            recipeCardList.Clear();

            IsProducting = false;
            SlotAllEndTime = 0;
            ProductAllAccelBtn.SetButtonSpriteState(false);
            ProductAllAccelBtn.interactable = false;

            SBFunc.RemoveAllChildrens(productItemContentsObject.transform);
            SBFunc.RemoveAllChildrens(productRecipeContentsObject.transform);
        }
    }
}