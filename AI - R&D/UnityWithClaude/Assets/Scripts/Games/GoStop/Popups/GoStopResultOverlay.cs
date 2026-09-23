using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 게임오버(승패) 결과 화면 — 2026-09-13 전면 재설계.
///
/// 예전엔 <see cref="GoStopUIManager"/>의 정적 Overlay/Card(항상 존재하는
/// 프리팹 자식, <c>ShowOverlay(title, scoreStr, subStr, ...)</c>로 평문
/// 하나에 모든 정보를 우겨넣음)를 그대로 썼다 — "내 정보"와 "다른 유저
/// 정보"가 한 텍스트 블록에 섞여 있고, 광박/피박/독박은 "(피박)" 같은
/// 텍스트 태그로만 표시됐다. 사용자가 이 방식 자체를 "너무 별로"라고
/// 지적 — ScoreDetailPopup/ModalTwoButtonPopup 등 다른 팝업들처럼 필요할
/// 때만 인스턴스화되는 <b>진짜 팝업</b>으로 바꾸고, 정보를 구조화해서
/// 보여달라는 요청이었다.
///
/// 이 컴포넌트가 그 대체품이다. 처음엔 Pipeline HTTP 서버가 응답
/// 불능이라 다른 팝업들처럼 실제 .prefab으로 못 굽고 "코드로 직접 UI를
/// 생성하는" 임시 방식(<c>BuildTree</c>가 트리 전체를 조립)으로 시작했지만,
/// 서버 복구 후 <c>Assets/Resources/Prefabs/GoStop/Popups/
/// GoStopResultOverlay.prefab</c>으로 정식 전환했다 — 다른 팝업들처럼
/// <c>HwatuUI.InstantiatePopup&lt;GoStopResultOverlay&gt;(...)</c>로
/// 인스턴스화된다. 모든 필드가 <c>[SerializeField]</c>라 에디터에서 직접
/// 열어 레이아웃을 편집할 수 있다.
///
/// <b>스프라이트는 프리팹에 안 구워진다</b> — <see cref="HwatuShapes"/>가
/// 만드는 절차적 스프라이트는 <c>HideAndDontSave</c> 플래그가 붙은 순수
/// 런타임 객체라 애초에 에셋으로 직렬화될 수 없다(이 프로젝트의 기존
/// 선례: <see cref="GoStopStatusBoxView.ApplyTurnState"/>도 같은 이유로
/// 스프라이트를 프리팹에 안 굽고 매번 코드로 다시 입힌다). 그래서
/// <see cref="ReapplyProceduralSprites"/>가 <c>Awake()</c>마다 카드/
/// 내정보행/버튼 3종의 스프라이트를 새로 생성해 입힌다 — Edit 모드에서
/// 프리팹을 그냥 Instantiate만 하면(Awake가 Play 모드에서만 불리므로)
/// 스프라이트가 비어 보이는 게 정상이다, Play 모드에서는 항상 채워진다.
///
/// 레이아웃 원칙 — 이 프로젝트 전역의 "이전 블록 바로 아래" 커서 누적
/// 방식(좌표 하드코딩 대신 실제로 쌓은 만큼만 다음 블록을 밀어낸다)을
/// 그대로 따른다. 라이브 에디터로 픽셀 단위 확인을 못 하는 상황이라,
/// VerticalLayoutGroup 같은 자동 레이아웃 대신 이 프로젝트가 이미 수십
/// 곳에서 검증해 온 결정적(deterministic) 수동 커서 수학을 썼다 —
/// 코드만 보고도 겹침 여부를 100% 확신할 수 있는 방식이다.
///
/// 정보 구조:
/// - 제목 + 점수 + (내가 승자가 아니면) 승자 총 획득액 한 줄.
/// - "내 정보" — Gold 테두리로 강조된 별도 행. 내 변동액(색)+잔액, 그리고
///   내가 광박/피박/독박에 해당하면 Badge_피 방식(원형 칩 + 단일 글자)의
///   배지를 그 행에 바로 붙인다.
/// - "다른 플레이어" 목록 — 나를 제외한 활성 좌석 전원(승자 포함)을 한
///   줄씩. 승자는 초록 +금액, 패자는 빨강 -금액(또는 "변동 없음") + 해당
///   배지. 최대 4줄까지 스크롤 없이 보이고, 그 이상이면 스크롤.
/// - 배율 칩 — "고 N회" 등 문자열을 이어붙이던 걸 작은 알약 칩 여러 개로.
/// - 각주(footer) — 나가리 등 "요약 텍스트만 있는" 단순 모드의 본문
///   으로도 재사용하고, 리치 모드에서는 네트워크 자동 재시작 카운트다운
///   같은 후속 안내 한 줄을 붙이는 자리로 쓴다(항상 공간을 예약해 둬서,
///   나중에 텍스트를 채워 넣어도 카드 전체 높이가 다시 안 흔들린다).
/// </summary>
public class GoStopResultOverlay : MonoBehaviour
{
    public struct Row
    {
        public string name;
        public string amountText; // 이미 색이 입혀진(HwatuTheme.MoneyColored) 완성 문자열
        public bool gwangBak, piBak, dokbak;
    }

