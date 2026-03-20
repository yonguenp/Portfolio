using UnityEngine;
using System.Collections;

/// <summary>
/// 게임오버 시 광고 시청 후 부활 시스템.
/// 실제 AdMob 연동 전 플레이스홀더 구현 (1.5초 로딩 시뮬레이션).
/// 게임당 1회 부활 가능.
/// </summary>
public class ReviveSystem : MonoBehaviour
{
    public static ReviveSystem Instance;

    public bool HasRevived   { get; private set; } = false;
    public bool CanRevive    => !HasRevived;

    const float OFFER_TIME = 5f; // 부활 제안 카운트다운 (초)

    void Awake() { Instance = this; }

    // ── 게임오버 발생 시 호출 ─────────────────────────────
    public void OfferRevive()
    {
        if (!CanRevive)
        {
            // 이미 부활 사용 → 바로 게임오버 화면
            FinalGameOver();
            return;
        }
        RunnerGameManager.Instance?.uiManager?.ShowReviveOffer(OFFER_TIME);
        StartCoroutine(CountdownRoutine());
    }

    // ── 광고 시청 클릭 ────────────────────────────────────
    public void AcceptRevive()
    {
        StopAllCoroutines();
        StartCoroutine(SimulateAdAndRevive());
    }

    // ── 포기 클릭 또는 타임아웃 ──────────────────────────
    public void DeclineRevive()
    {
        StopAllCoroutines();
        FinalGameOver();
    }

    // ── 카운트다운 ────────────────────────────────────────
    IEnumerator CountdownRoutine()
    {
        float remaining = OFFER_TIME;
        while (remaining > 0f)
        {
            RunnerGameManager.Instance?.uiManager?.UpdateReviveCountdown(remaining);
            yield return new WaitForSecondsRealtime(0.1f);
            remaining -= 0.1f;
        }
        DeclineRevive();
    }

    // ── 광고 시뮬레이션 → 부활 ───────────────────────────
    IEnumerator SimulateAdAndRevive()
    {
        RunnerGameManager.Instance?.uiManager?.ShowAdLoading(true);
        yield return new WaitForSecondsRealtime(1.5f); // 광고 로딩 시뮬레이션
        RunnerGameManager.Instance?.uiManager?.ShowAdLoading(false);
        Revive();
    }

    void Revive()
    {
        HasRevived = true;
        var gm = RunnerGameManager.Instance;
        if (gm == null) return;

        // 게임 상태 복구
        gm.state = RunnerGameManager.GameState.Playing;

        // 플레이어 안전 위치로 초기화
        if (gm.player != null)
        {
            var p = gm.player.transform.position;
            p.y = 1f;
            gm.player.transform.position = p;
            gm.player.ResetVelocity();
        }

        // 콤보 리셋
        ComboSystem.Instance?.ResetCombo();

        // 애니메이션 복귀
        gm.player?.PlayRevive();

        // 소리
        AudioManager.Instance?.PlayRevive();

        // 무적 2초
        StartCoroutine(Invincibility(gm, 2f));

        // HUD 복귀
        gm.uiManager?.ShowHUD();
    }

    IEnumerator Invincibility(RunnerGameManager gm, float dur)
    {
        gm.IsInvincible = true;
        yield return new WaitForSeconds(dur);
        gm.IsInvincible = false;
    }

    void FinalGameOver()
    {
        var gm = RunnerGameManager.Instance;
        if (gm == null) return;

        bool newRecord = GameDataManager.Instance?.SubmitScore(gm.score) ?? false;
        GameDataManager.Instance?.AddCoins(gm.CoinCount);
        gm.uiManager?.ShowGameOver(gm.score, newRecord,
            GameDataManager.Instance?.BestScore ?? 0);
    }
}
