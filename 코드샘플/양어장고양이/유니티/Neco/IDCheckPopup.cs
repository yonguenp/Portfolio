using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IDCheckPopup : MonoBehaviour
{
    public HahahaChat chatUI;
    public Text UserID;

    private string accountNo = "";
    private string chatMsg = "";
    public void Init(string acn, string name, string msg)
    {
        accountNo = acn;
        chatMsg = msg;

        UserID.text = name;
    }

    public void OnClose()
    {
        NecoCanvas.GetPopupCanvas().OnPopupClose(NecoPopupCanvas.POPUP_TYPE.ID_CHECKER);
    }

    public void OnBlock()
    {
        WWWForm data = new WWWForm();
        data.AddField("api", "friend");
        data.AddField("op", 15);
        data.AddField("uno", accountNo.ToString());

        NetworkManager.GetInstance().SendApiRequest("friend", 15, data, (response) =>
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
                if (uri == "friend")
                {
                    JToken resultCode = row["rs"];
                    if (resultCode != null && resultCode.Type == JTokenType.Integer)
                    {
                        int rs = resultCode.Value<int>();
                        switch (rs)
                        {
                            case 0:
                                NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("차단메시지"));
                                break;
                            case 1:
                                NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("NO_SUCH_USER"));
                                break;
                            case 2:
                                NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("ALREADY_FRIEND"));
                                break;
                            case 3:
                                NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("ALREADY_SENT"));
                                break;
                            case 4:
                                NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("NO_REQUEST_SENT"));
                                break;
                            case 5:
                                NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("NO_REQUEST_TAKEN"));
                                break;
                            case 6:
                                NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("FRIEND_LIST_FULL"));
                                break;
                            case 7:
                                NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("NOT_A_FRIEND"));
                                break;
                            case 8:
                                NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("ALREADY_RECEIVED"));
                                break;
                            case 9:
                                NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("CANNOT_SEND_YET"));
                                break;
                            case 10:
                                NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("NOTHING_TO_RECEIVE"));
                                break;
                            case 11:
                                NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("GIFT_DAILY_LIMITED"));
                                break;
                            default:
                                NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("LOCALIZE_199"));
                                break;
                        }
                    }
                }
            }
        }, null, false);

        OnClose();
    }

    public void OnSendFriend()
    {
        WWWForm data = new WWWForm();
        data.AddField("api", "friend");
        data.AddField("op", 5);
        data.AddField("uno", accountNo.ToString());

        NetworkManager.GetInstance().SendApiRequest("friend", 5, data, (response) =>
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
                if (uri == "friend")
                {
                    JToken resultCode = row["rs"];
                    if (resultCode != null && resultCode.Type == JTokenType.Integer)
                    {
                        int rs = resultCode.Value<int>();
                        switch (rs)
                        {
                            case 0:
                                NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("친구요청메시지"));
                                break;
                            case 1:
                                NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("NO_SUCH_USER"));
                                break;
                            case 2:
                                NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("ALREADY_FRIEND"));
                                break;
                            case 3:
                                NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("ALREADY_SENT"));
                                break;
                            case 4:
                                NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("NO_REQUEST_SENT"));
                                break;
                            case 5:
                                NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("NO_REQUEST_TAKEN"));
                                break;
                            case 6:
                                NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("FRIEND_LIST_FULL"));
                                break;
                            case 7:
                                NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("NOT_A_FRIEND"));
                                break;
                            case 8:
                                NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("ALREADY_RECEIVED"));
                                break;
                            case 9:
                                NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("CANNOT_SEND_YET"));
                                break;
                            case 10:
                                NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("NOTHING_TO_RECEIVE"));
                                break;
                            case 11:
                                NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("GIFT_DAILY_LIMITED"));
                                break;
                            default:
                                NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("LOCALIZE_199"));
                                break;
                        }
                    }
                }
            }
        }, null, false);

        OnClose();
    }

    public void OnAccusation()
    {
        WWWForm data = new WWWForm();
        data.AddField("api", "friend");
        data.AddField("op", 17);
        data.AddField("uno", accountNo.ToString());
        data.AddField("msg", chatMsg.ToString());
        NetworkManager.GetInstance().SendApiRequest("friend", 17, data, (response) =>
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
                if (uri == "friend")
                {
                    JToken resultCode = row["rs"];
                    if (resultCode != null && resultCode.Type == JTokenType.Integer)
                    {
                        int rs = resultCode.Value<int>();
                        switch (rs)
                        {
                            case 0:
                                NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("신고메시지"));
                                break;
                            case 1:
                                NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("NO_SUCH_USER"));
                                break;
                            case 2:
                                NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("ALREADY_FRIEND"));
                                break;
                            case 3:
                                NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("ALREADY_SENT"));
                                break;
                            case 4:
                                NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("NO_REQUEST_SENT"));
                                break;
                            case 5:
                                NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("NO_REQUEST_TAKEN"));
                                break;
                            case 6:
                                NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("FRIEND_LIST_FULL"));
                                break;
                            case 7:
                                NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("NOT_A_FRIEND"));
                                break;
                            case 8:
                                NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("ALREADY_RECEIVED"));
                                break;
                            case 9:
                                NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("CANNOT_SEND_YET"));
                                break;
                            case 10:
                                NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("NOTHING_TO_RECEIVE"));
                                break;
                            case 11:
                                NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("GIFT_DAILY_LIMITED"));
                                break;
                            default:
                                NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("LOCALIZE_199"));
                                break;
                        }
                    }
                }
            }
        }, null, false);

        //NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("신고메시지"));

        OnClose();
    }
}
