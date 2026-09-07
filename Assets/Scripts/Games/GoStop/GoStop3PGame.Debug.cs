using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 2026-09-07 — "이슈가 재발되는 케이스가 너무 많은데 특정 상황에서만
/// 재현되는 것들은 확인하기 힘들다, 버튼 누르면 그 상황으로 세팅해서
/// 딱 보여줄 수 있으면 좋겠다"는 요청으로 만든 디버그 시나리오 시스템.
///
/// 원칙 — 버튼은 항상 "무대만 세팅"한다. 시나리오 안에서 카드를 대신
/// 클릭해주거나 팝업에 자동으로 응답하지 않는다 — 실제 버그 대부분이
/// "클릭 이후의 흐름"(actionBusy 게이팅, 애니메이션 순서, 팝업 콜백)에서
/// 나므로, 사용자가 직접 카드를 눌러야 진짜 코드 경로(OnPlayerPlay 등)를
/// 그대로 밟는다 — 리플렉션으로 몰래 우회하면 검증 가치가 떨어진다.
///
/// Debug.isDebugBuild || Application.isEditor로만 노출된다 — 정식 릴리즈
/// 빌드(예: GitHub Pages에 올라가는 WebGL 배포판)에는 절대 안 보인다.
/// </summary>
public partial class GoStop3PGame
{
    public enum DebugScenario
    {
        Bomb, PpeokForm, PpeokResolve, Ddadak, Chok, JokerPark,
        DualPiChoice, FieldChoice, Shake, HongdanEmergency,
        ChodanAchievement, CheongdanBlocked, GwangEmergency, GoStopThreshold,
        SkipButton,
    }

    struct DebugScenarioInfo
    {
        public DebugScenario id;
        public string label;
        public string hint;
    }

    static readonly DebugScenarioInfo[] DebugScenarioList =
    {
        new() { id = DebugScenario.Bomb, label = "폭탄",
            hint = "손 3장+필드 1장(7월). 카드를 내면 4장 캡처 + 크레딧 2 적립 +\n덱도 정상으로 넘어가고 턴도 넘어가야 합니다." },
        new() { id = DebugScenario.PpeokForm, label = "뻑 형성",
            hint = "필드 1장+손 1장(1월), 다음 뒷패도 1월. 카드를 내면 뒷패까지\n합쳐 3장이 필드에 그대로 쌓여야 합니다(캡처 0장)." },
        new() { id = DebugScenario.PpeokResolve, label = "뻑 먹기",
            hint = "필드에 3장 쌓인 무더기(2월)+손 1장. 카드를 내면 4장을 한\n번에 캡처하고 상대 피도 뺏어와야 합니다." },
        new() { id = DebugScenario.Ddadak, label = "따닥",
            hint = "필드 2장+손 1장(3월), 다음 뒷패가 남은 한 장. 필드 선택\n팝업에서 고른 뒤, 뒷패가 나머지를 잡으면 따닥이 떠야 합니다." },
        new() { id = DebugScenario.Chok, label = "쪽",
            hint = "손 1장(4월, 필드에 매칭 없음), 다음 뒷패가 같은 달. 카드를\n내 필드에 얹은 뒤 뒷패가 잡으면 쪽 이펙트가 떠야 합니다." },
        new() { id = DebugScenario.JokerPark, label = "보너스패 파킹",
            hint = "손 1장(5월, 매칭 없음), 다음 뒷패가 보너스패, 그다음이 5월.\n조커가 낸 패 위에 붙었다가 다음 뒷패로 뻑이 형성돼야 합니다." },
        new() { id = DebugScenario.DualPiChoice, label = "9월 열끗 선택",
            hint = "필드 2장(9월, 국화 포함)+손 1장. 필드 선택 팝업에서 국화가\n낀 조합을 고르면 곧바로 열끗/쌍피 선택 팝업이 떠야 합니다." },
        new() { id = DebugScenario.FieldChoice, label = "필드 2장 선택",
            hint = "필드 2장(6월)+손 1장. 카드를 내면 둘 중 하나를 고르는\n팝업이 뜨고, 고른 즉시 2장만 캡처돼야 합니다." },
        new() { id = DebugScenario.Shake, label = "흔들기",
            hint = "손 3장(10월, 필드에 매칭 없음). 카드를 내면 흔들기 선언\n팝업이 떠야 합니다(폭탄이 아니라 흔들기여야 함)." },
        new() { id = DebugScenario.HongdanEmergency, label = "홍단 비상(2/3)",
            hint = "내 획득패에 홍단 2/3장. RebuildUI 즉시 '홍단 비상!'\n이펙트가 화면 상단에 떠야 합니다(클릭 불필요)." },
        new() { id = DebugScenario.ChodanAchievement, label = "초단 완성(3/3)",
            hint = "내 획득패에 초단 3/3장 전부. 곧바로 '초단 완성!' 벡터\n카드 이펙트가 화면 중앙에 떠야 합니다(클릭 불필요)." },
        new() { id = DebugScenario.CheongdanBlocked, label = "청단 실패(막힘)",
            hint = "내 획득패 청단 2장, 상대가 나머지 1장 보유. 곧바로\n'청단 실패' 이펙트가 떠야 합니다(클릭 불필요)." },
        new() { id = DebugScenario.GwangEmergency, label = "3광 비상(2장)",
            hint = "내 획득패에 광 2장. 3광 전용 판정 경로(홍단 등과 다른\n임계값)가 정상 발동하는지 확인합니다(클릭 불필요)." },
        new() { id = DebugScenario.GoStopThreshold, label = "고/스톱 임계값",
            hint = "내 획득패가 이미 광 3장(3점), 손 1장(매칭 없음). 카드를\n내면 즉시 고/스톱 팝업이 떠야 합니다." },
        new() { id = DebugScenario.SkipButton, label = "결과 넘기기 버튼",
            hint = "내가 이번 판 쉬는 걸로 강제 설정. Hand 자리에 '결과\n넘기기' 버튼이 즉시 나타나야 합니다(클릭 불필요)." },
    };

