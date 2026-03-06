using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static SandboxNetwork.ProductLayer;

namespace SandboxNetwork
{
    public class BatteryLayer : BuildingLayer
    {
        [SerializeField] ScrollRect queueTabScrollRect = null;
        [SerializeField] GameObject queueTabPrefab = null;

        [SerializeField] Text buildingNameLvText = null;
        [Space (10)]
        [Header ("Battery Info")]
        [SerializeField]
        GameObject BatteryInfoBox = null;
        [SerializeField]
        Image batteryImage = null;
        [SerializeField]
        Text batteryCount  = null;
        [SerializeField]
        Text batteryTimer = null;
        [Space(10)]
        [Header("Timer")]
        [SerializeField] 
        GameObject nodeTimer = null;
        [SerializeField] 
        Text timerText = null;
        [SerializeField]
        Button btnUpgrade = null;

        private List<ProductLayerQueueTab> queueTabList = new List<ProductLayerQueueTab>();
        private int currentClickQueueTabIndex = 0;
        private QueueTabClickCallBack queueTabClickCB = null;
		private TimeObject constructTimeObj = null;
		private TimeObject produceTimeObj = null;
        private List<int> BuildingTags = null;


        public void SetBuildingTab(BuildingPopupData data, List<int> buildingTags)
        {
            BuildingTags = buildingTags;

            SetData(data);
            if (string.IsNullOrEmpty(Data.BuildingKey))
            {
                currentClickQueueTabIndex = 0;
                return;
            }

            BuildingTags.Sort();//오름차순 정렬
            int checkIndex = BuildingTags.IndexOf(data.BuildingTag);
            if (checkIndex >= 0)
            {
                currentClickQueueTabIndex = checkIndex;
            }
            if (TutorialManager.tutorialManagement.IsPlayingTutorialByGroup(TutorialDefine.Battery))
                ConstructButton.gameObject.SetActive(false);
            else
                ConstructButton.gameObject.SetActive(true);
        }
        public override void RefreshUI()
        {
            base.RefreshUI();
            //Refresh();
            TryHarvest();
        }
        void InitBuildingBaseInfo()
        {
            nodeTimer.gameObject.SetActive(false);
            BatteryInfoBox.SetActive(true);
        }
        void InitwithUserData(string buildingIndex, int buildingTag)
        {
            SetData(new BuildingPopupData(buildingTag));
            InitBuildingBaseInfo();
            if (constructTimeObj.Refresh != null)
            {
                constructTimeObj.Refresh = null;
            }
            ButtonExtensions.SetButtonSpriteState(btnUpgrade, true);
            switch (Data.BuildInfo.State)
            {
                case eBuildingState.NORMAL:
                    int maxLevel = BuildingLevelData.GetBuildingMaxLevelByGroup(buildingIndex);
                    if (Data.BuildInfo.Level >= maxLevel)
                    {
                        SetUpgradeBtnState(eUpgradeButtonState.UpgradeMax);
                    }
                    else
                    {
                        var buildingLevelData = BuildingLevelData.GetDataByGroupAndLevel(Data.BaseData.KEY, Data.BuildInfo.Level);
                        if (buildingLevelData.NEED_AREA_LEVEL <= User.Instance.GetAreaLevel())
                            SetUpgradeBtnState(eUpgradeButtonState.UpgradeAble);
                        else
                        {
                            SetUpgradeBtnState(eUpgradeButtonState.UpgradeDisable);
                            ButtonExtensions.SetButtonSpriteState(btnUpgrade, false);
                        }
                    }
                    
                    break;
                case eBuildingState.CONSTRUCTING:
                    if (Data.BuildInfo.ActiveTime <= TimeManager.GetTime())
                    {
                        SetConstructFinish(Data.BuildInfo.Level > 1);
                        return;
                    }
                    if (Data.BuildInfo.Level > 0)
                    {
                        SetUpgradingState();
                    }
                    else
                    {
                        SetConstructingState();
                    }
                    ButtonExtensions.SetButtonSpriteState(btnUpgrade, false);
                    InitwithConstruct();
                    return;

                case eBuildingState.CONSTRUCT_FINISHED:
                    SetConstructFinish(Data.Level > 1);
                    return;
            }
        }

        void SetConstructFinish(bool isUpgrade = false)
        {
            nodeTimer.gameObject.SetActive(false);
            BatteryInfoBox.gameObject.SetActive(true);
            SetUpgradeBtnState(isUpgrade ? eUpgradeButtonState.UpgradeFinish : eUpgradeButtonState.ConstructFinish);
            if (buildingZoomPopup != null)
                buildingZoomPopup.ClosePopup();
        }

