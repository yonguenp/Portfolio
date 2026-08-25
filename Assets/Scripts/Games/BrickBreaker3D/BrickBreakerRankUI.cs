using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 랭킹 보드.
///
/// 조준 UI(<see cref="BrickBreakerAimUI"/>)와 달리 <b>GraphicRaycaster를 붙인다.</b>
/// 저쪽은 HUD 레이캐스트를 가로채면 안 되는 상시 UI라 raw 입력으로 돌지만,
/// 이건 열려 있는 동안 뒤를 다 막아야 하는 모달이라 막는 게 오히려 맞다.
///
/// 닫혀 있을 때 화면을 먹지 않는 이유: 루트에 전체 화면 Image를 두지 않고
/// 딤 패널을 통째로 비활성화하기 때문이다. GraphicRaycaster는 실제 그래픽이
/// 있는 자리만 맞히므로, 열기 버튼(칩) 말고는 전부 통과한다.
/// </summary>
public class BrickBreakerRankUI : MonoBehaviour
{
    public static BrickBreakerRankUI Instance { get; private set; }

    const int ROWS = LocalRankingStore.Capacity;

    RectTransform canvasRT;
    RectTransform safeRT;   // 칩처럼 화면 가장자리에 붙는 것만 여기에
    RectTransform chipRT;         // 항상 보이는 "랭킹" 열기 버튼
    RectTransform panelRT;        // 모달 전체 (딤 + 카드)
    RectTransform cardRT;
    RectTransform closeRT;
    readonly List<RectTransform> rowRT = new();

    RectTransform tabAllRT, tabMineRT, nameRowRT;
    TextMeshProUGUI tabAllTxt, tabMineTxt, statusTxt, nameTxt;
    Image tabAllBg, tabMineBg;

    /// <summary>0 = 전체 랭킹(서버), 1 = 내 기록(이 기기).</summary>
    int tab;

    // 세로 1080×1920 기준 치수. 화면이 더 낮으면 Layout()이 줄인다.
    const float CARD_H_MAX = 1240f;
    const float ROW_H_MAX  = 88f;
    const float TOP        = 250f;   // 카드 위 ~ 첫 줄 (제목·모드·탭·상태 아래)
    const float GAP        = 6f;
    const float FOOT       = 200f;   // 닉네임 줄 + 닫기 버튼 영역
    TextMeshProUGUI modeTxt;
    readonly List<TextMeshProUGUI> rankTxt  = new();
    readonly List<TextMeshProUGUI> scoreTxt = new();
    readonly List<TextMeshProUGUI> infoTxt  = new();
    readonly List<Image>           rowBg    = new();
    TextMeshProUGUI emptyTxt;

    /// <summary>이번 판 기록을 강조할 순위(1-based). 0이면 강조 없음.</summary>
    int highlight;

    /// <summary>게임오버가 정해 두면 칩으로 열었을 때도 그 줄이 강조된다.</summary>
    public int PendingHighlight { get; set; }

    public bool IsOpen => panelRT != null && panelRT.gameObject.activeSelf;

    // 2026-08-25 — Kenney 밝은 Depth 스킨 통일. 카드/칩 바깥 틀은
    // PanelBody(밝은 바탕)로 바꾸고, 안쪽 줄·탭처럼 상태에 따라 색이
    // 바뀌어야 하는 요소는 계속 틴트 가능한 Panel(회색 원본)을 쓴다 —
    // 그래서 그 배경 톤도 어두운 Surface에서 밝은 회색조로 같이 뒤집었다.
    // T95/T70/T40은 이름을 유지하되 값을 어두운 남색 계열로 바꿨다.
    static readonly Color Surface  = new Color(0.90f, 0.91f, 0.94f, 1f);   // 밝은 줄 배경(예전 #1B2244)
    static readonly Color Surface2 = new Color(0.929f, 0.729f, 0.180f, 1f); // 선택된 탭 = 강조색(예전 #2B3560)
    static readonly Color Accent   = new Color(0.929f, 0.729f, 0.180f, 1f);   // #EDBA2E
    static readonly Color T95 = new Color(0.106f, 0.133f, 0.267f, 0.95f);
    static readonly Color T70 = new Color(0.106f, 0.133f, 0.267f, 0.70f);
    static readonly Color T40 = new Color(0.106f, 0.133f, 0.267f, 0.40f);

    static string L(string key, string fallback)
    {
        var loc = LocalizationManager.Instance;
        return loc != null ? loc.GetOr(key, fallback) : fallback;
    }

