using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI 스프라이트의 단일 진입점.
///
/// 예전에는 각 UI 파일이 <c>Sprite.Create</c>로 둥근 사각형과 원을 직접 그려 썼다
/// (<c>RoundedSprite</c>, <c>CircleSprite</c>, <c>MakeCircleSprite</c>). 그래서
/// 모양을 바꾸려면 세 파일을 따로 고쳐야 했고 서로 조금씩 달랐다.
/// 지금은 전부 여기를 거치므로 **스킨 교체가 이 파일 하나 수정으로 끝난다.**
///
/// 스프라이트는 전부 <b>회색(중립) 원본</b>이다. 색은 <c>Image.color</c>로 곱해서
/// 낸다 — 기존 코드가 이미 그 방식이라 그대로 얹힌다. 색이 구워진 스프라이트를
/// 쓰면 게임별 액센트 컬러를 못 바꾼다.
///
/// 원본: Kenney UI Pack / Game Icons (CC0). <c>Assets/Art/Kenney/</c>에 전체가 있고,
/// <b>실제로 쓰는 것만</b> <c>Assets/Resources/UI/</c>에 역할 이름으로 복사해 뒀다.
/// Resources 폴더는 사용 여부와 무관하게 통째로 빌드에 들어가므로 원본 18MB를
/// 그대로 두면 안 된다.
/// </summary>
public static class UISkin
{
    const string ROOT = "UI/";
    const string ICON = "UI/Icon/";

    static readonly Dictionary<string, Sprite> cache = new();

    /// <summary>
    /// 스프라이트를 이름으로 가져온다. 없으면 <c>null</c>.
    ///
    /// null을 그대로 돌려주는 게 중요하다 — Image.sprite가 null이면 단색 사각형으로
    /// 그려지는데, 그게 스킨 도입 이전의 모습이다. 즉 에셋이 빠져도 UI가
    /// 사라지지 않고 예전 모습으로 낮춰 동작한다.
    /// </summary>
    public static Sprite Get(string name)
    {
        if (cache.TryGetValue(name, out var s)) return s;
        s = Resources.Load<Sprite>(ROOT + name);
        if (s == null) Debug.LogWarning($"[UISkin] 스프라이트 없음: {ROOT}{name}");
        cache[name] = s;
        return s;
    }

    /// <summary>아이콘. 이름은 Kenney 원본 그대로다(arrowLeft, home, trophy…).</summary>
    public static Sprite Icon(string name)
    {
        string key = "Icon/" + name;
        if (cache.TryGetValue(key, out var s)) return s;
        s = Resources.Load<Sprite>(ICON + name);
        if (s == null) Debug.LogWarning($"[UISkin] 아이콘 없음: {ICON}{name}");
        cache[key] = s;
        return s;
    }

    // ── 역할 이름 ────────────────────────────────────────
    // 호출부가 "button_rectangle_flat" 같은 생김새 이름을 알 필요가 없다.
    // 스킨을 갈아끼워도 호출부는 그대로 둔다.
    public static Sprite Panel       => Get("panel");        // 기본 패널 (9-slice)
    public static Sprite PanelLine   => Get("panel_line");   // 테두리만 있는 패널
    public static Sprite Button      => Get("button");       // 눌리는 느낌의 버튼
    public static Sprite ButtonLine  => Get("button_border");
    public static Sprite Chip        => Get("chip");         // 작은 정사각 칩
    public static Sprite Circle      => Get("circle");       // 원형 (늘리지 말 것)
    public static Sprite CircleLine  => Get("circle_line"); // 외곽선 원 (조이스틱 베이스)
    public static Sprite Input       => Get("input");
    public static Sprite Divider     => Get("divider");
    public static Sprite SliderTrack => Get("slider_track");
    public static Sprite SliderKnob  => Get("slider_knob");
    public static Sprite CheckOn     => Get("check_on");
    public static Sprite CheckOff    => Get("check_off");

    /// <summary>
    /// Image에 스프라이트를 씌운다. 보더가 있으면 9-slice, 없으면 통짜.
    /// 스프라이트가 없으면 아무것도 안 한다 — 단색 사각형으로 남아 예전 모습이 된다.
    /// </summary>
    public static Image Apply(Image img, Sprite sp)
    {
        if (img == null || sp == null) return img;
        img.sprite = sp;
        img.type   = sp.border.sqrMagnitude > 0f ? Image.Type.Sliced : Image.Type.Simple;
        return img;
    }

    /// <summary>
    /// 씬을 다시 로드해도 캐시는 유지된다(Resources 참조라 안전).
    /// 스킨을 런타임에 바꾸는 기능을 넣게 되면 이걸 부른다.
    /// </summary>
    public static void ClearCache() => cache.Clear();

