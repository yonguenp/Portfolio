using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 랭킹 기록 한 줄.
///
/// 점수만 남기면 나중에 "이 점수를 어떻게 냈나"를 복원할 수 없다. 턴·최대 콤보를
/// 같이 저장해 두면 같은 점수라도 오래 버틴 판인지 콤보로 터뜨린 판인지 구분된다.
/// </summary>
[Serializable]
public class BrickBreakerRecord
{
    public int    score;
    public int    turn;
    public int    combo;
    public long   ticks;   // DateTime.UtcNow.Ticks — 표시용
    public string name;    // 온라인 보드에서만 채워진다. 로컬은 빈 문자열.
    public bool   isMe;    // 온라인 목록에서 내 줄을 찾기 위한 표시

    public DateTime When => new DateTime(ticks, DateTimeKind.Utc).ToLocalTime();
}

/// <summary>
/// 랭킹 저장소. <b>온라인으로 올릴 때 이 인터페이스만 갈아끼운다.</b>
///
/// 지금은 백엔드가 없어서 <see cref="LocalRankingStore"/>(PlayerPrefs) 하나뿐이지만,
/// 매니저는 <see cref="BrickBreakerRanking.Store"/> 만 보므로 서버 구현을 넣어도
/// 게임 코드는 손댈 곳이 없다. 그래서 Submit이 순위를 **바로** 돌려주지 않고
/// 콜백을 받는다 — 네트워크 구현은 즉시 답할 수 없기 때문이다.
/// </summary>
public interface IRankingStore
{
    /// <summary>기록을 제출하고 1-based 순위를 콜백으로 준다. 순위 밖이면 0.</summary>
    void Submit(BrickBreakerMode mode, BrickBreakerRecord rec, Action<int> onRanked);

    /// <summary>상위 기록을 높은 점수 순으로 준다.</summary>
    void Load(BrickBreakerMode mode, Action<List<BrickBreakerRecord>> onLoaded);
}

/// <summary>PlayerPrefs 기반 로컬 보드. 모드별로 완전히 분리된다.</summary>
public class LocalRankingStore : IRankingStore
{
    public const int Capacity = 10;

    [Serializable] class Box { public List<BrickBreakerRecord> items = new(); }

    // 최고점수 키(BestBrickBreaker*)와 별개다. 저건 단일 값이고 이건 목록이라
    // 같은 키를 쓰면 서로 덮어쓴다.
    static string Key(BrickBreakerMode m) =>
        m == BrickBreakerMode.Item ? "BBRankItem" : "BBRankNormal";

    static List<BrickBreakerRecord> Read(BrickBreakerMode m)
    {
        string json = PlayerPrefs.GetString(Key(m), null);
        if (string.IsNullOrEmpty(json)) return new List<BrickBreakerRecord>();

        // 저장 형식이 깨져도 게임이 죽으면 안 된다 — 빈 보드로 시작한다.
        try
        {
            var box = JsonUtility.FromJson<Box>(json);
            return box?.items ?? new List<BrickBreakerRecord>();
        }
        catch { return new List<BrickBreakerRecord>(); }
    }

    static void Write(BrickBreakerMode m, List<BrickBreakerRecord> list)
    {
        PlayerPrefs.SetString(Key(m), JsonUtility.ToJson(new Box { items = list }));
        PlayerPrefs.Save();
    }

    public void Submit(BrickBreakerMode mode, BrickBreakerRecord rec, Action<int> onRanked)
    {
        var list = Read(mode);
        list.Add(rec);

        // 점수 내림차순. 동점이면 먼저 세운 기록이 위 — 나중에 같은 점수를 내도
        // 남의(이전의) 자리를 밀어내지 않는다.
        list.Sort((a, b) => b.score != a.score ? b.score.CompareTo(a.score)
                                               : a.ticks.CompareTo(b.ticks));
        if (list.Count > Capacity) list.RemoveRange(Capacity, list.Count - Capacity);

        Write(mode, list);

        int rank = list.IndexOf(rec) + 1;   // 잘려나갔으면 IndexOf가 -1 → 0
        onRanked?.Invoke(Mathf.Max(rank, 0));
    }

    public void Load(BrickBreakerMode mode, Action<List<BrickBreakerRecord>> onLoaded)
        => onLoaded?.Invoke(Read(mode));
}

/// <summary>게임 코드가 보는 단일 진입점.</summary>
public static class BrickBreakerRanking
{
    public static IRankingStore Store { get; set; } = new LocalRankingStore();

    public static void Submit(int score, int turn, int combo, Action<int> onRanked)
    {
        var rec = new BrickBreakerRecord
        {
            score = score,
            turn  = turn,
            combo = combo,
            ticks = DateTime.UtcNow.Ticks,
        };
        Store.Submit(BrickBreakerRules.Mode, rec, onRanked);
    }

    public static void Load(Action<List<BrickBreakerRecord>> onLoaded)
        => Store.Load(BrickBreakerRules.Mode, onLoaded);
}
