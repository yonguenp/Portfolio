using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

/// <summary>
/// <see cref="IGoStopHostTransport"/>의 순수 TCP 구현. <see cref="TcpListener"/>
/// 하나로 최대 3명의 게스트 접속을 동시에 받는다(호스트 자신 포함 최대 4인).
///
/// <b>스레드 구조</b> — Accept 루프 1개(새 게스트 받기) + 게스트별 읽기
/// 루프 1개씩, 전부 백그라운드 스레드다. Unity API(이벤트 발사 등)는
/// 메인 스레드에서만 안전하므로, 백그라운드 스레드는 <see cref="mainThreadQueue"/>
/// (스레드 안전한 <c>ConcurrentQueue</c>)에 "할 일"만 넣어두고, 이
/// 컴포넌트의 <see cref="Update"/>가 매 프레임 그 큐를 비우면서 실제
/// 이벤트를 발사한다. 소켓 쓰기(Send/Broadcast)는 반대로 항상 메인
/// 스레드에서만 호출된다는 전제이므로 별도 동기화가 필요 없다(게스트별로
/// "읽기 스레드 1개가 그 소켓만 읽는다"·"메인 스레드만 그 소켓에 쓴다"
/// 규칙만 지키면 됨 — 같은 소켓을 읽기/쓰기 서로 다른 스레드가 동시에
/// 다루는 건 .NET 소켓에서 안전하다).
///
/// <b>2026-08-24 — 재접속(design.md §50.2) 지원.</b> 접속 순서로만 좌석을
/// 정하던 것을, Accept 직후 <b>Hello를 먼저 동기적으로 읽어</b>(clientId
/// 포함) 그 clientId가 유예 중인(<see cref="pendingReconnect"/>) 좌석과
/// 일치하면 그 좌석을 그대로 돌려주도록 바꿨다 — 안 그러면 재접속해도
/// "새 게스트"로 보여 엉뚱한 좌석에 배정되거나(게임 시작 후엔 새 참가
/// 자체를 막아뒀으니) 아예 거절당한다. 게임 시작 전(로비)에는 유예 개념이
/// 없으므로 예전과 동일하게 즉시 다음 빈 자리를 받는다.
/// </summary>
public class TcpGoStopHostTransport : MonoBehaviour, IGoStopHostTransport
{
    public bool IsHosting { get; private set; }
    public int ConnectedGuestCount { get { lock (guestsLock) return guests.Count; } }
    public IEnumerable<int> ConnectedSeats { get { lock (guestsLock) return new List<int>(guests.Keys); } }

    // 소켓 쓰기가 막힌 채(수신 쪽이 안 받아가는 등) 무한정 블로킹되는 걸
    // 막는다 — 기본값(0=무한대기)이면 상대 네트워크가 잠깐만 불안정해도
    // Send/Broadcast가 영원히 멈춰서, 그걸 호출한 메인 스레드 로직
    // (예: HostStartGame → 씬 전환) 전체가 같이 멈춰버린다("씬 이동이
    // 오래 걸린다"는 신고의 유력한 원인). 다 쓰는 데 이 시간을 넘기면
    // 예외로 실패 처리하고 연결을 끊는다 — 조용히 영원히 매달리는 것보다
    // 명확하게 끊어지는 쪽이 훨씬 낫다. 읽기 타임아웃은 일부러 안 건다 —
    // "상대 턴을 기다린다"는 정상적인 상황이 분 단위로 길어질 수 있어서
    // 짧게 잡으면 멀쩡한 연결도 끊어버린다.
    const int SendTimeoutMs = 8000;

    // Accept 직후 Hello를 기다리는 시간 — 느리거나 잘못된 클라이언트가
    // Accept 스레드 자체를 영원히 막지 못하게 짧게 잡는다. 정상 클라이언트는
    // 연결 직후 곧바로 Hello를 보내므로(GoStopNetLobby.GuestOnConnected)
    // 5초면 충분하고도 남는다.
    const int HelloTimeoutMs = 5000;

