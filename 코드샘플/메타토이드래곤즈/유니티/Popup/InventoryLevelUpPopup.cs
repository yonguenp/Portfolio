using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork 
{
    public class InventoryLevelUpPopup : Popup<InventoryData>
    {
        public GameObject itemPrefab = null;
        public ScrollRect needItemScrollRect = null;
        public Text needItemText = null;
        public Button levelUpButton = null;

        public Text currentCountText = null;
        public Text nextCountText = null;

        public Text priceText = null;

        InventoryData currentInvenData = null;
        InventoryData nextInvenData = null;
        
        [SerializeField]
        Image iconTargetImage = null;

        [SerializeField]
        List<Sprite> iconList = new List<Sprite>();
        
        VoidDelegate upgradeCallBack = null;

        private bool isNetworkState = false;
        private bool isSufficientMaterial = false;
        private bool isSufficientCost = false;
        public void SetUpgradeCallBack(VoidDelegate cb)
        {
            if (cb != null)
            {
                upgradeCallBack = cb;
            }
        }

        public void OnClickLevelUp()
        {
            if(!isSufficientCost)
            {
                ToastManager.On(StringData.GetStringByStrKey("재화부족"));//재화가 부족합니다.
                return;
            }

            if(!isSufficientMaterial)
            {
                if (currentInvenData == null)
                    return;

                var needItemList = SBFunc.GetNeedItemList(currentInvenData.NEED_ITEM);
                ProductsBuyNowPopup.OpenPopup(needItemList, () => {
                    InitPopup();
                });
            }

            if (isNetworkState)
            {
                return;
            }
            isNetworkState = true;
            NetworkManager.Send("item/expand", null, (JObject jsonData) =>
            {
                isNetworkState = false;
                if (SBFunc.IsJTokenCheck(jsonData["rs"]))
                {
                    switch (jsonData["rs"].Value<int>())
                    {
                        case (int)eApiResCode.OK:
                            if (SBFunc.IsJTokenCheck(jsonData["inventory_step"]))
                            {
                                User.Instance.Inventory.SetStep(jsonData["inventory_step"].Value<int>());
                                upgradeCallBack?.Invoke();
                            }

                            PopupManager.ClosePopup<InventoryLevelUpPopup>();
                            break;
                    }
                }
            },(string arg) =>
            {
                isNetworkState = false;
            });
        }

        public void OnClickCloseButton()
        {
            PopupManager.ClosePopup<InventoryLevelUpPopup>();
        }

        public override void InitUI()
        {
            InitPopup();
        }

        void InitPopup()
        {
            int currentInvenLevel = User.Instance.Inventory.InvenStep;
            isNetworkState = false;
            currentInvenData = InventoryData.Get(currentInvenLevel.ToString());
            nextInvenData = InventoryData.Get((currentInvenLevel + 1).ToString());

            var needItemList = currentInvenData.NEED_ITEM;
            bool isNeedItem = (needItemList != null && needItemList.Count > 0);

            needItemText.gameObject.SetActive(!isNeedItem);
            needItemText.text = StringData.GetStringFormatByStrKey("필요재료없음");

            bool isSufficientItem = true;
            if(isNeedItem)
            {
                SBFunc.RemoveAllChildrens(needItemScrollRect.content);

                var isItemSufficientCount = 0;
                foreach(var item in needItemList)
                {
                    var clone = Instantiate(itemPrefab, needItemScrollRect.content);
                    var itemComp = clone.GetComponent<ItemFrame>();
                    if(itemComp == null)
                    {
                        Destroy(clone);
                        continue;
                    }

                    itemComp.setFrameRecipeInfo(item.ItemNo, item.Amount);
                    clone.GetComponent<RectTransform>().localScale = new Vector3(0.8f, 0.8f, 0.8f);

                    if (itemComp.IsSufficientAmount)
                        isItemSufficientCount++;
                }

                isSufficientItem = isItemSufficientCount == needItemList.Count;
            }

            currentCountText.text = currentInvenData.SLOT.ToString();
            nextCountText.text = nextInvenData.SLOT.ToString();

            priceText.text = SBFunc.CommaFromNumber(currentInvenData.COST_NUM);

            int goalCost = 0;
            int spriteIndex = 0;
            switch (currentInvenData.COST_TYPE.ToLower())
            {
                case "gold":
                    goalCost = User.Instance.GOLD;
                    break;
                case "gemstone":
                    goalCost = User.Instance.GEMSTONE;
                    spriteIndex = 1;
                    break;

            }
            iconTargetImage.sprite = iconList[spriteIndex];

            isSufficientCost = goalCost >= currentInvenData.COST_NUM;
            priceText.color = !isSufficientCost ? Color.red : Color.white;

            isSufficientMaterial = isSufficientItem;
            levelUpButton.SetButtonSpriteState(isSufficientMaterial);
        }
    }
}