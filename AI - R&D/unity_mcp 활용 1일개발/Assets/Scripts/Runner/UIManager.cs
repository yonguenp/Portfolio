using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class UIManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject charSelectPanel;
    public GameObject hudPanel;
    public GameObject pausePanel;
    public GameObject revivePanel;
    public GameObject adLoadingPanel;
    public GameObject gameOverPanel;
    public GameObject winPanel;

    [Header("CharSelect")]
    public SkinSelectController skinSelectController;
    public TMP_Text             bestScoreCharSelect;

    [Header("HUD")]
    public TMP_Text scoreText;
    public TMP_Text comboText;
    public Button   attackButton;
    public Button   skillButton;
    public Button   pauseButton;
    public Image    attackCooldownOverlay;
    public Image    skillCooldownOverlay;

    [Header("Progress")]
    public Image    progressBar;
    public TMP_Text progressText;

    [Header("Power-ups")]
    public GameObject shieldIndicator;
    public Image      shieldFillBar;
    public GameObject magnetIndicator;
    public Image      magnetFillBar;

    [Header("Audio Mute")]
    public Button   muteSFXButton;
    public TMP_Text muteSFXText;
    public Button   muteBGMButton;
    public TMP_Text muteBGMText;

    [Header("Pause")]
    public Button resumeButton;
    public Button quitButton;

    [Header("Revive")]
    public TMP_Text reviveCountdownText;
    public Button   watchAdButton;
    public Button   declineReviveButton;

    [Header("Game Over")]
    public TMP_Text finalScoreText;
    public TMP_Text bestScoreGameOver;
    public TMP_Text newRecordText;
    public Button   restartButton;

    [Header("Win")]
    public TMP_Text winScoreText;
    public TMP_Text bestScoreWin;
    public TMP_Text winNewRecordText;
    public Button   winRestartButton;

    // 쿨타임 트래킹
    private float _attackCooldown = 0f;
    private float _skillCooldown  = 0f;
    const float   ATK_CD = 0.8f;
    const float   SKL_CD = 8.0f;

    // ── 패널 팝인 애니메이션 ──────────────────────────────────────
    void PopIn(GameObject panel)
    {
        if (panel == null) return;
        panel.SetActive(true);
        StartCoroutine(PopInRoutine(panel.transform));
    }

    System.Collections.IEnumerator PopInRoutine(Transform t)
    {
        float dur = 0.18f, elapsed = 0f;
        t.localScale = Vector3.one * 0.75f;
        while (elapsed < dur)
        {
            elapsed += Time.unscaledDeltaTime;
            float p  = elapsed / dur;
            // ease-out back 커브
            float s  = 1f + 0.25f * Mathf.Sin(p * Mathf.PI);
            t.localScale = Vector3.one * Mathf.LerpUnclamped(0.75f, 1f, p < 1f ? p * p * (3f - 2f * p) : 1f) * s;
            yield return null;
        }
        t.localScale = Vector3.one;
    }

    void Start()
    {
        pauseButton         ?.onClick.AddListener(OnPauseClick);
        resumeButton        ?.onClick.AddListener(OnResumeClick);
        quitButton          ?.onClick.AddListener(OnQuitClick);
        restartButton       ?.onClick.AddListener(OnRestartClick);
        winRestartButton    ?.onClick.AddListener(OnRestartClick);
        watchAdButton       ?.onClick.AddListener(OnWatchAdClick);
        declineReviveButton ?.onClick.AddListener(OnDeclineReviveClick);
        attackButton        ?.onClick.AddListener(OnAttackClick);
        skillButton         ?.onClick.AddListener(OnSkillClick);
        muteSFXButton       ?.onClick.AddListener(OnMuteSFXClick);
        muteBGMButton       ?.onClick.AddListener(OnMuteBGMClick);

        if (attackCooldownOverlay) attackCooldownOverlay.fillAmount = 0f;
        if (skillCooldownOverlay)  skillCooldownOverlay.fillAmount  = 0f;
        if (comboText) comboText.gameObject.SetActive(false);

        ShowShieldIndicator(false);
        ShowMagnetIndicator(false);
        RefreshMuteButtons();

        if (progressBar == null && hudPanel != null)
            BuildProgressBar();
    }

    void Update()
    {
        if (_attackCooldown > 0f)
        {
            _attackCooldown -= Time.deltaTime;
            if (attackCooldownOverlay)
                attackCooldownOverlay.fillAmount = Mathf.Clamp01(_attackCooldown / ATK_CD);
            if (_attackCooldown <= 0f && attackButton) attackButton.interactable = true;
        }
        if (_skillCooldown > 0f)
        {
            _skillCooldown -= Time.deltaTime;
            if (skillCooldownOverlay)
                skillCooldownOverlay.fillAmount = Mathf.Clamp01(_skillCooldown / SKL_CD);
            if (_skillCooldown <= 0f && skillButton) skillButton.interactable = true;
        }

        if (progressBar && RunnerGameManager.Instance != null && RunnerGameManager.Instance.isPlaying)
        {
            float p = Mathf.Clamp01(RunnerGameManager.Instance.currentTime / RunnerGameManager.Instance.maxTime);
            progressBar.fillAmount = p;
            if (progressText)
                progressText.text = Mathf.FloorToInt(p * 100f) + "%";
        }
    }

    // ── 프로그레스 바 자동 생성 ───────────────────────────────
    void BuildProgressBar()
    {
        // 하단 얇은 바 컨테이너
        var barBgGO = new GameObject("ProgressBarBg");
        barBgGO.transform.SetParent(hudPanel.transform, false);
        var barBgRT = barBgGO.AddComponent<RectTransform>();
        barBgRT.anchorMin = new Vector2(0f, 0f);
        barBgRT.anchorMax = new Vector2(1f, 0f);
        barBgRT.pivot     = new Vector2(0.5f, 0f);
        barBgRT.offsetMin = new Vector2(0f,  0f);
        barBgRT.offsetMax = new Vector2(0f, 20f);
        var barBgImg = barBgGO.AddComponent<UnityEngine.UI.Image>();
        barBgImg.color = new Color(0f, 0f, 0f, 0.55f);

        // 채워지는 fill 이미지
        var fillGO = new GameObject("ProgressFill");
        fillGO.transform.SetParent(barBgGO.transform, false);
        var fillRT = fillGO.AddComponent<RectTransform>();
        fillRT.anchorMin = new Vector2(0f, 0f);
        fillRT.anchorMax = new Vector2(1f, 1f);
        fillRT.offsetMin = new Vector2(2f, 2f);
        fillRT.offsetMax = new Vector2(-2f, -2f);
        progressBar = fillGO.AddComponent<UnityEngine.UI.Image>();
        progressBar.type        = UnityEngine.UI.Image.Type.Filled;
        progressBar.fillMethod  = UnityEngine.UI.Image.FillMethod.Horizontal;
        progressBar.fillAmount  = 0f;
        progressBar.color       = new Color(0.27f, 0.85f, 0.36f, 1f);  // 초록

        // 골 아이콘 (깃발 표시 — 우측 끝)
        var goalGO = new GameObject("GoalIcon");
        goalGO.transform.SetParent(barBgGO.transform, false);
        var goalRT = goalGO.AddComponent<RectTransform>();
        goalRT.anchorMin = new Vector2(1f, 0f);
        goalRT.anchorMax = new Vector2(1f, 1f);
        goalRT.pivot     = new Vector2(1f, 0.5f);
        goalRT.offsetMin = new Vector2(-30f, 1f);
        goalRT.offsetMax = new Vector2(-2f, -1f);
        var goalTxt = goalGO.AddComponent<TMPro.TextMeshProUGUI>();
        goalTxt.text      = "GOAL";
        goalTxt.fontSize  = 10f;
        goalTxt.color     = new Color(1f, 0.9f, 0.2f, 1f);
        goalTxt.alignment = TMPro.TextAlignmentOptions.MidlineRight;
        goalTxt.fontStyle = TMPro.FontStyles.Bold;

        // % 텍스트 (왼쪽 끝)
        var pctGO = new GameObject("ProgressText");
        pctGO.transform.SetParent(barBgGO.transform, false);
        var pctRT = pctGO.AddComponent<RectTransform>();
        pctRT.anchorMin = new Vector2(0f, 0f);
        pctRT.anchorMax = new Vector2(0f, 1f);
        pctRT.pivot     = new Vector2(0f, 0.5f);
        pctRT.offsetMin = new Vector2(4f, 1f);
        pctRT.offsetMax = new Vector2(50f, -1f);
        progressText = pctGO.AddComponent<TMPro.TextMeshProUGUI>();
        progressText.text      = "0%";
        progressText.fontSize  = 10f;
        progressText.color     = Color.white;
        progressText.alignment = TMPro.TextAlignmentOptions.MidlineLeft;
    }

    // ── 스킨 목록: SkinSelectController 가 담당 ──────────────

    // ── 패널 전환 ─────────────────────────────────────────────
    public void ShowCharSelect()
    {
        SetAll(false);
        PopIn(charSelectPanel);
        UpdateBestScore(GameDataManager.Instance?.BestScore ?? 0);
        skinSelectController?.ResetScroll();
    }

    public void ShowHUD()
    {
        SetAll(false);
        PopIn(hudPanel);
        ShowShieldIndicator(false);
        ShowMagnetIndicator(false);
        RefreshMuteButtons();
        if (progressBar) progressBar.fillAmount = 0f;
        if (progressText) progressText.text = "0%";
    }

    public void ShowPause() { PopIn(pausePanel); }

    public void ShowReviveOffer(float countdownSec)
    {
        SetAll(false);
        PopIn(revivePanel);
        if (reviveCountdownText) reviveCountdownText.text = Mathf.CeilToInt(countdownSec).ToString();
    }

    public void UpdateReviveCountdown(float remaining)
    {
        if (reviveCountdownText)
            reviveCountdownText.text = Mathf.CeilToInt(remaining).ToString();
    }

    public void ShowAdLoading(bool show)
    {
        if (adLoadingPanel) adLoadingPanel.SetActive(show);
        if (revivePanel)    revivePanel.SetActive(!show);
    }

    public void ShowGameOver(int finalScore, bool newRecord, int best)
    {
        SetAll(false);
        PopIn(gameOverPanel);
        if (finalScoreText)    finalScoreText.text    = "점수: " + finalScore;
        if (bestScoreGameOver) bestScoreGameOver.text  = "최고 기록: " + best;
        if (newRecordText)
        {
            newRecordText.gameObject.SetActive(newRecord);
            if (newRecord) newRecordText.text = "신기록 달성!";
        }
    }

    public void ShowWin(int finalScore, bool newRecord, int best)
    {
        SetAll(false);
        PopIn(winPanel);
        if (winScoreText)     winScoreText.text     = "완주 점수: " + finalScore;
        if (bestScoreWin)     bestScoreWin.text      = "최고 기록: " + best;
        if (winNewRecordText)
        {
            winNewRecordText.gameObject.SetActive(newRecord);
            if (newRecord) winNewRecordText.text = "신기록 달성!";
        }
    }

    public void UpdateScore(int s)   { if (scoreText) scoreText.text = "점수: " + s; }

    public void UpdateBestScore(int best)
    {
        if (bestScoreCharSelect) bestScoreCharSelect.text = "최고 기록: " + best;
    }

    public void ShowCombo(int mult)
    {
        if (!comboText) return;
        comboText.gameObject.SetActive(true);
        comboText.text = "x" + mult + " COMBO!";
        StopCoroutine("ComboFlash");
        StartCoroutine("ComboFlash");
    }

    public void HideCombo() { if (comboText) comboText.gameObject.SetActive(false); }

    IEnumerator ComboFlash()
    {
        for (int i = 0; i < 3; i++)
        {
            comboText.color = Color.white;
            yield return new WaitForSeconds(0.08f);
            comboText.color = new Color(1f, 0.85f, 0.1f);
            yield return new WaitForSeconds(0.08f);
        }
    }

    // ── 파워업 인디케이터 ─────────────────────────────────────
    public void ShowShieldIndicator(bool on)
    {
        if (shieldIndicator) shieldIndicator.SetActive(on);
    }

    public void ShowMagnetIndicator(bool on)
    {
        if (magnetIndicator) magnetIndicator.SetActive(on);
    }

    // ── 뮤트 버튼 ────────────────────────────────────────────
    public void RefreshMuteButtons()
    {
        bool sfxM = AudioManager.Instance?.SFXMuted ?? false;
        bool bgmM = AudioManager.Instance?.BGMMuted ?? false;
        if (muteSFXText) muteSFXText.text = sfxM ? "SFX:OFF" : "SFX:ON";
        if (muteBGMText) muteBGMText.text = bgmM ? "BGM:OFF" : "BGM:ON";
    }

    void SetAll(bool v)
    {
        if (charSelectPanel)  charSelectPanel.SetActive(v);
        if (hudPanel)         hudPanel.SetActive(v);
        if (pausePanel)       pausePanel.SetActive(v);
        if (revivePanel)      revivePanel.SetActive(v);
        if (adLoadingPanel)   adLoadingPanel.SetActive(v);
        if (gameOverPanel)    gameOverPanel.SetActive(v);
        if (winPanel)         winPanel.SetActive(v);
    }

    // ── 버튼 핸들러 ───────────────────────────────────────────
    void OnPauseClick()   => PauseManager.Instance?.Pause();
    void OnResumeClick()  => PauseManager.Instance?.Resume();
    void OnQuitClick()    => PauseManager.Instance?.QuitToMenu();
    void OnRestartClick() { AudioManager.Instance?.PlayButton(); RunnerGameManager.Instance?.RestartGame(); }

    void OnAttackClick()
    {
        if (_attackCooldown > 0f) return;
        _attackCooldown = ATK_CD;
        if (attackButton) attackButton.interactable = false;
        RunnerGameManager.Instance?.AttackNearestEnemy();
    }

    void OnSkillClick()
    {
        if (_skillCooldown > 0f) return;
        _skillCooldown = SKL_CD;
        if (skillButton) skillButton.interactable = false;
        RunnerGameManager.Instance?.UseSkill();
    }

    void OnWatchAdClick()        => ReviveSystem.Instance?.AcceptRevive();
    void OnDeclineReviveClick()  => ReviveSystem.Instance?.DeclineRevive();

    void OnMuteSFXClick()
    {
        AudioManager.Instance?.SetSFXMuted(!(AudioManager.Instance?.SFXMuted ?? false));
        AudioManager.Instance?.PlayButton();
        RefreshMuteButtons();
    }

    void OnMuteBGMClick()
    {
        AudioManager.Instance?.SetBGMMuted(!(AudioManager.Instance?.BGMMuted ?? false));
        AudioManager.Instance?.PlayButton();
        RefreshMuteButtons();
    }
}
