using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.InputSystem.EnhancedTouch;
using Finger     = UnityEngine.InputSystem.EnhancedTouch.Finger;
using Touch      = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

/// <summary>
/// 화면 절반을 각각 담당하는 플로팅 조이스틱.
/// 터치한 자리에 베이스가 생기고, 거기서부터의 드래그가 입력이 된다.
/// 손가락은 touchId로 추적해 인덱스 재사용으로 인한 교차 점유를 막는다.
/// </summary>
public class VirtualJoystick : MonoBehaviour
{
    const float DEADZONE   = 0.14f;
    const float HUD_HEIGHT = 116f;     // 캔버스 단위. 상단 HUD는 조이스틱이 먹지 않음

    public Vector2 Direction            { get; private set; }
    public Vector2 LastDirection        { get; private set; }
    public bool    IsPressed            { get; private set; }
    public bool    WasReleasedThisFrame { get; private set; }
    public bool    Interactable         { get; set; } = true;

    /// <summary>두 스틱이 공유하는 캔버스. 통째로 숨길 때 쓴다.</summary>
    public GameObject Root => canvas ? canvas.gameObject : gameObject;

    /// <summary>이 스틱 하나만 보이거나 숨긴다. Root는 두 스틱이 공유하는 캔버스라
    /// 한쪽만 끌 때 쓸 수 없다.</summary>
    public void SetVisible(bool on)
    {
        if (on == gameObject.activeSelf) return;
        if (!on)
        {
            // 숨기면 Update가 안 돌아 점유가 남는다. 먼저 정리한다.
            ReleaseFinger();
            Direction = LastDirection = Vector2.zero;
            IsPressed = prevPressed = false;
            WasReleasedThisFrame = false;
        }
        gameObject.SetActive(on);
    }

    /// <summary>익숙해진 뒤 라벨을 치운다.</summary>
    public void HideLabel() { if (labelRT) labelRT.gameObject.SetActive(false); }

    Canvas        canvas;
    RectTransform canvasRT;
    RectTransform baseRT;
    RectTransform knobRT;
    RectTransform labelRT;
    CanvasGroup   group;
    float         maxRadius;
    float         responseExp;
    float         homePad;
    bool          leftSide;

    Finger activeFinger;
    int    activeTouchId = -1;
    bool   mouseActive;      // PC/에디터: 마우스를 손가락 하나처럼 다룬다
    bool   prevPressed;

    // 슬라이더·발사 버튼이 차지한 화면 영역은 조이스틱이 물지 않는다.
    // 조이스틱 캔버스에는 GraphicRaycaster가 없어서 UI가 입력을 못 막기 때문에
    // 여기서 직접 제외해야 한다.
    static readonly List<Rect> blockedZones = new();
    public static void ClearBlockedZones()          => blockedZones.Clear();
    public static void AddBlockedZone(Rect screen)  => blockedZones.Add(screen);

    void Awake() => EnhancedTouchSupport.Enable();

    /// <summary>Touch.activeTouches는 EnhancedTouch가 켜져 있어야 예외가 안 난다.</summary>
    static void EnsureTouch()
    {
        if (!EnhancedTouchSupport.enabled) EnhancedTouchSupport.Enable();
    }

    // ── Factory ──────────────────────────────────────────
    public static void CreatePair(out VirtualJoystick leftJs, out VirtualJoystick rightJs)
    {
        var go = new GameObject("JoystickCanvas");
        var cv = go.AddComponent<Canvas>();
        cv.renderMode   = RenderMode.ScreenSpaceOverlay;
        cv.sortingOrder = 50;
        var cs = go.AddComponent<CanvasScaler>();
        cs.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        cs.referenceResolution = new Vector2(1080f, 1920f);
        cs.matchWidthOrHeight  = 0.5f;

        // 조이스틱은 raw 터치로 동작한다. GraphicRaycaster를 두지 않아
        // HUD 버튼 레이캐스트를 가로채지 않는다.

        // 카메라 스틱(왼쪽) / 조준 스틱(오른쪽).
        // 조준 스틱은 더 크게 — 반경이 클수록 각도 분해능이 올라간다.
        ClearBlockedZones();   // 씬 재시작 시 이전 영역이 남지 않게

        var loc = LocalizationManager.Instance;
        leftJs  = CreateOne(cv, true,  260f, 105f, 1.0f,
                            loc?.Get("js_label_view") ?? "시점");
        rightJs = CreateOne(cv, false, 360f, 130f, 1.7f,
                            loc?.Get("js_label_aim") ?? "조준 X·Y");
    }