    public static BrickBreakerRankUI Create()
    {
        var go = new GameObject("RankUICanvas");
        var cv = go.AddComponent<Canvas>();
        cv.renderMode   = RenderMode.ScreenSpaceOverlay;
        cv.sortingOrder = 60;                       // 조준 UI(51)보다 위
        var cs = go.AddComponent<CanvasScaler>();
        cs.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        cs.referenceResolution = new Vector2(1080f, 1920f);
        cs.matchWidthOrHeight  = 0.5f;
        go.AddComponent<GraphicRaycaster>();

        var ui = go.AddComponent<BrickBreakerRankUI>();
        ui.canvasRT = go.GetComponent<RectTransform>();

        // 모달 딤은 화면 전체를 덮어야 하므로 캔버스 직속이지만,
        // 상시 노출되는 "랭킹" 칩은 노치에 걸리므로 safe area 안에 둔다.
        var safe = new GameObject("SafeArea", typeof(RectTransform));
        safe.transform.SetParent(go.transform, false);
        var safeRT = safe.GetComponent<RectTransform>();
        safeRT.anchorMin = Vector2.zero; safeRT.anchorMax = Vector2.one;
        safeRT.offsetMin = Vector2.zero; safeRT.offsetMax = Vector2.zero;
        safe.AddComponent<SafeArea>();
        ui.safeRT = safeRT;

        ui.Build();
        Instance = ui;
        return ui;
    }

    void OnDestroy() { if (Instance == this) Instance = null; }

    // ── 생성 ─────────────────────────────────────────────
    void Build()
    {
        BuildChip();
        BuildPanel();
        panelRT.gameObject.SetActive(false);
    }

    void BuildChip()
    {
        // 스탯 칩(-192, 높이 46) 아래. 터널이 화면 중앙이라 왼쪽 띠는 비어 있다.
        chipRT = MakeRect("RankChip", safeRT ?? canvasRT, new Vector2(0f, 1f), new Vector2(0f, 1f));
        chipRT.pivot            = new Vector2(0f, 1f);
        chipRT.sizeDelta        = new Vector2(196f, 52f);
        chipRT.anchoredPosition = new Vector2(22f, -252f);

        var img = AddImage(chipRT, UISkin.DepthButton(UISkin.Accent.Blue));
        img.color         = Color.white;
        img.raycastTarget = true;

        var lbl = MakeLabel(chipRT, L("bb_rank", "랭킹"), 24f);
        lbl.color = Color.white;
        Stretch(lbl.rectTransform);

        chipRT.gameObject.AddComponent<Button>().onClick.AddListener(Open);
    }