    /// <summary>design.md §50.2 — 판 도중 끊긴 좌석이 재접속을 시도할 수
    /// 있는 유예 시간. 이 안에 같은 clientId로 다시 접속하면 좌석을 그대로
    /// 돌려받는다. 넘기면 <see cref="OnGuestGoneForGood"/>가 최종 통보한다.
    /// design.md는 정확한 초 단위를 프로젝트가 정하라고 위임했다 — 너무
    /// 짧으면 와이파이가 잠깐 흔들린 것도 영구 이탈로 처리되고, 너무 길면
    /// 나머지 인원이 그 자리를 오래 붙들려 있게 된다(다만 §50.1 입력
    /// 타임아웃이 그 좌석의 턴/결정을 자동으로 넘겨주므로 진행 자체는
    /// 안 막힌다). 30초로 잡았다.</summary>
    public const float ReconnectGraceSeconds = 30f;

    public event Action<int, bool> OnGuestJoined;
    public event Action<int, string> OnGuestLeft;
    public event Action<int, string> OnGuestGoneForGood;
    public event Action<int> OnGuestReconnected;
    public event Action<int, GoStopNetMessage> OnMessage;

    class GuestConn
    {
        public int seat;
        public string clientId;
        public TcpClient client;
        public NetworkStream stream;
        public Thread readThread;
        public volatile bool closing;
    }

    struct PendingReconnect
    {
        public string clientId;
        public float deadline; // Time.unscaledTime 기준
    }

    TcpListener listener;
    Thread acceptThread;
    int maxGuestsField;
    volatile bool gameStarted;

    readonly object guestsLock = new object();
    readonly Dictionary<int, GuestConn> guests = new Dictionary<int, GuestConn>();
    // 게임 시작 후 끊긴 좌석의 유예 상태 — guestsLock으로 같이 보호한다.
    readonly Dictionary<int, PendingReconnect> pendingReconnect = new Dictionary<int, PendingReconnect>();

    // 백그라운드 스레드 → 메인 스레드로 넘길 작업들. Action 하나가
    // "이벤트 하나 발사"에 해당한다 — 종류별로 큐를 나누지 않고
    // 델리게이트 자체를 큐에 넣는 게 제일 단순하다.
    readonly ConcurrentQueue<Action> mainThreadQueue = new ConcurrentQueue<Action>();

    public void StartHosting(int port, int maxGuests = 3)
    {
        if (IsHosting) return;
        maxGuestsField = maxGuests;
        gameStarted = false;
        listener = new TcpListener(IPAddress.Any, port);
        listener.Start();
        IsHosting = true;
        acceptThread = new Thread(AcceptLoop) { IsBackground = true };
        acceptThread.Start();
    }

    public void StopHosting()
    {
        if (!IsHosting) return;
        IsHosting = false;
        try { listener.Stop(); } catch { }

        lock (guestsLock)
        {
            foreach (var g in guests.Values) CloseGuest(g);
            guests.Clear();
            pendingReconnect.Clear();
        }
    }

    public void MarkGameStarted() => gameStarted = true;

