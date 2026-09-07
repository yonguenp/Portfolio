using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 오프라인(vs AI) 고스톱의 CPU 등장인물 — 영화 "타짜" 등장인물을 레퍼런싱한
/// 이름 + 난이도 티어(2026-09-06, 사용자 확인). 네트워크 대전에는 적용되지
/// 않는다(그쪽은 실제 접속자 닉네임을 그대로 쓴다 — GoStop3PGame.SeatNameFor
/// 참고).
///
/// 이름별로 돈이 영구히 이어진다는 게 핵심 — 매 게임 시작마다 좌석 순서가
/// 랜덤으로 바뀌어도(예: 이번 판엔 김고니이 왼쪽, 다음 판엔 오른쪽) 같은
/// 이름은 같은 지갑을 들고 다닌다. 그래서 저장 키를 좌석 번호가 아니라
/// 캐릭터 "이름"으로 잡는다(GoStop3PGame의 기존 좌석 인덱스 기반
/// MoneyKey(int)와 별개 체계).
/// </summary>
public enum GoStopTier { A, B, C }

public readonly struct GoStopCharacter
{
    public readonly string name;
    public readonly GoStopTier tier;
    public readonly GoStopSkillProfile skills;
    public GoStopCharacter(string name, GoStopTier tier, GoStopSkillProfile skills)
    {
        this.name = name; this.tier = tier; this.skills = skills;
    }
}

public static class GoStopCharacters
{
    // 사용자가 직접 확정한 이름·티어 목록. A=잘함, B=보통, C=호구.
    //
    // 2026-09-07 — 스킬 프로필(GoStopSkillProfile) 추가. 각 캐릭터가 영화
    // "타짜" 시리즈(2006/신의 손 2014/원 아이드 잭 2019)와 원작 만화·드라마의
    // 실제 캐릭터성에서 근거를 찾아 "이 캐릭터가 유독 잘하는/못하는" 축만
    // 골라 덮어썼다(나머지는 GoStopSkillProfile.Base의 티어 기본값). 근거:
    //
    //   고니(조승우) — 평경장의 제자, 배짱+승부욕+깊은 의리로 그려진다.
    //     hand 정확도·밀어주기(의리)·세트완성 가중치를 최상급으로.
    //   평경장(백윤식) — "도박판의 전설" 타짜, 냉철한 통찰력의 노장.
    //     패흐름카운팅·폭탄크레딧전략·판돈위험인지·독박회피·연패저항 전부 최상급,
    //     대신 무리한 고는 안 부른다(goAggression 낮게).
    //   정마담(김혜수) — 화려하고 계산적인 심리전의 대가.
    //     동맹타겟팅(누굴 밀어줄지)·압박감지(견제당하는 걸 눈치챔)·쌍피
    //     최적화가 특기.
    //   고광렬(유해진) — 우스꽝스럽지만 정 많은 서포터 캐릭터.
    //     밀어주기(정)는 있지만 실행력(정확도)은 전반적으로 서투르다.
    //   아귀(김윤석) — 잔혹·탐욕적인 최상위 포식자, 수단방법 안 가림.
    //     go 공격성 최고, 독박도 안 두려워하고 판돈이 커져도 안 사림
    //     (StakeRiskAwareness 낮게=몰빵형), 연패에도 안 흔들림. 대신 협력
    //     (밀어주기)은 거의 안 한다 — 자기밖에 모른다.
    //   곽철용 — "묻고 더블로가"로 유명한 허세·자신감 과잉 조직 두목.
    //     go 공격성·기본 참가 최고, 대신 판돈 감각·연패 저항은 최악(계속
    //     본전 생각에 몰빵).
    //   화란(신의 손) — 능글맞고 대담한 동업자 타짜.
    //     동맹타겟팅·밀어주기는 있지만 잔액 걱정은 거의 안 한다(대담함).
    //   짝귀 — 경상도 지역 최고수로 불리는 우직한 실력파 타짜.
    //     패흐름카운팅·필드선택 정확도·hand 정확도가 특기. 정치질(동맹
    //     타겟팅)은 서투르다 — 우직한 성격.
    //   호구 — 이름 자체가 "잘 속는 사람"이라는 뜻의 도박 은어.
    //     정확도 계열 전반 최저, 잔액 걱정 없이 계속 들어가고 무모하게
    //     고를 부른다 — 이름 그대로.
    //   무석(박무석) — 고니를 처음 등쳐먹은 하수인/사기꾼, 기회주의적.
    //     눈치 빠르게 발 빼는 감각(잔액 자제·필드선택)은 있지만 의리
    //     (밀어주기)·독박 각오는 없다 — 자기 살길만 챙긴다.
    //   세란(신의 손, 고광렬의 반려) — 순박하게 정착한 인물.
    //     밀어주기(정)는 있지만 공격적인 승부(go)는 낮다.
    //   너구리(평경장의 죽음을 조사하는 탐정) — 관찰력·추리가 본업.
    //     티어는 C지만 패흐름카운팅·압박감지만은 예외적으로 날카롭다.
    //   점박이 교수 — 지적·분석형 도박꾼 아키타입.
    //     세트완성 가중치·쌍피 최적화 등 "이론"에 강하지만, 독박 각오·
    //     연패 저항 같은 실전 배짱은 약하다.
    public static readonly GoStopCharacter[] All =
    {
        new("고니", GoStopTier.A, GoStopSkillProfile.Base(GoStopTier.A,
            handAccuracy: 1f, goAggression: 0.75f, backingChance: 0.95f, setCompletionWeight: 0.85f,
            moneyCaution: 0.2f)),
        new("정마담", GoStopTier.B, GoStopSkillProfile.Base(GoStopTier.B,
            allyTargetingSkill: 0.95f, pressureDetection: 0.85f, dualPiSkill: 0.95f, fieldSetAwareness: 0.75f)),
        new("평경장", GoStopTier.A, GoStopSkillProfile.Base(GoStopTier.A,
            cardCountingSkill: 0.95f, bombCreditStrategy: 0.9f, stakeRiskAwareness: 0.9f,
            dokbakCaution: 0.85f, tiltResistance: 0.95f, goAggression: 0.35f)),
        new("고광렬", GoStopTier.B, GoStopSkillProfile.Base(GoStopTier.B,
            backingChance: 0.8f, handAccuracy: 0.55f, discardPrecision: 0.5f, goAggression: 0.4f)),
        new("아귀", GoStopTier.A, GoStopSkillProfile.Base(GoStopTier.A,
            goAggression: 0.95f, dokbakCaution: 0.1f, stakeRiskAwareness: 0.1f, tiltResistance: 0.9f,
            backingChance: 0.05f)),
        new("곽철용", GoStopTier.B, GoStopSkillProfile.Base(GoStopTier.B,
            goAggression: 0.9f, baseParticipation: 0.95f, moneyCaution: 0.05f, stakeRiskAwareness: 0.1f,
            tiltResistance: 0.2f)),
        new("화란", GoStopTier.C, GoStopSkillProfile.Base(GoStopTier.C,
            allyTargetingSkill: 0.7f, backingChance: 0.6f, moneyCaution: 0.15f, dokbakCaution: 0.2f)),
        new("짝귀", GoStopTier.A, GoStopSkillProfile.Base(GoStopTier.A,
            cardCountingSkill: 0.85f, fieldChoiceSkill: 0.9f, handAccuracy: 0.9f, allyTargetingSkill: 0.3f)),
        new("호구", GoStopTier.C, GoStopSkillProfile.Base(GoStopTier.C,
            handAccuracy: 0.35f, discardPrecision: 0.3f, shakeReliability: 0.4f, moneyCaution: 0.05f,
            goAggression: 0.8f, baseParticipation: 0.95f)),
        new("무석", GoStopTier.B, GoStopSkillProfile.Base(GoStopTier.B,
            moneyCaution: 0.7f, fieldChoiceSkill: 0.7f, backingChance: 0.1f, dokbakCaution: 0.1f)),
        new("세란", GoStopTier.C, GoStopSkillProfile.Base(GoStopTier.C,
            backingChance: 0.85f, goAggression: 0.3f, stakeRiskAwareness: 0.3f)),
        new("너구리", GoStopTier.C, GoStopSkillProfile.Base(GoStopTier.C,
            cardCountingSkill: 0.9f, pressureDetection: 0.7f, handAccuracy: 0.4f, goAggression: 0.3f)),
        new("점박이 교수", GoStopTier.C, GoStopSkillProfile.Base(GoStopTier.C,
            setCompletionWeight: 0.9f, fieldSetAwareness: 0.9f, dualPiSkill: 0.85f,
            dokbakCaution: 0.2f, tiltResistance: 0.3f, goAggression: 0.25f)),
    };

