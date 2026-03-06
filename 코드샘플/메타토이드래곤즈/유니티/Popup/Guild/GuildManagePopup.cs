using Newtonsoft.Json.Linq;
using SandboxNetwork;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GuildManagePopup : Popup<PopupData>
{

    [SerializeField]
    private Text guildDestroyBtnText = null;
    [SerializeField]
    private GameObject ApplyListReddot;
    [SerializeField]
    private GuildMarkObject grayMark;


    bool isDestroying = false;
    eGuildPosition MyPosition = eGuildPosition.None;
    public override void InitUI()
    {
        isDestroying = GuildManager.Instance.IsDestroying;
        MyPosition = GuildManager.Instance.MyData.GuildPosition;
        guildDestroyBtnText.text =  StringData.GetStringByStrKey(isDestroying ? "guild_desc:94" : "guild_desc:63");
        grayMark.SetGuildMark(GuildManager.Instance.MyGuildInfo.GetGuildEmblem(),GuildManager.Instance.MyGuildInfo.GetGuildMark());
        SetReddot(GuildManager.Instance.IsManageJoinAble);
    }
    void SetReddot(bool isOperatorManageJoin) // 길드 운영진이 조합 가입 승인 권한을 가졌는가?
    {
        ApplyListReddot.SetActive(false);

        if (isOperatorManageJoin)
        {
            if (GuildManager.Instance.JoinApplyToMyGuildList.Count > 0)
            {
                ApplyListReddot.SetActive(true);
            }
        }
    }
    public void OnClickOperatorPermissionSet()
    {
        if (MyPosition == eGuildPosition.Leader)
        {
            PopupManager.OpenPopup<GuildOperatorPermissionPopup>();
        }
        else
        {
            ToastManager.On(StringData.GetStringByStrKey("guild_desc:104"));
        }
            
    }

    public void OnClickNameChange()
    {
        var date = GuildManager.Instance.NextNameChangeTimeStamp;
        if (date >= TimeManager.GetTime())
        {
            var remainTime = TimeManager.GetTimeCompare((int)date);
            SystemPopup.OnSystemPopup(StringData.GetStringByStrKey("알림"), StringData.GetStringFormatByStrKey("guild_desc:101", SBFunc.TimeCustomString(remainTime, true, true, true, false, true)), true, false, true);
            return;
        }
        PopupManager.OpenPopup<GuildNameChangePopup>();
    }

    public void OnClickFlagChange()
    {
        var date = GuildManager.Instance.NextEmblemChangeTimeStamp;
        if(date >= TimeManager.GetTime()) 
        { 
            var remainTime = TimeManager.GetTimeCompare((int)date);
            SystemPopup.OnSystemPopup(StringData.GetStringByStrKey("알림"), StringData.GetStringFormatByStrKey("guild_desc:100", SBFunc.TimeCustomString(remainTime, true, true, true, false, true)), true, false, true);
            return;
        }
        PopupManager.OpenPopup<GuildEmblemChangePopup>();   
        
    }

    public void OnClickChangeJoinCondition()
    {
        PopupManager.OpenPopup<GuildMarketingChangePopup>();   
    }

    public void OnClickShowApplyList()
    {
        if (GuildManager.Instance.IsManageJoinAble)
            PopupManager.OpenPopup<GuildApplyListPopup>();
        else
        {
            // to do . 불가 알림
            ToastManager.On(StringData.GetStringByStrKey("guild_desc:104"));
        }
    }

    public void OnClickDestroyGuild()
    {
        if (MyPosition == eGuildPosition.Leader)
        {
            if (isDestroying)
            {
                if(GuildManager.Instance.DestroyAbleTimeStamp> TimeManager.GetTime())
                {
                    var dateTime = SBFunc.TimeStampToDateTime(GuildManager.Instance.MyGuildInfo.GuildTimeData.DissolutionCancelDate);
                    SystemPopup.OnSystemPopup(StringData.GetStringByStrKey("알림"), StringData.GetStringFormatByStrKey("guild_desc:106", dateTime),true,false,true);
                }
                else
                {
                    WWWForm form = new WWWForm();
                    form.AddField("gno", GuildManager.Instance.GuildID);
                    GuildManager.Instance.NetworkSend("guild/closecancel", form, () =>
                    {
                        PopupManager.ClosePopup<GuildManagePopup>();
                        GuildEvent.RefreshGuildUI(GuildEvent.eGuildEventType.GuildRefresh);
                    });
                }
            }
            else
            {
                var guildDetailData = GuildManager.Instance.MyGuildInfo;
                if (guildDetailData.GuildUserList.Count == 1)
                {
                    SystemPopup.OnSystemPopup(StringData.GetStringByStrKey("알림"), StringData.GetStringFormatByStrKey("guild_desc:99", guildDetailData.GetGuildName()), () =>
                    {
                        WWWForm form = new WWWForm();
                        form.AddField("gno", GuildManager.Instance.GuildID);
                        GuildManager.Instance.NetworkSend("guild/close", form, () =>
                        {
                            PopupManager.ClosePopup<GuildManagePopup>();
                            PopupManager.ClosePopup<GuildInfoPopup>();
                        });
                    });
                    PopupManager.GetPopup<SystemPopup>().SetButtonState(true, true, true);
                    return;
                }
                else
                {
                    if (GuildManager.Instance.DestroyAbleTimeStamp > TimeManager.GetTime())
                    {
                        var dateTime = SBFunc.TimeStampToDateTime(GuildManager.Instance.MyGuildInfo.GuildTimeData.DissolutionCancelDate);
                        SystemPopup.OnSystemPopup(StringData.GetStringByStrKey("알림"), StringData.GetStringFormatByStrKey("guild_desc:106", dateTime), true, false, true);
                    }
                    else
                    {
                        PopupManager.OpenPopup<GuildDestroyPopup>();
                    }
                }
            }
        }
        else
        {
            ToastManager.On(StringData.GetStringByStrKey("guild_desc:104"));
        }
    }
}
