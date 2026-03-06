using Newtonsoft.Json.Linq;
using SandboxNetwork;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class GuildAuthorityListObj : MonoBehaviour
{
    
    [SerializeField]
    Transform guildAuthorityListTr;

    [SerializeField]
    ScrollRect scrollView;

    VoidDelegate callBack;

    string UserNo = "";
    string UserNick = "";
    public void SetData(Transform targetTr, string userNo, string userNick, VoidDelegate cb)
    {
        guildAuthorityListTr.transform.position = new Vector2(guildAuthorityListTr.transform.position.x, targetTr.transform.position.y);
        scrollView.enabled =false;
        UserNo = userNo;
        if(cb != null) 
            callBack = cb;
    }

    public void OnClickMakeLeader()
    {
        if (GuildManager.Instance.MyData.GuildPosition == eGuildPosition.Leader)
        {
            SystemPopup.OnSystemPopup(StringData.GetStringByStrKey("알림"), StringData.GetStringFormatByStrKey("guild_desc:74", UserNick), () =>
            {
                WWWForm form = new WWWForm();
                form.AddField("gno", GuildManager.Instance.GuildID);
                form.AddField("tuno", UserNo);
                form.AddField("member_type", (int)eGuildPosition.Leader);
                GuildManager.Instance.NetworkSend("guild/changemembergrade", form, () =>
                {
                    callBack?.Invoke();
                    scrollView.enabled = true;
                });
            }, () => { }, () => { });
        }
        else
        {
            ToastManager.On(StringData.GetStringByStrKey("guild_desc:104"));
        }
    }

    public void OnClickOperator()
    {
        if (GuildManager.Instance.MyGuildInfo.GuildUserDictionary[long.Parse(UserNo)].GuildPosition == eGuildPosition.Leader)
        {
            ToastManager.On(StringData.GetStringByStrKey("guild_desc:104"));
            return;
        }

        if (GuildManager.Instance.IsChangeUserTypeAble)
        {
            WWWForm form = new WWWForm();
            form.AddField("gno", GuildManager.Instance.GuildID);
            form.AddField("tuno", UserNo);
            form.AddField("member_type", (int)eGuildPosition.Operator);
            GuildManager.Instance.NetworkSend("guild/changemembergrade", form, () =>
            {
                callBack?.Invoke();
                scrollView.enabled = true;
            });
        }
        else
        {
            ToastManager.On(StringData.GetStringByStrKey("guild_desc:104"));
        }
    }

    /// <summary>
    /// 일반 길드원으로 만들기
    /// </summary>
    public void OnClickNormal()
    {
        if (GuildManager.Instance.MyGuildInfo.GuildUserDictionary[long.Parse(UserNo)].GuildPosition == eGuildPosition.Leader)
        {
            ToastManager.On(StringData.GetStringByStrKey("guild_desc:104"));
            return;
        }

        if (GuildManager.Instance.IsChangeUserTypeAble)
        {
            WWWForm form = new WWWForm();
            form.AddField("gno", GuildManager.Instance.GuildID);
            form.AddField("tuno", UserNo);
            form.AddField("member_type", (int)eGuildPosition.Normal);
            GuildManager.Instance.NetworkSend("guild/changemembergrade", form, () =>
            {
                callBack?.Invoke();
                scrollView.enabled = true;
            });
        }
        else
        {
            ToastManager.On(StringData.GetStringByStrKey("guild_desc:104"));
        }
    }

    public void OnClickCancel()
    {
        callBack?.Invoke();
        scrollView.enabled = true;
    }
}
