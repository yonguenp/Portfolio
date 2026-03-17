using DG.Tweening;
using Google.Impl;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class GuildInfoLayer : TabLayer, EventListener<GuildEvent>
    {
        [SerializeField]
        Sprite[] rankSprite;
        [SerializeField]
        Sprite[] userRankSprite;

        [SerializeField]
        GuildMarkObject guildMarkObject;
        [SerializeField]
        Image RankIcon;
        [SerializeField]
        Image UserRankIcon;

        [SerializeField]
        Text guildNameText;
        [SerializeField]
        Text guildMemberTypeText;
        [SerializeField]        
        Text guildLevelText;
        [SerializeField]
        Text guildBuffInfoText;
        [SerializeField]
        Slider guildExpSlider;
        [SerializeField]
        Text guildExpPercentText;
        [SerializeField]
        Text guildExpAmountText;
        [SerializeField]
        Text guildRankingText;
        [SerializeField]
        Text guildLeaderText;
        [SerializeField]
        Text guildUserCntText;
        [SerializeField]
        Button guildAttendenceBtn;
        [SerializeField]
        GameObject attendIcon;
        [SerializeField]
        Text attendenceBtnText;
        [SerializeField]
        GameObject guildManageReddot;
        [SerializeField]
        GameObject donateIcon;
        [SerializeField]
        Button DonateBtn;

        [Space()]
        [Header("destroying guild timer..")]
        [SerializeField]
        GameObject guildDestroyTimeObj;
        [SerializeField]
        Text guildRemainTimeText;

        [Space()]
        [Header("info and introduce")]
        [SerializeField]
        InputField guildAnnounceInputField;
        [SerializeField]
        GameObject guildAnnounceModeObj;
        [SerializeField]
        GameObject guildAnnounceScrollObj;
        [SerializeField]
        Text guildAnnounceScrollText;
        [SerializeField]
        InputField guildIntroduceInputField;
        [SerializeField]
        GameObject guildIntroduceModeObj;
        [SerializeField]
        GameObject guildIntroduceScrollObj;
        [SerializeField]
        Text guildIntroduceScrollText;
        [SerializeField]
        GameObject guildManagementButton;
        [SerializeField]
        Button AnnounceEditButton;
        [SerializeField]
        Button IntroduceEditButton;
        
        bool isAnnounceModifyMode = false;
        bool isIntroduceModifyMode = false;
        bool isAnnounceIntroduceModifyAble = false;

        bool isAttendenceAble = false;
        bool isDonatable = false;

        private Tween breathingTween;
        public float scaleMultiplier = 1.3f;
        public float duration = 0.8f;

        public float shakeAmount = 0.3f;
        public float duration2 = 0.3f;
        public float tiltAngle = 15f; 
        private Tween shakeTween;

        GuildDetailData guildDetailData = null;

        List<GuildDonationData> guildDonationData = new List<GuildDonationData>();

        public override void InitUI(TabTypePopupData datas = null)
        {
            base.InitUI(datas);
            guildDestroyTimeObj.SetActive(false);
            isAnnounceIntroduceModifyAble = GuildManager.Instance.MyData.GuildPosition == eGuildPosition.Operator || GuildManager.Instance.MyData.GuildPosition == eGuildPosition.Leader;
            SetInfo();
            
            SetDefaultInputMode();
        }

        public override void RefreshUI()
        {
            base.RefreshUI();
            guildDestroyTimeObj.SetActive(false);
            isAnnounceIntroduceModifyAble = GuildManager.Instance.MyData.GuildPosition == eGuildPosition.Operator || GuildManager.Instance.MyData.GuildPosition == eGuildPosition.Leader;
            SetInfo();

            SetDefaultInputMode();
        }

        void SetDefaultInputMode()
        {
            isAnnounceModifyMode = false;
            isIntroduceModifyMode = false;
            guildAnnounceModeObj.SetActive(false);
            guildIntroduceModeObj.SetActive(false);
            guildAnnounceInputField.gameObject.SetActive(false);
            guildIntroduceInputField.gameObject.SetActive(false);
            guildAnnounceInputField.readOnly = true;
            guildIntroduceInputField.readOnly = true;

            AnnounceEditButton.gameObject.SetActive(true);
            IntroduceEditButton.gameObject.SetActive(true);
        }

        void SetInfo()
        {
            guildDetailData = GuildManager.Instance.MyGuildInfo;
            if (guildDetailData != null)
            {
                guildMarkObject.SetGuildMark(guildDetailData.GetGuildEmblem(), guildDetailData.GetGuildMark());
                guildNameText.text = guildDetailData.GetGuildName();
                guildLevelText.text = StringData.GetStringFormatByStrKey("user_info_lv_02", guildDetailData.GetGuildLevel());

                switch(GuildManager.Instance.MyData.GuildPosition)
                {
                    case eGuildPosition.Leader:
                        guildMemberTypeText.text = StringData.GetStringByStrKey("guild_desc:23");
                        break;
                    case eGuildPosition.Operator:
                        guildMemberTypeText.text = StringData.GetStringByStrKey("guild_desc:113");
                        break;
                    case eGuildPosition.Normal:
                    default:
                        guildMemberTypeText.text = StringData.GetStringByStrKey("guild_desc:28");
                        break;
                }
                
                int rank = guildDetailData.GetGuildRank();
                if (rank > 0 && rank < 50)
                {
                    RankIcon.gameObject.SetActive(true);
                    if (rank == 1)
                    {
                        RankIcon.sprite = rankSprite[0];
                    }
                    else if (rank == 2)
                    {
                        RankIcon.sprite = rankSprite[1];
                    }
                    else if (rank == 3)
                    {
                        RankIcon.sprite = rankSprite[2];
                    }
                    else if (rank <= 5)
                    {
                        RankIcon.sprite = rankSprite[3];
                    }
                    else if (rank <= 10)
                    {
                        RankIcon.sprite = rankSprite[4];
                    }
                    else if (rank <= 20)
                    {
                        RankIcon.sprite = rankSprite[5];
                    }
                    else if (rank <= 49)
                    {
                        RankIcon.sprite = rankSprite[6];
                    }
                    else
                    {
                        RankIcon.gameObject.SetActive(false);
                    }
                }
                else
                {
                    RankIcon.gameObject.SetActive(false);
                }

                int userrank = -1;
                if (guildDetailData.GuildUserDictionary.ContainsKey(User.Instance.UserAccountData.UserNumber))
                {
                    userrank = guildDetailData.GuildUserDictionary[User.Instance.UserAccountData.UserNumber].Rank;
                    if (userrank > 0 && userrank < 50)
                    {
                        UserRankIcon.gameObject.SetActive(true);
                        if (userrank == 1)
                        {
                            UserRankIcon.sprite = userRankSprite[0];
                        }
                        else if (userrank == 2)
                        {
                            UserRankIcon.sprite = userRankSprite[1];
                        }
                        else if (userrank == 3)
                        {
                            UserRankIcon.sprite = userRankSprite[2];
                        }
                        else if (userrank <= 5)
                        {
                            UserRankIcon.sprite = userRankSprite[3];
                        }
                        else if (userrank <= 10)
                        {
                            UserRankIcon.sprite = userRankSprite[4];
                        }
                        else if (userrank <= 20)
                        {
                            UserRankIcon.sprite = userRankSprite[5];
                        }
                        else if (userrank <= 49)
                        {
                            UserRankIcon.sprite = userRankSprite[6];
                        }
                        else
                        {
                            UserRankIcon.gameObject.SetActive(false);
                        }
                    }
                    else
                    {
                        UserRankIcon.gameObject.SetActive(false);
                    }
                }
                else
                {
                    UserRankIcon.gameObject.SetActive(false);
                }

                //guildRankIcon.gameObject.SetActive(
                //List<string> buff = new List<string>();
                //foreach (var data in GuildExpData.GetStatsByLv(guildDetailData.GetGuildLevel()))
                //{
                //    buff.Add(string.Format("{0} +{1}", data.Key, SBFunc.CommaFromNumber((int)data.Value)));
                //}
                //guildBuffInfoText.text = string.Join("\n", buff);

                //길드버프가 보스 대미지밖에 없다고 가정
                foreach (var data in GuildExpData.GetStatsByLv(guildDetailData.GetGuildLevel()))
                {
                    guildBuffInfoText.text = "+ " + SBFunc.CommaFromNumber((int)data.Value);
                }

                var guildExp = GuildManager.Instance.GuildExpData;
                if(guildExp != null)
                {
                    float ratio = 1.0f;

                    if (guildDetailData.GetGuildLevel() < 50)
                    {
                        var curGuildExp = guildDetailData.GetGuildExp();
                        var requireExp = guildExp.EXP;
                        var remainExp = curGuildExp - guildExp.TOTAL_EXP;
                        ratio = (float)remainExp / requireExp;

                        if (guildExpAmountText != null)
                            guildExpAmountText.text = "(" + SBFunc.StrBuilder(remainExp, "/", requireExp) + ")";

                        if (guildExpSlider != null)
                            guildExpSlider.value = ratio;

                        if (guildExpPercentText != null)
                            guildExpPercentText.text = SBFunc.StrBuilder(MathF.Floor(ratio * SBDefine.BASE_FLOAT), "%");
                    }
                    else
                    {
                        if (guildExpAmountText != null)
                            guildExpAmountText.text = StringData.GetStringByStrKey("길드맥스레벨");

                        if (guildExpSlider != null)
                            guildExpSlider.value = ratio;

                        if (guildExpPercentText != null)
                            guildExpPercentText.gameObject.SetActive(false);
                    }
                }

                if (rank > 0) 
                    guildRankingText.text = SBFunc.GetRankText(rank);
                else
                    guildRankingText.text = "-";
                guildLeaderText.text = guildDetailData.GetLeaderNick();
                guildUserCntText.text = string.Format("{0}/{1}",guildDetailData.InfoData.GetGuildGuildPeopleCount(),guildDetailData.InfoData.GetGuildGuildPeopleMaxCount());
                guildAnnounceScrollText.text = guildAnnounceInputField.text = guildDetailData.InfoData.GuildNotice;                
                guildIntroduceScrollText.text = guildIntroduceInputField.text = guildDetailData.InfoData.GuildDesc;
                SetInputFieldState(false, true);
                SetInputFieldState(false, false);
                SetReddot(GuildManager.Instance.IsManageJoinAble);
                SetAttendenceState();
                SetDonateState();
                if (GuildManager.Instance.IsDestroying)
                {
                    guildDestroyTimeObj.SetActive(true);

                    guildRemainTimeText.text = StringData.GetStringFormatByStrKey("guild_desc:76", SBFunc.TimeStampDeepRemainString(GuildManager.Instance.DestroyTimeStamp));
                }
                else
                {
                    guildDestroyTimeObj.SetActive(false);
                }


            }

            guildManagementButton.SetActive(isAnnounceIntroduceModifyAble);

            AnnounceEditButton.gameObject.SetActive(isAnnounceIntroduceModifyAble);
            IntroduceEditButton.gameObject.SetActive(isAnnounceIntroduceModifyAble);            
        }

        void SetReddot(bool isOperatorManageJoin) // 길드 운영진이 조합 가입 승인 권한을 가졌는가?
        {
            guildManageReddot.SetActive(false);
            if (isOperatorManageJoin)
            {
                if (GuildManager.Instance.JoinApplyToMyGuildList.Count > 0)
                {
                    guildManageReddot.SetActive(true);
                }
            }
        }
        void SetDonateState()
        {
            isDonatable = GuildManager.Instance.GuildRemainDonationCount > 0;
            DonateBtn.SetButtonSpriteState(isDonatable);
            if (isDonatable)
            {
                StartBreathing();
            }
            else
            {
                StopBreathing();
            }
        }

        private void StartBreathing()
        {
            StopBreathing();

            breathingTween = donateIcon.transform.DOScale(Vector3.one * scaleMultiplier, duration)
                             .SetLoops(-1, LoopType.Yoyo)
                             .SetEase(Ease.InOutSine);
        }

        private void StopBreathing()
        {
            breathingTween?.Kill();
            donateIcon.transform.localScale = Vector3.one;
        }


        void SetAttendenceState()
        { 
            isAttendenceAble = GuildManager.Instance.IsAttendenceAble;
            string strKey = isAttendenceAble ? "quest_base_ATTENDANCE" : "guild_desc:33";
            attendenceBtnText.text = StringData.GetStringByStrKey(strKey);
            guildAttendenceBtn.SetButtonSpriteState(isAttendenceAble);

            if (isAttendenceAble)
            {
                StartShaking();
            }
            else
            {
                StopShaking();
            }
        }
        private void StartShaking()
        {
            StopShaking();

            shakeTween = attendIcon.transform.DORotate(new Vector3(0, 0, tiltAngle), duration2)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
        }

        private void StopShaking()
        {
            shakeTween?.Kill();
            attendIcon.transform.rotation = Quaternion.identity;
        }

        private void StopAllTween()
        {
            StopShaking();
            StopBreathing();
        }

        public void OnClickLvRewardBtn()
        {
            PopupManager.OpenPopup<GuildLvRewardInfoPopup>();
        }

        public void OnClickChangeAnnounce()
        {
            if(isAnnounceIntroduceModifyAble)
            {
                isAnnounceModifyMode = true;
                AnnounceEditButton.gameObject.SetActive(false);
                guildAnnounceModeObj.SetActive(true);
                //guildAnnounceInputField.readOnly = false;
                SetInputFieldState(true, true);
            }
            else
            {
                ToastManager.On(StringData.GetStringByStrKey("guild_desc:104"));
            }
        }

        public void OnClickChangeIntroduce()
        {
            if (isAnnounceIntroduceModifyAble)
            {
                isIntroduceModifyMode = true;
                guildIntroduceModeObj.SetActive(true);
                IntroduceEditButton.gameObject.SetActive(false);
                //guildIntroduceInputField.readOnly = false;
                SetInputFieldState(true, false);
            }
            else
            {
                ToastManager.On(StringData.GetStringByStrKey("guild_desc:104"));
            }
        }

        void SetInputFieldState(bool isModifyAble, bool isAnnouce)
        {
            if (isAnnouce)
            {
                guildAnnounceInputField.gameObject.SetActive(isModifyAble);
                guildAnnounceInputField.readOnly = !isModifyAble;
                guildAnnounceScrollObj.SetActive(!isModifyAble);
                guildAnnounceScrollText.text = guildDetailData.InfoData.GuildNotice;
            }
            else
            {
                guildIntroduceInputField.gameObject.SetActive(isModifyAble);
                guildIntroduceInputField.readOnly = !isModifyAble;
                guildIntroduceScrollObj.SetActive(!isModifyAble);
                guildIntroduceScrollText.text = guildDetailData.InfoData.GuildDesc;
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(guildAnnounceScrollObj.GetComponentInChildren<ScrollRect>().content);
            LayoutRebuilder.ForceRebuildLayoutImmediate(guildIntroduceScrollObj.GetComponentInChildren<ScrollRect>().content);
        }


        public void OnClickCancelSetAnnounce()
        {
            isAnnounceModifyMode = false;
            guildAnnounceModeObj.SetActive(false);
            SetInputFieldState(false, true);
            AnnounceEditButton.gameObject.SetActive(true);
        }
        public void OnClickCancelSetIndtroduce()
        {
            isIntroduceModifyMode = false;
            guildIntroduceModeObj.SetActive(false);
            SetInputFieldState(false, false);
            IntroduceEditButton.gameObject.SetActive(true);
        }

        public void OnClickSetGuildAnnounce()
        {
            if (isAnnounceIntroduceModifyAble)
            {
                isAnnounceModifyMode = false;
                guildAnnounceModeObj.SetActive(false);
                //guildAnnounceInputField.readOnly = true;
                SetInputFieldState(false, true);
                WWWForm data = new WWWForm();
                data.AddField("gno", GuildManager.Instance.GuildID);
                data.AddField("guild_notice", guildAnnounceInputField.text);
                GuildManager.Instance.NetworkSend("guild/changeguildnotice", data, ()=> {
                    AnnounceEditButton.gameObject.SetActive(true);
                });
            }
            else
            {
                ToastManager.On(StringData.GetStringByStrKey("guild_desc:104"));
            }
        }

        public void OnClickSetGuildIntroduce()
        {
            if (isAnnounceIntroduceModifyAble)
            {
                isIntroduceModifyMode = false;
                guildIntroduceModeObj.SetActive(false);
                //guildIntroduceInputField.readOnly = true;
                SetInputFieldState(false, false);
                WWWForm data = new WWWForm();
                data.AddField("gno", GuildManager.Instance.GuildID);
                data.AddField("guild_desc", guildIntroduceInputField.text);
                GuildManager.Instance.NetworkSend("guild/changeguilddesc", data, ()=> {
                    IntroduceEditButton.gameObject.SetActive(true);
                });
            }
            else
            {
                ToastManager.On(StringData.GetStringByStrKey("guild_desc:104"));
            }
        }

        public void OnClickManageTeam()
        {
            if (GuildManager.Instance.MyData.GuildPosition == eGuildPosition.Normal)
            {
                ToastManager.On(StringData.GetStringByStrKey("guild_desc:104"));
            }
            else
            {
                PopupManager.OpenPopup<GuildManagePopup>();
            }
        }

        public void OnClickChat()
        {
            var popup = PopupManager.OpenPopup<ChattingPopup>();
            PopupManager.Instance.Top.SetGuildPointUI(false);
            popup.SetExitCallback(() =>
            {
                PopupManager.Instance.Top.SetGuildPointUI(true);
            });
            popup.SetDirectGuildChatLayer();
            //PopupManager.OpenPopup<ChattingPopup>(new ChattingPopupData())
        }

        public void OnClickQuit()
        {

            if (guildDetailData.GuildUserList.Count == 1)
            {
                SystemPopup.OnSystemPopup(StringData.GetStringByStrKey("알림"), StringData.GetStringFormatByStrKey("guild_desc:99",guildDetailData.GetGuildName()), () =>
                {
                    WWWForm form = new WWWForm();
                    form.AddField("gno", GuildManager.Instance.GuildID);
                    GuildManager.Instance.NetworkSend("guild/leave", form, () =>
                    {
                        PopupManager.ClosePopup<GuildInfoPopup>();
                    });
                });
                PopupManager.GetPopup<SystemPopup>().SetButtonState(true, true, true);
                return;
            }
            // 만약 이 유저가 길드장 이라면 
            if (GuildManager.Instance.MyData.GuildPosition == eGuildPosition.Leader)
            {
                SystemPopup.OnSystemPopup(StringData.GetStringByStrKey("알림"), StringData.GetStringByStrKey("guild_desc:39"), true, false, false);
            }
            else
            {
                SystemPopup.OnSystemPopup(StringData.GetStringByStrKey("알림"), StringData.GetStringByStrKey("guild_desc:38"), () =>
                {
                    WWWForm form = new WWWForm();
                    form.AddField("gno", GuildManager.Instance.GuildID);
                    GuildManager.Instance.NetworkSend("guild/leave", form, () =>
                    {
                        PopupManager.ClosePopup<GuildInfoPopup>();
                    });
                });
                PopupManager.GetPopup<SystemPopup>().SetButtonState(true, true, true);
            }
           
        }

        public void OnClickAttendence()
        {
            if (isAttendenceAble)
            {
                WWWForm form = new WWWForm();
                form.AddField("gno",GuildManager.Instance.GuildID);
                GuildManager.Instance.NetworkSend("guild/attendence", form, () =>
                {
                    SetInfo();
                });
            }
            else
            {
                ToastManager.On(StringData.GetStringByStrKey("guild_desc:33"));
            }
        }

        public void OnClickGuildShop()
        {
            PopupManager.OpenPopup<GuildShopPopup>();
        }

        public void OnClickDonate()
        {
            if (!isDonatable)
            {
                ToastManager.On(StringData.GetStringByStrKey("guild_desc:112"));
                return;
            }

            if (GuildManager.Instance.MyGuildInfo.GuildTimeData.JoinDate < TimeManager.GetContentClearTime(true))
                PopupManager.OpenPopup<GuildDonatePopup>();
            else
                ToastManager.On(StringData.GetStringByStrKey("guild_desc:111"));
        }

        public void OnClickDestroyingAlarmInfo()
        {
            PopupManager.OpenPopup<GuildDestroyInfoPopup>();
        }

        public void OnEvent(GuildEvent eventType)
        {
            switch(eventType.Event) {
                case GuildEvent.eGuildEventType.LostGuild :
                    break;
                case GuildEvent.eGuildEventType.GuildRefresh :
                default:
                    SetInfo();
                    break;
            }
            
        }

        private void OnEnable()
        {
            EventManager.AddListener(this);
            SetInfo();
        }
        private void OnDisable()
        {
            EventManager.RemoveListener(this);
            StopAllTween();
        }


        public void OnToolTipGuildRank()
        {
            int rank = guildDetailData.GetGuildRank();
            SimpleToolTip.OpenPopup(StringData.GetStringFormatByStrKey("조합누적랭킹", SBFunc.GetRankText(rank)), RankIcon.gameObject);
        }

        public void OnToolTipUserRank()
        {
            if (guildDetailData.GuildUserDictionary.ContainsKey(User.Instance.UserAccountData.UserNumber))
            {
                int userrank = guildDetailData.GuildUserDictionary[User.Instance.UserAccountData.UserNumber].Rank;
                SimpleToolTip.OpenPopup(StringData.GetStringFormatByStrKey("조합원누적랭킹", SBFunc.GetRankText(userrank)), UserRankIcon.gameObject);
            }            
        }
    }

}