    void AcceptLoop()
    {
        while (IsHosting)
        {
            TcpClient client;
            try { client = listener.AcceptTcpClient(); }
            catch { break; } // listener.Stop()이 호출되면 여기서 예외로 빠져나온다 — 정상 종료 경로

            // 2026-08-24: 좌석을 정하기 전에 Hello부터 동기로 받는다 —
            // clientId가 유예 중인 좌석과 일치하는지 봐야 "재접속"인지
            // "새 참가"인지 가를 수 있다. 이 읽기는 Accept 스레드를 잠깐
            // (최대 HelloTimeoutMs) 막지만, 게스트 여러 명이 동시에
            // 몰려도 Accept 자체는 순차 처리라 문제없다(턴제 게임이라
            // 동시 접속이 몰릴 상황 자체가 거의 없다).
            GoStopNetMessage hello;
            NetworkStream stream;
            try
            {
                stream = client.GetStream();
                stream.ReadTimeout = HelloTimeoutMs;
                hello = GoStopWireCodec.Read(stream);
                // 이후엔 정상 턴 대기처럼 무제한으로 되돌린다 — NetworkStream.
                // ReadTimeout은 Stream.ReadTimeout과 달리 0을 "무제한"으로
                // 받아주지 않는다(오히려 "즉시 타임아웃"으로 동작해 다음
                // 블로킹 읽기가 바로 실패한다 — 실제로 Hello 직후 곧바로
                // 연결이 끊기는 버그로 나타났다). System.Threading.Timeout.
                // Infinite(-1)을 명시해야 진짜 무제한이 된다.
                stream.ReadTimeout = System.Threading.Timeout.Infinite;
            }
            catch { client.Close(); continue; }
            if (hello == null || hello.type != GoStopNetMessage.Type.Hello) { client.Close(); continue; }

            int seat = -1;
            bool isReconnect = false;
            GuestConn staleConnToClose = null;
            lock (guestsLock)
            {
                if (!string.IsNullOrEmpty(hello.clientId))
                {
                    foreach (var kv in pendingReconnect)
                    {
                        if (kv.Value.clientId == hello.clientId) { seat = kv.Key; isReconnect = true; break; }
                    }
                    if (isReconnect) pendingReconnect.Remove(seat);

                    // 2026-08-24 — 경쟁 상황 방지: 클라이언트가 자기 쪽에서
                    // "끊김→재접속"을 순식간에 해치우면, 호스트가 옛 소켓의
                    // 죽음을 아직 감지 못해(백그라운드 읽기 스레드가 아직
                    // 실패를 못 봄) pendingReconnect에 등록되기 *전에* 같은
                    // clientId로 새 Hello가 먼저 도착할 수 있다 — 이 경우
                    // 위 pendingReconnect 매칭은 실패하지만, guests 사전에는
                    // 아직 "살아있는 척"하는 옛 연결이 그 clientId로 남아있다.
                    // 그 옛 연결을 찾아 좌석을 그대로 넘겨받고 옛 소켓은
                    // 강제로 닫는다 — 안 그러면 게임 시작 후엔 "새 참가자"로
                    // 오인돼 정당한 재접속이 거절된다.
                    if (seat < 0)
                    {
                        foreach (var kv in guests)
                        {
                            if (kv.Value.clientId == hello.clientId) { seat = kv.Key; isReconnect = true; staleConnToClose = kv.Value; break; }
                        }
                    }
                }
                if (seat < 0)
                {
                    // 재접속이 아니다 — 새 참가자. 게임이 이미 시작됐으면
                    // v1 스코프대로 중간 참가를 받지 않는다(clientId가 다르니
                    // 진짜 새 사람이거나, 유예가 이미 만료된 뒤 뒤늦게
                    // 돌아온 경우 — 어느 쪽이든 지금은 자리가 없다).
                    if (guests.Count >= maxGuestsField || gameStarted) { client.Close(); continue; }
                    for (int s = 1; s <= maxGuestsField; s++)
                    {
                        if (!guests.ContainsKey(s)) { seat = s; break; }
                    }
                    if (seat < 0) { client.Close(); continue; }
                }

                client.SendTimeout = SendTimeoutMs; // 위 SendTimeoutMs 문서 참고 — 쓰기가 무한 블로킹되는 것 방지
                var conn = new GuestConn { seat = seat, clientId = hello.clientId, client = client, stream = stream };
                conn.readThread = new Thread(() => ReadLoop(conn)) { IsBackground = true };
                guests[seat] = conn; // 기존 항목이 있었으면(=staleConnToClose) 여기서 자연히 덮어써진다
                conn.readThread.Start();
            }
            if (staleConnToClose != null)
            {
                staleConnToClose.closing = true; // ReadLoop가 이 소켓의 종료를 OnGuestLeft로 다시 보고하지 않도록 먼저 표시
                CloseGuest(staleConnToClose);
            }

            int joinedSeat = seat;
            bool reconnected = isReconnect;
            mainThreadQueue.Enqueue(() =>
            {
                OnGuestJoined?.Invoke(joinedSeat, reconnected);
                if (reconnected) OnGuestReconnected?.Invoke(joinedSeat);
                // Hello 자체도 평범한 메시지로 한 번 더 통지한다 —
                // GoStopNetLobby.HostOnMessage가 이걸로 PlayerNames를
                // 채우던 기존 경로를 그대로 재사용하기 위해서다(이 클래스가
                // Hello를 가로채기 전에는 ReadLoop가 이 통지를 자연히
                // 했었다 — 가로챈 지금도 동등하게 한 번은 통지해야 한다).
                OnMessage?.Invoke(joinedSeat, hello);
            });
        }
    }

