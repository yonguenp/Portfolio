using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NecoTutoSuccessPopup : MonoBehaviour
{
    public void OnClickCloseButton()
    {
        NecoCanvas.GetPopupCanvas().OnPopupClose(NecoPopupCanvas.POPUP_TYPE.TUTO_SUCCESS_POPUP);

        WWWForm pram = new WWWForm();
        pram.AddField("api", "iap");
        pram.AddField("op", 5);

        NetworkManager.GetInstance().SendApiRequest("iap", 5, pram, (res) =>
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

        NecoCanvas.GetPopupCanvas().ShowBanner(LocalizeData.GetText("LOCALIZE_465"), LocalizeData.GetText("LOCALIZE_466"), NecoBannerPopup.BANNER_TYPE.FIRST_BANNER, ()=> {
            NecoCanvas.GetPopupCanvas().OnPopupClose();
            NecoCanvas.GetPopupCanvas().OnShopListPopupShow(NecoShopPanel.SHOP_CATEGORY.PACKAGE, ShowMiniPackage);
        }, ShowMiniPackage);
    }

    public void ShowMiniPackage()
    {
        NecoCanvas.GetPopupCanvas().ShowBanner(LocalizeData.GetText("LOCALIZE_463"), LocalizeData.GetText("LOCALIZE_464"), NecoBannerPopup.BANNER_TYPE.MINI_PACKGAGE_TIMESALE,
                () => {
                    IAPManager.GetInstance().TryPurchase(55, "mini_package", (responseArr) =>
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

                        NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(LocalizeData.GetText("LOCALIZE_361"), LocalizeData.GetText("LOCALIZE_362"));
                        NecoCanvas.GetPopupCanvas().OnPopupClose(NecoPopupCanvas.POPUP_TYPE.BANNER_POPUP);

                        NecoCanvas.GetPopupCanvas().OnPopupShow(NecoPopupCanvas.POPUP_TYPE.ATTENDANCE_POPUP);
                        NecoAttendancePopup popup = NecoCanvas.GetPopupCanvas().PopupObject[(int)NecoPopupCanvas.POPUP_TYPE.ATTENDANCE_POPUP].GetComponent<NecoAttendancePopup>();
                        if (popup)
                        {
                            popup.SetToggleChat();
                        }
                    }, (responseArr) =>
                    {
                        NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(LocalizeData.GetText("LOCALIZE_316"), LocalizeData.GetText("LOCALIZE_344"));
                    });
                },
                () => {
                    NecoCanvas.GetPopupCanvas().OnPopupShow(NecoPopupCanvas.POPUP_TYPE.ATTENDANCE_POPUP);
                    NecoAttendancePopup popup = NecoCanvas.GetPopupCanvas().PopupObject[(int)NecoPopupCanvas.POPUP_TYPE.ATTENDANCE_POPUP].GetComponent<NecoAttendancePopup>();
                    if (popup)
                    {
                        popup.SetToggleChat();
                    }
                });
    }
}
