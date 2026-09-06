using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 상대(컴퓨터) 플레이어의 간단한 휴리스틱. 완전탐색은 하지 않는다 —
/// 한 수 앞만 보고 "지금 이 카드를 내면 뭘 먹는가"만 비교한다.
///
/// 2026-09-06 — "타짜 캐릭터 난이도 적용"(A=잘함, B=보통, C=호구) 요청으로
/// 모든 결정 함수에 <see cref="GoStopTier"/> 매개변수를 추가했다. 기본값은
/// 전부 B(보통) — 이미 있던 로직 그대로라, tier를 안 넘기는 기존 호출부
/// (예: 플레이어 자신의 턴-타임아웃 자동 플레이처럼 "AI 상대"가 아닌 곳)는
/// 전혀 안 건드려도 예전과 100% 동일하게 동작한다. B가 "기준"이고 A는 그보다
/// 낫게, C는 그보다 못하게 갈린다.
/// </summary>
public static class GoStopAI
{
    /// <summary>손패 중 낼 카드를 고른다. 먹을 수 있는 수가 있으면 가장 값진 것을 우선한다.</summary>
    public static HwatuCard ChooseCard(List<HwatuCard> hand, List<HwatuCard> field, GoStopTier tier = GoStopTier.B)
    {
        // 2026-08-23: "조커도 손패로 나와야 한다" 요청으로 손패에 조커가
        // 실제로 있을 수 있게 됐다 — 조커는 필드 상태와 무관하게 항상
        // Cap 1장 + 손패 리필이라는 확정 이득이라(다른 카드처럼 "지금
        // 내면 손해"인 경우가 없다), 아낄 이유 없이 무조건 먼저 낸다.
        // 조커를 낼지 말지는 난이도와 무관한 항상 옳은 선택이라 tier로
        // 안 가른다.
        var joker = hand.FirstOrDefault(c => c.isJoker);
        if (joker != null) return joker;

        // C(호구)는 가끔(40%) 패턴을 못 읽고 아무 손패나 낸다 — 매칭이
        // 있어도 그냥 지나칠 수 있다는 뜻이라 가장 눈에 띄는 약점이다.
        if (tier == GoStopTier.C && hand.Count > 1 && UnityEngine.Random.value < 0.4f)
            return hand[UnityEngine.Random.Range(0, hand.Count)];

        HwatuCard best = null;
        int bestValue = -1;

        foreach (var card in hand)
        {
            var matches = field.Count(f => f.month == card.month);
            if (matches == 0) continue; // 못 먹는 카드는 나중에 별도로 고른다

            // 먹는 장수(따닥·싹쓸이일수록 큼) + 먹는 패의 가치를 대략 점수화한다.
            int value = matches * 10;
            foreach (var f in field.Where(f => f.month == card.month))
                value += CardWeight(f);
            value += CardWeight(card);

            if (value > bestValue) { bestValue = value; best = card; }
        }
        if (best != null) return best;

        // 못 먹는다면 가장 안 아까운(낮은 가치) 카드를 내서 손해를 최소화한다.
        // C는 가끔(30%) 하위 3장 중 아무거나 내서 실수로 좀 더 아까운 걸
        // 버리기도 한다 — A/B는 항상 최적(가장 안 아까운 것)을 낸다.
        var ordered = hand.OrderBy(CardWeight).ToList();
        if (tier == GoStopTier.C && ordered.Count > 1 && UnityEngine.Random.value < 0.3f)
            return ordered[UnityEngine.Random.Range(0, System.Math.Min(3, ordered.Count))];
        return ordered.First();
    }

    static int CardWeight(HwatuCard c) => c.kind switch
    {
        HwatuKind.Gwang     => 8,
        HwatuKind.Yeolkkeut => c.godori ? 6 : 4,
        HwatuKind.Ddi       => c.ddi != DdiColor.None ? 4 : 2,
        HwatuKind.Pi        => c.piValue,
        _ => 0,
    };

