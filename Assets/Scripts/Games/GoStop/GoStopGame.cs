using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 고스톱(맞고) v3 — 싱글플레이(플레이어 vs 컴퓨터), 부가 규칙 포함.
///
/// 최종 목표는 로컬 네트워크 대전이지만, 그 전에 규칙 엔진이 맞는지 혼자
/// 확인할 수 있어야 한다. 턴 진행·연출만 여기서 담당하고, 승패를 가르는
/// 계산은 전부 <see cref="GoStopRules"/>에 위임한다.
///
/// v3에서 추가된 것: 상대 손패 뒷면 표시, 획득패를 광/열끗/띠/피 4줄로 분리
/// 표시, 뻑(필드 3장 쌓임) 인식, 폭탄으로 인한 손패 불균형 시 턴이 멈추던
/// 버그 수정, 공용 HUD 토스트 영역을 피하도록 레이아웃 재배치.
/// </summary>
public partial class GoStopGame : MonoBehaviour
{
    [SerializeField] GoStopUIManager ui; // 2026-08-22: 공용 GameUIManager에서 분리 — GoStopUI.prefab 전용

    enum State { PlayerTurn, AiTurn, GoStopChoice, GameOver }
    State state;

    List<HwatuCard> playerHand, aiHand, field, drawPile;
    List<HwatuCard> playerCaptured, aiCaptured;
    int playerGoCount, aiGoCount, playerSweeps, aiSweeps, playerHeundeul, aiHeundeul;

    // 판돈 — 점당 100원으로 정산한다.
    // 2026-08-18: "다시 시작해도 이전 잔액으로" 요청으로 PlayerPrefs에 영구
    // 저장하도록 바꿨다 — 예전엔 Start()에서 매번 10만원으로 리셋해서 씬을
    // 나갔다 들어오거나 앱을 재시작하면 그전 판돈이 사라졌다. 지금은 Start()가
    // 저장된 값이 있으면 그걸 불러오고, 없으면(첫 실행) 10만원으로 시작한다.
    // 어느 한쪽이 0원 이하가 되면 예전엔 세션이 끝났는데, 지금은 5만원을
    // 리필해서 계속 이어간다(사용자 요청) — 세션 종료 대신 "올인" 횟수만 기록.
    const int STARTING_MONEY = 100_000;
    const int REFILL_MONEY = 50_000;
    const int WON_PER_POINT = 100;
    const string PlayerMoneyKey = "GoStop2P_PlayerMoney";
    const string AiMoneyKey = "GoStop2P_AiMoney";
    const string PlayerAllInKey = "GoStop2P_PlayerAllIn";
    const string AiAllInKey = "GoStop2P_AiAllIn";
    int playerMoney, aiMoney;
    int playerAllInCount, aiAllInCount;

    /// <summary>돈이 바뀔 때마다 바로 저장한다 — 앱이 갑자기 꺼져도(정상 종료
    /// 흐름을 안 타도) 마지막 상태가 남게 하려고 라운드 끝에 한 번만 저장하지
    /// 않는다.</summary>
    void SaveMoney()
    {
        PlayerPrefs.SetInt(PlayerMoneyKey, playerMoney);
        PlayerPrefs.SetInt(AiMoneyKey, aiMoney);
        PlayerPrefs.SetInt(PlayerAllInKey, playerAllInCount);
        PlayerPrefs.SetInt(AiAllInKey, aiAllInCount);
        PlayerPrefs.Save();
    }

    /// <summary>이번 판 정산 후 0원 이하가 된 쪽을 <see cref="REFILL_MONEY"/>로
    /// 채우고 올인 횟수를 1 늘린다. 둘 다(드물지만 이론상) 0원 이하일 수 있어
    /// 독립적으로 각각 확인한다. 실제로 리필이 일어났으면 true를 돌려준다 —
    /// 호출부가 결과 문구에 반영할 수 있도록.</summary>
    bool RefillIfBankrupt()
    {
        bool refilled = false;
        if (playerMoney <= 0) { playerMoney = REFILL_MONEY; playerAllInCount++; refilled = true; }
        if (aiMoney <= 0) { aiMoney = REFILL_MONEY; aiAllInCount++; refilled = true; }
        return refilled;
    }
    // 나가리(무승부)가 나면 다음 판 판돈이 2배가 된다 — 연속 나가리면 배로
    // 계속 불어난다(2→4→8…). 결판이 나는 순간(누가 이기든) 1로 리셋한다.
    // Start()에서만 초기화하고 NewGame()에서는 안 건드린다 — 나가리→다음 판
    // 경계를 넘어 유지돼야 의미가 있다.
    int stakeMultiplier = 1;
    // 폭탄을 쓰면 손이 2장 짧아진다 — 그 보상으로 "이후 최대 2번, 손을 안 내고
    // 덱만 넘길 수 있는 권리"를 번다(강제 아님, 매 턴 본인 선택). 원문:
    // "판단에 따라 2번까지 패를 내려놓지 않고 더미에서 뒤집기만 할 수 있다."
    int playerBombCredits, aiBombCredits;
    readonly HashSet<int> playerShook = new(), aiShook = new();

    // 뻑 — 손패가 필드 1장과 매칭됐는데 곧바로 뒤집은 더미패도 같은 달이면
    // 아무도 못 먹고 3장이 그대로 필드에 쌓인다("싸다"). 나중에 그 달의
    // 마지막 한 장이 나와야 4장을 한 번에 쓸어간다(기존 matchCount==3 경로
    // 재사용). 이 사전은 "그 달 뻑을 누가 만들었는지" 기억해 뒀다가, 나중에
    // 해소한 사람이 causer와 같으면(자뻑) 피를 2장, 다르면 1장 가져가게 한다.
    readonly Dictionary<int, bool> ppeokCauser = new(); // 월 → 만든 쪽(true=플레이어)
    // 뻑 무더기에 같이 묻힌 보너스피 — 그 뻑을 나중에 해소하는 쪽이
    // ppeokCauser의 피 뺏기와 함께 이 카드도 가져간다(사용자 확인 규칙).
    readonly Dictionary<int, HwatuCard> ppeokBonusPi = new();
    // 비상 시스템 — (내 쪽인지, 세트 인덱스[0=고도리 1=홍단 2=초단 3=청단])
    // 조합이 이번 판에 이미 한 번 발동했는지. 4인판과 같은 이유로 좌석당
    // 세트당 한 번만 울린다.
    readonly HashSet<(bool isPlayerSide, int setIdx)> emergencyFired = new();
    const int PPEOK_MONEY_POINTS = 3; // 첫뻑/연뻑 즉시 획득 금액 = 3점 상당(점수엔 안 들어감)
    bool isFirstPlayOfRound; // 이번 판 첫 카드였는지 — 첫뻑/첫따닥 판정용, 소비되면 false
    int playerPpeokStreak, aiPpeokStreak; // 자기 턴에서 연속으로 뻑을 낸 횟수(뻑이 아니면 0으로 리셋) — "연뻑" 보너스 판정용
    // "쓰리뻑"(뻑을 3번 하면 그 자리에서 즉시 승리)은 구글링으로 확인한
    // 표준 규칙상 연속이 아니라 이번 판 통산 횟수다 — 연속용 스트릭과는
    // 별도 카운터가 필요하다(리셋 없음, NewGame에서만 0).
    int playerPpeokTotal, aiPpeokTotal;
    int playerBombCount, aiBombCount; // 이번 판 폭탄 횟수(최종 정산 배수용 — 크레딧과는 별개)

    // 역고 — "고를 부르는 쪽"이 이번 판에서 바뀐 횟수. 상대가 먼저 고를
    // 부른 뒤 내가 앞질러서 고를 부르면 역전(리버설)이다. 결판 시점에
    // goLeader가 곧 최종 승자와 같은 쪽이면(=마지막으로 고를 부른 쪽이
    // 이겼으면) 이 값을 역고 배수 계산에 쓴다.
    bool? goLeader;
    int goReversalCount;

    const string BestKey = "BestGoStop";
    // v4: "카드가 너무 작다" 피드백으로 전체적으로 키웠다.
    // v5-2: 그래도 AiCap/PlayerCap이 안 보인다는 재신고 — ContentArea 실측
    // 높이(GetWorldCorners로 확인, 964px)를 다시 보니 v4 배치가 바닥에
    // 138px이나 그냥 남겨두고 있었다("손패 아래는 여유 없다"고 지레짐작했던
    // 게 틀렸다). 그 여유를 전부 획득패 카드에 쓴다 — 자세한 계산 근거는
    // BuildStaticUI 주석 참고.
    const float FIELD_W = 92f, FIELD_H = 114f;
    const float HAND_W  = 88f, HAND_H  = 136f;
    const float CAP_W   = 36f, CAP_H   = 52f;
    const float BACK_W  = 30f, BACK_H  = 44f;
    const float PILE_W  = 58f, PILE_H  = 84f;  // 더미는 상대 손패 뒷면(BACK_*)보다 눈에 띄게 크게 — "더미가 잘 안 보인다" 피드백
    const float CAP_ROW_PITCH = 52f;   // 획득패 4줄(광/열끗/띠/피) 한 줄 높이

    RectTransform fieldArea, handArea, playerCapArea, aiCapArea, aiBackArea, drawPileArea;
    TextMeshProUGUI aiInfoText, aiSetText, playerSetText;
    TextMeshProUGUI aiMoneyText, playerMoneyText;

    // 팝업 4종 — 전부 Assets/Resources/Prefabs/GoStop/Popups/의 실제 .prefab
    // 에셋을 Instantiate해서 쓴다(2026-08-18 리스킨 때 런타임 코드 생성에서
    // 전환 — "생성되는 애들은 별도 프리펩으로 저장해달라"는 요청). 구조·색·
    // 정적 문구는 프리팹에 구워져 있고, 이 스크립트는 동적인 부분(문구 갱신,
    // 버튼 콜백, 카드 채우기)만 인스턴스화 직후 연결한다 — 프리팹이 씬 스크립트를
    // 직렬화 참조할 수 없다는 이 프로젝트의 기존 제약과 같은 이유
    // (GameUIManager의 런타임 등록 패턴 참고).
    ModalTwoButtonPopup shakePopup;
    HwatuCard pendingShakeCard;

    CardChoicePopup fieldChoicePopup;
    HwatuCard pendingFieldChoice;

    // 9월 열끗(국화, dualPi) 열끗/쌍피 선택 팝업 — 내 획득패에 새로 들어오는
    // "그 순간"에만 한 번 묻는다. 예전엔 획득패에서 아무 때나 클릭해 토글할
    // 수 있었는데, "가져올 때 정하고 그 뒤엔 못 바꾸는 게 맞다"는 피드백으로
    // 시점을 캡처 순간으로 좁혔다.
    ModalTwoButtonPopup dualPiPopup;
    bool? pendingDualPiChoice; // null=대기, false=열끗, true=쌍피

    // 점수 상세 팝업 — 게임오버 오버레이의 "점수 상세" 버튼에서 연다.
    // "왜 이 점수가 나왔는지" 신고를 받아 추가했다. 분석 대상은 항상
    // "이긴 쪽"(EndGame이 breakdown을 그 쪽 캡처로 계산한다) — 누구인지는
    // pendingBreakdownIsPlayer로 표시하고, 진 쪽의 기본 점수는 참고용으로
    // pendingOtherBaseScore에 따로 담아둔다(정산 배수가 안 붙는 값이라 섞으면 안 된다).
    ScoreDetailPopup scoreDetailPopup;
    GoStopRules.ScoreBreakdown pendingBreakdown;
    bool pendingBreakdownIsPlayer;
    int pendingOtherBaseScore;

    /// <summary>
    /// 이번 턴에 "어디서 날아왔는지"를 아는 카드들 — 손에서 낸 카드(핸드 슬롯
    /// 월드 좌표)와 더미에서 뒤집은 카드(더미 위치)가 여기 등록된다. RebuildUI가
    /// 카드를 새로 그릴 때 이 사전에 있으면 그 위치에서 날아와 딱 맞고 튕기는
    /// 연출(SlamIn)을 태우고, 없으면(기존에 이미 놓여 있던 카드) 그냥 제자리에
    /// 나타난다. 딱지치기처럼 "쳐서 맞춘다"는 느낌을 주려고 넣었다 — 순간이동
    /// 처럼 나타나던 예전 방식은 손맛이 하나도 없었다.
    /// </summary>
    readonly Dictionary<HwatuCard, Vector3> flyFrom = new();

