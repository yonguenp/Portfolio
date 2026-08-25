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

    // 2026-08-23(씬 통합): ContentArea 밑의 4개 좌석 부모 컨테이너 —
    // ApplySeatVisibility가 채운다. BuildStaticUI가 최초 1회, 자동
    // 다운그레이드(ApplyDowngrade)가 SEATS를 바꾼 뒤 다시 채운다.
    RectTransform leftSeatT, rightSeatT, topSeatT, mySeatT;

    /// <summary>좌석 수(SEATS)에 따라 LeftSeat/RightSeat/TopSeat 컨테이너를
    /// 켜고 끄고, TopSeat 안쪽(StatusBox2 위치·Back4/Cap4)을 재구성한다.
    /// BuildStaticUI()가 최초 1회 호출하고, 자동 다운그레이드가 SEATS를
    /// 바꾼 뒤에도 다시 호출해서 화면을 새 인원수에 맞게 갱신한다 —
    /// Field/Hand/PlayerCap 등 나머지 정적 컨테이너나 팝업은 좌석 수와
    /// 무관하게 그대로 재사용되므로(GetOrCreateContainer/InstantiatePopup
    /// 둘 다 "이미 있으면 재사용") 여기서 안 건드린다(중복 생성 방지 —
    /// BuildStaticUI를 통째로 다시 부르면 팝업이 매번 새로 Instantiate돼
    /// 겹겹이 쌓인다).</summary>
    void ApplySeatVisibility(RectTransform root)
    {
        // 없으면(예: 아직 이 구조로 안 바뀐 다른 씬, 인스펙터에서 아직
        // 안 연결한 상태) root로 폴백해서 예전과 동일하게 동작한다.
        leftSeatT = leftSeatRef != null ? leftSeatRef : root;
        rightSeatT = rightSeatRef != null ? rightSeatRef : root;
        topSeatT = topSeatRef != null ? topSeatRef : root;
        mySeatT = mySeatRef != null ? mySeatRef : root;

        // 좌석 수별 on/off (사용자 확인 규칙):
        //  맞고(2인)   — Left/Right 끔, Top 켬(상대 1명을 여기로)
        //  고스톱 3인 — Left/Right 켬, Top 끔
        //  고스톱 4인 — Left/Right/Top 전부 켬
        if (leftSeatT != root) leftSeatT.gameObject.SetActive(SEATS != 2);
        if (rightSeatT != root) rightSeatT.gameObject.SetActive(SEATS != 2);
        if (topSeatT != root) topSeatT.gameObject.SetActive(SEATS != 3);
        if (mySeatT != root) mySeatT.gameObject.SetActive(true);

        // TopSeat 안쪽 — 원래(4인) 설계는 "상단엔 Cap/Back 없이 정보
        // 블록만"이었다. 2인(맞고)은 상대가 딱 1명이라 그 뒷패·획득패를
        // 어딘가엔 보여줘야 해서, TopSeat를 상대 전용 자리로 재활용하며
        // 거기에만 있는 Back4/Cap4를 켠다 — StatusBox2도 왼쪽으로
        // 밀어(-700) 그 옆에 Back4/Cap4가 들어갈 자리를 만든다. 4인일 땐
        // 원래 자리(0)로 되돌리고 Back4/Cap4를 끈다.
        if (topSeatT != root)
        {
            var statusBox2 = statusBoxRefs[2];
            if (statusBox2 != null)
            {
                var p = statusBox2.anchoredPosition;
                p.x = SEATS == 2 ? -700f : 0f;
                statusBox2.anchoredPosition = p;
            }
            var back4 = back4Ref;
            var cap4 = cap4Ref;
            if (back4 != null) back4.gameObject.SetActive(SEATS == 2);
            if (cap4 != null) cap4.gameObject.SetActive(SEATS == 2);
            if (SEATS == 2 && back4 != null && cap4 != null)
            {
                // backArea[2]/capAreaAI[2]는 원래(4인) 설계상 항상 null
                // 이었다(상단엔 Cap/Back이 없었으므로) — 2인일 때만
                // 채워서, 아래 RebuildUI의 렌더 루프가 슬롯 2도 그리게
                // 한다.
                StripStrayLayoutGroup(back4);
                StripStrayLayoutGroup(cap4);
                backArea[2] = back4;
                capAreaAI[2] = cap4;
            }
            else
            {
                // 2026-08-23: 다운그레이드 등으로 SEATS가 2가 아니게
                // 되돌아가면 backArea[2]/capAreaAI[2]도 같이 비워야
                // 한다 — 안 그러면 RebuildUI가 이미 꺼진 Back4/Cap4에
                // 계속 카드를 그리려 든다.
                backArea[2] = null;
                capAreaAI[2] = null;
            }
        }
    }

    void BuildStaticUI()
    {
        var root = ui.ContentArea;
        ApplySeatVisibility(root);

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
        // 유저 정보 슬롯으로 재활용("저기다 넣으면 될것같아" 요청). 4인
        // 모드는 Cap/Back 없이 정보 블록(닉네임/고+점수/금액/아이콘)만
        // 있다 — "상단의 Cap, Back 영역은 없애야한다" 요청. 내가 쉬는
        // 드문 판엔 세 번째 활성 AI가 이 자리에 뜨는데, 그때도 마찬가지로
        // 정보 블록만 보인다(RecomputeSeatSlots 주석 참고). 2인(맞고)
        // 모드는 위에서 이미 Back4/Cap4를 켜뒀으므로 여기선 정보 블록만
        // 그대로 채우면 된다.
        float topBottom = BuildInfoBlock(2, 0f, 520f, -10f, topSeatT);

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
        // 2026-08-22: "코드 생성 컨테이너를 씬 기본 오브젝트로" 요청 —
        // 씬에 Field가 이미 있으면(에디터에서 위치·크기를 직접 조정한
        // 것) 그대로 쓴다. 아래 fieldBottom도 사전 계산한 fieldTop이
        // 아니라 실제 fieldArea의 transform에서 역산해야, 사용자가 Field를
        // 옮겨도 그 아래(좌/우/나) 배치가 여전히 안 겹치게 자동으로 따라온다.
        fieldArea = GetOrCreateContainer(fieldAreaRef, root, "Field", new Vector2(FIELD_AREA_W, fieldRowH * 2f), new Vector2(0f, fieldTop), out _);
        float fieldBottom = fieldArea.anchoredPosition.y - fieldArea.sizeDelta.y; // pivot=(0.5,1)이라 anchoredPosition.y가 곧 윗변
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
        drawPileArea = GetOrCreateContainer(drawPileAreaRef, root, "DrawPile", new Vector2(PILE_W, PILE_H), new Vector2(pileX, pileY), out _);
        // zoneGap은 이제 DrawAiCaptured가 안 읽는다(3존 나란히 배치를
        // 접었으므로) — 예전 호출 형태만 유지하고 값 자체는 의미 없다.
        float sideBottomL = BuildEdgeSeatBlock(1, -SIDE_X, SIDE_W, fieldTop + 16f, leftSeatT, zoneGap: 0f, maxPerRow: 5, capAreaH: 0f);
        float sideBottomR = BuildEdgeSeatBlock(3, SIDE_X, SIDE_W, fieldTop + 16f, rightSeatT, zoneGap: 0f, maxPerRow: 5, capAreaH: 0f);

        // 나(아래) — 위 세 구간(중앙 필드+더미, 좌, 우) 중 가장 낮은 지점
        // 바로 아래부터 시작한다. 하드코딩된 값이 아니라 실제 배치 결과에서
        // 계산하므로, 위쪽 블록이 커져도 자동으로 밀려나 겹치지 않는다.
        // 2026-08-22: 예전엔 여기에 사용자가 실측해서 맞춘 보정값
        // (MANUAL_LAYOUT_CORRECTION=+400)을 더했었는데, 그건 "씬에 있던
        // Back1/Cap1/Back3/Cap3 오브젝트의 실제 위치"와 "코드가 커서로
        // 추정한 위치"가 어긋나서 필요했던 땜질이었다 — sideBottomL/R
        // 자체가 이제 BuildEdgeSeatBlock에서 capAreaAI[seat]의 실제
        // transform으로 정확히 계산되므로(위 BuildEdgeSeatBlock 주석
        // 참고) 이 보정이 더 이상 필요 없다. 그 씬 오브젝트들이 사라진
        // 뒤에도 이 매직 넘버만 남아있어서 "나" 섹션이 필드/더미와
        // 겹치는 회귀로 이어졌었다 — 매직 넘버 자체를 없애는 게 근본
        // 해결책이라고 판단해 걷어냈다.
        float contentBottom = Mathf.Min(centerBottom, Mathf.Min(sideBottomL, sideBottomR));
        float capY = BuildInfoBlock(0, 0f, 700f, contentBottom - 10f, mySeatT);
        playerCapArea = GetOrCreateContainer(playerCapAreaRef, mySeatT, "PlayerCap", new Vector2(1000f, CAP_ROW_PITCH * 2f), new Vector2(0f, capY - 6f), out bool playerCapExisted);
        if (!playerCapExisted) HwatuUI.AddZoneBackground(playerCapArea, CapZoneColor); // 재사용 시엔 배경을 또 얹지 않는다(중복 Image 방지)
        // 2026-08-20: "Hand 영역 posY -878로 조절" 확인 값 — 커서 계산값
        // 대신 직접 지정한다(이 파일이 반복 채택해 온, 사용자가 실측/확인한
        // 값을 그대로 박아 넣는 패턴 — Body/Card 등 다른 팝업들과 동일).
        float handY = -878f;
        handArea = GetOrCreateContainer(handAreaRef, mySeatT, "Hand", new Vector2(1000f, HAND_H), new Vector2(0f, handY), out _);

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

        gwangSalePopup.amountText.text = $"광+쌍피 {n}장 × {WON_PER_POINT}원";
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
    /// (GoStopGame.BuildScoreDetailRows)과 같은 로직·같은 시각 스타일이다.
    /// 2026-08-22: 이 아래에 "전체 획득패" 구간(<see cref="AppendAllCapsSection"/>)이
    /// 이어 붙으므로, 컨텐츠 크기 확정은 호출부(<see cref="ShowScoreDetail"/>)로
    /// 넘기고 여기선 도달한 y 커서만 돌려준다.</summary>
    float BuildScoreDetailRows(RectTransform content, List<HwatuCard> captured, GoStopRules.Score baseScore)
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
        return y;
    }

    /// <summary>결과 화면에서 "승자 점수만 보이고 다른 사람이 뭘 먹었는지
    /// 모른다"는 요청 — 참가한 전 좌석의 획득패 실물을 승자 점수 분해
    /// 바로 아래, 같은 스크롤 콘텐츠에 이어서 보여준다. 별도 탭/토글을
    /// 새로 만드는 대신 한 스크롤에 다 넣어서 구조를 단순하게 유지했다
    /// (카드 ID나 문자열이 아니라 실제 카드 이미지로 — HwatuUI.MakeCard
    /// 재사용).</summary>
    float AppendAllCapsSection(RectTransform content, float y, IEnumerable<int> seats)
    {
        var textCol = new Color(0.16f, 0.14f, 0.06f, 1f);
        y += 16f;
        var divider = HwatuUI.MakeLabel(content, new Vector2(0f, -y), new Vector2(860f, 30f), 20f, new Color(textCol.r, textCol.g, textCol.b, 0.55f));
        divider.text = "── 전체 획득패 ──";
        divider.alignment = TextAlignmentOptions.Center;
        y += 36f;

        const float cardW = 30f, cardH = 44f, cardGap = 3f, rowGap = 8f;
        const int perRow = 12;
        foreach (int seat in seats)
        {
            var pile = captured[seat];
            var nameLbl = HwatuUI.MakeLabel(content, new Vector2(0f, -y), new Vector2(860f, 30f), 20f, textCol);
            nameLbl.text = $"{SeatName(seat)} ({pile.Count}장)";
            nameLbl.fontStyle = FontStyles.Bold;
            nameLbl.alignment = TextAlignmentOptions.TopLeft;
            y += 32f;

            if (pile.Count == 0)
            {
                var empty = HwatuUI.MakeLabel(content, new Vector2(0f, -y), new Vector2(860f, 28f), 18f, new Color(textCol.r, textCol.g, textCol.b, 0.6f));
                empty.text = "(없음)";
                empty.alignment = TextAlignmentOptions.TopLeft;
                y += 32f;
            }
            else
            {
                var sorted = pile.OrderBy(c => (int)c.EffectiveKind).ThenBy(c => c.month).ToList();
                for (int i = 0; i < sorted.Count; i++)
                {
                    int col = i % perRow, row = i / perRow;
                    int rowCount = Mathf.Min(perRow, sorted.Count - row * perRow);
                    float rowWidth = (rowCount - 1) * (cardW + cardGap) + cardW;
                    float x = -rowWidth * 0.5f + cardW * 0.5f + col * (cardW + cardGap);
                    HwatuUI.MakeCard(sorted[i], content, new Vector2(x, -(y + row * (cardH + rowGap))), cardW, cardH, null, false);
                }
                int rows = Mathf.CeilToInt(sorted.Count / (float)perRow);
                y += rows * (cardH + rowGap);
            }
            y += 14f; // 다음 좌석과의 간격
        }
        return y;
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

        float rowsY = BuildScoreDetailRows(scoreDetailPopup.rowsContent, captured[pendingWinnerSeat], p.baseScore);
        var allSeats = new List<int> { pendingWinnerSeat };
        allSeats.AddRange(pendingLoserSeats);
        rowsY = AppendAllCapsSection(scoreDetailPopup.rowsContent, rowsY, allSeats);
        scoreDetailPopup.rowsContent.sizeDelta = new Vector2(scoreDetailPopup.rowsContent.sizeDelta.x, Mathf.Max(rowsY, 420f));

        var mult = new List<string>();
        if (p.goMultiplier > 1) mult.Add($"고배수 ×{p.goMultiplier}");
        // 2026-08-24 — 폭탄은 흔들기의 즉시실행 버전이라 GoStopRules에서
        // 더 이상 별도로 곱하지 않는다(heundeulCount에 이미 포함). 그래서
        // "폭탄 ×N" 줄을 따로 더하면 실제 totalMultiplier보다 부풀려
        // 보이므로 없앴다 — 대신 흔들기 줄에 "그 중 폭탄 N회"만 붙인다.
        if (p.heundeulCount > 0)
        {
            string bombNote = p.bombCount > 0 ? $", 폭탄 {p.bombCount}회 포함" : "";
            mult.Add($"흔들기 ×{1 << p.heundeulCount}({p.heundeulCount}회{bombNote})");
        }
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

        // 2026-08-22: "각자 시작 자금 → 이번 판 변동 → 현재 잔액을 보여달라"
        // 요청 — pendingMoneyBefore는 EndGame이 정산 직전에 찍어둔 스냅샷,
        // 현재 잔액은 그 이후로 안 바뀌었으니 살아있는 money[]를 그대로 쓴다.
        // 쉬는 좌석(광팔이 등)은 이번 판 정산 대상이 아니므로 참가 좌석만 나열한다.
        foot.AppendLine();
        var allSeatsForMoney = new List<int> { pendingWinnerSeat };
        allSeatsForMoney.AddRange(pendingLoserSeats);
        for (int i = 0; i < allSeatsForMoney.Count; i++)
        {
            int seat = allSeatsForMoney[i];
            int d = money[seat] - pendingMoneyBefore[seat];
            string dStr = d == 0 ? "변동 없음" : (d > 0 ? $"+{d:N0}원" : $"{d:N0}원");
            bool isMe = seat == PLAYER_SEAT;
            string line = $"{SeatName(seat)}: {pendingMoneyBefore[seat]:N0}원 → {dStr} → {money[seat]:N0}원";
            if (isMe) line = $"<color=#EDBA2E><b>{line}</b></color>";
            if (i < allSeatsForMoney.Count - 1) foot.AppendLine(line); else foot.Append(line);
        }
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
    /// <summary>2026-08-24: "statusbox 프리펩화해서 디자인 바꾸고 싶다"
    /// 요청으로, 배경+이름+고점수+금액+배지 영역 전체를 하나의 자기완결형
    /// 프리팹(<c>GoStopStatusBoxView</c>, <c>Assets/Resources/Prefabs/
    /// GoStop/UI/StatusBoxView.prefab</c>)으로 교체했다 — 사용자가 그
    /// 프리팹을 열어 배경 스프라이트·색·폰트를 직접 바꾸면 다음 실행부터
    /// 바로 반영된다. 씬에 `statusBoxRefs[slot]`이 이미 이 프리팹의
    /// 인스턴스로 연결돼 있으면 그대로 재사용(위치·너비 우선), 아직
    /// 프리팹화 이전의 빈 배경 박스만 있으면(과거 세션 산출물) 그 위치만
    /// 이어받아 새 프리팹 인스턴스로 갈아 끼운다 — 씬을 미리 손보지
    /// 않아도 자동으로 마이그레이션된다.</summary>
    float BuildInfoBlock(int slot, float centerX, float width, float topY, RectTransform root)
    {
        var existingBoxRT = statusBoxRefs[slot];
        var view = existingBoxRT != null ? existingBoxRT.GetComponent<GoStopStatusBoxView>() : null;

        if (existingBoxRT != null)
        {
            StripStrayLayoutGroup(existingBoxRT);
            centerX = existingBoxRT.anchoredPosition.x;
            width = existingBoxRT.sizeDelta.x;
            topY = existingBoxRT.anchoredPosition.y - 7f; // 프리팹 내부가 -7 오프셋으로 시작하는 것과 대응(GoStopStatusBoxView.Configure 참고)
        }

        if (view == null)
        {
            if (existingBoxRT != null) Destroy(existingBoxRT.gameObject); // 프리팹화 이전 산출물 정리
            view = HwatuUI.InstantiateUIPrefab<GoStopStatusBoxView>("StatusBoxView", root);
            view.gameObject.name = $"StatusBox{slot}"; // 씬 계층에서 식별 가능하도록(기존 이름 규칙 유지)
            ((RectTransform)view.transform).anchoredPosition = new Vector2(centerX, topY + 7f);
            statusBoxRefs[slot] = (RectTransform)view.transform;
        }
        view.Configure(width);

        statusBoxImg[slot] = view.Background;
        statusText[slot] = view.NameText;
        goScoreText[slot] = view.GoScoreText;
        moneyText[slot] = view.MoneyText;
        badgeArea[slot] = view.BadgeArea;
        statusBoxView[slot] = view;

        return topY - GoStopStatusBoxView.TotalHeight;
    }

    // 배지 위험/카운트 색 — GoStopStatusBoxView 프리팹에 고정 슬롯으로
    // 구워둔 배지(선/광박/멍박/피박/흔들기/뻑)의 상태만 여기서 갱신한다.
    static readonly Color GwangBakColor = new Color(0.69f, 0.37f, 0.86f);
    static readonly Color MeongBakColor = new Color(0.55f, 0.42f, 0.30f);
    static readonly Color PiBakColor = new Color(0.88f, 0.32f, 0.32f);
    static readonly Color ShakeDotColor = new Color(0.93f, 0.78f, 0.20f);
    static readonly Color PpeokDotColor = new Color(0.85f, 0.25f, 0.22f);

    /// <summary>선/광박/멍박/피박/흔들기/뻑 배지 — 2026-08-24부터
    /// <c>GoStopStatusBoxView</c> 프리팹이 6개 슬롯을 고정으로 갖고 있어서
    /// (씬에서 디자인 편집 가능), 여기서는 매턴 상태(표시 여부/색/카운트)만
    /// 갱신한다 — 예전처럼 <c>GoStopIcons</c>로 매번 새로 그리거나
    /// <c>ClearChildren</c>으로 지우지 않는다.</summary>
    void DrawBadgeStrip(GoStopStatusBoxView view, int seat)
    {
        var mine = captured[seat];
        var others = ActiveSeats().Where(s => s != seat).Select(s => captured[s]);
        bool gwangBak = GoStopRules.IsLiveGwangBakRisk(mine, others);
        bool meongBak = GoStopRules.IsLiveMeongBakRisk(mine, others);
        bool piBak = GoStopRules.IsLivePiBakRisk(mine, others, GoStopRules.PI_BAK_THRESHOLD_3P);

        view.SetDealer(seat == dealerSeat);
        view.SetRisk(0, gwangBak, GwangBakColor, Color.white);
        view.SetRisk(1, meongBak, MeongBakColor, Color.white);
        view.SetRisk(2, piBak, PiBakColor, Color.white);
        view.SetCountBadge(true, Mathf.Min(shookMonths[seat].Count, 2), ShakeDotColor);
        view.SetCountBadge(false, Mathf.Min(ppeokTotalCount[seat], 2), PpeokDotColor);
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
        // 2026-08-24: Find 대신 backSeatRefs[seat]/capSeatRefs[seat](인스펙터
        // 연결)로 찾는다 — seat은 1 또는 3만 들어온다(이 함수 호출부 참고).
        var existingBack = backSeatRefs[seat];
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

        var existingCap = capSeatRefs[seat];
        if (existingCap != null)
        {
            capAreaAI[seat] = existingCap;
            StripStrayLayoutGroup(existingCap);
        }
        else
        {
            capAreaAI[seat] = MakeRotatedContainerByVisualTop($"Cap{seat}", root, CAP_DECLARED_W, CAP_DECLARED_H, centerX, cursor, zRot);
            HwatuUI.AddZoneBackground(capAreaAI[seat], CapZoneColor);
        }

        // 2026-08-22: 리턴값을 "커서 누적치"가 아니라 capAreaAI[seat]의
        // 실제 transform에서 직접 역산한다 — 씬 재사용 오브젝트는 사용자가
        // 인스펙터에서 자유롭게 옮길 수 있어서, cursor 변수(위쪽에서부터
        // 크기만 빼내려간 값)가 실제 화면 위치와 어긋날 수 있다("Back/Cap을
        // 씬에서 옮기면 이 아래 구간이 처진다"던 예전 문제, MANUAL_LAYOUT_
        // CORRECTION이라는 매직 넘버로 임시 땜질했었다 — 씬의 Back1/Cap1/
        // Back3/Cap3 오브젝트가 사라진 뒤 그 보정값만 남아 "나" 섹션이
        // 필드/더미와 겹치는 회귀로 이어졌다). pivot=(0.5,0.5)·회전
        // ±90도라 실제 화면 아래쪽 끝은 `anchoredPosition.y - sizeDelta.x*0.5`
        // (회전 후 sizeDelta.x가 화면상 세로 길이가 된다는, 이 파일이 이미
        // 여러 번 문서화한 규칙)다 — 코드로 새로 만든 경우도, 사용자가
        // 씬에서 옮긴 경우도 둘 다 이 공식 하나로 정확한 실제 바닥을 얻는다.
        return capAreaAI[seat].anchoredPosition.y - capAreaAI[seat].sizeDelta.x * 0.5f;
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

    /// <summary>정적 컨테이너(Field·DrawPile·PlayerCap·Hand 등) 공통 —
    /// <paramref name="existingRef"/>(인스펙터에서 미리 연결해 둔 참조)가
    /// 있으면 그대로 재사용하고, 없으면 기본값으로 새로 만든다.
    /// Back{seat}/Cap{seat}에 이미 있던 재사용 패턴을 모든 정적 컨테이너로
    /// 일반화한 것 — "코드로 생성하던 부분을 씬 기본 오브젝트로 만들어서
    /// 에디터에서 직접 조정하게 해달라"는 요청. 카드·텍스트처럼 매 턴
    /// 달라지는 내용은 여전히 코드가 채운다 — 이 함수는 오직 "그 내용이
    /// 담기는 그릇(위치·크기)"만 다룬다. 2026-08-24: Find(name) 대신
    /// SerializeField 참조를 직접 받는 방식으로 바꿨다.</summary>
    RectTransform GetOrCreateContainer(RectTransform existingRef, RectTransform root, string name, Vector2 defaultSize, Vector2 defaultPos, out bool wasExisting)
    {
        if (existingRef != null)
        {
            StripStrayLayoutGroup(existingRef);
            wasExisting = true;
            return existingRef;
        }
        wasExisting = false;
        return HwatuUI.MakeRect(name, root, defaultSize, defaultPos);
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

        // 상대 뒷패·획득패 — 3/4인 모드는 슬롯 1/3(좌/우)만 그린다(슬롯
        // 2=상단은 "Cap·Back 영역 제거" 요청으로 backArea[2]/capAreaAI[2]가
        // null인 채로 남는다). 2026-08-23: 맞고(2인)는 반대로 상단(슬롯2)
        // 하나만 그린다 — BuildStaticUI가 SEATS==2일 때만 backArea[2]/
        // capAreaAI[2]를 채워두므로, 아래 null 체크가 자동으로 올바른
        // 슬롯만 골라낸다(2인이면 1·3은 seat<0이라 건너뛰고 2만 그려짐,
        // 3/4인이면 2는 backArea==null이라 건너뛰고 1·3만 그려짐).
        for (int slot = 1; slot <= 3; slot++)
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
            // 앞에 "▶ "를 붙이는 대신, 상태창 배경 자체를 강조색으로
            // 바꾼다. 2026-08-24 — 실제 배경/글자 색 값은
            // GoStopStatusBoxView(프리팹)의 SerializeField로 옮겼다 — 여기서는
            // "지금 강조 상태냐"만 넘긴다(디자인은 프리팹에서 직접 조정).
            bool highlight = myTurn || decidingGoStop;
            string who = seat == PLAYER_SEAT ? "나" : SeatName(seat);
            nameLbl.text = who;

            statusBoxView[slot]?.ApplyTurnState(highlight);

            if (moneyLbl != null) moneyLbl.text = $"{money[seat]:N0}원";

            if (sittingOutSeat == seat)
            {
                if (goLbl != null) goLbl.text = $"쉬는 중 {sitOutReason}";
                statusBoxView[slot]?.HideAllBadges(); // 쉬는 좌석은 이번 판 캡처가 없어 배지가 의미 없다 — 지난 상태가 안 남게 리셋
                return;
            }

            int seatScore = GoStopRules.CalcScore(captured[seat], sweeps[seat]).Total;
            if (goLbl != null)
                goLbl.text = decidingGoStop ? "고/스톱 선택 중..." : $"{goCount[seat]}고 {seatScore}점";

            if (statusBoxView[slot] != null) DrawBadgeStrip(statusBoxView[slot], seat);
        }

        for (int slot = 1; slot <= 3; slot++)
        {
            int seat = slotSeat[slot];
            if (seat < 0)
            {
                if (statusText[slot]) statusText[slot].text = "";
                statusBoxView[slot]?.HideAllBadges();
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

        CheckEmergencies();

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

    // 필드의 같은 달 카드 부채꼴 간격 — DrawField(실제 필드 카드)에서만
    // 쓴다. 슬램다운 고스트 착지 오프셋은 별도로 GhostMatchOffset이
    // 담당한다(2026-08-25, 아래 참고 — 처음엔 이 상수를 공유했었는데
    // 사용자 피드백으로 고정값(15,-15) 방식으로 바뀌었다).
    const float FIELD_STACK_OFFSET = 22f;

    /// <summary>손패/뒷패가 필드에 슬램다운으로 착지할 때 쓰는 오프셋 —
    /// "필드에 매칭되는 패에 완벽하게 겹쳐서 어색하다" 신고로 추가했다.
    /// 2026-08-25 2차 정정(사용자 확인 값) — 1차 시도는 DrawField의
    /// 부채꼴 공식(<see cref="FIELD_STACK_OFFSET"/> 기준 ±11px)을
    /// 재사용했는데 "아직 안 되는 것 같다, 너무 적나?"는 피드백을 받아
    /// 폐기하고, 매칭되는 카드의 실제 포지션에서 <b>(x+15, y-15) 고정
    /// 오프셋</b>으로 바꿨다.
    /// <br/>
    /// 2026-08-25 3차 — "뻑이 날 3번째 패는 오프셋 30,-30이 적용되는거
    /// 맞지?" 확인 요청으로, 카드가 몇 장째 그 슬롯에 쌓이는지에 비례해
    /// 오프셋이 <b>누적</b>되도록 일반화했다(1장째=0, 2장째=15,-15,
    /// 3장째=30,-30…). 호출부가 "지금 이 카드 앞에 그 슬롯에 이미 몇
    /// 장이 있는지"(<paramref name="stackCount"/>)를 넘긴다 — 이 함수
    /// 자체는 <c>field</c>를 더 이상 직접 조회하지 않는다. <c>field</c>를
    /// 여기서 조회하던 이전 버전은 "손패는 여전히 오프셋 없이 나온다"는
    /// 버그가 있었다: <see cref="GoStopRules.Resolve"/>가 매칭된 필드
    /// 카드를 캡처 커밋 *전에* 곧바로 <c>field</c>에서 Remove해버려서,
    /// r1을 계산한 뒤 field를 다시 보면 이미 매칭 카드가 사라져 있었다
    /// — 그래서 호출부가 Resolve 호출 *전*에 미리 스냅샷 뜬 개수를
    /// 넘겨야 한다.</summary>
    Vector2 GhostMatchOffset(int stackCount) =>
        stackCount > 0 ? new Vector2(15f, -15f) * stackCount : Vector2.zero;

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
                float x = slotX + i * FIELD_STACK_OFFSET - (g.Count - 1) * FIELD_STACK_OFFSET * 0.5f;
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
        // maxTopY — 이 존이 위로 올라갈 수 있는 한계(존이 소유한 세로 예산의
        // 위쪽 끝). 광/피는 컨테이너 전체(두 줄 예산)를 혼자 쓰므로 컨테이너
        // 상단(로컬 Y=0)에서 CAP_PAD만큼만 남기고, 띠/열끗은 같은 칸(-60)을
        // 절반씩 나눠 쓰므로 서로의 경계(열끗 baseline)를 넘지 못한다.
        DrawZone(gwang, -320f, baseline, -CAP_PAD);
        DrawZone(ddi, -60f, baseline, baseline + CAP_ROW_PITCH - 4f);
        DrawZone(yeol, -60f, baseline + CAP_ROW_PITCH, -CAP_PAD);
        DrawZone(pi, 260f, baseline, -CAP_PAD, weighted: true); // 5장이 아니라 5피(쌍피=2) 기준으로 줄바꿈

        void DrawZone(List<HwatuCard> cards, float centerX, float baselineY, float maxTopY, bool weighted = false)
        {
            var rows = HwatuUI.GroupIntoRows(cards, CAP_MAX_PER_ROW, weighted);
            // 2026-08-24: "Cap 높이가 낮아져서 피가 3줄 이상이면 바깥으로
            // 삐져나간다" 신고 — 줄 간격이 항상 고정(CAP_H+4)이라 이 존이
            // 가진 세로 예산(maxTopY까지)을 넘는 줄 수가 되면 그대로
            // 컨테이너 밖으로 넘쳤다. 자연 간격으로 다 못 채우는 경우에만
            // 줄 간격을 좁혀(카드를 위아래로 살짝 겹쳐) 예산 안에 눌러
            // 담는다 — 필드의 같은 달 카드를 부채처럼 겹쳐 쌓는 것과 같은
            // 원리. 완전히 겹쳐 안 보이게 되는 것만 최소 간격으로 막는다.
            const float normalStep = CAP_H + 4f;
            float step = normalStep;
            if (rows.Count > 1)
            {
                float naturalTop = baselineY + (rows.Count - 1) * normalStep;
                if (naturalTop > maxTopY)
                    step = Mathf.Max((maxTopY - baselineY) / (rows.Count - 1), CAP_H * 0.35f);
            }
            for (int row = 0; row < rows.Count; row++)
            {
                var rowCards = rows[row];
                float rowWidth = (rowCards.Count - 1) * CAP_PITCH + CAP_W;
                float y = baselineY + row * step;
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
        // 2026-08-24: "피랑 광이 윗줄부터 차는데 아래줄부터 위로 차도록"
        // 요청 — 전체 높이를 혼자 쓰는 광/피만 바닥 기준(bottomUp)으로
        // 바꿨다. 열끗/띠는 가운데 칸을 위아래로 나눠 쓰는 별개의 배치라
        // 요청 대상이 아니라 그대로 뒀다(열끗=위쪽 절반 상단고정, 띠=
        // 아래쪽 절반 상단고정 — 내 획득패(DrawPlayerCaptured)의 광/피와
        // 같은 방향으로 통일한 것이기도 하다).
        // 버그 수정 — MakeCard는 카드를 "윗변" 기준(피벗 0.5,1)으로 놓으므로
        // anchoredPosition.y는 카드의 윗변이지 아랫변이 아니다. 카드
        // 아랫변이 바닥에서 CAP_PAD만큼 떨어지게 하려면 그 윗변(=y값)은
        // 카드 높이(CAP_AI_H)만큼 더 위에 있어야 한다 — 이걸 빼먹어서
        // "카드가 cap 밖에서부터 쌓인다"(카드 아랫부분이 컨테이너 바닥을
        // 뚫고 나감) 버그가 났었다.
        float bottomY = -capH + CAP_PAD + CAP_AI_H;
        DrawCapZoneInBox(container, gwang, -colW, bottomY, colW, bottomUp: true);
        DrawCapZoneInBox(container, yeol,  0f,    -CAP_PAD, colW);
        DrawCapZoneInBox(container, ddi,   0f,    -capH * 0.5f - CAP_PAD, colW);
        DrawCapZoneInBox(container, pi,    colW,  bottomY, colW, weighted: true, bottomUp: true);
    }

    /// <summary><see cref="DrawCapZone"/>의 얇은 래퍼 — 존 폭(<paramref
    /// name="boxWidth"/>)에서 한 줄에 몇 장이 들어가는지(maxPerRow)를
    /// 역산해서 넘긴다. 카드가 없으면 조용히 건너뛴다.</summary>
    void DrawCapZoneInBox(RectTransform area, List<HwatuCard> cards, float centerX, float baselineY, float boxWidth, bool weighted = false, bool bottomUp = false)
    {
        if (cards.Count == 0) return;
        int maxPerRow = Mathf.Max(1, Mathf.FloorToInt((boxWidth - CAP_AI_W) / CAP_AI_PITCH) + 1);
        float rowStep = CAP_AI_H + 3f;
        DrawCapZone(area, cards, centerX, baselineY, rowStep, maxPerRow, weighted, bottomUp);
    }

    /// <summary>상대 획득패 한 존(광/열끗/띠/피 중 하나)을 그린다.
    /// <paramref name="bottomUp"/>이 false(기본)면 위쪽 기준 정렬(<paramref
    /// name="baselineY"/>가 0번째 줄, 아래로 줄이 늘어난다) — true면 그
    /// 반대로 <paramref name="baselineY"/>가 바닥이고 위로 줄이 늘어난다.
    /// <paramref name="weighted"/>가 true면 장수가 아니라 피 값(쌍피=2) 합으로
    /// 줄바꿈한다("5장씩"이 아니라 "5피씩" 쌓여야 한다는 사용자 확인 규칙).</summary>
    void DrawCapZone(RectTransform area, List<HwatuCard> cards, float centerX, float baselineY, float rowStep, int maxPerRow, bool weighted = false, bool bottomUp = false)
    {
        var rows = HwatuUI.GroupIntoRows(cards, maxPerRow, weighted);
        for (int row = 0; row < rows.Count; row++)
        {
            var rowCards = rows[row];
            float rowWidth = (rowCards.Count - 1) * CAP_AI_PITCH + CAP_AI_W;
            float y = bottomUp ? baselineY + row * rowStep : baselineY - row * rowStep;
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

    // ── 2026-08-23: 카드 애니메이션 시퀀스 재설계 ──────────────
    // "손패 선택 → 필드에 슬램(매칭 위치/빈 슬롯) → 뒷패도 슬램 → 그제야
    // Cap 이동 → 피 뺏기"라는 사용자 지정 순서를 구현하기 위한 헬퍼들.
    // 실제 카드(필드/Cap)는 여전히 DrawField/DrawPlayerCaptured 등 기존
    // RebuildUI 파이프라인이 field/captured 데이터를 그대로 읽어 그린다 —
    // 여기 추가된 건 그 전에 잠깐 보여주는 "고스트"(임시 GameObject)뿐이라
    // 캡처·점수·피 뺏기 등 실제 판정 로직은 전혀 안 건드린다.

    /// <summary>월(1~12) 하나가 필드에서 차지하는 그리드 슬롯의 fieldArea
    /// 로컬 좌표 — DrawField가 실제 카드를 그릴 때 쓰는 것과 완전히 같은
    /// 공식이다(그 카드가 지금 필드에 있든 없든, 매칭됐든 안 됐든 이 슬롯은
    /// 항상 같다 — 매칭되는 카드도 안 되는 카드도 결국 같은 월이면 같은
    /// 자리에 앉으므로 고스트 착지 지점을 이 공식 하나로 통일할 수 있다).</summary>
    Vector2 FieldSlotLocalPos(int month)
    {
        float fieldRowH = FIELD_H + 10f;
        int slotIdx = Mathf.Clamp(month - 1, 0, FIELD_COLS * 2 - 1);
        int col = slotIdx % FIELD_COLS;
        int row = slotIdx / FIELD_COLS;
        float slotX = -FIELD_COL_PITCH * (FIELD_COLS - 1) * 0.5f + col * FIELD_COL_PITCH;
        float slotY = -row * fieldRowH;
        return new Vector2(slotX, slotY);
    }

    Vector3 FieldSlotWorldPos(int month) => fieldArea.TransformPoint(FieldSlotLocalPos(month));

    /// <summary>슬램다운 고스트 카드를 만든다 — ContentArea(안 지워지는 안정된
    /// 부모)에 최종 착지 위치로 바로 놓는다. 실제 클릭은 안 받는 순수
    /// 연출용이라 onClick은 항상 null.</summary>
    // 2026-08-24 버그 수정 — "슬램다운 착지지점이 실제 카드 위치와 많이
    // 차이난다" 신고로 발견: `InverseTransformPoint`로 구한 로컬 좌표를
    // 그대로 `anchoredPosition`에 대입하면, 카드(HwatuUI.MakeCard, 앵커/
    // 피벗 항상 (0.5,1) 상단중앙)의 앵커가 부모(ContentArea, 피벗 (0.5,0.5)
    // 중앙)의 피벗과 다를 때 어긋난다 — anchoredPosition은 "부모 rect 위의
    // 앵커 기준점"에서 잰 값인데, InverseTransformPoint는 "부모 Transform의
    // 피벗"에서 잰 값이라 둘이 다른 기준점이다. 실측으로 확인된 어긋남은
    // Y축으로 정확히 540px(ContentArea 높이 1080 × (카드앵커.y 1.0 −
    // ContentArea피벗.y 0.5)) — 고스트가 항상 의도한 자리보다 540px 위에
    // 떨어졌다. 이 프로젝트의 다른 모든 "월드 좌표로 정확히 놓기" 헬퍼
    // (GoStopFX.FlyMoney/FlyDealCard, SlamDown/SlamIn 등)는 전부 앵커
    // 수학을 거치지 않고 생성 직후 `rt.position = worldPos`를 직접
    // 대입하는 방식을 쓴다 — 여기도 그 방식으로 통일해서, 부모/카드의
    // 피벗이 무엇이든 항상 정확히 그 월드 좌표에 놓이게 했다.
    GameObject SpawnGhostCard(HwatuCard card, Vector3 worldLandingPos)
    {
        var go = HwatuUI.MakeCard(card, ui.ContentArea, Vector2.zero, FIELD_W, FIELD_H, null, false);
        (go.transform as RectTransform).position = worldLandingPos;
        return go;
    }

    static void DestroyGhost(GameObject go) { if (go != null) Destroy(go); }
    static void DestroyGhosts(List<GameObject> list)
    {
        if (list == null) return;
        foreach (var g in list) DestroyGhost(g);
    }

    /// <summary>"공중에서 내려치는" 슬램 모션 — 기존 SlamIn(좌우/사선 이동,
    /// ease-out)과 의도적으로 다르게 만들었다. 착지 지점 위쪽에서 시작해
    /// ease-in(가속)으로 빠르게 떨어뜨린 뒤 충격 플래시 + 펀치 스케일로
    /// 마무리한다 — "카드를 탁 내려놓는다"는 손맛을 노린 것. rt.position이
    /// 이미 최종 착지 좌표로 설정돼 있어야 한다(SpawnGhostCard가 그렇게
    /// 만든다) — 그 자리를 기준으로 위쪽 시작점을 역산한다.</summary>
    IEnumerator SlamDown(RectTransform rt, float dropHeight = 170f, float dropDur = 0.10f, float punchDur = 0.12f)
    {
        if (rt == null) yield break;
        Vector3 landing = rt.position;
        Vector3 start = landing + new Vector3(0f, dropHeight, 0f);
        Vector3 baseScale = rt.localScale;
        rt.position = start;

        float t = 0f;
        while (t < dropDur)
        {
            t += Time.deltaTime;
            float p = Mathf.Pow(Mathf.Clamp01(t / dropDur), 2f); // ease-in — 내려찍는 가속감
            if (rt == null) yield break;
            rt.position = Vector3.Lerp(start, landing, p);
            yield return null;
        }
        if (rt == null) yield break;
        rt.position = landing;
        SpawnImpactFlash(rt);

        t = 0f;
        while (t < punchDur)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / punchDur);
            float s = p < 0.4f ? Mathf.Lerp(1f, 1.22f, p / 0.4f) : Mathf.Lerp(1.22f, 1f, (p - 0.4f) / 0.6f);
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