    Button debugToggleBtn;
    RectTransform debugDim, debugPanel, debugContent;
    TextMeshProUGUI debugStatusText;

    /// <summary>ExitBtn/HelpBtn과 같은 자리에, 그 오른쪽에 이어 붙는다.
    /// 릴리즈 빌드에는 버튼 자체를 아예 안 만든다.</summary>
    void BuildDebugToggle(RectTransform root, Button anchorBtn)
    {
        if (!(Debug.isDebugBuild || Application.isEditor)) return;

        var existing = root.Find("DebugToggleBtn");
        if (existing != null)
        {
            debugToggleBtn = existing.GetComponent<Button>();
            debugToggleBtn.onClick.RemoveAllListeners();
            debugToggleBtn.onClick.AddListener(ToggleDebugPanel);
        }
        else
        {
            var anchorRT = anchorBtn.GetComponent<RectTransform>();
            debugToggleBtn = UISkin.MakeKenneyButton(root, "DebugToggleBtn", new Vector2(120f, 52f), Vector2.zero,
                UISkin.Accent.Grey, "디버그", ToggleDebugPanel);
            var rt = debugToggleBtn.GetComponent<RectTransform>();
            rt.anchorMin = anchorRT.anchorMin;
            rt.anchorMax = anchorRT.anchorMax;
            rt.pivot = anchorRT.pivot;
            rt.anchoredPosition = anchorRT.anchoredPosition + new Vector2(anchorRT.sizeDelta.x + 10f, 0f);
        }

        BuildDebugPanelContent(root.parent.parent as RectTransform); // Canvas 레벨(Overlay와 같은 층)
        debugDim?.gameObject.SetActive(false);
    }

