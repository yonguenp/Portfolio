using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExchangeSlot : MonoBehaviour
{
    public GameObject selectedObj;
    public GameObject unSelectedObj;
    public UIBundleItem UIBundleItem;

    public BundleInfo card_d;
    public Text soulStoneNum;
    public int idx;
    public int changeSoulAmount;

    ExchangeUI exchangeUI;

    public int itemUseCount;

    public GameObject select_itemList;
    public UIbundleEquip sampleItem;

    public RectTransform item_content;
    public RectTransform bg_content;
    public Text selectAmount_text;

    public void OnDisable()
    {
        UndoEquipBtn();
    }
    public void Init(int idx, ExchangeUI ui)
    {
        exchangeUI = ui;

        this.idx = idx;
        changeSoulAmount = 0;
        selectedObj.SetActive(false);
        unSelectedObj.SetActive(true);
        select_itemList.SetActive(false);
    }
    //장비 선택
    public void EquipmentSelectBtn()
    {
        var popup = PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.EXCHANGE_POPUP) as ExchangePopup;
        popup.ExchangeUIGold.slot_idx = idx;

        List<CombineInventoryPopup.InventoryTab> tab = new List<CombineInventoryPopup.InventoryTab>();
        tab.Add(CombineInventoryPopup.InventoryTab.EQUIPMENT);

        PopupCanvas.Instance.ShowInventoryPopup(true, () =>
        {
            if (popup.exchange_equipList.Count <= 0)
                return;

            unSelectedObj.SetActive(false);
            select_itemList.SetActive(true);

            foreach (Transform tr in sampleItem.transform.parent)
            {
                if (tr == sampleItem.transform)
                    continue;
                Destroy(tr.gameObject);
            }

            sampleItem.gameObject.SetActive(true);
            foreach (var item in popup.exchange_equipList)
            {
                var obj = GameObject.Instantiate(sampleItem, sampleItem.transform.parent);
                obj.Init(item.info);
            }

            sampleItem.gameObject.SetActive(false);

            int totalGold = 0;
            foreach (var item in popup.exchange_equipList)
            {
                switch (item.info.equipData.grade)
                {
                    case 1:
                        totalGold += EquipConfig.GetConfigDic()["base_grade_gold_c"] + EquipConfig.GetConfigDic()["base_level_gold_c"] * item.info.lv;
                        break;
                    case 2:
                        totalGold += EquipConfig.GetConfigDic()["base_grade_gold_b"] + EquipConfig.GetConfigDic()["base_level_gold_b"] * item.info.lv;
                        break;
                    case 3:
                        totalGold += EquipConfig.GetConfigDic()["base_grade_gold_a"] + EquipConfig.GetConfigDic()["base_level_gold_a"] * item.info.lv;
                        break;
                    case 4:
                        totalGold += EquipConfig.GetConfigDic()["base_grade_gold_s"] + EquipConfig.GetConfigDic()["base_level_gold_s"] * item.info.lv;
                        break;
                }
            }

            popup.ExchangeUIGold.RefreshGold(totalGold.ToString());
            selectAmount_text.text = StringManager.GetString("exchange_selected_equip", popup.exchange_equipList.Count);
            (PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.COMBINEINVENTORY_POPUP) as CombineInventoryPopup).equip.SetCurSelectItem(null);
        }, tab); ;

    }
    //장비 취소버튼
    public void UndoEquipBtn()
    {
        selectedObj.SetActive(false);
        select_itemList.SetActive(false);
        unSelectedObj.SetActive(true);

        var popup = PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.EXCHANGE_POPUP) as ExchangePopup;
        foreach (Transform tr in sampleItem.transform.parent)
        {
            if (tr == sampleItem.transform)
                continue;
            Destroy(tr.gameObject);
        }
        popup.exchange_equipList.Clear();
        popup.ExchangeUIGold.RefreshGold("0");

    }
    //카드 선택 버튼
    public void CardSelectBtn()
    {
        var popup = PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.EXCHANGE_POPUP) as ExchangePopup;
        popup.ExchangeUI.slot_idx = idx;

        List<CombineInventoryPopup.InventoryTab> tab = new List<CombineInventoryPopup.InventoryTab>();
        tab.Add(CombineInventoryPopup.InventoryTab.PIECE_LIST);

        PopupCanvas.Instance.ShowInventoryPopup((bundle) =>
        {
            var bundleInfos = bundle.GetBundleInfos();
            if (bundleInfos == null || bundleInfos.Count == 0)
                return;
            ItemGameData item = ItemGameData.GetItemData(bundleInfos[0].goods_param);

            if (item.type != ItemGameData.ITEM_TYPE.CHAR_PIECE)
                return;

            var popup = PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.EXCHANGE_POPUP) as ExchangePopup;
            popup.ExchangeUI.SetItemId(bundleInfos[0]);
            PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.COMBINEINVENTORY_POPUP).Close();
        }, tab);
    }

    //카드 정보 업데이트
    public void setCardInfo(BundleInfo info)
    {
        card_d = info;
        if (card_d == null)
        {
            selectedObj.SetActive(false);
            unSelectedObj.SetActive(true);
            return;
        }

        itemUseCount = 0;
        soulStoneNum.text = itemUseCount.ToString();
        this.UIBundleItem.SetItem(ItemGameData.GetItemData(card_d.goods_param), card_d.goods_amount);

        var popup = PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.EXCHANGE_POPUP) as ExchangePopup;
        popup.ExchangeUI.slot_idx = -1;

        selectedObj.SetActive(true);
        unSelectedObj.SetActive(false);

        MaxBtn();
    }

    //카드 취소 버튼
    public void CancelBtn()
    {
        card_d = null;

        selectedObj.SetActive(false);
        unSelectedObj.SetActive(true);

        var popup = PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.EXCHANGE_POPUP) as ExchangePopup;
        popup.ExchangeUI.RefreshSoulstone();
    }

    //슬라이더 업데이트
    public void SoulStoneUpdate()
    {
        if (card_d == null)
            return;

        var soulstoneAmount = 0;
        if (ItemGameData.GetItemGrade(card_d.goods_param) == 1)
            soulstoneAmount = GameConfig.Instance.SOUL_STONE_TRADE_C;
        else if (ItemGameData.GetItemGrade(card_d.goods_param) == 2)
            soulstoneAmount = GameConfig.Instance.SOUL_STONE_TRADE_B;
        else if (ItemGameData.GetItemGrade(card_d.goods_param) == 3)
            soulstoneAmount = GameConfig.Instance.SOUL_STONE_TRADE_A;
        else if (ItemGameData.GetItemGrade(card_d.goods_param) == 4)
            soulstoneAmount = GameConfig.Instance.SOUL_STONE_TRADE_S;

        var totalSoulStoneCnt = soulstoneAmount * itemUseCount;
        changeSoulAmount = totalSoulStoneCnt;

        var popup = PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.EXCHANGE_POPUP) as ExchangePopup;
        popup.ExchangeUI.RefreshSoulstone();

    }

    public void OnMinusBtn(int repeat)
    {
        itemUseCount -= repeat;
        if (itemUseCount < 0)
        {
            itemUseCount = 0;
        }

        soulStoneNum.text = itemUseCount.ToString();
        SoulStoneUpdate();
    }
    public void OnPlusBtn(int repeat)
    {
        itemUseCount += repeat;
        if (itemUseCount > Managers.UserData.GetMyItemCount(card_d.goods_param))
        {
            itemUseCount = Managers.UserData.GetMyItemCount(card_d.goods_param);
        }

        soulStoneNum.text = itemUseCount.ToString();
        SoulStoneUpdate();
    }

    public void MaxBtn()
    {
        itemUseCount = Managers.UserData.GetMyItemCount(card_d.goods_param);

        soulStoneNum.text = itemUseCount.ToString();
        SoulStoneUpdate();
    }
    public void MinBtn()
    {
        itemUseCount = 0;

        soulStoneNum.text = itemUseCount.ToString();
        SoulStoneUpdate();

    }


    public void UIRectTransformSync()
    {
        bg_content.anchoredPosition = new Vector2(item_content.anchoredPosition.x, item_content.anchoredPosition.y);
    }
}

