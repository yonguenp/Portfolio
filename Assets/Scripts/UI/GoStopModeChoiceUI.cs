using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// 타이틀에서 고스톱 카드를 누르면 뜨는 인원수 선택 팝업 — 2인(맞고)인지
/// 3인(고스톱)인지 고른다. 어느 쪽을 골라도 <c>GoStop3PScene</c>(2~4인을
/// 전부 처리하는 <see cref="GoStop3PGame"/>) 하나로 들어간다 —
/// 2026-08-26에 2인 전용 GoStopScene/GoStopGame.cs를 삭제했다.
/// <see cref="TitleOptionsUI"/>와 같은 패턴(Create 정적 팩토리 + 코드로
/// 직접 UI 생성)을 따른다 — 타이틀은 씬에 직접 만들어져 있지만, 이런 새
/// 오버레이는 이미 코드 생성 전례(설정 팝업)가 있다.
/// </summary>
public class GoStopModeChoiceUI : MonoBehaviour
{
    RectTransform panelRT;
    GoStopNetLobbyUI netLobby;

    // 2026-08-25 — Kenney 밝은 Depth 스킨 통일(TitleOptionsUI와 같은 이유·
    // 같은 패턴). 카드 배경이 밝아져서 T95/T70(이름 유지, 값만 어두운
    // 남색으로)이 카드 위 텍스트를 담당하고, Depth 버튼(선택지 행·닫기)
    // 위의 라벨은 항상 흰색을 직접 넘긴다.
    static readonly Color T95 = new Color(0.106f, 0.133f, 0.267f, 0.95f);
    static readonly Color T70 = new Color(0.106f, 0.133f, 0.267f, 0.70f);