    /// <summary>
    /// 필드에서 짝을 맞춰 가져온 카드들 — 값이 그 "맞은 필드패"가 있던 자리다.
    /// <see cref="flyFrom"/>과 같이 쓰인다(시작점은 flyFrom, 중간에 들를 필드
    /// 자리는 이것). 1:1로 딱 하나만 맞춘 캡처(일반 매칭, 선택 캡처로 고른
    /// 경우, 쪽)에서만 채운다 — 뻑 해소·폭탄처럼 여러 장이 한꺼번에 딸려오는
    /// 경우는 "어느 한 장을 쳤다"고 하기 애매해서 그냥 바로 날아간다.
    /// </summary>
    readonly Dictionary<HwatuCard, Vector3> flyViaField = new();

    // ── 네트워크 대전 (2026-08-20) ────────────────────────
    // 호스트 권위 모델(GoStop3PGame과 동일한 원칙) — 호스트만 진짜
    // GoStopRules 판정을 돌리고, 게스트는 매 RebuildUI마다 오는 스냅샷을
    // 그대로 받아 그리기만 한다. 다만 이 파일은 좌석 배열이 아니라
    // playerXxx/aiXxx로 이름 붙은 개별 필드라 GoStop3PGame과 구조가 다르다
    // — 대신 "player=나(이 화면을 보는 사람), ai=상대"라는 규칙이 이미
    // 싱글플레이에서 성립해 있으므로, 네트워크에서도 그 규칙을 그대로
    // 유지하려면 <b>호스트 입장에서 'ai' 역할을 실제로 조종하는 게 접속한
    // 게스트</b>라는 것만 정하면 된다 — 게스트 자신의 화면에서는 스냅샷을
    // 받을 때 player↔ai를 뒤바꿔서 적용한다(<see cref="ApplyNetworkSnapshot"/>
    // 문서 참고). 그러면 RebuildUI·OnPlayerPlay 등 기존 코드를 단 한 줄도
    // 안 바꾸고 "내 화면엔 항상 내 손패가 아래에 나온다"가 그대로 성립한다.
    bool isNetworkHost, isNetworkGuest;
    bool aiGoStopPendingFlag; // 호스트 전용 — "ai(=게스트)의 고/스톱 결정을 기다리는 중"
    bool goStopOverlayShown, gameOverOverlayShown; // 게스트 전용 — 오버레이 중복 표시 방지
    bool guestSeesOpponentDeciding; // 게스트 전용 — snap.hostGoStopPending을 옮겨 담는다(BuildTurnIndicator가 읽는다)

    void Awake()
    {
        var lobby = GoStopNetLobby.Instance;
        // PlayerCount==2일 때만 반응한다 — 3~4인으로 시작된 판이 실수로 이
        // 씬을 열 일은 없지만(HandleGameStarting이 인원수로 씬을 가른다),
        // 방어적으로 이 씬이 정말 2인 네트워크 판일 때만 네트워크 분기를 켠다.
        if (lobby != null && lobby.PlayerCount == 2)
        {
            isNetworkHost = lobby.IsHost;
            isNetworkGuest = lobby.IsGuest;
            lobby.OnGameMessage += OnNetGameMessage;
            if (isNetworkHost) lobby.OnGuestLeftDuringGame += OnGuestLeftDuringGame;
            if (isNetworkGuest) lobby.OnDisconnected += OnHostDisconnected;
        }
    }

    void OnDestroy()
    {
        if (GoStopNetLobby.Instance != null)
        {
            GoStopNetLobby.Instance.OnGameMessage -= OnNetGameMessage;
            GoStopNetLobby.Instance.OnGuestLeftDuringGame -= OnGuestLeftDuringGame;
            GoStopNetLobby.Instance.OnDisconnected -= OnHostDisconnected;
        }
    }

    /// <summary>호스트 전용 — 게스트(항상 좌석 1, 2인판은 게스트가 한
    /// 명뿐이다)의 다음 메시지를 기다린다. 받는 즉시 구독을 해제한다.</summary>
    IEnumerator WaitForRemoteMessage(System.Func<GoStopNetMessage, bool> accept, System.Action<GoStopNetMessage> onReceived)
    {
        GoStopNetMessage received = null;
        void Handler(int fromSeat, GoStopNetMessage msg)
        {
            if (fromSeat == 1 && (accept == null || accept(msg))) received = msg;
        }
        GoStopNetLobby.Instance.OnGameMessage += Handler;
        yield return new WaitUntil(() => received != null);
        GoStopNetLobby.Instance.OnGameMessage -= Handler;
        onReceived(received);
    }

