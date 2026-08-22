using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 고스톱(4인 기본, 광팔이) — 정식 "고스톱"(2인은 "맞고", <see cref="GoStopGame"/>).
/// 좌석 0=플레이어(아래), 1=AI-A(좌측), 2=AI-B(상단), 3=AI-C(우측) — 나를
/// 기준으로 턴 순서(0→1→2→3→0)를 그대로 반시계 방향 자리에 대응시켰다.
///
/// 고스톱은 원래 3인 게임이라 4명이 앉으면 매판 한 명이 쉰다("광팔이" —
/// 검색으로 확인한 전통 규칙, <see cref="GoStopRules.DealNew4PWithSitOut"/>).
/// 쉬는 좌석은 매판 순서대로 로테이션되고(<see cref="sitOutRotation"/>), 3장
/// 프로브 손패로 광 장수만 확인해서 광이 있으면 나머지 3명에게서 돈을 걷은
/// 뒤 그 판은 완전히 빠진다 — 실제 플레이(캡처·고/스톱·정산)는 나머지 3명이
/// 3인판과 똑같은 규칙(<see cref="GoStopRules"/>)으로 진행한다.
///
/// 카드 매칭·뻑·쪽·싹쓸이·폭탄·9월 열끗 선택·필드 2장 선택 등 순수 규칙은
/// <see cref="GoStopRules"/>를 그대로 재사용한다(인원수와 무관하게 "카드 한 장
/// 대 필드"로 동작하는 로직이라 손댈 필요가 없었다). 이 파일이 새로 하는 일은
/// 좌석 로테이션·다자간 정산(독박/개인별 광박·피박)·광팔이·화면 배치뿐이다.
/// </summary>
public partial class GoStop3PGame : MonoBehaviour
{
    [SerializeField] GameUIManager ui;

    enum State { Turn, GoStopChoice, GameOver }
    State state;

    // 2026-08-19: 네트워크 대전(3~4명 접속 시 인원에 맞춰 시작) 도입으로
    // SEATS가 더 이상 고정 상수가 아니다 — 배열은 항상 최대 크기
    // (SEATS_MAX)로 만들어 두고(필드 초기화는 Awake 이전에 실행되므로
    // 나중에 SetSeatCount로 줄여도 배열 자체를 다시 할당할 방법이 없다),
    // 실제 턴 진행에 쓰는 좌석 수만 SEATS로 런타임에 조절한다. 기본값
    // 4는 기존 싱글플레이(광팔이 로테이션 있는 4인) 동작을 그대로
    // 유지한다 — SetSeatCount를 아무도 안 부르면 예전과 100% 동일하다.
    const int SEATS_MAX = 4;
    int SEATS = SEATS_MAX;

    /// <summary>네트워크 로비가 접속 인원에 맞춰 좌석 수를 정할 때 쓴다
    /// (3=진짜 3인, 4=광팔이 있는 4인). Start()보다 먼저(Awake 시점) 불려야
    /// 하며, 그 뒤에 부르면 이미 진행 중이던 판과 좌석 수가 어긋난다 —
    /// 지금은 씬에 막 들어온 시점에서만 쓰는 걸 전제로 한다. 3·4 외의
    /// 값은 무시한다(이 파일은 3~4인 전용, 2인은 GoStopGame이 따로 맡는다).</summary>
    public void SetSeatCount(int n)
    {
        if (n == 3 || n == 4) SEATS = n;
    }
    // 맞고(2인)는 7점부터지만 정식 고스톱(3~4인)은 3점부터 난다 — 사용자
    // 확인 규칙. GoStopRules.CAPTURE_LINE(2인용, 7)과 별개로 이 파일만의
    // 상수를 둔다.
    const int CAPTURE_LINE = 3;
    // 2026-08-19: 네트워크 대전용 — 게스트 기기는 실제로 1~3번 좌석을
    // 배정받으므로 더 이상 상수로 고정할 수 없다. 기본값 0은 기존
    // 싱글플레이·호스트(항상 좌석 0) 동작을 그대로 유지한다. SetSeatCount와
    // 같은 이유로 Awake()에서, Start()가 돌기 전에 확정돼야 한다.
    int PLAYER_SEAT = 0;

    /// <summary>네트워크 게스트로 이 씬에 들어왔을 때 내가 배정받은 좌석을
    /// 반영한다. Awake()에서만 부를 것 — 그 이후엔 손패 표시·입력 처리가
    /// 이미 좌석 0 기준으로 시작됐을 수 있다.</summary>
    public void SetMySeat(int seat)
    {
        if (seat >= 0 && seat < SEATS_MAX) PLAYER_SEAT = seat;
    }

    // ── 판돈 ─────────────────────────────────────────────
    const int STARTING_MONEY = 100_000;
    const int WON_PER_POINT = 100;
    // 광팔이 — 사용자 확인 규칙: 광이나 쌍피 계열(쌍피·9월 열끗·보너스 조커)
    // 한 장당 100원씩을, 2·3번째(선을 제외한, 나를 밀어낸 두 명)에게서
    // "각각" 받는다(2인이 각자 100원씩 내므로 카드 한 장당 실수령은 200원).
    // 선(딜러)은 이 정산에서 빠진다 — 딜러는 밀어낸 쪽이 아니다.
    const int GWANG_SALE_WON_PER_CARD = 100;
    // 2026-08-18: "다시 시작해도 이전 잔액으로" 요청 — PlayerPrefs에 좌석별로
    // 영구 저장한다(2인판과 같은 패턴). 0원 이하가 되면 예전엔 세션이 끝났지만
    // 지금은 REFILL_MONEY로 채우고 올인 횟수만 기록한 채 계속 진행한다.
    const int REFILL_MONEY = 50_000;
    static string MoneyKey(int s) => "GoStop4P_Money_" + s;
    static string AllInKey(int s) => "GoStop4P_AllIn_" + s;
    readonly int[] money = new int[SEATS_MAX];
    readonly int[] allInCount = new int[SEATS_MAX];
    int stakeMultiplier = 1; // 나가리마다 2배, 결판나면 1로 리셋 (Start()에서만 초기화)

    void SaveMoney()
    {
        for (int s = 0; s < SEATS; s++)
        {
            PlayerPrefs.SetInt(MoneyKey(s), money[s]);
            PlayerPrefs.SetInt(AllInKey(s), allInCount[s]);
        }
        PlayerPrefs.Save();
    }

    /// <summary>이번 판 정산 후 0원 이하가 된 좌석을 전부 REFILL_MONEY로 채우고
    /// 올인 횟수를 늘린다. 4인이라 이론상 여러 좌석이 동시에 0원 이하가 될 수
    /// 있어(광팔이·독박 등으로 몰아 냈을 때) 전 좌석을 독립적으로 확인한다.
    /// 실제로 리필된 좌석 목록을 돌려준다 — 호출부가 결과 문구에 반영할 수 있게.</summary>
    List<int> RefillIfBankrupt()
    {
        var refilled = new List<int>();
        for (int s = 0; s < SEATS; s++)
        {
            if (money[s] <= 0)
            {
                money[s] = REFILL_MONEY;
                allInCount[s]++;
                refilled.Add(s);
            }
        }
        return refilled;
    }

    // ── 판 상태 (좌석별 배열) ─────────────────────────────
    readonly List<HwatuCard>[] hand = new List<HwatuCard>[SEATS_MAX];
    readonly List<HwatuCard>[] captured = new List<HwatuCard>[SEATS_MAX];
    List<HwatuCard> field, drawPile;
    readonly int[] goCount = new int[SEATS_MAX];
    readonly int[] sweeps = new int[SEATS_MAX];
    readonly int[] heundeulCount = new int[SEATS_MAX];
    readonly int[] bombCredits = new int[SEATS_MAX];
    readonly int[] bombCount = new int[SEATS_MAX];
    readonly int[] ppeokStreak = new int[SEATS_MAX]; // 연속 뻑 — "연뻑"(2연속) 판돈 보너스 판정용, 뻑이 아니면 0으로 리셋
    // "쓰리뻑"(뻑을 3번 하면 그 자리에서 즉시 승리)은 구글링으로 확인한
    // 표준 규칙상 연속이 아니라 이번 판 통산 횟수다("연뻑이나 삼연뻑은
    // 드물어서 온라인에서는 연속 아니어도 통산 3회면 종료" — 그래서
    // 연속용 ppeokStreak과 분리된 별도 카운터가 필요하다. 리셋 없음(NewGame
    // 에서만 0).
    readonly int[] ppeokTotalCount = new int[SEATS_MAX];
    readonly bool[] calledGo = new bool[SEATS_MAX]; // 이번 판에 고를 부른 적 있는가 — 독박(고박) 판정용
    // 마지막으로 고/스톱을 결정한 시점의 "생 캡처 점수"(고 보너스 제외).
    // 이 값보다 실제로 더 올라가야만 고/스톱을 다시 묻는다 — 안 그러면
    // 아무것도 못 먹어 점수가 그대로인 턴에도 매번 팝업이 뜬다("패를 하나도
    // 못 먹어서 점수 변동이 없어도 계속 고/스톱 팝업이 뜬다"는 신고).
    // NewGame()에서 CAPTURE_LINE 아래인 -1로 초기화해서 첫 도달은 항상 걸리게 한다.
    readonly int[] lastGoScore = new int[SEATS_MAX];
    int pendingGoRawScore; // ShowGoStopPrompt에 물린 생점수 — OnPlayerGo가 lastGoScore 갱신에 쓴다
    readonly HashSet<int>[] shookMonths = Enumerable.Range(0, SEATS_MAX).Select(_ => new HashSet<int>()).ToArray();
    readonly Dictionary<int, int> ppeokCauser = new(); // 월 → 뻑을 만든 좌석
    // 뻑 무더기에 같이 묻힌 보너스피 — 그 뻑을 나중에 해소하는 사람이
    // ppeokCauser의 피 뺏기와 함께 이 카드도 가져간다(사용자 확인 규칙).
    readonly Dictionary<int, HwatuCard> ppeokBonusPi = new();
    bool isFirstPlayOfRound;
    int currentSeat;

    // 광팔이 — 매판 시작마다 4명이 화투 한 장씩 뽑아 가장 높은 패를 뽑은
    // 사람이 선이 된다(사용자 확인 규칙 — "화투장을 뒷면이 보이게 펼쳐서
    // 한장씩 뽑아서 높은패로 선을 정한다"). 선은 항상 참가한다. 그다음
    // 순서(2번째·3번째)가 차례로 "이번 판 참가할지" 선언하고, 그 시점에
    // 이미 3명이 채워졌으면 4번째는 참가하고 싶어도 못 끼며("타의로 못
    // 침") 광팔이로 보상받는다 — 2·3번째 중 누가 스스로 포기하면 자리가
    // 남아 4번째가 그냥 정상 참가한다(보상 없음).
    int dealerSeat;
    // 선(딜러)은 씬에 들어와서 딱 한 번만 화투 뽑기로 정한다 — 그 뒤로는
    // "직전 판 승자가 선"(사용자 확인 규칙). Start()에서만 false로 초기화
    // 해야 하고, NewGame()(다시 시작)에서는 절대 건드리면 안 된다 — 매판
    // 다시 뽑으면 이 규칙 자체가 무의미해진다.
    bool dealerDetermined;
    int sittingOutSeat;
    // 쉬는 좌석이 "참가하고 싶었는데 자리가 없어 밀려난"(광팔이 대상)
    // 경우인지, 아니면 스스로 포기했거나 방어적으로 밀려난(보상 없음)
    // 경우인지 — 배지 문구가 이걸 구분해서 보여줘야 한다. 예전엔 이유와
    // 무관하게 항상 "(광팔이)"라고 써서 "쉬면 무조건 광팔이인 것처럼
    // 보인다"는 신고를 받았다.
    bool sittingOutWasSqueezed;

    // 손패를 냈는데 다음 리빌드까지의 대기(PLAY_STEP_DELAY들) 동안 같은 턴에
    // 카드를 또 클릭해서 PlaySeq 코루틴 두 개가 동시에 도는 걸 막는 잠금.
    // 예전엔 이게 없어서 빠르게 연속 클릭하면 실제로는 한 턴인데 손패 여러
    // 장이 한꺼번에 빠져나가는 버그가 있었다("내 손패만 유독 빨리 준다"는
    // 신고로 발견).
    bool actionBusy;

    const string BestKey = "BestGoStop3P";

    // ── 레이아웃 상수 (가로뷰, 2026-08-18) ────────────────
    // "우리 게임 가로뷰로 하자"(4인 고스톱만) 요청으로 세로 전용으로
    // 튜닝돼 있던 위 이력의 모든 숫자를 버리고 가로 1920×1080 참조
    // 해상도(GoStop3PGame.Start()에서 이 씬의 CanvasScaler에만 적용)
    // 기준으로 새로 짰다. 가로는 세로보다 폭이 넓고 높이가 짧다 —
    // 좌/우 좌석을 이제 회전 없이(공간이 넉넉해서) 화면 가장자리
    // 세로 기둥에 그대로 배치할 수 있어 90도 회전 트릭 자체가
    // 필요 없어졌다(MakeRotatedContainer 계열 삭제, BuildSideSeatUI →
    // BuildEdgeSeatUI로 교체 — 상단·좌·우가 전부 같은 코드를 쓴다).
    // 이 환경(Editor 샌드박스)은 Screen.orientation 변경이 실제로
    // 반영되지 않아 실측 검증이 불가능했다 — 아래 숫자는 선언한 참조
    // 해상도 기준 계산값이고, 최종 확인은 실기기가 필요하다.
    const float FIELD_W = 140f, FIELD_H = 160f;
    const float HAND_W = 107f, HAND_H = 174f;   // 하단 내 손패(사용자 확인 값)
    const float CAP_W = 62f, CAP_H = 86f;       // 내 획득패(하단)
    const float CAP_PITCH = 34f;
    const float CAP_AI_W = 44f, CAP_AI_H = 59f; // 상대 획득패(상/좌/우 공통)
    const float CAP_AI_PITCH = 28f;
    // 2026-08-20: 손패 7장이 Back 컨테이너 폭을 넘치는 문제를 처음엔
    // BACK_W(34→18)를 줄여서 고쳤는데, "뒷패가 일그러진다"는 신고로
    // 원인을 찾아보니 카드 뒷면의 9-slice 테두리(HwatuShapes.RoundedRect,
    // 고정 6px)와 점무늬 필드(DotGridPattern, preserveAspect 없이 그냥
    // 늘어남)가 34px 기준으로 비례를 맞춰둔 거라 18px로 줄이면 테두리가
    // 폭의 67%를 먹어버리고 점무늬도 심하게 찌그러졌다. **카드 크기는
    // 원래대로 되돌리고, 폭이 모자랄 때만 겹쳐서(fan) 배치한다** — 필드의
    // 같은 달 카드를 부채처럼 겹쳐 쌓는 것과 같은 원리. 실제 겹침 간격은
    // GoStop3PGame.UI.cs의 RebuildUI가 Back 컨테이너의 실제 폭에서
    // 매번 계산한다(사용자가 씬에서 폭을 넓히면 자동으로 안 겹치게 벌어짐).
    const float BACK_W = 34f, BACK_H = 48f;
    const float PILE_W = 100f, PILE_H = 180f; // 사용자 확인 값 — 카드 이미지 좌우 여백을 감안해 축소
    const float CAP_ROW_PITCH = 100f; // 내 획득패 2줄 예산 — CAP_H(86)보다 살짝 커야 안 겹친다

