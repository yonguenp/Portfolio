using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

/// <summary>
/// <see cref="IGoStopClientTransport"/>의 순수 TCP 구현 — 호스트 하나에
/// 접속한다. 스레드/메인스레드 디스패치 규칙은 <see cref="TcpGoStopHostTransport"/>
/// 와 동일(그쪽 클래스 문서 참고).
/// </summary>
public class TcpGoStopClientTransport : MonoBehaviour, IGoStopClientTransport
{
    public bool IsConnected { get; private set; }

    public event Action OnConnected;
    public event Action<string> OnDisconnected;
    public event Action<GoStopNetMessage> OnMessage;

    TcpClient client;
    NetworkStream stream;
    Thread connectThread;
    Thread readThread;
    volatile bool closing;

    readonly ConcurrentQueue<Action> mainThreadQueue = new ConcurrentQueue<Action>();

    // TcpGoStopHostTransport.SendTimeoutMs와 같은 이유 — 쓰기가 무한
    // 블로킹되면 그걸 부른 메인 스레드(OnPlayerPlay 등)가 같이 멈춘다.
    // 읽기 타임아웃은 안 건다(상대 턴을 기다리는 정상적인 긴 유휴 상태와
    // 구분이 안 된다).
    const int SendTimeoutMs = 8000;

    // 2026-09-05 — 같은 와이파이 접속(UDP로 이미 찾은 방)은 거의 즉시
    // 붙지만, 포트포워딩으로 연 원격 방을 IP/DuckDNS 직접 입력으로 접속할
    // 땐 포워딩이 잘못돼 있으면 패킷이 그냥 조용히 버려질 뿐이라
    // `TcpClient.Connect`의 기본 OS 타임아웃(수십 초)까지 "접속 중..."
    // 화면이 먹통처럼 걸린다. 명시적으로 10초에서 끊어서 실패를 빨리
    // 알려준다 — LAN 접속엔 사실상 영향 없다(원래도 그보다 훨씬 빨리 붙음).
    //
    // 함정 — 처음엔 TcpClient.BeginConnect+AsyncWaitHandle.WaitOne로
    // 구현했는데, 이 프로젝트의 백그라운드 Thread(스레드풀이 아니라
    // 수동 생성한 System.Threading.Thread) 안에서는 그 WaitOne이 연결
    // 성공/실패와 무관하게 영원히 안 풀렸다(같은 코드를 메인 스레드에서
    // 바로 실행하면 즉시 정상 동작 — 리플렉션으로 직접 재현·확인함).
    // 이 환경에서 백그라운드 스레드의 비동기 소켓 완료 신호가 정상적으로
    // 안 오는 것으로 보인다 — 원인 불명이지만, 기존에 이미 검증된 동기
    // Connect() 자체는 아무 문제 없이 백그라운드 스레드에서 잘 작동한다
    // (이 프로젝트가 처음부터 그 방식을 써 왔다). 그래서 비동기 API 대신,
    // **동기 Connect는 그대로 두고 별도 워치독 코루틴(메인 스레드)이 시간
    // 초과 시 소켓을 강제로 닫아 그 블로킹 호출을 깨우는** 방식으로
    // 바꿨다 — 소켓이 닫히면 블로킹 중이던 Connect()가 예외를 던지고,
    // 그 예외는 아래 catch가 정상적으로 잡아 OnDisconnected로 통지한다.
    const int ConnectTimeoutMs = 10000;
    Coroutine connectWatchdog;

    public void Connect(string ip, int port)
    {
        if (client != null) Disconnect(); // 이전 시도가 남아 있으면 정리하고 새로 시작
        closing = false;
        var newClient = new TcpClient();
        client = newClient; // 워치독이 정확히 "이번 시도"만 닫도록 로컬로 캡처해 둔다
        connectThread = new Thread(() => ConnectWorker(newClient, ip, port)) { IsBackground = true };
        connectThread.Start();
        if (connectWatchdog != null) StopCoroutine(connectWatchdog);
        connectWatchdog = StartCoroutine(ConnectTimeoutWatchdog(newClient));
    }

    System.Collections.IEnumerator ConnectTimeoutWatchdog(TcpClient target)
    {
        yield return new WaitForSeconds(ConnectTimeoutMs / 1000f);
        connectWatchdog = null;
        if (!IsConnected && client == target && !closing)
        {
            try { target.Close(); } catch { /* 이미 닫혔거나 정리된 경우 — 무시 */ }
        }
    }

    void ConnectWorker(TcpClient c, string ip, int port)
    {
        try
        {
            c.Connect(ip, port); // 블로킹 — 그래서 별도 스레드에서 돈다(워치독이 시간 초과 시 이 소켓을 강제로 닫아 깨운다)
            c.SendTimeout = SendTimeoutMs;
            stream = c.GetStream();
        }
        catch (Exception e)
        {
            string reason = e.Message;
            mainThreadQueue.Enqueue(() => OnDisconnected?.Invoke($"접속 실패: {reason}"));
            return;
        }

        mainThreadQueue.Enqueue(() => { IsConnected = true; OnConnected?.Invoke(); });

        readThread = new Thread(ReadLoop) { IsBackground = true };
        readThread.Start();
    }

    void ReadLoop()
    {
        while (!closing)
        {
            GoStopNetMessage msg;
            string readError = null;
            try { msg = GoStopWireCodec.Read(stream); }
            catch (Exception e) { msg = null; readError = $"{e.GetType().Name}: {e.Message}"; }

            if (msg == null)
            {
                if (closing) return; // Disconnect()가 이미 정리 중 — 중복 통지 방지
                if (readError != null) Debug.LogWarning($"[GoStopNet] 읽기 실패: {readError}"); // Debug.Log는 백그라운드 스레드에서도 안전
                mainThreadQueue.Enqueue(() => { IsConnected = false; OnDisconnected?.Invoke("연결이 끊김"); });
                return;
            }

            mainThreadQueue.Enqueue(() => OnMessage?.Invoke(msg));
        }
    }

    public void Send(GoStopNetMessage msg)
    {
        if (!IsConnected || stream == null) return;
        try { GoStopWireCodec.Write(stream, msg); }
        catch (Exception e)
        {
            Debug.LogWarning($"[GoStopNet] Send 실패 ({msg.type}): {e.GetType().Name}: {e.Message}");
            IsConnected = false;
            OnDisconnected?.Invoke("전송 실패");
        }
    }

    public void Disconnect()
    {
        closing = true;
        IsConnected = false;
        try { stream?.Close(); } catch { }
        try { client?.Close(); } catch { }
        stream = null;
        client = null;
    }

    void Update()
    {
        while (mainThreadQueue.TryDequeue(out var action)) action();
    }

    void OnDestroy() => Disconnect();
}
