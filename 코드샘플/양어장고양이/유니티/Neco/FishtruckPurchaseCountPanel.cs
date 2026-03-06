using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FishtruckPurchaseCountPanel : MonoBehaviour
{
    enum BUTTON_STATE
    {
        NONE,
        ADD,
        MINUS,
        MINUS_10,
        ADD_10,
    }

    [Header("[Common Layer]")]
    public Text arrowGuideText;

    [Header("[Before Item Info Layer]")]
    public Image beforeItemIcon;
    public Text beforeItemNameText;
    public Text beforeItemAmountText;
    public Text beforeTotalCountText;
    uint beforeItemTotalCount;

    [Header("[After Item Info Layer]")]
    public Image afterItemIcon;
    public Text afterItemNameText;
    public Text afterItemAmountText;
    public Text afterTotalCountText;
    uint afterItemTotalCount;

    [Header("[Button Layer]")]
    public Button purchaseButton;
    public Text purchaseCountText;
    public Color originButtonColor;
    public Color dimmedButtonColor;

    [Header("[Layout List]")]
    public RectTransform layoutRect;
    public RectTransform messageLayoutRect;

    uint purchaseTryCount = 0;
    uint maxPurcaseTryCount = 0;

    FishTruckData curFishtruckData;

    SystemConfirmPanel.CountCallback okCallback = null;

    #region UI CONTROL
    public void OnClickPurchaseButton()
    {
        if (purchaseTryCount <= 0)
        {
            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("활어차교환수량부족"));
            return;
        }

        ConfirmPopupData popupData = SetConfirmPopupData();

        NecoCanvas.GetPopupCanvas().OnSystemCountConfirmPopupShow(popupData, purchaseTryCount.ToString("n0"), CONFIRM_POPUP_TYPE.CONFIRM_FISHTRUCK, okCallback);
    }

    public void OnClickPopupCloseButton()
    {
        NecoCanvas.GetPopupCanvas().OnPopupClose(NecoPopupCanvas.POPUP_TYPE.FISH_TRUCK_BUY_COUNT_POPUP);
    }

    public void OnClickMinus10Button(int repeat)
    {
        CountButtonChecker(BUTTON_STATE.MINUS_10, repeat);
        ButtonChecker();
    }

    public void OnClickPlus10Button(int repeat)
    {
        CountButtonChecker(BUTTON_STATE.ADD_10, repeat);
        ButtonChecker();
    }

    public void OnClickPlusButton(int repeat)
    {
        CountButtonChecker(BUTTON_STATE.ADD, repeat);
        ButtonChecker();
    }

    public void OnClickMinusButton(int repeat)
    {
        CountButtonChecker(BUTTON_STATE.MINUS, repeat);
        ButtonChecker();
    }
    #endregion

    public void InitCommonPurchaseCountPanel(FishTruckData fishtruckData, SystemConfirmPanel.CountCallback _okCallback = null)
    {
        if (fishtruckData == null) { return; }

        ClearData();

        curFishtruckData = fishtruckData;

        uint itemAmount = user_items.GetUserItemAmount(curFishtruckData.itemData.GetItemID());
        uint needAmount = curFishtruckData.fishTradeData.GetBunchCount();
        maxPurcaseTryCount = itemAmount >= needAmount ? (itemAmount / needAmount) : 0;

        okCallback = _okCallback;

        SetPurchaseData();

        // 최대 1개이상 구매 가능하면 기본 구매 갯수 1 추가
        if (maxPurcaseTryCount > 0)
        {
            CountButtonChecker(BUTTON_STATE.ADD);
        }

        ButtonChecker();

        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(RefreshLayout());
        }
    }

    void SetPurchaseData()
    {
        //기본 아이템정보 세팅
        arrowGuideText.text = curFishtruckData.fishTradeData.GetBunchCount().ToString("n0");

        beforeItemIcon.sprite = curFishtruckData.itemData.GetItemIcon();
        beforeItemNameText.text = curFishtruckData.itemData.GetItemName();
        beforeItemAmountText.text = user_items.GetUserItemAmount(curFishtruckData.itemData.GetItemID()).ToString("n0");
        beforeTotalCountText.text = maxPurcaseTryCount > 0 ? (maxPurcaseTryCount * curFishtruckData.fishTradeData.GetBunchCount()).ToString("n0") : "0";

        afterItemIcon.sprite = Resources.Load<Sprite>("Sprites/Neco/Ui/Icon_Material_fishticket");
        afterItemNameText.text = items.GetItem(138).GetItemName();
        afterTotalCountText.text = maxPurcaseTryCount.ToString("n0");
    }

    void ButtonChecker()
    {
        if (purchaseButton == null) { return; }

        // 구매 진행 버튼
        purchaseButton.image.color = purchaseTryCount > 0 ? originButtonColor : dimmedButtonColor;
    }

    void CountButtonChecker(BUTTON_STATE buttonState, int repeat = 1)
    {
        switch (buttonState)
        {
            case BUTTON_STATE.ADD:
                purchaseTryCount += (uint)repeat;
                break;
            case BUTTON_STATE.MINUS:
                if (purchaseTryCount == 0) break;   // uint 예외처리
                if (purchaseTryCount < (uint)repeat)
                    purchaseTryCount = 0;
                else
                    purchaseTryCount -= (uint)repeat;
                break;
            case BUTTON_STATE.ADD_10:
                if (purchaseTryCount == maxPurcaseTryCount)
                    return;

                purchaseTryCount += 10 * (uint)repeat;
                if (purchaseTryCount >= maxPurcaseTryCount)
                {
                    //uint diff = (purchaseTryCount - maxPurcaseTryCount) % 10;
                    //if (diff == 0)
                    //    diff = 10;

                    //users user = GameDataManager.Instance.GetUserData();
                    //if (user == null)
                    //    return;

                    //string msg = LocalizeData.GetText("재화부족");

                    //bool enoughMoney = true;
                    //object obj;
                    //uint userGold = 0;
                    //if (user.data.TryGetValue("gold", out obj))
                    //{
                    //    userGold = (uint)obj;
                    //}

                    //userGold = userGold - (needMoney * maxPurcaseTryCount);
                    //if (userGold < needMoney * diff)
                    //{
                    //    uint needGold = (needMoney * diff) - userGold;
                    //    msg += "\n" + LocalizeData.GetText(string.Format(LocalizeData.GetText("재화상세"), LocalizeData.GetText("LOCALIZE_229"), needGold));
                    //    enoughMoney = false;
                    //}

                    //if (enoughMoney)
                    //{
                    //    msg = LocalizeData.GetText("재고부족");
                    //}

                    //NecoCanvas.GetPopupCanvas().OnToastPopupShow(msg);
                }
                break;
            case BUTTON_STATE.MINUS_10:
                purchaseTryCount = purchaseTryCount < 10 * (uint)repeat ? 0 : purchaseTryCount - (10 * (uint)repeat);
                break;
            case BUTTON_STATE.NONE:
            default:
                break;
        }

        if (purchaseTryCount >= maxPurcaseTryCount)
        {
            purchaseTryCount = maxPurcaseTryCount;
        }
        else if (purchaseTryCount <= 0)
        {
            purchaseTryCount = 0;
        }

        purchaseCountText.text = string.Format(LocalizeData.GetText("시도횟수"), purchaseTryCount.ToString("n0"));

        beforeTotalCountText.text = (purchaseTryCount * curFishtruckData.fishTradeData.GetBunchCount()).ToString("n0");
        afterTotalCountText.text = (purchaseTryCount * curFishtruckData.fishTradeData.GetCoin()).ToString("n0");

        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(RefreshLayout());
        }
    }

    ConfirmPopupData SetConfirmPopupData()
    {
        ConfirmPopupData popupData = new ConfirmPopupData();

        popupData.titleText = LocalizeData.GetText("아이템교환");
        popupData.titleMessageText = LocalizeData.GetText("아이템교환가이드");

        //popupData.contentsSprite = curFishtruckData.itemData.GetItemIcon();
        //popupData.contentsCountText = (purchaseTryCount * curFishtruckData.fishTradeData.GetBunchCount()).ToString("n0");
        //popupData.contentsNameText = curFishtruckData.itemData.GetItemName();

        //popupData.amountIcon = Resources.Load<Sprite>("Sprites/Neco/Ui/Icon_Material_fishticket");
        //popupData.amountText = (purchaseTryCount * curFishtruckData.fishTradeData.GetCoin()).ToString("n0");

        popupData.contentsSprite = Resources.Load<Sprite>("Sprites/Neco/Ui/Icon_Material_fishticket");
        popupData.contentsCountText = (purchaseTryCount * curFishtruckData.fishTradeData.GetCoin()).ToString("n0");
        popupData.contentsNameText = items.GetItem(138).GetItemName();

        popupData.amountIcon = curFishtruckData.itemData.GetItemIcon();
        popupData.amountText = (purchaseTryCount * curFishtruckData.fishTradeData.GetBunchCount()).ToString("n0");

        return popupData;
    }

    void ClearData()
    {
        purchaseTryCount = 0;
        purchaseCountText.text = string.Format("{0}", purchaseTryCount);

        purchaseButton.gameObject.GetComponent<RectTransform>().localScale = Vector3.one;
        purchaseButton.gameObject.GetComponent<RectTransform>().DORewind();
        purchaseButton.gameObject.GetComponent<RectTransform>().DOKill();
    }

    IEnumerator RefreshLayout()
    {
        // 원인 불명.. 2프레임에 걸쳐 최소 2회 갱신해야 정상 작동함

        yield return new WaitForEndOfFrame();

        if (layoutRect != null && messageLayoutRect != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(layoutRect);
            LayoutRebuilder.ForceRebuildLayoutImmediate(messageLayoutRect);
        }

        yield return new WaitForEndOfFrame();

        if (layoutRect != null && messageLayoutRect != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(layoutRect);
            LayoutRebuilder.ForceRebuildLayoutImmediate(messageLayoutRect);
        }
    }
}
