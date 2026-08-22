using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 고스톱 네트워크 대전의 대기실 진행자 — 호스트/게스트 양쪽 역할을
/// 한 클래스로 다룬다(세션당 둘 중 하나만 쓰이므로 굳이 나눠서 UI가
/// 두 클래스를 왔다갔다 참조하게 만들 필요가 없다). 씬 전환(대기실 →
/// 실제 게임)을 넘어가야 하므로 <see cref="Instance"/> 싱글톤 +
/// <c>DontDestroyOnLoad</c>로 산다.
///
/// <b>인원수 → 게임 모드 매핑(사용자 확인)</b>:
/// 총 인원(호스트 포함) 2명 = 맞고(<c>GoStopScene</c>), 3명 = 진짜 3인
/// 고스톱, 4명 = 4인 고스톱(광팔이 로테이션). 3인은 광팔이 로테이션이
/// 아예 없는 <b>진짜 3인 모드</b>로 새로 만든다(SEATS=4 고정 후 한 자리를
/// AI로 채우는 방식은 채택 안 함 — 실제 플레이어끼리만 하는 걸 원함).
///
/// <b>좌석 번호 = 접속 순서.</b> 호스트가 항상 0, 게스트는 1·2·3을
/// 접속한 순서대로 받는다 — <see cref="TcpGoStopHostTransport"/>가 이미
/// 이 규약으로 배정하므로 여기서는 그대로 따르기만 하면 된다.
///
/// <b>지금 이 클래스가 하는 일과 안 하는 일:</b> 대기실 상태 관리(입장/
/// 퇴장/시작 신호)까지가 이 파일의 책임이다. 실제 게임 씬에서 "카드를
/// 냈다" 같은 턴 메시지를 받아 <c>GoStopGame</c>/<c>GoStop3PGame</c>의
/// AI 자리를 대체하는 작업은 아직 여기 없다 — 그건 각 게임 파일에
/// 직접 손대야 하는 다음 단계 작업이라 의도적으로 분리해뒀다.
/// </summary>
public class GoStopNetLobby : MonoBehaviour
{
    public static GoStopNetLobby Instance { get; private set; }

    public const int DefaultPort = 47755;

    public bool IsHost { get; private set; }
    public bool IsGuest { get; private set; }

    /// <summary>내 좌석 번호 — 호스트는 항상 0. 게스트는 접속 직후엔
    /// -1(미배정)이었다가 StartGame 메시지를 받는 순간 확정된다.</summary>
    public int MySeat { get; private set; } = -1;

    /// <summary>대기실 인원 스냅샷 — 인덱스 = 좌석 번호(0=호스트). 접속
    /// 안 된 좌석은 빈 문자열.</summary>
    public string[] PlayerNames { get; private set; } = new string[4];

    /// <summary>총 참가 인원(호스트 포함) — <see cref="OnGameStarting"/>가
    /// 발사되는 순간 확정되고, 씬이 넘어간 뒤(이 오브젝트는 DontDestroyOnLoad라
    /// 살아남는다) 게임 스크립트(GoStopGame/GoStop3PGame)의 Start()가
    /// 이 값을 읽어 좌석 수·게임 모드를 정한다. 2=맞고, 3/4=고스톱.</summary>
    public int PlayerCount { get; private set; }

    string myName;

    TcpGoStopHostTransport hostTransport;
    GoStopRoomAdvertiser advertiser;

    TcpGoStopClientTransport clientTransport;
    GoStopRoomScanner scanner;

    /// <summary>대기실 인원이 바뀔 때마다(입장/퇴장/이름 갱신) 호출된다.
    /// UI가 이걸 구독해서 "N/4명" 같은 걸 다시 그리면 된다.</summary>
    public event Action OnLobbyChanged;

    /// <summary>게임이 시작됐다 — 인자는 (내 좌석, 총 인원). 호스트·게스트
    /// 양쪽 다 이 이벤트를 받는다. 이후 UI는 <c>SceneManager.LoadScene</c>
    /// 으로 총 인원에 맞는 씬(2명=GoStopScene, 3~4명=GoStop3PScene)을
    /// 열면 된다.</summary>
    public event Action<int, int> OnGameStarting;

    /// <summary>연결이 끊기거나 방이 닫혔을 때 — 인자는 사유 문자열.
    /// 게스트 전용(호스트와의 TCP 연결 자체가 끊어졌을 때).</summary>
    public event Action<string> OnDisconnected;

    /// <summary>호스트 전용 — 접속해 있던 게스트 한 명이 도중에 나갔다
    /// (앱 종료·네트워크 끊김 등). 인자는 그 좌석 번호. 대기실 화면에서는
    /// <see cref="OnLobbyChanged"/>만으로 충분하지만, 게임이 이미 시작된
    /// 뒤에는 "그 좌석이 진행 중이던 판을 계속할 수 없다"는 걸 게임 씬이
    /// 알아야 한다(예: 그 좌석의 메시지를 영원히 기다리며 멈춰있는 것을
    /// 막아야 함) — 그래서 로비 갱신과 별개의 전용 이벤트로 뺐다.</summary>
    public event Action<int> OnGuestLeftDuringGame;

