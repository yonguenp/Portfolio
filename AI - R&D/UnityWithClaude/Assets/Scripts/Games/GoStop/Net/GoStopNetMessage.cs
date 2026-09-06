using System;
using System.Linq;

/// <summary>
/// 고스톱 네트워크 대전(같은 로컬 네트워크, 서버 없이 호스트-클라이언트 P2P)의
/// 메시지 봉투. 종류가 많지 않아(플레이어 입력 5종 + 핸드셰이크 + 상태 동기화)
/// 다형 직렬화(polymorphic serialization) 대신 <b>필드를 전부 한 클래스에
/// 평평하게 두고 <see cref="type"/>으로 분기</b>하는 방식을 택했다 — Unity
/// 내장 <c>JsonUtility</c>가 다형 타입을 못 다루므로, 이 방식이 새 패키지
/// (Newtonsoft 등) 없이 가장 단순하게 돌아간다. 메시지 하나가 크지 않아서
/// 안 쓰는 필드가 몇 개 같이 실려도 대역폭 문제가 안 된다(턴마다 한 번,
/// LAN 안에서만 오간다).
///
/// <b>호스트가 유일한 판정 주체다</b> — 클라이언트는 <see cref="Type.PlayCard"/>
/// 류의 "의도"만 보내고, 호스트가 <c>GoStopRules</c>로 판정한 뒤
/// <see cref="Type.StateSync"/>/전용 이벤트 메시지로 결과를 돌려준다.
/// 그래서 덱 셔플·카드 매칭 로직은 호스트에만 있으면 되고, 두 기기가
/// 각자 독립적으로 규칙을 계산하다 어긋나는(desync) 상황 자체가 생기지
/// 않는다.
/// </summary>
[Serializable]
public class GoStopNetMessage
{
    /// <summary>메시지 종류. 문자열이 아니라 enum인 이유 — JsonUtility가
    /// enum을 정수로 직렬화해도 상관없고(양쪽 다 같은 enum 정의 공유),
    /// 오탈자로 인한 매칭 실패를 컴파일 타임에 잡을 수 있다.</summary>
    public enum Type
    {
        /// <summary>접속 직후 최초 핸드셰이크 — 닉네임 교환용.</summary>
        Hello,
        /// <summary>호스트 → 클라이언트. 판이 새로 시작됐다 — 이 좌석의
        /// 손패(<see cref="cardIds"/>)·필드·더미 장수 등 초기 상태를 담는다.</summary>
        DealState,
        /// <summary>클라이언트 → 호스트. 손패에서 이 카드를 낸다는 의도.</summary>
        PlayCard,
        /// <summary>클라이언트 → 호스트. 필드에 같은 달이 2장이라 골라야
        /// 할 때 고른 카드.</summary>
        FieldChoice,
        /// <summary>클라이언트 → 호스트. 흔들기 선언 여부(<see cref="boolValue"/>).</summary>
        ShakeDecision,
        /// <summary>클라이언트 → 호스트. 9월 열끗을 피로 쓸지(<see cref="boolValue"/>=true) 여부.</summary>
        DualPiChoice,
        /// <summary>클라이언트 → 호스트. 고(<see cref="boolValue"/>=true)/스톱(false).</summary>
        GoStopDecision,
        /// <summary>클라이언트 → 호스트. 폭탄 크레딧으로 "덱만 넘기기".</summary>
        BombSkip,
        /// <summary>클라이언트 → 호스트. 4인 모드 참가 선언(2·3번째 순번)
        /// — 이번 판에 참가할지(<see cref="boolValue"/>).</summary>
        DeclareChoice,
        /// <summary>호스트 → 클라이언트. 매 턴 판정 후 전체 화면 상태를
        /// 다시 그릴 수 있는 스냅샷 — 필드·양쪽 획득패·양쪽 손패 장수·
        /// 더미 장수·현재 차례·머니 등. 클라이언트는 이 메시지 하나만
        /// 받아도 RebuildUI에 해당하는 걸 그대로 그릴 수 있어야 한다.</summary>
        StateSync,
        /// <summary>호스트 → 클라이언트. 뻑/쪽/싹쓸이/폭탄 등 "지금 뭐가
        /// 일어났는지" — 클라이언트의 토스트/이펙트/사운드 재생용.
        /// 판정 자체는 이미 StateSync에 반영돼 있고, 이 메시지는 순수
        /// 연출 트리거다.</summary>
        Event,
        /// <summary>양방향. 연결 종료(정상 나가기).</summary>
        Bye,
        /// <summary>호스트 → 전체. 대기실 인원이 바뀔 때마다(입장/퇴장)
        /// 보낸다 — 게스트 쪽 대기 화면이 "3/4명 접속됨" 같은 걸 그리는 데
        /// 쓴다. <see cref="playerNames"/>에 접속 순서대로(호스트가 0번)
        /// 닉네임이 들어간다.</summary>
        LobbyUpdate,
        /// <summary>호스트 → 각 게스트(좌석마다 다른 내용이라 브로드캐스트가
        /// 아니라 좌석별 개별 전송). "시작" 버튼을 누른 순간 발사 —
        /// <see cref="seat"/>가 이 메시지를 받는 사람의 배정 좌석,
        /// <see cref="playerCount"/>가 총 인원(2=맞고, 3/4=고스톱)이다.
        /// 클라이언트는 이 메시지 하나로 "내가 몇 번 자리인지"와 "어느
        /// 게임 모드인지"를 전부 알 수 있다.</summary>
        StartGame,
        /// <summary>호스트 → 그 좌석 하나(design.md §49.4/§50.2, 2026-08-24).
        /// 판 도중 다른 좌석이 재접속 유예를 넘겨 영구 이탈해서 좌석을
        /// 압축(다운그레이드)했을 때, 남은 각 게스트에게 "네 새 좌석 번호는
        /// 이거다"를 알린다. 씬 재로딩 없이 <see cref="seat"/>(새 좌석)·
        /// <see cref="playerCount"/>(새 인원수)만 받아 제자리에서 계속한다.</summary>
        SeatReassign,
        /// <summary>호스트 → 그 좌석 하나(2026-08-26, 선 뽑기 원격 클릭).
        /// 선 뽑기 8칸 중 그 좌석 차례가 됐다는 신호 — <see cref="text"/>에
        /// 이미 찜된 칸을 8자리 '0'/'1' 문자열로 담는다("01000010" 식).
        /// 카드 값은 절대 안 보낸다(블라인드 픽이 규칙이라 값을 미리
        /// 알려주면 원격 플레이어만 유리해진다) — 어느 칸이 비었는지만
        /// 알려줘서 그 칸들만 클릭 가능하게 그리게 한다.</summary>
        DealerDrawPrompt,
        /// <summary>클라이언트 → 호스트. 선 뽑기에서 고른 칸 번호(0~7) —
        /// <see cref="seat"/> 필드를 좌석 식별이 아니라 슬롯 인덱스 용도로
        /// 재사용한다(이 메시지의 발신자 좌석은 트랜스포트가 이미
        /// 별도로 알고 있어서 <c>WaitForRemoteMessage</c>의 <c>fromSeat</c>
        /// 매개변수로 구분되므로 헷갈리지 않는다).</summary>
        DealerDrawPick,
        /// <summary>양방향(2026-08-28, 채팅 기능). 게스트 → 호스트로 보낼 때는
        /// <see cref="text"/>가 입력한 원문 그대로다(호스트가 보낸 사람 이름을
        /// 붙여 완성한다). 호스트 → 게스트(브로드캐스트)로 보낼 때는
        /// <see cref="text"/>가 이미 "이름: 내용" 또는 "OO 뻑!" 같은 완성된
        /// 한 줄이라 게스트는 그대로 붙이기만 하면 된다 — 방향에 따라 같은
        /// 필드의 "완성도"가 다르다는 걸 <c>GoStop3PGame</c>의 처리부(호스트는
        /// <c>HandleIncomingGuestChat</c>, 게스트는 <c>LogLocalLine</c>)가
        /// 구분해서 처리한다. <see cref="boolValue"/>=true면 사람이 직접 친
        /// 채팅(탭 "채팅"), false면 게임 이벤트 로그(탭 "로그") — 2026-08-28
        /// 채팅창 탭 분리 기능. 게스트→호스트 원문 전송 시점엔 아직 카테고리가
        /// 안 실려 있어도 된다(항상 채팅이므로 호스트가 true로 재구성한다).</summary>
        ChatLog,
    }

