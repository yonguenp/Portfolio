using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FriendsSearch : FriendsUIObject
{
    public InputField SearchInputField;
    public Button SearchButton;

    override public void RefreshUI()
    {
        ClearItems();

        SearchInputField.text = "";

        WWWForm data = new WWWForm();
        data.AddField("api", "friend");
        data.AddField("op", 2);

        NetworkManager.GetInstance().SendApiRequest("friend", 2, data, (response) =>
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
                            Invoke("RefreshSentList", 0.1f);                            
                        }
                        else
                        {
                            if (rs == 4)
                                NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("FRIEND_FULL"));
                            else
                                NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(LocalizeData.GetText("LOCALIZE_332"), LocalizeData.GetText("LOCALIZE_344"));
                        }
                    }
                }
            }
        }, null, false);
    }

    public void OnFirendSearch()
    {
        ClearItems();

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

            NetworkManager.GetInstance().SendApiRequest("friend", 4, data, (response) =>
            {
                List<UserProfile> searchResult = new List<UserProfile>();

                JObject root = JObject.Parse(response);
                JToken apiToken = root["api"];
                if (null != apiToken && apiToken.Type == JTokenType.Array
                    && apiToken.HasValues)
                {
                    JArray apiArr = (JArray)apiToken;
                    foreach (JObject row in apiArr)
                    {
                        string uri = row.GetValue("uri").ToString();
                        if (uri == "friend")
                        {
                            JObject userObject = (JObject)row["user"];
                            if (userObject != null)
                            {
                                UserProfile new_user = new UserProfile();
                                new_user.uno = userObject["uno"].Value<uint>();
                                new_user.level = userObject["lvl"].Value<uint>();
                                new_user.nick = userObject["nick"].Value<string>();
                                new_user.type = (UserProfile.FriendType)userObject["state"].Value<int>();
                                searchResult.Add(new_user);
                            }
                        }
                    }
                }

                RefreshSearchList(searchResult);
            }, null, false);
        }
    }

    public void RefreshSearchList(List<UserProfile> searchResult)
    {
        if(searchResult.Count == 0)
        {
            FriendsPanel friendsPanel = transform.GetComponentInParent<FriendsPanel>();
            if(friendsPanel != null)
            {
                NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("LOCALIZE_185"));
            }
            return;
        }

        foreach (UserProfile result in searchResult)
        {
            FriendItem item = cloneTargetItem.CloneItem(this);
            item.SetFriendSearchItem(result);
        }
    }

    public void RefreshSentList()
    {
        List<UserProfile> sents = FriendsManager.Instance.GetSentList();
        foreach (UserProfile sent in sents)
        {
            FriendItem item = cloneTargetItem.CloneItem(this);
            item.SetFriendSentItem(sent);
        }

        if(sents.Count >= 10)
        {
            return;
        }

        WWWForm data = new WWWForm();
        data.AddField("api", "friend");
        data.AddField("op", 13);

        NetworkManager.GetInstance().SendApiRequest("friend", 13, data, (response) =>
        {
            Invoke("RefreshRecommand", 0.1f);
        }, null, false);
    }

    public void RefreshRecommand()
    {
        List<UserProfile> sents = FriendsManager.Instance.GetSentList();
        List<UserProfile> recommands = FriendsManager.Instance.GetRecommandList();
        int count = sents.Count;
        int index = 0;
        for(int i = count; i <= 10; i++)
        {
            if (recommands.Count > index)
            {
                FriendItem item = cloneTargetItem.CloneItem(this);
                item.SetFriendRecommandItem(recommands[index++]);
            }
        }
    }

    override public bool IsNewFlag()
    {
        return false;
    }

    override public void NewFlagDone()
    {

    }
}
