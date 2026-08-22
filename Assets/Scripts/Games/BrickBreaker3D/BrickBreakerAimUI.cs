using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.UI;
using Touch      = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

/// <summary>
/// 터치와 마우스를 하나의 "포인터"로 다루는 헬퍼.
///
/// 조이스틱 캔버스에도 조준 UI에도 GraphicRaycaster가 없다(HUD 버튼 레이캐스트를
/// 가로채지 않으려고). 그래서 모든 입력을 raw로 읽어야 하고, 패드 모드와
/// 터치 모드가 같은 규칙으로 포인터를 잡도록 여기에 모아둔다.
/// </summary>
public static class BrickBreakerPointer
{
    public const int None  = -1;
    public const int Mouse = -2;

    /// <summary>
    /// Touch.activeTouches는 EnhancedTouch가 켜져 있어야 한다. 안 켜져 있으면
    /// **매 프레임 InvalidOperationException**이 나고 로그가 폭주한다.
    /// 이 헬퍼를 쓰는 쪽(조이스틱/조준UI/매니저)이 각자 Enable했겠거니 하면 안 되고
    /// 여기서 직접 보장한다.
    /// </summary>
    static void EnsureTouch()
    {
        if (!EnhancedTouchSupport.enabled) EnhancedTouchSupport.Enable();
    }

    /// <summary>이번 프레임의 모든 포인터. began이면 이번 프레임에 눌리기 시작한 것.</summary>
    public static IEnumerable<(int id, Vector2 pos, bool began)> All()
    {
        EnsureTouch();
        foreach (var t in Touch.activeTouches)
            yield return (t.touchId, t.screenPosition, t.phase == TouchPhase.Began);

        var m = UnityEngine.InputSystem.Mouse.current;
        if (m != null && m.leftButton.isPressed)
            yield return (Mouse, m.position.ReadValue(), m.leftButton.wasPressedThisFrame);
    }

    /// <summary>아직 눌려 있으면 true. 뗐으면 false — 호출부는 이걸 "발사" 신호로 쓴다.</summary>
    public static bool TryGet(int id, out Vector2 pos)
    {
        if (id == Mouse)
        {
            var m = UnityEngine.InputSystem.Mouse.current;
            if (m != null && m.leftButton.isPressed) { pos = m.position.ReadValue(); return true; }
            pos = default; return false;
        }

        EnsureTouch();
        foreach (var t in Touch.activeTouches)
        {
            if (t.touchId != id) continue;
            if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled) break;
            pos = t.screenPosition; return true;
        }
        pos = default; return false;
    }
}

/// <summary>
/// 조작 모드 토글 + (패드 모드용) 깊이 슬라이더·발사 버튼.
/// 자기 영역을 <see cref="VirtualJoystick.AddBlockedZone"/>에 등록해
/// 조이스틱이 같은 자리를 물지 않게 한다.
/// </summary>
public class BrickBreakerAimUI : MonoBehaviour
{
    public enum InputMode { Touch, Pad }

    public static BrickBreakerAimUI Instance { get; private set; }

    const string MODE_KEY = "BrickBreakerInputMode";

    public InputMode Mode { get; private set; } = InputMode.Pad;

    /// <summary>0 = 가장 가까이, 1 = 터널 안쪽 끝. 패드 모드에서만 의미 있다.</summary>
    public float ZNormalized { get; private set; } = 0.45f;

    /// <summary>이번 프레임에 발사 버튼이 눌렸는가.</summary>
    public bool FireRequested { get; private set; }

    public bool Interactable { get; set; } = true;

    const float TRACK_W = 74f;
    const float TRACK_H = 560f;
    const float FIRE_D  = 132f;
    const float TOGGLE_W = 224f;
    const float TOGGLE_H = 54f;

    Canvas        canvas;
    RectTransform canvasRT;
    RectTransform trackRT, fillRT, handleRT, fireRT, toggleRT, gameModeRT, statRT;
    TextMeshProUGUI gameModeTxt, statTxt;
    RectTransform touchSegRT, padSegRT;
    Image         fireImg, touchSegImg, padSegImg;
    TextMeshProUGUI touchSegTxt, padSegTxt;

