using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NecoBannerPopup : MonoBehaviour
{
    public Text Title;    
    public Text BannerDescription;
    public Text purchaseCountText;
    public Text Lv4PackagePurchaseCountText;
    public Text Lv5PackagePurchaseCountText;
    public Text Lv6PackagePurchaseCountText;
    public Text Lv7PackagePurchaseCountText;
    public Text Lv8PackagePurchaseCountText;
    public Text Lv9PackagePurchaseCountText;

    public GameObject[] BannerObject;

    public GameObject TargetEffect;
    public RectTransform EffectContainer;

    public RectTransform layoutRect;

    UnityEngine.Events.UnityAction BannerClickedCallback;
    UnityEngine.Events.UnityAction BannerClosedCallback;

    public enum BANNER_TYPE { 
        FIRST_BANNER,
        MINI_PACKGAGE_TIMESALE,
        LEVEL4_PACKAGE_TIMESALE,
        LEVEL5_PACKAGE_TIMESALE,
        LEVEL6_PACKAGE_TIMESALE,
        LEVEL7_PACKAGE_TIMESALE,
        LEVEL8_PACKAGE_TIMESALE,
        LEVEL9_PACKAGE_TIMESALE,
    };
    BANNER_TYPE curType;

    public void SetBannerInfo(List<uint> bannerID)
    {
        if (bannerID.Count <= 0)
            return;

        uint id = bannerID[0];
        bannerID.Remove(id);
        BANNER_TYPE type;

        string title = LocalizeData.GetText("LOCALIZE_463");
        string msg = LocalizeData.GetText("LOCALIZE_464");
        switch (id)
        {
            case 55:
                type = BANNER_TYPE.MINI_PACKGAGE_TIMESALE;
                break;
            case 34:
                type = BANNER_TYPE.LEVEL4_PACKAGE_TIMESALE;
                break;
            case 36:
                type = BANNER_TYPE.LEVEL5_PACKAGE_TIMESALE;
                break;
            case 37:
                type = BANNER_TYPE.LEVEL6_PACKAGE_TIMESALE;
                break;
            case 42:
                type = BANNER_TYPE.LEVEL7_PACKAGE_TIMESALE;
                break;
            case 43:
                type = BANNER_TYPE.LEVEL8_PACKAGE_TIMESALE;
                break;
            case 45:
                type = BANNER_TYPE.LEVEL9_PACKAGE_TIMESALE;
                break;
            default:
                return;
        }

        NecoCanvas.GetPopupCanvas().ShowBanner(title, msg, type,
            () => {
                string product = neco_shop.GetNecoShopData(id).GetIAPConstants();
                if (string.IsNullOrEmpty(product))
                {
                    return;
                }

                IAPManager.GetInstance().TryPurchase(id, product, (responseArr) =>
                {
                    NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(LocalizeData.GetText("LOCALIZE_361"), LocalizeData.GetText("LOCALIZE_362"), ()=> { BannerClosedCallback?.Invoke(); });
                }, (responseArr) =>
                {
                    NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(LocalizeData.GetText("LOCALIZE_316"), LocalizeData.GetText("LOCALIZE_344"));
                });
            },
            () => {
                SetBannerInfo(bannerID);
            }
        );

        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(RefreshLayout());
        }
    }

    public void SetBannerInfo(string title, string desc, BANNER_TYPE type, UnityEngine.Events.UnityAction banner_cb = null, UnityEngine.Events.UnityAction close_cb = null)
    {
        Title.text = title;
        BannerDescription.text = desc;
        
        foreach(GameObject go in BannerObject)
        {
            go.SetActive(false);
        }

        BannerObject[(int)type].SetActive(true);

        BannerClickedCallback = banner_cb;
        BannerClosedCallback = close_cb;
        curType = type;

        if (curType != BANNER_TYPE.FIRST_BANNER)
        {   
            SetTimer();
            // 구매 횟수 정보 세팅
            SetPurchaseCount();
        }

        foreach (Transform child in EffectContainer)
        {
            DestroyImmediate(child.gameObject);
        }

        ShowEffect();

        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(RefreshLayout());
        }
    }

    void ShowEffect()
    {
        if (EffectContainer.transform.childCount > 2)
            return;

        Vector2 size = (EffectContainer.transform as RectTransform).sizeDelta;
        GameObject effect = Instantiate(TargetEffect);
        effect.transform.SetParent(EffectContainer.transform);
        RectTransform rt = effect.GetComponent<RectTransform>();

        rt.localPosition = new Vector3((size.x * 0.5f) - (Random.value * size.x),10 + (size.y * 0.5f) - (Random.value * size.y), TargetEffect.transform.localPosition.z);
        rt.localScale = Vector3.one;

        effect.SetActive(true);

        Invoke("ShowEffect", 0.1f);
    }
    public void SetPurchaseCount()
    {
        uint productID = 0;
        switch (curType)
        {
            case BANNER_TYPE.MINI_PACKGAGE_TIMESALE:
                productID = 55;
                break;
            case BANNER_TYPE.LEVEL4_PACKAGE_TIMESALE:
                productID = 34;
                break;
            case BANNER_TYPE.LEVEL5_PACKAGE_TIMESALE:
                productID = 36;
                break;
            case BANNER_TYPE.LEVEL6_PACKAGE_TIMESALE:
                productID = 37;
                break;
            case BANNER_TYPE.LEVEL7_PACKAGE_TIMESALE:
                productID = 42;
                break;
            case BANNER_TYPE.LEVEL8_PACKAGE_TIMESALE:
                productID = 43;
                break;
            case BANNER_TYPE.LEVEL9_PACKAGE_TIMESALE:
                productID = 45;
                break;
            default:
                return;
        }

        neco_shop curShopData = neco_shop.GetNecoShopData(productID);
        if (curShopData != null)
        {
            uint purchaseCount = neco_data.Instance.GetPurchaseCount(curShopData.GetNecoShopID());
            uint purchaseLimitCount = curShopData.GetNecoShopPurchaseLimit();
            if (curShopData.GetNecoShopPurchaseLimit() > 0)
            {
                if (curType == BANNER_TYPE.MINI_PACKGAGE_TIMESALE)
                {
                    purchaseCountText.text = string.Format("({0}/{1})", purchaseCount, purchaseLimitCount);
                }
                else if (curType == BANNER_TYPE.LEVEL4_PACKAGE_TIMESALE)
                {
                    Lv4PackagePurchaseCountText.text = string.Format("({0}/{1})", purchaseCount, purchaseLimitCount);
                }
                else if (curType == BANNER_TYPE.LEVEL5_PACKAGE_TIMESALE)
                {
                    Lv5PackagePurchaseCountText.text = string.Format("({0}/{1})", purchaseCount, purchaseLimitCount);
                }
                else if (curType == BANNER_TYPE.LEVEL6_PACKAGE_TIMESALE)
                {
                    Lv6PackagePurchaseCountText.text = string.Format("({0}/{1})", purchaseCount, purchaseLimitCount);
                }
                else if (curType == BANNER_TYPE.LEVEL7_PACKAGE_TIMESALE)
                {
                    Lv7PackagePurchaseCountText.text = string.Format("({0}/{1})", purchaseCount, purchaseLimitCount);
                }
                else if (curType == BANNER_TYPE.LEVEL8_PACKAGE_TIMESALE)
                {
                    Lv8PackagePurchaseCountText.text = string.Format("({0}/{1})", purchaseCount, purchaseLimitCount);
                }
                else if (curType == BANNER_TYPE.LEVEL9_PACKAGE_TIMESALE)
                {
                    Lv9PackagePurchaseCountText.text = string.Format("({0}/{1})", purchaseCount, purchaseLimitCount);
                }
            }
        }
        else
        {
            purchaseCountText.text = "";
            Lv4PackagePurchaseCountText.text = "";
            Lv5PackagePurchaseCountText.text = "";
            Lv6PackagePurchaseCountText.text = "";
            Lv7PackagePurchaseCountText.text = "";
            Lv8PackagePurchaseCountText.text = "";
            Lv9PackagePurchaseCountText.text = "";
        }
    }

    public void SetTimer()
    {
        CancelInvoke("SetTimer");

        uint productID = 0;
        switch(curType)
        {
            case BANNER_TYPE.MINI_PACKGAGE_TIMESALE:
                productID = 55;
                break;
            case BANNER_TYPE.LEVEL4_PACKAGE_TIMESALE:
                productID = 34;
                break;
            case BANNER_TYPE.LEVEL5_PACKAGE_TIMESALE:
                productID = 36;
                break;
            case BANNER_TYPE.LEVEL6_PACKAGE_TIMESALE:
                productID = 37;
                break;
            case BANNER_TYPE.LEVEL7_PACKAGE_TIMESALE:
                productID = 42;
                break;
            case BANNER_TYPE.LEVEL8_PACKAGE_TIMESALE:
                productID = 43;
                break;
            case BANNER_TYPE.LEVEL9_PACKAGE_TIMESALE:
                productID = 45;
                break;
            default:
                return;
        }
        uint overTime = neco_data.Instance.GetTimeSaleProductRemain((uint)productID);
        int remain = (int)overTime - (int)NecoCanvas.GetCurTime();

        if (remain <= 0)
        {
            NecoCanvas.GetPopupCanvas().OnPopupClose(NecoPopupCanvas.POPUP_TYPE.BANNER_POPUP);
            return;
        }

        //string txt = (remain % 60).ToString("00") + "초 남음";
        string txt = string.Format(LocalizeData.GetText("LOCALIZE_211"), (remain % 60).ToString("00"));
        
        if (remain > 60)
        {
            int minute = (remain / 60);
            //txt = (minute % 60).ToString("00") + LocalizeData.GetText("LOCALIZE_351") + txt;
            txt = string.Format(LocalizeData.GetText("LOCALIZE_257"), (minute % 60).ToString("00")) + txt;

            if (minute > 60)
            {
                //txt = (minute / 60).ToString("00") + LocalizeData.GetText("LOCALIZE_350") + txt;
                txt = string.Format(LocalizeData.GetText("LOCALIZE_510"), (minute / 60).ToString("00")) + txt;
            }            
        }

        BannerObject[(int)curType].transform.Find("PackageBanner").Find("DurationLayer").Find("DurationText").GetComponent<Text>().text = txt;

        Invoke("SetTimer", 1.0f);
    }

    public void OnClickClose()
    {
        NecoCanvas.GetPopupCanvas().OnPopupClose(NecoPopupCanvas.POPUP_TYPE.BANNER_POPUP);
        BannerClosedCallback?.Invoke();
    }

    public void OnClickCloseButton()
    {
        NecoCanvas.GetPopupCanvas().OnPopupClose(NecoPopupCanvas.POPUP_TYPE.BANNER_POPUP);
        BannerClosedCallback?.Invoke();
    }

    public void OnBannerSelect()
    {
        if (BannerClickedCallback != null)
        {
            uint productID = 0;
            switch (curType)
            {
                case BANNER_TYPE.MINI_PACKGAGE_TIMESALE:
                    productID = 55;
                    break;
                case BANNER_TYPE.LEVEL4_PACKAGE_TIMESALE:
                    productID = 34;
                    break;
                case BANNER_TYPE.LEVEL5_PACKAGE_TIMESALE:
                    productID = 36;
                    break;
                case BANNER_TYPE.LEVEL6_PACKAGE_TIMESALE:
                    productID = 37;
                    break;
                case BANNER_TYPE.LEVEL7_PACKAGE_TIMESALE:
                    productID = 42;
                    break;
                case BANNER_TYPE.LEVEL8_PACKAGE_TIMESALE:
                    productID = 43;
                    break;
                case BANNER_TYPE.LEVEL9_PACKAGE_TIMESALE:
                    productID = 45;
                    break;
                default:
                    BannerClickedCallback.Invoke();
                    return;
            }

            neco_shop curShopData = neco_shop.GetNecoShopData(productID);
            if (curShopData != null)
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

                NecoCanvas.GetPopupCanvas().OnSystemConfirmPopupShow(popupData, CONFIRM_POPUP_TYPE.COMMON_WITHDRAWAL, ()=> { BannerClickedCallback.Invoke(); });
            }
        }
    }

    IEnumerator RefreshLayout()
    {
        // 원인 불명.. 2프레임에 걸쳐 최소 2회 갱신해야 정상 작동함

        yield return new WaitForEndOfFrame();

        if (layoutRect != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(layoutRect);
        }

        yield return new WaitForEndOfFrame();

        if (layoutRect != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(layoutRect);
        }
    }
}
