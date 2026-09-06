using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 타이틀 화면 설정 팝업 — BGM/효과음 볼륨 + 라이선스 정보.
///
/// 지금 소리가 있는 건 BrickBreaker3D뿐이지만, 여기서 조절하는 값은
/// <see cref="GameAudioSettings"/>(전 게임 공용)이라 나중에 다른 게임에
/// 소리가 붙어도 설정 화면을 새로 만들 필요가 없다.
///
/// 랭킹 UI(BrickBreakerRankUI)와 같은 원칙 — 모달은 뒤를 막아야 하므로
/// GraphicRaycaster가 있는 캔버스가 필요하고(타이틀 Canvas가 이미 그렇다),
/// 열기 버튼만 SafeArea 안에 두고 딤은 화면 전체를 덮는다.
/// </summary>
public class TitleOptionsUI : MonoBehaviour
{
    RectTransform panelRT;     // 딤 + 설정 카드
    RectTransform licenseRT;   // 라이선스 정보 서브패널 (설정 카드 위에 덮인다)
    Slider bgmSlider, sfxSlider;

    // 2026-08-25 — Kenney 밝은 Depth 스킨 전면 통일: 카드 배경이 어두운
    // Surface에서 밝은 PanelBody로 바뀌어서, 그 위에 놓이는 텍스트는
    // 전부 밝은색(T95/T70)에서 어두운 남색으로 뒤집었다(이름은 유지해
    // 호출부를 안 건드렸다 — 값만 바뀜). 버튼 라벨은 Depth 버튼(색이
    // 이미 구워져 있음) 위에 놓이므로 반대로 항상 흰색이어야 한다 —
    // 아래 각 버튼 라벨 호출에서 명시적으로 Color.white를 쓴다.
    static readonly Color Surface2 = new Color(0.85f, 0.87f, 0.90f, 1f); // 슬라이더 트랙(밝은 회색)
    static readonly Color Accent   = new Color(0.929f, 0.729f, 0.180f, 1f); // #EDBA2E — 슬라이더 채움만 유지
    static readonly Color T95 = new Color(0.106f, 0.133f, 0.267f, 0.95f);
    static readonly Color T70 = new Color(0.106f, 0.133f, 0.267f, 0.70f);

