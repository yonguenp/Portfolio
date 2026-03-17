using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FishTruckData
{
    public neco_fish_trade fishTradeData;
    public items itemData;
};

public class FishTruckItem : MonoBehaviour
{
    [Header("[FishTruck Item UI]")]
    public GameObject dimmedObject;
    
    public Image iconImage;
    public Text itemCountText;
    public Text itemNameText;
    public Text itemPerGuideText;

    public Image priceBgImage;
    public Image priceImage;
    public Text priceAmountText;

    [Header("[Layer Color Info]")]
    public Color originItemButtonColor;
    public Color NotEnoughItemButtonColor;
    public Color originItemCountTextColor;
    public Color NotEnoughItemCountTextColor;

    FishTruckData curFishTruckData;

    uint userItemCount;
    uint needItemCount;

    NecoFishTruckPanel rootParentPanel;

    public void OnClickFishTruckItem()
    {
        if (userItemCount < needItemCount)
        {
            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("활어차교환재료부족"));
            return;
        }

        NecoCanvas.GetPopupCanvas().OnFishtruckPurchaseCountPopupShow(curFishTruckData, TryPurchaseFishTruckItem);
    }

    public void SetFishTruckItemData(FishTruckData truckItemData, NecoFishTruckPanel parentPanel)
    {
        if (truckItemData == null) { return; }

        rootParentPanel = parentPanel;

        curFishTruckData = truckItemData;

        iconImage.sprite = curFishTruckData.itemData.GetItemIcon();
        itemNameText.text = curFishTruckData.itemData.GetItemName();

        userItemCount = user_items.GetUserItemAmount(curFishTruckData.itemData.GetItemID());
        needItemCount = curFishTruckData.fishTradeData.GetBunchCount();

        itemCountText.text = userItemCount.ToString("n0");
        itemPerGuideText.text = string.Format(LocalizeData.GetText("활어차물고기갯수"), needItemCount);

        priceImage.sprite = Resources.Load<Sprite>("Sprites/Neco/Ui/Icon_fishticket");
        priceAmountText.text = curFishTruckData.fishTradeData.GetCoin().ToString("n0");

        priceAmountText.color = userItemCount >= needItemCount ? originItemCountTextColor : NotEnoughItemCountTextColor;
        priceBgImage.color = userItemCount >= needItemCount ? originItemButtonColor : NotEnoughItemButtonColor;

        //dimmedObject.SetActive(userItemCount < needItemCount);
    }

    void TryPurchaseFishTruckItem(uint purchaseCount)
    {
        // 활어차 품목 구매 API
        WWWForm data = new WWWForm();
        data.AddField("api", "trade");
        data.AddField("op", 1);
        data.AddField("item", curFishTruckData.itemData.GetItemID().ToString());
        data.AddField("rep", purchaseCount.ToString());

        NetworkManager.GetInstance().SendApiRequest("trade", 1, data, (response) =>
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
                if (uri == "trade")
                {
                    JToken resultCode = row["rs"];

                    if (resultCode != null && resultCode.Type == JTokenType.Integer)
                    {
                        int rs = resultCode.Value<int>();
                        if (rs == 0)
                        {
                            NecoCanvas.GetPopupCanvas().OnPopupClose(NecoPopupCanvas.POPUP_TYPE.FISH_TRUCK_BUY_COUNT_POPUP);

                            if (row.ContainsKey("rew"))
                            {
                                NecoCanvas.GetPopupCanvas().OnRewardPopupShow(LocalizeData.GetText("활어차교환완료"), LocalizeData.GetText("활어차교환완료가이드"), "trade", apiArr, () => {
                                    
                                    rootParentPanel?.RefreshData();
                                });
                            }
                        }
                        else
                        {
                            string msg = rs.ToString();
                            switch (rs)
                            {
                                case 1: msg = LocalizeData.GetText("Fishtruck_Trade_Res_1"); break;
                                case 2: msg = LocalizeData.GetText("Fishtruck_Trade_Res_2"); break;
                                case 3: msg = LocalizeData.GetText("Fishtruck_Trade_Res_3"); break;
                                case 4: msg = LocalizeData.GetText("Fishtruck_Trade_Res_4"); break;
                            }

                            NecoCanvas.GetPopupCanvas().OnPopupClose(NecoPopupCanvas.POPUP_TYPE.FISH_TRUCK_BUY_COUNT_POPUP);

                            NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(LocalizeData.GetText("활어차교환실패"), msg);
                        }
                    }
                }
            }
        });
    }

    void RefreshData()
    {
        // UI 갱신 처리
        priceAmountText.color = userItemCount >= needItemCount ? originItemCountTextColor : NotEnoughItemCountTextColor;
        priceBgImage.color = userItemCount >= needItemCount ? originItemButtonColor : NotEnoughItemButtonColor;

        //dimmedObject.SetActive(userItemCount < needItemCount);
    }
}