        protected void RefresBatteryUI()
        {
			int produceSuccessBattaryQueue = 0;
			int remainTime = 0;
			int produceExp = 0;
			ProducesBuilding pBuilding = User.Instance.GetProduces(Data.BuildingTag);
            ProductAutoData autoProductInfo = Data.Product as ProductAutoData;
            if (pBuilding == null || autoProductInfo == null)
                return;

            int AllProductCount = autoProductInfo.ProductItem.Amount * pBuilding.Slot;
            if (autoProductInfo == null) 
				return;

            ItemBaseData itemInfo = autoProductInfo.ProductItem.BaseData;
            batteryImage.sprite = itemInfo.ICON_SPRITE;
            produceSuccessBattaryQueue = pBuilding.Items.Count-1;

			//슬릇 전체 수보다 생산완료 슬릇 수가 적다면 아직 생산중 이였다는것
			//생산정보 최신화가 오래전이였을 수 있으므로 클라에서 생산 완료 예측 계산
			if (pBuilding.Slot > pBuilding.Items.Count || pBuilding.Items.Last().ProductionExp > TimeManager.GetTime())
			{
				produceExp = pBuilding.Items.Last().ProductionExp;

				while (produceExp < TimeManager.GetTime())
				{
					++produceSuccessBattaryQueue;
					produceExp += autoProductInfo.TERM;
				}
			}

			//((총 남은 슬릇 * 생산 시간) + 현재 슬릇 남은 시간)
			remainTime = ((pBuilding.Slot - pBuilding.Items.Count) * autoProductInfo.TERM) + (produceExp - TimeManager.GetTime());

			if(remainTime > 0)
			{
				//타임오브젝트 가동
				produceTimeObj.Refresh = () =>
				{
					if (remainTime >= 0)
					{
						--remainTime;
						//시간이 지났을경우
						if (remainTime % autoProductInfo.TERM == 0)
						{
							++produceSuccessBattaryQueue;
						}
                        // 배터리 카운트 출력할 내용 - 생산할 아이템 갯수 ( 즉 시간이 흐르면 생산했으니 생산할 아이템 갯수가 생산 완료된 것만큼 줄어듬 ) 
						batteryCount.text = string.Format("x{0}", AllProductCount -  produceSuccessBattaryQueue * autoProductInfo.ProductItem.Amount);
						batteryTimer.text = SBFunc.TimeString(remainTime);
					}
					else
					{
						produceTimeObj.Refresh = null;
					}
				};
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

        void SetLayerInfo()
        {
            QueueTabObjListAllOff();

            int tempBuildingTag = 0;

            BuildingTags.Sort();//오름차순 정렬
            bool isFirst = false;

            int displayIndex = 1;
            for (int i = 0; i < BuildingTags.Count; i++)
            {
                int i_buildingtag = BuildingTags[i];
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
                if(i>= queueTabList.Count) { 
                    GameObject clone = Instantiate(queueTabPrefab);
                    clone.transform.SetParent(queueTabScrollRect.content.transform);
                    clone.transform.localScale = Vector3.one;
                    queueTabList.Add(clone.GetComponent<ProductLayerQueueTab>());
                }
                queueTabList[i].gameObject.SetActive(true);
                queueTabList[i].SetData(Data.BuildingKey, i_buildingtag, displayIndex);
                Button btn = queueTabList[i].GetComponent<Button>();
                btn.onClick.RemoveAllListeners();
                if (data.State != eBuildingState.NORMAL)  // 건설완료 상태의 건전지 공장은 건설완료를 끝내야 queue 에 반영 되도록 하기 위한 용도
                {
                    queueTabList[i].gameObject.SetActive(false);
                    continue;
                }
                btn.onClick.AddListener(() =>
                {
                    OnClickQueueTab(i, i_buildingtag, Data.BuildingKey);
                    if (queueTabClickCB != null)
                    {
                        queueTabClickCB(i_buildingtag);
                    }
                });
                displayIndex++;
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

        }

        public void SetQueueTabClickCallBack(QueueTabClickCallBack cb)
        {
            if (cb != null)
            {
                queueTabClickCB = cb;
            }
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

                Button btn = queComp.GetComponent<Button>();
                if (btn == null)
                    continue;

                if (currentClickQueueTabIndex == i)
                    queComp.SwitchTabState(true);
                else
                    queComp.SwitchTabState(false);
            }
        }
        void InitwithConstruct()
        {
            //완료 시간 - 서버 시간으로 시간 나타냄
            nodeTimer.gameObject.SetActive(true);
            BatteryInfoBox.SetActive(false);
            if (constructTimeObj.Refresh == null)
            {
                constructTimeObj.Refresh = () =>
                {
                    timerText.text = TimeManager.GetTimeCompareString(Data.BuildInfo.ActiveTime);
                    if (TimeManager.GetTimeCompare(Data.BuildInfo.ActiveTime) <= 0)
                    {
                        constructTimeObj.Refresh = null;
                        SetConstructFinish(Data.BuildInfo.Level > 1);
                    }
                };
            }
        }
        void RefreshBuildingNameLabel(bool isLvUp = false)
        {
            if (buildingNameLvText == null) return;
            string buildingName = StringData.GetStringByStrKey(BuildingBaseData.Get(Data.BuildingKey)._NAME);
            string currentBuildingLevel = string.Format(StringData.GetStringByIndex(100000056), Data.Level);
            if (Data.Level <= 0)
            {
                currentBuildingLevel = StringData.GetStringByStrKey("building_construction_progress");
            }
            buildingNameLvText.text = string.Format("<color=#ffdc00>{0}</color>  {1}", currentBuildingLevel, buildingName);
        }

        public void OnClickQueueTab(int clickTag, int clickIndex, string clickBuildingIndex)
        {
            BuildInfo buildingInstance = User.Instance.GetUserBuildingInfoByTag(clickTag);

            if (buildingInstance == null)
                return;

            if (Data.BuildingTag == clickTag && clickIndex == currentClickQueueTabIndex)
                return;

            currentClickQueueTabIndex = clickIndex;

            InitwithUserData(Data.BuildingKey, Data.BuildingTag);
            RefreshQueueTab();

        }
        public override void OnClickUpgrade()
        {
            if (IsAvailableBuildState(true))
            {
                var popup = PopupManager.OpenPopup<BatteryBuildingUpgradePopup>(new BuildingPopupData(Data.BuildInfo));
                popup.SetUpgradeCallBack(Refresh);
            }
            else
            {
                // 업그레이드 불가능에 대한 처리
                // ...
            }
        }

        
        public void OnClickBtnAccelerate()
        {
            AccelerationMainPopup.OpenPopup(eAccelerationType.LEVELUP, Data.BuildInfo.Tag, Data.LevelData.UPGRADE_TIME, Data.BuildInfo.ActiveTime, Refresh);
        }

        bool IsAvailableBuildState(bool sendToastMessage)
        {
            var buildingLevelData = BuildingLevelData.GetDataByGroupAndLevel(Data.BaseData.KEY, Data.BuildInfo.Level);
            if (buildingLevelData == null)// 최대 레벨 및 데이터 유효성 체크
            {
                if (sendToastMessage)
                    ToastManager.On(100002076);

                return false;
            }
            
            if (buildingLevelData.NEED_AREA_LEVEL > User.Instance.GetAreaLevel())// 건설 조건 체크
            {
                if (sendToastMessage)
                    ToastManager.On(100000059, buildingLevelData.NEED_AREA_LEVEL);

                return false;
            }

            var buildingMaxLevel = BuildingLevelData.GetBuildingMaxLevelByGroup(Data.BaseData.KEY);
            if(buildingMaxLevel > 0 && buildingMaxLevel <= Data.BuildInfo.Level)
            {
                //BuildingLayer 에서 UpgradeMax 타입 일 때 처리 하고 있음.
                //if (sendToastMessage)
                //    ToastManager.On(100002076);

                return false;
            }

            if (Data.BuildInfo.State != eBuildingState.NORMAL)// 건설 가능 상태 체크
            {
                if (sendToastMessage)
                {
                    switch (Data.BuildInfo.State)
                    {
                        case eBuildingState.CONSTRUCTING:
                            ToastManager.On(100000644);
                            break;
                        case eBuildingState.NONE:
                            ToastManager.On(100000633);
                            break;
                    }
                }

                return false;
            }
            
            if (isGetProductItem())// 생산중인 품목이 있는지 체크
            {
                if (sendToastMessage)
                    ToastManager.On(100000556);

                return false;
            }

            return true;
        }

        void InitTimeObj()
        {
            if (constructTimeObj == null)
            {
                constructTimeObj = nodeTimer.GetComponent<TimeObject>();
            }

            if (constructTimeObj.Refresh != null)
            {
                constructTimeObj.Refresh = null;
            }

			if(produceTimeObj == null)
			{
				produceTimeObj = batteryTimer.GetComponent<TimeObject>();
			}

			if(produceTimeObj.Refresh != null)
			{
				produceTimeObj.Refresh = null;
			}
        }

        protected override void Init()
        {
            SetBatteryTween();
            InitTimeObj();
        }

        private void SetBatteryTween()
        {
            batteryImage.transform.localScale = Vector3.one;
            batteryImage.transform.DOScale(Vector3.one * 1.4f, 0.5f).SetLoops(-1, LoopType.Yoyo);
        }

        protected override void Refresh()
        {
            InitwithUserData(Data.BuildingKey, Data.BuildInfo.Tag);
            SetLayerInfo();
            RefreshQueueTab();
            RefresBatteryUI();
            RefreshBuildingNameLabel();
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

        void TryHarvest()
        {
            var isAvailableHarvest = isGetProductItem();
            if (isAvailableHarvest)//회수 가능한 상태
            {
                RequestBuildingGetAllProduct();
            }
        }
    }
}