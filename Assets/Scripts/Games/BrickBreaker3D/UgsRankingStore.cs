using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Leaderboards;
using UnityEngine;

/// <summary>
/// Unity Gaming Services 리더보드 저장소. 다른 사람 기록까지 보이는 전역 보드다.
///
/// <b>로컬 보드를 버리지 않는다.</b> 제출은 항상 로컬에 먼저 쓰고 그 다음 서버로
/// 보낸다 — 비행기 모드나 서비스 장애에도 자기 기록은 남아야 하고, "내 기록"
/// 탭이 네트워크를 기다리지 않고 즉시 뜨기 때문이다. 서버가 실패하면 조용히
/// 로컬로 폴백한다. 랭킹이 안 뜨는 것 때문에 게임이 멈추면 안 된다.
///
/// 대시보드(cloud.unity.com)에 <b>리더보드 두 개</b>가 있어야 한다 —
/// <c>bb_normal</c>, <c>bb_item</c>. 정렬 Descending, 정책 KeepBest.
/// 모드마다 규칙이 달라 한 보드에 섞으면 비교가 성립하지 않는다.
/// </summary>
public class UgsRankingStore : IRankingStore
{
    // 서버가 죽어도 자기 기록은 남는다 — 항상 같이 쓴다.
    readonly LocalRankingStore local = new();

    /// <summary>서버 연결에 성공한 적이 있는가. UI가 "오프라인" 표시를 정할 때 쓴다.</summary>
    public static bool Online { get; private set; }

    /// <summary>마지막 실패 사유. 대시보드 설정 누락을 화면에 알려주기 위한 것.</summary>
    public static string LastError { get; private set; }

    static string BoardId(BrickBreakerMode m) =>
        m == BrickBreakerMode.Item ? "bb_item" : "bb_normal";

    // 메타데이터. 점수만 올리면 턴·콤보를 잃어버려 로컬 보드보다 정보가 줄어든다.
    [Serializable] class Meta { public int turn; public int combo; }

    static Task initTask;

    /// <summary>초기화 + 익명 로그인. 여러 번 불러도 한 번만 돈다.</summary>
    static Task EnsureReady()
    {
        // Task를 캐시해야 동시에 들어온 호출이 로그인을 두 번 하지 않는다.
        if (initTask != null) return initTask;
        return initTask = InitAsync();
    }

    static async Task InitAsync()
    {
        if (UnityServices.State != ServicesInitializationState.Initialized)
            await UnityServices.InitializeAsync();

        if (!AuthenticationService.Instance.IsSignedIn)
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    // ── 제출 ─────────────────────────────────────────────
    public void Submit(BrickBreakerMode mode, BrickBreakerRecord rec, Action<int> onRanked)
    {
        // 로컬 순위를 먼저 돌려준다. 게임오버 화면이 네트워크를 기다리면 안 된다.
        local.Submit(mode, rec, onRanked);
        _ = SubmitOnline(mode, rec);
    }

    static async Task SubmitOnline(BrickBreakerMode mode, BrickBreakerRecord rec)
    {
        try
        {
            await EnsureReady();
            await LeaderboardsService.Instance.AddPlayerScoreAsync(
                BoardId(mode), rec.score,
                new AddPlayerScoreOptions
                {
                    Metadata = new Meta { turn = rec.turn, combo = rec.combo },
                });
            Online    = true;
            LastError = null;
        }
        catch (Exception e)
        {
            Online    = false;
            LastError = e.Message;
            Debug.LogWarning($"[랭킹] 온라인 제출 실패 — 로컬에만 남는다: {e.Message}");
        }
    }

    // ── 조회 ─────────────────────────────────────────────
    public void Load(BrickBreakerMode mode, Action<List<BrickBreakerRecord>> onLoaded)
        => _ = LoadOnline(mode, onLoaded);

    static async Task LoadOnline(BrickBreakerMode mode,
                                 Action<List<BrickBreakerRecord>> onLoaded)
    {
        try
        {
            await EnsureReady();
            var page = await LeaderboardsService.Instance.GetScoresAsync(
                BoardId(mode),
                new GetScoresOptions { Offset = 0, Limit = LocalRankingStore.Capacity, IncludeMetadata = true });

            string myId = AuthenticationService.Instance.PlayerId;
            var list = new List<BrickBreakerRecord>(page.Results.Count);

            foreach (var e in page.Results)
            {
                var meta = ParseMeta(e.Metadata);
                list.Add(new BrickBreakerRecord
                {
                    score = (int)e.Score,
                    turn  = meta.turn,
                    combo = meta.combo,
                    name  = string.IsNullOrEmpty(e.PlayerName) ? "익명" : e.PlayerName,
                    isMe  = e.PlayerId == myId,
                    // 서버는 제출 시각을 주지 않는다. 0이면 UI가 날짜를 숨긴다.
                    ticks = 0,
                });
            }

            Online    = true;
            LastError = null;
            onLoaded?.Invoke(list);
        }
        catch (Exception e)
        {
            Online    = false;
            LastError = e.Message;
            Debug.LogWarning($"[랭킹] 온라인 조회 실패 — 로컬 보드로 폴백: {e.Message}");
            new LocalRankingStore().Load(mode, onLoaded);
        }
    }

    static Meta ParseMeta(string json)
    {
        if (string.IsNullOrEmpty(json)) return new Meta();
        try { return JsonUtility.FromJson<Meta>(json) ?? new Meta(); }
        catch { return new Meta(); }
    }

    // ── 닉네임 ───────────────────────────────────────────
    /// <summary>지금 표시되는 이름. 로그인 전이면 빈 문자열.</summary>
    public static string PlayerName =>
        AuthenticationService.Instance != null && AuthenticationService.Instance.IsSignedIn
            ? AuthenticationService.Instance.PlayerName ?? ""
            : "";

    /// <summary>
    /// 닉네임을 바꾼다. UGS는 뒤에 <c>#1234</c> 를 자동으로 붙이므로
    /// 같은 이름을 여러 명이 써도 서로 구분된다.
    /// </summary>
    public static async void SetPlayerName(string name, Action<bool> done = null)
    {
        try
        {
            await EnsureReady();
            await AuthenticationService.Instance.UpdatePlayerNameAsync(name);
            done?.Invoke(true);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[랭킹] 닉네임 변경 실패: {e.Message}");
            done?.Invoke(false);
        }
    }
}