    void BuildDebugPanelContent(RectTransform canvasRoot)
    {
        if (canvasRoot == null || debugDim != null) return;

        var dimGo = new GameObject("DebugDim", typeof(RectTransform), typeof(Image));
        debugDim = dimGo.GetComponent<RectTransform>();
        debugDim.SetParent(canvasRoot, false);
        debugDim.anchorMin = Vector2.zero;
        debugDim.anchorMax = Vector2.one;
        debugDim.offsetMin = Vector2.zero;
        debugDim.offsetMax = Vector2.zero;
        var dimImg = dimGo.GetComponent<Image>();
        dimImg.color = new Color(0f, 0f, 0f, 0.78f);
        dimImg.raycastTarget = true;

        var panel = UISkin.MakeKenneyPanel(debugDim, "DebugPanel", new Vector2(760f, 900f), Vector2.zero,
            UISkin.Accent.Grey, "디버그 — 버그 재현 시나리오", ToggleDebugPanel);
        debugPanel = panel.root;

        debugStatusText = HwatuUI.MakeLabel(panel.body, new Vector2(0f, -8f), new Vector2(700f, 32f), 20f,
            new Color(0.15f, 0.15f, 0.15f, 0.8f));
        debugStatusText.alignment = TextAlignmentOptions.Center;
        var statusRT = debugStatusText.GetComponent<RectTransform>();
        statusRT.anchorMin = statusRT.anchorMax = new Vector2(0.5f, 1f);
        statusRT.pivot = new Vector2(0.5f, 1f);
        debugStatusText.text = "버튼을 누르면 즉시 그 상황으로 세팅됩니다.";

        var (viewport, content) = HwatuUI.MakeScrollBody(panel.body, new Vector2(700f, 780f), new Vector2(0f, -44f));
        debugContent = content;

        const float rowH = 108f, gap = 8f;
        float y = 0f;
        foreach (var info in DebugScenarioList)
        {
            var rowRT = HwatuUI.MakeRect(info.id.ToString(), content, new Vector2(700f, rowH), new Vector2(0f, -y));
            rowRT.anchorMin = rowRT.anchorMax = new Vector2(0.5f, 1f);
            rowRT.pivot = new Vector2(0.5f, 1f);
            var rowImg = rowRT.gameObject.AddComponent<Image>();
            UISkin.Apply(rowImg, UISkin.PanelBody);

            var btnGo = new GameObject("Btn", typeof(RectTransform), typeof(Image), typeof(Button));
            var btnRT = btnGo.GetComponent<RectTransform>();
            btnRT.SetParent(rowRT, false);
            btnRT.anchorMin = Vector2.zero; btnRT.anchorMax = Vector2.one;
            btnRT.offsetMin = Vector2.zero; btnRT.offsetMax = Vector2.zero;
            var btnImg = btnGo.GetComponent<Image>();
            btnImg.color = new Color(0f, 0f, 0f, 0f); // 클릭 판정 전용, 배경은 rowImg가 담당
            var btn = btnGo.GetComponent<Button>();
            btn.targetGraphic = btnImg;
            var scenario = info.id;
            btn.onClick.AddListener(() => ApplyDebugScenario(scenario));

            var labelText = HwatuUI.MakeLabel(rowRT, new Vector2(20f, -12f), new Vector2(660f, 30f), 26f, Color.black);
            labelText.alignment = TextAlignmentOptions.TopLeft;
            labelText.fontStyle = FontStyles.Bold;
            labelText.text = info.label;
            var labelRT = labelText.GetComponent<RectTransform>();
            labelRT.anchorMin = labelRT.anchorMax = new Vector2(0f, 1f);
            labelRT.pivot = new Vector2(0f, 1f);

            var hintText = HwatuUI.MakeLabel(rowRT, new Vector2(20f, -46f), new Vector2(660f, 56f), 17f,
                new Color(0.25f, 0.25f, 0.25f, 0.9f));
            hintText.alignment = TextAlignmentOptions.TopLeft;
            hintText.text = info.hint;
            var hintRT = hintText.GetComponent<RectTransform>();
            hintRT.anchorMin = hintRT.anchorMax = new Vector2(0f, 1f);
            hintRT.pivot = new Vector2(0f, 1f);

            y += rowH + gap;
        }
        content.sizeDelta = new Vector2(0f, y);
    }

    void ToggleDebugPanel()
    {
        if (debugDim == null) return;
        debugDim.gameObject.SetActive(!debugDim.gameObject.activeSelf);
        if (debugDim.gameObject.activeSelf) debugDim.SetAsLastSibling();
    }

    // ── 시나리오 디스패처 ─────────────────────────────────────────────