    public static TitleOptionsUI Create(RectTransform canvasRT, RectTransform safeAreaRT, RectTransform placeLeftOf)
    {
        var go = new GameObject("TitleOptionsUI", typeof(RectTransform));
        go.transform.SetParent(canvasRT, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;

        var ui = go.AddComponent<TitleOptionsUI>();
        ui.Build(safeAreaRT, placeLeftOf);
        return ui;
    }

    void Build(RectTransform safeAreaRT, RectTransform placeLeftOf)
    {
        // ── 여는 버튼: 기존 언어 버튼 왼쪽에 같은 톤으로 붙인다 ──
        var btnRT = MakeRect("OptionsBtn", safeAreaRT, new Vector2(1f, 1f), new Vector2(1f, 1f));
        btnRT.pivot = new Vector2(1f, 1f);
        btnRT.sizeDelta = new Vector2(100f, 60f);
        float gap = placeLeftOf.sizeDelta.x + 12f;
        btnRT.anchoredPosition = placeLeftOf.anchoredPosition - new Vector2(gap, 0f);
        AddImg(btnRT, UISkin.DepthButton(UISkin.Accent.Grey), Color.white, true);
        AddLabel(btnRT, "설정", 22f, Color.white).rectTransform.anchoredPosition = Vector2.zero;
        Stretch(((RectTransform)btnRT.GetChild(btnRT.childCount - 1)));
        btnRT.gameObject.AddComponent<Button>().onClick.AddListener(Open);

        // ── 모달 ──
        panelRT = MakeRect("Panel", transform, Vector2.zero, Vector2.one);
        Stretch(panelRT);
        var dim = AddImg(panelRT, null, new Color(0f, 0f, 0f, 0.82f));
        dim.raycastTarget = true;
        panelRT.gameObject.AddComponent<Button>().onClick.AddListener(Close);

        var card = MakeRect("Card", panelRT, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        card.sizeDelta = new Vector2(760f, 900f);
        var cardImg = AddImg(card, UISkin.PanelBody, Color.white, true);
        cardImg.raycastTarget = true;   // 카드 안을 눌러도 바깥 탭으로 안 닫히게

        AddLabel(card, "설정", 44f, T95).rectTransform.anchoredPosition = new Vector2(0f, 380f);

        // BGM 슬라이더
        AddLabel(card, "배경음", 26f, T70).rectTransform.anchoredPosition = new Vector2(-260f, 220f);
        bgmSlider = MakeSlider(card, new Vector2(0f, 170f), GameAudioSettings.Bgm,
                                v => GameAudioSettings.Bgm = v);

        // 효과음 슬라이더
        AddLabel(card, "효과음", 26f, T70).rectTransform.anchoredPosition = new Vector2(-260f, 60f);
        sfxSlider = MakeSlider(card, new Vector2(0f, 10f), GameAudioSettings.Sfx,
                                v => GameAudioSettings.Sfx = v);

        // 라이선스 정보 버튼
        var licBtn = MakeRect("LicenseBtn", card, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        licBtn.sizeDelta = new Vector2(500f, 84f);
        licBtn.anchoredPosition = new Vector2(0f, -140f);
        // AddImg는 기본 raycastTarget=false(장식용 전제)다 — 이 버튼처럼 그
        // 이미지 자체가 클릭 대상이어야 하면 켜줘야 한다. 안 그러면 클릭이
        // 이미지를 그냥 통과해 카드 배경(cardImg, 핸들러 없음)을 거쳐 Panel의
        // "바깥 탭 닫기" 핸들러까지 뚫고 올라간다 — "라이선스 정보를 눌러도
        // 그냥 설정이 닫힌다"는 형태로 나타나는, 눈에 잘 안 띄는 버그였다.
        AddImg(licBtn, UISkin.DepthButton(UISkin.Accent.Blue), Color.white, true).raycastTarget = true;
        AddLabel(licBtn, "라이선스 정보", 26f, Color.white);
        licBtn.gameObject.AddComponent<Button>().onClick.AddListener(OpenLicense);

        // 닫기
        var closeBtn = MakeRect("Close", card, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f));
        closeBtn.pivot = new Vector2(0.5f, 0f);
        closeBtn.sizeDelta = new Vector2(340f, 88f);
        closeBtn.anchoredPosition = new Vector2(0f, 30f);
        AddImg(closeBtn, UISkin.DepthButton(UISkin.Accent.Grey), Color.white, true);
        AddLabel(closeBtn, "닫기", 28f, Color.white);
        closeBtn.gameObject.AddComponent<Button>().onClick.AddListener(Close);

        BuildLicensePanel(card);

        panelRT.gameObject.SetActive(false);
    }

    void BuildLicensePanel(Transform cardParent)
    {
        // 설정 카드와 같은 자리를 완전히 덮는 서브패널. 뒤로가기를 누르면 다시 설정으로.
        licenseRT = MakeRect("LicensePanel", panelRT, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        licenseRT.sizeDelta = new Vector2(760f, 900f);
        var bg = AddImg(licenseRT, UISkin.PanelBody, Color.white, true);
        bg.raycastTarget = true;

        AddLabel(licenseRT, "라이선스 정보", 36f, T95).rectTransform.anchoredPosition = new Vector2(0f, 380f);

        // 이 프로젝트가 실제로 가져다 쓴 리소스만 정확히 적는다 — 없는 걸 지어내지 않는다.
        string body =
            "<b>Kenney UI Pack / Game Icons</b>\n" +
            "Kenney (kenney.nl) · CC0 1.0\n" +
            "저작자 표시 의무 없음, 자유 이용\n\n" +
            "<b>화투 카드 일러스트</b>\n" +
            "Spencjo (Wikimedia Commons) · CC BY-SA 4.0\n" +
            "원본 수정 없이/일부 수정하여 사용\n" +
            "라이선스 전문: creativecommons.org/licenses/by-sa/4.0\n\n" +
            "<b>Twemoji</b>\n" +
            "Twitter, Inc / X Corp · CC BY 4.0\n" +
            "고스톱 이벤트 아이콘(뻑/쪽/따닥/쓸/스톱 등)에 사용\n" +
            "라이선스 전문: creativecommons.org/licenses/by/4.0\n\n" +
            "<b>ONE Mobile POP 폰트</b>\n" +
            "티몬 · OFL 1.1 (무료 상업적 이용 허용)";

        var textRT = MakeRect("Body", licenseRT, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        textRT.sizeDelta = new Vector2(660f, 620f);
        textRT.anchoredPosition = new Vector2(0f, 30f);
        var tmp = textRT.gameObject.AddComponent<TextMeshProUGUI>();
        tmp.font = Resources.Load<TMP_FontAsset>("TextMesh Pro/Fonts/ONE Mobile POP SDF");
        tmp.text = body; tmp.fontSize = 22f; tmp.color = T95;
        tmp.alignment = TextAlignmentOptions.TopLeft;
        tmp.textWrappingMode = TextWrappingModes.Normal;
        tmp.lineSpacing = 6f;

        var back = MakeRect("Back", licenseRT, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f));
        back.pivot = new Vector2(0.5f, 0f);
        back.sizeDelta = new Vector2(340f, 88f);
        back.anchoredPosition = new Vector2(0f, 30f);
        // 위 licBtn과 같은 이유 — 이 버튼은 licenseRT 배경(bg, 핸들러 없음)을
        // 거쳐 Panel의 "바깥 탭 닫기"까지 뚫고 올라가는 자리라 더 심각했다:
        // "라이선스에서 뒤로를 누르면 설정 카드로 안 돌아가고 팝업 전체가
        // 닫힌다"는 형태로 나타난다.
        AddImg(back, UISkin.DepthButton(UISkin.Accent.Grey), Color.white, true).raycastTarget = true;
        AddLabel(back, "뒤로", 28f, Color.white);
        back.gameObject.AddComponent<Button>().onClick.AddListener(() => licenseRT.gameObject.SetActive(false));

        licenseRT.gameObject.SetActive(false);
    }

    Slider MakeSlider(Transform parent, Vector2 pos, float initial, System.Action<float> onChange)
    {
        var root = MakeRect("Slider", parent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        root.sizeDelta = new Vector2(560f, 40f);
        root.anchoredPosition = pos;

        var track = AddImg(root, UISkin.SliderTrack, Surface2, true);

        var fillArea = MakeRect("FillArea", root, new Vector2(0f, 0.5f), new Vector2(1f, 0.5f));
        fillArea.sizeDelta = new Vector2(0f, 24f);
        var fill = MakeRect("Fill", fillArea, new Vector2(0f, 0f), new Vector2(1f, 1f));
        Stretch(fill);
        var fillImg = AddImg(fill, UISkin.SliderTrack, Accent, true);

        var handleArea = MakeRect("HandleArea", root, new Vector2(0f, 0.5f), new Vector2(1f, 0.5f));
        Stretch(handleArea);
        var handle = MakeRect("Handle", handleArea, new Vector2(0.5f, 0.5f), Vector2.zero);
        handle.sizeDelta = new Vector2(36f, 44f);
        var handleImg = AddImg(handle, UISkin.SliderKnob, Color.white);

        var slider = root.gameObject.AddComponent<Slider>();
        slider.targetGraphic = handleImg;
        slider.fillRect   = fill;
        slider.handleRect = handle;
        slider.direction  = Slider.Direction.LeftToRight;
        slider.minValue = 0f; slider.maxValue = 1f;
        slider.value = initial;
        slider.onValueChanged.AddListener(v => onChange(v));
        return slider;
    }

    public void Open()  { panelRT.gameObject.SetActive(true); panelRT.SetAsLastSibling(); }
    public void Close() { panelRT.gameObject.SetActive(false); }
    void OpenLicense()  { licenseRT.gameObject.SetActive(true); licenseRT.SetAsLastSibling(); }

    // ── 헬퍼 ─────────────────────────────────────────────
    static RectTransform MakeRect(string name, Transform parent, Vector2 aMin, Vector2 aMax)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = aMin; rt.anchorMax = aMax;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        return rt;
    }

    static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
    }

    static Image AddImg(RectTransform rt, Sprite sp, Color col, bool sliced = false)
    {
        var img = rt.gameObject.AddComponent<Image>();
        if (sp != null) { img.sprite = sp; if (sliced) img.type = Image.Type.Sliced; }
        img.color = col; img.raycastTarget = false;
        return img;
    }

    static TextMeshProUGUI AddLabel(Transform parent, string text, float size, Color col)
    {
        var go = new GameObject("Label", typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(400f, 60f);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.font = Resources.Load<TMP_FontAsset>("TextMesh Pro/Fonts/ONE Mobile POP SDF");
        tmp.text = text; tmp.fontSize = size; tmp.color = col;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.raycastTarget = false;
        return tmp;
    }
}
