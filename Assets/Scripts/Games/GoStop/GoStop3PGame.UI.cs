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
    static readonly Color CapZoneColor = HwatuTheme.DarkGreen; // 오리엔탈 팔레트 — 필드와 같은 짙은 초록 계열

    // 가로형 상대 좌석 블록(상/좌/우 공통) — 자리마다 필요한 폭·존 간격만
    // 다르다. (센터X, 위쪽 Y, 블록 폭, 캡 존 간격, 캡 줄당 최대 장수)를
    // seat별로 갖고 있다가 DrawAiCaptured가 그대로 재사용한다.
    struct EdgeSeatSpec { public float centerX, capZoneGap, blockWidth; public int capMaxPerRow; }
    EdgeSeatSpec[] edgeSpec = new EdgeSeatSpec[SEATS_MAX];

    // 2026-08-23(씬 통합): ContentArea 밑의 4개 좌석 부모 컨테이너 —
    // ApplySeatVisibility가 채운다. BuildStaticUI가 최초 1회, 자동
    // 다운그레이드(ApplyDowngrade)가 SEATS를 바꾼 뒤 다시 채운다.
    RectTransform leftSeatT, rightSeatT, topSeatT, mySeatT;

    // 2026-08-27 — 4인 모드 상단(슬롯2) 전용 Back/Cap. backSeatRefs[2]/
    // capSeatRefs[2]는 "씬에 미리 만들어둔 참조"용이라(항상 null, 아직
    // 아무도 씬에 안 만들었다) 여기 안 쓴다 — BuildEdgeSeatBlock(2,...)가
    // 코드로 새로 만든 실제 오브젝트는 backArea[2]/capAreaAI[2]에만
    // 기록되는데, 그 두 필드는 UpdateTopSeatCapBack이 매 라운드
    // null↔실제값으로 토글하는 대상이라(꺼진 라운드엔 null이 됨) 거기에
    // 원본 참조를 계속 의존하면 한 번 꺼진 뒤엔 다시 켤 방법이 없다.
    // 그래서 "진짜 그 오브젝트가 어디 있는지"는 이 필드에 따로 기억해 둔다.
    RectTransform back2Container, cap2Container;

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
            // 2026-09-01(목업 정합): SEATS==4는 BuildStaticUI의 Top-seat
            // 가로 배치(StatusBar|Back|Cap 옆으로 나란히, topStatusBarCenterX
            // 공식으로 계산)가 X를 직접 관리한다 — 여기서 0으로 강제하면
            // BuildInfoBlock의 "씬 재사용" 경로가 그 강제값을 그대로 읽어가
            // 매번 되돌아간다(StatusBox2가 계속 중앙(0)으로 스냅되던 버그의
            // 원인). 2인(맞고, Back4/Cap4를 옆에 붙이는 예전 가로 배치)만
            // 여전히 -700으로 직접 미는 게 맞다.
            if (SEATS == 2)
            {
                var statusBox2 = statusBoxRefs[2];
                if (statusBox2 != null)
                {
                    var p = statusBox2.anchoredPosition;
                    p.x = -700f;
                    statusBox2.anchoredPosition = p;
                }
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

    /// <summary>2026-08-27 — 4인 모드 상단(슬롯2) Cap/Back 표시 여부를 매
    /// 라운드 다시 판단한다(RebuildUI에서 매번 호출). BuildStaticUI가
    /// Back2/Cap2 공간은 이미 항상 예약해 뒀으므로(<see cref="BuildEdgeSeatBlock"/>
    /// 호출), 여기서는 그 자리에 지금 앉은 사람이 실제로 카드를 쥐고
    /// 있는지만 보고 켜고 끈다 — RecomputeSeatSlots의 slotSeat[2] 계산과
    /// 정확히 같은 조건(<c>sittingOutSeat == PLAYER_SEAT</c>)을 재사용한다:
    /// 그 조건이 참이면 슬롯2엔 실제로 플레이 중인 3번째 AI가 오고, 거짓이면
    /// 이번 판 카드를 아예 안 받은 쉬는 사람이 온다.</summary>
    void UpdateTopSeatCapBack()
    {
        if (SEATS != 4) return; // 2인은 ApplySeatVisibility가 고정으로 켜둔다, 3인은 TopSeat 자체가 꺼져 있다
        bool show = sittingOutSeat == PLAYER_SEAT;
        if (back2Container != null) back2Container.gameObject.SetActive(show);
        if (cap2Container != null) cap2Container.gameObject.SetActive(show);
        backArea[2] = show ? back2Container : null;
        capAreaAI[2] = show ? cap2Container : null;
    }

    void BuildStaticUI()
    {
        var root = ui.ContentArea;
        ApplySeatVisibility(root);

        // 오리엔탈 목업 참고 — 밋밋한 단색 테이블 배경 위에 아주 옅은 대각선
        // 격자무늬를 깔아 질감을 준다. ContentArea 자식 중 가장 먼저(=가장
        // 아래) 그려져야 하므로 SetAsFirstSibling — 뒤에 오는 필드·좌석·
        // 손패는 전부 그 위에 정상적으로 그려진다. 순수 데코라 raycastTarget
        // 은 항상 꺼져 있다(HwatuUI.AddImage 관례 그대로).
        var latticeRT = HwatuUI.MakeRect("BackgroundPattern", root, Vector2.zero, Vector2.zero);
        latticeRT.anchorMin = Vector2.zero;
        latticeRT.anchorMax = Vector2.one;
        latticeRT.offsetMin = Vector2.zero;
        latticeRT.offsetMax = Vector2.zero;
        var latticeImg = latticeRT.gameObject.AddComponent<Image>();
        latticeImg.sprite = HwatuShapes.LatticeTile();
        latticeImg.type = Image.Type.Tiled;
        latticeImg.color = Color.white; // 색은 이미 스프라이트에 구워져 있다
        latticeImg.raycastTarget = false;
        latticeRT.SetAsFirstSibling();

        // 2026-09-03 — "밋밋한 화면을 방해하지 않는 선에서 채우는" 배경
        // 바람 파티클. BackgroundPattern 바로 위(=게임 콘텐츠보다는 항상
        // 아래)에 붙는다 — GoStopWindParticles.Ensure 안에서 sibling
        // index를 직접 관리한다.
        GoStopWindParticles.Ensure(root, root.parent.parent as RectTransform);

        // HUD를 통째로 껐으므로(Start()의 SetHudVisible(false)) 뒤로가기
        // 버튼도 같이 사라졌다 — 작은 나가기 버튼 하나만 둔다.
        // 2026-08-18: "우측하단으로 옮기고, 누르면 바로 나가지 말고
        // 확인/취소 팝업으로 물어봐야 한다" 요청 — 위치를 bottom-right
        // 앵커로 옮기고, onClick을 GoToTitle 직접 호출에서 확인 팝업을
        // 여는 것으로 바꿨다(실제 나가기는 팝업의 "나가기" 버튼에서).
        // 2026-09-01: 씬에 이미 ExitBtn이 있으면(사용자가 직접 위치·크기를
        // 만져둔 것) 그걸 그대로 재사용한다 — 매번 새로 만들면 화면에 2개가
        // 겹쳐 보이는 버그가 된다(이 파일 다른 컨테이너들과 같은 원칙).
        var existingExitBtn = root.Find("ExitBtn");
        Button exitBtn;
        if (existingExitBtn != null)
        {
            exitBtn = existingExitBtn.GetComponent<Button>();
            exitBtn.onClick.RemoveAllListeners();
            exitBtn.onClick.AddListener(ShowExitConfirm);
        }
        else
        {
            exitBtn = UISkin.MakeKenneyButton(root, "ExitBtn", new Vector2(120f, 52f), Vector2.zero,
                UISkin.Accent.Red, "나가기", ShowExitConfirm);
            var exitRT = exitBtn.GetComponent<RectTransform>();
            exitRT.anchorMin = exitRT.anchorMax = new Vector2(1f, 0f);
            exitRT.pivot = new Vector2(1f, 0f);
            exitRT.anchoredPosition = new Vector2(-14f, 14f);
        }

        // 상단 중앙(슬롯2) — 참고 이미지의 "MISSION" 배너 자리를 광팔이/쉬는
        // 유저 정보 슬롯으로 재활용("저기다 넣으면 될것같아" 요청).
        // 2026-08-27(목업 4인 레이아웃 반영, 사용자 확인) — "4인 모드는
        // Cap/Back 없이 정보 블록만"이라는 예전 규칙을 뒤집었다: 목업(AI-B)은
        // 상단에도 실물 뒷패·획득패가 있다. 다만 이 자리는 "누가 앉아
        // 있는가"에 따라 의미가 달라진다 — 평소(내가 참가 중)엔 이번 판
        // 쉬는 사람이 여기 뜨는데 그 사람은 이번 판 손패·획득패 자체가
        // 없으므로(광팔이/참가포기 둘 다 카드를 아예 안 받는다) Cap/Back을
        // 켜봐야 빈 상자다. 반대로 내가 쉬는 드문 판엔 실제로 카드를 쥔
        // 활성 AI가 뜨므로 켜야 의미가 있다 — 이 판단(UpdateTopSeatCapBack)
        // 은 매 라운드 바뀌는 sittingOutSeat를 봐야 해서 RebuildUI에서
        // 매번 다시 하고, 여기서는 공간만 항상 예약해 둔다(안 그러면
        // "쉬는 사람이 안 보일 때만 Field가 올라온다"는 식으로 화면이
        // 매판 들썩인다). 2인(맞고)은 위에서 이미 Back4/Cap4를 켜뒀으므로
        // 여기선 정보 블록만 그대로 채우면 된다.
        //
        // 2026-08-27(목업 실측 재확인) — 목업 GoStopOrientalMockup의
        // Seat_Top_StatusBar/Back/Cap을 직접 열어보니 좌/우처럼 세로로
        // 쌓인 게 아니라 **가로로 나란히**(StatusBar 380 | Back 220 |
        // Cap 400, 전부 top 정렬, 간격 0) 붙어 있었다 — 예전엔 좌/우와
        // 똑같이 세로 스택으로 만들었는데 그건 틀린 구조였다. 총 폭
        // 1000을 화면 중앙(X=0)에 맞춘다.
        const float TOP_STATUSBAR_W = 380f, TOP_BACK_W = 220f, TOP_CAP_W = 400f;
        const float TOP_TOTAL_W = TOP_STATUSBAR_W + TOP_BACK_W + TOP_CAP_W; // 1000
        float topBlockLeft = -TOP_TOTAL_W * 0.5f;
        float topStatusBarCenterX = topBlockLeft + TOP_STATUSBAR_W * 0.5f;
        const float topY = -10f;
        float topBottom = BuildInfoBlock(2, topStatusBarCenterX, TOP_STATUSBAR_W, topY, topSeatT);

        // 이하 전부 "이전 블록 바로 아래" 커서 누적 방식(이 파일이 반복
        // 채택해 온 패턴) — 좌표 하드코딩으로 인한 겹침 재발을 구조적으로
        // 막는다. 가로뷰는 세로보다 높이 예산이 훨씬 빠듯해서(1080 전체 —
        // HUD를 꺼서 되찾은 116px까지 합쳐도 세로 때의 절반 수준) 상단
        // 슬롯을 얇게 만든 만큼 필드·좌우·하단이 여유를 더 가져간다.
        float topSeatBottom = topBottom;
        if (SEATS == 4)
        {
            float backCenterX = topBlockLeft + TOP_STATUSBAR_W + TOP_BACK_W * 0.5f;
            float capCenterX = topBlockLeft + TOP_STATUSBAR_W + TOP_BACK_W + TOP_CAP_W * 0.5f;

            // backSeatRefs[2]/capSeatRefs[2]는 "씬 사전 배치" 전용이라(항상
            // null, 아직 아무도 씬에 안 만들었다) 여기 안 쓴다 — 코드로 매번
            // 새로 만들고, UpdateTopSeatCapBack이 매 라운드 껐다 켰다 할 수
            // 있도록 진짜 참조를 back2Container/cap2Container에 기억해 둔다.
            back2Container = HwatuUI.MakeRect("Back2", topSeatT, new Vector2(TOP_BACK_W, BACK_CONTAINER_H), new Vector2(backCenterX, topY));
            backArea[2] = back2Container;

            cap2Container = HwatuUI.MakeRect("Cap2", topSeatT, new Vector2(TOP_CAP_W, 165f), new Vector2(capCenterX, topY));
            HwatuUI.AddZoneBackground(cap2Container, CapZoneColor);
            capAreaAI[2] = cap2Container;

            // 가로 배치라 세 요소가 전부 같은 topY에서 시작한다 — 그 중
            // 가장 큰 높이(StatusBar/Cap=165)가 이 줄 전체의 실제 바닥이다.
            topSeatBottom = topY - 165f;
        }
        float fieldTop = topSeatBottom - 14f;

        // 필드/더미 — 2026-08-18: "더미가 화면 중앙이면 필드 패 보는 게
        // 헷갈린다, 원래대로 좌상단으로" 요청으로 중앙 배치를 되돌렸다.
        // 필드는 다시 2줄 예산(더미가 줄을 안 차지하므로).
        // 2026-08-19: "Field를 800사이즈로 줄여서 DrawPile과 안 겹치게" 요청.
        // 2026-09-01: 실제 카드 배치는 이제 pos1~12 슬롯 기반(DrawField
        // 참고)이라 이 상수는 씬에 Field 참조가 아직 없는 폴백 생성 시의
        // 컨테이너 크기로만 쓰인다.
        const float FIELD_AREA_W = 800f;
        float fieldRowH = FIELD_H + 10f;
        // 2026-08-22: "코드 생성 컨테이너를 씬 기본 오브젝트로" 요청 —
        // 씬에 Field가 이미 있으면(에디터에서 위치·크기를 직접 조정한
        // 것) 그대로 쓴다. 아래 fieldBottom도 사전 계산한 fieldTop이
        // 아니라 실제 fieldArea의 transform에서 역산해야, 사용자가 Field를
        // 옮겨도 그 아래(좌/우/나) 배치가 여전히 안 겹치게 자동으로 따라온다.
        var fieldOuter = GetOrCreateContainer(fieldAreaRef, root, "Field", new Vector2(FIELD_AREA_W, fieldRowH * 2f), new Vector2(0f, fieldTop), out _);
        // 2026-09-01(사용자 씬 편집 반영) — "Field"가 이제 전체화면 스트레치
        // 래퍼로 바뀌었고, 그 밑에 실제 카드를 그리는 "FieldCards"(빈 서브
        // 컨테이너)와 "DrawPile"이 형제로 들어가 있다. fieldArea(카드 렌더링·
        // ClearChildren 대상)를 계속 "Field" 자체로 두면 매턴 ClearChildren이
        // DrawPile까지 같이 지워버린다 — FieldCards가 있으면 그쪽을 실제
        // 렌더 대상으로 쓰고, 없는(아직 이 구조로 안 바뀐) 씬은 예전처럼
        // 바깥 오브젝트를 그대로 쓴다(하위호환).
        var fieldCardsChild = fieldOuter.Find("FieldCards") as RectTransform;
        fieldArea = fieldCardsChild != null ? fieldCardsChild : fieldOuter;
        // 2026-09-02: FieldCards의 GridLayoutGroup은 여기서 일부러 스트립하지
        // 않는다(다른 재사용 컨테이너와 달리 예외) — 사용자가 pos1~12를 손으로
        // 하나씩 배치하는 대신 GridLayoutGroup이 자동으로 배열하도록 그대로
        // 살려두고 싶어 한다. 카드는 이제 fieldArea의 직계 자식이 아니라
        // 각 pos_i의 자식이라(DrawField 참고) GridLayoutGroup은 pos1~12
        // 자체의 배치에만 영향을 주고 카드 개별 위치엔 관여하지 않는다 —
        // SlamDown/FlyAndPunch가 pos_i의 실시간 transform.position을 매
        // 프레임 추적하므로, GridLayoutGroup이 pos_i를 어디로 옮기든 카드는
        // 항상 정확히 그 자리로 착지한다.
        CacheFieldPosSlots(); // pos1~pos12 마커 캐싱 — fieldArea가 정해진 직후여야 한다

        // 더미도 같은 이유로 — 사용자가 Field 밑에 "DrawPile"을 직접 만들어
        // 뒀으면(스트레치 하위 구조) 그걸 그대로 쓴다. 없으면 예전처럼
        // root(ContentArea) 밑에 만들거나 재사용한다.
        var drawPileChild = fieldOuter.Find("DrawPile") as RectTransform;

        // 오리엔탈 목업 참고 — Field가 지금까지 배경 자체가 없어서(투명 컨테이너,
        // 테이블 배경색만 그대로 비쳐 보임) "여기가 필드"라는 프레임감이 없었다.
        // 짙은 테두리+어두운 채움(panel_dark 상당)을 얹어 틀 있는 패널로 만든다.
        // Cap 존(HwatuUI.AddZoneBackground, 테두리 없는 플랫 색)과는 "테두리
        // 유무"로 구분되므로 색이 비슷해도(둘 다 짙은 초록 계열) 다시 헷갈리지
        // 않는다. 프레임은 항상 바깥(fieldOuter)에 붙인다 — fieldArea가
        // FieldCards로 바뀌어도 시각적 틀은 여전히 Field 전체 둘레여야 한다.
        // 재사용되는 씬 오브젝트에 이미 Image가 있으면(사용자가 직접
        // 붙여둔 경우) 중복으로 또 안 붙인다.
        if (fieldOuter.GetComponent<Image>() == null)
            HwatuUI.AddFramedZoneBackground(fieldOuter, HwatuTheme.DarkGreen, HwatuTheme.DeepGreen);

        // fieldBottom(아래 좌/우/나 섹션이 안 겹치게 커서를 이어받는 기준) —
        // 예전엔 "pivot=(0.5,1) 비스트레치"를 전제로 anchoredPosition.y -
        // sizeDelta.y로 역산했는데, Field/FieldCards가 전체화면 스트레치로
        // 바뀌면서 그 전제가 깨졌다(스트레치 하에선 sizeDelta가 실제 크기가
        // 아니라 인셋이다). 앵커 모드와 무관하게 항상 맞는 GetWorldCorners
        // 실측(이 파일이 이미 여러 번 써 온 방식)으로 fieldOuter의 실제
        // 화면 하단을 구해 root(ContentArea) 로컬 좌표로 환산한다.
        var fieldCorners = new Vector3[4];
        fieldOuter.GetWorldCorners(fieldCorners); // 0=좌하단
        float fieldBottom = root.InverseTransformPoint(fieldCorners[0]).y;
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
        const float SIDE_W = 380f; // StatusBar 폭(목업 실측) — Cap/Back은 별도 상수(400/300)라 안 흔들림
        const float SIDE_X = 750f;

        // 더미 — 2026-08-19: "-460,-200으로 수정" 확인 값. 필드도 800으로
        // 줄어서(위 FIELD_AREA_W 참고, 그리드 반경 400) 왼쪽 끝(-400)과
        // 더미 오른쪽 끝(-460+50=-410) 사이 10px 여백으로 안 겹친다.
        // fieldArea의 자식으로 넣지 않는 이유는 여전히 동일(ClearChildren이
        // 매턴 자식을 무차별로 지운다).
        float pileX = -460f;
        float pileY = -200f;
        drawPileArea = drawPileChild != null
            ? drawPileChild
            : GetOrCreateContainer(drawPileAreaRef, root, "DrawPile", new Vector2(PILE_W, PILE_H), new Vector2(pileX, pileY), out _);
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
        // 2026-08-27(목업 실측 재확인) — Seat_Bottom_Me_StatusBar/Cap 둘 다
        // 450×165, 간격 0으로 붙어 있었다. 예전 700×(가변 CAP_ROW_PITCH*2)는
        // 목업과 무관한 값이었다.
        float capY = BuildInfoBlock(0, 0f, 450f, contentBottom - 10f, mySeatT);
        playerCapArea = GetOrCreateContainer(playerCapAreaRef, mySeatT, "PlayerCap", new Vector2(450f, 165f), new Vector2(0f, capY), out bool playerCapExisted);
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
        // 채팅/이벤트 로그 — 이것만은 sibling 순서로 맨 위를 보장하지 않는다
        // (팝업들이 열릴 때마다 SetAsLastSibling을 스스로 불러 밀어낼 수
        // 있어서). BuildChatUI 안에서 override sorting 전용 Canvas를 얹어
        // 구조적으로 항상 최상단이 되게 한다 — 자세한 이유는 그 함수 문서 참고.
        BuildChatUI(canvasRoot);
    }

    /// <summary>선 뽑기 팝업 — 프리팹은 빈 pool 컨테이너만 갖고 있고, 실제
    /// 8장 카드는 매판 <see cref="DetermineDealerSeq"/>가 직접 그렸다 지운다.</summary>
    void BuildDealerDrawUI(RectTransform canvasRoot)
    {
        dealerDrawPopup = HwatuUI.InstantiatePopup<DealerDrawPopupView>("DealerDrawPopup", canvasRoot);
    }

    const float DRAW_CARD_W = 108f, DRAW_CARD_H = 176.4f;
    const float DRAW_COL_PITCH = 150f;
    const float DRAW_ROW_PITCH = 210f; // 카드(170)+태그(26)+여백 — 겹치지 않을 만큼

    /// <summary>2026-08-26 재설계(사용자 확인 규칙) — 8장을 Dim 배경 위에
    /// 뒷면으로 깔아두고 좌석마다 한 장씩 순서대로 고르게 한다. 다 고르면
    /// 한꺼번에 뒤집어 가장 높은 패(월이 우선, 같은 월이면 광→열끗→띠→피
    /// 순)를 고른 좌석을 선으로 정한다.
    /// <br/>8장은 반드시 서로 다른 <b>월</b>에서 한 장씩 뽑는다 — 화투
    /// 종류(광/열끗/띠/피)는 4가지뿐이라 8장 전부를 종류까지 다르게 만들
    /// 수는 없고, 실제로 동률을 막아주는 건 월이다: <see cref="DrawRank"/>가
    /// month*10+종류보너스(0~3)로 비교하므로 월이 다르면 종류 보너스가
    /// 아무리 커도 월 차이 1을 못 뒤집는다 — 월만 겹치지 않으면 그 8장
    /// 사이에서 동률이 수학적으로 나올 수 없다. "8개의 패는 종류가 겹치지
    /// 않게(동률 방지)" 요청을 이렇게 구현했다.
    /// <br/>이 코루틴 자체는 호스트/싱글플레이에서만 실행된다(게스트는
    /// NewGameSeq를 애초에 안 밟는다). 좌석은 셋으로 갈린다: 내 좌석
    /// (PLAYER_SEAT)은 로컬 클릭을 기다리고, 원격 좌석(<see cref="IsRemoteSeat"/>)
    /// 은 <see cref="GoStopNetMessage.Type.DealerDrawPrompt"/>로 "네 차례,
    /// 이 칸들은 이미 찜됐다"만 보내고 실제 클릭(<see cref="GoStopNetMessage.Type.DealerDrawPick"/>)
    /// 을 기다린다(2026-08-26, "원격 좌석에도 진짜 클릭을 받게 해달라"
    /// 요청 — 예전엔 원격 좌석도 호스트가 대신 뽑아줬었다), 나머지(AI)만
    /// 호스트 화면에서 무작위로 자동 픽한다. 카드 값은 원격 좌석에게
    /// 절대 미리 안 보낸다 — 블라인드 픽이 규칙이라 값을 미리 알려주면
    /// 원격 플레이어만 유리해진다(로컬 클릭도 뒷면 상태에서 고르므로
    /// 형평성이 맞는다).</summary>
    IEnumerator DetermineDealerSeq()
    {
        dealerDrawPopup.Show();
        dealerDrawPopup.resultText.text = "";
        HwatuUI.ClearChildren(dealerDrawPopup.pool);

        var deck = GoStopDeck.BuildFull();
        var months = Enumerable.Range(1, 12).OrderBy(_ => Random.value).Take(8).ToList();
        var pool = months
            .Select(m => { var cands = deck.Where(c => c.month == m).ToList(); return cands[Random.Range(0, cands.Count)]; })
            .OrderBy(_ => Random.value) // 화면에서 월 순서로 안 읽히게 한 번 더 섞는다
            .ToList();

        var slots = new GameObject[8];
        for (int i = 0; i < 8; i++)
        {
            int col = i % 4, row = i / 4;
            var pos = new Vector2(-225f + col * DRAW_COL_PITCH, -row * DRAW_ROW_PITCH);
            var backRT = HwatuUI.MakeCardBack(dealerDrawPopup.pool, pos, DRAW_CARD_W, DRAW_CARD_H);
            var img = backRT.GetComponent<Image>();
            img.raycastTarget = true;
            var btn = backRT.gameObject.AddComponent<Button>();
            btn.targetGraphic = img;
            int captured = i;
            btn.onClick.AddListener(() => pendingDealerPickIndex = captured);
            slots[i] = backRT.gameObject;
        }

        var pickedBy = new int[8];
        for (int i = 0; i < 8; i++) pickedBy[i] = -1;

        for (int s = 0; s < SEATS; s++)
        {
            dealerDrawPopup.promptText.text = $"{SeatName(s)} 차례 - 카드를 고르세요";
            int chosen;
            if (s == PLAYER_SEAT)
            {
                pendingDealerPickIndex = -1;
                yield return new WaitUntil(() => pendingDealerPickIndex >= 0);
                chosen = pendingDealerPickIndex;
            }
            else if (IsRemoteSeat(s))
            {
                var taken = new bool[8];
                for (int k = 0; k < 8; k++) taken[k] = pickedBy[k] != -1;
                GoStopNetLobby.Instance?.SendToSeat(s, GoStopNetMessage.DealerDrawPrompt(taken));
                GoStopNetMessage msg = null;
                yield return StartCoroutine(WaitForRemoteMessage(s,
                    m => m.type == GoStopNetMessage.Type.DealerDrawPick, m => msg = m));
                // design.md §50.1과 같은 원칙 — 타임아웃/범위 밖/이미 찜된
                // 칸이면 나머지 중 무작위로 대신 뽑아 판이 안 멈추게 한다.
                chosen = msg != null ? msg.seat : -1;
                if (chosen < 0 || chosen >= 8 || pickedBy[chosen] != -1)
                {
                    var avail = Enumerable.Range(0, 8).Where(i => pickedBy[i] == -1).ToList();
                    chosen = avail[Random.Range(0, avail.Count)];
                }
            }
            else
            {
                yield return new WaitForSeconds(0.5f); // "생각하는 척"
                var avail = Enumerable.Range(0, 8).Where(i => pickedBy[i] == -1).ToList();
                chosen = avail[Random.Range(0, avail.Count)];
            }
            pickedBy[chosen] = s;
            slots[chosen].GetComponent<Button>().interactable = false; // 재선택 방지 + 기본 disabled 틴트로 살짝 어두워짐
            // 2026-08-26: "어떤 카드를 누가 골랐는지 바로 알려주면 좋겠다"
            // 요청 — 예전엔 다 고른 뒤 뒤집는 순간에야 이름표를 붙였는데,
            // 이제 뒷면 상태 그대로 고른 즉시 이름표부터 붙인다(값은 여전히
            // 안 보인다 — 블라인드 픽 자체는 그대로 유지). 아래 공개 루프는
            // 이 태그를 그대로 재사용하고 새로 만들지 않는다.
            var pickPos = (slots[chosen].transform as RectTransform).anchoredPosition;
            var pickTag = HwatuUI.MakeLabel(dealerDrawPopup.pool, pickPos + new Vector2(0f, -DRAW_CARD_H - 4f), new Vector2(DRAW_CARD_W, 26f), 18f, Color.white);
            pickTag.text = SeatName(s);
            GoStopAudio.Instance?.CardPlay();
        }

        dealerDrawPopup.promptText.text = "";
        yield return new WaitForSeconds(0.3f);

        // 고른 카드만 순서대로 뒤집어 공개(안 고른 나머지는 뒷면 그대로 방치).
        // 이름표는 위 픽 단계에서 이미 붙여뒀으므로 여기서는 카드 얼굴만 바꾼다.
        for (int i = 0; i < 8; i++)
        {
            if (pickedBy[i] == -1) continue;
            var pos = (slots[i].transform as RectTransform).anchoredPosition;
            Object.Destroy(slots[i]);
            HwatuUI.MakeCard(pool[i], dealerDrawPopup.pool, pos, DRAW_CARD_W, DRAW_CARD_H, null, false);
            GoStopAudio.Instance?.CardPlay();
            yield return new WaitForSeconds(0.25f);
        }

        int best = -1;
        for (int i = 0; i < 8; i++)
            if (pickedBy[i] != -1 && (best == -1 || DrawRank(pool[i]) > DrawRank(pool[best])))
                best = i;

        dealerSeat = pickedBy[best];
        AppendChatLine($"{SeatNameFor(dealerSeat, -1)}이(가) 선이 되었습니다");
        dealerDrawPopup.resultText.text = $"{SeatName(dealerSeat)}이(가) 선입니다!";
        GoStopAudio.Instance?.Bonus(); // 결과가 정해지는 순간의 반짝이는 차임
        yield return new WaitForSeconds(1.1f);

        dealerDrawPopup.Hide();
    }

    /// <summary>게스트 전용 — 호스트가 보낸 <see cref="GoStopNetMessage.Type.DealerDrawPrompt"/>
    /// 를 받아 내 화면에도 같은 8칸 뒷면 팝업을 그린다. 이미 찜된 칸만
    /// <paramref name="taken"/>으로 받고 카드 값은 전혀 안 온다(호스트의
    /// <see cref="DetermineDealerSeq"/> 문서 참고 — 블라인드 픽 형평성).
    /// 고르면 그 즉시 호스트에 결과를 보내고 내 화면은 바로 닫는다 —
    /// 다른 좌석이 고르는 과정이나 최종 공개 연출은 호스트 화면 전용으로
    /// 남겨둔다(이 파일의 다른 타깃 프롬프트들과 같은 원칙 — 참가 선언/
    /// 필드 선택 팝업도 게스트 쪽은 결과만 보내고 바로 닫힌다).</summary>
    void ShowDealerDrawPickPopupForGuest(bool[] taken)
    {
        HwatuUI.ClearChildren(dealerDrawPopup.pool);
        dealerDrawPopup.promptText.text = "카드를 고르세요";
        dealerDrawPopup.resultText.text = "";

        for (int i = 0; i < 8; i++)
        {
            int col = i % 4, row = i / 4;
            var pos = new Vector2(-225f + col * DRAW_COL_PITCH, -row * DRAW_ROW_PITCH);
            var backRT = HwatuUI.MakeCardBack(dealerDrawPopup.pool, pos, DRAW_CARD_W, DRAW_CARD_H);
            var img = backRT.GetComponent<Image>();
            img.raycastTarget = true;
            var btn = backRT.gameObject.AddComponent<Button>();
            btn.targetGraphic = img;
            btn.interactable = !taken[i]; // 이미 찜된 칸은 기본 disabled 틴트로 어두워지고 클릭도 막힌다
            int captured = i;
            btn.onClick.AddListener(() => OnDealerDrawPickClicked(captured));
        }

        dealerDrawPopup.Show();
    }

    void OnDealerDrawPickClicked(int index)
    {
        GoStopNetLobby.Instance.SendToHost(GoStopNetMessage.DealerDrawPick(index));
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

    // 2026-08-19: FieldChoicePopup 카드 크기(사용자 확인) — 이 팝업은
    // 2인/4인이 같은 프리팹을 공유하는데, 예전엔 각 게임 자신의
    // FIELD_W/H(4인=140×160, 2인=92×114)를 그대로 재사용해서 게임마다
    // 다른 크기로 어긋났었다. 이 팝업 전용 고정 카드 크기를 따로 둬서
    // 두 게임이 동일한 결과를 내게 했다. 2026-09-01: 하이라이트는 이제
    // CardFront 프리팹 내부의 Highlight 자식(스트레치 앵커)이 카드
    // 크기에 자동으로 맞춰지므로, 이 크기와 무관하게 항상 정확히 맞는다.
    const float CHOICE_CARD_W = 94f, CHOICE_CARD_H = 154f;

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
                () => OnFieldChoiceClicked(c), true);
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

    /// <summary>점수 항목 줄(라벨+점수) 밑에 관여한 카드 실물을 늘어놓는다.
    /// 이 아래에 "전체 획득패" 구간(<see cref="AppendAllCapsSection"/>)이
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
                lbl.font = HwatuTheme.FontBold;
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
            nameLbl.font = HwatuTheme.FontBold;
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
    // 오리엔탈 팔레트 — "색은 하나의 의미만"(위험=레드, 턴/보상=골드) 원칙에 맞춰
    // 광박/멍박/피박 3종을 전부 같은 레드로, 흔들기/뻑 카운트는 같은 골드로 통일했다
    // (예전엔 보라/갈색/레드로 각자 달라서 "위험"이라는 의미가 색으로 안 읽혔다).
    static readonly Color GwangBakColor = HwatuTheme.HwatuRed;
    static readonly Color MeongBakColor = HwatuTheme.HwatuRed;
    static readonly Color PiBakColor = HwatuTheme.HwatuRed;
    static readonly Color ShakeDotColor = HwatuTheme.Gold;
    static readonly Color PpeokDotColor = HwatuTheme.Gold;

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

        // 목업 ScoreRow의 "광 X · 멍 Y · 피 Z" — 피는 장수가 아니라
        // EffectivePiValue 합(쌍피=2)이라야 실제 점수 집계와 일치한다.
        int gwangCount = mine.Count(c => c.EffectiveKind == HwatuKind.Gwang);
        int meongCount = mine.Count(c => c.EffectiveKind == HwatuKind.Yeolkkeut);
        int piCount = mine.Where(c => c.EffectiveKind == HwatuKind.Pi).Sum(c => c.EffectivePiValue);
        view.SetCounts(gwangCount, meongCount, piCount);
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

        // 2026-08-26(목업 정합) — "±90도로 눕혀서 배치"를 걷어내고 목업·이
        // 함수 원래 요약 주석("가로뷰는 폭이 넉넉해서 좌/우도 눕힐 필요가
        // 없다")이 뜻하던 비회전 가로 배치로 되돌렸다. 카드를 안 눕혀야
        // 목업처럼 바로 읽힌다는 게 이번 요청의 핵심이다 — 손패 뒷면·
        // DrawAiCaptured/DrawCapZone 안의 카드 하나하나는 원래도 "로컬
        // 좌표 그대로" 그려지므로(회전은 부모 컨테이너에만 걸려 있었다)
        // 여기서 회전을 빼는 것 자체는 그 아래 렌더링 코드를 전혀
        // 안 건드려도 된다.
        //
        // 2026-08-27(목업 실측 재확인) — 크기·간격을 GoStopOrientalMockup
        // 씬(Seat_Left_StatusBar/Back/Cap)을 직접 열어 잰 값으로 정정했다.
        // StatusBar(165)→Back(70)→Cap(165)이 간격 0으로 딱 붙어 있었다 —
        // 예전 코드가 쓰던 6px 갭은 근거 없는 임의값이었다.
        const float BACK_DECLARED_W = 300f;
        const float CAP_DECLARED_W = 400f;
        const float CAP_DECLARED_H = 165f;

        // 2026-08-20: "직접 수정할 수 있게 미리 만들어달라" 요청 — 씬에
        // Back{seat}/Cap{seat}가 이미 있으면(에디터에서 사용자가 손으로
        // 위치·크기를 다듬어 둔 것) 그대로 재사용한다. 코드가 매
        // RebuildUI/씬 로드마다 값을 덮어쓰지 않는다는 뜻이라, 사용자가
        // 인스펙터에서 바꾼 값이 그대로 유지된다. 없으면(예: 다른 슬롯,
        // 혹은 사용자가 아직 안 만진 상태) 기존처럼 코드가 계산해서
        // 새로 만든다. 씬에 예전 회전(±90도) 버전이 아직 남아있으면
        // MigrateEdgeContainerIfRotated가 이번 한 번만 비회전 기본값으로
        // 되돌린다(그 뒤로는 다시 자유롭게 손으로 조정 가능).
        var existingBack = backSeatRefs[seat];
        if (existingBack != null)
        {
            StripStrayLayoutGroup(existingBack);
            MigrateEdgeContainerIfRotated(existingBack, BACK_DECLARED_W, BACK_CONTAINER_H, centerX, cursor);
            backArea[seat] = existingBack;
        }
        else
        {
            backArea[seat] = HwatuUI.MakeRect($"Back{seat}", root, new Vector2(BACK_DECLARED_W, BACK_CONTAINER_H), new Vector2(centerX, cursor));
        }
        cursor -= backArea[seat].sizeDelta.y;

        var existingCap = capSeatRefs[seat];
        if (existingCap != null)
        {
            StripStrayLayoutGroup(existingCap);
            MigrateEdgeContainerIfRotated(existingCap, CAP_DECLARED_W, CAP_DECLARED_H, centerX, cursor);
            capAreaAI[seat] = existingCap;
        }
        else
        {
            capAreaAI[seat] = HwatuUI.MakeRect($"Cap{seat}", root, new Vector2(CAP_DECLARED_W, CAP_DECLARED_H), new Vector2(centerX, cursor));
            HwatuUI.AddZoneBackground(capAreaAI[seat], CapZoneColor);
        }

        // 2026-08-22: 리턴값을 "커서 누적치"가 아니라 capAreaAI[seat]의
        // 실제 transform에서 직접 역산한다 — 씬 재사용 오브젝트는 사용자가
        // 인스펙터에서 자유롭게 옮길 수 있어서, cursor 변수가 실제 화면
        // 위치와 어긋날 수 있다. 비회전(top-pivot 0.5,1)이라 실제 바닥은
        // 단순히 anchoredPosition.y - sizeDelta.y다.
        return capAreaAI[seat].anchoredPosition.y - capAreaAI[seat].sizeDelta.y;
    }

    /// <summary>2026-08-26 — 좌/우 Back·Cap을 회전(±90도) 방식에서 비회전
    /// 가로 배치로 되돌리면서, 씬에 이미 있던 예전 회전 버전(사용자가 손으로
    /// 다듬어 둔 Back1/Cap1/Back3/Cap3)을 자동으로 새 기본값으로 옮기는
    /// 1회성 마이그레이션. z회전이 이미 0이면(마이그레이션 완료했거나 애초에
    /// 비회전으로 만들어진 새 오브젝트) 아무것도 안 건드리고 그대로 재사용한다
    /// — 이후엔 사용자가 씬에서 다시 자유롭게 위치·크기를 조정할 수 있다.</summary>
    static void MigrateEdgeContainerIfRotated(RectTransform rt, float defaultW, float defaultH, float centerX, float visualTop)
    {
        if (Mathf.Approximately(rt.localEulerAngles.z, 0f)) return;
        rt.localEulerAngles = Vector3.zero;
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.sizeDelta = new Vector2(defaultW, defaultH);
        rt.anchoredPosition = new Vector2(centerX, visualTop);
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

    void RebuildUI()
    {
        UpdateTopSeatCapBack(); // 매 라운드 sittingOutSeat가 바뀌므로 항상 최신 상태로 재판단
        // pos1~pos12 마커는 이제 영구 고정 자식이다 — fieldArea 자체를
        // ClearChildren하면 마커까지 같이 파괴되므로, 마커 밑에 실제로 붙은
        // 카드(pos의 자식)만 각자 지운다. ClearFieldPosSlots 참고.
        ClearFieldPosSlots();
        // 더미(drawPileArea)는 여기서 안 지운다 — UpdatePileVisual이 기존
        // 레이어와 비교해서 필요한 만큼만 늘리거나(즉시) 줄인다(애니메이션
        // 후 제거). 매턴 통째로 지우고 다시 그리면 "5장 이하로 떨어질 때
        // 한 장씩 실제로 제거되는 연출"이 불가능해진다.
        HwatuUI.ClearChildren(handArea);
        // 2026-08-27(목업 LayoutGroup 반영) — playerCapArea/capAreaAI[slot]는
        // 이제 광/끗/띠/피 리프 존을 가진 고정 하위구조(HLG+VLG+GridLayoutGroup,
        // EnsureCapLayoutHierarchy 참고)를 담고 있다. 컨테이너 전체를 여기서
        // ClearChildren하면 그 구조 자체(광/끗띠/피 GameObject)까지 매턴
        // 통째로 부쉈다 다시 만드는 낭비가 되므로, 리프 존 4개만 개별적으로
        // 지우는 걸 DrawPlayerCaptured/DrawAiCaptured 안으로 옮겼다.
        for (int slot = 1; slot <= 3; slot++)
            if (backArea[slot]) HwatuUI.ClearChildren(backArea[slot]);

        UpdatePileVisual();
        DrawField();

        // 상대 뒷패·획득패 — 3인은 슬롯 1/3(좌/우)만 그린다(TopSeat 자체가
        // 꺼져 있다). 2인(맞고)은 반대로 상단(슬롯2) 하나만 그린다
        // (BuildStaticUI가 SEATS==2일 때만 backArea[2]/capAreaAI[2]를
        // Back4/Cap4로 채워둔다). 4인은 좌/우는 항상 그리고, 상단(슬롯2)은
        // UpdateTopSeatCapBack이 매 라운드 판단한 결과(backArea[2]가
        // null이면 이번 판 쉬는 사람이 앉아 있다는 뜻 — 카드가 없으니
        // 건너뛴다)를 그대로 따른다.
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
                HwatuUI.MakeCardBack(backArea[slot], new Vector2(x, 0f), BACK_W, BACK_H, true);
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
                statusBoxView[slot]?.SetDim(true);
                return;
            }
            statusBoxView[slot]?.SetDim(false); // 슬롯이 영구적이라 쉬다가 다시 참가한 판엔 명시적으로 꺼줘야 한다

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
                statusBoxView[slot]?.SetDim(false); // 이 슬롯이 지난 판엔 쉬는 좌석이었을 수 있다 — dim이 안 남게 리셋
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

        // 2026-09-01: "내가 쉴 때는 MySeat의 Hand/Info 안 Cap을 꺼달라"
        // 요청 — 이번 판 손패·획득패 자체가 없으니(광팔이/참가포기 둘 다
        // 카드를 아예 안 받는다) 빈 상자를 보여줄 이유가 없다. 상단
        // Cap/Back을 여닫는 UpdateTopSeatCapBack과 정확히 같은 조건
        // (sittingOutSeat == PLAYER_SEAT)을 재사용한다 — 내가 쉬는 판엔
        // 상단에 실제로 플레이 중인 3번째 AI가 뜨는 것과 대칭되는 처리.
        // 콘텐츠 자체는 그대로 그려 둔다(hand[PLAYER_SEAT]가 비어 있어
        // 어차피 아무것도 안 그려지는 무해한 호출이라, 숨김 여부와
        // 무관하게 매턴 갱신해 두면 다시 보일 때도 별도 처리가 필요 없다).
        bool iAmSittingOut = sittingOutSeat == PLAYER_SEAT;
        handArea.gameObject.SetActive(!iAmSittingOut);
        playerCapArea.gameObject.SetActive(!iAmSittingOut);

        DrawPlayerCaptured();
        DrawPlayerHand();

        flyFrom.Clear();
        flyViaField.Clear();
        flyFromSize.Clear();

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
        ClearFieldPosSlots(); // RebuildUI와 같은 이유 — pos 마커는 그대로 두고 그 자식 카드만 지운다
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

    /// <summary>2026-09-02: "pos1~12를 꼭 지워야 하나? 카드를 각 pos에
    /// attach시키고 싶다" 요청으로 캐싱 대상을 값(Vector2)에서 참조
    /// (RectTransform)로 바꿨다 — pos1~pos12는 이제 fieldArea의 영구
    /// 고정 자식이라 RebuildUI가 더 이상 지우지 않으므로(ClearFieldPosSlots
    /// 참고, 자식인 카드만 지운다) 참조를 캐싱해도 안전하다. BuildStaticUI가
    /// fieldArea를 정한 직후 한 번만 부른다. 없는 번호는 null로 남는다(방어적
    /// 폴백 — DrawField/FieldSlotWorldPos가 null이면 fieldArea 자신으로
    /// 대체한다).</summary>
    void CacheFieldPosSlots()
    {
        for (int i = 1; i <= 12; i++)
            fieldPosSlots[i] = fieldArea.Find("pos" + i) as RectTransform;
    }

    /// <summary>이 카드가 이미 슬롯을 배정받았으면 그대로, 같은 달 카드가
    /// 이미 필드에 있으면 그 슬롯에 같이 쌓인다(뻑처럼 한 달에 여러 장이
    /// 몰릴 때 "겹쳐 쌓인다"는 전통적인 모양 — 2026-09-02 정정, 처음엔
    /// 카드마다 무조건 새 빈 슬롯을 줬는데 그러면 뻑 무더기가 12칸에
    /// 뿔뿔이 흩어져 버렸다). 그 외엔 pos1~12 중 아직 아무도 안 쓰는 가장
    /// 낮은 번호를 새로 배정한다("빈칸에 카드를 배치, pos1이 비어있으면
    /// pos1에" 요청 그대로). 조커는 월이 없어 이 그룹핑 대상에서 제외 —
    /// 항상 자기만의 새 슬롯을 받는다. 카드 참조 자체가 키라 같은 카드에
    /// 대해 몇 번을 불러도 항상 같은 슬롯을 돌려준다(멱등) — 애니메이션
    /// 착지 지점 계산과 실제 렌더링이 이 함수 하나를 공유하므로 둘이
    /// 어긋날 수 없다.</summary>
    int AssignFieldSlot(HwatuCard card)
    {
        if (fieldSlotAssign.TryGetValue(card, out var existing)) return existing;

        if (!card.isJoker)
        {
            foreach (var kv in fieldSlotAssign)
            {
                if (!kv.Key.isJoker && kv.Key.month == card.month)
                {
                    fieldSlotAssign[card] = kv.Value;
                    return kv.Value;
                }
            }
        }

        for (int i = 1; i <= 12; i++)
        {
            if (!fieldSlotAssign.ContainsValue(i))
            {
                fieldSlotAssign[card] = i;
                return i;
            }
        }
        return 1; // 이론상 도달 안 함(맞고 기준 최대 11장) — 방어적 폴백
    }

    /// <summary>field 리스트와 슬롯 배정 상태를 맞춘다 — field에서 빠진
    /// 카드(캡처됨)는 슬롯을 반납하고, field에 있는데 아직 배정이 없는
    /// 카드(새로 깔린 패)는 새로 배정한다. DrawField 맨 앞에서 매번 불러
    /// "지금 필드에 실제로 있는 카드들"과 배정 상태가 항상 일치하게
    /// 만든다 — GoStopRules가 필드를 직접 Remove/Add하는 여러 경로를
    /// 일일이 쫓아다니지 않아도, 여기 한 곳에서 자연히 정리된다.
    ///
    /// 2026-09-02 버그 수정 — "field에 없으면 곧바로 반납"이 너무 성급했다.
    /// 뒷패(덱카드)는 슬램다운 애니메이션 때(SpawnGhostCard/AssignFieldSlot로
    /// 슬롯을 이미 배정받음) → GoStopRules.Resolve(drawn, field) 호출(매칭
    /// 없으면 이때 비로소 field.Add) 사이에 손패 결과를 반영하는 RebuildUI가
    /// 한 번 더 낀다(PlaySeq의 "④ 손패 결과 배치" 단계) — 그 중간 시점엔
    /// drawn이 아직 field에 없어서(진짜로 캡처된 게 아니라 그냥 아직 안
    /// 넣은 것뿐인데도) 여기서 슬롯을 반납해버렸다. 반납된 슬롯은 그 사이
    /// 캡처로 비워진 다른 슬롯(예: 매칭된 손패가 있던 자리)에 밀려나고,
    /// drawn이 실제로 field.Add된 뒤 다시 배정받을 땐 그 새로 빈 슬롯을
    /// 받아버려서 — 고스트는 pos2에 착지했는데 실제 카드는 pos1에 그려지는
    /// 불일치("패가 다른 자리로 순간이동한다")가 났다. **진짜로 캡처된
    /// 카드만 반납**하도록 기준을 "field에 없다"에서 "누군가의 captured에
    /// 들어갔다"로 좁혔다 — 이 게임 규칙상 필드를 떠나는 카드는 항상 어느
    /// 좌석의 captured로 들어가므로(조커도 ResolveBonusJoker가 항상 캡처
    /// 처리한다), "아직 field에 없지만 캡처되지도 않은" 중간 상태의 카드는
    /// 슬롯을 그대로 유지한 채 다음 배정을 기다린다.</summary>
    void SyncFieldSlotAssignments()
    {
        if (fieldSlotAssign.Count > 0)
        {
            var stale = fieldSlotAssign.Keys
                .Where(c => !field.Contains(c) && captured.Any(cap => cap != null && cap.Contains(c)))
                .ToList();
            foreach (var c in stale) fieldSlotAssign.Remove(c);
        }
        foreach (var c in field) AssignFieldSlot(c);
    }

    /// <summary>이 카드가 배정받은(또는 지금 새로 배정되는) pos 슬롯 마커 그
    /// 자체 — 카드가 attach될 부모이자, 애니메이션이 매 프레임 실시간으로
    /// 추적할 살아있는 타겟이다. 마커가 씬에 없으면(방어적 폴백) fieldArea로
    /// 대체한다.</summary>
    RectTransform FieldSlotTransform(HwatuCard card)
    {
        var t = fieldPosSlots[AssignFieldSlot(card)];
        return t != null ? t : fieldArea;
    }

    /// <summary>위 슬롯의 현재 월드 좌표 — 슬램다운 고스트가 처음 나타나는
    /// 자리를 한 번 스냅샷으로 남겨둘 때만 쓴다(예: 조커 리빌 지점). 실제
    /// 이동 애니메이션은 이 값이 아니라 <see cref="FieldSlotTransform"/>이
    /// 돌려주는 살아있는 Transform을 매 프레임 추적한다 — "moveTo 포지션
    /// 대신 타겟의 transform 위치로 이동" 요청 참고.</summary>
    Vector3 FieldSlotWorldPos(HwatuCard card) => FieldSlotTransform(card).position;

    /// <summary>pos1~pos12는 이제 영구 고정 자식이라 지우지 않는다 — 그
    /// 밑에 실제로 붙어있는 카드(들)만 존별로 지운다. fieldArea 자체를
    /// ClearChildren하던 예전 방식은 마커까지 파괴해서 참조 캐싱이
    /// 불가능했던 원인이었다.</summary>
    void ClearFieldPosSlots()
    {
        for (int i = 1; i <= 12; i++)
            if (fieldPosSlots[i] != null) ClearFieldSlotChildrenKeepGhosts(fieldPosSlots[i]);
    }

    // 2026-09-02 — HwatuUI.ClearChildren을 그대로 안 쓰는 이유는 위 GhostMarker
    // 주석 참고. 아직 살아있어야 하는(자기 차례가 안 온) 고스트만 건너뛰고
    // 나머지(지난 턴에 그려진 실제 카드 등)는 그대로 지운다.
    static void ClearFieldSlotChildrenKeepGhosts(Transform t)
    {
        for (int i = t.childCount - 1; i >= 0; i--)
        {
            var child = t.GetChild(i);
            if (child.GetComponent<GhostMarker>() != null) continue;
            Destroy(child.gameObject);
        }
    }

    // 같은 pos 슬롯에 여러 장(뻑 무더기 등)이 쌓일 때 완전히 포개지지 않게
    // 살짝 밀어내는 간격 — 몇 장인지 한눈에 보이면서도 "한 자리에 쌓였다"는
    // 느낌은 유지된다.
    const float FIELD_STACK_OFFSET = 20f;

    /// <summary>필드 — 2026-09-02: "카드를 각 pos에 attach" 요청으로 카드를
    /// fieldArea가 아니라 배정받은 pos 슬롯 마커 그 자체의 자식으로 만든다
    /// (마커 자신의 위치가 곧 카드 자리다). pos 마커가 fieldArea 밑에
    /// 영구 고정돼 있으므로 카드 위치도 자동으로 고정된다. 같은 달 카드는
    /// AssignFieldSlot이 같은 슬롯을 돌려주므로(뻑 등) 그 안에서
    /// FIELD_STACK_OFFSET만큼씩 밀려 겹쳐 쌓인 모양으로 그려진다 — 다른
    /// 달은 각자 새 슬롯을 받아 12칸에 펼쳐진다. 카드 자체의 판정(월 매칭
    /// 등)은 전혀 안 건드렸다 — 이건 순수하게 "어디에 그릴지"만 담당한다.</summary>
    void DrawField()
    {
        SyncFieldSlotAssignments();

        foreach (var group in field.GroupBy(AssignFieldSlot))
        {
            var cardsInSlot = group.ToList();
            for (int i = 0; i < cardsInSlot.Count; i++)
            {
                var c = cardsInSlot[i];
                var offset = new Vector2(i * FIELD_STACK_OFFSET, -i * FIELD_STACK_OFFSET);
                var target = FieldSlotTransform(c);
                var go = HwatuUI.MakeCard(c, target, offset, FIELD_W, FIELD_H, null, false);
                // 2026-09-02 버그 수정 — "뒷패가 깔린 패 pos에 들어갈 때 sibling이
                // 앞쪽으로 가나?" 신고로 발견. 이 슬롯에 아직 살아있는 고스트
                // (GhostMarker — 자기 차례가 아직 안 온 뒷패 등, ClearFieldPosSlots가
                // 건너뛴 것)가 있는데, 방금 만든 "진짜" 카드는 새 마지막 자식으로
                // 붙는다 — sibling이 늦을수록 위에 그려지므로, 나중에 도착해서
                // 원래 맨 위에 있어야 할 고스트가 방금 다시 그려진(먼저 있던)
                // 카드에 가려지는 역전이 생겼다. 고스트보다 앞선 sibling index로
                // 끼워 넣어 고스트가 항상 맨 위를 지키게 한다.
                for (int k = 0; k < target.childCount; k++)
                {
                    if (target.GetChild(k).GetComponent<GhostMarker>() != null)
                    {
                        go.transform.SetSiblingIndex(k);
                        break;
                    }
                }
                if (flyFrom.TryGetValue(c, out var from))
                {
                    // 2026-09-02 버그 수정 — 손패/뒷패 고스트가 이미 이 정확한
                    // 자리로 SlamDown(임팩트 플래시+펀치 스케일 포함)을 끝내고
                    // 나서 여기 flyFrom에 "자기가 도착한 그 자리"를 그대로
                    // 등록해 둔 경우(매칭 없이 필드에 남는 카드, 뻑으로 쌓이는
                    // 카드 등)엔 from이 이 카드의 최종 위치와 완전히 같다 —
                    // 그런데도 무조건 SlamIn을 또 돌리면 임팩트 플래시·펀치
                    // 스케일(1→1.28→1, top-center 피벗 기준이라 카드 아랫변이
                    // 아래로 부푼다)이 제자리에서 한 번 더 재생된다. 이게
                    // "필드에 카드가 깜빡인다"/"카드가 잠깐 아래로 쏠려
                    // 보인다"는 두 신고의 실제 정체 — 같은 자리에서 임팩트
                    // 연출이 중복 재생된 것이었다. 실제로 위치가 다른
                    // 경우(진짜 이동)만 애니메이션을 돌리도록 거리 체크를
                    // 추가했다.
                    var finalPos = (go.transform as RectTransform).position;
                    if ((finalPos - from).sqrMagnitude > 1f)
                        StartCoroutine(SlamIn(go.transform as RectTransform, from));
                    else
                        GoStopFX.SetArtShadow(go, true); // 사실상 제자리 — 애니메이션 없이 바로 "놓임" 표시
                }
                else
                {
                    GoStopFX.SetArtShadow(go, true); // 이번 리빌드에서 안 움직이는 정적 카드
                }
            }
        }
    }

    /// <summary>2026-08-27(목업 정확히 이식) — 목업 씬(GoStopOrientalMockup)의
    /// Seat_Left_Cap/Seat_Bottom_Cap 등을 직접 열어보니, 카드 위치를 코드로
    /// 계산하는 대신 진짜 Unity LayoutGroup을 쓰고 있었다: 가로
    /// (광 | 열끗+띠 | 피) 3열은 HorizontalLayoutGroup, 가운데 칸의 열끗(위)/
    /// 띠(아래) 분리는 VerticalLayoutGroup, 각 리프 존 안의 줄바꿈은
    /// GridLayoutGroup(고정 5열, 카드가 10px씩 겹치도록 음수 spacing)이
    /// 전담한다 — 내 획득패든 상대 획득패든 좌석 구분 없이 이 구조 하나를
    /// 공유한다(Seat_Bottom_Cap도 정확히 같은 계층이었다).
    /// <br/>
    /// 컨테이너당 한 번만 만들면 되는 "그릇"이라(카드 하나하나가 아니라
    /// 광/끗/띠/피 GameObject 자체는 매턴 바뀌지 않는다) 이미 만들어져
    /// 있으면(자식 "광"이 있으면) 그대로 재사용한다.</summary>
    readonly struct CapZones
    {
        public readonly RectTransform gwang, yeol, ddi, pi;
        public CapZones(RectTransform g, RectTransform y, RectTransform d, RectTransform p) { gwang = g; yeol = y; ddi = d; pi = p; }
    }

    CapZones EnsureCapLayoutHierarchy(RectTransform container)
    {
        var existingGwang = container.Find("광");
        var existingHlg = container.GetComponent<HorizontalLayoutGroup>();
        if (existingGwang != null && existingHlg != null)
        {
            return new CapZones((RectTransform)existingGwang,
                                 (RectTransform)container.Find("끗띠/끗"),
                                 (RectTransform)container.Find("끗띠/띠"),
                                 (RectTransform)container.Find("피"));
        }
        // existingGwang != null인데 existingHlg == null인 경우 — 이
        // 컨테이너가 GetOrCreateContainer/BuildEdgeSeatBlock의 "씬 참조
        // 재사용" 경로를 거치며 StripStrayLayoutGroup에 의해 HLG가
        // 지워진 것이다(그 함수는 "사용자가 실수로 붙인 LayoutGroup"을
        // 걷어내는 용도라 이 의도적인 것과 구분을 못 한다). 자식까지
        // 어중간하게 남아있으면 통째로 새로 짠다.
        if (existingGwang != null) HwatuUI.ClearChildren(container);

        // 2026-09-02 버그 수정 — existingHlg != null인데 existingGwang == null인
        // "반대" 경우도 있다: ClearBoardForDealing()이 "광"/"끗띠"/"피" 자식만
        // Destroy()하고(다음 새 판을 위해 캡 존을 비우는 용도) HLG 컴포넌트
        // 자체는 안 건드리는데, 그 Destroy가 실제로 반영되는 건 프레임 끝이라
        // DealingAnimationSeq()로 여러 프레임이 지난 뒤(다음 RebuildUI 시점)엔
        // "자식은 사라졌지만 HLG는 그대로 남은" 상태가 된다. 이 상태에서
        // 무조건 AddComponent<HorizontalLayoutGroup>()을 다시 호출하면 —
        // LayoutGroup 계열은 DisallowMultipleComponent라 Unity가 추가를 거부하고
        // null을 돌려줘서 바로 아래 hlg.spacing에서 NullReferenceException이
        // 난다. 이 예외가 NewGameSeq 코루틴 한복판(RebuildUI 호출 지점)에서
        // 터지면 코루틴 자체가 죽어 newGameStarting이 영원히 true로 남고
        // "판을 몇 번 진행하면 AI가 멈춘다"는 증상으로 나타난다 — 이미 있는
        // HLG를 재사용해서 막는다.
        var hlg = existingHlg != null ? existingHlg : container.gameObject.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 0f;
        hlg.childAlignment = TextAnchor.MiddleCenter;
        hlg.childControlWidth = true; hlg.childControlHeight = true;
        hlg.childForceExpandWidth = true; hlg.childForceExpandHeight = true;

        RectTransform MakeGrid(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var glg = go.AddComponent<GridLayoutGroup>();
            // 목업 실측값 그대로 — 카드(30×42) 5열 고정, 음수 spacing으로
            // 살짝 겹쳐 쌓인 느낌을 낸다(원래 CapStack이 overlap=14로 겹쳐
            // 쌓던 것과 같은 목적, GridLayoutGroup에선 spacing이 그 역할).
            glg.cellSize = new Vector2(CAP_W, CAP_H);
            glg.spacing = new Vector2(-27.5f, -20f);
            glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            glg.constraintCount = 5;
            glg.startCorner = GridLayoutGroup.Corner.LowerLeft;
            glg.childAlignment = TextAnchor.LowerCenter;
            glg.startAxis = GridLayoutGroup.Axis.Horizontal;
            return (RectTransform)go.transform;
        }

        var gwangZone = MakeGrid("광", container);

        var yeolDdiGo = new GameObject("끗띠", typeof(RectTransform));
        yeolDdiGo.transform.SetParent(container, false);
        var vlg = yeolDdiGo.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 0f;
        vlg.childAlignment = TextAnchor.MiddleCenter;
        vlg.childControlWidth = true; vlg.childControlHeight = true;
        vlg.childForceExpandWidth = true; vlg.childForceExpandHeight = true;
        var yeolZone = MakeGrid("끗", yeolDdiGo.transform);
        var ddiZone = MakeGrid("띠", yeolDdiGo.transform);

        var piZone = MakeGrid("피", container);

        return new CapZones(gwangZone, yeolZone, ddiZone, piZone);
    }

    /// <summary>리프 존 하나(광/끗/띠/피 중 하나)를 지우고 다시 채운다.
    /// 카드 위치는 GridLayoutGroup이 알아서 계산하므로 <see cref="HwatuUI.MakeCard"/>
    /// 에 넘기는 pos는 의미가 없다(즉시 덮어써짐). 애니메이션(flyFrom)이
    /// 걸린 카드는 여기서 바로 코루틴을 시작하지 않고 <paramref name="pending"/>
    /// 에 모아둔다 — GridLayoutGroup의 최종 위치는 이 프레임의 레이아웃
    /// 패스가 돌아야 확정되는데, 그 전에 rt.position(월드 좌표)을 읽으면
    /// 옛 위치가 잡힌다. 호출자가 4개 존을 다 채운 뒤
    /// LayoutRebuilder.ForceRebuildLayoutImmediate로 강제로 확정시키고 나서야
    /// pending을 순회하며 애니메이션을 시작해야 목적지가 정확하다.</summary>
    /// <paramref name="weighted"/>는 피 존 전용 — GridLayoutGroup은 "장수"만
    /// 세고 피 값(쌍피=2)을 모르므로 그냥 두면 줄마다 무조건 5장이 된다.
    /// 보이지 않는 필러 칸으로 그리드가 쌍피를 "2칸짜리"로 착각하게
    /// 만든다. 필러는 그 줄의 오른쪽 끝에 모아서 둔다(카드들 사이에 안
    /// 끼어 있게) — 먼저 "이번 줄에 들어갈 카드들"을 weight 합이 5가
    /// 될 때까지 모았다가 한 줄 분량이 차면(`FlushRow`) 그 줄의 실제
    /// 카드 전부를 먼저 그리고 그 다음에 그 줄이 필요로 하는 필러
    /// 개수만큼 이어서 그린다.
    /// <br/>2026-09-03(사용자 확인) — <b>줄이 어중간하게 안 차는 걸
    /// 최대한 피한다.</b> 예: 홑피 4장+쌍피 1장을 순서대로 넣으면(홑피가
    /// 먼저 4장 쌓여 weight=4) 쌍피(weight2)가 그대로는 4+2=6으로
    /// 넘친다 — 이때 그냥 새 줄을 시작하면 "홑피만 4장인 줄"과 "쌍피
    /// 혼자인 줄"로 갈라져 점수 계산 시 헷갈린다는 지적을 받았다. 이
    /// 게임의 피 값은 1(홑피)·2(쌍피)뿐이라 <b>이 오버플로는 수학적으로
    /// 항상 "줄이 정확히 4, 새 카드가 쌍피(2)"인 경우뿐</b>이다(0~3에
    /// 1이나 2를 더하면 항상 5 이하라 절대 안 넘친다) — 그래서 그 줄의
    /// 마지막 카드가 홑피(weight1)면 그 한 장만 빼서 쌍피를 대신
    /// 넣으면 정확히 5가 된다. 뺀 홑피는 다음 줄 맨 앞으로 넘긴다
    /// (순서 보존 — "먼저 가져온 순으로" 원칙을 최대한 지키면서 딱
    /// 한 장만 밀려난다). 줄 마지막이 쌍피라 뺄 수 없으면(빼도 2가
    /// 남아 정확히 안 맞음 — 애초에 홀수 weight를 짝수 카드로는 못
    /// 채운다) 줄을 있는 그대로(4/5) 닫는다 — 수학적으로 더 나은
    /// 방법이 없는 경우다.
    /// <br/>필러는 카드에 붙어 따라다닐 필요가 없다 — 이 함수가 매
    /// RebuildUI마다 ClearChildren으로 존을 통째로 비우고 cards 목록
    /// 그대로 다시 채우므로, 카드가 뻑·피뺏기 등으로 다른 곳에 가면
    /// 다음 프레임엔 애초에 이 목록에 안 들어있어 필러도 자동으로 같이
    /// 사라진다.
    void FillCapZone(RectTransform zone, List<HwatuCard> cards, List<(RectTransform rt, HwatuCard card, Vector3 from, Vector3? hit, Vector2 fromSize)> pending, bool weighted = false)
    {
        HwatuUI.ClearChildren(zone);

        void MakeOne(HwatuCard c)
        {
            var go = HwatuUI.MakeCard(c, zone, Vector2.zero, CAP_W, CAP_H, null, false);
            if (flyFrom.TryGetValue(c, out var from))
            {
                Vector3? hit = flyViaField.TryGetValue(c, out var hitPoint) ? hitPoint : (Vector3?)null;
                // 2026-09-04 — 등록이 없으면(대부분의 경우) 필드/더미에서
                // 온 것으로 본다 — flyFromSize 문서 참고.
                Vector2 fromSize = flyFromSize.TryGetValue(c, out var sz) ? sz : new Vector2(FIELD_W, FIELD_H);
                pending.Add(((RectTransform)go.transform, c, from, hit, fromSize));
            }
            else
            {
                GoStopFX.SetArtShadow(go, true); // 이번 리빌드에서 안 움직이는 정적 카드
            }
        }

        if (!weighted)
        {
            foreach (var c in cards) MakeOne(c);
            return;
        }

        var rowCards = new List<HwatuCard>();
        int rowWeight = 0;

        void FlushRow()
        {
            if (rowCards.Count == 0) return;
            foreach (var c in rowCards) MakeOne(c);
            int fillers = rowCards.Count(c => c.EffectivePiValue == 2);
            for (int i = 0; i < fillers; i++)
                new GameObject("PiWeightFiller", typeof(RectTransform)).transform.SetParent(zone, false);
            rowCards.Clear();
            rowWeight = 0;
        }

        foreach (var c in cards)
        {
            int w = c.EffectivePiValue == 2 ? 2 : 1;

            if (rowWeight + w <= 5)
            {
                rowCards.Add(c);
                rowWeight += w;
            }
            else // 수학적으로 rowWeight==4 && w==2인 경우뿐
            {
                var last = rowCards[rowCards.Count - 1];
                if (last.EffectivePiValue != 2)
                {
                    rowCards.RemoveAt(rowCards.Count - 1); // 마지막 홑피를 빼고
                    rowCards.Add(c);                        // 쌍피를 넣어 정확히 5
                    rowWeight = 5;
                    FlushRow();
                    rowCards.Add(last);                     // 뺀 홑피로 다음 줄 시작
                    rowWeight = 1;
                }
                else
                {
                    FlushRow();          // 스왑 불가 — 4로 못 채운 채 닫기
                    rowCards.Add(c);
                    rowWeight = w;
                }
                continue;
            }

            if (rowWeight == 5) FlushRow();
        }
        FlushRow();
    }

    void FlushPendingCapAnimations(RectTransform container, List<(RectTransform rt, HwatuCard card, Vector3 from, Vector3? hit, Vector2 fromSize)> pending)
    {
        if (pending.Count == 0) return;
        LayoutRebuilder.ForceRebuildLayoutImmediate(container);
        var capSize = new Vector2(CAP_W, CAP_H);
        foreach (var (rt, card, from, hit, fromSize) in pending)
        {
            if (fromSize == capSize)
            {
                // 2026-09-04 — 다른 획득패에서 온 카드(피뺏기)는 원래도
                // CAP_W/H 그대로라 크기 튠이 필요 없다 — 기존 방식 그대로.
                if (hit.HasValue) StartCoroutine(SlamInViaField(rt, from, hit.Value));
                else StartCoroutine(SlamIn(rt, from));
            }
            else
            {
                // 필드/손패/뒷면 등 다른 크기에서 온 카드 — SlamToCap 참고.
                StartCoroutine(SlamToCap(rt, card, from, fromSize, hit));
            }
        }
    }

    /// <summary>2026-09-04 — "필드에있는게 뿅사라지고 캡에 들어갈 사이즈로
    /// 뿅변하는게 이상해" 신고로 추가. <paramref name="rt"/>(획득패 자리에
    /// 이미 만들어진 "진짜" 카드)는 <see cref="EnsureCapLayoutHierarchy"/>의
    /// GridLayoutGroup 자식이라 sizeDelta를 직접 튠해도 다음 레이아웃
    /// 패스마다 cellSize로 강제로 되돌아간다(그리드가 자식 크기·위치를
    /// 전부 통제) — 그래서 진짜 오브젝트는 끝까지 숨겨 두고(CanvasGroup
    /// alpha 0), 레이아웃 그룹 밖(ui.ContentArea)에 별도 고스트를 하나
    /// 띄워 위치+크기를 동시에 자유롭게 보간한 뒤, 도착하면 고스트를
    /// 지우고 진짜 카드를 그 순간 드러낸다 — 이미 정확히 같은 자리·같은
    /// 크기라 이어붙는 지점에서 티가 안 난다.
    /// <br/>레이아웃 리빌드(<see cref="LayoutRebuilder.ForceRebuildLayoutImmediate"/>)
    /// 는 이 코루틴이 시작되기 전에 이미 끝나 있으므로(FlushPendingCapAnimations
    /// 참고) <c>rt.position</c>은 이 시점에 이미 최종 확정값이다 — 매 프레임
    /// 다시 읽을 필요 없이 한 번만 스냅샷해서 쓴다(그리드 자식이 날아다니는
    /// 동안 다시 움직일 리 없다).</summary>
    IEnumerator SlamToCap(RectTransform rt, HwatuCard card, Vector3 from, Vector2 fromSize, Vector3? hit)
    {
        if (rt == null) yield break;
        var cg = rt.gameObject.GetComponent<CanvasGroup>();
        if (cg == null) cg = rt.gameObject.AddComponent<CanvasGroup>();
        cg.alpha = 0f;
        cg.blocksRaycasts = false;
        cg.interactable = false;

        Vector3 to = rt.position;
        Vector2 toSize = new Vector2(CAP_W, CAP_H);

        var stableParent = ui != null ? ui.ContentArea : null;
        if (stableParent == null) { cg.alpha = 1f; cg.blocksRaycasts = true; cg.interactable = true; yield break; }

        var ghostGo = HwatuUI.MakeCard(card, stableParent, Vector2.zero, fromSize.x, fromSize.y, null, false);
        var ghost = ghostGo.transform as RectTransform;
        ghost.position = from;

        if (hit.HasValue)
        {
            // 2단 경유(필드에서 짝을 실제로 친 자리를 거쳐 감) — 1구간(필드
            // 안에서의 이동)은 출발·경유지 둘 다 필드 크기라 사이즈가 안
            // 바뀐다. 2구간에서만 획득패 크기로 줄어든다.
            float t1 = CaptureFlightDistanceT(Vector3.Distance(from, hit.Value));
            yield return FlyAndPunchGhost(ghost, from, hit.Value, fromSize, fromSize,
                Mathf.Lerp(0.09f, 0.30f, t1), Mathf.Lerp(0.10f, 0.16f, t1));
            if (ghost == null) { if (rt != null) { cg.alpha = 1f; cg.blocksRaycasts = true; cg.interactable = true; } yield break; }

            float t2 = CaptureFlightDistanceT(Vector3.Distance(hit.Value, to));
            yield return FlyAndPunchGhost(ghost, hit.Value, to, fromSize, toSize,
                Mathf.Lerp(0.14f, 0.34f, t2), Mathf.Lerp(0.16f, 0.22f, t2));
        }
        else
        {
            float t01 = CaptureFlightDistanceT(Vector3.Distance(from, to));
            yield return FlyAndPunchGhost(ghost, from, to, fromSize, toSize,
                Mathf.Lerp(0.11f, 0.38f, t01), Mathf.Lerp(0.14f, 0.22f, t01));
        }

        if (rt != null)
        {
            cg.alpha = 1f;
            cg.blocksRaycasts = true;
            cg.interactable = true;
            GoStopFX.SetArtShadow(rt.gameObject, true);
        }
        if (ghost != null) Destroy(ghost.gameObject);
    }

    /// <summary><see cref="FlyAndPunch"/>와 같은 위치 이동+임팩트+펀치
    /// 스케일이지만, 그 사이 <c>sizeDelta</c>도 <paramref name="fromSize"/>→
    /// <paramref name="toSize"/>로 같이 보간한다 — 레이아웃 그룹의 통제를
    /// 안 받는 고스트 전용이라 sizeDelta를 직접 건드려도 안전하다. 펀치
    /// 배율(1.28)은 <see cref="FlyAndPunch"/>와 통일해서 같은 타격감을 낸다.</summary>
    IEnumerator FlyAndPunchGhost(RectTransform ghost, Vector3 from, Vector3 to, Vector2 fromSize, Vector2 toSize, float flyDur, float punchDur)
    {
        Vector3 baseScale = ghost.localScale;

        float t = 0f;
        while (t < flyDur)
        {
            t += Time.deltaTime;
            float p = 1f - Mathf.Pow(1f - Mathf.Clamp01(t / flyDur), 3f); // ease-out
            if (ghost == null) yield break;
            ghost.position = Vector3.Lerp(from, to, p);
            ghost.sizeDelta = Vector2.Lerp(fromSize, toSize, p);
            yield return null;
        }
        if (ghost == null) yield break;
        ghost.position = to;
        ghost.sizeDelta = toSize;
        SpawnImpactFlash(ghost);

        t = 0f;
        while (t < punchDur)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / punchDur);
            float s = p < 0.4f ? Mathf.Lerp(1f, 1.28f, p / 0.4f) : Mathf.Lerp(1.28f, 1f, (p - 0.4f) / 0.6f);
            if (ghost == null) yield break;
            ghost.localScale = baseScale * s;
            yield return null;
        }
        if (ghost != null) ghost.localScale = baseScale;
    }

    void DrawPlayerCaptured()
    {
        var cap = captured[PLAYER_SEAT];
        var gwang = cap.Where(c => c.EffectiveKind == HwatuKind.Gwang).OrderBy(c => c.month).ToList();
        var yeol = cap.Where(c => c.EffectiveKind == HwatuKind.Yeolkkeut).OrderBy(c => c.month).ToList();
        var ddi = cap.Where(c => c.EffectiveKind == HwatuKind.Ddi).OrderBy(c => c.month).ToList();
        var pi = cap.Where(c => c.EffectiveKind == HwatuKind.Pi).OrderBy(c => c.month).ToList();

        var zones = EnsureCapLayoutHierarchy(playerCapArea);
        var pending = new List<(RectTransform, HwatuCard, Vector3, Vector3?, Vector2)>();
        FillCapZone(zones.gwang, gwang, pending);
        FillCapZone(zones.yeol, yeol, pending);
        FillCapZone(zones.ddi, ddi, pending);
        FillCapZone(zones.pi, pi, pending, weighted: true);
        FlushPendingCapAnimations(playerCapArea, pending);
    }

    /// <summary>상대(슬롯 1/2/3) 획득패 — 내 획득패와 완전히 같은 구조
    /// (<see cref="EnsureCapLayoutHierarchy"/>)를 그대로 재사용한다.</summary>
    void DrawAiCaptured(int slot, int seat)
    {
        var cap = captured[seat];
        var gwang = cap.Where(c => c.EffectiveKind == HwatuKind.Gwang).OrderBy(c => c.month).ToList();
        var yeol  = cap.Where(c => c.EffectiveKind == HwatuKind.Yeolkkeut).OrderBy(c => c.month).ToList();
        var ddi   = cap.Where(c => c.EffectiveKind == HwatuKind.Ddi).OrderBy(c => c.month).ToList();
        var pi    = cap.Where(c => c.EffectiveKind == HwatuKind.Pi).OrderBy(c => c.month).ToList();

        var container = capAreaAI[slot];
        var zones = EnsureCapLayoutHierarchy(container);
        var pending = new List<(RectTransform, HwatuCard, Vector3, Vector3?, Vector2)>();
        FillCapZone(zones.gwang, gwang, pending);
        FillCapZone(zones.yeol, yeol, pending);
        FillCapZone(zones.ddi, ddi, pending);
        FillCapZone(zones.pi, pi, pending, weighted: true);
        FlushPendingCapAnimations(container, pending);
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
            // 2026-08-27(목업 참고, 사용자 확인) — 목업(PlaceHand)은 강조할
            // 카드를 위로 34px 띄워서 표시한다. 기존 골드 링 강조는 그대로
            // 두고(둘 다 있는 게 더 눈에 띈다는 판단), 여기에 "낼 수 있는
            // 패는 위로 뜬다"를 더한다 — 링(highlightOffset)은 이 pos를
            // 그대로 이어받아 계산되므로(HwatuUI.MakeCard 참고) 카드와 함께
            // 자동으로 따라 올라간다.
            float y = playable ? 34f : 0f;
            // 2026-09-01: 하이라이트가 CardFront 프리팹 내부의 Highlight
            // 자식(스트레치 앵커)으로 바뀌면서 카드 크기에 자동으로 맞춰져,
            // 손패 전용 크기를 따로 안 넘겨도 된다.
            // 2026-09-02(사용자 확인) — 손패를 handArea 바닥에 붙이고 싶어서
            // anchor/pivot을 (0.5,0)(bottom)으로 바꿨다. 카드 자체가 아래쪽
            // 기준으로 자라 올라가므로, playable일 때 y=34로 올리는 이 "위로
            // 뜬다" 연출은 그대로 유지된다(카드 바닥이 handArea 바닥에서
            // 34px 뜬다는 뜻으로 자연스럽게 재해석된다).
            var go = HwatuUI.MakeCard(card, handArea, new Vector2(x, y), HAND_W, HAND_H,
                () => OnPlayerPlay(card), playable, pivotBottom: true);

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
        // 2026-09-02(사용자 확인) — 손패 카드들이 이제 bottom-pivot으로 바닥에
        // 붙는다(MakeCard pivotBottom, DrawPlayerHand 참고). 이 슬롯은
        // MakeCard를 안 쓰는 별도 오브젝트라 여기서도 똑같이 맞춰야
        // 나머지 손패와 세로로 어긋나지 않는다 — 자식(라벨 등)의 상대
        // 좌표는 go 자신의 로컬 좌표계라 이 변경과 무관하게 그대로 둔다.
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0f);
        rt.pivot = new Vector2(0.5f, 0f);
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
        numLabel.font = HwatuTheme.FontBold;

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
    /// (매칭 없음)에 쓰는 1단 연출. 목적지는 rt 자기 자신(이미 최종 부모
    /// 밑에 배치돼 있다 — pos 슬롯 마커 또는 Cap zone)을 타겟으로 매 프레임
    /// 실시간으로 추적한다("moveTo 포지션이 아니라 타겟의 transform 위치로
    /// 이동" 요청 참고 — 예전엔 rt.position을 코루틴 시작 시점에 한 번
    /// 스냅샷 떠서 고정 Vector3로 이동했다).</summary>
    // 2026-09-02(사용자 확인) — "패를 내고 들어올 때 어느 유저가 냈고
    // 어느 유저가 가져가는지 잘 안 보인다, 순간적으로 뿅 없어지는
    // 느낌"이라는 신고 — SlamIn이 필드↔획득패 사이의 캡처 비행에도
    // 쓰이는데, 그 거리(실측 600~1400px, 화면을 거의 가로지른다)에
    // 고정 0.11초는 너무 짧아 눈이 못 따라간다. 반면 필드 내부의 아주
    // 짧은 보정용 SlamIn(거의 안 걸림, 걸려도 몇십 px)은 지금 속도가
    // 이미 자연스럽다는 확인을 받았으므로 그대로 둔다 — 거리에 따라
    // 지속시간을 자동으로 늘리면 두 상황을 하나의 함수로 같이 만족시킬
    // 수 있다(짧으면 스냅, 길면 눈으로 좇을 수 있게).
    static float CaptureFlightDistanceT(float dist) => Mathf.Clamp01(dist / 500f);

    IEnumerator SlamIn(RectTransform rt, Vector3 fromWorld)
    {
        if (rt == null) yield break;
        float t01 = CaptureFlightDistanceT(Vector3.Distance(fromWorld, rt.position));
        float flyDur = Mathf.Lerp(0.11f, 0.38f, t01);
        float punchDur = Mathf.Lerp(0.14f, 0.22f, t01);
        yield return FlyAndPunch(rt, fromWorld, rt, flyDur, punchDur);
    }

    /// <summary>필드의 짝을 실제로 쳐서 맞추는 2단 연출 — 손/더미에서 <b>맞은
    /// 필드패 자리까지</b> 먼저 날아가 딱 맞고 튕긴 다음(1구간), 거기서 다시
    /// 최종 획득패 자리까지 날아간다(2구간). "cap으로 즉시 들어오는 느낌이라
    /// 어색하다"는 신고로 도입 — 카드가 어디서 왔는지 눈으로 따라갈 수 있다.
    /// <paramref name="hitWorld"/>(1구간 경유지)는 캡처 직전 사라질 필드패의
    /// 스냅샷 지점이라 살아있는 Transform이 없다 — 여기만 예외적으로 Vector3
    /// 그대로 쓴다. 최종 목적지(2구간)는 SlamIn과 같은 이유로 rt 자기
    /// 자신을 타겟으로 매 프레임 추적한다.</summary>
    IEnumerator SlamInViaField(RectTransform rt, Vector3 fromWorld, Vector3 hitWorld)
    {
        if (rt == null) yield break;

        // 2026-09-02 — SlamIn과 같은 이유로 각 구간을 거리에 맞춰 늘린다.
        float t1 = CaptureFlightDistanceT(Vector3.Distance(fromWorld, hitWorld));
        yield return FlyAndPunch(rt, fromWorld, hitWorld, Mathf.Lerp(0.09f, 0.30f, t1), Mathf.Lerp(0.10f, 0.16f, t1));
        if (rt == null) yield break;

        float t2 = CaptureFlightDistanceT(Vector3.Distance(hitWorld, rt.position));
        yield return FlyAndPunch(rt, hitWorld, rt, Mathf.Lerp(0.14f, 0.34f, t2), Mathf.Lerp(0.16f, 0.22f, t2));
    }

    /// <summary>이동(감속) + 도착 시 임팩트 플래시 + 펀치 스케일 — 목적지가
    /// 정적 스냅샷(Vector3)인 경우 전용. 캡처 직전 사라지는 필드패 자리처럼
    /// 살아있는 Transform이 없는 경유지에서만 쓴다. 살아있는 목적지가 있으면
    /// 아래 RectTransform 오버로드를 쓸 것.</summary>
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
        if (rt != null)
        {
            rt.localScale = baseScale;
            GoStopFX.SetArtShadow(rt.gameObject, true); // 착지 애니메이션 완료 — 이제 "놓임" 그림자 표시
        }
    }

    /// <summary>이동(감속) + 도착 시 임팩트 플래시 + 펀치 스케일 — 목적지가
    /// 살아있는 Transform인 경우("moveTo 포지션 대신 타겟의 transform
    /// 위치로 이동" 요청). 매 프레임 <paramref name="target"/>.position을
    /// 다시 읽으므로 애니메이션이 도는 동안 그 값이 바뀌어도(이 프로젝트의
    /// pos1~12는 실제로는 고정이라 안 바뀌지만) 항상 최신 위치에 정확히
    /// 도착한다.</summary>
    IEnumerator FlyAndPunch(RectTransform rt, Vector3 from, RectTransform target, float flyDur, float punchDur)
    {
        Vector3 baseScale = rt.localScale;

        float t = 0f;
        while (t < flyDur)
        {
            t += Time.deltaTime;
            float p = 1f - Mathf.Pow(1f - Mathf.Clamp01(t / flyDur), 3f); // ease-out
            if (rt == null || target == null) yield break;
            rt.position = Vector3.Lerp(from, target.position, p);
            yield return null;
        }
        if (rt == null || target == null) yield break;
        rt.position = target.position;
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
        if (rt != null)
        {
            rt.localScale = baseScale;
            GoStopFX.SetArtShadow(rt.gameObject, true); // 착지 애니메이션 완료 — 이제 "놓임" 그림자 표시
        }
    }

    // ── 2026-08-23: 카드 애니메이션 시퀀스 재설계 ──────────────
    // "손패 선택 → 필드에 슬램(매칭 위치/빈 슬롯) → 뒷패도 슬램 → 그제야
    // Cap 이동 → 피 뺏기"라는 사용자 지정 순서를 구현하기 위한 헬퍼들.
    // 실제 카드(필드/Cap)는 여전히 DrawField/DrawPlayerCaptured 등 기존
    // RebuildUI 파이프라인이 field/captured 데이터를 그대로 읽어 그린다 —
    // 여기 추가된 건 그 전에 잠깐 보여주는 "고스트"(임시 GameObject)뿐이라
    // 캡처·점수·피 뺏기 등 실제 판정 로직은 전혀 안 건드린다.


    /// <summary>슬램다운 고스트 카드 — pos 슬롯 마커처럼 "앞으로도 살아있을"
    /// 대상에 붙일 때 쓴다. 도착 타겟의 <b>자식으로 직접 attach</b>한다
    /// (부모 자신이 타겟이므로 자동으로 그 위치가 된다. DrawField가 실제
    /// 카드를 붙이는 방식과 완전히 동일해서 고스트→실카드 인계가 어긋날
    /// 수 없다). 실제 클릭은 안 받는 순수 연출용이라 onClick은 항상 null.
    /// <br/>
    /// 2026-09-02: 이 슬롯에 이미 카드가 있으면(AssignFieldSlot의 같은 달
    /// 그룹핑으로 합류하는 경우 — 단, "이 카드가 무엇과 매칭됐는지"까지는
    /// 몰라도 된다, target.childCount로 충분) DrawField가 나중에 그릴
    /// FIELD_STACK_OFFSET 간격을 미리 반영한다 — 안 그러면 고스트가 항상
    /// (0,0)에 등장해 기존 카드와 완전히 겹친 채 도착한 뒤에야 갑자기
    /// 옆으로 밀려나는 것처럼 보인다("해당 pos에 이미 카드가 있다면
    /// FIELD_STACK_OFFSET 적용된 포지션이면 딱 맞을듯" 요청).</summary>
    // 2026-09-02 버그 수정 — "손패는 안 깜빡이는데 뒷패만 깜빡인다"는 신고로
    // 발견. 뒷패 고스트는 "② 뒷패 슬램다운" 단계에서 pos 슬롯의 자식으로
    // 착지한 뒤에도, 자기 차례(GoStopRules.Resolve(drawn, field) 호출 +
    // 그 결과를 그리는 RebuildUI)가 오기 전에 "④ 손패 결과 배치" 단계의
    // RebuildUI가 먼저 한 번 낀다 — 그 RebuildUI 맨 앞의 ClearFieldPosSlots()
    // 가 pos 슬롯 자식을 통째로(뒷패 고스트까지 포함해서) 지워버려서, 아직
    // 자기 차례도 안 왔는데 뒷패가 조기에 사라졌다가 한참 뒤(다음 몇 번의
    // WaitForSeconds/선택 팝업을 지나) 실제 카드로 다시 나타났다 — 손패
    // 고스트는 자기 것을 지우는 DestroyGhosts 호출이 항상 "④"의 바로 그
    // RebuildUI 직전이라 이 문제가 안 생겼다(이미 없어진 뒤라 지울 게
    // 없다). GhostMarker로 표시해서 ClearFieldPosSlots가 "아직 살아있어야
    // 하는 고스트"는 건너뛰게 한다.
    sealed class GhostMarker : MonoBehaviour { }

    GameObject SpawnGhostCard(HwatuCard card, RectTransform target)
    {
        int existing = target.childCount;
        var offset = new Vector2(existing * FIELD_STACK_OFFSET, -existing * FIELD_STACK_OFFSET);
        var go = HwatuUI.MakeCard(card, target, offset, FIELD_W, FIELD_H, null, false);
        go.AddComponent<GhostMarker>();
        return go;
    }

    /// <summary>슬램다운 고스트 카드 — 2026-09-02: 매칭된 필드 카드처럼 곧
    /// 파괴될(RebuildUI가 지울) 대상의 "이미 렌더링된 정확한 자리"에 놓을
    /// 때 쓴다. 그 자리는 실측 스냅샷(Vector3)일 수밖에 없다 — 살아있는
    /// Transform에 붙이면 그 오브젝트가 파괴되는 순간 고스트까지 같이
    /// 사라진다. ContentArea(안 지워지는 안정된 부모)에 만들고 그 좌표로
    /// 바로 놓는다.</summary>
    GameObject SpawnGhostCard(HwatuCard card, Vector3 worldLandingPos)
    {
        var go = HwatuUI.MakeCard(card, ui.ContentArea, Vector2.zero, FIELD_W, FIELD_H, null, false);
        (go.transform as RectTransform).position = worldLandingPos;
        return go;
    }

    // 2026-09-02 버그 수정 — "매칭 안 되는 패가 빈 슬롯에 놓일 때 깜빡인다"는
    // 재신고로 발견. Destroy()는 실제 제거가 그 프레임 끝까지 미뤄지는데,
    // 이 함수를 부른 직후(같은 프레임 안, yield 없이) RebuildUI()가 곧장
    // "진짜" 카드를 같은 pos 슬롯의 자식으로 새로 만드는 호출부가 여러 곳
    // 있다 — 그러면 그 한 프레임 동안 죽어가는 고스트와 새 카드가 같은
    // 자리에 동시에 존재해서 겹쳐 그려진다(매칭된 카드는 필드를 아예
    // 떠나 Cap으로 이동하므로 이 문제가 없다 — 그 자리에 아무것도 새로
    // 안 생기니까. 매칭 안 된 카드만 "그 자리에 새 카드가 또 생기는"
    // 경우라 여기서만 증상이 났다). DestroyImmediate로 그 자리에서 바로
    // 제거해 겹치는 프레임 자체를 없앤다.
    static void DestroyGhost(GameObject go) { if (go != null) DestroyImmediate(go); }
    static void DestroyGhosts(List<GameObject> list)
    {
        if (list == null) return;
        foreach (var g in list) DestroyGhost(g);
    }

    /// <summary>2026-09-04(사용자 확인 — "쪼는 맛, 호쾌한 맛, 힘없이 실망한
    /// 느낌이 들게, 패를 낼 때마다/뒷패를 깔 때마다 유저 심리가 들어가면
    /// 좋겠다") — 착지 결과에 따라 <see cref="SlamDown"/>의 완급을 정해준다.
    /// 폭탄("쎄게")·뻑 형성("힘없이")은 이미 각자 전용 프리셋을 직접
    /// 부르고 있어 이 함수를 안 거친다 — 나머지 모든 착지(①손패 슬램·
    /// ②뒷패 슬램·덱만 넘기는 턴)가 이걸로 통일된다. 못 먹으면(그냥
    /// 필드에 놓임) 낮고 느리고 거의 안 튕기는 김빠진 낙하, 먹으면 높고
    /// 빠르고 크게 튕기는 호쾌한 낙하, 이미 3장 쌓인 자리를 마저 먹으면
    /// (뻑 먹기 등 4장을 한 번에 쓸어가는 순간) 그보다 한 단계 더 세게.</summary>
    (float dropHeight, float dropDur, float punchDur, float punchScale) LandingMood(bool willCapture, bool bigCapture)
    {
        if (bigCapture) return (220f, 0.07f, 0.14f, 1.38f);   // 호쾌 — 왕창 쓸어감
        if (willCapture) return (190f, 0.085f, 0.13f, 1.30f); // 호쾌 — 평범한 캡처
        return (95f, 0.13f, 0.09f, 1.08f);                     // 힘없이 — 아무것도 못 먹고 그냥 놓임
    }

    /// <summary>"공중에서 내려치는" 슬램 모션 — 목적지가 정적 스냅샷(Vector3)인
    /// 경우 전용(예: 매칭된 필드 카드의 이미 렌더링된 자리 — 그 오브젝트는
    /// 곧 파괴될 예정이라 살아있는 Transform을 못 쓴다). 착지 지점 위쪽에서
    /// 시작해 ease-in(가속)으로 빠르게 떨어뜨린 뒤 충격 플래시 + 펀치
    /// 스케일로 마무리한다 — "카드를 탁 내려놓는다"는 손맛을 노린 것.</summary>
    // 2026-09-02(사용자 확인) — "이펙트가 나오는 특수한 상황에서는 다들
    // 같은 속도감으로 흘러가서 긴장감이 안 산다, 쎄게 내려친다던지 뻑났을
    // 땐 힘없이 내려놓는다던지" 요청으로 punchScale을 노출한다 — 기본값
    // 1.22f는 기존 동작 그대로라 이 값을 안 넘기는 모든 호출부(일반
    // 매칭 등)는 전혀 안 바뀐다. 폭탄처럼 "쎄게"는 이 값을 키우고,
    // 뻑 형성처럼 "힘없이"는 1.0에 가깝게 낮춰서 부른다.
    IEnumerator SlamDown(RectTransform rt, Vector3 landing, float dropHeight = 170f, float dropDur = 0.10f, float punchDur = 0.12f, float punchScale = 1.22f)
    {
        if (rt == null) yield break;
        Vector3 baseScale = rt.localScale;
        Vector3 start = landing + new Vector3(0f, dropHeight, 0f);
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
            float s = p < 0.4f ? Mathf.Lerp(1f, punchScale, p / 0.4f) : Mathf.Lerp(punchScale, 1f, (p - 0.4f) / 0.6f);
            if (rt == null) yield break;
            rt.localScale = baseScale * s;
            yield return null;
        }
        if (rt != null) rt.localScale = baseScale;
    }

    /// <summary>2026-09-04(사용자 확인 — "뒷패가 까질 때 DrawPile 뭉치에서
    /// cardback 하나가 뒤집어져서 나오는 느낌이 약하다") — <paramref
    /// name="rt"/>(이미 앞면으로 렌더링돼 있는 고스트) 바로 위에 카드
    /// 뒷면 이미지를 겹쳐 놓고, 잠깐 그대로 보여준 뒤 세로 높이만 0으로
    /// 줄여 사라지게 한다("좌우로 접히니 뒷면 이미지랑 겹쳐 보인다,
    /// 상하로 접히게 해달라"는 2차 수정 — 세로로 접히듯 얇아지며 사라지면
    /// 그 밑에 이미 있던 앞면(rt)이 "뒤집혀서 드러난" 것처럼 보인다).
    /// rt 자신의 스프라이트를 바꿀 필요가 없어서(그러면 이 뒤에 이어지는
    /// 착지 로직이 어떤 카드가 진짜인지 헷갈릴 위험이 있다) 덧대는 방식을
    /// 택했다. 뒷면은 rt와 같은 부모(pos 슬롯)의 자식이라, rt가 도중에
    /// 파괴되면(예: 판이 갑자기 끝나는 등) 뒷면도 계층 구조를 따라
    /// 자동으로 같이 사라진다. 호출 시점의 <c>rt.position</c>이 곧 이
    /// 연출이 벌어지는 자리다 — <see cref="SlamDown"/>이 이 함수를 부르기
    /// 전에 <c>rt.position</c>을 DrawPile 위로 옮겨두므로(2차 수정, 아래
    /// 참고) 결과적으로 더미 위에서 뒤집힌다.</summary>
    IEnumerator FlipRevealBack(RectTransform rt)
    {
        if (rt == null) yield break;
        var back = HwatuUI.MakeCardBack(rt.parent, rt.anchoredPosition, rt.sizeDelta.x, rt.sizeDelta.y);
        back.SetAsLastSibling(); // rt(앞면) 바로 위에 그려지도록 보장

        yield return new WaitForSeconds(0.08f); // "뒷면이 나왔다"를 눈에 담을 짧은 틈
        if (back == null) yield break;

        // 2026-09-04 3차 수정(사용자 확인 — "너무 빠르게 애니메이팅돼서
        // 그냥 패만 끊겨 보인다, 속도를 줄여달라") — 0.12초 선형 보간은
        // 프레임 몇 개 만에 끝나 접히는 과정 자체가 안 보이고 순간적으로
        // 컷된 것처럼 보였다. 0.24초로 늘리고, 시작·끝이 부드러운
        // smoothstep 곡선을 써서(선형이면 등속이라 여전히 뚝뚝 끊겨
        // 보인다) 실제로 접히는 움직임이 눈에 들어오게 했다.
        const float flipDur = 0.24f;
        Vector3 baseScale = back.localScale;
        float t = 0f;
        while (t < flipDur)
        {
            t += Time.deltaTime;
            float linear = Mathf.Clamp01(t / flipDur);
            float p = linear * linear * (3f - 2f * linear); // smoothstep
            if (back == null) yield break;
            // 2026-09-04 2차 수정(사용자 확인) — "좌우로 접히니 뒷면 이미지랑
            // 겹쳐 보인다, 상하로 접히게 해달라" — x축 대신 y축을 줄인다
            // (가로 폭은 그대로 두고 세로만 접히듯 얇아진다).
            back.localScale = new Vector3(baseScale.x, baseScale.y * (1f - p), baseScale.z);
            yield return null;
        }
        if (back != null) Destroy(back.gameObject);
    }

    /// <summary>"공중에서 내려치는" 슬램 모션 — 목적지가 살아있는 Transform인
    /// 경우 전용(pos 슬롯 마커처럼 앞으로도 유지되는 대상). <paramref
    /// name="target"/>의 position을 매 프레임 다시 읽는다("moveTo 포지션이
    /// 아니라 타겟의 transform 위치로 이동" 요청).
    /// <br/>
    /// <b>함정 — 카드 피벗(top-center 0.5,1)과 pos 마커 피벗(center
    /// 0.5,0.5)이 다르다.</b> target.position을 그대로 착지 지점으로 쓰면
    /// 카드가 반 장 높이만큼 아래로 처진다(실측으로 확인 — pos 슬롯은
    /// sizeDelta=(120,196)/pivot=(0.5,0.5)인데 카드는 pivot=(0.5,1)이라,
    /// "카드의 top-center를 마커의 center에 맞추는" 셈이 돼서 카드 전체가
    /// 절반만큼 아래로 밀린다). rt는 SpawnGhostCard가 이미 anchoredPosition
    /// 기준으로 target 밑에 정확히 배치해 뒀으므로, 생성 직후의 실제
    /// 오프셋(anchorOffset = rt.position − target.position)을 한 번 구해서
    /// 매 프레임 그 보정값을 다시 더한다 — target이 움직여도(GridLayoutGroup
    /// 등) 정확한 피벗 보정이 유지된다.
    /// <br/>
    /// <paramref name="suspensePulses"/>(2026-09-04, "뒷패를 깔때마다 쪼는
    /// 맛이 있으면" 요청) — 0보다 크면 뒷패 전용 시퀀스로 들어간다.
    /// 뒷패(덱카드)를 까는 모든 지점에서만 쓰고, 이미 결과를 아는 손패
    /// 슬램에는 안 쓴다.
    /// <br/>
    /// 2026-09-04 2차 수정(사용자 확인 — "매칭되는 패 있을 땐 필드 위로,
    /// 없으면 빈 곳에서 애니메이팅되기 때문에 뭐가 나오는지 뻔하게
    /// 노출된다") — 예전엔 낙하 시작점(hover 자리, target 바로 위)에서
    /// 곧장 <see cref="FlipRevealBack"/>을 걸었는데, hover 자리 자체가
    /// 이미 target(매칭 슬롯이면 그 카드 위, 빈 슬롯이면 빈 자리) 근처라
    /// 뒤집히기도 전에 위치만으로 결과가 샜다. 이제 <b>DrawPile 위로
    /// 옮겨서</b> 그 자리에서 뒤집은 뒤에야 hover 자리까지 날아간다 —
    /// 뒤집히기 전까지는 화면 어디를 봐도 "더미에서 막 나온 카드"로만
    /// 보이고, 그 이후(흔들림 펄스 → 낙하 → 충격 이펙트)는 예전과
    /// 동일하다.</summary>
    IEnumerator SlamDown(RectTransform rt, RectTransform target, float dropHeight = 170f, float dropDur = 0.10f, float punchDur = 0.12f, float punchScale = 1.22f, int cardMonth = 0, int suspensePulses = 0)
    {
        if (rt == null || target == null) yield break;
        Vector3 anchorOffset = rt.position - target.position;
        Vector3 baseScale = rt.localScale;
        Vector3 hoverPos = target.position + anchorOffset + new Vector3(0f, dropHeight, 0f); // 낙하 시작점(타겟 바로 위)

        if (suspensePulses > 0)
        {
            // 2026-09-04 2차 수정(사용자 확인 — "매칭되는 패가 있을 땐
            // 필드 위로, 없으면 빈 곳에서 애니메이팅되기 때문에 뭐가
            // 나오는지 뻔하게 노출된다") — hover 위치 자체가 이미 target
            // (매칭 슬롯이면 카드 위, 빈 슬롯이면 빈 자리) 근처라 뒤집기
            // 전부터 위치만 보고도 결과가 샜다. 이제 DrawPile 위에서
            // 뒷면을 먼저 보여주고 뒤집은 뒤에야 최종 자리(hover) 쪽으로
            // 날아간다 — 뒤집히기 전까지는 화면 어디를 봐도 "그냥 더미
            // 위에 뜬 카드"로만 보인다.
            rt.position = drawPileArea.position;
            yield return FlipRevealBack(rt);
            if (rt == null || target == null) yield break;

            Vector3 flyStart = rt.position;
            float flyT = 0f;
            const float flyDur = 0.16f;
            while (flyT < flyDur)
            {
                flyT += Time.deltaTime;
                float p = 1f - Mathf.Pow(1f - Mathf.Clamp01(flyT / flyDur), 3f); // ease-out
                if (rt == null || target == null) yield break;
                rt.position = Vector3.Lerp(flyStart, hoverPos, p);
                yield return null;
            }
            if (rt == null || target == null) yield break;
            rt.position = hoverPos;

            // 기존 흔들림 펄스(문제 삼지 않은 부분)는 그대로 유지.
            float susDur = suspensePulses * 0.11f;
            float st = 0f;
            while (st < susDur)
            {
                st += Time.deltaTime;
                float wobble = 1f + 0.06f * Mathf.Sin(st / 0.11f * Mathf.PI * 2f);
                if (rt == null || target == null) yield break;
                rt.localScale = baseScale * wobble;
                yield return null;
            }
            if (rt == null || target == null) yield break;
            rt.localScale = baseScale;
        }
        else
        {
            rt.position = hoverPos; // 손패 슬램 — 기존과 동일(더미 경유 없음)
        }

        float t = 0f;
        while (t < dropDur)
        {
            t += Time.deltaTime;
            float p = Mathf.Pow(Mathf.Clamp01(t / dropDur), 2f); // ease-in — 내려찍는 가속감
            if (rt == null || target == null) yield break;
            Vector3 landing = target.position + anchorOffset; // 매 프레임 살아있는 값을 다시 읽는다
            rt.position = Vector3.Lerp(landing + new Vector3(0f, dropHeight, 0f), landing, p);
            yield return null;
        }
        if (rt == null || target == null) yield break;
        rt.position = target.position + anchorOffset;
        SpawnImpactFlash(rt);
        // 2026-09-03 — "필드에 패 나올 때 그 달에 맞는 파티클(1월→소나무 등)"
        // 요청. 임팩트 플래시와 같은 순간(착지 직후)에 카드 월별 모티프
        // 버스트를 같이 띄운다 — cardMonth<=0(조커, 또는 호출부가 아직
        // 이 파라미터를 안 넘기는 경우)이면 SpawnCardMotifBurst 내부에서
        // 조용히 무시된다.
        if (cardMonth >= 1 && cardMonth <= 12) SpawnCardMotifBurst(rt, cardMonth);

        t = 0f;
        while (t < punchDur)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / punchDur);
            float s = p < 0.4f ? Mathf.Lerp(1f, punchScale, p / 0.4f) : Mathf.Lerp(punchScale, 1f, (p - 0.4f) / 0.6f);
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
    /// 머니칩으로 날아가는 연출. 광팔이·뻑 보너스처럼 <b>여러 좌석이 한
    /// 명에게 동시에 낼 때</b>는 호출자가 지불자마다 한 번씩 불러서 동전
    /// 여러 개가 동시에 날아가게 한다(1:N을 이 함수 하나로 표현하지 않고
    /// 호출 횟수로 표현 — 함수 자체는 항상 1:1).
    /// <br/>쉬는 좌석 등 화면에 슬롯이 없는 좌석(<see cref="SlotOf"/>가 -1)
    /// 이면 날아갈 시작/도착점이 없으므로 조용히 아무것도 안 한다.</summary>
    /// <paramref name="reason"/>는 채팅 로그용 사유(예: "첫뻑비") — 생략하면
    /// "OO이(가) OO에게 N원 지급"처럼 사유 없이 적는다. 시각 효과와 무관하게
    /// 돈이 실제로 움직인 사실 자체는 항상 기록해야 하므로, 슬롯이 없어
    /// 이펙트를 못 그리는 경우(아래 return들)에도 로그는 먼저 남긴다.</summary>
    void FlyMoneyFX(int fromSeat, int toSeat, int amount, string reason = null)
    {
        if (amount <= 0) return;
        string reasonPart = string.IsNullOrEmpty(reason) ? "" : reason + " ";
        AppendChatLine($"{SeatNameFor(fromSeat, -1)}이(가) {SeatNameFor(toSeat, -1)}에게 {reasonPart}{amount:N0}원 지급");
        int fromSlot = SlotOf(fromSeat), toSlot = SlotOf(toSeat);
        if (fromSlot < 0 || toSlot < 0) return;
        var fromLbl = moneyText[fromSlot]; var toLbl = moneyText[toSlot];
        if (fromLbl == null || toLbl == null) return;
        GoStopFX.FlyMoney(ui.ContentArea, fromLbl.transform.position, toLbl.transform.position, amount);
    }

    /// <summary>충격 지점에 흰 원이 확 퍼졌다 사라지는 짧은 플래시 + 작은
    /// 파티클 스파크 — SlamIn 착지 시점에 호출한다.</summary>
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
        // 버그로 나타났다). ContentArea(root)처럼 절대 안 지워지는 부모에
        // 붙이고 월드 좌표를 그 공간으로 변환해서 위치만 맞춘다.
        var stableParent = ui != null ? ui.ContentArea : null;
        if (stableParent != null)
        {
            Vector2 localPos = stableParent.InverseTransformPoint(at.position);
            GoStopIcons.SpawnBurst(stableParent, localPos, new Color(1f, 0.9f, 0.6f), count: 5);
        }
    }

    /// <summary>카드가 필드에 착지한 자리에 그 달의 모티프 파티클(1월→소나무
    /// 등, <see cref="GoStopMotifAtlas"/>)을 정확히 터뜨린다. <see cref="GoStopWindParticles.Burst"/>
    /// 의 문서화된 계약대로 Canvas 기준 좌표(ContentArea가 아니라 그
    /// 두 단계 위)로 변환해서 넘긴다 — ContentArea 기준으로 넘기면 HUD
    /// 높이만큼 어긋날 수 있다는 <see cref="ShowActionPopup"/>의 기존
    /// 경고와 같은 이유(이번엔 HUD가 꺼져 있어 지금 당장은 우연히 맞지만,
    /// 계약대로 정확히 맞춰 둔다).</summary>
    void SpawnCardMotifBurst(RectTransform at, int month)
    {
        var canvasRoot = ui != null ? ui.ContentArea.parent.parent as RectTransform : null;
        if (canvasRoot == null) return;
        Vector2 localPos = canvasRoot.InverseTransformPoint(at.position);
        GoStopWindParticles.Instance?.BurstCardMotif(localPos, month);
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
