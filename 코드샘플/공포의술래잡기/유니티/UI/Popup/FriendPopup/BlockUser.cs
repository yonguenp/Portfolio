using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text;

public class BlockUser : FriendsUIObject
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
        data.AddField("op", 2);

        SBWeb.SendPost("friend/friend", data, (response) =>
        {
            Invoke("RefreshSentList", 0.1f);
            
            JObject root = (JObject)response;
            if (root.ContainsKey("op"))
            {
                SBWeb.Instance.OnResponseFriend(root["op"].Value<int>(), root);
            }
        });
    }

    override public bool IsNewFlag()
    {
        return Managers.FriendData.GetNewRecivedCount() > 0;
    }

    override public void NewFlagDone()
    {
        Managers.FriendData.SetNewRecivedCount(0);
    }

    public void RefreshSentList()
    {
        CancelInvoke("RefreshSentList");

        ClearItems();
        List<UserProfile> sents = Managers.FriendData.GetSentList();
        foreach (UserProfile sent in sents)
        {
            var item = cloneTargetItem.CloneItem(this);
            item.SetFriendSentItem(sent);
        }
        cloneTargetItem.SetActive(false);
        //txtTile.text = new StringBuilder().AppendFormat("({0}/10)",sents.Count).ToString();
        txtTile.text="";

        if(sents.Count == 0)
            txtNoSearch.text = StringManager.GetString("ui_nolist");

        if(sents.Count >= 10)
        {
            return;
        }
    }

    public override void HideHI()
    {
    }
}
