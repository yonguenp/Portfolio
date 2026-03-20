using UnityEngine;

/// <summary>
/// 파워업 시스템: 방패(무적 4초), 자석(코인 흡수 6초)
/// RunnerGameManager.EnsureSystems()에서 자동 생성.
/// </summary>
public class PowerUpSystem : MonoBehaviour
{
    public static PowerUpSystem Instance;

    public const float SHIELD_DUR   = 4f;
    public const float MAGNET_DUR   = 6f;
    const float        MAGNET_RANGE = 9f;
    const float        MAGNET_SPEED = 16f;

    public bool  ShieldActive    { get; private set; }
    public float ShieldRemaining { get; private set; }
    public bool  MagnetActive    { get; private set; }
    public float MagnetRemaining { get; private set; }

    void Awake() { Instance = this; }

    void Update()
    {
        var gm = RunnerGameManager.Instance;
        if (gm == null || !gm.isPlaying) return;
        var ui = gm.uiManager;

        // ── 방패 ──────────────────────────────────
        if (ShieldActive)
        {
            ShieldRemaining -= Time.deltaTime;
            if (ui?.shieldFillBar)
                ui.shieldFillBar.fillAmount = Mathf.Clamp01(ShieldRemaining / SHIELD_DUR);

            if (ShieldRemaining <= 0f)
            {
                ShieldActive    = false;
                gm.IsInvincible = false;
                ui?.ShowShieldIndicator(false);
            }
        }

        // ── 자석 ──────────────────────────────────
        if (MagnetActive)
        {
            MagnetRemaining -= Time.deltaTime;
            if (ui?.magnetFillBar)
                ui.magnetFillBar.fillAmount = Mathf.Clamp01(MagnetRemaining / MAGNET_DUR);

            if (MagnetRemaining <= 0f)
            {
                MagnetActive = false;
                ui?.ShowMagnetIndicator(false);
            }
            else
            {
                AttractCoins(gm);
            }
        }
    }

    // ── 코인 흡수 ──────────────────────────────────────────────
    void AttractCoins(RunnerGameManager gm)
    {
        if (gm.player == null) return;
        Vector3 pPos  = gm.player.transform.position;
        var     items = gm.activeItems;

        for (int i = items.Count - 1; i >= 0; i--)
        {
            var item = items[i];
            if (item == null || item.isEnemy || item.isPowerUp) continue;
            Vector3 dir = pPos - item.transform.position;
            float   d   = dir.magnitude;
            if (d < MAGNET_RANGE && d > 0.05f)
                item.transform.position += dir.normalized * MAGNET_SPEED * Time.deltaTime;
        }
    }

    // ── 활성화 API ────────────────────────────────────────────
    public void ActivateShield()
    {
        ShieldActive    = true;
        ShieldRemaining = SHIELD_DUR;
        var gm = RunnerGameManager.Instance;
        if (gm != null) gm.IsInvincible = true;
        gm?.uiManager?.ShowShieldIndicator(true);
        AudioManager.Instance?.PlayShield();
    }

    public void ActivateMagnet()
    {
        MagnetActive    = true;
        MagnetRemaining = MAGNET_DUR;
        RunnerGameManager.Instance?.uiManager?.ShowMagnetIndicator(true);
        AudioManager.Instance?.PlayMagnet();
    }

    // ── 리셋 (재시작 시) ──────────────────────────────────────
    public void ResetAll()
    {
        ShieldActive    = false;
        MagnetActive    = false;
        ShieldRemaining = 0f;
        MagnetRemaining = 0f;
        var gm = RunnerGameManager.Instance;
        if (gm != null) gm.IsInvincible = false;
        gm?.uiManager?.ShowShieldIndicator(false);
        gm?.uiManager?.ShowMagnetIndicator(false);
    }
}
