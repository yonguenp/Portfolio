using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using SBCommonLib;
using SBSocketPacketLib;
using UnityEngine;

/// <summary>
/// SandBox Socket Client Library
/// </summary>
namespace SBSocketClientLib
{
#if true
    #region SocketAsyncEventArgs(SAEA) 용
    /// <summary>
    /// TCP 클라이언트 세션 클래스
    /// </summary>
    public abstract class SBTcpSession : Session
    {
        // for Test
        public int CheckMsgPacket { get; set; } = 0;

        public bool IsEmptySessionID
        {
            get => new Guid(SessionID) == Guid.Empty ? true : false;
        }

        public SBTcpSession(int recvBufferSize_, int maxPacketSize_, byte[] cryptKey_, byte[] cryptIV_) : base(recvBufferSize_, maxPacketSize_, cryptKey_, cryptIV_)
        { }

        public byte[] GetSessionIdBytes()
        {
            return new Guid(SessionID).ToByteArray();
        }

        public void SetSessionID(byte[] sessionId_)
        {
            SessionID = new Guid(sessionId_).ToString();
        }

        // [Size(2)][PacketId(2)][SessionId(16)][Data(@)][...][Size(2)][PacketId(2)][SessionId(16)][Data(@)][...]
        public sealed override int OnRecv(ArraySegment<byte> buffer_)
        {
            int processLen = 0;
            int packetCount = 0;

            //if (SessionNo == 1)
            //    SBLog.PrintTrace($"[OnRecv] sessionId: {SessionID}, sessionNo: {SessionNo}, Array: {buffer_.Array.Length}, Offset: {buffer_.Offset}, Count: {buffer_.Count}, IsLittleEndian: {BitConverter.IsLittleEndian}");

            while (true)
            {
                // 최소한 헤더는 파싱할 수 있는지 확인(Size)
                if (buffer_.Count < SBPacket.kHeaderSize)
                    break;

                // 패킷이 완전체로 도착했는지 확인
                // 헤더에서 데이터 크기를 가져옴.
                ushort dataSize = BitConverter.ToUInt16(buffer_.Array, buffer_.Offset);
                //if (SessionNo == 1)
                //    SBLog.PrintTrace($"[OnRecv] sessionId: {SessionID}, sessionNo: {SessionNo}, Array: {buffer_.Array.Length}, Offset: {buffer_.Offset}, Count: {buffer_.Count}, dataSize: {dataSize}, IsLittleEndian: {BitConverter.IsLittleEndian}");
                // 버퍼의 크기가 헤더와 데이터 크기를 합친 것 보다 작으면
                // 전체 메시지가 온 것이 아니기 때문에 더 이상 진행하지 않음.
                if (buffer_.Count < SBPacket.kHeaderSize + dataSize)
                    break;

                // 여기까지 왔으면 패킷 조립 가능
                // 패킷이 해당하는 영역만 잘라서 넘겨줌.
                // 이때 Size 헤더 부분을 포함할지 말지는 케바케. 알아서 선택하면 됨.
                // 전체 패킷 사이즈를 알아야 할 때는 Size 헤더도 함께 넘겨주는게 좋음.
                // 우리는 Size 헤더 부분을 제외한 데이터 부분만 잘라서 넘김.
                // 1.
                // SBDebug.Log($"[SBTcpSession] OnRecv - array: {buffer_.Array.Length}, offset: {buffer_.Offset}, count: {buffer_.Count} ( headerSize: {SBPacket.kHeaderSize}, dataSize: {dataSize} )");
                //SBLog.PrintTrace($"[SBTcpSession] array: {buffer_.Array.Length}, offset: {buffer_.Offset}, count: {buffer_.Count}, dataSize: {dataSize}");
                OnRecvPacket(new ArraySegment<byte>(buffer_.Array, buffer_.Offset + SBPacket.kHeaderSize, dataSize));

                packetCount++;
                processLen += (SBPacket.kHeaderSize + dataSize);
                buffer_ = new ArraySegment<byte>(buffer_.Array, buffer_.Offset + SBPacket.kHeaderSize + dataSize, buffer_.Count - (SBPacket.kHeaderSize + dataSize));
                //if (SessionNo == 1)
                //    SBLog.PrintTrace($"[OnRecv] sessionId: {SessionID}, sessionNo: {SessionNo}, new buffer - Array: {buffer_.Array.Length}, Offset: {buffer_.Offset}, Count: {buffer_.Count}");
                //SBDebug.Log($"[SBTcpSession] OnRecv - array: {buffer_.Array.Length}, offset: {buffer_.Offset}, count: {buffer_.Count}");
                //SBLog.PrintTrace($"[SBTcpSession] array: {buffer_.Array.Length}, offset: {buffer_.Offset}, count: {buffer_.Count}");
            }

            //if (packetCount > 0 && processLen > 64 && SessionNo == 1)
            // SBDebug.Log($"SessionId: {SessionID}, Recieved packet count: {packetCount}, processed packet size: {processLen}, processMsg: {CheckMsgPacket}");

            return processLen;
        }