    void ReadLoop(GuestConn conn)
    {
        while (!conn.closing)
        {
            GoStopNetMessage msg;
            string readError = null;
            try { msg = GoStopWireCodec.Read(conn.stream); }
            catch (System.Exception e) { msg = null; readError = $"{e.GetType().Name}: {e.Message}"; }

            if (msg == null)
            {
                // Debug.Log 계열은 백그라운드 스레드에서 불러도 안전하다
                // (Unity API 중 예외적으로 스레드 세이프)
                if (readError != null) Debug.LogWarning($"[GoStopNet] 읽기 실패 (seat {conn.seat}): {readError}");
                HandleGuestGone(conn, "연결이 끊김");
                return;
            }

            int seat = conn.seat;
            mainThreadQueue.Enqueue(() => OnMessage?.Invoke(seat, msg));
        }
    }

    /// <summary>연결이 끊겼다 — 게임 시작 전이면 즉시 최종 처리(예전과
    /// 동일), 게임 시작 후면 곧바로 좌석을 비우지 않고 유예 목록에
    /// 올린다(design.md §50.2). 유예 만료는 <see cref="Update"/>가
    /// 매 프레임 확인한다.</summary>
    void HandleGuestGone(GuestConn conn, string reason)
    {
        bool wasGameStarted = gameStarted;
        lock (guestsLock)
        {
            if (!guests.TryGetValue(conn.seat, out var current) || current != conn) return; // 이미 정리됨
            guests.Remove(conn.seat);
            if (wasGameStarted)
                pendingReconnect[conn.seat] = new PendingReconnect { clientId = conn.clientId, deadline = 0f }; // 데드라인은 Update에서 실제 Time으로 세팅
        }
        CloseGuest(conn);
        int seat = conn.seat;
        mainThreadQueue.Enqueue(() =>
        {
            // 데드라인은 메인 스레드 Time.unscaledTime 기준으로 여기서 확정한다
            // (백그라운드 스레드에서는 UnityEngine.Time에 접근할 수 없다).
            if (wasGameStarted)
            {
                lock (guestsLock)
                {
                    if (pendingReconnect.TryGetValue(seat, out var pr))
                        pendingReconnect[seat] = new PendingReconnect { clientId = pr.clientId, deadline = Time.unscaledTime + ReconnectGraceSeconds };
                }
            }
            OnGuestLeft?.Invoke(seat, reason);
            if (!wasGameStarted) OnGuestGoneForGood?.Invoke(seat, reason); // 로비 단계는 유예가 없으므로 즉시 최종 통보
        });
    }

    static void CloseGuest(GuestConn conn)
    {
        conn.closing = true;
        try { conn.stream?.Close(); } catch { }
        try { conn.client?.Close(); } catch { }
    }

