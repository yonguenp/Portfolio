using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using SBCommonLib;
using SBSocketPacketLib;

/// <summary>
/// SandBox Socket Client Library
/// </summary>
namespace SBSocketClientLib
{
    /// <summary>
    /// TCP Channel ID 클래스
    /// </summary>
    //public class SBTcpChannelId
    //{
    //    /// <summary>
    //    /// ID 증가값
    //    /// </summary>
    //    private long _topId = 0;

    //    /// <summary>
    //    /// 유효하지 않은 ID
    //    /// </summary>
    //    public const int kInvalidId = 0;

    //    /// <summary>
    //    /// TCP Channel ID 생성 함수
    //    /// </summary>
    //    public long GenerateId()
    //    {
    //        long value = Interlocked.Increment(ref _topId);
    //        return value;
    //    }
    //}

    /// <summary>
    /// TCP Channel 클래스
    /// </summary>
    public class SBTcpChannel
    {
        /// <summary>
        /// TCP Channel ID 객체
        /// </summary>
        //private static SBTcpChannelId _channelId = new SBTcpChannelId();

        /// <summary>
        /// 패킷 사이즈 최대값
        /// </summary>
        private int _maxPacketSize;

        /// <summary>
        /// 초기화 콜백 액션
        /// </summary>
        private Action _connectedCallback;

        /// <summary>
        /// 연결해제 콜백 액션
        /// </summary>
        private Action _disconnectedCallback;

        /// <summary>
        /// TCP 클라이언트 세션 객체
        /// </summary>
        private SBTcpSession _session;

        /// <summary>
        /// 소켓 객체
        /// </summary>
        private Socket _socket;

        /// <summary>
        /// TCP Connection ID
        /// </summary>
        //public long Id { get; private set; }

        /// <summary>
        /// Remote end point
        /// </summary>
        public EndPoint RemoteEndPoint
        {
            get => (_socket != null) ? _socket.RemoteEndPoint : null;
            //{
            //    try
            //    {
            //        return _socket.RemoteEndPoint;
            //    }
            //    catch (Exception e_)
            //    {
            //        SBDebug.LogError($"[SBTcpChannel] RemoteEndPoint - Error(Message: {e_.Message})");
            //        //SBLog.PrintError($"[SBTcpChannel] RemoteEndPoint - Error(Message: {e_.Message})");
            //        return null;
            //    }
            //}
        }

