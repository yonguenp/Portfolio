using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CombineInventoryPopup : Popup
{
    public enum InventoryTab
    {
        ITEM_LIST,
        PIECE_LIST,
        EQUIPMENT,
        EMOTICON,
    };
    public InventoryPopup inventory;
    public EmotionPopup emotion;
    public InventoryEquip equip;

    [SerializeField] List<Toggle> tabList = new List<Toggle>();

    public delegate void SuccessCallback();

    public int isOpen = 0;
    public override void Open(CloseCallback cb = null)
    {
        base.Open(cb);
    }
    public override void Close()
    {
        if (tabList[(int)InventoryTab.EQUIPMENT].isOn && equip.IsSubMenuOpening())
        {
            equip.CloseSubPopup();
            return;
        }
        if (PopupCanvas.Instance.IsOpeningPopup(PopupCanvas.POPUP_TYPE.EXCHANGE_POPUP))
        {

            var popup = PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.EXCHANGE_POPUP) as ExchangePopup;
            if (!equip.isSell) popup.exchange_equipList.Clear();
        }
        base.Close();

        inventory.InventoryPanelClose();
    }

    public override void RefreshUI()
    {
        if (isOpen == 0)
            SetTab(null, null);
        base.RefreshUI();
    }

    public void SetTab(InventoryPopup.SelectedCallback cb, List<InventoryTab> tabs)
    {
        if (tabs == null || tabs.Count == 0)
        {
            tabs = new List<InventoryTab>();
            tabs.Add(InventoryTab.ITEM_LIST);
            tabs.Add(InventoryTab.PIECE_LIST);
            tabs.Add(InventoryTab.EQUIPMENT);
            tabs.Add(InventoryTab.EMOTICON);
        }

        for (int i = 0; i < tabList.Count; i++)
        {
            tabList[i].gameObject.SetActive(tabs.Contains((InventoryTab)i));
        }

        InventoryTab curTab = tabs[0];
        if (tabList[(int)curTab].isOn == true)
            tabList[(int)curTab].onValueChanged.Invoke(true);

        tabList[(int)curTab].isOn = true;
        switch (curTab)
        {
            case InventoryTab.ITEM_LIST:
            case InventoryTab.PIECE_LIST:
                inventory.RefreshUI();
                break;
            case InventoryTab.EQUIPMENT:
                equip.RefreshUI();
                break;
            case InventoryTab.EMOTICON:
                emotion.RefreshUI();
                break;
        }
        inventory.SetCallback(cb);
    }

    public static Dictionary<ItemGameData, int> GetItemList(InventoryTabBtn.TabType type)
    {
        Dictionary<ItemGameData, int> items = new Dictionary<ItemGameData, int>();
        switch (type)
        {
            case InventoryTabBtn.TabType.Item:
                foreach (var item in Managers.UserData.GetAllMyItemData())
                {
                    switch (item.Key.type)
                    {
                        case ItemGameData.ITEM_TYPE.EQUIP_ITEM:
                        case ItemGameData.ITEM_TYPE.EMOTICON:
                        case ItemGameData.ITEM_TYPE.BUFF_ITEM:
                        case ItemGameData.ITEM_TYPE.CHAR_PIECE:
                            continue;
                        default:
                            break;
                    }

                    items[item.Key] = item.Value;
                }
                break;
            case InventoryTabBtn.TabType.Piece:
                foreach (var item in Managers.UserData.GetAllMyItemData())
                {
                    if (item.Key.type == ItemGameData.ITEM_TYPE.CHAR_PIECE)
                    {
                        items[item.Key] = item.Value;
                    }
                }
                break;
        }

        return items;
    }
}