    public Type type;

    /// <summary>PlayCard/FieldChoice에서 카드 식별자로 쓴다. 새 ID 체계를
    /// 만들지 않고 <see cref="HwatuCard.spriteName"/>을 그대로 쓴다 —
    /// 48장+조커 전부 유일하고 이미 양쪽 기기가 같은 표준 덱 구성
    /// (<c>GoStopDeck.BuildFull</c>)을 공유하므로 문자열 하나로 카드
    /// 한 장을 완전히 특정할 수 있다.</summary>
    public string cardId;

    /// <summary>DealState에서 이 좌석의 손패 전체를 보낼 때 등, 카드 여러
    /// 장이 필요한 경우. JsonUtility는 최상위 배열 직렬화가 안 돼서
    /// 이렇게 필드로 감싼다.</summary>
    public string[] cardIds;

    /// <summary>흔들기/9월열끗/고스톱 등 예/아니오 계열 결정.</summary>
    public bool boolValue;

    /// <summary>Hello에서 닉네임, Event에서 토스트에 쓸 라벨 문자열
    /// (예: "뻑", "쪽", "싹쓸이") 등 범용 문자열 슬롯.</summary>
    public string text;

    /// <summary>이 메시지를 보낸/받는 좌석(호스트=0). StartGame에선
    /// "받는 사람의 배정 좌석", Event에선 "누구에게 일어난 일인지".</summary>
    public int seat;

