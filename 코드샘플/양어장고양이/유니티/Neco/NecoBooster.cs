using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NecoBooster : MonoBehaviour
{
    public enum BOOST_TYPE
    {
        NONE,
        FISH_FARM,
        CAT_GIFT,
        FISH_TRAP,
    }

    public delegate void Callback();

    Callback okCallback = null;

    public void PurchaseBoostItem(BOOST_TYPE curSelectedType, Callback _okCallback = null)
    {
        okCallback = _okCallback;

        WWWForm param = new WWWForm();
        param.AddField("api", "plant");
        param.AddField("op", 4);
        switch (curSelectedType)
        {
            case BOOST_TYPE.CAT_GIFT:
                {
                    param.AddField("id", 103);
                }
                break;
            case BOOST_TYPE.FISH_FARM:
                {
                    param.AddField("id", 101);
                }
                break;
            case BOOST_TYPE.FISH_TRAP:
                {
                    param.AddField("id", 102);
                }
                break;
        }

        NetworkManager.GetInstance().SendApiRequest("plant", 4, param, (response) => {
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
                if (uri == "plant")
                {
                    JToken resultCode = row["rs"];

                    if (resultCode != null && resultCode.Type == JTokenType.Integer)
                    {
                        int rs = resultCode.Value<int>();
                        if (rs == 0)
                        {
                            Invoke("RefreshBoosterData", 0.1f);
                        }
                        else
                        {
                            if (rs == 4)
                            {
                                NecoCanvas.GetPopupCanvas().OnPopupShow(NecoPopupCanvas.POPUP_TYPE.CATNIP_BUY_POPUP);
                            }
                            else
                            {
                                NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(LocalizeData.GetText("LOCALIZE_271"), LocalizeData.GetText("LOCALIZE_272"));
                            }
                        }
                    }
                }
            }
            
        });
    }

    public void RefreshBoosterData()
    {
        NecoCanvas.GetUICanvas().RefreshTopUILayer(TOP_UI_PANEL_TYPE.RESOURCE);

        okCallback?.Invoke();
    }

    public ConfirmPopupData SetConfirmPopupData(BOOST_TYPE boostType)
    {
        ConfirmPopupData popupData = new ConfirmPopupData();

        popupData.titleText = LocalizeData.GetText("LOCALIZE_235");
        popupData.titleMessageText = LocalizeData.GetText("LOCALIZE_236");
        neco_booster boosterData = null;
        switch (boostType)
        {
            case BOOST_TYPE.CAT_GIFT:
                popupData.messageText_1 = LocalizeData.GetText("LOCALIZE_237");
                boosterData = neco_booster.GetBoosterData(3);
                break;
            case BOOST_TYPE.FISH_FARM:
                popupData.messageText_1 = LocalizeData.GetText("LOCALIZE_238");
                boosterData = neco_booster.GetBoosterData(1);
                break;
            case BOOST_TYPE.FISH_TRAP:
                popupData.messageText_1 = LocalizeData.GetText("LOCALIZE_239");
                boosterData = neco_booster.GetBoosterData(2);
                break;
        }


        switch (boosterData.GetPriceType())
        {
            case "dia":
                popupData.amountIcon = Resources.Load<Sprite>("Sprites/Neco/Ui/Icon_catleaf");
                break;
            case "gold":
                popupData.amountIcon = Resources.Load<Sprite>("Sprites/Neco/Ui/Icon_Material_coin");
                break;
            case "point":
                popupData.amountIcon = Resources.Load<Sprite>("Sprites/Neco/Ui/Icon_Material_point");
                break;
        }

        popupData.amountText = boosterData.GetPrice().ToString("n0"); // todo bt - 추후 데이터 연동 필요

        return popupData;
    }
}