    void ApplyDebugScenario(DebugScenario scenario)
    {
        if (hand[PLAYER_SEAT] == null || field == null || drawPile == null)
        {
            ShowTimedToast("아직 게임이 준비되지 않았습니다 — 딜링이 끝난 뒤 다시 시도하세요.");
            return;
        }
        debugDim.gameObject.SetActive(false);

        Time.timeScale = 1f;
        shakePopup?.Hide();
        fieldChoicePopup?.Hide();
        dualPiPopup?.Hide();
        declarePopup?.Hide();
        if (skipResultBtn != null) skipResultBtn.gameObject.SetActive(false);
        sittingOutSeat = -1;
        state = State.Turn;
        currentSeat = PLAYER_SEAT;
        actionBusy = false;

        switch (scenario)
        {
            case DebugScenario.Bomb: SetupDebugBomb(); break;
            case DebugScenario.PpeokForm: SetupDebugPpeokForm(); break;
            case DebugScenario.PpeokResolve: SetupDebugPpeokResolve(); break;
            case DebugScenario.Ddadak: SetupDebugDdadak(); break;
            case DebugScenario.Chok: SetupDebugChok(); break;
            case DebugScenario.JokerPark: SetupDebugJokerPark(); break;
            case DebugScenario.DualPiChoice: SetupDebugDualPiChoice(); break;
            case DebugScenario.FieldChoice: SetupDebugFieldChoice(); break;
            case DebugScenario.Shake: SetupDebugShake(); break;
            case DebugScenario.HongdanEmergency: SetupDebugSetProgress(1, 2, false); break;
            case DebugScenario.ChodanAchievement: SetupDebugSetProgress(2, 3, false); break;
            case DebugScenario.CheongdanBlocked: SetupDebugSetProgress(3, 2, true); break;
            case DebugScenario.GwangEmergency: SetupDebugGwangEmergency(); break;
            case DebugScenario.GoStopThreshold: SetupDebugGoStopThreshold(); break;
            case DebugScenario.SkipButton: SetupDebugSkipButton(); break;
        }
    }

    // ── 헬퍼 ──────────────────────────────────────────────────────────

    /// <summary>이 달의 카드를 hand[전체]/captured[전체]/field/drawPile
    /// 전부에서 지운다 — 실제 딜링이 만든 "진짜" 카드와 테스트용으로
    /// 새로 만든 인스턴스가 섞여 중복 표시되는 걸 막는다(HwatuCard는
    /// 참조 동일성으로 다뤄지므로, 같은 spriteName이라도 다른 인스턴스면
    /// 그냥 둘 다 화면에 남는다 — 이 프로젝트가 이미 여러 번 겪은 함정).</summary>
    void DebugPurgeMonth(int month)
    {
        for (int s = 0; s < SEATS_MAX; s++)
        {
            hand[s]?.RemoveAll(c => c.month == month);
            captured[s]?.RemoveAll(c => c.month == month);
        }
        field.RemoveAll(c => c.month == month);
        drawPile.RemoveAll(c => c.month == month);
    }

    List<HwatuCard> DebugMonthSet(int month) => GoStopDeck.BuildFull().Where(c => c.month == month).ToList();

    HwatuCard DebugMakeJoker(int n) => new HwatuCard(0, HwatuKind.Pi, $"Joker_{n}", piValue: 1, isJoker: true);

    void DebugFinish(string status)
    {
        debugStatusText.text = status;
        RebuildUI();
    }

    void SetupDebugBomb()
    {
        DebugPurgeMonth(7);
        var set = DebugMonthSet(7);
        hand[PLAYER_SEAT].Add(set[0]); hand[PLAYER_SEAT].Add(set[1]); hand[PLAYER_SEAT].Add(set[2]);
        field.Add(set[3]);
        DebugFinish("7월 카드 중 하나를 손패에서 클릭하세요 — 폭탄이 떠야 합니다.");
    }

    void SetupDebugPpeokForm()
    {
        DebugPurgeMonth(1);
        var set = DebugMonthSet(1);
        field.Add(set[0]);
        hand[PLAYER_SEAT].Add(set[1]);
        drawPile.Insert(0, set[2]); // 다음 뒷패로 확정
        DebugFinish("1월 손패를 내세요 — 뒷패가 같은 1월이라 뻑이 형성돼야 합니다.");
    }

