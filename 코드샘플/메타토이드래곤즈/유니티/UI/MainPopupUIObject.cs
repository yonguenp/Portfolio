using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public enum eMainButton
    {
        Town,
        LandMark,
        Building,
        Product,
        Inven,
        Mining,
        MagicShowcase,
    }

    public class MainPopupUIObject : UIObject, EventListener<UIObjectEvent>
	{
		[SerializeField] private Animator pAnimator = null;
		[SerializeField] private Animator lAnimator = null;

		bool isFold = false;

		public override void Init()
		{
			base.Init();

			EventManager.AddListener(this);
        }

        public override void InitUI(eUIType targetType)
        {
            base.InitUI(targetType);
            if (curSceneType > eUIType.None && curUIType.HasFlag(curSceneType))
            {
                RequestUpdateReddotUI();
            }
        }

        public override bool RefreshUI(eUIType targetType) //타입 갱신부는 아래에서 상속으로 구현
        {
            if(base.RefreshUI(targetType))
            {
                RequestUpdateReddotUI();
                return true;
            }
			return false;
		}

        public void RefreshTargetUI(eMainButton targetMenu)
        {
            UpdateTargetReddotUI(targetMenu);
        }

		public void OnClickFoldButton()
		{
			isFold = !isFold;
			
			if (isFold)
			{
				lAnimator.SetTrigger("HIDE");
			}
			else
			{
				lAnimator.SetTrigger("SHOW");
			}
		}

        public void OnClickBuildingConstructListPopup()
        {
            var openBuildingData = new BuildingConstructListData();
            BuildingConstructListPopup.OpenPopup(openBuildingData);
            //var isAvailableOpen = openBuildingData.IsAvailableOpen();
            //if(isAvailableOpen)
            //    BuildingConstructListPopup.OpenPopup(openBuildingData);
            //else//불가능 팝업 연결
            //{
            //    SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100002193));
            //}
        }

        public void OnClickInventoryPopup()
        {
            PopupManager.OpenPopup<InventoryPopup>();
        }

		public void OnEvent(UIObjectEvent eventType)
		{
			if((eventType.t & UIObjectEvent.eUITarget.LB) != UIObjectEvent.eUITarget.NONE)
			{
				switch (eventType.e)
				{
					case UIObjectEvent.eEvent.EVENT_SHOW:
                        pAnimator.SetBool("FOLD", false);
                        pAnimator.SetTrigger("SHOW");
						break;

					case UIObjectEvent.eEvent.EVENT_HIDE:
                        pAnimator.SetBool("SHOW", false);
                        pAnimator.SetTrigger("FOLD");
						break;

                    case UIObjectEvent.eEvent.PRODUCT_DONE:
                        break;

                    //case UIObjectEvent.eEvent.ITEM_GET:
                    //    ReddotManager.Set(eReddotEvent.INVENTORY, true);
                    //    break;
                    //case UIObjectEvent.eEvent.ITEM_CHECK:
                    //    ReddotManager.Set(eReddotEvent.INVENTORY, false);
                    //    break;
                }
			}
		}

        public void OnClickProductBtn()
        {

            //var buildingList = User.Instance.GetUserBuildingList();
            
            var building = BuildingOpenData.GetByBuildingGroup("brick_factory");
            if (building != null && building.Count>0)
            {
                ProductPopup.OpenPopup(building[0].INSTALL_TAG);
            }

        }
        public void OnClickTownManageBtn()
        {
            PopupManager.OpenPopup<TownManagePopup>();
        }

        public void OnClickProductManageButton()
        {
            if (User.Instance.GetAllProducesList(true).Count <= 0)
            {
                ToastManager.On(100003229);
                return;
            }

            //현재 생산 가능한 건물이 있는지 체크
            var buildingList = User.Instance.GetUserBuildingList();
            var state = false;

            for (var i = 0; i < buildingList.Count; i++)
            {
                if (buildingList[i].Tag > 999 && (int)buildingList[i].State >= 3)
                {
                    var data = BuildingOpenData.GetByInstallTag(buildingList[i].Tag);
                    if (data != null)
                    {
                        switch (data.BUILDING)
                        {
                            case "dozer":
                            case "subway":
                            case "travel":
                            {
                            }
                            break;
                            default:
                            {
                                state = true;
                            }
                            break;
                        }
                    }
                }
            }

            if(!state)
            {
                ToastManager.On(100003229);
                return;
            }


            PopupManager.OpenPopup<ProductManagePopup>();
        }

        public void OnClickMiningButton()
        {
            bool active = GameConfigTable.WEB3_MENU_OPEN_ON_KOREAN || User.Instance.ENABLE_P2E;
            if (!active)
            {
                ToastManager.On(StringData.GetStringByIndex(100000636));
                return;
            }

            //타운 조건 체크
            if (!MiningManager.Instance.IsAvailableMiningUIOpen(true))
                return;

            //클라 선처리 조건 - mine/state 를 받기 위해서는 level : 1 이상 일 때 가동이 필요함.

            var buildingInfo = MiningManager.Instance.MineBuildingInfo;
            if(buildingInfo == null)
            {
                ToastManager.On(StringData.GetStringFormatByStrKey("광산토스트6"));
                return;
            }

            if(buildingInfo.Level < 1 && buildingInfo.State != eBuildingState.NORMAL)
            {
                PopupManager.OpenPopup<MiningMainPopup>();
            }
            else
            {
                NetworkManager.Send("mine/state", null, (jsonData) =>
                {
                    if (jsonData.ContainsKey("rs") && jsonData["rs"].Value<int>() == 0)
                    {
                        PopupManager.OpenPopup<MiningMainPopup>();
                    }
                });
            }
        }

        public void OnClickMagicShowcase()
        {
            MagicShowcasePopup.OpenPopup(0);
        }

        #region Reddot

        eReddotEvent GetReddotEventType(eMainButton type)
        {
            switch (type)
            {
                case eMainButton.Town:
                    return eReddotEvent.TOWN;
                case eMainButton.Building:
                    return eReddotEvent.CONSTRUCTION;
                case eMainButton.Product:
                    return eReddotEvent.PRODUCT;
                case eMainButton.Mining:
                    return eReddotEvent.MINING;
                default:
                    return eReddotEvent.NONE;
            }
        }

        public void RequestUpdateReddotUI()
        {
            CancelInvoke(nameof(UpdateReddotUI));
            Invoke(nameof(UpdateReddotUI), 0.2f);
        }

        public void RequestUpdateTownReddot()
        {
            CancelInvoke(nameof(UpdateTownReddot));
            Invoke(nameof(UpdateTownReddot), 0.2f);
        }

        public void RequestUpdateConstructionReddot()
        {
            CancelInvoke(nameof(UpdateConstructionReddot));
            Invoke(nameof(UpdateConstructionReddot), 0.2f);
        }

        public void RequestUpdateProductReddot()
        {
            CancelInvoke(nameof(UpdateProductManageReddot));
            Invoke(nameof(UpdateProductManageReddot), 0.2f);
        }

        public void RequestUpdateMiningReddot()
        {
            CancelInvoke(nameof(UpdateMiningReddot));
            Invoke(nameof(UpdateMiningReddot), 0.2f);
        }
        public void RequestUpdateMagicShowcaseReddot()
        {
            CancelInvoke(nameof(UpdateMagicShowcaseReddot));
            Invoke(nameof(UpdateMagicShowcaseReddot), 0.2f);
        }

        void UpdateReddotUI()
        {
            UpdateTownReddot();
            UpdateConstructionReddot();
            UpdateProductManageReddot();
            UpdateMiningReddot();
            UpdateMagicShowcaseReddot();
        }

        void UpdateTargetReddotUI(eMainButton menuType)
        {
            switch (menuType)
            {
                case eMainButton.Town:
                    UpdateTownReddot();
                    break;
                case eMainButton.Building:
                    UpdateConstructionReddot();
                    break;
                case eMainButton.Product:
                    UpdateProductManageReddot();
                    break;
                case eMainButton.Mining:
                    UpdateMiningReddot();
                    break;
                case eMainButton.MagicShowcase:
                    UpdateMagicShowcaseReddot();
                    break;
            }
        }

        void UpdateConstructionReddot()
        {
            bool isAvailConstruct = false;

            List<BuildingBaseData> BuildingList = BuildingBaseData.GetProductBuildingList();
            List<ConstructInfoData> constructInfoList = new List<ConstructInfoData>();
            constructInfoList.Clear();

            foreach (BuildingBaseData buildingData in BuildingList)
            {
                if (buildingData == null)
                {
                    continue;
                }

                constructInfoList.Add(new ConstructInfoData(buildingData.KEY));
            }

            var contructBuildingList = constructInfoList.FindAll(element => element.openData != null && element.openData.OPEN_LEVEL <= User.Instance.TownInfo.AreaLevel && element.eBuildingState == eBuildingState.NOT_BUILT).ToList();
            if (contructBuildingList != null && contructBuildingList.Count > 0)
            {
                foreach (var building in contructBuildingList)
                {
                    BuildingLevelData levelData = BuildingLevelData.GetDataByGroupAndLevel(building.KEY, 0);
                    if (levelData == null) continue;

                    bool costCheck = User.Instance.IsSufficientCost(SBFunc.GetGoodType(levelData.COST_TYPE), levelData.COST_NUM);
                    bool itemCheck = true;
                    foreach (var needItem in levelData.NEED_ITEM)
                    {
                        itemCheck = User.Instance.GetItemCount(needItem.ItemNo) >= needItem.Amount;
                        if (itemCheck == false)
                        {
                            break;
                        }
                    }

                    if (costCheck && itemCheck)
                    {
                        isAvailConstruct = true;
                        break;
                    }
                }
            }

            // 레드닷 갱신
            ReddotManager.Set(eReddotEvent.CONSTRUCTION, isAvailConstruct);
        }

        void UpdateProductManageReddot()
        {
            List<ProducesBuilding> buildingList = User.Instance.GetAllProducesList(true);

            bool checkProductReddot = false;

            int totalProductCount = 0;
            int completeProductCount = 0;

            int largestRemainTime = 0;
            foreach (ProducesBuilding building in buildingList)
            {
                if (building.Items == null || building.Items.Count <= 0) continue;

                string buildingGroup = BuildingOpenData.GetWithTag(building.Tag).BUILDING;

                totalProductCount += building.Items.Count;
                foreach (ProducesRecipe productItem in building.Items)
                {
                    if (productItem.State == eProducesState.Ing)
                    {
                        int remainTime = TimeManager.GetTimeCompare(productItem.ProductionExp);
                        largestRemainTime = largestRemainTime > remainTime ? largestRemainTime : remainTime;
                    }
                    else if (productItem.State == eProducesState.Complete ||
                        (productItem.State == eProducesState.Ing && productItem.ProductionExp <= TimeManager.GetTime()))
                    {
                        completeProductCount++;
                    }
                }
            }

            checkProductReddot = totalProductCount > 0 && totalProductCount == completeProductCount;

            // 레드닷 갱신
            ReddotManager.Set(eReddotEvent.PRODUCT, checkProductReddot);

            CancelInvoke(nameof(RequestProductReddot));
            if (largestRemainTime > 0)
            {
                Invoke(nameof(RequestProductReddot), largestRemainTime);
            }
            
        }

        void RequestProductReddot()
        {
            ReddotManager.Set(eReddotEvent.PRODUCT, true);
        }

        void UpdateMiningReddot()
        {
            //Debug.Log(">>>>>> UpdateMiningReddot ");
            var buildingInfo = MiningManager.Instance.MineBuildingInfo;
            if(buildingInfo == null)
            {
                ReddotManager.Set(eReddotEvent.MINING, false);
                return;
            }

            var isUpperLevelOne = buildingInfo.Level >= 1 && buildingInfo.State == eBuildingState.NORMAL;//1이상 이냐
            if (isUpperLevelOne)
            {
                MiningManager.Instance.RefreshMiningState(() => {
                    ReddotManager.Set(eReddotEvent.MINING, MiningManager.Instance.IsReddotCondition());
                });
            }
            else
                ReddotManager.Set(eReddotEvent.MINING, MiningManager.Instance.IsReddotCondition());
        }

        void UpdateTownReddot()
        {
            TownExteriorData curExteriorData = User.Instance.ExteriorData;

            if (curExteriorData != null)
            {
                // 업그레이드 가능 여부 체크
                bool reddot = false;

                AreaLevelData levelData = AreaLevelData.GetByLevel(curExteriorData.ExteriorLevel);
                if (levelData != null)
                {

                    if (curExteriorData.ExteriorState == eBuildingState.NORMAL)
                    {
                        bool townMissionCheck = IsTownMissionComplete();
                        bool goldCheck = User.Instance.GOLD >= levelData.NEED_GOLD;
                        bool itemCheck = true;
                        foreach (var needItem in levelData.NEED_ITEM)
                        {
                            itemCheck = User.Instance.GetItemCount(needItem.ItemNo) >= needItem.Amount;
                            if (itemCheck == false)
                            {
                                break;
                            }
                        }

                        reddot = townMissionCheck && goldCheck && itemCheck;
                    }
                    else
                    {
                        reddot = curExteriorData.ExteriorState == eBuildingState.CONSTRUCT_FINISHED;
                    }
                }

                // 층 확장 가능 체크
                if (!reddot)
                {
                    if (curExteriorData.ExteriorFloor < AreaExpansionData.GetMaxFloorLevel())
                    {
                        AreaExpansionData expansionData = AreaExpansionData.GetFloorData(curExteriorData.ExteriorFloor);
                        if (expansionData != null)
                        {
                            bool stateCheck = curExteriorData.ExteriorState == eBuildingState.NORMAL;
                            bool levelCheck = curExteriorData.ExteriorLevel >= expansionData.OPEN_LEVEL;
                            bool costCheck = User.Instance.IsSufficientCost(SBFunc.GetGoodType(expansionData.COST_TYPE), expansionData.COST_NUM);

                            reddot = stateCheck && levelCheck && costCheck;
                        }
                    }
                }

                // 레드닷 갱신
                ReddotManager.Set(eReddotEvent.TOWN, reddot);
            }
        }

        bool IsTownMissionComplete()//현재 타운 미션이 완료가 되었는지 체크 - 확인 후 리턴 풀기
        {
            var currentTownMission = QuestManager.Instance.GetProceedQuestDataByType(eQuestType.TOWN);
            if (currentTownMission == null || currentTownMission.Count <= 0)
                return false;

            var questSize = currentTownMission.Count;
            if (questSize == 1)
            {
                return currentTownMission[0].IsQuestClear();
            }
            else//다중 퀘스트가 들어올 경우 는 없겠지만(지금 타운미션은 1개의 퀘스트와 하위 여러개의 컨디션으로 나뉨) 일단 대응
            {
                int isQuestCompleteCount = 0;
                foreach (var questData in currentTownMission)
                {
                    if (questData == null)
                        continue;

                    if (questData.IsQuestClear())
                        isQuestCompleteCount++;
                }
                return isQuestCompleteCount == currentTownMission.Count;
            }
        }

        void UpdateMagicShowcaseReddot()
        {
            ReddotManager.Set(eReddotEvent.MAGIC_SHOWCASE, MagicShowcaseManager.Instance.IsReddotCondition());
        }

        /// <summary>
        /// 광산 아이콘이 0렙 해금 조건 (타운렙 5 전까지 자물쇠 상태 요청)
        /// </summary>
        public void UpdateMiningLockState()
        {
            if (uiChildrens == null || uiChildrens.Count <= 0)
                return;

            if (uiChildrens.Count <= (int)eMainButton.Mining)
                return;

            uiChildrens[(int)eMainButton.Mining].RefreshUI();
        }
        #endregion
    }
}