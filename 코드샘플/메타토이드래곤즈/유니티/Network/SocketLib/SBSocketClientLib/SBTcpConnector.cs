using System;
using System.Linq;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace SBSocketClientLib
{
    public class SBTcpConnector
    {

        public Socket? Sock { get; private set; } = null;
        public string ServerIP { get; private set; } = "127.0.0.1";
        public int ServerPort { get; private set; } = 0;

        private int _maxTryConnect = 1;
        private int _connectCnt = 0;
        private bool _isConnect = false;

        public SBTcpConnector()
        {
            
        }

        public void Connect(string ip_, int port_, int tryConnet_)
        {
            ServerIP = ip_;
            ServerPort = port_;
            _maxTryConnect = tryConnet_;

            _isConnect = false;

            
            IPAddress serverIP = null;
            if (false == IPAddress.TryParse(ServerIP, out serverIP))
            {
                switch (UnityEngine.Application.internetReachability)
                {
                    case UnityEngine.NetworkReachability.NotReachable://인터넷 연결 불가
#if DEBUG
                        UnityEngine.Debug.Log("##ChatConnect => 인터넷 연결이 끊어져 DNS 호출 불가능합니다.");
#endif
                        ShutDown();
                        return;
                    default:
                        break;
                }
                var address = Dns.GetHostAddresses(ServerIP);
                if(address == null || address.Length < 1)
                {
#if DEBUG
                    UnityEngine.Debug.Log(SandboxNetwork.SBFunc.StrBuilder("##ChatConnector Address Error, ServerIP => ", ServerIP));
#endif
                    return;
                }
#if DEBUG
                UnityEngine.Debug.Log(SandboxNetwork.SBFunc.StrBuilder("##ChatConnector Domain => ", SandboxNetwork.NetworkManager.CHAT_SERVER, ", HostName => ", Dns.GetHostName(), ", ServerIP => ", ServerIP, ", ServerPort => ", ServerPort, ", Address => ", address[0]));
#endif
                serverIP = address[0];
            }

            if (null == serverIP)
            {
#if DEBUG
                UnityEngine.Debug.Log(SandboxNetwork.SBFunc.StrBuilder("##ChatConnector Address null, ServerIP => ", ServerIP));
#endif
                //error
                return;
            }

#if DEBUG
            if (Sock != null)
            {
                UnityEngine.Debug.Log(SandboxNetwork.SBFunc.StrBuilder("##ChatConnector 이곳에 오면 Close없이 재연결 시도 구멍을 찾아야함."));
            }
#endif
            ShutDown();
            Sock = new Socket(serverIP.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            args.Completed += new EventHandler<SocketAsyncEventArgs>(OnConnectCompleted);
            args.RemoteEndPoint = new IPEndPoint(serverIP, ServerPort);
            //

            RegisterConnect(args);

        }

        private void RegisterConnect(SocketAsyncEventArgs args_)
        {
            try
            {
                if (Sock == null)
                {
                    Console.WriteLine($"RegisterConnect. sock is null");
                    return;
                }

                bool pending = Sock.ConnectAsync(args_);

                if (false == pending)
                    OnConnectCompleted(null, args_);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"RegisterConnect. message : {ex.Message}");
            }
        }


        private void OnConnectCompleted(object? sender_, SocketAsyncEventArgs args_)
        {
            if (SocketError.Success == args_.SocketError || SocketError.IsConnected == args_.SocketError)
            {
                //접속 성공
                Console.WriteLine($"서버 연결 성공");
                _connectCnt = 0;
                _isConnect = true;
            }
            else
            {
                _connectCnt++;

                if (_maxTryConnect <= _connectCnt)
                {
                    //wo접속 시도 끝
                    Console.WriteLine($"서버 연결을 시도 하지 않습니다.");
                    return;
                }

                //재접속 시도             
                Console.WriteLine($"서버 연결 실패");

                Thread.Sleep(3000);
                RegisterConnect(args_);

            }
        }

        public void ReConnect(string ip_, int port_, int tryConnet_)
        {
            //초기화 필요
            _connectCnt = 0;
            _isConnect = false;

            Connect(ip_, port_, tryConnet_);
        }


        public Tuple<int, byte[]>? Receive()
        {

            try
            {
                if(Sock == null)
                {
                    //소켓이 null이다. 처리 관련은 고민이 필요
                    return null;
                }

                byte[] ReadBuffer = new byte[2048];
                var nRecv = Sock.Receive(ReadBuffer, 0, ReadBuffer.Length, SocketFlags.None);

                if (nRecv == 0)
                {
                    return null;
                }

                return Tuple.Create(nRecv, ReadBuffer);
            }
            catch (SocketException se)
            {
                //LatestErrorMsg = se.Message;
            }

            return null;
        }


        //스트림에 쓰기
        public void Send(byte[] sendData)
        {
            try
            {
                if (Sock != null && Sock.Connected) //연결상태 유무 확인
                {
                    Sock.Send(sendData, 0, sendData.Length, SocketFlags.None);
                }
                else
                {
                    //LatestErrorMsg = "먼저 채팅서버에 접속하세요!";
                }
            }
            catch (SocketException se)
            {
                //LatestErrorMsg = se.Message;
            }
        }

        //소켓과 스트림 닫기
        public void Close()
        {
            if (Sock != null)
            {
                //Sock.Shutdown(SocketShutdown.Both);
                Sock.Close();                
            }
            Sock = null;
        }
        public void ShutDown()
        {
            if (Sock != null)
            {
                Sock.Shutdown(SocketShutdown.Both);
                Sock.Close();
            }
            Sock = null;
        }

        public bool IsSock() { return  Sock == null ? true : false; }
        public bool IsConnected() { return (Sock != null && Sock.Connected) == true ? _isConnect : false; }

    }

}
