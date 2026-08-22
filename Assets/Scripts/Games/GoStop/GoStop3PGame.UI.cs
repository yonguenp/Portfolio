using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

/// <summary>
/// GoStop3PGame(3~4인 고스톱)의 UI 구성 부분 — "테이블에 둘러앉기" 화면 배치
/// (BuildStaticUI), 팝업 빌더(흔들기/필드선택/9월열끗/참가선언/선뽑기/광판다/
/// 점수상세), 카드 렌더링(RebuildUI/DrawField/DrawPlayerCaptured/DrawAiCaptured),
/// 애니메이션(SlamIn류) 전부 여기 있다. 턴 진행·규칙 판정 등 게임 로직은
/// GoStop3PGame.cs(Core)에 있다 — 같은 타입을 partial class로 역할별로 나눴을
/// 뿐, 멤버는 두 파일 사이에서 자유롭게 보인다.
/// </summary>
public partial class GoStop3PGame
{
    // ══════════════════════════════════════════════════════
    // UI 구성 — "테이블에 둘러앉기": 나=아래, AI-A=좌측, AI-B=상단, AI-C=우측.
    // 좌우 좌석은 카드를 90도 눕혀서(뒷면이 세로가 아니라 가로로 놓인 모습)
    // "옆에 사람이 앉아 손을 내밀고 있다"는 인상을 준다. 필드는 화면
    // 정중앙, 더미는 그 바로 아래.
    // ══════════════════════════════════════════════════════

    // "Cap 영역이 필드와 헷갈린다"는 신고로 획득패 존(내/상대 공통) 배경을
    // 필드와 다른 색으로 구분한다. alpha를 살짝 남겨(0.92) 배경 펠트
    // 위에 완전히 딱딱한 상자처럼 뜨지 않게 했다.
    static readonly Color CapZoneColor = new Color(0.180f, 0.247f, 0.161f, 0.92f); // #2E3F29

    // 가로형 상대 좌석 블록(상/좌/우 공통) — 자리마다 필요한 폭·존 간격만
    // 다르다. (센터X, 위쪽 Y, 블록 폭, 캡 존 간격, 캡 줄당 최대 장수)를
    // seat별로 갖고 있다가 DrawAiCaptured가 그대로 재사용한다.
    struct EdgeSeatSpec { public float centerX, capZoneGap, blockWidth; public int capMaxPerRow; }
    EdgeSeatSpec[] edgeSpec = new EdgeSeatSpec[SEATS_MAX];

    void BuildStaticUI()
    {
        var root = ui.ContentArea;

        // HUD를 통째로 껐으므로(Start()의 SetHudVisible(false)) 뒤로가기
        // 버튼도 같이 사라졌다 — 작은 나가기 버튼 하나만 둔다.
        // 2026-08-18: "우측하단으로 옮기고, 누르면 바로 나가지 말고
        // 확인/취소 팝업으로 물어봐야 한다" 요청 — 위치를 bottom-right
        // 앵커로 옮기고, onClick을 GoToTitle 직접 호출에서 확인 팝업을
        // 여는 것으로 바꿨다(실제 나가기는 팝업의 "나가기" 버튼에서).
        var exitBtn = UISkin.MakeKenneyButton(root, "ExitBtn", new Vector2(120f, 52f), Vector2.zero,
            UISkin.Accent.Red, "나가기", ShowExitConfirm);
        var exitRT = exitBtn.GetComponent<RectTransform>();
        exitRT.anchorMin = exitRT.anchorMax = new Vector2(1f, 0f);
        exitRT.pivot = new Vector2(1f, 0f);
        exitRT.anchoredPosition = new Vector2(-14f, 14f);

        // 상단 중앙(슬롯2) — 참고 이미지의 "MISSION" 배너 자리를 광팔이/쉬는
        // 유저 정보 슬롯으로 재활용("저기다 넣으면 될것같아" 요청). 상단은
        // Cap/Back 없이 정보 블록(닉네임/고+점수/금액/아이콘)만 있다 —
        // "상단의 Cap, Back 영역은 없애야한다" 요청. 내가 쉬는 드문 판엔
        // 세 번째 활성 AI가 이 자리에 뜨는데, 그때도 마찬가지로 정보
        // 블록만 보인다(RecomputeSeatSlots 주석 참고).
        float topBottom = BuildInfoBlock(2, 0f, 520f, -10f, root);

        // 이하 전부 "이전 블록 바로 아래" 커서 누적 방식(이 파일이 반복
        // 채택해 온 패턴) — 좌표 하드코딩으로 인한 겹침 재발을 구조적으로
        // 막는다. 가로뷰는 세로보다 높이 예산이 훨씬 빠듯해서(1080 전체 —
        // HUD를 꺼서 되찾은 116px까지 합쳐도 세로 때의 절반 수준) 상단
        // 슬롯을 얇게 만든 만큼 필드·좌우·하단이 여유를 더 가져간다.
        float fieldTop = topBottom - 14f;

        // 필드/더미 — 2026-08-18: "더미가 화면 중앙이면 필드 패 보는 게
        // 헷갈린다, 원래대로 좌상단으로" 요청으로 중앙 배치를 되돌렸다.
        // 필드는 다시 2줄 예산(더미가 줄을 안 차지하므로).
        // 2026-08-19: "Field를 800사이즈로 줄여서 DrawPile과 안 겹치게"
        // 요청 — FIELD_COL_PITCH도 같이 줄여야 그리드가 실제로 800 안에
        // 들어간다(DrawField의 FIELD_COL_PITCH와 반드시 같이 맞출 것).
        const float FIELD_AREA_W = 800f; // FIELD_COLS(6) × FIELD_COL_PITCH(133) — DrawField와 맞출 것
        float fieldRowH = FIELD_H + 10f;
        fieldArea = HwatuUI.MakeRect("Field", root, new Vector2(FIELD_AREA_W, fieldRowH * 2f), new Vector2(0f, fieldTop));
        float fieldBottom = fieldTop - fieldRowH * 2f;
        float centerBottom = fieldBottom - 10f;

        // 좌/우(AI-A/C) — 가로뷰라 회전 없이 화면 가장자리 세로 기둥에
        // 그대로 놓는다(세로판의 90도 회전 트릭이 필요 없어졌다 — 가로는
        // 폭이 넉넉해서 좌우 여백에 안 눕힌 카드로도 자리가 난다). 블록
        // 폭은 필드보다 좁게(캡 존이 3개뿐이라 zoneGap·maxPerRow를 줄여서
        // 맞춘다), 위쪽 Y는 필드와 같은 기준(fieldTop)으로 맞춰 "테이블에
        // 둘러앉은" 느낌을 유지한다. 슬롯 1/3(seat 아님!)로 만든다 —
        // RecomputeSeatSlots가 매판 어느 좌석을 여기 그릴지 정한다.
        // 2026-08-20 3차 정정: "정보창은 400으로, Back·Cap은 회전시켜서
        // 배치"(사용자 확인) — 정보창은 원래 폭(400)/위치(750)로 되돌린다.
        // Back·Cap은 이제 blockWidth와 무관하게 회전 컨테이너 자체 크기를
        // 쓴다(BuildEdgeSeatBlock 안 상수 참고).
        const float SIDE_W = 400f;
        const float SIDE_X = 750f;

        // 더미 — 2026-08-19: "-460,-200으로 수정" 확인 값. 필드도 800으로
        // 줄어서(위 FIELD_AREA_W 참고, 그리드 반경 400) 왼쪽 끝(-400)과
        // 더미 오른쪽 끝(-460+50=-410) 사이 10px 여백으로 안 겹친다.
        // fieldArea의 자식으로 넣지 않는 이유는 여전히 동일(ClearChildren이
        // 매턴 자식을 무차별로 지운다).
        float pileX = -460f;
        float pileY = -200f;
        drawPileArea = HwatuUI.MakeRect("DrawPile", root, new Vector2(PILE_W, PILE_H), new Vector2(pileX, pileY));
        // zoneGap은 이제 DrawAiCaptured가 안 읽는다(3존 나란히 배치를
        // 접었으므로) — 예전 호출 형태만 유지하고 값 자체는 의미 없다.
        float sideBottomL = BuildEdgeSeatBlock(1, -SIDE_X, SIDE_W, fieldTop + 16f, root, zoneGap: 0f, maxPerRow: 5, capAreaH: 0f);
        float sideBottomR = BuildEdgeSeatBlock(3, SIDE_X, SIDE_W, fieldTop + 16f, root, zoneGap: 0f, maxPerRow: 5, capAreaH: 0f);

        // 나(아래) — 위 세 구간(중앙 필드+더미, 좌, 우) 중 가장 낮은 지점
        // 바로 아래부터 시작한다. 하드코딩된 값이 아니라 실제 배치 결과에서
        // 계산하므로, 위쪽 블록이 커져도 자동으로 밀려나 겹치지 않는다.
        // 2026-08-20: Back/Cap을 씬에서 사용자가 직접 옮기면서(위 재사용
        // 로직 참고) sideBottomL/R의 커서 감산이 실제 배치와 어긋나 이
        // 아래 구간(StatusBox~PlayerCap)이 너무 아래로 처졌다 — 사용자가
        // 직접 재보고 확인한 보정값 +400을 그대로 반영한다. Back/Cap을
        // 씬에서 또 옮기면 이 값도 다시 맞춰야 할 수 있다.
        const float MANUAL_LAYOUT_CORRECTION = 400f;
        float contentBottom = Mathf.Min(centerBottom, Mathf.Min(sideBottomL, sideBottomR)) + MANUAL_LAYOUT_CORRECTION;
        float capY = BuildInfoBlock(0, 0f, 700f, contentBottom - 10f, root);
        playerCapArea = HwatuUI.MakeRect("PlayerCap", root, new Vector2(1000f, CAP_ROW_PITCH * 2f), new Vector2(0f, capY - 6f));
        HwatuUI.AddZoneBackground(playerCapArea, CapZoneColor);
        // 2026-08-20: "Hand 영역 posY -878로 조절" 확인 값 — 커서 계산값
        // 대신 직접 지정한다(이 파일이 반복 채택해 온, 사용자가 실측/확인한
        // 값을 그대로 박아 넣는 패턴 — Body/Card 등 다른 팝업들과 동일).
        float handY = -878f;
        handArea = HwatuUI.MakeRect("Hand", root, new Vector2(1000f, HAND_H), new Vector2(0f, handY));

        // 팝업(딤+패널)은 전부 ContentArea가 아니라 Canvas 바로 밑(Overlay와
        // 같은 층)에 붙인다 — ContentArea 밑에 두면 게임오버 Overlay(Canvas
        // 자식 중 나중 순번이라 항상 위에 그려진다)가 팝업을 덮어버릴 수
        // 있다("점수 상세가 오버레이보다 뒤에 있어서 안 보인다"는 신고로
        // 발견). 지금 뜨는 시점이 게임 중이라 안 겹치는 팝업도 미리 통일해
        // 둔다 — 규칙이 하나여야 나중에 또 걸리지 않는다.
        var canvasRoot = root.parent.parent as RectTransform;
        BuildShakeConfirmUI(canvasRoot);
        BuildExitConfirmUI(canvasRoot);
        BuildFieldChoiceUI(canvasRoot);
        BuildDualPiChoiceUI(canvasRoot);
        BuildDeclareUI(canvasRoot);
        BuildDealerDrawUI(canvasRoot);
        BuildGwangSaleUI(canvasRoot);
        BuildScoreDetailUI(canvasRoot);
    }

