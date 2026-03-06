using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RecipeMaterialInfo : MonoBehaviour
{
    public Image iconImage;
    public Text itemCountText;
    public Text itemNameText;
    public GameObject objectMaxIcon;

    [Header("[Item Count Text Color]")]
    public Color originItemCountTextColor;
    public Color NotEnoughItemCountTextColor;

    //recipe curRecipeData;

    KeyValuePair<uint, uint> input;
    items curItemData;
    bool hasItem = false;

    NecoCookPanel parentPanelUI;

    private uint TryMaxCount = 0;
    bool isMoneyData = false;
    public bool IsMoneyData
    {
        get { return isMoneyData; }
        set
        {
            isMoneyData = value;
        }
    }

    bool isCatnipData = false;
    public bool IsCatnipData
    {
        get { return isCatnipData; }
        set
        {
            isCatnipData = value;
        }
    }

    // 2차 제작 재료일 경우 만드는 획득처로 보냄
    public void OnClickMaterialInfo()
    {
        // 튜토리얼 중 예외처리
        if (neco_data.GetPrologueSeq() < neco_data.PrologueSeq.프리플레이) { return; }

        if (curItemData == null) { return; }
        if (hasItem) { return; }    // 재료가 충분하면 off

        // 획득처로 이동전 컨펌 팝업 사용 시 사용
        ConfirmPopupData popupData = new ConfirmPopupData();

        if (curItemData.GetItemType() == "T_MATERIAL")  // 2차제작 재료
        {
            popupData.titleText = LocalizeData.GetText("LOCALIZE_330");
            popupData.titleMessageText = LocalizeData.GetText("move_countertop");
            popupData.messageText_1 = LocalizeData.GetText("move_countertop_guide");

            NecoCanvas.GetPopupCanvas().OnSystemConfirmPopupShow(popupData, CONFIRM_POPUP_TYPE.COMMON, () => {
                NecoCanvas.GetPopupCanvas().OnPopupClose();
                NecoCanvas.GetPopupCanvas().OnCraftListPopupShow(true);
            });
        }
        else if (curItemData.GetItemType() == "S_MATERIAL") // 상점 제작재료
        {
            popupData.titleText = LocalizeData.GetText("LOCALIZE_330");
            popupData.titleMessageText = LocalizeData.GetText("LOCALIZE_183");
            popupData.messageText_1 = LocalizeData.GetText("move_shop_guide");

            NecoCanvas.GetPopupCanvas().OnSystemConfirmPopupShow(popupData, CONFIRM_POPUP_TYPE.COMMON, () => {
                NecoCanvas.GetPopupCanvas().OnPopupClose();

                NecoShopPanel.SHOP_CATEGORY type = NecoShopPanel.SHOP_CATEGORY.COMMON;
                List<game_data> datas = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.NECO_SHOP);
                foreach (neco_shop shop in datas)
                {
                    if (curItemData.GetItemID() == shop.GetNecoShopGoodsID())
                    {
                        type = NecoShopPanel.SHOP_CATEGORY.CAT_LEAF;
                        break;
                    }
                }
                NecoCanvas.GetPopupCanvas().OnShopListPopupShow(type);
            });
        }
        else if (curItemData.GetItemType() == "FOOD")   // 요리
        {
            if (NecoCanvas.GetPopupCanvas().IsPopupOpen(NecoPopupCanvas.POPUP_TYPE.CAT_COOK_LIST_POPUP) == false)
            {
                popupData.titleText = LocalizeData.GetText("LOCALIZE_330");
                popupData.titleMessageText = LocalizeData.GetText("move_workbench");
                popupData.messageText_1 = LocalizeData.GetText("move_workbench_guide");

                NecoCanvas.GetPopupCanvas().OnSystemConfirmPopupShow(popupData, CONFIRM_POPUP_TYPE.COMMON, () => {
                    NecoCanvas.GetPopupCanvas().OnPopupClose();
                    NecoCanvas.GetPopupCanvas().OnPopupShow(NecoPopupCanvas.POPUP_TYPE.CAT_COOK_LIST_POPUP);
                });
            }
        }
        //else if (curItemData.GetItemType() == "M_MATERIAL") // 1차 제작재료
        //{
        //    NecoCanvas.GetPopupCanvas().OnToastPopupShow("blablabla material");
        //}
        //else if (curItemData.GetItemType() == "F_MATERIAL") // 1차 요리재료
        //{
        //    NecoCanvas.GetPopupCanvas().OnToastPopupShow("blablabla food material");
        //}
    }

    public void UpdateTryCount(uint count, uint userMoney, uint needMoney)
    {
        // 갱신 시에 제작가능한 갯수가 0개라도 최소1개는 유지해야함
        count = count < 1 ? 1 : count;

        if (IsMoneyData)
        {
            SetMoneyMaterialData(needMoney, userMoney, count);
        }
        else
        {
            SetMarerialData(input, count);
        }
    }

    public void SetMarerialData(KeyValuePair<uint, uint> _input, uint count = 1)
    {
        input = _input;
        //parentPanelUI = cookUI;

        object obj;

        curItemData = items.GetItem(input.Key);

        float maxRatio = 0.0f;
        string itemName = curItemData.GetItemName();
        uint need = input.Value * count;
        hasItem = false;
        uint cur = 0;
        
        cur = user_items.GetUserItemAmount(input.Key);
        hasItem = cur >= need;

        if (objectMaxIcon != null)
            objectMaxIcon.SetActive(false);

        switch (input.Key)
        {
            case 124://이동식캣하우스 예외처리
                if (neco_spot.GetNecoSpotObjectByItemID(124).GetSpotLevel() < neco_data.OBJECT_MAX_LEVEL)
                {
                    cur = 0;
                    hasItem = false;
                }

                if (objectMaxIcon != null)
                    objectMaxIcon.SetActive(true);
                break;

            case 109://화이트캣하우스 예외처리
                if (neco_spot.GetNecoSpotObjectByItemID(109).GetSpotLevel() < neco_data.OBJECT_MAX_LEVEL)
                {
                    cur = 0;
                    hasItem = false;
                }

                if (objectMaxIcon != null)
                    objectMaxIcon.SetActive(true);
                break;
        }

        itemCountText.text = cur.ToString("n0") + "/" + need.ToString("n0");
        if (itemCountText.text.Length > 10)
        {
            itemCountText.text = cur.ToString("n0") + "\n/" + need.ToString("n0");
        }

        if (hasItem == false)
        {
            itemCountText.color = NotEnoughItemCountTextColor;
            itemCountText.text = cur.ToString("n0") + "/" + input.Value.ToString("n0");
            if (itemCountText.text.Length > 10)
            {
                itemCountText.text = cur.ToString("n0") + "\n/" + input.Value.ToString("n0");
            }
            maxRatio = 0;
        }
        else
        {
            itemCountText.color = originItemCountTextColor;
            maxRatio = Mathf.Min(99, ((float)cur / need));
        }

        iconImage.sprite = curItemData.GetItemIcon();
        itemNameText.text = itemName;



        TryMaxCount = (uint)maxRatio;
    }

    public void SetMoneyMaterialData(uint needGold, uint userGold, uint count = 1)
    {
        isMoneyData = true;

        iconImage.sprite = Resources.Load<Sprite>("Sprites/Neco/Ui/Icon_Material_coin");

        uint totalNeedGold = needGold * count;
        if (totalNeedGold > userGold)
        {
            itemCountText.color = NotEnoughItemCountTextColor;
        }
        else
        {
            itemCountText.color = originItemCountTextColor;
        }

        if (objectMaxIcon != null)
            objectMaxIcon.SetActive(false);

        itemNameText.text = LocalizeData.GetText("LOCALIZE_334");

        itemCountText.text = totalNeedGold.ToString("n0");
    }

    public void SetCatnipMaterialData(uint needCatnip, uint userCatnip, uint count = 1)
    {
        isCatnipData = true;

        iconImage.sprite = Resources.Load<Sprite>("Sprites/Neco/Ui/Icon_Material_catleaf");

        uint totalNeedGold = needCatnip * count;
        if (totalNeedGold > userCatnip)
        {
            itemCountText.color = NotEnoughItemCountTextColor;
        }
        else
        {
            itemCountText.color = originItemCountTextColor;
        }

        if (objectMaxIcon != null)
            objectMaxIcon.SetActive(false);

        itemNameText.text = LocalizeData.GetText("LOCALIZE_348");

        itemCountText.text = totalNeedGold.ToString("n0");
    }

    public uint GetTryMaxCount()
    {
        return TryMaxCount;
    }
}
