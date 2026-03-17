using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChristmasEventPopup : MonoBehaviour
{
    public enum CATEGORY { NONE, DRAW, PACKAGE, MISSION, ATTENDANCE };
    CATEGORY cur = CATEGORY.NONE;

    public Text periodText;

    public ChristmasDrawLayer drawLayer;
    public ChristmasPackageLayer packageLayer;
    public ChristmasMissionLayer missionLayer;
    public ChristmasAttendanceLayer attendanceLayer;

    public Button drawButtonLayer;
    public Button packageButtonLayer;
    public Button missionButtonLayer;
    public Button attendanceButtonLayer;

    public void CheckBaseInfo(CATEGORY category)
    {
        WWWForm data = new WWWForm();
        data.AddField("uri", "event");
        data.AddField("eid", (int)neco_event.EVENT_TYPE.CHRISTMAS);
        data.AddField("op", 1);

        NetworkManager.GetInstance().SendApiRequest("event", 1, data, (response) =>
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
                            JObject info = (JObject)row["info"];

                            if(NecoCanvas.GetCurTime() > info["end"].Value<uint>())
                            {
                                NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(LocalizeData.GetText("LOCALIZE_330"), LocalizeData.GetText("이벤트종료안내"), OnCloseEventPopup);
                            }
                            else
                            {
                                Init(category);
                            }                            
                        }
                        else
                        {
                            ChristmasEventWWWErrorCode(rs);
                        }
                    }
                }
            }
        });
    }

    public void Init(CATEGORY category)
    {
        if (cur == CATEGORY.NONE || cur != category)
        {
            //끔
            drawLayer.gameObject.SetActive(false);
            packageLayer.gameObject.SetActive(false);
            missionLayer.gameObject.SetActive(false);
            attendanceLayer.gameObject.SetActive(false);

            drawButtonLayer.interactable = true;
            packageButtonLayer.interactable = true;
            missionButtonLayer.interactable = true;
            attendanceButtonLayer.interactable = true;
        }

        if (cur == category)
            return;

        cur = category;
        switch (cur)
        {
            case CATEGORY.DRAW:
                drawLayer.gameObject.SetActive(true);
                drawButtonLayer.interactable = false;
                break;
            case CATEGORY.PACKAGE:
                packageLayer.gameObject.SetActive(true);
                packageButtonLayer.interactable = false;
                break;
            case CATEGORY.MISSION:
                missionLayer.gameObject.SetActive(true);
                missionButtonLayer.interactable = false;
                break;
            case CATEGORY.ATTENDANCE:
                attendanceLayer.gameObject.SetActive(true);
                attendanceButtonLayer.interactable = false;
                break;
        }

        periodText.text = LocalizeData.GetText("xmas기간");
    }

    public void MoveToCategory(int icategory)
    {
        Init((CATEGORY)icategory);
    }

    public void OnCloseEventPopup()
    {
        cur = CATEGORY.NONE;
        drawLayer.gameObject.SetActive(false);
        packageLayer.gameObject.SetActive(false);
        missionLayer.gameObject.SetActive(false);
        attendanceLayer.gameObject.SetActive(false);

        drawButtonLayer.interactable = true;
        packageButtonLayer.interactable = true;
        missionButtonLayer.interactable = true;
        attendanceButtonLayer.interactable = true;

        NecoCanvas.GetPopupCanvas().OnPopupClose(NecoPopupCanvas.POPUP_TYPE.CHRISTMAS_EVENT);
    }

    private void OnEnable()
    {
        WWWForm data = new WWWForm();
        data.AddField("uri", "event");
        data.AddField("eid", (int)neco_event.EVENT_TYPE.CHRISTMAS);
        data.AddField("op", 1);

        NetworkManager.GetInstance().SendApiRequest("event", 1, data, (response) =>
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
                            //Invoke("CheckAttendance", 1.5f);
                        }
                        else
                        {
                            string msg = rs.ToString();

                            ChristmasEventWWWErrorCode(rs);
                        }
                    }
                }
            }
        });
    }

    static public void ChristmasEventWWWErrorCode(int rs, bool isPopupEnd = false)
    {
        string msg = rs.ToString();
        switch (rs)
        {
            case 1: msg = LocalizeData.GetText("Event_Res_1"); break;
            case 2: msg = LocalizeData.GetText("이벤트종료안내"); break;
            case 11: msg = LocalizeData.GetText("Event_Res_11"); break;
            case 30: msg = LocalizeData.GetText("CHRISTMAS_NET_ERR_30"); break;
            case 31: msg = LocalizeData.GetText("CHRISTMAS_NET_ERR_31"); break;
            case 32: msg = LocalizeData.GetText("CHRISTMAS_NET_ERR_32"); break;
            case 42: msg = LocalizeData.GetText("Event_Res_42"); break;
        }

        if(isPopupEnd)
            NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(LocalizeData.GetText("LOCALIZE_330"), msg, ()=> NecoCanvas.GetPopupCanvas().OnPopupClose(NecoPopupCanvas.POPUP_TYPE.CHRISTMAS_EVENT));
        else
            NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(LocalizeData.GetText("LOCALIZE_330"), LocalizeData.GetText("LOCALIZE_199")); 
    }
}