    const float CARD_W = 900f;
    const float PAD = 36f;
    const float ROW_W = CARD_W - PAD * 2f;
    const float ROW_H = 66f;
    const float MAX_VISIBLE_ROWS = 4f;
    const float BADGE_SIZE = 34f;
    const float BADGE_GAP = 6f;
    const float LEFT_MARGIN = 20f;  // 이름 라벨~행 왼쪽 끝 여백
    const float RIGHT_MARGIN = 20f; // 배지 클러스터~행 오른쪽 끝 여백
    const float AMOUNT_GAP = 12f;   // 금액 라벨~배지 클러스터 사이 여백
    const float NAME_GAP = 16f;     // 이름 라벨~금액 라벨 사이 여백
    const float AMOUNT_W = 190f;

    // FillBadgeRow가 그리는 n개짜리 배지 클러스터가 실제로 차지하는 폭.
    // (n-1)개의 (배지+간격) + 배지 하나 자체의 폭 — 오른쪽 끝에 딱 맞춰
    // 그리므로 이 폭이 곧 배지 영역에 필요한 최소 폭이다(모자라면 왼쪽으로
    // 튀어나오고, 남으면 그만큼 낭비된다 — 정확히 맞춰야 한다).
    static float BadgeClusterWidth(int count) => count <= 0 ? 0f : (count - 1) * (BADGE_SIZE + BADGE_GAP) + BADGE_SIZE;

    // 2026-09-13(2차) — 프리팹으로 전환하면서 전부 [SerializeField]로 바꿨다.
    // 처음엔 순수 private였는데(런타임 코드 생성 컴포넌트라 직렬화가 필요
    // 없었다), 프리팹 에셋으로 구우면 그 순간의 참조를 실제로 저장해 둬야
    // Instantiate 이후에도(=BuildTree를 다시 안 불러도) Show()가 정상
    // 동작한다 — 이 프로젝트의 다른 팝업 View(ScoreDetailPopup 등)와 같은
    // 이유·같은 패턴.
    [SerializeField] RectTransform dim, card;
    [SerializeField] Image dimImg;
    [SerializeField] TextMeshProUGUI titleText, scoreText, winnerLineText;
    [SerializeField] RectTransform myRow;
    [SerializeField] Image myRowBg;
    [SerializeField] TextMeshProUGUI myNameText, myDeltaText, myBalanceText;
    [SerializeField] RectTransform myBadgeArea;
    [SerializeField] RectTransform othersScroll, othersContent;
    [SerializeField] RectTransform multiplierRow;
    [SerializeField] TextMeshProUGUI footerText;
    [SerializeField] RectTransform buttonRow;
    [SerializeField] Button primaryBtn, secondaryBtn, tertiaryBtn;
    [SerializeField] TextMeshProUGUI primaryLabel, secondaryLabel, tertiaryLabel;

    // SetFooterNote가 매초 덮어써도 원래 있던 내용(단순 모드의 plainSub)이
    // 안 사라지도록 Show()가 기억해 두는 "기본" 각주 — 리치 모드는 항상
    // 빈 문자열이라 SetFooterNote가 넘긴 텍스트만 그대로 보인다.
    string baseFooterText = "";

