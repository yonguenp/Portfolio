using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text;

public class FriendItem : ScrollUIControllerItem
{
    public FriendPopup.FriendsUI type;
    //common
    public Image RankIcon;
    public Text NickName;
    public Text ClanName;

    //for list
    public Button DeleteButton;
    public GameObject RedDot;
    //for Call
    public Button AcceptButton;
    public Button DuoInviteBtn;

    private FriendsUIObject parentFriendUI;
    private Coroutine updateUserAlarm = null;
    private UserProfile friendProfile = null;

    [SerializeField]
    Text txtOnline = null;
    [SerializeField]
    Text txtLastConnectTime = null;
    [SerializeField]
    GameObject lastConnectInfo = null;

    [SerializeField]
    GameObject waitAccept = null;
    [SerializeField]
    GameObject addFriend = null;

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
        if(RankIcon)
            RankIcon.sprite = RankType.GetRankFromPoint(friend.point).rank_resource;
    
        if(ClanName)
        {
            if (friend.clan_no > 0 && !string.IsNullOrEmpty(friend.clan_name))
            {
                ClanName.gameObject.SetActive(true);
                ClanName.text = friend.clan_name;
            }
            else
            {
                ClanName.gameObject.SetActive(false);
            }
        }
        friend.UIShown(System.Convert.ToUInt32(SBCommonLib.SBUtil.GetCurrentSecTimestamp()));
    }

    //for list
    public void SetFriendListItem(UserProfile friend)
    {
        RedDot.SetActive(Managers.Chat.IsChatNotice(friend.uno));
        SetCommonUI(friend);

        string lastTime = "";
        if(friend.lastLogin >= friend.lastLogout)
        {
            txtOnline.gameObject.SetActive(true);
            lastConnectInfo.SetActive(false);
            DuoInviteBtn.interactable = true;
            foreach (Transform item in DuoInviteBtn.GetComponent<Transform>())
            {
                item.GetComponent<Graphic>().color = DuoInviteBtn.colors.normalColor;
            }
        }
        else
        {
            DuoInviteBtn.interactable = false;
            foreach (Transform item in DuoInviteBtn.GetComponent<Transform>())
            {
                item.GetComponent<Graphic>().color = DuoInviteBtn.colors.disabledColor;
            }
            System.DateTime now = SBCommonLib.SBUtil.KoreanTime;
            if(now < friend.lastLogout)
            {
                lastTime = StringManager.GetString("ui_access_error");
            }
            else
            {
                txtOnline.gameObject.SetActive(false);
                lastConnectInfo.SetActive(true);

                System.TimeSpan diff = now - friend.lastLogout;

                if (diff.TotalHours > 24)
                {
                    lastTime = StringManager.GetString("ui_frlist_time_day", (int)diff.TotalDays);
                    //lastTime = ((int)diff.TotalDays).ToString() + "일전접속";
                }
                else if(diff.TotalMinutes > 60)
                {
                    lastTime = StringManager.GetString("ui_frlist_time_hours", (int)diff.TotalHours);
                    //lastTime = ((int)diff.TotalHours).ToString() + "시간전접속";
                }
                else // if(diff.TotalMinutes > 0)
                {
                    lastTime = StringManager.GetString("ui_frlist_time_min", (int)diff.TotalMinutes);
                    //lastTime = ((int)diff.TotalMinutes).ToString() + "분전접속";
                }
                // else
                //     lastTime = "방금전접속";
                // }

                txtLastConnectTime.text = lastTime;
            }
        }
    }

    public void SetEnableDuoListItem(UserProfile friend)
    {
        SetCommonUI(friend);

    }

    void DeleteFriend()
    {
        WWWForm data = new WWWForm();
        data.AddField("api", "friend");
        data.AddField("op", 9);
        data.AddField("uno", friendProfile.uno.ToString());

        SBWeb.SendPost("friend/friend", data, (response) =>
        {
            parentFriendUI.Invoke("RefreshUI", 0.1f);
            JObject root = (JObject)response;
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

                        (PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.FRIEND_POPUP) as FriendPopup).ShowErrorMessage(rs);
                    }
                }
            }
        });
    }
    
    public void OnDeleteButton()
    {
        var msg =StringManager.GetString("ui_frlist_delet",NickName.text );

        PopupCanvas.Instance.ShowConfirmPopup(msg, () =>
        {
           DeleteFriend();
        });
    }

    public void SetFriendSentItem(UserProfile friend)
    {
        SetCommonUI(friend);
    }

    public void SetFriendRecommandItem(UserProfile friend)
    {
        SetCommonUI(friend);
    }

    public void SetChatUI(UserProfile friend)
    {
        SetCommonUI(friend);
    }

    public void SetFriendSearchItem(UserProfile friend)
    {
        SetCommonUI(friend);
        List<UserProfile> friends = Managers.FriendData.GetFriendList();
        foreach(UserProfile fr in friends)
        {
            if(fr.uno == friend.uno)
            {
                waitAccept.SetActive(false);
                addFriend.SetActive(false);
            }
            else
            {
                waitAccept.SetActive(false);
                addFriend.SetActive(true);
            }
        }

        if(friends.Count >= Managers.FriendData.FRIEND_MAX_COUNT)
        {
            waitAccept.SetActive(false);
            addFriend.SetActive(false);
        }

        if (friendProfile.type == UserProfile.FriendType.SENT)
        {
            waitAccept.SetActive(true);
            addFriend.SetActive(false);
        }
    }

    public void OnSentRequestButton()
    {
        Debug.Log("OnRequestButton");
        WWWForm data = new WWWForm();
        data.AddField("api", "friend");
        data.AddField("op", 5);
        data.AddField("uno", friendProfile.uno.ToString());

        SBWeb.SendPost("friend/friend", data, (response) =>
        {
            //parentFriendUI.Invoke("RefreshUI", 0.1f);
            
            JObject root = (JObject)response;
            JToken resultCode = root["resFriend"];
            if (resultCode != null && resultCode.Type == JTokenType.Integer)
            {
                int rs = resultCode.Value<int>();
                if (rs == 0)
                {
                    Managers.FriendData.AddUserProfile(friendProfile, UserProfile.FriendType.SENT);
                    CompleteRequest();
                }
                else (PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.FRIEND_POPUP) as FriendPopup).ShowErrorMessage(rs);
            }
        });
    }

    void CompleteRequest()
    {
        waitAccept.SetActive(true);
        addFriend.SetActive(false);
    }

    public void OnCancelRequestButton()
    {
        Debug.Log("OnCancelRequestButton");
        WWWForm data = new WWWForm();
        data.AddField("api", "friend");
        data.AddField("op", 6);
        data.AddField("uno", friendProfile.uno.ToString());

        SBWeb.SendPost("friend/friend", data, (response) =>
        {
            parentFriendUI.Invoke("RefreshUI", 0.1f);
            JObject root = (JObject)response;
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
                        (PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.FRIEND_POPUP) as FriendPopup).ShowErrorMessage(rs);
                    }
                }
            }
        });
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
        WWWForm data = new WWWForm();
        data.AddField("api", "friend");
        data.AddField("op", 7);
        data.AddField("uno", friendProfile.uno.ToString());

        SBWeb.SendPost("friend/friend", data, (response) =>
        {
            parentFriendUI.Invoke("RefreshUI", 0.1f);
            JObject root = (JObject)response;
            SBDebug.Log($"OnAcceptButton {root}");
            int rs = -1;
            if (root.ContainsKey("resFriend"))
                rs = root["resFriend"].Value<int>();

            if (rs != 0)
            {
                (PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.FRIEND_POPUP) as FriendPopup).ShowErrorMessage(rs);
            }
            else
            {
                PopupCanvas.Instance.ShowFadeText(StringManager.GetString("msg_fr_accept"));
                Managers.FriendData.AddUserProfile(friendProfile, UserProfile.FriendType.FRIEND);
            }
        });
    }

    public void OnDeclineButton()
    {
        Debug.Log("OnDeclineButton");
        WWWForm data = new WWWForm();
        data.AddField("api", "friend");
        data.AddField("op", 8);
        data.AddField("uno", friendProfile.uno.ToString());

        SBWeb.SendPost("friend/friend", data, (response) =>
        {
            parentFriendUI.Invoke("RefreshUI", 0.1f);
            JObject root = (JObject)response;
            int rs = -1;
            if (root.ContainsKey("rs"))
                rs = root["rs"].Value<int>();

            if (rs != 0)
            {
                (PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.FRIEND_POPUP) as FriendPopup).ShowErrorMessage(rs);
            }
            else
            {
                PopupCanvas.Instance.ShowFadeText(StringManager.GetString("msg_fr_refuse"));
            }
        });
    }

    public void OnPersnalChatOpen()
    {   
        
    }

    public IEnumerator UpdateUserAlarm()
    {
        while(true)
        {
            if(!RedDot.activeSelf)
                RedDot.SetActive(friendProfile.isAlarm);

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

    public void OnChat()
    {
        //(PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.FRIEND_POPUP) as FriendPopup).OnPrivateChat(friendProfile);
        var parent = parentFriendUI as FriendsList;
        parent.OnChat(friendProfile);
        RedDot.SetActive(false);
    }

    public void SetChatNotice()
    {

    }

    public UserProfile FriendProfile()
    {
        return friendProfile;
    }

    public void OnDuoInvite()
    {
        Managers.FriendData.DUO.SendDuoRequest(friendProfile, SBSocketSharedLib.DuoType.NormalDuo);
    }
}