    public void Send(int seat, GoStopNetMessage msg)
    {
        GuestConn conn;
        lock (guestsLock)
        {
            if (!guests.TryGetValue(seat, out conn))
            {
                // 이 좌석에 진짜 연결이 없다 — 예전엔 여기서 조용히
                // return해서, 호출부(예: HostStartGame)가 "보냈다"고
                // 착각한 채 계속 진행했다("호스트는 씬을 넘어갔는데
                // 게스트는 대기 화면에 그대로 있다"는 신고의 유력한
                // 원인 후보). 로그만이라도 남겨서 다음 재현 때 바로
                // 원인을 알 수 있게 한다. 재접속 유예 중인 좌석도 여기
                // 걸린다 — 정상이다(돌아올 때까지는 보낼 데가 없다).
                Debug.LogWarning($"[GoStopNet] Send: seat {seat}에 연결된 게스트가 없음 — {msg.type} 메시지 전송 안 됨");
                return;
            }
        }
        try { GoStopWireCodec.Write(conn.stream, msg); }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[GoStopNet] Send 실패 (seat {seat}, {msg.type}): {e.GetType().Name}: {e.Message}");
            HandleGuestGone(conn, "전송 실패");
        }
    }

    public void Broadcast(GoStopNetMessage msg)
    {
        List<GuestConn> snapshot;
        lock (guestsLock) snapshot = new List<GuestConn>(guests.Values);
        foreach (var conn in snapshot)
        {
            try { GoStopWireCodec.Write(conn.stream, msg); }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[GoStopNet] Broadcast 실패 (seat {conn.seat}, {msg.type}): {e.GetType().Name}: {e.Message}");
                HandleGuestGone(conn, "전송 실패");
            }
        }
    }

    /// <summary>design.md §49.4 네트워크 확장 — 좌석 압축(다운그레이드) 뒤
    /// 이 트랜스포트의 "좌석 번호 → 소켓" 매핑도 게임 쪽 새 번호에 맞춰
    /// 다시 붙인다. 유예 목록에 남아있던 항목 중 살아남지 못한 좌석(=
    /// 제거 대상, oldToNew에 없음)은 그냥 버린다 — 그 좌석은 이미 영구
    /// 이탈이 확정됐기 때문에 압축이 일어난 것이므로 재접속을 기다릴
    /// 이유가 없다.</summary>
    public void RenumberSeats(Dictionary<int, int> oldToNew)
    {
        lock (guestsLock)
        {
            var newGuests = new Dictionary<int, GuestConn>();
            foreach (var kv in guests)
            {
                if (oldToNew.TryGetValue(kv.Key, out int newSeat))
                {
                    kv.Value.seat = newSeat;
                    newGuests[newSeat] = kv.Value;
                }
                // 매핑에 없는 좌석(=이번에 제거된 좌석 본인)은 이미 연결이
                // 끊긴 상태라 여기 없는 게 정상이다.
            }
            guests.Clear();
            foreach (var kv in newGuests) guests[kv.Key] = kv.Value;
            pendingReconnect.Clear(); // 압축이 일어났다는 건 유예가 이미 끝났거나 무관해졌다는 뜻
        }
    }

    void Update()
    {
        // 한 프레임에 몰려 들어와도 다 처리 — 턴제 게임이라 프레임당
        // 몇 개 안 되므로 무한루프 걱정은 없다.
        while (mainThreadQueue.TryDequeue(out var action)) action();

        // 재접속 유예 만료 확인 — 매 프레임 훑기엔 항목이 최대 3개뿐이라
        // 비용이 무의미하다.
        List<int> expired = null;
        lock (guestsLock)
        {
            foreach (var kv in pendingReconnect)
            {
                if (kv.Value.deadline > 0f && Time.unscaledTime >= kv.Value.deadline)
                {
                    (expired ??= new List<int>()).Add(kv.Key);
                }
            }
            if (expired != null) foreach (var s in expired) pendingReconnect.Remove(s);
        }
        if (expired != null) foreach (var s in expired) OnGuestGoneForGood?.Invoke(s, "재접속 유예 시간 초과");
    }

    void OnDestroy() => StopHosting();
}
