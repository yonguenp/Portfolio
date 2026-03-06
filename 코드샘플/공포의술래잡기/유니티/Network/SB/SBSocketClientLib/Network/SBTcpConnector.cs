using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using SBCommonLib;

/// <summary>
/// SandBox Socket Client Library
/// </summary>
namespace SBSocketClientLib
{
    public class CustomUserToken
    {
#if DEBUG
        public int Index { get; private set; }
#endif
        public Socket Socket { get; private set; }
        public int TryConnectCount { get; set; }

#if DEBUG
        public CustomUserToken(Socket socket_, int index_)
        {
            Index = index_;
            Socket = socket_;
            TryConnectCount = 0;
        }
#else
        public CustomUserToken(Socket socket_)
        {
            Socket = socket_;
            TryConnectCount = 0;
        }
#endif
    }

    /// <summary>
    /// TCP Connector 클래스
    /// </summary>
    public class SBTcpConnector
    {
        private const int kSleepTime = 3000;
        private Func<Session> _sessionFactory;
        private SBAtomicInt _maxNumberAcceptedClients = new SBAtomicInt();

        /// <summary>
        /// TCP 클라이언트 서비스 객체
        /// </summary>
        //private SBTcpClientService _tcpService = null;

        public Session Session { get; private set; }

        /// <summary>
        /// 연결 시도 횟수
        /// </summary>
        //private int _tryConnectCount = 0;

        /// <summary>
        /// Connector 이름
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// 연결 IP 주소
        /// </summary>
        public string ConnectIp { get; private set; }

        /// <summary>
        /// 연결 Port 번호
        /// </summary>
        public int ConnectPort { get; private set; }

        /// <summary>
        /// 최대 연결 시도 횟수
        /// </summary>
        public int MaxTryConnectCount { get; private set; }

        public SBTcpConnector(string name_)
        {
            Name = name_;
        }

        /// <summary>
        /// 연결 정보 설정
        /// </summary>
        /// <param name="tcpService_">TCP 클라이언트 서비스 객체</param>
        /// <param name="connectIp_">연결 IP 주소</param>
        /// <param name="connectPort_">연결 Port 번호</param>
        /// <param name="maxTryConnectCount_">최대 연결 시도 횟수</param>
        //public void SetConnectorInfo(SBTcpClientService tcpService_, string connectIp_, int connectPort_, int maxTryConnectCount_)
        //{
        //    _tcpService = tcpService_;
        //    ConnectIp = connectIp_;
        //    ConnectPort = connectPort_;
        //    MaxTryConnectCount = maxTryConnectCount_;
        //}

        /// <summary>
        /// 연결 정보 설정
        /// </summary>
        /// <param name="connectIp_">연결 IP 주소</param>
        /// <param name="connectPort_">연결 Port 번호</param>
        /// <param name="maxTryConnectCount_">최대 연결 시도 횟수</param>
        public void SetConnectorInfo(string connectIp_, int connectPort_, int maxTryConnectCount_)
        {
            ConnectIp = connectIp_;
            ConnectPort = connectPort_;
            MaxTryConnectCount = maxTryConnectCount_;
        }

        /// <summary>
        /// 연결 정보 설정
        /// </summary>
        /// <param name="connectIp_">연결 IP 주소</param>
        /// <param name="connectPort_">연결 Port 번호</param>
        public void SetConnectorInfo(string connectIp_, int connectPort_)
        {
            ConnectIp = connectIp_;
            ConnectPort = connectPort_;
        }

        /// <summary>
        /// 연결 시도 횟수 초기화
        /// </summary>
        //public void ResetTryConnectCount()
        //{
        //    _tryConnectCount = 0;
        //}

        /// <summary>
        /// Called when [connected].
        /// </summary>
        /// <param name="socket_">소켓</param>
        
        protected virtual void OnConnected(Session session_)
        {
            SBLog.PrintTrace($"[SBTcpConnector] Client-OnConnected: {session_.Channel.RemoteEndPoint}");

        }

        /// <summary>
        /// Called when [fail connect].
        /// </summary>
        public virtual void OnFailConnect()
        {
            SBLog.PrintError($"[SBTcpConnector] Called OnFailConnect. Connect fail.");
        }

