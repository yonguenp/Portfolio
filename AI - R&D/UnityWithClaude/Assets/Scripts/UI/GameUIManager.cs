using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// 모든 게임 씬이 공유하는 UI. Assets/Prefabs/GameUI.prefab 하나만 존재하며
/// 씬마다 HUD/오버레이를 다시 만들지 않는다.
///
/// 게임 스크립트는 자기 콘텐츠를 <see cref="ContentArea"/> 아래에 붙이고,
/// 버튼 동작은 <see cref="SetNewGameAction"/> / <see cref="SetBackAction"/>으로 등록한다.
/// 프리팹은 씬 안의 스크립트를 직렬화 참조할 수 없으므로 persistent listener 대신
/// 런타임 등록을 쓴다.
/// </summary>
public class GameUIManager : MonoBehaviour
{
    public static GameUIManager Instance { get; private set; }

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

        // GoStop3PGame이 진짜 가로 고정을 걸 수 있으려면 프로젝트 기본
        // 화면 방향이 AutoRotation이어야 한다(단일 orientation이면 iOS
        // Info.plist 자체가 그 방향만 지원해서 런타임 강제가 무의미해짐 —
        // SplashManager.cs 주석 참고). 그 대가로 GameUI.prefab을 쓰는
        // 나머지 7개 게임은 전부 여기서 세로를 직접 잠가야 한다 — GoStop만
        // 별도 GoStopUIManager를 쓰므로 이 잠금과 무관하게 자기 씬에서
        // 가로를 강제한다.
        Screen.orientation = ScreenOrientation.Portrait;

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
        if (helpBody)  helpBody.text  = body;
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
    /// 다시 늘어난다 — "공간을 많이 차지한다"는 요청에서, 자체 나가기 버튼을
    /// 따로 둘 게임(가로뷰 4인 고스톱 등)을 위해 추가했다. 다른 게임은 이
    /// 메서드를 안 부르므로 기존 동작에 영향 없다.</summary>
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

    public void HideOverlay() { if (overlayPanel) overlayPanel.SetActive(false); }
    public void GoBack()      => SceneManager.LoadScene("TitleScene");
}
