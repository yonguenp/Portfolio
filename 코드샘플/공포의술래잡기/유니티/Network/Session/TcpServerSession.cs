using System;
using SBCommonLib;
using SBSocketClientLib;
using SBSocketPacketLib;
using SBSocketSharedLib;

public class TcpServerSession : SBTcpSession
{
    public SBAtomicInt InLobby = new SBAtomicInt();
    public SBAtomicInt InGameRoom = new SBAtomicInt();

    public Action CallbackConnected {get;set;} = null;
    public Action CallbackDisconnected {get;set;} = null;

    public TcpServerSession(int recvBufferSize_, int maxPacketSize_, byte[] cryptKey_, byte[] cryptIV_) : base(recvBufferSize_, maxPacketSize_, cryptKey_, cryptIV_)
    {
        SessionID = Guid.Empty.ToString();
    }

    public void SendPacket<TObject>(PacketId packetId_, TObject packetObject_)
    {
        if (IsConnected)
        {
            try
            {
                SBPacket packet = SBTcpPacket.MakePacket(CryptKey, CryptIV, packetId_, packetObject_); //SBTcpPacket.BuildZeroFormatterPacket(_cryptKey, _cryptIV, (ushort)packetId_, packetObject_);
                packet.SessionId = GetSessionIdBytes();

                // SAEA용 Send
                Send(packet.GetBytes());
                //UnityEngine.Debug.LogFormat("SendPacket -- {0}, {1} ", packetId_.ToString(), packet.Data);

                // BSBR용 Send
                //Send(packet);
                SBLog.PrintInfo($"[TcpServerSession] sessionId: {SessionID}, packetId: {packetId_}, packet data: {packet.Data}, data length: {packet.DataLength}");
            }
            catch (Exception)
            {
                // TODO: 로그
                return;
            }
        }
    }

    public override void OnRecvPacket(ArraySegment<byte> buffer_)
    {
//        SBDebug.Log($"[TcpServerSession] OnRecvPacket - array: {buffer_.Array.Length}, offset: {buffer_.Offset}, count: {buffer_.Count}");
        //SBLog.PrintTrace($"[TcpServerSession] array: {buffer_.Array.Length}, offset: {buffer_.Offset}, count: {buffer_.Count}");
        PacketManager.Instance.OnRecvPacket(this, buffer_, (session_, packet_) => PacketQueue.Instance.Push(packet_));
    }

    public override void OnSend(int numOfPacket_, int numOfBytes_)
    {
        //SBDebug.Log($"Transferred Packet sessionId: {SessionID}, count: {numOfPacket_}, bytes: {numOfBytes_}");
//        SBLog.PrintTrace($"Transferred Packet sessionId: {SessionID}, count: {numOfPacket_}, bytes: {numOfBytes_}");
    }

    protected override void OnConnected()
    {
        SBLog.PrintTrace($"[Session] #{SessionNo} Client-OnConnected: {Channel.RemoteEndPoint}");
        if(CallbackConnected!=null)CallbackConnected.Invoke();
    }

    /// <summary>
    /// Called when [disconnected].
    /// </summary>
    protected override void OnDisconnected()
    {
        //SBLog.PrintTrace($"[Session] #{SessionNo} Client-OnDisconnected: {Channel.RemoteEndPoint}");
         if(CallbackDisconnected !=null) CallbackDisconnected.Invoke();
    }
}
