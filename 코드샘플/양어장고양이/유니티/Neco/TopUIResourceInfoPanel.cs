using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

public class TopUIResourceInfoPanel : MonoBehaviour
{
    [Header("[MoneyInfo Layer]")]
    public GameObject goldLayerObject;
    public Text goldAmountCount;

    [Header("[CatLeafInfo Layer]")]
    public GameObject catLeafLayerObject;
    public Text catLeafAmountCount;

    [Header("[CatStickInfo Layer]")]
    public GameObject catStickLayerObject;
    public Text catStickAmountCount;

    [Header("[PointInfo Layer]")]
    public GameObject pointLayerObject;
    public Text pointAmountCount;

    [Header("[PassTicketInfo Layer]")]
    public GameObject passTicketLayerObject;
    public Text passTicketAmountCount;

    uint userGold = 0;
    uint userCatLeaf = 0;
    uint userChewrr = 0;
    uint userPoint = 0;
    uint userPassTicket = 0;

    public enum UI_MODE { 
        NORMAL,
        CATSTICK,
        PASS_TICKET,
        CAT_PACKAGE_SHOP,
        CAT_MARKET_SHOP,
        CAT_LEAF_SHOP,
    };
    public void OnEnable()
    {
        Init();
    }

    public void Init()
    {
        RefreshGoldData(false);
        RefreshCatLeafData(false);
        RefreshCatJellyData(false);
        RefreshCatPointData(false);
        RefreshPassTicketData(false);
    }

    public void OnClickGoldLayer()
    {
        if (neco_data.GetPrologueSeq() >= neco_data.PrologueSeq.프리플레이)
        {
            if (!NecoCanvas.GetPopupCanvas().IsPopupOpen(NecoPopupCanvas.POPUP_TYPE.SHOP_LIST_POPUP))
            {
                NecoCanvas.GetPopupCanvas().OnPopupClose();
                NecoCanvas.GetPopupCanvas().OnShopListPopupShow(NecoShopPanel.SHOP_CATEGORY.PACKAGE);
            }
        }
    }

    public void OnClickCatLeafLayer()
    {
        if (neco_data.GetPrologueSeq() >= neco_data.PrologueSeq.프리플레이)
        {
            if (!NecoCanvas.GetPopupCanvas().IsPopupOpen(NecoPopupCanvas.POPUP_TYPE.SHOP_LIST_POPUP))
            {
                NecoCanvas.GetPopupCanvas().OnPopupClose();
                NecoCanvas.GetPopupCanvas().OnShopListPopupShow(NecoShopPanel.SHOP_CATEGORY.CAT_LEAF);
            }
        }
    }

    public void OnClickCatStickLayer()
    {
        if (neco_data.GetPrologueSeq() >= neco_data.PrologueSeq.프리플레이)
        {
            if (!NecoCanvas.GetPopupCanvas().IsPopupOpen(NecoPopupCanvas.POPUP_TYPE.SHOP_LIST_POPUP))
            {
                NecoCanvas.GetPopupCanvas().OnPopupClose();
                NecoCanvas.GetPopupCanvas().OnShopListPopupShow(NecoShopPanel.SHOP_CATEGORY.CAT_LEAF);
            }
        }
    }

    public void OnClickPointLayer()
    {
        if (neco_data.GetPrologueSeq() >= neco_data.PrologueSeq.프리플레이)
        {
            if (!NecoCanvas.GetPopupCanvas().IsPopupOpen(NecoPopupCanvas.POPUP_TYPE.SHOP_LIST_POPUP))
            {
                NecoCanvas.GetPopupCanvas().OnPopupClose();
                NecoCanvas.GetPopupCanvas().OnShopListPopupShow(NecoShopPanel.SHOP_CATEGORY.POINT);
            }
        }
    }

    public void OnClickPassTicketLayer()
    {
        if (neco_data.GetPrologueSeq() >= neco_data.PrologueSeq.프리플레이)
        {
            if (!NecoCanvas.GetPopupCanvas().IsPopupOpen(NecoPopupCanvas.POPUP_TYPE.SHOP_LIST_POPUP))
            {
                NecoCanvas.GetPopupCanvas().OnPopupClose();
                NecoCanvas.GetPopupCanvas().OnShopListPopupShow(NecoShopPanel.SHOP_CATEGORY.CAT_LEAF);
            }
        }
    }

    public uint GetUserResource(TOP_UI_RESOURCE_TYPE resourceType)
    {
        switch (resourceType)
        {
            case TOP_UI_RESOURCE_TYPE.GOLD:
                return userGold;
            case TOP_UI_RESOURCE_TYPE.CAT_LEAF:
                return userCatLeaf;
            case TOP_UI_RESOURCE_TYPE.CHEWRR:
                return userChewrr;
            case TOP_UI_RESOURCE_TYPE.POINT:
                return userPoint;
            case TOP_UI_RESOURCE_TYPE.PASS_TICKET:
                return userPassTicket;
            default:
                return 0;
        }
    }

    public void RefreshResourceData()
    {
        RefreshGoldData();
        RefreshCatLeafData();
        RefreshCatJellyData();
        RefreshCatPointData();
        RefreshPassTicketData();
    }