    /// <summary>
    /// 필드에 같은 달이 2장 있을 때(선택 캡처) 어느 걸 가져올지 고른다.
    /// A/B는 CardWeight 기준으로 더 값진 쪽을 택한다(플레이어가 직접 고르는
    /// 것과 같은 판단 기준). C는 가끔(40%) 덜 좋은 쪽을 고른다.
    /// </summary>
    public static HwatuCard ChooseFieldMatch(List<HwatuCard> candidates, GoStopTier tier = GoStopTier.B)
    {
        var ordered = candidates.OrderByDescending(CardWeight).ToList();
        if (tier == GoStopTier.C && ordered.Count > 1 && UnityEngine.Random.value < 0.4f)
            return ordered[1];
        return ordered[0];
    }

    /// <summary>
    /// 캡처 라인을 넘겼을 때 고/스톱을 결정한다. B(보통)는 기존 그대로(3고까지,
    /// 손패 1장 이하면 멈춤). A(잘함)는 4고까지 더 적극적으로 밀어붙이고
    /// 손이 완전히 없을 때만 멈춘다. C(호구)는 손패 상황과 무관하게 5고까지
    /// 무리하게 욕심낸다 — "언제 멈춰야 할지 모르는" 약체 특성.
    /// </summary>
    public static bool ShouldGo(int currentScore, int goCount, int cardsLeftInHand, GoStopTier tier = GoStopTier.B)
    {
        if (tier == GoStopTier.A)
        {
            if (goCount >= 4) return false;
            if (cardsLeftInHand <= 0) return false;
            return true;
        }
        if (tier == GoStopTier.C) return goCount < 5;

        if (goCount >= 3) return false;
        if (cardsLeftInHand <= 1) return false;
        return true;
    }

    /// <summary>
    /// 흔들기 선언 여부. A/B는 항상 선언한다(배수 이득이 확정적이라 정보
    /// 은닉의 이득을 계산 못 하는 한 수 앞 봇에겐 이게 합리적). C는 가끔(30%)
    /// 깜빡하고 안 하기도 한다 — 확정 이득조차 놓치는 게 "호구"다운 실수.
    /// </summary>
    public static bool ShouldShake(GoStopTier tier = GoStopTier.B) =>
        tier != GoStopTier.C || UnityEngine.Random.value < 0.7f;

    /// <summary>
    /// 4인 광판다 — 2번째/3번째 선언 순서에서 "이번 판에 참가할지" 결정한다.
    /// 광이 있으면 항상 참가(모든 티어 공통 — 큰 판을 노릴 수 있으니 당연히
    /// 낀다). 광이 없을 때: A(잘함)는 신중해서 낮은 확률로만 참가하고, B는
    /// 기존 기준, C(호구)는 손패가 안 좋아도 거의 항상 낀다 — 위험을
    /// 못 가리는 게 호구의 정의다.
    /// </summary>
    public static bool WantsToPlay(List<HwatuCard> hand, GoStopTier tier = GoStopTier.B)
    {
        int gwang = hand.Count(c => c.kind == HwatuKind.Gwang);
        if (gwang > 0) return true;
        float chance = tier switch { GoStopTier.A => 0.35f, GoStopTier.C => 0.9f, _ => 0.6f };
        return UnityEngine.Random.value < chance;
    }

    /// <summary>
    /// 9월 열끗(국화 술잔, <see cref="HwatuCard.dualPi"/>)을 열끗/쌍피 중 지금
    /// 점수가 더 높아지는 쪽으로 즉시 맞춘다. A/B는 항상 최적으로 고른다.
    /// C는 가끔(30%) 반대로(손해 보는 쪽으로) 고른다 — 단순 계산조차
    /// 가끔 실수하는 약체 특성.
    /// </summary>
    public static void OptimizeDualPi(List<HwatuCard> captured, GoStopTier tier = GoStopTier.B)
    {
        foreach (var c in captured.Where(c => c.dualPi))
        {
            c.useAsPi = false;
            int asYeol = GoStopRules.CalcScore(captured, 0).Total;
            c.useAsPi = true;
            int asPi = GoStopRules.CalcScore(captured, 0).Total;
            bool optimal = asPi > asYeol;
            if (tier == GoStopTier.C && UnityEngine.Random.value < 0.3f) optimal = !optimal;
            c.useAsPi = optimal;
        }
    }
}