    int sliderPtr = BrickBreakerPointer.None;

    void Awake()
    {
        Instance = this;
        EnhancedTouchSupport.Enable();
    }

    public static BrickBreakerAimUI Create()
    {
        var go = new GameObject("AimUICanvas");
        var cv = go.AddComponent<Canvas>();
        cv.renderMode   = RenderMode.ScreenSpaceOverlay;
        cv.sortingOrder = 51;                      // 조이스틱(50) 바로 위
        var cs = go.AddComponent<CanvasScaler>();
        cs.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        cs.referenceResolution = new Vector2(1080f, 1920f);
        cs.matchWidthOrHeight  = 0.5f;

        var ui = go.AddComponent<BrickBreakerAimUI>();
        ui.canvas = cv;

        // 이 캔버스는 코드로 만들어서 safe area를 몰랐다. 노치·홈 인디케이터가 있는
        // 기기에서 상단 토글과 하단 발사 버튼이 화면 밖으로 밀리거나 겹쳤다.
        // 콘텐츠를 전부 SafeArea 자식으로 넣어 인셋을 받게 한다.
        var safe = new GameObject("SafeArea", typeof(RectTransform));
        safe.transform.SetParent(go.transform, false);
        var safeRT = safe.GetComponent<RectTransform>();
        safeRT.anchorMin = Vector2.zero; safeRT.anchorMax = Vector2.one;
        safeRT.offsetMin = Vector2.zero; safeRT.offsetMax = Vector2.zero;
        safe.AddComponent<SafeArea>();
        ui.canvasRT = safeRT;
        ui.Mode     = (InputMode)PlayerPrefs.GetInt(MODE_KEY, (int)InputMode.Pad);
        ui.Build();
        return ui;
    }

    // ── 생성 ─────────────────────────────────────────────
    static string L(string key, string fallback)
    {
        var loc = LocalizationManager.Instance;
        return loc != null ? loc.GetOr(key, fallback) : fallback;
    }

    void Build()
    {

        // ── 모드 토글 (HUD 바로 아래, 상단 중앙) ───────────
        // HUD 바(높이 116) 안쪽. 왼쪽은 Back/New/Help(~x332), 오른쪽은 Score/Best가
        // 차지하므로 그 사이에 넣는다. 화면(터널) 위로는 내려오지 않는다.
        toggleRT = MakeRect("ModeToggle", canvasRT, new Vector2(1f, 1f), new Vector2(1f, 1f));
        toggleRT.pivot            = new Vector2(1f, 0.5f);
        toggleRT.sizeDelta        = new Vector2(TOGGLE_W, TOGGLE_H);
        toggleRT.anchoredPosition = new Vector2(-330f, -58f);   // HUD 세로 중앙
        AddImage(toggleRT, RoundedSprite(48, new Color(1f, 1f, 1f, 0.14f)));

        touchSegRT = MakeRect("SegTouch", toggleRT, new Vector2(0f, 0f), new Vector2(0.5f, 1f));
        touchSegRT.offsetMin = new Vector2(5f, 5f);
        touchSegRT.offsetMax = new Vector2(-2.5f, -5f);
        touchSegImg = AddImage(touchSegRT, RoundedSprite(48, Color.white));
        touchSegTxt = MakeLabel(touchSegRT, L("mode_touch", "터치"), 26f);
        Stretch(touchSegTxt.rectTransform);

        padSegRT = MakeRect("SegPad", toggleRT, new Vector2(0.5f, 0f), new Vector2(1f, 1f));
        padSegRT.offsetMin = new Vector2(2.5f, 5f);
        padSegRT.offsetMax = new Vector2(-5f, -5f);
        padSegImg = AddImage(padSegRT, RoundedSprite(48, Color.white));
        padSegTxt = MakeLabel(padSegRT, L("mode_pad", "패드"), 26f);
        Stretch(padSegTxt.rectTransform);

        // ── 깊이 슬라이더 (오른쪽 가장자리, 세로) ──────────
        trackRT = MakeRect("ZTrack", canvasRT, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f));
        trackRT.pivot            = new Vector2(1f, 0.5f);
        trackRT.sizeDelta        = new Vector2(TRACK_W, TRACK_H);
        trackRT.anchoredPosition = new Vector2(-34f, 170f);
        AddImage(trackRT, RoundedSprite(48, new Color(1f, 1f, 1f, 0.16f)));

