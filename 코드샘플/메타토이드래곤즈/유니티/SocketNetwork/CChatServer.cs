using SBPacketLib;
using SBSocketClientLib;
using System;
using System.Collections;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

namespace SandboxNetwork
{
    public class CChatServer : SBTcpClientService
    {
        public CChatServer() : base()
        {
        }

        const int SERVER_CONNECT_TRY_MAX_COUNT = 3;     // 소켓 재접속 최대 시도 횟수
        const int SERVER_CONNECT_WAIT_MAX_COUNT = 3;    // 소켓 재접속 시도 후 최대 대기 횟수

        const float SERVER_CONNECT_DELAY_TIME = 1.0f;
        //public string SERVER_IP { get; private set; } = "192.168.1.25";  //192.168.1.25, 3000   //"221.146.242.2" , 52530

        private IEnumerator connectCo = null;

        bool isTryingSocketConnect = false;     // 현재 소켓 재접속 시도중인지 확인용
        bool isSocketConnectFail = false;       // 소켓 연결 시도 실패 여부 확인용
        int connectTryCount = 0;                 // 소켓 재접속 시도 횟수
        int connectWaitCount = 0;               // 소켓 재접속 시도 후 대기 횟수

        bool isInitChatServer = false;          // 채팅 서버 최초init 상태확인

        bool isFirst = true;
        float reconnectDelay = 0f;
        private NetworkReachability LastState = NetworkReachability.NotReachable;
        private string LastIP { get; set; } = string.Empty;
        string CHAT_SERVER = string.Empty;
        int SERVER_PORT = 0;

        bool IsSystem = false;
        MonoBehaviour CoroutineTargetMono = null;
        public static string ClientIP
        {
            get
            {
                IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
                if (host != null)
                    return host.AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork).ToString();
                else
                    return string.Empty;
            }
        }

        public void Initialize(string server, int port, bool isSystem = false)
        {
            CHAT_SERVER = server;
            SERVER_PORT = port;
            IsSystem = isSystem;

            isInitChatServer = false;
            ResetConnectCount();
            DisConnect();
            ChatEvent.SendSocketConnectState(ChatEvent.eChatEventEnum.SocketDisconnected);
        }

        public void InitConnectChatServer(MonoBehaviour target = null)
        {
            CoroutineTargetMono = target;
            if (CoroutineTargetMono == null)
                CoroutineTargetMono = SBGameManager.Instance.GameObject.GetComponent<MonoBehaviour>();

            if (connectCo != null)
                CoroutineTargetMono.StopCoroutine(connectCo);

            connectCo = TryConnect(false);
            CoroutineTargetMono.StartCoroutine(connectCo);

            isInitChatServer = true;
        }

        bool IpCheck()
        {
            if (LastState != Application.internetReachability)//DNS 변경확인으로 인해 먼저 확인
            {
                LastState = Application.internetReachability;
                if (isFirst)
                {
                    isFirst = false;
                    return true;
                }

                switch (Application.internetReachability)
                {
                    case NetworkReachability.NotReachable://인터넷 연결 불가
#if DEBUG
                        Debug.Log(SBFunc.StrBuilder("##Chat => 인터넷 연결이 아예 안됩니다."));
#endif
                        return false;
                    case NetworkReachability.ReachableViaCarrierDataNetwork://데이터 네트워크 연결
#if DEBUG
                        Debug.Log(SBFunc.StrBuilder("##Chat => 데이터 네트워크로 전환합니다."));
#endif
                        return false;
                    case NetworkReachability.ReachableViaLocalAreaNetwork://Len 네트워크 연결(ex. WIFI)
#if DEBUG
                        Debug.Log(SBFunc.StrBuilder("##Chat => Len 네트워크로 전환합니다."));
#endif
                        return false;
                    default:
                        break;
                }
            }

            if (LastIP == string.Empty)//최초
            {
                LastIP = ClientIP;
#if DEBUG
                Debug.Log(SBFunc.StrBuilder("##Chat => 최초연결 채팅연결 IP => ", LastIP));
#endif
            }

            if (LastIP != ClientIP)
            {
#if DEBUG
                Debug.Log(SBFunc.StrBuilder("##Chat => IP변경 Current => ", LastIP, ", Next => ", ClientIP));
#endif
                LastIP = ClientIP;
                return false;
            }

            return true;
        }