    RectTransform fieldArea, drawPileArea, handArea, playerCapArea;
    RectTransform[] backArea = new RectTransform[SEATS_MAX];   // [0] 안 씀(플레이어는 실물 손패)
    RectTransform[] capAreaAI = new RectTransform[SEATS_MAX];  // [0] 안 씀(플레이어는 playerCapArea)
    // 2026-08-18: "정보슬롯을 쫌스럽게 쓰지 말고 닉네임 한줄·고점수 한줄·
    // 금액·상태 아이콘 한줄로 넓고 크게" 요청으로 한 줄짜리 statusText를
    // 4단으로 나눴다(이름 위주 statusText는 유지하고 이름으로 그대로 씀,
    // 고+점수·금액을 별도 텍스트로 분리, 아이콘 줄 Y는 badgeRowY에 명시적으로
    // 저장해서 실제 렌더 높이 기준으로 계산 — 예전엔 이 위치를 텍스트 rect에서
    // "대충 추정"해서 뒷패 영역과 겹쳤다).
    TextMeshProUGUI[] statusText = new TextMeshProUGUI[SEATS_MAX];   // 닉네임(+선 표시는 배지로 이동)
    TextMeshProUGUI[] goScoreText = new TextMeshProUGUI[SEATS_MAX];  // "N고 M점"
    TextMeshProUGUI[] moneyText = new TextMeshProUGUI[SEATS_MAX];    // 코인 아이콘 + 금액
    // 2026-08-20: "화살표 대신 상태창 자체를 노란색으로" 요청 — 이 배경
    // Image를 FillSlot에서 좌석 차례일 때 색을 바꾼다.
    Image[] statusBoxImg = new Image[SEATS_MAX];
    // 2026-08-19: 상태 아이콘 전용 컨테이너 — 정보 패널을 좌(닉네임/고점수/
    // 금액)/우(아이콘) 반분할로 재설계하며 추가했다. 이전엔 아이콘을
    // ui.ContentArea에 직접 그려서 매턴 안 지워지는 버그가 있었다(아래
    // BuildInfoBlock/FillSlot 주석 참고) — 전용 컨테이너를 매턴
    // ClearChildren하는 것으로 고쳤다.
    RectTransform[] badgeArea = new RectTransform[SEATS_MAX];

    // 팝업 7종 — 전부 Assets/Resources/Prefabs/GoStop/Popups/의 실제 .prefab
    // 에셋을 Instantiate해서 쓴다(2026-08-18 전환). ShakeConfirm/FieldChoice/
    // DualPi/ScoreDetail은 2인판과 완전히 같은 프리팹을 공유한다(레이아웃이
    // 규칙 차이 없이 동일해서 — 프리팹 하나 고치면 양쪽에 다 반영된다).
    // Declare/DealerDraw/GwangSale은 4인판 전용 프리팹.
    ModalTwoButtonPopup shakePopup;
    HwatuCard pendingShakeCard;

    CardChoicePopup fieldChoicePopup;
    HwatuCard pendingFieldChoice;

    ModalTwoButtonPopup dualPiPopup;
    bool? pendingDualPiChoice;

    // 점수 상세 팝업 — 게임오버 오버레이의 "점수 상세" 버튼에서 연다.
    // "왜 이 점수가 나왔는지" 신고를 받아 추가했다. 4인판은 패자가 여럿이라
    // 승자와 패자 각각의 광박/피박 여부가 갈릴 수 있다(사용자 확인 규칙 —
    // 패자 개인 기준 판정) — 그래서 패자별 목록을 따로 담는다.
    ScoreDetailPopup scoreDetailPopup;
    GoStopRules.MultiPayout pendingPayout;
    int pendingWinnerSeat;
    List<int> pendingLoserSeats;

    // 광팔이 참가 선언 팝업 (플레이어가 2번째·3번째 순번일 때만 뜬다)
    ModalTwoButtonPopup declarePopup;
    bool? pendingDeclareChoice;

    // 나가기 확인 팝업 — "누르면 바로 나가지 말고 확인/취소로 물어봐야 한다"
    // 요청. ShakeConfirmPopup과 같은 프리팹(범용 2버튼 모달)을 재사용한다.
    ModalTwoButtonPopup exitConfirmPopup;

    // 선 뽑기 — 매판 시작마다 4명이 화투 한 장씩 뽑아 가장 높은 패가 선이
    // 된다(사용자 확인 규칙). 플레이어 입력이 필요 없는 연출용 팝업이라
    // 별도의 pending 변수는 없다.
    DealerDrawPopupView dealerDrawPopup;

    // 광팔이 결과 — 어떤 패로 팔았는지·총액·누가 내는지를 화면에 보여준다.
    // 연출용 팝업이라(선 뽑기와 마찬가지로) 플레이어 입력은 없다.
    GwangSalePopupView gwangSalePopup;

    const float PLAY_STEP_DELAY = 0.35f;

    // 카드가 날아드는 연출 — 손/더미의 실제 위치에서 최종 자리까지 이동+펀치
    // 스케일. 2인판(GoStopGame v4)에서 검증된 SlamIn 방식을 좌석 배열에
    // 맞게 이식했다.
    readonly Dictionary<HwatuCard, Vector3> flyFrom = new();

    // 2026-08-20: "cap으로 즉시 들어오는 느낌이라 수정이 필요하다"는 신고로
    // 2인판의 via-field 2단 연출(SlamInViaField)을 여기도 이식했다 — v1
    // 시절엔 "화면이 붐벼서" 생략했었지만, 실제로 이게 정확히 그 신고의
    // 원인이었다(손/덱 → 최종 획득패 자리로 한 방에 날아가서 "필드에서
    // 짝을 맞춰 가져온다"는 손맛이 없었다). 매칭으로 캡처된 카드가 그
    // 필드패가 있던 자리를 거쳐 가도록, 그 좌표를 임시로 담아둔다.
    readonly Dictionary<HwatuCard, Vector3> flyViaField = new();

    Coroutine toastHideCo;

    // ── 시작 ─────────────────────────────────────────────
    // 2026-08-18: "우리 게임 가로뷰로 하자" — 8개 게임 전체가 아니라 이
    // 화면(4인 고스톱)만 가로다. PlayerSettings의 기본 방향은 세로로 그대로
    // 두고(다른 7개 게임·2인 맞고에 영향 없음), Screen.orientation을 이 씬에
    // 진입할 때만 명시적으로 가로로 강제한다 — AutoRotation이 아니라 특정
    // enum 값을 직접 대입하면 allowedAutorotateTo* 플래그와 무관하게
    // 즉시 그 방향으로 고정된다(Unity 문서 기준). 나갈 때 반드시 세로로
    // 되돌려야 한다 — 안 그러면 타이틀/다른 게임까지 가로로 남는다.
    /// <summary>Start()보다 먼저 실행돼야 SEATS가 BuildStaticUI/NewGame이
    /// 돌기 전에 확정된다. 네트워크 로비를 거쳐 이 씬에 들어온 경우에만
    /// 로비가 정해준 인원수를 따르고, 싱글플레이(로비 없이 타이틀에서
    /// 바로 진입)면 기존 그대로 4인이다 — SEATS 필드의 기본값 자체가
    /// 4이므로 이 블록이 아예 안 걸려도 동작은 그대로다.
    /// <br/>
    /// <b>아직 안 된 것</b> — "내가 몇 번 좌석인지"(PLAYER_SEAT)는 지금도
    /// 항상 0으로 고정이다. 네트워크 게스트(호스트가 아닌 쪽)는 실제로
    /// 1~3번 좌석을 배정받으므로, 그 기기에서 "내 손패가 항상 화면
    /// 하단에 보이고 하단이 곧 나"라는 지금 구조가 그대로는 안 맞는다 —
    /// 이건 턴 메시지 송수신(카드를 냈다는 의도를 호스트로 보내고,
    /// 호스트의 판정 결과를 받아 그리는 것)과 함께 다음 단계에서
    /// 손볼 몫이다. 지금 이 커밋은 "인원수에 맞춰 좌석 수 자체를
    /// 정하는" 부분까지만 다룬다.
    /// </summary>
    // ── 네트워크 대전 ────────────────────────────────────
    // 2026-08-19: 호스트 권위 모델(문서 "고스톱 네트워크 대전" 섹션 참고) —
    // 호스트만 진짜 GoStopRules 판정을 돌리고, 게스트는 매 RebuildUI마다
    // 오는 스냅샷을 그대로 받아 그리기만 한다. 그래서 이 클래스 자체의
    // 판정 로직(PlaySeq 등)은 손 안 대고, "누가 이 좌석의 카드를
    // 고르는가"만 세 갈래(로컬 팝업/원격 메시지 대기/AI)로 나눈다.
    bool isNetworkHost, isNetworkGuest;

    /// <summary>이 좌석이 "원격 사람"인지 — 호스트일 때만 의미가 있다.
    /// 네트워크로 시작된 판은 호스트 자신을 제외한 모든 좌석이 접속한
    /// 게스트다(AI와 섞이지 않는다 — 로비가 인원이 다 찰 때까지 시작을
    /// 안 받아준다). 게스트 쪽에서는 항상 false — 게스트는 자기 자신
    /// (PLAYER_SEAT) 말고는 어떤 좌석도 직접 판정하지 않는다.</summary>
    bool IsRemoteSeat(int seat) => isNetworkHost && seat != PLAYER_SEAT && seat >= 0 && seat < SEATS;

    /// <summary>호스트 쪽에서 특정 원격 좌석의 다음 메시지를 기다린다.
    /// <paramref name="accept"/>가 null이면 그 좌석에서 오는 아무 메시지나
    /// 받는다(예: 이번 턴엔 PlayCard 또는 BombSkip 둘 다 유효한 응답).
    /// 받는 즉시 구독을 해제하므로 이후 같은 좌석의 낡은/중복 메시지는
    /// 자동으로 무시된다.</summary>
    IEnumerator WaitForRemoteMessage(int seat, System.Func<GoStopNetMessage, bool> accept, System.Action<GoStopNetMessage> onReceived)
    {
        GoStopNetMessage received = null;
        void Handler(int fromSeat, GoStopNetMessage msg)
        {
            if (fromSeat == seat && (accept == null || accept(msg))) received = msg;
        }
        GoStopNetLobby.Instance.OnGameMessage += Handler;
        yield return new WaitUntil(() => received != null);
        GoStopNetLobby.Instance.OnGameMessage -= Handler;
        onReceived(received);
    }

    /// <summary>호스트·게스트 공용 진입점 — 로비가 넘겨준 턴 메시지를
    /// 여기서 받는다. 호스트 쪽 메시지(PlayCard 등)는 각자 필요한 순간의
    /// <see cref="WaitForRemoteMessage"/> 호출이 직접 구독해서 가져가므로
    /// 여기서는 아무것도 안 한다 — 이 핸들러는 <b>게스트가 호스트로부터
    /// 받는 StateSync/Event만</b> 처리한다.</summary>
    void OnNetGameMessage(int fromSeat, GoStopNetMessage msg)
    {
        if (!isNetworkGuest) return;
        switch (msg.type)
        {
            case GoStopNetMessage.Type.StateSync:
                var snap = JsonUtility.FromJson<GoStopStateSnapshot>(msg.text);
                if (snap != null) ApplyNetworkSnapshot(snap);
                break;
            case GoStopNetMessage.Type.Event:
                // 호스트가 겪은 이벤트(뻑/쪽/싹쓸이 등)를 게스트 화면에도
                // 그대로 재생한다 — Toast()가 이미 토스트+사운드+파티클/
                // 팝업까지 전부 담당하는 단일 진입점이라 그대로 재사용한다.
                Toast(msg.seat, msg.text);
                break;
            case GoStopNetMessage.Type.Bye:
                // 다른 좌석의 연결이 끊겨 호스트가 판을 강제 종료했다는 안내.
                if (state != State.GameOver)
                {
                    state = State.GameOver;
                    ui?.ShowOverlay(new Color(.8f, .35f, .3f), "연결 끊김", "-",
                        string.IsNullOrEmpty(msg.text) ? "이번 판이 종료됐습니다." : msg.text,
                        "타이틀", GoToTitle);
                }
                break;
        }
    }

    /// <summary>readonly 배열은 참조 자체를 새 배열로 바꿔치기할 수
    /// 없다(CS0191) — 원소만 하나씩 덮어쓴다. src가 null이거나(구버전
    /// 스냅샷 등) 길이가 안 맞으면 조용히 무시(기존 값 유지).</summary>
    static void CopyInto(int[] dst, int[] src)
    {
        if (src == null) return;
        int n = Mathf.Min(dst.Length, src.Length);
        for (int i = 0; i < n; i++) dst[i] = src[i];
    }
    static void CopyInto(bool[] dst, bool[] src)
    {
        if (src == null) return;
        int n = Mathf.Min(dst.Length, src.Length);
        for (int i = 0; i < n; i++) dst[i] = src[i];
    }

    /// <summary>게스트 전용 — 호스트가 보낸 스냅샷을 내 필드에 그대로
    /// 덮어쓰고 다시 그린다. 판정을 전혀 다시 계산하지 않는다(호스트가
    /// 이미 다 끝낸 결과를 받는 것뿐) — 그래서 desync가 구조적으로 생길
    /// 수 없다.</summary>
    void ApplyNetworkSnapshot(GoStopStateSnapshot snap)
    {
        SEATS = snap.seats;
        for (int s = 0; s < SEATS_MAX; s++)
        {
            hand[s] = GoStopStateSnapshot.Dec(snap.HandFor(s));
            captured[s] = GoStopStateSnapshot.Dec(snap.CapturedFor(s));
        }
        field = GoStopStateSnapshot.Dec(snap.field);
        drawPile = new List<HwatuCard>();
        for (int i = 0; i < snap.drawPileCount; i++) drawPile.Add(new HwatuCard(0, HwatuKind.Pi, "Joker_1", piValue: 1, isJoker: true));
        // ↑ 더미는 개수만 온다(위 GoStopStateSnapshot 문서 참고) — 실제로
        // 뒤집히기 전까지 어떤 카드인지 게스트 화면에서 볼 일이 없으므로
        // 장수만 맞으면 되는 자리표시자다. 절대 개별적으로 그리거나
        // 판정에 쓰면 안 된다.

        currentSeat = snap.currentSeat;
        sittingOutSeat = snap.sittingOutSeat;
        sittingOutWasSqueezed = snap.sittingOutWasSqueezed;
        dealerSeat = snap.dealerSeat;
        state = (State)snap.state;
        // money/goCount/sweeps/heundeulCount/bombCredits/calledGo는 전부
        // readonly 배열(참조 자체를 통째로 못 바꾼다) — 원소만 하나씩
        // 덮어쓴다.
        CopyInto(money, snap.money);
        CopyInto(goCount, snap.goCount);
        CopyInto(sweeps, snap.sweeps);
        CopyInto(heundeulCount, snap.heundeulCount);
        CopyInto(bombCredits, snap.bombCredits);
        CopyInto(calledGo, snap.calledGo);

        RecomputeSeatSlots();
        RebuildUI();

        // 이 좌석만 받는 타깃 신호(SendTargetedPrompt) — 필드 초이스/
        // 9월열끗/참가선언은 정규 스냅샷 필드만으로는 "지금 내가 결정해야
        // 한다"는 게 안 드러나서 이 3개는 호스트가 명시적으로 신호를
        // 얹어 보낸다. 각각 팝업을 띄우기만 하고, 실제 대답은 그 팝업의
        // 버튼(BuildFieldChoiceUI/BuildDualPiChoiceUI/BuildDeclareUI에서
        // isNetworkGuest 분기로 이미 네트워크 전송용으로 갈아둔 것)이 보낸다.
        if (snap.fieldChoiceCandidates != null && snap.fieldChoiceCandidates.Length > 0)
            ShowFieldChoicePopup(GoStopStateSnapshot.Dec(snap.fieldChoiceCandidates));
        if (snap.dualPiChoicePending)
            dualPiPopup.Show();
        if (snap.declarePending)
        {
            declarePopup.messageText.text = $"{snap.declareDealerName}이(가) 선입니다. 이번 판 참가하시겠습니까?";
            declarePopup.Show();
        }

        // 고/스톱은 위와 달리 정규 스냅샷의 state/currentSeat만으로 판단할
        // 수 있다 — 별도 타깃 신호가 필요 없다. 오버레이가 이미 떠 있는
        // 채로 스냅샷이 또 와도(RebuildUI가 잦다) 매번 다시 띄우면 안
        // 되므로 goStopOverlayShown로 한 번만 띄운다.
        if (state == State.GoStopChoice && currentSeat == PLAYER_SEAT && isNetworkGuest)
        {
            if (!goStopOverlayShown)
            {
                goStopOverlayShown = true;
                int rawScore = GoStopRules.CalcScore(captured[PLAYER_SEAT], sweeps[PLAYER_SEAT]).Total;
                int displayScore = rawScore + goCount[PLAYER_SEAT];
                ui?.ShowOverlay(new Color(.93f, .73f, .18f), $"{displayScore}점 달성!", displayScore.ToString(),
                    "고 하시겠습니까, 스톱 하시겠습니까?", "고", OnPlayerGo, "스톱", OnPlayerStop);
            }
        }
        else
        {
            // true→false로 넘어가는 순간에만 닫는다 — 매번 닫으면 애초에
            // 안 떠 있던 턴에도 계속 HideOverlay를 부르는 낭비고, 이
            // 가드가 없으면 host가 다음 판을 시작해도(gameOverActive가
            // false로 돌아와도) 옛 오버레이가 화면에 계속 남는다.
            if (goStopOverlayShown) ui?.HideOverlay();
            goStopOverlayShown = false;
        }

        // 게임오버 — 정규 스냅샷엔 안 실려 있고 BroadcastGameOverState가
        // 별도로 쏘는 필드다(gameOverActive). 오버레이가 이미 떠 있는데
        // 또 안 뜨게 goStopOverlayShown과 같은 방식으로 가드하고, false로
        // 돌아오는 순간(호스트가 새 판을 시작) 명시적으로 닫아준다.
        if (snap.gameOverActive)
        {
            if (!gameOverOverlayShown)
            {
                gameOverOverlayShown = true;
                ShowGuestGameOverOverlay(snap);
            }
        }
        else
        {
            if (gameOverOverlayShown) ui?.HideOverlay();
            gameOverOverlayShown = false;
        }
    }

