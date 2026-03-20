using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// Metatoy-Dragons 스타일 UI 빌드.
/// 진한 보라/남색 배경 + 골드 포인트 + 채도 높은 버튼.
/// </summary>
public static class SetupRunnerUI
{
    // ── 팔레트 ─────────────────────────────────────────────
    static readonly Color C_BG_DEEP     = Hex("#0D0A22");   // 패널 배경
    static readonly Color C_BG_MID      = Hex("#1A1545");   // 패널 배경 (밝게)
    static readonly Color C_GOLD        = Hex("#FFD84D");   // 골드 포인트
    static readonly Color C_GOLD_DIM    = Hex("#B8860B");   // 골드 어둡게
    static readonly Color C_BTN_GREEN   = Hex("#1DB954");   // 시작/계속 버튼
    static readonly Color C_BTN_RED     = Hex("#E8402A");   // 종료/포기 버튼
    static readonly Color C_BTN_BLUE    = Hex("#2D7FEA");   // 일반 행동 버튼
    static readonly Color C_BTN_PURPLE  = Hex("#7C3AED");   // 스킬 버튼
    static readonly Color C_BTN_CYAN    = Hex("#06B6D4");   // 광고 버튼
    static readonly Color C_ATK         = Hex("#EF4444");   // 공격 버튼
    static readonly Color C_OVERLAY     = new Color(0,0,0,0.78f);

    // 빌드 시 공유 폰트
    private static TMP_FontAsset _font;