    /// <summary>호스트·게스트 공용 진입점 — 게스트가 호스트로부터 받는
    /// StateSync/Event/Bye만 처리한다(호스트 쪽 메시지는 각 WaitForRemoteMessage
    /// 호출이 직접 구독해서 가져간다).</summary>
    void OnNetGameMessage(int fromSeat, GoStopNetMessage msg)
    {
        if (!isNetworkGuest) return;
        switch (msg.type)
        {
            case GoStopNetMessage.Type.StateSync:
                var snap = JsonUtility.FromJson<GoStopStateSnapshot2P>(msg.text);
                if (snap != null) ApplyNetworkSnapshot(snap);
                break;
            case GoStopNetMessage.Type.Event:
                // msg.seat: 0=호스트(player) 쪽 행동, 1=게스트(ai) 쪽 행동 —
                // 게스트 자신은 항상 "player" 역할로 보이므로 seat==1이 곧
                // "내가 한 일"이다.
                Toast(msg.seat == 1, msg.text);
                break;
            case GoStopNetMessage.Type.Bye:
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

    /// <summary>호스트 전용 — 접속해 있던 게스트가 판 도중 나갔다. 남은
    /// 혼자서는 계속할 수 없으므로(그 사람 메시지를 영원히 기다리며 멈추는
    /// 게 최악이다) 판을 즉시 끝내고 타이틀로 돌아갈 길을 안내한다.</summary>
    void OnGuestLeftDuringGame(int seat)
    {
        if (state == State.GameOver) return;
        state = State.GameOver;
        ui?.ShowOverlay(new Color(.8f, .35f, .3f), "연결 끊김", "-",
            "상대의 연결이 끊어져 이번 판을 종료합니다.", "타이틀", GoToTitle);
        GoStopNetLobby.Instance?.BroadcastToGuests(
            new GoStopNetMessage { type = GoStopNetMessage.Type.Bye, text = "상대의 연결이 끊어져 이번 판이 종료됐습니다." });
    }

    /// <summary>게스트 전용 — 호스트와의 TCP 연결 자체가 끊겼다.</summary>
    void OnHostDisconnected(string reason)
    {
        if (state == State.GameOver) return;
        state = State.GameOver;
        ui?.ShowOverlay(new Color(.8f, .35f, .3f), "연결 끊김", "-",
            "호스트와의 연결이 끊어졌습니다.", "타이틀", GoToTitle);
    }

    GoStopStateSnapshot2P BuildSnapshot() => new GoStopStateSnapshot2P
    {
        state = (int)state,
        playerHand = GoStopDeck.EncodeAll(playerHand ?? new List<HwatuCard>()),
        aiHand = GoStopDeck.EncodeAll(aiHand ?? new List<HwatuCard>()),
        field = GoStopDeck.EncodeAll(field ?? new List<HwatuCard>()),
        drawPileCount = drawPile?.Count ?? 0,
        playerCaptured = GoStopDeck.EncodeAll(playerCaptured ?? new List<HwatuCard>()),
        aiCaptured = GoStopDeck.EncodeAll(aiCaptured ?? new List<HwatuCard>()),
        playerGoCount = playerGoCount,
        aiGoCount = aiGoCount,
        playerSweeps = playerSweeps,
        aiSweeps = aiSweeps,
        playerBombCredits = playerBombCredits,
        aiBombCredits = aiBombCredits,
        playerMoney = playerMoney,
        aiMoney = aiMoney,
        playerShookMonths = new List<int>(playerShook).ToArray(),
        aiShookMonths = new List<int>(aiShook).ToArray(),
        aiGoStopPending = aiGoStopPendingFlag,
        hostGoStopPending = (state == State.GoStopChoice), // 호스트 자신(player)의 고/스톱 결정 중
    };

    /// <summary>호스트 전용 — 매 RebuildUI 끝에서 부른다(로컬 화면이 갱신되는
    /// 시점과 정확히 같은 타이밍에 게스트도 갱신되게).</summary>
    void BroadcastNetworkState()
    {
        var lobby = GoStopNetLobby.Instance;
        if (lobby == null) return;
        lobby.BroadcastToGuests(new GoStopNetMessage { type = GoStopNetMessage.Type.StateSync, text = JsonUtility.ToJson(BuildSnapshot()) });
    }

    /// <summary>필드선택/9월열끗처럼 "지금 게스트(ai 역할) 한 명만 결정해야
    /// 하는" 순간을 그 사람에게만 알린다. 2인판은 게스트가 항상 좌석 1
    /// 하나뿐이라 GoStop3PGame처럼 좌석을 인자로 받을 필요가 없다.</summary>
    void SendTargetedPrompt(System.Action<GoStopStateSnapshot2P> configure)
    {
        var lobby = GoStopNetLobby.Instance;
        if (lobby == null) return;
        var snap = BuildSnapshot();
        configure(snap);
        lobby.SendToSeat(1, new GoStopNetMessage { type = GoStopNetMessage.Type.StateSync, text = JsonUtility.ToJson(snap) });
    }

    /// <summary>호스트 전용 — EndGame은 RebuildUI를 거치지 않으므로 게스트에게
    /// 판이 끝난 걸 따로 알려야 한다(GoStop3PGame의 BroadcastGameOverState와
    /// 같은 이유).</summary>
    void BroadcastGameOverState(bool isNagari, bool aiWonForGuest, int finalScoreValue, int stakeMultiplierValue)
    {
        var lobby = GoStopNetLobby.Instance;
        if (lobby == null) return;
        var snap = BuildSnapshot();
        snap.gameOverActive = true;
        snap.gameOverIsNagari = isNagari;
        snap.gameOverAiWon = aiWonForGuest;
        snap.gameOverFinalScore = finalScoreValue;
        snap.gameOverStakeMultiplier = stakeMultiplierValue;
        lobby.BroadcastToGuests(new GoStopNetMessage { type = GoStopNetMessage.Type.StateSync, text = JsonUtility.ToJson(snap) });
    }

    /// <summary>게스트 전용 — 호스트가 보낸 스냅샷을 내 필드에 적용한다.
    /// <b>player↔ai를 반드시 뒤바꿔서 받는다</b> — 호스트 입장에서 "ai"는
    /// 실제로는 이 게스트 자신이 조종하는 자리이므로, 이 화면에서는 그
    /// 데이터가 손패가 늘 나오는 <c>player</c> 필드에 들어가야 아래쪽에
    /// 정상적으로 보인다. state도 마찬가지로 뒤집어 해석한다 — 호스트의
    /// "AiTurn"이 곧 이 게스트 자신의 차례다. 정확히 GoStopStateSnapshot2P
    /// 클래스 문서에 적어둔 규칙 그대로다.</summary>
    void ApplyNetworkSnapshot(GoStopStateSnapshot2P snap)
    {
        playerHand = GoStopStateSnapshot2P.Dec(snap.aiHand);
        aiHand = GoStopStateSnapshot2P.Dec(snap.playerHand);
        field = GoStopStateSnapshot2P.Dec(snap.field);
        drawPile = new List<HwatuCard>();
        for (int i = 0; i < snap.drawPileCount; i++)
            drawPile.Add(new HwatuCard(0, HwatuKind.Pi, "Joker_1", piValue: 1, isJoker: true));
        // ↑ 더미는 개수만 온다 — 뒤집히기 전엔 어떤 카드인지 화면에서
        // 볼 일이 없다(GoStopStateSnapshot 3~4인판과 같은 이유).

        playerCaptured = GoStopStateSnapshot2P.Dec(snap.aiCaptured);
        aiCaptured = GoStopStateSnapshot2P.Dec(snap.playerCaptured);

        playerGoCount = snap.aiGoCount; aiGoCount = snap.playerGoCount;
        playerSweeps = snap.aiSweeps; aiSweeps = snap.playerSweeps;
        playerBombCredits = snap.aiBombCredits; aiBombCredits = snap.playerBombCredits;
        playerMoney = snap.aiMoney; aiMoney = snap.playerMoney;

        playerShook.Clear();
        if (snap.aiShookMonths != null) foreach (var m in snap.aiShookMonths) playerShook.Add(m);
        aiShook.Clear();
        if (snap.playerShookMonths != null) foreach (var m in snap.playerShookMonths) aiShook.Add(m);

        state = SwapStateForGuest(snap.state);
        guestSeesOpponentDeciding = snap.hostGoStopPending;

        RebuildUI();

        // 타깃 프롬프트 — 필드선택/9월열끗은 정규 스냅샷 필드만으로는
        // "지금 내가 결정해야 한다"가 안 드러나서 별도로 얹어 보낸다.
        if (snap.fieldChoiceCandidates != null && snap.fieldChoiceCandidates.Length > 0)
            ShowFieldChoicePopup(GoStopStateSnapshot2P.Dec(snap.fieldChoiceCandidates));
        if (snap.dualPiChoicePending)
            dualPiPopup.Show();

        // 고/스톱은 aiGoStopPending 플래그 하나로 충분하다 — ShowGoStopPrompt를
        // 그대로 재사용한다(OnPlayerGo/OnPlayerStop이 이미 게스트 분기를
        // 갖고 있어서 손댈 필요가 없다).
        if (snap.aiGoStopPending)
        {
            if (!goStopOverlayShown)
            {
                goStopOverlayShown = true;
                int score = GoStopRules.CalcScore(playerCaptured, playerSweeps).Total;
                ShowGoStopPrompt(score);
            }
        }
        else
        {
            if (goStopOverlayShown) ui?.HideOverlay();
            goStopOverlayShown = false;
        }

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

    static State SwapStateForGuest(int hostState) => (State)hostState switch
    {
        State.PlayerTurn => State.AiTurn,      // 호스트(=상대) 차례 → 내겐 상대 턴
        State.AiTurn => State.PlayerTurn,      // ai(=나) 차례 → 내 턴
        State.GoStopChoice => State.AiTurn,    // 호스트 자신의 고/스톱 결정 중 → 나는 기다림
        _ => State.GameOver,
    };

    /// <summary>네트워크 대전에서 "지금 누구 차례인지 모르겠다"는 신고로
    /// 추가했다 — 싱글플레이는 손패 상호작용(눌리는 카드 강조)만으로도
    /// 충분히 자기 차례를 알 수 있었지만, 실제 사람 둘이 붙으면 상대
    /// 화면에서 뭘 하는지 전혀 안 보여서 "게임이 멈췄나?" 싶은 순간이
    /// 많다. 우선순위: 상대가 고/스톱 결정 중(가장 구체적) → 내가
    /// 고/스톱 결정할 차례 → 일반 턴(state 그대로). 싱글플레이(둘 다
    /// false)면 null — 기존 정적 타이틀을 그대로 둔다.</summary>
    string BuildTurnIndicator()
    {
        if (!isNetworkHost && !isNetworkGuest) return null;
        if (isNetworkHost && aiGoStopPendingFlag) return "상대가 고/스톱을 선택 중입니다";
        if (isNetworkGuest && guestSeesOpponentDeciding) return "상대가 고/스톱을 선택 중입니다";
        if (state == State.GoStopChoice) return "고/스톱을 선택해주세요";
        if (state == State.PlayerTurn) return "내 차례입니다";
        if (state == State.AiTurn) return "상대 차례입니다";
        return null;
    }

    /// <summary>게스트 전용 — 호스트의 EndGame과 동등한 화면을 내 관점으로
    /// 다시 조립한다. 정산은 호스트가 이미 다 끝냈고(money 필드에 이미
    /// 반영돼 있다) 여기서는 표시만 한다. "다시 시작"은 호스트만 누를 수
    /// 있어 버튼 자체를 안 보여준다.</summary>
    void ShowGuestGameOverOverlay(GoStopStateSnapshot2P snap)
    {
        if (snap.gameOverIsNagari)
        {
            ui?.ShowOverlay(new Color(.6f, .6f, .68f), "나가리", "-",
                $"무승부 — 아무도 {GoStopRules.CAPTURE_LINE}점을 못 넘겼습니다 · 다음 판 판돈 {snap.gameOverStakeMultiplier}배 (호스트가 다시 시작합니다)",
                "타이틀", GoToTitle);
            return;
        }

        bool iWon = snap.gameOverAiWon; // 게스트는 항상 ai 역할이므로 true=내가 이김
        string title = iWon ? "승리!" : "패배...";
        Color col = iWon ? new Color(.30f, .78f, .42f) : new Color(.86f, .32f, .30f);
        string sub = $"내 머니 {playerMoney:N0}원"; // playerMoney는 이미 스왑돼서 내 돈이다
        ui?.SetScore(playerMoney);
        ui?.ShowOverlay(col, title, snap.gameOverFinalScore.ToString(), sub, "타이틀", GoToTitle);
    }

    void Start()
    {
        // 게스트는 새 판을 직접 못 시작한다 — 호스트가 다음 판을 시작하면
        // 그 StateSync를 받아 화면이 알아서 바뀐다(GoStop3PGame과 같은 패턴).
        ui?.SetNewGameAction(isNetworkGuest ? (System.Action)null : NewGame);
        ui?.SetTitle(isNetworkHost || isNetworkGuest ? "맞고 (네트워크)" : "고스톱");
        ui?.SetBest(PlayerPrefs.GetInt(BestKey, 0));
        // #485F41 — 카드 테이블 느낌의 무광 올리브그린. 직접 골라준 값.
        ui?.SetBackground(new Color(0.282f, 0.373f, 0.255f));

        if (!isNetworkHost && !isNetworkGuest)
        {
            // 저장된 잔액이 있으면 이어서 쓰고, 없으면(첫 실행) 10만원으로 시작한다.
            playerMoney = PlayerPrefs.GetInt(PlayerMoneyKey, STARTING_MONEY);
            aiMoney = PlayerPrefs.GetInt(AiMoneyKey, STARTING_MONEY);
            playerAllInCount = PlayerPrefs.GetInt(PlayerAllInKey, 0);
            aiAllInCount = PlayerPrefs.GetInt(AiAllInKey, 0);
        }
        else
        {
            // 네트워크 판은 이 로컬 저장을 안 쓴다 — 매판 접속하는 실제
            // 사람이 달라질 수 있어 "이 기기의 잔액"이라는 개념이 안 맞는다
            // (GoStop3PGame과 같은 이유).
            playerMoney = aiMoney = STARTING_MONEY;
        }
        stakeMultiplier = 1;

        // 효과음 (절차적 생성 — 오디오 에셋 없음, BrickBreakerAudio와 같은 패턴).
        // 4인판과 파일을 공유한다(GoStopAudio.cs) — 이벤트 종류가 거의 같아서.
        if (GoStopAudio.Instance == null)
            new GameObject("GoStopAudio").AddComponent<GoStopAudio>();

        BuildStaticUI();
        // 게스트는 여기서 아무것도 시작 안 한다 — 호스트의 첫 StateSync가
        // OnNetGameMessage → ApplyNetworkSnapshot으로 손패를 채우고 화면을 그린다.
        if (!isNetworkGuest) NewGame();
    }

    /// <summary>타이틀로 나가기 전 네트워크 세션을 확실히 접는다 — 안 그러면
    /// 호스트는 타이틀로 돌아간 뒤에도 방을 계속 열어둔 채고, 게스트는 죽은
    /// TCP 연결을 계속 붙들고 있게 된다(GoStop3PGame과 같은 이유).</summary>
    void GoToTitle()
    {
        if (isNetworkHost || isNetworkGuest) GoStopNetLobby.Instance?.StopAll();
        ui?.GoBack();
    }

    // ── 판 시작 ──────────────────────────────────────────
    /// <summary>버튼/오버레이 콜백은 void 메서드를 기대하므로 코루틴을
    /// 감싸는 얇은 래퍼만 둔다(4인판 NewGame과 같은 패턴) — 실제 절차는
    /// <see cref="NewGameSeq"/>.</summary>
    public void NewGame() => StartCoroutine(NewGameSeq());

    IEnumerator NewGameSeq()
    {
        var deal = GoStopRules.DealNew();
        playerHand = deal.playerHand; aiHand = deal.aiHand;
        field = deal.field; drawPile = deal.drawPile;
        SortHand(playerHand); SortHand(aiHand);

        playerCaptured = new(); aiCaptured = new();
        playerGoCount = aiGoCount = playerSweeps = aiSweeps = playerHeundeul = aiHeundeul = 0;
        playerBombCredits = aiBombCredits = 0;
        playerBombCount = aiBombCount = 0;
        playerShook.Clear(); aiShook.Clear();
        ppeokCauser.Clear();
        ppeokBonusPi.Clear();
        emergencyFired.Clear();
        playerPpeokStreak = aiPpeokStreak = 0;
        playerPpeokTotal = aiPpeokTotal = 0;
        goLeader = null; goReversalCount = 0;
        isFirstPlayOfRound = true; // stakeMultiplier는 여기서 안 건드린다 — 나가리 다음 판까지 이어져야 한다.
        state = State.PlayerTurn;

        ui?.HideOverlay();
        ui?.SetScore(playerMoney); // 상단 HUD의 SCORE는 판점이 아니라 내 보유 머니를 보여준다(사용자 요청)

        // 딜링 연출 — 손패/필드가 아직 화면에 하나도 안 그려진 이 시점에만
        // 걸 수 있다. 2026-08-20 정정(사용자 신고) — 지난 판 필드/획득패가
        // 화면에 그대로 남아있으면 어색하다. 먼저 지우고, 더미 시각(레이어
        // 스택)만 채워서 카드가 실제로 그 더미에서 나오는 것처럼 보이게 한다.
        ClearBoardForDealing();
        RedrawDrawPile();
        yield return StartCoroutine(DealingAnimationSeq());

        RebuildUI();

        // 총통 — 딜 받은 손패에 같은 달 4장이 통째로 있으면 그 자리에서 즉시
        // 승리한다. 카드를 한 장도 안 낸 시점이라 캡처 점수가 없으므로 고정
        // 점수(3점 × 총통 배수 x4)로 정산한다.
        if (GoStopRules.IsChongtong(playerHand)) { EndGameChongtong(isPlayerSide: true); yield break; }
        if (GoStopRules.IsChongtong(aiHand)) { EndGameChongtong(isPlayerSide: false); yield break; }
    }

    /// <summary>
    /// 월 순으로 먼저, 같은 월이면 광·열끗·띠·피 순으로.
    /// 처음엔 종류를 1순위로 했더니 같은 달 카드끼리 서로 다른 종류 그룹에
    /// 흩어져 버렸다(매칭은 달로 하는데 손패에서 같은 달이 떨어져 보이면
    /// 오히려 헷갈린다) — 월을 1순위로 바꿔서 매칭 가능한 카드끼리 뭉치게 한다.
    /// </summary>
    static void SortHand(List<HwatuCard> hand) =>
        hand.Sort((a, b) => a.month != b.month ? a.month.CompareTo(b.month) : ((int)a.kind).CompareTo((int)b.kind));

    // ── 카드 한 장 내기 (플레이어/AI 공용) ──────────────────
    const float PLAY_STEP_DELAY = 0.35f; // SlamIn(0.27초)이 끝나고 살짝 여유를 두는 시간

    /// <summary>
    /// 캡처 결과가 "낸/뒤집은 카드 + 맞은 필드패 1장"(딱 2장) 형태면, 그 필드패가
    /// 있던 자리를 <see cref="flyViaField"/>에 기록해 둔다 — 다음 RebuildUI 때
    /// 그 자리를 거쳐 날아가는 2단 연출(SlamInViaField)이 걸린다. 이번 판 첫
    /// 리빌드가 지나가기 전에(필드 GameObject가 아직 살아있을 때) 불러야 한다.
    /// 3장 이상 딸려오는 뻑 해소/폭탄은 "어느 한 장을 쳤다"고 하기 애매해서
    /// 대상에서 뺀다(captured.Count==2가 아니면 조용히 아무것도 안 한다).
    /// </summary>
    void RegisterFlyViaField(GoStopRules.CaptureResult r)
    {
        if (r.captured.Count != 2) return;
        var mover = r.captured[0];
        var hit = r.captured[1];
        var hitGo = fieldArea.Find(hit.spriteName);
        if (hitGo != null) flyViaField[mover] = hitGo.position;
    }

    /// <summary>
    /// 흔들기 감지 → 폭탄 포함 캡처 해결(따닥·뻑·폭탄 피 뺏기) →
    /// (여기서 한 번 리빌드해서 <b>낸 카드가 먼저</b> 날아가 자리잡는 걸 보여준다) →
    /// 더미에서 한 장 뒤집어 같은 절차(+쪽 판정) → 다시 리빌드해서 <b>뒤집은
    /// 카드가 뒤이어</b> 날아가 자리잡는 걸 보여준다.
    /// <br/>
    /// 예전엔 손패 캡처와 덱 뒤집기를 한 함수 안에서 같이 처리하고 리빌드를
    /// 딱 한 번만 해서, 낸 카드와 뒤집힌 카드가 <b>동시에</b> 날아들어 뭐가
    /// 뭔지 순서가 안 읽혔다("친다"는 느낌이 없다는 신고). 코루틴으로 나눠
    /// 두 단계 사이에 <see cref="PLAY_STEP_DELAY"/>만큼 쉬어가는 게 핵심 수정이다.
    /// <br/>
    /// <paramref name="declareShake"/> — 손에 같은 달이 3장(이 카드 포함) 모였을 때
    /// 실제로 흔들기를 선언할지. 호출자가 미리 결정해서 넘긴다(플레이어는 팝업으로
    /// 물어보고, AI는 <see cref="GoStopAI.ShouldShake"/>로 즉시 정한다) — 조건이
    /// 안 맞으면(3장이 아니거나 이미 이 달로 결정을 내렸으면) 값과 무관하게 무시된다.
    /// </summary>
    IEnumerator PlayFromHandSeq(HwatuCard card, bool isPlayerSide, bool declareShake, System.Action onDone)
    {
        var hand = isPlayerSide ? playerHand : aiHand;
        var captured = isPlayerSide ? playerCaptured : aiCaptured;
        var opponentCaptured = isPlayerSide ? aiCaptured : playerCaptured;
        var shookMonths = isPlayerSide ? playerShook : aiShook;

        GoStopAudio.Instance?.CardPlay();

        // 이 카드가 어디서 날아왔는지 기록해 둔다 — 내 손이면 실제 슬롯 위치,
        // 상대 손이면 뒷면(장수만 표시, 개별 카드 GO가 없다) 뭉치의 자리를 대신 쓴다.
        var originSlot = isPlayerSide ? handArea.Find(card.spriteName) : null;
        flyFrom[card] = originSlot != null ? originSlot.position
                      : (isPlayerSide ? handArea.position : aiBackArea.position);

        if (hand.Count(c => c.month == card.month) == 3 && declareShake && shookMonths.Add(card.month))
        {
            if (isPlayerSide) playerHeundeul++; else aiHeundeul++;
            Toast(isPlayerSide, $"{card.month}월 흔들기");
        }

        // 이번 카드가 이번 판의 첫 수였는지 먼저 기록해 두고 바로 내린다 —
        // 첫뻑/첫따닥 판정에만 쓰고, 이후 카드들은 "첫"이 아니다.
        bool wasFirstPlay = isFirstPlayOfRound;
        isFirstPlayOfRound = false;

        var r1 = GoStopRules.ResolveWithBomb(card, hand, field, out bool bomb);

        // 필드에 같은 달이 2장 있으면(선택 캡처) 여기서 고르게 한다 — 골라야
        // matchCount/captured가 확정된다. 원래 2장 매칭이었다는 사실은 따로
        // 기억해 둔다: 고른 뒤엔 matchCount가 1로 바뀌어서(ResolveChoice가
        // 그렇게 만든다) 아래 뻑 감지 조건과 구분이 안 되기 때문이다 — 하지만
        // 선택을 거친 경우는 필드에 안 고른 1장이 그대로 남아 있어서 "필드에
        // 정확히 1장뿐"이라는 뻑의 전제가 깨진다. 그래서 뻑 감지에서 반드시
        // 제외해야 한다(안 그러면 그 달 4장이 전부 필드에 몰려 아무도 다시
        // 못 꺼내는 상태가 된다).
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
                yield return StartCoroutine(ContinueChoice(card, r1, isPlayerSide, res => chosen1 = res));
                r1 = chosen1;
                ddadakWatch = candidates.FirstOrDefault(c => !r1.captured.Contains(c));
                if (wasFirstPlay) { ApplyMoneyBonus(isPlayerSide, PpeokMoney()); Toast(isPlayerSide, "첫따닥"); }
            }
        }

        if (bomb) { if (isPlayerSide) playerBombCount++; else aiBombCount++; }

        int before1 = captured.Count;

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

        // 뻑 감지 — 이제 이미 공개된 drawn의 월을 직접 비교한다.
        bool ppeokFormed = !bomb && !r1HadChoice && r1.matchCount == 1
                           && drawn != null && !drawn.isJoker && drawn.month == card.month;
        if (ppeokFormed)
        {
            field.AddRange(r1.captured);
            field.Add(drawn);
            ppeokCauser[card.month] = isPlayerSide;

            int streak = isPlayerSide ? ++playerPpeokStreak : ++aiPpeokStreak;
            int total = isPlayerSide ? ++playerPpeokTotal : ++aiPpeokTotal;

            // 첫뻑/연뻑 둘 다 "3점에 해당하는 금액"으로 동일하다 — 점수에는
            // 안 들어가고 판돈만 그 자리에서 오간다. 쓰리뻑은 별도(아래) —
            // 돈이 아니라 고정 3점 즉시 승리로 정산된다.
            if (wasFirstPlay) { ApplyMoneyBonus(isPlayerSide, PpeokMoney()); Toast(isPlayerSide, "첫뻑"); }
            else if (streak == 2) { ApplyMoneyBonus(isPlayerSide, PpeokMoney()); Toast(isPlayerSide, "연뻑"); }
            else Toast(isPlayerSide, "뻑");

            RebuildUI(); // 필드에 3장 쌓인 모습을 반영 — 이번 턴은 아무도 캡처가 없다.
            yield return new WaitForSeconds(PLAY_STEP_DELAY);

            // 쓰리뻑 — 연속일 필요 없이 이번 판 통산 3번째 뻑이면 즉시 승리
            // (구글링으로 확인한 표준 규칙 — "연속 아니어도 통산 3회면
            // 쓰리뻑"). 예전엔 연속(streak)으로만 판정해서 "3연뻑"이라는
            // 실제로는 없는 용어를 썼었다.
            if (total >= 3)
            {
                // 쓰리뻑 — 지금까지 모은 점수와 무관하게 고정 3점으로 즉시 승리.
                // 여기서 게임이 끝났으니 onDone(AfterPlayerAction/AfterAiAction)은
                // 부르면 안 된다 — 그게 "정상적으로 턴이 끝났다"는 뜻이라
                // EndPlayerTurn 등으로 이어져서 방금 EndGame이 세운 GameOver
                // 상태를 덮어써 버린다(실제로 이 버그로 상태가 되돌아가는 걸
                // 리플렉션 테스트에서 잡았다).
                EndGame(aiWon: !isPlayerSide, fixedBaseScore: 3);
                yield break;
            }

            onDone?.Invoke();
            yield break;
        }

        // 뻑이 아니었으면 이번에 낸 카드로 이 쪽의 연속 뻑 스트릭이 끊긴다.
        if (isPlayerSide) playerPpeokStreak = 0; else aiPpeokStreak = 0;

        // 국열끗(9월 열끗) 선택 팝업 — "모든 패가 Cap에 들어간 뒤"로 미룬다.
        // r1/r2 어느 쪽에서 잡히든 여기 모아뒀다가 턴 맨 끝에 순서대로 묻는다.
        var dualPiPending = new List<HwatuCard>();

        if (r1.captured.Count > 0)
        {
            captured.AddRange(r1.captured);
            GoStopAudio.Instance?.Capture();
            // 손패 캡처(r1)만으로 필드가 비어도 아직 이 턴이 끝난 게 아니다 —
            // 뒤이어 더미패를 한 장 더 뒤집는데, 그 카드는 (필드가 비어 있으니)
            // 무조건 매칭 없이 필드에 그대로 놓인다. 즉 폭탄이 아니고 덱이
            // 남아있는 한, r1이 만든 "빈 필드"는 몇 줄 뒤 항상 다시 채워져서
            // 실제로는 싹쓸이가 아니다. 싹쓸이 인정은 (1) 폭탄 턴(이번 턴에
            // 덱을 안 넘긴다) (2) 덱이 이미 바닥나 더 뒤집을 패가 없는 경우로
            // 한정한다 — 그 외엔 r2(덱 캡처) 쪽에서 최종 상태를 다시 판정한다.
            ApplyMatchBonus(isPlayerSide, r1, bomb, allowSweep: bomb || !willDraw);
            RegisterFlyViaField(r1);
            // isNetworkHost도 같이 확인 — "ai" 쪽 캡처여도 실제로는 접속한
            // 게스트라 팝업으로 직접 물어봐야 한다(AI는 팝업 없이 나중에
            // AfterAiAction에서 자동으로 정한다).
            if (isPlayerSide || isNetworkHost)
            {
                var dual = r1.captured.FirstOrDefault(c => c.dualPi);
                if (dual != null) dualPiPending.Add(dual);
            }
        }

        // 1단계 리빌드 — 낸 카드만 반영한다. 덱은 아직 안 건드렸다.
        RebuildUI(newPlayerCapturedFrom: isPlayerSide ? before1 : (int?)null,
                  newAiCapturedFrom: !isPlayerSide ? before1 : (int?)null);
        yield return new WaitForSeconds(PLAY_STEP_DELAY);

        if (bomb)
        {
            // 폭탄을 낸 턴은 손 2장을 한 번에 썼으니 이 턴의 덱 뒤집기는 생략하고,
            // 그 대신 이후 최대 2번 "손 안 내고 덱만 넘기기"를 선택할 수 있는
            // 권리를 적립한다(강제 소모 아님 — PlayerBombSkip/AiTurnStep에서 사용).
            if (isPlayerSide) playerBombCredits += 2; else aiBombCredits += 2;
            foreach (var dual in dualPiPending)
                yield return StartCoroutine(PromptDualPiChoice(dual, isPlayerSide));
            onDone?.Invoke();
            yield break;
        }

        if (willDraw)
        {
            int before2 = captured.Count;

            if (drawn.isJoker)
            {
                // "이전 손패에서 선택한 패" = 이번에 낸 카드가 매칭 안 돼
                // 그대로 필드에 남은 경우(r1.captured가 비었으면 card가
                // 필드에 있다) 그 카드다. 손패가 뭔가를 잡았으면 겹쳐놓을
                // 대상이 없다 — 즉시 캡처로 단순화한다.
                HwatuCard anchor = r1.captured.Count == 0 ? card : null;
                yield return StartCoroutine(ResolveBonusJoker(isPlayerSide, drawn, anchor, captured));
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
                        yield return StartCoroutine(ContinueChoice(drawn, r2, isPlayerSide, res => chosen2 = res));
                        r2 = chosen2;
                    }
                }
                if (r2.captured.Count > 0)
                {
                    captured.AddRange(r2.captured);
                    GoStopAudio.Instance?.Capture();
                    RegisterFlyViaField(r2);
                    // 쪽: 내 손패가 안 먹고 필드에 놓였다가, 곧바로 내가 뒤집은 카드가
                    // 그 카드와만 매칭됐다 — matchCount==1인 일반 매칭의 특수 사례라
                    // ApplyMatchBonus보다 먼저 확인해야 한다(안 그러면 그냥 "일반 매칭"으로
                    // 지나쳐서 쪽 보너스가 안 붙는다).
                    bool chok = r1.placedOnField && r2.captured.Contains(card) && !isLastDeckCard;
                    // 따닥: 손패로 필드 2장 중 하나를 고른 뒤(ddadakWatch=고르지
                    // 않은 나머지 한 장), 같은 턴의 뒷패가 그 나머지 한 장마저
                    // 잡았다. chok과는 조건이 겹치지 않는다(chok은 r1.placedOnField,
                    // 즉 손패가 아무것도 못 먹은 경우에만 성립하는데, ddadakWatch는
                    // 반대로 손패가 선택 캡처로 뭔가를 먹었을 때만 채워진다).
                    bool ddadak = ddadakWatch != null && r2.captured.Contains(ddadakWatch) && !isLastDeckCard;
                    if (chok)
                    {
                        GoStopRules.StealPi(opponentCaptured, captured, 1);
                        Toast(isPlayerSide, "쪽");
                        // 쪽과 싹쓸이는 중복 인정 — 쪽으로 먹은 게 필드를 마저 비웠으면
                        // 싹쓸이 보너스도 그 위에 그대로 쌓인다.
                        if (r2.sweep)
                        {
                            if (isPlayerSide) playerSweeps++; else aiSweeps++;
                            GoStopRules.StealPi(opponentCaptured, captured, 1);
                            Toast(isPlayerSide, "싹쓸이");
                        }
                    }
                    else if (ddadak)
                    {
                        GoStopRules.StealPi(opponentCaptured, captured, 1);
                        Toast(isPlayerSide, "따닥");
                        if (r2.sweep)
                        {
                            if (isPlayerSide) playerSweeps++; else aiSweeps++;
                            GoStopRules.StealPi(opponentCaptured, captured, 1);
                            Toast(isPlayerSide, "싹쓸이");
                        }
                    }
                    else ApplyMatchBonus(isPlayerSide, r2, false, allowSweep: !isLastDeckCard);

                    if (isPlayerSide || isNetworkHost)
                    {
                        var dual2 = r2.captured.FirstOrDefault(c => c.dualPi);
                        if (dual2 != null) dualPiPending.Add(dual2);
                    }
                }

                // 2단계 리빌드 — 뒤집은 덱 카드가 뒤이어 날아가 자리잡는다.
                // 조커 경로는 ResolveBonusJoker가 자기 리빌드를 이미 다
                // 처리했으므로(다른 페이싱으로 여러 번 그린다) 여기서 또
                // 부르면 방금 자리잡은 카드에 새 카드 펀치 연출이 중복으로
                // 걸린다 — 그 경로는 건너뛴다.
                RebuildUI(newPlayerCapturedFrom: isPlayerSide ? before2 : (int?)null,
                          newAiCapturedFrom: !isPlayerSide ? before2 : (int?)null);
            }
        }

