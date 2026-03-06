using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FriendItem : MonoBehaviour
{
    public FriendsPanel.FriendsUI type;
    //common
    public Image LevelIcon;
    public Text LevelText;
    public Text NickName;

    //for list
    public Button DeleteButton;
    public Button GiftSendButton;
    public Button GiftReciveButton;
    public GameObject RedDot;
    public Text LastChat;

    //for search
    public GameObject ReccomandText;
    public Button SentRequestButton;
    public GameObject WaitForAcceptText;
    public Button WaitCancelButton;
    public Button BlockButton;

    //for Call
    public Button AcceptButton;

    private FriendsUIObject parentFriendUI;
    private Coroutine updateUserAlarm = null;
    private UserProfile friendProfile = null;

    public FriendItem CloneItem(FriendsUIObject parent)
    {
        //parentFriendUI = parent;
        gameObject.SetActive(true);

        GameObject listItem = Instantiate(this.gameObject);
        listItem.transform.SetParent(parent.itemContainer);
        RectTransform rt = listItem.GetComponent<RectTransform>();
        rt.localPosition = Vector3.zero;
        rt.localScale = Vector3.one;

        gameObject.SetActive(false);

        FriendItem ret = listItem.GetComponent<FriendItem>();
        ret.parentFriendUI = parent;

        return ret;
    }

    private void SetCommonUI(UserProfile friend)
    {
        friendProfile = friend;
        NickName.text = friend.nick;
        //LevelText.text = friend.level.ToString();
        //Vector3 color = GameCanvas.GetLevelColorTable(friend.level);
        //LevelIcon.color = new Color(color.x / 255.0f, color.y / 255.0f, color.z / 255.0f);

        friend.UIShown(System.Convert.ToUInt32(NecoCanvas.GetCurTime()));
    }

    //for list
    public void SetFriendListItem(UserProfile friend)
    {
        RedDot.SetActive(friend.isAlarm);
        string message = friend.lastMessage;
        if (!string.IsNullOrEmpty(message))
        {
            if(IsCardMessage(message))
                message = LocalizeData.GetText("LOCALIZE_184");

            LastChat.text = message;
        }
        SetCommonUI(friend);
                
        GiftSendButton.interactable = (friend.gift_flag & 1) != 0;
        GiftReciveButton.interactable = (friend.gift_flag & 2) != 0;

        if(updateUserAlarm == null)
        {
            updateUserAlarm = StartCoroutine(UpdateUserAlarm());
        }
    }
    
    public void OnDeleteButton()
    {
        Debug.Log("OnDeleteButton");
        
        WWWForm data = new WWWForm();
        data.AddField("api", "friend");
        data.AddField("op", 9);
        data.AddField("uno", friendProfile.uno.ToString());

        NetworkManager.GetInstance().SendApiRequest("friend", 9, data, (response) =>
        {
            parentFriendUI.Invoke("RefreshUI", 0.1f);
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

    public void OnGiftSendButton()
    {
        Debug.Log("OnGiftSendButton");

        WWWForm data = new WWWForm();
        data.AddField("api", "friend");
        data.AddField("op", 10);
        data.AddField("uno", friendProfile.uno.ToString());

        NetworkManager.GetInstance().SendApiRequest("friend", 10, data, (response) =>
        {
            parentFriendUI.Invoke("RefreshUI", 0.1f);
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

    public void OnGiftReciveButton()
    {
        Debug.Log("OnGiftReciveButton");

        WWWForm data = new WWWForm();
        data.AddField("api", "friend");
        data.AddField("op", 11);
        data.AddField("uno", friendProfile.uno.ToString());

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
            parentFriendUI.Invoke("RefreshUI", 0.1f);
        }, null, false);
    }

    //for search

    public void SetFriendSentItem(UserProfile friend)
    {
        SetCommonUI(friend);
        ReccomandText.SetActive(false);
        SentRequestButton.gameObject.SetActive(false);
        WaitForAcceptText.SetActive(true);
        WaitCancelButton.gameObject.SetActive(true);
        BlockButton?.gameObject.SetActive(false);
    }

    public void SetFriendRecommandItem(UserProfile friend)
    {
        SetCommonUI(friend);
        SentRequestButton.gameObject.SetActive(true);
        ReccomandText.SetActive(true);
        WaitForAcceptText.SetActive(false);
        WaitCancelButton.gameObject.SetActive(false);
        BlockButton?.gameObject.SetActive(false);
    }

    public void SetBlcokedCallItem(UserProfile friend)
    {
        SetCommonUI(friend);
        ReccomandText.SetActive(false);
        SentRequestButton.gameObject.SetActive(false);
        WaitForAcceptText.SetActive(false);
        WaitCancelButton.gameObject.SetActive(true);
        BlockButton?.gameObject.SetActive(false);
    }

    public void SetFriendSearchItem(UserProfile friend)
    {
        SetCommonUI(friend);
        ReccomandText.SetActive(false);
        SentRequestButton.gameObject.SetActive(true);
        WaitForAcceptText.SetActive(false);
        WaitCancelButton.gameObject.SetActive(false);
        BlockButton?.gameObject.SetActive(true);
        List<UserProfile> friends = FriendsManager.Instance.GetFriendList();
        foreach(UserProfile fr in friends)
        {
            if(fr.uno == friend.uno)//이미 친구
            {
                SentRequestButton.gameObject.SetActive(false);
            }
        }

        if(friends.Count >= 40)
        {
            SentRequestButton.gameObject.SetActive(false);
        }
    }

    public void OnSentRequestButton()
    {
        Debug.Log("OnRequestButton");
        WWWForm data = new WWWForm();
        data.AddField("api", "friend");
        data.AddField("op", 5);
        data.AddField("uno", friendProfile.uno.ToString());

        NetworkManager.GetInstance().SendApiRequest("friend", 5, data, (response) =>
        {
            parentFriendUI.Invoke("RefreshUI", 0.1f);
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

    public void OnUnBlockButton()
    {
        Debug.Log("OnRequestButton");
        WWWForm data = new WWWForm();
        data.AddField("api", "friend");
        data.AddField("op", 16);
        data.AddField("uno", friendProfile.uno.ToString());

        NetworkManager.GetInstance().SendApiRequest("friend", 16, data, (response) =>
        {
            parentFriendUI.Invoke("RefreshUI", 0.1f);
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

    public void OnCancelRequestButton()
    {
        Debug.Log("OnCancelRequestButton");
        WWWForm data = new WWWForm();
        data.AddField("api", "friend");
        data.AddField("op", 6);
        data.AddField("uno", friendProfile.uno.ToString());

        NetworkManager.GetInstance().SendApiRequest("friend", 6, data, (response) =>
        {
            parentFriendUI.Invoke("RefreshUI", 0.1f);
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

    //for call

    public void SetFriendCallItem(UserProfile friend)
    {
        if(RedDot)
            RedDot.SetActive(friend.isAlarm);
        SetCommonUI(friend);
    }

    public void OnAcceptButton()
    {
        Debug.Log("OnAcceptButton");
        WWWForm data = new WWWForm();
        data.AddField("api", "friend");
        data.AddField("op", 7);
        data.AddField("uno", friendProfile.uno.ToString());

        NetworkManager.GetInstance().SendApiRequest("friend", 7, data, (response) =>
        {
            parentFriendUI.Invoke("RefreshUI", 0.1f);
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

    public void OnDeclineButton()
    {
        Debug.Log("OnDeclineButton");
        WWWForm data = new WWWForm();
        data.AddField("api", "friend");
        data.AddField("op", 8);
        data.AddField("uno", friendProfile.uno.ToString());

        NetworkManager.GetInstance().SendApiRequest("friend", 8, data, (response) =>
        {
            parentFriendUI.Invoke("RefreshUI", 0.1f);
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

    public void OnBlockButton()
    {
        Debug.Log("OnBlockButton");
        WWWForm data = new WWWForm();
        data.AddField("api", "friend");
        data.AddField("op", 15);
        data.AddField("uno", friendProfile.uno.ToString());

        NetworkManager.GetInstance().SendApiRequest("friend", 15, data, (response) =>
        {
            parentFriendUI.Invoke("RefreshUI", 0.1f);
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

    public void OnPersnalChatOpen()
    {   
        parentFriendUI.FriendPanel.chatUI.ChatRoomID = friendProfile.uno.ToString();
        parentFriendUI.FriendPanel.OnExitButton();
    }

    public IEnumerator UpdateUserAlarm()
    {
        while(true)
        {
            if(!RedDot.activeSelf)
                RedDot.SetActive(friendProfile.isAlarm);

            string message = friendProfile.lastMessage;
            if (!string.IsNullOrEmpty(message))
            {
                if (IsCardMessage(message))
                    message = LocalizeData.GetText("LOCALIZE_184");
                LastChat.text = message;
            }

            yield return new WaitForSeconds(2.0f);
        }
    }
    
    public bool IsCardMessage(string message)
    {
        string[] spliter = {
                "[e960cdb67f2cb7488f16347705580180",
                "e960cdb67f2cb7488f16347705580180]"
            };

        string[] checker = message.Split(new string[] { spliter[1] }, System.StringSplitOptions.None);
        
        return checker.Length > 1;
    }
}
