using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text;

public class FriendsCall : FriendsUIObject
{
    [SerializeField]
    Text txtTile = null;
    [SerializeField]
    Text txtNoSearch = null;
    override public void RefreshUI()
    {
        CancelInvoke("RefreshUI");

        ClearItems();
        txtNoSearch.text = "";

        WWWForm data = new WWWForm();
        data.AddField("api", "friend");
        data.AddField("op", 3);

        SBWeb.SendPost("friend/friend", data, (response) =>
        {
            Invoke("RefreshCallList", 0.1f);
            JObject root = (JObject)response;
            if (root.ContainsKey("op"))
            {
                SBWeb.Instance.OnResponseFriend(root["op"].Value<int>(), root);
            }
        });
    }

    public void RefreshCallList()
    {
        CancelInvoke("RefreshCallList");
        ClearItems();
        List<UserProfile> takens = Managers.FriendData.GetTakenList();
        foreach (UserProfile taken in takens)
        {
            FriendItem item = cloneTargetItem.CloneItem(this);
            item.SetFriendCallItem(taken);
        }

        cloneTargetItem.SetActive(false);
        //txtTile.text = new StringBuilder().AppendFormat("({0}/{1})",takens.Count, Managers.FriendData.FRIEND_MAX_COUNT.ToString()).ToString();
        txtTile.text = "";
        if(takens.Count == 0)
            txtNoSearch.text = StringManager.GetString("ui_nolist");
    }

    override public bool IsNewFlag()
    {
        //return Managers.FriendData.GetNewRecivedCount() > 0;
        return Managers.FriendData.CheckRequestFriendNotice();
    }

    override public void NewFlagDone()
    {
        Managers.FriendData.SetNewRecivedCount(0);
    }

    public override void HideHI()
    {
    }
}
