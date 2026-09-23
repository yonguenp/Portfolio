using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;

/// <summary>
/// ui.md("고스톱 게임 UI 리디자인 및 Unity GameObject·Prefab 구조 개편 통합 기획서")의
/// 색상/레이아웃 방향("Modern Traditional Go-Stop")을 실제로 적용했을 때 어떤 느낌인지
/// 확인하기 위한 순수 시각 목업이다.
///
/// - 기존 GoStop3PScene/GoStop3PGame.cs 등 프로덕션 씬·스크립트는 전혀 건드리지 않는다.
/// - 완전히 새로운 씬(Assets/Scenes/GoStopOrientalMockup.unity)에 정적 GameObject로만
///   구성한다 — 런타임 스크립트/상호작용 전혀 없음(버튼 클릭도 안 먹는다), 순수 레이아웃/
///   색상 확인용.
/// - 카드 이미지는 기존 Resources/Hwatu 실제 스프라이트를 그대로 재사용한다.
/// - 필드/뒷패뭉치/필드패/유저별 상태바/Cap(획득패)/Back(상대 손패 뒷면)/샘플 팝업(승리
///   모달) 구성을 담는다.
///
/// 메뉴(Tools → GoStop Mockup → Build Oriental Layout Mockup)에서 재실행하면 매번
/// 새로 생성한다(멱등) — 위 상수(색상/크기)를 바꾼 뒤 다시 실행해서 바로 비교해볼 수 있다.
/// </summary>
public static class GoStopOrientalMockupBuilder
{
    // ── 디자인 토큰 (ui.md §6, 색상 시스템) ──
    public static readonly Color DeepGreen     = Hex("#24452F"); // 테이블 배경
    public static readonly Color DarkGreen     = Hex("#193523"); // 중앙 필드/어두운 표면
    public static readonly Color WarmCream     = Hex("#F3EBDD"); // 플레이어 패널/모달 표면
    public static readonly Color CreamWhite    = Hex("#FFFDF8"); // 밝은 텍스트/카드 프레임
    public static readonly Color HwatuRed      = Hex("#C93A32"); // Primary 액션/승리
    public static readonly Color Gold          = Hex("#D5A43A"); // 현재 턴/선택/보상
    public static readonly Color TextPrimary   = Hex("#20251F");
    public static readonly Color TextSecondary = Hex("#687066");
    public static readonly Color CardBackMaroon= Hex("#5C1A1A"); // 카드 뒷면(전통 화투 뒷면 톤)

    const string ArtDir = "Assets/Art/Mockup/OrientalUI";
    const string ScenePath = "Assets/Scenes/GoStopOrientalMockup.unity";
    const string FontPath = "TextMesh Pro/Fonts/GmarketSans SDF Medium";
    const string FontBoldPath = "TextMesh Pro/Fonts/GmarketSans SDF Bold";

    public static TMP_FontAsset _font;
    public static TMP_FontAsset Font => _font != null ? _font : (_font = Resources.Load<TMP_FontAsset>(FontPath));
    public static TMP_FontAsset _fontBold;
    public static TMP_FontAsset FontBold => _fontBold != null ? _fontBold : (_fontBold = Resources.Load<TMP_FontAsset>(FontBoldPath));

    /// <summary>
    /// 메뉴에서 한 번에 실행할 때(진짜 사용자 클릭, 파이프라인 eval의 5초 응답
    /// 제한과 무관)를 위한 편의 래퍼 — 스프라이트 생성 + 씬 구성을 순서대로 한다.
    /// 파이프라인 eval로 자동화할 때는 <see cref="BuildSprites"/>와
    /// <see cref="BuildScene"/>을 별도 호출로 나눠 쓸 것(둘 다 합치면 5초
    /// 타임아웃에 걸린다 — 실제로 겪은 문제).
    /// </summary>
    [MenuItem("Tools/GoStop Mockup/Build Oriental Layout Mockup (전체)")]
    public static void Build()
    {
        BuildSprites();
        BuildScene();
    }

