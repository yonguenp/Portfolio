using Google.Impl;
using SBPacketLib;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class ChattingPopup : Popup<ChattingPopupData>, EventListener<ChatEvent>, EventListener<GuildEvent>
    {
        [SerializeField] RectTransform safeAreaRect = null;//좌측 화면(화면 절반) 앵커로 인해서 화면 줄이기 만들어야함
        [SerializeField] ChatTabController tabController = null;

        [SerializeField] GameObject chatBlockLayerObject = null;
        [SerializeField] Text chatBlockGuideText = null;


        private void Awake()
        {
            EventManager.AddListener<ChatEvent>(this);
            EventManager.AddListener<GuildEvent>(this);
        }

        private void OnDestroy()
        {
            EventManager.RemoveListener<ChatEvent>(this);
            EventManager.RemoveListener<GuildEvent>(this);
        }
        
        public override void ClosePopup()
        {
            if (PopupManager.GetPopup<ChattingProfilePopup>() != null)
                PopupManager.ClosePopup<ChattingProfilePopup>();
            base.ClosePopup();
        }


        public override void ForceUpdate(ChattingPopupData data = null)
        {
            tabController?.RefreshTab();
        }

        public override void InitUI()
        {
            if (tabController == null)
            {
                return;
            }

            tabController.InitTab(0);

            chatBlockLayerObject?.SetActive(false);
            tabController.chatLayer.gameObject.SetActive(false);
            if (false == FriendManager.IsLoaded)
            {
                FriendManager.Instance.FriendList();
            }

            // 소켓 연결상태 확인
            if (ChatManager.Instance.IsConnect() == false)
            {
                ChatManager.Instance.ResetConnectCount();    // 재연결 시도 가능하도록 관련 값 초기화
            }
            else
            {
                SendPing();
            }
        }

        public void SetDirectChat(ChatDataInfo chatData, int returnTab)
        {
            SetDirectChat(new ThumbnailUserData(chatData), returnTab);
        }

        public void SetDirectChat(ThumbnailUserData otherUser, int returnTab)
        {
            ChangeTab(returnTab);

            ChatManager.Instance.EnterOneOnOne(otherUser == null ? 0 : otherUser.UID, () =>
            {
                tabController.CloseAllTab();

                tabController.chatLayer.gameObject.SetActive(true);
                tabController.SetReturnTab(returnTab);
                tabController.chatLayer.SetChatTarget(otherUser);  // 유저 id가 먼저 할당돼 있어야 함
            });
        }

        public void ChangeTab(int tabIndex)
        {
            tabController.InitController();
            tabController.chatLayer.InitUI();
            tabController.ChangeTab(tabIndex);
            tabController.chatLayer.gameObject.SetActive(false);
        }

        public void SetDirectFriendListLayer()
        {
            tabController.chatLayer.InitUI();
            tabController.chatLayer.gameObject.SetActive(false);
            //tabController.CloseAllTab();
            tabController.InitTab(2);
        }
        public void SetDirectGuildChatLayer()
        {
            tabController.chatLayer.InitUI();
            tabController.chatLayer.gameObject.SetActive(false);
            //tabController.CloseAllTab();
            tabController.InitTab(3);
        }

        public void RefreshSafeArea()//해상도 변경 대응코드 (safe_area가 앵커 계산을 따로 안해줘서 추가로 해줌)
        {
            if (safeAreaRect == null)
                return;

            Vector2 currentAnchorMin = safeAreaRect.anchorMin;
            safeAreaRect.anchorMin = new Vector2(currentAnchorMin.x * 2, currentAnchorMin.y);

            //원래는 팝업 진입시에 최초 한번 타지만 디버깅 때문에 탭 강제 갱신코드 추가 - 지울 것
            tabController?.RefreshTab();
        }


        public void OnClickUpdateGuideButton()
        {
            ToastManager.On(StringData.GetStringByStrKey("system_message_update_01"));
        }

        public void OnEvent(ChatEvent eventType)
        {
            switch (eventType.Event)
            {
                case ChatEvent.eChatEventEnum.SocketConnected:
                    chatBlockLayerObject.SetActive(false);
                    SendPing();
                    break;
                case ChatEvent.eChatEventEnum.SocketTryConnect:
                    chatBlockLayerObject.SetActive(true);
                    chatBlockGuideText.text = StringData.GetStringByStrKey("chat_server_try_connect");
                    break;
                case ChatEvent.eChatEventEnum.SocketDisconnected:
                    chatBlockLayerObject.SetActive(true);
                    chatBlockGuideText.text = StringData.GetStringByIndex(100002525);
                    break;
                case ChatEvent.eChatEventEnum.RefreshUI:
                    tabController.RefreshTitle();
                    break;
            }
        }

        public void SendPing()
        {
            CancelInvoke("SendPing");
            CancelInvoke("NoPong");

            ChatManager.Instance.Send(PacketToBytes.Make(ChatServer_PacketID.CSPing, new CSPing()
            {
                UserUID = User.Instance.UserAccountData.UserNumber,
                Timestamp = SBFunc.GetNowDateTimeToTimeStamp(),
                ServerTag = NetworkManager.ServerTag,
            }));
            Invoke("NoPong", 3.0f);
        }

        public void RecvPong(long time)
        {
            CancelInvoke("SendPing");
            CancelInvoke("NoPong");

            Invoke("SendPing", 10.0f);
        }

        public void NoPoing()
        {
            ChatManager.Instance.Disconnect();
            ChatManager.Instance.ResetConnectCount();
        }

        public void OnEvent(GuildEvent eventType)
        {
            switch (eventType.Event)
            {
                case GuildEvent.eGuildEventType.GuildRefresh:
                    break;
                case GuildEvent.eGuildEventType.LostGuild:
                    if (gameObject.activeInHierarchy)
                    {
                        InitUI();
                    }
                    break;
            }

        }
    }
}
