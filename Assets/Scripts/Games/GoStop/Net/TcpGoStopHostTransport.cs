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

    public event Action<int> OnGuestJoined;
    public event Action<int, string> OnGuestLeft;
    public event Action<int, GoStopNetMessage> OnMessage;

    class GuestConn
    {
        public int seat;
        public TcpClient client;
        public NetworkStream stream;
        public Thread readThread;
        public volatile bool closing;
    }

    TcpListener listener;
    Thread acceptThread;
    int maxGuestsField;

    readonly object guestsLock = new object();
    readonly Dictionary<int, GuestConn> guests = new Dictionary<int, GuestConn>();

    // 백그라운드 스레드 → 메인 스레드로 넘길 작업들. Action 하나가
    // "이벤트 하나 발사"에 해당한다 — 종류별로 큐를 나누지 않고
    // 델리게이트 자체를 큐에 넣는 게 제일 단순하다.
    readonly ConcurrentQueue<Action> mainThreadQueue = new ConcurrentQueue<Action>();

    public void StartHosting(int port, int maxGuests = 3)
    {
        if (IsHosting) return;
        maxGuestsField = maxGuests;
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
            foreach (var g in guests.Values) CloseGuest(g, "호스트가 방을 닫음");
            guests.Clear();
        }
    }

    void AcceptLoop()
    {
        while (IsHosting)
        {
            TcpClient client;
            try { client = listener.AcceptTcpClient(); }
            catch { break; } // listener.Stop()이 호출되면 여기서 예외로 빠져나온다 — 정상 종료 경로

            int seat = -1;
            lock (guestsLock)
            {
                if (guests.Count >= maxGuestsField) { client.Close(); continue; } // 정원 초과 — 조용히 거절
                for (int s = 1; s <= maxGuestsField; s++)
                {
                    if (!guests.ContainsKey(s)) { seat = s; break; }
                }
                if (seat < 0) { client.Close(); continue; }

                client.SendTimeout = SendTimeoutMs; // 위 SendTimeoutMs 문서 참고 — 쓰기가 무한 블로킹되는 것 방지
                var conn = new GuestConn { seat = seat, client = client, stream = client.GetStream() };
                conn.readThread = new Thread(() => ReadLoop(conn)) { IsBackground = true };
                guests[seat] = conn;
                conn.readThread.Start();
            }

            int joinedSeat = seat;
            mainThreadQueue.Enqueue(() => OnGuestJoined?.Invoke(joinedSeat));
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

    void HandleGuestGone(GuestConn conn, string reason)
    {
        lock (guestsLock)
        {
            if (!guests.TryGetValue(conn.seat, out var current) || current != conn) return; // 이미 정리됨
            guests.Remove(conn.seat);
        }
        CloseGuest(conn, reason);
        int seat = conn.seat;
        mainThreadQueue.Enqueue(() => OnGuestLeft?.Invoke(seat, reason));
    }

    static void CloseGuest(GuestConn conn, string reason)
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
                // 원인을 알 수 있게 한다.
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

    void Update()
    {
        // 한 프레임에 몰려 들어와도 다 처리 — 턴제 게임이라 프레임당
        // 몇 개 안 되므로 무한루프 걱정은 없다.
        while (mainThreadQueue.TryDequeue(out var action)) action();
    }

    void OnDestroy() => StopHosting();
}