        /// <summary>
        /// 패킷 처리 함수
        /// </summary>
        /// <param name="packetObject_">패킷 객체</param>
        public void PacketHandler(object packetObject_)
        {
            SBPacket packet = (SBPacket)packetObject_;
            SBDebug.Log($"[SBTcpSession] PacketHandler(). packet info - length: {packet.Length}, id: {packet.PacketId}, sessionId: {Session.ConvertSessionIdByteToString(packet.SessionId)}");
            SBLog.PrintTrace($"[SBTcpSession] packet info - length: {packet.Length}, id: {packet.PacketId}, sessionId: {Session.ConvertSessionIdByteToString(packet.SessionId)}");
            OnRecvPacket(packet.GetBytesWithoutSizeField());
        }

        public abstract void OnRecvPacket(ArraySegment<byte> buffer_);
    }

    public abstract class Session
    {
        //private Socket _socket;
        private int _disconnected = 0;
        private int _maxPacketSize = 0;

        //private int _recvBufferSize = 4096;
        private SBTcpRecvBuffer _recvBuffer = null;// = new SBTcpRecvBuffer(65535);
        private object _lock = new object();
        private Queue<ArraySegment<byte>> _sendQueue = new Queue<ArraySegment<byte>>();
        private List<ArraySegment<byte>> _pendingList = new List<ArraySegment<byte>>();
        private SocketAsyncEventArgs _sendArgs = new SocketAsyncEventArgs();
        private SocketAsyncEventArgs _recvArgs = new SocketAsyncEventArgs();

        /// <summary>
        /// Connection 객체
        /// </summary>
        public SBTcpChannel Channel { get; private set; } = null;

        public string SessionID { get; set; }
        public int SessionNo { get; set; }

        public byte[] CryptKey { get; set; } = new byte[SBCrypt.kCryptKeyLen];
        public byte[] CryptIV { get; set; } = new byte[SBCrypt.kCryptIvLen];

        public bool IsPopup{get;set;} = false;

        /// <summary>
        /// 연결 여부
        /// </summary>
        public bool IsConnected
        {
            get => ((null == Channel) ? false : Channel.IsAlive);
        }

        public Session(int recvBufferSize_, int maxPacketSize_, byte[] cryptKey_, byte[] cryptIV_)
        {
            _recvBuffer = new SBTcpRecvBuffer(recvBufferSize_);
            _maxPacketSize = maxPacketSize_;
            CryptKey = cryptKey_;
            CryptIV = cryptIV_;
        }

        public static string ConvertSessionIdByteToString(byte[] sessionId_)
        {
            return new Guid(sessionId_).ToString();
        }

        /// <summary>
        /// Connection 설정 함수
        /// </summary>
        /// <param name="channel_">Connection</param>
        public void SetChannel(SBTcpChannel channel_)
        {
            Channel = channel_;
        }

        /// <summary>
        /// End point IP 가져오기
        /// </summary>
        /// <returns></returns>
        public string GetEndPointIp()
        {
            return (null == Channel) ? "Channel is null" : Channel.GetEndPointIp();
        }

        public string GetRemoteEndPoint()
        {
            return (null == Channel) ? "Channel is null" : Channel.GetRemoteEndPoint();
        }

        /// <summary>
        /// Called when [connected].
        /// </summary>
        protected virtual void OnConnected()
        {
            SBLog.PrintTrace($"[Session] #{SessionNo} Client-OnConnected: {Channel.RemoteEndPoint}");
        }

        /// <summary>
        /// Called when [disconnected].
        /// </summary>
        protected virtual void OnDisconnected()
        {
            SBLog.PrintTrace($"[Session] #{SessionNo} Client-OnDisconnected: {Channel.RemoteEndPoint}");
        }

        public abstract int OnRecv(ArraySegment<byte> buffer_);
        public abstract void OnSend(int numOfPacket_, int numOfBytes_);

