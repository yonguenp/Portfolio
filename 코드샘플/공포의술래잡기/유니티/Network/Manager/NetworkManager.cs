using System;
using System.Collections.Generic;
using SBCommonLib;
using SBSocketClientLib;
using SBSocketPacketLib;
using SBSocketSharedLib;

public partial class NetworkManager
{
    public GameServerManager GameServer { get; private set; } = null;

    TcpServerSession _session = null;
    SBTcpClientService _tcpClientService = null;
    TcpConnector _serverConnector = null;

    SBTcpClientConfig _config = null;

    public string IP{get;set;}
    public short PORT{get;set;}

    Dictionary<int, Action> recvCallback = new Dictionary<int, Action>();

    Action callbackDisconnected = null;

    public NetworkManager()
    {
        GameServer = new GameServerManager();
    }

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
            SBLog.PrintError($"Client is not running.");
            return;
        }
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

    public void ClearDisconnectCallback()
    {
        if (callbackDisconnected != null)
            callbackDisconnected = null;
    }
    public void OnConnect(Action actionConnected = null)
    {
        InitClient();
        Connect(actionConnected);
    }

     public void OnConnect(string ip, short port, Action actiopnConnected = null, Action actiopnDisConnected = null)
    {
        IP = ip;
        PORT = port;
        SetCallbackDisconnected(actiopnDisConnected);
        OnConnect(actiopnConnected);
        _session.IsPopup = true;
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

    }

    public bool IsAlive()
    {
        return _session != null && _session.IsConnected;
    }

#if UNITY_EDITOR
    public void DisconnectTest()
    {
        if (IsAlive())
            _session.Disconnect();
    }
#endif

    public void DisconnectDodge()
    {
        GameServer.DisconnectDodge();

        if (IsAlive())
            _session.Disconnect();
    }
}
