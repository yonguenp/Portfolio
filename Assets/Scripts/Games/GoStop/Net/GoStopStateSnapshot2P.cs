using System;
using System.Collections.Generic;

/// <summary>
/// 2인 맞고(<see cref="GoStopGame"/>)의 네트워크 상태 스냅샷 —
/// <see cref="GoStopStateSnapshot"/>(3~4인용)와 같은 역할이지만, 2인판은
/// 좌석 배열이 아니라 <c>playerXxx</c>/<c>aiXxx</c>로 이름 붙은 개별
/// 필드로 짜여 있어(<see cref="GoStopGame"/> 문서 참고) 같은 클래스를
/// 못 쓴다 — 필드 이름을 호스트 내부 이름 그대로("player"=호스트,
/// "ai"=게스트) 맞춰서 새로 만들었다.
///
/// <b>게스트가 이 스냅샷을 적용할 땐 반드시 player↔ai를 뒤바꿔서
/// 받는다</b>(<see cref="GoStopGame.ApplyNetworkSnapshot"/>) — 호스트
/// 입장에서 "ai"는 실제로는 접속한 게스트가 조종하는 자리이므로, 그
/// 게스트 자신의 화면에서는 그 데이터가 화면 <b>아래쪽</b>(내 손패)에
/// 나와야 한다. 반대로 <c>state</c>(PlayerTurn/AiTurn)도 게스트 쪽에서는
/// 뒤집어 해석해야 한다 — 호스트의 "AiTurn"이 곧 게스트 자신의 차례다.
/// 딱 하나, <see cref="aiGoStopPending"/>만 예외로 뒤집지 않는다 —
/// 이미 "ai(=게스트) 쪽의 결정이 필요하다"는 뜻으로 게스트 관점에서
/// 작성돼 있어서다.
/// </summary>
[Serializable]
public class GoStopStateSnapshot2P
{
    public int state; // GoStopGame.State를 int로 (호스트 관점 — 게스트는 PlayerTurn↔AiTurn을 뒤집어 읽는다)

    public string[] playerHand, aiHand, field;
    public int drawPileCount;
    public string[] playerCaptured, aiCaptured;

    public int playerGoCount, aiGoCount;
    public int playerSweeps, aiSweeps;
    public int playerBombCredits, aiBombCredits;
    public int playerMoney, aiMoney;

    // 손패 아이콘(흔들기 가능 여부) 판정에 필요 — HashSet<int>를 배열로.
    public int[] playerShookMonths, aiShookMonths;

    // 타깃 프롬프트 — "ai"(게스트) 쪽에 결정이 필요한 순간에만 채워진다.
    // GoStopStateSnapshot(3~4인)과 같은 이유로 정규 브로드캐스트가 아니라
    // 게스트에게만 개별로 보내는 스냅샷에서 쓴다.
    public string[] fieldChoiceCandidates;
    public bool dualPiChoicePending;
    public bool aiGoStopPending; // 뒤집어 읽지 않는다 — 이미 "게스트 결정 필요"라는 뜻

    /// <summary>호스트 자신(player 역할)이 지금 고/스톱 오버레이를 보고
    /// 있는지 — <c>state==GoStopChoice</c>와 동치라 별도 필드 없이도
    /// 유도 가능하지만, "지금 이 스냅샷이 그 상태를 반영한다"는 걸
    /// 명시적으로 표시해서 게스트가 "상대가 고/스톱을 선택 중입니다"
    /// 안내를 띄울 수 있게 한다. 게스트 쪽엔 뒤집을 필요 없다 — 이미
    /// "상대(호스트)가 결정 중"이라는 뜻으로 작성돼 있다.</summary>
    public bool hostGoStopPending;

    // 게임오버 — RebuildUI를 안 거치는 EndGame 경로라 별도로 쏜다
    // (GoStopStateSnapshot의 gameOverActive 계열과 같은 이유).
    public bool gameOverActive;
    public bool gameOverIsNagari;
    public bool gameOverAiWon; // 나가리가 아닐 때만 의미 있음 — true=ai(게스트) 승
    public int gameOverFinalScore;
    public int gameOverStakeMultiplier;

    public static string[] Enc(IEnumerable<HwatuCard> cards) => GoStopDeck.EncodeAll(cards);
    public static List<HwatuCard> Dec(string[] arr) => GoStopDeck.DecodeAll(arr);
}