        fillRT = MakeRect("ZFill", trackRT, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f));
        fillRT.pivot            = new Vector2(0.5f, 0f);
        fillRT.sizeDelta        = new Vector2(TRACK_W - 26f, TRACK_H * 0.45f);
        fillRT.anchoredPosition = Vector2.zero;
        AddImage(fillRT, RoundedSprite(48, new Color(0.45f, 0.75f, 1f, 0.5f)));

        handleRT = MakeRect("ZHandle", trackRT, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f));
        handleRT.pivot     = new Vector2(0.5f, 0.5f);
        handleRT.sizeDelta = new Vector2(TRACK_W + 22f, 54f);
        AddImage(handleRT, RoundedSprite(48, new Color(1f, 1f, 1f, 0.92f)));

        var zl = MakeLabel(trackRT, L("js_label_depth", "깊이"), 26f);
        // 트랙 위에 두면 HUD의 BEST와 겹친다 → 아래로
        zl.rectTransform.anchorMin = zl.rectTransform.anchorMax = new Vector2(0.5f, 0f);
        zl.rectTransform.pivot     = new Vector2(0.5f, 1f);
        zl.rectTransform.anchoredPosition = new Vector2(0f, -10f);
        zl.rectTransform.sizeDelta        = new Vector2(180f, 40f);

        // ── 발사 버튼 (하단 중앙) ─────────────────────────
        fireRT = MakeRect("FireBtn", canvasRT, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f));
        fireRT.pivot            = new Vector2(0.5f, 0f);
        fireRT.sizeDelta        = new Vector2(FIRE_D, FIRE_D);
        fireRT.anchoredPosition = new Vector2(0f, 78f);
        fireImg = AddImage(fireRT, CircleSprite(160, Color.white));
        fireImg.color = new Color(1f, 0.42f, 0.20f, 0.75f);
        var fl = MakeLabel(fireRT, L("btn_fire", "발사"), 32f);
        Stretch(fl.rectTransform);

        // ── 게임 모드 칩 (좌상단, HUD 아래) ───────────────
        // 터널은 화면 중앙이라 왼쪽 가장자리는 비어 있다 — 플레이를 안 가린다.
        gameModeRT = MakeRect("GameModeChip", canvasRT, new Vector2(0f, 1f), new Vector2(0f, 1f));
        gameModeRT.pivot            = new Vector2(0f, 1f);
        gameModeRT.sizeDelta        = new Vector2(196f, 52f);
        gameModeRT.anchoredPosition = new Vector2(22f, -132f);
        AddImage(gameModeRT, RoundedSprite(48, new Color(1f, 1f, 1f, 0.13f)));
        gameModeTxt = MakeLabel(gameModeRT, "", 24f);
        Stretch(gameModeTxt.rectTransform);
        RefreshGameModeChip();

        // ── 적용 중인 파워업 표시 (모드 칩 아래) ───────────
        statRT = MakeRect("StatChip", canvasRT, new Vector2(0f, 1f), new Vector2(0f, 1f));
        statRT.pivot            = new Vector2(0f, 1f);
        statRT.sizeDelta        = new Vector2(370f, 46f);
        statRT.anchoredPosition = new Vector2(22f, -192f);   // 모드 칩(-132, 높이 52) 아래
        AddImage(statRT, RoundedSprite(48, new Color(1f, 1f, 1f, 0.11f)));
        statTxt = MakeLabel(statRT, "", 23f);
        Stretch(statTxt.rectTransform);
        statRT.gameObject.SetActive(false);

        ApplyZ(ZNormalized);
        ApplyMode();
    }

    // ── 모드 ─────────────────────────────────────────────
    void ApplyMode()
    {
        bool pad = Mode == InputMode.Pad;

        trackRT.gameObject.SetActive(pad);
        fireRT.gameObject.SetActive(pad);

        var on  = new Color(1f, 1f, 1f, 0.85f);
        var off = new Color(1f, 1f, 1f, 0.08f);
        touchSegImg.color = pad ? off : on;
        padSegImg.color   = pad ? on  : off;
        touchSegTxt.color = pad ? new Color(1f, 1f, 1f, 0.75f) : new Color(0.05f, 0.06f, 0.12f, 1f);
        padSegTxt.color   = pad ? new Color(0.05f, 0.06f, 0.12f, 1f) : new Color(1f, 1f, 1f, 0.75f);

        RefreshBlockedZones();
    }

    void SetMode(InputMode m)
    {
        if (Mode == m) return;
        Mode = m;
        PlayerPrefs.SetInt(MODE_KEY, (int)m);
        PlayerPrefs.Save();
        ApplyMode();
    }

    /// <summary>
    /// 조이스틱이 이 UI 위를 물지 않도록 등록한다.
    /// 모드에 따라 보이는 요소가 달라지므로 바뀔 때마다 다시 만든다.
    /// </summary>
    public void RefreshBlockedZones()
    {
        VirtualJoystick.ClearBlockedZones();
        VirtualJoystick.AddBlockedZone(ScreenRectOf(toggleRT, 16f));
        if (gameModeRT != null) VirtualJoystick.AddBlockedZone(ScreenRectOf(gameModeRT, 12f));
        if (statRT != null && statRT.gameObject.activeSelf)
            VirtualJoystick.AddBlockedZone(ScreenRectOf(statRT, 10f));
        if (Mode == InputMode.Pad)
        {
            VirtualJoystick.AddBlockedZone(ScreenRectOf(trackRT, 30f));
            VirtualJoystick.AddBlockedZone(ScreenRectOf(fireRT,  20f));
        }

        // 여기서 ClearBlockedZones()를 먼저 하므로, 다른 UI가 따로 등록해 두면
        // 모드가 바뀔 때마다 지워진다. 매번 같이 다시 등록해야 한다.
        BrickBreakerRankUI.Instance?.RegisterBlockedZones();
    }

    /// <summary>터치 모드에서 조준 입력을 받아도 되는 화면 영역인가.</summary>
    public bool IsFreeForAim(Vector2 screenPos) =>
        !ScreenRectOf(toggleRT, 16f).Contains(screenPos)
     && !ScreenRectOf(gameModeRT, 12f).Contains(screenPos)
     && !(statRT != null && statRT.gameObject.activeSelf && ScreenRectOf(statRT, 10f).Contains(screenPos));

    public void RefreshGameModeChip()
    {
        if (gameModeTxt == null) return;
        gameModeTxt.text = BrickBreakerRules.NameOf(BrickBreakerRules.Mode);
        gameModeTxt.color = BrickBreakerRules.IsItemMode
            ? new Color(1f, 0.85f, 0.25f, 0.95f)
            : new Color(1f, 1f, 1f, 0.80f);
    }

    /// <summary>이번 프레임에 게임 모드 칩이 눌렸는가. 매니저가 읽고 재시작한다.</summary>
    public bool GameModeTapped { get; private set; }

    /// <summary>
    /// 볼 개수와 적용 중인 파워업을 상단에 모아 띄운다.
    /// 볼 개수는 점수 줄에 붙어 있으면 어색해서 이쪽으로 옮겼다.
    /// </summary>
    public void SetStats(int balls, float damage, float sizeMul, int luck)
    {
        if (statRT == null) return;
        if (!statRT.gameObject.activeSelf) statRT.gameObject.SetActive(true);

        statTxt.text = BrickBreakerRules.IsItemMode
            ? $"볼 {balls}   공격 x{damage:0.00}   크기 x{sizeMul:0.00}   행운 {luck}"
            : $"볼 {balls}";
    }

    // ── 입력 ─────────────────────────────────────────────
    void Update()
    {
        FireRequested  = false;
        GameModeTapped = false;

        // 조작/게임 모드 칩은 게임이 멈춰 있어도 항상 눌린다
        Rect togScreen  = ScreenRectOf(toggleRT, 0f);
        Rect gmScreen   = gameModeRT != null ? ScreenRectOf(gameModeRT, 0f) : new Rect();
        foreach (var (_, pos, began) in BrickBreakerPointer.All())
        {
            if (!began) continue;
            if (togScreen.Contains(pos))
            {
                SetMode(pos.x < togScreen.center.x ? InputMode.Touch : InputMode.Pad);
                break;
            }
            if (gameModeRT != null && gmScreen.Contains(pos)) { GameModeTapped = true; break; }
        }

        if (!Interactable || Mode != InputMode.Pad)
        {
            sliderPtr = BrickBreakerPointer.None;
            return;
        }

        Rect trackScreen = ScreenRectOf(trackRT, 30f);
        Rect fireScreen  = ScreenRectOf(fireRT,  0f);

        if (sliderPtr != BrickBreakerPointer.None)
        {
            if (BrickBreakerPointer.TryGet(sliderPtr, out Vector2 sp)) SetZFromScreen(sp, trackScreen);
            else sliderPtr = BrickBreakerPointer.None;
        }

        foreach (var (id, pos, began) in BrickBreakerPointer.All())
        {
            if (!began) continue;

            if (sliderPtr == BrickBreakerPointer.None && trackScreen.Contains(pos))
            {
                sliderPtr = id;
                SetZFromScreen(pos, trackScreen);
                continue;
            }
            if (fireScreen.Contains(pos)) FireRequested = true;
        }

        fireImg.color = new Color(1f, 0.42f, 0.20f, FireRequested ? 1f : 0.75f);
    }

    void SetZFromScreen(Vector2 screenPos, Rect track)
        => ApplyZ(Mathf.Clamp01(Mathf.InverseLerp(track.yMin, track.yMax, screenPos.y)));

    void ApplyZ(float t)
    {
        ZNormalized = t;
        if (handleRT) handleRT.anchoredPosition = new Vector2(0f, TRACK_H * t);
        if (fillRT)   fillRT.sizeDelta = new Vector2(fillRT.sizeDelta.x, TRACK_H * t);
    }

    // ── 헬퍼 ─────────────────────────────────────────────
    static readonly Vector3[] corners = new Vector3[4];

    /// <summary>ScreenSpaceOverlay 캔버스는 월드 좌표가 곧 스크린 좌표다.</summary>
    static Rect ScreenRectOf(RectTransform rt, float padPx)
    {
        rt.GetWorldCorners(corners);
        float xMin = corners[0].x - padPx, yMin = corners[0].y - padPx;
        float xMax = corners[2].x + padPx, yMax = corners[2].y + padPx;
        return new Rect(xMin, yMin, xMax - xMin, yMax - yMin);
    }

    static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
    }

    static RectTransform MakeRect(string name, Transform parent, Vector2 aMin, Vector2 aMax)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = aMin; rt.anchorMax = aMax;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        return rt;
    }

    static Image AddImage(RectTransform rt, Sprite sprite)
    {
        var img = rt.gameObject.AddComponent<Image>();
        img.sprite        = sprite;
        img.type          = sprite != null && sprite.border.sqrMagnitude > 0f
                          ? Image.Type.Sliced : Image.Type.Simple;
        img.raycastTarget = false;
        return img;
    }

    static TextMeshProUGUI MakeLabel(Transform parent, string text, float size)
    {
        var go = new GameObject("Label", typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.font          = Resources.Load<TMP_FontAsset>("TextMesh Pro/Fonts/ONE Mobile POP SDF");
        tmp.text          = text;
        tmp.fontSize      = size;
        tmp.alignment     = TextAlignmentOptions.Center;
        tmp.color         = new Color(1f, 1f, 1f, 0.92f);
        tmp.raycastTarget = false;
        return tmp;
    }

    static Sprite CircleSprite(int size, Color c) => UISkin.Circle;

    /// <summary>모서리만 둥근 9-slice 스프라이트. 색별로 캐시한다.</summary>
    static Sprite RoundedSprite(int size, Color c) => UISkin.Panel;

    public void SetVisible(bool on) { if (canvas) canvas.gameObject.SetActive(on); }

    void OnDestroy() { if (Instance == this) Instance = null; }
}
