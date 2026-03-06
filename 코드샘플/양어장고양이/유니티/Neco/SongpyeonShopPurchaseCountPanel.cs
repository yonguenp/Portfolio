using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SongpyeonShopPurchaseCountPanel : MonoBehaviour
{
    enum BUTTON_STATE
    {
        NONE,
        ADD,
        MINUS,
        MINUS_10,
        ADD_10,
    }

    [Header("[Item Info Layer]")]
    public Image itemIcon;
    public Text itemNameText;
    public Text itemAmountText;

    [Header("[Price Info Layer]")]
    public Image priceTypeIcon;
    public Text priceAmountText;

    [Header("[Button Layer]")]
    public Button purchaseButton;
    public Text purchaseCountText;
    public Color originButtonColor;
    public Color dimmedButtonColor;

    [Header("[Layout List]")]
    public RectTransform layoutRect;
    public RectTransform messageLayoutRect;

    int purchaseTryCount = 0;
    int maxPurcaseTryCount = 0;
    int remainTryCount = 0;

    neco_event_thanks_shop curEventShopData;
    chuseok_event.chuseok_shop_data curShopData;

    uint needSongpyeon;
    uint totalSongpyeon;

    SystemConfirmPanel.CountCallback okCallback = null;

    #region UI CONTROL
    public void OnClickPurchaseButton()
    {
        if (purchaseTryCount <= 0)
        {
            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("LOCALIZE_297"));
            return;
        }

        ConfirmPopupData popupData = SetConfirmPopupData();

        NecoCanvas.GetPopupCanvas().OnSystemCountConfirmPopupShow(popupData, purchaseTryCount.ToString(), CONFIRM_POPUP_TYPE.COMMON, okCallback);
    }

    public void OnClickPopupCloseButton()
    {
        NecoCanvas.GetPopupCanvas().OnPopupClose(NecoPopupCanvas.POPUP_TYPE.SHOP_BUY_COUNT_POPUP);
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

    public void InitShopPurchaseCountPanel(neco_event_thanks_shop eventShopData, int maxBuyCount, SystemConfirmPanel.CountCallback _okCallback = null)
    {
        chuseok_event eventData = null;
        foreach (neco_event evt in neco_data.Instance.GetEvents())
        {
            if ((neco_event.EVENT_TYPE)evt.GetEventID() == neco_event.EVENT_TYPE.CHUSEOK)
                eventData = (chuseok_event)evt;
        }

        if (eventData != null)
        {
            curShopData = eventData.GetShopData();
        }

        if (curShopData == null) { return; }
        if (eventShopData == null) { return; }

        ClearData();

        curEventShopData = eventShopData;

        maxPurcaseTryCount = maxBuyCount;
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
        switch (curEventShopData.GetNecoEventShopItemType())
        {
            case "gold":
                itemIcon.sprite = Resources.Load<Sprite>("Sprites/Neco/Ui/Icon_Material_coin");
                itemNameText.text = LocalizeData.GetText("LOCALIZE_229");
                break;
            case "item":
                items itemData = items.GetItem(curEventShopData.GetNecoEventShopItemID());

                if (itemData != null)
                {
                    itemIcon.sprite = itemData.GetItemIcon();
                    itemNameText.text = itemData.GetItemName();
                }
                break;
        }

        remainTryCount = curShopData.saleDic[curEventShopData.GetNecoEventShopID()];

        itemAmountText.text = string.Format("{0}/{1}", purchaseTryCount, remainTryCount);

        // UI 제어
        itemAmountText.gameObject.SetActive(remainTryCount > 0);    // 무제한 구매가능한 상품은 재고 텍스트 표시 off

        // 필요 재화 계산
        needSongpyeon = curEventShopData.GetNecoEventShopPrice();

        priceTypeIcon.sprite = Resources.Load<Sprite>("Sprites/Neco/Ui/Icon_songpyeon");
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
                purchaseTryCount += repeat;
                break;
            case BUTTON_STATE.MINUS:
                if (purchaseTryCount == 0) break;   // uint 예외처리
                if (purchaseTryCount < repeat)
                    purchaseTryCount = 0;
                else
                    purchaseTryCount -= repeat;
                break;
            case BUTTON_STATE.ADD_10:
                //if (purchaseTryCount == maxPurcaseTryCount)
                //    return;

                purchaseTryCount += 10 * repeat;
                //if (purchaseTryCount >= maxPurcaseTryCount)
                //{
                //    uint diff = (purchaseTryCount - maxPurcaseTryCount) % 10;
                //    if (diff == 0)
                //        diff = 10;

                //    users user = GameDataManager.Instance.GetUserData();
                //    if (user == null)
                //        return;

                //    string msg = LocalizeData.GetText("재화부족");

                //    bool enoughMoney = true;
                //    object obj;
                //    uint userGold = 0;
                //    if (user.data.TryGetValue("gold", out obj))
                //    {
                //        userGold = (uint)obj;
                //    }

                //    userGold = userGold - (needMoney * maxPurcaseTryCount);
                //    if (userGold < needMoney * diff)
                //    {
                //        uint needGold = (needMoney * diff) - userGold;
                //        msg += "\n" + LocalizeData.GetText(string.Format(LocalizeData.GetText("재화상세"), LocalizeData.GetText("LOCALIZE_229"), needGold));
                //        enoughMoney = false;
                //    }

                //    if (enoughMoney)
                //    {
                //        msg = LocalizeData.GetText("재고부족");
                //    }

                //    NecoCanvas.GetPopupCanvas().OnToastPopupShow(msg);
                //}
                break;
            case BUTTON_STATE.MINUS_10:
                purchaseTryCount = purchaseTryCount < 10 * repeat ? 0 : purchaseTryCount - (10 * repeat);
                break;
            case BUTTON_STATE.NONE:
            default:
                break;
        }

        if (purchaseTryCount >= maxPurcaseTryCount)
        {
            purchaseTryCount = maxPurcaseTryCount;

            if (remainTryCount < 0 && purchaseTryCount >= SongpyeonShopItemInfo.MAX_PURCHASE_LIMIT)
            {
                string guideText = string.Format(LocalizeData.GetText("무제한품목구매제한"), SongpyeonShopItemInfo.MAX_PURCHASE_LIMIT);
                NecoCanvas.GetPopupCanvas().OnToastPopupShow(guideText);
            }
            //else
            //{
            //    NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("LOCALIZE_374"));
            //}
        }
        else if (purchaseTryCount <= 0)
        {
            purchaseTryCount = 0;
        }

        purchaseCountText.text = string.Format("{0}", purchaseTryCount);
        totalSongpyeon = needSongpyeon * (uint)purchaseTryCount;
        priceAmountText.text = totalSongpyeon.ToString("n0");

        itemAmountText.text = string.Format("{0}/{1}", purchaseTryCount, remainTryCount);

        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(RefreshLayout());
        }
    }

    ConfirmPopupData SetConfirmPopupData()
    {
        ConfirmPopupData popupData = new ConfirmPopupData();

        popupData.titleText = LocalizeData.GetText("LOCALIZE_298");
        popupData.titleMessageText = LocalizeData.GetText("LOCALIZE_299");

        switch (curEventShopData.GetNecoEventShopItemType())
        {
            case "gold":
                popupData.contentsSprite = Resources.Load<Sprite>("Sprites/Neco/Ui/Icon_Material_coin");
                popupData.contentsNameText = LocalizeData.GetText("LOCALIZE_229");
                popupData.contentsCountText = (purchaseTryCount * curEventShopData.GetNecoEventShopCount()).ToString("n0");
                break;
            case "item":
                items itemData = items.GetItem(curEventShopData.GetNecoEventShopItemID());

                if (itemData != null)
                {
                    popupData.contentsSprite = itemData.GetItemIcon();
                    popupData.contentsCountText = (purchaseTryCount * curEventShopData.GetNecoEventShopCount()).ToString("n0");
                    popupData.contentsNameText = itemData.GetItemName();
                }
                break;
        }

        popupData.amountIcon = Resources.Load<Sprite>("Sprites/Neco/Ui/Icon_songpyeon");
        popupData.amountText = totalSongpyeon.ToString("n0");

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

    private void OnDisable()
    {
        gameObject.SetActive(false);
    }
}
