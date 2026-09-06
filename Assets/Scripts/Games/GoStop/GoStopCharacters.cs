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
    public GoStopCharacter(string name, GoStopTier tier) { this.name = name; this.tier = tier; }
}

public static class GoStopCharacters
{
    // 사용자가 직접 확정한 이름·티어 목록. A=잘함, B=보통, C=호구.
    public static readonly GoStopCharacter[] All =
    {
        new("김고니",   GoStopTier.A),
        new("정마담", GoStopTier.B),
        new("평경장", GoStopTier.A),
        new("고광렬", GoStopTier.B),
        new("아귀",   GoStopTier.A),
        new("곽철용", GoStopTier.B),
        new("화란",   GoStopTier.C),
        new("짝귀",   GoStopTier.A),
        new("호구",   GoStopTier.C),
        new("무석",   GoStopTier.B),
        new("세란이", GoStopTier.C),
        new("너구리", GoStopTier.C),
        new("교수",   GoStopTier.C),
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