    bool goStopOverlayShown;

    /// <summary>호스트 전용 — 매 RebuildUI 끝에서 부른다(로컬 화면이
    /// 갱신되는 시점과 정확히 같은 타이밍에 게스트들도 갱신되게). LAN
    /// 안에서만 오가는 작은 JSON이라 턴마다 여러 번 불려도 대역폭 문제가
    /// 안 된다.</summary>
    GoStopStateSnapshot BuildSnapshot() => new GoStopStateSnapshot
    {
        seats = SEATS,
        currentSeat = currentSeat,
        sittingOutSeat = sittingOutSeat,
        sittingOutWasSqueezed = sittingOutWasSqueezed,
        dealerSeat = dealerSeat,
        state = (int)state,
        hand0 = GoStopDeck.EncodeAll(hand[0] ?? new List<HwatuCard>()),
        hand1 = GoStopDeck.EncodeAll(hand[1] ?? new List<HwatuCard>()),
        hand2 = GoStopDeck.EncodeAll(hand[2] ?? new List<HwatuCard>()),
        hand3 = GoStopDeck.EncodeAll(hand[3] ?? new List<HwatuCard>()),
        captured0 = GoStopDeck.EncodeAll(captured[0] ?? new List<HwatuCard>()),
        captured1 = GoStopDeck.EncodeAll(captured[1] ?? new List<HwatuCard>()),
        captured2 = GoStopDeck.EncodeAll(captured[2] ?? new List<HwatuCard>()),
        captured3 = GoStopDeck.EncodeAll(captured[3] ?? new List<HwatuCard>()),
        field = GoStopDeck.EncodeAll(field ?? new List<HwatuCard>()),
        drawPileCount = drawPile?.Count ?? 0,
        money = (int[])money.Clone(),
        goCount = (int[])goCount.Clone(),
        sweeps = (int[])sweeps.Clone(),
        heundeulCount = (int[])heundeulCount.Clone(),
        bombCredits = (int[])bombCredits.Clone(),
        calledGo = (bool[])calledGo.Clone(),
    };

    void BroadcastNetworkState()
    {
        var lobby = GoStopNetLobby.Instance;
        if (lobby == null) return;
        lobby.BroadcastToGuests(new GoStopNetMessage { type = GoStopNetMessage.Type.StateSync, text = JsonUtility.ToJson(BuildSnapshot()) });
    }

    /// <summary>호스트 전용 — EndGame은 RebuildUI를 거치지 않으므로(위
    /// BroadcastNetworkState 문서 참고) 게스트가 판이 끝난 걸 알 방법이
    /// 정규 경로엔 없다. EndGame의 두 탈출 지점(나가리 조기 리턴 + 정상
    /// 정산)에서 각각 이걸 명시적으로 불러 게스트에게 결과를 알린다.</summary>
    void BroadcastGameOverState(bool isNagari, int winnerSeat, int finalScore, int dokbakSeat, int[] refilledSeats)
    {
        var lobby = GoStopNetLobby.Instance;
        if (lobby == null) return;
        var snap = BuildSnapshot();
        snap.gameOverActive = true;
        snap.gameOverIsNagari = isNagari;
        snap.gameOverWinnerSeat = winnerSeat;
        snap.gameOverFinalScore = finalScore;
        snap.gameOverDokbakSeat = dokbakSeat;
        snap.gameOverStakeMultiplier = stakeMultiplier;
        snap.gameOverRefilledSeats = refilledSeats ?? System.Array.Empty<int>();
        lobby.BroadcastToGuests(new GoStopNetMessage { type = GoStopNetMessage.Type.StateSync, text = JsonUtility.ToJson(snap) });
    }

    bool gameOverOverlayShown;

    /// <summary>게스트 전용 — 호스트의 EndGame과 동등한 화면을 내 관점으로
    /// 다시 조립해서 보여준다. 판정은 호스트가 이미 다 끝냈고(정산·머니
    /// 이동은 money[] 스냅샷에 이미 반영돼 있다) 여기서는 표시만 한다.
    /// "다시 시작"은 host만 누를 수 있어 버튼 자체를 안 보여준다(호스트가
    /// 다시 시작하면 다음 StateSync가 자동으로 화면을 새 판으로 바꾼다).</summary>
    void ShowGuestGameOverOverlay(GoStopStateSnapshot snap)
    {
        if (snap.gameOverIsNagari)
        {
            ui?.ShowOverlay(new Color(.6f, .6f, .6f), "나가리", "-",
                $"아무도 {CAPTURE_LINE}점을 못 넘겼습니다 · 다음 판 판돈 {snap.gameOverStakeMultiplier}배 (호스트가 다시 시작합니다)",
                "타이틀", GoToTitle);
            return;
        }

        int winnerSeat = snap.gameOverWinnerSeat;
        string title = winnerSeat == PLAYER_SEAT ? "승리!" : $"{SeatName(winnerSeat)} 승리";
        Color col = winnerSeat == PLAYER_SEAT ? new Color(.93f, .73f, .18f) : new Color(.55f, .55f, .60f);
        string sub = snap.gameOverDokbakSeat >= 0
            ? $"{SeatName(snap.gameOverDokbakSeat)} 독박 · 내 머니 {money[PLAYER_SEAT]:N0}원"
            : $"내 머니 {money[PLAYER_SEAT]:N0}원";
        if (snap.gameOverRefilledSeats != null && snap.gameOverRefilledSeats.Length > 0)
        {
            string names = string.Join(", ", snap.gameOverRefilledSeats.Select(s => SeatName(s)));
            sub += $" · 잔액 소진 → 5만원 재충전: {names}";
        }
        ui?.SetScore(money[PLAYER_SEAT]);
        ui?.ShowOverlay(col, title, snap.gameOverFinalScore.ToString(), sub, "타이틀", GoToTitle);
    }

    /// <summary>필드 초이스/9월열끗/참가선언처럼 "지금 이 좌석 한 명만
    /// 결정해야 하는" 순간을 그 좌석에게만 알린다. 나머지 게스트는 이
    /// 메시지 자체를 안 받으므로(SendToSeat, Broadcast 아님) 남의 선택
    /// 팝업이 잘못 뜰 일이 없다. <paramref name="configure"/>가 스냅샷의
    /// 타깃 전용 필드(fieldChoiceCandidates 등)만 채워 넣는다 — 나머지는
    /// 평소와 같은 정규 스냅샷이라 게스트는 화면도 같이 최신 상태로 맞는다.</summary>
    void SendTargetedPrompt(int seat, System.Action<GoStopStateSnapshot> configure)
    {
        var lobby = GoStopNetLobby.Instance;
        if (lobby == null) return;
        var snap = BuildSnapshot();
        configure(snap);
        lobby.SendToSeat(seat, new GoStopNetMessage { type = GoStopNetMessage.Type.StateSync, text = JsonUtility.ToJson(snap) });
    }

    void Awake()
    {
        var lobby = GoStopNetLobby.Instance;
        if (lobby != null && lobby.PlayerCount > 0)
        {
            SetSeatCount(lobby.PlayerCount);
            SetMySeat(lobby.MySeat); // 호스트는 항상 0이라 사실상 no-op, 게스트는 1~3
            isNetworkHost = lobby.IsHost;
            isNetworkGuest = lobby.IsGuest;
            lobby.OnGameMessage += OnNetGameMessage;
            if (isNetworkHost) lobby.OnGuestLeftDuringGame += OnGuestLeftDuringGame;
            if (isNetworkGuest) lobby.OnDisconnected += OnHostDisconnected;
        }
    }

    /// <summary>호스트 전용 — 접속해 있던 게스트 한 명이 판 도중 나갔다.
    /// 남은 좌석들끼리 판을 계속할 방법이 없어서(그 좌석의 메시지를
    /// 영원히 기다리며 멈추는 게 최악이다 — "콜백은 반드시 한 번은
    /// 불려야 한다"는 이 프로젝트의 광고 콜백 원칙과 같은 이유) 판
    /// 자체를 즉시 끝내고 전원을 타이틀로 돌려보낸다. 재접속·좌석
    /// 대체는 v1 스코프 밖.</summary>
    void OnGuestLeftDuringGame(int seat)
    {
        if (state == State.GameOver) return; // 이미 끝난 판이면 신경 쓸 필요 없음
        if (seat < 0 || seat >= SEATS) return;
        string name = SeatName(seat);
        state = State.GameOver; // 더 이상 아무 턴도 진행되지 않게 막는다
        ui?.ShowOverlay(new Color(.8f, .35f, .3f), "연결 끊김", "-",
            $"{name}의 연결이 끊어져 이번 판을 종료합니다.", "타이틀", GoToTitle);
        GoStopNetLobby.Instance?.BroadcastToGuests(
            new GoStopNetMessage { type = GoStopNetMessage.Type.Bye, text = $"{name}의 연결이 끊어져 이번 판이 종료됐습니다." });
    }

    /// <summary>게스트 전용 — 호스트와의 TCP 연결 자체가 끊겼다(호스트가
    /// 종료했거나 네트워크가 끊긴 경우). 더 기다려도 다음 StateSync가
    /// 올 방법이 없으므로 바로 안내하고 타이틀로 돌려보낸다.</summary>
    void OnHostDisconnected(string reason)
    {
        if (state == State.GameOver) return;
        state = State.GameOver;
        ui?.ShowOverlay(new Color(.8f, .35f, .3f), "연결 끊김", "-",
            "호스트와의 연결이 끊어졌습니다.", "타이틀", GoToTitle);
    }

    void Start()
    {
        Screen.orientation = ScreenOrientation.LandscapeLeft;
        // GameUI는 씬마다 별도 인스턴스라(공유 싱글톤이 아니다) 이 인스턴스의
        // CanvasScaler만 가로용 참조 해상도로 바꿔도 다른 7개 게임·2인 맞고에는
        // 영향이 없다. 세로 참조(1080×1920)를 가로 물리 화면에 그대로 쓰면
        // matchWidthOrHeight 계산이 어긋나 스케일이 크게 틀어진다.
        var scaler = ui ? ui.GetComponent<CanvasScaler>() : null;
        if (scaler) scaler.referenceResolution = new Vector2(1920f, 1080f);
        // "상단 UI가 공간을 많이 차지한다, 나가기 버튼만 있으면 된다" 요청 —
        // 공용 HUD(제목·점수·NEW·뒤로 버튼 바)를 통째로 끄고 ContentArea가
        // 그 116px까지 전부 쓰도록 늘린다. 나가기는 BuildStaticUI에서 직접
        // 만드는 작은 버튼 하나로 대체한다.
        ui?.SetHudVisible(false);
        // 네트워크 게스트는 새 판을 직접 못 시작한다 — 언제 다시 시작할지는
        // 호스트만 결정한다(호스트가 다음 판을 시작하면 그 StateSync를
        // 받아 화면이 알아서 바뀐다).
        ui?.SetNewGameAction(isNetworkGuest ? (System.Action)null : NewGame);
        ui?.SetTitle(isNetworkHost || isNetworkGuest ? "고스톱 (네트워크)" : "고스톱 (4인)");
        ui?.SetBest(PlayerPrefs.GetInt(BestKey, 0));
        ui?.SetBackground(new Color(0.282f, 0.373f, 0.255f)); // 2인판과 같은 카드테이블 그린

        if (!isNetworkHost && !isNetworkGuest)
        {
            // 저장된 잔액이 있으면 이어서 쓰고, 없으면(첫 실행) 10만원으로
            // 시작한다. 네트워크 판은 이 로컬 저장을 안 쓴다 — 매판 접속하는
            // 실제 사람이 달라질 수 있어 "이 기기의 좌석 N 잔액"이라는
            // 개념 자체가 성립하지 않는다(잔액은 항상 호스트의 StateSync가
            // 정답이다).
            for (int s = 0; s < SEATS; s++)
            {
                money[s] = PlayerPrefs.GetInt(MoneyKey(s), STARTING_MONEY);
                allInCount[s] = PlayerPrefs.GetInt(AllInKey(s), 0);
            }
        }
        else
        {
            for (int s = 0; s < SEATS_MAX; s++) money[s] = STARTING_MONEY;
        }
        stakeMultiplier = 1;

        // 효과음 (절차적 생성 — 오디오 에셋 없음, BrickBreakerAudio와 같은 패턴)
        if (GoStopAudio.Instance == null)
            new GameObject("GoStopAudio").AddComponent<GoStopAudio>();

        BuildStaticUI();
        // 게스트는 여기서 아무것도 시작 안 한다 — 호스트가 첫 StateSync를
        // 보내오면 OnNetGameMessage → ApplyNetworkSnapshot이 손패를 채우고
        // 화면을 그린다. 호스트(네트워크 여부 무관)와 싱글플레이는 기존대로
        // 바로 새 판을 시작한다.
        if (!isNetworkGuest) NewGame();
    }

    /// <summary>타이틀로 나가기 전 화면 방향부터 세로로 되돌린다 — 이 씬만
    /// 가로로 강제했으므로, 그대로 씬을 넘기면 타이틀·다른 게임까지 가로로
    /// 남는다. 오버레이의 "타이틀" 버튼·자체 나가기 버튼 전부 이걸 쓴다.</summary>
    void GoToTitle()
    {
        Screen.orientation = ScreenOrientation.Portrait;
        // 네트워크 판이었으면 세션을 확실히 접는다 — 안 그러면 호스트는
        // 타이틀로 돌아간 뒤에도 계속 방을 열어둔 채 UDP 광고를 쏘고
        // 있고(다음에 이 기기로 다시 호스트/게스트 어느 쪽을 눌러도
        // 옛 세션과 뒤엉킨다), 게스트는 죽은 TCP 연결을 계속 붙들고 있게 된다.
        if (isNetworkHost || isNetworkGuest) GoStopNetLobby.Instance?.StopAll();
        ui?.GoBack();
    }

    /// <summary>안드로이드 뒤로가기 제스처 등 버튼을 안 거치고 씬이 파괴되는
    /// 경로에 대한 안전망 — OnDestroy는 다음 씬의 Start()보다 먼저 불린다
    /// (SceneManager.LoadScene 동기 호출 안에서 이전 씬 정리가 먼저 끝난다).</summary>
    void OnDestroy()
    {
        Screen.orientation = ScreenOrientation.Portrait;
        // 로비는 DontDestroyOnLoad라 이 오브젝트보다 오래 산다 — 구독을
        // 안 풀면 다음 판/씬에서도 이미 파괴된 이 인스턴스를 계속
        // 호출하려 들어 조용한 메모리 누수 + 예외 위험이 된다.
        if (GoStopNetLobby.Instance != null)
        {
            GoStopNetLobby.Instance.OnGameMessage -= OnNetGameMessage;
            GoStopNetLobby.Instance.OnGuestLeftDuringGame -= OnGuestLeftDuringGame;
            GoStopNetLobby.Instance.OnDisconnected -= OnHostDisconnected;
        }
    }