    void SetupDebugPpeokResolve()
    {
        DebugPurgeMonth(2);
        var set = DebugMonthSet(2);
        field.Add(set[0]); field.Add(set[1]); field.Add(set[2]);
        int otherSeat = ActiveSeats().FirstOrDefault(s => s != PLAYER_SEAT);
        ppeokCauser[2] = otherSeat;
        // 뻑 무더기가 상대의 피를 갖고 있어야 "피 뺏기"가 눈에 보인다.
        var piCard = new HwatuCard(11, HwatuKind.Pi, "November_Kasu_1", piValue: 1);
        captured[otherSeat].RemoveAll(c => c.spriteName == "November_Kasu_1");
        captured[otherSeat].Add(piCard);
        hand[PLAYER_SEAT].Add(set[3]);
        DebugFinish("2월 손패를 내세요 — 4장을 한 번에 캡처하고 상대 피도 뺏어와야 합니다.");
    }

    void SetupDebugDdadak()
    {
        DebugPurgeMonth(3);
        var set = DebugMonthSet(3);
        field.Add(set[0]); field.Add(set[1]);
        hand[PLAYER_SEAT].Add(set[2]);
        drawPile.Insert(0, set[3]); // 필드 선택 후 남는 한 장을 뒷패가 마저 잡는다
        DebugFinish("3월 손패를 내고 필드 선택 팝업에서 아무거나 고르세요 — 뒷패가 따닥을 완성해야 합니다.");
    }

    void SetupDebugChok()
    {
        DebugPurgeMonth(4);
        var set = DebugMonthSet(4);
        hand[PLAYER_SEAT].Add(set[0]);
        drawPile.Insert(0, set[1]);
        DebugFinish("4월 손패를 내세요 — 필드에 놓인 뒤 뒷패가 잡으면 쪽이 떠야 합니다.");
    }

    void SetupDebugJokerPark()
    {
        DebugPurgeMonth(5);
        var set = DebugMonthSet(5);
        hand[PLAYER_SEAT].Add(set[0]);
        drawPile.Insert(0, set[1]); // 5월 두 번째 장 — 뻑 형성용
        drawPile.Insert(0, DebugMakeJoker(1)); // 가장 먼저 나올 뒷패
        DebugFinish("5월 손패를 내세요 — 뒷패로 보너스패가 나와 붙었다가, 다음 뒷패로 뻑이 형성돼야 합니다.");
    }

    void SetupDebugDualPiChoice()
    {
        DebugPurgeMonth(9);
        var set = DebugMonthSet(9); // [Tane(dualPi), Tanzaku, Kasu_1, Kasu_2] — GoStopDeck 순서
        var tane = set.First(c => c.dualPi);
        var others = set.Where(c => !c.dualPi).ToList();
        field.Add(tane); field.Add(others[0]);
        hand[PLAYER_SEAT].Add(others[1]);
        DebugFinish("9월 손패를 내고 필드 선택 팝업에서 국화(열끗) 쪽을 고르세요 — 곧바로 쌍피 선택 팝업이 떠야 합니다.");
    }

    void SetupDebugFieldChoice()
    {
        DebugPurgeMonth(6);
        var set = DebugMonthSet(6);
        field.Add(set[0]); field.Add(set[1]);
        hand[PLAYER_SEAT].Add(set[2]);
        DebugFinish("6월 손패를 내세요 — 필드 2장 중 하나를 고르는 팝업이 떠야 합니다.");
    }

    void SetupDebugShake()
    {
        DebugPurgeMonth(10);
        var set = DebugMonthSet(10);
        hand[PLAYER_SEAT].Add(set[0]); hand[PLAYER_SEAT].Add(set[1]); hand[PLAYER_SEAT].Add(set[2]);
        // 필드엔 10월 카드를 절대 안 놓는다 — 있으면 폭탄 조건이 되어 흔들기 팝업 자체가 안 뜬다.
        DebugFinish("10월 손패 중 하나를 클릭하세요 — 흔들기 선언 팝업이 떠야 합니다.");
    }