        void Clear()
        {
            lock (_lock)
            {
                _sendQueue.Clear();
                _pendingList.Clear();
            }
        }

        public void Start(Socket socket_)
        {
            try
            {
                Channel = new SBTcpChannel(socket_, this, _maxPacketSize, OnConnected, OnDisconnected);
            }
            catch (SocketException e_)
            {
                SBLog.PrintError($"[Session] Constructor - 소켓 오류(Message: {e_.Message})");
            }
            catch (Exception e_)
            {
                SBLog.PrintError($"[Session] Constructor - SBTcpConnection instance 생성 오류(Message: {e_.Message})");
            }
        }

        public virtual void Listen()
        {
            // SAEA / BSBR 상태 변경
            // 현재 소켓을 사용한 메시지 전송 방식은 SocketAsyncEventArgs(이하 SAEA)를 사용한 방식과
            // BeginSend/BeginReceive(이하 BSBR)를 이용한 방식 두가지로 구현되어 있음.
            // 두 방식 모두 테스트를 진행 중임.
            // 두 방식간 전환을 할 때 변경해줘야 하는 변경 점.
            // SBTcpSession의 Listen()에서 원하는 방식의 코드 주석을 해제하고 사용하지 않는 방식의 코드는 주석 처리함.
            // TcpServerSession의 SendPacket()에서 원하는 방식의 코드 주석을 해제하고 사용하지 않는 방식의 코드는 주석 처리함.
            // SAEA
            SBDebug.Log($"Session#{SessionNo} Listen Start");
            SBLog.PrintTrace($"Session#{SessionNo} Listen Start");

            _sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendCompleted);
            _recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnRecvCompleted);

            RegisterRecvAsync();

            // BSBR
            //Channel.BeginReceive(true, null);
        }

#if true
        #region SocketAsyncEventArgs(SAEA) 용 Send
        protected void Send(List<ArraySegment<byte>> sendBuffList_)
        {
            if (0 == sendBuffList_.Count)
                return;

            lock (_lock)
            {
                foreach (ArraySegment<byte> sendBuff in sendBuffList_)
                    _sendQueue.Enqueue(sendBuff);

                if (0 == _pendingList.Count)
                    RegisterSendAsync();
            }
        }

        protected void Send(ArraySegment<byte> sendBuff_)
        {
            lock (_lock)
            {
                _sendQueue.Enqueue(sendBuff_);
                if (0 == _pendingList.Count)
                    RegisterSendAsync();
            }
        }
        #endregion
#else
        #region BeginSend/BeginReceive(BSBR) 용 Send
        protected void Send(SBPacket packet_)
        {
            Channel.Send(packet_);
        }
        #endregion