    static VirtualJoystick CreateOne(Canvas canvas, bool leftSide,
        float baseSize, float knobSize, float responseExp, string label)
    {
        var baseGO = new GameObject(leftSide ? "LJoystick" : "RJoystick");
        var bRT    = baseGO.AddComponent<RectTransform>();
        baseGO.transform.SetParent(canvas.transform, false);

        var baseImg = baseGO.AddComponent<Image>();
        baseImg.sprite        = MakeRingSprite(160, new Color(1f, 1f, 1f, 0.55f), 0.12f);
        baseImg.raycastTarget = false;
        bRT.sizeDelta = new Vector2(baseSize, baseSize);
        bRT.anchorMin = bRT.anchorMax = new Vector2(0.5f, 0.5f);
        bRT.pivot     = new Vector2(0.5f, 0.5f);

        var knobGO = new GameObject("Knob");
        var kRT    = knobGO.AddComponent<RectTransform>();
        knobGO.transform.SetParent(baseGO.transform, false);

        var knobImg = knobGO.AddComponent<Image>();
        knobImg.sprite        = MakeCircleSprite(128, new Color(1f, 1f, 1f, 0.55f));
        knobImg.raycastTarget = false;
        kRT.sizeDelta        = new Vector2(knobSize, knobSize);
        kRT.anchoredPosition = Vector2.zero;

        // 스틱이 뭐 하는 물건인지 알 수 있게 라벨을 단다.
        // 게임에 익숙해지면(HideLabel) 사라진다.
        var labelGO = new GameObject("Label");
        var lblRT   = labelGO.AddComponent<RectTransform>();
        labelGO.transform.SetParent(baseGO.transform, false);
        var lblTmp = labelGO.AddComponent<TextMeshProUGUI>();
        lblTmp.font          = Resources.Load<TMP_FontAsset>("TextMesh Pro/Fonts/ONE Mobile POP SDF");
        lblTmp.text          = label;
        lblTmp.fontSize      = 26f;
        lblTmp.color         = new Color(1f, 1f, 1f, 0.9f);
        lblTmp.alignment     = TextAlignmentOptions.Center;
        lblTmp.raycastTarget = false;
        lblRT.anchorMin = lblRT.anchorMax = new Vector2(0.5f, 0f);
        lblRT.pivot     = new Vector2(0.5f, 1f);
        lblRT.anchoredPosition = new Vector2(0f, -10f);
        lblRT.sizeDelta        = new Vector2(baseSize + 80f, 40f);

        var cg = baseGO.AddComponent<CanvasGroup>();
        cg.alpha          = 0.55f;
        cg.blocksRaycasts = false;
        cg.interactable   = false;

        var js         = baseGO.AddComponent<VirtualJoystick>();
        js.canvas      = canvas;
        js.canvasRT    = canvas.GetComponent<RectTransform>();
        js.baseRT      = bRT;
        js.knobRT      = kRT;
        js.group       = cg;
        js.maxRadius   = (baseSize - knobSize) * 0.5f;
        js.responseExp = responseExp;
        js.homePad     = baseSize * 0.75f;
        js.leftSide    = leftSide;
        js.labelRT     = lblRT;
        bRT.anchoredPosition = js.HomePosition();
        return js;
    }

