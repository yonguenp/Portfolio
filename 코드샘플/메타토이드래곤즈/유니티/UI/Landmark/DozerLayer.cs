using DG.Tweening;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class DozerAutoProductInfo
    {
        public int Level { get; private set; } = -1;

        Dictionary<int, ProductAutoData> productDataDic = new Dictionary<int, ProductAutoData>();

        public DozerAutoProductInfo(List<ProductAutoData> data)
        {
            SetData(data);
        }
        public DozerAutoProductInfo(int level)
        {
            SetData(ProductAutoData.GetListByGroupAndLevel("dozer", level));
        }

        public ProductAutoData Gold
        {
            get
            {
                if (productDataDic.ContainsKey((int)eDozerRewardType.GOLD))
                {
                    return productDataDic[(int)eDozerRewardType.GOLD];
                }
                return null;
            }
        }

        public ProductAutoData Gemstone
        {
            get
            {
                if (productDataDic.ContainsKey((int)eDozerRewardType.GEMSTONE))
                {
                    return productDataDic[(int)eDozerRewardType.GEMSTONE];
                }
                return null;
            }
        }

        public ProductAutoData Item
        {
            get
            {
                if (productDataDic.ContainsKey((int)eDozerRewardType.ITEM))
                {
                    return productDataDic[(int)eDozerRewardType.ITEM];
                }
                return null;
            }
        }

        public ProductAutoData ItemGroup
        {
            get
            {
                if (productDataDic.ContainsKey((int)eDozerRewardType.ITEM_GROUP))
                {
                    return productDataDic[(int)eDozerRewardType.ITEM_GROUP];
                }
                return null;
            }
        }

        public void SetData(List<ProductAutoData> productDatas)
        {
            if (productDatas == null)
                return;

            Level = productDatas[0].LEVEL;

            foreach (var item in productDatas)
            {
                int tempKey = convertRewardType(item.TYPE);

                if (productDataDic.ContainsKey(tempKey))
                {
                    productDataDic[tempKey] = item;
                    return;
                }

                productDataDic.Add(tempKey, item);
            }
        }

        public ProductAutoData GetData(string type)
        {
            int tempKey = convertRewardType(type);

            productDataDic.TryGetValue(tempKey, out ProductAutoData resultData);

            return resultData;
        }

        int convertRewardType(string type)
        {
            eDozerRewardType tempType = eDozerRewardType.GOLD;
            switch (type)
            {
                case "GOLD":
                    tempType = eDozerRewardType.GOLD;
                    break;

                case "GEMSTONE":
                    tempType = eDozerRewardType.GEMSTONE;
                    break;

                case "ITEM":
                    tempType = eDozerRewardType.ITEM;
                    break;

                case "ITEM_GROUP":
                    tempType = eDozerRewardType.ITEM_GROUP;
                    break;
            }

            return (int)tempType;
        }
    }
    public class DozerLayer : BuildingLayer, EventListener<LandmarkUpdateEvent>
    {
        [SerializeField] GameObject levelUpNode = null;

        [Space(10)]
        [Header("dozer reward info")]
        [SerializeField] Text labelRewardGemstone = null;
        [SerializeField] Text labelRewardGold = null;
        [SerializeField] Text labelRewardBat = null;
        [SerializeField] Text labelRewardItem = null;
        [SerializeField] private TableViewGrid tableView = null;

		// 건설 관련 작업 레드 닷
		[Space(10)]
        [Header("Red Dot")]
        [SerializeField] GameObject nodeReddotConstruct = null;
        [SerializeField] GameObject nodeReddotGetItem = null;

        [Space(10)]
        [Header("button and button effect")]
        [SerializeField] GameObject levelupButtonEffect = null;
        [SerializeField] Button getItemButton = null;


        [Space(10)]
        [Header("dozer info")]
        [SerializeField] Text dozerLevelText = null;
        [SerializeField] Text labelRemainTimer = null;
        [SerializeField] Text labelConstructTimer = null;
        [SerializeField] Text GuideLabel = null;

        [SerializeField] Animation rightMenuAnim = null;
        [SerializeField] LandMarkPopup landMarkTempPopup = null;

        bool isTableViewFirstInit = false;

        private LandmarkDozer dozer = null;
        public LandmarkDozer Dozer
        {
            get
            {
                if (dozer == null)
                    dozer = User.Instance.GetLandmarkData<LandmarkDozer>();

                return dozer;
            }
        }

        protected void OnDisable()
        {
            EventManager.RemoveListener(this);
        }

        private void OnEnable()
        {
            EventManager.AddListener(this);
            rightMenuAnim.Play();
        }
        public void OnEvent(LandmarkUpdateEvent eventData)
        {
            if (eventData.eLandmark != eLandmarkType.Dozer)
                return;

            RewardRefresh();
            //PopupManager.ForceUpdate<LandMarkTempPopup>();
        }

        protected override void Clear()
        {

        }
        protected override void Init()
        {
            eLandmarkType = eLandmarkType.Dozer;
            

            if (tableView != null && !isTableViewFirstInit)
            {
                tableView.OnStart();
                isTableViewFirstInit = true;
            }
            RefreshUI();
        }

        protected override void Refresh()//데이터 갱신        
        {
            if (levelUpNode != null)
                levelUpNode.SetActive(false);

            if (levelupButtonEffect != null)
                levelupButtonEffect.SetActive(false);

            RefreshLevelTextByBuildingLevel();

            labelRemainTimer.text = "00:00";

            switch (Data.BuildInfo.State)
            {
                case eBuildingState.LOCKED:
                    SetLockState();
                    break;
                case eBuildingState.NOT_BUILT:
                    SetNotBuiltState();
                    break;
                case eBuildingState.CONSTRUCTING:
                    SetConstructingState();
                    break;
                case eBuildingState.CONSTRUCT_FINISHED:
                    if (timeObject != null)
                        timeObject.Refresh = null;
                    if (buildingZoomPopup != null)
                        buildingZoomPopup.ClosePopup();
                    break;
                default:
                    SetNormalState();
                    break;
            }
        }

        void RefreshLevelTextByBuildingLevel()
        {
            string currentBuildingLevel = string.Format(StringData.GetStringByIndex(100000056), Data.Level);
            if (Data.Level <= 0)
            {
                currentBuildingLevel = StringData.GetStringByStrKey("building_construction_progress");
            }
            switch (Data.BuildInfo.State)
            {
                case eBuildingState.LOCKED:
                case eBuildingState.NOT_BUILT:
                    currentBuildingLevel = StringData.GetStringByStrKey("건설 전");
                    break;
            }
            dozerLevelText.text = currentBuildingLevel;
        }

        protected override void SetLockState()
        {
            SetUpgradeBtnState(eUpgradeButtonState.LOCK);
            CheckConstructAble();

            if (getItemButton != null)
            {
                getItemButton.gameObject.SetActive(false);
            }

            if (GuideLabel != null)
            {
                GuideLabel.gameObject.SetActive(true);
                string constStr = SBFunc.StrBuilder(StringData.GetStringByIndex(100000059), Data.OpenData.OPEN_LEVEL);
                GuideLabel.text = constStr;
            }
        }

        protected override void SetNotBuiltState()
        {
            SetUpgradeBtnState(eUpgradeButtonState.ConstructAble);
            CheckConstructAble();

            if (getItemButton != null)
            {
                getItemButton.gameObject.SetActive(false);
            }

            if (GuideLabel != null)
            {
                GuideLabel.gameObject.SetActive(true);
                GuideLabel.text = StringData.GetStringByStrKey("도저건설시작요청");
            }
        }

        protected override void SetConstructingState()
        {
            GuideLabel.gameObject.SetActive(true);
            
			var buildingInfo = User.Instance.GetUserBuildingInfoByTag((int)eLandmarkType.Dozer);

            if (buildingInfo.Level == 0)
            {
                SetUpgradeBtnState(eUpgradeButtonState.Constructing);
				GuideLabel.text = StringData.GetStringByIndex(100000068);
			}
            else
            {
                SetUpgradeBtnState(eUpgradeButtonState.Upgrading);
				GuideLabel.text = StringData.GetStringByIndex(100000108);
			}

			nodeReddotConstruct.SetActive(false);

			if (TimeManager.GetTimeCompare(buildingInfo.ActiveTime) > 0 && buildingInfo.State != eBuildingState.CONSTRUCT_FINISHED)
            {
                timeObject = GetComponent<TimeObject>();
                timeObject.Time = TimeManager.GetTime();
                levelUpNode.SetActive(true);
                timeObject.Refresh = () =>
                {
                    labelConstructTimer.text = TimeManager.GetTimeCompareString(buildingInfo.ActiveTime);
                    if (TimeManager.GetTimeCompare(buildingInfo.ActiveTime) <= 0)
                    {
                        //완료
                        if (timeObject != null)
                        {
                            timeObject.Refresh = null;
                        }
                        if (buildingInfo.Level == 1)
                        {
                            SetUpgradeBtnState(eUpgradeButtonState.ConstructFinish);
                        }
                        else
                        {
                            SetUpgradeBtnState(eUpgradeButtonState.UpgradeFinish);
                        }
                        levelUpNode.SetActive(false);
                        
                        //RefreshCompleteButtonLabel(buildingInfo, false);//건설 완료 버튼 라벨 갱신
                    }
                };

                timeObject.Refresh();
            }
            else
            {
                GuideLabel.text = StringData.GetStringByIndex(100000949);

                if (buildingInfo.Level == 1)
                {
                    SetUpgradeBtnState(eUpgradeButtonState.ConstructFinish);
                }
                else
                {
                    SetUpgradeBtnState(eUpgradeButtonState.UpgradeFinish);
                }
            }

            if (getItemButton != null)
            {
                getItemButton.gameObject.SetActive(false);
            }
        }

        protected override void SetNormalState()
        {
            if (Data.BuildingKey != "dozer")
                return;

            GuideLabel.text = "";
            DozerAutoProductInfo dozerInfo = new DozerAutoProductInfo(Data.Level);
            BuildingLevelData buildLevelData = Data.LevelData;

            if (buildLevelData.UPGRADE_TIME == 0)
            {
                SetUpgradeBtnState(eUpgradeButtonState.None);
            }
            else
            {
                int exteriorLevel = User.Instance.ExteriorData.ExteriorLevel;//현재 외형렙
                int needExteriroLevel = BuildingLevelData.GetLevelUpNeedAreaLevel("dozer", Data.BuildInfo.Level);//필요 외형렙
                int maxLevel = BuildingLevelData.GetBuildingMaxLevelByGroup(Data.BaseData.KEY);

                var upgradeState = eUpgradeButtonState.None;
                if(Data.BuildInfo.Level >= maxLevel)
                {
                    upgradeState = eUpgradeButtonState.UpgradeMax;
                }
                else
                {
                    if (needExteriroLevel <= exteriorLevel)
                    {
                        bool checkUpgrade = true;
                        foreach (var item in buildLevelData.NEED_ITEM)
                        {
                            checkUpgrade &= User.Instance.GetItemCount(item.ItemNo) >= item.Amount;
                        }

                        upgradeState = checkUpgrade ? eUpgradeButtonState.UpgradeAble : eUpgradeButtonState.UpgradeDisable;
                    }
                    else
                    {
                        upgradeState = eUpgradeButtonState.UpgradeDisable;
                    }
                }

                nodeReddotConstruct.SetActive(upgradeState == eUpgradeButtonState.UpgradeAble);
                SetUpgradeBtnState(upgradeState);

                if (levelupButtonEffect != null)
                    levelupButtonEffect.SetActive(upgradeState == eUpgradeButtonState.UpgradeAble);
            }

            if (getItemButton != null)
            {
                getItemButton.gameObject.SetActive(true);
            }

            if (dozerInfo != null)
            {
                if(labelRewardGemstone != null && dozerInfo.Gemstone != null)
                    labelRewardGemstone.text = string.Format(StringData.GetStringByIndex(100000109), dozerInfo.Gemstone.TERM / 60, dozerInfo.Gemstone.ProductItem.Amount);
                if (labelRewardGold != null && dozerInfo.Gold != null)
                    labelRewardGold.text = string.Format(StringData.GetStringByIndex(100000109), dozerInfo.Gold.TERM / 60, dozerInfo.Gold.ProductItem.Amount);
                if (labelRewardBat != null && dozerInfo.Item != null)
                    labelRewardBat.text = string.Format(StringData.GetStringByIndex(100000109), dozerInfo.Item.TERM / 60, dozerInfo.Item.ProductItem.Amount);
                if (labelRewardItem != null && dozerInfo.ItemGroup != null)
                    labelRewardItem.text = string.Format(StringData.GetStringByIndex(100000109), dozerInfo.ItemGroup.TERM / 60, dozerInfo.ItemGroup.ProductItem.Amount);
            }

            timeObject = GetComponent<TimeObject>();
            timeObject.Time = TimeManager.GetTime();

            if (TimeManager.GetTime() < Dozer.ExpireTime)
            {
                if (timeObject.Refresh == null)
                {
                    timeObject.Refresh = () =>
                    {                        
                        if (nodeReddotGetItem != null)
                        {
                            if (Dozer.RewardList != null)
                            {
                                nodeReddotGetItem.SetActive(Dozer.RewardList.Count > 0);
                            }
                            else
                            {
                                nodeReddotGetItem.SetActive(false);
                            }
                        }

                        labelRemainTimer.text = TimeManager.GetTimeCompareString(Dozer.ExpireTime);


                        if (TimeManager.GetTimeCompare(Dozer.ExpireTime) <= 0)
                        {
                            Dozer.Recall(true);
                            timeObject.Refresh = null;
                            RewardRefresh();
                        }
                    };
                    timeObject.Refresh();
                }
                else
                {
                    labelRemainTimer.text = TimeManager.GetTimeCompareString(Dozer.ExpireTime);
                }
            }
            else
            {
                labelRemainTimer.text = TimeManager.GetTimeCompareString(0);
                if (nodeReddotGetItem != null)
                {
                    nodeReddotGetItem.SetActive(Dozer.RewardList.Count > 0);
                }
            }

            RewardRefresh();
        }

        public void RewardRefresh()
        {
            List<Asset> rewards = new List<Asset>();
            if (Dozer.RewardList != null && tableView != null)
            {
                foreach(var reward in Dozer.RewardList)
                {
                    if (reward.Amount > 0)
                        rewards.Add(reward);
                }
            }

            RefreshRewardScrollView(rewards, false);

            // 비어있는 경우 문구 처리
            GuideLabel.gameObject.SetActive(rewards.Count <= 0);
        }

        void RefreshRewardScrollView(List<Asset> rewards, bool initPos)
        {
            var tableViewItemList = new List<ITableData>(rewards);

            tableView.SetDelegate(new TableViewDelegate(tableViewItemList, (GameObject node, ITableData item) =>
            {
                if (node == null)
                {
                    return;
                }

                var frame = node.GetComponent<ItemFrame>();
                if (frame == null)
                {
                    return;
                }

                var itemData = (ProductReward)item;

                if (itemData.isNew)
                {
                    Sequence itemTween = DOTween.Sequence();
                    var itemCanvasGroup = node.GetComponent<CanvasGroup>();
                    if (itemCanvasGroup == null)
                    {
                        itemCanvasGroup = node.AddComponent<CanvasGroup>();
                    }
                    itemCanvasGroup.alpha = 0f;
                    node.transform.localScale = Vector3.one * 1.2f;
                    itemTween.Append(itemCanvasGroup.DOFade(1, .3f));
                    itemTween.Join(node.transform.DOScale(Vector3.one, .3f));
                }

                itemData.Checked();

                switch (itemData.GoodType)
                {
                    case eGoodType.GOLD:        // 골드
                    case eGoodType.GEMSTONE:    // 젬스톤
                    case eGoodType.ARENA_TICKET:   // 아레나 티켓
                        frame.setFrameCashInfo((int)itemData.GoodType, itemData.Amount);
                        break;

                    case eGoodType.ENERGY: // 에너지
                        frame.setFrameEnergyInfo(itemData.Amount);
                        break;

                    case eGoodType.ITEM: // 아이템
                        frame.SetFrameItemInfo(itemData.ItemNo, itemData.Amount);
                        break;
                }
            }));
            tableView.ReLoad(initPos);
        }

        void CheckConstructAble()
        {
            var needItems = BuildingLevelData.GetDataByGroupAndLevel("dozer", 0).NEED_ITEM;
            var checkUpgrade = true;

            foreach (var needItem in needItems)
            {
                checkUpgrade &= (needItem.Amount <= User.Instance.GetItemCount(needItem.ItemNo));
            }

            nodeReddotConstruct.SetActive(checkUpgrade);
        }

        public void OnClickGetReward()
        {
            Dozer.GetReward();
        }

        public override void OnClickUpgrade()
        {
            //업그레이드 위한 외형 조건 선체크 및 토스트 뿌려주기
            var dozerbuildingInfo = User.Instance.GetUserBuildingInfoByTag((int)eLandmarkType.Dozer);
            var currentLevel = dozerbuildingInfo.Level;//현재 도저 레벨

            var exteriorLevel = User.Instance.ExteriorData.ExteriorLevel;//현재 외형렙
            var needExteriroLevel = BuildingLevelData.GetLevelUpNeedAreaLevel("dozer", currentLevel);//필요 외형렙

            if (needExteriroLevel > exteriorLevel)
            {
                ToastManager.On(100000059, needExteriroLevel);
                return;
            }

            var currentReward = Dozer.RewardList;
            if (currentReward != null && currentReward.Count > 0)
            {
                SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100000555));
                return;
            }

            var popup = PopupManager.OpenPopup<BuildingTypeDozer>();
            popup.setMessage("dozer", ((int)eLandmarkType.Dozer).ToString());
        }

        public void OnClickLevelUpAccel()
        {
            AccelerationMainPopup.OpenPopup(eAccelerationType.LEVELUP, (int)eLandmarkType.Dozer, Data.LevelData.UPGRADE_TIME, Data.BuildInfo.ActiveTime, Refresh);
        }

        public override void OnClickConstruct()
        {
            PopupManager.OpenPopup<BuildingConstructPopup>(Data);
        }

        public override void OnClickConstructFinish()
        {
            WWWForm msg = new WWWForm();
            msg.AddField("tag", (int)eLandmarkType.Dozer);

            NetworkManager.Send("building/complete", msg, (jsonData) =>
            {
                if (timeObject != null)
                {
                    timeObject.Refresh = null;
                }
                PopupManager.ForceUpdate<LandMarkPopup>();

                var buildingData = User.Instance.GetUserBuildingInfoByTag((int)eLandmarkType.Dozer);
                if (buildingData != null)
                {
                    var currentBuildingLevel = buildingData.Level;

                    BuildingCompletePopup.OpenPopup(!(currentBuildingLevel == 1));
                    Refresh();
                }

                Town.Instance.RefreshMap();
                //Town.Instance.dozer?.SetDozerAnimState(buildingData.State);
            });
        }
    }
}