        /// <summary>
        /// 정상연결 확인용도
        /// </summary>
        public bool Update(float dt)
        {
            if (reconnectDelay > 0f)
                reconnectDelay -= dt;

            // 채팅 Init 상태 확인
            if (false == isInitChatServer)
                return false;

            // 유저 상태 체크
            if (!IsSystem && false == IsUserConnect())
            {
#if DEBUG
                Debug.Log(SBFunc.StrBuilder("##Chat => Update In false == IsUserConnect()"));
#endif
                return false;
            }

            if (false == IpCheck())
            {
#if DEBUG
                Debug.Log(SBFunc.StrBuilder("##Chat => Update In false == IpCheck()"));
#endif
                if (connectCo != null)
                {
#if DEBUG
                    Debug.Log(SBFunc.StrBuilder("##Chat => IP변경으로 인한 재시도 중인 코루틴이 취소됨"));
#endif
                    CoroutineTargetMono.StopCoroutine(connectCo);
                    connectCo = null;
                }
                DisConnect();
                ChatEvent.SendSocketConnectState(ChatEvent.eChatEventEnum.SocketDisconnected);
                //약간의 대기시간을 주기 위해 추가.
                reconnectDelay = SERVER_CONNECT_DELAY_TIME;
                ResetConnectCount();
                return false;
            }

            // 채팅 소켓 서버 연결 상태 체크
            if (IsConnect())
            {
                return true;
            }
            else
            {
                if (isSocketConnectFail || reconnectDelay > 0f) // 재연결 실패처리 or 재연결 대기 딜레이
                {
                    return false;
                }

                if (connectTryCount >= SERVER_CONNECT_TRY_MAX_COUNT) // 최대 시도횟수를 초과했을 경우
                {
#if DEBUG
                    Debug.Log(SBFunc.StrBuilder("##Chat => 최대 시도횟수 초과"));
#endif
                    ChatEvent.SendSocketConnectState(ChatEvent.eChatEventEnum.SocketDisconnected);
                    isSocketConnectFail = true;
                    return false;
                }
                else if (isTryingSocketConnect == false)
                {
#if DEBUG
                    Debug.Log(SBFunc.StrBuilder("##Chat => 재연결 시도 Last => ", LastIP, ", Current => ", ClientIP));
#endif
                    ChatEvent.SendSocketConnectState(ChatEvent.eChatEventEnum.SocketTryConnect);

                    if (connectCo != null)
                    {
                        CoroutineTargetMono.StopCoroutine(connectCo);
                        connectCo = null;
                    }

                    connectCo = TryConnect(true);
                    CoroutineTargetMono.StartCoroutine(connectCo);
                }
            }

#if UNITY_EDITOR
            CheatSocketConnetTest();
#endif
            return false;
        }

#if UNITY_EDITOR
        void CheatSocketConnetTest()
        {
            // 소켓 연결 강제 해제
            if (Input.GetKey(KeyCode.Minus))
            {
                DisConnect();
            }

            // 소켓 재연결
            if (Input.GetKey(KeyCode.Plus))
            {
                if (isTryingSocketConnect == false)
                {
                    ChatEvent.SendSocketConnectState(ChatEvent.eChatEventEnum.SocketTryConnect);

                    if (connectCo != null)
                        CoroutineTargetMono.StopCoroutine(connectCo);

                    connectCo = TryConnect(true);
                    CoroutineTargetMono.StartCoroutine(connectCo);
                }
            }
        }
#endif

        public bool IsSocketConnect()
        {
            if (!IsConnect())
            {
                ToastManager.On(100002525);
                //InitConnectChatServer();  // 재접속시도를 명시적으로 구분하여 처리하기 위해 주석처리
                return false;
            }

            return true;
        }

        bool IsUserConnect()
        {
            return User.Instance.UserAccountData.UserNumber > 0;
        }

        void ConnectChatServer()
        {
            Connect(CHAT_SERVER, SERVER_PORT, connectWaitCount);
        }

        void ReconnectChatServer()
        {
            ReConnent(CHAT_SERVER, SERVER_PORT, connectWaitCount);
        }

