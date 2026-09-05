using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// 고스톱 전용 UI — Assets/Prefabs/GoStopUI.prefab 하나만 존재하며
/// GoStop3PScene(2~4인 전부)이 그 인스턴스를 쓴다.
///
/// 2026-08-22: <see cref="GameUIManager"/>(다른 7개 게임이 공유하는 공통
/// UI)에서 완전히 분리한 것 — 고스톱 UI 구조가 다른 게임들과 많이 달라서
/// (가로뷰, 카드/Cap/판돈 표시 등) 공용 GameUI를 억지로 겸용하는 대신
/// 독립된 클래스+프리팹으로 뗐다. 필드 이름·공개 API는 GameUIManager와
/// 의도적으로 동일하게 유지했다 — GoStop3PGame.UI.cs가 참조하는
/// `ui.ContentArea`/`ui?.ShowOverlay(...)` 등 호출부를 전혀 안 바꾸고
/// 타입만 GameUIManager→GoStopUIManager로 바꾸는 것으로 마이그레이션이
/// 끝나게 하려는 것 — 이후 고스톱 UI를 고칠 때(리소스 교체, 레이아웃 조정)
/// 다른 7개 게임에 영향을 줄 걱정이 구조적으로 없다. (2026-08-26: 2인
/// 전용이던 GoStopScene/GoStopGame.cs는 삭제했다.)
/// </summary>
public class GoStopUIManager : MonoBehaviour
{
    public static GoStopUIManager Instance { get; private set; }

    [Header("Layout")]
    [SerializeField] Image         bgImage;
    [SerializeField] Image         hudBar;
    [SerializeField] RectTransform contentArea;

    /// <summary>HUD 전체 컨테이너("SafeArea/HUD") — 프리팹 직렬화 필드를 새로
    /// 추가하는 대신 Awake에서 이름으로 찾는다(프리팹 에셋을 직접 건드리지
    /// 않는 게 더 안전하다는 이 프로젝트의 기존 관례).</summary>
    RectTransform hudRoot;
    Vector2 contentAreaOffsetMaxWithHud;   // HUD 있을 때의 원래 offsetMax(0,-116) — 되돌릴 때 씀

    [Header("HUD")]
    [SerializeField] TextMeshProUGUI titleText;
    [SerializeField] TextMeshProUGUI scoreText;
    [SerializeField] TextMeshProUGUI scoreLabel;
    [SerializeField] TextMeshProUGUI bestText;
    [SerializeField] TextMeshProUGUI bestLabel;
    [SerializeField] Button          newGameButton;
    [SerializeField] Button          backButton;

    [Header("Overlay")]
    [SerializeField] GameObject      overlayPanel;
    [SerializeField] TextMeshProUGUI overlayTitle;
    [SerializeField] TextMeshProUGUI overlayScore;
    [SerializeField] TextMeshProUGUI overlaySub;
    [SerializeField] Button          overlayPrimaryBtn;
    [SerializeField] TextMeshProUGUI overlayPrimaryLabel;
    [SerializeField] Button          overlaySecondaryBtn;
    [SerializeField] TextMeshProUGUI overlaySecondaryLabel;
    [SerializeField] Button          overlayTertiaryBtn;
    [SerializeField] TextMeshProUGUI overlayTertiaryLabel;

    [Header("Help")]
    [SerializeField] Button          helpButton;      // HUD의 ? 버튼
    [SerializeField] GameObject      helpPanel;
    [SerializeField] TextMeshProUGUI helpTitle;
    [SerializeField] TextMeshProUGUI helpBody;
    [SerializeField] Button          helpCloseBtn;
    [SerializeField] TextMeshProUGUI helpCloseLabel;

    [Header("Toast")]
    [SerializeField] GameObject      toastPanel;
    [SerializeField] TextMeshProUGUI toastText;

    Action newGameAction;
    Action backAction;

    /// <summary>게임 콘텐츠를 붙일 부모. HUD 아래 safe area 영역.</summary>
    public RectTransform ContentArea => contentArea;

    /// <summary>점수 텍스트 RectTransform. 획득 연출(스케일 펀치)용.</summary>
    public RectTransform ScoreTextRT => scoreText ? scoreText.rectTransform : null;

