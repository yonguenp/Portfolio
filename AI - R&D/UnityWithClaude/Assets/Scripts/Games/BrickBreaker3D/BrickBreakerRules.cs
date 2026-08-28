using UnityEngine;

public enum BrickBreakerMode { Normal, Item }

/// <summary>
/// 모드별 규칙 모음.
///
/// 예전 "클래식"(hp = turn, 흩뿌리기 스폰, 2턴당 아이템)은 제거했다 —
/// 볼 증가(기울기 0.5)보다 HP 기울기(1.0)가 가팔라 수학적으로 반드시 무너지는
/// 곡선이었다. 아래 규칙이 이제 기본이다.
///
/// 모드는 규칙의 **덧셈**으로만 갈린다:
///   Normal : 아래 기본 규칙
///   Item   : 기본 규칙 + 파워업 아이템 드롭
/// </summary>
public static class BrickBreakerRules
{
    // 예전 키(Classic/Arcade 값)가 남아 엉뚱한 모드로 시작하지 않도록 키를 새로 판다
    const string KEY = "BrickBreakerMode2";

    public static BrickBreakerMode Mode { get; private set; } =
        (BrickBreakerMode)PlayerPrefs.GetInt(KEY, (int)BrickBreakerMode.Normal);

    public static bool IsItemMode => Mode == BrickBreakerMode.Item;

    public static void SetMode(BrickBreakerMode m)
    {
        Mode = m;
        PlayerPrefs.SetInt(KEY, (int)m);
        PlayerPrefs.Save();
    }

    public static BrickBreakerMode Other =>
        Mode == BrickBreakerMode.Normal ? BrickBreakerMode.Item : BrickBreakerMode.Normal;

    public static string NameOf(BrickBreakerMode m)
    {
        var loc = LocalizationManager.Instance;
        return m == BrickBreakerMode.Item
            ? (loc != null ? loc.GetOr("bb_mode_item",   "아이템") : "아이템")
            : (loc != null ? loc.GetOr("bb_mode_normal", "기본")   : "기본");
    }

    // ── 난이도 ───────────────────────────────────────────
    /// <summary>이번 턴에 나오는 브릭의 HP. 볼 증가(2턴당 +1)와 같은 기울기 0.5.</summary>
    public static int HpForTurn(int turn) => 1 + turn / 2;

    // ── 스폰 ─────────────────────────────────────────────
    public static int ClusterCount(int turn) => Mathf.Clamp(1 + turn / 8, 1, 3);

    /// <summary>붙어 있는 칸 묶음. 흩어놓으면 연쇄가 없어 콤보가 안 쌓인다.</summary>
    public static readonly Vector2Int[][] Clusters =
    {
        new[] { V(0,0), V(1,0), V(0,1), V(1,1) },            // 2×2 블록
        new[] { V(0,0), V(1,0), V(2,0) },                    // 가로 3
        new[] { V(0,0), V(0,1), V(0,2) },                    // 세로 3
        new[] { V(0,0), V(1,0), V(1,1) },                    // L
        new[] { V(0,0), V(1,0), V(2,0), V(1,1) },            // T
        new[] { V(0,0), V(1,1), V(2,2) },                    // 대각
        new[] { V(0,0), V(1,0), V(0,1) },                    // 작은 ㄱ
    };

    static Vector2Int V(int x, int y) => new Vector2Int(x, y);

    // ── 볼 경제 ──────────────────────────────────────────
    public const int ItemTurnInterval = 1;   // 매 턴 추가볼 하나
    // 항상 +1. 턴에 따라 배수로 늘리면 후반에 화력이 폭주해 판이 무너진다.
    public const int ItemBallValue    = 1;

    // ── 턴 클리어 보너스 ─────────────────────────────────
    public const int CLEAR_BONUS_SCORE = 25;

    // ── 아이템 모드: 파워업 드롭 ─────────────────────────
    /// <param name="luck">지금까지 먹은 행운 아이템 수.</param>
    public static float PowerUpChance(int luck) =>
        IsItemMode ? Mathf.Min(0.35f + luck * 0.15f, 0.85f) : 0f;

    /// <summary>
    /// 구형 브릭 등장 확률. 아이템 모드에서 후반에만 나온다 —
    /// 축 정렬 반사만 있으면 판이 읽혀서 ALL CLEAR가 계속 나오고 단조로워진다.
    /// 구는 맞은 지점에 따라 튀는 방향이 달라져 판을 다시 어렵게 만든다.
    /// </summary>
    public const int SPHERE_FROM_TURN = 12;
    public static float SphereChance(int turn) =>
        IsItemMode && turn >= SPHERE_FROM_TURN
            ? Mathf.Min((turn - SPHERE_FROM_TURN) * 0.05f + 0.15f, 0.55f)
            : 0f;

    /// <summary>정사면체 브릭 등장 확률. 구형과 같은 시점부터, 조금 낮게.</summary>
    public static float TetraChance(int turn) =>
        IsItemMode && turn >= SPHERE_FROM_TURN
            ? Mathf.Min((turn - SPHERE_FROM_TURN) * 0.03f + 0.10f, 0.35f)
            : 0f;

    /// <summary>박스가 랜덤 축으로 기울어질 확률. 축 정렬이 깨지면 반사 예측이 어려워진다.</summary>
    public static float BoxTiltChance(int turn) =>
        IsItemMode && turn >= SPHERE_FROM_TURN
            ? Mathf.Min((turn - SPHERE_FROM_TURN) * 0.04f + 0.15f, 0.50f)
            : 0f;

    /// <summary>
    /// 계속 회전하는 브릭의 비율. 타점이 매 순간 바뀌므로 뒤따르는 공들이
    /// 사방으로 흩어진다 — 판을 읽어서 ALL CLEAR를 반복하던 단조로움을 깬다.
    /// 조준 예측선은 발사 시점 기준 가이드가 된다.
    /// </summary>
    public static float SpinChance(int turn) =>
        IsItemMode && turn >= SPHERE_FROM_TURN
            ? Mathf.Min((turn - SPHERE_FROM_TURN) * 0.05f + 0.20f, 0.60f)
            : 0f;

    // 한 바퀴에 5~10초. 뒤따르는 공의 타점이 달라지기엔 충분하고,
    // 눈이 피곤할 만큼 빠르지는 않은 구간.
    public const float SPIN_MIN_DEG = 36f;
    public const float SPIN_MAX_DEG = 72f;

    // 데미지는 곱이 아니라 완만한 덧셈으로. 기본 1.0에서 한 번에 +0.30(=30%)씩,
    // 최대 2.5배까지. 예전엔 정수 +1이라 첫 획득이 곧 2배였다.
    public const float BALL_DAMAGE_STEP = 0.30f;
    public const float MAX_BALL_DAMAGE  = 2.50f;
    public const float BALL_SIZE_STEP   = 1.16f;   // 먹을 때마다 반지름 배수
    public const float MAX_BALL_RADIUS  = 0.60f;   // 기본 0.22
}