    // ── Depth 스킨 (Kenney ui-pack Sample.png 느낌) ─────────
    // 2026-08-18: "너무 투박하다 / 샘플 스크린샷 느낌으로" 요청으로 추가.
    // 위의 Panel/Button 등은 회색 원본을 Image.color로 틴트하는 방식인데,
    // 이 세트는 색이 이미 구워진 "_flat"/"_depth_flat" 원본을 그대로 쓴다
    // (Kenney 색상 세트 규칙 — depth 스프라이트는 틴트하면 입체감이 죽는다,
    // 위 "UI 스킨 — Kenney" 섹션 참고). 그래서 여기는 이름이 아니라
    // <see cref="Accent"/> 색상별로 파일이 따로 있고, 틴트 대신 "어떤 색
    // 파일을 쓸지"로 색을 고른다 — 나중에 새 색을 추가하려면 스프라이트
    // 파일만 더 넣으면 된다(코드 구조는 안 바뀐다).
    public enum Accent { Blue, Green, Red, Yellow, Grey }

    static string AccentName(Accent a) => a switch
    {
        Accent.Blue => "blue", Accent.Green => "green", Accent.Red => "red",
        Accent.Yellow => "yellow", _ => "grey",
    };

    /// <summary>입체 버튼(아래 그림자 립) — 실제로 눌리는 액션(확인/시작 등)에 쓴다.</summary>
    public static Sprite DepthButton(Accent a) => Get("Kenney/button_depth_" + AccentName(a));

    /// <summary>그림자 없는 납작한 색 패널 — 팝업 헤더바, 안 눌리는 배지에 쓴다.</summary>
    public static Sprite HeaderBar(Accent a) => Get("Kenney/header_bar_" + AccentName(a));

    /// <summary>원형 입체 버튼 — 닫기(X) 등 아이콘 버튼.</summary>
    public static Sprite RoundDepthButton(Accent a) => Get("Kenney/button_round_depth_" + AccentName(a));

    /// <summary>패널 본문(밝은 바탕, 얇은 회색 테두리) — 헤더바 아래 내용 영역.</summary>
    public static Sprite PanelBody => Get("Kenney/panel_body");

    public static Sprite IconCross        => Get("Kenney/icon_cross");
    public static Sprite IconOutlineCross => Get("Kenney/icon_outline_cross");

    /// <summary>
    /// 팝업 표준 헤더바+본문 패널을 만든다. 헤더바(색상 스트립+제목+닫기 X) 위에
    /// 본문(밝은 배경)이 이어지는 Kenney 샘플 구도 — 딤 아래 아무 판이나 하나
    /// 깔던 기존 <c>MakeModalPanel</c>보다 한 단계 더 완성된 형태다. 반환값의
    /// <c>body</c>가 실제 콘텐츠를 붙일 곳이다(헤더바 높이만큼 아래로 내려와 있다).
    /// </summary>
    public const float HeaderH = 76f;

    public struct KenneyPanel { public RectTransform root, header, body; public TMPro.TextMeshProUGUI titleText; public Button closeButton; }