    void Awake()
    {
        Instance = this;

        if (hudBar) hudRoot = hudBar.transform.parent as RectTransform; // "Bar"의 부모 = "HUD"
        if (contentArea) contentAreaOffsetMaxWithHud = contentArea.offsetMax;

        if (newGameButton)
        {
            newGameButton.onClick.RemoveAllListeners();
            newGameButton.onClick.AddListener(() => newGameAction?.Invoke());
        }
        if (backButton)
        {
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(() =>
            {
                if (backAction != null) backAction();
                else                    GoBack();
            });
        }

        if (helpButton)
        {
            helpButton.onClick.RemoveAllListeners();
            helpButton.onClick.AddListener(ShowHelp);
        }
        if (helpCloseBtn)
        {
            helpCloseBtn.onClick.RemoveAllListeners();
            helpCloseBtn.onClick.AddListener(HideHelp);
        }

        // 액션/내용이 등록되기 전(게임 Start 이전)에는 숨겨 둔다
        SetNewGameVisible(false);
        SetHelpButtonVisible(false);
        HideHelp();
        HideToast();
    }

    // ── 도움말 ────────────────────────────────────────────
    public bool IsHelpOpen => helpPanel && helpPanel.activeSelf;

    /// <summary>도움말 내용 등록. 등록하면 HUD에 ? 버튼이 나타난다.</summary>
    public void SetHelp(string title, string body, string closeLabel = null)
    {
        if (helpTitle) helpTitle.text = title;
        if (helpBody)
        {
            helpBody.text = body;
            // 2026-09-05: 고스톱 규칙 요약이 다른 게임의 짧은 조작법보다
            // 훨씬 길어서 고정 폰트 크기로는 박스를 넘친다(overflowMode가
            // Overflow라 넘치면 CloseBtn과 겹친다) — 자동 크기 축소로
            // 항상 박스 안에 들어오게 한다. GoStopUIManager는 고스톱
            // 전용이라 다른 7개 게임(GameUIManager)의 기존 고정 크기
            // 동작에는 영향이 없다.
            helpBody.enableAutoSizing = true;
            helpBody.fontSizeMin = 14f;
            helpBody.fontSizeMax = helpBody.fontSize;
        }
        if (helpCloseLabel && !string.IsNullOrEmpty(closeLabel)) helpCloseLabel.text = closeLabel;
        SetHelpButtonVisible(true);
    }

    public void SetHelpButtonVisible(bool v) { if (helpButton) helpButton.gameObject.SetActive(v); }
    public void ShowHelp() { if (helpPanel) helpPanel.SetActive(true);  HideToast(); }
    public void HideHelp() { if (helpPanel) helpPanel.SetActive(false); }

    // ── 하단 안내 문구 ────────────────────────────────────
    public void ShowToast(string msg)
    {
        if (!toastPanel) return;
        if (toastText) toastText.text = msg;
        toastPanel.SetActive(true);
    }

    public void HideToast() { if (toastPanel) toastPanel.SetActive(false); }

    public bool IsToastVisible => toastPanel && toastPanel.activeSelf;

    // ── 액션 등록 ─────────────────────────────────────────
    /// <summary>NEW 버튼 동작. null이면 버튼이 숨겨진다.</summary>
    public void SetNewGameAction(Action a)
    {
        newGameAction = a;
        SetNewGameVisible(a != null);
    }

    /// <summary>뒤로가기 동작. 등록하지 않으면 TitleScene으로 이동한다.</summary>
    public void SetBackAction(Action a) => backAction = a;

    // ── 테마 ──────────────────────────────────────────────
    public void SetBackground(Color c) { if (bgImage) bgImage.color = c; }
    public void SetHudColor(Color c)   { if (hudBar)  hudBar.color  = c; }

    /// <summary>3D 게임처럼 UI 배경이 필요 없는 씬에서 호출.</summary>
    public void SetBackgroundTransparent() => SetBackground(new Color(0f, 0f, 0f, 0f));

    // ── HUD helpers ──────────────────────────────────────
    public void SetTitle(string t)        { if (titleText)     titleText.text     = t; }
    public void SetScore(string s)        { if (scoreText)     scoreText.text     = s; }
    public void SetScore(int s)           => SetScore(s.ToString());
    public void SetBest(string b)         { if (bestText)      bestText.text      = b; }
    public void SetBest(int b)            => SetBest(b.ToString());
    public void SetScoreLabel(string s)   { if (scoreLabel)    scoreLabel.text    = s; }
    public void SetBestLabel(string s)    { if (bestLabel)     bestLabel.text     = s; }

    public void SetBestVisible(bool v)
    {
        if (bestText)  bestText.gameObject.SetActive(v);
        if (bestLabel) bestLabel.gameObject.SetActive(v);
    }

    public void SetScoreVisible(bool v)
    {
        if (scoreText)  scoreText.gameObject.SetActive(v);
        if (scoreLabel) scoreLabel.gameObject.SetActive(v);
    }