    // 2026-09-13(2차) — 게임 코드에서 직접 부르는 정적 팩토리(Build)는
    // 프리팹 전환으로 필요 없어져서 지웠다. BuildTree() 자체는 프리팹을
    // (다시) 구울 때 에디터 전용 베이킹 스크립트가 리플렉션으로 한 번
    // 호출하는 용도로만 남겨뒀다 — 이 프로젝트의 다른 팝업들도 전부
    // 이런 베이킹 스크립트는 파일로 영구 보관하지 않고 그때그때 실행
    // 하고 버린다.
    void BuildTree()
    {
        var root = (RectTransform)transform;
        root.anchorMin = Vector2.zero;
        root.anchorMax = Vector2.one;
        root.offsetMin = Vector2.zero;
        root.offsetMax = Vector2.zero;

        dim = HwatuUI.MakeRect("Dim", root, Vector2.zero, Vector2.zero);
        dim.anchorMin = Vector2.zero;
        dim.anchorMax = Vector2.one;
        dim.offsetMin = Vector2.zero;
        dim.offsetMax = Vector2.zero;
        dimImg = dim.gameObject.AddComponent<Image>();
        dimImg.color = new Color(0f, 0f, 0f, 0.72f);
        dimImg.raycastTarget = true;

        // 카드는 화면 정중앙(anchor=0.5,0.5)에 pivot=top-center로 걸어서,
        // Show()가 매번 계산하는 finalHeight만큼 위/아래로 대칭 확장되게 한다
        // (anchoredPosition.y = finalHeight/2로 top edge를 밀어 올리는 방식).
        var cardGo = new GameObject("Card", typeof(RectTransform));
        cardGo.transform.SetParent(dim, false);
        card = cardGo.GetComponent<RectTransform>();
        card.anchorMin = card.anchorMax = new Vector2(0.5f, 0.5f);
        card.pivot = new Vector2(0.5f, 1f);
        card.sizeDelta = new Vector2(CARD_W, 100f);
        var cardImg = cardGo.AddComponent<Image>();
        cardImg.color = Color.white; // 스프라이트는 절차적 생성 — ReapplyProceduralSprites()가 매번 입힌다(아래 참고)

        titleText = HwatuUI.MakeLabel(card, Vector2.zero, new Vector2(ROW_W, 64f), 52f, HwatuTheme.Gold);
        titleText.fontStyle = FontStyles.Bold;

        scoreText = HwatuUI.MakeLabel(card, Vector2.zero, new Vector2(ROW_W, 44f), 30f, HwatuTheme.TextPrimary);

        winnerLineText = HwatuUI.MakeLabel(card, Vector2.zero, new Vector2(ROW_W, 32f), 22f, HwatuTheme.TextSecondary);

        // 내 정보 행 — Gold 테두리로 강조.
        myRow = HwatuUI.MakeRect("MyRow", card, new Vector2(ROW_W, 96f), Vector2.zero);
        myRowBg = myRow.gameObject.AddComponent<Image>();
        myRowBg.color = Color.white; // 스프라이트는 ReapplyProceduralSprites()가 입힌다

        // 배지(최대 3개: 광/피/독)가 항상 오른쪽 끝에 딱 맞게 자리하고,
        // 델타/잔액 텍스트는 그 왼쪽에서 시작하도록 폭을 역산한다 — 배지가
        // 0개여도 3개여도 델타 텍스트 오른쪽 끝이 배지 클러스터를 절대
        // 침범하지 않는다(둘 다 같은 "오른쪽 끝 기준" 공식을 공유).
        float myBadgesW = BadgeClusterWidth(3);
        float myBadgesRight = ROW_W / 2f - RIGHT_MARGIN;
        float myAmountRight = myBadgesRight - myBadgesW - AMOUNT_GAP;
        float myAmountLeft = myAmountRight - AMOUNT_W;

        // 이름 라벨의 폭을 "왼쪽 여백부터 금액 라벨 시작 전까지 실제로 남는
        // 공간"으로 역산한다 — 예전엔 고정 220px를 임의로 잡아서 행/카드
        // 왼쪽 경계를 40px 넘어가는 버그가 있었다(캐릭터 이름이 4글자만
        // 돼도 넘칠 수 있는 폭). 이 방식이면 이름이 몇 글자든 절대 왼쪽
        // 경계나 금액 영역을 침범할 수 없다 — 아래 MakeOtherRow도 같은
        // 원리를 공유한다.
        float myNameLeft = -ROW_W / 2f + LEFT_MARGIN;
        float myNameRight = myAmountLeft - NAME_GAP;
        float myNameW = myNameRight - myNameLeft;
        float myNameCenterX = (myNameLeft + myNameRight) / 2f;

        var myTag = HwatuUI.MakeLabel(myRow, new Vector2(myNameCenterX, -14f), new Vector2(myNameW, 30f), 20f, HwatuTheme.TextSecondary);
        myTag.alignment = TextAlignmentOptions.Left;
        myTag.text = "내 정보";

        myNameText = HwatuUI.MakeLabel(myRow, new Vector2(myNameCenterX, -46f), new Vector2(myNameW, 36f), 26f, HwatuTheme.TextPrimary);
        myNameText.alignment = TextAlignmentOptions.Left;
        myNameText.fontStyle = FontStyles.Bold;

        myDeltaText = HwatuUI.MakeLabel(myRow, new Vector2(myAmountRight - AMOUNT_W / 2f, -14f), new Vector2(AMOUNT_W, 34f), 30f, HwatuTheme.TextPrimary);
        myDeltaText.alignment = TextAlignmentOptions.Right;
        myDeltaText.fontStyle = FontStyles.Bold;

        myBalanceText = HwatuUI.MakeLabel(myRow, new Vector2(myAmountRight - AMOUNT_W / 2f, -50f), new Vector2(AMOUNT_W, 26f), 18f, HwatuTheme.TextSecondary);
        myBalanceText.alignment = TextAlignmentOptions.Right;

        myBadgeArea = HwatuUI.MakeRect("MyBadges", myRow, new Vector2(myBadgesW, BADGE_SIZE), new Vector2(myBadgesRight - myBadgesW / 2f, -ROW_H / 2f + 15f));

        // "다른 플레이어" 스크롤 목록 — Show()가 매번 ClearChildren 후 다시 채운다.
        var scroll = HwatuUI.MakeScrollBody(card, new Vector2(ROW_W, ROW_H), Vector2.zero);
        othersScroll = scroll.viewport;
        othersContent = scroll.content;

        // 배율 칩 한 줄(HorizontalLayoutGroup으로 자동 정렬 — 칩 개수가
        // 매번 달라지는 가변 콘텐츠라 이 한 곳만 예외적으로 자동 레이아웃을
        // 쓴다, 개별 칩 폭까지 손수 계산할 필요가 없어서 더 안전하다).
        multiplierRow = HwatuUI.MakeRect("MultiplierRow", card, new Vector2(ROW_W, 44f), Vector2.zero);
        var hlg = multiplierRow.gameObject.AddComponent<HorizontalLayoutGroup>();
        hlg.childAlignment = TextAnchor.MiddleCenter;
        hlg.spacing = 10f;
        hlg.childControlWidth = false;
        hlg.childControlHeight = false;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = false;

        footerText = HwatuUI.MakeLabel(card, Vector2.zero, new Vector2(ROW_W, 30f), 18f, HwatuTheme.TextSecondary);

        buttonRow = HwatuUI.MakeRect("Buttons", card, new Vector2(ROW_W, 84f), Vector2.zero);
        primaryBtn = MakeButton(buttonRow, "PrimaryBtn", Color.white, out primaryLabel);
        secondaryBtn = MakeButton(buttonRow, "SecondaryBtn", HwatuTheme.TextPrimary, out secondaryLabel);
        tertiaryBtn = MakeButton(buttonRow, "TertiaryBtn", HwatuTheme.TextPrimary, out tertiaryLabel);

        ReapplyProceduralSprites();
    }

