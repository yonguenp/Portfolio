using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FriendsSearch : FriendsUIObject
{
    public InputField SearchInputField;

    [SerializeField]
    Text txtSearchDesc = null;

    [SerializeField]
    Text txtNoSearch = null;

    override public void RefreshUI()
    {
        CancelInvoke("RefreshUI");

        ClearItems();
        txtSearchDesc.text = StringManager.GetString("ui_fr_recom");
        SearchInputField.text = "";
        txtNoSearch.text = "";

        WWWForm data = new WWWForm();
        data.AddField("api", "friend");
        data.AddField("op", 2);

        SBWeb.SendPost("friend/friend", data, (response) =>
        {
            Invoke("RefreshRecommandList", 0.1f);
            
            JObject root = (JObject)response;
            if (root.ContainsKey("op"))
            {
                SBWeb.Instance.OnResponseFriend(root["op"].Value<int>(), root);
            }
        });
    }

    public void OnFirendSearch()
    {
        ClearItems();
        txtSearchDesc.text = StringManager.GetString("ui_fr_search_result");

        string searchNick = SearchInputField.text;
        if (string.IsNullOrEmpty(searchNick))
        {
            RefreshSentList();
        }
        else
        { 
            WWWForm data = new WWWForm();
            data.AddField("api", "friend");
            data.AddField("op", 4);
            data.AddField("nick", searchNick);

            SBWeb.SendPost("friend/friend", data, (response) =>
            {
                List<UserProfile> searchResult = new List<UserProfile>();

                JObject root = (JObject)response;

                if (root.ContainsKey("user_no"))
                {
                    JObject userObject = root;
                    UserProfile new_user = new UserProfile();
                    new_user.uno = userObject["user_no"].Value<long>();
                    new_user.point = userObject["point"].Value<int>();
                    new_user.nick = userObject["nick"].Value<string>();
                    new_user.type = (UserProfile.FriendType)userObject["state"].Value<int>();
                    if (userObject.ContainsKey("clan_name"))
                        new_user.clan_name = userObject["clan_name"].Value<string>();
                    if (userObject.ContainsKey("clan_no"))
                        new_user.clan_no = userObject["clan_no"].Value<int>();
                    searchResult.Add(new_user);
                }
                RefreshSearchList(searchResult);
            });
        }
    }

    public void RefreshSearchList(List<UserProfile> searchResult)
    {
        ClearItems();

        if (searchResult.Count == 0)
        {
            PopupCanvas.Instance.ShowFadeText(StringManager.GetString("ui_fr_search_fail"));
            txtNoSearch.text = StringManager.GetString("ui_fr_search_fail");
            return;
        }

        foreach (UserProfile result in searchResult)
        {
            FriendItem item = cloneTargetItem.CloneItem(this);
            item.SetFriendSearchItem(result);
        }

        cloneTargetItem.SetActive(false);
    }

    public void RefreshSentList()
    {
        ClearItems();

        List<UserProfile> sents = Managers.FriendData.GetSentList();
        foreach (UserProfile sent in sents)
        {
            FriendItem item = cloneTargetItem.CloneItem(this);
            item.SetFriendSentItem(sent);
        }

        cloneTargetItem.SetActive(false);

        if (sents.Count >= 10)
        {
            return;
        }

        WWWForm data = new WWWForm();
        data.AddField("api", "friend");
        data.AddField("op", 13);

        SBWeb.SendPost("friend/friend", data, (response) =>
        {
            Invoke("RefreshRecommand", 0.1f);
            JObject root = (JObject)response;
            if (root.ContainsKey("op"))
            {
                SBWeb.Instance.OnResponseFriend(root["op"].Value<int>(), root);
            }
        });
    }

    public void OnRefreshButton()
    {
        RefreshUI();
    }

    public void RefreshRecommandList()
    {
        CancelInvoke("RefreshRecommandList");

        WWWForm data = new WWWForm();
        data.AddField("api", "friend");
        data.AddField("op", 13);

        SBWeb.SendPost("friend/friend", data, (response) =>
        {
            Invoke("RefreshRecommand", 0.1f);
            JObject root = (JObject)response;
            if (root.ContainsKey("op"))
            {
                SBWeb.Instance.OnResponseFriend(root["op"].Value<int>(), root);
            }
        });
    }

    public void RefreshRecommand()
    {
        CancelInvoke("RefreshRecommand");
        ClearItems();

        List<UserProfile> sents = Managers.FriendData.GetSentList();
        List<UserProfile> recommands = Managers.FriendData.GetRecommandList();
        int count = 0;//sents.Count + 1;
        int index = 0;
        for(int i = count; i <= 10; i++)
        {
            if (recommands.Count > index)
            {
                FriendItem item = cloneTargetItem.CloneItem(this);
                item.SetFriendRecommandItem(recommands[index++]);
            }
        }

        cloneTargetItem.SetActive(false);
    }

    override public bool IsNewFlag()
    {
        return false;
    }

    override public void NewFlagDone()
    {

    }

    public override void HideHI()
    {
    }
}