    /// <summary>선 뽑기 연출 팝업 — 좌석 4개 자리에 카드를 한 장씩 순서대로
    /// 뒤집어 보여주고, 가장 높은 패를 뽑은 좌석을 강조한다.</summary>
    void BuildDealerDrawUI(RectTransform canvasRoot)
    {
        dealerDrawPopup = HwatuUI.InstantiatePopup<DealerDrawPopupView>("DealerDrawPopup", canvasRoot);
        string[] seatNames = { SeatName(0), SeatName(1), SeatName(2), SeatName(3) };
        for (int s = 0; s < SEATS; s++) dealerDrawPopup.seatLabels[s].text = seatNames[s];
    }

    /// <summary>4장을 순서대로 공개하고 가장 높은 패(월이 우선, 같은 월이면
    /// 광→열끗→띠→피 순)를 뽑은 좌석을 선으로 정한다.</summary>
    IEnumerator DetermineDealerSeq()
    {
        dealerDrawPopup.Show();
        dealerDrawPopup.resultText.text = "";
        for (int s = 0; s < SEATS; s++) HwatuUI.ClearChildren(dealerDrawPopup.cardSlots[s]);

        var deck = GoStopDeck.BuildFull();
        GoStopDeck.Shuffle(deck);
        var draws = new HwatuCard[SEATS];

        for (int s = 0; s < SEATS; s++)
        {
            draws[s] = deck[s];
            HwatuUI.MakeCard(draws[s], dealerDrawPopup.cardSlots[s], Vector2.zero, FIELD_W, FIELD_H, null, false);
            // 2026-08-19: "사운드가 빠진 부분" — 선 뽑기 4장 공개가 완전히
            // 무음이었다. 카드 내는 소리(CardPlay)를 그대로 재사용해 한
            // 장씩 뒤집힐 때마다 틱을 준다.
            GoStopAudio.Instance?.CardPlay();
            yield return new WaitForSeconds(0.22f); // 한 장씩 순서대로 뒤집히는 느낌
        }

        int best = 0;
        for (int s = 1; s < SEATS; s++)
            if (DrawRank(draws[s]) > DrawRank(draws[best])) best = s;

        dealerSeat = best;
        dealerDrawPopup.resultText.text = $"{SeatName(best)}이(가) 선입니다!";
        GoStopAudio.Instance?.Bonus(); // 결과가 정해지는 순간의 반짝이는 차임
        yield return new WaitForSeconds(1.1f);

        dealerDrawPopup.Hide();
    }

    /// <summary>선 뽑기 순위 — 월이 높을수록, 같은 월이면 광→열끗→띠→피 순으로 높다.</summary>
    static int DrawRank(HwatuCard c)
    {
        int kindBonus = c.kind switch { HwatuKind.Gwang => 3, HwatuKind.Yeolkkeut => 2, HwatuKind.Ddi => 1, _ => 0 };
        return c.month * 10 + kindBonus;
    }

    /// <summary>광팔이 결과 팝업 — 토스트 한 줄("광팔이! (N장)")만으로는 근거를
    /// 알 수 없다는 신고. 어떤 패로 팔았는지 실물로 보여주고, 총액·누가
    /// 내는지를 텍스트로 덧붙인다.</summary>
    void BuildGwangSaleUI(RectTransform canvasRoot)
    {
        gwangSalePopup = HwatuUI.InstantiatePopup<GwangSalePopupView>("GwangSalePopup", canvasRoot);
    }

    /// <summary>판 카드를 한 장씩 순서대로 보여준 뒤, 총액·지불자를 표시한다.
    /// <paramref name="payAmounts"/>는 실제 지급액(perPayer를 좌석 잔액으로
    /// clamp한 값) — 상대가 돈이 부족했으면 명목 금액과 달라질 수 있어서
    /// 실제로 오간 금액을 그대로 보여준다.</summary>
    IEnumerator ShowGwangSaleSeq(int sellerSeat, List<HwatuCard> soldCards, Dictionary<int, int> payAmounts, int payerA, int payerB)
    {
        // 2026-08-19: 이 팝업이 예전 "토스트 한 줄"을 대체하면서(v7) 그
        // 토스트가 갖고 있던 Toast()의 자동 사운드(PlayForLabel)까지 같이
        // 빠졌다 — GoStopAudio.GwangPali()가 아무도 안 부르는 죽은 코드로
        // 남아 있던 원인. 여기서 직접 불러서 채운다.
        GoStopAudio.Instance?.GwangPali();
        gwangSalePopup.Show();
        gwangSalePopup.titleText.text = $"{SeatName(sellerSeat)} 광팔이!";
        HwatuUI.ClearChildren(gwangSalePopup.cardRow);
        gwangSalePopup.amountText.text = "";
        gwangSalePopup.payerText.text = "";

        int n = soldCards.Count;
        float total = n * (FIELD_W + 6f) - 6f;
        for (int i = 0; i < n; i++)
        {
            float x = -total * 0.5f + FIELD_W * 0.5f + i * (FIELD_W + 6f);
            HwatuUI.MakeCard(soldCards[i], gwangSalePopup.cardRow, new Vector2(x, 0f), FIELD_W, FIELD_H, null, false);
            yield return new WaitForSeconds(0.15f); // 한 장씩 순서대로 공개
        }

        gwangSalePopup.amountText.text = $"광+쌍피 {n}장 × {GWANG_SALE_WON_PER_CARD}원";
        gwangSalePopup.payerText.text = $"{SeatName(payerA)} {payAmounts[payerA]:N0}원, {SeatName(payerB)} {payAmounts[payerB]:N0}원 → {SeatName(sellerSeat)}";
        yield return new WaitForSeconds(1.8f);

        gwangSalePopup.Hide();
    }

    /// <summary>광팔이 참가 선언 팝업 — 플레이어가 2번째·3번째 선언 순번일 때만 뜬다.
    /// 2026-08-18: "선정하는 팝업 디자인 어색하다"는 지적으로 Kenney 헤더바 패널로
    /// 다시 그렸다 — 예전엔 어두운 판 위에 흰 글자만 있어서 다른 팝업들과 톤이
    /// 안 맞았다(그리고 그 팝업들도 이번에 다 같은 스타일로 맞췄다).</summary>
    void BuildDeclareUI(RectTransform canvasRoot)
    {
        declarePopup = HwatuUI.InstantiatePopup<ModalTwoButtonPopup>("DeclarePopup", canvasRoot);
        declarePopup.SetPrimary(() => OnDeclareChoiceClicked(true));
        declarePopup.SetSecondary(() => OnDeclareChoiceClicked(false));
    }

    /// <summary>참가 선언 버튼 클릭 — 로컬(싱글플레이·호스트 자신)이면
    /// 예전처럼 pendingDeclareChoice를 세워 NewGameSeq의 WaitUntil을
    /// 풀어주고, 네트워크 게스트면 그 결정을 호스트에게 보낸다(호스트
    /// 쪽 코루틴이 대신 기다리고 있다 — GoStop3PGame.cs의 참가선언
    /// 원격 분기 참고). 게스트 쪽엔 그 WaitUntil이 아예 없으므로 팝업을
    /// 직접 닫아줘야 한다 — 안 그러면 아무도 안 닫아준다.</summary>
    void OnDeclareChoiceClicked(bool wantsIn)
    {
        if (isNetworkGuest)
        {
            GoStopNetLobby.Instance.SendToHost(GoStopNetMessage.Declare(wantsIn));
            declarePopup.Hide();
        }
        else pendingDeclareChoice = wantsIn;
    }

    void BuildShakeConfirmUI(RectTransform canvasRoot)
    {
        shakePopup = HwatuUI.InstantiatePopup<ModalTwoButtonPopup>("ShakeConfirmPopup", canvasRoot);
        shakePopup.SetPrimary(() => OnShakeChoice(true));
        shakePopup.SetSecondary(() => OnShakeChoice(false));
    }

    /// <summary>나가기 확인 팝업 — ShakeConfirmPopup과 같은 범용 2버튼
    /// 프리팹을 새 인스턴스로 하나 더 만든다(프리팹은 공유해도 인스턴스는
    /// 독립적이라 서로 다른 용도로 동시에 존재할 수 있다). 버튼 라벨은
    /// 프리팹 기본값(흔들기용 "예/아니오")과 달라야 하므로 런타임에
    /// 텍스트를 직접 덮어쓴다.</summary>
    void BuildExitConfirmUI(RectTransform canvasRoot)
    {
        exitConfirmPopup = HwatuUI.InstantiatePopup<ModalTwoButtonPopup>("ShakeConfirmPopup", canvasRoot);
        exitConfirmPopup.messageText.text = "게임을 종료하고 타이틀로 나가시겠습니까?";
        var primaryLabel = exitConfirmPopup.primaryButton.GetComponentInChildren<TextMeshProUGUI>();
        if (primaryLabel) primaryLabel.text = "나가기";
        var secondaryLabel = exitConfirmPopup.secondaryButton.GetComponentInChildren<TextMeshProUGUI>();
        if (secondaryLabel) secondaryLabel.text = "취소";
        exitConfirmPopup.SetPrimary(GoToTitle);
        exitConfirmPopup.SetSecondary(() => exitConfirmPopup.Hide());
    }

    void ShowExitConfirm() => exitConfirmPopup.Show();

    void BuildFieldChoiceUI(RectTransform canvasRoot)
    {
        fieldChoicePopup = HwatuUI.InstantiatePopup<CardChoicePopup>("FieldChoicePopup", canvasRoot);
    }

    // 2026-08-19: FieldChoicePopup 카드/하이라이트 재조정(사용자 확인) — 이
    // 팝업은 2인/4인이 같은 프리팹을 공유하는데, 예전엔 각 게임 자신의
    // FIELD_W/H(4인=140×160, 2인=92×114)를 그대로 재사용해서 하이라이트
    // 기본 공식(카드+16)이 게임마다 다른 크기로 어긋났었다. 이 팝업 전용
    // 고정 카드 크기(94×154)를 따로 둬서 두 게임이 동일한 결과를 내게
    // 했고, 하이라이트 110×170은 그 카드+16과 정확히 맞아떨어진다.
    // MakeCard의 하이라이트는 카드와 같은 top-center pivot을 쓰므로
    // 커진 만큼이 전부 아래로만 붙는다(폭은 pivot.x=0.5라 자동 대칭) —
    // offset.y=+8로 위아래 8px씩 균등하게 갈라 카드를 감싸도록 맞춘다.
    const float CHOICE_CARD_W = 94f, CHOICE_CARD_H = 154f;
    static readonly Vector2 ChoiceHighlightSize = new Vector2(110f, 170f);
    static readonly Vector2 ChoiceHighlightOffset = new Vector2(0f, 8f);

    void ShowFieldChoicePopup(List<HwatuCard> candidates)
    {
        HwatuUI.ClearChildren(fieldChoicePopup.cardContainer);

        float spacing = CHOICE_CARD_W + 40f;
        float startX = -(candidates.Count - 1) * spacing * 0.5f;
        // 팝업 Body 실측 높이(264, FieldChoicePopup.prefab)에서 카드가
        // 위아래로 정확히 가운데 오도록 top-pivot 기준 y를 역산한다.
        const float BODY_H = 264f;
        float cardY = -(BODY_H - CHOICE_CARD_H) * 0.5f;
        for (int i = 0; i < candidates.Count; i++)
        {
            var c = candidates[i];
            float x = startX + i * spacing;
            HwatuUI.MakeCard(c, fieldChoicePopup.cardContainer, new Vector2(x, cardY), CHOICE_CARD_W, CHOICE_CARD_H,
                () => OnFieldChoiceClicked(c), true,
                highlightSize: ChoiceHighlightSize, highlightOffset: ChoiceHighlightOffset);
        }
        fieldChoicePopup.Show();
    }

