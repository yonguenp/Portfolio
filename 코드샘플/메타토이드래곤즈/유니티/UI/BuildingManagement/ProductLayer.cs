using DG.Tweening;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class ProductLayer : BuildingLayer
    {
		[SerializeField] ScrollRect queueTabScrollRect = null;
		[SerializeField] GameObject queueTabPrefab = null;
		[SerializeField] GameObject emptyQueueCover = null;

		[SerializeField] ScrollRect receipeScollRect = null;
		[SerializeField] GameObject receipeScrollAbleAlertArrow = null;

		[Space(10)]
		[Header("생산 중인 아이템 큐")]
		[SerializeField] protected ScrollRect productQueueScrollRect = null;

		[SerializeField] GameObject receipeCover = null;

		[Space(10)]
		[SerializeField] GameObject nodeTimer = null;
		[SerializeField] Text timerText = null;
		[SerializeField] Text buildingNameLvText = null;

		[SerializeField] Button btnGetAllRow = null;
		[SerializeField] Button btnAllCancel = null;
		[SerializeField] GameObject getItemAnimLayer = null;
		[SerializeField] GameObject constructingLayer = null;
		[SerializeField] GameObject mainDim = null;

        // 슬롯추가
        GameObject addSlotObj = null;

        private TimeObject timeObj = null;
        private List<ProductLayerQueueTab> queueTabList = new List<ProductLayerQueueTab>();
        private int currentClickQueueTabIndex = 0;

        //채우기가 생기면 없앨 것
        protected List<RecipeFrame> currentProductQueue = new List<RecipeFrame>();
        private int itemCompleteCount = 0;
        private int productableItemCount = 0;
        public delegate void QueueTabClickCallBack(int tag);
        private QueueTabClickCallBack queueTabClickCB=null;

        private List<GameObject> recipeTimerList = new List<GameObject>();
        private List<GameObject> recipeCardList = new List<GameObject>();

        public void SetBuildingTab(BuildingPopupData data)
        {
            SetData(data);
            if (string.IsNullOrEmpty(Data.BuildingKey))
            {
                currentClickQueueTabIndex = 0;
                return;
            }

            List<int> buildingTagList = BuildingOpenData.GetTagList(Data.BuildingKey);
            buildingTagList.Sort();//오름차순 정렬
            int checkIndex = buildingTagList.IndexOf(data.BuildingTag);
            if (checkIndex >= 0)
            {
                currentClickQueueTabIndex = checkIndex;
            }
        }

        public void SetQueueTabClickCallBack(QueueTabClickCallBack cb)
        {
            if (cb != null)
            {
                queueTabClickCB = cb;
            }
        }

        void InitProductLayer()
        {
            InitwithUserData(Data.BuildingKey, Data.BuildingTag);
            RefreshQueueTab();
            RefreshScroll(true);
        }

        void RecipeTimerAllOff()
        {
            if(recipeTimerList == null)
            {
                recipeTimerList = new List<GameObject>();
            }
            foreach(GameObject item in recipeTimerList)
            {
                if (item != null)
                {
                    item.SetActive(false);
                }
            }
        }

        void RecipeCardAllOff()
        {
            if (recipeCardList == null)
            {
                recipeCardList = new List<GameObject>();
            }
            foreach (GameObject item in recipeCardList)
            {
                if (item != null)
                {
                    item.SetActive(false);
                }
            }
        }

        void QueueTabObjListAllOff()
        {
            if (queueTabList == null)
            {
                queueTabList = new List<ProductLayerQueueTab>();
            }
            foreach (ProductLayerQueueTab item in queueTabList)
            {
                if (item != null)
                {
                    item.gameObject.SetActive(false);
                }
            }
        }

        protected virtual void RefreshProductQueueScroll(bool alignScroll = false)
        {
            RecipeTimerAllOff();
            currentProductQueue.Clear();

            if (Data.BaseData.TYPE != 2)
            {
                var produces = Data.ProduceBuilding;
                
                List<ProducesRecipe> queueList = null;
                int queueLength = 4;

                if (produces != null)
                {
                    queueLength = produces.Slot;
                    queueList = produces.Items;
                }


                int localCheck = 0;  //오래된 생산큐 정보 내부 갱신용

                if (queueList != null)
                    queueList.Sort((a, b) => b.State - a.State);

                itemCompleteCount = 0;
                if (queueList != null)
                {
                    for (int i = 0; i < queueLength; i++)
                    {
                        if (i >= recipeTimerList.Count)
                        {
                            GameObject itemClone = Instantiate(ResourceManager.GetResource<GameObject>(eResourcePath.PrefabClonePath, "recipeTimer"));
                            itemClone.transform.SetParent(productQueueScrollRect.content.transform);
                            itemClone.transform.localScale = Vector3.one;
                            recipeTimerList.Add(itemClone);
                            RecipeFrame itemFrame = recipeTimerList[i].GetComponent<RecipeFrame>();
                            itemFrame.OnClickCallback = null;
                        }
                        recipeTimerList[i].SetActive(true);
                        if (queueList.Count > i)
                        {
                            ProductData itemInfo = ProductData.GetProductDataByGroupAndKey(Data.BuildingKey, queueList[i].RecipeID);
                            if (itemInfo == null)
                            {
                                recipeTimerList[i].SetActive(false);
                                continue;
                            }

                            RecipeFrame itemFrame = recipeTimerList[i].GetComponent<RecipeFrame>();
                            itemFrame.OnClickCallback = productHarvestProcess;
                            currentProductQueue.Add(itemFrame);

                            if ((int)queueList[i].State < 3)
                            {
                                localCheck += itemInfo.PRODUCT_TIME;

                                if ((int)queueList[i].State == 2)
                                    localCheck = queueList[i].ProductionExp;

                                itemFrame.SetReceipeIcon(i, itemInfo, localCheck, Data.BuildingTag, ForceUpdate);
                                if ((0 < TimeManager.GetTimeCompare(localCheck) && TimeManager.GetTimeCompare(localCheck) <= itemInfo.PRODUCT_TIME))
                                    itemFrame.TimerStart();

                                if (localCheck == -1 || localCheck != 0 && localCheck <= TimeManager.GetTime())
                                {
                                    ++itemCompleteCount;
                                }
                            }
                            else//아이템 제작 성공 카운트
                            {
                                itemFrame.SetReceipeIcon(i, itemInfo, -1, Data.BuildingTag, ForceUpdate);
                                ++itemCompleteCount;
                            }
                        }
                        else
                        {
                            recipeTimerList[i].GetComponent<RecipeFrame>().SetFrameBlank();
                        }
                    }
                }

                // 슬롯추가 관련
                ProducesBuilding produceBuildingData = User.Instance.GetProduces(Data.BuildingTag);
                if (addSlotObj != null)
                {
                    addSlotObj.GetComponent<EmptySlotFrame>()?.RefreshSlot(produceBuildingData, Data.BaseData);
                    addSlotObj.transform.SetAsLastSibling();
                }
                else
                {
                    addSlotObj = Instantiate(ResourceManager.GetResource<GameObject>(eResourcePath.PrefabClonePath, "EmptySlot"));
                    addSlotObj.transform.SetParent(productQueueScrollRect.content.transform);
                    addSlotObj.transform.localScale = Vector3.one;
                    EmptySlotFrame slotFrame = addSlotObj.GetComponent<EmptySlotFrame>();

                    slotFrame?.InitSlot(produceBuildingData, Data.BaseData, () => {
                        RefreshProductQueueScroll(true);
                        RefreshReceipeScroll();
                        PopupManager.ClosePopup<PricePopup>();
                        Town.Instance.RefreshMap();
                    });
                }
            }
            

            if (productQueueScrollRect != null && alignScroll)//좌로 정렬
            {
                productQueueScrollRect.verticalNormalizedPosition = 0;
            }

            RefreshContentFitter(productQueueScrollRect.content.GetComponent<RectTransform>());
            //SetProductItemLayoutArrow();
        }

        void RefreshReceipeScroll()
        {
            RecipeCardAllOff();

            BuildingBaseData buildingInfo = BuildingBaseData.Get(Data.BuildingKey);
            List<ProductData> receipeArray = ProductData.GetProductListByGroup(Data.BuildingKey);

            //건물의 최대 수가 1개씩 이므로 배열이 아님, 추후에 건물 최대치가 늘어나면 배열로 수정 필요
            if (receipeArray == null || receipeArray.Count <= 0)
                return;
            productableItemCount = 0;
            for (int i =0; i< receipeArray.Count; ++i) { 
                if(i >= recipeCardList.Count) { 
                    GameObject newRecipeCard = Instantiate(ResourceManager.GetResource<GameObject>(eResourcePath.PrefabClonePath, "RecipeCard"), receipeScollRect.content.transform);
                    recipeCardList.Add(newRecipeCard);
                }
                recipeCardList[i].gameObject.SetActive(true);
                int key = int.Parse(receipeArray[i].KEY.ToString());

                var recipeCardComp = recipeCardList[i].GetComponent<RecipeCard>();
                if (recipeCardComp != null)
                {
                    recipeCardComp.Init(ProductData.GetProductDataByGroupAndKey(receipeArray[i].BUILDING_GROUP, key), Data.BuildingTag, Data.Level >= receipeArray[i].BUILDING_LEVEL, AfterEnqueueProduct, itemCompleteCount, ()=> {
                        Refresh();
                    });
                    if (Data.Level >= receipeArray[i].BUILDING_LEVEL)
                    {
                        ++productableItemCount;
                    }
                    recipeCardList[i].transform.localScale = Vector3.one;
                }

            }
            RefreshContentFitter(receipeScollRect.content.GetComponent<RectTransform>());
            SetRecipeLayoutArrow();
        }

        void SetRecipeScrollSize(float size)
        {
            if (receipeScollRect != null && receipeScollRect.verticalScrollbar != null)
            {
                receipeScollRect.verticalScrollbar.GetComponent<Scrollbar>().size = size;
                receipeScollRect.verticalScrollbar.GetComponent<Scrollbar>().value = size;
            }
        }

        RectTransform GetCurrentRecipeRect()
        {
            RectTransform tempRect = null;
            List<ProductData> receipeArray = ProductData.GetProductListByGroup(Data.BuildingKey);

            //건물의 최대 수가 1개씩 이므로 배열이 아님, 추후에 건물 최대치가 늘어나면 배열로 수정 필요
            if (receipeArray == null || receipeArray.Count <= 0)
                return tempRect;

            int currentIndex = 0;
            for (int i = 0; i < receipeArray.Count; i++)
            {
                var recipe = receipeArray[i];

                if (Data.Level >= recipe.BUILDING_LEVEL)
                {
                    currentIndex = i;
                }
            }

            var children = SBFunc.GetChildren(receipeScollRect.content);
            if (children != null && children.Length > 0 && children.Length > currentIndex)
            {
                tempRect = children[currentIndex].GetComponent<RectTransform>();
            }

            return tempRect;
        }

        int GetRecipeCount()
        {
            List<ProductData> recipeArray = ProductData.GetProductListByGroup(Data.BuildingKey);

            if (recipeArray == null)
                return 0;
            else
                return recipeArray.Count;
        }
        protected void RefreshContentFitter(RectTransform transform)
        {
            if (transform == null || !transform.gameObject.activeSelf)
            {
                return;
            }

            foreach (RectTransform child in transform)
            {
                RefreshContentFitter(child);
            }

            var layoutGroup = transform.GetComponent<LayoutGroup>();
            var contentSizeFitter = transform.GetComponent<ContentSizeFitter>();
            if (layoutGroup != null)
            {
                layoutGroup.SetLayoutHorizontal();
                layoutGroup.SetLayoutVertical();
            }

            if (contentSizeFitter != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(transform);
            }
        }

        public void SetRecipeLayoutArrow()
        {
            receipeScrollAbleAlertArrow.SetActive(false);
            
            if (productableItemCount > 3)
            {
                receipeScrollAbleAlertArrow.SetActive(receipeScollRect.normalizedPosition.y > 0);
            }
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

                var rewardList = SBFunc.ConvertSystemRewardDataList(JArray.FromObject(jsonObj["rewards"]));
                InventoryIncomeEvent.Send(rewardList, Data.BuildingTag);
            }

            if (currentProductQueue != null && currentProductQueue.Count > 0 && SBFunc.IsJTokenCheck(jsonObj["rs"]))
            {
                bool? check = currentProductQueue[0]?.sprChecker?.gameObject.activeInHierarchy;
                if (jsonObj["rs"].Value<int>() == 15 && check.Value)
                {
                    WWWForm data = new WWWForm();
                    data.AddField("tag", Data.BuildingTag);
                    data.AddField("slot", 0);

                    NetworkManager.SendWithCAPTCHA("produce/harvest", data, (jsonObj) =>
                    {
                        switch (jsonObj["rs"].Value<int>())
                        {
                            case 14:
                                SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100002078));
                                break;
                        }
                        if (SBFunc.IsJTokenCheck(jsonObj["rewards"]))
                        {
                            List<Asset> rewards = new();
                            JArray itemArr = (JArray)jsonObj["rewards"];
                            for (int i = 0; i < itemArr.Count; i++)
                            {
                                rewards.Add(new(itemArr[i]));
                            }

                            if (false == User.Instance.Inventory.CanItems(rewards))
                            {
                                SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100002078));
                            }
                        
                            RewardText(currentProductQueue[0].gameObject, currentProductQueue[0].Amount);                            
                        }
                        Refresh();
                        return;
                    });
                }
            }

            PopupManager.ForceUpdate<ProductPopup>();
        }

        void RefreshQueueTab()
        {
            if (queueTabList == null || queueTabList.Count <= 0)
                return;

            for (int i = 0; i < queueTabList.Count; i++)
            {
                ProductLayerQueueTab queComp = queueTabList[i];
                if (queComp == null)
                    continue;
                if (queueTabList[i].gameObject.activeInHierarchy == false)
                    continue;
                Button btn = queComp.GetComponent<Button>();
                if (btn == null)
                    continue;

                if (currentClickQueueTabIndex == i)
                    queComp.SwitchTabState(true);
                else
                    queComp.SwitchTabState(false);
            }
        }

        void SetLayerInfo()
        {
            QueueTabObjListAllOff();

            int tempBuildingTag = 0;
            List<string> buildingIndexList = new List<string>();
            buildingIndexList = GetUserBuildingKeyList();

            List<int> buildingTagList = BuildingOpenData.GetTagList(Data.BuildingKey);
            buildingTagList.Sort();//오름차순 정렬
            bool isFirst = false;

            for (int i = 0; i < buildingTagList.Count; i++)
            {
                int i_buildingtag = buildingTagList[i];
                BuildInfo data = User.Instance.GetUserBuildingInfoByTag(i_buildingtag);

                if (data == null)
                    continue;

                if (!isFirst)
                {
                    isFirst = true;
                    tempBuildingTag = i_buildingtag;//가장 첫 태그를 첫 탭 인덱스 삼고 클릭 처리
                }

                if (currentClickQueueTabIndex >= 0 && i == currentClickQueueTabIndex)//이전 클릭한 인덱스 있으면 처리
                    tempBuildingTag = i_buildingtag;
                if(i>=queueTabList.Count) { 
                    GameObject clone = Instantiate(queueTabPrefab);
                    clone.transform.SetParent(queueTabScrollRect.content.transform);
                    clone.transform.localScale = Vector3.one;
                    queueTabList.Add(clone.GetComponent<ProductLayerQueueTab>());
                }
                queueTabList[i].gameObject.SetActive(true);

                queueTabList[i].SetData(Data.BuildingKey, i_buildingtag, i + 1);

                Button btn = queueTabList[i].GetComponent<Button>();

                JObject customData = new JObject();
                customData.Add("index", i);
                customData.Add("tag", i_buildingtag);
                customData.Add("buildingIndex", Data.BuildingKey);
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() =>
                { 
                    OnClickQueueTab(customData);
                    if(queueTabClickCB != null)
                    {
                        queueTabClickCB(i_buildingtag);
                    }
                });

                
            }

            if (currentClickQueueTabIndex < 0)
            {
                currentClickQueueTabIndex = 0;
            }

            SetData(new BuildingPopupData(tempBuildingTag));

            RefreshBuildingNameLabel();

            List<BuildInfo> pbuildings = User.Instance.GetUserBuildingList();
            bool pReddot = false;

            if (pbuildings != null)
            {
                pbuildings.ForEach((bElement) =>
                {
                    if (pReddot || bElement.Tag == (int)eLandmarkType.Dozer || bElement.Tag == (int)eLandmarkType.Travel || bElement.Tag == (int)eLandmarkType.SUBWAY || bElement.State != eBuildingState.NORMAL)
                        return;

                    ProducesBuilding produces = User.Instance.GetProduces(bElement.Tag);

                    if (produces == null || produces.Items.Count == 0)
                        return;

                    if (bElement.Tag < 1100)
                    {
                        produces.Items.ForEach((rElement) =>
                        {
                            if (rElement.ProductionExp == 0 || rElement.ProductionExp <= TimeManager.GetTime())
                            {
                                pReddot = true;
                                return;
                            }
                        });
                    }
                    else
                    {
                        BuildingOpenData bName = BuildingOpenData.GetByInstallTag(bElement.Tag);
                        List<ProducesRecipe> itemList = produces.Items;
                        if (itemList.Count > 0)
                        {
                            for (int i = 0; i < itemList.Count; i++)
                            {
                                if (pReddot) { continue; }

                                ProducesRecipe produceReceipe = itemList[i];
                                if (produceReceipe == null) { continue; }

                                ProductData itemReceipe = ProductData.GetProductDataByGroupAndKey(bName.BUILDING, produceReceipe.RecipeID);

                                if (produceReceipe.State == eProducesState.Complete ||
                                produceReceipe.State == eProducesState.Ing && (produceReceipe.ProductionExp + i * itemReceipe.PRODUCT_TIME) <= TimeManager.GetTime())
                                {
                                    pReddot = true;
                                }
                            }
                        }
                    }
                });
            }

            Transform reddot = SBFunc.GetChildrensByName(btnGetAllRow.transform, new string[] { "reddot" });
            if (reddot != null)
            {
                reddot.gameObject.SetActive(pReddot);
            }
        }

        List<string> GetUserBuildingKeyList()
        {
            return BuildingOpenData.GetUserBuildingKeyList();
        }

      
        void RefreshBuildingNameLabel(bool isLvUp =false)
        {
            if (buildingNameLvText == null) return;
            string buildingName = StringData.GetStringByStrKey(BuildingBaseData.Get(Data.BuildingKey)._NAME);
            string currentBuildingLevel = string.Format(StringData.GetStringByIndex(100000056), Data.Level);
            if (Data.Level <= 0)
            {
                currentBuildingLevel = StringData.GetStringByStrKey("building_construction_progress");
            }
            buildingNameLvText.text = string.Format("<color=#ffdc00>{0}</color>  {1}",currentBuildingLevel,buildingName);
        }

        public void OnClickQueueTab(JObject data)
        {
            int clickTag = data["tag"].Value<int>();
            int clickIndex = data["index"].Value<int>();
            string clickBuildingIndex = data["buildingIndex"].Value<string>();

            BuildInfo buildingInstance = User.Instance.GetUserBuildingInfoByTag(clickTag);

            if (buildingInstance == null)
                return;

            if (Data.BuildingTag == clickTag && clickIndex == currentClickQueueTabIndex)
                return;

            currentClickQueueTabIndex = clickIndex;

            RefreshScroll(true);
            InitwithUserData(Data.BuildingKey, Data.BuildingTag);
            RefreshQueueTab();
            
        }

        public void ForceUpdate()
        {
            Refresh();
        }

        public void RefreshGetAllorAllClearBtn()//전체 취소인지 일괄 회수 버튼이 있는지 선택
        {
            var isProductCondition = IsProductCondition();//대기큐 없거나, 큐 카운트 0
            if(isProductCondition)
            {
                int currentOperationIndex = GetOperationProductIndex();
                SetButtonSetInteract(btnAllCancel, currentOperationIndex >= 0);//현재 생산중인 슬롯이 있으면 활성화
                SetButtonSetInteract(btnGetAllRow, isGetProductItem());
            }
            else
            {
                SetButtonSetInteract(btnAllCancel, false);
                SetButtonSetInteract(btnGetAllRow, false);
            }
        }

        void SetButtonSetInteract(Button _targetButton, bool interactable)
        {
            if(_targetButton != null)
            {
                _targetButton.SetButtonSpriteState(interactable);
            }
        }

        public void OnClickGetAll()
        {
            if (!isGetProductItem())
            {
                ToastManager.On(100002536);
                return;
            }

            if (IsAvailableGetItemCondition())
                RequestInventoryOpenPopup();
            else
                RequestBuildingGetAllProduct();
        }

        bool IsProductCondition()
        {
            if (currentProductQueue == null || currentProductQueue.Count <= 0)//취소할 것이 없음
            {
                return false;
            }
            return true;
        }

        int GetOperationProductIndex()//현재 가동중인 슬롯 넘버
        {
            int currentProductIndex = -1;
            for (int i = 0; i < currentProductQueue.Count; i++)
            {
                var isComplete = currentProductQueue[i]?.IsTimeObjectRunning();
                if (isComplete.Value)
                {
                    currentProductIndex = i;
                    break;
                }
            }
            return currentProductIndex;
        }

        public void OnClickAllProductCancle()//전체 취소 기능 추가
        {
            var isAvailableCancle = IsProductCondition();
            int currentOperationIndex = GetOperationProductIndex();
            if (!isAvailableCancle || currentOperationIndex < 0)
            {
                ToastManager.On(100002535);
                return;
            }

            SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100002513),
                () => {
                    var isAvailableCancle = IsProductCondition();
                    int currentOperationIndex = GetOperationProductIndex();
                    if (!isAvailableCancle || currentOperationIndex < 0)
                    {
                        ToastManager.On(100002535);
                        return;
                    }

                    WWWForm data = new WWWForm();
                    data.AddField("tag", Data.BuildingTag);
                    data.AddField("auto", 1);//해당 건물에서만 생산된 건물 받기 플래그
                    data.AddField("slot", currentOperationIndex);//현재 생산이 돌고 있는 인덱스

                    NetworkManager.SendWithCAPTCHA("produce/harvest", data, (jsonObj) =>
                    {
                        if (SBFunc.IsJTokenCheck(jsonObj["rs"]))
                        {
                            switch (jsonObj["rs"].Value<int>())
                            {
                                case (int)eApiResCode.OK:
                                    Refresh();
                                    if (jsonObj.ContainsKey("rewards"))
                                    {
                                        var rewardList = SBFunc.ConvertSystemRewardDataList(JArray.FromObject(jsonObj["rewards"]));

                                        InventoryIncomeEvent.Send(rewardList, Data.BuildingTag);
                                        //SystemRewardPopup.OpenPopup(rewardList);
                                    }
                                    break;

                                case (int)eApiResCode.INVENTORY_FULL:
                                    SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100002077), StringData.GetStringByIndex(100000414), "",
                                        () => {
                                            PopupManager.OpenPopup<InventoryPopup>();
                                        }, () => { }, () => { });
                                    break;

                                case (int)eApiResCode.NOTHING_TO_HARVEST:
                                    ToastManager.On(100000812);
                                    break;
                            }
                        }
                    });
                }, 
                () => {
                },
                () => {
                }
            );
        }

        void InitBuildingBaseInfo()
        {
            nodeTimer.gameObject.SetActive(false);
            emptyQueueCover.gameObject.SetActive(false);
            receipeCover.gameObject.SetActive(false);
        }

        void InitwithUserData(string buildingIndex, int buildingTag)
        {
            SetData(new BuildingPopupData(buildingTag));
            if (timeObj.Refresh != null)
            {
                timeObj.Refresh = null;
            }
            //유저 정보에서 건물 정보 가져오기
            InitBuildingBaseInfo();
            constructingLayer.SetActive(false);
            mainDim.SetActive(false);

            switch (Data.BuildInfo.State)
            {
                case eBuildingState.CONSTRUCTING:
                    if (Data.BuildInfo.ActiveTime <= TimeManager.GetTime())
                    {
                        SetConstructFinish(Data.BuildInfo.Level > 1);
                        return;
                    }

                    constructingLayer.SetActive(true);
                    if (Data.BuildInfo.Level > 1)
                    {
                        SetUpgradingState();
                    }
                    else
                    {
                        SetConstructingState();
                    }
                    
                    emptyQueueCover.gameObject.SetActive(true);
                    InitwithConstruct();
                    SetRecipeCardDimmed();//레시피 강제 딤드 처리
                    btnGetAllRow.gameObject.SetActive(false);
                    btnAllCancel.gameObject.SetActive(false);
                    return;

                case eBuildingState.CONSTRUCT_FINISHED:
                    SetConstructFinish(Data.Level > 1);
                    btnGetAllRow.gameObject.SetActive(false);
                    btnAllCancel.gameObject.SetActive(false);
                    return;
            }

            btnGetAllRow.gameObject.SetActive(true);
            btnAllCancel.gameObject.SetActive(true);

            var isAvailable = IsAvailableBuildLevel();
            if (isAvailable)
            {
                if (Data.LevelData.NEED_AREA_LEVEL <= User.Instance.GetAreaLevel())// 업그레이드 가능
                {
                    SetUpgradeBtnState(eUpgradeButtonState.UpgradeAble);
                }
                else
                {
                    SetUpgradeBtnState(eUpgradeButtonState.UpgradeDisable);
                }
            }
            else
            {
                SetUpgradeBtnState(eUpgradeButtonState.None);
            }
        }

        bool IsAvailableBuildLevel()
        {
            return BuildingLevelData.GetDataByGroupAndLevel(Data.BaseData.KEY, Data.BuildInfo.Level + 1) != null;
        }

        void SetConstructFinish(bool isUpgrade = false)
        {
            mainDim.SetActive(true);
            nodeTimer.gameObject.SetActive(false);
            constructingLayer.SetActive(false);
            SetUpgradeBtnState(isUpgrade? eUpgradeButtonState.UpgradeFinish : eUpgradeButtonState.ConstructFinish);
            emptyQueueCover.gameObject.SetActive(true);
            SetRecipeCardDimmed();
            if (buildingZoomPopup != null)
                buildingZoomPopup.ClosePopup();
        }


        void InitwithConstruct()
        {
            //완료 시간 - 서버 시간으로 시간 나타냄
            nodeTimer.gameObject.SetActive(true);
            if (timeObj.Refresh == null)
            {
                timeObj.Refresh = () =>
                {
                    timerText.text = TimeManager.GetTimeCompareString(Data.BuildInfo.ActiveTime);
                    if (TimeManager.GetTimeCompare(Data.BuildInfo.ActiveTime) <= 0)
                    {
                        timeObj.Refresh = null;
                        SetConstructFinish(Data.BuildInfo.Level > 1);
                    }
                };
            }
        }

        void SetRecipeCardDimmed()//건설중이나 완료 상태 일때 강제 딤드 처리
        {
            int receipeChildCount = receipeScollRect.content.transform.childCount;
            if (receipeChildCount > 0) { 
                for (int i = 0; i < receipeChildCount; ++i)
                {
                    Transform receipeNode = receipeScollRect.content.transform.GetChild(i);
                    if (receipeNode == null)
                    {
                        continue;
                    }
                    RecipeCard receipeComp = receipeNode.GetComponent<RecipeCard>();
                    if (receipeComp == null)
                    {
                        continue;
                    }
                    receipeComp.SetActiveDimmedBlock(true);
                }
            }
        }

        public void OnClickBtnAccelerate()
        {
            AccelerationMainPopup.OpenPopup(eAccelerationType.LEVELUP, Data.BuildInfo.Tag, Data.LevelData.UPGRADE_TIME, Data.BuildInfo.ActiveTime, Refresh);
        }
        public void productHarvestProcess(GameObject target, int amount)
        {
            if(amount > 0 && target != null)
                RewardText(target, amount);

            PopupManager.ForceUpdate<ProductPopup>();
        }
        void RewardText(GameObject target, int amount)
        {
            if (getItemAnimLayer == null) { return; }

            // 트윈개체 생성
            GameObject getItemAnimPrefab = ResourceManager.GetResource<GameObject>(eResourcePath.PrefabClonePath, "GetItemAmountAnimObject");
            GameObject getItemAnimObject = Instantiate(getItemAnimPrefab, getItemAnimLayer.transform);
            Vector2 targetRectPos = SBFunc.WorldToUICanvasPosition(target.transform.position);
            getItemAnimObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(targetRectPos.x-280, targetRectPos.y+200);
            getItemAnimObject.transform.localScale = Vector3.one*0.7f;

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

        protected override void Init()
        {
            InitTimeObj();
            InitProductLayer();
        }

        public override void RefreshUI()
        {
            base.RefreshUI();
           // Refresh();
        }

        void InitTimeObj()
        {
            if (timeObj == null)
            {
                timeObj = nodeTimer.GetComponent<TimeObject>();
            }

            if (timeObj.Refresh != null)
            {
                timeObj.Refresh = null;
            }
        }
       
        private void RefreshScroll(bool isProductScrollAlign=false) {
            SetLayerInfo();
            RefreshProductQueueScroll(isProductScrollAlign);
            RefreshReceipeScroll();
            RefreshBuildingNameLabel();
        }


        protected override void Refresh()
        {
            InitwithUserData(Data.BuildingKey, Data.BuildInfo.Tag);
            RefreshScroll();
            RefreshQueueTab();
            RefreshGetAllorAllClearBtn();
        }

        protected override void Clear()
        {
            
        }

        protected override void SetLockState()
        {
            SetUpgradeBtnState(eUpgradeButtonState.UpgradeDisable);
        }

        protected override void SetNotBuiltState() 
        {
            SetUpgradeBtnState(eUpgradeButtonState.UpgradeAble);
        }

        protected override void SetConstructingState()
        {
            SetUpgradeBtnState(eUpgradeButtonState.Constructing);
        }
        void SetUpgradingState()
        {
            SetUpgradeBtnState(eUpgradeButtonState.Upgrading);
        }

        protected override void SetNormalState()
        {
            SetUpgradeBtnState(eUpgradeButtonState.None);
        }

        public override void OnClickConstruct()
        {

        }

        public override void OnClickConstructFinish()
        {
            WWWForm data = new WWWForm();
            data.AddField("tag", Data.BuildingTag);

            NetworkManager.Send("building/complete", data, (jsonData) =>
            {
                InitwithUserData(Data.BuildingKey.ToString(), Data.BuildingTag);
                Refresh();
                BuildingCompletePopup.OpenPopup();
                Town.Instance.RefreshMap();
            });
        }

        public override void OnClickUpgrade()
        {
            if (IsAvailableBuildLevel())
            {
                BuildingLevelData buildingCurInfo = BuildingLevelData.GetDataByGroupAndLevel(Data.BaseData.KEY, Data.BuildInfo.Level);

                if (buildingCurInfo.NEED_AREA_LEVEL > User.Instance.GetAreaLevel())
                {
                    ToastManager.On(100000059, buildingCurInfo.NEED_AREA_LEVEL);
                    return;
                }
            }

            if (isGetProductItem() || isRunning())
            {
                SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100000555));
                return;
            }

            if (Data.BaseData.TYPE != 2)
            {
                var popup = PopupManager.OpenPopup<BuildingUpgradePopup>(new BuildingPopupData(Data.BuildingTag));
                popup.SetUpgradeCallBack(Refresh);
            }
            else
            {
                var popup = PopupManager.OpenPopup<BatteryBuildingUpgradePopup>(new BuildingPopupData(Data.BuildingTag));
                popup.SetUpgradeCallBack(Refresh);
            }
        }
    }
}