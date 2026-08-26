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
    [SerializeField] GoStopUIManager ui; // 2026-08-22: 공용 GameUIManager에서 분리 — GoStopUI.prefab 전용

    // 2026-08-24: "오브젝트 참조할 때 Find 쓰지 말고 SerializeField로
    // 선언된 변수로 참조해달라" 요청 — 그동안 ApplySeatVisibility/
    // BuildInfoBlock/BuildEdgeSeatBlock/GetOrCreateContainer가 매번
    // transform.Find(이름)으로 씬 오브젝트를 찾던 것을, 인스펙터에서
    // 미리 연결해 둔 참조로 바꿨다. 비워두면(예: 이 구조가 아직 없는
    // 씬) 기존처럼 코드가 새로 생성하는 폴백은 그대로 유지한다 — "씬에
    // 있으면 재사용, 없으면 생성" 원칙 자체는 안 바뀌고 "있는지 확인하는
    // 방법"만 Find→SerializeField로 바뀐 것.
    [SerializeField] RectTransform leftSeatRef, rightSeatRef, topSeatRef, mySeatRef;
    [SerializeField] RectTransform back4Ref, cap4Ref; // TopSeat 안쪽(2인 전용 상대 자리)
    [SerializeField] RectTransform fieldAreaRef, drawPileAreaRef, playerCapAreaRef, handAreaRef;
    // StatusBox0~3 — ApplySeatVisibility(StatusBox2 위치 조정)와 BuildInfoBlock이
    // 같은 배열을 같이 쓴다(같은 오브젝트를 두 번 따로 참조하면 어긋날 수 있어서 하나로 통일).
    [SerializeField] RectTransform[] statusBoxRefs = new RectTransform[SEATS_MAX];
    [SerializeField] RectTransform[] backSeatRefs = new RectTransform[SEATS_MAX];  // Back1/Back3(1·3만 사용)
    [SerializeField] RectTransform[] capSeatRefs = new RectTransform[SEATS_MAX];   // Cap1/Cap3(1·3만 사용)

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
    // 2026-08-23: 씬 통합(design.md 게임모드 자동결정/다운그레이드 선행 작업) —
    // SEATS=2(맞고)도 이 엔진 안에서 돌릴 수 있게 확장했다. 2는 이제
    // GoStopGame.cs(2인 전용 클래스)가 아니라 이 클래스가 직접 처리한다.
    public void SetSeatCount(int n)
    {
        if (n == 2 || n == 3 || n == 4) SEATS = n;
    }
    // 맞고(2인)는 7점부터지만 정식 고스톱(3~4인)은 3점부터 난다 — 사용자
    // 확인 규칙. SEATS==2일 때만 맞고 기준(7)을 쓴다 — GoStopRules.CAPTURE_LINE
    // (2인 전용 파일 GoStopGame.cs가 쓰던 상수, 값은 7로 동일)과는 별개다.
    int CaptureLine => SEATS == 2 ? 7 : 3;
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
    // 2026-08-23(design.md §49.2): 예전엔 고정 상수였다 — 이제 네트워크
    // 방에서는 호스트가 Home 화면에서 정한 값을 쓴다(Awake()에서 읽어옴).
    // 오프라인(vs AI) 플레이는 이 UI 대상이 아니라서 계속 기본값(100원).
    int WON_PER_POINT = 100;
    // 광팔이 — 사용자 확인 규칙: 광이나 쌍피 계열(쌍피·9월 열끗·보너스 조커)
    // 한 장당 "1점 가격"씩을, 2·3번째(선을 제외한, 나를 밀어낸 두 명)에게서
    // "각각" 받는다(2인이 각자 내므로 카드 한 장당 실수령은 1점 가격의
    // 2배). 선(딜러)은 이 정산에서 빠진다 — 딜러는 밀어낸 쪽이 아니다.
    // 2026-08-23: 예전엔 GWANG_SALE_WON_PER_CARD라는 별도 상수(우연히
    // WON_PER_POINT와 같은 100)를 썼는데, design.md §8이 "광팔이 단가 =
    // 1점 가격"이라고 명시하고 있어 별도 상수를 없애고 WON_PER_POINT를
    // 그대로 쓴다 — 안 그러면 호스트가 1점 가격을 바꿔도 광팔이 단가는
    // 그대로 100원에 고정된 채 어긋난다.
    // 2026-08-18: "다시 시작해도 이전 잔액으로" 요청 — PlayerPrefs에 좌석별로
    // 영구 저장한다(2인판과 같은 패턴). 2026-08-23(design.md §49.4 확정):
    // 0원 이하가 되면 예전엔 5만원을 리필해서 계속 이어갔지만, 이번
    // 통합 작업에서 그 규칙을 폐기했다 — BankruptSeats() 참고.
    static string MoneyKey(int s) => "GoStop4P_Money_" + s;
    static string AllInKey(int s) => "GoStop4P_AllIn_" + s;
    readonly int[] money = new int[SEATS_MAX];
    readonly int[] allInCount = new int[SEATS_MAX]; // 이제 "리필 횟수"가 아니라 "파산으로 세션이 끝난 횟수"
    int stakeMultiplier = 1; // 나가리마다 2배, 결판나면 1로 리셋 (Start()에서만 초기화)

    // 2026-08-22: "결과 화면에 자금 상세(시작 자금·이번 판 변동·현재 잔액)를
    // 보여달라" 요청 — EndGame이 정산을 적용하기 직전의 좌석별 잔액 스냅샷.
    // ShowScoreDetail은 버튼을 눌러야 나중에 실행되므로 그때는 이미 정산이
    // 끝난 money[]만 남아있다 — "시작 자금"을 보여주려면 여기 따로 남겨둬야
    // 한다(2인판 GoStopGame.cs의 pendingMoneyBeforePlayer/Ai와 같은 이유).
    readonly int[] pendingMoneyBefore = new int[SEATS_MAX];

    void SaveMoney()
    {
        for (int s = 0; s < SEATS; s++)
        {
            PlayerPrefs.SetInt(MoneyKey(s), money[s]);
            PlayerPrefs.SetInt(AllInKey(s), allInCount[s]);
        }
        PlayerPrefs.Save();
    }

    /// <summary>2026-08-23(design.md §49.4 확정): 이번 판 정산 후 0원 이하가
    /// 된 좌석 목록을 돌려준다. 예전엔 REFILL_MONEY로 채워 계속 이어갔지만
    /// (2026-08-18 확정), 이번 통합 작업에서 그 규칙을 폐기했다.
    /// 2026-08-23(2차): 씬 통합(GoStop3PGame이 2/3/4인을 전부 처리)이
    /// 끝나서 "4인→3인→2인 자동 다운그레이드"가 오프라인(vs AI)에선
    /// 실제로 가능해졌다 — 아래 CanDowngrade/ApplyDowngrade 참고. 네트워크는
    /// 아직 미구현(연결된 각 게스트에게 새 좌석 번호를 재배정하는
    /// 프로토콜이 필요해서 범위 밖으로 남겼다 — 최종 보고서 참고), 나
    /// 자신이 파산했거나 이미 2인이면 다운그레이드로 못 내려가므로 그
    /// 판을 끝으로 세션을 종료한다(호출부 참고).</summary>
    List<int> BankruptSeats()
    {
        var bankrupt = new List<int>();
        for (int s = 0; s < SEATS; s++) if (money[s] <= 0) bankrupt.Add(s);
        return bankrupt;
    }

    /// <summary>파산한 좌석이 있고, 그게 나(사람)가 아니고, 아직 2인보다
    /// 위(더 내려갈 데가 있음)라면 자동 다운그레이드가 가능하다 —
    /// 오프라인(vs AI) 전용. 내가 파산했으면 다운그레이드로 구제할 방법이
    /// 없다(계속할 사람 자체가 없다는 뜻이라 세션 종료가 맞다). 네트워크는
    /// 아직 미구현 — BankruptSeats() 문서 참고.</summary>
    bool CanDowngrade(List<int> bankruptSeats) =>
        bankruptSeats.Count > 0 && !isNetworkHost && !isNetworkGuest &&
        !bankruptSeats.Contains(PLAYER_SEAT) && SEATS > 2;

    /// <summary>2026-08-23(design.md §49.4): 파산한 좌석(들)을 빼고 남은
    /// 좌석의 잔액을 그대로 들고 새 인원수로 재구성한다 — "돈을 다 잃은
    /// 사람만 나가고, 남은 사람들은 가진 돈 그대로 계속"이라는 규칙이다
    /// (다운그레이드라고 전원 잔액을 초기화하지 않는다 — 그건 "세션
    /// 종료"쪽 규칙과 다르다). 오프라인 전용: AI 좌석은 익명이라(고유
    /// 정체성이 없다) 몇 번 좌석이 빠지든 그냥 나머지를 0번부터 다시
    /// 채워 넣으면 된다 — 나(PLAYER_SEAT)는 CanDowngrade가 이미 "파산
    /// 안 했음"을 보장해서 항상 새 0번으로 살아남는다.</summary>
    void ApplyDowngrade(List<int> bankruptSeats)
    {
        var survivorMoney = new List<int>();
        var survivorAllIn = new List<int>();
        for (int s = 0; s < SEATS; s++)
        {
            if (bankruptSeats.Contains(s)) continue;
            survivorMoney.Add(money[s]);
            survivorAllIn.Add(allInCount[s]);
        }
        int newSeats = survivorMoney.Count;
        for (int i = 0; i < newSeats; i++) { money[i] = survivorMoney[i]; allInCount[i] = survivorAllIn[i]; }
        SetSeatCount(newSeats);
        dealerSeat = 0; // 다운그레이드 직후엔 선을 단순하게 나로 리셋한다(누가 이겼는지와 무관하게)
        SaveMoney();
        ApplySeatVisibility(ui.ContentArea); // Left/Right/TopSeat 표시를 새 인원수에 맞게 다시 계산
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
    // 비상 시스템 — (좌석, 세트 인덱스[0=고도리 1=홍단 2=초단 3=청단]) 조합이
    // 이번 판에 이미 한 번 발동했는지. 세트 카드는 (피와 달리) 획득 이후
    // 다시 뺏기지 않으므로 have==2에서 3으로만 진행하지 되돌아가지
    // 않는다 — 한 번 발동하면 그 판 내내 다시 안 울려도 된다.
    readonly HashSet<(int seat, int setIdx)> emergencyFired = new();
    // 2026-08-25 — 족보 "완성" 이펙트(비상과 별개)가 이번 판에 이미 한 번
    // 발동했는지. 뻑/폭탄 등으로 카드 여러 장이 한 번에 들어오면 have가
    // 2를 거치지 않고 곧장 3으로 뛸 수 있어서(비상이 안 뜬 채로 완성)
    // emergencyFired에 얹어 계산하지 않고 완전히 독립적으로 추적한다.
    readonly HashSet<(int seat, int setIdx)> achievedFired = new();
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
    // 금액)/우(아이콘) 반분할로 재설계하며 추가했다. 2026-08-24부터는
    // GoStopStatusBoxView 프리팹이 배지 6종을 고정 슬롯으로 미리 갖고
    // 있어서(아래 statusBoxView) 이 필드는 그 프리팹의 BadgeArea 자식을
    // 그대로 가리키기만 한다 — 더 이상 ClearChildren 대상이 아니다.
    RectTransform[] badgeArea = new RectTransform[SEATS_MAX];
    // 2026-08-24: BuildInfoBlock이 인스턴스화한 프리팹 뷰 — DrawBadgeStrip이
    // 이걸 통해 배지 상태(선/광박/멍박/피박/흔들기/뻑)만 갱신한다(재생성 안 함).
    GoStopStatusBoxView[] statusBoxView = new GoStopStatusBoxView[SEATS_MAX];

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

    /// <summary>design.md §50.2 — 재접속 유예 시간을 넘겨 영구 이탈이
    /// 확정된 좌석(호스트 전용, <see cref="OnGuestGoneForGood"/>가 표시).
    /// 한 번 true가 되면 이번 네트워크 세션 내내 그대로다(그 좌석이
    /// 다시 쓰일 일은 없다 — 다음에 접속하는 사람은 다운그레이드 이후
    /// 압축된 새 번호를 받는다). <see cref="IsRemoteSeat"/>가 이 값도
    /// 확인해서, 이후 이 좌석의 모든 결정이 자동으로 AI 경로로 떨어진다.</summary>
    readonly bool[] permaGoneNetworkSeat = new bool[SEATS_MAX];

    /// <summary>이 좌석이 "원격 사람"인지 — 호스트일 때만 의미가 있다.
    /// 네트워크로 시작된 판은 호스트 자신을 제외한 모든 좌석이 접속한
    /// 게스트다(AI와 섞이지 않는다 — 로비가 인원이 다 찰 때까지 시작을
    /// 안 받아준다). 게스트 쪽에서는 항상 false — 게스트는 자기 자신
    /// (PLAYER_SEAT) 말고는 어떤 좌석도 직접 판정하지 않는다. 영구
    /// 이탈이 확정된 좌석(<see cref="permaGoneNetworkSeat"/>)도 false —
    /// 그 순간부터 AI가 대신한다(design.md §50.2).</summary>
    bool IsRemoteSeat(int seat) => isNetworkHost && seat != PLAYER_SEAT && seat >= 0 && seat < SEATS && !permaGoneNetworkSeat[seat];

    /// <summary>design.md §50.1 — 참가 여부/Go-Stop/국열끗/카드 선택(턴) 등
    /// 원격 좌석의 응답을 기다리는 모든 지점이 공유하는 무응답 제한
    /// 시간. 넘기면 <see cref="WaitForRemoteMessage"/>가 <c>null</c>을
    /// 넘겨주고, 호출부가 각자의 기본값(불참/스톱/쌍피/첫 번째 카드)을
    /// 적용한다. 연결이 아예 끊긴 좌석(재접속 유예 중, design.md §50.2)도
    /// 이 시간 안엔 절대 응답할 수 없으므로 자동으로 같은 경로를 탄다 —
    /// 유예와 이 타임아웃을 따로 조율할 필요가 없다. 정확한 초 단위는
    /// 이 프로젝트에 선례가 없어 새로 정한 값이다(너무 짧으면 정상적으로
    /// 고민 중인 사람도 잘리고, 너무 길면 나머지 인원이 오래 기다린다).</summary>
    const float REMOTE_INPUT_TIMEOUT_SECONDS = 25f;

    /// <summary>호스트 쪽에서 특정 원격 좌석의 다음 메시지를 기다린다.
    /// <paramref name="accept"/>가 null이면 그 좌석에서 오는 아무 메시지나
    /// 받는다(예: 이번 턴엔 PlayCard 또는 BombSkip 둘 다 유효한 응답).
    /// 받는 즉시 구독을 해제하므로 이후 같은 좌석의 낡은/중복 메시지는
    /// 자동으로 무시된다. <see cref="REMOTE_INPUT_TIMEOUT_SECONDS"/> 안에
    /// 응답이 없으면 <paramref name="onReceived"/>에 <c>null</c>을 넘긴다 —
    /// 호출부가 반드시 null을 확인해서 자기 몫의 기본값을 적용해야 한다.</summary>
    IEnumerator WaitForRemoteMessage(int seat, System.Func<GoStopNetMessage, bool> accept, System.Action<GoStopNetMessage> onReceived)
    {
        GoStopNetMessage received = null;
        void Handler(int fromSeat, GoStopNetMessage msg)
        {
            if (fromSeat == seat && (accept == null || accept(msg))) received = msg;
        }
        GoStopNetLobby.Instance.OnGameMessage += Handler;
        float deadline = Time.unscaledTime + REMOTE_INPUT_TIMEOUT_SECONDS;
        yield return new WaitUntil(() => received != null || Time.unscaledTime >= deadline);
        GoStopNetLobby.Instance.OnGameMessage -= Handler;
        onReceived(received); // 타임아웃이면 received==null 그대로 넘어간다
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
                $"아무도 {CaptureLine}점을 못 넘겼습니다 · 다음 판 판돈 {snap.gameOverStakeMultiplier}배 (호스트가 다시 시작합니다)",
                "타이틀", GoToTitle);
            return;
        }

        int winnerSeat = snap.gameOverWinnerSeat;
        string title = winnerSeat == PLAYER_SEAT ? "승리!" : $"{SeatName(winnerSeat)} 승리";
        Color col = winnerSeat == PLAYER_SEAT ? new Color(.93f, .73f, .18f) : new Color(.55f, .55f, .60f);
        string sub = snap.gameOverDokbakSeat >= 0
            ? $"{SeatName(snap.gameOverDokbakSeat)} 독박 · 내 머니 {money[PLAYER_SEAT]:N0}원"
            : $"내 머니 {money[PLAYER_SEAT]:N0}원";
        // gameOverRefilledSeats 필드명은 그대로 재사용하지만(스냅샷 구조체 변경
        // 회피), 2026-08-23부터는 "리필된 좌석"이 아니라 "파산해서 세션이
        // 끝난 좌석" 목록이다 — design.md §49.4 확정.
        if (snap.gameOverRefilledSeats != null && snap.gameOverRefilledSeats.Length > 0)
        {
            string names = string.Join(", ", snap.gameOverRefilledSeats.Select(s => SeatName(s)));
            sub += $" · {names} 잔액을 모두 잃어 이 판을 끝으로 세션을 종료합니다";
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

    /// <summary>오프라인(vs AI) 진입 전용 — 타이틀의 인원수 선택 팝업
    /// (GoStopModeChoiceUI)이 씬을 열기 직전에 세팅한다. static이라 씬
    /// 전환을 넘어 살아남는다(로비 싱글톤이 없는 오프라인 경로라 이
    /// 방법으로 대신한다 — 2026-08-23, 씬 통합의 일부). Awake()가 읽는
    /// 즉시 null로 비워서, 나중에 이 씬 안에서 "다시 시작"을 눌러도
    /// 이미 확정된 SEATS를 실수로 다시 덮어쓰지 않는다.</summary>
    public static int? PendingOfflineSeatCount;

    /// <summary>true면 이번 씬 진입이 이미 인원수를 알고 들어왔다는 뜻
    /// (네트워크 로비 또는 타이틀의 인원수 선택 팝업). false면 씬을
    /// 직접 열었다는 뜻(에디터에서 바로 Play 등, 주로 테스트) — 이 경우
    /// Start()가 곧장 4인으로 시작하지 않고 인원수 선택 화면을 먼저
    /// 띄운다(2026-08-23, 테스트 편의 요청).</summary>
    bool seatCountPreset;

    void Awake()
    {
        var lobby = GoStopNetLobby.Instance;
        if (lobby != null && lobby.PlayerCount > 0)
        {
            SetSeatCount(lobby.PlayerCount);
            SetMySeat(lobby.MySeat); // 호스트는 항상 0이라 사실상 no-op, 게스트는 1~3
            isNetworkHost = lobby.IsHost;
            isNetworkGuest = lobby.IsGuest;
            WON_PER_POINT = lobby.PointPrice; // design.md §49.2 — 호스트가 Home에서 정한 값(게스트는 StartGame으로 전달받은 값)
            lobby.OnGameMessage += OnNetGameMessage;
            if (isNetworkHost)
            {
                lobby.OnGuestLeftDuringGame += OnGuestLeftDuringGame;
                lobby.OnGuestGoneForGood += OnGuestGoneForGoodHandler;
                lobby.OnGuestReconnected += OnGuestReconnectedHandler;
            }
            if (isNetworkGuest)
            {
                lobby.OnDisconnected += OnHostDisconnected;
                lobby.OnReconnecting += OnReconnectingHandler;
                lobby.OnReconnected += OnReconnectedHandler;
                lobby.OnSeatReassigned += OnSeatReassignedHandler;
            }
            seatCountPreset = true;
        }
        else if (PendingOfflineSeatCount.HasValue)
        {
            SetSeatCount(PendingOfflineSeatCount.Value);
            PendingOfflineSeatCount = null;
            seatCountPreset = true;
        }
    }

    /// <summary>호스트 전용 — 접속해 있던 게스트 한 명의 소켓이 판 도중
    /// 끊겼다. design.md §50.2 확장 전에는 여기서 곧바로 판을 끝냈지만,
    /// 지금은 재접속 유예 중이라는 뜻일 뿐이다 — 판은 계속 진행되고
    /// (§50.1 타임아웃이 이 좌석의 턴/결정을 자동으로 대신 처리한다),
    /// 여기서는 안내 토스트만 띄운다. 실제로 "이 좌석을 포기한다"는
    /// 판단은 유예가 끝난 뒤 <see cref="OnGuestGoneForGoodHandler"/>에서
    /// 내린다.</summary>
    void OnGuestLeftDuringGame(int seat)
    {
        if (state == State.GameOver) return;
        if (seat < 0 || seat >= SEATS) return;
        ShowTimedToast($"{SeatName(seat)} 연결 끊김 — 재접속을 기다립니다");
    }

    /// <summary>호스트 전용(design.md §50.2) — 판 도중 끊긴 좌석이 재접속
    /// 유예 시간을 넘겨 영구 이탈이 확정됐다. 이제부터 이 좌석은 AI가
    /// 대신한다(<see cref="IsRemoteSeat"/>가 자동으로 걸러준다) — 이번
    /// 판은 그대로 마저 진행하고, 판이 끝나는 시점(<see cref="EndGame"/>)에
    /// 좌석을 압축(다운그레이드)해서 남은 인원으로 다음 판을 잇는다(§49.4
    /// 네트워크 확장). 그 자리에서 즉시 압축하지 않는 이유 — 손패/필드/
    /// 캡처가 전부 좌석 번호로 인덱싱돼 있어, 판 도중에 번호를 당기면
    /// 진행 중인 상태 전체를 다시 매핑해야 한다(오프라인 다운그레이드도
    /// 원래 EndGame에서만 일어나는 것과 같은 이유 — ApplyDowngrade 문서
    /// 참고).</summary>
    void OnGuestGoneForGoodHandler(int seat)
    {
        if (seat < 0 || seat >= SEATS || permaGoneNetworkSeat[seat]) return;
        permaGoneNetworkSeat[seat] = true;
        ShowTimedToast($"{SeatName(seat)} 재접속 실패 — 이번 판은 AI가 대신하고, 다음 판부터 인원이 줄어듭니다");
    }

    /// <summary>호스트 전용(design.md §50.2) — 유예 중이던 좌석이 돌아왔다.
    /// 그동안 놓친 StateSync가 여러 번 있었을 테니, 지금 상태 전체를
    /// 그 좌석에게 즉시 다시 보내 화면을 바로 복원시킨다(다음 자연스러운
    /// 이벤트가 생길 때까지 기다리게 두지 않는다).</summary>
    void OnGuestReconnectedHandler(int seat)
    {
        if (seat < 0 || seat >= SEATS) return;
        ShowTimedToast($"{SeatName(seat)} 재접속했습니다");
        SendTargetedPrompt(seat, _ => { }); // configure 없이 정규 스냅샷만 — 최신 상태로 즉시 복원
    }

    /// <summary>게스트 전용 — 호스트와의 TCP 연결 자체가 끊겼다. design.md
    /// §50.2 확장 후로는 <c>GoStopNetLobby</c>가 먼저 자동 재접속을
    /// 시도하고(유예 시간 동안), 그마저 실패해야 이 콜백이 최종적으로
    /// 불린다 — 그래서 지금은 정말 더 기다려도 소용없다는 뜻이라 바로
    /// 안내하고 타이틀로 돌려보낸다.</summary>
    void OnHostDisconnected(string reason)
    {
        if (state == State.GameOver) return;
        state = State.GameOver;
        ui?.ShowOverlay(new Color(.8f, .35f, .3f), "연결 끊김", "-",
            "호스트와의 연결이 끊어졌습니다.", "타이틀", GoToTitle);
    }

    /// <summary>게스트 전용(design.md §50.2) — 자동 재접속 시도가 시작됐다.
    /// 게임오버 오버레이처럼 화면을 통째로 가리진 않는다(§50.1 타임아웃이
    /// 알아서 진행시켜 주므로 다른 사람 화면은 안 멈춘다) — 가벼운
    /// 토스트로만 알린다.</summary>
    void OnReconnectingHandler() => ShowTimedToast("연결이 끊어졌습니다 — 재접속 시도 중...");

    /// <summary>게스트 전용 — 자동 재접속에 성공했다. 곧이어 호스트가
    /// 최신 StateSync를 보내오므로(OnGuestReconnectedHandler) 별도로
    /// 화면을 다시 그릴 필요는 없다 — 안내만 띄운다.</summary>
    void OnReconnectedHandler() => ShowTimedToast("재접속 완료");

    /// <summary>게스트 전용(design.md §49.4 네트워크 확장) — 다른 좌석이
    /// 영구 이탈해서 호스트가 좌석을 압축했다. 씬을 다시 로드하지 않고
    /// 제자리에서 내 좌석 번호·인원수만 갱신한다 — 곧이어 오는 StateSync가
    /// 나머지(손패·필드 등)를 정상적으로 채워준다.</summary>
    void OnSeatReassignedHandler(int newSeat, int newPlayerCount)
    {
        SetMySeat(newSeat);
        SetSeatCount(newPlayerCount);
        ApplySeatVisibility(ui.ContentArea);
        ShowTimedToast("다른 자리가 정리되어 좌석이 조정됐습니다");
    }

    void Start()
    {
        Screen.orientation = ScreenOrientation.LandscapeLeft;
        // GameUI는 씬마다 별도 인스턴스라(공유 싱글톤이 아니다) 이 인스턴스의
        // CanvasScaler만 가로용 참조 해상도로 바꿔도 다른 7개 게임·2인 맞고에는
        // 영향이 없다. 세로 참조(1080×1920)를 가로 물리 화면에 그대로 쓰면
        // matchWidthOrHeight 계산이 어긋나 스케일이 크게 틀어진다.
        var scaler = ui ? ui.GetComponent<CanvasScaler>() : null;
        if (scaler)
        {
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            // 2026-08-25 — "다른 해상도에서 화면 밖으로 나가거나 겹친다"
            // 신고로 발견: 기본 MatchWidthOrHeight 모드는 화면비가 16:9에서
            // 벗어나면 캔버스의 *논리적* 크기 자체가 1920×1080에서 어긋난다
            // (실측: 2587×1227 화면에서 논리 캔버스가 2118×979로 나왔다 —
            // 세로가 1080보다 좁아짐). 이 필드/좌석/Cap 레이아웃은 전부
            // "캔버스가 정확히 1920×1080"이라는 전제로 절대 픽셀 좌표를
            // 하드코딩해 뒀으므로, 세로가 줄어드는 화면비에서는 아래쪽
            // 요소가 겹치거나 화면 밖으로 밀려난다.
            // Expand 모드는 반대로 "캔버스가 절대 기준보다 작아지지 않는다"
            // (width/height 스케일 중 더 작은 쪽을 쓴다)를 보장한다 —
            // 즉 어떤 화면비든 논리 캔버스가 항상 1920×1080 *이상*이라
            // 이 레이아웃이 가정한 공간보다 여유가 부족해질 수 없다.
            // 트레이드오프: 16:9가 아닌 화면에서는 좌우(넓은 화면) 또는
            // 상하(좁은 화면)에 배경이 조금 더 보인다 — 화면을 완전히
            // 꽉 채우진 않지만, 겹침·화면 밖 이탈은 구조적으로 불가능해진다.
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
        }
        // "상단 UI가 공간을 많이 차지한다, 나가기 버튼만 있으면 된다" 요청 —
        // 공용 HUD(제목·점수·NEW·뒤로 버튼 바)를 통째로 끄고 ContentArea가
        // 그 116px까지 전부 쓰도록 늘린다. 나가기는 BuildStaticUI에서 직접
        // 만드는 작은 버튼 하나로 대체한다.
        ui?.SetHudVisible(false);
        // 네트워크 게스트는 새 판을 직접 못 시작한다 — 언제 다시 시작할지는
        // 호스트만 결정한다(호스트가 다음 판을 시작하면 그 StateSync를
        // 받아 화면이 알아서 바뀐다).
        ui?.SetNewGameAction(isNetworkGuest ? (System.Action)null : NewGame);
        ui?.SetTitle(isNetworkHost || isNetworkGuest ? "고스톱 (네트워크)" : SeatCountTitle());
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

        // 게스트는 여기서 아무것도 시작 안 한다 — 호스트가 첫 StateSync를
        // 보내오면 OnNetGameMessage → ApplyNetworkSnapshot이 손패를 채우고
        // 화면을 그린다. BuildStaticUI는 그 StateSync를 받을 그릇을 미리
        // 지어두기 위해 필요하다(게스트는 이미 lobby가 SEATS를 정해줬으므로
        // seatCountPreset이 항상 true다).
        if (isNetworkGuest)
        {
            BuildStaticUI();
            return;
        }

        // 2026-08-23: 씬을 직접 열면(에디터에서 바로 Play 등, 주로 테스트)
        // 로비도 없고 타이틀의 인원수 선택도 안 거쳐서 seatCountPreset이
        // false다 — 이 경우 곧장 4인(SEATS 기본값)으로 시작하는 대신
        // 인원수 선택 화면부터 띄운다(테스트 편의 요청). BuildStaticUI는
        // SEATS 값에 따라 LeftSeat/RightSeat/TopSeat on-off를 정하므로,
        // 인원수가 정해지기 전엔 아직 부르면 안 된다.
        if (seatCountPreset)
        {
            BuildStaticUI();
            NewGame();
        }
        else
        {
            // 2026-08-24: "모드 선택할 때 left/right/top/my seat를 전부
            // 끈 상태에서 시작해달라, 초기 UI가 데이터 없이 세팅되니
            // 어색하다" 요청 — 이 시점엔 아직 BuildStaticUI()/
            // ApplySeatVisibility()가 한 번도 안 불려서, 씬에 저장된
            // 기본 활성 상태(대개 넷 다 켜짐)가 그대로 노출돼 카드도
            // 이름도 없는 빈 좌석 상자들이 모드 선택 팝업 뒤에 보이고
            // 있었다. 인원수를 고르기 전까지는 넷 다 명시적으로 꺼둔다 —
            // BeginWithSeatCount가 부르는 BuildStaticUI/ApplySeatVisibility가
            // 실제 인원수에 맞게 다시 켠다.
            if (leftSeatRef)  leftSeatRef.gameObject.SetActive(false);
            if (rightSeatRef) rightSeatRef.gameObject.SetActive(false);
            if (topSeatRef)   topSeatRef.gameObject.SetActive(false);
            if (mySeatRef)    mySeatRef.gameObject.SetActive(false);
            ShowModeSelectPopup();
        }
    }

    string SeatCountTitle() => SEATS switch { 2 => "맞고", 3 => "고스톱 (3인)", _ => "고스톱 (4인)" };

    /// <summary>테스트 편의용 — 로비/타이틀을 거치지 않고 씬을 바로 열었을
    /// 때만 뜬다. 기존 게임오버 오버레이 인프라(ui.ShowOverlay)를 그대로
    /// 재사용한다 — 버튼 3개(2/3/4인)라 딱 맞는다.</summary>
    void ShowModeSelectPopup()
    {
        ui?.ShowOverlay(new Color(.93f, .73f, .18f), "인원수를 선택하세요", null,
            "테스트용 — 로비/타이틀을 거치면 자동으로 정해집니다",
            "2인 (맞고)", () => BeginWithSeatCount(2),
            "3인 (고스톱)", () => BeginWithSeatCount(3),
            "4인 (고스톱)", () => BeginWithSeatCount(4));
    }

    void BeginWithSeatCount(int n)
    {
        ui?.HideOverlay();
        SetSeatCount(n);
        seatCountPreset = true;
        ui?.SetTitle(SeatCountTitle());
        BuildStaticUI();
        NewGame();
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
            GoStopNetLobby.Instance.OnGuestGoneForGood -= OnGuestGoneForGoodHandler;
            GoStopNetLobby.Instance.OnGuestReconnected -= OnGuestReconnectedHandler;
            GoStopNetLobby.Instance.OnDisconnected -= OnHostDisconnected;
            GoStopNetLobby.Instance.OnReconnecting -= OnReconnectingHandler;
            GoStopNetLobby.Instance.OnReconnected -= OnReconnectedHandler;
            GoStopNetLobby.Instance.OnSeatReassigned -= OnSeatReassignedHandler;
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

        if (SEATS == 2)
        {
            // 맞고(2인) — 좌/우 좌석 자체가 없다. 하단=나, 상단=상대만 쓴다.
            slotSeat[2] = (PLAYER_SEAT + 1) % SEATS;
            slotSeat[1] = -1;
            slotSeat[3] = -1;
            return;
        }

        if (SEATS == 3)
        {
            // 2026-08-23(씬 통합, 사용자 확인 규칙 정정): 3인 모드는 이제
            // 좌/우(LeftSeat/RightSeat)만 쓰고 상단(TopSeat)은 꺼둔다 —
            // TopSeat는 이제 맞고(2인)가 상대 1명의 뒷패·Cap을 보여주는
            // 용도로 전용됐다(BuildStaticUI 참고). 3인 모드는 광팔이
            // 로테이션이 없어 sittingOutSeat가 항상 -1로 고정된다.
            slotSeat[1] = (PLAYER_SEAT + 1) % SEATS;
            slotSeat[3] = (PLAYER_SEAT + 2) % SEATS;
            slotSeat[2] = -1;
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
    ///
    /// 2026-08-23(design.md §5.2/§5.3 확정): 2번째가 불참하면 3번째에게는
    /// 참가 여부를 아예 묻지 않는다("2번째가 참가한 경우에만 3번째에게
    /// 묻는다") — 3번째는 자동으로 참가 처리된다. 예전엔 2번째 답과 무관하게
    /// 항상 둘 다에게 물었다.
    /// </summary>
    IEnumerator AskParticipation(int candidate, System.Action<bool> onResult)
    {
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
            // 판정은 candidate 기준이어야 한다.
            SendTargetedPrompt(candidate, s => { s.declarePending = true; s.declareDealerName = SeatNameFor(dealerSeat, candidate); });
            GoStopNetMessage declMsg = null;
            yield return StartCoroutine(WaitForRemoteMessage(candidate,
                m => m.type == GoStopNetMessage.Type.DeclareChoice, m => declMsg = m));
            // design.md §50.1 — 무응답(타임아웃) 시 불참(죽기) 처리.
            wantsIn = declMsg?.boolValue ?? false;
        }
        else wantsIn = GoStopAI.WantsToPlay(hand[candidate]);
        onResult(wantsIn);
    }

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
        // 2026-08-23: SEATS==2(맞고)는 GoStopGame.cs가 쓰던 GoStopRules.DealNew()
        // (손 10장×2·필드 8장·더미 22장, 조커 포함 50장)를 그대로 재사용하되
        // 결과를 SEATS 배열 모양(hand[0]/hand[1])으로 옮겨 담는다 — 딜링 로직
        // 자체는 새로 안 만들고 기존 검증된 걸 그대로 쓴다.
        List<HwatuCard> jokersInField;
        if (SEATS == 2)
        {
            var deal2 = GoStopRules.DealNew();
            hand[0] = deal2.playerHand; hand[1] = deal2.aiHand;
            field = deal2.field; drawPile = deal2.drawPile;
            jokersInField = deal2.jokersInField;
        }
        else if (SEATS == 3)
        {
            var deal3 = GoStopRules.DealNew3P();
            hand[0] = deal3.hand0; hand[1] = deal3.hand1; hand[2] = deal3.hand2;
            field = deal3.field; drawPile = deal3.drawPile;
            jokersInField = deal3.jokersInField;
        }
        else
        {
            var deal = GoStopRules.DealNew4PFull();
            for (int s = 0; s < SEATS; s++) hand[s] = deal.hands[s];
            field = deal.field; drawPile = deal.drawPile;
            jokersInField = deal.jokersInField;
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

        // 2026-08-23: "조커도 손패로 나와야 한다" 요청으로 딜링을 50장(48+조커2)
        // 통째로 섞는 방식으로 바꿨다(GoStopRules.BuildFullDeckWithJokers) —
        // 그 결과 조커가 필드에 떨어질 수도 있게 됐는데, 조커는 월이 없어
        // 아무도 매칭으로 못 가져간다. 더미에서 뒤집힐 때 즉시 그 사람 피로
        // 들어가는 기존 규칙과 같은 원리로, 딜 직후 즉시 선(dealerSeat)에게
        // 지급한다(사용자 확인 규칙).
        foreach (var j in jokersInField) captured[dealerSeat].Add(j);

        ppeokCauser.Clear();
        ppeokBonusPi.Clear();
        emergencyFired.Clear();
        achievedFired.Clear();
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
        if (jokersInField.Count > 0) Toast(dealerSeat, "보너스 획득");

        // 2026-08-19: 3인 모드는 광팔이 로테이션 자체가 없다 — 접속한
        // 3명이 전원 그대로 플레이한다. 4인 전용인 참가 선언·광판다
        // 정산 절차 전체를 건너뛴다. 2026-08-23: SEATS==2(맞고)도 참가
        // 선언·광팔이라는 개념 자체가 없어 마찬가지로 건너뛴다.
        if (SEATS == 2 || SEATS == 3)
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

        // 2026-08-23(design.md §5.2/§5.3 확정): 2번째가 불참하면 이미 4번째까지
        // 자리가 자연히 채워지므로(선+3번째+4번째), 3번째에게는 참가 여부를
        // 아예 묻지 않고 자동 참가시킨다 — 예전엔 2번째 답과 무관하게 항상
        // 3번째에게도 물었다. "2번째가 참가한 경우에만" 3번째에게 묻는다.
        bool secondIn = false;
        yield return StartCoroutine(AskParticipation(order[1], r => secondIn = r));
        if (secondIn) active.Add(order[1]); else declined.Add(order[1]);

        bool thirdIn;
        if (secondIn)
        {
            bool thirdResult = false;
            yield return StartCoroutine(AskParticipation(order[2], r => thirdResult = r));
            thirdIn = thirdResult;
        }
        else
        {
            thirdIn = true; // 3번째에게 묻지 않고 자동 참가
        }
        if (thirdIn) active.Add(order[2]); else declined.Add(order[2]);

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
            int perPayer = sellableCount * WON_PER_POINT * stakeMultiplier;
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
    // 2026-08-23: "첫뻑/연뻑/첫따닥 시에 이펙트 추가" 요청. 조사해보니
    // 첫뻑·연뻑은 label.Contains("뻑")에 우연히 걸려 이미 이펙트가
    // 뜨고는 있었지만 평범한 "뻑"과 완전히 같은 색(주황)이라 구분이
    // 안 됐고, "첫따닥"은 정확 일치("따닥")·Contains("쪽") 어느 것도
    // 안 걸려 **이펙트 자체가 아예 안 떴다**(버그). 셋 다 실제 돈이
    // 오가는 이벤트라 다른 뻑/따닥과는 다른 색(금전 신호 — 초록)으로
    // 명시적으로 분리한다.
    static readonly Color MoneyEventColor = new Color(0.20f, 0.85f, 0.45f);
    static bool IsMoneyEventLabel(string label) => label == "첫뻑" || label == "연뻑" || label == "첫따닥";

    void ShowActionPopup(string label)
    {
        // "따닥"은 전용 프리팹을 새로 굽는 대신(2026-08-20) EffectJjok의
        // 구조(팝인·유지·페이드)를 그대로 재사용하고 Play()의 overrideColor로
        // 색만 바꾼다 — 폭탄/뻑이 EffectPpeok을 공유하는 것과 같은 원칙.
        string prefabName =
            label == "자뻑"          ? "EffectThanksMore" :
            label == "뻑 먹기"       ? "EffectThanks" :
            label == "첫뻑" || label == "연뻑" ? "EffectPpeok" : // 구조는 뻑과 공유, 색만 금전 이벤트로 override
            label == "첫따닥"        ? "EffectJjok" :            // 구조는 따닥과 공유, 색만 override
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

        bool moneyEvent = IsMoneyEventLabel(label);
        // 2026-08-19: "파티클 이펙트로 애니메이션을 좀 더 역동적으로" 요청 —
        // 텍스트 팝업과 같은 자리에 원형 파티클 버스트를 같이 터뜨린다.
        // 금전 이벤트는 살짝 더 화려하게(16개, 기본 12개보다 많이).
        GoStopIcons.SpawnBurst(canvasRoot, local, moneyEvent ? MoneyEventColor : BurstColorForLabel(label), moneyEvent ? 16 : 12);

        var fx = HwatuUI.InstantiateEffect<GoStopEffectPopup>(prefabName, canvasRoot);
        if (fx == null) return;
        // 필드(더미) 위치를 Canvas 로컬 좌표로 변환해서 그 자리에 띄운다 —
        // ContentArea는 Canvas 안에서 HUD만큼 오프셋돼 있을 수 있어(이 씬은
        // HUD를 꺼서 지금은 오프셋이 없지만, 좌표계 자체는 항상 이렇게
        // 다뤄야 안전하다) Canvas 정중앙(0,0)과 필드 위치가 다를 수 있다.
        fx.root.anchoredPosition = local;

        // "감사합니다"/"더 감사합니다"는 프리팹 기본 문구를 그대로 쓰고,
        // 나머지는 실제 라벨(첫뻑!/연뻑! 등 상황별 문구)을 덮어써서 보여준다.
        // "따닥"/금전 이벤트만 색까지 override해서 평범한 뻑/따닥과
        // 구분되게 한다.
        if (prefabName == "EffectThanks" || prefabName == "EffectThanksMore") fx.Play();
        else if (moneyEvent) fx.Play(label, MoneyEventColor);
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

    // ── 비상 시스템 ──────────────────────────────────────
    // 고도리/홍단/초단/청단이 완성 직전(2/3, 안 막힘)이면 다른 플레이어
    // 들에게 알린다 — 단순 장식이 아니라 "이 패를 선점해야 한다"는 게임
    // 플레이 정보 전달용. RebuildUI 맨 끝에서 매번 호출되므로(캡처가
    // 일어나는 모든 경로 — r1/r2/조커/DeckOnlySeq — 뒤에 항상 걸린다)
    // 개별 캡처 지점마다 따로 걸어줄 필요가 없다.
    static readonly (string name, System.Func<HwatuCard, bool> pred)[] EmergencySets =
    {
        ("고도리", GoStopRules.IsGodori),
        ("홍단",   GoStopRules.IsHongdan),
        ("초단",   GoStopRules.IsChodan),
        ("청단",   GoStopRules.IsCheongdan),
    };

    // design.md §26/§27이 요구하는 "3광" 비상 전용 판정 슬롯 — emergencyFired의
    // setIdx로 4를 쓴다(0~3은 위 EmergencySets 배열 인덱스와 겹치지 않는다).
    const int GwangEmergencyIdx = 4;

    void CheckEmergencies()
    {
        foreach (int seat in ActiveSeats())
        {
            var mine = captured[seat];
            if (mine.Count == 0) continue;
            List<HwatuCard> theirs = null; // 필요할 때만(세트 하나라도 검사 대상일 때) 만든다

            for (int i = 0; i < EmergencySets.Length; i++)
            {
                bool needEmergency = !emergencyFired.Contains((seat, i));
                bool needAchieve   = !achievedFired.Contains((seat, i));
                if (!needEmergency && !needAchieve) continue;
                theirs ??= ActiveSeats().Where(s => s != seat).SelectMany(s => captured[s]).ToList();
                var (state, have) = GoStopRules.CheckSet(mine, theirs, EmergencySets[i].pred);
                if (needEmergency && state == GoStopRules.SetState.Alive && have == 2)
                {
                    emergencyFired.Add((seat, i));
                    FireEmergency(seat, EmergencySets[i].name);
                }
                // 2026-08-25 — "완성" 이펙트는 비상과 완전히 독립적으로 판정한다.
                // 뻑/폭탄처럼 한 번에 여러 장이 들어오면 have가 2를 거치지
                // 않고 곧장 3으로 뛸 수 있어서(비상이 안 뜨고 바로 완성),
                // emergencyFired 발동 여부에 기대면 안 된다.
                if (needAchieve && state == GoStopRules.SetState.Achieved)
                {
                    achievedFired.Add((seat, i));
                    FireAchievement(seat, EmergencySets[i].name);
                }
            }

            // 2026-08-23(design.md §26 확정): 3광도 비상 대상에 추가한다.
            // 광은 5장 중 3장만 있으면 되는 "풀에서 N장" 조건이라, 정확히
            // 3장뿐인 홍단/청단/초단/고도리에 쓰던 CheckSet의 "상대가 1장만
            // 가져도 막힘" 판정을 그대로 쓰면 오탐(과잉 차단)이 난다 —
            // 전용 판정(CheckGwangEmergency)을 따로 쓴다.
            {
                bool needEmergency = !emergencyFired.Contains((seat, GwangEmergencyIdx));
                bool needAchieve   = !achievedFired.Contains((seat, GwangEmergencyIdx));
                if (needEmergency || needAchieve)
                {
                    theirs ??= ActiveSeats().Where(s => s != seat).SelectMany(s => captured[s]).ToList();
                    var (state, have) = CheckGwangEmergency(mine, theirs);
                    if (needEmergency && state == GoStopRules.SetState.Alive && have == 2)
                    {
                        emergencyFired.Add((seat, GwangEmergencyIdx));
                        FireEmergency(seat, "3광");
                    }
                    if (needAchieve && state == GoStopRules.SetState.Achieved)
                    {
                        achievedFired.Add((seat, GwangEmergencyIdx));
                        FireGwangAchievement(seat, mine);
                    }
                }
            }
        }
    }

    /// <summary>3광 비상 판정 — 광 5장 중 3장을 채우면 되므로, 상대가 광을
    /// 몇 장 가져갔든 "아직 아무도 안 가져간 광"이 필요한 만큼 남아있으면
    /// 여전히 Alive다(홍단류처럼 상대가 1장만 가져도 바로 Blocked가 아니다).
    /// </summary>
    static (GoStopRules.SetState state, int have) CheckGwangEmergency(List<HwatuCard> mine, List<HwatuCard> theirs)
    {
        int have = mine.Count(c => c.kind == HwatuKind.Gwang);
        if (have >= 3) return (GoStopRules.SetState.Achieved, have);
        int theirsCount = theirs.Count(c => c.kind == HwatuKind.Gwang);
        int stillObtainable = 5 - have - theirsCount; // 내 손에도 상대 손에도 없는 나머지 광
        if (stillObtainable < 3 - have) return (GoStopRules.SetState.Blocked, have);
        return (GoStopRules.SetState.Alive, have);
    }

    /// <summary>비상 이펙트 발동 — 필드 중앙에 큼직하게, 어느 좌석이 어떤
    /// 족보에 근접했는지 알려준다. 프리팹은 EffectGodoriEmergency/
    /// EffectHongdanEmergency/EffectChodanEmergency/EffectCheongdanEmergency/
    /// EffectGwangEmergency(GoStopEffectPopup 공유 — Assets/Resources/
    /// Prefabs/GoStop/Effects/) — 기본 문구에 좌석 이름을 앞에 붙여서 덮어쓴다.
    /// <br/>2026-08-25 — 완성 이펙트(<see cref="FireAchievement"/>/
    /// <see cref="FireGwangAchievement"/>)와 프리팹을 완전히 분리했다
    /// (예전엔 같은 프리팹을 문구·색·파티클 수만 다르게 재사용했는데, 사용자가
    /// 각각 따로 디자인하려면 별개 에셋이어야 한다).
    /// <br/>네트워크 동기화는 이번엔 안 걸었다 — 호스트 화면에서만 보인다
    /// (게스트에게 안 뜬다). Toast처럼 EventMsg로 실어 보내려면 게스트
    /// 쪽 수신 핸들러가 이 라벨 형식을 알아야 하는데, 아직 검증 안 된
    /// 네트워크 경로에 새 메시지 형식을 얹는 리스크를 이번엔 피했다 —
    /// 다음에 실제 두 기기 테스트를 할 때 같이 확인할 것.</summary>
    void FireEmergency(int seat, string setName)
    {
        string prefabName = setName switch
        {
            "고도리" => "EffectGodoriEmergency",
            "홍단" => "EffectHongdanEmergency",
            "초단" => "EffectChodanEmergency",
            "청단" => "EffectCheongdanEmergency",
            "3광" => "EffectGwangEmergency", // 2026-08-23(design.md §26/§27 확정)
            _ => null,
        };
        if (prefabName == null || fieldArea == null) return;

        var canvasRoot = fieldArea.parent.parent.parent as RectTransform;
        Vector2 local = canvasRoot.InverseTransformPoint(fieldArea.position);

        GoStopIcons.SpawnBurst(canvasRoot, local, EmergencyColor(setName), 20);

        var fx = HwatuUI.InstantiateEffect<GoStopEffectPopup>(prefabName, canvasRoot);
        if (fx != null)
        {
            fx.root.anchoredPosition = local;
            fx.Play($"{SeatName(seat)} {setName} 비상!", EmergencyColor(setName));
        }

        ShowTimedToast($"{SeatName(seat)}이(가) {setName} 완성 직전!");
        GoStopAudio.Instance?.Bonus();
    }

    /// <summary>족보(광 제외) "완성" 이펙트 — 비상(2/3 경고)과 별개로, 실제로
    /// 3장을 채운 순간 한 번 터진다. 프리팹은 EffectGodoriAchieved/
    /// EffectHongdanAchieved/EffectChodanAchieved/EffectCheongdanAchieved —
    /// 비상 프리팹과 완전히 분리된 별개 에셋이라 서로 다르게 디자인할 수
    /// 있다. 파티클을 비상보다 화려하게(20→30개, 총통/광팔이급) 키우고,
    /// 사운드도 경고음(Bonus)이 아니라 축하음(Win)을 쓴다 — 같은 프레임에
    /// 비상과 완성이 동시에 뜨는 경우(뻑/폭탄으로 2/3을 건너뛰고 곧장
    /// 3장이 들어온 경우)는 없다 — CheckEmergencies가 애초에 별개 조건
    /// (have==2 vs Achieved)이라 한 세트가 같은 판정 안에서 둘 다를 동시에
    /// 만족할 수 없다. 광은 3/4/5장·비삼광 여부에 따라 프리팹이 4개로
    /// 더 갈리므로 <see cref="FireGwangAchievement"/>가 따로 담당한다.</summary>
    void FireAchievement(int seat, string setName)
    {
        string prefabName = setName switch
        {
            "고도리" => "EffectGodoriAchieved",
            "홍단" => "EffectHongdanAchieved",
            "초단" => "EffectChodanAchieved",
            "청단" => "EffectCheongdanAchieved",
            _ => null,
        };
        if (prefabName == null || fieldArea == null) return;

        var canvasRoot = fieldArea.parent.parent.parent as RectTransform;
        Vector2 local = canvasRoot.InverseTransformPoint(fieldArea.position);

        GoStopIcons.SpawnBurst(canvasRoot, local, EmergencyColor(setName), 30);

        var fx = HwatuUI.InstantiateEffect<GoStopEffectPopup>(prefabName, canvasRoot);
        if (fx != null)
        {
            fx.root.anchoredPosition = local;
            fx.Play($"{SeatName(seat)} {setName} 완성!", EmergencyColor(setName));
        }

        ShowTimedToast($"{SeatName(seat)}이(가) {setName} 완성!");
        GoStopAudio.Instance?.Win();
    }

    /// <summary>광 완성 이펙트 — 사용자 확인(2026-08-25)에 따라 <b>완성만</b>
    /// 4단계로 나눈다(비상은 그대로 하나, <see cref="FireEmergency"/>의
    /// "3광" 케이스). 실제 정산 점수표(광 점수표 정정 문서 참고: 비삼광=2점,
    /// 3광=3점, 4광=4점, 5광=15점)와 정확히 같은 기준(광 5장 중 몇 장을
    /// 가졌는지, 3장일 때 12월 비광 포함 여부)으로 프리팹을 고른다 —
    /// EffectBiSamGwang/EffectSamGwang/EffectSaGwang/EffectOGwang.
    /// 한 판에 한 번만 발동하므로(achievedFired) 3광으로 먼저 완성한 뒤
    /// 나중에 4·5광으로 늘어나도 재발동하지 않는다 — "완성 그 순간"의
    /// 구성으로 어느 프리팹인지 정해진다.</summary>
    void FireGwangAchievement(int seat, List<HwatuCard> mine)
    {
        var gwangCards = mine.Where(c => c.kind == HwatuKind.Gwang).ToList();
        int count = gwangCards.Count;
        bool hasBiGwang = gwangCards.Any(c => c.month == 12);

        string prefabName, label;
        if (count >= 5)      { prefabName = "EffectOGwang";     label = "5광"; }
        else if (count == 4) { prefabName = "EffectSaGwang";    label = "4광"; }
        else if (hasBiGwang) { prefabName = "EffectBiSamGwang"; label = "비삼광"; }
        else                 { prefabName = "EffectSamGwang";   label = "3광"; }

        if (fieldArea == null) return;

        var canvasRoot = fieldArea.parent.parent.parent as RectTransform;
        Vector2 local = canvasRoot.InverseTransformPoint(fieldArea.position);
        var color = EmergencyColor("3광"); // 광 계열 톤 — 4개 프리팹은 각자 기본색이 이미 갈려 있다

        GoStopIcons.SpawnBurst(canvasRoot, local, color, 30);

        var fx = HwatuUI.InstantiateEffect<GoStopEffectPopup>(prefabName, canvasRoot);
        if (fx != null)
        {
            fx.root.anchoredPosition = local;
            fx.Play($"{SeatName(seat)} {label} 완성!", color);
        }

        ShowTimedToast($"{SeatName(seat)}이(가) {label} 완성!");
        GoStopAudio.Instance?.Win();
    }

    static Color EmergencyColor(string setName) => setName switch
    {
        "고도리" => new Color(0.949f, 0.718f, 0.020f),
        "홍단"   => new Color(0.906f, 0.298f, 0.235f),
        "초단"   => new Color(0.180f, 0.800f, 0.443f),
        "청단"   => new Color(0.231f, 0.616f, 0.910f),
        "3광"    => new Color(0.95f, 0.78f, 0.15f), // 고도리와 톤을 살짝 갈랐다(둘 다 금색 계열이라 구분 필요)
        _        => Color.white,
    };

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

        // 2026-08-23: "조커도 손패로 나와야 한다" 요청으로 손패에 조커가
        // 실제로 있을 수 있게 됐다 — 조커는 월이 없어 필드 매칭·폭탄·흔들기
        // 어느 판정에도 안 걸리므로(전부 월 비교 기반), 흔들기 팝업 로직까지
        // 가기 전에 완전히 별도 경로로 분리한다.
        if (card.isJoker) { ContinuePlayerPlay(card, false); return; }

        // 2026-08-23: "폭탄하면 그냥 2배인데 흔들기까지 물어봐서 4배가
        // 된다" 신고 — 폭탄(손 3장+필드 1장)은 GoStopRules.ResolveWithBomb에서
        // 조건이 맞으면 무조건·자동으로 터진다(선택의 여지가 없다). 그런데
        // 흔들기는 원래 "패를 안 내고 들고 있겠다"는 선언이라, 클릭하는
        // 순간 무조건 폭탄으로 4장이 한꺼번에 나가는 상황에서는 애초에
        // "들고 있을" 여지가 없다 — 그런데도 hand.Count==3 조건만 보고
        // 흔들기부터 물어봐서, 폭탄 배수(×2)와 흔들기 배수(×2)가 같은
        // 판에 중복으로 곱해졌다. 필드에 그 달이 정확히 1장 있으면(폭탄
        // 조건) 흔들기를 아예 안 묻는다 — 이전 턴에 이미 흔들기를
        // 선언해 둔 뒤(그때는 필드에 아직 매칭 카드가 없었을 수 있다)
        // 나중에 필드가 채워져 폭탄이 되는 경우는 shookMonths에 이미
        // 기록돼 있어 여기 안 걸리므로(재질문 안 함) 정상적으로 두 배수가
        // 다 인정된다 — 막는 건 "이번 클릭 한 번으로 흔들기+폭탄이
        // 동시에 성립하는" 경우뿐이다.
        bool tripleInHand = hand[PLAYER_SEAT].Count(c => c.month == card.month) == 3;
        bool bombEligible = field.Count(c => c.month == card.month) == 1;
        if (tripleInHand && !bombEligible && !shookMonths[PLAYER_SEAT].Contains(card.month))
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

    /// <summary>2026-08-23: "조커도 손패로 나와야 한다" 요청으로 손패에 조커가
    /// 실제로 존재할 수 있게 됐다(GoStopRules.BuildFullDeckWithJokers). 손패
    /// 조커를 내면 필드 매칭·폭탄·흔들기·뻑 어느 것도 안 거치고 곧장 Cap에
    /// 들어가고, 그 대신 뒷패 한 장을 뽑아 손패를 채운다(사용자 확인 규칙 —
    /// "캡에 추가하고 뒷패를 까서 내 손패로 가져온다"). 덱에서 뒤집힌
    /// 조커(ResolveBonusJoker)는 그 뒤 카드를 필드 매칭 파이프라인에 그대로
    /// 태우는 것과 달리, 이쪽은 필드를 아예 거치지 않는 완전히 독립된
    /// 액션이다 — 뻑/쪽/따닥 같은 부가 규칙과 무관하다.</summary>
    IEnumerator PlayJokerFromHandSeq(int seat, HwatuCard joker)
    {
        var h = hand[seat];
        var cap = captured[seat];

        GoStopAudio.Instance?.CardPlay();

        int originSlotIdx = SlotOf(seat);
        RectTransform originSlot = seat == PLAYER_SEAT ? FindHandSlot(joker)
            : (originSlotIdx == 1 || originSlotIdx == 3) ? backArea[originSlotIdx] : fieldArea;
        flyFrom[joker] = originSlot != null ? originSlot.position : fieldArea.position;

        h.Remove(joker);
        cap.Add(joker);
        GoStopAudio.Instance?.Capture();
        Toast(seat, "보너스 획득");
        RebuildUI();
        yield return new WaitForSeconds(PLAY_STEP_DELAY);

        // 뒷패를 까서 손으로 — 연달아 조커가 나오는 극히 드문 경우엔 같은
        // 처리를 반복한다(재귀 대신 루프로, 손패 조작을 한 곳에 모아 둔다).
        while (drawPile.Count > 0)
        {
            var next = drawPile[0]; drawPile.RemoveAt(0);
            flyFrom[next] = drawPileArea.position;

            if (next.isJoker)
            {
                cap.Add(next);
                GoStopAudio.Instance?.Capture();
                Toast(seat, "보너스 획득");
                RebuildUI();
                yield return new WaitForSeconds(PLAY_STEP_DELAY);
                continue;
            }

            h.Add(next);
            SortHand(h);
            RebuildUI();
            yield return new WaitForSeconds(PLAY_STEP_DELAY);
            break;
        }
    }

    // ── 카드 한 장 처리 (손패 → 필드 매칭 → 덱 뒤집기 → 필드 매칭) ─────
    IEnumerator PlaySeq(int seat, HwatuCard card, bool declareShake, System.Action onDone)
    {
        var h = hand[seat];
        var cap = captured[seat];

        if (card.isJoker)
        {
            yield return StartCoroutine(PlayJokerFromHandSeq(seat, card));
            actionBusy = false;

            // 2026-08-23: "조커를 내면 캡+리필까지 하고 바로 턴이 넘어가면
            // 안 된다, 리필된 손패에서 다시 한 장을 골라 내야 턴이 넘어가야
            // 한다" 신고 — 조커는 진짜로 낸 카드가 아니라 덤(캡 1장 +
            // 손패 리필)이라, 여기서 곧장 onDone(=AfterAction, 턴 종료)을
            // 부르면 이번 턴에 카드를 한 장도 안 내고 턴이 그냥 넘어가
            // 버린다. 리필 후 손패가 남아있으면(거의 항상 그렇다) 진짜로
            // 낼 카드를 다시 고르게 한다 — 로컬 AI는 곧바로 재귀 호출로
            // 이어서 고르고, 원격 좌석은 RemoteTurn을 다시 걸어 다음
            // 메시지를 기다리고(안 그러면 게스트가 다음 카드를 보내도
            // 듣는 사람이 없다), 로컬 플레이어는 onDone을 안 불러서 턴을
            // 그대로 유지한다 — state/currentSeat가 안 바뀌었으니
            // OnPlayerPlay가 이미 다음 클릭을 받아줄 준비가 돼 있다.
            // (원래 딜에서 조커 2장을 다 받은 극히 드문 경우, 여기서 고른
            // "다음 카드"가 또 조커일 수도 있는데 — 그럼 이 분기가 다시
            // 한 번 더 걸릴 뿐이라 자연스럽게 처리된다.)
            if (h.Count > 0)
            {
                if (seat != PLAYER_SEAT && !IsRemoteSeat(seat))
                {
                    var next = GoStopAI.ChooseCard(h, field);
                    yield return StartCoroutine(PlaySeq(seat, next, GoStopAI.ShouldShake(), onDone));
                }
                else if (IsRemoteSeat(seat))
                {
                    yield return StartCoroutine(RemoteTurn(seat));
                }
                yield break;
            }

            // 손패가 완전히 바닥났으면(조커를 뽑았는데 더미도 바닥나
            // 리필이 안 된 경우) 더는 낼 게 없으니 턴을 끝낸다.
            onDone?.Invoke();
            yield break;
        }

        GoStopAudio.Instance?.CardPlay();

        if (h.Count(c => c.month == card.month) == 3 && declareShake && shookMonths[seat].Add(card.month))
        {
            heundeulCount[seat]++;
            Toast(seat, $"{card.month}월 흔들기");
        }

        bool wasFirstPlay = isFirstPlayOfRound;
        isFirstPlayOfRound = false;

        // 2026-08-25 버그 수정 — "손패는 여전히 오프셋 없이 나온다" 신고.
        // GoStopRules.Resolve가 매칭된 필드 카드를 곧바로 field에서
        // Remove해버리므로(캡처를 실제로 커밋하기 전인데도), r1을 계산한
        // 뒤에 field를 다시 조회하면 이미 매칭 카드가 사라져 있어 "매칭
        // 없음"으로 잘못 판정됐다. Resolve를 부르기 *전에* 미리 스냅샷을
        // 떠 둔다 — 이 값이 "손패가 실제로 마주친 필드 카드 수"의 정답이다.
        int preTurnCardMonthCount = field.Count(c => c.month == card.month);

        var r1 = GoStopRules.ResolveWithBomb(card, h, field, out bool bomb);

        // 2026-08-26 정정(사용자 확인) — 쪽/따닥/싹쓸이의 "마지막 턴" 예외는
        // 더미의 마지막 한 장이 아니라 **각자 자기 손패의 마지막 장을 낼 때**
        // (맞고 10번째, 3~4인 고스톱 7번째)를 가리키는 것이었다 — 남은 손패가
        // 적을수록 어떤 패가 나올지 예측이 쉬워져서 랜덤성이 없어지기 때문.
        // 이 손패(h)에서 card(폭탄이면 파트너까지)가 이미 빠진 뒤라 h.Count==0
        // 이면 정확히 "이번이 그 손의 마지막 카드"다. 손이 다 떨어진 뒤 덱만
        // 넘기는 턴(DeckOnlySeq)은 이미 그 마지막 턴이 지난 뒤라 이 예외가
        // 다시 적용되지 않는다(사용자 확인 — 손이 빈 이후 턴부터는 정상적으로
        // 쪽/따닥/싹쓸이가 붙는다).
        bool isLastHandCard = h.Count == 0;

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

        // 2026-08-24 정정(사용자 확인) — "폭탄이란 흔들고(흔들기 카운트
        // 올리고) 매칭되는 패를 즉시 내서 상대 피를 가져오는 것"이다.
        // 폭탄은 손패에 3장이 모여야만 성립하는데(그 자체가 흔들기 조건과
        // 동일), 예전엔 폭탄이면 흔들기 팝업 자체를 건너뛰어서(4배 방지)
        // heundeulCount가 전혀 안 올랐다 — 배지도 같이 안 떴다. 폭탄은
        // "흔들기를 즉시 실행한 것"이므로 배지/카운트는 그대로 올리고,
        // 배수 중복(×4)은 GoStopRules 쪽에서 막는다(폭탄 전용 곱셈 루프를
        // 없애고 heundeulCount 하나로 통일 — 폭탄이 흔들기 개수에 이미
        // 포함되므로 따로 또 곱할 필요가 없다).
        if (bomb)
        {
            bombCount[seat]++;
            if (shookMonths[seat].Add(card.month)) heundeulCount[seat]++;
        }

        bool willDraw = !bomb && drawPile.Count > 0;
        // 뻑 감지: 뒷패 공개로 뒤집힐 수 있는 건 순수 1:1 매칭(선택도
        // 폭탄도 아님)뿐이다 — 이 조건 하나로 "결과를 아직 확정 지으면
        // 안 되는" 카드를 가려낸다(2026-08-22 결정 그대로 유지).
        bool couldBePpeok = !bomb && !r1HadChoice && r1.matchCount == 1;

        var dualPiPending = new List<HwatuCard>();

        // ════════════════════════════════════════════════════════════
        // 2026-08-23: 카드 애니메이션 시퀀스 재설계(사용자 지정 순서) —
        // ① 손패 카드가 필드에 슬램다운으로 등장(매칭 위치/빈 슬롯, 폭탄은
        //   3장 연속) → ② 뒷패도 같은 방식으로 슬램다운 등장 → ③ 뻑이
        //   아니라면 그제서야 캡에 배치 → ④ 그 후 피 뺏기. 실제 캡처·점수·
        //   뻑/쪽/따닥/폭탄 판정 로직은 위(r1 계산)에서 이미 끝나 있고 전혀
        //   안 바뀐다 — 여기서부터는 "언제 무엇을 보여줄지"만 다룬다.
        //
        // 슬램다운은 진짜 카드가 아니라 임시 "고스트"(SpawnGhostCard)다.
        // 매칭된 필드 카드는 아직 실제로 존재하므로(RebuildUI가 아직 한
        // 번도 안 돔) 고스트가 그 위에 겹쳐 앉는 모양이 된다. 고스트를
        // 지우는 시점에 맞춰 flyFrom을 그 착지 지점으로 등록해 두면,
        // 나중에 RebuildUI가 그리는 "진짜" 카드가 고스트가 있던 자리에서
        // 자연스럽게 이어서 움직인다(SlamIn) — 그래서 기존 2단 경유
        // 연출(RegisterFlyViaField/SlamInViaField)은 더 이상 필요 없다 —
        // 고스트 자체가 그 "경유"를 담당한다.

        // 필드 쪽 매칭 카드(들)의 현재 위치를 손패 고스트가 등장하기 전에
        // 미리 기록해 둔다 — 나중에 실제로 캡처될 때 그 자리에서 이어서
        // 날아가는 것처럼 보이게 한다. r1.captured는 항상 "손패 쪽 카드
        // 먼저, 필드 쪽 매칭 카드가 그 다음"으로 채워진다(손패 쪽 장수는
        // 폭탄이면 3장, 아니면 1장 — GoStopRules.Resolve/ResolveWithBomb의
        // 구성 순서를 그대로 따른 것).
        int handSideCount = bomb ? 3 : 1;
        foreach (var fc in r1.captured.Skip(handSideCount))
        {
            var go = fieldArea.Find(fc.spriteName);
            if (go != null) flyFrom[fc] = go.position;
        }

        // --- ① 손패 카드 슬램다운 ---
        // handLandingWorld는 "그 달 슬롯의 중심"(오프셋 없음). 실제
        // 고스트는 GhostMatchOffset으로 매칭 시에만 (x+15,y-15) 비켜
        // 착지한다("필드에 매칭되는 패에 완벽하게 겹쳐서 어색하다" 신고,
        // 2026-08-25 — 1차로 시도한 부채꼴 공식(±11px)은 "너무 적다"는
        // 피드백으로 폐기하고 사용자가 직접 지정한 고정 오프셋으로
        // 바꿨다). handActualLanding(실제 착지 지점, 오프셋 포함)은 뒤이어
        // ②에서 조커가 나올 때 "손패 필드 포지션"으로 그대로 재사용한다.
        Vector3 handLandingWorld = FieldSlotWorldPos(card.month);
        Vector3 handActualLanding = handLandingWorld; // 조커 착지 참조용 — 아래에서 실제 값으로 갱신
        var handGhosts = new List<GameObject>();
        if (bomb)
        {
            // r1.captured = [card, partner1, partner2, fieldMatch] — 앞 3장이
            // 손패에서 나온 카드다. 파파팍 — 짧은 간격으로 하나씩 착지한다.
            // 폭탄은 항상 매칭 상황이라(필드에 그 달 카드 1장 필수) 셋 다
            // 같은 매칭 오프셋으로 착지 — 조커는 폭탄 턴엔 절대 안 나오므로
            // (willDraw=false) handActualLanding 갱신은 필요 없다.
            Vector3 bombLanding = handLandingWorld + (Vector3)GhostMatchOffset(preTurnCardMonthCount);
            foreach (var hc in r1.captured.Take(3))
            {
                var ghost = SpawnGhostCard(hc, bombLanding);
                handGhosts.Add(ghost);
                StartCoroutine(SlamDown(ghost.transform as RectTransform));
                flyFrom[hc] = bombLanding;
                yield return new WaitForSeconds(0.07f);
            }
            yield return new WaitForSeconds(0.10f); // 마지막 카드가 실제로 착지할 여유
        }
        else
        {
            Vector3 landing = handLandingWorld + (Vector3)GhostMatchOffset(preTurnCardMonthCount);
            handActualLanding = landing;
            var ghost = SpawnGhostCard(card, landing);
            handGhosts.Add(ghost);
            flyFrom[card] = landing;
            yield return StartCoroutine(SlamDown(ghost.transform as RectTransform));
        }

        // --- ② 뒷패 슬램다운(있다면) ---
        HwatuCard drawn = null;
        GameObject deckGhost = null;
        if (willDraw)
        {
            drawn = drawPile[0]; drawPile.RemoveAt(0);

            if (drawn.isJoker)
            {
                // 2026-08-25 2차 정정(사용자 지시) — "뒷패에서 조커가
                // 나오면 내가 낸 손패 필드 포지션에 일단 놔줘." 필드
                // 정중앙 고정안은 폐기하고, 방금 낸 손패가 실제로 착지한
                // 자리(handActualLanding — 매칭이었으면 그 오프셋까지 포함된
                // 실제 위치)에 그대로 놓는다.
                Vector3 jokerLanding = handActualLanding;
                deckGhost = SpawnGhostCard(drawn, jokerLanding);
                yield return StartCoroutine(SlamDown(deckGhost.transform as RectTransform, dropHeight: 90f));
                flyFrom[drawn] = jokerLanding;
            }
            else
            {
                // 뒷패가 손패와 같은 달이면(=뻑이 형성될 수 있는 경우) 그
                // 슬롯엔 이미 "원래 필드 카드 + 방금 착지한 손패" 두 장이
                // 논리적으로 쌓여 있다 — 손패 쪽은 Resolve가 field에서
                // 이미 지워버려서 직접 조회로는 안 잡히므로, 손패 계산 때
                // 미리 떠 둔 preTurnCardMonthCount에 +1(손패 자신)을 더한다.
                // 다른 달이면(가장 흔한 경우) field가 그 달에 대해서는
                // 손패 처리로 전혀 안 건드려졌으니 그냥 직접 세면 된다
                // ("뻑이 날 3번째 패는 오프셋 30,-30이 맞지?" 확인 요청 —
                // preTurn(1) + 손패(1) = 2장째 → 2×(15,-15) = (30,-30)).
                int deckStackCount = (drawn.month == card.month && r1.captured.Count > 0)
                    ? preTurnCardMonthCount + 1
                    : field.Count(c => c.month == drawn.month);
                Vector3 slot = FieldSlotWorldPos(drawn.month) + (Vector3)GhostMatchOffset(deckStackCount);
                deckGhost = SpawnGhostCard(drawn, slot);
                yield return StartCoroutine(SlamDown(deckGhost.transform as RectTransform));
                flyFrom[drawn] = slot;
            }
        }

        // --- ③ 뻑 판정(순수 1:1 매칭이었을 때만) ---
        if (couldBePpeok)
        {
            bool ppeokFormed = drawn != null && !drawn.isJoker && drawn.month == card.month;
            if (ppeokFormed)
            {
                DestroyGhosts(handGhosts);
                DestroyGhost(deckGhost);

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
        }
        ppeokStreak[seat] = 0;

        // --- ④ 손패 결과를 Cap에 배치(둘 다 착지가 끝난 뒤 항상 여기서) ---
        DestroyGhosts(handGhosts);
        if (r1.captured.Count > 0)
        {
            cap.AddRange(r1.captured);
            GoStopAudio.Instance?.Capture();
            if (seat == PLAYER_SEAT || IsRemoteSeat(seat))
            {
                var dual = r1.captured.FirstOrDefault(c => c.dualPi);
                if (dual != null) dualPiPending.Add(dual);
            }
        }
        RebuildUI();
        yield return new WaitForSeconds(PLAY_STEP_DELAY);

        // --- ⑤ 피 뺏기는 Cap 배치가 끝난 뒤 별도 비트로 ---
        if (r1.captured.Count > 0)
        {
            bool stole = ApplyMatchBonus(seat, r1, bomb, allowSweep: bomb || !willDraw);
            if (stole)
            {
                RebuildUI();
                yield return new WaitForSeconds(PLAY_STEP_DELAY);
            }
        }

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
            DestroyGhost(deckGhost);

            if (drawn.isJoker)
            {
                // "필드에 방금 나온 패" = 이번에 낸 손패가 매칭 안 돼 그대로
                // 필드에 남은 경우(r1.captured가 비었으면 card가 필드에 있다)
                // 그 카드다. 손패가 뭔가를 잡았으면 남은 카드가 없어 겹쳐놓을
                // 대상이 없다 — 그런 경우엔 즉시 캡처로 단순화한다.
                HwatuCard anchor = r1.captured.Count == 0 ? card : null;
                yield return StartCoroutine(ResolveBonusJoker(seat, drawn, anchor, cap, isLastHandCard, handLandingWorld));
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
                    // 필드 쪽 매칭 카드의 위치도 손패와 같은 방식으로 미리
                    // 기록해 둔다(뒷패 쪽은 항상 손패 없이 1장이라 handSideCount
                    // 개념 없이 바로 Skip(1)).
                    foreach (var fc in r2.captured.Skip(1))
                    {
                        var go = fieldArea.Find(fc.spriteName);
                        if (go != null) flyFrom[fc] = go.position;
                    }

                    cap.AddRange(r2.captured);
                    GoStopAudio.Instance?.Capture();
                    bool chok = r1.placedOnField && r2.captured.Contains(card) && !isLastHandCard;
                    // 따닥: 손패로 필드 2장 중 하나를 고른 뒤(ddadakWatch=고르지
                    // 않은 나머지 한 장), 같은 턴의 뒷패가 그 나머지 한 장마저
                    // 잡았다. chok과는 조건이 겹치지 않는다(chok은 r1.placedOnField,
                    // 즉 손패가 아무것도 못 먹은 경우에만 성립하는데, ddadakWatch는
                    // 반대로 손패가 선택 캡처로 뭔가를 먹었을 때만 채워진다).
                    bool ddadak = ddadakWatch != null && r2.captured.Contains(ddadakWatch) && !isLastHandCard;

                    if (seat == PLAYER_SEAT || IsRemoteSeat(seat))
                    {
                        var dual2 = r2.captured.FirstOrDefault(c => c.dualPi);
                        if (dual2 != null) dualPiPending.Add(dual2);
                    }

                    RebuildUI();
                    yield return new WaitForSeconds(PLAY_STEP_DELAY);

                    // 피 뺏기 — 여기서도 Cap 배치 다음 별도 비트로 분리한다.
                    bool stole2 = false;
                    if (chok)
                    {
                        StealPiFromEachOther(seat, 1);
                        Toast(seat, "쪽");
                        stole2 = true;
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
                        stole2 = true;
                        if (r2.sweep)
                        {
                            sweeps[seat]++;
                            StealPiFromEachOther(seat, 1);
                            Toast(seat, "싹쓸이");
                        }
                    }
                    else stole2 = ApplyMatchBonus(seat, r2, false, allowSweep: !isLastHandCard);

                    if (stole2)
                    {
                        RebuildUI();
                        yield return new WaitForSeconds(PLAY_STEP_DELAY);
                    }
                }
                else
                {
                    RebuildUI();
                    yield return new WaitForSeconds(PLAY_STEP_DELAY);
                }
            }
        }

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
            // 없다 — 겹쳐놓을 대상이 없으므로 즉시 캡처로 단순화한다. 이미
            // 손이 빈 뒤라 "마지막 손패 턴"이 아니다(isLastHandCard: false).
            yield return StartCoroutine(ResolveBonusJoker(seat, drawn, null, cap, false));
        }
        else
        {
            // 2026-08-26 정정(사용자 확인) — 쪽/따닥/싹쓸이 예외는 더미의
            // 마지막 한 장이 아니라 "각자 자기 손패의 마지막 장을 낼 때"를
            // 가리킨다. 이 턴은 손이 이미 다 떨어진 뒤(그 마지막 턴은 이미
            // 지났다)라 더 이상 예외 대상이 아니다 — 항상 정상적으로
            // 쪽/따닥/싹쓸이가 붙는다(allowSweep 기본값 true 그대로).
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
                ApplyMatchBonus(seat, r, false);
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
    /// <param name="revealFrom">2026-08-23: "뒷패가 보너스패라면 유저가
    /// 직전에 선택한 손패 위 포지션에 등장한다" 요청 — PlaySeq가 방금
    /// 손패 슬램다운이 착지한 지점을 넘겨주면 그 자리에서 나타난다. 안
    /// 주어지면(DeckOnlySeq처럼 이번 턴에 손패를 안 낸 경우) 기존처럼
    /// 더미 자리에서 나타난다.</param>
    /// <param name="isLastHandCard">2026-08-26 — 이 조커가 "손패의 마지막
    /// 장을 낸 턴"에 뒤집힌 것인지(PlaySeq가 자기 turn-scope의 isLastHandCard를
    /// 그대로 넘겨준다). DeckOnlySeq에서 호출될 때는 이미 손이 빈 뒤라
    /// 항상 false를 넘긴다 — 쪽/싹쓸이 예외는 정확히 그 한 번의 턴에만
    /// 적용된다(사용자 확인).</param>
    IEnumerator ResolveBonusJoker(int seat, HwatuCard joker, HwatuCard anchor, List<HwatuCard> cap, bool isLastHandCard, Vector3? revealFrom = null)
    {
        field.Add(joker);
        flyFrom[joker] = revealFrom ?? drawPileArea.position;
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
            yield return StartCoroutine(ResolveBonusJoker(seat, extra, anchor, cap, isLastHandCard));
            yield break;
        }

        flyFrom[extra] = drawPileArea.position;
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
            bool chok = anchor != null && r.captured.Contains(anchor) && !isLastHandCard;
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
            else ApplyMatchBonus(seat, r, false, allowSweep: !isLastHandCard);

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
    /// ApplyMatchBonus보다 먼저 걸러진다(안 그러면 그냥 일반 매칭으로 지나쳐 버린다).
    /// 2026-08-23: 반환값(무언가 실제로 뺏겼는지)을 추가했다 — "피 뺏기는
    /// Cap 이동 애니메이션이 끝난 뒤 별도 단계로 보여달라" 요청으로,
    /// 호출자(PlaySeq)가 이 값을 보고 별도 RebuildUI+대기를 걸지 말지
    /// 정한다(아무것도 안 뺏겼으면 빈 대기 시간만 낭비하므로).</summary>
    bool ApplyMatchBonus(int seat, GoStopRules.CaptureResult r, bool bomb, bool allowSweep = true)
    {
        bool did = false;
        if (bomb) { StealPiFromEachOther(seat, 1); Toast(seat, "폭탄"); did = true; }
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
                did = true;
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
                did = true;
            }
        }

        if (r.sweep && allowSweep)
        {
            sweeps[seat]++;
            StealPiFromEachOther(seat, 1);
            Toast(seat, "싹쓸이");
            did = true;
        }
        return did;
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
            // design.md §50.1엔 필드선택 전용 기본값이 명시돼 있지 않지만
            // 같은 원칙(가능한 것 중 자동 선택)을 적용한다 — msg==null(타임아웃)
            // 이면 decoded도 자연히 null이 되어 아래 AI 기본값으로 떨어진다.
            // 게스트가 보낸 카드 이름으로 진짜 후보 인스턴스를 찾는다 —
            // 게스트가 갖고 있는 건 스냅샷에서 새로 디코딩한 별개의
            // HwatuCard 객체라 참조가 다르다(GoStopRules 내부는 리스트
            // 안 참조 동일성으로 카드를 다루므로 반드시 원본을 찾아 써야 한다).
            var decoded = msg != null ? GoStopDeck.Decode(msg.cardId) : null;
            chosen = decoded != null ? initial.choiceCandidates.FirstOrDefault(c => c.spriteName == decoded.spriteName) : null;
            if (chosen == null) chosen = GoStopAI.ChooseFieldMatch(initial.choiceCandidates); // 방어 — 오염된 메시지/타임아웃이 와도 판이 안 멈추게
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
            // design.md §50.1 — 국열끗 무응답(타임아웃) 시 쌍피 처리.
            card.useAsPi = msg?.boolValue ?? true;
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
        if (seat == PLAYER_SEAT && hand[PLAYER_SEAT].Count == 0 && bombCredits[PLAYER_SEAT] == 0 && rawScore >= CaptureLine)
        {
            EndGame(PLAYER_SEAT);
            return;
        }

        // lastGoScore보다 실제로 더 올라갔을 때만 다시 묻는다 — 안 그러면
        // 아무것도 못 먹어 점수가 그대로인 턴에도 매번 고/스톱을 물어보게
        // 된다("점수 변동이 없어도 계속 팝업이 뜬다"는 신고).
        if (rawScore >= CaptureLine && rawScore > lastGoScore[seat])
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

        // design.md §50.1 — Go/Stop 무응답(타임아웃) 시 스톱 처리.
        if (msg != null && msg.boolValue)
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
            // 2026-08-23: 플레이어 쪽과 같은 이유 — 이 카드가 폭탄으로
            // 터질 조건(손 3장+필드 1장)이면 흔들기 배수까지 같이 주면
            // 안 된다(OnPlayerPlay 주석 참고). AI도 예외 없이 같은 규칙을
            // 받는다.
            bool bombEligible = hand[seat].Count(c => c.month == card.month) == 3
                              && field.Count(c => c.month == card.month) == 1;
            StartCoroutine(PlaySeq(seat, card, !bombEligible && GoStopAI.ShouldShake(), () => AfterAction(seat)));
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

        if (msg == null)
        {
            // design.md §50.1 — 카드 선택(턴) 무응답(타임아웃) 시 가능한
            // 카드 중 첫 번째를 자동 선택한다.
            var autoCard = hand[seat][0];
            StartCoroutine(PlaySeq(seat, autoCard, false, () => AfterAction(seat)));
            yield break;
        }

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

        if (bestScore < CaptureLine) { EndGame(-1); return true; }
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
                $"아무도 {CaptureLine}점을 못 넘겼습니다 · 다음 판 판돈 {stakeMultiplier}배",
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

        // 2026-08-23: SEATS==2(맞고)는 피박 기준이 7장(고스톱 3~4인의 5장과
        // 다르다 — 2인 전용 GoStopGame.cs가 쓰던 값과 동일). FinalScoreMulti는
        // 기본값(5)을 그대로 쓰므로 2인일 때만 명시적으로 7을 넘긴다.
        var payout = GoStopRules.FinalScoreMulti(captured[winnerSeat], sweeps[winnerSeat], goCount[winnerSeat],
            heundeulCount[winnerSeat], bombCount[winnerSeat], loserCaptured, WON_PER_POINT,
            dokbakIdx, fixedBaseScore, extraMultiplier, piBakThreshold: SEATS == 2 ? 7 : GoStopRules.PI_BAK_THRESHOLD_3P);
        pendingPayout = payout;
        pendingWinnerSeat = winnerSeat;
        pendingLoserSeats = loserSeats;

        for (int s = 0; s < SEATS_MAX; s++) pendingMoneyBefore[s] = money[s];
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

        // 2026-08-23(design.md §49.4): 파산한 좌석이 있으면 두 갈래로
        // 갈린다 — CanDowngrade가 참이면(오프라인, 내가 아닌 좌석이
        // 파산, 아직 2인보다 위) 그 좌석만 빼고 남은 인원·잔액 그대로
        // 계속한다("자동 다운그레이드"). 아니면(네트워크거나, 내가
        // 파산했거나, 이미 2인이면) 예전처럼 그 판을 끝으로 세션을
        // 종료한다 — 이땐 다음에 다시 열었을 때 0원으로 영구히 막히지
        // 않도록 전 좌석 잔액을 초기 자금으로 되돌린다.
        //
        // 이름(SeatName)은 좌석 번호 기준이라 ApplyDowngrade가 좌석을
        // 재배치하면 더 이상 같은 사람을 안 가리킨다 — 그래서 표시용
        // 문자열은 전부 재배치 *전에* 미리 뽑아 둔다.
        var bankruptSeats = BankruptSeats();
        bool downgrade = CanDowngrade(bankruptSeats);
        string bankruptNames = bankruptSeats.Count > 0 ? string.Join(", ", bankruptSeats.Select(SeatName)) : null;

        // 2026-08-24(design.md §49.4 네트워크 확장) — 판 도중 재접속 유예를
        // 넘겨 영구 이탈이 확정된 좌석(permaGoneNetworkSeat)이 있으면, 이
        // 판이 끝나는 시점에 그 좌석을 뺀 채로 압축한다. 파산 다운그레이드와
        // 같은 ApplyDowngrade를 재사용하되(둘 다 "좌석 하나를 빼고 남은
        // 잔액 그대로 압축"이라는 같은 동작이라 그대로 쓸 수 있다), 네트워크
        // 전용으로 남은 접속자들에게 새 좌석 번호를 반드시 알려야 한다(안
        // 그러면 게임은 새 번호를 쓰는데 실제 메시지는 옛 소켓으로 계속
        // 오간다). 남은 인원이 2명 미만이 되면 압축이 무의미해(1인 게임은
        // 성립하지 않는다) 예전처럼 판을 끝낸다.
        var permaGoneSeats = isNetworkHost ? Enumerable.Range(0, SEATS).Where(s => permaGoneNetworkSeat[s]).ToList() : new List<int>();
        bool networkDowngrade = permaGoneSeats.Count > 0 && SEATS - permaGoneSeats.Count >= 2;
        string permaGoneNames = permaGoneSeats.Count > 0 ? string.Join(", ", permaGoneSeats.Select(SeatName)) : null;

        if (!downgrade && bankruptSeats.Count > 0)
        {
            for (int s = 0; s < SEATS; s++) money[s] = STARTING_MONEY;
            foreach (var s in bankruptSeats) allInCount[s]++;
            // 네트워크 판은 로컬 저장을 안 한다(Start()와 같은 이유 — 매판
            // 접속하는 사람이 달라질 수 있어 "이 기기의 좌석 N 잔액"이라는
            // 개념이 안 맞는다). downgrade 분기는 ApplyDowngrade가 저장까지
            // 알아서 한다.
            if (!isNetworkHost && !isNetworkGuest) SaveMoney();
        }

        string title = winnerSeat == PLAYER_SEAT ? "승리!" : $"{SeatName(winnerSeat)} 승리";
        Color col = winnerSeat == PLAYER_SEAT ? new Color(.93f, .73f, .18f) : new Color(.55f, .55f, .60f);
        // "이번 판 얼마를 벌었는지/잃었는지"가 최종 잔액만으론 안 보인다는 요청 —
        // 정산 직전 스냅샷(pendingMoneyBefore) 대비 내 변동을 부호와 함께 보여준다.
        // (파산으로 세션이 끝나는 판은 위에서 이미 전 좌석 money를 리셋했으므로
        // 이 delta는 "리셋 후" 기준이 된다 — 아래에서 별도 파산 문구로 구분한다.)
        int myDelta = money[PLAYER_SEAT] - pendingMoneyBefore[PLAYER_SEAT];
        string myDeltaStr = myDelta == 0 ? "변동 없음" : (myDelta > 0 ? $"+{myDelta:N0}원" : $"{myDelta:N0}원");
        string moneyLine = $"이번 판 {myDeltaStr} · 내 머니 {money[PLAYER_SEAT]:N0}원";
        string sub = dokbakIdx >= 0 ? $"{SeatName(loserSeats[dokbakIdx])} 독박 · {moneyLine}" : moneyLine;

        ui?.SetScore(money[PLAYER_SEAT]); // 상단 HUD의 SCORE는 판점이 아니라 내 보유 머니를 보여준다(사용자 요청)
        if (permaGoneSeats.Count > 0 && !networkDowngrade)
        {
            // 압축해도 2명 미만이 남는다 — 더 이어갈 수 없다(design.md
            // §49.4 "방 폭파"). §50.2 확장 전의 OnGuestLeftDuringGame
            // 즉시-종료 동작을 그대로 재사용한다.
            sub += $" · {permaGoneNames} 연결이 끊겨 더 이상 진행할 수 없습니다";
            ui?.ShowOverlay(col, title, finalScore.ToString(), sub, "타이틀", GoToTitle); // "다시 시작" 없음
            GoStopNetLobby.Instance?.BroadcastToGuests(
                new GoStopNetMessage { type = GoStopNetMessage.Type.Bye, text = $"{permaGoneNames} 연결이 끊겨 게임을 종료합니다." });
        }
        else if (networkDowngrade)
        {
            // 좌석을 재배치하기 전에 old→new 매핑부터 계산해 둔다 —
            // ApplyDowngrade가 SEATS/좌석 번호를 바꾸고 나면 "누가 몇 번
            // 이었는지"를 더 이상 알 수 없다.
            var oldToNew = new Dictionary<int, int>();
            int next = 0;
            for (int s = 0; s < SEATS; s++) { if (permaGoneSeats.Contains(s)) continue; oldToNew[s] = next++; }

            ApplyDowngrade(permaGoneSeats); // 파산 다운그레이드와 동일한 압축 로직 재사용(이유 무관 — "좌석 하나 빼고 압축"은 같은 동작)
            System.Array.Clear(permaGoneNetworkSeat, 0, SEATS_MAX); // 살아남은 좌석 기준으로 전부 리셋 — 낡은 인덱스가 다른 사람을 가리키면 안 된다

            // 트랜스포트의 좌석↔소켓 매핑도 같이 다시 붙이고, 남은 각
            // 접속자에게 새 좌석 번호를 알린다(호스트 자신=좌석0은 항상
            // 자기 자신이라 알릴 대상이 아니다).
            GoStopNetLobby.Instance?.RenumberSeats(oldToNew);
            foreach (var kv in oldToNew)
            {
                if (kv.Key == 0) continue; // 호스트 자신
                GoStopNetLobby.Instance?.SendToSeat(kv.Value, GoStopNetMessage.SeatReassignMsg(kv.Value, SEATS));
            }

            sub += $" · {permaGoneNames} 연결이 끊겨 퇴장 — 남은 {SEATS}명으로 계속합니다";
            ui?.ShowOverlay(col, title, finalScore.ToString(), sub,
                "다시 시작", NewGame, "타이틀", GoToTitle, "점수 상세", ShowScoreDetail);
        }
        else if (downgrade)
        {
            // 표시 문자열은 다 만들었으니 이제 실제로 좌석을 재배치한다 —
            // 이 아래로는 SEATS/좌석 번호가 이미 새 구성이다.
            ApplyDowngrade(bankruptSeats);
            sub += $" · {bankruptNames} 잔액을 모두 잃어 퇴장 — 남은 {SEATS}명으로 계속합니다";
            ui?.ShowOverlay(col, title, finalScore.ToString(), sub,
                "다시 시작", NewGame, "타이틀", GoToTitle, "점수 상세", ShowScoreDetail);
        }
        else if (bankruptSeats.Count > 0)
        {
            sub += $" · {bankruptNames} 잔액을 모두 잃어 이 판을 끝으로 세션을 종료합니다";
            ui?.ShowOverlay(col, title, finalScore.ToString(), sub, "타이틀", GoToTitle); // "다시 시작" 없음
        }
        else
        {
            ui?.ShowOverlay(col, title, finalScore.ToString(), sub,
                "다시 시작", NewGame, "타이틀", GoToTitle, "점수 상세", ShowScoreDetail);
        }

        if (isNetworkHost)
            BroadcastGameOverState(false, winnerSeat, finalScore, dokbakIdx >= 0 ? loserSeats[dokbakIdx] : -1, bankruptSeats.ToArray());
    }

}
