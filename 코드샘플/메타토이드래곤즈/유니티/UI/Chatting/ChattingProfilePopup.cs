using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class ChattingProfilePopup : Popup<ChattingPopupData>
    {
        [SerializeField] private Text userName_text;
        [SerializeField] private Image rank_icon;
        [SerializeField] private Text rank_text;
        [SerializeField] private GuildBaseInfoObject guild_obj;
        [SerializeField] private UserPortraitFrame userPortraitFrame;

        [Header("[버 튼]")]
        [SerializeField] private Button inviteFriendButton;
        [SerializeField] private Button removeFriendButton;
        [SerializeField] private Button practiceButton;
        [SerializeField] private Button chatBlockButton;

        bool isLocalFriendSend = false;
        #region OpenPopup
        public static ChattingProfilePopup OpenPopup(ChattingPopupData data)
        {
            return PopupManager.OpenPopup<ChattingProfilePopup>(data);
        }
        #endregion

        public void onClickCloseBtn()
        {
            PopupManager.ClosePopup<ChattingProfilePopup>();
        }
        public override void ForceUpdate(ChattingPopupData data = null)
        {
        }

        public override void InitUI()
        {
            isLocalFriendSend = false;
            SetUI();
        }

        public void OnClickBlockUserPopup()
        {
            if (Data.UserData == null)
                return;

            if (ChatManager.Instance.IsInBlockUserList(Data.UserData.UID))
            {
                ToastManager.Instance.Set(StringData.GetStringByIndex(100002482));
                return;
            }

            var popup = PopupManager.OpenPopup<ChattingBlockAlarmPopup>();
            popup.SetMessage(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100002426));
            popup.SetUserNickName(Data.UserData.Nick);
            popup.SetCallBack(() =>//ok
                {
                    //to do - 서버 리스폰스 확인 절차 필요함
                    ToastManager.On(100002141);//"해당 유저의 대화 내용을 차단하였습니다."
                    ChatManager.Instance.AddUserBlockList(Data.UserData);
                },
                () =>
                {//cancle
                },
                () =>
                {//exit
                }
            );
        }

        public void OnClickInviteFriendBtn()
        {
            if (null == Data.UserData)
                return;
            if (isLocalFriendSend || false == Data.UserData.IsCanReqFriend)
            {
                ToastManager.Instance.Set(StringData.GetStringByStrKey("errorcode_1409"));
                return;
            }

            isLocalFriendSend = true;
            FriendManager.Instance.SendFriendInvite(Data.UserData.UID, null, (s) => isLocalFriendSend = false);
        }
        public void OnClickRemoveFriend()
        {
            if (Data.UserData == null)
                return;

            var popup = PopupManager.OpenPopup<SystemPopup>();
            popup.SetMessage(StringData.GetStringByIndex(100000248), StringData.GetStringFormatByStrKey("친구삭제팝업설명", Data.UserData.Nick));
            popup.SetCallBack(() =>//ok
            {
                FriendManager.Instance.DeleteFriend(Data.UserData.UID);
            },
                () =>
                {//cancle
                },
                () =>
                {//exit
                }
            );
        }
        public void OnClickPracticeButton()
        {
            FriendManager.Instance.OnPractice(Data.UserData.UID, () => ToastManager.Instance.Set(StringData.GetStringByStrKey("친선대전불가")));
        }
        //public void OnClickUpdateGuideButton()
        //{
        //    ToastManager.On(StringData.GetStringByStrKey("system_message_update_01"));
        //}
        //public void OnClickDirectChat()
        //{
        //    if (currentOtherUserData == null || currentOtherUserData.userNumber <= 0) return;

        //    PopupManager.GetPopup<ChattingPopup>().SetDirectChat(currentOtherUserData.userNumber);
        //    ClosePopup();
        //}

        /// <summary> UI 정보 세팅 </summary>
        void SetUI()
        {
            if (Data.UserData == null)
                return;

            userName_text.text = Data.UserData.Nick;
            userPortraitFrame.SetUserPortraitFrame(Data.UserData.PortraitIcon, Data.UserData.Level, true, Data.UserData.EtcInfo);

            SetArenaUI();
            SetGuildUI();
            SetButton();
        }
        /// <summary> 아레나 정보 세팅 </summary>
        void SetArenaUI()
        {
            if (rank_icon != null)
            {
                var icon = ResourceManager.GetResource<Sprite>(eResourcePath.ArenaRankPath, ArenaRankData.GetIconNameByPoint(Data.UserData.RankGrade, 0));
                if (icon != null)
                    rank_icon.sprite = icon;
            }

            if (rank_text != null)
            {
                if (Data.UserData.Rank <= 0)
                    rank_text.text = StringData.GetStringFormatByStrKey("순위", "-");
                else
                    rank_text.text = SBFunc.GetRankText(Data.UserData.Rank);
            }
        }
        /// <summary> 길드 정보 세팅 </summary>
        void SetGuildUI()
        {
            if (guild_obj != null)
            {
                bool enableGuild = GuildManager.Instance.GuildWorkAble && Data.UserData.GuildNo > 0;
                guild_obj.gameObject.SetActive(enableGuild);

                if (enableGuild)
                    guild_obj.Init(Data.UserData.GuildName, Data.UserData.GuildMarkNo, Data.UserData.GuildEmblemNo, Data.UserData.GuildNo);
            }
        }

        private void SetButton()
        {
            if (inviteFriendButton != null)
            {
                inviteFriendButton.gameObject.SetActive(false == Data.UserData.IsFriend);
            }
            if (removeFriendButton != null)
            {
                removeFriendButton.gameObject.SetActive(Data.UserData.IsFriend);
            }
        }
    }
}