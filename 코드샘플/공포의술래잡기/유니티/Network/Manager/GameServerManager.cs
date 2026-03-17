using System;
using System.Collections.Generic;
using SBCommonLib;
using SBSocketClientLib;
using SBSocketPacketLib;
using SBSocketSharedLib;

public partial class GameServerManager
{
    TcpServerSession _session = null;
    SBTcpClientService _tcpClientService = null;
    TcpConnector _serverConnector = null;

    SBTcpClientConfig _config = null;

    public string IP{get;set;}
    public short PORT{get;set;}

    Dictionary<int, Action> recvCallback = new Dictionary<int, Action>();

    Action callbackDisconnected = null;
#if UNITY_EDITOR
    int sendCount = 0;
    int recievCount = 0;
#endif
    public void SetCallbackDisconnected(Action action)
    {
        callbackDisconnected = action;
    }
    void AddRecvCallback(int id, Action action)
    {
        if (recvCallback.ContainsKey(id) == true)
            return;
        recvCallback.Add(id, action);
    }

    void SendMessage<TObject>(PacketId packetId_, TObject packetObject_, int recvId, Action action)
    {
        if (action != null) AddRecvCallback(recvId, action);
        _session.SendPacket(packetId_, packetObject_);
    }

    void SendMessage<TObject>(PacketId packetId_, TObject packetObject_)
    {
#if UNITY_EDITOR
        sendCount++;
#endif
        _session.SendPacket(packetId_, packetObject_);
    }

    void InitClient()
    {
        var networkConfig = new NetworkConfig();  
        _config = networkConfig.Load(IP, PORT);
        _tcpClientService = new SBTcpClientService(_config);

        _session = new TcpServerSession(_tcpClientService.Config.ReceiveBufferSize, _tcpClientService.Config.MaxPacketSize, _tcpClientService.Config.CryptKey, _tcpClientService.Config.CryptIV);
        _session.CallbackDisconnected += OnDisconnected;
#if UNITY_EDITOR || SB_TEST
        SBLog.StartLogProcess<SBSocketClientLogger>(SBLogLevel.Trace);
#else
        SBLog.StartLogProcess<SBSocketClientLogger>(SBLogLevel.Off);
#endif

        if (false == _tcpClientService.Run(() => { return _session; }))
        {
            Managers.Instance.FailGameServerConnect();
            SBLog.PrintError($"Client is not running.");
            return;
        }

#if UNITY_EDITOR
        sendCount = 0;
        recievCount = 0;
#endif
    }


    void Connect(Action actionConnected = null)
    {
        if (null == _serverConnector)
        {
            _serverConnector = new TcpConnector(@"devServerConnector");
            if (_tcpClientService.AddConnector(_serverConnector))
            {
                _serverConnector.SetActionConneced(actionConnected);
                _serverConnector.Connect(() => { return _session; });
            }
        }
    }

    public void Update()
    {
        List<ISBPacket> list = PacketQueue.Instance.PopAll();
        foreach (ISBPacket packet_ in list)
        {
            SBPacket packet = packet_ as SBPacket;
            //SBDebug.Log($"[Update] foreach. packet length: {packet.Length} packetId: {packet.PacketId}, sessionId: {Session.ConvertSessionIdByteToString(packet.SessionId)}");
            SBLog.PrintTrace($"foreach. packet length: {packet.Length} packetId: {packet.PacketId}, sessionId: {Session.ConvertSessionIdByteToString(packet.SessionId)}");
            PacketManager.Instance.HandlePacket(_session, packet);

            if (recvCallback.ContainsKey(packet.PacketId) == true)
            {
                recvCallback[packet.PacketId].Invoke();
                recvCallback.Remove(packet.PacketId);
            }
        }
    }

    public void OnConnect(Action actionConnected = null)
    {
        InitClient();
        Connect(actionConnected);
    }

     public void OnConnectGameServer(string ip, short port, Action actiopnConnected = null, Action actiopnDisConnected = null)
    {
        IP = ip;
        PORT = port;
        SetCallbackDisconnected(actiopnDisConnected);
        OnConnect(actiopnConnected);
        _session.IsPopup = false;
    }

    public void ClearDisconnectCallback()
    {
        if (callbackDisconnected != null)
            callbackDisconnected = null;
    }
    public void OnDisconnected()
    {
        if(callbackDisconnected!=null)
            callbackDisconnected.Invoke();
    }

    public void Disconnect()
    {
        if (IsAlive())
        {
            SBDebug.Log($"Disconnect");
            _session.Disconnect();            
        }

        _tcpClientService = null;
        _serverConnector = null;
        recvCallback.Clear();


#if UNITY_EDITOR
        SBDebug.Log($"sendCount : {sendCount}, recievCount : {recievCount}");
        sendCount = 0;
        recievCount = 0;
#endif
    }

    public bool IsAlive()
    {
        return _session != null && _session.IsConnected;
    }

    public void DisconnectDodge()
    {
        if (IsAlive())
            _session.Disconnect();
    }

#if UNITY_EDITOR
    public void RecievedPacket()
    {
        recievCount++;
    }
#endif
}