    /// <summary>1단계 — 스프라이트 12종을 생성해 실제 에셋으로 저장한다(씬은 안 건드림).</summary>
    [MenuItem("Tools/GoStop Mockup/1. Build Sprites")]
    public static void BuildSprites()
    {
        EnsureFolder(ArtDir);

        AssetDatabase.StartAssetEditing();
        try
        {
            // 9-slice라 텍스처 자체는 작게(코너 반경+테두리만 표현) — 화면에는
            // Image.Type.Sliced가 그대로 원하는 최종 크기로 늘려 그린다.
            // (처음에 최종 화면 크기 그대로 큰 텍스처를 만들었다가 픽셀 루프+에셋
            // 저장 용량이 커져 파이프라인 eval의 5초 제한에 걸렸다 — 실제로 겪은 문제)
            SaveSprite("panel_cream",      RoundedRect(96, 96, 20, WarmCream, 2, DarkGreen, 0.30f), 24);
            SaveSprite("panel_cream_gold", RoundedRect(96, 96, 20, WarmCream, 4, Gold, 1f), 24);
            SaveSprite("panel_dark",       RoundedRect(112, 112, 24, DarkGreen, 3, DeepGreen, 0.45f), 28);
            SaveSprite("card_frame",       RoundedRect(72, 100, 8, CreamWhite, 0, Color.clear, 0f), 10);
            SaveSprite("card_frame_gold",  RoundedRect(72, 100, 8, CreamWhite, 4, Gold, 1f), 12);
            SaveSprite("card_back",        RoundedRect(72, 100, 10, CardBackMaroon, 6, Gold, 1f), 16);
            SaveSprite("btn_primary",      RoundedRect(96, 48, 10, HwatuRed, 0, Color.clear, 0f), 12);
            SaveSprite("btn_secondary",    RoundedRect(80, 48, 8, WarmCream, 2, DarkGreen, 0.5f), 10);
            SaveSprite("modal_bg",         RoundedRect(128, 128, 26, WarmCream, 3, Gold, 1f), 30);
            SaveSprite("gold_glow",        RoundedRect(160, 160, 40, Gold, 0, Color.clear, 0f, 0.25f, softness: 30f), 44);
            SaveSprite("count_badge",      RoundedRect(64, 40, 10, WarmCream, 2, DarkGreen, 0.35f), 12);
            SaveSprite("lattice_tile",     LatticeTile(64), 0, wrap: TextureWrapMode.Repeat);

            // 선/광박·멍박·피박/흔들기·뻑 배지용 원형 칩 3종(상태별로 색을 미리 구웠다 —
            // 원본이 회색조가 아니라 이미 색이 있는 작은 칩이라 런타임 틴트 대신
            // 상태마다 별도 스프라이트를 쓰는 쪽을 택했다).
            SaveSprite("badge_dim",  RoundedRect(48, 48, 24, WarmCream, 3, DarkGreen, 0.30f), 24);
            SaveSprite("badge_gold", RoundedRect(48, 48, 24, Gold, 3, DarkGreen, 0.30f), 24);
            SaveSprite("badge_red",  RoundedRect(48, 48, 24, HwatuRed, 3, DarkGreen, 0.30f), 24);
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
        }
        AssetDatabase.SaveAssets();
        Logger.Log("[OrientalMockup] 스프라이트 12개 생성 완료: " + ArtDir);
    }

    public static Sprite LoadSprite(string name) => AssetDatabase.LoadAssetAtPath<Sprite>($"{ArtDir}/{name}.asset");