#endif

        public void TryReconnect(int curCount = 0)
        {
            //todo 패킷이 누락됬을 가능성이 큰 상태에서 
            //현재 유저의 상태 (매칭중, 매칭대기룸, 게임중 등등)을 싱크 하고
            //상태에 따른 상세데이터(타 유저의 데이터, 위치정보, 버프 등등)을 추가로 싱크한뒤 일상게임으로 돌아가는 형태로 변경해될듯함.
            Disconnect();
        }

        public void Disconnect()
        {
            SBDebug.Log($"Session Disconnect");
            if (Interlocked.Exchange(ref _disconnected, 1) == 1)
                return;

            if (Channel != null)
            {
                try
                {
                    OnDisconnected();
                    Channel.Disconnect();
                    Channel = null;
                    Clear();
                    SBDebug.Log($"Session Disconnect Complete");
                }
                catch (Exception e_)
                {
                    SBLog.PrintError($"[Session] Disconnect - Error(Message:{e_.Message})");
                }
            }
        }

        #region SocketAsyncEventArgs(SAEA) 용 네트워크 통신
        void RegisterSendAsync()
        {
            if (1 == _disconnected)
            {
                SBDebug.Log($"RegisterSendAsync _disconnected, IsPopup {IsPopup}");
                if(IsPopup == false) return;
                if (!PopupCanvas.Instance.IsOpeningPopup(PopupCanvas.POPUP_TYPE.MESSAGE_POPUP))
                {
                    SBDebug.Log($"RegisterSendAsync Popup On");
                    PopupCanvas.Instance.ClearAll();

                    PopupCanvas.Instance.ShowMessagePopup(StringManager.GetString("서버연결종료"), () =>
                    {
                        UnityEngine.SceneManagement.SceneManager.LoadScene("Start");
                    });
                }
                return;
            }

            // SendArgs의 Buffer와 BufferList를 동시에 사용할 수 없다.
            // BufferList는 Add를 사용해 그때그때 추가할 수 없고
            // temp에 값을 다 채운 후 한번에 assign 해야 한다.
            //
            // a[][][][][][][][][][]
            //__pendingList.Clear();
            while (_sendQueue.Count > 0)
            {
                ArraySegment<byte> buff = _sendQueue.Dequeue();
                //__sendArgs.BufferList.Add(new ArraySegment<byte>(buff, 0, buff.Length)); // 이렇게 사용할 수 없음.
                _pendingList.Add(buff);
            }
            _sendArgs.BufferList = _pendingList;

            try
            {
                // SAEA
                bool pending = Channel.SendAsync(_sendArgs);
                if (false == pending)
                    OnSendCompleted(null, _sendArgs);
            }
            catch (ArgumentException e_)
            {
                //SBLog.PrintError($"RegisterSend Failed. ArgumentException. sessionId: {SessionID}, error: {e_.ToString()}");
                SBDebug.LogError($"RegisterSend Failed. ArgumentException. sessionId: {SessionID}, error: {e_.Message}");
            }
            catch (ObjectDisposedException e_)
            {
                //SBLog.PrintError($"RegisterSend Failed. ObjectDisposedException. sessionId: {SessionID}, error: {e_.ToString()}");
                SBDebug.LogError($"RegisterSend Failed. ObjectDisposedException. sessionId: {SessionID}, error: {e_.Message}");
            }
            catch (InvalidOperationException e_)
            {
                //SBLog.PrintError($"RegisterSend Failed. InvalidOperationException. sessionId: {SessionID}, error: {e_.ToString()}");
                SBDebug.LogError($"RegisterSend Failed. InvalidOperationException. sessionId: {SessionID}, error: {e_.Message}");
            }
            catch (NotSupportedException e_)
            {
                //SBLog.PrintError($"RegisterSend Failed. NotSupportedException. sessionId: {SessionID}, error: {e_.ToString()}");
                SBDebug.LogError($"RegisterSend Failed. NotSupportedException. sessionId: {SessionID}, error: {e_.Message}");
            }
            catch (SocketException e_)
            {
                //SBLog.PrintError($"RegisterSend Failed. SocketException. sessionId: {SessionID}, error: {e_.ToString()}");
                SBDebug.LogError($"RegisterSend Failed. SocketException. sessionId: {SessionID}, error: {e_.Message}");
            }
            catch (Exception e_)
            {
                //Console.WriteLine($"RegisterSend Failed {e_}");
                //SBLog.PrintError($"RegisterSend Failed. sessionId: {SessionID}, error: {e_.ToString()}");
                SBDebug.LogError($"RegisterSend Failed. sessionId: {SessionID}, error: {e_.Message}");
            }
        }

        void OnSendCompleted(object sender_, SocketAsyncEventArgs args_)
        {
            lock (_lock)
            {
                if (args_.BytesTransferred > 0 && SocketError.Success == args_.SocketError)
                {
                    try
                    {
                        int count = _sendArgs.BufferList.Count;
                        _sendArgs.BufferList = null;
                        _pendingList.Clear();

                        OnSend(count, _sendArgs.BytesTransferred);
                        //SBLog.PrintTrace($"SessionId: {SessionID}, Transferred Packet count: {count}, bytes: {_sendArgs.BytesTransferred}");

                        if (_sendQueue.Count > 0)
                            RegisterSendAsync();
                    }
                    catch (Exception e_)
                    {
                        //Console.WriteLine($"OnSendCompleted Failed {e_.Message}");
                        //Console.WriteLine($"OnSendCompleted Failed. sessionId: {SessionID}, error: {e_.ToString()}");
                        SBLog.PrintError($"OnSendCompleted Failed. sessionId: {SessionID}, error: {e_.ToString()}");
                    }
                }
                else
                {
                    TryReconnect();
                }
            }
        }

        void RegisterRecvAsync()
        {
            if (1 == _disconnected)
            {
                if (!PopupCanvas.Instance.IsOpeningPopup(PopupCanvas.POPUP_TYPE.MESSAGE_POPUP))
                {
                    PopupCanvas.Instance.ClearAll();

                    PopupCanvas.Instance.ShowMessagePopup(StringManager.GetString("서버연결종료"), () =>
                    {
                        UnityEngine.SceneManagement.SceneManager.LoadScene("Start");
                    });
                }
                return;
            }

            _recvBuffer.Clean();
            ArraySegment<byte> segment = _recvBuffer.WriteSegment;

            _recvArgs.SetBuffer(segment.Array, segment.Offset, segment.Count);

            try
            {
                bool pending = Channel.ReceiveAsync(_recvArgs);
                if (false == pending)
                    OnRecvCompleted(null, _recvArgs);
            }
            catch (ArgumentException e_)
            {
                //SBLog.PrintError($"RegisterRecvAsync Failed. ArgumentException. sessionId: {SessionID}, error: {e_.ToString()}");
                SBDebug.LogError($"RegisterRecvAsync Failed. ArgumentException. sessionId: {SessionID}, error: {e_.Message}");
            }
            catch (ObjectDisposedException e_)
            {
                //SBLog.PrintError($"RegisterRecvAsync Failed. ObjectDisposedException. sessionId: {SessionID}, error: {e_.ToString()}");
                SBDebug.LogError($"RegisterRecvAsync Failed. ObjectDisposedException. sessionId: {SessionID}, error: {e_.Message}");
            }
            catch (InvalidOperationException e_)
            {
                //SBLog.PrintError($"RegisterRecvAsync Failed. InvalidOperationException. sessionId: {SessionID}, error: {e_.ToString()}");
                SBDebug.LogError($"RegisterRecvAsync Failed. InvalidOperationException. sessionId: {SessionID}, error: {e_.Message}");
            }
            catch (NotSupportedException e_)
            {
                //SBLog.PrintError($"RegisterRecvAsync Failed. NotSupportedException. sessionId: {SessionID}, error: {e_.ToString()}");
                SBDebug.LogError($"RegisterRecvAsync Failed. NotSupportedException. sessionId: {SessionID}, error: {e_.Message}");
            }
            catch (SocketException e_)
            {
                //SBLog.PrintError($"RegisterRecvAsync Failed. SocketException. sessionId: {SessionID}, error: {e_.ToString()}");
                SBDebug.LogError($"RegisterRecvAsync Failed. SocketException. sessionId: {SessionID}, error: {e_.Message}");
            }
            catch (Exception e_)
            {
                //Console.WriteLine($"RegisterRecv Failed {e_}");
                //SBLog.PrintError($"RegisterRecvAsync Failed. Exception. sessionId: {SessionID}, error: {e_.ToString()}");
                SBDebug.LogError($"RegisterRecvAsync Failed. Exception. sessionId: {SessionID}, error: {e_.Message}");
            }
        }

        void OnRecvCompleted(object sender_, SocketAsyncEventArgs args_)
        {
            if (args_.BytesTransferred > 0 && SocketError.Success == args_.SocketError)
            {
                // TODO
                try
                {
                    // Write 위치 이동
                    if (false == _recvBuffer.OnWrite(args_.BytesTransferred))
                    {
                        Disconnect();
                        return;
                    }

                    // 콘텐츠 쪽으로 데이터를 넘겨주고 얼마나 처리했는지 받는다.
                    int processLen = OnRecv(_recvBuffer.ReadSegment);
                    if (processLen < 0 || processLen > _recvBuffer.DataSize)
                    {
                        Disconnect();
                        return;
                    }

                    // Read 위치 이동
                    if (false == _recvBuffer.OnRead(processLen))
                    {
                        Disconnect();
                        return;
                    }

                    //OnRecv(new ArraySegment<byte>(args_.Buffer, args_.Offset, args_.BytesTransferred));
                    //string recvData = Encoding.UTF8.GetString(args_.Buffer, args_.Offset, args_.BytesTransferred);
                    //Console.WriteLine($"[From Client] {recvData}");
                    RegisterRecvAsync();
                }
                catch (Exception e_)
                {
                    //SBLog.PrintError($"OnRecvCompleted Failed {e_.Message}");
                    //SBLog.PrintError($"OnRecvCompleted Failed. sessionId: {SessionID}, error: {e_.ToString()}");
                    SBDebug.Log($"OnRecvCompleted Failed. sessionId: {SessionID}, error: {e_.Message}");
                }
            }
            else
            {
                TryReconnect();
            }
        }
        #endregion
    }
    #endregion
#else
    #region BeginReceive/BeginSend(BRBS) 용
    #endregion
#endif
}