    /// <summary>대기실 관리용이 아닌 나머지 전부(PlayCard·FieldChoice·
    /// StateSync 등 실제 턴 메시지) — 게임 씬(GoStop3PGame 등)이 이걸
    /// 구독해서 AI 자리를 원격 좌석으로 대체한다. 인자는 (보낸 좌석,
    /// 메시지) — <b>호스트 쪽에서는 실제 보낸 게스트 좌석</b>, <b>게스트
    /// 쪽에서는 항상 0(호스트)</b>이다(게스트는 오직 호스트하고만 통신
    /// 하므로 발신자를 구분할 필요가 없다).</summary>
    public event Action<int, GoStopNetMessage> OnGameMessage;

    /// <summary>게스트 전용 — 호스트에게 턴 메시지를 보낸다.</summary>
    public void SendToHost(GoStopNetMessage msg) => clientTransport?.Send(msg);

    /// <summary>호스트 전용 — 특정 좌석 한 명에게만 보낸다(StateSync처럼
    /// 좌석마다 내용이 달라야 할 때. 지금은 대부분 Broadcast를 쓴다).</summary>
    public void SendToSeat(int seat, GoStopNetMessage msg) => hostTransport?.Send(seat, msg);

    /// <summary>호스트 전용 — 접속한 게스트 전원에게 같은 메시지를 보낸다
    /// (StateSync·Event 등 대부분의 턴 메시지가 이 경로를 쓴다).</summary>
    public void BroadcastToGuests(GoStopNetMessage msg) => hostTransport?.Broadcast(msg);

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ── 호스트 ──────────────────────────────────────────

    public void HostRoom(string displayName, int port = DefaultPort)
    {
        StopAll();
        myName = displayName;
        IsHost = true;
        MySeat = 0;
        PlayerNames = new string[4];
        PlayerNames[0] = displayName;

        hostTransport = gameObject.AddComponent<TcpGoStopHostTransport>();
        hostTransport.OnGuestJoined += HostOnGuestJoined;
        hostTransport.OnGuestLeft += HostOnGuestLeft;
        hostTransport.OnMessage += HostOnMessage;
        hostTransport.StartHosting(port, maxGuests: 3);

        advertiser = gameObject.AddComponent<GoStopRoomAdvertiser>();
        advertiser.CurrentPlayerCount = 1;
        advertiser.StartAdvertising(displayName, port);

        OnLobbyChanged?.Invoke();
    }

    void HostOnGuestJoined(int seat)
    {
        // 이름은 아직 모른다 — 게스트가 접속 직후 보내는 Hello를 받아야
        // PlayerNames가 채워진다. 그때까지는 "누군가 들어왔다"만 알린다.
        if (PlayerNames[seat] == null || PlayerNames[seat] == "") PlayerNames[seat] = "(입장 중...)";
        RefreshAdvertiserCount();
        BroadcastLobbyUpdate();
        OnLobbyChanged?.Invoke();
    }

    void HostOnGuestLeft(int seat, string reason)
    {
        PlayerNames[seat] = null;
        RefreshAdvertiserCount();
        BroadcastLobbyUpdate();
        OnLobbyChanged?.Invoke();
        // 대기실 단계(아직 PlayerCount==0, 게임 씬이 없음)에서는 아무도
        // 이 이벤트를 안 듣고 있어 no-op — 게임이 시작된 뒤에만
        // GoStop3PGame/GoStopGame이 구독해서 실제로 반응한다.
        OnGuestLeftDuringGame?.Invoke(seat);
    }

    void HostOnMessage(int seat, GoStopNetMessage msg)
    {
        if (msg.type == GoStopNetMessage.Type.Hello)
        {
            PlayerNames[seat] = string.IsNullOrEmpty(msg.text) ? $"게스트{seat}" : msg.text;
            BroadcastLobbyUpdate();
            OnLobbyChanged?.Invoke();
            return;
        }
        if (msg.type == GoStopNetMessage.Type.Bye)
        {
            // 게스트가 스스로 나감 — TCP 연결도 곧 끊겨 HostOnGuestLeft가
            // 뒤이어 불리지만, 사유를 더 정확히 남기고 싶으면 여기서 처리.
            return;
        }
        // 그 외(PlayCard 등 실제 턴 메시지)는 게임 씬 쪽(GoStop3PGame)이
        // 구독해서 처리한다 — 여기서는 발신 좌석 정보를 붙여 그대로 넘겨주기만 한다.
        OnGameMessage?.Invoke(seat, msg);
    }

    void RefreshAdvertiserCount()
    {
        if (advertiser != null) advertiser.CurrentPlayerCount = PlayerNames.Count(n => !string.IsNullOrEmpty(n));
    }

    void BroadcastLobbyUpdate()
    {
        hostTransport?.Broadcast(GoStopNetMessage.LobbyUpdateMsg((string[])PlayerNames.Clone()));
    }