    /// <summary><paramref name="onClose"/>가 있으면 즉시 런타임 클로저로 닫기 X를
    /// 연결한다(씬에서 바로 쓰는 경우). <paramref name="showClose"/>를 별도로 두는
    /// 이유 — 프리팹을 굽는 에디터 스크립트는 onClose로 즉석 클로저를 넘길 수
    /// 없다(그 클로저는 저장 시점에 사라진다, 런타임 AddListener는 직렬화 안 됨).
    /// 그런 경우 <c>showClose: true, onClose: null</c>로 구조만 만들고, 반환값의
    /// <see cref="KenneyPanel.closeButton"/>을 <c>UnityEventTools.AddVoidPersistentListener</c>로
    /// 직접 영구 연결한다.</summary>
    public static KenneyPanel MakeKenneyPanel(RectTransform parent, string name, Vector2 size, Vector2 pos,
                                               Accent accent, string title, System.Action onClose = null, bool showClose = false)
    {
        var root = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
        root.SetParent(parent, false);
        root.anchorMin = root.anchorMax = new Vector2(0.5f, 1f);
        root.pivot = new Vector2(0.5f, 1f);
        root.sizeDelta = size;
        root.anchoredPosition = pos;

        // 본문 — 헤더바 바로 아래부터 패널 하단까지. 헤더바와 겹치지 않게
        // 정확히 HeaderH만큼 내려서 시작한다(겹치면 본문 맨 위 자식이 헤더에
        // 가려 안 보이는 버그가 난다 — 실제로 한 번 겪고 고쳤다).
        var bodyGo = new GameObject("Body", typeof(RectTransform), typeof(Image));
        var body = bodyGo.GetComponent<RectTransform>();
        body.SetParent(root, false);
        body.anchorMin = Vector2.zero; body.anchorMax = Vector2.one;
        body.offsetMin = Vector2.zero; body.offsetMax = new Vector2(0f, -HeaderH);
        Apply(bodyGo.GetComponent<Image>(), PanelBody);

        // 헤더바 — 패널 상단에 고정, 색 스트립.
        var headerGo = new GameObject("Header", typeof(RectTransform), typeof(Image));
        var header = headerGo.GetComponent<RectTransform>();
        header.SetParent(root, false);
        header.anchorMin = header.anchorMax = new Vector2(0.5f, 1f);
        header.pivot = new Vector2(0.5f, 1f);
        header.sizeDelta = new Vector2(size.x, HeaderH);
        header.anchoredPosition = Vector2.zero;
        Apply(headerGo.GetComponent<Image>(), HeaderBar(accent));

        var titleGo = new GameObject("Title", typeof(RectTransform), typeof(TMPro.TextMeshProUGUI));
        var titleRT = titleGo.GetComponent<RectTransform>();
        titleRT.SetParent(header, false);
        titleRT.anchorMin = Vector2.zero; titleRT.anchorMax = Vector2.one;
        titleRT.offsetMin = new Vector2(28f, 0f); titleRT.offsetMax = new Vector2(-60f, 0f);
        var titleText = titleGo.GetComponent<TMPro.TextMeshProUGUI>();
        titleText.text = title;
        titleText.fontSize = 30f;
        titleText.color = Color.white;
        titleText.fontStyle = TMPro.FontStyles.Bold;
        titleText.alignment = TMPro.TextAlignmentOptions.MidlineLeft;
        var font = Resources.Load<TMPro.TMP_FontAsset>("TextMesh Pro/Fonts/ONE Mobile POP SDF");
        if (font != null) titleText.font = font;

        Button closeButton = null;
        if (onClose != null || showClose)
        {
            var closeGo = new GameObject("Close", typeof(RectTransform), typeof(Image), typeof(Button));
            var closeRT = closeGo.GetComponent<RectTransform>();
            closeRT.SetParent(header, false);
            closeRT.anchorMin = closeRT.anchorMax = new Vector2(1f, 0.5f);
            closeRT.pivot = new Vector2(1f, 0.5f);
            closeRT.sizeDelta = new Vector2(48f, 48f);
            closeRT.anchoredPosition = new Vector2(-14f, 0f);
            var closeImg = closeGo.GetComponent<Image>();
            Apply(closeImg, RoundDepthButton(Accent.Red));
            var closeBtn = closeGo.GetComponent<Button>();
            closeBtn.targetGraphic = closeImg;
            if (onClose != null) closeBtn.onClick.AddListener(() => onClose());
            closeButton = closeBtn;

            var xGo = new GameObject("X", typeof(RectTransform), typeof(Image));
            var xRT = xGo.GetComponent<RectTransform>();
            xRT.SetParent(closeRT, false);
            xRT.anchorMin = xRT.anchorMax = new Vector2(0.5f, 0.5f);
            xRT.sizeDelta = new Vector2(20f, 20f);
            var xImg = xGo.GetComponent<Image>();
            xImg.sprite = IconCross;
            xImg.raycastTarget = false;
        }

        return new KenneyPanel { root = root, header = header, body = body, titleText = titleText, closeButton = closeButton };
    }

    /// <summary>Depth 버튼 하나(라벨 포함)를 만든다. 확인/취소류 팝업 버튼 공용.</summary>
    public static Button MakeKenneyButton(RectTransform parent, string name, Vector2 size, Vector2 pos,
                                           Accent accent, string label, System.Action onClick)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        var rt = go.GetComponent<RectTransform>();
        rt.SetParent(parent, false);
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = size;
        rt.anchoredPosition = pos;
        var img = go.GetComponent<Image>();
        Apply(img, DepthButton(accent));

        var labelGo = new GameObject("Label", typeof(RectTransform), typeof(TMPro.TextMeshProUGUI));
        var labelRT = labelGo.GetComponent<RectTransform>();
        labelRT.SetParent(rt, false);
        labelRT.anchorMin = Vector2.zero; labelRT.anchorMax = Vector2.one;
        labelRT.offsetMin = Vector2.zero; labelRT.offsetMax = new Vector2(0f, -4f); // 립 두께만큼 살짝 위로
        var labelText = labelGo.GetComponent<TMPro.TextMeshProUGUI>();
        labelText.text = label;
        labelText.fontSize = 26f;
        labelText.color = Color.white;
        labelText.fontStyle = TMPro.FontStyles.Bold;
        labelText.alignment = TMPro.TextAlignmentOptions.Center;
        labelText.raycastTarget = false;
        var font = Resources.Load<TMPro.TMP_FontAsset>("TextMesh Pro/Fonts/ONE Mobile POP SDF");
        if (font != null) labelText.font = font;

        var btn = go.GetComponent<Button>();
        btn.targetGraphic = img;
        btn.onClick.AddListener(() => onClick());
        return btn;
    }
}