    /// <summary>2단계 — BuildSprites()가 이미 저장해 둔 스프라이트를 불러와 새 씬을 구성한다.</summary>
    [MenuItem("Tools/GoStop Mockup/2. Build Scene (스프라이트 먼저 생성했어야 함)")]
    public static void BuildScene()
    {
        var sprPanelCream      = LoadSprite("panel_cream");
        var sprPanelCreamGold  = LoadSprite("panel_cream_gold");
        var sprPanelDark       = LoadSprite("panel_dark");
        var sprCardFrame       = LoadSprite("card_frame");
        var sprCardFrameGold   = LoadSprite("card_frame_gold");
        var sprCardBack        = LoadSprite("card_back");
        var sprButtonPrimary   = LoadSprite("btn_primary");
        var sprButtonSecondary = LoadSprite("btn_secondary");
        var sprModal           = LoadSprite("modal_bg");
        var sprGlow            = LoadSprite("gold_glow");
        var sprBadge           = LoadSprite("count_badge");
        var sprLattice         = LoadSprite("lattice_tile");
        var sprBadgeDim        = LoadSprite("badge_dim");
        var sprBadgeGold       = LoadSprite("badge_gold");
        var sprBadgeRed        = LoadSprite("badge_red");
        if (sprPanelCream == null || sprPanelDark == null || sprModal == null)
        {
            Debug.LogError("[OrientalMockup] 스프라이트를 못 찾았다 — 먼저 BuildSprites()를 실행할 것.");
            return;
        }

        // 안전장치 — 지금 열려 있는 씬(사용자가 에디터에서 직접 작업 중이던 씬일 수
        // 있다)에 저장 안 된 변경이 있으면 새 씬으로 갈아치우기 전에 먼저 저장해서
        // 절대 유실되지 않게 한다.
        var current = EditorSceneManager.GetActiveScene();
        string previousScenePath = current.path;
        if (current.isDirty && !string.IsNullOrEmpty(current.path))
        {
            Debug.LogWarning("[OrientalMockup] 현재 씬에 저장되지 않은 변경사항이 있어 먼저 저장합니다: " + current.path);
            EditorSceneManager.SaveScene(current);
        }

        // ── 새 씬 ──
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        var camGo = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
        var cam = camGo.GetComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = DeepGreen;
        cam.orthographic = true;
        camGo.tag = "MainCamera";

        new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));

        var canvasGo = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
        var root = canvasGo.GetComponent<RectTransform>();

        // ── 배경 ──
        AddImage(MakeRect(root, "Background", Vector2.zero, Vector2.one, Half, Vector2.zero, Vector2.zero), null, DeepGreen);
        var latticeImg = AddImage(MakeRect(root, "BackgroundPattern", Vector2.zero, Vector2.one, Half, Vector2.zero, Vector2.zero), sprLattice, Color.white, Image.Type.Tiled);

        var table = MakeRect(root, "GameTable", Vector2.zero, Vector2.one, Half, Vector2.zero, Vector2.zero);

        // ── Field (필드영역) ──
        var fieldRt = MakeRect(table, "Field", Half, Half, Half, new Vector2(1000, 440), new Vector2(0, 10));
        AddImage(fieldRt, sprPanelDark, Color.white);

        // 뒷패뭉치 (DrawPile)
        var pileRoot = MakeRect(fieldRt, "DrawPile", Half, Half, Half, new Vector2(140, 190), new Vector2(-330, 110));
        for (int i = 0; i < 5; i++)
        {
            var c = MakeRect(pileRoot, "PileCard" + i, Half, Half, Half, new Vector2(110, 150), new Vector2(-i * 3f, i * 3f));
            AddImage(c, sprCardBack, Color.white);
        }
        var pileBadge = MakeRect(pileRoot, "CountBadge", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(64, 40), new Vector2(0, -34));
        AddImage(pileBadge, sprBadge, Color.white);
        AddLabel(SubRect(pileBadge, Vector2.zero, Vector2.one), "22", 24, TextPrimary, TextAlignmentOptions.Center, FontStyles.Bold);

        // 필드패 (FieldCards)
        string[] fieldCards = { "January_Hikari", "March_Tanzaku", "June_Tane", "August_Hikari", "September_Tanzaku", "November_Kasu_2" };
        var fieldCardsRoot = MakeRect(fieldRt, "FieldCards", Half, Half, Half, new Vector2(760, 380), new Vector2(90, 10));
        PlaceGrid(fieldCardsRoot, fieldCards, 3, 130, 180, 24, sprCardFrame);

        // ── 상단 좌석: AI-B ──
        var topStatus = BuildStatusBar(table, "Seat_Top", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -16),
            new Vector2(480, 165), "AI-B", "101,200원", "0점", 1, 0, 3, false,
            new BadgeState { gwangBak = true, shakeCount = 1 }, sprPanelCream, sprPanelCreamGold, sprGlow, sprBadgeDim, sprBadgeGold, sprBadgeRed);
        var topBack = MakeRect(table, "Seat_Top_Back", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(220, 70), new Vector2(-130, -185));
        PlaceBackRow(topBack, 7, sprCardBack, 1f);
        var topCap = MakeRect(table, "Seat_Top_Cap", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(260, 115), new Vector2(130, -185));
        CapStack(topCap, -60, 0, new[] { "August_Hikari", "July_Tane", "July_Tanzaku" }, sprCardFrame);
        CapStack(topCap, 60, 0, new[] { "July_Kasu_1", "July_Kasu_2" }, sprCardFrame);

        // ── 좌측 좌석: AI-A ──
        var leftStatus = BuildStatusBar(table, "Seat_Left", new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(16, 90),
            new Vector2(380, 165), "AI-A", "96,350원", "0점", 0, 2, 5, false,
            new BadgeState { meongBak = true, ppeokCount = 1 }, sprPanelCream, sprPanelCreamGold, sprGlow, sprBadgeDim, sprBadgeGold, sprBadgeRed);
        var leftBack = MakeRect(table, "Seat_Left_Back", new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(300, 70), new Vector2(16, -40));
        PlaceBackRow(leftBack, 7, sprCardBack, 0.85f);
        var leftCap = MakeRect(table, "Seat_Left_Cap", new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(300, 160), new Vector2(16, -165));
        CapStack(leftCap, -60, 0, new[] { "September_Tane", "September_Tanzaku", "May_Tanzaku" }, sprCardFrame);
        CapStack(leftCap, 60, 0, new[] { "September_Kasu_1", "September_Kasu_2", "May_Kasu_1" }, sprCardFrame);

        // ── 우측 좌석: AI-C ──
        var rightStatus = BuildStatusBar(table, "Seat_Right", new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-16, 90),
            new Vector2(380, 165), "AI-C", "100,000원", "0점", 1, 1, 2, false,
            new BadgeState { piBak = true }, sprPanelCream, sprPanelCreamGold, sprGlow, sprBadgeDim, sprBadgeGold, sprBadgeRed);
        var rightBack = MakeRect(table, "Seat_Right_Back", new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(300, 70), new Vector2(-16, -40));
        PlaceBackRow(rightBack, 7, sprCardBack, 0.85f);
        var rightCap = MakeRect(table, "Seat_Right_Cap", new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(300, 160), new Vector2(-16, -165));
        CapStack(rightCap, -60, 0, new[] { "November_Hikari", "October_Tanzaku" }, sprCardFrame);
        CapStack(rightCap, 60, 0, new[] { "October_Kasu_1", "October_Kasu_2", "May_Kasu_2" }, sprCardFrame);

        // ── 하단 좌석: 나 (현재 턴 강조, 선까지 겸함) ──
        var meStatus = BuildStatusBar(table, "Seat_Bottom_Me", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-260, 284),
            new Vector2(560, 165), "나", "99,800원", "3점 · 고 1회", 2, 1, 6, true,
            new BadgeState { dealer = true, shakeCount = 2, ppeokCount = 1 }, sprPanelCream, sprPanelCreamGold, sprGlow, sprBadgeDim, sprBadgeGold, sprBadgeRed);
        var meCap = MakeRect(table, "Seat_Bottom_Cap", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(420, 165), new Vector2(340, 284));
        // 광 | 열끗(위)+띠(아래) | 피 — 3존, 여유가 가장 넉넉한 내 Cap에만 전체 구성을 보여준다.
        CapStack(meCap, -140, 0, new[] { "January_Hikari", "March_Hikari" }, sprCardFrame);
        CapStack(meCap, 0, 40, new[] { "June_Tane" }, sprCardFrame);
        CapStack(meCap, 0, -40, new[] { "January_Tanzaku", "February_Tanzaku" }, sprCardFrame);
        CapStack(meCap, 140, 0, new[] { "January_Kasu_1", "June_Kasu_2", "February_Kasu_1", "February_Kasu_2" }, sprCardFrame);

        // 손패 (Hand) — 카드 한 장 선택 상태 표시
        string[] hand = { "February_Tanzaku", "April_Kasu_1", "May_Tane", "July_Kasu_2", "October_Tane", "December_Hikari", "March_Kasu_1" };
        var handRt = MakeRect(table, "Hand", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 0), new Vector2(1600, 270), Vector2.zero);
        PlaceHand(handRt, hand, hand.Length - 1, sprCardFrame, sprCardFrameGold);

        // ── 샘플 팝업 (ResultModal, 기본 비활성 — Hierarchy에서 체크박스 켜서 미리보기) ──
        var popupRoot = MakeRect(root, "Popup_SampleResultModal", Vector2.zero, Vector2.one, Half, Vector2.zero, Vector2.zero);
        AddImage(popupRoot, null, new Color(0, 0, 0, 0.65f));
        var modalRt = MakeRect(popupRoot, "Modal", Half, Half, Half, new Vector2(760, 620), Vector2.zero);
        AddImage(modalRt, sprModal, Color.white);

        AddLabel(MakeRect(modalRt, "Title", new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1f), new Vector2(-80, 60), new Vector2(0, -70)),
            "AI-C 승리", 40, TextPrimary, TextAlignmentOptions.Center, FontStyles.Bold);
        AddLabel(MakeRect(modalRt, "Score", new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1f), new Vector2(-80, 80), new Vector2(0, -150)),
            "7점", 56, HwatuRed, TextAlignmentOptions.Center, FontStyles.Bold);
        AddLabel(MakeRect(modalRt, "Sub", new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1f), new Vector2(-80, 40), new Vector2(0, -250)),
            "내 머니 100,000원", 24, TextSecondary, TextAlignmentOptions.Center);

        var primaryBtn = MakeRect(modalRt, "PrimaryBtn", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(420, 96), new Vector2(0, 150));
        AddImage(primaryBtn, sprButtonPrimary, Color.white);
        AddLabel(SubRect(primaryBtn, Vector2.zero, Vector2.one), "다시 시작", 28, CreamWhite, TextAlignmentOptions.Center, FontStyles.Bold);

        var secondaryBtn = MakeRect(modalRt, "SecondaryBtn", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(260, 72), new Vector2(-150, 60));
        AddImage(secondaryBtn, sprButtonSecondary, Color.white);
        AddLabel(SubRect(secondaryBtn, Vector2.zero, Vector2.one), "점수 상세", 22, TextPrimary, TextAlignmentOptions.Center);

        var tertiaryBtn = MakeRect(modalRt, "TertiaryBtn", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(200, 72), new Vector2(150, 60));
        AddLabel(tertiaryBtn, "타이틀", 22, TextSecondary, TextAlignmentOptions.Center);

        popupRoot.gameObject.SetActive(false);

        // ── 겹침 자동 점검 (이 프로젝트의 확립된 "실측 검증" 원칙 — 손으로 계산한
        // 좌표를 그대로 믿지 않고 실제 월드 좌표로 재확인한다) ──
        Check("Field vs Seat_Top_Back", fieldRt, topBack);
        Check("Field vs Seat_Top_Cap", fieldRt, topCap);
        Check("Field vs Seat_Left_Cap", fieldRt, leftCap);
        Check("Field vs Seat_Right_Cap", fieldRt, rightCap);
        Check("Field vs Seat_Bottom_Me(Status)", fieldRt, meStatus);
        Check("Field vs Seat_Bottom_Cap", fieldRt, meCap);
        Check("Seat_Bottom_Me(Status) vs Seat_Bottom_Cap", meStatus, meCap);
        Check("Seat_Bottom_Me(Status) vs Hand", meStatus, handRt);
        Check("Seat_Bottom_Cap vs Hand", meCap, handRt);
        Check("Seat_Left_Cap vs Seat_Bottom_Me(Status)", leftCap, meStatus);
        Check("Seat_Right_Cap vs Seat_Bottom_Cap", rightCap, meCap);
        Check("Seat_Top_Back vs Seat_Top_Cap", topBack, topCap);
        Check("Seat_Left_Status vs Seat_Left_Back", leftStatus, leftBack);
        Check("Seat_Left_Back vs Seat_Left_Cap", leftBack, leftCap);
        Check("Seat_Right_Status vs Seat_Right_Back", rightStatus, rightBack);
        Check("Seat_Right_Back vs Seat_Right_Cap", rightBack, rightCap);

        EnsureFolder("Assets/Scenes");
        EditorSceneManager.SaveScene(scene, ScenePath);
        Logger.Log("[OrientalMockup] 완료: " + ScenePath);

        // 사용자가 원래 열어두고 있던(또는 마지막으로 저장했던) 씬으로 복귀
        if (!string.IsNullOrEmpty(previousScenePath) && previousScenePath != ScenePath)
        {
            EditorSceneManager.OpenScene(previousScenePath, OpenSceneMode.Single);
            Logger.Log("[OrientalMockup] 원래 씬으로 복귀: " + previousScenePath);
        }
    }

    // ───────────────────────── 상태바(PlayerPanel) ─────────────────────────

    /// <summary>선/광박·멍박·피박/흔들기·뻑 표시 상태 — 프로덕션
    /// GoStopStatusBoxView의 6칸 배지(선, 광박/멍박/피박, 흔들기 카운트, 뻑 카운트)와
    /// 같은 정보를 새 색상 언어로 옮긴 것. 흔들기/뻑은 최대 2칸 카운트 도트로 표시.</summary>
    public struct BadgeState
    {
        public bool dealer, gwangBak, meongBak, piBak;
        public int shakeCount, ppeokCount;
    }

    public static RectTransform BuildStatusBar(RectTransform parent, string name, Vector2 anchor, Vector2 pivot, Vector2 pos,
        Vector2 size, string displayName, string money, string scoreLine, int gwang, int meong, int pi,
        bool highlighted, BadgeState badges,
        Sprite panelCream, Sprite panelCreamGold, Sprite glow, Sprite badgeDim, Sprite badgeGold, Sprite badgeRed)
    {
        if (highlighted)
        {
            // 사용자가 에디터에서 직접 맞춘 값(패널보다 가로 10 / 세로 5 크게) —
            // 원래 +60/+60은 패널 테두리와 너무 떨어져 보였다.
            var glowRt = MakeRect(parent, name + "_Glow", anchor, anchor, pivot, size + new Vector2(10, 5), pos);
            AddImage(glowRt, glow, Color.white);
        }

        var panelRt = MakeRect(parent, name + "_StatusBar", anchor, anchor, pivot, size, pos);
        AddImage(panelRt, highlighted ? panelCreamGold : panelCream, Color.white);

        float pad = 26f;
        var headerRt = MakeRect(panelRt, "Header", new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1f), new Vector2(-pad * 2, 46), new Vector2(0, -16));
        AddLabel(SubRect(headerRt, new Vector2(0, 0), new Vector2(0.55f, 1)), displayName, 30, TextPrimary, TextAlignmentOptions.Left, FontStyles.Bold);
        AddLabel(SubRect(headerRt, new Vector2(0.45f, 0), new Vector2(1, 1)), money, 24, TextPrimary, TextAlignmentOptions.Right);

        AddImage(MakeRect(panelRt, "Divider", new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1f), new Vector2(-pad * 2, 2), new Vector2(0, -64)),
            null, new Color(TextPrimary.r, TextPrimary.g, TextPrimary.b, 0.18f));

        // 점수 줄 — 배지 줄이 새로 생긴 만큼 패널 하단에서 위로 한 칸 밀어 올렸다.
        var scoreRowRt = MakeRect(panelRt, "ScoreRow", new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 0f), new Vector2(-pad * 2, 36), new Vector2(0, 58));
        AddLabel(SubRect(scoreRowRt, new Vector2(0, 0), new Vector2(0.5f, 1)), scoreLine, 22, TextSecondary, TextAlignmentOptions.Left);
        AddLabel(SubRect(scoreRowRt, new Vector2(0.5f, 0), new Vector2(1, 1)), $"광 {gwang} · 멍 {meong} · 피 {pi}", 20, TextSecondary, TextAlignmentOptions.Right);

        // 배지 줄 — 선 | 광박 멍박 피박 | 흔들기·뻑 카운트
        var badgeRowRt = MakeRect(panelRt, "BadgeRow", new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 0f), new Vector2(-pad * 2, 32), new Vector2(0, 14));
        // 배지 칩은 anchor/pivot이 (0, 0.5)(왼쪽 기준)이라 bx는 행의 왼쪽 끝(0)부터
        // 시작해야 한다 — 가운데 기준(-width/2)으로 잘못 시작해서 첫 칩이 패널
        // 밖으로 삐져나갔던 버그를 고쳤다.
        float bx = 0f;
        float chip = 30f, chipGap = 8f;

        void Chip(string label, bool active, bool alwaysVisible, Sprite activeSprite)
        {
            bool visible = active || alwaysVisible;
            var c = MakeRect(badgeRowRt, "Badge_" + label, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(chip, chip), new Vector2(bx, 0));
            c.gameObject.SetActive(visible);
            if (visible)
            {
                AddImage(c, active ? activeSprite : badgeDim, Color.white);
                AddLabel(SubRect(c, Vector2.zero, Vector2.one), label, 14, active ? CreamWhite : TextSecondary, TextAlignmentOptions.Center, FontStyles.Bold);
                bx += chip + chipGap;
            }
        }

        // 선(딜러)은 아닐 때 자리 자체를 접는다(칸을 안 남김) — 광/멍/피 위험은
        // 안전할 때도 회색 칩으로 항상 자리를 유지하고 색으로만 상태를 구분한다.
        Chip("선", badges.dealer, false, badgeGold);
        Chip("광", badges.gwangBak, true, badgeRed);
        Chip("멍", badges.meongBak, true, badgeRed);
        Chip("피", badges.piBak, true, badgeRed);

        void DotPair(string label, int count)
        {
            var labelRt = MakeRect(badgeRowRt, "Badge_" + label + "Label", new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(40, chip), new Vector2(bx, 0));
            AddLabel(labelRt, label, 16, TextSecondary, TextAlignmentOptions.Left);
            bx += 42f;
            for (int i = 0; i < 2; i++)
            {
                var dot = MakeRect(badgeRowRt, "Badge_" + label + "Dot" + i, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(18, 18), new Vector2(bx, 0));
                AddImage(dot, i < count ? badgeGold : badgeDim, Color.white);
                bx += 18f + 6f;
            }
            bx += chipGap;
        }

        DotPair("흔", badges.shakeCount);
        DotPair("뻑", badges.ppeokCount);

        return panelRt;
    }

    // ───────────────────────── 카드 배치 ─────────────────────────

    public static void PlaceGrid(RectTransform parent, string[] cardNames, int cols, float cardW, float cardH, float spacing, Sprite frame)
    {
        int n = cardNames.Length;
        int rows = Mathf.CeilToInt(n / (float)cols);
        float totalW = cols * cardW + (cols - 1) * spacing;
        float totalH = rows * cardH + (rows - 1) * spacing;
        float startX = -totalW / 2f + cardW / 2f;
        float startY = totalH / 2f - cardH / 2f;
        for (int i = 0; i < n; i++)
        {
            int col = i % cols, row = i / cols;
            float x = startX + col * (cardW + spacing);
            float y = startY - row * (cardH + spacing);
            var cardRt = MakeRect(parent, "Field_" + cardNames[i], Half, Half, Half, new Vector2(cardW, cardH), new Vector2(x, y));
            AddImage(cardRt, frame, Color.white);
            var art = MakeRect(cardRt, "Art", Vector2.zero, Vector2.one, Half, new Vector2(-10, -10), Vector2.zero);
            AddImage(art, LoadCard(cardNames[i]), Color.white, Image.Type.Simple);
        }
    }

    public static void PlaceHand(RectTransform parent, string[] cardNames, int selectedIndex, Sprite frame, Sprite frameGold)
    {
        int n = cardNames.Length;
        float cardW = 190, cardH = 270, spacing = 170;
        float totalW = (n - 1) * spacing + cardW;
        float startX = -totalW / 2f + cardW / 2f;
        for (int i = 0; i < n; i++)
        {
            float x = startX + i * spacing;
            bool sel = i == selectedIndex;
            float y = sel ? 34f : 0f;
            var cardRt = MakeRect(parent, "Hand_" + cardNames[i], new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(cardW, cardH), new Vector2(x, y));
            AddImage(cardRt, sel ? frameGold : frame, Color.white);
            var art = MakeRect(cardRt, "Art", Vector2.zero, Vector2.one, Half, new Vector2(-10, -10), Vector2.zero);
            AddImage(art, LoadCard(cardNames[i]), Color.white, Image.Type.Simple);
        }
    }

    /// <summary>획득패 한 무더기 — 카드가 (x, centerY)를 중심으로 세로로 살짝
    /// 겹쳐 쌓인다("게임판 위에 쌓인 카드처럼", 별도 배경 없음). 광/열끗/띠/피처럼
    /// Cap 영역 안에서 종류별로 나눠 표시할 때 이 함수를 여러 번 부른다.</summary>
    public static void CapStack(RectTransform parent, float x, float centerY, string[] cardNames, Sprite frame)
    {
        const float w = 30f, h = 42f, overlap = 14f;
        int n = cardNames.Length;
        if (n == 0) return;
        float totalH = h + (n - 1) * overlap;
        float startY = centerY + totalH / 2f - h / 2f;
        for (int i = 0; i < n; i++)
        {
            var c = MakeRect(parent, "Cap_" + cardNames[i], Half, Half, Half, new Vector2(w, h), new Vector2(x, startY - i * overlap));
            AddImage(c, frame, Color.white);
            var art = MakeRect(c, "Art", Vector2.zero, Vector2.one, Half, new Vector2(-4, -4), Vector2.zero);
            AddImage(art, LoadCard(cardNames[i]), Color.white, Image.Type.Simple);
        }
    }

    public static void PlaceBackRow(RectTransform parent, int count, Sprite cardBack, float scale)
    {
        float w = 46 * scale, h = 64 * scale, spacing = 26 * scale;
        float totalW = (count - 1) * spacing + w;
        float startX = -totalW / 2f + w / 2f;
        for (int i = 0; i < count; i++)
        {
            var c = MakeRect(parent, "Back_" + i, Half, Half, Half, new Vector2(w, h), new Vector2(startX + i * spacing, 0));
            AddImage(c, cardBack, Color.white);
        }
    }

    public static Sprite LoadCard(string name) => Resources.Load<Sprite>("Hwatu/" + name);

    // ───────────────────────── 겹침 점검 ─────────────────────────

    public static void Check(string label, RectTransform a, RectTransform b)
    {
        var ca = new Vector3[4]; a.GetWorldCorners(ca);
        var cb = new Vector3[4]; b.GetWorldCorners(cb);
        var ra = new Rect(ca[0].x, ca[0].y, ca[2].x - ca[0].x, ca[2].y - ca[0].y);
        var rb = new Rect(cb[0].x, cb[0].y, cb[2].x - cb[0].x, cb[2].y - cb[0].y);
        if (ra.Overlaps(rb))
            Debug.LogWarning($"[OrientalMockup] 겹침 감지: {label}");
    }

    // ───────────────────────── 저수준 GameObject/텍스처 헬퍼 ─────────────────────────

    public static readonly Vector2 Half = new Vector2(0.5f, 0.5f);

    public static Color Hex(string hex)
    {
        ColorUtility.TryParseHtmlString(hex, out var c);
        return c;
    }

    public static RectTransform MakeRect(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 sizeDelta, Vector2 anchoredPos)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rt = (RectTransform)go.transform;
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = pivot;
        rt.sizeDelta = sizeDelta;
        rt.anchoredPosition = anchoredPos;
        return rt;
    }

    public static RectTransform SubRect(RectTransform parent, Vector2 anchorMin, Vector2 anchorMax)
    {
        var go = new GameObject("Sub", typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rt = (RectTransform)go.transform;
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        rt.pivot = Half;
        return rt;
    }

    public static Image AddImage(RectTransform rt, Sprite sprite, Color color, Image.Type type = Image.Type.Sliced)
    {
        var img = rt.gameObject.AddComponent<Image>();
        img.sprite = sprite;
        img.color = color;
        img.type = sprite != null ? type : Image.Type.Simple;
        img.raycastTarget = false;
        return img;
    }

    public static TextMeshProUGUI AddLabel(RectTransform rt, string text, float size, Color color, TextAlignmentOptions align, FontStyles style = FontStyles.Normal)
    {
        var tmp = rt.gameObject.AddComponent<TextMeshProUGUI>();
        // Bold로 요청된 라벨은 합성(faux) 굵기 대신 실제 Bold 웨이트 폰트 에셋을 쓴다 —
        // 제목/이름처럼 굵게 강조돼야 하는 자리가 전부 FontStyles.Bold로 이미 표시돼
        // 있어서, 호출부를 안 건드리고 여기서만 골라 쓰면 된다.
        tmp.font = style == FontStyles.Bold ? FontBold : Font;
        tmp.text = text;
        tmp.fontSize = size;
        tmp.color = color;
        tmp.alignment = align;
        tmp.fontStyle = FontStyles.Normal; // 위에서 이미 실제 Bold 폰트를 골랐으므로 합성 볼드는 끈다
        tmp.textWrappingMode = TextWrappingModes.NoWrap;
        tmp.raycastTarget = false;
        return tmp;
    }

    public static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        string[] parts = path.Split('/');
        string cur = parts[0];
        for (int i = 1; i < parts.Length; i++)
        {
            string next = cur + "/" + parts[i];
            if (!AssetDatabase.IsValidFolder(next)) AssetDatabase.CreateFolder(cur, parts[i]);
            cur = next;
        }
    }

    public static Sprite SaveSprite(string name, Texture2D tex, int border, TextureWrapMode wrap = TextureWrapMode.Clamp)
    {
        string path = $"{ArtDir}/{name}.asset";
        if (AssetDatabase.LoadAssetAtPath<Texture2D>(path) != null) AssetDatabase.DeleteAsset(path);
        tex.name = name + "_tex";
        tex.wrapMode = wrap;
        tex.filterMode = FilterMode.Bilinear;
        AssetDatabase.CreateAsset(tex, path);
        var sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Half, 100f, 0, SpriteMeshType.FullRect,
            new Vector4(border, border, border, border));
        sprite.name = name;
        AssetDatabase.AddObjectToAsset(sprite, tex);
        return sprite;
    }

    // ── 둥근 사각형 SDF 텍스처 (테두리 유무 모두 지원) ──
    public static Texture2D RoundedRect(int w, int h, int radius, Color fill, int borderWidth, Color borderColor, float borderAlpha, float fillAlpha = 1f, float softness = 1f)
    {
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        var px = new Color[w * h];
        Color fillC = fill; fillC.a *= fillAlpha;
        Color borderC = borderColor; borderC.a *= borderAlpha;
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                float outerDist = SdRoundRect(x, y, w, h, radius, 0);
                float outerAA = Mathf.Clamp01(0.5f - outerDist / softness);
                Color c;
                if (borderWidth > 0)
                {
                    float innerDist = SdRoundRect(x, y, w, h, radius, borderWidth);
                    float innerAA = Mathf.Clamp01(0.5f - innerDist);
                    c = Color.Lerp(borderC, fillC, innerAA);
                }
                else
                {
                    c = fillC;
                }
                c.a *= outerAA;
                px[y * w + x] = c;
            }
        }
        tex.SetPixels(px);
        tex.Apply();
        return tex;
    }

    public static float SdRoundRect(float x, float y, int w, int h, int radius, int inset)
    {
        float px = x + 0.5f - w / 2f;
        float py = y + 0.5f - h / 2f;
        float hw = w / 2f - inset;
        float hh = h / 2f - inset;
        float r = Mathf.Max(0.5f, radius - inset);
        float qx = Mathf.Abs(px) - (hw - r);
        float qy = Mathf.Abs(py) - (hh - r);
        float ax = Mathf.Max(qx, 0f);
        float ay = Mathf.Max(qy, 0f);
        float outside = Mathf.Sqrt(ax * ax + ay * ay) - r;
        float inside = Mathf.Min(Mathf.Max(qx, qy), 0f);
        return outside + inside;
    }

    // ── 오리엔탈 격자무늬 배경 타일 (아주 옅은 대각선 격자, 데코용) ──
    public static Texture2D LatticeTile(int size)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        var px = new Color[size * size];
        Color clear = new Color(0, 0, 0, 0);
        for (int i = 0; i < px.Length; i++) px[i] = clear;
        Color line = DarkGreen; line.a = 0.12f;
        int thickness = 1;
        for (int x = 0; x < size; x++)
        {
            for (int t = -thickness; t <= thickness; t++)
            {
                SetPx(px, size, x, ((x + t) % size + size) % size, line);
                SetPx(px, size, x, ((size - 1 - x + t) % size + size) % size, line);
            }
        }
        tex.SetPixels(px);
        tex.Apply();
        return tex;
    }

    public static void SetPx(Color[] px, int size, int x, int y, Color c)
    {
        if (x >= 0 && x < size && y >= 0 && y < size) px[y * size + x] = c;
    }
}