    IEnumerable<int> ActiveSeats() => Enumerable.Range(0, SEATS).Where(s => s != sittingOutSeat);

    // ── 화면 슬롯 ↔ 좌석 매핑 ─────────────────────────────
    // 슬롯: 0=하단(나) 1=좌측 2=상단(쉬는 사람 전용) 3=우측. 실제 턴 로테이션은
    // 좌석 번호(0~3, AdvanceTurn)로만 돌아가고 절대 안 바뀐다 — 이 배열은
    // "화면 어디에 그릴지"만 담당한다("실제 순서를 변경시키지 않고 UI만"
    // 요청). -1이면 그 슬롯에 표시할 좌석이 없다(아래 예외 상황).
    readonly int[] slotSeat = new int[4] { 0, 1, 2, 3 };

    /// <summary>쉬는 좌석을 상단 슬롯으로, 나머지를 실제 플레이 순서대로
    /// 좌/우에 배치한다. 하단(슬롯0)은 항상 나(PLAYER_SEAT) 고정 — 내가
    /// 쉬는 판이어도 그 자리에 "이번 판은 쉽니다" 메시지가 뜨는 기존 동작을
    /// 그대로 유지한다(RebuildUI). 흔한 경우(내가 활성)엔 쉬는 AI가 상단,
    /// 남은 AI 2명이 턴 순서대로 좌→우로 깔끔하게 떨어진다. 드문 경우
    /// (내가 쉬는 판)엔 활성 AI가 3명이라 상단도 필요한데, 상단은 항상
    /// "간단한 상태 텍스트만"(Cap·Back 없음, 아래 BuildStaticUI 참고)이라
    /// 구조 변경 없이 세 번째 활성 AI를 상단에 그냥 얹으면 된다 — 상단
    /// UI가 "쉬는 사람 전용"이 아니라 "Cap/Back이 필요 없는 세 번째 자리"
    /// 였을 뿐이라는 뜻.</summary>
    /// <summary>턴 순서(좌석 인덱스 0→1→2→3 증가)를 화면 슬롯(0=하단·1=좌·
    /// 2=상·3=우)에 매핑한다. 2026-08-20 정정(사용자 신고) — 화면상 방향이
    /// 시계 방향(하단→좌→상→우)이었는데, 실제 화투/고스톱 관례는 선부터
    /// 반시계 방향(하단→우→상→좌)이다. 12시=상단·3시=우측·6시=하단·9시=좌측
    /// 기준으로 시계 방향은 6→9→12→3(하단→좌→상→우)이므로, 예전 매핑이
    /// 정확히 그 방향이었다 — 반시계로 뒤집으려면 좌(1)/우(3) 슬롯의
    /// 배정만 맞바꾸면 된다(상단은 방향과 무관하게 그대로). **턴 진행
    /// 로직(AdvanceTurn 등)은 좌석 인덱스 증가 순서 그대로라 전혀 안
    /// 건드린다 — 어느 화면 위치에 그릴지만 바뀐다.</summary>
    void RecomputeSeatSlots()
    {
        slotSeat[0] = PLAYER_SEAT;

        if (SEATS == 3)
        {
            // 3인 모드는 광팔이 로테이션이 아예 없어 sittingOutSeat가 항상
            // -1로 고정된다 — 그대로 두면 바로 아래 "아직 안 정해짐"
            // placeholder 분기(4인 전용)를 타서 존재하지 않는 4번째
            // 좌석(3)을 화면에 그리려 들어 깨진다. 상(2)·좌(1)만 실제
            // 좌석을 쓰고 우(3)는 항상 비워둔다(RebuildUI는 seat<0인
            // 슬롯을 이미 빈 자리로 처리한다). 우측이 항상 빈 자리라
            // 반시계 방향(하단→우→상→좌)에서 우측을 건너뛰면 실제로는
            // 하단→상→좌 순서가 된다.
            slotSeat[2] = (PLAYER_SEAT + 1) % SEATS;
            slotSeat[1] = (PLAYER_SEAT + 2) % SEATS;
            slotSeat[3] = -1;
            return;
        }

        if (sittingOutSeat < 0)
        {
            // 아직 안 정해짐(참가 선언 진행 중) — 기본 배치로 임시 표시
            slotSeat[1] = 3; slotSeat[2] = 2; slotSeat[3] = 1;
            return;
        }

        var others = new List<int>();
        for (int i = 1; i <= 3; i++)
        {
            int s = (PLAYER_SEAT + i) % SEATS;
            if (s != sittingOutSeat && s != PLAYER_SEAT) others.Add(s);
        }

        slotSeat[2] = sittingOutSeat != PLAYER_SEAT ? sittingOutSeat : (others.Count > 2 ? others[2] : -1);
        slotSeat[3] = others.Count > 0 ? others[0] : -1;
        slotSeat[1] = others.Count > 1 ? others[1] : -1;
    }

    /// <summary>이 좌석이 지금 화면 어느 슬롯에 그려지는지(0=하단·1=좌·2=상·3=우).
    /// 못 찾으면 -1(있을 수 없는 상태지만 방어적으로).</summary>
    int SlotOf(int seat)
    {
        for (int i = 0; i < 4; i++) if (slotSeat[i] == seat) return i;
        return -1;
    }

    bool newGameStarting; // 재진입 방지 — 참가 선언 팝업이 뜬 채로 "다시 시작"이 중복 호출되는 걸 막는다

    /// <summary>버튼/오버레이 콜백은 void 메서드를 기대하므로 코루틴을 감싸는
    /// 얇은 래퍼만 둔다 — 실제 절차는 <see cref="NewGameSeq"/>. 참가 선언
    /// 팝업이 응답을 기다리는 동안 이 메서드가 다시 불리면(예: 빠르게 두 번
    /// 클릭) 새 코루틴이 겹쳐 돌면서 카드가 다시 섞이고 팝업 상태가 꼬일
    /// 수 있어 재진입을 막는다.</summary>
    public void NewGame()
    {
        if (newGameStarting) return;
        newGameStarting = true;
        StartCoroutine(NewGameSeq());
    }

    /// <summary>
    /// 4인 참가 선언 절차. 선(딜러)은 항상 참가하고, 2번째·3번째가 순서대로
    /// "이번 판 참가할지"를 선언한다(플레이어면 팝업, AI면
    /// <see cref="GoStopAI.WantsToPlay"/>로 즉시). 그 시점에 이미 3명이
    /// 채워졌으면 4번째는 참가하고 싶어도 못 끼며 광팔이로 보상받는다 —
    /// 2·3번째 중 누가 스스로 포기하면 자리가 남아 4번째가 그냥 정상
    /// 참가한다(보상 없음). 사용자가 직접 확정해준 규칙.
    /// </summary>
    IEnumerator NewGameSeq()
    {
        // 이전 판 종료 오버레이("다시 시작" 버튼이 있던 그 화면)를 여기서
        // 바로 지운다 — 선 뽑기 연출을 넣기 전엔 판을 다 준비한 뒤(총통
        // 체크 직전)에야 지웠는데, 이제 선 뽑기가 몇 초 걸리는 코루틴이라
        // 그 앞에 두지 않으면 선 뽑기·참가 선언 팝업이 뜨는 동안 예전
        // 오버레이가 그대로 화면을 덮고 있는다("다시 시작을 눌러도
        // 오버레이가 안 사라진다"는 신고 — 실은 몇 초 뒤엔 사라지지만
        // 그 사이 뒤에서 뜬 팝업이 오버레이에 가려 아무 반응이 없는
        // 것처럼 보였다).
        ui?.HideOverlay();

        // 선(딜러)은 씬에 들어와서 딱 한 번만 화투 뽑기 연출로 정한다.
        // 그 이후 판부터는 EndGame이 승자를 dealerSeat에 그대로 옮겨 적어둔
        // 값을 쓴다("직전 판 승자가 선" — 사용자 확인 규칙, 매판 다시 뽑던
        // 예전 동작을 대체했다). 나가리(무승부)면 EndGame이 dealerSeat를
        // 건드리지 않으므로 자동으로 "선 유지"가 된다.
        if (!dealerDetermined)
        {
            yield return StartCoroutine(DetermineDealerSeq());
            dealerDetermined = true;
        }

        // 2026-08-19: 네트워크 대전에서 접속 인원이 3명이면 진짜 3인
        // 고스톱(광팔이 로테이션 없음)으로 딜한다 — 4인 전용 DealNew4PFull
        // 대신 원래 있던 3인용 DealNew3P를 그대로 재사용한다.
        if (SEATS == 3)
        {
            var deal3 = GoStopRules.DealNew3P();
            hand[0] = deal3.hand0; hand[1] = deal3.hand1; hand[2] = deal3.hand2;
            field = deal3.field; drawPile = deal3.drawPile;
        }
        else
        {
            var deal = GoStopRules.DealNew4PFull();
            for (int s = 0; s < SEATS; s++) hand[s] = deal.hands[s];
            field = deal.field; drawPile = deal.drawPile;
        }
        for (int s = 0; s < SEATS; s++) SortHand(hand[s]);

        for (int s = 0; s < SEATS; s++)
        {
            captured[s] = new List<HwatuCard>();
            goCount[s] = sweeps[s] = heundeulCount[s] = 0;
            bombCredits[s] = bombCount[s] = ppeokStreak[s] = ppeokTotalCount[s] = 0;
            calledGo[s] = false;
            lastGoScore[s] = -1; // CAPTURE_LINE보다 항상 작게 — 첫 도달은 반드시 걸리게
            shookMonths[s].Clear();
        }
        ppeokCauser.Clear();
        ppeokBonusPi.Clear();
        flyFrom.Clear();
        flyViaField.Clear();
        isFirstPlayOfRound = true;
        actionBusy = false;
        state = State.Turn;

        // 참가 여부를 묻기 전에 손패부터 보여준다 — "내 패를 알아야 참가할지
        // 정할 텐데 팝업이 먼저 뜬다"는 신고. currentSeat/sittingOutSeat는
        // 아직 이번 판 값으로 정해지지 않았으므로(둘 다 이 시점엔 지난 판의
        // 값이 남아있다) -1(미정 센티널)로 비워서 RebuildUI가 엉뚱한 차례
        // 강조·쉬는 배지를 그리지 않게 한다. 카드가 눌려도 OnPlayerPlay가
        // `currentSeat != PLAYER_SEAT`로 걸러 무시하므로 안전하다.
        currentSeat = -1;
        sittingOutSeat = -1;
        RecomputeSeatSlots(); // 아직 안 정해졌으니 기본 배치(1=좌,2=상,3=우)로 임시 표시

        // 딜링 연출 — 손패/필드가 아직 화면에 하나도 안 그려진 이 시점에만
        // 걸 수 있다(RebuildUI가 한 번이라도 돌면 실제 카드가 바로 보여서
        // "나눠지는 중"이라는 느낌이 안 산다). 2026-08-20 정정(사용자 신고) —
        // 지난 판 필드/획득패가 화면에 그대로 남아있으면 새 카드가 그 위로
        // 날아드는 것처럼 보여 어색하다. 먼저 지우고, 더미 시각(레이어
        // 스택)만 채워서 카드가 실제로 그 더미에서 나오는 것처럼 보이게 한다.
        ClearBoardForDealing();
        UpdatePileVisual();
        yield return StartCoroutine(DealingAnimationSeq());

        RebuildUI();

        // 2026-08-19: 3인 모드는 광팔이 로테이션 자체가 없다 — 접속한
        // 3명이 전원 그대로 플레이한다. 4인 전용인 참가 선언·광판다
        // 정산 절차 전체를 건너뛴다.
        if (SEATS == 3)
        {
            sittingOutSeat = -1;
            sittingOutWasSqueezed = false;
            RecomputeSeatSlots();
        }
        else
        {
        var order = new int[SEATS];
        for (int i = 0; i < SEATS; i++) order[i] = (dealerSeat + i) % SEATS;

        var active = new List<int> { order[0] }; // 선 — 무조건 참가
        var declined = new List<int>();
        for (int i = 1; i <= 2; i++) // 2번째, 3번째
        {
            int candidate = order[i];
            bool wantsIn;
            if (candidate == PLAYER_SEAT)
            {
                pendingDeclareChoice = null;
                declarePopup.messageText.text = $"{SeatName(dealerSeat)}이(가) 선입니다. 이번 판 참가하시겠습니까?";
                declarePopup.Show();
                yield return new WaitUntil(() => pendingDeclareChoice != null);
                declarePopup.Hide();
                wantsIn = pendingDeclareChoice.Value;
            }
            else if (IsRemoteSeat(candidate))
            {
                // SeatName(dealerSeat)이 아니라 SeatNameFor(dealerSeat, candidate) —
                // 이 문구를 받는 건 호스트가 아니라 candidate 좌석이라, "나"
                // 판정은 candidate 기준이어야 한다(위 SeatNameFor 문서 참고).
                SendTargetedPrompt(candidate, s => { s.declarePending = true; s.declareDealerName = SeatNameFor(dealerSeat, candidate); });
                GoStopNetMessage declMsg = null;
                yield return StartCoroutine(WaitForRemoteMessage(candidate,
                    m => m.type == GoStopNetMessage.Type.DeclareChoice, m => declMsg = m));
                wantsIn = declMsg.boolValue;
            }
            else wantsIn = GoStopAI.WantsToPlay(hand[candidate]);

            if (wantsIn) active.Add(candidate); else declined.Add(candidate);
        }

        int fourth = order[3];
        bool fourthSqueezedOut = active.Count == 3;
        sittingOutWasSqueezed = fourthSqueezedOut;
        if (fourthSqueezedOut)
        {
            sittingOutSeat = fourth;
        }
        else
        {
            active.Add(fourth);
            // 방어적 백필 — 2·3번째가 둘 다 포기해도 활성 인원은 반드시
            // 3명을 채운다(GoStopRules 엔진 자체가 "3명이 다툰다"는 전제로
            // 검증돼 있다). 이 경우는 순리대로 채워진 것뿐이라 보상은 없다.
            foreach (var d in declined) { if (active.Count >= 3) break; active.Add(d); }
            sittingOutSeat = Enumerable.Range(0, SEATS).First(s => !active.Contains(s));
        }
        // "쉬는 유저를 상단 슬롯으로, 나머지는 실제 플레이 순서대로 좌/우에"
        // 요청 — 화면 위치(슬롯)와 좌석 번호를 분리한다. 실제 턴 로테이션
        // (0→1→2→3→0, 쉬는 좌석 건너뜀)은 전혀 안 건드리고 화면에 "누구를
        // 어디에 그릴지"만 매판 다시 계산한다.
        RecomputeSeatSlots();

        // 광팔이 — "참가하고 싶었는데 자리가 없어 밀려난"(fourthSqueezedOut)
        // 경우에만 보상한다. 스스로 포기했거나 방어적으로 밀려난 경우는
        // 대상이 아니다(사용자 확인 규칙 — "2,3번째가 포기하면 4번째는
        // 광 못 팔고 게임을 하게 된다"는 문장을 이렇게 읽었다).
        //
        // 정산 대상 카드 = 광 + "쌍피 계열"(실제 쌍피·9월 열끗·보너스 조커
        // 전부 포함 — 사용자 확인: "광, 쌍피(국열끗, 보너스포함)"). 9월
        // 열끗은 이번 판에서 실제로 안 쓰였으니 useAsPi 선택이 없다 —
        // dualPi 카드 자체를 쌍피 계열로 친다. 지불은 딜러를 제외한
        // 2·3번째(나를 밀어낸 두 명) **각자**가 장당 100원씩(사용자 확인
        // 규칙 — 기존의 "활성 좌석 전원이 나눠 낸다"는 방식에서 교체).
        bool CountsForGwangSale(HwatuCard c) => c.kind == HwatuKind.Gwang || c.piValue == 2 || c.dualPi || c.isJoker;
        var soldCards = hand[sittingOutSeat].Where(CountsForGwangSale).ToList();
        int sellableCount = soldCards.Count;

        // 쉬는 좌석의 손패는 사라지는 게 아니라 필드의 뒷패(더미)에 섞여
        // 들어간다 — 그냥 버리면 48장 체계가 깨져서 이후 판에서 실제로
        // 나올 수 있는 패의 총량·확률이 어긋난다. 광팔이로 이미 대가를
        // 정산한 패든(그 경우도 카드 자체는 여전히 존재한다) 스스로
        // 포기해서 쉬는 패든 이유와 무관하게 전부 더미로 되돌리고 섞는다.
        drawPile.AddRange(hand[sittingOutSeat]);
        GoStopDeck.Shuffle(drawPile);
        hand[sittingOutSeat] = new List<HwatuCard>();
        if (fourthSqueezedOut && sellableCount > 0)
        {
            int perPayer = sellableCount * GWANG_SALE_WON_PER_CARD * stakeMultiplier;
            var payAmounts = new Dictionary<int, int>();
            foreach (var payer in new[] { order[1], order[2] }) // 2·3번째만 — 선은 빠진다
            {
                int pay = Mathf.Min(perPayer, money[payer]);
                money[payer] -= pay; money[sittingOutSeat] += pay;
                payAmounts[payer] = pay;
                FlyMoneyFX(payer, sittingOutSeat, pay);
            }
            GoStopAudio.Instance?.Money();
            // 어떤 패로 팔았는지·총 얼마인지·누가 내는지를 화면에 직접
            // 보여준다 — 토스트 한 줄("광팔이! (N장)")만으로는 근거를 알 수
            // 없다는 신고. 실제 지급액(perPayer를 clamp한 값)을 그대로
            // 표시한다 — 상대가 그만큼 돈이 없었으면 명목 금액과 달라질 수
            // 있어서다.
            yield return StartCoroutine(ShowGwangSaleSeq(sittingOutSeat, soldCards, payAmounts, order[1], order[2]));
        }
        } // else (SEATS == 4)

        // 2026-08-18: "참가를 누르면 내가 항상 선으로 바뀌는 것 같다"는 신고로
        // 찾은 버그 — 예전엔 ActiveSeats().First()(활성 좌석 중 가장 작은
        // 번호)로 시작 좌석을 정했는데, 내가 항상 좌석 0번이라 내가 참가하는
        // 판에서는 실제 선(dealerSeat)이 누구든 상관없이 **항상 내가 먼저
        // 시작하는** 꼴이 됐다 — 그게 "항상 선이 된 것 같다"는 체감의 정체다.
        // 선은 참가 선언 단계에서 이미 무조건 참가로 확정돼 있으므로(위
        // "선 — 무조건 참가" 참고) 쉬는 좌석 걱정 없이 바로 dealerSeat에서
        // 시작하면 된다.
        currentSeat = dealerSeat;

        ui?.SetScore(money[PLAYER_SEAT]); // 상단 HUD의 SCORE는 판점이 아니라 내 보유 머니를 보여준다(사용자 요청)
        RebuildUI();

        // 총통 — 딜 받은 손패에 같은 달 4장이 통째로 있으면 그 좌석이 즉시 승리한다.
        foreach (var s in ActiveSeats())
        {
            if (GoStopRules.IsChongtong(hand[s]))
            {
                Toast(s, "총통!");
                // 총통 전용 팝업 프리팹은 없어서(ShowActionPopup은 조기
                // 리턴) 대신 큼직한 금색 파티클 버스트로 화려하게
                // 알린다 — 딜 직후 즉시 승리하는 희귀 이벤트라 다른
                // 어떤 캡처 이벤트보다도 임팩트가 커야 한다.
                var canvasRoot = fieldArea.parent.parent.parent as RectTransform;
                GoStopIcons.SpawnBurst(canvasRoot, canvasRoot.InverseTransformPoint(fieldArea.position),
                    new Color(1f, 0.85f, 0.3f), count: 24);
                EndGame(s, fixedBaseScore: 3, extraMultiplier: 4);
                newGameStarting = false;
                yield break;
            }
        }

        // 이번 판 시작 좌석이 내가 아닐 수 있다(내가 쉬는 좌석이면 시작이
        // AI부터다) — 예전엔 currentSeat이 항상 0(나)으로 고정이라 신경 쓸
        // 필요가 없었는데, 광팔이로 시작 좌석이 바뀔 수 있게 되면서
        // 아무도 첫 턴을 걸어주지 않아 게임이 그대로 멈추는 버그가 됐다.
        if (currentSeat != PLAYER_SEAT) StartCoroutine(DelayedAiTurn(currentSeat));
        newGameStarting = false;
    }