    /// <summary>참가 선언과 같은 이유(OnDeclareChoiceClicked 문서 참고) —
    /// 게스트는 결정을 호스트로 보내고 팝업을 직접 닫는다.</summary>
    void OnFieldChoiceClicked(HwatuCard c)
    {
        if (isNetworkGuest)
        {
            GoStopNetLobby.Instance.SendToHost(GoStopNetMessage.Choice(c.spriteName));
            HideFieldChoicePopup();
        }
        else pendingFieldChoice = c;
    }

    void HideFieldChoicePopup() => fieldChoicePopup.Hide();

    void BuildDualPiChoiceUI(RectTransform canvasRoot)
    {
        dualPiPopup = HwatuUI.InstantiatePopup<ModalTwoButtonPopup>("DualPiPopup", canvasRoot);
        dualPiPopup.SetPrimary(() => OnDualPiChoiceClicked(false));
        dualPiPopup.SetSecondary(() => OnDualPiChoiceClicked(true));
    }

    /// <summary>참가 선언과 같은 이유(OnDeclareChoiceClicked 문서 참고) —
    /// 게스트는 결정을 호스트로 보내고 팝업을 직접 닫는다.</summary>
    void OnDualPiChoiceClicked(bool useAsPi)
    {
        if (isNetworkGuest)
        {
            GoStopNetLobby.Instance.SendToHost(GoStopNetMessage.DualPi(useAsPi));
            dualPiPopup.Hide();
        }
        else pendingDualPiChoice = useAsPi;
    }

    void BuildScoreDetailUI(RectTransform canvasRoot)
    {
        scoreDetailPopup = HwatuUI.InstantiatePopup<ScoreDetailPopup>("ScoreDetailPopup", canvasRoot);
        // 닫기 버튼(헤더 X + 하단 "닫기")은 프리팹 저장 시점에 이미 comp.Hide로
        // persistent 연결돼 있다 — 여기서 다시 연결할 필요 없다.
    }

    /// <summary>점수 항목 줄(라벨+점수) 밑에 관여한 카드 실물을 늘어놓는다 — 2인판
    /// (GoStopGame.BuildScoreDetailRows)과 같은 로직·같은 시각 스타일이다.</summary>
    void BuildScoreDetailRows(RectTransform content, List<HwatuCard> captured, GoStopRules.Score baseScore)
    {
        HwatuUI.ClearChildren(content);
        var lines = GoStopRules.BuildScoreLines(captured, baseScore);
        var textCol = new Color(0.16f, 0.14f, 0.06f, 1f);

        float y = 4f;
        if (lines.Count == 0)
        {
            var empty = HwatuUI.MakeLabel(content, new Vector2(0f, -y), new Vector2(860f, 36f), 22f, new Color(textCol.r, textCol.g, textCol.b, 0.7f));
            empty.text = "(기본 점수 없음)";
            empty.alignment = TextAlignmentOptions.TopLeft;
            y += 44f;
        }
        else
        {
            const float cardGap = 4f;
            foreach (var line in lines)
            {
                var lbl = HwatuUI.MakeLabel(content, new Vector2(0f, -y), new Vector2(860f, 32f), 22f, textCol);
                lbl.text = $"{line.label}  {line.points}점";
                lbl.fontStyle = FontStyles.Bold;
                lbl.alignment = TextAlignmentOptions.TopLeft;
                y += 34f;

                if (line.cards.Count > 0)
                {
                    float x = -430f + CAP_W * 0.5f;
                    foreach (var c in line.cards)
                    {
                        HwatuUI.MakeCard(c, content, new Vector2(x, -y), CAP_W, CAP_H, null, false);
                        x += CAP_W + cardGap;
                    }
                    y += CAP_H + 20f;
                }
                else y += 12f;
            }
        }
        content.sizeDelta = new Vector2(content.sizeDelta.x, Mathf.Max(y, 420f));
    }

    /// <summary>"왜 이 점수가 나왔는지" 항목별로 보여준다 — 게임오버 오버레이의
    /// "점수 상세" 버튼에서 연다. 4인판은 패자가 여럿이라, 승자 쪽 항목별
    /// 점수·고/흔들기/폭탄 배수는 공통으로 한 번만 보여주고, 광박/피박은
    /// 패자 개인마다 갈릴 수 있어(사용자 확인 규칙) 패자별로 따로 나열한다.
    /// 항목 옆 카드 실물 표시는 2인판과 같은 요청으로 추가했다.</summary>
    void ShowScoreDetail()
    {
        if (pendingPayout == null || scoreDetailPopup == null) return;
        var p = pendingPayout;

        scoreDetailPopup.summaryText.text = $"[{SeatName(pendingWinnerSeat)} 획득패 기준]  기본 소계 {p.baseScore.Total}점" +
            (p.goCount > 0 ? $"  ·  고 {p.goCount}회(+{p.goBonus}) → {p.subtotal}점" : "");

        BuildScoreDetailRows(scoreDetailPopup.rowsContent, captured[pendingWinnerSeat], p.baseScore);

        var mult = new List<string>();
        if (p.goMultiplier > 1) mult.Add($"고배수 ×{p.goMultiplier}");
        if (p.heundeulCount > 0) mult.Add($"흔들기 ×{1 << p.heundeulCount}({p.heundeulCount}회)");
        if (p.bombCount > 0) mult.Add($"폭탄 ×{1 << p.bombCount}({p.bombCount}회)");
        if (p.extraMultiplier > 1) mult.Add($"고정배수 ×{p.extraMultiplier}");

        var foot = new System.Text.StringBuilder();
        foot.AppendLine(mult.Count > 0 ? $"공통 배수: {string.Join(" · ", mult)}" : "공통 배수 없음");
        // 독박(고박) — 패자 중 한 명이 전원분을 몰아서 낸 경우. amounts만
        // 보고는 "왜 이 사람만 냈는지" 유추해야 해서 점수 상세에 전혀 안
        // 보인다는 신고를 받아, dokbakLoserIndex로 그 줄에 직접 태그를 단다.
        // 2026-08-18: "패자 닉네임과 광박·멍박·피박 정보(아이콘)·지출금액,
        // 승자의 획득금액, 현재 내 금액을 정리해서 보여달라" 요청 —
        // 패자별 배지를 실제 아이콘(GoStopIcons)으로 옆에 그리고, 승자
        // 획득금액 총합·내 잔액을 마지막 줄에 추가했다.
        long totalWon = 0;
        for (int i = 0; i < pendingLoserSeats.Count; i++)
        {
            int seat = pendingLoserSeats[i];
            totalWon += p.amounts[i];
            string dokbakTag = i == p.dokbakLoserIndex ? " <color=#B03A2E><b>(독박)</b></color>" : "";
            foot.AppendLine($"<color=#8A6300><b>{SeatName(seat)}: {p.amounts[i]:N0}원</b></color>{dokbakTag}");
        }
        foot.AppendLine();
        foot.AppendLine($"<color=#EDBA2E><b>{SeatName(pendingWinnerSeat)} 획득: {totalWon:N0}원</b></color>");
        foot.Append($"내 보유 금액: {money[PLAYER_SEAT]:N0}원");
        scoreDetailPopup.footerText.text = foot.ToString();

        // 패자별 광박/멍박/피박 아이콘 — footerText 각 줄 옆에 정확히 맞추기
        // 어려우므로(멀티라인 자동 줄바꿈), 별도 컨테이너에 패자 수만큼 행을
        // 새로 그린다. footerText 바로 아래, panel 폭에 맞춘 가로 스트립.
        if (scoreDetailPopup.badgeStripArea == null) { scoreDetailPopup.Show(); return; }
        HwatuUI.ClearChildren(scoreDetailPopup.badgeStripArea);
        for (int i = 0; i < pendingLoserSeats.Count; i++)
        {
            int seat = pendingLoserSeats[i];
            float y = -i * 34f;
            var nameLbl = HwatuUI.MakeLabel(scoreDetailPopup.badgeStripArea, new Vector2(-140f, y), new Vector2(160f, 28f), 16f, new Color(0.16f, 0.14f, 0.06f, 1f));
            nameLbl.text = SeatName(seat);
            nameLbl.alignment = TextAlignmentOptions.MidlineLeft;
            float bx = 0f;
            void PlaceMini(bool on, string label, Color col)
            {
                if (!on) return;
                GoStopIcons.MakeTextIcon(scoreDetailPopup.badgeStripArea, new Vector2(bx, y), 24f, label, col, Color.white);
                bx += 30f;
            }
            PlaceMini(p.gwangBakPerLoser[i], "광", new Color(0.69f, 0.37f, 0.86f));
            PlaceMini(GoStopRules.IsLiveMeongBakRisk(captured[seat], new[] { captured[pendingWinnerSeat] }), "멍", new Color(0.55f, 0.42f, 0.30f));
            PlaceMini(p.piBakPerLoser[i], "피", new Color(0.88f, 0.32f, 0.32f));
        }

        scoreDetailPopup.Show(); // dim 활성화 + SetAsLastSibling까지 컴포넌트가 처리
    }

    /// <summary>손패 영역에서 이 카드에 해당하는 슬롯(RectTransform)을 찾는다 —
    /// SlamIn 출발점 계산용. 이름이 spriteName과 같다는 MakeCard의 규칙에 의존한다.</summary>
    RectTransform FindHandSlot(HwatuCard card)
    {
        var t = handArea.Find(card.spriteName);
        return t as RectTransform;
    }