    void BuildPanel()
    {
        panelRT = MakeRect("RankPanel", canvasRT, Vector2.zero, Vector2.one);

        // 딤이 곧 모달 차단막이다 — raycastTarget을 켜 둬야 뒤가 안 눌린다.
        var dim = AddImage(panelRT, null);
        dim.color         = new Color(0f, 0f, 0f, 0.82f);
        dim.raycastTarget = true;
        panelRT.gameObject.AddComponent<Button>().onClick.AddListener(Close);  // 바깥 탭 = 닫기

        cardRT = MakeRect("Card", panelRT, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        var card = cardRT;
        card.sizeDelta        = new Vector2(920f, CARD_H_MAX);
        card.anchoredPosition = Vector2.zero;
        var cardImg = AddImage(card, UISkin.PanelBody);
        cardImg.color         = Color.white;
        cardImg.raycastTarget = true;    // 카드 안을 눌러 실수로 닫히지 않게

        var title = MakeLabel(card, L("bb_rank", "랭킹"), 56f);
        title.color = T95;
        title.rectTransform.anchorMin        = new Vector2(0f, 1f);
        title.rectTransform.anchorMax        = new Vector2(1f, 1f);
        title.rectTransform.pivot            = new Vector2(0.5f, 1f);
        title.rectTransform.sizeDelta        = new Vector2(0f, 72f);
        title.rectTransform.anchoredPosition = new Vector2(0f, -34f);

        modeTxt = MakeLabel(card, "", 28f);
        modeTxt.color                        = T70;
        modeTxt.rectTransform.anchorMin      = new Vector2(0f, 1f);
        modeTxt.rectTransform.anchorMax      = new Vector2(1f, 1f);
        modeTxt.rectTransform.pivot          = new Vector2(0.5f, 1f);
        modeTxt.rectTransform.sizeDelta      = new Vector2(0f, 40f);
        modeTxt.rectTransform.anchoredPosition = new Vector2(0f, -108f);

        // ── 탭: 전체 랭킹 / 내 기록 ──────────────────────
        tabAllRT  = MakeTab(card, L("bb_rank_all",  "전체 랭킹"), -152f, out tabAllBg,  out tabAllTxt);
        tabMineRT = MakeTab(card, L("bb_rank_mine", "내 기록"),    152f, out tabMineBg, out tabMineTxt);
        tabAllRT.gameObject.AddComponent<Button>().onClick.AddListener(() => SetTab(0));
        tabMineRT.gameObject.AddComponent<Button>().onClick.AddListener(() => SetTab(1));

        // 서버가 안 되면 조용히 로컬로 폴백하는데, 그때 왜 남의 기록이 없는지
        // 알려주지 않으면 버그처럼 보인다.
        statusTxt = MakeLabel(card, "", 22f);
        statusTxt.color = Accent;
        statusTxt.rectTransform.anchorMin = new Vector2(0f, 1f);
        statusTxt.rectTransform.anchorMax = new Vector2(1f, 1f);
        statusTxt.rectTransform.pivot     = new Vector2(0.5f, 1f);
        statusTxt.rectTransform.sizeDelta = new Vector2(0f, 32f);
        statusTxt.rectTransform.anchoredPosition = new Vector2(0f, -214f);

        for (int i = 0; i < ROWS; i++)
        {
            var row = MakeRect($"Row{i}", card, new Vector2(0f, 1f), new Vector2(1f, 1f));
            row.pivot            = new Vector2(0.5f, 1f);
            row.sizeDelta        = new Vector2(-48f, ROW_H_MAX);
            row.anchoredPosition = new Vector2(0f, -(TOP + i * (ROW_H_MAX + GAP)));
            rowRT.Add(row);

            var bg = AddImage(row, Rounded());
            bg.color         = new Color(1f, 1f, 1f, 0.05f);
            bg.raycastTarget = false;
            rowBg.Add(bg);

            var rk = MakeLabel(row, "", 34f);
            rk.alignment = TextAlignmentOptions.Center;
            rk.rectTransform.anchorMin = new Vector2(0f, 0f);
            rk.rectTransform.anchorMax = new Vector2(0f, 1f);
            rk.rectTransform.pivot     = new Vector2(0f, 0.5f);
            rk.rectTransform.sizeDelta = new Vector2(96f, 0f);
            rk.rectTransform.anchoredPosition = new Vector2(10f, 0f);
            rankTxt.Add(rk);

            var sc = MakeLabel(row, "", 40f);
            sc.alignment           = TextAlignmentOptions.Left;
            // NoWrap만 건다. 이름이 길어도 두 줄로 넘어가 다음 줄을 덮지 않게 하려는 것.
            // Ellipsis는 쓰지 말 것 — TMP는 rect **높이**를 넘어도 텍스트를 통째로
            // 감춰서, 가로 화면처럼 줄 높이가 폰트보다 빠듯하면 점수까지 사라진다.
            sc.textWrappingMode    = TextWrappingModes.NoWrap;
            sc.rectTransform.anchorMin = new Vector2(0f, 0f);
            sc.rectTransform.anchorMax = new Vector2(1f, 1f);
            sc.rectTransform.offsetMin = new Vector2(118f, 0f);
            sc.rectTransform.offsetMax = new Vector2(-300f, 0f);
            scoreTxt.Add(sc);

            var inf = MakeLabel(row, "", 25f);
            inf.alignment = TextAlignmentOptions.Right;
            inf.color     = T70;
            inf.rectTransform.anchorMin = new Vector2(1f, 0f);
            inf.rectTransform.anchorMax = new Vector2(1f, 1f);
            inf.rectTransform.pivot     = new Vector2(1f, 0.5f);
            inf.rectTransform.sizeDelta = new Vector2(290f, 0f);
            inf.rectTransform.anchoredPosition = new Vector2(-18f, 0f);
            infoTxt.Add(inf);
        }

        emptyTxt = MakeLabel(card, L("bb_rank_empty", "아직 기록이 없습니다"), 30f);
        emptyTxt.color = T70;
        Stretch(emptyTxt.rectTransform);
        emptyTxt.gameObject.SetActive(false);

        // ── 닉네임 ───────────────────────────────────────
        nameRowRT = MakeRect("NameRow", card, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f));
        nameRowRT.pivot            = new Vector2(0.5f, 0f);
        nameRowRT.sizeDelta        = new Vector2(820f, 62f);
        nameRowRT.anchoredPosition = new Vector2(0f, 126f);
        nameTxt = MakeLabel(nameRowRT, "", 24f);
        nameTxt.color = T70;
        Stretch(nameTxt.rectTransform);

        closeRT = MakeRect("Close", card, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f));
        var close = closeRT;
        close.pivot            = new Vector2(0.5f, 0f);
        close.sizeDelta        = new Vector2(340f, 88f);
        close.anchoredPosition = new Vector2(0f, 28f);
        // 2026-08-25 — Surface2가 이제 강조색(골드)이라 그 위에 기본 흰 글자를
        // 얹으면 "노란 배경 위 흰 글자는 안 읽힌다"는 이 프로젝트의 반복된
        // 함정에 또 걸린다. 닫기는 중립 회색 Depth 버튼으로 분리했다.
        var ci = AddImage(close, UISkin.DepthButton(UISkin.Accent.Grey));
        ci.color         = Color.white;
        ci.raycastTarget = true;
        var closeLbl = MakeLabel(close, L("btn_close", "닫기"), 32f);
        closeLbl.color = Color.white;
        Stretch(closeLbl.rectTransform);
        close.gameObject.AddComponent<Button>().onClick.AddListener(Close);
    }

    RectTransform MakeTab(Transform parent, string label, float x,
                          out Image bg, out TextMeshProUGUI txt)
    {
        var rt = MakeRect("Tab", parent, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));
        rt.pivot            = new Vector2(0.5f, 1f);
        rt.sizeDelta        = new Vector2(290f, 62f);
        rt.anchoredPosition = new Vector2(x, -146f);
        bg = AddImage(rt, Rounded());
        bg.raycastTarget = true;
        txt = MakeLabel(rt, label, 26f);
        Stretch(txt.rectTransform);
        return rt;
    }

    void SetTab(int t)
    {
        tab = t;
        ApplyTabLook();
        LoadForTab();
    }

    void ApplyTabLook()
    {
        tabAllBg.color   = tab == 0 ? Surface2 : Surface;
        tabMineBg.color  = tab == 1 ? Surface2 : Surface;
        tabAllTxt.color  = tab == 0 ? T95 : T40;
        tabMineTxt.color = tab == 1 ? T95 : T40;
    }

    void LoadForTab()
    {
        statusTxt.text = "";

        if (tab == 1)
        {
            // "내 기록"은 언제나 이 기기의 로컬 보드다 — 네트워크를 안 탄다.
            new LocalRankingStore().Load(BrickBreakerRules.Mode, Fill);
            nameTxt.text = "";
            return;
        }

        BrickBreakerRanking.Load(list =>
        {
            Fill(list);
            if (!UgsRankingStore.Online)
                statusTxt.text = L("bb_rank_offline", "오프라인 — 이 기기 기록만 표시합니다");
        });

        string me = UgsRankingStore.PlayerName;
        nameTxt.text = string.IsNullOrEmpty(me)
            ? L("bb_rank_noname", "닉네임 없음")
            : string.Format(L("bb_rank_me", "내 이름: {0}"), me);
    }

    // ── 열기/닫기 ────────────────────────────────────────
    public void Open() => Open(PendingHighlight);

    public void Open(int highlightRank)
    {
        highlight = highlightRank;
        modeTxt.text = BrickBreakerRules.NameOf(BrickBreakerRules.Mode);
        panelRT.gameObject.SetActive(true);
        panelRT.SetAsLastSibling();
        Layout();
        ApplyTabLook();
        LoadForTab();
    }

    public void Close() => panelRT.gameObject.SetActive(false);

    /// <summary>
    /// 화면 높이에 카드를 맞춘다.
    ///
    /// 세로 1080×1920에선 고정 치수가 그대로 들어가지만, 에디터 Game 뷰를
    /// 가로로 두면 높이가 1080까지 떨어져 제목과 닫기 버튼이 잘렸다.
    /// 카드를 먼저 줄이고, 남은 높이를 10줄이 나눠 갖는다.
    /// </summary>
    void Layout()
    {
        float avail = canvasRT.rect.height - 160f;          // 위아래 여백
        float cardH = Mathf.Min(CARD_H_MAX, avail);
        cardRT.sizeDelta = new Vector2(cardRT.sizeDelta.x, cardH);

        // 줄 높이는 남은 공간을 10등분하되 원래 치수를 넘지 않는다
        float rowsH = cardH - TOP - FOOT;
        float h     = Mathf.Clamp(rowsH / ROWS - GAP, 34f, ROW_H_MAX);

        for (int i = 0; i < rowRT.Count; i++)
        {
            rowRT[i].sizeDelta        = new Vector2(-48f, h);
            rowRT[i].anchoredPosition = new Vector2(0f, -(TOP + i * (h + GAP)));
        }

        closeRT.anchoredPosition = new Vector2(0f, 28f);
    }

    /// <summary>열기 버튼만 숨긴다(게임오버 오버레이가 떠 있을 때 등).</summary>
    public void SetChipVisible(bool on)
    {
        if (chipRT != null) chipRT.gameObject.SetActive(on);
    }

    void Fill(List<BrickBreakerRecord> list)
    {
        emptyTxt.gameObject.SetActive(list.Count == 0);

        for (int i = 0; i < ROWS; i++)
        {
            bool has = i < list.Count;
            // rowBg[i]는 줄에 붙은 Image다 — .transform.parent를 쓰면 줄이 아니라
            // 카드가 꺼져서 보드 전체가 사라진다. 줄 자체를 직접 껐다 켠다.
            rowRT[i].gameObject.SetActive(has);
            if (!has) continue;

            var r = list[i];
            // 로컬 탭은 이번 판을, 전체 탭은 서버가 알려준 내 줄을 강조한다.
            bool mine = tab == 1 ? (i + 1) == highlight : r.isMe;

            rankTxt[i].text  = (i + 1).ToString();
            scoreTxt[i].text = string.IsNullOrEmpty(r.name)
                ? r.score.ToString("N0")
                : $"{r.score:N0}  <size=55%><alpha=#99>{ShortName(r.name)}</size>";

            // 서버는 제출 시각을 안 준다(ticks=0) — 그때는 날짜 줄을 뺀다.
            infoTxt[i].text = r.ticks > 0
                ? $"{r.turn}턴  {r.combo}콤보\n{r.When:MM/dd}"
                : $"{r.turn}턴  {r.combo}콤보";

            // 이번 판 기록은 금색으로 — 목록에서 자기 줄을 바로 찾게 한다.
            rankTxt[i].color  = mine ? Accent : (i < 3 ? T95 : T70);
            scoreTxt[i].color = mine ? Accent : T95;
            rowBg[i].color    = mine ? new Color(Accent.r, Accent.g, Accent.b, 0.18f) : Surface;
        }
    }

    /// <summary>
    /// UGS 자동 생성 이름은 <c>TropicalMuffledPostcard#84949</c> 처럼 길다.
    /// 뒤의 <c>#숫자</c>가 동명이인을 가르는 부분이라 <b>그건 남기고 앞을 줄인다</b>.
    /// </summary>
    static string ShortName(string name)
    {
        const int MAX = 14;
        int hash = name.LastIndexOf('#');
        string body = hash >= 0 ? name.Substring(0, hash) : name;
        string tag  = hash >= 0 ? name.Substring(hash)    : "";
        if (body.Length > MAX) body = body.Substring(0, MAX - 1) + "…";
        return body + tag;
    }

    // ── 조이스틱 차단 영역 ────────────────────────────────
    /// <summary>열기 칩 자리를 조이스틱이 물지 않게 한다.</summary>
    public void RegisterBlockedZones()
    {
        if (chipRT == null || !chipRT.gameObject.activeSelf) return;
        VirtualJoystick.AddBlockedZone(ScreenRectOf(chipRT, 12f));
    }

    static Rect ScreenRectOf(RectTransform rt, float pad)
    {
        var c = new Vector3[4];
        rt.GetWorldCorners(c);
        return new Rect(c[0].x - pad, c[0].y - pad,
                        (c[2].x - c[0].x) + pad * 2f, (c[2].y - c[0].y) + pad * 2f);
    }

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

    static Image AddImage(RectTransform rt, Sprite sprite)
    {
        var img = rt.gameObject.AddComponent<Image>();
        if (sprite != null)
        {
            img.sprite = sprite;
            // 보더가 있는 스프라이트만 9-slice. 원형처럼 보더 0인 걸 Sliced로 두면
            // 그냥 늘어나기만 하고 의미가 없다.
            img.type = sprite.border.sqrMagnitude > 0f ? Image.Type.Sliced : Image.Type.Simple;
        }
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

    /// <summary>패널 스프라이트. 예전엔 여기서 텍스처를 직접 그렸다 — 지금은 스킨을 쓴다.</summary>
    static Sprite Rounded() => UISkin.Panel;
}