    static void SortHand(List<HwatuCard> h) =>
        h.Sort((a, b) => a.month != b.month ? a.month.CompareTo(b.month) : ((int)a.kind).CompareTo((int)b.kind));

    // 2026-08-19: 네트워크 대전 도입 전엔 "seat==0 → 나"가 항상 맞았다
    // (PLAYER_SEAT가 상수 0으로 고정이었으므로). 이제 게스트는 1~3번
    // 좌석을 배정받을 수 있어 더 이상 static일 수 없다 — this.PLAYER_SEAT
    // 기준으로 "나"를 판정하고, 네트워크 판이면 그 좌석에 실제로 접속한
    // 사람의 닉네임(GoStopNetLobby.PlayerNames)을 쓴다. 싱글플레이(호스트도
    // 게스트도 아님)에서는 PLAYER_SEAT가 항상 0으로 고정된 채고 다른
    // 좌석은 전부 AI이므로 예전과 100% 동일한 결과가 나온다.
    string SeatName(int seat) => SeatNameFor(seat, PLAYER_SEAT);

    /// <summary>viewerSeat 기준으로 seat의 표시 이름을 계산한다 — 대부분은
    /// <see cref="SeatName"/>(현재 이 기기 관점)으로 충분하지만,
    /// <see cref="SendTargetedPrompt"/>처럼 <b>호스트가 다른 좌석(게스트)이
    /// 받을 문구를 미리 조립</b>할 때는 호스트 자신의 PLAYER_SEAT(=0) 기준으로
    /// 계산하면 안 된다 — "나"가 실제로는 그 문구를 받는 게스트가 아니라
    /// 호스트 자신을 가리키게 되는 버그가 난다.</summary>
    string SeatNameFor(int seat, int viewerSeat)
    {
        if (seat == viewerSeat) return "나";
        if (isNetworkHost || isNetworkGuest)
        {
            var names = GoStopNetLobby.Instance?.PlayerNames;
            if (names != null && seat >= 0 && seat < names.Length && !string.IsNullOrEmpty(names[seat]))
                return names[seat];
        }
        return seat switch { 0 => "AI", 1 => "AI-A", 2 => "AI-B", 3 => "AI-C", _ => "?" };
    }

    // ── 피 뺏기 헬퍼 (다자간 일반화) ────────────────────────
    // 2인판은 "상대"가 하나뿐이라 고민할 필요가 없었지만, 3인 이상은 뻑 해소만
    // 특정 대상(그 뻑을 만든 좌석)이 뚜렷하고, 쪽·싹쓸이·폭탄은 필드의 중립
    // 카드를 가져가는 보너스라 "누구 걸 뺏는가"가 원래 불분명하다. 이 프로젝트는
    // 2인판의 총 스틸량(쪽/싹쓸이=1장, 폭탄=2장, 자뻑=2장)을 유지하되 이번 판
    // 활성 상대 인원수만큼 나눠서 "각 상대에게서 균등하게" 가져가는 규칙을
    // 택했다 — 쉬는 좌석은 이번 판 캡처 더미가 없으므로 대상에서 제외한다.
    void StealPiFromEachOther(int toSeat, int countEach)
    {
        foreach (var s in ActiveSeats())
        {
            if (s == toSeat) continue;
            int before = captured[toSeat].Count;
            GoStopRules.StealPi(captured[s], captured[toSeat], countEach);
            for (int i = before; i < captured[toSeat].Count; i++)
                RegisterPiFly(s, captured[toSeat][i]);
        }
    }

    /// <summary>피 뺏기 애니메이션 — "피가 이동되는 걸 파악할 수 있도록" 요청.
    /// 카드가 지금 화면에 그려져 있는(뺏기기 전) 위치를 찾아 flyFrom에
    /// 기록해 둔다. 손패→필드→획득패 애니메이션(SlamIn)이 이미 flyFrom을
    /// 보고 도착 지점까지 날아오게 돼 있으므로, DrawPlayerCaptured/
    /// DrawAiCaptured가 다음 RebuildUI에서 이 카드를 뺏은 사람의 획득패
    /// 자리에 새로 그릴 때 자동으로 SlamIn을 태운다 — 새 애니메이션
    /// 시스템을 따로 안 만들고 기존 것을 재사용한 것.</summary>
    void RegisterPiFly(int fromSeat, HwatuCard card)
    {
        int slot = SlotOf(fromSeat);
        if (slot < 0) return;
        var area = slot == 0 ? playerCapArea : (slot <= 3 ? capAreaAI[slot] : null);
        if (area == null) return;
        var t = area.Find(card.spriteName);
        if (t != null) flyFrom[card] = t.position;
    }

    int PpeokMoney() => 3 * WON_PER_POINT * stakeMultiplier;

    /// <summary>보너스 금액을 이번 판 활성 좌석들에게서 균등하게(나머지는 버림) 걷는다.</summary>
    void ApplyMoneyBonus(int seat, int amount)
    {
        var others = ActiveSeats().Where(s => s != seat).ToList();
        if (others.Count == 0) return;
        int share = amount / others.Count;
        foreach (var o in others)
        {
            int pay = Mathf.Min(share, money[o]);
            money[o] -= pay; money[seat] += pay;
            FlyMoneyFX(o, seat, pay);
        }
    }

    // ── 토스트 ───────────────────────────────────────────
    void Toast(int seat, string label)
    {
        ShowTimedToast((seat == PLAYER_SEAT ? "" : SeatName(seat) + " ") + label);
        GoStopAudio.Instance?.PlayForLabel(label);
        ShowActionPopup(label);

        // 호스트 전용 — 뻑/쪽/싹쓸이 등 판정 이벤트는 호스트에서만
        // 발생한다(PlaySeq/DeckOnlySeq는 호스트만 돈다). 게스트도 같은
        // 토스트/사운드/이펙트를 보게 하려면 여기서 직접 실어 보내야 한다
        // (라벨+좌석만 보내고, 받는 쪽이 자기 SeatName()으로 다시 렌더링
        // 하므로 "나"/이름 판정이 받는 사람 기준으로 저절로 맞는다).
        if (isNetworkHost) GoStopNetLobby.Instance?.BroadcastToGuests(GoStopNetMessage.EventMsg(label, seat));
    }

    /// <summary>뻑/쪽/싹쓸이/폭탄(피뺏기 동반)처럼 "지금 뭐가 일어났는지" 피드백이
    /// 약하다는 신고를 받아 추가했다 — 작은 토스트 한 줄만으로는 눈에 잘 안
    /// 띈다. 필드 중앙 위에 큼직한 컬러 텍스트를 띄워 순간적으로 확 커졌다
    /// 사라지게 한다.
    /// 2026-08-18: "쪽/쓸/뻑/뻑난거 가져올 때(감사합니다)/자뻑(더 감사합니다)
    /// 이펙트를 각각 적용하고 프리팹화" 요청 — 코드로 텍스트만 바꿔 그리던
    /// 것을 5개 프리팹(EffectJjok/EffectSweep/EffectPpeok/EffectThanks/
    /// EffectThanksMore, <c>Assets/Resources/Prefabs/GoStop/Effects/</c>)으로
    /// 뽑아서, 문구·색·배경을 코드 수정 없이 프리팹만 열어 바꿀 수 있게 했다
    /// (GoStopEffectPopup 컴포넌트가 DOTween으로 팝인·유지·페이드아웃을
    /// 재생하고 끝나면 스스로 파괴된다). 어떤 프리팹을 쓸지는 label로
    /// 판정하되, "뻑 먹기"(뻑을 남이 해소)와 "자뻑"은 프리팹 기본 문구
    /// ("뻑"이 아니라 "감사합니다"/"더 감사합니다")를 그대로 쓰도록
    /// overrideText 없이 호출한다 — 나머지(쪽/싹쓸이/첫뻑/연뻑 등)는
    /// 실제 라벨 문자열을 그대로 보여준다.</summary>
    void ShowActionPopup(string label)
    {
        // "따닥"은 전용 프리팹을 새로 굽는 대신(2026-08-20) EffectJjok의
        // 구조(팝인·유지·페이드)를 그대로 재사용하고 Play()의 overrideColor로
        // 색만 바꾼다 — 폭탄/뻑이 EffectPpeok을 공유하는 것과 같은 원칙.
        string prefabName =
            label == "자뻑"          ? "EffectThanksMore" :
            label == "뻑 먹기"       ? "EffectThanks" :
            label == "따닥"          ? "EffectJjok" : // exact — "첫따닥"과는 다른 이벤트, 구조 재사용+색만 override
            label.Contains("쪽")     ? "EffectJjok" :
            label.Contains("싹쓸이") ? "EffectSweep" :
            label.Contains("폭탄")   ? "EffectPpeok" : // 전용 프리팹이 없어 뻑과 톤을 공유(주황)
            label.Contains("뻑")     ? "EffectPpeok" : // 뻑/첫뻑/연뻑
            null;
        if (prefabName == null) return;

        // fieldArea → ContentArea(root) → SafeArea → Canvas — 3단계 위.
        var canvasRoot = fieldArea.parent.parent.parent as RectTransform; // Canvas — Overlay와 같은 층
        Vector2 local = canvasRoot.InverseTransformPoint(fieldArea.position);

        // 2026-08-19: "파티클 이펙트로 애니메이션을 좀 더 역동적으로" 요청 —
        // 텍스트 팝업과 같은 자리에 원형 파티클 버스트를 같이 터뜨린다.
        GoStopIcons.SpawnBurst(canvasRoot, local, BurstColorForLabel(label));

        var fx = HwatuUI.InstantiateEffect<GoStopEffectPopup>(prefabName, canvasRoot);
        if (fx == null) return;
        // 필드(더미) 위치를 Canvas 로컬 좌표로 변환해서 그 자리에 띄운다 —
        // ContentArea는 Canvas 안에서 HUD만큼 오프셋돼 있을 수 있어(이 씬은
        // HUD를 꺼서 지금은 오프셋이 없지만, 좌표계 자체는 항상 이렇게
        // 다뤄야 안전하다) Canvas 정중앙(0,0)과 필드 위치가 다를 수 있다.
        fx.root.anchoredPosition = local;

        // "감사합니다"/"더 감사합니다"는 프리팹 기본 문구를 그대로 쓰고,
        // 나머지는 실제 라벨(첫뻑!/연뻑! 등 상황별 문구)을 덮어써서 보여준다.
        // "따닥"만 색까지 override해서(EffectJjok의 하늘색과 구분) 별개
        // 이벤트로 보이게 한다.
        if (prefabName == "EffectThanks" || prefabName == "EffectThanksMore") fx.Play();
        else if (label == "따닥") fx.Play(label, new Color(0.72f, 0.45f, 0.95f));
        else fx.Play(label);
    }

    /// <summary>파티클 버스트 색 — 텍스트 팝업(EffectJjok=하늘색 등)과 톤을
    /// 맞춘다. 흔들기·보너스처럼 팝업 자체가 없는 가벼운 이벤트는 버스트도
    /// 안 뜬다(<see cref="ShowActionPopup"/>가 label 매칭 실패 시 조기
    /// 리턴하므로 이 함수까지 안 온다).</summary>
    static Color BurstColorForLabel(string label)
    {
        if (label == "따닥") return new Color(0.72f, 0.45f, 0.95f); // exact — "첫따닥"과는 다른 이벤트
        if (label.Contains("쪽")) return new Color(0.35f, 0.85f, 1.0f);
        if (label.Contains("싹쓸이")) return new Color(1.0f, 0.82f, 0.25f);
        if (label.Contains("폭탄")) return new Color(1.0f, 0.35f, 0.15f);
        return new Color(0.95f, 0.55f, 0.15f); // 뻑 계열
    }