    // 스프라이트는 색만 넣고 만들지 않는다 — MakeButton은 구조(Image/Button/
    // Label)만 준비한다.
    static Button MakeButton(RectTransform parent, string name, Color textColor, out TextMeshProUGUI label)
    {
        var rt = HwatuUI.MakeRect(name, parent, new Vector2(200f, 76f), Vector2.zero);
        var img = rt.gameObject.AddComponent<Image>();
        img.color = Color.white;
        var btn = rt.gameObject.AddComponent<Button>();
        label = HwatuUI.MakeLabel(rt, Vector2.zero, rt.sizeDelta, 26f, textColor);
        label.fontStyle = FontStyles.Bold;
        return btn;
    }

    // 2026-09-13(3차) — "프리팹에 스프라이트 매칭이 다 빠져있다" 신고로
    // 발견. HwatuShapes.*()가 만드는 Sprite는 HideFlags.HideAndDontSave로
    // 생성되는 순수 런타임 오브젝트라 프리팹 에셋으로 저장할 수 없다(디스크에
    // 못 써서 재로드하면 그 참조가 통째로 사라진다) — GoStopStatusBoxView.
    // ApplyTurnState가 이미 겪고 우회해 둔 것과 같은 제약. 그래서 프리팹엔
    // 구조(GameObject/컴포넌트/위치/색)만 저장하고, 절차적 스프라이트는
    // 인스턴스가 생길 때마다(Awake) 코드로 다시 입힌다 — BuildTree()도
    // (베이킹 직후 에디터에서 미리보기가 되도록) 끝에서 한 번 같이 부른다.
    void ReapplyProceduralSprites()
    {
        var cardImg = card.GetComponent<Image>();
        cardImg.sprite = HwatuShapes.RoundedRectBordered(160, 26, 5, HwatuTheme.WarmCream, HwatuTheme.DarkGreen);
        cardImg.type = Image.Type.Sliced;

        myRowBg.sprite = HwatuShapes.RoundedRectBordered(140, 20, 4, HwatuTheme.CreamWhite, HwatuTheme.Gold);
        myRowBg.type = Image.Type.Sliced;

        var primaryImg = primaryBtn.GetComponent<Image>();
        primaryImg.sprite = HwatuShapes.RoundedRectBordered(120, 18, 0, HwatuTheme.HwatuRed, HwatuTheme.HwatuRed);
        primaryImg.type = Image.Type.Sliced;

        var secondaryImg = secondaryBtn.GetComponent<Image>();
        secondaryImg.sprite = HwatuShapes.RoundedRectBordered(120, 18, 4, HwatuTheme.CreamWhite, HwatuTheme.DarkGreen);
        secondaryImg.type = Image.Type.Sliced;

        var tertiaryImg = tertiaryBtn.GetComponent<Image>();
        tertiaryImg.sprite = HwatuShapes.RoundedRectBordered(120, 18, 4, HwatuTheme.CreamWhite, HwatuTheme.DarkGreen);
        tertiaryImg.type = Image.Type.Sliced;
    }