    public void SetNewGameVisible(bool v) { if (newGameButton) newGameButton.gameObject.SetActive(v); }
    public void SetBackVisible(bool v)    { if (backButton)    backButton.gameObject.SetActive(v); }

    /// <summary>HUD(제목·점수·NEW·뒤로 버튼이 있는 상단 바) 전체를 껐다 켠다.
    /// 끄면 <see cref="ContentArea"/>가 HUD가 차지하던 116px까지 전부 차지하도록
    /// 다시 늘어난다 — 자체 나가기 버튼을 따로 둔 가로뷰 4인 고스톱이 쓴다.</summary>
    public void SetHudVisible(bool v)
    {
        if (hudRoot) hudRoot.gameObject.SetActive(v);
        if (contentArea) contentArea.offsetMax = v ? contentAreaOffsetMaxWithHud : Vector2.zero;
    }

    // ── Overlay ──────────────────────────────────────────
    /// <summary>
    /// 보조 버튼은 라벨이 null이면 숨겨진다.
    /// 3번째 버튼까지 쓰면 하단 두 버튼이 좌우로 나뉘고, 없으면 2번째가 가운데로 넓어진다.
    /// </summary>
    public void ShowOverlay(Color titleColor, string title,
                            string scoreStr, string subStr,
                            string primaryLabel, Action primaryAction,
                            string secondaryLabel = null, Action secondaryAction = null,
                            string tertiaryLabel  = null, Action tertiaryAction  = null)
    {
        if (!overlayPanel) return;

        if (overlayTitle) { overlayTitle.text = title; overlayTitle.color = titleColor; }
        if (overlayScore) { overlayScore.text = scoreStr ?? ""; overlayScore.gameObject.SetActive(!string.IsNullOrEmpty(scoreStr)); }
        if (overlaySub)   { overlaySub.text   = subStr  ?? ""; overlaySub.gameObject.SetActive(!string.IsNullOrEmpty(subStr)); }

        if (overlayPrimaryBtn)
        {
            overlayPrimaryBtn.onClick.RemoveAllListeners();
            overlayPrimaryBtn.onClick.AddListener(() => primaryAction?.Invoke());
            if (overlayPrimaryLabel) overlayPrimaryLabel.text = primaryLabel;
        }

        bool hasSec = secondaryLabel != null;
        bool hasTer = tertiaryLabel  != null;

        if (overlaySecondaryBtn)
        {
            overlaySecondaryBtn.gameObject.SetActive(hasSec);
            if (hasSec)
            {
                overlaySecondaryBtn.onClick.RemoveAllListeners();
                overlaySecondaryBtn.onClick.AddListener(() => secondaryAction?.Invoke());
                if (overlaySecondaryLabel) overlaySecondaryLabel.text = secondaryLabel;

                // 3번째 버튼 유무에 따라 하단 행을 1열/2열로 바꾼다
                var r = overlaySecondaryBtn.GetComponent<RectTransform>();
                r.anchorMin = new Vector2(hasTer ? 0.08f : 0.20f, r.anchorMin.y);
                r.anchorMax = new Vector2(hasTer ? 0.48f : 0.80f, r.anchorMax.y);
                r.offsetMin = new Vector2(0f, r.offsetMin.y);
                r.offsetMax = new Vector2(0f, r.offsetMax.y);
            }
        }

        if (overlayTertiaryBtn)
        {
            overlayTertiaryBtn.gameObject.SetActive(hasTer);
            if (hasTer)
            {
                overlayTertiaryBtn.onClick.RemoveAllListeners();
                overlayTertiaryBtn.onClick.AddListener(() => tertiaryAction?.Invoke());
                if (overlayTertiaryLabel) overlayTertiaryLabel.text = tertiaryLabel;
            }
        }

        overlayPanel.SetActive(true);
    }

    /// <summary>이미 떠 있는 오버레이의 서브 텍스트만 갱신한다 — 고/스톱
    /// 오버레이에 무응답 타임아웃 카운트다운을 얹기 위해 2026-09-05 추가.
    /// ShowOverlay를 다시 부르면 버튼 리스너까지 통째로 새로 붙어야 해서
    /// 매 프레임 부르기엔 낭비고, 이 메서드는 텍스트 한 줄만 바꾼다.</summary>
    public void SetOverlaySub(string subStr)
    {
        if (!overlaySub) return;
        overlaySub.text = subStr ?? "";
        overlaySub.gameObject.SetActive(!string.IsNullOrEmpty(subStr));
    }

    public void HideOverlay() { if (overlayPanel) overlayPanel.SetActive(false); }
    public void GoBack()      => SceneManager.LoadScene("TitleScene");
}
