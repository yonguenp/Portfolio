using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 상대(컴퓨터) 플레이어의 간단한 휴리스틱. 완전탐색은 하지 않는다 —
/// 한 수 앞만 보고 "지금 이 카드를 내면 뭘 먹는가"만 비교한다.
/// </summary>
public static class GoStopAI
{
    /// <summary>손패 중 낼 카드를 고른다. 먹을 수 있는 수가 있으면 가장 값진 것을 우선한다.</summary>
    public static HwatuCard ChooseCard(List<HwatuCard> hand, List<HwatuCard> field)
    {
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
        return hand.OrderBy(CardWeight).First();
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
    /// 같은 CardWeight 기준으로 더 값진 쪽을 택한다 — 플레이어가 직접
    /// 고르는 것과 같은 판단 기준을 쓴다.
    /// </summary>
    public static HwatuCard ChooseFieldMatch(List<HwatuCard> candidates) =>
        candidates.OrderByDescending(CardWeight).First();

    /// <summary>
    /// 7점을 넘겼을 때 고/스톱을 결정한다. 손패가 거의 떨어졌거나 이미 여러 번
    /// 고를 불렀으면 안전하게 멈춘다 — 최대 3고까지만 욕심낸다.
    /// </summary>
    public static bool ShouldGo(int currentScore, int goCount, int cardsLeftInHand)
    {
        if (goCount >= 3) return false;
        if (cardsLeftInHand <= 1) return false;
        return true;
    }

    /// <summary>
    /// 흔들기 선언 여부. 플레이어는 팝업으로 직접 고르지만 AI는 정보를 숨겨서
    /// 얻는 이득을 계산할 만큼 정교하지 않으므로(한 수 앞 휴리스틱) 항상
    /// 선언한다 — 배수 이득이 확정적이라 단순 봇에게는 이게 더 합리적이다.
    /// </summary>
    public static bool ShouldShake() => true;

    /// <summary>
    /// 4인 광판다 — 2번째/3번째 선언 순서에서 "이번 판에 참가할지" 결정한다.
    /// 참가를 포기해도(사용자 확인 규칙상) 아무 보상이 없으므로, 합리적인
    /// 기준은 "이길 확률이 낮아 보이면 아예 안 낀다"는 위험 회피다 — 손패에
    /// 광이 한 장도 없으면 큰 판(광 3장 이상 보너스)을 노릴 수 없어 상대적으로
    /// 승산이 낮다고 보고 다소 소극적으로 판단한다(60% 확률로만 참가).
    /// 광이 하나라도 있으면 항상 참가한다.
    /// </summary>
    public static bool WantsToPlay(List<HwatuCard> hand)
    {
        int gwang = hand.Count(c => c.kind == HwatuKind.Gwang);
        if (gwang > 0) return true;
        return UnityEngine.Random.value < 0.6f;
    }

    /// <summary>
    /// 9월 열끗(국화 술잔, <see cref="HwatuCard.dualPi"/>)을 열끗/쌍피 중 지금
    /// 점수가 더 높아지는 쪽으로 즉시 맞춘다. 매 캡처 직후 불러도 싸다 —
    /// 후보가 최대 한 장뿐이라 <see cref="GoStopRules.CalcScore"/> 두 번이면 끝난다.
    /// </summary>
    public static void OptimizeDualPi(List<HwatuCard> captured)
    {
        foreach (var c in captured.Where(c => c.dualPi))
        {
            c.useAsPi = false;
            int asYeol = GoStopRules.CalcScore(captured, 0).Total;
            c.useAsPi = true;
            int asPi = GoStopRules.CalcScore(captured, 0).Total;
            c.useAsPi = asPi > asYeol;
        }
    }
}
