using DG.Tweening;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class BuildingUpgradePopup : Popup<BuildingPopupData>
    {
        public Text buildingNameText = null;
        public Text currentLevelText = null;
        public Text nextLevelText = null;

        public Image itemImage = null;
        public Text itemDescText = null;

        public Text timeText = null;

        public ScrollRect receipeScrollRect = null;

        public Button levelupButton = null;
        public Text priceText = null;

        public GameObject NewItemObject = null;

        ProductData newProductData = null;
        ItemBaseData newProductItemData = null;

        VoidDelegate buildingUpgradeCallBack = null;

        protected bool isSufficientMaterial = true;
        protected bool isSufficientGold = true;
        protected bool isSufficientTown = true;

        private bool isNetworkState = false;
        public override void ForceUpdate(BuildingPopupData data)
        {
            base.DataRefresh(data);
            Refresh();
        }

        public override void InitUI()
        {
            InitPopup();
        }

        public void SetUpgradeCallBack(VoidDelegate callBack)
        {
            if(callBack != null)
            {
                buildingUpgradeCallBack = callBack;
            }
        }

        public void OnClickLevelUpButton()
        {
            if (Data == null)
                return;

            if (!isSufficientGold)//골드 부족
            {
                ToastManager.On(100000646);
                return;
            }

            if(!isSufficientMaterial)//재료 부족 -> 즉시구매연결
            {
                var needItemList = SBFunc.GetNeedItemList(Data.LevelData.NEED_ITEM);
                if (needItemList.Count <= 0)
                {
                    ToastManager.On(100000646);
                    return;
                }

                ProductsBuyNowPopup.OpenPopup(needItemList, () => {
                    InitPopup();
                });

                return;
            }

            if(!isSufficientTown)//타운 조건 부족
            {
                ToastManager.On(100000646);
                return;
            }

            var sendUrl = Data.Level > 0 ? "building/levelup" : "building/construct";

            WWWForm paramData = new WWWForm();
            paramData.AddField("tag", Data.BuildingTag);
            if (isNetworkState)
            {
                return;
            }
            isNetworkState = true;
            NetworkManager.Send(sendUrl, paramData, (jsonData) =>
            {
                isNetworkState = false;
                if (SBFunc.IsJTokenCheck(jsonData["rs"]))
                {
                    switch (jsonData["rs"].Value<int>())
                    {
                        case (int)eApiResCode.OK:

                            PopupManager.ClosePopup<BuildingUpgradePopup>();
                            PopupManager.ClosePopup<BatteryBuildingUpgradePopup>();
                            PopupManager.ClosePopup<BuildingMineUpgradePopup>();
                            buildingUpgradeCallBack?.Invoke();
                            Town.Instance.RefreshMap();
                            break;
                        case (int)eApiResCode.BUILDING_QUEUE_NOT_EMPTY:
                            SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100000556));
                            break;
                    }
                }
            }, (string arg) =>
            {
                isNetworkState = false;
            });
        }

        protected virtual void InitProduct()
        {
            newProductData = ProductData.GetProductDataByGroupAndLevel(Data.OpenData.BUILDING, Data.BuildInfo.Level + 1);
            if (newProductData != null)
            {
                if(NewItemObject != null)
                    NewItemObject.SetActive(true);
                newProductItemData = newProductData.ProductItem.BaseData;
                itemImage.sprite = ResourceManager.GetResource<Sprite>(eResourcePath.ItemIconPath, newProductData.ICON);
                itemDescText.text = string.Format(StringData.GetStringByIndex(100000076), newProductItemData.NAME);
            }
            else
            {
                itemDescText.text = StringData.GetStringByStrKey(Data.OpenData.BUILDING + "_" + (Data.BuildInfo.Level + 1).ToString() +"_desc");
                if (NewItemObject != null)
                    NewItemObject.SetActive(false);
            }
        }
        protected void InitPopup()
        {
            if (Data == null) { return; }
            isNetworkState = false;
            // UI 세팅
            buildingNameText.text = StringData.GetStringByStrKey(Data.BaseData._NAME);
            currentLevelText.text = Data.Level.ToString();
            nextLevelText.text = (Data.Level + 1).ToString();

            timeText.text = SBFunc.TimeString(Data.LevelData.UPGRADE_TIME);

            InitProduct();

            isSufficientGold = true;
            isSufficientMaterial = true;
            isSufficientTown = true;

            // 재료 정보 세팅 및 업그레이드 가능 여부 체크
            SBFunc.RemoveAllChildrens(receipeScrollRect.content);

            foreach (var needItem in Data.LevelData.NEED_ITEM)
            {
                GameObject newReceipe = Instantiate(ResourceManager.GetResource<GameObject>(eResourcePath.PrefabClonePath, "itemPrefab"));
                newReceipe.transform.SetParent(receipeScrollRect.content.transform);
                newReceipe.transform.localScale = Vector3.one;

                ItemFrame itemframe = newReceipe.GetComponent<ItemFrame>();
                if (itemframe != null)
                {
                    itemframe.setFrameRecipeInfo(needItem.ItemNo, needItem.Amount);

                    isSufficientMaterial = isSufficientMaterial && itemframe.IsSufficientAmount;
                }
            }
            if (Data.LevelData.COST_NUM > 0)
            {
                int userGold = User.Instance.GOLD;
                isSufficientGold = userGold >= Data.LevelData.COST_NUM;

                priceText.text = SBFunc.CommaFromNumber(Data.LevelData.COST_NUM);
            }

            SetTownCondition();

            levelupButton.SetButtonSpriteState(isSufficientGold && isSufficientMaterial && isSufficientTown);
			priceText.color = isSufficientGold ? Color.white : Color.red;
        }

        public virtual void SetTownCondition()
        {
            isSufficientTown = Data.LevelData.NEED_AREA_LEVEL <= User.Instance.TownInfo.AreaLevel;
        }


        void Refresh()
        {
            //InitPopup();
        }


    }
}