    void RefreshGoldData(bool action = true)
    {
        users user = GameDataManager.Instance.GetUserData();
        if (user == null)
            return;

        object obj;
        uint money = 0;
        if (user.data.TryGetValue("gold", out obj))
        {
            money = (uint)obj;
        }

        userGold = money;
        goldAmountCount.DOKill();
        goldLayerObject.transform.DOKill();
        goldLayerObject.transform.localScale = Vector3.one;
        if (action)
        {
            int curValue = int.Parse(goldAmountCount.text, NumberStyles.AllowThousands);
            if (curValue != (int)userGold)
            {
                goldAmountCount.DOTextInt(curValue, (int)userGold, 0.5f, it => it.ToString("n0")).OnComplete(() => { RefreshGoldData(false); });
                if (Mathf.Abs(curValue - (int)userGold) > 1)
                    goldLayerObject.transform.DOScale(Vector3.one * 1.1f, 0.1f).SetEase(Ease.OutElastic).SetLoops(5, LoopType.Yoyo);
            }
        }
        else
        {
            goldAmountCount.text = userGold.ToString("n0");
        }
    }

    void RefreshCatLeafData(bool action = true)
    {
        users user = GameDataManager.Instance.GetUserData();
        if (user == null)
            return;

        object obj;
        uint catLeaf = 0;
        if (user.data.TryGetValue("catnip", out obj))
        {
            catLeaf = (uint)obj;
        }

        userCatLeaf = catLeaf;
        catLeafLayerObject.transform.DOKill();
        catLeafLayerObject.transform.localScale = Vector3.one;
        if (action)
        {
            int curValue = int.Parse(catLeafAmountCount.text, NumberStyles.AllowThousands);
            if (curValue != (int)userCatLeaf)
            {
                catLeafAmountCount.DOTextInt(int.Parse(catLeafAmountCount.text, NumberStyles.AllowThousands), (int)userCatLeaf, 0.5f, it => it.ToString("n0"));
                catLeafLayerObject.transform.DOScale(Vector3.one * 1.1f, 0.1f).SetEase(Ease.OutElastic).SetLoops(5, LoopType.Yoyo);
            }
        }
        else
            catLeafAmountCount.text = userCatLeaf.ToString("n0");
    }

    void RefreshCatJellyData(bool action = true)
    {
        uint catJelly = user_items.GetUserItemAmount(136);

        userChewrr = catJelly;
        catStickAmountCount.text = catJelly.ToString("n0");
    }

    void RefreshCatPointData(bool action = true)
    {
        users user = GameDataManager.Instance.GetUserData();
        if (user == null)
            return;

        object obj;
        uint point = 0;
        if (user.data.TryGetValue("point", out obj))
        {
            point = (uint)obj;
        }

        userPoint = point;
        pointLayerObject.transform.DOKill();
        pointLayerObject.transform.localScale = Vector3.one;
        if (action)
        {
            int curValue = int.Parse(pointAmountCount.text, NumberStyles.AllowThousands);
            if (curValue != (int)userPoint)
            {
                pointAmountCount.DOTextInt(curValue, (int)userPoint, 0.5f, it => it.ToString("n0"));
                pointLayerObject.transform.DOScale(Vector3.one * 1.1f, 0.1f).SetEase(Ease.OutElastic).SetLoops(5, LoopType.Yoyo);
            }
        }
        else
            pointAmountCount.text = userPoint.ToString("n0");
    }

     void RefreshPassTicketData(bool action = true)
    {
        uint passTicket = user_items.GetUserItemAmount(135);

        userPassTicket = passTicket;
        passTicketAmountCount.text = passTicket.ToString("n0");
    }
    

    public void SetTopUIMode(UI_MODE mode)
    {
        switch(mode)
        {   
            case UI_MODE.CATSTICK:
                goldLayerObject.SetActive(true);
                catLeafLayerObject.SetActive(true);
                pointLayerObject.SetActive(true);
                catStickLayerObject.SetActive(true);
                break;
            case UI_MODE.PASS_TICKET:
                goldLayerObject.SetActive(true);
                catLeafLayerObject.SetActive(true);
                pointLayerObject.SetActive(true);
                passTicketLayerObject.SetActive(true);
                break;
            case UI_MODE.CAT_PACKAGE_SHOP:
                goldLayerObject.SetActive(true);
                catLeafLayerObject.SetActive(true);
                pointLayerObject.SetActive(true);
                break;
            case UI_MODE.CAT_MARKET_SHOP:
                goldLayerObject.SetActive(true);
                catLeafLayerObject.SetActive(true);
                pointLayerObject.SetActive(true);
                break;
            case UI_MODE.CAT_LEAF_SHOP:
                goldLayerObject.SetActive(true);
                catLeafLayerObject.SetActive(true);
                pointLayerObject.SetActive(false);
                catStickLayerObject.SetActive(true);
                passTicketLayerObject.SetActive(true);
                break;
            case UI_MODE.NORMAL:
            default:
                goldLayerObject.SetActive(true);
                catLeafLayerObject.SetActive(true);
                pointLayerObject.SetActive(true);
                catStickLayerObject.SetActive(false);
                passTicketLayerObject.SetActive(false);
                break;
        }
    }
}