    // ── Per-frame update ─────────────────────────────────
    void Update()
    {
        bool    found    = false;
        Vector2 touchPos = Vector2.zero;

        EnsureTouch();
        if (!Interactable) ReleaseFinger();

        // 1) 점유 중인 손가락이 아직 살아있는지 확인.
        //    touchId까지 비교해 인덱스 재사용으로 남의 터치를 물지 않게 한다.
        if (activeFinger != null)
        {
            var t = activeFinger.currentTouch;
            bool alive = activeFinger.isActive
                      && t.valid
                      && t.touchId == activeTouchId
                      && t.phase != TouchPhase.Ended
                      && t.phase != TouchPhase.Canceled;

            if (alive) { touchPos = t.screenPosition; found = true; }
            else       ReleaseFinger();
        }

        // 1-b) 마우스 점유 유지 (PC/에디터)
        if (mouseActive)
        {
            var m = Mouse.current;
            if (Interactable && m != null && m.leftButton.isPressed)
            {
                touchPos = m.position.ReadValue();
                found    = true;
            }
            else mouseActive = false;
        }

        // 2) 비어 있으면 내 구역에서 새로 시작한 터치를 잡는다.
        if (Interactable && !found && activeFinger == null)
        {
            EnsureTouch();
            foreach (var touch in Touch.activeTouches)
            {
                if (touch.phase != TouchPhase.Began) continue;
                if (!InZone(touch.screenPosition))   continue;

                activeFinger  = touch.finger;
                activeTouchId = touch.touchId;
                LastDirection = Vector2.zero;      // 새 제스처 시작
                touchPos      = touch.screenPosition;
                found         = true;

                // 플로팅: 누른 자리로 베이스를 옮긴다
                baseRT.anchoredPosition = ScreenToCanvas(touchPos);
                knobRT.anchoredPosition = Vector2.zero;
                break;
            }
        }

        // 2-b) 터치가 없으면 마우스로 새로 잡는다 (PC/에디터)
        if (Interactable && !found && activeFinger == null && !mouseActive)
        {
            var m = Mouse.current;
            if (m != null && m.leftButton.wasPressedThisFrame)
            {
                Vector2 mp = m.position.ReadValue();
                if (InZone(mp))
                {
                    mouseActive   = true;
                    LastDirection = Vector2.zero;
                    touchPos      = mp;
                    found         = true;
                    baseRT.anchoredPosition = ScreenToCanvas(touchPos);
                    knobRT.anchoredPosition = Vector2.zero;
                }
            }
        }

        WasReleasedThisFrame = prevPressed && !found;
        prevPressed          = found;
        IsPressed            = found;

        if (found)
        {
            Vector2 delta   = ScreenToCanvas(touchPos) - baseRT.anchoredPosition;
            Vector2 clamped = Vector2.ClampMagnitude(delta, maxRadius);
            knobRT.anchoredPosition = clamped;

            Vector2 raw = clamped / maxRadius;
            float   mag = raw.magnitude;
            Direction = mag > DEADZONE
                ? raw.normalized * Mathf.Pow(Mathf.InverseLerp(DEADZONE, 1f, mag), responseExp)
                : Vector2.zero;

            if (Direction.sqrMagnitude > 0f) LastDirection = Direction;
            group.alpha = 0.85f;
        }
        else
        {
            Direction = Vector2.zero;
            knobRT.anchoredPosition = Vector2.zero;
            baseRT.anchoredPosition = HomePosition();
            group.alpha = 0.35f;
        }
    }

    void ReleaseFinger()
    {
        activeFinger  = null;
        activeTouchId = -1;
        mouseActive   = false;
    }

    bool InZone(Vector2 sp)
    {
        // HUD는 safe area 상단에 붙어 있으므로 노치 높이만큼 같이 내려온다
        float hudPx = HUD_HEIGHT * (canvas ? canvas.scaleFactor : 1f);
        if (sp.y > Screen.safeArea.yMax - hudPx) return false;

        for (int i = 0; i < blockedZones.Count; i++)
            if (blockedZones[i].Contains(sp)) return false;

        return leftSide ? sp.x <  Screen.width * 0.5f
                        : sp.x >= Screen.width * 0.5f;
    }

    /// <summary>스크린 좌표 → 캔버스 중앙 기준 로컬 좌표.</summary>
    Vector2 ScreenToCanvas(Vector2 sp)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRT, sp, null, out Vector2 local);
        return local;
    }

    /// <summary>대기 위치. safe area 안쪽으로 물려서 홈 인디케이터에 깔리지 않게 한다.</summary>
    Vector2 HomePosition()
    {
        Vector2 half = canvasRT.rect.size * 0.5f;

        // 스크린 픽셀 → 캔버스 단위
        float sx = canvasRT.rect.width  / Mathf.Max(1f, Screen.width);
        float sy = canvasRT.rect.height / Mathf.Max(1f, Screen.height);

        Rect  safe    = Screen.safeArea;
        float insetL  = safe.xMin * sx;
        float insetR  = (Screen.width - safe.xMax) * sx;
        float insetB  = safe.yMin * sy;

        float x = leftSide ? -half.x + insetL + homePad
                           :  half.x - insetR - homePad;
        return new Vector2(x, -half.y + insetB + homePad);
    }

    // ── Sprites ───────────────────────────────────────────
    // 예전엔 원·링 텍스처를 픽셀 루프로 직접 그렸다. 지금은 공용 스킨을 쓴다.
    static Sprite MakeCircleSprite(int size, Color color) => UISkin.Circle;

    /// <summary>thickness 인자는 남겨둔다 — 호출부를 건드리지 않기 위한 것.</summary>
    static Sprite MakeRingSprite(int size, Color color, float thickness) => UISkin.CircleLine;
}
