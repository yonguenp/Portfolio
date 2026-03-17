using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimeSaleIcon : MonoBehaviour
{
    public Text RemainText;
    uint overTime = 0;
    uint curProductID = 0;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
        
    public void OnEnable()
    {
        if (overTime == 0)
            return;

        SetUI();
    }

    public void SetTimeOver(uint productID)
    {
        curProductID = productID;
        overTime = neco_data.Instance.GetTimeSaleProductRemain(curProductID);
        SetUI();
    }

    void SetUI()
    {
        CancelInvoke("SetUI");

        int remain = (int)overTime - (int)NecoCanvas.GetCurTime();

        if (remain <= 0)
        {
            Destroy(gameObject);
            NecoCanvas.GetUICanvas().UIObject[(int)NecoUICanvas.UI_TYPE.TOP_INFO_UI].GetComponent<NecoTopUIInfoPanel>().CheckTimeSaleIcon();
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
            txt = string.Format("{0}:{1}:{2}", timeSpan.Hours.ToString("00"), timeSpan.Minutes.ToString("00"), timeSpan.Seconds.ToString("00"));
        }

        RemainText.text = txt;

        Invoke("SetUI", 1.0f);
    }

    public void OnClickIcon()
    {
        NecoBannerPopup.BANNER_TYPE type;
        switch (curProductID)
        {
            case 55:
                type = NecoBannerPopup.BANNER_TYPE.MINI_PACKGAGE_TIMESALE;
                break;
            case 34:
                type = NecoBannerPopup.BANNER_TYPE.LEVEL4_PACKAGE_TIMESALE;
                break;
            case 36:
                type = NecoBannerPopup.BANNER_TYPE.LEVEL5_PACKAGE_TIMESALE;
                break;
            case 37:
                type = NecoBannerPopup.BANNER_TYPE.LEVEL6_PACKAGE_TIMESALE;
                break;
            case 42:
                type = NecoBannerPopup.BANNER_TYPE.LEVEL7_PACKAGE_TIMESALE;
                break;
            case 43:
                type = NecoBannerPopup.BANNER_TYPE.LEVEL8_PACKAGE_TIMESALE;
                break;
            case 45:
                type = NecoBannerPopup.BANNER_TYPE.LEVEL9_PACKAGE_TIMESALE;
                break;
            default:
                return;
        }
        

        NecoCanvas.GetPopupCanvas().ShowBanner(LocalizeData.GetText("LOCALIZE_463"), LocalizeData.GetText("LOCALIZE_464"), type,
            () => {
                string product = neco_shop.GetNecoShopData(curProductID).GetIAPConstants();
                if (string.IsNullOrEmpty(product))
                {
                    return;
                }

                IAPManager.GetInstance().TryPurchase(curProductID, product, (responseArr) =>
                {
                    WWWForm data = new WWWForm();
                    data.AddField("api", "iap");
                    data.AddField("op", 5);

                    NetworkManager.GetInstance().SendApiRequest("iap", 5, data, (res) =>
                    {
                        JObject root = JObject.Parse(res);
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
                            if (uri == "iap")
                            {
                                JToken resultCode = row["rs"];
                                if (resultCode != null && resultCode.Type == JTokenType.Integer)
                                {
                                    int rs = resultCode.Value<int>();
                                    if (rs == 0)
                                    {
                                        if (row.ContainsKey("limit"))
                                        {                                            
                                            neco_data.Instance.SetTimeSale((JObject)row["limit"]);
                                        }

                                        neco_data.Instance.SetBenefit(false);
                                        if (row.ContainsKey("first"))
                                        {
                                            neco_data.Instance.SetBenefit(row["first"].Value<uint>() > 0);
                                        }
                                    }
                                    else
                                    {
                                        string msg = rs.ToString();
                                        switch (rs)
                                        {
                                            case 1: msg = LocalizeData.GetText("LOCALIZE_199"); break;
                                            case 2: msg = LocalizeData.GetText("LOCALIZE_507"); break;
                                            case 3: msg = LocalizeData.GetText("LOCALIZE_375"); break;
                                            case 4: msg = LocalizeData.GetText("LOCALIZE_199"); break;
                                            case 5: msg = LocalizeData.GetText("LOCALIZE_199"); break;
                                            case 6: msg = LocalizeData.GetText("LOCALIZE_333"); break;
                                        }

                                        NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(LocalizeData.GetText("LOCALIZE_360"), msg);
                                    }
                                }
                            }
                        }

                        NecoCanvas.GetUICanvas().UIObject[(int)NecoUICanvas.UI_TYPE.TOP_INFO_UI].GetComponent<NecoTopUIInfoPanel>().CheckTimeSaleIcon();
                    });

                    NecoCanvas.GetPopupCanvas().OnPopupClose();
                    NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(LocalizeData.GetText("LOCALIZE_361"), LocalizeData.GetText("LOCALIZE_362"));
                }, (responseArr) =>
                {
                    NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(LocalizeData.GetText("LOCALIZE_316"), LocalizeData.GetText("LOCALIZE_344"));
                });
            }
        );
    }
}
