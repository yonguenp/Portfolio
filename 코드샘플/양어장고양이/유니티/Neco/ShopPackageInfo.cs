using EasyMobile;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopPackageInfo : MonoBehaviour
{
    [Header("[Package Object]")]
    public GameObject dimmedLayer;
    public Text packageTitle;
    //public Text packageSubTitle;
    //public Text buyInfoText;
    //public Text buyCountText;
    public Text priceAmountText;
    public Text DurationText;

    [Header("[Layout List]")]
    public RectTransform layoutRect;

    neco_shop curShopData;
    List<neco_package> curPackageData;

    NecoShopPanel rootParentPanel;
    IAPObjectHelpPopup shortcutPanel;
    public void OnClickPackageItem()
    {
        if(curShopData.GetNecoShopPurchaseLimit() > 0 && neco_data.Instance.GetPurchaseCount(curShopData.GetNecoShopID()) >= curShopData.GetNecoShopPurchaseLimit())
        {
            NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(LocalizeData.GetText("LOCALIZE_374"), LocalizeData.GetText("LOCALIZE_375"));
            return;
        }

        ConfirmPopupData popupData = SetConfirmPopupData();

        NecoCanvas.GetPopupCanvas().OnSystemConfirmPopupShow(popupData, CONFIRM_POPUP_TYPE.COMMON_WITHDRAWAL, TryPurchase);

        Firebase.Analytics.FirebaseAnalytics.LogEvent("iap_selected");
    }

    public void SetPackageInfoData(neco_shop shopData, NecoShopPanel parentPanel, IAPObjectHelpPopup shortcut = null)
    {
        if (shopData == null) { return; }

        curShopData = shopData;
        rootParentPanel = parentPanel;
        shortcutPanel = shortcut;

        curPackageData = neco_package.GetNecoPackageListByID(curShopData.GetNecoShopID());

        if (curPackageData == null || curPackageData.Count <= 0) { return; }

        ClearData();

        // 패키지 데이터 세팅
        packageTitle.text = curShopData.GetNecoShopName();

        //packageSubTitle.text = curShopData.GetNecoShopProductDesc();

        // 구매 관련 정보 데이터 세팅
        //buyInfoText.text = curShopData.GetPurchaseTypeString();
        uint limitPurchaseCount = curShopData.GetNecoShopPurchaseLimit();
        uint currentPurchaseCount = neco_data.Instance.GetPurchaseCount(curShopData.GetNecoShopID());

        //if (limitPurchaseCount > 0)
        //{            
        //    buyCountText.text = string.Format("({0}/{1})", currentPurchaseCount, limitPurchaseCount);
        //}
        //else
        //{
        //    buyCountText.text = "";
        //}

        priceAmountText.text = string.Format("\\ {0}", curShopData.GetNecoShopPrice().ToString("n0"));

        dimmedLayer.SetActive(limitPurchaseCount > 0 && currentPurchaseCount >= limitPurchaseCount);

        if(DurationText != null)
        {
            SetTimer();
        }

        if (layoutRect != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(layoutRect);
        }
    }

    public void SetTimer()
    {
        CancelInvoke("SetTimer");

        int remain = 0;
        uint overTime = 0;
        if (curShopData.GetNecoShopProductType() == "event")
        {
            remain = (int)neco_data.Instance.GetEventSaleProductRemain(curShopData.GetNecoShopID());
        }
        else
        {
            overTime = neco_data.Instance.GetTimeSaleProductRemain(curShopData.GetNecoShopID());
            remain = (int)overTime - (int)NecoCanvas.GetCurTime();
        }

        if (remain <= 0)
        {
            Destroy(gameObject);
            return;
        }

        string txt = "";
        TimeSpan timeSpan = TimeSpan.FromSeconds(remain);

        if (timeSpan.Days > 0)
        {
            if (timeSpan.Days <= 0 && timeSpan.Hours <= 0 && timeSpan.Minutes <= 0)
            {
                txt = LocalizeData.GetText("1분미만");
            }
            else
            {
                txt = string.Format(LocalizeData.GetText("시간_일시분"), timeSpan.Days.ToString(), timeSpan.Hours.ToString(), timeSpan.Minutes.ToString());
            }
        }
        else
        {
            txt = string.Format(LocalizeData.GetText("시간_시분초"), timeSpan.Hours.ToString("00"), timeSpan.Minutes.ToString("00"), timeSpan.Seconds.ToString("00"));
        }

        DurationText.text = txt;

        Invoke("SetTimer", 1.0f);
    }

    void TryPurchase()
    {
        string product = curShopData.GetIAPConstants();
        if (string.IsNullOrEmpty(product))
        {
            FailPurchase(null);
            return;
        }

        IAPManager.GetInstance().TryPurchase(curShopData.GetNecoShopID(), product, SuccessPurchase, FailPurchase);
    }

    public void SuccessPurchase(JArray responseArr)
    {
        NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(LocalizeData.GetText("LOCALIZE_361"), LocalizeData.GetText("LOCALIZE_362"));

        if (rootParentPanel)
            rootParentPanel.RefreshLayer();
        if (shortcutPanel)
            shortcutPanel.RefreshLayer();
    }

    public void FailPurchase(JArray responseArr)
    {
        NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(LocalizeData.GetText("LOCALIZE_364"), LocalizeData.GetText("LOCALIZE_365"));

        if (rootParentPanel)
            rootParentPanel.RefreshLayer();
        if (shortcutPanel)
            shortcutPanel.RefreshLayer();
    }

    public void RefreshData()
    {
        if (rootParentPanel)
            rootParentPanel.RefreshLayer();
        if (shortcutPanel)
            shortcutPanel.RefreshLayer();
    }

    ConfirmPopupData SetConfirmPopupData()
    {
        uint limitPurchaseCount = curShopData.GetNecoShopPurchaseLimit();
        uint currentPurchaseCount = neco_data.Instance.GetPurchaseCount(curShopData.GetNecoShopID());
        ConfirmPopupData popupData = new ConfirmPopupData();

        popupData.titleText = LocalizeData.GetText("LOCALIZE_376");
        popupData.titleMessageText = LocalizeData.GetText("LOCALIZE_377");

        popupData.messageText_1 = curShopData.GetNecoShopName();
        popupData.messageText_2 = curShopData.GetNecoShopDetail();
        popupData.messageText_3 = curShopData.GetNecoShopProductDesc();

        string limitedTypeString = "";
        limitedTypeString = curShopData.GetPurchaseTypeString();
        if (limitPurchaseCount > 0)
            limitedTypeString += string.Format("({0}/{1})", currentPurchaseCount, limitPurchaseCount);            

        popupData.messageText_4 = limitedTypeString;
        popupData.amountText = string.Format("\\ {0}", curShopData.GetNecoShopPrice().ToString("n0"));

        return popupData;
    }

    void ClearData()
    {
        dimmedLayer.SetActive(false);

        //packageSubTitle.text = "";
    }
}
