using System;

/// <summary>
/// UDP 브로드캐스트로 뿌리는 "방 열림" 알림 한 개 — TCP 게임 연결
/// (<see cref="GoStopNetMessage"/>)과는 완전히 별개 채널이다. 게스트가
/// IP를 몰라도 같은 와이파이 안에서 열린 방을 자동으로 찾게 해주는
/// 용도라서, 딱 "누가·몇 명이·어느 포트로 기다리는지"만 담는다.
///
/// <see cref="magic"/>이 있는 이유 — 같은 공유기 아래 다른 UDP 브로드캐스트
/// 트래픽(다른 앱, mDNS 등)이 섞여 들어와도 우리 패킷이 아니면 즉시
/// 버리기 위한 식별 태그. 진짜 보안 목적이 아니라 오탐 필터다.
/// </summary>
[Serializable]
public class GoStopRoomAnnounce
{
    public const string Magic = "GoStopLAN_v1";

    public string magic = Magic;
    public string hostName;
    public int tcpPort;
    public int playerCount;   // 현재 인원(호스트 포함)
    public int maxPlayers;    // 4 고정(호스트+게스트 3명) — 필드로 남겨서 나중에 규칙이 바뀌어도 대응 가능하게
}
