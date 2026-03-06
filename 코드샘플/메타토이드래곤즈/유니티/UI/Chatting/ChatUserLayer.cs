using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public enum ChatUserState
    {
        UNKNOWN,
        UNCONNECT,
        CONNECT,
        CONNECT_CHECK
    }
    public class ChatUserLayer : ChatLayerBase, EventListener<ChatEvent>
    {
        [Header("[ChatUser]")]
        [SerializeField] ChatOneOnOneScrollview scrollview = null;

        [SerializeField] Text titleText = null;
        [SerializeField] GameObject NoOneInHereAlertObj = null;
        [SerializeField] GameObject botChatLayerObject = null;

        [SerializeField] GameObject UnConnectObject = null;
        [SerializeField] GameObject UnknownObject = null;
        [SerializeField] GameObject ConnectObject = null;

        [Header("[ChatUser Buttons]")]
        [SerializeField] GameObject closeButtonObject = null;
        [SerializeField] GameObject backButtonObject = null;
        [SerializeField] GameObject blockButton = null;
        [SerializeField] Text blockButtonText = null;
        [SerializeField] TimeObject timeObj = null;

        protected ChatUserState CurState { get; set; } = ChatUserState.UNKNOWN;
        private int chatDelayTimeStamp = 0;//유저가 마지막으로 입력한 당시(채팅 반영시간)의 시간 값을 들고있어야함 - 발송 시간 기준 + 딜레이 추가 값

        ThumbnailUserData otherUserData = null;
        Coroutine coroutine = null;

        long chatTargetID = 0;

        private void OnEnable()
        {
            this.EventStart();

            SetPopupButtonState(true);
        }

        private void OnDisable()
        {
            this.EventStop();
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
                coroutine = null;
            }

            if (timeObj != null)
                timeObj.Refresh = null;

            SetPopupButtonState(false);
        }

        public void OnEvent(ChatEvent eventType)
        {
            switch (eventType.Event)
            {
                case ChatEvent.eChatEventEnum.SendMacro:
                    scrollview?.RequestScrollviewGotoBottom();
                    break;
                case ChatEvent.eChatEventEnum.RefreshUI:
                    //to do - 신규 메세지 데이터 가져와서 스크롤 뷰 아이템 등록
                    scrollview?.RequestRefreshScrollview(chatLayerType);
                    scrollview?.CheckNewMessageButton();//스크롤이 바닥이 아닐 때 신규 메세지 확인 버튼 출력
                    break;
                case ChatEvent.eChatEventEnum.WhisperRes:
                    //접속 확인
                    if (chatTargetID != eventType.TargetUID)
                        return;

                    if (coroutine != null)
                    {
                        StopCoroutine(coroutine);
                        coroutine = null;
                    }

                    SetConnectObject(ChatUserState.CONNECT);
                    break;
            }
        }
        public override void InitUI(TabTypePopupData datas = null)//ChangeTab
        {
            base.InitUI(datas);
            scrollview?.Initialize();
            scrollview?.ClearScrollView();

            SetConnectObject(ChatUserState.UNKNOWN);
        }

        IEnumerator CheckResDelay()
        {
            yield return SBDefine.GetWaitForSecondsRealtime(2f);
            if (CurState == ChatUserState.CONNECT_CHECK)
            {
                yield return SBDefine.GetWaitForSecondsRealtime(2f);
                ToastManager.Instance.Set(StringData.GetStringByIndex(100002264));
                if (timeObj != null)
                    timeObj.Refresh = null;

                SetConnectObject(ChatUserState.UNCONNECT);
                coroutine = null;
                yield break;
            }

            SetConnectObject(ChatUserState.UNCONNECT);
            coroutine = null;
            yield break;
        }

        void SetConnectObject(ChatUserState state)
        {
            if (UnknownObject != null)
                UnknownObject.gameObject.SetActive(state == ChatUserState.UNKNOWN);
            if (UnConnectObject != null)
                UnConnectObject.gameObject.SetActive(state == ChatUserState.UNCONNECT);
            if (ConnectObject != null)
                ConnectObject.gameObject.SetActive(state == ChatUserState.CONNECT || state == ChatUserState.CONNECT_CHECK);

            switch (state)
            {
                case ChatUserState.UNCONNECT:
                    if (blockButton != null)
                        blockButton.SetActive(true);
                    if (blockButtonText != null)
                        blockButtonText.text = StringData.GetStringByIndex(100002483);
                    break;
                case ChatUserState.CONNECT:
                    if (CurState == ChatUserState.CONNECT_CHECK)
                        break;

                    if (blockButton != null)
                        blockButton.SetActive(false);
                    break;
                case ChatUserState.CONNECT_CHECK:
                    break;
                case ChatUserState.UNKNOWN:
                default:
                    if (blockButton != null)
                        blockButton.SetActive(true);
                    if (blockButtonText != null)
                        blockButtonText.text = StringData.GetStringByStrKey("guide_wait");
                    break;
            }

            CurState = state;
        }

        public override void InitChatLayer()
        {
            base.InitChatLayer();
            scrollview?.SetVisibleNewMsgButton(false);//일단 신규 메세지 버튼 끔

            scrollview?.RequestRefreshScrollview(chatLayerType);
            scrollview?.RequestScrollviewGotoBottom();

            NoOneInHereAlertObj.SetActive(chatTargetID <= 0);
            botChatLayerObject.SetActive(chatTargetID > 0);

            // 채팅방 이름 설정
            if (chatTargetID > 0)
            {
                titleText.text = StringData.GetStringFormatByIndex(100002150, otherUserData.Nick);
                ChatManager.Instance.SendWhisperCheck(chatTargetID);
                if (coroutine != null)
                {
                    StopCoroutine(coroutine);
                    coroutine = null;
                }
                coroutine = StartCoroutine(CheckResDelay());
            }
            else
            {
                titleText.text = StringData.GetStringByIndex(titleIndex);
                SetConnectObject(ChatUserState.UNCONNECT);
            }
        }

        public override void RefreshUI()//ForceUpdate();
        {
            base.RefreshUI();

            RefreshChatLayer();
        }

        public override void RefreshChatLayer()
        {
            base.RefreshChatLayer();

            scrollview?.RequestRefreshScrollview(chatLayerType);
        }

        public void SetChatTarget(ThumbnailUserData otherUser)
        {
            otherUserData = otherUser;
            chatTargetID = otherUserData == null ? -1 : otherUserData.UID;

            NoOneInHereAlertObj.SetActive(chatTargetID <= 0);
            botChatLayerObject.SetActive(chatTargetID > 0);

            scrollview.SetTargetUno(chatTargetID);

            InitChatLayer();
        }

        public override void OnChatSubmit(eChatCommentType chatType, long targetUserID = 0)
        {
            var _text = chatEditBox.text;
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

            ChatDataInfo data = new(chatType, User.Instance.UserAccountData.UserNumber, User.Instance.UserData.UserNick, User.Instance.UserData.UserPortrait, GuildManager.Instance.GuildID, GuildManager.Instance.GuildName, SBFunc.GetDateTimeToTimeStamp(), targetUserID,
                SBFunc.GetNowDateTimeToTimeStamp(), _text, (int)ePortraitEtcType.RAID, User.Instance.UserData.UserPortraitFrameInfo.GetValue(ePortraitEtcType.RAID));

            SetConnectObject(ChatUserState.CONNECT_CHECK);
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
                coroutine = null;
            }
            coroutine = StartCoroutine(CheckResDelay());
            ChatManager.Instance.SendMessage(data);
        }

        public void OnClickSendMessage()//전송 버튼 누를 때
        {
            if (chatTargetID <= 0) return;

            OnChatSubmit(chatLayerType, chatTargetID);
            SendMsgAndSettingDelayProcess();
        }
        void SendMsgAndSettingDelayProcess()
        {
            //임시 타임값 세팅 + 시간 제어 갱신
            chatDelayTimeStamp = TimeManager.GetTime() + SBDefine.CHAT_TIME_UI_DELAY;
            RefreshChatDelayTime();
            scrollview?.RequestScrollviewGotoBottom(true);
        }

        void RefreshChatDelayTime()
        {
            if (chatDelayTimeStamp <= 0)
            {
                if (blockButton != null)
                    blockButton.gameObject.SetActive(false);
                return;
            }

            if (timeObj != null && chatDelayTimeStamp - TimeManager.GetTime() > 0)
            {
                if (blockButton != null)
                    blockButton.gameObject.SetActive(true);

                timeObj.Refresh = delegate
                {
                    float remain = chatDelayTimeStamp - TimeManager.GetTime();
                    blockButtonText.text = string.Format(StringData.GetStringByIndex(100002435), remain); //SBFunc.StrBuilder(remain, "초뒤 입력 가능");
                    
                    if (remain <= 0)
                    {
                        if (blockButton != null)
                            blockButton.gameObject.SetActive(false);

                        timeObj.Refresh = null;
                        chatDelayTimeStamp = 0;
                    }
                };
            }
        }

        // 1:1 채팅중일 경우 챗룸으로 돌아가기 버튼 제어용
        void SetPopupButtonState(bool isActivateBackButton)
        {
            //closeButtonObject.SetActive(!isActivateBackButton);
            ///안보이게 변경.
            closeButtonObject.SetActive(false);
            backButtonObject.SetActive(isActivateBackButton);
        }
    }
}
