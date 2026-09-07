using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 상대(컴퓨터) 플레이어의 간단한 휴리스틱. 완전탐색은 하지 않는다 —
/// 한 수 앞만 보고 "지금 이 카드를 내면 뭘 먹는가"만 비교한다.
///
/// 2026-09-07 — "등급(A/B/C) 하나로 뭉뚱그리지 말고 캐릭터별로 스킬을
/// 배분해달라" 요청으로 <see cref="GoStopTier"/> 매개변수를 전부
/// <see cref="GoStopSkillProfile"/>로 교체했다. GoStopTier 자체는 폐기하지
/// 않았다 — 시드머니만 그 기준으로 남는다(GoStopCharacters.StartingMoney).
/// 캐릭터별 실제 값 배분·근거는 GoStopCharacters.cs 문서 주석 참고.
/// </summary>
public static class GoStopAI
{
    // 2026-09-07 — 예전 "GoStopTier tier = GoStopTier.B" 기본값과 같은
    // 역할. 특정 캐릭터가 없는 범용 폴백 호출부(사람 턴 타임아웃 자동
    // 플레이, 오염된 네트워크 메시지 방어 등 — 이 경로들엔 애초에 "누구"가
    // 없다)를 위한 중립 프로필 — 모든 스킬 매개변수의 기본값으로 쓴다.
    public static readonly GoStopSkillProfile Neutral = GoStopSkillProfile.Base(GoStopTier.B);

    // 홍단/초단/청단/고도리 — GoStopRules의 공개 판정 함수를 그대로 재사용.
    // GoStop3PGame.EmergencySets와 같은 4종이지만, GoStopAI는 그 클래스에
    // 접근할 수 없는(정적 유틸이라 서로 참조 안 함) 별도 정적 클래스라
    // 여기 독립적으로 하나 더 둔다 — 내용은 항상 같은 4개 공개 함수라
    // 어긋날 일이 없다.
    static readonly System.Func<HwatuCard, bool>[] SetPreds =
    {
        GoStopRules.IsGodori, GoStopRules.IsHongdan, GoStopRules.IsChodan, GoStopRules.IsCheongdan,
    };

