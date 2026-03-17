using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json.Linq;
using System.Text;
using SBSocketSharedLib;

public class DuoList : FriendsUIObject
{
    [SerializeField]
    GameObject duoListEmpty;
    override public void RefreshUI()
    {
        CancelInvoke("RefreshUI");

        ClearItems();

        WWWForm data = new WWWForm();
        data.AddField("api", "friend");
        data.AddField("op", 1);

        SBWeb.SendPost("friend/friend", data, (response) =>
        {
            JObject root = (JObject)response;
            if (root.ContainsKey("op"))
            {
                SBWeb.Instance.OnResponseFriend(root["op"].Value<int>(), root);

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
    }

    public void SetCandidateDuo(IList<FriendInfo> friendInfos)
    {
        //ClearItems();
        bool empty = true;
        List<UserProfile> friends = Managers.FriendData.GetFriendList();
        foreach (UserProfile friend in friends)
        {
            foreach (FriendInfo info in friendInfos)
            {
                if (info.UserState == (byte)UserPlayState.Lobby)
                {
                    if (info.UserNo == friend.uno)
                    {
                        FriendItem item = cloneTargetItem.CloneItem(this);
                        item.SetEnableDuoListItem(friend);
                        empty = false;
                    }
                }
            }
        }

        cloneTargetItem.SetActive(false);

        if (duoListEmpty != null)
            duoListEmpty.SetActive(empty);
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