    /// <summary>LobbyUpdate에서 대기실 인원 전체의 닉네임(접속 순서 =
    /// 좌석 순서, [0]이 호스트). StartGame에서는 총 인원수만 필요하므로
    /// 대신 <see cref="playerCount"/>를 쓴다(배열 하나로 겸용하면 헷갈려서
    /// 분리했다).</summary>
    public string[] playerNames;

    /// <summary>StartGame — 총 참가 인원(호스트 포함). GoStop3PGame이
    /// 2(맞고)~4(고스톱) 전부 이 값 하나로 좌석 수를 맞춘다.</summary>
    public int playerCount;

    /// <summary>StartGame — 호스트가 방 생성 시 정한 1점 가격(원). 실제
    /// 정산은 호스트만 계산하므로(43번 규칙, 서버 권한) 게스트에게는
    /// 정보 표시용으로만 실어 보낸다(2026-08-23, design.md §49.2).</summary>
    public int pointPrice;

    /// <summary>Hello — 이 기기를 식별하는 영구 ID(<c>SystemInfo.
    /// deviceUniqueIdentifier</c>, 앱을 다시 켜도 같은 값). 재접속(design.md
    /// §50.2)을 판별하는 유일한 근거다 — 판 도중 연결이 끊긴 좌석과 같은
    /// clientId로 다시 접속하면 호스트가 새 참가자가 아니라 "그 좌석이
    /// 돌아왔다"로 인식해 같은 좌석을 그대로 돌려준다. 2026-08-24 추가.</summary>
    public string clientId;

    /// <summary>Hello — 게스트가 자기 기기에 저장해 둔 닉네임별 보유 머니
    /// (<see cref="GoStopNetLobby.LoadNetworkMoney"/>). 호스트는 이 값으로
    /// 그 좌석의 시작 잔액을 seed한다 — 서버가 없는 P2P 구조라 "이 닉네임의
    /// 돈"은 항상 그 사람 자신의 기기에만 존재하므로, 접속할 때마다 자기
    /// 값을 스스로 보고해야 한다(2026-09-05).</summary>
    public int money;

    public static GoStopNetMessage Hello(string name, string clientId, int money) => new GoStopNetMessage { type = Type.Hello, text = name, clientId = clientId, money = money };
    public static GoStopNetMessage Play(string cardId) => new GoStopNetMessage { type = Type.PlayCard, cardId = cardId };
    public static GoStopNetMessage Choice(string cardId) => new GoStopNetMessage { type = Type.FieldChoice, cardId = cardId };
    public static GoStopNetMessage Shake(bool shake) => new GoStopNetMessage { type = Type.ShakeDecision, boolValue = shake };
    public static GoStopNetMessage DualPi(bool useAsPi) => new GoStopNetMessage { type = Type.DualPiChoice, boolValue = useAsPi };
    public static GoStopNetMessage GoStop(bool go) => new GoStopNetMessage { type = Type.GoStopDecision, boolValue = go };
    public static GoStopNetMessage BombSkipMsg() => new GoStopNetMessage { type = Type.BombSkip };
    public static GoStopNetMessage Declare(bool wantsIn) => new GoStopNetMessage { type = Type.DeclareChoice, boolValue = wantsIn };
    // PlayCard에 흔들기 선언을 실어 보낼 때는 Play() 대신 이걸 쓴다 —
    // "카드+흔들기 여부"가 한 번의 결정이라 메시지 두 개로 쪼갤 이유가 없다.
    public static GoStopNetMessage PlayWithShake(string cardId, bool shake) => new GoStopNetMessage { type = Type.PlayCard, cardId = cardId, boolValue = shake };
    public static GoStopNetMessage EventMsg(string label, int seat) => new GoStopNetMessage { type = Type.Event, text = label, seat = seat };
    public static GoStopNetMessage ByeMsg() => new GoStopNetMessage { type = Type.Bye };
    public static GoStopNetMessage LobbyUpdateMsg(string[] names) => new GoStopNetMessage { type = Type.LobbyUpdate, playerNames = names };
    public static GoStopNetMessage StartGameMsg(int seat, int playerCount, int pointPrice) => new GoStopNetMessage { type = Type.StartGame, seat = seat, playerCount = playerCount, pointPrice = pointPrice };
    public static GoStopNetMessage SeatReassignMsg(int seat, int playerCount) => new GoStopNetMessage { type = Type.SeatReassign, seat = seat, playerCount = playerCount };
    public static GoStopNetMessage DealerDrawPrompt(bool[] taken) => new GoStopNetMessage { type = Type.DealerDrawPrompt, text = string.Concat(taken.Select(t => t ? '1' : '0')) };
    public static GoStopNetMessage DealerDrawPick(int slotIndex) => new GoStopNetMessage { type = Type.DealerDrawPick, seat = slotIndex };
    public static GoStopNetMessage ChatLogMsg(string text, bool isChat = false) => new GoStopNetMessage { type = Type.ChatLog, text = text, boolValue = isChat };
}