    void Awake() => ReapplyProceduralSprites();

    static RectTransform MakeBadge(RectTransform parent, Vector2 pos, string glyph)
    {
        var rt = HwatuUI.MakeRect("Badge_" + glyph, parent, new Vector2(BADGE_SIZE, BADGE_SIZE), pos);
        var img = rt.gameObject.AddComponent<Image>();
        img.sprite = HwatuShapes.Circle();
        img.color = HwatuTheme.HwatuRed;
        var lbl = HwatuUI.MakeLabel(rt, Vector2.zero, rt.sizeDelta, 18f, Color.white);
        lbl.fontStyle = FontStyles.Bold;
        lbl.text = glyph;
        return rt;
    }

    // 오른쪽에서 왼쪽으로 배지를 채운다(있는 것만 그려서 없으면 공간을
    // 안 차지한다) — area는 이미 우측 정렬된 컨테이너(myBadgeArea/각 행의
    // Badges 컨테이너). Show()/MakeOtherRow가 공유하는 한 곳뿐인 배지
    // 배치 로직 — "Badge_피" 형식(원형 칩 + 단일 글자)을 여기서만 정의한다.
    static void FillBadgeRow(RectTransform area, bool gwangBak, bool piBak, bool dokbak)
    {
        float x = area.sizeDelta.x / 2f - BADGE_SIZE / 2f;
        if (dokbak) { MakeBadge(area, new Vector2(x, 0f), "독"); x -= BADGE_SIZE + 6f; }
        if (piBak) { MakeBadge(area, new Vector2(x, 0f), "피"); x -= BADGE_SIZE + 6f; }
        if (gwangBak) MakeBadge(area, new Vector2(x, 0f), "광");
    }

