using DG.Tweening;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopCommonItemInfo : MonoBehaviour
{
    [Header("[Special Object]")]
    public GameObject specialLayerObject;
    public GameObject specialDimmedObject;
    public Image specialItemIcon;
    public Text specialItemName;
    public Text remainText;

    [Space(15)]
    public Image specialPriceIcon;
    public Text specialPriceAmount;
    public Color specialPriceOriginColor;
    public Color specialPriceAlertColor;

    [Header("[Normal Object]")]
    public GameObject normalLayerObject;
    public GameObject normalDimmedObject;
    public Image normalItemIcon;
    public Text normalItemName;

    [Space(15)]
    public Image normalPriceIcon;
    public Text normalPriceAmount;
    public Color normalPriceOriginColor;
    public Color normalPriceAlertColor;

    neco_shop curShopData;
    neco_market curMarketData;
    bool isSpecialItem = false;

    NecoShopPanel rootParentPanel;

    uint userMoney;
    uint needMoney;

    uint maxPurcaseTryCount = 0;
    uint remainTryCount = 0;

    [Header("[Layout List]")]
    public RectTransform speciallayoutRect;
    public RectTransform normallayoutRect;

    public void OnClickItem()
    {
        // 프롤로그 체크
        neco_data.PrologueSeq seq = neco_data.GetPrologueSeq();
        switch (seq)
        {
            case neco_data.PrologueSeq.상점배스구매가이드퀘스트:
                if (curMarketData == null)
                {
                    NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("LOCALIZE_202"));
                    return;
                }
                else
                {
                    if (curMarketData.GetNecoMarketItemID() != 64)
                    {
                        NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("LOCALIZE_202"));
                        return;
                    }
                }
                break;
        }

        // 구매 가능한 상태 체크
        if (userMoney < needMoney)
        {
            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("LOCALIZE_370"));
            return;
        }
        else if (maxPurcaseTryCount <= 0)
        {
            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("LOCALIZE_371"));
            return;
        }

        NecoCanvas.GetPopupCanvas().OnShopPurchaseCountPopupShow(isSpecialItem, curShopData, curMarketData, maxPurcaseTryCount, TryPurchase);
    }

    public void SetNormalItemData(bool isSpecial, neco_shop shopData, NecoShopPanel parentPanel)
    {
        if (shopData == null) { return; }

        ClearData();

        curShopData = shopData;
        rootParentPanel = parentPanel;
        isSpecialItem = isSpecial;

        CalculateMaxCount();

        normalLayerObject.SetActive(true);

        normalItemIcon.sprite = curShopData.GetNecoShopIcon();
        normalItemName.text = curShopData.GetNecoShopName();

        normalPriceIcon.sprite = curShopData.GetNecoShopPriceIcon();
        normalPriceAmount.text = curShopData.GetNecoShopPrice().ToString("n0");
        normalPriceAmount.color = userMoney >= needMoney ? normalPriceOriginColor : normalPriceAlertColor;
        if (normalPriceAmount.GetComponent<Outline>() != null)
        {
            normalPriceAmount.GetComponent<Outline>().enabled = userMoney >= needMoney;
        }

        if (normallayoutRect != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(normallayoutRect);
        }
    }

    public void SetSpecialItemData(bool isSpecial, neco_market marketData, NecoShopPanel parentPanel, uint remain)
    {
        if (marketData == null) { return; }

        ClearData();

        curMarketData = marketData;
        rootParentPanel = parentPanel;
        isSpecialItem = isSpecial;
        remainTryCount = remain;

        items marketItem = items.GetItem(curMarketData.GetNecoMarketItemID());

        if (marketItem == null) { return; }

        CalculateMaxCount();

        specialLayerObject.SetActive(true);

        specialItemIcon.sprite = marketItem.GetItemIcon();
        specialItemName.text = marketItem.GetItemName();

        specialPriceIcon.sprite = Resources.Load<Sprite>("Sprites/Neco/Ui/Icon_coin");
        specialPriceAmount.text = curMarketData.GetNecoMarketPrice().ToString("n0");
        specialPriceAmount.color = userMoney >= needMoney ? specialPriceOriginColor : specialPriceAlertColor;
        if (specialPriceAmount.GetComponent<Outline>() != null)
        {
            specialPriceAmount.GetComponent<Outline>().enabled = userMoney >= needMoney;
        }

        specialDimmedObject.SetActive(remainTryCount <= 0);
        remainText.text = remainTryCount == 0 ? LocalizeData.GetText("LOCALIZE_372") : LocalizeData.GetText("LOCALIZE_373") + remainTryCount.ToString();

        // 프롤로그 체크
        if (neco_data.GetPrologueSeq() == neco_data.PrologueSeq.상점배스구매가이드퀘스트 && curMarketData.GetNecoMarketItemID() == 64)
        {
            gameObject.GetComponent<RectTransform>().DOScale(1.1f, 0.5f).SetEase(Ease.InOutQuad).SetLoops(-1, LoopType.Yoyo);
        }

        if (speciallayoutRect != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(speciallayoutRect);
        }
    }

    void TryPurchase(uint count)
    {
        WWWForm data = new WWWForm();
        data.AddField("api", "shop");
        data.AddField("op", 1);

        if (isSpecialItem)
        {
            data.AddField("list", 1);
            data.AddField("prod", curMarketData.GetNecoMarketID().ToString());
        }
        else
        {
            data.AddField("list", 0);
            data.AddField("prod", curShopData.GetNecoShopID().ToString());
        }

        data.AddField("cnt", count.ToString());

        NetworkManager.GetInstance().SendApiRequest("shop", 1, data, (response) =>
        {
            JObject root = JObject.Parse(response);
            JToken apiToken = root["api"];
            if (null == apiToken || apiToken.Type != JTokenType.Array
                || !apiToken.HasValues)
            {
                return;
            }

            JArray apiArr = (JArray)apiToken;
            foreach (JObject row in apiArr)
            {
                string uri = row.GetValue("uri").ToString();
                if (uri == "shop")
                {
                    JToken resultCode = row["rs"];

                    if (resultCode != null && resultCode.Type == JTokenType.Integer)
                    {
                        int rs = resultCode.Value<int>();
                        if (rs == 0)
                        {
                            NecoCanvas.GetPopupCanvas().OnPopupClose(NecoPopupCanvas.POPUP_TYPE.SHOP_BUY_COUNT_POPUP);

                            NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(LocalizeData.GetText("LOCALIZE_142"), LocalizeData.GetText("purchase_complete"), ()=> {
                                MapObjectController mapController = NecoCanvas.GetGameCanvas().GetCurMapController();
                                if (mapController != null)
                                {
                                    neco_data.PrologueSeq seq = neco_data.GetPrologueSeq();
                                    switch (seq)
                                    {
                                        case neco_data.PrologueSeq.상점배스구매가이드퀘스트:
                                            if (curMarketData != null && curMarketData.GetNecoMarketItemID() == 64)
                                            {
                                                int curRemain = PlayerPrefs.GetInt("GUIDE_QUEST_COUNT", 3);
                                                PlayerPrefs.SetInt("GUIDE_QUEST_COUNT", curRemain - (((int)count) - 1));
                                                mapController.SendMessage("상점배스구매완료", SendMessageOptions.DontRequireReceiver);
                                                return;
                                            }
                                            break;
                                    }
                                }
                            });

                            if (rootParentPanel == null) { return; }

                            rootParentPanel.RefreshLayer();
                        }
                        else
                        {
                            string msg = rs.ToString();
                            switch(rs)
                            {
                                case 1: msg = LocalizeData.GetText("LOCALIZE_355"); break;
                                case 2: msg = LocalizeData.GetText("LOCALIZE_356"); break; 
                                case 3: msg = LocalizeData.GetText("LOCALIZE_357"); break; 
                                case 4: msg = LocalizeData.GetText("LOCALIZE_358"); break; 
                                case 5: msg = LocalizeData.GetText("LOCALIZE_359"); break; 
                            }

                            NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(LocalizeData.GetText("LOCALIZE_360"), msg);

                            rootParentPanel.RefreshLayer();
                        }
                    }
                }
            }
        });
    }

    public void RefreshData()
    {
        if (rootParentPanel == null) { return; }

        rootParentPanel.RefreshLayer();
    }

    void CalculateMaxCount()
    {
        // 보유 골드 검사
        userMoney = rootParentPanel.GetUserResource(NecoShopPanel.SHOP_RESOURCE_TYPE.GOLD);
        needMoney = isSpecialItem ? curMarketData.GetNecoMarketPrice() : curShopData.GetNecoShopPrice();
        if (needMoney > 0)
        {
            // 골드 부족시 0개 처리
            maxPurcaseTryCount = needMoney > userMoney ? 0 : userMoney / needMoney;
        }

        if (curMarketData != null && isSpecialItem)
        {
            maxPurcaseTryCount = maxPurcaseTryCount > remainTryCount ? remainTryCount : maxPurcaseTryCount;
        }
    }

    void ClearData()
    {
        normalLayerObject.SetActive(false);
        normalDimmedObject.SetActive(false);

        specialLayerObject.SetActive(false);
        specialDimmedObject.SetActive(false);

        gameObject.GetComponent<RectTransform>().localScale = Vector3.one;
        gameObject.GetComponent<RectTransform>().DORewind();
        gameObject.GetComponent<RectTransform>().DOKill();
    }
}