        /// <summary>
        /// 연결하기
        /// </summary>
        public void Connect(Func<Session> sessionFactory_, int count_ = 1)
        {
            if (string.IsNullOrWhiteSpace(ConnectIp))
            {
                SBLog.PrintError($"[SBTcpConnector] Connect - ConnectIp string is null.");
                return;
            }

            IPAddress hostIp = null;

            if (false == IPAddress.TryParse(ConnectIp, out hostIp))
            {
                hostIp = Dns.GetHostEntry(ConnectIp).AddressList[0];
            }

            if (null == hostIp)
            {
                SBLog.PrintError($"[SBTcpConnector] Connect - Host Ip Error(null)");
                return;
            }

            _sessionFactory = sessionFactory_;
            for (int i = 0; i < count_; ++i)
            {
                Socket socket = new Socket(hostIp.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                SocketAsyncEventArgs args = new SocketAsyncEventArgs();
                args.Completed += new EventHandler<SocketAsyncEventArgs>(OnConnectCompleted);
                args.RemoteEndPoint = new IPEndPoint(hostIp, ConnectPort);
#if DEBUG
                args.UserToken = new CustomUserToken(socket, _maxNumberAcceptedClients.Incrementer());
#else
                args.UserToken = new CustomUserToken(socket);
#endif

                //socket.ConnectAsync(socketEventArg);
                RegisterConnect(args);
            }
        }

        public void RegisterConnect(SocketAsyncEventArgs args_)
        {
            CustomUserToken customToken = args_.UserToken as CustomUserToken;
            Socket socket = customToken.Socket;
            
            if (null == socket)
                return;
#if DEBUG
            SBLog.PrintTrace($"Idx#{customToken.Index} client request to connect server.");
#endif
            bool pending = socket.ConnectAsync(args_);
            if (false == pending)
                OnConnectCompleted(null, args_);
        }

        /// <summary>
        /// 연결 콜백함수
        /// </summary>
        /// <param name="sender_">Sender 객체</param>
        /// <param name="args_">SocketAsyncEventArgs</param>
        public virtual void OnConnectCompleted(object sender_, SocketAsyncEventArgs args_)
        {
            if (SocketError.Success == args_.SocketError)
            {
#if DEBUG
                CustomUserToken customToken = args_.UserToken as CustomUserToken;
                SBLog.PrintTrace($"Idx#{customToken.Index} client success to connect server.");

                //if (sender_ != null)
                //{
                //    SBLog.PrintTrace($"Idx#{customToken.Index} client: {(((Socket)sender_ == args_.ConnectSocket) ? "Same Socket!" : "Different Socket!")}");
                //}

                //OnConnected(args_);
                Session = _sessionFactory.Invoke(); //new TcpSession(socket_);
                Session.Start(args_.ConnectSocket);
                Session.Listen();

                OnConnected(Session);


#else
                // 연결 성공하면 후처리 진행. (1, 2, 3 중 어떤 것을 사용해도 무방)
                // 1.
                //Socket connectedSocket = (sender_ != null) ? (Socket)sender_ : args_.ConnectSocket; // ConnectAsync()가 즉시 처리되어 pending이 false일 때 sender_가 null인 경우를 대비하기 위함. 
                //OnConnected(connectedSocket);
                // 2.
                //Socket connectedSocket = args_.ConnectSocket;
                //OnConnected(connectedSocket);
                // 3.
                //OnConnected(args_.ConnectSocket);
                Session = _sessionFactory.Invoke(); //new TcpSession(socket_);
                Session.Start(args_.ConnectSocket);
                Session.Listen();

                OnConnected(Session);
#endif
            }
            else
            {
                CustomUserToken customToken = args_.UserToken as CustomUserToken;
#if DEBUG
                SBLog.PrintWarn($"[SBTcpConnector] OnConnectCompleted - Failed to connect. Index: {customToken.Index}, reason: {args_.SocketError}");
#else
                SBLog.PrintWarn($"[SBTcpConnector] OnConnectCompleted - Failed to connect. reason: {args_.SocketError}");
#endif

                // 연결 시도 횟수 체크 후 최대 횟수를 초과했을 경우엔 최종 실패 처리.
                if ((0 < MaxTryConnectCount) && (customToken.TryConnectCount >= MaxTryConnectCount))
                {
#if DEBUG
                    SBLog.PrintWarn($"[SBTcpConnector] OnConnectCompleted - 연결 시도 횟수 초과(Index: {customToken.Index}, TryConnect: {customToken.TryConnectCount}, Max: {MaxTryConnectCount})");
#else
                    SBLog.PrintWarn($"[SBTcpConnector] OnConnectCompleted - 연결 시도 횟수 초과(TryConnect: {customToken.TryConnectCount}, Max: {MaxTryConnectCount})");
#endif
                    OnFailConnect();
                    return;
                }

                // Retry
                // 연결 실패시 재시도 횟수만큼 다시 시도함.
                Socket socket = customToken.Socket; // (sender_ != null) ? (Socket)sender_ : args_.UserToken as Socket;
                if (null == socket || false == socket.Connected)
                {
                    if (0 < MaxTryConnectCount)
                    {
                        ++customToken.TryConnectCount;
                        args_.UserToken = customToken;
#if DEBUG
                        SBLog.PrintTrace($"Idx#{customToken.Index} client to connect retry. (#{customToken.TryConnectCount})");
#endif
                    }

                    Thread.Sleep(kSleepTime);
                    RegisterConnect(args_);
                    return;
                }
            }
#if false
            if (args_.SocketError != SocketError.Success)
            {
                CustomUserToken customToken = args_.UserToken as CustomUserToken;
                // 연결 시도 횟수 체크 후 최대 횟수를 초과했을 경우엔 최종 실패 처리.
                if ((0 < MaxTryConnectCount) && (customToken.TryConnectCount >= MaxTryConnectCount))
                {
                    SBLog.PrintWarn($"[SBTcpConnector] OnConnectCompleted - 연결 시도 횟수 초과(Index: {customToken.Index}, TryConnect: {customToken.TryConnectCount}, Max: {MaxTryConnectCount})");
                    OnFailConnect();
                    return;
                }

                // 연결 실패 시
                SBLog.PrintWarn($"[SBTcpConnector] OnConnectCompleted - Failed to connect. Index: {customToken.Index}, reason: {args_.SocketError}");

                // Retry
                // 연결 실패시 재시도 횟수만큼 다시 시도함.
                Socket socket = customToken.Socket; // (sender_ != null) ? (Socket)sender_ : args_.UserToken as Socket;
                if (null == socket || false == socket.Connected)
                {
                    if (0 < MaxTryConnectCount)
                    {
                        ++customToken.TryConnectCount;
                        args_.UserToken = customToken;
                    }

                    Thread.Sleep(kSleepTime);
                    RegisterConnect(args_);
                    return;
                }
            }
            else
            {
                Socket connectedSocket = (sender_ != null) ? (Socket)sender_ : args_.ConnectSocket;

                // 연결 성공하면 후처리 진행.
                OnConnected(connectedSocket);
            }
#endif
        }
    }
}