    static RectTransform MakeChip(RectTransform parent, string text)
    {
        // HorizontalLayoutGroup 자식이라 폭은 텍스트 길이에 맞춰 대략
        // 넉넉하게 잡는다(정확한 텍스트 측정 없이도 안전하도록 여유를 둠).
        float w = Mathf.Clamp(text.Length * 15f + 32f, 90f, 320f);
        var rt = HwatuUI.MakeRect("Chip", parent, new Vector2(w, 40f), Vector2.zero);
        var img = rt.gameObject.AddComponent<Image>();
        img.sprite = HwatuShapes.RoundedRectBordered(100, 18, 0, HwatuTheme.DarkGreen, HwatuTheme.DarkGreen);
        img.type = Image.Type.Sliced;
        img.color = Color.white;
        var lbl = HwatuUI.MakeLabel(rt, Vector2.zero, rt.sizeDelta, 18f, HwatuTheme.CreamWhite);
        lbl.text = text;
        return rt;
    }

    static RectTransform MakeOtherRow(RectTransform parent, Vector2 pos, Row row)
    {
        var rt = HwatuUI.MakeRect("Row", parent, new Vector2(ROW_W, ROW_H), pos);
        var bg = rt.gameObject.AddComponent<Image>();
        bg.sprite = HwatuShapes.RoundedRect(64, 12);
        bg.type = Image.Type.Sliced; // 9-slice 보더가 구워진 스프라이트 — 안 하면 모서리가 늘어나 찌그러진다
        bg.color = new Color(HwatuTheme.DarkGreen.r, HwatuTheme.DarkGreen.g, HwatuTheme.DarkGreen.b, 0.06f);

        // 배지 개수와 무관하게 항상 최대(3개) 폭을 예약해서, 행마다 배지
        // 개수가 달라도 금액 텍스트의 오른쪽 정렬 위치가 서로 어긋나지
        // 않게(시각적으로 열이 맞게) 한다.
        float badgesW = BadgeClusterWidth(3);
        float badgesRight = ROW_W / 2f - RIGHT_MARGIN;
        float amountRight = badgesRight - badgesW - AMOUNT_GAP;
        float amountLeft = amountRight - AMOUNT_W;

        // 이름 라벨 폭도 myRow와 동일한 원리로 "왼쪽 여백~금액 시작 전"
        // 실제 공간을 역산한다 — 캐릭터 이름 길이와 무관하게 이 행(=Scroll의
        // Mask에 실제로 잘리는 영역)을 절대 못 넘는다. 이전엔 고정 200px를
        // 왼쪽 여백 없이 잡아서 Mask 왼쪽 경계를 10px 넘어 실제로 잘려
        // 보이는 버그였다("좌우로 짤리는" 신고의 원인).
        float nameLeft = -ROW_W / 2f + LEFT_MARGIN;
        float nameRight = amountLeft - NAME_GAP;
        float nameW = nameRight - nameLeft;
        float nameCenterX = (nameLeft + nameRight) / 2f;

        var name = HwatuUI.MakeLabel(rt, new Vector2(nameCenterX, -ROW_H / 2f), new Vector2(nameW, 32f), 22f, HwatuTheme.TextPrimary);
        name.alignment = TextAlignmentOptions.Left;
        name.text = row.name;

        var amount = HwatuUI.MakeLabel(rt, new Vector2(amountRight - AMOUNT_W / 2f, -ROW_H / 2f), new Vector2(AMOUNT_W, 32f), 22f, HwatuTheme.TextPrimary);
        amount.alignment = TextAlignmentOptions.Right;
        amount.text = row.amountText;

        int badgeCount = (row.gwangBak ? 1 : 0) + (row.piBak ? 1 : 0) + (row.dokbak ? 1 : 0);
        if (badgeCount > 0)
        {
            var badgeArea = HwatuUI.MakeRect("Badges", rt, new Vector2(badgesW, BADGE_SIZE), new Vector2(badgesRight - badgesW / 2f, -ROW_H / 2f));
            FillBadgeRow(badgeArea, row.gwangBak, row.piBak, row.dokbak);
        }
        return rt;
    }