    /// <summary>고도리/홍단/초단/청단 진행 상황을 강제로 세팅한다.
    /// haveCount=2 → 비상, 3 → 완성. blockedTest면 나머지 1장을 상대가
    /// 이미 갖고 있는 상태로 만들고 emergencyFired를 미리 채워(실패
    /// 이펙트는 "이미 비상이 떴던 세트만" 대상이라는 기존 설계) 막힘
    /// 이펙트가 즉시 뜨게 한다.</summary>
    void SetupDebugSetProgress(int setIdx, int haveCount, bool blockedTest)
    {
        var info = EmergencySets[setIdx];
        var full = GoStopDeck.BuildFull().Where(info.pred).ToList(); // 정확히 3장
        for (int s = 0; s < SEATS_MAX; s++)
            captured[s]?.RemoveAll(c => info.pred(c));

        emergencyFired.Remove((PLAYER_SEAT, setIdx));
        achievedFired.Remove((PLAYER_SEAT, setIdx));
        blockedFired.Remove((PLAYER_SEAT, setIdx));

        if (blockedTest)
        {
            int otherSeat = ActiveSeats().FirstOrDefault(s => s != PLAYER_SEAT);
            captured[PLAYER_SEAT].Add(full[0]);
            captured[PLAYER_SEAT].Add(full[1]);
            captured[otherSeat].Add(full[2]); // 상대가 이미 마지막 한 장을 가져가 막힘
            emergencyFired.Add((PLAYER_SEAT, setIdx)); // "이미 비상이 떴던 세트"로 취급
        }
        else
        {
            for (int i = 0; i < haveCount; i++) captured[PLAYER_SEAT].Add(full[i]);
        }
        DebugFinish($"{info.name} 진행 상황을 강제로 세팅했습니다 — 이펙트가 자동으로 떠야 합니다(클릭 불필요).");
    }

    void SetupDebugGwangEmergency()
    {
        var gwang = GoStopDeck.BuildFull().Where(c => c.kind == HwatuKind.Gwang).ToList(); // 5장
        for (int s = 0; s < SEATS_MAX; s++)
            captured[s]?.RemoveAll(c => c.kind == HwatuKind.Gwang);

        emergencyFired.Remove((PLAYER_SEAT, GwangEmergencyIdx));
        achievedFired.Remove((PLAYER_SEAT, GwangEmergencyIdx));
        blockedFired.Remove((PLAYER_SEAT, GwangEmergencyIdx));

        captured[PLAYER_SEAT].Add(gwang[0]);
        captured[PLAYER_SEAT].Add(gwang[1]);
        DebugFinish("광 2장을 세팅했습니다 — '3광 비상!' 이펙트가 자동으로 떠야 합니다(클릭 불필요).");
    }

    void SetupDebugGoStopThreshold()
    {
        var gwang = GoStopDeck.BuildFull().Where(c => c.kind == HwatuKind.Gwang).ToList();
        for (int s = 0; s < SEATS_MAX; s++)
            captured[s]?.RemoveAll(c => c.kind == HwatuKind.Gwang);
        captured[PLAYER_SEAT].Add(gwang[0]);
        captured[PLAYER_SEAT].Add(gwang[1]);
        captured[PLAYER_SEAT].Add(gwang[2]); // 비광 없는 3광 = 3점, CaptureLine(3~4인)과 정확히 같음
        lastGoScore[PLAYER_SEAT] = -1;

        DebugPurgeMonth(11);
        var set = DebugMonthSet(11);
        hand[PLAYER_SEAT].Add(set[0]); // 필드에 매칭 없음 — 그냥 필드에 놓이기만 하면 됨
        DebugFinish("11월 손패를 내세요 — 이미 3점이라 즉시 고/스톱 팝업이 떠야 합니다.");
    }

    void SetupDebugSkipButton()
    {
        sittingOutSeat = PLAYER_SEAT;
        DebugFinish("이번 판은 쉬는 걸로 세팅했습니다 — 손패 자리에 '결과 넘기기' 버튼이 보여야 합니다.");
    }
}