    /// <summary>정보 슬롯(닉네임/고+점수/금액/상태아이콘, 4단) — 상단·좌·우·
    /// 하단 전부 이 하나로 통일한다("정보슬롯을 쫌스럽게 쓰지 말고 크게
    /// 크게" 요청). <paramref name="topY"/>부터 아래로 4줄을 쌓고, 이
    /// 블록이 차지하는 가장 낮은 y를 돌려준다. 아이콘 줄의 y는
    /// <see cref="badgeRowY"/>에 저장해 두어 RebuildUI가 그 자리에
    /// 정확히 그리게 한다 — 예전엔 이 위치를 텍스트 rect에서 추정해서
    /// 뒷패 영역과 겹치는 버그가 있었다.</summary>
    /// <summary>2026-08-19: "상태 아이콘이 안 보인다, 패널을 반으로 갈라서
    /// 좌측=닉네임/고점수/금액, 우측=아이콘을 큼직하게" 요청으로 레이아웃을
    /// 좌우 분할로 다시 짰다. 우측 아이콘 영역(<see cref="badgeArea"/>)은
    /// 매 RebuildUI마다 확실히 지워지는 전용 컨테이너다 — 예전엔 아이콘을
    /// `ui.ContentArea`에 직접 그려서 **한 번도 안 지워졌다**(ContentArea
    /// 자체는 RebuildUI의 클리어 목록에 없다 — 필드/손패/캡 영역만 지운다).
    /// 그 결과 매턴 아이콘이 계속 누적돼, 이전 라운드에 그 좌석이 선이었을
    /// 때 그려진 "선" 배지가 그 좌석이 광팔이로 쉬는 지금도 그대로 남아있는
    /// 버그로 나타났다("광팔이한테 선 아이콘이 떠있다" 신고) — 전용 컨테이너를
    /// 두고 매턴 `ClearChildren`하는 것으로 구조적으로 막는다.</summary>
    float BuildInfoBlock(int slot, float centerX, float width, float topY, RectTransform root)
    {
        const float NAME_H = 32f, GOSCORE_H = 28f, MONEY_H = 32f, GAP = 5f;
        float totalH = NAME_H + GOSCORE_H + MONEY_H + GAP * 2f;
        float halfW = width * 0.5f;
        float leftCenterX = centerX - halfW * 0.5f - 4f;
        float rightCenterX = centerX + halfW * 0.5f + 4f;

        statusBoxImg[slot] = HwatuUI.MakeStatusBox(root, new Vector2(centerX, topY), totalH - 14f, width);

        float cursor = topY;
        statusText[slot] = HwatuUI.MakeLabel(root, new Vector2(leftCenterX, cursor), new Vector2(halfW - 20f, NAME_H), 21f, Color.white);
        statusText[slot].textWrappingMode = TextWrappingModes.NoWrap;
        statusText[slot].alignment = TextAlignmentOptions.MidlineLeft;
        cursor -= NAME_H + GAP;

        goScoreText[slot] = HwatuUI.MakeLabel(root, new Vector2(leftCenterX, cursor), new Vector2(halfW - 20f, GOSCORE_H), 17f, new Color(1f, 1f, 1f, 0.82f));
        goScoreText[slot].textWrappingMode = TextWrappingModes.NoWrap;
        goScoreText[slot].alignment = TextAlignmentOptions.MidlineLeft;
        cursor -= GOSCORE_H + GAP;

        // 2026-08-19: "보유 금액이 상태 박스 바깥에 표시된다" 버그 —
        // BuildMoneyChip은 자기 자신의 왼쪽 끝을 기준으로 아이콘+글자를
        // 그리는데(anchorMin=anchorMax=(0,1)), 그 칩의 중심 좌표(pos.x)를
        // 이름/고점수 줄과 같은 leftCenterX가 아니라 거기서 한 번 더
        // 왼쪽으로 옮긴 값을 넘기고 있었다 — 칩 전체가 그만큼 왼쪽으로
        // 밀려나 배경 박스(centerX 기준 width 폭) 밖으로 삐져나왔다.
        // 이름/고점수와 같은 leftCenterX를 그대로 써야 세 줄의 왼쪽
        // 끝이 정확히 맞는다.
        moneyText[slot] = HwatuUI.BuildMoneyChip(root, new Vector2(leftCenterX, cursor), halfW - 20f, iconSize: 24f, fontSize: 19f);

        // 우측 절반 — 상태 아이콘 전용 컨테이너. 세로 예산(totalH) 전체를
        // 그대로 준다(가로로 다 안 들어가면 DrawBadgeStrip이 알아서 다음
        // 줄로 감싼다).
        badgeArea[slot] = HwatuUI.MakeRect($"BadgeArea{slot}", root, new Vector2(halfW - 12f, totalH), new Vector2(rightCenterX, topY));

        return topY - totalH;
    }

    // 상태 아이콘 크기·색(전 슬롯 공통) — "아이콘이 작고 대비가 약해 안
    // 보인다"는 신고로 34px(기존 26)로 키우고, 꺼진 상태 배경을 반투명
    // 흰색(백색 위 백색이라 거의 안 보였다)에서 짙은 남색 표면색으로,
    // 글자도 alpha 0.35(거의 투명)에서 0.6 이상으로 올렸다.
    const float BADGE_SIZE = 34f;
    static readonly Color BadgeDimBg = new Color(0.106f, 0.133f, 0.267f, 0.95f); // #1B2244 계열 — B안 표면색
    static readonly Color BadgeDimFg = new Color(1f, 1f, 1f, 0.62f);

    /// <summary>선/광박/멍박/피박/흔들기/뻑 아이콘 — 정보 슬롯(RebuildUI)과
    /// 승리 화면 점수 상세(ShowScoreDetail) 양쪽에서 공유한다. <paramref
    /// name="maxWidth"/>를 넘으면 다음 줄로 감싼다(우측 절반 폭이 자리마다
    /// 달라서 한 줄에 다 안 들어갈 수 있다 — 특히 좌우 슬롯).</summary>
    void DrawBadgeStrip(RectTransform parent, int seat, Vector2 startPos, float maxWidth)
    {
        var mine = captured[seat];
        var others = ActiveSeats().Where(s => s != seat).Select(s => captured[s]);
        bool shook = shookMonths[seat].Count > 0;
        bool gwangBak = GoStopRules.IsLiveGwangBakRisk(mine, others);
        bool meongBak = GoStopRules.IsLiveMeongBakRisk(mine, others);
        bool piBak = GoStopRules.IsLivePiBakRisk(mine, others, GoStopRules.PI_BAK_THRESHOLD_3P);
        bool isDealer = seat == dealerSeat;

        float x = startPos.x, y = startPos.y;
        const float STEP = BADGE_SIZE + 6f;
        void Place(System.Action<Vector2> draw)
        {
            if (x + STEP > startPos.x + maxWidth) { x = startPos.x; y -= STEP; } // 다음 줄로 감싸기
            draw(new Vector2(x, y));
            x += STEP;
        }

        // 표기 순서(요청): 선 → 광박 → 멍박 → 피박 → 흔들기 → 뻑
        // 2026-08-18: "先/光 한자가 폰트에 없어 □로 깨진다"는 신고 —
        // 이 프로젝트 폰트 공통 함정(한자 미출력)이라 한자 대신 그
        // 한자의 한글 훈/음(선/광)을 그대로 쓴다.
        if (isDealer)
            Place(p => GoStopIcons.MakeTextIcon(parent, p, BADGE_SIZE, "선", new Color(0.93f, 0.73f, 0.18f), Color.black));
        Place(p => GoStopIcons.MakeTextIcon(parent, p, BADGE_SIZE, "광", gwangBak ? new Color(0.69f, 0.37f, 0.86f) : BadgeDimBg, gwangBak ? Color.white : BadgeDimFg));
        Place(p => GoStopIcons.MakeTextIcon(parent, p, BADGE_SIZE, "멍", meongBak ? new Color(0.55f, 0.42f, 0.30f) : BadgeDimBg, meongBak ? Color.white : BadgeDimFg));
        Place(p => GoStopIcons.MakeTextIcon(parent, p, BADGE_SIZE, "피", piBak ? new Color(0.88f, 0.32f, 0.32f) : BadgeDimBg, piBak ? Color.white : BadgeDimFg));

        // 2026-08-19: "마지막 아이콘이 뭔지 모르겠다" 신고 — 원형 아이콘
        // 구석에 작은 숫자만 떠 있던 뻑 표시가 안 읽혔다. 흔들기·뻑을
        // "[흔듬]"/"[뻑]" 글자 박스 + 원 2개(횟수만큼 채워짐)로 바꾸고,
        // 폭이 기존 정사각 아이콘과 달라 Place()의 줄바꿈 계산에 안 맞으므로
        // 아예 다음 줄에 고정으로 그린다(요청: "다음줄에 흔듬,뻑 아이콘
        // 추가"). 뻑은 3회째 즉시 승리라(쓰리뻑 규칙) 원 2개로 충분하다.
        float row2Y = y - STEP;
        float rowX = startPos.x;
        var shakeBadge = GoStopIcons.MakeCountBadge(parent, new Vector2(rowX, row2Y), "흔듬",
            new Color(0.93f, 0.78f, 0.20f), Mathf.Min(shookMonths[seat].Count, 2));
        rowX += shakeBadge.sizeDelta.x + 8f;
        GoStopIcons.MakeCountBadge(parent, new Vector2(rowX, row2Y), "뻑",
            new Color(0.85f, 0.25f, 0.22f), Mathf.Min(ppeokTotalCount[seat], 2));
    }

    /// <summary>상대 좌석 한 블록(상태줄→뒷패 줄→획득패 존) — 상단(seat2)·
    /// 좌(seat1)·우(seat3) 전부 이 하나의 함수를 쓴다. 가로뷰는 폭이
    /// 넉넉해서 좌/우도 세로판처럼 90도로 눕힐 필요가 없다 — 세 자리가
    /// 전부 "가로로 카드를 나열"하는 같은 모양이라 코드가 하나로
    /// 합쳐졌다(세로판엔 있던 회전 보정 함수 두 개가 통째로 필요 없어졌다).
    /// <paramref name="topY"/>(화면상 위쪽 y)부터 아래로 쌓고, 이 블록이
    /// 차지하는 가장 낮은 y를 돌려준다 — 호출자가 다음 구간을 겹치지
    /// 않게 이어 붙일 수 있도록. <paramref name="zoneGap"/>/<paramref
    /// name="maxPerRow"/>는 <see cref="DrawAiCaptured"/>가 이 좌석의 획득패
    /// 존을 그릴 때 그대로 쓰도록 <see cref="edgeSpec"/>에 저장해 둔다.</summary>
    float BuildEdgeSeatBlock(int seat, float centerX, float blockWidth, float topY, RectTransform root,
                              float zoneGap, int maxPerRow, float capAreaH)
    {
        edgeSpec[seat] = new EdgeSeatSpec { centerX = centerX, capZoneGap = zoneGap, capMaxPerRow = maxPerRow, blockWidth = blockWidth };

        // 2026-08-18: "정보슬롯을 쫌스럽게 쓰지 말고 크게크게" 요청으로
        // 한 줄 압축 상태줄을 BuildInfoBlock(닉네임/고+점수/금액/아이콘 4단)
        // 으로 교체 — 아이콘 줄 위치를 실측값(badgeRowY)으로 명시해서
        // 뒷패(Back) 영역과 겹치던 버그도 같이 해결된다. 정보창 자체는
        // 회전 없이 blockWidth(400) 그대로다 — "정보창은 원래대로"
        // 사용자 확인.
        float cursor = BuildInfoBlock(seat, centerX, blockWidth, topY, root);
        cursor -= 10f;

        // 2026-08-20: Back·Cap을 컨테이너째 회전시켜서 배치한다(사용자
        // 확인 — 예전 세로판 시절 있었다가 가로뷰로 오면서 삭제됐던
        // MakeRotatedContainer 기법을 다시 만들었다). 좌측(seat==1)
        // -90도·우측(seat==3) +90도 — 카드의 "위쪽"이 필드(화면 중앙)
        // 방향을 향하게 하는 방향이다. 안의 카드는 평소처럼(회전 안
        // 걸린 것처럼) 그리면 부모 회전 때문에 자동으로 돌아간 모습으로
        // 보인다 — 손패 뒷면 그리기 루프·DrawAiCaptured/DrawCapZone
        // 전부 손 안 대도 된다.
        //
        // 크기는 실측 예산에서 역산했다 — fieldTop+16(-110)부터 손패
        // 고정 위치(-878)까지 752px 중, 정보창(102)+간격(16)을 빼면
        // Back+Cap 합쳐 쓸 수 있는 세로 커서 예산은 약 338px뿐이다.
        // 회전 후엔 declaredW가 세로(커서 소모) 길이가 되므로, 그 예산을
        // Back(170)·Cap(162, 피 5장 기준 폭 156px+6px 여유)으로 나눴다.
        // Back이 손패 최대 7장(262px 필요)보다 좁아서 초반 턴엔 카드가
        // 살짝 겹쳐 보일 수 있다 — 손패가 줄면서 자연히 해소된다.
        float zRot = seat == 1 ? -90f : 90f;
        const float BACK_DECLARED_W = 170f;
        const float CAP_DECLARED_W = 162f;
        const float CAP_DECLARED_H = 200f; // 회전 후 가로 폭 — 존 2블록이 각 1~2줄 쓸 여유

        // 2026-08-20: "직접 수정할 수 있게 미리 만들어달라" 요청 — 씬에
        // Back{seat}/Cap{seat}가 이미 있으면(에디터에서 사용자가 손으로
        // 위치·크기·회전을 다듬어 둔 것) 그대로 재사용한다. 코드가 매
        // RebuildUI/씬 로드마다 값을 덮어쓰지 않는다는 뜻이라, 사용자가
        // 인스펙터에서 바꾼 값이 그대로 유지된다. 없으면(예: 다른 슬롯,
        // 혹은 사용자가 아직 안 만진 상태) 기존처럼 코드가 계산해서
        // 새로 만든다. 재사용할 땐 실제 sizeDelta.x(회전 후 시각적 세로
        // 길이)를 커서 계산에 반영해서, 사용자가 크기를 키우거나 줄여도
        // 그 아래(플레이어 자신의 정보창 등) 배치가 자동으로 따라온다.
        var existingBack = root.Find($"Back{seat}") as RectTransform;
        float backDeclaredW;
        if (existingBack != null)
        {
            backArea[seat] = existingBack;
            backDeclaredW = existingBack.sizeDelta.x;
            StripStrayLayoutGroup(existingBack);
        }
        else
        {
            backArea[seat] = MakeRotatedContainerByVisualTop($"Back{seat}", root, BACK_DECLARED_W, BACK_H, centerX, cursor, zRot);
            backDeclaredW = BACK_DECLARED_W;
        }
        cursor -= backDeclaredW + 6f;

        var existingCap = root.Find($"Cap{seat}") as RectTransform;
        float capDeclaredW;
        if (existingCap != null)
        {
            capAreaAI[seat] = existingCap;
            capDeclaredW = existingCap.sizeDelta.x;
            StripStrayLayoutGroup(existingCap);
        }
        else
        {
            capAreaAI[seat] = MakeRotatedContainerByVisualTop($"Cap{seat}", root, CAP_DECLARED_W, CAP_DECLARED_H, centerX, cursor, zRot);
            HwatuUI.AddZoneBackground(capAreaAI[seat], CapZoneColor);
            capDeclaredW = CAP_DECLARED_W;
        }
        cursor -= capDeclaredW;

        return cursor;
    }