        /// <summary>
        /// 소켓 연결 여부
        /// </summary>
        public bool IsAlive
        {
            get => ((null == _socket || false == _socket.Connected) ? false : true);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="socket_"></param>
        /// <param name="session_"></param>
        /// <param name="maxPacketSize_"></param>
        /// <param name="connectedCallback_"></param>
        /// <param name="disconnectedCallback_"></param>
        public SBTcpChannel(Socket socket_, Session session_, int maxPacketSize_, Action connectedCallback_, Action disconnectedCallback_)
        {
            _socket = socket_;
            _session = session_ as SBTcpSession;
            //Id = _channelId.GenerateId();
            _connectedCallback = connectedCallback_;
            _disconnectedCallback = disconnectedCallback_;
            _maxPacketSize = maxPacketSize_;
        }

        /// <summary>
        /// End pint IP 주소 가져오기
        /// </summary>
        /// <returns></returns>
        public string GetEndPointIp()
        {
            var ip = string.Empty;

            if (null == _socket)
                return "Socket is null.";

            try
            {
                ip = ((IPEndPoint)_socket.RemoteEndPoint).Address.ToString();
            }
            catch (Exception e_)
            {
                //SBDebug.LogError($"[SBTcpChannel] GetEndPointIP - Error(Message: {e_.Message})");
                SBLog.PrintError($"[SBTcpChannel] Error(Message: {e_.Message})");
            }

            return ip;
        }

        public string GetRemoteEndPoint()
        {
            return (RemoteEndPoint != null) ? RemoteEndPoint.ToString() : "Socket is null.";
        }

        /// <summary>
        /// 연결 해제
        /// </summary>
        /// <param name="reconnect_">재연결 여부</param>
        public void Disconnect()
        {
            try
            {
                if (_socket != null)
                {
                    _socket.Shutdown(SocketShutdown.Both);
                    _socket.Close();
                    _socket = null;
                }
            }
            catch (NullReferenceException e_)
            {
                SBDebug.LogError($"[SBTcpChannel] Disconnect - NullReferenceException Error(Message: {e_.Message})");
                SBLog.PrintError($"[SBTcpChannel] NullReferenceException Error(Message: {e_.Message})");
            }
            catch (ObjectDisposedException e_)
            {
                SBDebug.LogError($"[SBTcpChannel] Disconnect - ObjectDisposedException Error(Message: {e_.Message})");
                SBLog.PrintError($"[SBTcpChannel] ObjectDisposedException Error(Message: {e_.Message})");
            }
            catch (Exception e_)
            {
                SBDebug.LogError($"[SBTcpChannel] Disconnect - Error(Message: {e_.Message})");
                SBLog.PrintError($"[SBTcpChannel] Error(Message: {e_.Message})");
            }

            if (_disconnectedCallback != null)
            {
                _disconnectedCallback.Invoke();
            }
        }

        public void Disconnect(bool reconnect_ = false)
        {
            try
            {
                if (_socket != null)
                {
                    _socket.Shutdown(SocketShutdown.Both);
                    _socket.Close();
                    _socket = null;
                }
            }
            catch (NullReferenceException e_)
            {
                SBLog.PrintError($"[SBTcpChannel] #{_session.SessionNo} Disconnect - NullReferenceException Error(Message: {e_.Message})");
            }
            catch (ObjectDisposedException e_)
            {
                SBLog.PrintError($"[SBTcpChannel] #{_session.SessionNo} Disconnect - ObjectDisposedException Error(Message: {e_.Message})");
            }
            catch (Exception e_)
            {
                SBLog.PrintError($"[SBTcpChannel] #{_session.SessionNo} Disconnect - Error(Message: {e_.Message})");
            }

            if (false == reconnect_)
            {
                if (_disconnectedCallback != null)
                {
                    _disconnectedCallback.Invoke();
                }
            }
        }

#if true
        #region SocketAsyncEventArgs(SAEA) 용
        public bool SendAsync(SocketAsyncEventArgs args_)
        {
            return _socket.SendAsync(args_);
        }

        public bool ReceiveAsync(SocketAsyncEventArgs args_)
        {
            return _socket.ReceiveAsync(args_);
        }
        #endregion
#else
        #region BeginSend/BeginReceive(BSBR) 용
        /// <summary>
        /// 받을 패킷 사이즈(헤더를 제외한 데이터 영역의 크기(PacketId + SessionId + Data))
        /// </summary>
        private int _recvPacketDataSize;

        /// <summary>
        /// 받은 사이즈
        /// </summary>
        private int _receivedSize;

        /// <summary>
        /// Begins the receive
        /// </summary>
        /// <param name="isRecvHeader_">Header를 받아야 하는지 여부(header를 받아야 하면 true, 이미 header를 받았으면 false)</param>
        /// <param name="recvBuffer_">Receive 버퍼</param>
        public void BeginReceive(bool isRecvHeader_, byte[] recvBuffer_)
        {
            byte[] buffer = recvBuffer_;

            if (null == _socket)
            {
                if (null != _disconnectedCallback)
                {
                    _disconnectedCallback.Invoke();
                }

                return;
            }

            try
            {
                if (isRecvHeader_)
                {
                    // Header 부분만 먼저 받는다.
                    if (null == buffer)
                    {
                        _receivedSize = 0;
                        buffer = new byte[SBPacket.kHeaderSize];
                    }
                    SBDebug.Log($"Waiting Recv Header.");
                    SBLog.PrintInfo($"Waiting Recv Header.");
                    _socket.BeginReceive(buffer, _receivedSize, SBPacket.kHeaderSize - _receivedSize, SocketFlags.None, Receive, buffer);
                }
                else
                {
                    // 잔여 데이터 수신
                    SBDebug.Log($"Waiting Recv Body.");
                    SBLog.PrintInfo($"Waiting Recv Body.");
                    _socket.BeginReceive(buffer, _receivedSize, _recvPacketDataSize - _receivedSize, SocketFlags.None, Receive, buffer);
                }
            }
            catch (NullReferenceException e_)
            {
                SBDebug.LogError($"[SBTcpChannel] BeginReceive - NullReferenceException Error(Message: {e_.Message})");
                SBLog.PrintError($"[SBTcpChannel] NullReferenceException Error(Message: {e_.Message})");
            }
            catch (ArgumentOutOfRangeException e_)
            {
                SBDebug.LogError($"[SBTcpChannel] BeginReceive - ArgumentOutOfRangeException Error(Message: {e_.Message})");
                SBLog.PrintError($"[SBTcpChannel] ArgumentOutOfRangeException Error(Message: {e_.Message})");
            }
            catch (SocketException e_)
            {
                SBDebug.LogError($"[SBTcpChannel] BeginReceive - SocketException Error(Message: {e_.Message})");
                SBLog.PrintError($"[SBTcpChannel] SocketException Error(Message: {e_.Message})");
            }
            catch (ObjectDisposedException e_)
            {
                SBDebug.LogError($"[SBTcpChannel] BeginReceive - ObjectDisposedException Error(Message: {e_.Message})");
                SBLog.PrintError($"[SBTcpChannel] ObjectDisposedException Error(Message: {e_.Message})");
            }
            catch (Exception e_)
            {
                SBDebug.LogError($"[SBTcpChannel] BeginReceive - Error(Message: {e_.Message})");
                SBLog.PrintError($"[SBTcpChannel] Error(Message: {e_.Message})");
            }
        }

        /// <summary>
        /// Receive 함수
        /// </summary>
        /// <param name="ar_">IAsyncResult</param>
        public void Receive(IAsyncResult ar_)
        {
            byte[] buffer = (byte[])ar_.AsyncState;

            if (0 == _recvPacketDataSize)
            {
                // 헤더 처리 과정
                try
                {
                    int len = _socket.EndReceive(ar_);

                    // EndReceive값이 0이면 Disconnect
                    if (0 == len)
                    {
                        Disconnect();
                        return;
                    }

                    _receivedSize += len;
                }
                catch (NullReferenceException e_)
                {
                    SBDebug.LogError($"[SBTcpChannel] Receive - NullReferenceException Error(Message: {e_.Message})");
                    SBLog.PrintError($"[SBTcpChannel] NullReferenceException Error(Message: {e_.Message})");
                    Disconnect();
                    return;
                }
                catch (SocketException e_)
                {
                    SBDebug.LogError($"[SBTcpChannel] Receive - SocketException Error(Message: {e_.Message})");
                    SBLog.PrintError($"[SBTcpChannel] SocketException Error(Message: {e_.Message})");
                    Disconnect();
                    return;
                }
                catch (ObjectDisposedException e_)
                {
                    SBDebug.LogError($"[SBTcpChannel] Receive - ObjectDisposedException Error(Message: {e_.Message})");
                    SBLog.PrintError($"[SBTcpChannel] ObjectDisposedException Error(Message: {e_.Message})");
                    Disconnect();
                    return;
                }
                catch (Exception e_)
                {
                    SBDebug.LogError($"[SBTcpChannel] Receive - Error(Message: {e_.Message})");
                    SBLog.PrintError($"[SBTcpChannel] Error(Message: {e_.Message})");
                    Disconnect();
                    return;
                }

                if (0 == _receivedSize)
                {
                    SBDebug.LogError($"[SBTcpChannel] Receive - Receive size is zero.");
                    SBLog.PrintError($"[SBTcpChannel] Receive size is zero.");
                    Disconnect();
                    return;
                }
                else if (_receivedSize < SBPacket.kHeaderSize)
                {
                    // 받은 사이즈가 헤더 크기보다 작으면
                    // 계속 해서 헤더를 받음.
                    BeginReceive(true, buffer);
                    return;
                }
                else
                {
                    // 받은 사이즈가 헤더 크기보다 크면
                    // 패킷 데이터 크기를 확인하고
                    // 버퍼를 생성한 후 데이터 수신을 진행.
                    _receivedSize = 0;
                    _recvPacketDataSize = SBPacket.DecodeUInt16FromBytes(buffer, 0);

                    if (_recvPacketDataSize <= 0 || _recvPacketDataSize < SBPacket.kHeaderSize || _recvPacketDataSize > _maxPacketSize)
                    {
                        SBDebug.LogError($"[SBTcpChannel] Receive - Receive packet size Error(Received Packet Size: {_recvPacketDataSize})");
                        SBLog.PrintError($"[SBTcpChannel] Receive packet size Error(Received Packet Size: {_recvPacketDataSize})");
                        Disconnect();
                        return;
                    }

                    buffer = new byte[_recvPacketDataSize];
                    //buffer = new byte[(_recvPacketSize - SBPacket.kPacketLengthSize)];
                    BeginReceive(false, buffer);
                    return;
                }
            }
            else
            {
                // 데이터 처리 과정
                try
                {
                    int len = _socket.EndReceive(ar_);

                    if (0 == len)
                    {
                        SBDebug.LogError($"[SBTcpChannel] Receive - 소켓 EndReceive Error(Length: {len})");
                        SBLog.PrintError($"[SBTcpChannel] 소켓 EndReceive Error(Length: {len})");
                        Disconnect();
                        return;
                    }

                    _receivedSize += len;
                }
                catch (NullReferenceException e_)
                {
                    SBDebug.LogError($"[SBTcpChannel] Receive - NullReferenceException Error(Message: {e_.Message})");
                    SBLog.PrintError($"[SBTcpChannel] NullReferenceException Error(Message: {e_.Message})");
                    Disconnect();
                    return;
                }
                catch (SocketException e_)
                {
                    SBDebug.LogError($"[SBTcpChannel] Receive - SocketException Error(Message: {e_.Message})");
                    SBLog.PrintError($"[SBTcpChannel] SocketException Error(Message: {e_.Message})");
                    Disconnect();
                    return;
                }
                catch (ObjectDisposedException e_)
                {
                    SBDebug.LogError($"[SBTcpChannel] Receive - ObjectDisposedException Error(Message: {e_.Message})");
                    SBLog.PrintError($"[SBTcpChannel] ObjectDisposedException Error(Message: {e_.Message})");
                    Disconnect();
                    return;
                }
                catch (Exception e_)
                {
                    SBDebug.LogError($"[SBTcpChannel] Receive - Error(Message: {e_.Message})");
                    SBLog.PrintError($"[SBTcpChannel] Error(Message: {e_.Message})");
                    Disconnect();
                    return;
                }

                if (_receivedSize < _recvPacketDataSize)
                {
                    // 받은 사이즈가 받아야할 사이즈보다 작으면 계속해서 받는다.
                    BeginReceive(false, buffer);
                    return;
                }
                else
                {
                    _receivedSize = _recvPacketDataSize = 0;
                    SBPacket packet = new SBPacket(buffer);
                    SBDebug.Log($"[SBTcpChannel] Receive(). packet info - length: {packet.Length}, id: {packet.PacketId}, sessionId: {Session.ConvertSessionIdByteToString(packet.SessionId)}");
                    SBLog.PrintTrace($"[SBTcpChannel] packet info - length: {packet.Length}, id: {packet.PacketId}, sessionId: {Session.ConvertSessionIdByteToString(packet.SessionId)}");
                    ThreadPool.QueueUserWorkItem(new WaitCallback(_session.PacketHandler), packet);
                }
            }

            BeginReceive(true, null);
        }

        /// <summary>
        /// 패킷 Send 함수
        /// </summary>
        /// <param name="packet_">패킷</param>
        /// <returns></returns>
        public bool Send(SBPacket packet_)
        //public bool Send(ArraySegment<byte> sendBuff_)
        {
            if (null == _socket)
            {
                return false;
            }

            var segment = packet_.GetBytes();
            byte[] buffer = new byte[segment.Count];
            Buffer.BlockCopy(segment.Array, segment.Offset, buffer, 0, segment.Count);

            //byte[] buffer = new byte[sendBuff_.Count];
            //Buffer.BlockCopy(sendBuff_.Array, sendBuff_.Offset, buffer, 0, sendBuff_.Count);

            try
            {
                _socket.BeginSend(buffer, 0, buffer.Length, SocketFlags.None, null, null);
            }
            catch (ArgumentNullException e_)
            {
                SBLog.PrintError($"[SBTcpChannel] ArgumentNullException Error(Message: {e_.Message})");
                Disconnect();
                return false;
            }
            catch (ArgumentException e_)
            {
                SBLog.PrintError($"[SBTcpChannel] ArgumentException Error(Message: {e_.Message})");
                Disconnect();
                return false;
            }
            catch (SocketException e_)
            {
                SBLog.PrintError($"[SBTcpChannel] SocketException Error(Message: {e_.Message})");
                Disconnect();
                return false;
            }
            catch (ObjectDisposedException e_)
            {
                SBLog.PrintError($"[SBTcpChannel] ObjectDisposedException Error(Message: {e_.Message})");
                return false;
            }
            catch (Exception e_)
            {
                SBLog.PrintError($"[SBTcpChannel] Error(Message: {e_.Message})");
                Disconnect();
                return false;
            }

            return true;
        }
        #endregion
#endif
    }
}
