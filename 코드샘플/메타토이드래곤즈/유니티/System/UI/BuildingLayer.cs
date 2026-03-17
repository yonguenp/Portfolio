using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public abstract class BuildingLayer : TabLayer
    {
        protected enum eUpgradeButtonState
        {
            LOCK,
            ConstructAble,
            Constructing,
            ConstructFinish,
            UpgradeDisable,
            UpgradeAble,
            Upgrading,
            UpgradeFinish,
            UpgradeMax,
            None,
        }

        [SerializeField]
        protected Button ConstructButton = null;
        [SerializeField]
        protected Text ConstructButtonText = null;
        [SerializeField]
        protected BuildingZoomPopup buildingZoomPopup = null;

        protected BuildingPopupData Data { get; private set; } = null;
        
        public eLandmarkType eLandmarkType { get; protected set; } = eLandmarkType.UNKNOWN;
        protected abstract void Init();
        protected abstract void Refresh();
        protected abstract void Clear();

        protected abstract void SetLockState();
        protected abstract void SetNotBuiltState();
        protected abstract void SetConstructingState();
        protected abstract void SetNormalState();

        public abstract void OnClickConstruct();
        public abstract void OnClickConstructFinish();
        public abstract void OnClickUpgrade();

        protected virtual void SetUpgradeBtnState(eUpgradeButtonState state)
        {
            ConstructButton.onClick.RemoveAllListeners();
            ConstructButton.gameObject.SetActive(true);
            ConstructButton.SetButtonSpriteState(true);
            switch (state)
            {
                case eUpgradeButtonState.LOCK:
                    ConstructButton.onClick.AddListener(() => OnClickConstruct());
                    ConstructButtonText.text = StringData.GetStringByIndex(100000019);
                    ConstructButton.SetButtonSpriteState(false);
                    break;
                case eUpgradeButtonState.ConstructAble:
                    ConstructButton.onClick.AddListener(() => OnClickConstruct());
                    ConstructButtonText.text = StringData.GetStringByIndex(100000019);
                    break;
                case eUpgradeButtonState.Constructing:
                    ConstructButtonText.text = StringData.GetStringByIndex(100000068);
                    ConstructButton.SetButtonSpriteState(false);
                    break;
                case eUpgradeButtonState.ConstructFinish:
                    //ConstructButton.onClick.AddListener(() => OnClickConstructFinish());
                    //ConstructButtonText.text = StringData.GetString(100000073);
                    ConstructButton.gameObject.SetActive(false);
                    break;
                case eUpgradeButtonState.UpgradeDisable:
                    ConstructButton.onClick.AddListener(() => OnClickUpgrade());
                    ConstructButtonText.text = StringData.GetStringByIndex(100000020);
                    ConstructButton.SetButtonSpriteState(false);
                    break;
                case eUpgradeButtonState.UpgradeAble:
                    ConstructButton.onClick.AddListener(() => OnClickUpgrade());
                    ConstructButtonText.text = StringData.GetStringByIndex(100000020);
                    break;
                case eUpgradeButtonState.Upgrading:
                    ConstructButtonText.text = StringData.GetStringByIndex(100000108);
                    ConstructButton.SetButtonSpriteState(false);
                    break;
                case eUpgradeButtonState.UpgradeFinish:
                    //ConstructButton.onClick.AddListener(() => OnClickConstructFinish());
                    //ConstructButtonText.text = StringData.GetString(100000074);
                    ConstructButton.gameObject.SetActive(false);
                    break;
                case eUpgradeButtonState.UpgradeMax:
                    ConstructButtonText.text = StringData.GetStringByIndex(100000329);
                    ConstructButton.onClick.AddListener(() => { ToastManager.On(100002530); });
                    ConstructButton.SetButtonSpriteState(false);
                    break;
                case eUpgradeButtonState.None:
                    ConstructButton.gameObject.SetActive(false);
                    break;
            }
        }

        protected TimeObject timeObject = null;
        
        void OnDisable()
        {
            if (timeObject != null)
            {
                timeObject.Refresh = null;
            }

            Clear();
        }

        public override void InitUI(TabTypePopupData datas)
        {
            base.InitUI(datas);

            Init();
            //Refresh();
        }

        public virtual void SetData(BuildingPopupData data)
        {
            Data = data;
        }

        public virtual void InitData(BuildingPopupData data)
        {
            SetData(data);

            Init();
           // Refresh();
        }

        public override void RefreshUI()
        {
            RefreshTime();
            Refresh();
        }

        void RefreshTime()
        {
            if (timeObject != null && timeObject.Refresh != null)
            {
                timeObject.Refresh();
            }
        }

        bool isBuildingProductItem()
        {
            if (Data == null)
                return false;

            var buildingInfo = Data.BuildInfo;
            if (buildingInfo == null)
                return false;

            if (buildingInfo.State != eBuildingState.NORMAL)
                return false;

            var currentBuildingTag = Data.BuildingTag;
            if (currentBuildingTag <= 0)
                return false;

            var produce = User.Instance.GetProduces(currentBuildingTag);
            if (produce == null)
                return false;

            var produceItemList = produce.Items;
            if (produceItemList == null || produceItemList.Count <= 0)
                return false;

            return true;
        }

        public virtual bool isGetProductItem()//현재 건물이 회수 가능 품목이 있는지
        {
            if (!isBuildingProductItem())
                return false;

            var produceItemList = User.Instance.GetProduces(Data.BuildingTag).Items;
            foreach (var produceRecipe in produceItemList)
            {
                var state = produceRecipe.State;
                switch(state)
                {
                    case eProducesState.UnKnown:
                    case eProducesState.Idle:
                    case eProducesState.None:
                        break;

                    case eProducesState.Complete:
                        return true;
                    case eProducesState.Ing:
                        var isCompleteCondition = produceRecipe.ProductionExp <= TimeManager.GetTime();
                        if (isCompleteCondition)
                            return true;
                        break;
                }
            }

            return false;
        }
        public virtual bool isRunning()//현재 건물이 생산 중인지
        {
            if (!isBuildingProductItem())
                return false;

            var produceItemList = User.Instance.GetProduces(Data.BuildingTag).Items;
            foreach (var produceRecipe in produceItemList)
            {
                var state = produceRecipe.State;
                switch (state)
                {
                    case eProducesState.UnKnown:
                    case eProducesState.Idle:
                    case eProducesState.None:
                    case eProducesState.Complete:
                        break;
                    case eProducesState.Ing:
                        var isRunning = produceRecipe.ProductionExp > TimeManager.GetTime();
                        if (isRunning)
                            return true;
                        break;
                }
            }

            return false;
        }

        public virtual void RequestBuildingGetAllProduct()
        {
            WWWForm data = new WWWForm();
            data.AddField("tag", Data.BuildingTag);
            data.AddField("auto", 1);//해당 건물에서만 생산된 건물 받기 플래그

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
                            RequestInventoryOpenPopup();
                            break;
                        case (int)eApiResCode.NOTHING_TO_HARVEST:
                            ToastManager.On(100000812);
                            break;
                    }
                }
            });
        }

        protected void RequestInventoryOpenPopup()
        {
            SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100002077), StringData.GetStringByIndex(100000414), "",
                () => {
                    PopupManager.OpenPopup<InventoryPopup>();
                }
            );
        }

        protected bool IsAvailableGetItemCondition()//지금은 일반 생산에서만 쓰고 있지만, 건전지 공장도 필요할 것 같아서 부모쪽으로 이동
        {
            List<Asset> ItemList = GetRewardItemFirstCheck();
            return ItemList.Count > 0 && User.Instance.CheckInventoryGetItem(ItemList);
        }

        List<Asset> GetRewardItemFirstCheck()
        {
            List<Asset> ItemList = new List<Asset>();//현재 건물 인덱스를 가지고 획득 생산품만 체크로 변경
            var currentProduceBuilding = User.Instance.GetProduces(Data.BuildingTag);
            string index = BuildingOpenData.GetWithTag(Data.BuildingTag).BUILDING;
            List<ProducesRecipe> queueList = currentProduceBuilding.Items;

            if (queueList != null)
            {
                List<int> item = new List<int>();
                for (int i = 0; i < queueList.Count; ++i)
                {
                    item.Clear();
                    if ((int)queueList[i].State == 3)
                    {
                        ProductData itemInfo = ProductData.GetProductDataByGroupAndKey(index, queueList[i].RecipeID);
                        ItemList.Add(new Asset(itemInfo.ProductItem));
                    }
                }
            }
            return ItemList;
        }

        public virtual bool IsCloseAble()
        {
            return true;
        }

        public virtual void CloseAction()
        {

        }
    }
}