    /// <summary>씬에서 인스펙터로 만지다 보면 LayoutGroup(Horizontal/Vertical/
    /// GridLayoutGroup)이나 ContentSizeFitter가 실수로 붙을 수 있다 —
    /// 이 컴포넌트들은 자식 RectTransform의 위치·크기를 매 프레임 강제로
    /// 덮어써서, 카드 하나하나를 직접 좌표 계산해 배치하는 이 파일의
    /// 렌더링(DrawCapZone·손패/뒷패 루프)을 통째로 무력화한다(실측으로
    /// 발견 — Cap1/Cap3에 붙은 HorizontalLayoutGroup 하나가 11장 카드를
    /// 전부 균등 분할된 그리드 칸으로 짓눌러 3존 분리 자체가 안 보였다).
    /// 재사용하는 씬 오브젝트에서 매번 확인해서 있으면 지운다 — 사용자가
    /// 에디터에서 다시 실수로 붙여도 다음 실행 때 자동으로 정리된다.</summary>
    static void StripStrayLayoutGroup(RectTransform rt)
    {
        var lg = rt.GetComponent<LayoutGroup>();
        if (lg != null) Destroy(lg);
        var fitter = rt.GetComponent<ContentSizeFitter>();
        if (fitter != null) Destroy(fitter);
    }