    void ShowTimedToast(string msg)
    {
        ui?.ShowToast(msg);
        if (toastHideCo != null) StopCoroutine(toastHideCo);
        toastHideCo = StartCoroutine(HideToastAfter(1.1f));
    }

    IEnumerator HideToastAfter(float sec)
    {
        yield return new WaitForSeconds(sec);
        ui?.HideToast();
        toastHideCo = null;
    }

    // ── 플레이어 입력 ────────────────────────────────────
    void OnPlayerPlay(HwatuCard card)
    {
        if (state != State.Turn || currentSeat != PLAYER_SEAT || actionBusy) return;

        bool tripleInHand = hand[PLAYER_SEAT].Count(c => c.month == card.month) == 3;
        if (tripleInHand && !shookMonths[PLAYER_SEAT].Contains(card.month))
        {
            pendingShakeCard = card;
            shakePopup.messageText.text = $"{card.month}월 흔들기 선언하시겠습니까?";
            shakePopup.Show();
            return;
        }
        ContinuePlayerPlay(card, false);
    }

    void OnShakeChoice(bool shake)
    {
        shakePopup.Hide();
        var card = pendingShakeCard;
        pendingShakeCard = null;
        if (card == null || state != State.Turn || currentSeat != PLAYER_SEAT || actionBusy) return;
        ContinuePlayerPlay(card, shake);
    }

    void ContinuePlayerPlay(HwatuCard card, bool declareShake)
    {
        if (isNetworkGuest)
        {
            // 로컬 상태를 안 건드린다 — 판정은 호스트만 하고, 그 결과가
            // StateSync로 돌아오면 ApplyNetworkSnapshot이 화면을 맞춘다.
            GoStopNetLobby.Instance.SendToHost(GoStopNetMessage.PlayWithShake(card.spriteName, declareShake));
            return;
        }
        actionBusy = true;
        StartCoroutine(PlaySeq(PLAYER_SEAT, card, declareShake, () => AfterAction(PLAYER_SEAT)));
    }

    void OnPlayerBombSkip()
    {
        if (state != State.Turn || currentSeat != PLAYER_SEAT || bombCredits[PLAYER_SEAT] == 0 || actionBusy) return;
        if (isNetworkGuest)
        {
            GoStopNetLobby.Instance.SendToHost(GoStopNetMessage.BombSkipMsg());
            return;
        }
        bombCredits[PLAYER_SEAT]--;
        actionBusy = true;
        StartCoroutine(DeckOnlySeq(PLAYER_SEAT, () => AfterAction(PLAYER_SEAT)));
    }

    /// <summary>캡처 결과가 "낸/뒤집은 카드 + 맞은 필드패 1장"(딱 2장) 형태면,
    /// 그 필드패가 있던 자리를 <see cref="flyViaField"/>에 기록해 둔다 — 다음
    /// RebuildUI 때 그 자리를 거쳐 날아가는 2단 연출(SlamInViaField)이 걸린다.
    /// 이번 판 리빌드가 필드를 갈아엎기 전에(필드 GameObject가 아직 살아있을
    /// 때) 불러야 한다. 3장 이상 딸려오는 뻑 해소/폭탄은 "어느 한 장을
    /// 쳤다"고 하기 애매해서 대상에서 뺀다 — 2인판(GoStopGame.cs)의 같은
    /// 이름 함수와 완전히 동일한 판정이다.</summary>
    void RegisterFlyViaField(GoStopRules.CaptureResult r)
    {
        if (r.captured.Count != 2) return;
        var mover = r.captured[0];
        var hit = r.captured[1];
        var hitGo = fieldArea.Find(hit.spriteName);
        if (hitGo != null) flyViaField[mover] = hitGo.position;
    }

    // ── 카드 한 장 처리 (손패 → 필드 매칭 → 덱 뒤집기 → 필드 매칭) ─────
    IEnumerator PlaySeq(int seat, HwatuCard card, bool declareShake, System.Action onDone)
    {
        var h = hand[seat];
        var cap = captured[seat];

        GoStopAudio.Instance?.CardPlay();

        // 낸 카드가 어디서 날아왔는지 기록 — 내 손이면 실제 슬롯 자리,
        // 상대 손이면 그 좌석의 뒷면 뭉치 자리. backArea는 이제 좌석이
        // 아니라 화면 슬롯(0~3) 인덱스라 SlotOf로 변환해야 한다 — 그 좌석이
        // 상단 슬롯(2, Back 영역 없음)이나 하단(0, AI가 대신 앉는 드문 경우)에
        // 있으면 대신 필드(테이블 중앙)에서 날아오는 것으로 근사한다.
        int originSlotIdx = SlotOf(seat);
        RectTransform originSlot = seat == PLAYER_SEAT ? FindHandSlot(card)
            : (originSlotIdx == 1 || originSlotIdx == 3) ? backArea[originSlotIdx] : fieldArea;
        flyFrom[card] = originSlot != null ? originSlot.position : fieldArea.position;

        if (h.Count(c => c.month == card.month) == 3 && declareShake && shookMonths[seat].Add(card.month))
        {
            heundeulCount[seat]++;
            Toast(seat, $"{card.month}월 흔들기");
        }

        bool wasFirstPlay = isFirstPlayOfRound;
        isFirstPlayOfRound = false;

        var r1 = GoStopRules.ResolveWithBomb(card, h, field, out bool bomb);

        // 따닥 — 필드에 같은 달이 2장 있어 손패로 그중 하나를 고른 뒤, 같은
        // 턴의 뒷패가 남은 나머지 한 장과 마저 맞아떨어지는 것(사용자 확인
        // 규칙, 2026-08-20). 선택 직후엔 아직 뒷패가 안 나왔으니 "고르지
        // 않은 나머지 한 장"만 기억해 두고, 뒷패 처리(r2) 쪽에서 그 카드가
        // 실제로 잡히는지 확인한다.
        HwatuCard ddadakWatch = null;

        bool r1HadChoice = !bomb && r1.choiceCandidates != null;
        if (r1HadChoice)
        {
            // 이 2장 매칭이 사실은 "보너스피가 얹힌 뻑"이면(그 달에 ppeokBonusPi
            // 항목이 있으면) 선택 팝업을 띄우지 말고 조커까지 포함해 통째로
            // 쓸어간다 — 안 그러면 고르지 않은 1장+조커가 필드에 영원히 남는다.
            if (ppeokBonusPi.TryGetValue(card.month, out var jokerAtCard))
            {
                r1 = GoStopRules.ResolveJokerPpeok(card, r1.choiceCandidates, jokerAtCard, field);
                r1HadChoice = false;
                // 조커를 이미 캡처 목록에 포함시켰다 — ApplyMatchBonus의 별도
                // ppeokBonusPi 핸드오프(matchCount==3 분기)가 또 한 번 넘겨주면
                // 캡처 목록에 조커가 두 번 들어간다. 여기서 먼저 지워 막는다.
                ppeokBonusPi.Remove(card.month);
            }
            else
            {
                var candidates = r1.choiceCandidates;
                GoStopRules.CaptureResult chosen1 = null;
                yield return StartCoroutine(ContinueChoice(card, r1, seat, res => chosen1 = res));
                r1 = chosen1;
                ddadakWatch = candidates.FirstOrDefault(c => !r1.captured.Contains(c));
                if (wasFirstPlay) { ApplyMoneyBonus(seat, PpeokMoney()); Toast(seat, "첫따닥"); }
            }
        }

        if (bomb) bombCount[seat]++;

        // 2026-08-22: "뒷패가 공개되기 전에 결과가 노출되면 안 된다" 요청으로
        // 순서를 바꿨다 — 예전엔 뻑 여부를 drawPile[0].month를 몰래 들여다봐서
        // (화면엔 아무것도 안 보여준 채) 먼저 정하고, 그 결과에 따라 r1을
        // 곧장 Cap으로 보내거나(뻑 아님) 필드에 묶어뒀다(뻑) — 그런데 "카드가
        // 곧장 Cap으로 날아가는 애니메이션이 나온다"는 사실 자체가 뒷패 얼굴을
        // 보기도 전에 "이번엔 뻑이 아니다"를 알려주는 셈이었다. 지금은 뒷패를
        // 먼저 뽑아 **얼굴만** 공개(아직 field/captured 어디에도 안 넣는다,
        // 더미 자리에 잠깐 보여주고 지운다)하고, 그 다음에야 뻑·일반 캡처·
        // 쪽·따닥을 전부 판정해서 최종 위치로 옮긴다. 손패→뒷패 2단계 페이싱
        // (SlamIn이 헷갈리지 않게 나눠 보여주는 것)은 그대로 유지한다.
        bool willDraw = !bomb && drawPile.Count > 0;
        HwatuCard drawn = null;
        bool isLastDeckCard = false;
        if (willDraw)
        {
            drawn = drawPile[0]; drawPile.RemoveAt(0);
            isLastDeckCard = drawPile.Count == 0;
            flyFrom[drawn] = drawPileArea.position;

            var revealGo = HwatuUI.MakeCard(drawn, ui.ContentArea, drawPileArea.anchoredPosition, FIELD_W, FIELD_H, null, false);
            yield return new WaitForSeconds(PLAY_STEP_DELAY);
            Destroy(revealGo);
        }

        // 뻑 감지 — 이제 이미 공개된 drawn의 월을 직접 비교한다(2인판과 동일
        // 조건: matchCount==1, 선택 캡처 제외, 조커는 뻑을 못 만든다).
        bool ppeokFormed = !bomb && !r1HadChoice && r1.matchCount == 1
                           && drawn != null && !drawn.isJoker && drawn.month == card.month;
        if (ppeokFormed)
        {
            field.AddRange(r1.captured);
            field.Add(drawn);
            ppeokCauser[card.month] = seat;

            int streak = ++ppeokStreak[seat];
            int total = ++ppeokTotalCount[seat];
            if (wasFirstPlay) { ApplyMoneyBonus(seat, PpeokMoney()); Toast(seat, "첫뻑"); }
            else if (streak == 2) { ApplyMoneyBonus(seat, PpeokMoney()); Toast(seat, "연뻑"); }
            else Toast(seat, "뻑");

            RebuildUI();
            yield return new WaitForSeconds(PLAY_STEP_DELAY);

            // 쓰리뻑 — 연속일 필요 없이 이번 판 통산 3번째 뻑이면 그 자리에서
            // 즉시 승리(구글링으로 확인한 표준 규칙, "연속 아니어도 통산
            // 3회면 쓰리뻑"). 예전엔 연속(streak)으로만 판정해서 "3연뻑"
            // 이라는 실제로는 없는 용어를 썼었다.
            if (total >= 3) { EndGame(seat, fixedBaseScore: 3); yield break; }

            actionBusy = false;
            onDone?.Invoke();
            yield break;
        }
        ppeokStreak[seat] = 0;

        // 국열끗(9월 열끗) 선택 팝업 — "모든 패가 Cap에 들어간 뒤"로 미룬다
        // (요청 8번). r1/r2 어느 쪽에서 잡히든 여기 모아뒀다가 턴 맨 끝에
        // 순서대로 묻는다.
        var dualPiPending = new List<HwatuCard>();

        if (r1.captured.Count > 0)
        {
            cap.AddRange(r1.captured);
            GoStopAudio.Instance?.Capture();
            ApplyMatchBonus(seat, r1, bomb, allowSweep: bomb || !willDraw);
            RegisterFlyViaField(r1);
            if (seat == PLAYER_SEAT || IsRemoteSeat(seat))
            {
                var dual = r1.captured.FirstOrDefault(c => c.dualPi);
                if (dual != null) dualPiPending.Add(dual);
            }
        }
        RebuildUI();
        yield return new WaitForSeconds(PLAY_STEP_DELAY);

        if (bomb)
        {
            bombCredits[seat] += 2;
            foreach (var dual in dualPiPending)
                yield return StartCoroutine(PromptDualPiChoice(dual, seat));
            actionBusy = false;
            onDone?.Invoke();
            yield break;
        }

        if (willDraw)
        {
            if (drawn.isJoker)
            {
                // "필드에 방금 나온 패" = 이번에 낸 손패가 매칭 안 돼 그대로
                // 필드에 남은 경우(r1.captured가 비었으면 card가 필드에 있다)
                // 그 카드다. 손패가 뭔가를 잡았으면 남은 카드가 없어 겹쳐놓을
                // 대상이 없다 — 그런 경우엔 즉시 캡처로 단순화한다.
                HwatuCard anchor = r1.captured.Count == 0 ? card : null;
                yield return StartCoroutine(ResolveBonusJoker(seat, drawn, anchor, cap));
            }
            else
            {
                var r2 = GoStopRules.Resolve(drawn, field);
                if (r2.choiceCandidates != null)
                {
                    if (ppeokBonusPi.TryGetValue(drawn.month, out var jokerAtDrawn))
                    {
                        r2 = GoStopRules.ResolveJokerPpeok(drawn, r2.choiceCandidates, jokerAtDrawn, field);
                        ppeokBonusPi.Remove(drawn.month); // 이중 지급 방지 (위 r1 분기와 같은 이유)
                    }
                    else
                    {
                        GoStopRules.CaptureResult chosen2 = null;
                        yield return StartCoroutine(ContinueChoice(drawn, r2, seat, res => chosen2 = res));
                        r2 = chosen2;
                    }
                }
                if (r2.captured.Count > 0)
                {
                    cap.AddRange(r2.captured);
                    GoStopAudio.Instance?.Capture();
                    RegisterFlyViaField(r2);
                    bool chok = r1.placedOnField && r2.captured.Contains(card) && !isLastDeckCard;
                    // 따닥: 손패로 필드 2장 중 하나를 고른 뒤(ddadakWatch=고르지
                    // 않은 나머지 한 장), 같은 턴의 뒷패가 그 나머지 한 장마저
                    // 잡았다. chok과는 조건이 겹치지 않는다(chok은 r1.placedOnField,
                    // 즉 손패가 아무것도 못 먹은 경우에만 성립하는데, ddadakWatch는
                    // 반대로 손패가 선택 캡처로 뭔가를 먹었을 때만 채워진다).
                    bool ddadak = ddadakWatch != null && r2.captured.Contains(ddadakWatch) && !isLastDeckCard;
                    if (chok)
                    {
                        StealPiFromEachOther(seat, 1);
                        Toast(seat, "쪽");
                        if (r2.sweep)
                        {
                            sweeps[seat]++;
                            StealPiFromEachOther(seat, 1);
                            Toast(seat, "싹쓸이");
                        }
                    }
                    else if (ddadak)
                    {
                        StealPiFromEachOther(seat, 1);
                        Toast(seat, "따닥");
                        if (r2.sweep)
                        {
                            sweeps[seat]++;
                            StealPiFromEachOther(seat, 1);
                            Toast(seat, "싹쓸이");
                        }
                    }
                    else ApplyMatchBonus(seat, r2, false, allowSweep: !isLastDeckCard);

                    if (seat == PLAYER_SEAT || IsRemoteSeat(seat))
                    {
                        var dual2 = r2.captured.FirstOrDefault(c => c.dualPi);
                        if (dual2 != null) dualPiPending.Add(dual2);
                    }
                }
            }
            RebuildUI();
        }

        yield return new WaitForSeconds(PLAY_STEP_DELAY);

        foreach (var dual in dualPiPending)
            yield return StartCoroutine(PromptDualPiChoice(dual, seat));

        actionBusy = false;
        onDone?.Invoke();
    }

