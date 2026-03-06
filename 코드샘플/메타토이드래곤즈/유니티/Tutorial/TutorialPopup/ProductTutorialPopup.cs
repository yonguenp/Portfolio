using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace SandboxNetwork
{
    public class ProductTutorialPopup : Popup<PopupData>
    {
        [Header("buttons")]
        [SerializeField]
        Button GetAllBtn;
        [SerializeField]
        Button CancelAllBtn;

        [Header("item Slot")]
        [SerializeField]
        GameObject itemLayer;
        [SerializeField]
        Image QueueItemImg;
        [SerializeField]
        Text QueueItemAmountText;
        [SerializeField]
        GameObject itemCheck;

        [Header("add Slot")]
        [SerializeField]
        Text addSlotCostText;
        [SerializeField]
        Image addSlotItemImg;

        [Header("recipe card")]
        [SerializeField]
        RecipeCard[] recipeCards;

        int buildingTag = 0;
        int itemCompleteCount = 0;
        int recipeID = 0;
        ProducesBuilding produceBuildingData;
        BuildingBaseData currentBuildingData;
        BuildingOpenData openData;

        public override void InitUI()
        {
            buildingTag = TutorialManager.tutorialManagement.GetCurTutoPrivateKey();
            DataSetting();
            SetDefaultBtnState();
            AddSlotSetting();
            SetRecipeCard();

            TutorialManager.tutorialManagement.NextTutorialStart();
        }

        void SetDefaultBtnState()
        {
            GetAllBtn.SetButtonSpriteState(false);
            CancelAllBtn.SetButtonSpriteState(false);
        }

        void DataSetting()
        {
            openData = BuildingOpenData.GetWithTag(buildingTag);
            produceBuildingData = User.Instance.GetProduces(buildingTag);
            currentBuildingData = openData.BaseData;
        }


        void AddSlotSetting()
        {
            SlotCostData slotCostInfo = SlotCostData.GetByType(eSlotCostInfoType.Product, (produceBuildingData.Slot - currentBuildingData.START_SLOT) + 1);

            if (produceBuildingData.Slot < currentBuildingData.MAX_SLOT && slotCostInfo != null)
            {
                gameObject.SetActive(true);
                addSlotCostText.text = slotCostInfo.COST_NUM.ToString();
                switch (slotCostInfo.COST_TYPE.ToUpper())
                {
                    case "GOLD":
                        addSlotItemImg.sprite = ResourceManager.GetResource<Sprite>(eResourcePath.ItemIconPath, "gold");
                        break;
                    case "GEMSTONE":
                        addSlotItemImg.sprite = ResourceManager.GetResource<Sprite>(eResourcePath.ItemIconPath, "gemstone");
                        break;
                }
            }
        }

        void SetRecipeCard()
        {
            List<ProductData> receipeArray = ProductData.GetProductListByGroup(currentBuildingData.KEY);
            for (int i = 0; i < receipeArray.Count; ++i)
            {
                int key = int.Parse(receipeArray[i].KEY.ToString());

                var recipeCardComp = recipeCards[i];
                if (recipeCardComp != null)
                {
                    if(i == 0)
                        recipeID = key;
                    recipeCardComp.Init(ProductData.GetProductDataByGroupAndKey(receipeArray[i].BUILDING_GROUP, key), buildingTag, 1 >= receipeArray[i].BUILDING_LEVEL, null, itemCompleteCount, null);
                }
            }
        }

        public void EnqueueItem()
        {
            ProductData itemInfo = ProductData.GetProductDataByGroupAndKey(currentBuildingData.KEY, recipeID);
            itemLayer.SetActive(true);
            QueueItemImg.sprite = itemInfo.ICON_SPRITE;
            QueueItemAmountText.text = itemInfo.ProductItem.Amount.ToString();
        }

        public void OnClickRecipeCard()
        {
            //
            CancelAllBtn.SetButtonSpriteState(true);
            EnqueueItem();
        }

        public void OnClickQueueItem()
        {
           
            ProductData itemInfo = ProductData.GetProductDataByGroupAndKey(currentBuildingData.KEY, recipeID);
            var time = itemInfo.PRODUCT_TIME;
            PopupManager.OpenPopup<AccelerationTutorialPopup>(new AccelerationMainData(eAccelerationType.JOB, buildingTag, time, time + TimeManager.GetTime(),
                () =>
                {
                    OnChangeQueueItem();
                })).SetRecipeId(recipeID);
            
        }

        public void OnChangeQueueItem()
        {
            itemCheck.SetActive(true);
            GetAllBtn.SetButtonSpriteState(true);
            CancelAllBtn.SetButtonSpriteState(false);
        }

        public void ClickItemGet()
        {
            itemLayer.SetActive(false);
            GetAllBtn.SetButtonSpriteState(false);
            itemCheck.SetActive(false);
            WWWForm sendData = new WWWForm();
            sendData.AddField("tag", buildingTag);
            sendData.AddField("slot", 0);
            NetworkManager.SendWithCAPTCHA("produce/harvest", sendData, (jsonObj) => {
                if (SBFunc.IsJTokenType(jsonObj["rs"], JTokenType.Integer) && (eApiResCode)(jsonObj["rs"].Value<int>()) == eApiResCode.INVENTORY_FULL)
                {
                    return;
                }
                if (SBFunc.IsJTokenCheck(jsonObj["rewards"]))
                {
                    var rewardList = SBFunc.ConvertSystemRewardDataList(JArray.FromObject(jsonObj["rewards"]));
                    InventoryIncomeEvent.Send(rewardList, buildingTag);
                }
            });
        }

        public override void ClosePopup()
        {
            base.ClosePopup();
        }
    }
}