        IEnumerator TryConnect(bool isReconnect)
        {
            var chatpopup = PopupManager.GetPopup<ChattingPopup>();

            isTryingSocketConnect = true;

            if (!IsSystem && !IsUserConnect())
            {
#if DEBUG
                Debug.Log(SBFunc.StrBuilder("##Chat => TryConnect(", isReconnect, ") In false == IsUserConnect"));
#endif
                isTryingSocketConnect = false;
                yield break;
            }

            // 재연결인지에 대한 분기 처리
            if (isReconnect)
            {
                //CChatServer.Instance.DisConnect();  // 일단 완전히 끊어낸 후 재연결
                ReconnectChatServer();
            }
            else
            {
                ConnectChatServer();
            }

            connectTryCount++;

#if DEBUG
            Debug.Log("##Chat SocketServer Connected Try Count :: " + connectWaitCount);
#endif

            yield return SBDefine.GetWaitForSecondsRealtime(0.5f);

            while (connectWaitCount <= SERVER_CONNECT_WAIT_MAX_COUNT)
            {
                if (IsConnect())
                {
#if DEBUG
                    Debug.Log("##Chat SocketServer Connected");
#endif
                    Send(PacketToBytes.Make(ChatServer_PacketID.CSAuth, new CSAuth()
                    {
                        UserUID = User.Instance.UserAccountData.UserNumber,
                        ServerTag = NetworkManager.ServerTag,
                    }));

                    ResetConnectCount();
                    ChatEvent.SendSocketConnectState(ChatEvent.eChatEventEnum.SocketConnected);

                    yield break;
                }
#if DEBUG
                Debug.Log("##Chat SocketServer Connected Wait Count :: " + connectWaitCount);
#endif
                connectWaitCount++;
                yield return SBDefine.GetWaitForSecondsRealtime(SERVER_CONNECT_DELAY_TIME);
            }

            connectWaitCount = 0;
            isTryingSocketConnect = false;

#if DEBUG
            Debug.Log("##Chat SocketServer Connected failed!!  [Connect End]");
#endif
        }

        public void SendMessage(ChatDataInfo chatData)
        {
            if (!IsSocketConnect())
            {
#if DEBUG
                Debug.Log(SBFunc.StrBuilder("##Chat CCConnectSocket => SendMessage In false == IsSocketConnect"));
#endif
                return;
            }

            if (chatData.Comment.Contains(ChatManager.CHAT_WHISPER_CHECKREQ))
                chatData.Comment.Replace(ChatManager.CHAT_WHISPER_CHECKREQ, ChatManager.CHAT_WHISPER_CHECKREQ + " ");
            if (chatData.Comment.Contains(ChatManager.CHAT_WHISPER_CHECKRES))
                chatData.Comment.Replace(ChatManager.CHAT_WHISPER_CHECKRES, ChatManager.CHAT_WHISPER_CHECKRES + " ");

            Send(PacketToBytes.Make(ChatServer_PacketID.CSChatMessage, new CSChatMessage()
            {
                ChatType = (byte)chatData.CommentType,
                SendUserUID = chatData.SendUID,
                RecvUserUID = chatData.RecvUID,
                SendUserName = chatData.SendNickname,
                SendIcon = chatData.SendIcon,
                SendUserGuildUID = chatData.SendUserGuildUID,
                SendUserLastEnterTimeStamp = chatData.SendUserLastEnterTimestamp,
                SendPortraitType = Convert.ToInt64(chatData.PortraitType),
                SendPortraitValue = Convert.ToInt64(chatData.PortraitValue),
                Message = chatData.Comment,
                CurrTimeStamp = chatData.Time,
                ServerTag = NetworkManager.ServerTag,
            }));
        }

        public void SendCheckMessage(ChatDataInfo chatData)
        {
            if (!IsSocketConnect())
            {
#if DEBUG
                Debug.Log(SBFunc.StrBuilder("##Chat CCConnectSocket => SendMessage In false == IsSocketConnect"));
#endif
                return;
            }

            Send(PacketToBytes.Make(ChatServer_PacketID.CSChatMessage, new CSChatMessage()
            {
                ChatType = (byte)chatData.CommentType,
                SendUserUID = chatData.SendUID,
                RecvUserUID = chatData.RecvUID,
                SendUserName = chatData.SendNickname,
                SendIcon = chatData.SendIcon,
                SendUserGuildUID = chatData.SendUserGuildUID,
                SendUserLastEnterTimeStamp = chatData.SendUserLastEnterTimestamp,
                SendPortraitType = Convert.ToInt64(chatData.PortraitType),
                SendPortraitValue = Convert.ToInt64(chatData.PortraitValue),
                Message = chatData.Comment,
                CurrTimeStamp = chatData.Time,
                ServerTag = NetworkManager.ServerTag,
            }));
        }

        public void ResetConnectCount()
        {
            isTryingSocketConnect = false;
            isSocketConnectFail = false;
            connectTryCount = 0;
            connectWaitCount = 0;
        }
    }
}
