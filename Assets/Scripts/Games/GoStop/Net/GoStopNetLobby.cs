using System;
using System.Collections;
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
/// 총 인원(호스트 포함) 2명 = 맞고, 3명 = 진짜 3인 고스톱, 4명 = 4인
/// 고스톱(광팔이 로테이션). 3인은 광팔이 로테이션이 아예 없는
/// <b>진짜 3인 모드</b>로 새로 만든다(SEATS=4 고정 후 한 자리를 AI로
/// 채우는 방식은 채택 안 함 — 실제 플레이어끼리만 하는 걸 원함).
/// 2026-08-23(씬 통합): 2/3/4인 전부 <c>GoStop3PScene</c> 하나로 들어간다.
/// 2026-08-26: 2인 전용이던 <c>GoStopScene</c>/<c>GoStopGame.cs</c>는
/// 완전히 삭제했다 — <c>GoStop3PGame</c> 하나가 2~4인을 전부 처리한다.
///
/// <b>좌석 번호 = 접속 순서.</b> 호스트가 항상 0, 게스트는 1·2·3을
/// 접속한 순서대로 받는다 — <see cref="TcpGoStopHostTransport"/>가 이미
/// 이 규약으로 배정하므로 여기서는 그대로 따르기만 하면 된다.
///
/// <b>지금 이 클래스가 하는 일과 안 하는 일:</b> 대기실 상태 관리(입장/
/// 퇴장/시작 신호)까지가 이 파일의 책임이다. 실제 게임 씬에서 "카드를
/// 냈다" 같은 턴 메시지를 받아 <c>GoStop3PGame</c>의 AI 자리를 대체하는
/// 작업은 그 파일 쪽에서 직접 처리한다.
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
    /// 살아남는다) <c>GoStop3PGame</c>의 Start()가 이 값을 읽어 좌석 수·
    /// 게임 모드를 정한다. 2=맞고, 3/4=고스톱.</summary>
    public int PlayerCount { get; private set; }

    /// <summary>1점 가격(원) — design.md §49.2. 방 생성 시 호스트가 정하고
    /// (기본 100원, 기존 고정 상수와 동일) 게임 진행 중에는 안 바뀐다.
    /// 게스트는 StartGame 메시지로 이 값을 그대로 전달받는다(표시용 —
    /// 실제 정산은 호스트만 계산하므로 게스트가 이 값을 몰라도 결과는
    /// 어긋나지 않는다).</summary>
    public int PointPrice { get; private set; } = DefaultPointPrice;

    public const int DefaultPointPrice = 100;
    public const int MinPointPrice = 10;
    /// <summary>1점 가격 최댓값. design.md는 "호스트 보유 머니 이하"로
    /// 제한하라고 하는데, 네트워크 판은 로컬 저장 잔액을 안 쓰고 항상
    /// <c>STARTING_MONEY</c>(10만원, GoStop3PGame.cs와 동일한 값)로 새로
    /// 시작한다 — 그래서 "호스트 보유 머니"라는 게 이 시점엔 사실상 이
    /// 값 하나뿐이라 그대로 상한으로 쓴다. design.md 자체의 절대 상한
    /// (100만원)보다 낮지만, 실제로 의미 있는 제약은 이쪽이다.</summary>
    public const int MaxPointPrice = 100_000;

    /// <summary>선택 가능한 단계 — 임의의 숫자를 직접 입력하는 대신
    /// 프리셋 사이를 오가게 했다(2026-08-23). 이 프로젝트에 TMP_InputField
    /// 전례가 없어 새로 들여오는 리스크보다, 어차피 [10, 10만원] 범위를
    /// 벗어날 수 없는 스텝퍼 쪽이 안전하다고 판단했다 — 값 자체는
    /// design.md가 요구하는 범위·상한을 그대로 만족한다.</summary>
    public static readonly int[] PointPriceSteps = { 10, 50, 100, 500, 1_000, 5_000, 10_000, 50_000, 100_000 };

    /// <summary>1점 가격을 한 단계 올리거나(+1) 내린다(-1). 이미 양 끝이면
    /// 그대로 둔다. 호스트가 방을 만들기 전(Home 화면)에만 호출된다 —
    /// 게임 진행 중에는 이 값을 바꿀 UI 자체가 없다.</summary>
    public void StepPointPrice(int direction)
    {
        int idx = System.Array.IndexOf(PointPriceSteps, PointPrice);
        if (idx < 0) idx = System.Array.IndexOf(PointPriceSteps, DefaultPointPrice); // 방어적 폴백
        idx = Mathf.Clamp(idx + (direction > 0 ? 1 : -1), 0, PointPriceSteps.Length - 1);
        PointPrice = PointPriceSteps[idx];
    }

    string myName;

    TcpGoStopHostTransport hostTransport;
    GoStopRoomAdvertiser advertiser;

    TcpGoStopClientTransport clientTransport;
    GoStopRoomScanner scanner;

    /// <summary>대기실 인원이 바뀔 때마다(입장/퇴장/이름 갱신) 호출된다.
    /// UI가 이걸 구독해서 "N/4명" 같은 걸 다시 그리면 된다.</summary>
    public event Action OnLobbyChanged;

    /// <summary>게임이 시작됐다 — 인자는 (내 좌석, 총 인원). 호스트·게스트
    /// 양쪽 다 이 이벤트를 받는다. 2026-08-23부터 UI는 인원수와 무관하게
    /// 항상 <c>GoStop3PScene</c> 하나만 여는데(GoStop3PGame이 SEATS=2~4를
    /// 다 처리한다), 이 이벤트 시그니처 자체는 그대로 둔다 — UI가 "몇
    /// 명인지" 알아야 안내 문구 등을 표시할 수 있어서다.</summary>
    public event Action<int, int> OnGameStarting;

    /// <summary>연결이 끊기거나 방이 닫혔을 때 — 인자는 사유 문자열.
    /// 게스트 전용(호스트와의 TCP 연결 자체가 끊어졌을 때).</summary>
    public event Action<string> OnDisconnected;

    /// <summary>호스트 전용 — 접속해 있던 게스트 한 명의 소켓이 끊겼다
    /// (앱 종료·네트워크 끊김 등). 인자는 그 좌석 번호. <b>2026-08-24
    /// 이후로는 이것만으로 판을 끝내면 안 된다</b> — 게임이 시작된 뒤라면
    /// 이 좌석은 재접속 유예(design.md §50.2) 중일 뿐이라, 이 이벤트는
    /// "잠깐 끊겼다"는 안내(토스트 등)에만 쓰고, 실제로 그 좌석을 포기하고
    /// 정리(다운그레이드/게임 종료)하는 판단은 반드시
    /// <see cref="OnGuestGoneForGood"/>를 기다려서 내려야 한다.</summary>
    public event Action<int> OnGuestLeftDuringGame;

    /// <summary>호스트 전용(design.md §50.2) — 게임 중 끊긴 좌석이 재접속
    /// 유예 시간 안에 못 돌아와 영구 이탈로 확정됐다. 게임 씬은 이 시점에
    /// 서야 그 좌석을 포기하고 다운그레이드/게임 종료를 판단해야 한다.</summary>
    public event Action<int> OnGuestGoneForGood;

    /// <summary>호스트 전용 — 유예 중이던 좌석이 같은 clientId로 되돌아와
    /// 정상 복귀했다. 인자는 좌석 번호. 게임 씬이 이걸 받으면 그 좌석에게
    /// 최신 상태를 다시 보내줘야 한다(그동안 놓친 StateSync를 못 받았으므로).</summary>
    public event Action<int> OnGuestReconnected;

    /// <summary>게스트 전용(design.md §50.2) — 게임 중 호스트와의 연결이
    /// 끊겨 자동 재접속을 시도하는 동안 발사된다. UI가 "재접속 중..."
    /// 안내를 띄우는 데 쓴다. 재접속이 성공하면 <see cref="OnReconnected"/>,
    /// 유예 시간을 넘기면 평소처럼 <see cref="OnDisconnected"/>가 최종
    /// 통보한다.</summary>
    public event Action OnReconnecting;

    /// <summary>게스트 전용 — 자동 재접속이 성공했다. 호스트가 뒤이어
    /// 최신 StateSync를 보내오므로 화면은 곧 정상으로 돌아온다.</summary>
    public event Action OnReconnected;

    /// <summary>게스트 전용(design.md §49.4 네트워크 확장) — 다른 좌석이
    /// 영구 이탈해서 좌석이 압축된 뒤, 내 새 좌석 번호와 새 인원수를
    /// 알려준다. 씬 재로딩 없이 게임 씬이 <c>SetMySeat</c>/<c>SetSeatCount</c>
    /// 를 다시 불러 제자리에서 이어가야 한다.</summary>
    public event Action<int, int> OnSeatReassigned;

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

    /// <summary>호스트 전용(design.md §49.4 네트워크 확장) — 좌석 압축 뒤
    /// 트랜스포트의 좌석↔소켓 매핑을 게임 쪽 새 번호에 맞춰 다시 붙인다.</summary>
    public void RenumberSeats(Dictionary<int, int> oldToNew) => hostTransport?.RenumberSeats(oldToNew);

    /// <summary>호스트 전용 — 이 시점부터 접속 끊김이 재접속 유예를
    /// 거친다(design.md §50.2). <c>GoStop3PGame</c>이 실제로 판을 시작할 때
    /// (딜링 직후) 부른다.</summary>
    public void MarkGameStarted() => hostTransport?.MarkGameStarted();

    /// <summary>이 기기의 영구 식별자 — 앱을 다시 켜도 같은 값이라 재접속
    /// (design.md §50.2)의 판별 근거로 그대로 쓸 수 있다. 새 GUID를 따로
    /// 만들어 저장할 필요 없이 플랫폼이 이미 제공하는 값을 재사용한다.</summary>
    string MyClientId => SystemInfo.deviceUniqueIdentifier;

    // 게스트 전용 — 자동 재접속 시도에 필요한, 최초 접속 때 썼던 주소.
    string lastHostIp;
    int lastHostPort;
    Coroutine reconnectCoroutine;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ── 호스트 ──────────────────────────────────────────

    /// <summary>웹 빌드에서는 브라우저 샌드박스가 raw TCP/UDP 소켓 자체를
    /// 막아놔서(System.Net.Sockets가 링크는 되더라도 실제로 열리지 않는다)
    /// 이 프로젝트의 P2P 구조가 근본적으로 성립하지 않는다 — 버그가 아니라
    /// 플랫폼 한계다. 실제로 소켓을 열어보려다 알 수 없는 예외로 멈추는 대신,
    /// 이미 있는 "연결 실패" 화면(GoStopNetLobbyUI의 Screen.Error)을 그대로
    /// 재사용해 명확한 사유를 즉시 보여준다.</summary>
    static bool IsPlatformUnsupported =>
        Application.platform == RuntimePlatform.WebGLPlayer;
    const string PlatformUnsupportedMessage = "웹 버전은 브라우저 보안 정책상 네트워크 대전을 지원하지 않습니다.\nPC 또는 모바일 앱에서 이용해주세요.";

    public void HostRoom(string displayName, int port = DefaultPort)
    {
        if (IsPlatformUnsupported) { OnDisconnected?.Invoke(PlatformUnsupportedMessage); return; }
        StopAll();
        myName = displayName;
        IsHost = true;
        MySeat = 0;
        PlayerNames = new string[4];
        PlayerNames[0] = displayName;

        hostTransport = gameObject.AddComponent<TcpGoStopHostTransport>();
        hostTransport.OnGuestJoined += HostOnGuestJoined;
        hostTransport.OnGuestLeft += HostOnGuestLeft;
        hostTransport.OnGuestGoneForGood += (seat, reason) => OnGuestGoneForGood?.Invoke(seat);
        hostTransport.OnGuestReconnected += seat => OnGuestReconnected?.Invoke(seat);
        hostTransport.OnMessage += HostOnMessage;
        hostTransport.StartHosting(port, maxGuests: 3);

        advertiser = gameObject.AddComponent<GoStopRoomAdvertiser>();
        advertiser.CurrentPlayerCount = 1;
        advertiser.StartAdvertising(displayName, port);

        OnLobbyChanged?.Invoke();
    }

    void HostOnGuestJoined(int seat, bool isReconnect)
    {
        // 이름은 아직 모른다 — 게스트가 접속 직후 보내는 Hello를 받아야
        // PlayerNames가 채워진다(재접속이면 곧바로 같은 이름으로 다시
        // 채워질 뿐이다). 그때까지는 "누군가 들어왔다"만 알린다.
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
        // GoStop3PGame이 구독해서 "재접속 대기 중" 안내에 쓴다(2026-08-24
        // 부터는 이 이벤트만으로 판을 끝내면 안 된다 — 위 문서 참고,
        // 실제 종료 판단은 OnGuestGoneForGood를 기다린다).
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
            hostTransport.Send(seat, GoStopNetMessage.StartGameMsg(seat, total, PointPrice));
        }
        PlayerCount = total;
        // design.md §50.2 — 이 시점부터 접속 끊김은 재접속 유예를 거친다.
        // 판이 시작되기 전(로비 단계) 끊김은 예전처럼 즉시 최종 처리다.
        hostTransport.MarkGameStarted();
        OnGameStarting?.Invoke(0, total);
        return true;
    }

    // ── 게스트 ──────────────────────────────────────────

    public void StartScanningForRooms()
    {
        if (IsPlatformUnsupported) { OnDisconnected?.Invoke(PlatformUnsupportedMessage); return; }
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
        lastHostIp = room.ip;
        lastHostPort = room.tcpPort;

        clientTransport = gameObject.AddComponent<TcpGoStopClientTransport>();
        clientTransport.OnConnected += GuestOnConnected;
        clientTransport.OnMessage += GuestOnMessage;
        clientTransport.OnDisconnected += GuestOnDisconnected;
        clientTransport.Connect(room.ip, room.tcpPort);
    }

    void GuestOnConnected()
    {
        clientTransport.Send(GoStopNetMessage.Hello(myName, MyClientId));
    }

    /// <summary>design.md §50.2 — 게임이 이미 시작된 뒤(PlayerCount>0)의
    /// 끊김은 곧바로 "연결 끊김" 통보 대신 자동 재접속을 먼저 시도한다.
    /// 로비 단계(아직 시작 전)는 예전처럼 즉시 통보한다 — 그 단계는
    /// 재접속으로 되돌아갈 "진행 중인 판" 자체가 없다.</summary>
    void GuestOnDisconnected(string reason)
    {
        // 이미 재접속 루프가 도는 중이면 그 루프의 임시 핸들러가 이번
        // 실패를 직접 보고 있다(아래 ReconnectLoop의 OnFail) — 여기서
        // 또 반응하면 유예 시간이 남았는데도 성급하게 최종 통보를 해버린다.
        if (reconnectCoroutine != null) return;
        if (PlayerCount > 0)
        {
            reconnectCoroutine = StartCoroutine(ReconnectLoop(reason));
            return;
        }
        OnDisconnected?.Invoke(reason);
    }

    IEnumerator ReconnectLoop(string reason)
    {
        OnReconnecting?.Invoke();
        float deadline = Time.unscaledTime + TcpGoStopHostTransport.ReconnectGraceSeconds;
        const float RetryIntervalSeconds = 3f;
        while (Time.unscaledTime < deadline)
        {
            bool connected = false;
            bool failed = false;
            void OnOk() => connected = true;
            void OnFail(string r) => failed = true;
            clientTransport.OnConnected += OnOk;
            clientTransport.OnDisconnected += OnFail;
            clientTransport.Connect(lastHostIp, lastHostPort);
            yield return new WaitUntil(() => connected || failed || Time.unscaledTime >= deadline);
            clientTransport.OnConnected -= OnOk;
            clientTransport.OnDisconnected -= OnFail;
            if (connected)
            {
                reconnectCoroutine = null;
                OnReconnected?.Invoke();
                yield break;
            }
            yield return new WaitForSeconds(RetryIntervalSeconds);
        }
        reconnectCoroutine = null;
        OnDisconnected?.Invoke(reason); // 유예 시간 초과 — 이제야 진짜 최종 통보
    }

    void GuestOnMessage(GoStopNetMessage msg)
    {
        switch (msg.type)
        {
            case GoStopNetMessage.Type.LobbyUpdate:
                PlayerNames = msg.playerNames ?? new string[4];
                OnLobbyChanged?.Invoke();
                break;
            case GoStopNetMessage.Type.SeatReassign:
                MySeat = msg.seat;
                PlayerCount = msg.playerCount;
                OnSeatReassigned?.Invoke(msg.seat, msg.playerCount);
                break;
            case GoStopNetMessage.Type.StartGame:
                MySeat = msg.seat;
                PlayerCount = msg.playerCount;
                PointPrice = msg.pointPrice > 0 ? msg.pointPrice : DefaultPointPrice; // 방어적 폴백(구버전 호스트 등)
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
        if (reconnectCoroutine != null) { StopCoroutine(reconnectCoroutine); reconnectCoroutine = null; }
        if (hostTransport != null) { hostTransport.StopHosting(); Destroy(hostTransport); hostTransport = null; }
        if (advertiser != null) { advertiser.StopAdvertising(); Destroy(advertiser); advertiser = null; }
        if (clientTransport != null) { clientTransport.Disconnect(); Destroy(clientTransport); clientTransport = null; }
        if (scanner != null) { scanner.StopScanning(); Destroy(scanner); scanner = null; }
        IsHost = false;
        IsGuest = false;
        MySeat = -1;
        PlayerCount = 0;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}