    /// <summary>결과 화면을 연다.
    /// <paramref name="myName"/>이 null이면 "단순 모드"(나가리 등 —
    /// 내 정보/다른 플레이어 행/배율 칩을 전부 숨기고 <paramref
    /// name="plainSub"/> 한 덩어리만 보여준다). 아니면 "리치 모드"(일반
    /// 승부 — 내 정보 카드 + 다른 플레이어 행 목록 + 배율 칩).</summary>
    public void Show(
        Color titleColor, string title, string scoreStr, string winnerLine,
        string myName, string myDeltaText_, string myBalanceText_,
        bool myGwangBak, bool myPiBak, bool myDokbak,
        List<Row> otherRows, List<string> multiplierChips,
        string plainSub,
        string primaryLabel_, Action primaryAction,
        string secondaryLabel_ = null, Action secondaryAction = null,
        string tertiaryLabel_ = null, Action tertiaryAction = null,
        string extraNote = null)
    {
        gameObject.SetActive(true);
        // 2026-09-13(프리팹 전환 검증 중 발견) — Hide()가 dim을 개별
        // SetActive(false)로 끄는데, 부모(루트)를 다시 켜도 자식(dim)의
        // 개별 활성 상태는 자동으로 안 돌아온다(Unity의 activeSelf는
        // 부모 체인과 무관하게 각자 독립적으로 유지된다) — 여기서 명시적으로
        // 다시 켜지 않으면 한 번이라도 Hide()가 불린 뒤엔 Show()를 다시
        // 불러도 영영 화면에 아무것도 안 뜬다.
        dim.gameObject.SetActive(true);
        dim.SetAsLastSibling();

        titleText.text = title;
        titleText.color = titleColor;
        scoreText.text = string.IsNullOrEmpty(scoreStr) || scoreStr == "-" ? "" : $"{scoreStr}점";

        bool rich = myName != null;
        bool hasWinnerLine = rich && !string.IsNullOrEmpty(winnerLine);
        winnerLineText.text = winnerLine ?? "";

        // 카드 상단부터 "이전 블록 바로 아래"로 쌓는 커서. y는 항상
        // "다음 블록의 top edge"를 가리키다가, 각 블록을 배치할 때마다
        // 그 블록의 높이만큼 더 내려간다(SetY 참고).
        float y = -18f; // card 상단 padding
        SetY(titleText.rectTransform, ref y, 64f);
        SetY(scoreText.rectTransform, ref y, 44f);
        winnerLineText.gameObject.SetActive(hasWinnerLine);
        if (hasWinnerLine) SetY(winnerLineText.rectTransform, ref y, 32f);

        myRow.gameObject.SetActive(rich);
        othersScroll.gameObject.SetActive(rich && otherRows != null && otherRows.Count > 0);
        multiplierRow.gameObject.SetActive(rich && multiplierChips != null && multiplierChips.Count > 0);
        footerText.gameObject.SetActive(true);

        if (rich)
        {
            y -= 12f;
            SetY(myRow, ref y, 96f);
            myNameText.text = myName;
            myDeltaText.text = myDeltaText_;
            myBalanceText.text = myBalanceText_;
            HwatuUI.ClearChildren(myBadgeArea);
            FillBadgeRow(myBadgeArea, myGwangBak, myPiBak, myDokbak);

            if (otherRows != null && otherRows.Count > 0)
            {
                y -= 14f;
                int visible = Mathf.Min(otherRows.Count, (int)MAX_VISIBLE_ROWS);
                float scrollH = visible * ROW_H;
                othersScroll.sizeDelta = new Vector2(ROW_W, scrollH);
                SetY(othersScroll, ref y, scrollH);
                HwatuUI.ClearChildren(othersContent);
                float ry = 0f;
                foreach (var row in otherRows)
                {
                    MakeOtherRow(othersContent, new Vector2(0, -ry), row);
                    ry += ROW_H;
                }
                othersContent.sizeDelta = new Vector2(ROW_W, ry);
            }

            if (multiplierChips != null && multiplierChips.Count > 0)
            {
                y -= 14f;
                SetY(multiplierRow, ref y, 44f);
                HwatuUI.ClearChildren(multiplierRow);
                foreach (var chip in multiplierChips) MakeChip(multiplierRow, chip);
            }

            // 리치 모드는 footerText가 "본문 뒤에 덧붙는 한 줄"(다운그레이드/
            // 세션종료 안내, 자동 재시작 카운트다운 등) 전용이라 항상 자리만
            // 예약해 두고 extraNote로 시작한다 — SetFooterNote()가 나중에
            // 카운트다운을 더 붙여도(baseFooterText 뒤에 이어붙는 방식) 카드
            // 높이가 안 흔들린다.
            baseFooterText = extraNote ?? "";
            footerText.fontSize = 18f;
            footerText.alignment = TextAlignmentOptions.Center;
            y -= 10f;
            SetY(footerText.rectTransform, ref y, 30f);
        }
        else
        {
            // 단순 모드는 footerText 자체가 본문이라 줄 수가 가변이다
            // (나가리 등은 보통 2~4줄) — 넉넉히 예약해 잘리지 않게 한다.
            baseFooterText = string.IsNullOrEmpty(extraNote) ? (plainSub ?? "") : $"{plainSub}\n{extraNote}";
            footerText.fontSize = 22f;
            footerText.alignment = TextAlignmentOptions.Center;
            y -= 10f;
            int lines = baseFooterText.Split('\n').Length;
            SetY(footerText.rectTransform, ref y, Mathf.Max(30f, lines * 30f));
        }
        footerText.text = baseFooterText;

        y -= 20f;
        SetY(buttonRow, ref y, 84f);
        LayoutButtons(primaryLabel_, primaryAction, secondaryLabel_, secondaryAction, tertiaryLabel_, tertiaryAction);
        y -= 26f; // 하단 padding

        card.sizeDelta = new Vector2(CARD_W, -y);
        card.anchoredPosition = new Vector2(0, -y / 2f);

        GoStopFX.PlayPopupFadeIn(dimImg);
    }

