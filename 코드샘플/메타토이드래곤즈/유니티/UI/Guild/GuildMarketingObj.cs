using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace SandboxNetwork { 

    //public class GuildBaseInfo
    //{
    //    public int Tag { get; private set; }
    //    public string Name { get; private set; }
    //    public string Info { get; private set; }
    //    public string Flag { get; private set; }
    //    public string Mark { get; private set; }
    //    public int Lv { get; private set; }
    //    public int CurrentUserAmount { get; private set; }
    //    public int MaxUserAmount { get; private set; }
    //    public eGuildMarketingType MarketingType { get; private set; }
    //    public int GuildRank { get; private set; }
    //    public int GuildExp { get; private set; }
    //    public GuildBaseInfo(int guildTag,string name, string info, string flag, string mark, int lv, int curUserNum, int maxUserNum)
    //    {
    //        Tag = guildTag;
    //        Name = name;
    //        Info = info;
    //        Flag = flag;
    //        Mark = mark;
    //        Lv = lv;
    //        CurrentUserAmount = curUserNum;
    //        MaxUserAmount = maxUserNum;
    //    }
    //    public GuildBaseInfo(int guildTag, string name, string info, string flag, string mark, int lv, int curUserNum, int maxUserNum, eGuildMarketingType guildMarketingType)
    //    {
    //        Tag = guildTag;
    //        Name = name;
    //        Info = info;
    //        Flag = flag;
    //        Mark = mark;
    //        Lv = lv;
    //        CurrentUserAmount = curUserNum;
    //        MaxUserAmount = maxUserNum;
    //        MarketingType = guildMarketingType;
    //    }
    //    public GuildBaseInfo(int guildTag, string name, string info, string flag, string mark, int lv, int curUserNum, int maxUserNum, int rank, int exp)
    //    {
    //        Tag = guildTag;
    //        Name = name;
    //        Info = info;
    //        Flag = flag;
    //        Mark = mark;
    //        Lv = lv;
    //        CurrentUserAmount = curUserNum;
    //        MaxUserAmount = maxUserNum;
    //        GuildRank = rank;
    //        GuildExp = exp;
    //    }
    //}




    public class GuildMarketingObj : MonoBehaviour
    {
        [SerializeField]
        Text guildRankText;
        [SerializeField]
        GuildMarkObject guildMarkObject;
        [SerializeField]
        Text lvText;
        [SerializeField]
        Text guildNameText;
        [SerializeField]
        Text guidlLeaderNameText;
        [SerializeField]
        Text guildInfoText;
        [SerializeField]
        Text guildUserNumText = null;
        [SerializeField]
        GameObject guildJoinBtn = null;
        [SerializeField]
        GameObject guildApplyBtn = null;
        [SerializeField]
        GameObject guildCancelApplyBtn = null;


        public GuildInfoData guildMarketingInfo { get; private set; }  = null;
        VoidDelegate ClickCallBack = null;
        //public void Init(int id, string name, string info, int emblem, int mark, int lv, int curUserNum,int maxUserNum, eGuildJoinType type)
        //{
        //    guildMarketingInfo = new GuildInfoData(id,name, emblem,mark,0,lv,info,curUserNum, maxUserNum, type);
        //    SetInfo();
        //}

        public void Init(GuildInfoData guildInformation)
        {
            guildMarketingInfo = guildInformation;
            SetInfo();
        }

        void SetInfo()
        {
            if (guildMarketingInfo.GuildRank > 0)
                guildRankText.text = SBFunc.GetRankText(guildMarketingInfo.GuildRank);
            else
                guildRankText.text = "-";
            guildMarkObject.SetGuildMark(guildMarketingInfo.GuildEmblem, guildMarketingInfo.GuildMark);
            guildNameText.text = guildMarketingInfo.GuildName;
            guidlLeaderNameText.text = guildMarketingInfo.GetGuildLeaderNick();
            lvText.text = StringData.GetStringFormatByStrKey("user_info_lv_02", guildMarketingInfo.GuildLv);
            guildInfoText.text = guildMarketingInfo.GuildDesc;
            guildUserNumText.text = string.Format("{0}/{1}", guildMarketingInfo.GuildPeopleCount, guildMarketingInfo.GuildMaxPeopleCount);
            SetAllBtnOff();

            switch (guildMarketingInfo.GuildJoinType)
            {
                
                case eGuildJoinType.JoinRightNow:
                    guildJoinBtn.SetActive(true);
                    break;
                case eGuildJoinType.JoinWait:                    
                    if (GuildManager.Instance.ReqGuildList.ContainsKey(guildMarketingInfo.GuildID))
                    {
                        guildCancelApplyBtn.SetActive(true);
                    }
                    else
                    {
                        guildApplyBtn.SetActive(true);
                    }
                    break;
            }
        }

        public void SetCallBack(VoidDelegate clickCallBack)
        {
            if(clickCallBack != null)
            {
                ClickCallBack = clickCallBack;
            }
        }


        public void SetAppliedMode()
        {
            SetAllBtnOff();
            guildCancelApplyBtn.SetActive(true);
        }

        void SetAllBtnOff()
        {
            guildJoinBtn.SetActive(false);
            guildApplyBtn.SetActive(false);
            guildCancelApplyBtn.SetActive(false);
        }

        public void OnClickJoin()
        {
            int guildId = guildMarketingInfo.GuildID;
            if (GuildManager.Instance.NextRejoinGuildTimeStamp >= TimeManager.GetTime())
            {
                if (guildId == GuildManager.Instance.LastGuildID)
                {
                    SystemPopup.OnSystemPopup(StringData.GetStringByStrKey("알림"), StringData.GetStringByStrKey("guild_desc:102"), true, false, true);
                    return;
                }
                else
                {
                    SystemPopup.OnSystemPopup(StringData.GetStringByStrKey("알림"), StringData.GetStringByStrKey("guild_errorcode_10"), true, false, true);
                    return;
                }
            }


            SystemPopup.OnSystemPopup(StringData.GetStringByStrKey("guild_desc:15"), StringData.GetStringByStrKey("guild_desc:16"), StringData.GetStringByStrKey("확인"), StringData.GetStringByStrKey("취소"),
                () =>
                {
                    WWWForm form = new WWWForm();
                    form.AddField("gno", guildId);
                    GuildManager.Instance.NetworkSend("guild/joinrequest", form, () =>
                    {
                        ClickCallBack?.Invoke();
                        PopupManager.GetPopup<GuildStartPopup>().ForceClose();
                        PopupManager.ClosePopup<GuildJoinPopup>();
                        PopupManager.OpenPopup<GuildInfoPopup>();
                    });
                }
                , () =>
                {

                },
                () =>
                {

                });
        }

        public void OnClickApply()
        {
            int guildId = guildMarketingInfo.GuildID;
            if (GuildManager.Instance.NextRejoinGuildTimeStamp >= TimeManager.GetTime())
            {
                if (guildId == GuildManager.Instance.LastGuildID)
                {
                    SystemPopup.OnSystemPopup(StringData.GetStringByStrKey("알림"), StringData.GetStringByStrKey("guild_desc:102"), true, false, true);
                    return;
                }
                else
                {
                    SystemPopup.OnSystemPopup(StringData.GetStringByStrKey("알림"), StringData.GetStringByStrKey("guild_errorcode_10"), true, false, true);
                    return;
                }
            }

            if (GuildManager.Instance.ReqGuildList.ContainsKey(guildMarketingInfo.GuildID))
            {
                ToastManager.On(StringData.GetStringByStrKey("guild_errorcode_2"));
                return;
            }
            SystemPopup.OnSystemPopup(StringData.GetStringByStrKey("guild_desc:15"), StringData.GetStringByStrKey("guild_desc:16"), StringData.GetStringByStrKey("확인"), StringData.GetStringByStrKey("취소"),
                () =>
                {
                    WWWForm form = new WWWForm();
                    form.AddField("gno", guildId);
                    GuildManager.Instance.NetworkSend("guild/joinrequest", form, () =>
                    {
                        ClickCallBack?.Invoke();
                    });
                }, () =>
                {

                }
                , () =>
                {

                });
        }

        public void OnClickCancelApply()
        {
            SystemPopup.OnSystemPopup(StringData.GetStringByStrKey("알림"), StringData.GetStringByStrKey("guild_desc:96"), StringData.GetStringByStrKey("확인"), StringData.GetStringByStrKey("취소"),
                () =>
                {
                    WWWForm form = new WWWForm();
                    form.AddField("gno", guildMarketingInfo.GuildID);
                    GuildManager.Instance.NetworkSend("guild/joincancel", form, () =>
                    {
                        GuildManager.Instance.RemoveReqGuildData(guildMarketingInfo.GuildID);
                        ClickCallBack?.Invoke();
                    });
                },
                () =>
                {

                },
                () =>
                {

                });
        }


    }
}