    /// <summary>손패 없이 덱만 한 장 넘긴다 — 폭탄 크레딧 소모, 또는 손패가 이미 바닥난 뒤.</summary>
    IEnumerator DeckOnlySeq(int seat, System.Action onDone)
    {
        var cap = captured[seat];

        if (drawPile.Count == 0) { RebuildUI(); actionBusy = false; onDone?.Invoke(); yield break; }

        var drawn = drawPile[0]; drawPile.RemoveAt(0);

        if (drawn.isJoker)
        {
            // 손패를 안 낸 턴(덱만 넘기기)이라 "이전 손패에서 선택한 패"가
            // 없다 — 겹쳐놓을 대상이 없으므로 즉시 캡처로 단순화한다.
            yield return StartCoroutine(ResolveBonusJoker(seat, drawn, null, cap));
        }
        else
        {
            bool isLastDeckCard = drawPile.Count == 0;
            flyFrom[drawn] = drawPileArea.position;
            var r = GoStopRules.Resolve(drawn, field);
            if (r.choiceCandidates != null)
            {
                if (ppeokBonusPi.TryGetValue(drawn.month, out var jokerAtDrawn2))
                {
                    r = GoStopRules.ResolveJokerPpeok(drawn, r.choiceCandidates, jokerAtDrawn2, field);
                    ppeokBonusPi.Remove(drawn.month); // 이중 지급 방지 (위 r1 분기와 같은 이유)
                }
                else
                {
                    GoStopRules.CaptureResult chosen = null;
                    yield return StartCoroutine(ContinueChoice(drawn, r, seat, res => chosen = res));
                    r = chosen;
                }
            }
            HwatuCard dualPending = null;
            if (r.captured.Count > 0)
            {
                cap.AddRange(r.captured);
                GoStopAudio.Instance?.Capture();
                ApplyMatchBonus(seat, r, false, allowSweep: !isLastDeckCard);
                RegisterFlyViaField(r);
                if (seat == PLAYER_SEAT || IsRemoteSeat(seat))
                    dualPending = r.captured.FirstOrDefault(c => c.dualPi);
            }

            RebuildUI();
            yield return new WaitForSeconds(PLAY_STEP_DELAY);

            // 국열끗 선택은 모든 패가 Cap에 들어간 뒤(요청 8번).
            if (dualPending != null)
                yield return StartCoroutine(PromptDualPiChoice(dualPending, seat));

            actionBusy = false;
            onDone?.Invoke();
            yield break;
        }

        RebuildUI();
        yield return new WaitForSeconds(PLAY_STEP_DELAY);
        actionBusy = false;
        onDone?.Invoke();
    }

    /// <summary>보너스피(조커) 처리. 조커는 월이 없어(<see cref="HwatuCard.isJoker"/>)
    /// 실제 매칭에 참여할 수 없으므로 <paramref name="anchor"/>(이번 턴에
    /// 낸 손패가 매칭 안 돼 필드에 남은 카드, 없으면 null) 유무와 무관하게
    /// 항상 그 자리에서 바로 가져간다.
    /// <br/>
    /// 2026-08-20 재작성(사용자 신고 — "필드에 홀수 개의 패가 남는다"의
    /// 원인을 찾음, 2인판 GoStopGame.cs와 동일한 버그·동일한 수정). 예전엔
    /// anchor가 없으면 뒷패를 아예 더 안 깠고, anchor가 있어도 "다른
    /// 달이면" 뒷패(extra)를 <see cref="GoStopRules.Resolve"/> 없이 그냥
    /// 필드에 던져버렸다 — extra가 필드에 이미 있는(anchor와 무관한) 다른
    /// 카드와 우연히 짝이 맞아도 절대 안 먹히고 계속 필드에 쌓이기만 했다.
    /// 조커는 "진짜 카드"가 아니라 이번 턴의 덱 소모 몫을 아직 못 채웠으므로,
    /// **anchor 유무와 무관하게 항상** 뒷패를 한 장 더 까고 일반 덱 캡처와
    /// 완전히 같은 경로(Resolve→선택→매칭 판정)를 거친다 — anchor가 이
    /// 카드에 맞춰 잡히면 그게 곧 쪽이다(예전의 "extra.month==anchor.month"
    /// 특수 분기를 Resolve()의 결과로 자연스럽게 흡수했다). 3장이 함께
    /// 캡처되던 예전 "쪽" 연출과 최종 결과(anchor·extra·joker가 전부 같은
    /// 좌석 것이 됨)는 동일하다 — 조커가 한 박자 먼저 캡처되고 anchor+extra가
    /// 뒤이어 잡히는 것으로 나뉠 뿐이다.</summary>
    IEnumerator ResolveBonusJoker(int seat, HwatuCard joker, HwatuCard anchor, List<HwatuCard> cap)
    {
        field.Add(joker);
        flyFrom[joker] = drawPileArea.position;
        RebuildUI();
        yield return new WaitForSeconds(PLAY_STEP_DELAY * 0.5f);

        field.Remove(joker);
        cap.Add(joker);
        flyFrom[joker] = fieldArea.position;
        Toast(seat, "보너스 획득");
        RebuildUI();
        yield return new WaitForSeconds(PLAY_STEP_DELAY * 0.5f);

        if (drawPile.Count == 0) yield break;

        var extra = drawPile[0]; drawPile.RemoveAt(0);

        if (extra.isJoker)
        {
            // 두 조커가 연달아 나오는 극히 드문 경우 — 같은 함수를 재귀
            // 호출해서 이번에도 같은 anchor 기준으로 처리한다.
            yield return StartCoroutine(ResolveBonusJoker(seat, extra, anchor, cap));
            yield break;
        }

        flyFrom[extra] = drawPileArea.position;
        bool isLastDeckCard = drawPile.Count == 0;
        var r = GoStopRules.Resolve(extra, field);

        if (r.choiceCandidates != null)
        {
            if (ppeokBonusPi.TryGetValue(extra.month, out var jokerAtExtra))
            {
                r = GoStopRules.ResolveJokerPpeok(extra, r.choiceCandidates, jokerAtExtra, field);
                ppeokBonusPi.Remove(extra.month); // 이중 지급 방지 (PlaySeq의 r1/r2 분기와 같은 이유)
            }
            else
            {
                GoStopRules.CaptureResult chosen = null;
                yield return StartCoroutine(ContinueChoice(extra, r, seat, res => chosen = res));
                r = chosen;
            }
        }

        if (r.captured.Count > 0)
        {
            cap.AddRange(r.captured);
            GoStopAudio.Instance?.Capture();
            RegisterFlyViaField(r);

            // 쪽 — anchor가 이 뒷패에 맞춰 잡혔다. PlaySeq의 일반 쪽 판정
            // (r1.placedOnField && r2.captured.Contains(card))과 완전히 같은 형태다.
            bool chok = anchor != null && r.captured.Contains(anchor) && !isLastDeckCard;
            if (chok)
            {
                StealPiFromEachOther(seat, 1);
                Toast(seat, "보너스+쪽");
                if (r.sweep)
                {
                    sweeps[seat]++;
                    StealPiFromEachOther(seat, 1);
                    Toast(seat, "싹쓸이");
                }
            }
            else ApplyMatchBonus(seat, r, false, allowSweep: !isLastDeckCard);

            if (seat == PLAYER_SEAT || IsRemoteSeat(seat))
            {
                var dual = r.captured.FirstOrDefault(c => c.dualPi);
                if (dual != null) yield return StartCoroutine(PromptDualPiChoice(dual, seat));
            }
        }

        RebuildUI();
        yield return new WaitForSeconds(PLAY_STEP_DELAY);
    }

    /// <summary>폭탄이 아닌 매칭 보너스(뻑 해소/자뻑·싹쓸이) — 쪽은 <see cref="PlaySeq"/>에서
    /// ApplyMatchBonus보다 먼저 걸러진다(안 그러면 그냥 일반 매칭으로 지나쳐 버린다).</summary>
    void ApplyMatchBonus(int seat, GoStopRules.CaptureResult r, bool bomb, bool allowSweep = true)
    {
        if (bomb) { StealPiFromEachOther(seat, 1); Toast(seat, "폭탄"); }
        else if (r.matchCount == 3)
        {
            // 2026-08-21 정정(사용자 확인) — 예전엔 "비자뻑은 causer 한 명
            // 에게서만 1장"이었는데, 실제 규칙은 **일반 뻑 먹기도 자뻑처럼
            // 다른 유저 전원에게서** 뺏는다 — 다만 자뻑은 각자에게서 2장씩,
            // 일반은 각자에게서 1장씩으로 배수만 다르다. causer가 누군지는
            // "이게 진짜 뻑 해소가 맞는지"(ppeokCauser에 항목이 있는지)
            // 확인하는 용도로만 쓰고, 스틸 자체는 항상 StealPiFromEachOther
            // 하나로 통일한다.
            int month = r.captured[0].month;
            if (ppeokCauser.TryGetValue(month, out int causer))
            {
                bool selfPpeok = causer == seat;
                StealPiFromEachOther(seat, selfPpeok ? 2 : 1);
                Toast(seat, selfPpeok ? "자뻑" : "뻑 먹기");
                ppeokCauser.Remove(month);
            }
            // 그 뻑에 보너스피가 같이 묻혀 있었으면(ResolveBonusJoker 참고)
            // 지금 이걸 해소하는 사람이 그 보너스피도 같이 가져간다.
            if (ppeokBonusPi.TryGetValue(month, out var bonus))
            {
                field.Remove(bonus);
                captured[seat].Add(bonus);
                flyFrom[bonus] = fieldArea.position;
                ppeokBonusPi.Remove(month);
                Toast(seat, "보너스 획득");
            }
        }

        if (r.sweep && allowSweep)
        {
            sweeps[seat]++;
            StealPiFromEachOther(seat, 1);
            Toast(seat, "싹쓸이");
        }
    }

    /// <summary>필드 2장 매칭 선택 — 플레이어는 팝업, AI는 GoStopAI.ChooseFieldMatch로 즉시.</summary>
    IEnumerator ContinueChoice(HwatuCard played, GoStopRules.CaptureResult initial, int seat,
                               System.Action<GoStopRules.CaptureResult> onResolved)
    {
        if (initial.choiceCandidates == null) { onResolved(initial); yield break; }

        HwatuCard chosen;
        if (seat == PLAYER_SEAT)
        {
            pendingFieldChoice = null;
            ShowFieldChoicePopup(initial.choiceCandidates);
            yield return new WaitUntil(() => pendingFieldChoice != null);
            chosen = pendingFieldChoice;
            HideFieldChoicePopup();
        }
        else if (IsRemoteSeat(seat))
        {
            SendTargetedPrompt(seat, s => s.fieldChoiceCandidates = GoStopDeck.EncodeAll(initial.choiceCandidates));
            GoStopNetMessage msg = null;
            yield return StartCoroutine(WaitForRemoteMessage(seat,
                m => m.type == GoStopNetMessage.Type.FieldChoice, m => msg = m));
            // 게스트가 보낸 카드 이름으로 진짜 후보 인스턴스를 찾는다 —
            // 게스트가 갖고 있는 건 스냅샷에서 새로 디코딩한 별개의
            // HwatuCard 객체라 참조가 다르다(GoStopRules 내부는 리스트
            // 안 참조 동일성으로 카드를 다루므로 반드시 원본을 찾아 써야 한다).
            var decoded = GoStopDeck.Decode(msg.cardId);
            chosen = decoded != null ? initial.choiceCandidates.FirstOrDefault(c => c.spriteName == decoded.spriteName) : null;
            if (chosen == null) chosen = GoStopAI.ChooseFieldMatch(initial.choiceCandidates); // 방어 — 오염된 메시지가 와도 판이 안 멈추게
        }
        else chosen = GoStopAI.ChooseFieldMatch(initial.choiceCandidates);

        onResolved(GoStopRules.ResolveChoice(played, chosen, field));
    }

    IEnumerator PromptDualPiChoice(HwatuCard card, int seat)
    {
        if (IsRemoteSeat(seat))
        {
            SendTargetedPrompt(seat, s => s.dualPiChoicePending = true);
            GoStopNetMessage msg = null;
            yield return StartCoroutine(WaitForRemoteMessage(seat,
                m => m.type == GoStopNetMessage.Type.DualPiChoice, m => msg = m));
            card.useAsPi = msg.boolValue;
            yield break;
        }
        pendingDualPiChoice = null;
        dualPiPopup.Show();
        yield return new WaitUntil(() => pendingDualPiChoice != null);
        card.useAsPi = pendingDualPiChoice.Value;
        dualPiPopup.Hide();
    }

    // ── 턴 이후 판정 ─────────────────────────────────────
    void AfterAction(int seat)
    {
        if (seat != PLAYER_SEAT && !IsRemoteSeat(seat))
        {
            // AI는 팝업이 없으니 9월 열끗을 곧바로 유리한 쪽으로 최적화한다.
            // 원격 좌석은 여기서 제외한다 — 그쪽은 이미 캡처 시점에
            // PromptDualPiChoice가 실제 사람에게 물어봤다(위 PlaySeq/
            // DeckOnlySeq의 호출부 참고), 여기서 또 AI가 덮어쓰면 안 된다.
            if (captured[seat].Any(c => c.dualPi))
            {
                GoStopAI.OptimizeDualPi(captured[seat]);
                RebuildUI();
            }
        }

        if (CheckHandsEmpty()) return;

        int rawScore = GoStopRules.CalcScore(captured[seat], sweeps[seat]).Total;

        // 더 낼 손패도, 쓸 폭탄 크레딧도 없으면 점수 변동 여부와 무관하게
        // 그 자리에서 끝난다 — 더 진행할 방법이 없다.
        if (seat == PLAYER_SEAT && hand[PLAYER_SEAT].Count == 0 && bombCredits[PLAYER_SEAT] == 0 && rawScore >= CAPTURE_LINE)
        {
            EndGame(PLAYER_SEAT);
            return;
        }

        // lastGoScore보다 실제로 더 올라갔을 때만 다시 묻는다 — 안 그러면
        // 아무것도 못 먹어 점수가 그대로인 턴에도 매번 고/스톱을 물어보게
        // 된다("점수 변동이 없어도 계속 팝업이 뜬다"는 신고).
        if (rawScore >= CAPTURE_LINE && rawScore > lastGoScore[seat])
        {
            if (seat == PLAYER_SEAT)
            {
                ShowGoStopPrompt(rawScore);
                return;
            }

            if (IsRemoteSeat(seat))
            {
                StartCoroutine(RemoteGoStopSeq(seat, rawScore));
                return;
            }

            if (GoStopAI.ShouldGo(rawScore, goCount[seat], hand[seat].Count))
            {
                goCount[seat]++;
                lastGoScore[seat] = rawScore;
                calledGo[seat] = true;
                // 고를 부른 순간 점수에도 +1이 즉시 반영된다(정산 때만 반영되던
                // 걸 화면 표시에도 맞췄다 — "3점에서 고하면 4점이 돼야 한다"는
                // 신고).
                ShowTimedToast($"{SeatName(seat)}가 고를 외쳤습니다! ({rawScore + goCount[seat]}점)");
                GoStopAudio.Instance?.Go();
                AdvanceTurn();
                return;
            }
            GoStopAudio.Instance?.Stop();
            EndGame(seat);
            return;
        }
        AdvanceTurn();
    }

