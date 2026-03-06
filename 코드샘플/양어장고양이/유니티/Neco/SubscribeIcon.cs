using Coffee.UIEffects;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SubscribeIcon : MonoBehaviour
{
    private neco_subscribe_data curData = null;
    //public UIEffect curImageEffect;
    //public Text curStateText;

    public void SetIcon(neco_subscribe_data data)
    {
        curData = data;

        //if (data.enable_recive)
        //{
        //    curImageEffect.effectFactor = 0.0f;
        //}
        //else
        //{
        //    curImageEffect.effectFactor = 1.0f;
        //}

        //curStateText.text = data.cur_day + "/" + data.max_day;
    }

    public void OnClick()
    {
        if (curData == null)
        {
            Destroy(gameObject);
            NecoCanvas.GetUICanvas().UIObject[(int)NecoUICanvas.UI_TYPE.TOP_INFO_UI].GetComponent<NecoTopUIInfoPanel>().CheckTimeSaleIcon();
            return;
        }

        if (curData.enable_recive == false)
        {
            if (curData.next_day_time < NecoCanvas.GetCurTime())
            {
                uint diff = NecoCanvas.GetCurTime() - curData.next_day_time;
                uint hour = diff / 60 * 60;

                string leftTime = "";
                if (hour > 0)
                {
                    leftTime = string.Format(LocalizeData.GetText("LOCALIZE_510"), hour);
                }
                else
                {
                    uint min = diff / 60;
                    if (min > 0)
                    {
                        leftTime = string.Format(LocalizeData.GetText("LOCALIZE_257"), min);
                    }
                    else
                    {
                        leftTime = string.Format(LocalizeData.GetText("LOCALIZE_211"), diff);
                    }
                }

                NecoCanvas.GetPopupCanvas().OnToastPopupShow(string.Format(LocalizeData.GetText("구독형상품수령대기안내"), leftTime));
            }
            else
            {
                NecoCanvas.GetUICanvas().UIObject[(int)NecoUICanvas.UI_TYPE.TOP_INFO_UI].GetComponent<NecoTopUIInfoPanel>().CheckTimeSaleIcon();
            }

            return;
        }

        GameObject popup = NecoCanvas.GetPopupCanvas().PopupObject[(int)NecoPopupCanvas.POPUP_TYPE.SUBSCRIBE_POPUP];
        if(popup != null)
        {
            SubscribeRecivePopup srp = popup.GetComponent<SubscribeRecivePopup>();
            if(srp != null)
            {
                srp.SetDay(curData, TryRecive);
                NecoCanvas.GetPopupCanvas().OnPopupShow(NecoPopupCanvas.POPUP_TYPE.SUBSCRIBE_POPUP);
            }
        }
    }

    public void TryRecive()
    {
        WWWForm data = new WWWForm();
        data.AddField("api", "iap");
        data.AddField("op", 7);
        data.AddField("prod", curData.prod_id.ToString());

        NetworkManager.GetInstance().SendApiRequest("iap", 7, data, (res) =>
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
                            NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(LocalizeData.GetText("LOCALIZE_330"), LocalizeData.GetText("구독상품수령"));
                        }
                        else
                        {
                            NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(LocalizeData.GetText("error"), LocalizeData.GetText("구독상품오류"));
                        }
                        NecoCanvas.GetUICanvas().UIObject[(int)NecoUICanvas.UI_TYPE.TOP_INFO_UI].GetComponent<NecoTopUIInfoPanel>().Invoke("CheckTimeSaleIcon", 0.1f);
                    }
                }
            }
        });
    }
}
