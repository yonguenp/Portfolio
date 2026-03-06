using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NecoShopPurchaseCountPanel : MonoBehaviour
{
    enum BUTTON_STATE
    {
        NONE,
        ADD,
        MINUS,
        MINUS_10,
        ADD_10,
    }

    public NecoShopPanel shopPanel;

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

    uint purchaseTryCount = 0;
    uint maxPurcaseTryCount = 0;
    uint remainTryCount = 0;

    neco_shop curShopData;
    neco_market curMarketData;
    bool isSpecialItem = false;

    uint needMoney;
    uint totalMoney;

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

        NecoCanvas.GetPopupCanvas().OnSystemCountConfirmPopupShow(popupData, purchaseTryCount.ToString(), CONFIRM_POPUP_TYPE.COMMON ,okCallback);
    }

    public void OnClickPopupCloseButton()
    {
        // 프롤로그 체크
        if (neco_data.GetPrologueSeq() == neco_data.PrologueSeq.상점배스구매가이드퀘스트)
        {
            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("LOCALIZE_202"));
            return;
        }

        NecoCanvas.GetPopupCanvas().OnPopupClose(NecoPopupCanvas.POPUP_TYPE.SHOP_BUY_COUNT_POPUP);
    }

    public void OnClickMinus10Button(int repeat)
    {
        // 프롤로그 체크
        if (neco_data.GetPrologueSeq() == neco_data.PrologueSeq.상점배스구매가이드퀘스트)
        {
            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("LOCALIZE_202"));
            return;
        }

        CountButtonChecker(BUTTON_STATE.MINUS_10, repeat);
        ButtonChecker();
    }

    public void OnClickPlus10Button(int repeat)
    {
        // 프롤로그 체크
        if (neco_data.GetPrologueSeq() == neco_data.PrologueSeq.상점배스구매가이드퀘스트)
        {
            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("LOCALIZE_202"));
            return;
        }

        CountButtonChecker(BUTTON_STATE.ADD_10, repeat);
        ButtonChecker();
    }

    public void OnClickPlusButton(int repeat)
    {
        // 프롤로그 체크
        if (neco_data.GetPrologueSeq() == neco_data.PrologueSeq.상점배스구매가이드퀘스트)
        {
            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("LOCALIZE_202"));
            return;
        }

        CountButtonChecker(BUTTON_STATE.ADD, repeat);
        ButtonChecker();
    }

    public void OnClickMinusButton(int repeat)
    {
        // 프롤로그 체크
        if (neco_data.GetPrologueSeq() == neco_data.PrologueSeq.상점배스구매가이드퀘스트)
        {
            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("LOCALIZE_202"));
            return;
        }

        CountButtonChecker(BUTTON_STATE.MINUS, repeat);
        ButtonChecker();
    }
    #endregion

    public void InitShopPurchaseCountPanel(bool isSpecial, neco_shop shopData, neco_market marketData, uint maxBuyCount, SystemConfirmPanel.CountCallback _okCallback = null)
    {
        if (shopData == null && marketData == null) { return; }

        ClearData();

        curShopData = shopData;
        curMarketData = marketData;
        isSpecialItem = isSpecial;
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

        // 프롤로그 체크
        if (neco_data.GetPrologueSeq() == neco_data.PrologueSeq.상점배스구매가이드퀘스트 && curMarketData?.GetNecoMarketItemID() == 64)
        {
            purchaseTryCount = 3;
            CountButtonChecker(BUTTON_STATE.NONE);
            purchaseButton.gameObject.GetComponent<RectTransform>().DOScale(1.1f, 0.5f).SetEase(Ease.InOutQuad).SetLoops(-1, LoopType.Yoyo);
        }
    }

    void SetPurchaseData()
    {
        itemAmountText.gameObject.SetActive(isSpecialItem);

        if (isSpecialItem)
        {
            // 기본 아이템정보 세팅
            items itemData = items.GetItem(curMarketData.GetNecoMarketItemID());

            if (itemData != null)
            {
                itemIcon.sprite = itemData.GetItemIcon();
                itemNameText.text = itemData.GetItemName();
            }

            // 남은 재고 수량 세팅
            if (curMarketData.GetNecoMarketType() == "fish_store")
            {
                remainTryCount = neco_data.Instance.GetMarketData().saleFish[curMarketData.GetNecoMarketID()];
            }
            else if (curMarketData.GetNecoMarketType() == "hardware_store")
            {
                remainTryCount = neco_data.Instance.GetMarketData().saleHardware[curMarketData.GetNecoMarketID()];
            }

            itemAmountText.text = string.Format("{0}/{1}", purchaseTryCount, remainTryCount);

            // 필요 재화 계산
            needMoney = curMarketData.GetNecoMarketPrice();

            priceTypeIcon.sprite = Resources.Load<Sprite>("Sprites/Neco/Ui/Icon_coin");
        }
        else
        {
            // 기본 아이템정보 세팅
            itemIcon.sprite = curShopData.GetNecoShopIcon();
            itemNameText.text = curShopData.GetNecoShopName();

            // 필요 재화 계산
            needMoney = curShopData.GetNecoShopPrice();

            priceTypeIcon.sprite = curShopData.GetNecoShopPriceIcon();
        }
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
                    uint diff = (purchaseTryCount - maxPurcaseTryCount) % 10;
                    if (diff == 0)
                        diff = 10;

                    users user = GameDataManager.Instance.GetUserData();
                    if (user == null)
                        return;

                    string msg = LocalizeData.GetText("재화부족");

                    bool enoughMoney = true;
                    object obj;
                    uint userGold = 0;
                    if (user.data.TryGetValue("gold", out obj))
                    {
                        userGold = (uint)obj;
                    }

                    userGold = userGold - (needMoney * maxPurcaseTryCount);
                    if (userGold < needMoney * diff)
                    {
                        uint needGold = (needMoney * diff) - userGold;
                        msg += "\n" + LocalizeData.GetText(string.Format(LocalizeData.GetText("재화상세"), LocalizeData.GetText("LOCALIZE_229"), needGold));
                        enoughMoney = false;
                    }

                    if(enoughMoney)
                    {
                        msg = LocalizeData.GetText("재고부족");
                    }

                    NecoCanvas.GetPopupCanvas().OnToastPopupShow(msg);
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

        purchaseCountText.text = string.Format("{0}", purchaseTryCount);
        totalMoney = needMoney * purchaseTryCount;
        priceAmountText.text = totalMoney.ToString("n0");

        if (isSpecialItem)
        {
            itemAmountText.text = string.Format("{0}/{1}", purchaseTryCount, remainTryCount);
        }

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

        if (isSpecialItem)
        {
            items itemData = items.GetItem(curMarketData.GetNecoMarketItemID());

            if (itemData != null)
            {
                popupData.contentsSprite = itemData.GetItemIcon();
                popupData.contentsCountText = purchaseTryCount.ToString();
                popupData.contentsNameText = itemData.GetItemName();

                popupData.amountIcon = Resources.Load<Sprite>("Sprites/Neco/Ui/Icon_coin");
                popupData.amountText = totalMoney.ToString("n0");
            }
        }
        else
        {
            popupData.contentsSprite = curShopData.GetNecoShopIcon();
            popupData.contentsCountText = purchaseTryCount.ToString();
            popupData.contentsNameText = curShopData.GetNecoShopName();

            popupData.amountIcon = curShopData.GetNecoShopPriceIcon();
            popupData.amountText = totalMoney.ToString("n0");
        }

        return popupData;
    }

    void ClearData()
    {
        purchaseTryCount = 0;
        purchaseCountText.text = string.Format("{0}", purchaseTryCount);
        totalMoney = needMoney * purchaseTryCount;
        priceAmountText.text = totalMoney.ToString("n0");

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