        // onDone(AfterPlayerAction 등)이 여기서 곧바로 고/스톱 팝업을 띄울 수
        // 있는데, RebuildUI는 SlamIn 코루틴을 시작만 하고 기다리지 않으므로
        // 그 즉시 onDone을 부르면 마지막 카드가 아직 날아드는 도중에 팝업이
        // 화면을 덮어버린다 — "필드·상대패 파악이 안 된다"는 신고. 연출이
        // 끝날 시간만큼 여기서 쉬어가서 팝업은 항상 정지된 화면 위에 뜨게 한다.
        //
        // 국열끗 선택은 그 뒤(요청 8번 — 모든 패가 최종적으로 Cap에 들어간
        // 다음)에 순서대로 묻는다.
        yield return new WaitForSeconds(PLAY_STEP_DELAY);

        foreach (var dual in dualPiPending)
            yield return StartCoroutine(PromptDualPiChoice(dual, isPlayerSide));

        onDone?.Invoke();
    }

    /// <summary>
    /// 손패 없이 덱만 한 장 넘긴다 — 폭탄 페널티 턴, 또는 손패가 이미 바닥난 뒤
    /// 남은 덱을 나눠 가지는 턴에 쓴다. 필드와 매칭되면 보너스도 그대로 붙는다
    /// (뒤집은 카드 혼자만의 매칭이라 쪽은 발생하지 않는다 — 손패를 안 냈으니까).
    /// </summary>
    IEnumerator DeckOnlyTurnSeq(bool isPlayerSide, System.Action onDone)
    {
        var captured = isPlayerSide ? playerCaptured : aiCaptured;
        if (drawPile.Count == 0) { onDone?.Invoke(); yield break; }

        var drawn = drawPile[0]; drawPile.RemoveAt(0);
        int before = captured.Count;

        if (drawn.isJoker)
        {
            // 손패를 안 낸 턴이라 "이전 손패에서 선택한 패"가 없다 — 겹쳐놓을
            // 대상이 없으므로 즉시 캡처로 단순화한다.
            yield return StartCoroutine(ResolveBonusJoker(isPlayerSide, drawn, null, captured));
        }
        else
        {
            bool isLastDeckCard = drawPile.Count == 0; // 마지막 더미패는 싹쓸이를 인정하지 않는다
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
                    yield return StartCoroutine(ContinueChoice(drawn, r, isPlayerSide, res => chosen = res));
                    r = chosen;
                }
            }
            HwatuCard dualPending = null;
            if (r.captured.Count > 0)
            {
                captured.AddRange(r.captured);
                GoStopAudio.Instance?.Capture();
                ApplyMatchBonus(isPlayerSide, r, false, allowSweep: !isLastDeckCard);
                RegisterFlyViaField(r);
                if (isPlayerSide || isNetworkHost)
                    dualPending = r.captured.FirstOrDefault(c => c.dualPi);
            }

            RebuildUI(newPlayerCapturedFrom: isPlayerSide ? before : (int?)null,
                      newAiCapturedFrom: !isPlayerSide ? before : (int?)null);

            // PlayFromHandSeq와 같은 이유 — 날아드는 카드 연출이 끝날 시간을 주고
            // 나서 onDone(고/스톱 팝업 등)을 부른다. 국열끗 선택은 모든 패가
            // Cap에 들어간 뒤로 미룬다(요청 8번).
            yield return new WaitForSeconds(PLAY_STEP_DELAY);
            if (dualPending != null)
                yield return StartCoroutine(PromptDualPiChoice(dualPending, isPlayerSide));
            onDone?.Invoke();
            yield break;
        }
        yield return new WaitForSeconds(PLAY_STEP_DELAY);
        onDone?.Invoke();
    }

    /// <summary>보너스피(조커) 처리. 조커는 월이 없어(<see cref="HwatuCard.isJoker"/>)
    /// 실제 매칭에 참여할 수 없으므로 <paramref name="anchor"/>(이번 턴에
    /// 낸 손패가 매칭 안 돼 필드에 남은 카드, 없으면 null) 유무와 무관하게
    /// 항상 그 자리에서 바로 가져간다.
    /// <br/>
    /// 2026-08-20 재작성(사용자 신고 — "필드에 홀수 개의 패가 남는다"의
    /// 원인을 찾음). 예전엔 anchor가 없으면 뒷패를 아예 더 안 깠고, anchor가
    /// 있어도 "다른 달이면" 뒷패(extra)를 <see cref="GoStopRules.Resolve"/>
    /// 없이 그냥 필드에 던져버렸다 — 그래서 extra가 필드에 이미 있는(anchor와
    /// 무관한) 다른 카드와 우연히 짝이 맞아도 절대 안 먹히고 계속 필드에
    /// 쌓이기만 했다. 조커는 "진짜 카드"가 아니라 이번 턴의 덱 소모 몫을
    /// 아직 못 채웠으므로, **anchor 유무와 무관하게 항상** 뒷패를 한 장 더
    /// 까고 일반 덱 캡처와 완전히 같은 경로(Resolve→선택→매칭 판정)를
    /// 거친다 — anchor가 이 카드에 맞춰 잡히면 그게 곧 쪽이다(예전의
    /// "extra.month==anchor.month" 특수 분기를 Resolve()의 결과로 자연스럽게
    /// 흡수했다). 3장이 함께 캡처되던 예전 "쪽" 연출과 최종 결과(anchor·
    /// extra·joker가 전부 같은 사람 것이 됨)는 동일하다 — 조커가 한 박자
    /// 먼저 캡처되고 anchor+extra가 뒤이어 잡히는 것으로 나뉠 뿐이다.</summary>
    IEnumerator ResolveBonusJoker(bool isPlayerSide, HwatuCard joker, HwatuCard anchor, List<HwatuCard> captured)
    {
        field.Add(joker);
        flyFrom[joker] = drawPileArea.position;
        RebuildUI();
        yield return new WaitForSeconds(PLAY_STEP_DELAY * 0.5f);

        field.Remove(joker);
        captured.Add(joker);
        flyFrom[joker] = fieldArea.position;
        Toast(isPlayerSide, "보너스");
        RebuildUI();
        yield return new WaitForSeconds(PLAY_STEP_DELAY * 0.5f);

        if (drawPile.Count == 0) yield break;

        var extra = drawPile[0]; drawPile.RemoveAt(0);

        if (extra.isJoker)
        {
            // 두 조커가 연달아 나오는 극히 드문 경우 — 같은 함수를 재귀
            // 호출해서 이번에도 같은 anchor 기준으로 처리한다.
            yield return StartCoroutine(ResolveBonusJoker(isPlayerSide, extra, anchor, captured));
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
                ppeokBonusPi.Remove(extra.month); // 이중 지급 방지 (PlayFromHandSeq의 r1/r2 분기와 같은 이유)
            }
            else
            {
                GoStopRules.CaptureResult chosen = null;
                yield return StartCoroutine(ContinueChoice(extra, r, isPlayerSide, res => chosen = res));
                r = chosen;
            }
        }

        if (r.captured.Count > 0)
        {
            captured.AddRange(r.captured);
            GoStopAudio.Instance?.Capture();
            RegisterFlyViaField(r);

            var opponentCaptured = isPlayerSide ? aiCaptured : playerCaptured;
            // 쪽 — anchor가 이 뒷패에 맞춰 잡혔다. PlayFromHandSeq의 일반
            // 쪽 판정(r1.placedOnField && r2.captured.Contains(card))과
            // 완전히 같은 형태다.
            bool chok = anchor != null && r.captured.Contains(anchor) && !isLastDeckCard;
            if (chok)
            {
                GoStopRules.StealPi(opponentCaptured, captured, 1);
                Toast(isPlayerSide, "보너스+쪽");
                if (r.sweep)
                {
                    if (isPlayerSide) playerSweeps++; else aiSweeps++;
                    GoStopRules.StealPi(opponentCaptured, captured, 1);
                    Toast(isPlayerSide, "싹쓸이");
                }
            }
            else ApplyMatchBonus(isPlayerSide, r, false, allowSweep: !isLastDeckCard);

            if (isPlayerSide || isNetworkHost)
            {
                var dual = r.captured.FirstOrDefault(c => c.dualPi);
                if (dual != null) yield return StartCoroutine(PromptDualPiChoice(dual, isPlayerSide));
            }
        }

        RebuildUI();
        yield return new WaitForSeconds(PLAY_STEP_DELAY);
    }

    /// <summary>
    /// <paramref name="allowSweep"/>=false면 싹쓸이 보너스(점수 카운트+피 뺏기+토스트)를
    /// 스킵한다 — 더미의 마지막 한 장은 "남은 패와 반드시 맞게 돼 있다"는
    /// 이유로 싹쓸이(와 쪽)를 인정하지 않는다는 규칙 때문이다. 캡처 자체는
    /// 정상 진행된다.
    /// </summary>
    void ApplyMatchBonus(bool isPlayerSide, GoStopRules.CaptureResult r, bool bomb, bool allowSweep = true)
    {
        var captured = isPlayerSide ? playerCaptured : aiCaptured;
        var opponentCaptured = isPlayerSide ? aiCaptured : playerCaptured;

        if (bomb) { GoStopRules.StealPi(opponentCaptured, captured, 2); Toast(isPlayerSide, "폭탄"); }
        else if (r.matchCount == 3)
        {
            // 뻑 해소 — 이 달을 누가 뻑으로 쌓았는지 기억해 뒀다가, 해소하는
            // 쪽이 causer와 같으면(자뻑) 2장, 다르면 1장을 가져간다.
            int month = r.captured[0].month;
            bool selfPpeok = ppeokCauser.TryGetValue(month, out bool causer) && causer == isPlayerSide;
            GoStopRules.StealPi(opponentCaptured, captured, selfPpeok ? 2 : 1);
            ppeokCauser.Remove(month);
            Toast(isPlayerSide, selfPpeok ? "자뻑" : "뻑 먹기");

            // 그 뻑에 보너스피가 같이 묻혀 있었으면(ResolveBonusJoker 참고)
            // 지금 이걸 해소하는 쪽이 그 보너스피도 같이 가져간다.
            if (ppeokBonusPi.TryGetValue(month, out var bonus))
            {
                field.Remove(bonus);
                captured.Add(bonus);
                flyFrom[bonus] = fieldArea.position;
                ppeokBonusPi.Remove(month);
                Toast(isPlayerSide, "보너스");
            }
        }
        // matchCount==2(옛 "따닥")는 더 이상 여기 안 들어온다 — 필드에 같은 달이
        // 2장이면 GoStopRules.Resolve가 자동으로 안 가져가고 choiceCandidates로
        // 미루고, GoStopGame.ContinueChoice가 고른 뒤 matchCount=1로 확정해서
        // 넘긴다(사용자가 확정한 규칙 — "다 가져가는 게 아니라 골라서 하나만").
        if (r.sweep && allowSweep)
        {
            if (isPlayerSide) playerSweeps++; else aiSweeps++;
            GoStopRules.StealPi(opponentCaptured, captured, 1);
            Toast(isPlayerSide, "싹쓸이");
        }
    }

    /// <summary>
    /// 첫뻑/첫따닥/연뻑/삼연뻑처럼 캡처와 무관하게 바로 오가는 판돈 보너스.
    /// 상대가 가진 것보다 더는 못 뺏으므로 clamp한다 — 이 보너스들은 대개
    /// 작아서(500~2000원) 실제로 걸릴 일은 거의 없지만, 파산 직전 판에서는
    /// 방어적으로 필요하다.
    /// </summary>
    void ApplyMoneyBonus(bool isPlayerSide, int amount)
    {
        int pay = isPlayerSide ? Mathf.Min(amount, aiMoney) : Mathf.Min(amount, playerMoney);
        if (isPlayerSide) { playerMoney += pay; aiMoney -= pay; }
        else { aiMoney += pay; playerMoney -= pay; }
        FlyMoneyFX(toPlayer: isPlayerSide, amount: pay);
    }

    /// <summary>첫뻑/연뻑/첫따닥이 즉시 오가는 금액 = 3점 상당. 나가리로 판돈이
    /// 불어나 있으면(stakeMultiplier) 즉시 보너스도 같이 불어난다 — 최종
    /// 정산과 같은 기준을 쓰는 게 일관적이다.</summary>
    int PpeokMoney() => PPEOK_MONEY_POINTS * WON_PER_POINT * stakeMultiplier;

    Coroutine toastHideCo;

    /// <summary>
    /// <see cref="GameUIManager.ShowToast"/>는 패널을 켜기만 하고 스스로 끄지
    /// 않는다(공용 프리팹이라 여기서 못 고친다) — 그래서 여기서 직접 타이머를
    /// 걸어 꺼준다. 안 그러면 첫 보너스 토스트가 뜬 뒤로 게임이 끝날 때까지
    /// 화면 하단(손패 바로 위)을 계속 가린 채로 남는다 — 실제로 신고된 버그.
    /// 연달아 여러 토스트가 뜨면(흔들기→따닥→쪽 등) 타이머를 매번 새로 걸어
    /// 마지막 토스트 기준으로 꺼지게 한다.
    /// </summary>
    void Toast(bool isPlayerSide, string label)
    {
        ShowTimedToast((isPlayerSide ? "" : "상대 ") + label + "!");
        GoStopAudio.Instance?.PlayForLabel(label);
        ShowActionPopup(label);

        // 호스트 전용 — 뻑/쪽/싹쓸이 등은 호스트에서만 발생한다
        // (PlayFromHandSeq/DeckOnlyTurnSeq는 호스트만 돈다). 게스트도 같은
        // 토스트/사운드/이펙트를 보게 하려면 여기서 직접 실어 보낸다.
        // seat: 0=player(호스트) 쪽 행동, 1=ai(게스트) 쪽 행동.
        if (isNetworkHost)
            GoStopNetLobby.Instance?.BroadcastToGuests(GoStopNetMessage.EventMsg(label, isPlayerSide ? 0 : 1));
    }

    // ── 비상 시스템 ──────────────────────────────────────
    // 4인판(GoStop3PGame.cs)과 동일한 규칙 — 고도리/홍단/초단/청단이
    // 완성 직전(2/3, 안 막힘)이면 알린다. RebuildUI 맨 끝에서 매번
    // 호출되므로 캡처가 일어나는 모든 경로 뒤에 항상 걸린다.
    static readonly (string name, System.Func<HwatuCard, bool> pred)[] EmergencySets =
    {
        ("고도리", GoStopRules.IsGodori),
        ("홍단",   GoStopRules.IsHongdan),
        ("초단",   GoStopRules.IsChodan),
        ("청단",   GoStopRules.IsCheongdan),
    };

    void CheckEmergencies()
    {
        CheckEmergencySide(true, playerCaptured, aiCaptured);
        CheckEmergencySide(false, aiCaptured, playerCaptured);
    }

    void CheckEmergencySide(bool isPlayerSide, List<HwatuCard> mine, List<HwatuCard> theirs)
    {
        if (mine.Count == 0) return;
        for (int i = 0; i < EmergencySets.Length; i++)
        {
            if (emergencyFired.Contains((isPlayerSide, i))) continue;
            var (state, have) = GoStopRules.CheckSet(mine, theirs, EmergencySets[i].pred);
            if (state == GoStopRules.SetState.Alive && have == 2)
            {
                emergencyFired.Add((isPlayerSide, i));
                FireEmergency(isPlayerSide, EmergencySets[i].name);
            }
        }
    }

    /// <summary>비상 이펙트 발동 — 4인판과 같은 프리팹(EffectGodori/
    /// EffectHongdan/EffectChodan/EffectCheongdan, GoStopEffectPopup 공유)을
    /// 그대로 재사용한다 — 2인판 자체 ShowActionPopup(코드 생성 텍스트)
    /// 대신 프리팹 쪽을 택했다(디자인 리소스 교체가 쉽도록).</summary>
    void FireEmergency(bool isPlayerSide, string setName)
    {
        string prefabName = setName switch
        {
            "고도리" => "EffectGodori",
            "홍단" => "EffectHongdan",
            "초단" => "EffectChodan",
            "청단" => "EffectCheongdan",
            _ => null,
        };
        if (prefabName == null || fieldArea == null) return;

        var canvasRoot = fieldArea.parent as RectTransform;
        Vector2 local = fieldArea.anchoredPosition + new Vector2(0f, -60f);

        GoStopIcons.SpawnBurst(canvasRoot, local, EmergencyColor(setName), 20);

        var fx = HwatuUI.InstantiateEffect<GoStopEffectPopup>(prefabName, canvasRoot);
        if (fx != null)
        {
            fx.root.anchoredPosition = local;
            string who = isPlayerSide ? "" : "상대 ";
            fx.Play($"{who}{setName} 비상!", EmergencyColor(setName));
        }

        ShowTimedToast($"{(isPlayerSide ? "" : "상대가 ")}{setName} 완성 직전!");
        GoStopAudio.Instance?.Bonus();
    }

    static Color EmergencyColor(string setName) => setName switch
    {
        "고도리" => new Color(0.949f, 0.718f, 0.020f),
        "홍단"   => new Color(0.906f, 0.298f, 0.235f),
        "초단"   => new Color(0.180f, 0.800f, 0.443f),
        "청단"   => new Color(0.231f, 0.616f, 0.910f),
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

    // ── 플레이어 턴 ──────────────────────────────────────
    /// <summary>
    /// 손에 같은 달 3장이 갓 모인 시점(이 카드를 내려는 게 그 세 장 중 첫 번째)에만
    /// 흔들기 여부를 물어본다 — 카드 수 조건 자체가 "이 달 첫 번째 플레이"로
    /// 자연스럽게 한정되므로, 남은 두 장을 나중에 낼 때는 다시 묻지 않는다.
    /// </summary>
    void OnPlayerPlay(HwatuCard card)
    {
        if (state != State.PlayerTurn) return;

        bool tripleInHand = playerHand.Count(c => c.month == card.month) == 3;
        if (tripleInHand && !playerShook.Contains(card.month))
        {
            pendingShakeCard = card;
            ShowShakeConfirm(card.month);
            return;
        }

        ContinuePlayerPlay(card, declareShake: false);
    }

    void ShowShakeConfirm(int month)
    {
        shakePopup.messageText.text = $"{month}월 흔들기 선언하시겠습니까?";
        shakePopup.Show();
    }

    void OnShakeChoice(bool shake)
    {
        shakePopup.Hide();
        var card = pendingShakeCard;
        pendingShakeCard = null;
        if (card == null || state != State.PlayerTurn) return;
        ContinuePlayerPlay(card, declareShake: shake);
    }

    void ContinuePlayerPlay(HwatuCard card, bool declareShake)
    {
        if (isNetworkGuest)
        {
            // 로컬 상태를 안 건드린다 — 판정은 호스트만 하고, 결과가
            // StateSync로 돌아오면 ApplyNetworkSnapshot이 화면을 맞춘다.
            GoStopNetLobby.Instance.SendToHost(GoStopNetMessage.PlayWithShake(card.spriteName, declareShake));
            return;
        }
        StartCoroutine(PlayFromHandSeq(card, true, declareShake, AfterPlayerAction));
    }

    /// <summary>
    /// 적립해 둔 폭탄 크레딧을 써서 손을 안 내고 덱만 넘긴다 — 본인 선택.
    /// "덱만 넘기기" 버튼에 연결된다.
    /// </summary>
    void OnPlayerBombSkip()
    {
        if (state != State.PlayerTurn || playerBombCredits == 0) return;
        if (isNetworkGuest)
        {
            GoStopNetLobby.Instance.SendToHost(GoStopNetMessage.BombSkipMsg());
            return;
        }
        playerBombCredits--;
        StartCoroutine(DeckOnlyTurnSeq(true, AfterPlayerAction));
    }

    void PlayerHandEmptyStep()
    {
        if (state != State.PlayerTurn) return;
        StartCoroutine(DeckOnlyTurnSeq(true, AfterPlayerAction));
    }

    /// <summary>
    /// 손패 액션(낸 카드 + 덱 뒤집기 애니메이션까지 전부) 이후 공통 뒷정리 —
    /// 세 진입점(정상 플레이/폭탄 크레딧 소모/손패 소진)이 전부 같은 순서를
    /// 반복하고 있어서 하나로 묶었다.
    /// </summary>
    void AfterPlayerAction()
    {
        if (CheckHandsEmpty()) return;
        int score = GoStopRules.CalcScore(playerCaptured, playerSweeps).Total;
        if (score >= GoStopRules.CAPTURE_LINE)
        {
            // 손패가 정말 바닥났고(0장) 더 쓸 수 있는 폭탄 크레딧도 없어야만
            // "더 낼 패가 없다"고 본다 — 폭탄 직후엔 손이 0이어도 "덱만
            // 넘기기" 크레딧이 남아 있어서 아직 선택지가 있다. 이 구분 없이
            // 손 0장만 보고 즉시 끝내면, 폭탄으로 손을 다 쓰면서 7점을 넘긴
            // 경우 더미에 카드가 남아 있어도 고/스톱을 못 물어보고 그냥
            // 끝나버리는 버그가 된다(실제로 신고받음).
            if (playerHand.Count == 0 && playerBombCredits == 0) { EndGame(aiWon: false); return; }
            ShowGoStopPrompt(score);
            return;
        }
        EndPlayerTurn();
    }

    void EndPlayerTurn() => AdvanceTurn(nextIsPlayer: false);

    /// <summary>
    /// 다음 턴 주체를 정한다. <b>손패가 진짜로 비어 있을 때만</b> 자동으로 덱을
    /// 넘긴다 — 폭탄 크레딧은 강제가 아니라 손이 있어도 본인이 골라 쓰는
    /// 선택지라(버튼으로 제공) 여기서 자동 소모하지 않는다. 예전엔 손패 빈
    /// 쪽을 통째로 건너뛰기만 해서 게임이 일찍 끝나버렸다 — 손이 비어도 덱은
    /// 계속 나눠 받아야 정상이다. 둘 다 손패가 비어야 CheckHandsEmpty가 끝낸다.
    /// </summary>
    void AdvanceTurn(bool nextIsPlayer)
    {
        if (CheckHandsEmpty()) return;
        GoStopAudio.Instance?.TurnChange();

        if (nextIsPlayer)
        {
            state = State.PlayerTurn;
            // 손패가 0장이어도 폭탄 크레딧이 남아있으면 아직 선택지가 있다 —
            // 자동으로 덱을 넘겨버리면(PlayerHandEmptyStep) 크레딧을 쓸 기회를
            // 아예 건너뛰게 된다. 이때는 일반 손패 턴처럼 RebuildUI로 크레딧
            // 슬롯만 보여주고 플레이어의 클릭(OnPlayerBombSkip)을 기다린다.
            if (playerHand.Count == 0 && playerBombCredits == 0) Invoke(nameof(PlayerHandEmptyStep), 0.6f);
            else
            {
                // 폭탄 크레딧 슬롯(BombSkip)은 state==PlayerTurn일 때만 보인다.
                // 그런데 폭탄을 낸 직후엔 RebuildUI가 상대 턴 도중(state가
                // 아직 AiTurn일 때) 마지막으로 불려서 슬롯이 조건에 안 걸려
                // 안 그려졌고, 그 뒤 상태가 PlayerTurn으로 바뀔 때는 아무도
                // 다시 그려주지 않아 "폭탄 쓰고 나서 손패 끝에 아무것도 안
                // 붙는다"는 신고로 이어졌다. 내 턴이 실제로 시작되는 지금
                // 한 번 더 그려서 슬롯이 최신 크레딧/위치로 나타나게 한다.
                RebuildUI();
            }
            // 그 외엔 UI가 손패 클릭(또는 폭탄 크레딧 슬롯)을 기다린다.
        }
        else
        {
            state = State.AiTurn;
            // 2026-08-20: 예전엔 "내 차례가 됐을 때만"(위 if 분기) 다시
            // 그렸다 — AI 턴은 어차피 AI가 알아서 움직이니 그릴 필요가
            // 없다고 가정했는데, 네트워크 대전에서 이 가정이 깨진다.
            // 여기서 다시 그리지 않으면(=isNetworkHost일 때 브로드캐스트가
            // 안 나가면) 게스트 화면은 지난 state를 그대로 들고 있어 "내
            // 차례"를 전혀 못 알아채고, 호스트는 원격 응답을 영원히
            // 기다리는 교착이 생긴다(GoStop3PGame의 AdvanceTurn에서 실제로
            // 겪은 버그와 같은 종류). 싱글플레이에도 적용해도 무해하다.
            RebuildUI();
            if (isNetworkHost) StartCoroutine(RemoteAiTurn());
            else Invoke(nameof(AiTurnStep), 0.7f);   // 상대가 생각하는 척하는 최소한의 텀
        }
    }

    void ShowGoStopPrompt(int score)
    {
        state = State.GoStopChoice;
        ui?.ShowOverlay(new Color(.93f, .73f, .18f), $"{score}점 달성!", score.ToString(),
            "고 하시겠습니까, 스톱 하시겠습니까?",
            "고", OnPlayerGo, "스톱", OnPlayerStop);

        // 2026-08-20: 예전엔 여기서 state만 바뀌고 아무도 다시 그리지
        // 않았다 — 호스트 자신의 고/스톱 결정은 이 오버레이가 로컬에서만
        // 뜨고 네트워크로 알린 적이 없어서, 상대(게스트)는 그동안 화면이
        // 왜 멈췄는지 전혀 알 방법이 없었다("상대방이 선택 중이라고
        // 표시 필요" 신고). RebuildUI()가 타이틀 텍스트(BuildTurnIndicator)를
        // 갱신하고, 호스트라면 그 김에 게스트에게도 브로드캐스트한다
        // (RebuildUI 꼬리의 기존 훅). 싱글플레이·게스트 자신의 결정
        // 팝업(ApplyNetworkSnapshot 안에서도 이 함수가 재사용된다)에도
        // 안전하게 적용된다 — 그냥 다시 그릴 뿐이라 무해하다.
        RebuildUI();
    }

    void OnPlayerGo()
    {
        ui?.HideOverlay();
        if (isNetworkGuest)
        {
            // 호스트 쪽 RemoteAiGoStopSeq가 이미 응답을 기다리고 있다 —
            // 로컬 상태는 안 건드리고 보내기만 한다. 결과는 다음 StateSync로 온다.
            GoStopNetLobby.Instance.SendToHost(GoStopNetMessage.GoStop(true));
            return;
        }
        playerGoCount++;
        RecordGoCall(true);
        GoStopAudio.Instance?.Go();
        EndPlayerTurn();
    }

    /// <summary>역고 판정용 — "고를 부르는 쪽"이 바뀔 때마다(=상대가 먼저
    /// 불렀는데 내가 앞질러 부르거나, 그 반대) 역전 횟수를 센다.</summary>
    void RecordGoCall(bool isPlayerSide)
    {
        if (goLeader.HasValue && goLeader.Value != isPlayerSide) goReversalCount++;
        goLeader = isPlayerSide;
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
        EndGame(aiWon: false);
    }

    // ── 상대(AI) 턴 ──────────────────────────────────────
    void AiTurnStep()
    {
        if (state != State.AiTurn) return;

        // 손패가 진짜로 비었을 때만 덱만 넘긴다. 폭탄 크레딧은 플레이어처럼
        // 선택 사항인데, AI는 굳이 아껴 쓸 필요가 없으니(어차피 자기가 손해볼
        // 것도 없다) 그냥 안 쓰고 항상 손패를 낸다 — 구현을 단순하게 유지한다.
        if (aiHand.Count == 0)
            StartCoroutine(DeckOnlyTurnSeq(false, AfterAiAction));
        else
        {
            var card = GoStopAI.ChooseCard(aiHand, field);
            StartCoroutine(PlayFromHandSeq(card, false, GoStopAI.ShouldShake(), AfterAiAction));
        }
    }

    /// <summary>네트워크 호스트 전용 — "ai" 역할의 한 턴을 실제 사람(접속한
    /// 게스트)의 응답으로 진행한다. AiTurnStep과 갈라진 이유는 원격 좌석엔
    /// "생각하는 척" 지연이 필요 없어서다(늦게 듣기 시작하면 그새 도착한
    /// 빠른 응답을 놓친다 — GoStop3PGame의 RemoteTurn과 같은 이유로 여기서도
    /// AdvanceTurn이 이 코루틴을 지연 없이 바로 부른다).</summary>
    IEnumerator RemoteAiTurn()
    {
        if (state != State.AiTurn) yield break;

        if (aiHand.Count == 0)
        {
            StartCoroutine(DeckOnlyTurnSeq(false, AfterAiAction));
            yield break;
        }

        GoStopNetMessage msg = null;
        yield return StartCoroutine(WaitForRemoteMessage(
            m => m.type == GoStopNetMessage.Type.PlayCard || (m.type == GoStopNetMessage.Type.BombSkip && aiBombCredits > 0),
            m => msg = m));

        if (msg.type == GoStopNetMessage.Type.BombSkip)
        {
            aiBombCredits--;
            StartCoroutine(DeckOnlyTurnSeq(false, AfterAiAction));
            yield break;
        }

        // 게스트가 보낸 건 스냅샷에서 새로 디코딩한 별개의 HwatuCard
        // 객체다 — 참조 동일성으로 카드를 다루는 곳이 있어(hand.Remove 등)
        // 반드시 aiHand 안의 진짜 인스턴스를 찾아 써야 한다. 못 찾으면
        // (오염된/오래된 메시지) 판이 안 멈추도록 AI 선택으로 방어한다.
        var decoded = GoStopDeck.Decode(msg.cardId);
        var card = decoded != null ? aiHand.FirstOrDefault(c => c.spriteName == decoded.spriteName) : null;
        if (card == null) card = GoStopAI.ChooseCard(aiHand, field);
        StartCoroutine(PlayFromHandSeq(card, false, msg.boolValue, AfterAiAction));
    }

    /// <summary>네트워크 호스트 전용 — "ai"(게스트) 쪽의 고/스톱 결정을
    /// 실제 사람의 응답으로 처리한다. state는 그대로 두고(AiTurn) 대신
    /// aiGoStopPendingFlag로 게스트에게 "네 결정이 필요하다"를 알린다 —
    /// State.GoStopChoice는 이 파일에서 "호스트 자신의 결정"만을 뜻하므로
    /// 여기 쓰면 안 된다(GoStopStateSnapshot2P의 SwapStateForGuest 참고).</summary>
    IEnumerator RemoteAiGoStopSeq(int score)
    {
        aiGoStopPendingFlag = true;
        // 2026-08-20: 예전엔 BroadcastNetworkState()만 불러서 게스트에게는
        // 알렸지만, 정작 호스트 자신의 화면(타이틀의 턴 표시)은 안
        // 바뀌었다 — "상대가 선택 중"이라는 걸 정작 기다리는 호스트
        // 본인도 몰랐다는 뜻. RebuildUI()로 바꾸면 호스트 자신도 다시
        // 그려지면서(꼬리에서 브로드캐스트도 그대로 나간다) 양쪽 다
        // 갱신된다.
        RebuildUI();
        GoStopNetMessage msg = null;
        yield return StartCoroutine(WaitForRemoteMessage(
            m => m.type == GoStopNetMessage.Type.GoStopDecision, m => msg = m));
        aiGoStopPendingFlag = false;

        if (msg.boolValue)
        {
            aiGoCount++;
            RecordGoCall(false);
            ShowTimedToast($"상대가 고를 외쳤습니다! ({score}점)");
            GoStopAudio.Instance?.Go();
            AdvanceTurn(nextIsPlayer: true);
        }
        else
        {
            GoStopAudio.Instance?.Stop();
            EndGame(aiWon: true);
        }
    }

    void AfterAiAction()
    {
        // 9월 열끗(국화 술잔)을 방금 먹었을 수 있다 — AI는 팝업 없이 그때그때
        // 점수가 더 높아지는 쪽(열끗/쌍피)으로 즉시 정한다. 역할이 바뀌었으면
        // 태그 표시가 최신 상태를 보여주도록 한 번 더 리빌드한다. 원격
        // 좌석(isNetworkHost)은 제외한다 — 이미 캡처 시점에 PromptDualPiChoice가
        // 실제 사람에게 물어봤다(위 PlayFromHandSeq/DeckOnlyTurnSeq 참고).
        bool anyDual = aiCaptured.Any(c => c.dualPi);
        if (anyDual && !isNetworkHost)
        {
            GoStopAI.OptimizeDualPi(aiCaptured);
            RebuildUI();
        }

        if (CheckHandsEmpty()) return;

        int score = GoStopRules.CalcScore(aiCaptured, aiSweeps).Total;
        if (score >= GoStopRules.CAPTURE_LINE)
        {
            if (isNetworkHost)
            {
                StartCoroutine(RemoteAiGoStopSeq(score));
                return;
            }
            if (GoStopAI.ShouldGo(score, aiGoCount, aiHand.Count))
            {
                aiGoCount++;
                RecordGoCall(false);
                ShowTimedToast($"상대가 고를 외쳤습니다! ({score}점)");
                GoStopAudio.Instance?.Go();
                AdvanceTurn(nextIsPlayer: true);
                return;
            }
            GoStopAudio.Instance?.Stop();
            EndGame(aiWon: true);
            return;
        }
        AdvanceTurn(nextIsPlayer: true);
    }

    // ── 종료 ─────────────────────────────────────────────
    bool CheckHandsEmpty()
    {
        if (playerHand.Count > 0 || aiHand.Count > 0) return false;

        int pScore = GoStopRules.CalcScore(playerCaptured, playerSweeps).Total;
        int aScore = GoStopRules.CalcScore(aiCaptured, aiSweeps).Total;

        if (pScore < GoStopRules.CAPTURE_LINE && aScore < GoStopRules.CAPTURE_LINE)
        {
            EndGame(null);
            return true;
        }
        EndGame(aiWon: aScore > pScore);
        return true;
    }

    /// <summary>
    /// <paramref name="fixedBaseScore"/> — 쓰리뻑/총통처럼 실제 캡처 점수 대신
    /// 고정 점수로 정산해야 할 때. <paramref name="extraMultiplier"/> — 총통의
    /// x4처럼 다른 배수들과 무관하게 통째로 곱할 배수.
    /// </summary>
    void EndGame(bool? aiWon, int? fixedBaseScore = null, int extraMultiplier = 1)
    {
        state = State.GameOver;

        if (aiWon == null) GoStopAudio.Instance?.Nagari();
        else
        {
            GoStopAudio.Instance?.Money();
            if (aiWon == false) { GoStopAudio.Instance?.Win(); PlayWinConfettiFX(); }
            else GoStopAudio.Instance?.Lose();
        }

        string title; string sub = null; int finalScore = 0; Color col;
        bool loserCapturedNothing = false;

        if (aiWon == null)
        {
            // 나가리는 판돈이 안 오가는 대신, 다음 판 판돈이 배로 뛴다 —
            // 연속 나가리면 계속 곱해진다(2→4→8…). 결판이 나면 리셋한다.
            stakeMultiplier *= 2;
            pendingBreakdown = null; // 나가리는 승자가 없어 분석할 점수 자체가 없다
            title = "나가리";
            sub   = $"무승부 — 아무도 7점을 넘지 못했습니다 · 다음 판 판돈 {stakeMultiplier}배";
            col   = new Color(.6f, .6f, .68f);
        }
        else if (aiWon == false)
        {
            // 역고 — 이번 판 마지막으로 고를 부른 쪽이 나(플레이어)일 때만
            // 역전 배수를 적용한다. 내가 아예 고를 안 불렀거나 상대가
            // 마지막으로 불렀으면(=내가 역전당한 채 이겼을 뿐이면) 0.
            int reversal = (goLeader == true) ? goReversalCount : 0;
            var breakdown = GoStopRules.FinalScoreBreakdown(playerCaptured, playerSweeps, playerGoCount,
                playerHeundeul, playerBombCount, aiCaptured, reversal, fixedBaseScore, extraMultiplier);
            finalScore = breakdown.finalScore;
            int oppBase = GoStopRules.CalcScore(aiCaptured, aiSweeps).Total;
            pendingBreakdown = breakdown;
            pendingBreakdownIsPlayer = true;
            pendingOtherBaseScore = oppBase;
            loserCapturedNothing = aiCaptured.Count == 0;
            title = "승리!";
            col   = new Color(.30f, .78f, .42f);
            sub   = "상대 " + oppBase + "점";
            int best = PlayerPrefs.GetInt(BestKey, 0);
            if (finalScore > best)
            {
                PlayerPrefs.SetInt(BestKey, finalScore);
                PlayerPrefs.Save();
                ui?.SetBest(finalScore);
                // 오버레이 서브텍스트가 한 줄 높이로 고정돼 있어(GameUIManager 프리팹은
                // 여러 게임이 공유하므로 여기서 손대지 않는다) 줄바꿈 대신 한 줄로 합친다.
                sub = "신기록! · " + sub;
            }
        }
        else
        {
            int reversal = (goLeader == false) ? goReversalCount : 0;
            var breakdown = GoStopRules.FinalScoreBreakdown(aiCaptured, aiSweeps, aiGoCount, aiHeundeul,
                aiBombCount, playerCaptured, reversal, fixedBaseScore, extraMultiplier);
            finalScore = breakdown.finalScore;
            // 화면 중앙 큰 숫자는 이번 판을 가른 상대 최종점이라 여기서도 그걸 쓰지만,
            // 내 기본점수를 같이 보여줘야 "36 대 36"처럼 같은 수가 중복 표시되는 걸 피한다.
            int myBase = GoStopRules.CalcScore(playerCaptured, playerSweeps).Total;
            pendingBreakdown = breakdown;
            pendingBreakdownIsPlayer = false;
            pendingOtherBaseScore = myBase;
            loserCapturedNothing = playerCaptured.Count == 0;
            title = "패배...";
            sub   = "내 " + myBase + "점 · 상대 " + finalScore + "점";
            col   = new Color(.86f, .32f, .30f);
        }

        // 판돈 정산 — 점당 100원 × 판돈 배수(연속 나가리로 불어난 만큼).
        // 가진 돈보다 더는 못 잃으므로 Min으로 clamp한다(진 쪽이 0원 밑으로
        // 내려가지 않는다). 진 쪽이 이번 판 카드를 한 장도 못 먹었으면 그
        // 판은 "화투를 안 친 것"으로 쳐서 돈이 아예 안 오간다(실제 규칙) —
        // 승패·최고기록 표시는 그대로 하되 판돈만 스킵한다. 결판이 났으니
        // (돈이 오갔든 안 오갔든) 다음 판을 위해 배수는 리셋한다.
        if (aiWon.HasValue && !loserCapturedNothing)
        {
            if (aiWon == false)
            {
                int payout = Mathf.Min(finalScore * WON_PER_POINT * stakeMultiplier, aiMoney);
                playerMoney += payout; aiMoney -= payout;
                FlyMoneyFX(toPlayer: true, amount: payout);
            }
            else
            {
                int payout = Mathf.Min(finalScore * WON_PER_POINT * stakeMultiplier, playerMoney);
                playerMoney -= payout; aiMoney += payout;
                FlyMoneyFX(toPlayer: false, amount: payout);
            }
        }
        if (aiWon.HasValue) stakeMultiplier = 1;

        // 2026-08-18: 예전엔 어느 한쪽이 0원이 되면 세션이 그대로 끝났는데
        // ("다시 시작"이 의미 없다고 봤었다), 사용자 요청으로 대신 5만원을
        // 리필해서 계속 이어가는 쪽으로 바꿨다 — 그래서 "다시 시작"이
        // 항상 유효하다. 몇 번 파산했는지는 올인 횟수로 기록만 하고 정산에는
        // 영향 없다.
        if (RefillIfBankrupt())
            sub += $" · 잔액 소진 → 5만원 재충전(올인 나 {playerAllInCount}회 · 상대 {aiAllInCount}회)";
        // 네트워크 판은 로컬 저장을 안 한다 — 매판 접속하는 사람이 달라질
        // 수 있어 "이 기기의 잔액"이라는 개념이 안 맞는다.
        if (!isNetworkHost && !isNetworkGuest) SaveMoney();

        if (loserCapturedNothing && aiWon.HasValue) sub += " · 상대가 한 장도 못 먹어 판돈 없음";
        // 오버레이 서브텍스트는 한 줄 고정이라(위 신기록 처리와 같은 이유) 이어 붙인다.
        sub += $" · 내 머니 {playerMoney:N0}원";

        ui?.SetScore(playerMoney); // 상단 HUD의 SCORE는 판점이 아니라 내 보유 머니를 보여준다(사용자 요청)
        if (aiWon.HasValue)
            ui?.ShowOverlay(col, title, finalScore.ToString(), sub,
                "다시 시작", NewGame, "타이틀", GoToTitle, "점수 상세", ShowScoreDetail);
        else
            ui?.ShowOverlay(col, title, finalScore.ToString(), sub, "다시 시작", NewGame, "타이틀", GoToTitle);

        // EndGame은 RebuildUI를 거치지 않으므로(정규 브로드캐스트 경로
        // 밖) 게스트에게 판이 끝난 걸 따로 알려야 한다.
        if (isNetworkHost)
        {
            if (aiWon == null) BroadcastGameOverState(isNagari: true, aiWonForGuest: false, finalScoreValue: 0, stakeMultiplierValue: stakeMultiplier);
            else BroadcastGameOverState(isNagari: false, aiWonForGuest: aiWon.Value, finalScoreValue: finalScore, stakeMultiplierValue: stakeMultiplier);
        }
    }

    /// <summary>"왜 이 점수가 나왔는지" 항목별로 보여준다 — 게임오버 오버레이의
    /// "점수 상세" 버튼에서 연다. 각 항목 옆에 실제로 관여한 카드 실물을 같이
    /// 보여준다("광 3점이면 광 3장이 같이 보였으면" 요청) — 텍스트 목록만 있던
    /// 예전 버전보다 한눈에 근거가 들어온다.</summary>
    void ShowScoreDetail()
    {
        if (pendingBreakdown == null || scoreDetailPopup == null) return;
        var b = pendingBreakdown;
        string owner = pendingBreakdownIsPlayer ? "내" : "상대";
        string other = pendingBreakdownIsPlayer ? "상대" : "내";
        var captured = pendingBreakdownIsPlayer ? playerCaptured : aiCaptured;

        var sum = new System.Text.StringBuilder();
        sum.Append($"[{owner} 획득패 기준]  기본 소계 {b.baseScore.Total}점");
        if (b.goCount > 0) sum.Append($"  ·  고 {b.goCount}회(+{b.goBonus}) → {b.subtotal}점");
        scoreDetailPopup.summaryText.text = sum.ToString();

        float rowsY = BuildScoreDetailRows(scoreDetailPopup.rowsContent, captured, b.baseScore);
        rowsY = AppendAllCapsSection(scoreDetailPopup.rowsContent, rowsY,
            new (string name, List<HwatuCard> cards)[] { ("나", playerCaptured), ("상대", aiCaptured) });
        scoreDetailPopup.rowsContent.sizeDelta = new Vector2(scoreDetailPopup.rowsContent.sizeDelta.x, Mathf.Max(rowsY, 520f));

        var mult = new List<string>();
        if (b.isReversalGo) mult.Add($"역고 ×{b.goMultiplier}");
        else if (b.goMultiplier > 1) mult.Add($"고배수 ×{b.goMultiplier}");
        if (b.heundeulCount > 0) mult.Add($"흔들기 ×{1 << b.heundeulCount}({b.heundeulCount}회)");
        if (b.bombCount > 0) mult.Add($"폭탄 ×{1 << b.bombCount}({b.bombCount}회)");
        if (b.gwangBak) mult.Add("광박 ×2");
        if (b.piBak) mult.Add("피박 ×2");
        if (b.extraMultiplier > 1) mult.Add($"고정배수 ×{b.extraMultiplier}");

        var foot = new System.Text.StringBuilder();
        foot.AppendLine(mult.Count > 0
            ? $"배수: {string.Join(" · ", mult)}  =  ×{b.totalMultiplier}"
            : "배수 없음(×1)");
        foot.AppendLine($"<color=#8A6300><b>{owner} 최종 점수: {b.finalScore}점</b></color>"); // 흰 본문 위라 어두운 금색으로 — 밝은 금색(FFD966 등)은 대비가 안 나온다
        foot.Append($"({other} 기본 점수 {pendingOtherBaseScore}점 — 정산 배수 미적용, 참고용)");
        scoreDetailPopup.footerText.text = foot.ToString();

        scoreDetailPopup.Show(); // dim 활성화 + Overlay보다 위로 SetAsLastSibling까지 컴포넌트가 처리
    }

    /// <summary>점수 항목 줄(라벨+점수) 밑에 그 점수에 관여한 카드 실물을 작게 늘어놓는다.
    /// <see cref="GoStopRules.BuildScoreLines"/>가 텍스트(<see cref="GoStopRules.FormatScoreLines"/>와
    /// 동일 판정)와 카드 목록을 같이 돌려주므로 텍스트·카드가 어긋날 일이 없다.
    /// 스크롤 콘텐츠의 실제 필요 높이를 재서 <c>content.sizeDelta</c>에 반영한다 —
    /// 안 그러면 ScrollRect가 몇 줄까지 스크롤해야 할지 모른다.</summary>
    float BuildScoreDetailRows(RectTransform content, List<HwatuCard> captured, GoStopRules.Score baseScore)
    {
        HwatuUI.ClearChildren(content);
        var lines = GoStopRules.BuildScoreLines(captured, baseScore);

        // 본문(PanelBody)이 밝은 바탕이라 어두운 글자를 쓴다 — 딤 위에 뜨던
        // 예전 팝업(어두운 배경 + 흰 글자)과 정반대라 자칫 안 바꾸면 안 보인다.
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

    /// <summary>결과 화면에서 승자 점수만 보이고 상대가 뭘 먹었는지 모른다는
    /// 요청 — 나/상대 양쪽의 획득패 실물을 점수 분해 바로 아래, 같은 스크롤
    /// 콘텐츠에 이어서 보여준다(4인판 AppendAllCapsSection과 같은 설계·
    /// 같은 시각 스타일 — 카드 ID/문자열이 아니라 실제 카드 이미지).</summary>
    float AppendAllCapsSection(RectTransform content, float y, (string name, List<HwatuCard> cards)[] piles)
    {
        var textCol = new Color(0.16f, 0.14f, 0.06f, 1f);
        y += 16f;
        var divider = HwatuUI.MakeLabel(content, new Vector2(0f, -y), new Vector2(860f, 30f), 20f, new Color(textCol.r, textCol.g, textCol.b, 0.55f));
        divider.text = "── 전체 획득패 ──";
        divider.alignment = TextAlignmentOptions.Center;
        y += 36f;

        const float cardW = 30f, cardH = 44f, cardGap = 3f, rowGap = 8f;
        const int perRow = 12;
        foreach (var (name, pile) in piles)
        {
            var nameLbl = HwatuUI.MakeLabel(content, new Vector2(0f, -y), new Vector2(860f, 30f), 20f, textCol);
            nameLbl.text = $"{name} ({pile.Count}장)";
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
            y += 14f;
        }
        return y;
    }

    void BuildScoreDetailUI(RectTransform canvasRoot)
    {
        scoreDetailPopup = HwatuUI.InstantiatePopup<ScoreDetailPopup>("ScoreDetailPopup", canvasRoot);
        // 닫기 버튼(헤더 X + 하단 "닫기")은 프리팹 저장 시점에 이미
        // comp.Hide로 persistent 연결돼 있다 — 여기서 다시 연결할 필요 없다.
    }

    /// <summary>총통 — 딜 직후 즉시 승리. 캡처 점수가 없으니 고정 3점에
    /// 총통 배수(x4)만 적용해서 정산한다.</summary>
    void EndGameChongtong(bool isPlayerSide)
    {
        // 2026-08-19: 예전엔 ShowTimedToast를 직접 불러서 Toast()가 원래
        // 해주는 사운드(PlayForLabel)·이펙트(ShowActionPopup) 둘 다 빠져
        // 있었다 — "총통!"에 대응하는 사운드가 아예 없던 원인. Toast()를
        // 거치도록 바꿨다("!"는 Toast가 자동으로 붙이므로 라벨에서 뺐다).
        Toast(isPlayerSide, "총통");
        // 총통 전용 팝업 프리팹은 없어서(ShowActionPopup은 조기 리턴) 대신
        // 큼직한 금색 파티클 버스트로 화려하게 알린다 — 딜 직후 즉시
        // 승리하는 희귀 이벤트라 다른 어떤 캡처 이벤트보다도 임팩트가
        // 커야 한다("파티클 이펙트로 좀 더 역동적으로" 요청).
        GoStopIcons.SpawnBurst(fieldArea.parent as RectTransform,
            fieldArea.anchoredPosition + new Vector2(0f, -60f), new Color(1f, 0.85f, 0.3f), count: 24);
        EndGame(aiWon: !isPlayerSide, fixedBaseScore: 3, extraMultiplier: 4);
    }
}