    /// <summary>컨테이너 하나를 통째로 회전시켜서 배치한다 — 안의 내용물은
    /// 평소처럼(회전 안 걸린 것처럼) 그리면 부모 회전 때문에 자동으로
    /// 화면에서 돌아간 모습으로 보인다. pivot을 중심(0.5,0.5)으로 둬서
    /// 회전이 그 중심을 축으로 일어나게 하고, "화면에 보이는 위쪽
    /// y좌표"(<paramref name="visualTop"/>)를 그대로 받아 내부적으로
    /// 역산한다 — <b>선언한 폭(<paramref name="declaredW"/>)이 90도
    /// 회전 후엔 화면 세로 길이가 된다</b>는 함정(이 프로젝트가 세로판
    /// 시절 이미 겪었던 것)을 여기서 한 번만 처리해 두면 호출부는
    /// 신경 쓸 필요가 없다.</summary>
    RectTransform MakeRotatedContainerByVisualTop(string name, Transform parent, float declaredW, float declaredH,
                                                   float centerX, float visualTop, float zRotation)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(declaredW, declaredH);
        rt.anchoredPosition = new Vector2(centerX, visualTop - declaredW * 0.5f);
        rt.localEulerAngles = new Vector3(0f, 0f, zRotation);
        return rt;
    }

    void RebuildUI()
    {
        HwatuUI.ClearChildren(fieldArea);
        // 더미(drawPileArea)는 여기서 안 지운다 — UpdatePileVisual이 기존
        // 레이어와 비교해서 필요한 만큼만 늘리거나(즉시) 줄인다(애니메이션
        // 후 제거). 매턴 통째로 지우고 다시 그리면 "5장 이하로 떨어질 때
        // 한 장씩 실제로 제거되는 연출"이 불가능해진다.
        HwatuUI.ClearChildren(handArea);
        HwatuUI.ClearChildren(playerCapArea);
        for (int slot = 1; slot <= 3; slot++)
        {
            if (backArea[slot]) HwatuUI.ClearChildren(backArea[slot]);
            if (capAreaAI[slot]) HwatuUI.ClearChildren(capAreaAI[slot]);
        }

        UpdatePileVisual();
        DrawField();

        // 상대 뒷패·획득패 — 슬롯 1/3(좌/우)만 그린다. 슬롯 2(상단)는
        // "Cap·Back 영역 제거" 요청으로 애초에 backArea[2]/capAreaAI[2]가
        // null이다(BuildStaticUI에서 안 만듦).
        for (int slot = 1; slot <= 3; slot += 2) // 1, 3
        {
            int seat = slotSeat[slot];
            if (seat < 0 || backArea[slot] == null) continue;
            int n = hand[seat].Count;
            // 2026-08-20: 기본 간격(BACK_W+4)으로 다 안 들어갈 만큼 좁으면
            // (씬에서 사용자가 폭을 줄인 경우 등) 그때만 겹치게 좁힌다 —
            // 카드 자체 크기(BACK_W)는 항상 그대로라 9-slice 테두리·점무늬
            // 비율이 안 깨진다. 폭이 넉넉하면 원래 간격 그대로 안 겹친다.
            float availW = backArea[slot].sizeDelta.x;
            float pitch = BACK_W + 4f;
            if (n > 1) pitch = Mathf.Min(pitch, Mathf.Max((availW - BACK_W) / (n - 1), 1f));
            float total = (n - 1) * pitch + BACK_W;
            for (int i = 0; i < n; i++)
            {
                float x = -total * 0.5f + BACK_W * 0.5f + i * pitch;
                HwatuUI.MakeCardBack(backArea[slot], new Vector2(x, 0f), BACK_W, BACK_H);
            }
            DrawAiCaptured(slot, seat);
        }

        // 왜 쉬는지에 따라 문구가 다르다 — "광팔이"는 참가하고 싶었는데
        // 자리가 없어 밀려난 경우에만 쓴다(sittingOutWasSqueezed).
        string sitOutReason = sittingOutWasSqueezed ? "(광팔이)" : "(참가 포기)";

        // 슬롯 하나(닉네임/고+점수/금액/배지)를 채운다 — 상단/좌/우/하단
        // 전부 이 함수 하나로 통일.
        // 2026-08-20: "상대방 고/스톱 선택 중일 때는 선택 중이라고 표시
        // 필요" 신고로 decidingGoStop을 추가했다 — 예전엔 myTurn이
        // state==Turn일 때만 켜져서, 정작 누군가 고/스톱을 고르는 동안엔
        // (state==GoStopChoice) 아무도 "▶" 표시를 못 받아 화면이 왜
        // 멈췄는지 알 길이 없었다.
        void FillSlot(int slot, int seat, bool myTurn, bool decidingGoStop)
        {
            var nameLbl = statusText[slot];
            var goLbl = goScoreText[slot];
            var moneyLbl = moneyText[slot];
            if (nameLbl == null) return;

            // 2026-08-20 정정(사용자 신고 — "화살표가 눈에 안 띈다") — 이름
            // 앞에 "▶ "를 붙이는 대신, 상태창 배경 자체를 강조색(노랑)으로
            // 바꾼다. 노란 배경 위 흰 글자는 안 읽히므로(2048 카드 v.., 이
            // 프로젝트 공통 함정) 글자는 밝을 때만 어두운 남색으로 뒤집는다.
            bool highlight = myTurn || decidingGoStop;
            string who = seat == PLAYER_SEAT ? "나" : SeatName(seat);
            nameLbl.text = who;

            Color darkText = new Color(0.106f, 0.133f, 0.267f, 1f); // 상태창 기본 배경색을 그대로 글자색으로
            if (statusBoxImg[slot] != null)
                statusBoxImg[slot].color = highlight ? new Color(0.929f, 0.729f, 0.180f, 0.95f) /* #EDBA2E */
                                                      : new Color(0.106f, 0.133f, 0.267f, 0.88f);
            nameLbl.color = highlight ? darkText : Color.white;
            nameLbl.fontStyle = highlight ? FontStyles.Bold : FontStyles.Normal;
            if (goLbl != null) goLbl.color = highlight ? darkText : new Color(1f, 1f, 1f, 0.82f);
            if (moneyLbl != null) moneyLbl.color = highlight ? darkText : Color.white;

            if (moneyLbl != null) moneyLbl.text = $"{money[seat]:N0}원";

            // 배지 영역은 매턴 여기서 지운다 — 전용 컨테이너라(badgeArea)
            // 매번 새로 그려도 이전 것들이 안 남는다("광팔이한테 선 아이콘이
            // 남아있다"는 신고의 원인이 바로 이 클리어 누락이었다).
            if (badgeArea[slot] != null) HwatuUI.ClearChildren(badgeArea[slot]);

            if (sittingOutSeat == seat)
            {
                if (goLbl != null) goLbl.text = $"쉬는 중 {sitOutReason}";
                return; // 쉬는 좌석은 이번 판 캡처가 없어 배지가 의미 없다 — 안 그림
            }

            int seatScore = GoStopRules.CalcScore(captured[seat], sweeps[seat]).Total;
            if (goLbl != null)
                goLbl.text = decidingGoStop ? "고/스톱 선택 중..." : $"{goCount[seat]}고 {seatScore}점";

            if (badgeArea[slot] != null)
            {
                float w = badgeArea[slot].sizeDelta.x;
                float startX = -w * 0.5f + BADGE_SIZE * 0.5f;
                DrawBadgeStrip(badgeArea[slot], seat, new Vector2(startX, 0f), w);
            }
        }

        for (int slot = 1; slot <= 3; slot++)
        {
            int seat = slotSeat[slot];
            if (seat < 0)
            {
                if (statusText[slot]) statusText[slot].text = "";
                if (badgeArea[slot] != null) HwatuUI.ClearChildren(badgeArea[slot]);
                continue;
            }
            bool myTurn = state == State.Turn && currentSeat == seat;
            bool decidingGoStop = state == State.GoStopChoice && currentSeat == seat;
            FillSlot(slot, seat, myTurn, decidingGoStop);
        }

        // 내 상태줄(슬롯0 — 하단, 항상 statusText[0] 고정 오브젝트를 쓴다).
        // 내가 쉬는 드문 판엔 slotSeat[0]이 다른 좌석일 수 있다.
        {
            int bottomSeat = slotSeat[0] < 0 ? PLAYER_SEAT : slotSeat[0];
            bool myTurn = state == State.Turn && currentSeat == bottomSeat;
            bool decidingGoStop = state == State.GoStopChoice && currentSeat == bottomSeat;
            FillSlot(0, bottomSeat, myTurn, decidingGoStop);
            ui?.SetScore(money[PLAYER_SEAT]); // HUD 점수는 항상 내 보유 머니(사용자 요청)
        }

        DrawPlayerCaptured();
        DrawPlayerHand();

        flyFrom.Clear();
        flyViaField.Clear();

        // 호스트만 — 로컬 화면이 갱신되는 매 순간 접속한 게스트들도 같이
        // 갱신시킨다(BuildSnapshot/BroadcastNetworkState 문서 참고).
        if (isNetworkHost) BroadcastNetworkState();
    }

    const int PILE_MAX_LAYERS = 5;
    const float PILE_LAYER_STEP = 2f;

    /// <summary>더미 뒷면 더미(스택) — 2026-08-18 전면 개편.
    /// "기본 5장, 5장 이하로 떨어지면 실제로 한 장씩 제거되는 연출, 배지는
    /// 필요 없음, 아래(-8)에서 위(0)로 2px씩 쌓임" 요청. 예전엔 매턴
    /// 전부 지우고 다시 그려서 "줄어드는 느낌"이 없었다 — 지금은 기존
    /// 레이어 개수와 목표 개수를 비교해서 **차이만큼만** 만들거나(즉시
    /// 등장) DOTween으로 축소·페이드시킨 뒤 제거한다(줄어드는 게 실제로
    /// 보인다). 레이어는 <c>PileLayer0</c>(맨 아래, y=-8) ~
    /// <c>PileLayer4</c>(맨 위, y=0) 이름으로 순서를 추적한다.</summary>
    /// <summary>딜링 애니메이션(<see cref="DealingAnimationSeq"/>) 시작 전에
    /// 이전 판 카드들을 화면에서 지운다 — 2026-08-20 정정(사용자 신고
    /// "cap이나 필드에 패들이 없어진 상태여야 될텐데 안 없어져서 어색해").
    /// 애니메이션은 실제 `RebuildUI()`를 일부러 뒤로 미루는데, 그러면
    /// 지난 판 필드/획득패/손패가 화면에 그대로 남은 채로 새 카드가
    /// 날아드는 것처럼 보였다 — RebuildUI가 매턴 지우는 것과 똑같은
    /// 목록(더미만 빼고)을 그대로 지운다.</summary>
    void ClearBoardForDealing()
    {
        HwatuUI.ClearChildren(fieldArea);
        HwatuUI.ClearChildren(handArea);
        HwatuUI.ClearChildren(playerCapArea);
        for (int slot = 1; slot <= 3; slot++)
        {
            if (backArea[slot]) HwatuUI.ClearChildren(backArea[slot]);
            if (capAreaAI[slot]) HwatuUI.ClearChildren(capAreaAI[slot]);
        }
    }

    void UpdatePileVisual()
    {
        int target = Mathf.Clamp(drawPile.Count, 0, PILE_MAX_LAYERS);

        var layers = new List<Transform>();
        foreach (Transform c in drawPileArea) if (c.name.StartsWith("PileLayer")) layers.Add(c);
        layers.Sort((a, b) => string.Compare(a.name, b.name, System.StringComparison.Ordinal));

        if (layers.Count > target)
        {
            // 초과분(맨 위, 즉 리스트 끝)부터 줄어들며 사라지는 연출 후 제거.
            for (int i = layers.Count - 1; i >= target; i--)
            {
                var tr = layers[i] as RectTransform;
                var img = layers[i].GetComponent<Image>();
                tr.DOScale(0.5f, 0.22f).SetEase(Ease.InBack);
                if (img != null) img.DOFade(0f, 0.22f);
                var toDestroy = layers[i].gameObject;
                DOVirtual.DelayedCall(0.22f, () => { if (toDestroy != null) Destroy(toDestroy); });
            }
        }
        else if (layers.Count < target)
        {
            for (int i = layers.Count; i < target; i++)
            {
                float y = -(PILE_MAX_LAYERS - 1 - i) * PILE_LAYER_STEP; // 0(i=4)…-8(i=0)
                var rt = HwatuUI.MakeCardBack(drawPileArea, new Vector2(0f, y), PILE_W, PILE_H);
                rt.name = "PileLayer" + i;
            }
        }
    }

    // 필드 그리드 — 12달을 6열×2행 고정 슬롯에 매핑한다. 열 간격
    // FIELD_COL_PITCH(150)는 카드 폭(140)보다 넉넉해 인접한 두 달이
    // 동시에 필드에 있어도 안 겹친다(BuildStaticUI의 FIELD_AREA_W=900이
    // 이 6열을 정확히 담도록 맞춰져 있다 — 상수를 따로 바꾸면 같이 맞출 것).
    const float FIELD_COL_PITCH = 133f; // BuildStaticUI의 FIELD_AREA_W(800)/6 — 같이 맞출 것
    const int FIELD_COLS = 6;

    /// <summary>필드 — 2026-08-18: "패가 나오고 들어가는 과정에서 계속
    /// 포지션이 바뀐다, 한 번 깔리면 고정돼야 한다"는 신고로 알고리즘을
    /// 완전히 바꿨다. 예전엔 매 RebuildUI마다 "지금 필드에 있는 달들"만
    /// 모아서 다시 꽉 채워 정렬했기 때문에, 다른 달 카드가 추가/제거될
    /// 때마다 기존 카드까지 자리를 옮겨야 했다(패킹 알고리즘이 통째로
    /// 다시 도니까). 지금은 **달 번호 자체가 고정된 그리드 좌표**다 —
    /// 1월은 항상 (열0,행0), 7월은 항상 (열0,행1)… 이런 식으로, 그 달이
    /// 필드에 있든 없든 좌표가 절대 안 바뀐다. 다른 달이 들어오고 나가는
    /// 것과 무관하게 내 달의 카드는 항상 같은 자리에 그대로 있다.
    /// 같은 달 여러 장(따닥 등)은 그 고정 슬롯 안에서 기존처럼
    /// STACK_OFFSET만큼 겹쳐 쌓는다 — 그룹핑 자체는 그대로다.</summary>
    void DrawField()
    {
        const float STACK_OFFSET = 22f;
        float fieldRowH = FIELD_H + 10f;

        var groups = field.GroupBy(c => c.month)
                          .Select(g => g.OrderBy(c => (int)c.kind).ToList());

        foreach (var g in groups)
        {
            int month = g[0].month; // 조커(month=0)는 그리드 밖 — 별도 처리 필요 없이 열0/행0 쪽에 자연히 몰릴 수 있으나 실전에서 즉시 소비되는 카드라 문제 없음
            int slotIdx = Mathf.Clamp(month - 1, 0, FIELD_COLS * 2 - 1);
            int col = slotIdx % FIELD_COLS;
            int row = slotIdx / FIELD_COLS;
            float slotX = -FIELD_COL_PITCH * (FIELD_COLS - 1) * 0.5f + col * FIELD_COL_PITCH;
            float slotY = -row * fieldRowH;

            for (int i = 0; i < g.Count; i++)
            {
                float x = slotX + i * STACK_OFFSET - (g.Count - 1) * STACK_OFFSET * 0.5f;
                var go = HwatuUI.MakeCard(g[i], fieldArea, new Vector2(x, slotY), FIELD_W, FIELD_H, null, false);
                if (flyFrom.TryGetValue(g[i], out var from))
                    StartCoroutine(SlamIn(go.transform as RectTransform, from));
            }
        }
    }

    // 6이면 1.5배 커진 카드가 존(-320/-60/+260) 간격을 침범해 옆 존과 겹친다
    // (실측으로 확인) — 5로 줄여서 겹침 없이 맞춘다.
    const int CAP_MAX_PER_ROW = 5;

    void DrawPlayerCaptured()
    {
        var cap = captured[PLAYER_SEAT];
        var gwang = cap.Where(c => c.EffectiveKind == HwatuKind.Gwang).OrderBy(c => c.month).ToList();
        var yeol = cap.Where(c => c.EffectiveKind == HwatuKind.Yeolkkeut).OrderBy(c => c.month).ToList();
        var ddi = cap.Where(c => c.EffectiveKind == HwatuKind.Ddi).OrderBy(c => c.month).ToList();
        var pi = cap.Where(c => c.EffectiveKind == HwatuKind.Pi).OrderBy(c => c.month).ToList();

        // 2026-08-18: "Cap 영역 카드들이 외곽선과 겹친다, 여백을 줘야 한다"
        // 요청 — 예전엔 baseline이 컨테이너 바닥에 정확히 닿아서(광/띠 줄
        // 하단 여백 0) 카드가 테두리에 붙어 보였다. 위/아래 8px씩 안쪽으로 뺐다.
        const float CAP_PAD = 8f;
        float baseline = -(CAP_ROW_PITCH * 2f - CAP_H) + CAP_PAD;
        DrawZone(gwang, -320f, baseline);
        DrawZone(ddi, -60f, baseline);
        DrawZone(yeol, -60f, baseline + CAP_ROW_PITCH);
        DrawZone(pi, 260f, baseline, weighted: true); // 5장이 아니라 5피(쌍피=2) 기준으로 줄바꿈

        void DrawZone(List<HwatuCard> cards, float centerX, float baselineY, bool weighted = false)
        {
            var rows = HwatuUI.GroupIntoRows(cards, CAP_MAX_PER_ROW, weighted);
            for (int row = 0; row < rows.Count; row++)
            {
                var rowCards = rows[row];
                float rowWidth = (rowCards.Count - 1) * CAP_PITCH + CAP_W;
                float y = baselineY + row * (CAP_H + 4f);
                for (int i = 0; i < rowCards.Count; i++)
                {
                    float x = centerX - rowWidth * 0.5f + CAP_W * 0.5f + i * CAP_PITCH;
                    var go = HwatuUI.MakeCard(rowCards[i], playerCapArea, new Vector2(x, y), CAP_W, CAP_H, null, false);
                    if (flyFrom.TryGetValue(rowCards[i], out var from))
                    {
                        if (flyViaField.TryGetValue(rowCards[i], out var hitPoint))
                            StartCoroutine(SlamInViaField(go.transform as RectTransform, from, hitPoint));
                        else
                            StartCoroutine(SlamIn(go.transform as RectTransform, from));
                    }
                }
            }
        }
    }

    /// <summary>상대(슬롯 1/3, 좌·우) 획득패. 2026-08-20 4차 정정 — 사용자가
    /// 씬에 직접 참조용 GameObject 5개(광/띠·끗 그룹/끗/띠/피)를 배치해
    /// 정확한 목표 구조를 지정해줬다: Cap 컨테이너(로컬 미회전 기준
    /// 가로 400×세로 200)를 **가로로 3등분**(각 1/3 폭, 세로는 꽉 채움)
    /// — 광 | 열끗+띠 그룹 | 피. 가운데 열끗+띠 그룹만 **세로로 반씩**
    /// 나눠 위쪽에 끗, 아래쪽에 띠. 이전 시도들(세로로 쌓기, 폭만 늘리기)
    /// 은 전부 "카드 스프레드 축=로컬X, 존 나열 축=로컬Y"였는데, 이번
    /// 참조 구조는 정반대(존 나열=로컬X, 존 안에서의 카드 스프레드는
    /// 여전히 로컬X이지만 존 폭 자체가 컨테이너의 1/3로 좁아짐, 로컬Y는
    /// 존별 세로 버짓으로 쓰인다) — 좌표는 참조 오브젝트의 실측값을
    /// 그대로 공식화했다(top-left 앵커 기준 값을 top-center 기준으로
    /// 환산: centerX = topLeftX - capW/2).
    /// <br/>
    /// 존 폭·세로 버짓은 <b>컨테이너의 실제 sizeDelta에서 매번 다시
    /// 계산</b>한다 — 하드코딩하면 사용자가 나중에 Cap1/Cap3 크기를 또
    /// 바꿀 때마다 여기도 다시 고쳐야 한다. 좌측 -90도·우측 +90도(기존과
    /// 동일) — 카드는 평소처럼(회전 안 걸린 것처럼) 그리면 부모 회전
    /// 때문에 자동으로 돌아간 모습으로 보인다(<see cref="DrawCapZone"/>은
    /// 이번에도 손 안 댔다).</summary>
    void DrawAiCaptured(int slot, int seat)
    {
        var cap = captured[seat];
        var gwang = cap.Where(c => c.EffectiveKind == HwatuKind.Gwang).OrderBy(c => c.month).ToList();
        var yeol  = cap.Where(c => c.EffectiveKind == HwatuKind.Yeolkkeut).OrderBy(c => c.month).ToList();
        var ddi   = cap.Where(c => c.EffectiveKind == HwatuKind.Ddi).OrderBy(c => c.month).ToList();
        var pi    = cap.Where(c => c.EffectiveKind == HwatuKind.Pi).OrderBy(c => c.month).ToList();

        var container = capAreaAI[slot];
        float capW = container.sizeDelta.x;
        float capH = container.sizeDelta.y;
        float colW = capW / 3f;
        const float CAP_PAD = 8f; // 상단 여백 — "카드가 외곽선과 겹친다" 요청

        // 3열: 광(왼쪽,전체높이) | 열끗(가운데 위쪽 절반)+띠(가운데 아래쪽
        // 절반) | 피(오른쪽,전체높이) — centerX는 참조 오브젝트 실측값을
        // 일반화한 공식(±capW/3, 가운데 0).
        DrawCapZoneInBox(container, gwang, -colW, -CAP_PAD, colW);
        DrawCapZoneInBox(container, yeol,  0f,    -CAP_PAD, colW);
        DrawCapZoneInBox(container, ddi,   0f,    -capH * 0.5f - CAP_PAD, colW);
        DrawCapZoneInBox(container, pi,    colW,  -CAP_PAD, colW, weighted: true);
    }

    /// <summary><see cref="DrawCapZone"/>의 얇은 래퍼 — 존 폭(<paramref
    /// name="boxWidth"/>)에서 한 줄에 몇 장이 들어가는지(maxPerRow)를
    /// 역산해서 넘긴다. 카드가 없으면 조용히 건너뛴다.</summary>
    void DrawCapZoneInBox(RectTransform area, List<HwatuCard> cards, float centerX, float topY, float boxWidth, bool weighted = false)
    {
        if (cards.Count == 0) return;
        int maxPerRow = Mathf.Max(1, Mathf.FloorToInt((boxWidth - CAP_AI_W) / CAP_AI_PITCH) + 1);
        float rowStep = CAP_AI_H + 3f;
        DrawCapZone(area, cards, centerX, topY, rowStep, maxPerRow, weighted);
    }

    /// <summary>상대 획득패 한 존(광/열끗/띠/피 중 하나)을 그린다 — 위쪽 기준
    /// 정렬(<paramref name="baselineY"/>가 0번째 줄, 아래로 줄이 늘어난다).
    /// <paramref name="weighted"/>가 true면 장수가 아니라 피 값(쌍피=2) 합으로
    /// 줄바꿈한다("5장씩"이 아니라 "5피씩" 쌓여야 한다는 사용자 확인 규칙).</summary>
    void DrawCapZone(RectTransform area, List<HwatuCard> cards, float centerX, float baselineY, float rowStep, int maxPerRow, bool weighted = false)
    {
        var rows = HwatuUI.GroupIntoRows(cards, maxPerRow, weighted);
        for (int row = 0; row < rows.Count; row++)
        {
            var rowCards = rows[row];
            float rowWidth = (rowCards.Count - 1) * CAP_AI_PITCH + CAP_AI_W;
            float y = baselineY - row * rowStep;
            for (int i = 0; i < rowCards.Count; i++)
            {
                float x = centerX - rowWidth * 0.5f + CAP_AI_W * 0.5f + i * CAP_AI_PITCH;
                var go = HwatuUI.MakeCard(rowCards[i], area, new Vector2(x, y), CAP_AI_W, CAP_AI_H, null, false);
                if (flyFrom.TryGetValue(rowCards[i], out var from))
                {
                    if (flyViaField.TryGetValue(rowCards[i], out var hitPoint))
                        StartCoroutine(SlamInViaField(go.transform as RectTransform, from, hitPoint));
                    else
                        StartCoroutine(SlamIn(go.transform as RectTransform, from));
                }
            }
        }
    }

    void DrawPlayerHand()
    {
        bool showBombSkip = bombCredits[PLAYER_SEAT] > 0 && state == State.Turn && currentSeat == PLAYER_SEAT;
        var h = hand[PLAYER_SEAT];
        int n = h.Count + (showBombSkip ? 1 : 0);
        float total = n * (HAND_W + 6f) - 6f;

        for (int i = 0; i < h.Count; i++)
        {
            var card = h[i];
            float x = -total * 0.5f + HAND_W * 0.5f + i * (HAND_W + 6f);
            bool playable = state == State.Turn && currentSeat == PLAYER_SEAT && field.Any(f => f.month == card.month);
            // 2026-08-18: "하이라이트 크기가 패랑 안 맞는다"는 신고 — 손패
            // 카드가 107×174로 커진 만큼 하이라이트도 다시 맞췄다(114×183,
            // posY=4, 사용자 확인 값).
            var go = HwatuUI.MakeCard(card, handArea, new Vector2(x, 0f), HAND_W, HAND_H,
                () => OnPlayerPlay(card), playable,
                highlightSize: new Vector2(114f, 183f), highlightOffset: new Vector2(0f, 4f));

            // 폭탄/흔들기/굳은자 가능 표시 — 카드 자체는 안 건드리고 작은
            // 아이콘을 모서리에 얹는다. 셋 다 손패 안에서만 조건이 갈리므로
            // (전통 규칙 (3,1) 폭탄, 3장 흔들기, 2장+필드매치0 굳은자) 서로
            // 배타적이지 않을 수 있어(흔들기·폭탄은 둘 다 "3장 보유" 조건을
            // 공유) 위치를 나눠 동시에 보여준다.
            int sameMonthHand = h.Count(c => c.month == card.month);
            int sameMonthField = field.Count(f => f.month == card.month);
            bool bombable = sameMonthHand == 3 && sameMonthField == 1;
            bool shakeable = sameMonthHand == 3 && !shookMonths[PLAYER_SEAT].Contains(card.month);
            // 2026-08-19 재수정(사용자 확인) — 바로 전 버전은 "손1+Cap2"를
            // 필드 상태와 무관하게 하나로 합쳤는데, 그러면 4번째 패가 아직
            // 남의 손/덱에 묻혀 있어 이번 턴엔 먹을 수도 없는 경우까지 표시돼
            // "필드에 매칭 패가 없는데도 뜬다"는 신고를 받았다. 손 장수로
            // 완전히 다른 두 상황을 가리킨다는 걸로 정리했다:
            //  - 손에 1장: 지금 당장 필드에도 매칭 패가 있어야("등장했을 때")
            //    표시 — 나머지 2장이 이미 Cap에 있어(capsCount==2) 이번에
            //    내면 그 자리에서 바로 내가 가져가는, 지금 당장 실행 가능한
            //    상황만 굳은자다.
            //  - 손에 2장: 나머지 2장 중 하나라도 이미 누군가의 Cap에
            //    들어갔으면("이미 다른매칭패가 누군가 먹었을때") 표시 — 필드
            //    상태와 무관하다(손에 쥔 페어 자체가 희소해지는 신호라서).
            int capsCount = 0;
            for (int s = 0; s < SEATS; s++) capsCount += captured[s].Count(c => c.month == card.month);
            bool stuckPair = (sameMonthHand == 1 && capsCount == 2 && sameMonthField >= 1)
                           || (sameMonthHand == 2 && capsCount >= 1);
            // 2026-08-19: "아이콘이 겹친다"·"굳은자 아닌데 느낌표가 보인다"
            // 신고 — 실제로는 서로 다른 두 버그가 아니라 하나였다. 폭탄(우)·
            // 흔들기(좌)·굳은자(중앙)를 카드 하단에 나란히 흩어 놓았더니,
            // 폭탄+흔들기가 동시에 뜨는 흔한 경우(둘 다 "3장 보유"가 조건이라
            // 자주 같이 뜬다) 두 아이콘이 서로 가깝게 붙어 사람 눈에는
            // "느낌표처럼 생긴 뭔가"로 뭉뚱그려 보였다. 전부 우측상단 한
            // 자리로 모으고, 여러 개면 그 자리에서 아래로 쌓는다 —
            // 굳은자는 폭탄·흔들기와 조건이 배타적(2장 vs 3장)이라 항상
            // 혼자 뜬다.
            // 2026-08-20: 위 주석은 "전부 우측상단 한 자리로 모았다"고
            // 적혀 있었지만 실제로는 폭탄/흔들기만 낡은 공식(HAND_W 기준
            // 계산)을 그대로 쓰고 있었고 굳은자(stuckPair)만 실제로
            // (40,5)로 옮겨져 있었다 — 서로 다른 위치라 시각적으로도
            // 안 맞았다("굳은자만 정상 위치, 폭탄/흔들기는 예전 자리"
            // 재신고). 셋 다 같은 시작점을 쓰도록 통일한다.
            const float ICON_S = 30f;
            float iconX = 40f, iconY = 5f;
            if (bombable) { GoStopIcons.MakeShapeIcon(go.transform, new Vector2(iconX, iconY), ICON_S, GoStopIcons.Bomb(), new Color(0.1f, 0.1f, 0.12f, 0.92f)); iconY -= ICON_S + 4f; }
            if (shakeable) { GoStopIcons.MakeShapeIcon(go.transform, new Vector2(iconX, iconY), ICON_S, GoStopIcons.Bell(), new Color(0.29f, 0.64f, 0.91f, 0.95f)); iconY -= ICON_S + 4f; }
            if (stuckPair) GoStopIcons.MakeTextIcon(go.transform, new Vector2(iconX, iconY), ICON_S, "!", new Color(0.85f, 0.2f, 0.2f), Color.white);
        }

        if (showBombSkip)
        {
            float x = -total * 0.5f + HAND_W * 0.5f + h.Count * (HAND_W + 6f);
            MakeBombSkipSlot(new Vector2(x, 0f));
        }
    }

    void MakeBombSkipSlot(Vector2 pos)
    {
        var go = new GameObject("BombSkip", typeof(RectTransform));
        go.transform.SetParent(handArea, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.sizeDelta = new Vector2(HAND_W, HAND_H);
        rt.anchoredPosition = pos;

        var frame = go.AddComponent<Image>();
        frame.sprite = HwatuShapes.RoundedRect(64, 10);
        frame.type = Image.Type.Sliced;
        frame.color = new Color(0.78f, 0.64f, 0.22f, 1f);

        var fieldGo = new GameObject("PatternField", typeof(RectTransform));
        fieldGo.transform.SetParent(go.transform, false);
        var fieldRT = fieldGo.GetComponent<RectTransform>();
        fieldRT.anchorMin = fieldRT.anchorMax = new Vector2(0.5f, 0.5f);
        fieldRT.sizeDelta = new Vector2(HAND_W - 6f, HAND_H - 6f);
        fieldRT.anchoredPosition = Vector2.zero;
        var fieldImg = fieldGo.AddComponent<Image>();
        fieldImg.sprite = HwatuShapes.DotGridPattern();
        fieldImg.raycastTarget = false;

        var capLabel = HwatuUI.MakeLabel(go.transform, new Vector2(0f, -14f), new Vector2(HAND_W - 8f, 20f), 12f, new Color(1, 1, 1, 0.9f));
        capLabel.text = "덱만";
        var numLabel = HwatuUI.MakeLabel(go.transform, new Vector2(0f, -HAND_H * 0.5f - 2f), new Vector2(HAND_W - 8f, 56f), 30f, Color.white);
        numLabel.text = bombCredits[PLAYER_SEAT].ToString();
        numLabel.fontStyle = FontStyles.Bold;

        var btn = go.AddComponent<Button>();
        btn.targetGraphic = frame;
        btn.onClick.AddListener(OnPlayerBombSkip);
    }

    // ── 게임 시작 딜링 연출 ──────────────────────────────
    /// <summary>게임을 시작하고 패를 나눠주는 연출(사용자 확인 규칙) —
    /// 1차 돌리기: 각 좌석 4장 + 필드 3장. 2차 돌리기: 각 좌석 3장 더 +
    /// 필드 3장 더. 최종 손 7장·필드 6장(<see cref="GoStopRules.DealNew3P"/>/
    /// <see cref="GoStopRules.DealNew4PFull"/>이 이미 만들어 둔 실제 딜과
    /// 정확히 같은 장수). <see cref="NewGameSeq"/>가 hand[]/field[]/drawPile[]를
    /// 전부 채운 뒤, 그 상태를 화면에 실제로 그리기(RebuildUI) 전에 호출한다
    /// — 이 코루틴은 순수하게 시각적인 카드 뒷면만 날릴 뿐 게임 상태를
    /// 전혀 건드리지 않는다.</summary>
    IEnumerator DealingAnimationSeq()
    {
        yield return StartCoroutine(DealRound(4, 3));
        yield return StartCoroutine(DealRound(3, 3));
    }

    IEnumerator DealRound(int perSeat, int toField)
    {
        for (int s = 0; s < SEATS; s++)
        {
            Vector3 dest = DealDestinationFor(s);
            for (int i = 0; i < perSeat; i++)
            {
                GoStopFX.FlyDealCard(ui.ContentArea, drawPileArea.position, dest, BACK_W, BACK_H);
                yield return new WaitForSeconds(0.035f);
            }
            yield return new WaitForSeconds(0.05f); // 다음 좌석으로 넘어가기 전 짧은 틈
        }
        for (int i = 0; i < toField; i++)
        {
            GoStopFX.FlyDealCard(ui.ContentArea, drawPileArea.position, fieldArea.position, FIELD_W, FIELD_H);
            yield return new WaitForSeconds(0.04f);
        }
        yield return new WaitForSeconds(0.12f); // 라운드 사이 여백
    }

    /// <summary>딜링 연출이 날아갈 목적지 — 내 좌석은 손패 영역, 다른 좌석은
    /// 그 좌석 정보 블록(<see cref="statusText"/>) 자리를 대신 쓴다(상단
    /// 슬롯은 Back 영역 자체가 없어서 <see cref="backArea"/>를 못 쓴다 —
    /// statusText는 4슬롯 전부 항상 존재한다).</summary>
    Vector3 DealDestinationFor(int seat)
    {
        if (seat == PLAYER_SEAT) return handArea.position;
        int slot = SlotOf(seat);
        if (slot >= 0 && statusText[slot] != null) return statusText[slot].transform.position;
        return fieldArea.position; // 방어적 폴백
    }

    // ── 연출 ─────────────────────────────────────────────
    /// <summary>출발 월드 좌표에서 최종 자리까지 짧게 이동한 뒤 살짝 부풀었다
    /// 줄어드는 펀치 스케일 — 짝을 안 맞추고 그냥 필드에 놓이는 카드
    /// (매칭 없음)에 쓰는 1단 연출.</summary>
    IEnumerator SlamIn(RectTransform rt, Vector3 fromWorld)
    {
        if (rt == null) yield break;
        yield return FlyAndPunch(rt, fromWorld, rt.position, 0.11f, 0.14f);
    }

    /// <summary>필드의 짝을 실제로 쳐서 맞추는 2단 연출 — 손/더미에서 <b>맞은
    /// 필드패 자리까지</b> 먼저 날아가 딱 맞고 튕긴 다음(1구간), 거기서 다시
    /// 최종 획득패 자리까지 날아간다(2구간). 2026-08-20: "cap으로 즉시
    /// 들어오는 느낌"이라는 신고로 2인판(GoStopGame.UI.cs)에서 이미 검증된
    /// via-field 방식을 그대로 이식했다 — v1 시절 "화면이 붐벼서" 생략했던
    /// 결정을 뒤집은 것.</summary>
    IEnumerator SlamInViaField(RectTransform rt, Vector3 fromWorld, Vector3 hitWorld)
    {
        if (rt == null) yield break;
        Vector3 toWorld = rt.position;

        yield return FlyAndPunch(rt, fromWorld, hitWorld, 0.09f, 0.10f);
        if (rt == null) yield break;

        yield return FlyAndPunch(rt, hitWorld, toWorld, 0.14f, 0.16f);
    }

    /// <summary>이동(감속) + 도착 시 임팩트 플래시 + 펀치 스케일. SlamIn 계열이
    /// 공유하는 한 구간 — 기존 SlamIn의 이동/펀치 곡선을 그대로 뽑아냈다
    /// (2인판과 달리 baseScale을 기준으로 삼는 이 파일의 관례를 유지).</summary>
    IEnumerator FlyAndPunch(RectTransform rt, Vector3 from, Vector3 to, float flyDur, float punchDur)
    {
        Vector3 baseScale = rt.localScale;

        float t = 0f;
        while (t < flyDur)
        {
            t += Time.deltaTime;
            float p = 1f - Mathf.Pow(1f - Mathf.Clamp01(t / flyDur), 3f); // ease-out
            if (rt == null) yield break;
            rt.position = Vector3.Lerp(from, to, p);
            yield return null;
        }
        if (rt == null) yield break;
        rt.position = to;
        SpawnImpactFlash(rt);

        t = 0f;
        while (t < punchDur)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / punchDur);
            float s = p < 0.4f ? Mathf.Lerp(1f, 1.28f, p / 0.4f) : Mathf.Lerp(1.28f, 1f, (p - 0.4f) / 0.6f);
            if (rt == null) yield break;
            rt.localScale = baseScale * s;
            yield return null;
        }
        if (rt != null) rt.localScale = baseScale;
    }

    /// <summary>내가 이겼을 때 화면 위에 색종이 폭죽을 터뜨린다. Canvas
    /// 바로 밑(Overlay와 같은 층)에 붙인다 — 새로 생성된 GameObject는
    /// 자동으로 마지막 sibling이 되어 Overlay보다 나중에 그려지므로
    /// 가려지지 않는다("점수 상세" 팝업의 z-order 버그를 고칠 때 확립한
    /// 것과 같은 규칙, 위 "고스톱 UI 구조화" 문서 참고).</summary>
    void PlayWinConfettiFX()
    {
        var canvasRoot = ui.ContentArea.parent.parent as RectTransform;
        if (canvasRoot == null) return;
        GoStopFX.PlayWinConfetti(canvasRoot, Vector2.zero);
    }

    /// <summary>판돈이 오갈 때 동전이 낸 좌석의 머니칩에서 받는 좌석의
    /// 머니칩으로 날아가는 연출 — 2인판(GoStopGame.UI.cs)과 같은 헬퍼를
    /// 좌석 배열에 맞게 이식했다. 광팔이·뻑 보너스처럼 <b>여러 좌석이 한
    /// 명에게 동시에 낼 때</b>는 호출자가 지불자마다 한 번씩 불러서 동전
    /// 여러 개가 동시에 날아가게 한다(1:N을 이 함수 하나로 표현하지 않고
    /// 호출 횟수로 표현 — 함수 자체는 항상 1:1).
    /// <br/>쉬는 좌석 등 화면에 슬롯이 없는 좌석(<see cref="SlotOf"/>가 -1)
    /// 이면 날아갈 시작/도착점이 없으므로 조용히 아무것도 안 한다.</summary>
    void FlyMoneyFX(int fromSeat, int toSeat, int amount)
    {
        if (amount <= 0) return;
        int fromSlot = SlotOf(fromSeat), toSlot = SlotOf(toSeat);
        if (fromSlot < 0 || toSlot < 0) return;
        var fromLbl = moneyText[fromSlot]; var toLbl = moneyText[toSlot];
        if (fromLbl == null || toLbl == null) return;
        GoStopFX.FlyMoney(ui.ContentArea, fromLbl.transform.position, toLbl.transform.position, amount);
    }

    /// <summary>충격 지점에 흰 원이 확 퍼졌다 사라지는 짧은 플래시 + 작은 파티클
    /// 스파크 — 2인판(GoStopGame.UI.cs)의 SpawnImpactFlash와 같은 패턴을
    /// 4인판에도 이식했다(2026-08-19, "애니메이션을 좀 더 역동적으로" 요청 —
    /// 예전엔 4인판 SlamIn엔 이 연출 자체가 없었다).</summary>
    void SpawnImpactFlash(RectTransform at)
    {
        var go = new GameObject("Impact", typeof(RectTransform));
        go.transform.SetParent(at.parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = at.anchorMin; rt.anchorMax = at.anchorMax; rt.pivot = at.pivot;
        rt.anchoredPosition = at.anchoredPosition;
        rt.sizeDelta = new Vector2(18f, 18f);
        var img = go.AddComponent<Image>();
        img.sprite = HwatuShapes.Circle(64);
        img.color = new Color(1f, 1f, 1f, 0.85f);
        img.raycastTarget = false;
        StartCoroutine(FlashAndDestroy(rt, img));

        // 함정 — at.parent(필드/획득패/손패 컨테이너)에 그대로 붙이면 안 된다.
        // 이 컨테이너들은 매 RebuildUI마다 ClearChildren으로 통째로 지워지는데,
        // 파티클의 DOTween 트윈(0.4~0.6초)이 끝나기 전에 다음 RebuildUI가
        // 먼저 돌면 트윈 대상 Image가 중간에 파괴돼 DOTween이 예외를 던지며
        // 그 프레임의 코루틴을 통째로 멈춘다(actionBusy가 영원히 true로 남는
        // 버그로 나타났다 — 2인판과 같은 함정, 상세 원인은 GoStopGame.UI.cs
        // 참고). ContentArea(root)처럼 절대 안 지워지는 부모에 붙이고
        // 월드 좌표를 그 공간으로 변환해서 위치만 맞춘다.
        var stableParent = ui != null ? ui.ContentArea : null;
        if (stableParent != null)
        {
            Vector2 localPos = stableParent.InverseTransformPoint(at.position);
            GoStopIcons.SpawnBurst(stableParent, localPos, new Color(1f, 0.9f, 0.6f), count: 5);
        }
    }

    IEnumerator FlashAndDestroy(RectTransform rt, Image img)
    {
        const float dur = 0.20f;
        float t = 0f;
        Vector2 startSize = rt.sizeDelta;
        Vector2 endSize = startSize * 4.5f;
        while (t < dur && rt != null)
        {
            t += Time.deltaTime;
            float p = t / dur;
            rt.sizeDelta = Vector2.Lerp(startSize, endSize, p);
            img.color = new Color(1f, 1f, 1f, Mathf.Lerp(0.85f, 0f, p));
            yield return null;
        }
        if (rt != null) Destroy(rt.gameObject);
    }
}
