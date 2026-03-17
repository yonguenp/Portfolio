using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SongpyeonShopItemInfo : MonoBehaviour
{
    [HideInInspector]
    public const int MAX_PURCHASE_LIMIT = 100; // 한번에 구매 가능한 최대 수

    public GameObject dimmedObjectLayer;

    public Image itemImage;
    public Image priceImage;

    public Text itemNameText;
    public Text itemCountText;
    public Text priceText;
    public Text purchaseCountText;

    public Color originTextColor;
    public Color notEnoughTextColor;

    public RectTransform layoutRect;

    neco_event_thanks_shop curEventShopData;

    ChuseokSongpyeonShopPanel shopParentPanel;

    uint userSongpyeon = 0;
    int maxPurcaseTryCount = 0;
    int remainTryCount = 0;   

    public void OnClickItem()
    {
        // 구매 가능한 상태 체크
        if (userSongpyeon < curEventShopData.GetNecoEventShopPrice())
        {
            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("송편부족"));
            return;
        }
        else if (maxPurcaseTryCount <= 0)
        {
            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("LOCALIZE_371"));
            return;
        }

        shopParentPanel.OnShowPurchasePopup(curEventShopData, maxPurcaseTryCount, TryPurchase);
    }

    public void SetPurchaseLimitItemData(neco_event_thanks_shop eventShopData, ChuseokSongpyeonShopPanel parentPanel)
    {
        if (eventShopData == null) { return; }

        curEventShopData = eventShopData;
        shopParentPanel = parentPanel;

        chuseok_event eventData = null;
        chuseok_event.chuseok_shop_data ShopData = null;
        foreach (neco_event evt in neco_data.Instance.GetEvents())
        {
            if ((neco_event.EVENT_TYPE)evt.GetEventID() == neco_event.EVENT_TYPE.CHUSEOK)
                eventData = (chuseok_event)evt;
        }

        if (eventData != null)
        {
            ShopData = eventData.GetShopData();
        }

        if (ShopData == null) { return; }

        remainTryCount = ShopData.saleDic[eventShopData.GetNecoEventShopID()];

        switch (curEventShopData.GetNecoEventShopItemType())
        {
            case "gold":
                itemImage.sprite = Resources.Load<Sprite>("Sprites/Neco/Ui/Icon_Material_coin");
                itemNameText.text = LocalizeData.GetText("LOCALIZE_229");
                itemCountText.text = eventShopData.GetNecoEventShopCount().ToString("n0");

                priceImage.sprite = Resources.Load<Sprite>("Sprites/Neco/Ui/Icon_songpyeon");
                priceText.text = eventShopData.GetNecoEventShopPrice().ToString("n0");
                purchaseCountText.text = remainTryCount == 0 ? LocalizeData.GetText("LOCALIZE_372") : LocalizeData.GetText("LOCALIZE_373") + remainTryCount.ToString();

                break;
            case "item":
                items itemData = items.GetItem(eventShopData.GetNecoEventShopItemID());
                if (itemData != null)
                {
                    itemImage.sprite = itemData.GetItemIcon();
                    itemNameText.text = itemData.GetItemName();
                    itemCountText.text = eventShopData.GetNecoEventShopCount().ToString("n0");

                    priceImage.sprite = Resources.Load<Sprite>("Sprites/Neco/Ui/Icon_songpyeon");
                    priceText.text = eventShopData.GetNecoEventShopPrice().ToString("n0");
                    purchaseCountText.text = remainTryCount == 0 ? LocalizeData.GetText("LOCALIZE_372") : LocalizeData.GetText("LOCALIZE_373") + remainTryCount.ToString();
                }

                break;
        }

        // 거래 가능 최대 갯수 계산
        userSongpyeon = user_items.GetUserItemAmount(144);
        maxPurcaseTryCount = (int)(userSongpyeon / curEventShopData.GetNecoEventShopPrice());

        // 구매 가능 횟수 검사 
        if (remainTryCount < 0) // remainTryCount가 음수인 경우는 구매 제한 없는 품목
        {
            // MAX_PURCHASE_LIMIT 만큼 한번에 구매가능한 횟수 제한
            maxPurcaseTryCount = maxPurcaseTryCount > MAX_PURCHASE_LIMIT ? MAX_PURCHASE_LIMIT : maxPurcaseTryCount;
        }
        else
        {
            maxPurcaseTryCount = maxPurcaseTryCount > remainTryCount ? remainTryCount : maxPurcaseTryCount;
        }

        // Ui 관련 처리
        priceText.color = maxPurcaseTryCount > 0 ? originTextColor : notEnoughTextColor;
        dimmedObjectLayer.SetActive(remainTryCount == 0);
        purchaseCountText.gameObject.SetActive(remainTryCount > 0); // 무제한 구매가능한 상품은 재고 텍스트 표시 off

        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(RefreshLayout());
        }
    }

    void TryPurchase(uint count)
    {
        // 추석 아이템 구매 API 추가
        WWWForm data = new WWWForm();
        data.AddField("uri", "event");
        data.AddField("eid", (int)neco_event.EVENT_TYPE.CHUSEOK);
        data.AddField("op", 31);
        data.AddField("prod", curEventShopData.GetNecoEventShopID().ToString());
        data.AddField("cnt", count.ToString());

        NetworkManager.GetInstance().SendApiRequest("event", 31, data, (response) =>
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
                if (uri == "event")
                {
                    JToken resultCode = row["rs"];

                    if (resultCode != null && resultCode.Type == JTokenType.Integer)
                    {
                        int rs = resultCode.Value<int>();
                        if (rs == 0)
                        {
                            shopParentPanel.OnClosePurchasePopup();

                            if (row.ContainsKey("rew"))
                            {
                                NecoCanvas.GetPopupCanvas().OnRewardPopupShow(LocalizeData.GetText("LOCALIZE_142"), LocalizeData.GetText("purchase_complete"), "event", apiArr, () => {

                                    shopParentPanel.RefreshData();
                                });
                            }
                        }
                        else
                        {
                            string msg = rs.ToString();
                            switch (rs)
                            {
                                case 1: msg = LocalizeData.GetText("Event_Res_1"); break;
                                case 2: msg = LocalizeData.GetText("Event_Res_2"); break;
                                case 11: msg = LocalizeData.GetText("Event_Res_11"); break;
                                case 12: msg = LocalizeData.GetText("Event_Res_12"); break;
                                case 13: msg = LocalizeData.GetText("Event_Res_13"); break;
                                case 14: msg = LocalizeData.GetText("Event_Res_14"); break;
                                case 31: msg = LocalizeData.GetText("Event_Res_31"); break;
                                case 32: msg = LocalizeData.GetText("Event_Res_32"); break;
                                case 41: msg = LocalizeData.GetText("Event_Res_41"); break;
                                case 42: msg = LocalizeData.GetText("Event_Res_42"); break;
                            }

                            NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(LocalizeData.GetText("LOCALIZE_316"), msg);
                        }
                    }
                }
            }
        });
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
