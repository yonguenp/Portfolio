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

    public void Connect(string ip, int port)
    {
        if (client != null) Disconnect(); // 이전 시도가 남아 있으면 정리하고 새로 시작
        closing = false;
        connectThread = new Thread(() => ConnectWorker(ip, port)) { IsBackground = true };
        connectThread.Start();
    }

    void ConnectWorker(string ip, int port)
    {
        try
        {
            client = new TcpClient();
            client.Connect(ip, port); // 블로킹 — 그래서 별도 스레드에서 돈다
            client.SendTimeout = SendTimeoutMs;
            stream = client.GetStream();
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
