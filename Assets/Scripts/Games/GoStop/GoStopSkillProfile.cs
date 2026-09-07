/// <summary>
/// 2026-09-07(사용자 요청 — "웹에서 타짜 영화 찾아보고 각 캐릭터에 맞게 스킬능력
/// 배분") — <see cref="GoStopTier"/> 하나로 뭉뚱그려 A/B/C 세 단계만 갈리던
/// AI 행동을 캐릭터별로 세분화한다. 17개 축(0~1, 낮을수록 서투름/소극적,
/// 높을수록 능숙함/대담함 — 단, <see cref="StakeRiskAwareness"/>·
/// <see cref="DokbakCaution"/>처럼 "위험 인지" 계열은 높을수록 더 몸을
/// 사린다) 각각을 <see cref="GoStopAI"/>의 개별 판단 지점이 하나씩 읽는다.
/// <br/>
/// <see cref="GoStopTier"/>는 폐기하지 않았다 — 시드머니(<see cref="GoStopCharacters.StartingMoney"/>)는
/// 여전히 티어 기준이라 그 용도로만 남긴다. 실제 플레이 행동은 전부 이
/// 프로필을 읽는다.
/// </summary>
public class GoStopSkillProfile
{
    // ── 정확도 계열(실수 확률 = 1-값) — 예전 tier==C 전용 랜덤 실수 분기들을
    // 일반화한 것. 기본값(캐릭터가 특정 값을 안 넘기면)은 티어 그대로.
    public float HandAccuracy;      // ChooseCard: 먹을 수 있는 걸 못 보고 지나칠 확률의 반대
    public float DiscardPrecision;  // ChooseCard: 못 먹을 때 최적(가장 안 아까운) 카드를 버릴 확률
    public float ShakeReliability;  // 흔들기 조건이면 실제로 선언할 확률
    public float FieldChoiceSkill;  // ChooseFieldMatch: 더 값진 쪽을 고를 확률
    public float DualPiSkill;       // OptimizeDualPi: 손해 안 보는 쪽으로 정확히 고를 확률

    // ── 성향 계열(기존) ──
    public float GoAggression;      // ShouldGo: 높을수록 고를 더 오래/무리하게 부름
    public float BaseParticipation; // WantsToPlay: 광 없을 때 기본 참가 확률
    public float MoneyCaution;      // WantsToPlay: 잔액이 적을 때(moneyRatio<0.3) 추가로 자제하는 정도
    public float BackingChance;     // 밀어주기 실행 확률

    // ── 2026-09-07 신규 8종 ──
    public float DokbakCaution;      // ShouldGo: 내가 유일한 고 콜러+손 적음(독박 위험)일 때 더 일찍 멈추는 정도
    public float BombCreditStrategy; // 손이 줄어든 후반에 폭탄 크레딧을 자발적으로 아꼈다 쓰는 정도(0=예전처럼 절대 자발적으로 안 씀)
    public float CardCountingSkill;  // ChooseCard: discard 선택 시 "이미 많이 나온 달"을 우선 버리는 정도(패 흐름 카운팅)
    public float SetCompletionWeight;// ChooseCard: 자기 진행 중 세트(홍단 등 2/3)를 완성시키는 카드에 주는 가중치
    public float FieldSetAwareness;  // ChooseFieldMatch: 필드 2장 후보 중 세트 완성 카드를 우선하는 가중치
    public float AllyTargetingSkill; // 밀어주기 대상이 여럿일 때 "고 콜러를 가장 잘 이길 동맹"을 우선 선택하는 정확도
    public float StakeRiskAwareness; // 판돈 배수(나가리 배증)가 커졌을 때 위험 인지 — 높을수록 더 보수적으로 참가/고 판단
    public float TiltResistance;     // 연속 손실 후에도 침착함 유지 — 낮으면 본전 생각에 더 무모해진다
    public float PressureDetection;  // 내가 고 콜러일 때 다른 활성 좌석들이 나를 따라잡고 있다는 신호를 감지해 더 일찍 스톱

    static float TierDefault(GoStopTier tier, float a, float b, float c) => tier switch
    {
        GoStopTier.A => a,
        GoStopTier.B => b,
        GoStopTier.C => c,
        _ => b,
    };

    /// <summary>
    /// 티어 기준값에서 시작해, 명시적으로 넘긴 값만 덮어쓴다(나머지는 티어
    /// 기본값 그대로) — 캐릭터 하나당 "이 캐릭터가 유독 잘하는/못하는" 2~4개
    /// 축만 지정하면 되므로 13명×17축을 전부 손으로 채우지 않아도 된다.
    /// </summary>
    public static GoStopSkillProfile Base(GoStopTier tier,
        float? handAccuracy = null, float? discardPrecision = null, float? shakeReliability = null,
        float? fieldChoiceSkill = null, float? dualPiSkill = null,
        float? goAggression = null, float? baseParticipation = null, float? moneyCaution = null,
        float? backingChance = null,
        float? dokbakCaution = null, float? bombCreditStrategy = null, float? cardCountingSkill = null,
        float? setCompletionWeight = null, float? fieldSetAwareness = null, float? allyTargetingSkill = null,
        float? stakeRiskAwareness = null, float? tiltResistance = null, float? pressureDetection = null)
    {
        return new GoStopSkillProfile
        {
            HandAccuracy = handAccuracy ?? TierDefault(tier, 1f, 1f, 0.6f),
            DiscardPrecision = discardPrecision ?? TierDefault(tier, 1f, 1f, 0.7f),
            ShakeReliability = shakeReliability ?? TierDefault(tier, 1f, 1f, 0.7f),
            FieldChoiceSkill = fieldChoiceSkill ?? TierDefault(tier, 1f, 1f, 0.6f),
            DualPiSkill = dualPiSkill ?? TierDefault(tier, 1f, 1f, 0.7f),
            GoAggression = goAggression ?? TierDefault(tier, 0.7f, 0.5f, 0.9f),
            BaseParticipation = baseParticipation ?? TierDefault(tier, 0.35f, 0.6f, 0.9f),
            MoneyCaution = moneyCaution ?? TierDefault(tier, 0.5f, 0.25f, 0.05f),
            BackingChance = backingChance ?? TierDefault(tier, 1f, 0.7f, 0.2f),
            DokbakCaution = dokbakCaution ?? TierDefault(tier, 0.6f, 0.35f, 0.15f),
            BombCreditStrategy = bombCreditStrategy ?? TierDefault(tier, 0.6f, 0.35f, 0.15f),
            CardCountingSkill = cardCountingSkill ?? TierDefault(tier, 0.6f, 0.35f, 0.15f),
            SetCompletionWeight = setCompletionWeight ?? TierDefault(tier, 0.6f, 0.35f, 0.15f),
            FieldSetAwareness = fieldSetAwareness ?? TierDefault(tier, 0.6f, 0.35f, 0.15f),
            AllyTargetingSkill = allyTargetingSkill ?? TierDefault(tier, 0.6f, 0.35f, 0.15f),
            StakeRiskAwareness = stakeRiskAwareness ?? TierDefault(tier, 0.6f, 0.35f, 0.15f),
            TiltResistance = tiltResistance ?? TierDefault(tier, 0.6f, 0.35f, 0.15f),
            PressureDetection = pressureDetection ?? TierDefault(tier, 0.6f, 0.35f, 0.15f),
        };
    }
}