    /// <summary>총 인원(호스트+접속한 게스트)이 2명 이상이어야 부를 수
    /// 있다. 2명=맞고, 3~4명=고스톱으로 각 게스트에게 좌석·인원수를
    /// 실어 StartGame을 보내고, 광고(UDP 브로드캐스트)는 멈춘다 — 판이
    /// 시작되면 더 이상 새 참가자를 받지 않는다(중간 참가 미지원, v1
    /// 스코프).</summary>
    public bool HostStartGame()
    {
        int total = PlayerNames.Count(n => !string.IsNullOrEmpty(n) && n != "(입장 중...)");
        if (total < 2) return false;

        advertiser?.StopAdvertising();

        // 2026-08-20: 예전엔 PlayerNames(닉네임 배열) 기준으로 "이름이 있는
        // 좌석"에 그냥 Send를 불렀다 — 만약 그 사이 실제 TCP 연결은
        // 끊겼는데(순간적인 와이파이 문제 등) PlayerNames가 아직 정리되기
        // 전이면, Send는 "연결 없음"으로 조용히 아무것도 안 하고 반환하고
        // (예전엔 로그도 없었다), 그래도 아래 OnGameStarting은 그대로
        // 발사돼 호스트 자신은 씬을 넘어가 버린다 — "호스트는 게임
        // 화면으로 넘어갔는데 게스트는 대기 화면에 그대로 있다"는 신고와
        // 정확히 들어맞는 실패 모드였다. 지금은 실제로 연결돼 있는
        // 좌석(hostTransport.ConnectedSeats)만 대상으로 삼는다 — PlayerNames와
        // 어긋나는 좌석이 있으면 그 자체가 버그 신호이므로 로그를 남긴다.
        var connectedSeats = new HashSet<int>(hostTransport.ConnectedSeats);
        for (int seat = 1; seat < 4; seat++)
        {
            bool hasName = !string.IsNullOrEmpty(PlayerNames[seat]);
            bool isConnected = connectedSeats.Contains(seat);
            if (hasName != isConnected)
                Debug.LogWarning($"[GoStopNet] HostStartGame: seat {seat} 상태 불일치 — PlayerNames={hasName}, 실제연결={isConnected}");
            if (!isConnected) continue;
            hostTransport.Send(seat, GoStopNetMessage.StartGameMsg(seat, total));
        }
        PlayerCount = total;
        OnGameStarting?.Invoke(0, total);
        return true;
    }

    // ── 게스트 ──────────────────────────────────────────

    public void StartScanningForRooms()
    {
        StopAll();
        IsGuest = true;
        scanner = gameObject.AddComponent<GoStopRoomScanner>();
        scanner.StartScanning();
    }

    public IReadOnlyCollection<GoStopRoomScanner.DiscoveredRoom> DiscoveredRooms =>
        scanner != null ? scanner.Rooms : Array.Empty<GoStopRoomScanner.DiscoveredRoom>();

    public void JoinRoom(GoStopRoomScanner.DiscoveredRoom room, string displayName)
    {
        myName = displayName;
        scanner?.StopScanning(); // 접속을 시도하는 동안엔 더 이상 다른 방을 찾을 필요 없다

        clientTransport = gameObject.AddComponent<TcpGoStopClientTransport>();
        clientTransport.OnConnected += GuestOnConnected;
        clientTransport.OnMessage += GuestOnMessage;
        clientTransport.OnDisconnected += reason => OnDisconnected?.Invoke(reason);
        clientTransport.Connect(room.ip, room.tcpPort);
    }

    void GuestOnConnected()
    {
        clientTransport.Send(GoStopNetMessage.Hello(myName));
    }

    void GuestOnMessage(GoStopNetMessage msg)
    {
        switch (msg.type)
        {
            case GoStopNetMessage.Type.LobbyUpdate:
                PlayerNames = msg.playerNames ?? new string[4];
                OnLobbyChanged?.Invoke();
                break;
            case GoStopNetMessage.Type.StartGame:
                MySeat = msg.seat;
                PlayerCount = msg.playerCount;
                OnGameStarting?.Invoke(msg.seat, msg.playerCount);
                break;
            default:
                // StateSync·Event 등 실제 턴 메시지 — 게스트는 호스트하고만
                // 통신하므로 발신 좌석을 항상 0(호스트)으로 넘긴다.
                OnGameMessage?.Invoke(0, msg);
                break;
        }
    }

    // ── 공통 ────────────────────────────────────────────

    /// <summary>호스트/게스트 어느 쪽으로든 진행 중이던 세션을 전부
    /// 정리한다 — 새 역할로 다시 시작하기 전에 항상 먼저 부른다.</summary>
    public void StopAll()
    {
        if (hostTransport != null) { hostTransport.StopHosting(); Destroy(hostTransport); hostTransport = null; }
        if (advertiser != null) { advertiser.StopAdvertising(); Destroy(advertiser); advertiser = null; }
        if (clientTransport != null) { clientTransport.Disconnect(); Destroy(clientTransport); clientTransport = null; }
        if (scanner != null) { scanner.StopScanning(); Destroy(scanner); scanner = null; }
        IsHost = false;
        IsGuest = false;
        MySeat = -1;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}