    [MenuItem("Tools/Setup Runner UI")]
    public static void Run()
    {
        _font = FontSetup.GetOrCreateFont();

        if (Object.FindFirstObjectByType<EventSystem>() == null)
        {
            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }

        var canvasGO = GameObject.Find("UICanvas") ?? new GameObject("UICanvas");
        for (int i = canvasGO.transform.childCount - 1; i >= 0; i--)
            Object.DestroyImmediate(canvasGO.transform.GetChild(i).gameObject);

        var canvas = canvasGO.GetComponent<Canvas>() ?? canvasGO.AddComponent<Canvas>();
        canvas.renderMode  = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;
        var scaler = canvasGO.GetComponent<CanvasScaler>() ?? canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight  = 0.5f;
        if (!canvasGO.GetComponent<GraphicRaycaster>()) canvasGO.AddComponent<GraphicRaycaster>();

        var ui = canvasGO.GetComponent<UIManager>() ?? canvasGO.AddComponent<UIManager>();

        // ══════════════════════════════════════════════════
        //  캐릭터 선택 패널  (앵커 기반 — 해상도 독립)
        // ══════════════════════════════════════════════════
        var csPanel = Panel(canvasGO.transform, "CharSelectPanel", C_BG_DEEP);

        // 상단 타이틀 영역 — 화면 위 13% 고정
        {
            var go = new GameObject("TitleBar"); go.transform.SetParent(csPanel.transform, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 0.87f); rt.anchorMax = new Vector2(1f, 1f);
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            go.AddComponent<Image>().color = C_BG_MID;

            var tgo = new GameObject("Title"); tgo.transform.SetParent(go.transform, false);
            var trt = tgo.AddComponent<RectTransform>();
            trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one;
            trt.offsetMin = trt.offsetMax = Vector2.zero;
            var ttmp = tgo.AddComponent<TextMeshProUGUI>();
            if (_font != null) ttmp.font = _font;
            ttmp.text      = "캐릭터 선택";
            ttmp.fontSize  = 70;
            ttmp.fontStyle = FontStyles.Bold;
            ttmp.alignment = TextAlignmentOptions.Center;
            ttmp.color     = C_GOLD;
        }

        // 골드 구분선 — 타이틀 바로 아래
        {
            var go = new GameObject("AccentBar"); go.transform.SetParent(csPanel.transform, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.04f, 0.865f); rt.anchorMax = new Vector2(0.96f, 0.870f);
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            go.AddComponent<Image>().color = C_GOLD;
        }

        // 최고기록 — 구분선 아래 한 줄
        GameObject bestGoCS;
        {
            var go = new GameObject("BestScore"); go.transform.SetParent(csPanel.transform, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 0.81f); rt.anchorMax = new Vector2(1f, 0.865f);
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            var t = go.AddComponent<TextMeshProUGUI>();
            if (_font != null) t.font = _font;
            t.text = "최고 기록: 0"; t.fontSize = 34;
            t.alignment = TextAlignmentOptions.Center;
            t.color = new Color(0.85f, 0.85f, 1f);
            bestGoCS = go;
        }

        // 스크롤뷰 — 나머지 전체 차지
        var scroll   = ScrollView(csPanel.transform, Vector2.zero, Vector2.zero, true);
        var skinCtrl = scroll.GetComponent<SkinSelectController>();

        // ══════════════════════════════════════════════════
        //  HUD 패널
        // ══════════════════════════════════════════════════
        var hud = Panel(canvasGO.transform, "HUDPanel", Color.clear);
        hud.GetComponent<Image>().raycastTarget = false;

        // 점수 (좌상단 — 반투명 배지)
        var scoreBadge = Rect(hud.transform, "ScoreBadge",
            Vector2.zero, new Vector2(320, 64), new Color(0,0,0,0.52f));
        var sbRT = scoreBadge.GetComponent<RectTransform>();
        sbRT.anchorMin = sbRT.anchorMax = sbRT.pivot = new Vector2(0, 1);
        sbRT.anchoredPosition = new Vector2(16, -16);
        var scoreTxt = Label(scoreBadge.transform, "ScoreText",
            "점수: 0", 36, Vector2.zero, Vector2.zero);
        scoreTxt.GetComponent<RectTransform>().anchorMin = Vector2.zero;
        scoreTxt.GetComponent<RectTransform>().anchorMax = Vector2.one;
        scoreTxt.GetComponent<RectTransform>().offsetMin =
        scoreTxt.GetComponent<RectTransform>().offsetMax = Vector2.zero;
        scoreTxt.alignment = TextAlignmentOptions.MidlineLeft;
        var scoreRTFix = scoreTxt.GetComponent<RectTransform>();
        scoreRTFix.offsetMin = new Vector2(12, 0);
        scoreRTFix.offsetMax = new Vector2(-4, 0);

        // 콤보 (상단 중앙)
        var comboTxt = Label(hud.transform, "ComboText",
            "x2 COMBO!", 52, new Vector2(0, -90), new Vector2(520, 76));
        comboTxt.color     = C_GOLD;
        comboTxt.fontStyle = FontStyles.Bold;
        comboTxt.gameObject.SetActive(false);

        // ── 일시정지 버튼 (우상단) ──────────────────────
        var pauseBtn = CircleBtn(hud.transform, "PauseButton", "⏸",
            Vector2.zero, 88, new Color(0.1f, 0.1f, 0.2f, 0.82f));
        var pRT = pauseBtn.GetComponent<RectTransform>();
        pRT.anchorMin = pRT.anchorMax = pRT.pivot = new Vector2(1, 1);
        pRT.anchoredPosition = new Vector2(-20, -20);

        // ── SFX / BGM 뮤트 버튼 ──────────────────────────
        var sfxBtn = SmallBtn(hud.transform, "MuteSFXBtn", "SFX♪",
            Vector2.zero, new Vector2(100, 52), new Color(0.1f, 0.1f, 0.2f, 0.78f));
        var sfxRT = sfxBtn.GetComponent<RectTransform>();
        sfxRT.anchorMin = sfxRT.anchorMax = sfxRT.pivot = new Vector2(1, 1);
        sfxRT.anchoredPosition = new Vector2(-118, -20);
        var sfxTxt = sfxBtn.GetComponentInChildren<TextMeshProUGUI>();
        sfxTxt.fontSize = 22;

        var bgmBtn = SmallBtn(hud.transform, "MuteBGMBtn", "BGM♪",
            Vector2.zero, new Vector2(100, 52), new Color(0.1f, 0.1f, 0.2f, 0.78f));
        var bgmRT = bgmBtn.GetComponent<RectTransform>();
        bgmRT.anchorMin = bgmRT.anchorMax = bgmRT.pivot = new Vector2(1, 1);
        bgmRT.anchoredPosition = new Vector2(-118, -80);
        var bgmTxt = bgmBtn.GetComponentInChildren<TextMeshProUGUI>();
        bgmTxt.fontSize = 22;

        // ── 전투 버튼 (우하단) ───────────────────────────
        var atkBtn = BigBtn(hud.transform, "AttackButton", "⚔", "공격",
            Vector2.zero, new Vector2(220, 130), C_ATK);
        var aRT = atkBtn.GetComponent<RectTransform>();
        aRT.anchorMin = aRT.anchorMax = aRT.pivot = new Vector2(1, 0);
        aRT.anchoredPosition = new Vector2(-20, 120);
        var atkCD = CooldownOverlay(atkBtn.gameObject);

        var sklBtn = BigBtn(hud.transform, "SkillButton", "✨", "스킬",
            Vector2.zero, new Vector2(220, 130), C_BTN_PURPLE);
        var sRT2 = sklBtn.GetComponent<RectTransform>();
        sRT2.anchorMin = sRT2.anchorMax = sRT2.pivot = new Vector2(1, 0);
        sRT2.anchoredPosition = new Vector2(-20, 270);
        var sklCD = CooldownOverlay(sklBtn.gameObject);

        // ── 파워업 인디케이터 (좌하단) ─────────────────
        var shieldInd = PowerUpIndicator(hud.transform, "ShieldIndicator",
            "⛨", Hex("#00C8E0CC"), new Vector2(16, 270));
        var shieldBar = shieldInd.transform.Find("FillBar")?.GetComponent<Image>();

        var magnetInd = PowerUpIndicator(hud.transform, "MagnetIndicator",
            "🧲", Hex("#9B30F0CC"), new Vector2(16, 342));
        var magnetBar = magnetInd.transform.Find("FillBar")?.GetComponent<Image>();

        shieldInd.SetActive(false);
        magnetInd.SetActive(false);

        // ══════════════════════════════════════════════════
        //  일시정지 패널
        // ══════════════════════════════════════════════════
        var pausePanel = Panel(canvasGO.transform, "PausePanel", C_OVERLAY);

        // 중앙 카드
        var pauseCard = Rect(pausePanel.transform, "Card",
            Vector2.zero, new Vector2(700, 560), C_BG_MID);
        RoundBorder(pauseCard.transform, C_GOLD, 3);

        Label(pauseCard.transform, "PauseTxt", "일 시 정 지",
            68, new Vector2(0, 190), new Vector2(600, 90)).color = C_GOLD;
        AccentBar(pauseCard.transform, new Vector2(0, 148), new Vector2(500, 3), C_GOLD);

        Btn(pauseCard.transform, "ResumeButton", "▶  계속하기",
            new Vector2(0, 40), new Vector2(520, 110), C_BTN_GREEN);
        Btn(pauseCard.transform, "QuitButton", "🏠  메뉴로",
            new Vector2(0, -100), new Vector2(520, 100), C_BTN_RED);

        // ── UIManager 참조용 별칭
        var resumeBtn = pauseCard.transform.Find("ResumeButton").GetComponent<Button>();
        var quitBtn   = pauseCard.transform.Find("QuitButton").GetComponent<Button>();

        // ══════════════════════════════════════════════════
        //  부활 패널
        // ══════════════════════════════════════════════════
        var revivePanel = Panel(canvasGO.transform, "RevivePanel", C_OVERLAY);
        var reviveCard  = Rect(revivePanel.transform, "Card",
            Vector2.zero, new Vector2(760, 700), C_BG_MID);
        RoundBorder(reviveCard.transform, C_BTN_CYAN, 3);

        Label(reviveCard.transform, "ReviveTxt", "계속하시겠습니까?",
            58, new Vector2(0, 270), new Vector2(700, 82)).color = Color.white;
        AccentBar(reviveCard.transform, new Vector2(0, 228), new Vector2(560, 3), C_BTN_CYAN);

        var reviveCntTxt = Label(reviveCard.transform, "Countdown",
            "5", 130, new Vector2(0, 80), new Vector2(200, 170));
        reviveCntTxt.color = C_GOLD;
        reviveCntTxt.fontStyle = FontStyles.Bold;

        var watchAdBtn  = Btn(reviveCard.transform, "WatchAdBtn", "📺  광고 보고 부활",
            new Vector2(0, -90), new Vector2(580, 120), C_BTN_CYAN);
        var declineBtn  = Btn(reviveCard.transform, "DeclineBtn", "포기하기",
            new Vector2(0, -230), new Vector2(420, 96), C_BTN_RED);

        // 광고 로딩 패널
        var adPanel = Panel(canvasGO.transform, "AdLoadingPanel", new Color(0,0,0,0.62f));
        Label(adPanel.transform, "AdTxt", "광고 불러오는 중...",
            52, Vector2.zero, new Vector2(700, 90));
        adPanel.SetActive(false);

        // ══════════════════════════════════════════════════
        //  게임오버 패널
        // ══════════════════════════════════════════════════
        var goPanel = Panel(canvasGO.transform, "GameOverPanel", C_OVERLAY);

        // 상단 빨간 배너
        var goBanner = Rect(goPanel.transform, "Banner",
            new Vector2(0, 540), new Vector2(1080, 200), Hex("#3A0000E0"));
        Label(goBanner.transform, "GOTxt", "GAME OVER",
            90, Vector2.zero, new Vector2(800, 120)).color = Hex("#FF3B3B");

        var goCard = Rect(goPanel.transform, "Card",
            new Vector2(0, 80), new Vector2(760, 520), C_BG_MID);
        RoundBorder(goCard.transform, Hex("#FF3B3B"), 3);

        var newRecTxt = Label(goCard.transform, "NewRecord", "🏅 신기록!",
            56, new Vector2(0, 190), new Vector2(560, 76));
        newRecTxt.color = C_GOLD;
        newRecTxt.fontStyle = FontStyles.Bold;
        newRecTxt.gameObject.SetActive(false);

        var finalTxt  = Label(goCard.transform, "FinalScore", "점수: 0",
            54, new Vector2(0, 90), new Vector2(560, 76));
        var bestGO    = Label(goCard.transform, "BestScore", "최고 기록: 0",
            36, new Vector2(0, 10), new Vector2(560, 56));
        bestGO.color = new Color(0.75f, 0.75f, 0.92f);

        var restartBtn = Btn(goCard.transform, "RestartBtn", "🔄  다시 시작",
            new Vector2(0, -140), new Vector2(480, 110), C_BTN_BLUE);

        // ══════════════════════════════════════════════════
        //  완주 패널
        // ══════════════════════════════════════════════════
        var winPanel = Panel(canvasGO.transform, "WinPanel", C_OVERLAY);

        var winBanner = Rect(winPanel.transform, "Banner",
            new Vector2(0, 540), new Vector2(1080, 200), Hex("#002A00E0"));
        Label(winBanner.transform, "WinTxt", "🏆  완  주!",
            86, Vector2.zero, new Vector2(800, 120)).color = C_GOLD;

        var winCard = Rect(winPanel.transform, "Card",
            new Vector2(0, 80), new Vector2(760, 520), C_BG_MID);
        RoundBorder(winCard.transform, C_GOLD, 3);

        var winNewRec = Label(winCard.transform, "WinRecord", "🏅 신기록!",
            56, new Vector2(0, 190), new Vector2(560, 76));
        winNewRec.color = C_GOLD;
        winNewRec.fontStyle = FontStyles.Bold;
        winNewRec.gameObject.SetActive(false);

        var winScore  = Label(winCard.transform, "WinScore", "완주 점수: 0",
            52, new Vector2(0, 90), new Vector2(600, 76));
        var bestWin   = Label(winCard.transform, "BestWin", "최고 기록: 0",
            36, new Vector2(0, 10), new Vector2(560, 56));
        bestWin.color = new Color(0.75f, 0.75f, 0.92f);

        var winRestart = Btn(winCard.transform, "WinRestartBtn", "🔄  다시 시작",
            new Vector2(0, -140), new Vector2(480, 110), C_BTN_GREEN);

        // ══════════════════════════════════════════════════
        //  UIManager 연결
        // ══════════════════════════════════════════════════
        ui.charSelectPanel  = csPanel;
        ui.hudPanel         = hud;
        ui.pausePanel       = pausePanel;
        ui.revivePanel      = revivePanel;
        ui.adLoadingPanel   = adPanel;
        ui.gameOverPanel    = goPanel;
        ui.winPanel         = winPanel;

        ui.skinSelectController = skinCtrl;
        ui.bestScoreCharSelect  = bestGoCS.GetComponent<TMP_Text>();

        ui.scoreText             = scoreTxt;
        ui.comboText             = comboTxt;
        ui.pauseButton           = pauseBtn;
        ui.attackButton          = atkBtn;
        ui.skillButton           = sklBtn;
        ui.attackCooldownOverlay = atkCD;
        ui.skillCooldownOverlay  = sklCD;

        ui.shieldIndicator = shieldInd;
        ui.shieldFillBar   = shieldBar;
        ui.magnetIndicator = magnetInd;
        ui.magnetFillBar   = magnetBar;

        ui.muteSFXButton = sfxBtn;
        ui.muteSFXText   = sfxTxt;
        ui.muteBGMButton = bgmBtn;
        ui.muteBGMText   = bgmTxt;

        ui.resumeButton        = resumeBtn;
        ui.quitButton          = quitBtn;

        ui.reviveCountdownText  = reviveCntTxt;
        ui.watchAdButton        = watchAdBtn;
        ui.declineReviveButton  = declineBtn;

        ui.finalScoreText    = finalTxt;
        ui.bestScoreGameOver = bestGO;
        ui.newRecordText     = newRecTxt;
        ui.restartButton     = restartBtn;

        ui.winScoreText     = winScore;
        ui.bestScoreWin     = bestWin;
        ui.winNewRecordText = winNewRec;
        ui.winRestartButton = winRestart;

        // 초기 상태
        csPanel.SetActive(true);
        hud.SetActive(false);
        pausePanel.SetActive(false);
        revivePanel.SetActive(false);
        goPanel.SetActive(false);
        winPanel.SetActive(false);

        var gm = Object.FindFirstObjectByType<RunnerGameManager>();
        if (gm) { gm.uiManager = ui; EditorUtility.SetDirty(gm); }
        EditorUtility.SetDirty(canvasGO);

        // ── 씬 내 모든 TMP에 폰트 강제 재적용 ──────────────────
        if (_font != null)
        {
            foreach (var t in Object.FindObjectsByType<TextMeshProUGUI>(
                FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                t.font = _font;
                EditorUtility.SetDirty(t);
            }
        }

        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
        AssetDatabase.SaveAssets();
        Debug.Log("[SetupRunnerUI] Metatoy-Dragons 스타일 UI 빌드 완료!");
    }

    // ═══════════════════════════════════════════════════════
    //  헬퍼
    // ═══════════════════════════════════════════════════════

    static Color Hex(string h)
    {
        ColorUtility.TryParseHtmlString(h, out var c);
        return c;
    }

    // 전체 화면 패널
    static GameObject Panel(Transform p, string name, Color col)
    {
        var go = new GameObject(name); go.transform.SetParent(p, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        var img = go.AddComponent<Image>(); img.color = col;
        img.raycastTarget = col.a > 0.01f;
        return go;
    }

    // 고정 크기 사각형
    static GameObject Rect(Transform p, string name, Vector2 pos, Vector2 size, Color col)
    {
        var go = new GameObject(name); go.transform.SetParent(p, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchoredPosition = pos; rt.sizeDelta = size;
        go.AddComponent<Image>().color = col;
        return go;
    }

    // 얇은 구분선/악센트 바
    static void AccentBar(Transform p, Vector2 pos, Vector2 size, Color col)
    {
        var go = new GameObject("AccentBar"); go.transform.SetParent(p, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchoredPosition = pos; rt.sizeDelta = size;
        go.AddComponent<Image>().color = col;
    }

    // 테두리선 (4개 얇은 Image 로 카드 테두리)
    static void RoundBorder(Transform card, Color col, float thickness)
    {
        void Edge(string n, Vector2 anchor0, Vector2 anchor1, Vector2 oMin, Vector2 oMax)
        {
            var g = new GameObject(n); g.transform.SetParent(card, false);
            var r = g.AddComponent<RectTransform>();
            r.anchorMin = anchor0; r.anchorMax = anchor1;
            r.offsetMin = oMin; r.offsetMax = oMax;
            g.AddComponent<Image>().color = col;
        }
        float t = thickness;
        Edge("B_Top",    new Vector2(0,1), new Vector2(1,1), new Vector2(0,-t), new Vector2(0,0));
        Edge("B_Bot",    new Vector2(0,0), new Vector2(1,0), new Vector2(0,0),  new Vector2(0,t));
        Edge("B_Left",   new Vector2(0,0), new Vector2(0,1), new Vector2(0,0),  new Vector2(t,0));
        Edge("B_Right",  new Vector2(1,0), new Vector2(1,1), new Vector2(-t,0), new Vector2(0,0));
    }

    // 레이블
    static TMP_Text Label(Transform p, string name, string txt, int fs, Vector2 pos, Vector2 size)
    {
        var go = new GameObject(name); go.transform.SetParent(p, false);
        var rt = go.AddComponent<RectTransform>(); rt.anchoredPosition = pos; rt.sizeDelta = size;
        var t  = go.AddComponent<TextMeshProUGUI>();
        if (_font != null) t.font = _font;
        t.text = txt; t.fontSize = fs;
        t.alignment = TextAlignmentOptions.Center;
        t.color = Color.white;
        return t;
    }

    // 일반 버튼 (텍스트만)
    static Button Btn(Transform p, string name, string label, Vector2 pos, Vector2 size, Color col)
    {
        var go  = new GameObject(name); go.transform.SetParent(p, false);
        var rt  = go.AddComponent<RectTransform>(); rt.anchoredPosition = pos; rt.sizeDelta = size;
        var img = go.AddComponent<Image>(); img.color = col;
        var btn = go.AddComponent<Button>();
        var cb  = btn.colors;
        cb.highlightedColor = new Color(
            Mathf.Min(col.r + 0.18f, 1), Mathf.Min(col.g + 0.18f, 1), Mathf.Min(col.b + 0.18f, 1));
        cb.pressedColor  = new Color(col.r * 0.7f, col.g * 0.7f, col.b * 0.7f);
        cb.fadeDuration  = 0.07f;
        btn.colors = cb;

        var tgo = new GameObject("L"); tgo.transform.SetParent(go.transform, false);
        var trt = tgo.AddComponent<RectTransform>();
        trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one;
        trt.offsetMin = trt.offsetMax = Vector2.zero;
        var tmp = tgo.AddComponent<TextMeshProUGUI>();
        if (_font != null) tmp.font = _font;
        tmp.text      = label;
        tmp.fontSize  = 38;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color     = Color.white;
        return btn;
    }

    // 큰 아이콘+텍스트 전투 버튼
    static Button BigBtn(Transform p, string name, string icon, string txt,
                         Vector2 pos, Vector2 size, Color col)
    {
        var go  = new GameObject(name); go.transform.SetParent(p, false);
        var rt  = go.AddComponent<RectTransform>(); rt.anchoredPosition = pos; rt.sizeDelta = size;
        var img = go.AddComponent<Image>(); img.color = col;
        var btn = go.AddComponent<Button>();
        var cb  = btn.colors;
        cb.highlightedColor = new Color(
            Mathf.Min(col.r+0.2f,1), Mathf.Min(col.g+0.2f,1), Mathf.Min(col.b+0.2f,1));
        cb.pressedColor = new Color(col.r*0.65f, col.g*0.65f, col.b*0.65f);
        cb.fadeDuration = 0.07f;
        btn.colors = cb;

        // 아이콘 (상단)
        var igo = new GameObject("Icon"); igo.transform.SetParent(go.transform, false);
        var irt = igo.AddComponent<RectTransform>();
        irt.anchorMin = new Vector2(0,0.42f); irt.anchorMax = new Vector2(1,1f);
        irt.offsetMin = irt.offsetMax = Vector2.zero;
        var itmp = igo.AddComponent<TextMeshProUGUI>();
        if (_font != null) itmp.font = _font;
        itmp.text = icon; itmp.fontSize = 42;
        itmp.alignment = TextAlignmentOptions.Center;
        itmp.color = Color.white;

        // 레이블 (하단)
        var lgo = new GameObject("L"); lgo.transform.SetParent(go.transform, false);
        var lrt = lgo.AddComponent<RectTransform>();
        lrt.anchorMin = new Vector2(0,0); lrt.anchorMax = new Vector2(1,0.44f);
        lrt.offsetMin = lrt.offsetMax = Vector2.zero;
        var ltmp = lgo.AddComponent<TextMeshProUGUI>();
        if (_font != null) ltmp.font = _font;
        ltmp.text      = txt;
        ltmp.fontSize  = 30;
        ltmp.fontStyle = FontStyles.Bold;
        ltmp.alignment = TextAlignmentOptions.Center;
        ltmp.color     = Color.white;
        return btn;
    }

    // 원형 버튼 (일시정지 등)
    static Button CircleBtn(Transform p, string name, string icon,
                             Vector2 pos, float sz, Color col)
    {
        var go  = new GameObject(name); go.transform.SetParent(p, false);
        var rt  = go.AddComponent<RectTransform>();
        rt.anchoredPosition = pos; rt.sizeDelta = new Vector2(sz, sz);
        var img = go.AddComponent<Image>(); img.color = col;
        var btn = go.AddComponent<Button>();

        var tgo = new GameObject("L"); tgo.transform.SetParent(go.transform, false);
        var trt = tgo.AddComponent<RectTransform>();
        trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one;
        trt.offsetMin = trt.offsetMax = Vector2.zero;
        var tmp = tgo.AddComponent<TextMeshProUGUI>();
        if (_font != null) tmp.font = _font;
        tmp.text      = icon;
        tmp.fontSize  = 36;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color     = Color.white;
        return btn;
    }

    // 작은 버튼
    static Button SmallBtn(Transform p, string name, string label,
                            Vector2 pos, Vector2 size, Color col)
    {
        var go  = new GameObject(name); go.transform.SetParent(p, false);
        var rt  = go.AddComponent<RectTransform>(); rt.anchoredPosition = pos; rt.sizeDelta = size;
        go.AddComponent<Image>().color = col;
        var btn = go.AddComponent<Button>();

        var tgo = new GameObject("L"); tgo.transform.SetParent(go.transform, false);
        var trt = tgo.AddComponent<RectTransform>();
        trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one;
        trt.offsetMin = trt.offsetMax = Vector2.zero;
        var tmp = tgo.AddComponent<TextMeshProUGUI>();
        if (_font != null) tmp.font = _font;
        tmp.text      = label;
        tmp.fontSize  = 24;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color     = Color.white;
        return btn;
    }

    // 쿨다운 오버레이
    static Image CooldownOverlay(GameObject parent)
    {
        var go  = new GameObject("CooldownOverlay"); go.transform.SetParent(parent.transform, false);
        var rt  = go.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        var img = go.AddComponent<Image>();
        img.color       = new Color(0, 0, 0, 0.58f);
        img.type        = Image.Type.Filled;
        img.fillMethod  = Image.FillMethod.Vertical;
        img.fillOrigin  = (int)Image.OriginVertical.Bottom;
        img.fillAmount  = 0f;
        img.raycastTarget = false;
        return img;
    }

    // 파워업 인디케이터
    static GameObject PowerUpIndicator(Transform parent, string name,
                                        string icon, Color bgCol, Vector2 pos)
    {
        var go  = new GameObject(name); go.transform.SetParent(parent, false);
        var rt  = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0, 0);
        rt.anchoredPosition = pos; rt.sizeDelta = new Vector2(210, 60);
        go.AddComponent<Image>().color = bgCol;

        // 아이콘
        var iconGO  = new GameObject("Icon"); iconGO.transform.SetParent(go.transform, false);
        var iconRT  = iconGO.AddComponent<RectTransform>();
        iconRT.anchorMin = new Vector2(0,0); iconRT.anchorMax = new Vector2(0,1);
        iconRT.offsetMin = new Vector2(4,4); iconRT.offsetMax = new Vector2(56,-4);
        var iconTMP = iconGO.AddComponent<TextMeshProUGUI>();
        if (_font != null) iconTMP.font = _font;
        iconTMP.text = icon; iconTMP.fontSize = 28;
        iconTMP.alignment = TextAlignmentOptions.Center;
        iconTMP.color = Color.white;

        // 필 바 배경
        var barBG   = new GameObject("BarBG"); barBG.transform.SetParent(go.transform, false);
        var barBGRT = barBG.AddComponent<RectTransform>();
        barBGRT.anchorMin = new Vector2(0,0); barBGRT.anchorMax = new Vector2(1,1);
        barBGRT.offsetMin = new Vector2(62,14); barBGRT.offsetMax = new Vector2(-8,-14);
        barBG.AddComponent<Image>().color = new Color(0,0,0,0.45f);

        // 필 바
        var bar    = new GameObject("FillBar"); bar.transform.SetParent(go.transform, false);
        var barRT  = bar.AddComponent<RectTransform>();
        barRT.anchorMin = new Vector2(0,0); barRT.anchorMax = new Vector2(1,1);
        barRT.offsetMin = new Vector2(62,14); barRT.offsetMax = new Vector2(-8,-14);
        var barImg = bar.AddComponent<Image>();
        barImg.color      = Color.white;
        barImg.type       = Image.Type.Filled;
        barImg.fillMethod = Image.FillMethod.Horizontal;
        barImg.fillOrigin = (int)Image.OriginHorizontal.Left;
        barImg.fillAmount = 1f;
        barImg.raycastTarget = false;

        return go;
    }

    // 스크롤뷰 (SkinSelectController 포함)
    // useAnchors=true 이면 pos/size 무시하고 하단 81% 앵커로 꽉 채움
    static GameObject ScrollView(Transform p, Vector2 pos, Vector2 size, bool useAnchors = false)
    {
        var sv   = new GameObject("SkinScrollView"); sv.transform.SetParent(p, false);
        var svRT = sv.AddComponent<RectTransform>();
        if (useAnchors)
        {
            svRT.anchorMin = new Vector2(0f, 0f); svRT.anchorMax = new Vector2(1f, 0.81f);
            svRT.offsetMin = new Vector2(10f, 8f); svRT.offsetMax = new Vector2(-10f, -4f);
        }
        else { svRT.anchoredPosition = pos; svRT.sizeDelta = size; }
        sv.AddComponent<Image>().color = new Color(0.05f, 0.04f, 0.14f, 0.85f);
        var sr   = sv.AddComponent<ScrollRect>(); sr.horizontal = false;

        var vp   = new GameObject("Viewport"); vp.transform.SetParent(sv.transform, false);
        var vpRT = vp.AddComponent<RectTransform>();
        vpRT.anchorMin = Vector2.zero; vpRT.anchorMax = Vector2.one;
        vpRT.offsetMin = vpRT.offsetMax = Vector2.zero;
        vp.AddComponent<Image>().color = new Color(0,0,0,0.01f);
        vp.AddComponent<Mask>().showMaskGraphic = false;

        var ct   = new GameObject("Content"); ct.transform.SetParent(vp.transform, false);
        var ctRT = ct.AddComponent<RectTransform>();
        ctRT.anchorMin = new Vector2(0f,1f); ctRT.anchorMax = new Vector2(1f,1f);
        ctRT.pivot     = new Vector2(0.5f,1f);
        ctRT.offsetMin = ctRT.offsetMax = Vector2.zero;

        sr.viewport          = vpRT;
        sr.content           = ctRT;
        sr.scrollSensitivity = 40f;
        sr.movementType      = ScrollRect.MovementType.Elastic;
        sr.elasticity        = 0.1f;
        sr.inertia           = true;
        sr.decelerationRate  = 0.135f;

        sv.AddComponent<SkinSelectController>();
        return sv;
    }
}