    /// <summary>티어별 최초 시드머니(사용자 확인) — A=100만, B=50만, C=10만.</summary>
    public static int StartingMoney(GoStopTier tier) => tier switch
    {
        GoStopTier.A => 1_000_000,
        GoStopTier.B => 500_000,
        GoStopTier.C => 100_000,
        _ => 100_000,
    };

    static string MoneyKey(string charName) => "GoStopChar_Money_" + charName;

    /// <summary>이 캐릭터가 이미 파산해서 은퇴했는지 — 별도 플래그 없이
    /// "저장된 돈이 있고 그게 0 이하"로 판단한다. 키 자체가 없으면(한
    /// 번도 등장한 적 없음) 은퇴가 아니라 "아직 시드 전"이다.</summary>
    public static bool IsRetired(string charName) =>
        PlayerPrefs.HasKey(MoneyKey(charName)) && PlayerPrefs.GetInt(MoneyKey(charName)) <= 0;

    /// <summary>저장된 돈이 있으면 그대로, 없으면(첫 등장) 티어 기준
    /// 시드머니로 시작한다.</summary>
    public static int LoadMoney(string charName, GoStopTier tier) =>
        PlayerPrefs.HasKey(MoneyKey(charName)) ? PlayerPrefs.GetInt(MoneyKey(charName)) : StartingMoney(tier);

    public static void SaveMoney(string charName, int amount) => PlayerPrefs.SetInt(MoneyKey(charName), amount);

    /// <summary>은퇴(0원 이하) 안 한 캐릭터 중에서 무작위로 최대 count명을
    /// 중복 없이 뽑는다. 은퇴자가 너무 많아 count를 못 채우면(13명 중
    /// 대부분이 파산 — 사실상 일어나기 힘들다) 예외 없이 진행되도록 은퇴자로
    /// 채운다 — "한 명도 못 뽑아서 게임이 아예 안 열리는" 것보다는 낫다.</summary>
    public static List<GoStopCharacter> DrawRandom(int count)
    {
        var alive = All.Where(c => !IsRetired(c.name)).ToList();
        Shuffle(alive);
        if (alive.Count >= count) return alive.Take(count).ToList();

        var retired = All.Where(c => IsRetired(c.name)).ToList();
        Shuffle(retired);
        alive.AddRange(retired.Take(count - alive.Count));
        return alive;
    }

    static void Shuffle(List<GoStopCharacter> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