    /// <summary>원격 좌석의 고/스톱 결정 — 로컬 플레이어의 ShowGoStopPrompt/
    /// OnPlayerGo/OnPlayerStop과 같은 판정을 코루틴 하나로 묶은 것뿐이다
    /// (원격 쪽엔 버튼 클릭으로 다시 진입할 지점이 없으니 굳이 나눌
    /// 이유가 없다).</summary>
    IEnumerator RemoteGoStopSeq(int seat, int rawScore)
    {
        state = State.GoStopChoice;
        // 이 state 변경을 그 좌석에게 즉시 알려야 한다 — 안 그러면
        // 게스트 화면은 여전히 지난 스냅샷(state=Turn)을 들고 있어서
        // "고/스톱을 물어야 한다"는 걸 전혀 모른 채로 남는다(ApplyNetworkSnapshot의
        // 고/스톱 오버레이 판정은 순전히 state/currentSeat를 보고 하므로).
        // 그 사이 호스트는 WaitForRemoteMessage에서 응답을 영원히 기다리는
        // 교착이 된다 — AdvanceTurn의 같은 버그를 여기서도 그대로 겪는다.
        // BroadcastNetworkState() 대신 RebuildUI()를 쓴다 — 게스트에게
        // 브로드캐스트하는 건 물론, 호스트 자기 자신의 화면도 같이
        // 갱신해야 FillSlot의 "고/스톱 선택 중..." 표시가 호스트 쪽에도
        // 뜬다(2인판 ShowGoStopPrompt에서도 같은 이유로 RebuildUI로 고쳤다).
        RebuildUI();
        GoStopNetMessage msg = null;
        yield return StartCoroutine(WaitForRemoteMessage(seat,
            m => m.type == GoStopNetMessage.Type.GoStopDecision, m => msg = m));

        if (msg.boolValue)
        {
            goCount[seat]++;
            lastGoScore[seat] = rawScore;
            calledGo[seat] = true;
            ShowTimedToast($"{SeatName(seat)}가 고를 외쳤습니다! ({rawScore + goCount[seat]}점)");
            GoStopAudio.Instance?.Go();
            AdvanceTurn();
        }
        else
        {
            GoStopAudio.Instance?.Stop();
            EndGame(seat);
        }
    }

    void ShowGoStopPrompt(int rawScore)
    {
        state = State.GoStopChoice;
        pendingGoRawScore = rawScore;
        int displayScore = rawScore + goCount[PLAYER_SEAT]; // 이미 쌓인 고 보너스까지 반영해서 보여준다
        ui?.ShowOverlay(new Color(.93f, .73f, .18f), $"{displayScore}점 달성!", displayScore.ToString(),
            "고 하시겠습니까, 스톱 하시겠습니까?", "고", OnPlayerGo, "스톱", OnPlayerStop);

        // 2026-08-20: 이 함수는 항상 호스트 자신(PLAYER_SEAT)의 결정에서만
        // 불린다(AfterAction의 seat==PLAYER_SEAT 분기) — 예전엔 여기서
        // state만 바뀌고 아무도 다시 그리지 않아서, 다른 좌석들은 호스트가
        // 왜 멈췄는지 몰랐다(RemoteGoStopSeq와 같은 버그, 2인판에서도
        // 똑같이 겪었다). RebuildUI()가 FillSlot의 "▶ 고/스톱 선택 중"
        // 표시를 갱신하고 게스트에게도 브로드캐스트한다.
        RebuildUI();
    }

    void OnPlayerGo()
    {
        ui?.HideOverlay();
        if (isNetworkGuest)
        {
            // 호스트 쪽 RemoteGoStopSeq가 이미 이 좌석의 GoStopDecision을
            // 기다리고 있다 — 여기서는 로컬 상태를 안 건드리고 보내기만
            // 한다. 판정 결과(goCount 증가 등)는 다음 StateSync로 온다.
            GoStopNetLobby.Instance.SendToHost(GoStopNetMessage.GoStop(true));
            return;
        }
        goCount[PLAYER_SEAT]++;
        lastGoScore[PLAYER_SEAT] = pendingGoRawScore; // 이 점수를 넘어서야 다음에 다시 묻는다
        calledGo[PLAYER_SEAT] = true;
        GoStopAudio.Instance?.Go();
        AdvanceTurn();
    }

    void OnPlayerStop()
    {
        ui?.HideOverlay();
        if (isNetworkGuest)
        {
            GoStopNetLobby.Instance.SendToHost(GoStopNetMessage.GoStop(false));
            return;
        }
        GoStopAudio.Instance?.Stop();
        EndGame(PLAYER_SEAT);
    }

    void AdvanceTurn()
    {
        if (CheckHandsEmpty()) return;

        do { currentSeat = (currentSeat + 1) % SEATS; } while (currentSeat == sittingOutSeat);
        state = State.Turn;
        GoStopAudio.Instance?.TurnChange();

        if (currentSeat == PLAYER_SEAT)
        {
            if (hand[PLAYER_SEAT].Count == 0 && bombCredits[PLAYER_SEAT] == 0) StartCoroutine(DelayedPlayerHandEmpty());
            else RebuildUI();
        }
        else
        {
            // 2026-08-19: 예전엔 "내 차례가 됐을 때만" 다시 그렸다(AI 턴은
            // 어차피 AI가 알아서 움직이니 그릴 필요가 없다고 가정) — 네트워크
            // 대전에서 이 가정이 깨진다. 원격 좌석 차례로 넘어갈 때 여기서
            // 다시 그리지 않으면(=BroadcastNetworkState가 안 나가면) 그
            // 게스트의 화면은 지난 currentSeat를 그대로 들고 있어 "내 차례"를
            // 전혀 못 알아채고, 호스트는 RemoteTurn에서 응답을 영원히 기다리는
            // 교착이 생긴다(실제로 겪은 버그). 싱글플레이/AI 턴에도 똑같이
            // 적용해도 무해하다 — 오히려 누구 차례인지 더 빨리 반영된다.
            RebuildUI();
            StartCoroutine(DelayedAiTurn(currentSeat));
        }
    }

    IEnumerator DelayedPlayerHandEmpty()
    {
        yield return new WaitForSeconds(0.6f);
        if (state == State.Turn && currentSeat == PLAYER_SEAT)
        {
            actionBusy = true;
            StartCoroutine(DeckOnlySeq(PLAYER_SEAT, () => AfterAction(PLAYER_SEAT)));
        }
    }

    IEnumerator DelayedAiTurn(int seat)
    {
        // 원격 좌석은 "AI가 생각하는 척" 지연이 필요 없다 — 오히려 늦게
        // 듣기 시작하면 그새 도착한 빠른 응답을 놓친다(구독 전에 온
        // 메시지는 그냥 사라진다, 이벤트라 버퍼링이 없다). 그 좌석
        // 차례가 되는 즉시 듣기 시작해야 한다.
        if (IsRemoteSeat(seat))
        {
            yield return StartCoroutine(RemoteTurn(seat));
            yield break;
        }

        yield return new WaitForSeconds(0.7f);
        if (state != State.Turn || currentSeat != seat) yield break;

        if (hand[seat].Count == 0)
            StartCoroutine(DeckOnlySeq(seat, () => AfterAction(seat)));
        else
        {
            var card = GoStopAI.ChooseCard(hand[seat], field);
            StartCoroutine(PlaySeq(seat, card, GoStopAI.ShouldShake(), () => AfterAction(seat)));
        }
    }

    /// <summary>원격 좌석의 한 턴 — 카드를 낼지(흔들기 여부 포함) 폭탄
    /// 크레딧으로 덱만 넘길지 실제 사람(다른 기기)의 응답을 기다린다.
    /// 응답이 오면 그 뒤 판정은 로컬 플레이어·AI와 완전히 같은
    /// PlaySeq/DeckOnlySeq를 그대로 탄다 — "누가 카드를 골랐는지"만
    /// 다르고 판정 자체는 이 함수가 끝나는 순간부터 동일한 코드다.</summary>
    IEnumerator RemoteTurn(int seat)
    {
        if (state != State.Turn || currentSeat != seat) yield break;

        if (hand[seat].Count == 0)
        {
            StartCoroutine(DeckOnlySeq(seat, () => AfterAction(seat)));
            yield break;
        }

        GoStopNetMessage msg = null;
        yield return StartCoroutine(WaitForRemoteMessage(seat,
            m => m.type == GoStopNetMessage.Type.PlayCard
              || (m.type == GoStopNetMessage.Type.BombSkip && bombCredits[seat] > 0),
            m => msg = m));

        if (msg.type == GoStopNetMessage.Type.BombSkip)
        {
            bombCredits[seat]--;
            StartCoroutine(DeckOnlySeq(seat, () => AfterAction(seat)));
            yield break;
        }

        // 게스트가 보낸 건 스냅샷에서 새로 디코딩한 별개의 HwatuCard
        // 객체다 — GoStopRules 내부는 리스트 안 참조 동일성으로 카드를
        // 다루므로(hand.Remove(card) 등) 반드시 손패 안의 진짜 인스턴스를
        // 찾아 써야 한다. 손패에 없는 카드 이름이 오면(오염된/오래된
        // 메시지) 판이 멈추지 않도록 AI 선택으로 방어한다.
        var decoded = GoStopDeck.Decode(msg.cardId);
        var card = decoded != null ? hand[seat].FirstOrDefault(c => c.spriteName == decoded.spriteName) : null;
        if (card == null) card = GoStopAI.ChooseCard(hand[seat], field);
        StartCoroutine(PlaySeq(seat, card, msg.boolValue, () => AfterAction(seat)));
    }

    // ── 종료 ─────────────────────────────────────────────
    /// <summary>
    /// 손패뿐 아니라 <b>더미도 같이 비어야</b> 판이 끝난다. 4인 딜은 조커 2장을
    /// 더미에 끼우고 쉬는 좌석의 손패 7장을 통째로 버리는 만큼, 활성 손패
    /// 총합(7×3=21)보다 더미가 작다(14+조커2=16) — 폭탄은 손패 3장을 한
    /// 번에 쓰면서 그 턴의 더미는 안 넘기므로, 폭탄이 여러 번 겹치면
    /// "손패는 다 냈는데 더미가 아직 남아있는" 상태가 될 수 있다. 예전엔
    /// 손패만 보고 바로 끝내서 이 더미가 그대로 안 쓰인 채 남았다("패
    /// 짝수가 맞으면 더미가 하나도 안 남아야 하는데 남는다"는 신고) —
    /// 손패가 없는 활성 좌석은 AdvanceTurn/DelayedAiTurn이 이미 자동으로
    /// "덱만 넘기기"로 돌려주므로, 여기서 더미 조건만 추가하면 더미가
    /// 완전히 소진될 때까지 자연히 계속 돈다.
    /// </summary>
    bool CheckHandsEmpty()
    {
        if (ActiveSeats().Any(s => hand[s].Count > 0)) return false;
        if (drawPile.Count > 0) return false;

        int bestSeat = -1, bestScore = -1;
        foreach (var s in ActiveSeats())
        {
            int sc = GoStopRules.CalcScore(captured[s], sweeps[s]).Total;
            if (sc > bestScore) { bestScore = sc; bestSeat = s; }
        }

        if (bestScore < CAPTURE_LINE) { EndGame(-1); return true; }
        EndGame(bestSeat);
        return true;
    }

    /// <param name="winnerSeat">-1이면 나가리.</param>
    void EndGame(int winnerSeat, int? fixedBaseScore = null, int extraMultiplier = 1)
    {
        state = State.GameOver;

        // 다음 판 선(딜러)은 이번 판 승자다(사용자 확인 규칙) — 승패가 갈리는
        // 모든 경로(일반 승리·총통·쓰리뻑)가 이 한 줄로 커버된다. 나가리
        // (winnerSeat<0)면 건드리지 않아서 자동으로 "선 유지"가 된다. 쉬는
        // 좌석은 ActiveSeats() 밖이라 애초에 winnerSeat가 될 수 없으므로
        // "이번 판에 쉰 사람이 다음 판 선이 되는" 상황은 생기지 않는다.
        if (winnerSeat >= 0) dealerSeat = winnerSeat;

        if (winnerSeat < 0)
        {
            stakeMultiplier *= 2;
            pendingPayout = null; // 나가리는 승자가 없어 분석할 점수 자체가 없다
            GoStopAudio.Instance?.Nagari();
            ui?.ShowOverlay(new Color(.6f, .6f, .6f), "나가리", "-",
                $"아무도 {CAPTURE_LINE}점을 못 넘겼습니다 · 다음 판 판돈 {stakeMultiplier}배",
                "다시 시작", NewGame, "타이틀", GoToTitle);
            if (isNetworkHost) BroadcastGameOverState(true, -1, 0, -1, null);
            return;
        }

        GoStopAudio.Instance?.Money();
        if (winnerSeat == PLAYER_SEAT) { GoStopAudio.Instance?.Win(); PlayWinConfettiFX(); }
        else GoStopAudio.Instance?.Lose();

        // 쉬는 좌석은 이번 판 캡처가 없으므로(광팔이로 이미 정산 끝) 정산
        // 대상에서 제외한다 — 낀 사람만 이기고 지는 판이다.
        var loserSeats = ActiveSeats().Where(s => s != winnerSeat).ToList();
        var loserCaptured = loserSeats.Select(s => captured[s]).ToList();

        // 독박(고박) — 패자 중 이번 판에 고를 부른 적 있는 사람이 정확히 한 명이면
        // 그 사람이 전원분을 몰아서 낸다. 여럿이거나 아무도 안 불렀으면
        // 특정할 대상이 없다고 보고 각자 자기 몫만 낸다(단순화 — 문서 참고).
        var goCallers = loserSeats.Where(s => calledGo[s]).ToList();
        int dokbakIdx = goCallers.Count == 1 ? loserSeats.IndexOf(goCallers[0]) : -1;

        var payout = GoStopRules.FinalScoreMulti(captured[winnerSeat], sweeps[winnerSeat], goCount[winnerSeat],
            heundeulCount[winnerSeat], bombCount[winnerSeat], loserCaptured, WON_PER_POINT,
            dokbakIdx, fixedBaseScore, extraMultiplier);
        pendingPayout = payout;
        pendingWinnerSeat = winnerSeat;
        pendingLoserSeats = loserSeats;

        for (int i = 0; i < loserSeats.Count; i++)
        {
            int amount = Mathf.Min(payout.amounts[i], money[loserSeats[i]]);
            money[loserSeats[i]] -= amount;
            money[winnerSeat] += amount;
            FlyMoneyFX(loserSeats[i], winnerSeat, amount);
        }
        stakeMultiplier = 1;

        int finalScore = payout.baseTotal;
        if (winnerSeat == PLAYER_SEAT && finalScore > PlayerPrefs.GetInt(BestKey, 0))
        {
            PlayerPrefs.SetInt(BestKey, finalScore);
            ui?.SetBest(finalScore);
        }

        // 2026-08-18: 예전엔 누구든 0원 이하가 되면 세션이 끝났는데("다시
        // 시작"이 의미 없다고 봤었다), 사용자 요청으로 대신 REFILL_MONEY를
        // 채워서 계속 이어가는 쪽으로 바꿨다. 4인이라 여러 좌석이 동시에
        // 0원 이하가 될 수 있어(광팔이·독박으로 몰아 냈을 때) 전 좌석을
        // 독립적으로 확인한다.
        var refilledSeats = RefillIfBankrupt();
        // 네트워크 판은 로컬 저장을 안 한다(Start()와 같은 이유 — 매판
        // 접속하는 사람이 달라질 수 있어 "이 기기의 좌석 N 잔액"이라는
        // 개념이 안 맞는다).
        if (!isNetworkHost && !isNetworkGuest) SaveMoney();

        string title = winnerSeat == PLAYER_SEAT ? "승리!" : $"{SeatName(winnerSeat)} 승리";
        Color col = winnerSeat == PLAYER_SEAT ? new Color(.93f, .73f, .18f) : new Color(.55f, .55f, .60f);
        string sub = dokbakIdx >= 0 ? $"{SeatName(loserSeats[dokbakIdx])} 독박 · 내 머니 {money[PLAYER_SEAT]:N0}원"
                                     : $"내 머니 {money[PLAYER_SEAT]:N0}원";
        if (refilledSeats.Count > 0)
        {
            string names = string.Join(", ", refilledSeats.Select(s => $"{SeatName(s)}(올인 {allInCount[s]}회)"));
            sub += $" · 잔액 소진 → 5만원 재충전: {names}";
        }

        ui?.SetScore(money[PLAYER_SEAT]); // 상단 HUD의 SCORE는 판점이 아니라 내 보유 머니를 보여준다(사용자 요청)
        ui?.ShowOverlay(col, title, finalScore.ToString(), sub,
            "다시 시작", NewGame, "타이틀", GoToTitle, "점수 상세", ShowScoreDetail);

        if (isNetworkHost)
            BroadcastGameOverState(false, winnerSeat, finalScore, dokbakIdx >= 0 ? loserSeats[dokbakIdx] : -1, refilledSeats.ToArray());
    }

}
