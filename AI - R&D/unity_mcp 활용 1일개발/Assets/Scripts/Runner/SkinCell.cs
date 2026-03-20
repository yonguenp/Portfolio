using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 캐릭터 선택 셀 — Kenney 스프라이트 캐릭터 미리보기.
/// Spine 의존성 없음. Image로 idle 스프라이트 표시.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class SkinCell : MonoBehaviour
{
    static Color Hex(string h)
    {
        ColorUtility.TryParseHtmlString(h, out var c);
        return c;
    }

    // 팔레트
    static readonly Color C_BG_NORMAL = Hex("#12103A");
    static readonly Color C_BG_PRESS  = Hex("#2A2880");
    static readonly Color C_GOLD      = Hex("#FFD84D");
    static readonly Color C_LABEL     = Color.white;
    static readonly Color C_HINT      = Hex("#A0B8FF");
    static readonly Color C_DIVIDER   = Hex("#FFFFFF12");
    static readonly Color C_PREVIEW   = Hex("#1C1844");  // 미리보기 배경

    private RectTransform        _rt;
    private Image                _bg;
    private Image                _previewBg;
    private Image                _preview;
    private TMP_Text             _numLabel;
    private TMP_Text             _nameLabel;
    private Button               _btn;
    private SkinSelectController _ctrl;
    private string               _currentFolder;

    // ── 빌드 (풀 생성 시 1회) ────────────────────────────────
    public void Build(TMP_FontAsset font)
    {
        _rt = GetComponent<RectTransform>();

        // 배경
        _bg       = gameObject.AddComponent<Image>();
        _bg.color = C_BG_NORMAL;

        // 왼쪽 골드 포인트 바 (6px)
        var bar   = new GameObject("AccentBar"); bar.transform.SetParent(transform, false);
        var barRT = bar.AddComponent<RectTransform>();
        barRT.anchorMin = new Vector2(0f, 0f);
        barRT.anchorMax = new Vector2(0f, 1f);
        barRT.offsetMin = new Vector2(0f,  4f);
        barRT.offsetMax = new Vector2(6f, -4f);
        bar.AddComponent<Image>().color = C_GOLD;

        // 미리보기 패널 배경
        var preBgGO = new GameObject("PreviewBg"); preBgGO.transform.SetParent(transform, false);
        var preBgRT = preBgGO.AddComponent<RectTransform>();
        preBgRT.anchorMin = new Vector2(0.02f, 0.05f);
        preBgRT.anchorMax = new Vector2(0.44f, 0.95f);
        preBgRT.offsetMin = preBgRT.offsetMax = Vector2.zero;
        _previewBg       = preBgGO.AddComponent<Image>();
        _previewBg.color = C_PREVIEW;

        // 캐릭터 스프라이트 미리보기
        var preGO = new GameObject("Preview"); preGO.transform.SetParent(preBgGO.transform, false);
        var preRT = preGO.AddComponent<RectTransform>();
        preRT.anchorMin = new Vector2(0.05f, 0.05f);
        preRT.anchorMax = new Vector2(0.95f, 0.95f);
        preRT.offsetMin = preRT.offsetMax = Vector2.zero;
        _preview                = preGO.AddComponent<Image>();
        _preview.preserveAspect = true;

        // 번호 라벨 (골드, 작게)
        var numGO = new GameObject("NumLabel"); numGO.transform.SetParent(transform, false);
        var numRT = numGO.AddComponent<RectTransform>();
        numRT.anchorMin = new Vector2(0.46f, 0.60f);
        numRT.anchorMax = new Vector2(0.98f, 0.96f);
        numRT.offsetMin = numRT.offsetMax = Vector2.zero;
        _numLabel = numGO.AddComponent<TextMeshProUGUI>();
        if (font != null) _numLabel.font = font;
        _numLabel.fontSize  = 22f;
        _numLabel.color     = C_GOLD;
        _numLabel.fontStyle = FontStyles.Bold;
        _numLabel.alignment = TextAlignmentOptions.BottomLeft;
        _numLabel.text      = "";

        // 이름 라벨 (흰색, 크게)
        var nameGO = new GameObject("NameLabel"); nameGO.transform.SetParent(transform, false);
        var nameRT = nameGO.AddComponent<RectTransform>();
        nameRT.anchorMin = new Vector2(0.46f, 0.28f);
        nameRT.anchorMax = new Vector2(0.98f, 0.62f);
        nameRT.offsetMin = nameRT.offsetMax = Vector2.zero;
        _nameLabel = nameGO.AddComponent<TextMeshProUGUI>();
        if (font != null) _nameLabel.font = font;
        _nameLabel.fontSize  = 36f;
        _nameLabel.color     = C_LABEL;
        _nameLabel.fontStyle = FontStyles.Bold;
        _nameLabel.alignment = TextAlignmentOptions.MidlineLeft;
        _nameLabel.text      = "";

        // 힌트 텍스트
        var hintGO = new GameObject("Hint"); hintGO.transform.SetParent(transform, false);
        var hintRT = hintGO.AddComponent<RectTransform>();
        hintRT.anchorMin = new Vector2(0.46f, 0.02f);
        hintRT.anchorMax = new Vector2(0.98f, 0.28f);
        hintRT.offsetMin = hintRT.offsetMax = Vector2.zero;
        var hint = hintGO.AddComponent<TextMeshProUGUI>();
        if (font != null) hint.font = font;
        hint.fontSize  = 20f;
        hint.color     = C_HINT;
        hint.alignment = TextAlignmentOptions.MidlineLeft;
        hint.text      = "탭하여 선택";

        // 하단 구분선
        var lineGO = new GameObject("Divider"); lineGO.transform.SetParent(transform, false);
        var lineRT = lineGO.AddComponent<RectTransform>();
        lineRT.anchorMin = new Vector2(0f, 0f);
        lineRT.anchorMax = new Vector2(1f, 0f);
        lineRT.offsetMin = new Vector2(10f, -1f);
        lineRT.offsetMax = new Vector2(-10f, 0f);
        lineGO.AddComponent<Image>().color = C_DIVIDER;

        // 버튼
        _btn = gameObject.AddComponent<Button>();
        var cb = _btn.colors;
        cb.normalColor      = Color.white;
        cb.highlightedColor = new Color(1.1f, 1.1f, 1.35f, 1f);
        cb.pressedColor     = new Color(0.7f, 0.7f, 0.95f, 1f);
        cb.fadeDuration     = 0.07f;
        _btn.colors        = cb;
        _btn.targetGraphic = _bg;
        _btn.onClick.AddListener(OnClick);
    }

    // ── 재사용 시 데이터 갱신 ─────────────────────────────────
    public void Setup(string folder, string displayName, int index,
                      float cellH, float cellPad, SkinSelectController ctrl)
    {
        _ctrl          = ctrl;
        _currentFolder = folder;

        _rt.anchorMin        = new Vector2(0f, 1f);
        _rt.anchorMax        = new Vector2(1f, 1f);
        _rt.pivot            = new Vector2(0.5f, 1f);
        _rt.anchoredPosition = new Vector2(0f, -(index * cellH + cellPad));
        _rt.sizeDelta        = new Vector2(0f, cellH - cellPad);

        if (_numLabel)  _numLabel.text  = "No." + (index + 1);
        if (_nameLabel) _nameLabel.text = displayName;
        if (_bg)        _bg.color       = C_BG_NORMAL;

        // 미리보기 스프라이트 로드 (idle → stand → walk1 순으로 시도)
        if (_preview != null)
        {
            string fn  = folder.ToLower();
            string pfx = "Characters/" + folder + "/" + fn;
            var sp = Resources.Load<Sprite>(pfx + "_idle");
            if (sp == null) sp = Resources.Load<Sprite>(pfx + "_stand");
            if (sp == null) sp = Resources.Load<Sprite>(pfx + "_walk1");
            _preview.sprite  = sp;
            _preview.color   = sp != null ? Color.white : new Color(0, 0, 0, 0);
        }
    }

    void OnClick()
    {
        _ctrl?.SelectSkin(_currentFolder);
        if (_bg)
        {
            _bg.color = C_BG_PRESS;
            Invoke(nameof(ResetBg), 0.18f);
        }
    }

    void ResetBg()
    {
        if (_bg) _bg.color = C_BG_NORMAL;
    }
}
