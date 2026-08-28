using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 호스트가 매 <c>RebuildUI</c>마다 만들어서 게스트에게 보내는 판 상태
/// 전체 — 게스트는 이걸 그대로 자기 필드에 덮어쓰고 자기 RebuildUI를
/// 부르는 것만으로 화면을 맞춘다(호스트가 직접 겪는 판정 로직을 게스트가
/// 다시 계산할 필요가 전혀 없다 — 호스트 권위 모델의 핵심).
///
/// <see cref="GoStopNetMessage.Type.StateSync"/> 메시지의 <c>text</c>
/// 필드에 이 클래스를 <c>JsonUtility</c>로 다시 한 번 직렬화해서 담는다
/// (JSON 안에 JSON) — <c>GoStopNetMessage</c>를 매번 새 필드로 늘리지
/// 않고 상태 스냅샷만 별도로 진화시킬 수 있게 분리했다.
///
/// 카드는 <see cref="HwatuCard"/> 객체를 통째로 안 보내고
/// <see cref="GoStopDeck.Encode"/>/<see cref="GoStopDeck.Decode"/>로
/// spriteName 문자열만 주고받는다. <c>JsonUtility</c>가 배열의 배열
/// (jagged array)을 못 다뤄서 좌석별 손패/획득패를 4개씩 이름 붙인
/// 필드로 펼쳐뒀다 — 배열 하나로 깔끔하게 감쌀 수 없는 건 아쉽지만
/// 이 프로젝트가 이미 어디서든 쓰는 방식(JsonUtility 유지, 새 직렬화
/// 패키지 안 씀)과 일관성을 지키는 쪽을 택했다.
///
/// <b>더미(drawPile)는 개수만 보낸다.</b> 게스트는 그 카드들의 실제
/// 정체를 볼 일이 없다(뒤집히기 전까지는 화면에도 안 나온다) — 카드
/// 자체를 안 보내면 대역폭도 아끼고, 호스트가 다음에 뭘 뒤집을지
/// 게스트가 미리 알 길도 없어진다(패킷을 몰래 들여다봐도 다음 패를
/// 못 안다는 뜻 — 이 프로젝트가 굳이 노리진 않았지만 자연스러운 부수효과).
/// </summary>
[Serializable]
public class GoStopStateSnapshot
{
    public int seats;
    public int currentSeat;
    public int sittingOutSeat;
    public bool sittingOutWasSqueezed;
    public int dealerSeat;
    public int state; // GoStop3PGame.State를 int로

    public string[] hand0, hand1, hand2, hand3;
    public string[] captured0, captured1, captured2, captured3;
    public string[] field;
    public int drawPileCount;

    public int[] money;
    public int[] goCount;
    public int[] sweeps;
    public int[] heundeulCount;
    public int[] bombCredits;
    public bool[] calledGo;

    // 아래 3개는 정규 브로드캐스트(전원에게)가 아니라 "지금 결정이
    // 필요한 그 좌석 한 명에게만" 보내는 타깃 StateSync에서만 채워진다
    // (GoStop3PGame.SendTargetedPrompt 참고) — 나머지 게스트가 받는
    // 정규 스냅샷에서는 전부 기본값(비어있음/false)이다. 고/스톱은 이런
    // 타깃 신호가 따로 필요 없다 — state/currentSeat 자체가 이미 정규
    // 스냅샷에 실려 있어서 "state==GoStopChoice && currentSeat==내좌석"
    // 조건만으로 판단할 수 있다.
    public string[] fieldChoiceCandidates;   // 같은 달 2장 중 고르기 — 후보 카드
    public bool dualPiChoicePending;         // 9월 열끗 열끗/피 선택
    public bool declarePending;              // 4인 모드 참가 선언
    public string declareDealerName;         // 참가 선언 팝업 문구용("OO이 선입니다")

    // 게임오버 — EndGame은 RebuildUI를 거치지 않아서(정규 브로드캐스트
    // 경로 밖) 호스트가 별도로 한 번 더 쏴준다(BroadcastGameOverState).
    // "누가 이겼다/졌다" 문구는 보는 사람마다 달라서("나" 판정이 좌석마다
    // 다르다) 완성된 텍스트를 안 보내고 원시 데이터만 보낸다 — 받는 쪽이
    // 자기 SeatName()으로 직접 조립한다.
    public bool gameOverActive;
    public bool gameOverIsNagari;
    public int gameOverWinnerSeat = -1;
    public int gameOverFinalScore;
    public int gameOverDokbakSeat = -1;      // 독박 낸 좌석, 없으면 -1
    public int gameOverStakeMultiplier = 1;  // 나가리 다음 판 판돈 배수
    public int[] gameOverRefilledSeats;      // 잔액 소진→리필된 좌석들(이름은 안 보냄)

    public static string[] Enc(IEnumerable<HwatuCard> cards) => GoStopDeck.EncodeAll(cards);
    public static List<HwatuCard> Dec(string[] arr) => GoStopDeck.DecodeAll(arr);

    public string[] HandFor(int seat) => seat switch { 0 => hand0, 1 => hand1, 2 => hand2, 3 => hand3, _ => null };
    public string[] CapturedFor(int seat) => seat switch { 0 => captured0, 1 => captured1, 2 => captured2, 3 => captured3, _ => null };
}
