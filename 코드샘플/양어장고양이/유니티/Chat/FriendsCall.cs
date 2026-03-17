using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FriendsCall : FriendsUIObject
{

    override public void RefreshUI()
    {
        ClearItems();

        WWWForm data = new WWWForm();
        data.AddField("api", "friend");
        data.AddField("op", 3);

        NetworkManager.GetInstance().SendApiRequest("friend", 3, data, (response) =>
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
                            Invoke("RefreshCallList", 0.1f);
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

    public void RefreshCallList()
    {
        List<UserProfile> takens = FriendsManager.Instance.GetTakenList();
        foreach (UserProfile taken in takens)
        {
            FriendItem item = cloneTargetItem.CloneItem(this);
            item.SetFriendCallItem(taken);
        }
    }

    override public bool IsNewFlag()
    {
        return FriendsManager.Instance.GetNewRecivedCount() > 0;
    }

    override public void NewFlagDone()
    {
        FriendsManager.Instance.SetNewRecivedCount(0);
    }
}
