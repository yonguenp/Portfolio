using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class ChatLayerBase : TabLayer
    {
        [Header("[ChatLayerBase]")]
        public eChatCommentType chatLayerType = eChatCommentType.None;
        [SerializeField] protected InputField chatEditBox = null;

        protected int bwfCount = 0;
        protected bool chatEnable = true;

        public virtual void InitChatLayer()
        {

        }

        public virtual void RefreshChatLayer()
        {

        }

        public virtual void OnChatSubmit(eChatCommentType chatType, long targetUserID = 0)
        {
            OnSubmit(chatEditBox.text, chatType, targetUserID);
        }

        void OnSubmit(string _text, eChatCommentType chatType, long targetUserID = 0/*대상이없는 경우 -> 0 (ex월드)*/)
        {
            if (!chatEnable)
            {
                ToastManager.On(StringData.GetStringFormatByIndex(100002680, bwfCount));
                return;
            }

            _text = _text.Trim();//공란 체크
            if (string.IsNullOrEmpty(_text))
            {
                InitInputField();
                ToastManager.On(100002128);
                return;
            }

            InitInputField();

            if (Crosstales.BWF.BWFManager.Instance.Contains(_text))
            {
                bwfCount++;
                if (bwfCount > 3)
                {
                    ToastManager.On(StringData.GetStringFormatByIndex(100002680, bwfCount));
                    chatEnable = false;

                    Invoke("ChatEnable", bwfCount * 60.0f);
                    return;
                }
            }
            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex("[<>]");
            if (regex.IsMatch(_text))
            {
                ToastManager.On(StringData.GetStringByStrKey("비허용문자채팅"));
                return;
            }

            ChatDataInfo data = new(chatType, User.Instance.UserAccountData.UserNumber, User.Instance.UserData.UserNick, User.Instance.UserData.UserPortrait, GuildManager.Instance.GuildID, GuildManager.Instance.GuildName, SBFunc.GetDateTimeToTimeStamp(), targetUserID,
            SBFunc.GetNowDateTimeToTimeStamp(), _text, (int) ePortraitEtcType.RAID , User.Instance.UserData.UserPortraitFrameInfo.GetValue(ePortraitEtcType.RAID));

            ChatManager.Instance.SendMessage(data);
        }

        void ChatEnable()
        {
            CancelInvoke("ChatEnable");
            chatEnable = true;
        }

        protected void InitInputField()//인풋박스 초기화
        {
            chatEditBox.text = "";//입력 데이터 초기화
            chatEditBox.placeholder.GetComponent<Text>().enabled = true;
        }
    }
}
