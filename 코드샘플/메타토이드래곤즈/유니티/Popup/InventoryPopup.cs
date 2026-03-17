using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    [Flags]
    public enum INVENSORT
    {
        None1 = 0,
        None2 = 1 << 0,
        consumeType_1 = 1 << 1,
        ectType_1 = 1 << 2,
        ectType_2 = 1 << 3,
        consumeType_2 = 1 << 4,
        productType_1 = 1 << 5,
        productType_2 = 1 << 6,
        consumeType_3 = 1 << 7,
        consumeType_4 = 1 << 8,
        adv_materialType_1 = 1 << 9,
        //consumeType_1 = 1 << 10,
        consumeType_5 = 1 << 11,
        ectType_3 = 1 << 12,
        ectType_4 = 1 << 13,
        consumeType_6 = 1 << 14,
        adv_materialType_2 = 1 << 15,
        ectType_5 = 1 << 16,
        ectType_6 = 1 << 17,
        consumeType_7 = 1 << 18,

        CONSUMABLE = (consumeType_1 | consumeType_2 | consumeType_3 | consumeType_4 | consumeType_5 | consumeType_6 | consumeType_7),
        ADV_MATERIAL = adv_materialType_1 | adv_materialType_2,
        PRODUCT = productType_1 | productType_2,
        ETC = ectType_1 | ectType_2 | ectType_3 | ectType_4 | ectType_5 | ectType_6,
        ALL = CONSUMABLE | ADV_MATERIAL | PRODUCT | ETC
    }
    public class InventoryPopup : Popup<PopupData>
    {
        [Header("Common")]
        public GameObject[] buttonObjectList = null;

        public GameObject invenContentObject = null;

        [Header("UI")]
        public ScrollRect invenScrollRect = null;
        public Text slotText = null;

        protected TableViewGrid tableView = null;


        bool isTableInit = false;

        public override void InitUI()
        {
            UIObjectEvent.Event(UIObjectEvent.eEvent.ITEM_CHECK, UIObjectEvent.eUITarget.LB);
            UIObjectEvent.Event(UIObjectEvent.eEvent.EVENT_HIDE, UIObjectEvent.eUITarget.ALL);
            InitInventoryLayer();
        }
        public void RefreshUI()
        {
            DrawInven();
            DeactivateBtn();
        }

        public override void ClosePopup()
        {
            UIObjectEvent.Event(UIObjectEvent.eEvent.EVENT_SHOW, UIObjectEvent.eUITarget.ALL);
            base.ClosePopup();
        }

        public void OnClickFilter(string filterTypeString)
        {
            INVENSORT sort = INVENSORT.None1;
            switch (filterTypeString)
            {
                case "all": sort = INVENSORT.ALL; DeactivateBtn(0); break;
                case "consume": sort = INVENSORT.CONSUMABLE; DeactivateBtn(1); break;
                case "adv_material": sort = INVENSORT.ADV_MATERIAL; DeactivateBtn(2); break;
                case "product": sort = INVENSORT.PRODUCT; DeactivateBtn(3); break;
                case "etc": sort = INVENSORT.ETC; DeactivateBtn(4); break;
            }

            DrawInven(sort);
        }

        public void OnClickLevelUp()
        {
            // 이미 만렙일 경우 버튼 상태 처리
            InventoryData nextInvenData = InventoryData.Get((User.Instance.Inventory.InvenStep + 1).ToString());
            if (nextInvenData == null)
            {
                SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100000622));
                return;
            }

            if (StageManager.Instance.AdventureProgressData.IsClearedStage(1, 1))  //1-1 스테이지 클리어 퀘스트 이후
            {
                var popup = PopupManager.OpenPopup<InventoryLevelUpPopup>();
                popup.SetUpgradeCallBack(RefreshUI);
            }
            else
            {
                SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100000422));
            }
        }

        void InitInventoryLayer()
        {
            if (tableView == null && invenScrollRect != null)
            {
                tableView = invenScrollRect.gameObject.GetComponent<TableViewGrid>();
            }

            if (tableView != null && !isTableInit)
            {
                tableView?.OnStart();
                isTableInit = true;
            }

            DeactivateBtn(0);
            DrawInven(INVENSORT.ALL);
        }

        void DrawInven(INVENSORT sort = INVENSORT.ALL)
        {
            int invenSlot = User.Instance.Inventory.GetSlot();
            int invenEmptySlot = User.Instance.Inventory.GetEmptySlotCount();
            int invenUsage = invenSlot - invenEmptySlot;

            var itemList = User.Instance.Inventory.GetViewItems(sort);
            // 빈공간을 보여주기 위한 처리
            if (sort == INVENSORT.ALL)
            {
                for (int i = invenUsage; i < invenSlot; i++)
                {
                    itemList.Add(null);
                }
            }

            if (tableView != null)
            {
                tableView.SetDelegate(new TableViewDelegate(itemList, (GameObject node, ITableData item) =>
                {
                    ItemFrame itemFrame = node.GetComponent<ItemFrame>();
                    if (itemFrame == null) { return; }

                    InventoryItem invenItemData = (InventoryItem)item;
                    if (invenItemData == null)
                    {
                        itemFrame.setFrameBlank();
                        return;
                    }
                    itemFrame.SetFrameItemInfo(invenItemData.ItemNo, invenItemData.Amount);
                    itemFrame.SetVisibleNFTNode(invenItemData.BaseData);
                    itemFrame.setInventoryFunc();

                }));

                tableView.ReLoad();
            }

            slotText.text = string.Format("{0} / {1}", invenUsage, invenSlot);
        }

        void DeactivateBtn(int index = 0)
        {
            if (buttonObjectList == null || buttonObjectList.Length <= 0) { return; }

            for (int i = 0; i < buttonObjectList.Length; i++)
            {
                GameObject buttonObject = buttonObjectList[i];
                if (buttonObject == null)
                {
                    continue;
                }

                //buttonObject.GetComponent<Button>().SetInteractable(!(index == i));//폰트 컬러 원본으로 나오게 해달라고함.
                buttonObject.GetComponent<Button>().interactable = !(index == i);
            }
        }
    }
}