    public static GoStopModeChoiceUI Create(RectTransform canvasRT)
    {
        var go = new GameObject("GoStopModeChoiceUI", typeof(RectTransform));
        go.transform.SetParent(canvasRT, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;

        var ui = go.AddComponent<GoStopModeChoiceUI>();
        ui.Build(canvasRT);
        return ui;
    }

    void Build(RectTransform canvasRT)
    {
        panelRT = MakeRect("Panel", transform, Vector2.zero, Vector2.one);
        Stretch(panelRT);
        var dim = AddImg(panelRT, null, new Color(0f, 0f, 0f, 0.82f));
        dim.raycastTarget = true;
        panelRT.gameObject.AddComponent<Button>().onClick.AddListener(Close);

        var card = MakeRect("Card", panelRT, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        card.sizeDelta = new Vector2(760f, 700f);
        var cardImg = AddImg(card, UISkin.PanelBody, Color.white, true);
        cardImg.raycastTarget = true;

        AddLabel(card, "고스톱", 44f, T95).rectTransform.anchoredPosition = new Vector2(0f, 290f);
        AddLabel(card, "인원수를 고르세요", 24f, T70).rectTransform.anchoredPosition = new Vector2(0f, 230f);

        // 2026-08-23(씬 통합): GoStop3PGame이 SEATS=2(맞고)까지 처리할 수
        // 있게 확장돼 2인도 이제 GoStop3PScene을 연다 — GoStop3PGame.
        // PendingOfflineSeatCount를 미리 세팅해 두면 그 씬의 Awake()가
        // 읽어서 좌석 수를 맞춘다(네트워크 로비가 없는 오프라인 경로라
        // 로비 인스턴스 대신 이 static 값으로 전달한다).
        MakeChoiceButton(card, new Vector2(0f, 110f), UISkin.Accent.Blue, "2인 (맞고)", "손패 10장 · 필드 8장 · 부가 규칙 전부",
            () => { GoStop3PGame.PendingOfflineSeatCount = 2; SceneManager.LoadScene("GoStop3PScene"); });
        MakeChoiceButton(card, new Vector2(0f, -20f), UISkin.Accent.Green, "3인 (고스톱)", "손패 7장 · 필드 6장 · 독박·개인별 광박/피박",
            () => { GoStop3PGame.PendingOfflineSeatCount = 3; SceneManager.LoadScene("GoStop3PScene"); });
        // 2026-08-19: 로컬 네트워크(같은 와이파이) 대전 — IP 입력 없이 자동으로
        // 방을 찾는다(사용자 확인 요청). 이 버튼은 씬을 바로 안 열고 로비
        // 팝업(GoStopNetLobbyUI)을 연다 — 실제 씬 전환은 그 팝업이 호스트가
        // "시작"을 누른 뒤 GoStopNetLobby.OnGameStarting을 받아서 한다.
        MakeChoiceButton(card, new Vector2(0f, -150f), UISkin.Accent.Yellow, "네트워크 대전", "같은 와이파이 · 자동으로 방을 찾습니다",
            () => { Close(); netLobby?.Open(); });

        var closeBtn = MakeRect("Close", card, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f));
        closeBtn.pivot = new Vector2(0.5f, 0f);
        closeBtn.sizeDelta = new Vector2(300f, 76f);
        closeBtn.anchoredPosition = new Vector2(0f, 24f);
        AddImg(closeBtn, UISkin.DepthButton(UISkin.Accent.Grey), Color.white, true);
        AddLabel(closeBtn, "닫기", 24f, Color.white);
        closeBtn.gameObject.AddComponent<Button>().onClick.AddListener(Close);

        panelRT.gameObject.SetActive(false);

        // 같은 캔버스에 로비 팝업도 함께 만들어둔다(TitleOptionsUI/이
        // 클래스 자신과 같은 "타이틀에서 한 번만 생성" 패턴).
        netLobby = GoStopNetLobbyUI.Create(canvasRT);
    }

    void MakeChoiceButton(Transform parent, Vector2 pos, UISkin.Accent accent, string title, string sub, UnityEngine.Events.UnityAction onClick)
    {
        var btn = MakeRect("Choice", parent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        btn.sizeDelta = new Vector2(660f, 116f);
        btn.anchoredPosition = pos;
        var btnImg = AddImg(btn, UISkin.DepthButton(accent), Color.white, true);
        // AddImg는 기본 raycastTarget=false다(장식용 이미지 전제) — 이 버튼은
        // 자기 이미지 위에서 직접 클릭을 받아야 하는데, false로 두면 클릭이
        // 이 위를 그냥 통과해 버려서 밑에 깔린 카드 배경(핸들러 없음)을 거쳐
        // 결국 Panel의 "바깥 탭 닫기" 버튼까지 뚫고 올라간다 — "2인/3인을
        // 눌러도 팝업만 닫힌다"는 신고의 원인이었다.
        btnImg.raycastTarget = true;

        // 2026-08-25 — Depth 버튼은 색이 이미 구워져 있어 라벨은 항상
        // 흰색(카드 위 어두운 T95/T70과 반대).
        var titleLbl = AddLabel(btn, title, 28f, Color.white);
        titleLbl.rectTransform.anchoredPosition = new Vector2(0f, 20f);
        var subLbl = AddLabel(btn, sub, 16f, new Color(1f, 1f, 1f, 0.85f));
        subLbl.rectTransform.anchoredPosition = new Vector2(0f, -22f);
        subLbl.rectTransform.sizeDelta = new Vector2(600f, 40f);

        btn.gameObject.AddComponent<Button>().onClick.AddListener(onClick);
    }

    public void Open()  { panelRT.gameObject.SetActive(true); panelRT.SetAsLastSibling(); }
    public void Close() { panelRT.gameObject.SetActive(false); }

    // ── 헬퍼 (TitleOptionsUI와 동일 패턴) ────────────────────
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
        rt.sizeDelta = new Vector2(660f, 60f);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.font = Resources.Load<TMP_FontAsset>("TextMesh Pro/Fonts/ONE Mobile POP SDF");
        tmp.text = text; tmp.fontSize = size; tmp.color = col;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.raycastTarget = false;
        return tmp;
    }
}