    static void SetY(RectTransform rt, ref float y, float h)
    {
        rt.anchoredPosition = new Vector2(0, y);
        y -= h;
    }

    void LayoutButtons(string p, Action pa, string s, Action sa, string t, Action ta)
    {
        int count = (p != null ? 1 : 0) + (s != null ? 1 : 0) + (t != null ? 1 : 0);
        primaryBtn.gameObject.SetActive(p != null);
        secondaryBtn.gameObject.SetActive(s != null);
        tertiaryBtn.gameObject.SetActive(t != null);

        void Wire(Button b, TextMeshProUGUI l, string label, Action action)
        {
            if (label == null) return;
            l.text = label;
            b.onClick.RemoveAllListeners();
            if (action != null) b.onClick.AddListener(() => action());
        }
        Wire(primaryBtn, primaryLabel, p, pa);
        Wire(secondaryBtn, secondaryLabel, s, sa);
        Wire(tertiaryBtn, tertiaryLabel, t, ta);

        float gap = 16f;
        if (count == 1)
        {
            primaryBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(ROW_W, 76f);
            primaryBtn.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        }
        else if (count == 2)
        {
            float w = (ROW_W - gap) / 2f;
            var a = p != null ? primaryBtn : secondaryBtn;
            var bb = p != null ? secondaryBtn : tertiaryBtn;
            a.GetComponent<RectTransform>().sizeDelta = new Vector2(w, 76f);
            a.GetComponent<RectTransform>().anchoredPosition = new Vector2(-w / 2f - gap / 2f, 0);
            bb.GetComponent<RectTransform>().sizeDelta = new Vector2(w, 76f);
            bb.GetComponent<RectTransform>().anchoredPosition = new Vector2(w / 2f + gap / 2f, 0);
        }
        else if (count == 3)
        {
            float w = (ROW_W - gap * 2f) / 3f;
            primaryBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(w, 76f);
            primaryBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(-w - gap, 0);
            secondaryBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(w, 76f);
            secondaryBtn.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            tertiaryBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(w, 76f);
            tertiaryBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(w + gap, 0);
        }
    }

    /// <summary>나가리/일반 승부 공통 — 자동 재시작 카운트다운 등 후속 한 줄을
    /// Show()가 이미 그려둔 본문(baseFooterText) 아래에 덧붙인다. 매초
    /// 다시 불러도(카운트다운) 본문이 사라지지 않는다 — 리치 모드는
    /// baseFooterText가 항상 빈 문자열이라 이 텍스트만 단독으로 보인다.</summary>
    public void SetFooterNote(string text)
    {
        if (footerText == null) return;
        footerText.text = string.IsNullOrEmpty(text)
            ? baseFooterText
            : (string.IsNullOrEmpty(baseFooterText) ? text : $"{baseFooterText}\n{text}");
    }

    public void Hide()
    {
        if (dim != null) dim.gameObject.SetActive(false);
        gameObject.SetActive(false);
    }
}
