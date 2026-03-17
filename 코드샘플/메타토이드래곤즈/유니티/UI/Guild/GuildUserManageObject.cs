using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace SandboxNetwork { 


    public class GuildUserManageObject : MonoBehaviour, EventListener<FriendEvent>
    {
        [SerializeField]
        GameObject normalBack;
        [SerializeField]
        Image userRankImg;
        [SerializeField]
        GameObject myInfoBack;
        [SerializeField]
        Text rankText;
        [SerializeField]
        UserPortraitFrame userFrame;
        [SerializeField]
        Text nickText;
        [SerializeField]
        Text positionText;
        [SerializeField]
        Text lastActiveText;
        [SerializeField]
        Text lvText;
        [SerializeField]
        Text contributeText;

        [SerializeField]
        Image arenaRankIcon;

        [SerializeField]
        GameObject defaultLayer;
        [SerializeField]
        GameObject manageLayer;
        [SerializeField]
        Transform AuthLayerTr;
        [SerializeField]
        GuildAuthorityListObj changeAuthorityLayer;

        [Space(10)]
        [Header("buttons")]
        [SerializeField] Button requestFriendBtn = null;
        [SerializeField] Button whisperBtn = null;


        public eGuildManageMode manageMode { get; private set; } = eGuildManageMode.Default;

        eGuildManageMode lastManageMode = eGuildManageMode.Default;

        eGuildPosition myGuildPosition= eGuildPosition.None;
        public long userNo { get; private set; } = 0;
        public long myUserNo { get { return User.Instance.UserAccountData.UserNumber; } }

        public bool CanRequest { get; private set; } = false;

        string userNick = "";
        VoidDelegate changeCallBack;

        GuildUserData data = null;

        private void OnEnable()
        {
            this.EventStart();
        }
        private void OnDisable()
        {
            this.EventStop();
        }
        public void Init(GuildUserManageLayer.GUILDUSERMENU curMenu, GuildUserData userData, eGuildRankType rankType, VoidDelegate changeCB)
        {
            if(userData == null)
            {
                Debug.LogError(">>>> guildUserData null");
                return;
            }

            data = userData;
            myGuildPosition = GuildManager.Instance.MyData.GuildPosition;
            normalBack.SetActive(true);
            myInfoBack.SetActive(false);

            userNo = data.UID;
            changeCallBack = changeCB;
            userFrame.SetUserPortraitFrame(data);
            userNick = data.Nick;
            nickText.text = userNick;
            positionText.text = data.GuildPosition switch
            {
                eGuildPosition.Leader => StringData.GetStringByStrKey("guild_desc:23"),
                eGuildPosition.Operator => StringData.GetStringByStrKey("guild_desc:113"),
                eGuildPosition.Normal => StringData.GetStringByStrKey("guild_desc:28"),
                _ => ""
            };
            lastActiveText.gameObject.SetActive(userNo != myUserNo);
            string timeDistStr = data.LastActiveTime > 0 ? SBFunc.TimeStampDeepString(data.LastActiveTime) : ""; // 접속중 표시에 나타낼 수도 있을까봐 빼 둠
            lastActiveText.text = StringData.GetStringFormatByStrKey("마지막접속시간", timeDistStr);

            lvText.text = StringData.GetStringFormatByStrKey("user_info_lv_02", data.Level);
            
            switch (rankType)
            {
                case eGuildRankType.SumRanking:                    
                    rankText.text = data.SumContribution > 0 ? SBFunc.GetRankText(data.Rank) : "-";
                    break;
                case eGuildRankType.WeeklyRanking:
                    rankText.text = data.WeekContribution > 0 ? SBFunc.GetRankText(data.WeekRank) : "-";
                    break;
                case eGuildRankType.MonthlyRanking:
                    rankText.text = data.MonthlyContribution > 0 ? SBFunc.GetRankText(data.MonthRank) : "-";
                    break;
            }

            switch (curMenu)
            {
                case GuildUserManageLayer.GUILDUSERMENU.SUM_TOTAL:
                    contributeText.text = SBFunc.CommaFromNumber(data.TotalPoint[(int)rankType]).ToString();
                    break;
                case GuildUserManageLayer.GUILDUSERMENU.SUM_ARENA:
                    contributeText.text = SBFunc.CommaFromNumber(data.ArenaPoint[(int)rankType]).ToString();
                    break;
                case GuildUserManageLayer.GUILDUSERMENU.SUM_RAID:
                    contributeText.text = SBFunc.CommaFromNumber(data.RaidPoint[(int)rankType]).ToString();
                    break;
                case GuildUserManageLayer.GUILDUSERMENU.SUM_EXP:
                    contributeText.text = SBFunc.CommaFromNumber(data.ExpPoint[(int)rankType]).ToString();
                    break;
                case GuildUserManageLayer.GUILDUSERMENU.SUM_MAGNET:
                    contributeText.text = SBFunc.CommaFromNumber(data.MagnetPoint[(int)rankType]).ToString();
                    break;
                case GuildUserManageLayer.GUILDUSERMENU.WEEK_TOTAL:
                    contributeText.text = SBFunc.CommaFromNumber(data.TotalPoint[(int)rankType]).ToString();
                    break;
                case GuildUserManageLayer.GUILDUSERMENU.WEEK_ARENA:
                    contributeText.text = SBFunc.CommaFromNumber(data.ArenaPoint[(int)rankType]).ToString();
                    break;
                case GuildUserManageLayer.GUILDUSERMENU.WEEK_RAID:
                    contributeText.text = SBFunc.CommaFromNumber(data.RaidPoint[(int)rankType]).ToString();
                    break;
                case GuildUserManageLayer.GUILDUSERMENU.WEEK_EXP:
                    contributeText.text = SBFunc.CommaFromNumber(data.ExpPoint[(int)rankType]).ToString();
                    break;
                case GuildUserManageLayer.GUILDUSERMENU.WEEK_MAGNET:
                    contributeText.text = SBFunc.CommaFromNumber(data.MagnetPoint[(int)rankType]).ToString();
                    break;
                case GuildUserManageLayer.GUILDUSERMENU.MONTH_TOTAL:
                    contributeText.text = SBFunc.CommaFromNumber(data.TotalPoint[(int)rankType]).ToString();
                    break;
                case GuildUserManageLayer.GUILDUSERMENU.MONTH_ARENA:
                    contributeText.text = SBFunc.CommaFromNumber(data.ArenaPoint[(int)rankType]).ToString();
                    break;
                case GuildUserManageLayer.GUILDUSERMENU.MONTH_RAID:
                    contributeText.text = SBFunc.CommaFromNumber(data.RaidPoint[(int)rankType]).ToString();
                    break;
                case GuildUserManageLayer.GUILDUSERMENU.MONTH_EXP:
                    contributeText.text = SBFunc.CommaFromNumber(data.ExpPoint[(int)rankType]).ToString();
                    break;
                case GuildUserManageLayer.GUILDUSERMENU.MONTH_MAGNET:
                    contributeText.text = SBFunc.CommaFromNumber(data.MagnetPoint[(int)rankType]).ToString();
                    break;
            }

            int pickSpriteIdx = 0;
            if (data.Rank != 0)
               pickSpriteIdx = GuildRankRewardData.GetByRankGroup(data.Rank, eGuildRankRewardGroup.UserRank).ACCUMULATE_REWARD;
            var resourceData = GuildResourceData.Get(pickSpriteIdx);
            if (resourceData != null)
            {
                userRankImg.color = new Color(1f, 1f, 1f, 1f);
                userRankImg.sprite = resourceData.RESOURCE;
            }
            else
            {
                userRankImg.color = new Color(1f, 1f, 1f, 0f);
            }
            arenaRankIcon.sprite = ResourceManager.GetResource<Sprite>(eResourcePath.ArenaRankPath, ArenaRankData.GetIconNameByPoint(data.ArenaGrade, 0));
            CanRequest = data.CanRequest;

            RefreshButton();
        }

        public void SetManageMode(eGuildManageMode state)
        {
            if(myUserNo == userNo)
            {
                normalBack.SetActive(false);
                myInfoBack.SetActive(true);
                defaultLayer.SetActive(false);
                manageLayer.SetActive(false);
                return;
            }
            lastManageMode = manageMode;
            manageMode = state;
            // 권한 체크
            defaultLayer.SetActive(manageMode == eGuildManageMode.Default);
            changeAuthorityLayer.gameObject.SetActive(manageMode == eGuildManageMode.ChangeAuthority);
            if (manageMode == eGuildManageMode.ChangeAuthority)
                changeAuthorityLayer.SetData(AuthLayerTr, userNo.ToString(),userNick,()=> {
                    changeCallBack?.Invoke();
                    SetManageMode(lastManageMode);
                });
            manageLayer.SetActive(manageMode == eGuildManageMode.Manage);
        }

        public void RefreshButton()
        {
            var IsAlreadyFriend = IsAlreadyFriendCondition();

            if (requestFriendBtn != null)
                requestFriendBtn.SetButtonSpriteState(!IsAlreadyFriend);
            whisperBtn.SetButtonSpriteState(true);
        }
        /// <summary>
        /// 이미 친구 상태인가
        /// </summary>
        /// <returns></returns>
        public bool IsAlreadyFriendCondition()
        {
            if (!CanRequest)
            {
                var friendData = FriendManager.Instance.GetFriendInfoData(userNo);
                if (friendData != null)
                    return true;
            }

            return CanRequest;
        }

        public void OnClickFriend()
        {
            if (userNo == myUserNo)
            {
                return;
                ToastManager.On("임시 = 자기자신과 친구 불가");
            }
            else
            {
                if (IsAlreadyFriendCondition())
                    ToastManager.On(StringData.GetStringByStrKey("guild_desc:105"));
                else
                {
                    //친구 요청 프로세스
                    FriendManager.Instance.SendFriendInvite(userNo,(successFriendInfo => {
                        //to do 친구 요청 성공 이후 - 해당 타겟 데이터 갱신
                        RefreshButton();
                    }));

                }
            }
        }

        public void OnClickWhisper()
        {
            if (userNo == myUserNo)
            {
                return;
                ToastManager.On("임시 = 자기자신에게 귓속말 불가");
            }
            else
            {
                //PopupManager.ClosePopup<GuildInfoPopup>();
                var popup = PopupManager.OpenPopup<ChattingPopup>();
                PopupManager.Instance.Top.SetGuildPointUI(false);
                popup.SetExitCallback(() =>
                {
                    PopupManager.Instance.Top.SetGuildPointUI(true);
                });
                popup.SetDirectChat(data, 3);
            }
        }

        public void OnClickChangeAuthority()
        {
            if (userNo == myUserNo)
            {
                return;
            }

            if(myGuildPosition == data.GuildPosition)
            {
                SystemPopup.OnSystemPopup(StringData.GetStringByStrKey("알림"), StringData.GetStringByStrKey("guild_desc:68"), true, false, false);
                return;
            }

            if (GuildManager.Instance.IsChangeUserTypeAble)
            {
                SetManageMode(eGuildManageMode.ChangeAuthority);
            }
            else
            {
                SystemPopup.OnSystemPopup(StringData.GetStringByStrKey("알림"), StringData.GetStringByStrKey("guild_desc:68"), true, false, false);
                
            }
        }

        public void OnClickManageMode()
        {
            SetManageMode(eGuildManageMode.Manage);
        }

        public void OnClickFire()
        {
            if (data.GuildPosition == eGuildPosition.Leader)
            {
                ToastManager.On(StringData.GetStringByStrKey("guild_desc:104"));
                return;
            }

            if (userNo == myUserNo)
            {
                //ToastManager.On("임시 = 자기자신을 강퇴 불가");
                return;
            }
            if (GuildManager.Instance.IsFireNormalUserAble)
            {
                if (myGuildPosition == data.GuildPosition)
                {
                    SystemPopup.OnSystemPopup(StringData.GetStringByStrKey("알림"), StringData.GetStringByStrKey("guild_desc:68"), true, false, false);
                    return;
                }

                SystemPopup.OnSystemPopup(StringData.GetStringByStrKey("알림"), StringData.GetStringFormatByStrKey("guild_desc:75", userNick), () =>
                {
                    WWWForm form = new WWWForm();
                    form.AddField("tuno", userNo.ToString());
                    form.AddField("gno", GuildManager.Instance.GuildID);

                    GuildManager.Instance.NetworkSend("guild/expel", form, () =>
                    {
                        ChatManager.Instance.SendGuildExileSystemMessage(new GuildSystemMessage(eGuildSystemMsgType.Exile, userNick));
                        changeCallBack?.Invoke();
                    });
                }, () => { }, () => { });
            }
            else
            {
                SystemPopup.OnSystemPopup(StringData.GetStringByStrKey("알림"), StringData.GetStringByStrKey("guild_desc:68"), true, false, false);
            }
        }

        public void OnClickMakeLeader()
        {
            if (GuildManager.Instance.MyData.GuildPosition == eGuildPosition.Leader)
            {
                SystemPopup.OnSystemPopup(StringData.GetStringByStrKey("알림"), StringData.GetStringFormatByStrKey("guild_desc:74",userNick), () =>
                {
                    WWWForm form = new WWWForm();
                    form.AddField("gno", GuildManager.Instance.GuildID);
                    form.AddField("tuno", userNo.ToString());
                    form.AddField("member_type", (int)eGuildPosition.Leader);
                    GuildManager.Instance.NetworkSend("guild/changemembergrade", form, () =>
                    {
                        changeCallBack?.Invoke();
                        SetManageMode(eGuildManageMode.Default);
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
            if (GuildManager.Instance.IsChangeUserTypeAble) 
            { 
                WWWForm form = new WWWForm();
                form.AddField("gno", GuildManager.Instance.GuildID);
                form.AddField("tuno", userNo.ToString());
                form.AddField("member_type", (int)eGuildPosition.Operator);
                GuildManager.Instance.NetworkSend("guild/changemembergrade", form, () =>
                {
                    changeCallBack?.Invoke();
                    SetManageMode(eGuildManageMode.Default);
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
            if (data.GuildPosition == eGuildPosition.Leader)
            {
                ToastManager.On(StringData.GetStringByStrKey("guild_desc:104"));
                return;
            }

            if (GuildManager.Instance.IsChangeUserTypeAble)
            {
                if (myGuildPosition == data.GuildPosition)
                {
                    SystemPopup.OnSystemPopup(StringData.GetStringByStrKey("알림"), StringData.GetStringByStrKey("guild_desc:68"), true, false, false);
                    return;
                }
                WWWForm form = new WWWForm();
                form.AddField("gno", GuildManager.Instance.GuildID);
                form.AddField("tuno", userNo.ToString());
                form.AddField("member_type", (int)eGuildPosition.Normal);
                GuildManager.Instance.NetworkSend("guild/changemembergrade", form, () =>
                {
                    changeCallBack?.Invoke();
                    SetManageMode(eGuildManageMode.Default);
                });
            }
            else
            {
                ToastManager.On(StringData.GetStringByStrKey("guild_desc:104"));
            }
        }

        public void OnEvent(FriendEvent eventType)
        {
            if(eventType.UserUID == data.UID)
                RefreshButton();
        }
    }
}