using System;

/// <summary>
/// 고스톱 네트워크 대전의 통신 계층 인터페이스 — 호스트용과 클라이언트용을
/// 따로 둔다. 호스트는 <b>최대 3명(게스트)</b>과 동시에 연결을 유지해야
/// 하고(호스트 자신 포함 최대 4인), 클라이언트는 호스트 하나에만 붙으면
/// 되므로 두 역할의 모양 자체가 다르다 — 억지로 하나의 "피어" 인터페이스로
/// 합치면 호스트 쪽에서 "지금 이게 몇 번 게스트가 보낸 메시지인지"를
/// 표현할 방법이 없어진다.
///
/// 좌석 번호 규약: <b>호스트 = 항상 0</b>, 게스트는 접속한 순서대로
/// 1, 2, 3을 받는다. 참가자가 2명(호스트+게스트1)이면 좌석 0/1만 쓰는
/// 맞고, 3~4명이면 3~4인 고스톱으로 <c>GoStopNetLobby</c>가 좌석 수를
/// 보고 갈라 띄운다.
///
/// 콜백은 전부 <b>메인 스레드</b>에서 불린다 — 실제 소켓 읽기는 백그라운드
/// 스레드에서 돌지만 Unity API는 메인 스레드에서만 호출 가능하므로,
/// 구현체가 큐잉 + 메인 스레드 디스패치(MonoBehaviour.Update)를 책임진다.
/// </summary>
public interface IGoStopHostTransport
{
    /// <summary>지정한 포트에서 게스트 접속을 받기 시작한다.
    /// <paramref name="maxGuests"/>를 넘는 접속은 즉시 거절한다(3인용
    /// 고스톱 4인 정원을 넘길 수 없으므로).</summary>
    void StartHosting(int port, int maxGuests = 3);

    /// <summary>리스닝을 멈추고 모든 게스트 연결을 끊는다.</summary>
    void StopHosting();

    /// <summary>지금 리스닝 중인지.</summary>
    bool IsHosting { get; }

    /// <summary>현재 접속해 있는 게스트 수(호스트 자신 제외).</summary>
    int ConnectedGuestCount { get; }

    /// <summary>지금 실제로 연결돼 있는 게스트들의 좌석 번호. 좌석마다
    /// 다른 내용을 보내야 하는 메시지(StartGame 등)를 "이 좌석에 정말
    /// 연결이 있는지" 미리 확인하지 않고 <see cref="Send"/>만 호출하면,
    /// 연결이 실제로는 없는(또는 막 끊긴) 좌석으로 조용히 실패하는
    /// 메시지가 아무 로그도 없이 사라질 수 있다 — 호출부가 이 목록으로
    /// 먼저 걸러 쓰면 그런 침묵 실패를 원천적으로 피할 수 있다.</summary>
    System.Collections.Generic.IEnumerable<int> ConnectedSeats { get; }

    /// <summary>새 게스트가 접속해서 좌석을 배정받았을 때. 인자는 배정된
    /// 좌석 번호(1~3).</summary>
    event Action<int> OnGuestJoined;

    /// <summary>게스트 연결이 끊겼을 때. 인자는 (좌석 번호, 사유).</summary>
    event Action<int, string> OnGuestLeft;

    /// <summary>게스트로부터 메시지를 받았을 때. 인자는 (보낸 좌석 번호, 메시지).</summary>
    event Action<int, GoStopNetMessage> OnMessage;

    /// <summary>특정 좌석 한 명에게만 보낸다(예: StartGame처럼 좌석마다
    /// 내용이 다른 메시지).</summary>
    void Send(int seat, GoStopNetMessage msg);

    /// <summary>접속해 있는 게스트 전원에게 같은 메시지를 보낸다
    /// (StateSync·LobbyUpdate 등).</summary>
    void Broadcast(GoStopNetMessage msg);
}

/// <summary>클라이언트(게스트) 쪽 — 호스트 하나에만 연결한다.</summary>
public interface IGoStopClientTransport
{
    /// <summary>지정한 IP·포트의 호스트에 접속을 시도한다. 성공/실패는
    /// <see cref="OnConnected"/>/<see cref="OnDisconnected"/>로 비동기 통지된다.</summary>
    void Connect(string ip, int port);

    /// <summary>연결을 끊는다.</summary>
    void Disconnect();

    /// <summary>지금 호스트와 연결돼 있는지.</summary>
    bool IsConnected { get; }

    /// <summary>연결에 성공했을 때.</summary>
    event Action OnConnected;

    /// <summary>연결이 끊겼을 때(정상/비정상 모두) — 인자는 사유.</summary>
    event Action<string> OnDisconnected;

    /// <summary>호스트로부터 메시지를 받았을 때.</summary>
    event Action<GoStopNetMessage> OnMessage;

    /// <summary>호스트에게 메시지 한 개를 보낸다. 연결 전이면 조용히
    /// 무시한다(호출부가 매번 IsConnected를 확인할 필요 없게).</summary>
    void Send(GoStopNetMessage msg);
}
