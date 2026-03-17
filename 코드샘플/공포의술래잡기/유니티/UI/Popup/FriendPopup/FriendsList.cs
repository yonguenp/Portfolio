using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json.Linq;
using System.Text;
using SBSocketSharedLib;

public class FriendsList : FriendsUIObject
{
    public Text FriendCount;

    [SerializeField]
    Text txtNoSearch = null;

    [SerializeField]
    PrivateChat uiChat = null;

    [SerializeField]
    List<FriendItem> itemList = new List<FriendItem>();

    override public void RefreshUI()
    {
        CancelInvoke("RefreshUI");

        ClearItems();

        Managers.Chat.SetAddMessageCallback(typeof(FriendsList), ReceiveMessage);

        txtNoSearch.text = "";

        WWWForm data = new WWWForm();
        data.AddField("api", "friend");
        data.AddField("op", 1);



        var scene = Managers.Scene.CurrentScene as LobbyScene;

        SBWeb.SendPost("friend/friend", data, (response) =>
        {
            //Invoke("RefreshFriendList", 0.1f);
            JObject root = (JObject)response;
            if (root.ContainsKey("op"))
            {
                SBWeb.Instance.OnResponseFriend(root["op"].Value<int>(), root);

                RefreshFriendList();
                List<UserProfile> friends = Managers.FriendData.GetFriendList();
                List<long> users = new List<long>();
                foreach (UserProfile profile in friends)
                {
                    if (profile.IsLoginUser())
                    {
                        users.Add((long)profile.uno);
                    }
                }

                if (users.Count > 0)
                {
                    Managers.Network.SendDuoList(users);
                }
            }
        });

        ShowChatUI(false);
    }

    public void RefreshFriendList()
    {
        CancelInvoke("RefreshFriendList");

        ClearItems();
        List<UserProfile> friends = Managers.FriendData.GetFriendList();
        itemList.Clear();
        foreach (UserProfile friend in friends)
        {
            if (!friend.IsLoginUser())
                continue;

            FriendItem item = cloneTargetItem.CloneItem(this);
            item.SetFriendListItem(friend);
            itemList.Add(item);
        }

        foreach (UserProfile friend in friends)
        {
            if (friend.IsLoginUser())
                continue;

            FriendItem item = cloneTargetItem.CloneItem(this);
            item.SetFriendListItem(friend);
            itemList.Add(item);
        }

        cloneTargetItem.SetActive(false);

        FriendCount.text = new StringBuilder().AppendFormat("({0}/{1})", friends.Count, Managers.FriendData.FRIEND_MAX_COUNT.ToString()).ToString();

        if (friends.Count == 0)
            txtNoSearch.text = StringManager.GetString("ui_nofriend");
    }

    public void SetCandidateDuo(IList<FriendInfo> friendInfos)
    {
        //ClearItems();
        if (itemList == null || itemList.Count < 0)
            return;
        var scene = Managers.Scene.CurrentScene as LobbyScene;

        List<UserProfile> friends = Managers.FriendData.GetFriendList();
        foreach (UserProfile friend in friends)
        {
            int idx = 0;
            foreach (FriendInfo info in friendInfos)
            {
                if (info.UserState == (byte)UserPlayState.Lobby)
                {
                    if (info.UserNo == friend.uno)
                    {
                        if (scene != null && !scene.SetEnableSendDuo())
                        {
                            itemList[idx].DuoInviteBtn.interactable = false;
                            foreach (Transform item in itemList[idx].DuoInviteBtn.GetComponent<Transform>())
                            {
                                item.GetComponent<Graphic>().color = itemList[idx].DuoInviteBtn.colors.disabledColor;
                            }
                        }
                        else
                        {
                            itemList[idx].DuoInviteBtn.interactable = true;
                            foreach (Transform item in itemList[idx].DuoInviteBtn.GetComponent<Transform>())
                            {
                                item.GetComponent<Graphic>().color = itemList[idx].DuoInviteBtn.colors.normalColor;
                            }
                        }

                        //FriendItem item = cloneTargetItem.CloneItem(this);
                        //item.SetEnableDuoListItem(friend);
                    }
                }
                else
                {
                    if (info.UserNo == friend.uno)
                    {
                        itemList[idx].DuoInviteBtn.interactable = false;
                        foreach (Transform item in itemList[idx].DuoInviteBtn.GetComponent<Transform>())
                        {
                            item.GetComponent<Graphic>().color = itemList[idx].DuoInviteBtn.colors.disabledColor;
                        }
                    }
                }
                idx++;
            }
        }
        //cloneTargetItem.SetActive(false);
    }

    override public bool IsNewFlag()
    {
        return Managers.Chat.IsChatNotice() || Managers.FriendData.GetNewFriendCount() > 0;
    }

    override public void NewFlagDone()
    {
        Managers.FriendData.SetNewFriendCount(0);
    }

    public void OnCloseChatUI()
    {
        uiChat.ClearData();
        uiChat.gameObject.SetActive(false);
        RefreshFriendAlarm();
    }

    public void ShowChatUI(bool isShow)
    {
        uiChat.gameObject.SetActive(isShow);
    }

    public void OnChat(UserProfile userdata)
    {
        ShowChatUI(true);
        uiChat.SetFriend(userdata);

        Managers.Chat.ReadMessage(userdata.uno);
        FriendPanel.OnFriendAlarmCheck();
    }

    public void RefreshFriendAlarm()
    {
        foreach (Transform t in itemContainer)
        {
            var item = t.gameObject.GetComponent<FriendItem>();
            var userProfile = item.FriendProfile();
            if (userProfile == null) continue;
            bool isNotice = Managers.Chat.IsChatNotice(userProfile.uno);
            item.RedDot.gameObject.SetActive(isNotice);
        }
    }

    public void ReceiveMessage(sChatData chatData)
    {
        if (chatData.Type == eChatType.Send) return;
        foreach (Transform t in itemContainer)
        {
            var item = t.gameObject.GetComponent<FriendItem>();
            var userProfile = item.FriendProfile();
            if (userProfile == null) continue;
            if (chatData.ChatId == userProfile.uno)
                item.RedDot.gameObject.SetActive(true);
        }
    }

    void OnEnable()
    {

    }

    void OnDisable()
    {

    }

    public override void HideHI()
    {
        Managers.Chat.RemoveAddMessageCallback(typeof(FriendsList));
        uiChat.ClearData();
    }
}
