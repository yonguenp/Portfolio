using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json.Linq;
public class FriendsList : FriendsUIObject
{
    public Button SendAllButton;
    public Button ReciveAllButton;


    public Text FriendCount;

    override public void RefreshUI()
    {
        ClearItems();

        WWWForm data = new WWWForm();
        data.AddField("api", "friend");
        data.AddField("op", 1);

        SendAllButton.interactable = false;
        ReciveAllButton.interactable = false;

        NetworkManager.GetInstance().SendApiRequest("friend", 1, data, (response) =>
        {
            Invoke("RefreshFriendList", 0.1f);
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
    }

    public void RefreshFriendList()
    {
        bool enableSendAll = false;
        bool enableReciveAll = false;

        List<UserProfile> friends = FriendsManager.Instance.GetFriendList();
        foreach (UserProfile friend in friends)
        {
            FriendItem item = cloneTargetItem.CloneItem(this);
            item.SetFriendListItem(friend);

            if ((friend.gift_flag & 1) != 0)
                enableSendAll = true;
            if ((friend.gift_flag & 2) != 0)
                enableReciveAll = true;

        }

        FriendCount.text = "(" + friends.Count.ToString() + "/40)";
        SendAllButton.interactable = enableSendAll;
        ReciveAllButton.interactable = enableReciveAll;
    }

    public void OnGiftSendAll()
    {
        WWWForm data = new WWWForm();
        data.AddField("api", "friend");
        data.AddField("op", 10);

        NetworkManager.GetInstance().SendApiRequest("friend", 10, data, (response) =>
        {
            Invoke("RefreshUI", 0.1f);
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
    }

    public void OnGiftReciveAll()
    {
        WWWForm data = new WWWForm();
        data.AddField("api", "friend");
        data.AddField("op", 11);

        NetworkManager.GetInstance().SendApiRequest("friend", 11, data, (response) =>
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
                        if (rs == 0)
                        {
                            if (row.ContainsKey("rew"))
                            {
                                NecoCanvas.GetPopupCanvas().OnRewardPopupShow(LocalizeData.GetText("LOCALIZE_200"), LocalizeData.GetText("LOCALIZE_201"), "friend", apiArr);
                            }
                        }
                        else
                        {
                            if (rs == 1)
                                NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(LocalizeData.GetText("LOCALIZE_332"), LocalizeData.GetText("name_edit_error_nocard"));
                        }
                    }
                }
            }

            Invoke("RefreshUI", 0.1f);
        }, null, false);
    }

    override public bool IsNewFlag()
    {
        return FriendsManager.Instance.GetNewFriendCount() > 0;
    }

    override public void NewFlagDone()
    {
        FriendsManager.Instance.SetNewFriendCount(0);
    }
}