    /// <summary>손패 중 낼 카드를 고른다. 먹을 수 있는 수가 있으면 가장 값진 것을 우선한다.</summary>
    /// <param name="backingCard">밀어주기(고 콜러 견제) 대상 카드 — 못 먹는 턴에만 적용.</param>
    /// <param name="myCaptured">2026-09-07 신규 — 세트완성가중치(SetCompletionWeight)·
    /// 패흐름카운팅(CardCountingSkill) 계산에 쓴다. null이면 두 기능 다 비활성(안전한 기본값).</param>
    /// <param name="othersCaptured">패흐름카운팅용 — 다른 좌석들의 획득패를 합친 것(공개 정보).</param>
    public static HwatuCard ChooseCard(List<HwatuCard> hand, List<HwatuCard> field, GoStopSkillProfile skill = null,
        HwatuCard backingCard = null, List<HwatuCard> myCaptured = null, List<HwatuCard> othersCaptured = null)
    {
        skill ??= Neutral;
        // 2026-08-23: 조커는 필드 상태와 무관하게 항상 확정 이득(Cap 1장 +
        // 손패 리필)이라 스킬과 무관하게 무조건 먼저 낸다.
        var joker = hand.FirstOrDefault(c => c.isJoker);
        if (joker != null) return joker;

        // HandAccuracy — 매칭이 있어도 가끔 못 보고 지나칠 확률(1-HandAccuracy).
        if (hand.Count > 1 && Random.value > skill.HandAccuracy)
            return hand[Random.Range(0, hand.Count)];

        HwatuCard best = null;
        int bestValue = -1;

        foreach (var card in hand)
        {
            var matches = field.Count(f => f.month == card.month);
            if (matches == 0) continue; // 못 먹는 카드는 나중에 별도로 고른다

            int value = matches * 10;
            foreach (var f in field.Where(f => f.month == card.month))
                value += CardWeight(f);
            value += CardWeight(card);
            value += SetCompletionBonus(card, myCaptured, skill.SetCompletionWeight);

            if (value > bestValue) { bestValue = value; best = card; }
        }
        if (best != null) return best;

        // 밀어주기 — 못 먹는 턴에만 적용(먹을 수 있으면 위에서 이미 return됨).
        if (backingCard != null && hand.Contains(backingCard) && Random.value < skill.BackingChance)
            return backingCard;

        // 못 먹는다면 가장 안 아까운 카드를 낸다 — 패흐름카운팅(CardCountingSkill)이
        // 높으면 "이미 필드/캡처 더미에 많이 나온 달"의 카드를 우선 버린다
        // (상대가 나중에 그 카드로 이득 볼 확률이 낮으므로 더 안전하다).
        var ordered = hand.OrderBy(c => CardWeight(c) - CountingSafetyBonus(c, field, myCaptured, othersCaptured, skill.CardCountingSkill)).ToList();
        if (ordered.Count > 1 && Random.value > skill.DiscardPrecision)
            return ordered[Random.Range(0, Mathf.Min(3, ordered.Count))];
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

    /// <summary>2026-09-07 신규(세트완성가중치) — 이 카드가 내가 이미 2/3
    /// 모은 세트(홍단·초단·청단·고도리)를 완성시키면 큰 가산점을 준다.</summary>
    static int SetCompletionBonus(HwatuCard card, List<HwatuCard> myCaptured, float weight)
    {
        if (myCaptured == null || weight <= 0f) return 0;
        foreach (var pred in SetPreds)
        {
            if (!pred(card)) continue;
            if (myCaptured.Count(pred) == 2) return Mathf.RoundToInt(weight * 40f);
        }
        return 0;
    }

    /// <summary>2026-09-07 신규(패흐름카운팅) — 이 카드의 달이 이미 필드·
    /// 캡처 더미에 얼마나 많이 나와 있는지(공개 정보) 세서, 많이 나온
    /// 달일수록 "지금 버려도 상대적으로 안전하다"는 보너스를 준다.</summary>
    static float CountingSafetyBonus(HwatuCard card, List<HwatuCard> field, List<HwatuCard> myCaptured,
        List<HwatuCard> othersCaptured, float skill)
    {
        if (skill <= 0f) return 0f;
        int accountedFor = 0;
        if (field != null) accountedFor += field.Count(c => c.month == card.month);
        if (myCaptured != null) accountedFor += myCaptured.Count(c => c.month == card.month);
        if (othersCaptured != null) accountedFor += othersCaptured.Count(c => c.month == card.month);
        return accountedFor * skill * 2f;
    }

    /// <summary>
    /// 필드에 같은 달이 2장 있을 때(선택 캡처) 어느 걸 가져올지 고른다.
    /// FieldSetAwareness가 있으면(myCaptured 전달) 세트 완성 카드를 최우선
    /// 후보로 검토하고, 없으면 기존처럼 CardWeight 기준으로 더 값진 쪽을 택한다.
    /// </summary>
    public static HwatuCard ChooseFieldMatch(List<HwatuCard> candidates, GoStopSkillProfile skill = null,
        List<HwatuCard> myCaptured = null)
    {
        skill ??= Neutral;
        if (myCaptured != null && skill.FieldSetAwareness > 0f)
        {
            foreach (var pred in SetPreds)
            {
                if (myCaptured.Count(pred) != 2) continue;
                var completing = candidates.FirstOrDefault(pred);
                if (completing != null && Random.value < skill.FieldSetAwareness) return completing;
            }
        }

        var ordered = candidates.OrderByDescending(CardWeight).ToList();
        if (ordered.Count > 1 && Random.value > skill.FieldChoiceSkill)
            return ordered[1];
        return ordered[0];
    }

    /// <summary>
    /// 캡처 라인을 넘겼을 때 고/스톱을 결정한다. GoAggression이 높을수록
    /// 더 오래(더 높은 고 횟수까지) 밀어붙인다.
    /// </summary>
    /// <param name="isSoleGoCaller">2026-09-07 신규(독박회피) — 활성 좌석 중
    /// 고를 부른 사람이 나 하나뿐인지(=이번 판이 끝나면 내가 독박 대상).</param>
    /// <param name="stakeMultiplierNormalized">2026-09-07 신규(판돈배수인지) — 나가리
    /// 배증으로 커진 판돈을 0~1로 정규화한 값(1배=0, 8배 이상=1).</param>
    /// <param name="rivalsCloseCount">2026-09-07 신규(동맹관찰형 방어스톱) — 다른
    /// 활성 좌석 중 내 점수에 근접한(따라잡을 만한) 좌석 수.</param>
    /// <param name="lossStreak">2026-09-07 신규(연패심리) — 최근 연속 손실 판 수.</param>
    public static bool ShouldGo(int currentScore, int goCount, int cardsLeftInHand, GoStopSkillProfile skill = null,
        bool isSoleGoCaller = false, float stakeMultiplierNormalized = 0f, int rivalsCloseCount = 0, int lossStreak = 0)
    {
        skill ??= Neutral;
        int stopAtGoCount = Mathf.RoundToInt(Mathf.Lerp(3f, 6f, skill.GoAggression));
        // 연패 중인데 침착함(TiltResistance)이 낮으면 본전 생각에 평소보다 더 밀어붙인다.
        if (lossStreak >= 2 && Random.value > skill.TiltResistance) stopAtGoCount += 2;
        if (goCount >= stopAtGoCount) return false;

        int handStopThreshold = Mathf.RoundToInt(Mathf.Lerp(1f, 0f, skill.GoAggression));
        if (cardsLeftInHand <= handStopThreshold) return false;

        if (isSoleGoCaller && Random.value < skill.DokbakCaution * 0.6f) return false;
        if (stakeMultiplierNormalized > 0.3f && Random.value < skill.StakeRiskAwareness * stakeMultiplierNormalized) return false;
        if (rivalsCloseCount >= 2 && Random.value < skill.PressureDetection) return false;

        return true;
    }

    /// <summary>흔들기 선언 여부 — ShakeReliability 확률로 실제 선언한다.</summary>
    public static bool ShouldShake(GoStopSkillProfile skill = null) => Random.value < (skill ?? Neutral).ShakeReliability;

    /// <summary>
    /// 4인 광판다 — 2번째/3번째 선언 순서에서 "이번 판에 참가할지" 결정한다.
    /// 광이 있으면 항상 참가(공통 규칙).
    /// </summary>
    /// <param name="moneyRatio">시드머니 대비 현재 잔액 비율(0~1).</param>
    /// <param name="stakeMultiplierNormalized">2026-09-07 신규(판돈배수인지).</param>
    /// <param name="lossStreak">2026-09-07 신규(연패심리) — 연속 손실 중이면
    /// TiltResistance가 낮은 캐릭터는 오히려 더 적극적으로 참가한다(본전 생각).</param>
    public static bool WantsToPlay(List<HwatuCard> hand, GoStopSkillProfile skill = null, float moneyRatio = 1f,
        float stakeMultiplierNormalized = 0f, int lossStreak = 0)
    {
        skill ??= Neutral;
        int gwang = hand.Count(c => c.kind == HwatuKind.Gwang);
        if (gwang > 0) return true;

        float chance = skill.BaseParticipation;
        if (moneyRatio < 0.3f) chance *= 1f - skill.MoneyCaution;
        if (stakeMultiplierNormalized > 0.3f) chance *= 1f - skill.StakeRiskAwareness * stakeMultiplierNormalized * 0.5f;
        if (lossStreak >= 2 && Random.value > skill.TiltResistance) chance = Mathf.Min(1f, chance * 1.3f);
        return Random.value < chance;
    }

    /// <summary>
    /// 9월 열끗(국화 술잔)을 열끗/쌍피 중 지금 점수가 더 높아지는 쪽으로
    /// 맞춘다. DualPiSkill 확률로 정확히 고르고, 그 외엔 반대로(손해 보는
    /// 쪽으로) 고른다.
    /// </summary>
    public static void OptimizeDualPi(List<HwatuCard> captured, GoStopSkillProfile skill = null)
    {
        skill ??= Neutral;
        foreach (var c in captured.Where(c => c.dualPi))
        {
            c.useAsPi = false;
            int asYeol = GoStopRules.CalcScore(captured, 0).Total;
            c.useAsPi = true;
            int asPi = GoStopRules.CalcScore(captured, 0).Total;
            bool optimal = asPi > asYeol;
            if (Random.value > skill.DualPiSkill) optimal = !optimal;
            c.useAsPi = optimal;
        }
    }

    /// <summary>
    /// 2026-09-07 신규(폭탄크레딧 타이밍 전략화) — 손이 줄어든 후반(3장 이하)에,
    /// 스킬이 높은 캐릭터는 자발적으로 폭탄 크레딧(덱만 넘기기)을 써서 그
    /// 턴의 카드 소모를 아낀다. 기존엔 AI가 이 크레딧을 절대 자발적으로
    /// 안 썼다(문서화된 단순화) — BombCreditStrategy가 0에 가까운 캐릭터는
    /// 여전히 거의 안 쓴다.
    /// </summary>
    public static bool ShouldUseBombCredit(int handCount, int bombCreditsLeft, GoStopSkillProfile skill)
    {
        if (bombCreditsLeft <= 0 || handCount == 0 || handCount > 3) return false;
        return Random.value < skill.BombCreditStrategy * 0.5f;
    }
}
