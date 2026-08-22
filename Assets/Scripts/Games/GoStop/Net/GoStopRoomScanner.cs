using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

/// <summary>
/// 게스트 쪽 — <see cref="GoStopRoomAdvertiser"/>가 뿌리는 브로드캐스트를
/// 듣고 "지금 근처에 열려 있는 방 목록"을 유지한다. UI는 IP 입력란 대신
/// 이 목록을 보여주고 탭 한 번으로 <see cref="TcpGoStopClientTransport.Connect"/>
/// 를 호출하면 된다.
///
/// 방 목록은 <b>일정 시간 소식이 없으면 자동으로 사라진다</b>
/// (<see cref="RoomTimeoutSec"/>) — 호스트가 비정상 종료(앱 강제 종료,
/// 와이파이 이탈 등)돼서 "방 닫힘"을 보낼 기회조차 없었던 경우까지
/// 커버하려면 유실을 전제로 한 타임아웃이 브로드캐스트 자체를 못 믿는
/// UDP 특성상 유일하게 신뢰할 수 있는 방법이다.
/// </summary>
public class GoStopRoomScanner : MonoBehaviour
{
    const float RoomTimeoutSec = 3.0f; // 광고 주기(1초)의 3배 — 패킷 한두 개 유실돼도 안 사라지게 여유를 둔다

    public class DiscoveredRoom
    {
        public string hostName;
        public string ip;
        public int tcpPort;
        public int playerCount;
        public int maxPlayers;
        public float lastSeenTime;
    }

    UdpClient udp;
    Thread listenThread;
    volatile bool listening;

    readonly ConcurrentQueue<GoStopRoomAnnounce> incoming = new ConcurrentQueue<GoStopRoomAnnounce>();
    readonly ConcurrentQueue<string> incomingIp = new ConcurrentQueue<string>(); // incoming과 1:1 대응(같은 순서로 쌓인다)

    readonly Dictionary<string, DiscoveredRoom> rooms = new Dictionary<string, DiscoveredRoom>(); // key = ip

    /// <summary>지금 살아있는(타임아웃 안 지난) 방 목록 — 매 프레임 새로
    /// 만들지 않고 <see cref="Update"/>에서 정리된 상태를 그대로 노출한다.</summary>
    public IReadOnlyCollection<DiscoveredRoom> Rooms => rooms.Values;

    public void StartScanning()
    {
        StopScanning();
        udp = new UdpClient();
        udp.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        udp.Client.Bind(new IPEndPoint(IPAddress.Any, GoStopRoomAdvertiser.DiscoveryPort));
        listening = true;
        listenThread = new Thread(ListenLoop) { IsBackground = true };
        listenThread.Start();
    }

    public void StopScanning()
    {
        listening = false;
        udp?.Close();
        udp = null;
        rooms.Clear();
    }

    void ListenLoop()
    {
        var anyEndpoint = new IPEndPoint(IPAddress.Any, 0);
        while (listening)
        {
            byte[] data;
            IPEndPoint sender = anyEndpoint;
            try { data = udp.Receive(ref sender); }
            catch { return; } // 소켓이 닫히면(StopScanning) 여기로 빠져서 스레드가 자연 종료된다

            string json;
            try { json = System.Text.Encoding.UTF8.GetString(data); }
            catch { continue; }

            GoStopRoomAnnounce msg;
            try { msg = JsonUtility.FromJson<GoStopRoomAnnounce>(json); }
            catch { continue; }

            if (msg == null || msg.magic != GoStopRoomAnnounce.Magic) continue; // 우리 프로토콜이 아닌 UDP 트래픽은 조용히 버린다

            incoming.Enqueue(msg);
            incomingIp.Enqueue(sender.Address.ToString());
        }
    }

    void Update()
    {
        while (incoming.TryDequeue(out var msg) && incomingIp.TryDequeue(out var ip))
        {
            if (!rooms.TryGetValue(ip, out var room))
            {
                room = new DiscoveredRoom { ip = ip };
                rooms[ip] = room;
            }
            room.hostName = msg.hostName;
            room.tcpPort = msg.tcpPort;
            room.playerCount = msg.playerCount;
            room.maxPlayers = msg.maxPlayers;
            room.lastSeenTime = Time.unscaledTime;
        }

        if (rooms.Count == 0) return;
        var stale = new List<string>();
        foreach (var kv in rooms)
            if (Time.unscaledTime - kv.Value.lastSeenTime > RoomTimeoutSec) stale.Add(kv.Key);
        foreach (var key in stale) rooms.Remove(key);
    }

    void OnDestroy() => StopScanning();
}
