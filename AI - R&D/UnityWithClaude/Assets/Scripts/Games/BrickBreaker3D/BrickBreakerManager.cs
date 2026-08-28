using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using TMPro;

public class BrickBreakerManager : MonoBehaviour
{
    public static BrickBreakerManager Instance { get; private set; }

    const float S = 1.75f;

    const int   COLS        = 8;
    const int   GRID_ROWS   = 6;
    const float COL_W       = 1.0f * S;
    const float ROW_H       = 1.0f * S;
    const float LAYER_STEP  = 1.0f * S;
    // 5층. 브릭은 LAYER_START에서 생겨 매 턴 LAYER_STEP씩 다가오고,
    // 5번째 전진에서 z=0이 되어 GAME_OVER_Z(0.4S)를 넘는다.
    const float LAYER_START = 5.0f * S;
    const float GAME_OVER_Z = 0.4f * S;

    const float LEFT_WALL   = -4f * S;
    const float RIGHT_WALL  =  4f * S;
    const float BOTTOM_WALL = -3f * S;
    const float TOP_WALL    =  3f * S;
    // 최심층 브릭 뒷면(LAYER_START + HalfZ = 5.5S) 뒤로 여유를 둔 위치.
    // 터널이 짧아졌으므로 같이 당기지 않으면 뒤쪽이 텅 빈 채 공만 오래 왕복한다.
    const float BACK_WALL   = 7.0f * S;
    const float RETURN_Z    = -0.5f * S;
    // 터널 표면·와이어프레임이 시작하는 z (카메라 바로 앞)
    const float TUNNEL_Z0   = -9f;

    const float FIRE_Z      = -0.1f * S;
    const float BALL_DELAY  = 0.05f;
    // 발사 전체에 쓰는 최대 시간. 볼이 늘수록 0.05×N이 무한정 커져서
    // (48개면 2.4초) 턴 대기의 지배적인 원인이 된다. 총량을 묶는다.
    const float MAX_LAUNCH_WINDOW = 0.9f;
    // 모드마다 규칙이 달라서 기록을 공유하면 비교가 성립하지 않는다.
    // 기본 모드는 기존 키를 그대로 써서 지금까지의 최고점을 보존한다.
    static string BestKey => BrickBreakerRules.IsItemMode
        ? "BestBrickBreakerItem"
        : "BestBrickBreaker";
    const string TUTORIAL_KEY = "BrickBreakerTutorialSeen";

    // 온보딩: 아무 조작 없이 이 시간이 지나면 하단에 조작 안내를 띄운다
    const float HINT_DELAY  = 2.5f;
    const int   HINT_TURNS  = 3;    // 처음 N턴까지만 안내

    [SerializeField] Camera         gameCamera;
    [SerializeField] GameUIManager  ui;
    [SerializeField] BrickBreakerAimer aimer;

    public List<BrickBreakerBrick> Bricks { get; private set; } = new();

    /// <summary>블롭 그림자가 활성 볼을 찾기 위해 읽는다. 비활성 항목이 섞여 있다.</summary>
    public IReadOnlyList<BrickBreakerBall> BallPool => ballPool;

    /// <summary>조준 중 발사 지점에 놓이는 대기 볼. 숨겨져 있으면 null.</summary>
    public Transform ReadyBall =>
        (readyBall != null && readyBall.activeSelf) ? readyBall.transform : null;

    enum State { Aiming, Firing, Advancing, GameOver }
    State state = State.Aiming;

    int   turn;
    int   score;
    int   ballCount = 1;

    // ── 파워업 (아이템 모드) ─────────────────────────────
    public const  float BALL_BASE_RADIUS = 0.22f;
    public float  BallRadius { get; private set; } = BALL_BASE_RADIUS;
    public float  BallDamage { get; private set; } = 1f;
    int           itemLuck;
    float fireX;

    int  roundBricksDestroyed;

    List<BrickBreakerBall> ballPool  = new();
    List<BrickBreakerBall> shotBalls = new();
    GameObject             readyBall;
    int   ballsInFlight;
    bool  launchComplete = true;
    float firstReturnX  = float.NaN;
    float leaderReturnX = float.NaN;   // 첫 번째로 쏜 공(리더)의 복귀 지점

    BrickBreakerAimUI  aimUI;
    BrickBreakerRankUI rankUI;

    // 이어하기는 한 판에 한 번. 무제한이면 광고만 보면 절대 안 죽는 게임이 된다.
    const int MAX_CONTINUES = 1;
    int continuesUsed;

    // 매판 전면 광고는 과하다. 게임오버 N번마다 한 번.
    const string PLAYS_KEY = "BBPlayCount";
    const int    INTERSTITIAL_EVERY = 3;

    float idleTime;      // 조준 상태에서 아무 입력 없이 흐른 시간
    int   shotsFired;    // 발사 횟수 (안내 종료 판단용)

    // Camera orbit
    const float ORBIT_SENSITIVITY    = 0.18f;
    const float ORBIT_CLAMP_AZ       = 90f;
    const float ORBIT_CLAMP_EL       = 60f;
    const float JOYSTICK_ORBIT_SPEED = 100f; // deg/sec at full deflection

    // Joysticks
    VirtualJoystick leftJoystick;
    VirtualJoystick rightJoystick;
    Vector3 camBasePos;
    Vector3 camTarget;
    float   camOrbitRadius;
    float   camAzimuth;
    float   camElevation;
    Vector3 camShakeOffset;

    LineRenderer   aimArrowLR;
    LineRenderer[] gameOverEdges;

    // ── Lifecycle ────────────────────────────────────────
    void Awake() => Instance = this;

    void Start()
    {
        if (!gameCamera) gameCamera = Camera.main;
        if (gameCamera)
        {
            gameCamera.clearFlags       = CameraClearFlags.SolidColor;
            // 터널 내부(0.12~0.20대)와 확실히 갈라지도록 배경은 거의 검정으로.
            // 예전엔 배경과 벽이 같은 어두운 남색이라 안팎 경계가 안 읽혔다.
            gameCamera.backgroundColor  = new Color(0.015f, 0.015f, 0.035f);
        }
        camBasePos     = gameCamera ? gameCamera.transform.position : Vector3.zero;
        // 타깃 y를 카메라 y에 맞춰 수평 시선을 유지한다. 0으로 고정하면 카메라를
        // 올렸을 때 화면이 내려가는 게 아니라 카메라가 아래로 기울기만 한다.
        // 씬의 카메라 y가 곧 화면 프레이밍 오프셋 — HUD가 상단을 가리는 만큼 내린다.
        camTarget      = new Vector3(0f, camBasePos.y, LAYER_START * 0.45f);
        camOrbitRadius = Vector3.Distance(camBasePos, camTarget);
        if (aimer) aimer.SetBounds(LEFT_WALL, RIGHT_WALL, BOTTOM_WALL, TOP_WALL, BACK_WALL);
        fireX = 0f;

        InitUI();
        CreateReadyBall();
        CreateReticle();
        aimCursor = new Vector3(0f, 0f, LAYER_START);
        CreateAimArrow();
        CreateGameOverLine();
        DrawTunnel();
        SpawnLayer();
        UpdateDangerBricks();
        UpdateHUD();
        CreateComboUI();
        VirtualJoystick.CreatePair(out leftJoystick, out rightJoystick);

        // 깊이 슬라이더 + 발사 버튼. 조이스틱이 같은 자리를 물지 않도록
        // 자기 영역을 등록한다 (조이스틱 캔버스에는 레이캐스터가 없다).
        // Create() 안에서 모드에 맞춰 조이스틱 제외 영역까지 등록한다
        BrickBreakerAds.Create();

        // 온라인 보드로 올린다. 서버가 안 되면 UgsRankingStore가 알아서 로컬로
        // 폴백하므로, 대시보드 설정 전에도 게임은 그대로 돌아간다.
        BrickBreakerRanking.Store = new UgsRankingStore();

        // 랭킹 UI를 먼저 만든다 — AimUI.Create()가 안에서 RefreshBlockedZones()를
        // 부르는데, 거기서 랭킹 칩 영역까지 같이 등록하기 때문이다.
        rankUI = BrickBreakerRankUI.Create();
        aimUI  = BrickBreakerAimUI.Create();

        // 이펙트 매니저
        if (BrickBreakerFX.Instance == null)
            new GameObject("BrickBreakerFX").AddComponent<BrickBreakerFX>();
        BrickBreakerFX.Instance?.SetCamera(gameCamera);

        // 효과음 (절차적 생성 — 오디오 에셋 없음)
        if (BrickBreakerAudio.Instance == null)
            new GameObject("BrickBreakerAudio").AddComponent<BrickBreakerAudio>();

        // 터널 표면 + 바닥 그림자 — z축 깊이감용
        if (BrickBreakerShadows.Instance == null)
            new GameObject("BrickBreakerShadows").AddComponent<BrickBreakerShadows>();
        BrickBreakerShadows.Instance?.Configure(
            LEFT_WALL, RIGHT_WALL, BOTTOM_WALL, TOP_WALL, TUNNEL_Z0, BACK_WALL);
    }

    void InitUI()
    {
        if (!ui) return;
        var loc = LocalizationManager.Instance;
        string baseTitle = loc != null ? loc.GetOr("title_brickbreaker", "벽돌깨기 3D") : "벽돌깨기 3D";
        ui.SetTitle(baseTitle + " · " + BrickBreakerRules.NameOf(BrickBreakerRules.Mode));
        ui.SetBestVisible(true);
        ui.SetBest(PlayerPrefs.GetInt(BestKey, 0));
        // 공용 UI(GameUI 프리팹) 버튼 동작 등록. Back은 기본 동작(TitleScene) 사용.
        ui.SetNewGameAction(OnNewGame);

        // ── 온보딩 ────────────────────────────────────────
        string L(string k, string fallback) => loc?.Get(k) ?? fallback;
        string body = string.Join("\n\n", new[]
        {
            "1.  " + L("bb_help_1", "화면 오른쪽을 드래그해 조준하세요"),
            "2.  " + L("bb_help_2", "손을 떼면 공이 발사됩니다"),
            "3.  " + L("bb_help_3", "화면 왼쪽을 드래그하면 시점이 돌아갑니다"),
            "4.  " + L("bb_help_4", "노란 구슬을 맞히면 공이 늘어납니다"),
            "5.  " + L("bb_help_5", "벽돌이 앞쪽 빨간 선에 닿으면 게임 오버"),
        });
        ui.SetHelp(L("help_title", "조작법"), body, L("btn_close", "확인"));

        // 첫 실행이면 자동으로 펼친다
        if (PlayerPrefs.GetInt(TUTORIAL_KEY, 0) == 0)
        {
            ui.ShowHelp();
            PlayerPrefs.SetInt(TUTORIAL_KEY, 1);
            PlayerPrefs.Save();
        }
    }

    // ── Input ────────────────────────────────────────────
    void Update()
    {
        // 도움말이 열려 있는 동안에는 게임 입력을 전부 막는다
        if (ui != null && ui.IsHelpOpen)
        {
            if (leftJoystick  != null) leftJoystick.Interactable  = false;
            if (rightJoystick != null) rightJoystick.Interactable = false;
            if (aimUI != null) aimUI.Interactable = false;
            return;
        }
        if (state != State.GameOver)
        {
            if (leftJoystick  != null) leftJoystick.Interactable  = true;
            if (rightJoystick != null) rightJoystick.Interactable = true;
        }

        // 게임 규칙 모드 전환 — 판이 통째로 달라지므로 바로 새 게임을 시작한다
        if (aimUI != null && aimUI.GameModeTapped)
        {
            BrickBreakerRules.SetMode(BrickBreakerRules.Other);
            Restart();
            return;
        }

        UpdateIdleHint();
        UpdateComboTimer();
        UpdateReadyBall();
        UpdateBgmLayers();

        HandleCameraOrbit();

        // Left joystick: continuous camera orbit (mobile)
        if (leftJoystick != null && leftJoystick.IsPressed)
        {
            var joy = leftJoystick.Direction;
            camAzimuth   = Mathf.Clamp(camAzimuth   + joy.x * JOYSTICK_ORBIT_SPEED * Time.deltaTime, -ORBIT_CLAMP_AZ, ORBIT_CLAMP_AZ);
            camElevation = Mathf.Clamp(camElevation  - joy.y * JOYSTICK_ORBIT_SPEED * Time.deltaTime, -ORBIT_CLAMP_EL, ORBIT_CLAMP_EL);
        }

        ApplyCameraOrbit();

        if (aimUI != null) aimUI.Interactable = state == State.Aiming;

        bool padMode = aimUI == null || aimUI.Mode == BrickBreakerAimUI.InputMode.Pad;

        // 모드에 따라 패드를 통째로 넣고 뺀다.
        //  · 패드 모드 : 패드가 화면 터치를 전부 가져간다 (조준은 스틱·슬라이더·버튼)
        //  · 터치 모드 : 패드를 숨기고 화면을 직접 만진다
        if (state != State.GameOver)
        {
            if (leftJoystick != null)
            {
                leftJoystick.SetVisible(padMode);
                leftJoystick.Interactable = padMode;
            }
            if (rightJoystick != null)
            {
                rightJoystick.SetVisible(padMode);
                rightJoystick.Interactable = padMode;
            }
        }

        if (state != State.Aiming)
        {
            aimer?.HideAim();
            HideAimArrow();
            if (reticle) reticle.SetActive(false);
            touchAimPtr = BrickBreakerPointer.None;

            // 공이 날아가는 동안엔 조준할 게 없다 → 화면 아무 데나 끌면 시점 회전
            if (!padMode) { ClaimTouchPointers(false); UpdateTouchOrbit(); }
            else            touchOrbitPtr = BrickBreakerPointer.None;
            return;
        }

        Vector3 origin = new Vector3(fireX, 0f, FIRE_Z);

        if (padMode)
        {
            UpdatePadAim(origin);
        }
        else
        {
            ClaimTouchPointers(true);   // 터널을 짚었나 바깥을 짚었나로 역할이 갈린다
            UpdateTouchOrbit();
            UpdateTouchAim(origin);
        }
    }

    // ── 터치 모드 시점 조작 ───────────────────────────────
    int     touchOrbitPtr = BrickBreakerPointer.None;
    Vector2 touchOrbitPrev;

    /// <summary>
    /// 왼쪽 절반 드래그로 카메라를 돌린다. 터치 모드에선 왼쪽 패드가 숨겨지므로
    /// 이게 없으면 모바일에서 시점 조작 수단이 아예 사라진다.
    /// </summary>
    /// <summary>
    /// 새 포인터의 역할을 정한다. **터널 안쪽을 짚었으면 조준, 바깥(배경)이면 시점.**
    /// 화면을 좌우로 나누던 방식보다 직관적이다 — 조준하고 싶은 곳은 언제나
    /// 터널 안이고, 돌리고 싶을 땐 빈 배경을 잡으면 된다.
    /// </summary>
    /// <param name="aimAllowed">false면 전부 시점 회전으로 준다 (발사 중).</param>
    void ClaimTouchPointers(bool aimAllowed)
    {
        if (gameCamera == null) return;

        foreach (var (id, pos, began) in BrickBreakerPointer.All())
        {
            if (!began) continue;
            if (id == touchAimPtr || id == touchOrbitPtr) continue;
            if (!InTouchAimZone(pos)) continue;

            bool onTunnel = aimAllowed && RayHitsTunnel(gameCamera.ScreenPointToRay(pos));

            if (onTunnel && touchAimPtr == BrickBreakerPointer.None)
            {
                touchAimPtr = id;
                touchAimPos = pos;
            }
            else if (!onTunnel && touchOrbitPtr == BrickBreakerPointer.None)
            {
                touchOrbitPtr  = id;
                touchOrbitPrev = pos;
            }
        }
    }

    /// <summary>
    /// 레이가 터널 상자를 통과하는가. 터치 모드에서 조준/시점을 가르는 기준.
    ///
    /// 근거리 끝(AIM_NEAR_Z)은 <b>TUNNEL_Z0(-9)로 넓히면 안 된다.</b>
    /// 카메라가 z=-10이라 z=-9 면은 1유닛 앞이고, 14×10.5 크기라 화면 전체를
    /// 덮어버린다 → 모든 터치가 조준이 되어 시점 회전이 불가능해진다.
    /// 화면에 보이는 가까운 바닥은 z<0 구간이라 조준 영역에서 빠지지만,
    /// 그 대가로 배경(코너)이 시점 조작용으로 남는다.
    /// </summary>
    const float AIM_NEAR_Z = FIRE_Z;

    bool RayHitsTunnel(Ray r)
    {
        float tE = -1e9f, tX = 1e9f;
        return Slab(r.origin.x, r.direction.x, LEFT_WALL,   RIGHT_WALL, ref tE, ref tX)
            && Slab(r.origin.y, r.direction.y, BOTTOM_WALL, TOP_WALL,   ref tE, ref tX)
            && Slab(r.origin.z, r.direction.z, AIM_NEAR_Z,  BACK_WALL,  ref tE, ref tX)
            && tX > 0f;
    }

    void UpdateTouchOrbit()
    {
        if (touchOrbitPtr == BrickBreakerPointer.None) return;

        if (!BrickBreakerPointer.TryGet(touchOrbitPtr, out Vector2 p))
        {
            touchOrbitPtr = BrickBreakerPointer.None;
            return;
        }

        Vector2 d = p - touchOrbitPrev;
        touchOrbitPrev = p;
        camAzimuth   = Mathf.Clamp(camAzimuth   + d.x * ORBIT_SENSITIVITY, -ORBIT_CLAMP_AZ, ORBIT_CLAMP_AZ);
        camElevation = Mathf.Clamp(camElevation - d.y * ORBIT_SENSITIVITY, -ORBIT_CLAMP_EL, ORBIT_CLAMP_EL);
    }

    // ── 패드 모드 ────────────────────────────────────────
    /// <summary>
    /// 조준 = 터널 안의 3D 목표점을 옮기는 것.
    /// 오른쪽 스틱이 x·y를, 깊이 슬라이더가 z를 정하고 발사 방향은
    /// 발사 지점 → 목표점이 된다. 각도만 정하던 예전 방식으로는
    /// "저 브릭 앞쪽"처럼 깊이를 지정할 방법이 없었다.
    /// </summary>
    void UpdatePadAim(Vector3 origin)
    {
        if (reticle && !reticle.activeSelf) reticle.SetActive(true);

        UpdateAimCursor();
        Vector3 dir = AimDirection(origin);

        aimer?.ShowAim(origin, dir);
        UpdateAimArrow(origin, dir);

        if (aimUI != null && aimUI.FireRequested) StartCoroutine(FireAll(dir));
    }

    // ── 터치 모드 ────────────────────────────────────────
    int     touchAimPtr = BrickBreakerPointer.None;
    Vector2 touchAimPos;

    /// <summary>
    /// 화면을 눌러 조준점을 잡고, 끌어서 조정하고, 떼면 발사한다.
    ///
    /// 오른쪽 절반에서 **시작한** 포인터만 잡고 touchId로 끝까지 추적한다.
    /// primaryTouch 같은 폴백을 쓰면 왼쪽 엄지(카메라)가 primaryTouch가 되어
    /// 시점 조작이 그대로 조준선을 끌고 다니다 손을 떼는 순간 발사된다.
    /// </summary>
    void UpdateTouchAim(Vector3 origin)
    {
        if (reticle) reticle.SetActive(false);

        if (touchAimPtr == BrickBreakerPointer.None)
        {
            aimer?.HideAim(); HideAimArrow();
            if (reticle) reticle.SetActive(false);
            return;
        }

        if (BrickBreakerPointer.TryGet(touchAimPtr, out Vector2 p))
        {
            touchAimPos = p;   // 드래그로 조정
        }
        else
        {
            // 손을 뗐다 → 발사
            touchAimPtr = BrickBreakerPointer.None;
            aimer?.HideAim();
            HideAimArrow();
            Vector3 fired = TouchPointToDir(touchAimPos, origin);
            if (fired.z > 0.02f) StartCoroutine(FireAll(fired));
            return;
        }

        if (touchAimPtr == BrickBreakerPointer.None)
        {
            aimer?.HideAim(); HideAimArrow();
            if (reticle) reticle.SetActive(false);
            return;
        }

        Vector3 dir = TouchPointToDir(touchAimPos, origin);
        if (reticle) { reticle.SetActive(true); reticle.transform.position = aimCursor; }
        if (dir.z > 0.02f) { aimer?.ShowAim(origin, dir); UpdateAimArrow(origin, dir); }
        else               { aimer?.HideAim(); HideAimArrow(); }
    }

    /// <summary>
    /// 손끝이 가리키는 **터널 안의 실제 지점**으로 조준한다. 브릭 위를 짚으면
    /// 그 브릭이 목표가 된다.
    ///
    /// 예전 ScreenToTunnelDir은 발사 지점 대비 드래그 "변위"를 최대 80° 각도로
    /// 환산했다. 그래서 조금만 끌어도 조준이 크게 튀고, 손끝 위치와 실제 목표가
    /// 전혀 맞지 않았다("가중치가 걸린 것 같다"는 증상).
    /// </summary>
    Vector3 TouchPointToDir(Vector2 screenPos, Vector3 origin)
    {
        if (!gameCamera) return Vector3.forward;

        Ray ray = gameCamera.ScreenPointToRay(screenPos);

        // 터널 안쪽 벽까지가 기본 목표 거리
        float t = RayTunnelExit(ray);

        // 브릭을 짚었으면 그 브릭이 우선
        var half = new Vector3(BrickBreakerBrick.HalfX, BrickBreakerBrick.HalfY, BrickBreakerBrick.HalfZ);
        foreach (var b in Bricks)
        {
            if (b == null || b.Dead) continue;
            Vector3 c  = b.transform.position;
            float   te = RayAabbEnter(ray, c - half, c + half);
            if (te > 0f && te < t) t = te;
        }

        aimCursor = ray.origin + ray.direction * t;   // 레티클이 여기에 선다

        // 터널을 완전히 빗나간 레이는 폴백 평면에 떨어져 벽 바깥으로 나간다.
        // 안으로 물려야 레티클이 화면 밖으로 도망가지 않고 조준도 예측 가능해진다.
        aimCursor.x = Mathf.Clamp(aimCursor.x, LEFT_WALL   + CURSOR_MARGIN, RIGHT_WALL - CURSOR_MARGIN);
        aimCursor.y = Mathf.Clamp(aimCursor.y, BOTTOM_WALL + CURSOR_MARGIN, TOP_WALL   - CURSOR_MARGIN);
        aimCursor.z = Mathf.Clamp(aimCursor.z, CURSOR_Z_MIN, CURSOR_Z_MAX);

        Vector3 d = aimCursor - origin;
        if (d.z < 0.001f) d.z = 0.001f;                 // 항상 앞으로
        return ClampToCone(d.normalized);
    }

    static bool Slab(float o, float d, float mn, float mx, ref float tEnter, ref float tExit)
    {
        if (Mathf.Abs(d) < 1e-6f) return o >= mn && o <= mx;
        float t1 = (mn - o) / d, t2 = (mx - o) / d;
        if (t1 > t2) (t1, t2) = (t2, t1);
        tEnter = Mathf.Max(tEnter, t1);
        tExit  = Mathf.Min(tExit,  t2);
        return tEnter <= tExit;
    }

    /// <summary>레이가 터널 상자를 빠져나가는 거리. 빗나가면 브릭 층 평면으로 떨어뜨린다.</summary>
    float RayTunnelExit(Ray r)
    {
        float tE = -1e9f, tX = 1e9f;
        bool hit = Slab(r.origin.x, r.direction.x, LEFT_WALL,   RIGHT_WALL, ref tE, ref tX)
                && Slab(r.origin.y, r.direction.y, BOTTOM_WALL, TOP_WALL,   ref tE, ref tX)
                && Slab(r.origin.z, r.direction.z, FIRE_Z,      BACK_WALL,  ref tE, ref tX);

        if (hit && tX > 0f) return tX;

        // 폴백: 브릭 스폰 평면에 투영
        float dz = r.direction.z;
        if (Mathf.Abs(dz) < 1e-4f) return LAYER_START;
        return Mathf.Max(0.5f, (LAYER_START - r.origin.z) / dz);
    }

    static float RayAabbEnter(Ray r, Vector3 mn, Vector3 mx)
    {
        float tE = -1e9f, tX = 1e9f;
        if (!Slab(r.origin.x, r.direction.x, mn.x, mx.x, ref tE, ref tX)) return -1f;
        if (!Slab(r.origin.y, r.direction.y, mn.y, mx.y, ref tE, ref tX)) return -1f;
        if (!Slab(r.origin.z, r.direction.z, mn.z, mx.z, ref tE, ref tX)) return -1f;
        return tX < 0f ? -1f : Mathf.Max(tE, 0f);
    }

    /// <summary>HUD·모드 토글만 피하면 화면 어디든 입력을 받는다.</summary>
    bool InTouchAimZone(Vector2 sp)
    {
        if (sp.y > Screen.safeArea.yMax - 116f * (Screen.height / 1920f)) return false;
        return aimUI == null || aimUI.IsFreeForAim(sp);
    }

    // ── 조준 커서 ────────────────────────────────────────
    const float CURSOR_SPEED = 11f;              // 스틱 최대일 때 유닛/초
    const float CURSOR_Z_MIN = 1.0f;
    const float CURSOR_Z_MAX = BACK_WALL - 0.75f;
    const float CURSOR_MARGIN = 0.4f;

    Vector3      aimCursor;
    GameObject   reticle;

    void UpdateAimCursor()
    {
        // 오른쪽 스틱 — 360도 자유 이동 (예전처럼 각도로 환산하지 않는다)
        if (rightJoystick != null && rightJoystick.IsPressed)
        {
            Vector2 j = rightJoystick.Direction;
            aimCursor.x += j.x * CURSOR_SPEED * Time.deltaTime;
            aimCursor.y += j.y * CURSOR_SPEED * Time.deltaTime;
        }

        if (aimUI != null)
            aimCursor.z = Mathf.Lerp(CURSOR_Z_MIN, CURSOR_Z_MAX, aimUI.ZNormalized);

        aimCursor.x = Mathf.Clamp(aimCursor.x, LEFT_WALL   + CURSOR_MARGIN, RIGHT_WALL - CURSOR_MARGIN);
        aimCursor.y = Mathf.Clamp(aimCursor.y, BOTTOM_WALL + CURSOR_MARGIN, TOP_WALL   - CURSOR_MARGIN);

        if (reticle) reticle.transform.position = aimCursor;
    }

    Vector3 AimDirection(Vector3 origin)
    {
        Vector3 d = aimCursor - origin;
        if (d.z < 0.001f) d.z = 0.001f;    // 항상 앞으로 나가야 한다
        return ClampToCone(d.normalized);
    }

    /// <summary>블롭 그림자가 커서에도 붙도록 노출. 커서 z를 눈으로 읽는 핵심 수단.</summary>
    public Transform AimCursorTransform =>
        (reticle != null && reticle.activeSelf) ? reticle.transform : null;

    void CreateReticle()
    {
        reticle = new GameObject("AimReticle");
        const float s = 0.6f;
        MakeReticleAxis(reticle.transform, Vector3.right   * s);
        MakeReticleAxis(reticle.transform, Vector3.up      * s);
        MakeReticleAxis(reticle.transform, Vector3.forward * s);
        reticle.SetActive(false);
    }

    static void MakeReticleAxis(Transform parent, Vector3 halfAxis)
    {
        var go = new GameObject("ReticleAxis");
        go.transform.SetParent(parent, false);
        var lr = go.AddComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.SetPosition(0, -halfAxis);
        lr.SetPosition(1,  halfAxis);
        lr.useWorldSpace = false;          // 부모(커서)를 따라다닌다
        lr.startWidth = lr.endWidth = 0.075f;
        lr.material   = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = lr.endColor = new Color(0.30f, 1f, 1f, 0.95f);
        lr.sortingOrder = 20;
    }

    /// <summary>
    /// 처음 몇 턴 동안, 조준 상태에서 아무것도 안 하고 있으면
    /// 하단에 조작 안내를 띄운다. 손을 대면 바로 사라진다.
    /// </summary>
    void UpdateIdleHint()
    {
        if (ui == null) return;

        bool eligible = state == State.Aiming && shotsFired < HINT_TURNS;
        if (!eligible) { if (ui.IsToastVisible) ui.HideToast(); idleTime = 0f; shownHint = null; return; }

        // 터치 모드엔 스틱이 없으므로 화면 포인터도 "조작 중"으로 쳐야 한다
        bool touching = (rightJoystick != null && rightJoystick.IsPressed)
                     || (leftJoystick  != null && leftJoystick.IsPressed)
                     || touchAimPtr   != BrickBreakerPointer.None
                     || touchOrbitPtr != BrickBreakerPointer.None;

        if (touching)
        {
            idleTime = 0f;
            if (ui.IsToastVisible) ui.HideToast();
            shownHint = null;
            return;
        }

        idleTime += Time.deltaTime;
        if (idleTime < HINT_DELAY) return;

        // 모드를 바꾸면 떠 있는 안내도 그 자리에서 바뀌어야 한다
        string hint = CurrentHint();
        if (!ui.IsToastVisible || hint != shownHint)
        {
            ui.ShowToast(hint);
            shownHint = hint;
        }
    }

    string shownHint;

    string CurrentHint()
    {
        var  loc = LocalizationManager.Instance;
        bool pad = aimUI == null || aimUI.Mode == BrickBreakerAimUI.InputMode.Pad;

        string key      = pad ? "bb_hint_pad" : "bb_hint_touch";
        string fallback = pad
            ? "오른쪽 스틱으로 조준 · 슬라이더로 깊이 · 발사 버튼"
            : "터널 안을 드래그해 조준, 떼면 발사 · 바깥을 끌면 시점";

        return loc != null ? loc.GetOr(key, fallback) : fallback;
    }

    // ── 층별 BGM ─────────────────────────────────────────
    int lastBgmMask = -1;

    /// <summary>
    /// 브릭이 존재하는 깊이 층을 비트마스크로 오디오에 넘긴다.
    /// 층 0이 플레이어에 가장 가까운 줄 — 가까울수록 리듬이 두꺼워져
    /// 브릭이 밀려올수록 곡이 저절로 거칠어진다.
    /// </summary>
    void UpdateBgmLayers()
    {
        var audio = BrickBreakerAudio.Instance;
        if (audio == null) return;

        int mask = 0;
        foreach (var b in Bricks)
        {
            if (b == null || b.Dead || b.IsBallItem) continue;

            // z = LAYER_START - k*LAYER_STEP  →  k가 클수록 가깝다.
            // 층 0을 '가장 가까움'으로 뒤집어 쓴다.
            int k = Mathf.RoundToInt((LAYER_START - b.transform.position.z) / LAYER_STEP);
            int layer = Mathf.Clamp(BrickBreakerAudio.BGM_LAYERS - 1 - k, 0,
                                    BrickBreakerAudio.BGM_LAYERS - 1);
            mask |= 1 << layer;
        }

        if (mask == lastBgmMask) return;
        lastBgmMask = mask;
        audio.SetActiveLayers(mask);
    }

    // ── Camera ───────────────────────────────────────────
    void HandleCameraOrbit()
    {
        if (!gameCamera) return;
        var mouse = Mouse.current;
        if (mouse == null || !mouse.rightButton.isPressed) return;
        Vector2 delta = mouse.delta.ReadValue();
        camAzimuth   = Mathf.Clamp(camAzimuth   + delta.x * ORBIT_SENSITIVITY, -ORBIT_CLAMP_AZ, ORBIT_CLAMP_AZ);
        camElevation = Mathf.Clamp(camElevation  - delta.y * ORBIT_SENSITIVITY, -ORBIT_CLAMP_EL, ORBIT_CLAMP_EL);
    }

    void ApplyCameraOrbit()
    {
        if (!gameCamera) return;
        Quaternion rot     = Quaternion.Euler(camElevation, camAzimuth, 0f);
        Vector3    baseDir = (camBasePos - camTarget).normalized;
        Vector3    orbitPos = camTarget + rot * baseDir * camOrbitRadius;
        gameCamera.transform.position = orbitPos + camShakeOffset;
        gameCamera.transform.LookAt(camTarget);
    }

    IEnumerator DoCameraShake(float amount, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            camShakeOffset = new Vector3(
                Random.Range(-amount, amount),
                Random.Range(-amount, amount), 0f);
            elapsed += Time.deltaTime;
            yield return null;
        }
        camShakeOffset = Vector3.zero;
    }

    Vector3 ScreenToTunnelDir(Vector2 sp)
    {
        // Use ball's screen position as anchor — point where you want to shoot, like 2D Brick Breaker
        Vector3 ballScreen = gameCamera
            ? gameCamera.WorldToScreenPoint(new Vector3(fireX, 0f, FIRE_Z))
            : new Vector3(Screen.width * 0.5f, Screen.height * 0.25f, 0f);
        Vector2 delta = sp - new Vector2(ballScreen.x, ballScreen.y);
        float nx = Mathf.Clamp(delta.x / (Screen.width  * 0.5f), -1f, 1f);
        float ny = Mathf.Clamp(delta.y / (Screen.height * 0.5f), -1f, 1f);
        float xAngle = nx * MAX_OFF_AXIS_DEG * Mathf.Deg2Rad;
        float yAngle = ny * MAX_OFF_AXIS_DEG * Mathf.Deg2Rad;
        float sx = Mathf.Sin(xAngle);
        float sy = Mathf.Sin(yAngle);
        float cz = Mathf.Cos(xAngle) * Mathf.Cos(yAngle);
        if (cz < 0.02f) return Vector3.zero; // reject near-perpendicular
        return ClampToCone(new Vector3(sx, sy, cz).normalized);
    }

    Vector3 JoystickToTunnelDir(Vector2 joy)
    {
        // 상하·좌우 동일 범위. 예전엔 수직만 40°로 묶여 있어 천장·바닥은
        // 스치듯 맞힐 수가 없었다(입사각 최소 50°).
        float xAngle = joy.x * MAX_OFF_AXIS_DEG * Mathf.Deg2Rad;
        float yAngle = joy.y * MAX_OFF_AXIS_DEG * Mathf.Deg2Rad;
        float sx = Mathf.Sin(xAngle);
        float sy = Mathf.Sin(yAngle);
        float cz = Mathf.Cos(xAngle) * Mathf.Cos(yAngle);
        if (cz < 0.02f) return Vector3.zero;
        return ClampToCone(new Vector3(sx, sy, cz).normalized);
    }

    // 터널 축에서 벗어날 수 있는 최대 각. 90 - 80 = 10°가 벽과 이루는 최소 각이며,
    // 2D 벽돌깨기의 "10°~170°" 제한과 같은 의미다.
    const float MAX_OFF_AXIS_DEG = 80f;

    /// <summary>
    /// 발사 방향을 축 기준 원뿔 안으로 밀어 넣는다.
    /// x·y 각도를 따로 제한하면 합성 방향이 한계를 넘어(대각선 83.5°) 거의 수평으로
    /// 나가고, 그런 공은 좌우 벽만 오래 왕복한다.
    /// </summary>
    static Vector3 ClampToCone(Vector3 dir)
    {
        dir = dir.normalized;
        if (Vector3.Angle(Vector3.forward, dir) <= MAX_OFF_AXIS_DEG) return dir;

        Vector3 lateral = new Vector3(dir.x, dir.y, 0f);
        if (lateral.sqrMagnitude < 1e-8f) return Vector3.forward;
        lateral.Normalize();

        float rad = MAX_OFF_AXIS_DEG * Mathf.Deg2Rad;
        return (lateral * Mathf.Sin(rad) + Vector3.forward * Mathf.Cos(rad)).normalized;
    }

    // ── Ready ball ───────────────────────────────────────
    /// <summary>
    /// 조준 중 발사 지점에 놓이는 대기 볼. 예전에는 조준 상태에 아무 오브젝트도
    /// 없어서 어디서 공이 나가는지, 발사 지점이 터널 어디쯤인지 알 수 없었다.
    /// 바닥 그림자도 이 볼이 있어야 생긴다(그림자는 실제 오브젝트를 따라간다).
    /// </summary>
    void CreateReadyBall()
    {
        readyBall = BrickBreakerMeshes.Make("ReadyBall", BrickBreakerMeshes.Sphere,
                                            MakeMat(new Color(1f, 0.82f, 0.25f)));
        readyBall.transform.localScale = Vector3.one * 0.55f;
        readyBall.SetActive(false);
    }

    void UpdateReadyBall()
    {
        if (readyBall == null) return;
        bool show = state == State.Aiming;
        if (readyBall.activeSelf != show) readyBall.SetActive(show);
        if (show) readyBall.transform.position = new Vector3(fireX, 0f, FIRE_Z);
    }

    // ── Aim arrow ────────────────────────────────────────
    void CreateAimArrow()
    {
        var go = new GameObject("AimArrow");
        aimArrowLR = go.AddComponent<LineRenderer>();
        aimArrowLR.positionCount = 2;
        aimArrowLR.startWidth    = 0.25f;
        aimArrowLR.endWidth      = 0.02f;
        var mat = new Material(Shader.Find("Sprites/Default"));
        mat.color = new Color(1f, 1f, 0.3f, 0.88f);
        aimArrowLR.material      = mat;
        aimArrowLR.useWorldSpace = true;
        aimArrowLR.sortingOrder  = 100;
        go.SetActive(false);
    }

    void UpdateAimArrow(Vector3 origin, Vector3 dir)
    {
        if (!aimArrowLR) return;
        aimArrowLR.gameObject.SetActive(true);
        aimArrowLR.SetPosition(0, origin);
        aimArrowLR.SetPosition(1, origin + dir);
    }

    void HideAimArrow() { if (aimArrowLR) aimArrowLR.gameObject.SetActive(false); }

    static readonly Color EdgeColorReady  = new Color(0.10f, 1.00f, 0.20f, 0.90f);
    static readonly Color EdgeColorFiring = new Color(1.00f, 0.12f, 0.04f, 0.85f);

    // ── Game over line ───────────────────────────────────
    void CreateGameOverLine()
    {
        float z = GAME_OVER_Z;
        gameOverEdges = new LineRenderer[4];
        gameOverEdges[0] = MakeGameOverEdge(new Vector3(LEFT_WALL,  BOTTOM_WALL, z), new Vector3(RIGHT_WALL, BOTTOM_WALL, z));
        gameOverEdges[1] = MakeGameOverEdge(new Vector3(LEFT_WALL,  TOP_WALL,    z), new Vector3(RIGHT_WALL, TOP_WALL,    z));
        gameOverEdges[2] = MakeGameOverEdge(new Vector3(LEFT_WALL,  BOTTOM_WALL, z), new Vector3(LEFT_WALL,  TOP_WALL,    z));
        gameOverEdges[3] = MakeGameOverEdge(new Vector3(RIGHT_WALL, BOTTOM_WALL, z), new Vector3(RIGHT_WALL, TOP_WALL,    z));
        SetEdgeColor(EdgeColorReady);
    }

    bool      dangerActive;
    Coroutine edgePulse;

    /// <summary>위험 펄스가 켜져 있으면 평상시 색(준비/발사중)은 무시한다.</summary>
    void SetEdgeColor(Color c)
    {
        if (dangerActive) return;
        ApplyEdgeColor(c);
    }

    void ApplyEdgeColor(Color c)
    {
        if (gameOverEdges == null) return;
        foreach (var lr in gameOverEdges)
            if (lr) { lr.startColor = lr.endColor = c; }
    }

    /// <summary>브릭이 코앞까지 오면 게임오버 프레임 전체가 붉게 맥동한다.</summary>
    void SetDangerPulse(bool on)
    {
        if (on == dangerActive) return;
        dangerActive = on;

        if (edgePulse != null) { StopCoroutine(edgePulse); edgePulse = null; }
        if (on) edgePulse = StartCoroutine(PulseEdges());
        else    ApplyEdgeColor(state == State.Firing ? EdgeColorFiring : EdgeColorReady);
    }

    IEnumerator PulseEdges()
    {
        var dim    = new Color(1f, 0.30f, 0.10f, 0.45f);
        var bright = new Color(1f, 0.10f, 0.05f, 1.00f);
        while (true)
        {
            float k = (Mathf.Sin(Time.time * 7f) + 1f) * 0.5f;
            ApplyEdgeColor(Color.Lerp(dim, bright, k));
            yield return null;
        }
    }

    static LineRenderer MakeGameOverEdge(Vector3 a, Vector3 b)
    {
        var go = new GameObject("GameOverEdge");
        var lr = go.AddComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.SetPosition(0, a); lr.SetPosition(1, b);
        lr.startWidth = lr.endWidth = 0.12f;
        lr.material   = new Material(Shader.Find("Sprites/Default"));
        lr.sortingOrder  = 5;
        lr.useWorldSpace = true;
        return lr;
    }

    // ── Fire ─────────────────────────────────────────────
    IEnumerator FireAll(Vector3 direction)
    {
        // 같은 프레임에 두 번 들어오면 ballsInFlight가 덮어써져 카운트가 깨진다.
        if (state != State.Aiming) yield break;

        state = State.Firing;
        shotsFired++;
        ui?.HideToast();
        if (shotsFired >= HINT_TURNS)
        {
            leftJoystick?.HideLabel();
            rightJoystick?.HideLabel();
        }
        SetEdgeColor(EdgeColorFiring);
        ballsInFlight        = ballCount;
        launchComplete       = false;
        firstReturnX         = float.NaN;
        leaderReturnX        = float.NaN;
        roundBricksDestroyed = 0;
        turnCleared          = false;
        shotBalls.Clear();

        Vector3 origin = new Vector3(fireX, 0f, FIRE_Z);

        // 볼이 많으면 간격을 줄여 전체 발사 시간을 상한 안에 묶는다
        float launchDelay = Mathf.Min(BALL_DELAY, MAX_LAUNCH_WINDOW / Mathf.Max(1, ballCount));

        // 발사를 코루틴 하나로 직렬화한다. 개별 코루틴으로 흩어놓으면
        // 라운드가 끝난 뒤에도 대기 중이던 발사가 살아남아 볼이 튀어나온다.
        for (int i = 0; i < ballCount; i++)
        {
            if (state != State.Firing) yield break;

            var ball = GetOrCreateBall();
            ball.SetLeader(i == 0);          // 첫 발이 다음 턴 발사 지점을 정한다
            ball.gameObject.SetActive(true); // reserve immediately so next iteration doesn't reuse same ball
            ball.transform.position = origin;
            shotBalls.Add(ball);
            ball.Fire(origin, direction, LEFT_WALL, RIGHT_WALL, BOTTOM_WALL, TOP_WALL, BACK_WALL, RETURN_Z);
            BrickBreakerFX.Instance?.MuzzleFlash(origin, direction);
            BrickBreakerAudio.Instance?.Fire();

            if (i < ballCount - 1) yield return new WaitForSeconds(launchDelay);
        }

        launchComplete = true;
        // 마지막 볼을 쏘기 전에 나머지가 전부 돌아왔을 수 있다
        if (ballsInFlight <= 0 && state == State.Firing) StartCoroutine(AfterFiring());
    }

    public float GetFrontBrickZ()
    {
        float minZ = float.MaxValue;
        foreach (var b in Bricks)
            if (b != null && !b.Dead)
                minZ = Mathf.Min(minZ, b.transform.position.z - BrickBreakerBrick.HalfZ);
        return minZ;
    }

    public void OnBallReturned(Vector3 pos, bool leader)
    {
        if (float.IsNaN(firstReturnX)) firstReturnX = pos.x;

        if (leader)
        {
            leaderReturnX = pos.x;

            // 여기가 다음 턴 발사 지점이라는 걸 그 자리에서 알려준다
            var landing = new Vector3(pos.x, BOTTOM_WALL + 0.05f, FIRE_Z);
            BrickBreakerFX.Instance?.Shockwave(landing, new Color(1f, 0.82f, 0.25f), 0.3f, 1.9f, 0.45f);
            BrickBreakerFX.Instance?.Popup(new Vector3(pos.x, BOTTOM_WALL + 1.2f, FIRE_Z),
                                           "\u25B2", new Color(1f, 0.82f, 0.25f), 2.6f);
        }
        ballsInFlight--;
        // launchComplete 확인 전에는 라운드를 끝내지 않는다.
        // 빠르게 돌아온 볼이 아직 안 쏜 볼까지 소진시켜 라운드를 조기 종료시킨다.
        if (launchComplete && ballsInFlight <= 0 && state == State.Firing)
            StartCoroutine(AfterFiring());
    }

    IEnumerator AfterFiring()
    {
        if (state != State.Firing) yield break;
        // 리더(첫 발)의 복귀 지점이 기준. 색·마킹·규칙이 같은 공을 가리켜야
        // 플레이어가 다음 위치를 예측할 수 있다. 리더가 없으면 첫 복귀로 폴백.
        float nextX = !float.IsNaN(leaderReturnX) ? leaderReturnX : firstReturnX;
        if (!float.IsNaN(nextX))
            fireX = Mathf.Clamp(nextX, LEFT_WALL + 0.5f, RIGHT_WALL - 0.5f);
        state = State.Advancing;


        yield return new WaitForSeconds(0.2f);
        if (state == State.GameOver) yield break;
        yield return AdvanceLayers();
    }

    bool turnCleared;

    /// <summary>
    /// 마지막 브릭이 깨진 **그 순간** 호출된다(예전엔 모든 공이 돌아온 뒤였다).
    /// 남은 공을 즉시 복귀시키고 연출을 크게 터뜨린다 — 다 부순 뒤 공이
    /// 터널을 한 바퀴 더 도는 걸 기다릴 이유가 없다.
    /// </summary>
    void OnAllCleared()
    {
        turnCleared = true;

        score += BrickBreakerRules.CLEAR_BONUS_SCORE;
        ballCount++;                       // 역전 장치

        // 날아다니는 공 전부 즉시 복귀
        foreach (var b in ballPool)
            if (b != null && b.gameObject.activeSelf) b.RushHome();

        StartCoroutine(AllClearShow());
        PunchScore();
        UpdateHUD();
    }

    IEnumerator AllClearShow()
    {
        var loc = LocalizationManager.Instance;
        var fx  = BrickBreakerFX.Instance;

        BrickBreakerAudio.Instance?.AllClear(this);

        Vector3 mid = new Vector3(0f, 0.5f, LAYER_START * 0.45f);
        fx?.Popup(mid, loc != null ? loc.GetOr("bb_clear", "ALL CLEAR!") : "ALL CLEAR!",
                  new Color(1f, 0.92f, 0.25f), 7.5f);

        // 터널 안쪽을 훑는 연쇄 폭발
        var cols = new[]
        {
            new Color(1f, 0.9f, 0.2f), new Color(0.3f, 1f, 0.6f),
            new Color(0.4f, 0.8f, 1f), new Color(1f, 0.5f, 0.9f),
        };
        for (int i = 0; i < 8; i++)
        {
            Vector3 p = new Vector3(
                Random.Range(LEFT_WALL + 1f, RIGHT_WALL - 1f),
                Random.Range(BOTTOM_WALL + 1f, TOP_WALL - 1f),
                Mathf.Lerp(1.5f, LAYER_START, i / 7f));
            fx?.Explode(p, cols[i % cols.Length], 1.6f);
            StartCoroutine(DoCameraShake(0.18f, 0.10f));
            yield return new WaitForSeconds(0.05f);
        }

        fx?.Popup(mid + Vector3.down * 1.8f, "+" + BrickBreakerRules.CLEAR_BONUS_SCORE,
                  new Color(1f, 1f, 0.5f), 4.5f);
    }

    bool NoBricksLeft()
    {
        foreach (var b in Bricks)
            if (b != null && !b.Dead && !b.IsBallItem) return false;
        return true;
    }

    // ── Brick events ─────────────────────────────────────
    public void OnBrickHit(bool destroyed) => OnBrickHit(destroyed, Vector3.zero, Color.white);

    public void OnBrickHit(bool destroyed, Vector3 pos, Color color)
    {
        if (!destroyed) return;

        // ── 콤보 ──────────────────────────────────────────
        combo++;
        comboTimer = COMBO_WINDOW;
        maxCombo   = Mathf.Max(maxCombo, combo);

        int gain = Mathf.Min(combo, COMBO_SCORE_CAP);
        score += gain;
        roundBricksDestroyed++;

        var fx = BrickBreakerFX.Instance;
        if (fx != null)
        {
            fx.Popup(pos, "+" + gain, ComboColor(combo), 3.0f + Mathf.Min(combo, 8) * 0.22f);
            if (combo >= 2) fx.Explode(pos, ComboColor(combo), 0.5f);
        }

        BrickBreakerAudio.Instance?.Combo(combo);
        ShowCombo();
        PunchScore();
        UpdateHUD();

        // 이번 파괴로 판이 비었나 — 공이 다 돌아오길 기다리지 않고 즉시 처리한다
        if (!turnCleared && roundBricksDestroyed > 0 && NoBricksLeft()) OnAllCleared();

        // 콤보가 쌓일수록 화면이 더 흔들린다
        float amount = Mathf.Clamp(combo * 0.035f, 0.05f, 0.42f);
        StartCoroutine(DoCameraShake(amount, 0.10f + Mathf.Min(combo, 6) * 0.012f));
    }

    public void OnItemCollected(BrickBreakerBrick brick)
    {
        var    loc = LocalizationManager.Instance;
        string msg;
        Color  col;

        switch (brick.Item)
        {
            case BrickBreakerBrick.ItemType.BallAdd:
            {
                int gain = BrickBreakerRules.ItemBallValue;
                ballCount += gain;
                msg = "+" + gain + " " + (loc != null ? loc.GetOr("hud_balls", "볼") : "볼");
                col = new Color(0.2f, 1f, 0.5f);
                break;
            }

            case BrickBreakerBrick.ItemType.DamageUp:
                BallDamage = Mathf.Min(BallDamage + BrickBreakerRules.BALL_DAMAGE_STEP,
                                       BrickBreakerRules.MAX_BALL_DAMAGE);
                msg = $"공격 x{BallDamage:0.00}";
                col = new Color(1f, 0.45f, 0.3f);
                break;

            case BrickBreakerBrick.ItemType.BallSize:
                BallRadius = Mathf.Min(BallRadius * BrickBreakerRules.BALL_SIZE_STEP,
                                       BrickBreakerRules.MAX_BALL_RADIUS);
                msg = "공 크기 UP";
                col = new Color(0.4f, 0.75f, 1f);
                break;

            case BrickBreakerBrick.ItemType.LuckUp:
                itemLuck++;
                msg = "행운 UP";
                col = new Color(0.9f, 0.55f, 1f);
                break;

            default:
                return;
        }

        BrickBreakerAudio.Instance?.Item();
        BrickBreakerFX.Instance?.Popup(brick.transform.position, msg, col, 3.4f);
        StartCoroutine(DoCameraShake(0.10f, 0.12f));
        UpdateHUD();
    }

    // ── 콤보 ─────────────────────────────────────────────
    const float COMBO_WINDOW    = 1.8f;  // 이 시간 안에 다시 부수면 콤보 유지
    const int   COMBO_SCORE_CAP = 8;     // 콤보당 획득 점수 상한

    int   combo;
    int   maxCombo;
    float comboTimer;

    static readonly Color[] ComboColors =
    {
        new Color(0.60f, 0.85f, 1.00f),  // 1-2
        new Color(0.30f, 1.00f, 0.55f),  // 3-4
        new Color(1.00f, 0.92f, 0.25f),  // 5-6
        new Color(1.00f, 0.58f, 0.15f),  // 7-9
        new Color(1.00f, 0.25f, 0.25f),  // 10+
    };

    static Color ComboColor(int c)
    {
        if (c <= 2) return ComboColors[0];
        if (c <= 4) return ComboColors[1];
        if (c <= 6) return ComboColors[2];
        if (c <= 9) return ComboColors[3];
        return ComboColors[4];
    }

    void UpdateComboTimer()
    {
        if (combo <= 0) return;
        comboTimer -= Time.deltaTime;
        if (comboTimer <= 0f) BreakCombo();
    }

    void BreakCombo()
    {
        combo      = 0;
        comboTimer = 0f;
        if (comboText) StartCoroutine(FadeOutCombo());
    }

    // ── 콤보 / 점수 UI ───────────────────────────────────
    TextMeshProUGUI comboText;
    Coroutine       comboAnim;
    Coroutine       scoreAnim;

    /// <summary>
    /// 콤보 표시는 이 게임에만 필요하므로 공용 프리팹이 아니라
    /// ContentArea 아래에 런타임으로 만든다.
    /// </summary>
    void CreateComboUI()
    {
        if (ui == null || ui.ContentArea == null) return;

        var go = new GameObject("ComboText", typeof(RectTransform));
        go.transform.SetParent(ui.ContentArea, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot     = new Vector2(0.5f, 1f);
        rt.anchoredPosition = new Vector2(0f, -70f);
        rt.sizeDelta        = new Vector2(700f, 120f);

        comboText = go.AddComponent<TextMeshProUGUI>();
        comboText.font          = Resources.Load<TMP_FontAsset>("TextMesh Pro/Fonts/ONE Mobile POP SDF");
        comboText.text          = "";
        comboText.fontSize      = 68f;
        comboText.alignment     = TextAlignmentOptions.Center;
        comboText.fontStyle     = FontStyles.Bold;
        comboText.raycastTarget = false;
        comboText.color         = new Color(1f, 1f, 1f, 0f);
    }

    void ShowCombo()
    {
        if (!comboText || combo < 2) return;   // 2콤보부터 표시
        comboText.text  = $"{combo} COMBO";
        comboText.color = ComboColor(combo);
        if (comboAnim != null) StopCoroutine(comboAnim);
        comboAnim = StartCoroutine(ComboPunch());
    }

    IEnumerator ComboPunch()
    {
        var rt = comboText.rectTransform;
        float t = 0f, dur = 0.28f;
        float peak = 1.35f + Mathf.Min(combo, 10) * 0.035f;
        while (t < dur)
        {
            t += Time.deltaTime;
            float k = t / dur;
            float s = k < 0.35f ? Mathf.Lerp(0.6f, peak, k / 0.35f)
                                : Mathf.Lerp(peak, 1f, Mathf.InverseLerp(0.35f, 1f, k));
            rt.localScale = Vector3.one * s;
            var c = comboText.color; c.a = 1f;
            comboText.color = c;
            yield return null;
        }
        rt.localScale = Vector3.one;
        comboAnim = null;
    }

    IEnumerator FadeOutCombo()
    {
        float t = 0f, dur = 0.35f;
        Color c0 = comboText.color;
        while (t < dur)
        {
            t += Time.deltaTime;
            var c = c0; c.a = Mathf.Lerp(c0.a, 0f, t / dur);
            comboText.color = c;
            yield return null;
        }
        comboText.text = "";
    }

    /// <summary>HUD 점수 텍스트를 튕겨준다.</summary>
    void PunchScore()
    {
        var st = ui != null ? ui.ScoreTextRT : null;
        if (st == null) return;
        if (scoreAnim != null) StopCoroutine(scoreAnim);
        scoreAnim = StartCoroutine(ScorePunch(st));
    }

    IEnumerator ScorePunch(RectTransform rt)
    {
        float t = 0f, dur = 0.22f;
        while (t < dur)
        {
            t += Time.deltaTime;
            float k = t / dur;
            rt.localScale = Vector3.one * (1f + Mathf.Sin(k * Mathf.PI) * 0.35f);
            yield return null;
        }
        rt.localScale = Vector3.one;
        scoreAnim = null;
    }

    // ── Layer management ─────────────────────────────────
    // 한 턴에 나오는 벽돌 수 (그리드를 채우지 않고 소수만 랜덤 배치)
    const int MIN_BRICKS = 2;
    const int MAX_BRICKS = 4;

    // BallAdd 아이템 주기. 확률로 두면 운 나쁠 때 여러 턴 굶으므로 확정 주기로 둔다.
    // 1턴부터 시작해 2턴마다 하나 (turn 1, 3, 5 …)
    const int ITEM_TURN_INTERVAL = 2;

    void SpawnLayer()
    {
        turn++;

        int hp = BrickBreakerRules.HpForTurn(turn);

        // 그리드 전 칸에서 필요한 개수만 골라 쓴다
        var positions = new List<(int col, int row)>(COLS * GRID_ROWS);
        for (int r = 0; r < GRID_ROWS; r++)
            for (int c = 0; c < COLS; c++)
                positions.Add((c, r));

        Shuffle(positions);
        int taken = 0;
        (int col, int row)? itemCell = null;

        // BallAdd 아이템 — 2턴마다 하나 보장
        if ((turn - 1) % BrickBreakerRules.ItemTurnInterval == 0 && taken < positions.Count)
        {
            var cell = positions[taken++];
            itemCell = cell;
            Bricks.Add(CreateBrick(cell.col, cell.row, LAYER_START, 0, BrickBreakerBrick.ItemType.BallAdd));
            BrickBreakerAudio.Instance?.ItemSpawn();
        }

        // 아이템 모드: 확률로 파워업 하나 더
        (int col, int row)? powerCell = null;
        if (Random.value < BrickBreakerRules.PowerUpChance(itemLuck) && taken < positions.Count)
        {
            var cell = positions[taken++];
            powerCell = cell;
            var kinds = new[] { BrickBreakerBrick.ItemType.DamageUp,
                                BrickBreakerBrick.ItemType.BallSize,
                                BrickBreakerBrick.ItemType.LuckUp };
            Bricks.Add(CreateBrick(cell.col, cell.row, LAYER_START, 0,
                                   kinds[Random.Range(0, kinds.Length)]));
            BrickBreakerAudio.Instance?.ItemSpawn();
        }

        SpawnClusters(hp, itemCell, powerCell);
        BrickBreakerAudio.Instance?.BrickSpawn();
    }

    /// <summary>
    /// 붙어 있는 덩어리로 놓는다. 공 하나가 여러 개를 연쇄로 때려야
    /// 콤보가 쌓이고, 그래야 콤보·점수·사운드가 의미를 갖는다.
    /// </summary>
    void SpawnClusters(int hp, (int col, int row)? itemCell, (int col, int row)? powerCell = null)
    {
        var used = new HashSet<(int, int)>();
        if (itemCell.HasValue)  used.Add((itemCell.Value.col,  itemCell.Value.row));
        if (powerCell.HasValue) used.Add((powerCell.Value.col, powerCell.Value.row));
        int want = BrickBreakerRules.ClusterCount(turn);

        for (int n = 0; n < want; n++)
        {
            var shape = BrickBreakerRules.Clusters[Random.Range(0, BrickBreakerRules.Clusters.Length)];

            int w = 0, h = 0;
            foreach (var o in shape) { w = Mathf.Max(w, o.x); h = Mathf.Max(h, o.y); }

            // 겹치지 않는 자리를 몇 번 시도해 본다. 못 찾으면 이번 덩어리는 건너뛴다.
            for (int attempt = 0; attempt < 12; attempt++)
            {
                int bc = Random.Range(0, COLS - w);
                int br = Random.Range(0, GRID_ROWS - h);

                bool free = true;
                foreach (var o in shape)
                    if (used.Contains((bc + o.x, br + o.y))) { free = false; break; }
                if (!free) continue;

                foreach (var o in shape)
                {
                    used.Add((bc + o.x, br + o.y));
                    float r = Random.value;
                    var form = r < BrickBreakerRules.SphereChance(turn) ? BrickBreakerBrick.Shape.Sphere
                             : r < BrickBreakerRules.SphereChance(turn) + BrickBreakerRules.TetraChance(turn)
                               ? BrickBreakerBrick.Shape.Tetra
                               : BrickBreakerBrick.Shape.Box;
                    Bricks.Add(CreateBrick(bc + o.x, br + o.y, LAYER_START, hp,
                                           BrickBreakerBrick.ItemType.None, form));
                }
                break;
            }
        }
    }

    IEnumerator AdvanceLayers()
    {
        BrickBreakerAudio.Instance?.TurnAdvance();

        foreach (var b in Bricks)
            if (b != null && !b.Dead) b.SetDanger(false);

        var alive = new List<BrickBreakerBrick>();
        foreach (var b in Bricks)
        {
            if (b == null || b.Dead) continue;
            float finalZ = b.MoveTowardPlayer(LAYER_STEP);

            if (finalZ <= GAME_OVER_Z)
            {
                // 보너스 아이템이 사망 원인이 되는 건 아케이드에선 맞지 않는다
                if (b.IsBallItem) { b.Collect(); continue; }   // 보너스가 사망 원인이 되면 안 된다
                EndGame();
                yield break;
            }
            alive.Add(b);
        }
        Bricks = alive;
        SpawnLayer();
        UpdateDangerBricks();
        UpdateHUD();

        // 전진 애니메이션이 끝나기 전에 조준을 열면 움직이는 브릭을 쏘게 된다
        yield return new WaitForSeconds(BrickBreakerBrick.AdvanceDuration);
        if (state == State.GameOver) yield break;

        state = State.Aiming;
        SetEdgeColor(EdgeColorReady);
    }

    void UpdateDangerBricks()
    {
        float threshold = GAME_OVER_Z + LAYER_STEP;
        bool  any = false;
        foreach (var b in Bricks)
            if (b != null && !b.Dead)
            {
                bool d = b.transform.position.z <= threshold;
                b.SetDanger(d);
                any |= d;
            }
        SetDangerPulse(any);
    }

    // ── Game Over ─────────────────────────────────────────
    void EndGame()
    {
        state = State.GameOver;
        BreakCombo();
        ui?.HideToast();
        // 조이스틱 캔버스는 sortingOrder 50이라 오버레이 위에 그려진다.
        // 통째로 내려 입력과 렌더링을 동시에 차단한다.
        if (leftJoystick  != null) leftJoystick.Interactable  = false;
        if (rightJoystick != null) rightJoystick.Interactable = false;
        if (leftJoystick  != null) leftJoystick.Root.SetActive(false);
        if (aimUI != null) { aimUI.Interactable = false; aimUI.SetVisible(false); }
        if (reticle) reticle.SetActive(false);

        int  best    = PlayerPrefs.GetInt(BestKey, 0);
        bool newBest = score > best;
        if (newBest) { PlayerPrefs.SetInt(BestKey, score); ui?.SetBest(score); SpawnNewBestFX(); }
        var audio = BrickBreakerAudio.Instance;
        if (audio != null) { if (newBest) audio.NewBest(); else audio.GameOver(); }

        var loc = LocalizationManager.Instance;
        string sub = newBest ? loc?.Get("overlay_newbest") ?? "New Best!" : null;
        if (maxCombo >= 2)
        {
            string comboLine = $"MAX {maxCombo} COMBO";
            sub = string.IsNullOrEmpty(sub) ? comboLine : sub + "\n" + comboLine;
        }

        // 로컬 저장소는 콜백이 동기라 여기서 sub에 바로 붙는다. 온라인 저장소로
        // 바꾸면 늦게 도착하므로, 그때는 오버레이를 띄운 뒤 갱신해야 한다.
        int myRank = 0;
        BrickBreakerRanking.Submit(score, turn, maxCombo, r => myRank = r);
        if (myRank > 0)
        {
            string rankLine = string.Format(
                loc != null ? loc.GetOr("bb_rank_line", "랭킹 {0}위") : "랭킹 {0}위", myRank);
            sub = string.IsNullOrEmpty(sub) ? rankLine : sub + "\n" + rankLine;
        }
        // 랭킹을 열면 이번 판 줄이 금색으로 강조된다
        if (rankUI != null) rankUI.PendingHighlight = myRank;

        string restartL = loc != null ? loc.GetOr("btn_restart", "다시 시작") : "다시 시작";
        string titleL    = loc != null ? loc.GetOr("btn_title",   "타이틀")    : "타이틀";

        var ads = BrickBreakerAds.Instance;
        // 로드 완료를 조건으로 걸지 않는다 — 광고가 늦게 실리면 버튼 자체가 안 떠서
        // 플레이어가 기능이 있는지조차 알 수 없었다. 누른 뒤에 받아온다.
        bool canContinue = ads != null && continuesUsed < MAX_CONTINUES;

        // 실기기에는 콘솔이 없어서 광고가 왜 안 나오는지 알 방법이 없다.
        // TEST_MODE(개발 빌드)일 때만 붙고, 출시 시 TEST_MODE를 끄면 같이 사라진다.
        if (BrickBreakerAds.TestMode && ads != null)
            sub = string.IsNullOrEmpty(sub) ? ads.Status() : sub + "\n" + ads.Status();

        if (canContinue)
        {
            // 이어하기가 가능하면 그게 가장 급한 선택지다. 모드 전환 버튼은
            // 다시 시작한 뒤에도 누를 수 있으니 이번만 자리를 내준다.
            ui?.ShowOverlay(
                new Color(.96f, .37f, .23f),
                loc?.Get("overlay_gameover") ?? "Game Over",
                score.ToString(), sub,
                loc != null ? loc.GetOr("bb_continue_ad", "광고 보고 이어하기") : "광고 보고 이어하기",
                WatchAdAndContinue,
                restartL, Restart,
                titleL,   () => SceneManager.LoadScene("TitleScene"));
            return;
        }

        ui?.ShowOverlay(
            new Color(.96f, .37f, .23f),
            loc?.Get("overlay_gameover") ?? "Game Over",
            score.ToString(),
            sub,
            restartL, Restart,
            titleL,   () => SceneManager.LoadScene("TitleScene"),
            // 진 직후가 다른 모드를 눌러보기 가장 자연스러운 순간이다
            BrickBreakerRules.NameOf(BrickBreakerRules.Other),
            () => { BrickBreakerRules.SetMode(BrickBreakerRules.Other); Restart(); });
    }

    void SpawnNewBestFX()
    {
        var go  = new GameObject("NewBestFX");
        go.transform.position = camBasePos + new Vector3(0f, 0f, 4f);
        var ps  = go.AddComponent<ParticleSystem>();
        var psr = go.GetComponent<ParticleSystemRenderer>();
        var mat = new Material(Shader.Find("Sprites/Default"));
        mat.color = Color.white;
        psr.material   = mat;
        psr.renderMode = ParticleSystemRenderMode.Billboard;
        var main = ps.main;
        main.startColor    = new ParticleSystem.MinMaxGradient(
            new Color(1f, .85f, 0.1f), new Color(1f, 0.35f, 0.1f));
        main.startSize     = new ParticleSystem.MinMaxCurve(0.2f, 0.65f);
        main.startSpeed    = new ParticleSystem.MinMaxCurve(4f, 10f);
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.7f, 1.6f);
        main.maxParticles  = 100;
        var emit = ps.emission;
        emit.SetBursts(new[] { new ParticleSystem.Burst(0f, 70) });
        emit.enabled = true;
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius    = 0.8f;
        ps.Play();
        Destroy(go, 3f);
    }

    // HUD 버튼용 public 래퍼 (다른 게임과 동일한 규약)
    public void OnNewGame() => Restart();
    public void OnBack()    => SceneManager.LoadScene("TitleScene");

    /// <summary>
    /// 리워드 광고를 보고 이어한다. 광고가 실패하거나 중간에 닫으면 아무 일도
    /// 일어나지 않고 게임오버 화면이 그대로 남는다 — 콜백은 어느 경로로든
    /// 반드시 한 번 오므로 화면이 멈추지는 않는다.
    /// </summary>
    void WatchAdAndContinue()
    {
        var ads = BrickBreakerAds.Instance;
        if (ads == null) return;

        ads.ShowRewarded(rewarded =>
        {
            if (!rewarded)
            {
                // 광고를 못 받았는지 중간에 닫았는지 플레이어는 구분할 수 없다.
                // 최소한 눌러도 아무 일 없는 상태로 두지는 않는다.
                var l = LocalizationManager.Instance;
                ui?.ShowToast(l != null ? l.GetOr("bb_ad_failed", "광고를 불러오지 못했습니다")
                                        : "광고를 불러오지 못했습니다");
                return;
            }
            continuesUsed++;
            ContinueGame();
        });
    }

    /// <summary>
    /// 점수·볼·파워업을 그대로 두고 판만 되살린다.
    ///
    /// 라인을 넘은 브릭만 지우면 바로 다음 턴에 또 죽는다 — 광고를 본 대가가
    /// 한 턴이면 보상이 아니다. 그래서 **위험 구간(GAME_OVER_Z + 한 칸) 전체**를
    /// 비워 숨 돌릴 여유를 준다.
    /// </summary>
    void ContinueGame()
    {
        float threshold = GAME_OVER_Z + LAYER_STEP;
        var alive = new List<BrickBreakerBrick>();
        foreach (var b in Bricks)
        {
            if (b == null || b.Dead) continue;
            if (b.transform.position.z <= threshold) { b.Collect(); continue; }
            alive.Add(b);
        }
        Bricks = alive;

        ui?.HideOverlay();

        // EndGame에서 내린 것들을 되돌린다.
        bool padMode = aimUI == null || aimUI.Mode == BrickBreakerAimUI.InputMode.Pad;
        if (leftJoystick != null)
        {
            leftJoystick.Root.SetActive(true);
            leftJoystick.SetVisible(padMode);
            leftJoystick.Interactable = true;
        }
        if (rightJoystick != null)
        {
            rightJoystick.SetVisible(padMode);
            rightJoystick.Interactable = true;
        }
        if (aimUI != null) { aimUI.Interactable = true; aimUI.SetVisible(true); }

        state = State.Aiming;
        SetEdgeColor(EdgeColorReady);
        UpdateDangerBricks();
        UpdateHUD();

        BrickBreakerAudio.Instance?.Item();
    }

    void Restart()
    {
        // 매판이 아니라 N번마다. 재시작이 느려지는 게 제일 짜증나는 광고다.
        int plays = PlayerPrefs.GetInt(PLAYS_KEY, 0) + 1;
        PlayerPrefs.SetInt(PLAYS_KEY, plays);
        PlayerPrefs.Save();

        var ads = BrickBreakerAds.Instance;
        if (ads != null && plays % INTERSTITIAL_EVERY == 0)
        {
            ads.ShowInterstitial(DoRestartNow);
            return;
        }
        DoRestartNow();
    }

    void DoRestartNow()
    {
        StopAllCoroutines();
        foreach (var b in ballPool) if (b != null) b.ForceStop();
        StartCoroutine(DoRestart());
    }

    IEnumerator DoRestart()
    {
        yield return SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().name);
    }

    void UpdateHUD()
    {
        if (!ui) return;

        ui.SetScore(score.ToString());

        // 볼 개수·파워업은 점수 줄에 붙이면 어색해서 전용 칩에 모아 보여준다
        aimUI?.SetStats(ballCount, BallDamage, BallRadius / BALL_BASE_RADIUS, itemLuck);
    }

    // ── Brick creation ───────────────────────────────────
    BrickBreakerBrick CreateBrick(int col, int row, float z, int hp, BrickBreakerBrick.ItemType item,
                                  BrickBreakerBrick.Shape form = BrickBreakerBrick.Shape.Box)
    {
        float x  = -((COLS - 1)      * COL_W * 0.5f) + col * COL_W;
        float y  = -((GRID_ROWS - 1) * ROW_H * 0.5f) + row * ROW_H;

        GameObject go;
        if (item != BrickBreakerBrick.ItemType.None)      go = MakeItemGO(item);
        else if (form == BrickBreakerBrick.Shape.Sphere)  go = MakeSphereBrickGO();
        else if (form == BrickBreakerBrick.Shape.Tetra)   go = MakeTetraBrickGO();
        else                                              go = MakeBrickGO();

        go.transform.position = new Vector3(x, y, z);

        // 기울이기 — 축 정렬이 깨지면 반사각을 눈으로 읽기 어려워져 판이 다시 어려워진다.
        // 회전은 루트에 준다(충돌은 브릭 로컬 공간에서 계산되므로 그대로 맞는다).
        if (item == BrickBreakerBrick.ItemType.None && form != BrickBreakerBrick.Shape.Sphere &&
            (form == BrickBreakerBrick.Shape.Tetra || Random.value < BrickBreakerRules.BoxTiltChance(turn)))
        {
            var rot = Random.rotationUniform;
            go.transform.rotation = rot;

            // 라벨은 세워둔다 — 브릭은 이후 이동만 하므로 한 번만 맞추면 유지된다
            var lbl = go.transform.Find("Label");
            if (lbl != null)
            {
                lbl.localRotation = Quaternion.Inverse(rot);
                lbl.localPosition = Quaternion.Inverse(rot) * new Vector3(0f, 0f, -1.0f);
            }
        }
        var brick = go.AddComponent<BrickBreakerBrick>();
        brick.Init(col, row, hp, item, form);

        // 계속 회전 — 구는 돌려봐야 표가 안 나므로 제외한다
        if (item == BrickBreakerBrick.ItemType.None &&
            form != BrickBreakerBrick.Shape.Sphere &&
            Random.value < BrickBreakerRules.SpinChance(turn))
        {
            brick.SetSpin(Random.onUnitSphere,
                          Random.Range(BrickBreakerRules.SPIN_MIN_DEG, BrickBreakerRules.SPIN_MAX_DEG)
                          * (Random.value < 0.5f ? -1f : 1f));
        }

        brick.PlaySpawnIn();
        return brick;
    }

    /// <summary>정사면체 브릭.</summary>
    GameObject MakeTetraBrickGO()
    {
        var go = BrickBreakerMeshes.Make("BrickTetra", BrickBreakerMeshes.Tetra, MakeMat(Color.white));
        AddBrickLabel(go, 3.2f, -0.55f);
        return go;
    }

    /// <summary>구형 브릭. 지름을 셀 크기에 맞춰 박스와 같은 자리를 차지한다.</summary>
    GameObject MakeSphereBrickGO()
    {
        var go = BrickBreakerMeshes.Make("BrickSphere", BrickBreakerMeshes.Sphere, MakeMat(Color.white));
        go.transform.localScale = Vector3.one * (BrickBreakerBrick.SphereR * 2f);
        AddBrickLabel(go, 4f, -0.55f);
        return go;
    }

    static TMP_FontAsset GetFont() =>
        Resources.Load<TMP_FontAsset>("TextMesh Pro/Fonts/ONE Mobile POP SDF");

    static Material MakeMat(Color c)
    {
        var mat = new Material(Shader.Find("Sprites/Default"));
        mat.color = c;
        return mat;
    }

    GameObject MakeBrickGO()
    {
        // 면 음영을 정점 색으로 구운 큐브. 언릿이라도 입체로 보인다.
        var go = BrickBreakerMeshes.Make("Brick", BrickBreakerMeshes.Cube, MakeMat(Color.white));
        go.transform.localScale = new Vector3(COL_W, ROW_H, COL_W);
        AddBrickLabel(go, 4f, -0.55f);
        return go;
    }

    /// <summary>HP 숫자 라벨. 모양별 GO가 공유한다.</summary>
    static void AddBrickLabel(GameObject go, float size, float localZ)
    {
        var lGo = new GameObject("Label");
        lGo.transform.SetParent(go.transform, false);
        lGo.transform.localPosition = new Vector3(0f, 0f, localZ);
        var tmp = lGo.AddComponent<TextMeshPro>();
        tmp.font      = GetFont();
        tmp.fontSize  = size;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color     = Color.black;
    }

    GameObject MakeItemGO(BrickBreakerBrick.ItemType item)
    {
        var go = BrickBreakerMeshes.Make(item.ToString(), BrickBreakerMeshes.Sphere, MakeMat(Color.white));
        go.transform.localScale = Vector3.one * (COL_W * 0.5f);

        var lGo = new GameObject("Label");
        lGo.transform.SetParent(go.transform, false);
        lGo.transform.localPosition = Vector3.zero;
        var tmp = lGo.AddComponent<TextMeshPro>();
        tmp.font      = GetFont();
        tmp.fontSize  = 3f;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color     = Color.black;
        return go;
    }

    BrickBreakerBall GetOrCreateBall()
    {
        foreach (var b in ballPool)
            if (b != null && !b.gameObject.activeSelf) return b;

        var go = BrickBreakerMeshes.Make("Ball", BrickBreakerMeshes.Sphere, MakeMat(Color.white));
        go.transform.localScale = Vector3.one * 0.55f;

        // 궤적 — 공이 터널 안에서 지나온 경로를 그려 z 이동을 읽게 한다.
        // 폭은 월드 단위지만 트랜스폼 스케일(0.55)이 곱해진다.
        var trail = go.AddComponent<TrailRenderer>();
        trail.time            = 0.13f;
        trail.startWidth      = 0.95f;
        trail.endWidth        = 0.05f;
        trail.material        = MakeMat(Color.white);
        trail.startColor      = new Color(1.00f, 1.00f, 1.00f, 0.50f);
        trail.endColor        = new Color(0.35f, 0.65f, 1.00f, 0.00f);
        trail.numCapVertices  = 4;
        trail.alignment       = LineAlignment.View;
        trail.autodestruct    = false;
        trail.Clear();

        var ball = go.AddComponent<BrickBreakerBall>();
        ball.OnReturned += OnBallReturned;
        ballPool.Add(ball);
        return ball;
    }

    // ── Tunnel wire-frame ─────────────────────────────────
    void DrawTunnel()
    {
        float z0    = TUNNEL_Z0;
        float z1    = BACK_WALL;
        Color cEdge  = new Color(0.25f, 0.50f, 1.00f, 0.30f);
        // 입구 테두리 — 안과 밖을 가르는 유일한 선이라 확실히 밝게
        Color cFront = new Color(0.55f, 0.85f, 1.00f, 0.95f);
        Color cBack  = new Color(0.20f, 0.40f, 1.00f, 0.18f);
        Color cGrid  = new Color(0.20f, 0.35f, 0.80f, 0.08f);

        MakeLine(new Vector3(LEFT_WALL,  BOTTOM_WALL, z0), new Vector3(LEFT_WALL,  BOTTOM_WALL, z1), cEdge);
        MakeLine(new Vector3(RIGHT_WALL, BOTTOM_WALL, z0), new Vector3(RIGHT_WALL, BOTTOM_WALL, z1), cEdge);
        MakeLine(new Vector3(LEFT_WALL,  TOP_WALL,    z0), new Vector3(LEFT_WALL,  TOP_WALL,    z1), cEdge);
        MakeLine(new Vector3(RIGHT_WALL, TOP_WALL,    z0), new Vector3(RIGHT_WALL, TOP_WALL,    z1), cEdge);

        // 입구 프레임은 두껍게 — 여기가 '터널 안'의 시작이라는 신호
        MakeLine(new Vector3(LEFT_WALL,  BOTTOM_WALL, 0f), new Vector3(RIGHT_WALL, BOTTOM_WALL, 0f), cFront, 0.11f);
        MakeLine(new Vector3(LEFT_WALL,  TOP_WALL,    0f), new Vector3(RIGHT_WALL, TOP_WALL,    0f), cFront, 0.11f);
        MakeLine(new Vector3(LEFT_WALL,  BOTTOM_WALL, 0f), new Vector3(LEFT_WALL,  TOP_WALL,    0f), cFront, 0.11f);
        MakeLine(new Vector3(RIGHT_WALL, BOTTOM_WALL, 0f), new Vector3(RIGHT_WALL, TOP_WALL,    0f), cFront, 0.11f);

        MakeLine(new Vector3(LEFT_WALL,  BOTTOM_WALL, z1), new Vector3(RIGHT_WALL, BOTTOM_WALL, z1), cBack);
        MakeLine(new Vector3(LEFT_WALL,  TOP_WALL,    z1), new Vector3(RIGHT_WALL, TOP_WALL,    z1), cBack);
        MakeLine(new Vector3(LEFT_WALL,  BOTTOM_WALL, z1), new Vector3(LEFT_WALL,  TOP_WALL,    z1), cBack);
        MakeLine(new Vector3(RIGHT_WALL, BOTTOM_WALL, z1), new Vector3(RIGHT_WALL, TOP_WALL,    z1), cBack);

        // 링을 LAYER_STEP 배수(=브릭 중심 z)에 그리면 선이 박스를 반으로 가른다.
        // 반 칸 밀어 브릭 앞뒤 면에 맞추면 브릭 하나가 링 사이 한 칸에 딱 들어간다.
        const float RING_OFFSET = 0.5f * LAYER_STEP;
        for (float gz = RING_OFFSET; gz <= LAYER_START + RING_OFFSET + 0.01f; gz += LAYER_STEP)
        {
            MakeLine(new Vector3(LEFT_WALL,  BOTTOM_WALL, gz), new Vector3(RIGHT_WALL, BOTTOM_WALL, gz), cGrid);
            MakeLine(new Vector3(LEFT_WALL,  TOP_WALL,    gz), new Vector3(RIGHT_WALL, TOP_WALL,    gz), cGrid);
            MakeLine(new Vector3(LEFT_WALL,  BOTTOM_WALL, gz), new Vector3(LEFT_WALL,  TOP_WALL,    gz), cGrid);
            MakeLine(new Vector3(RIGHT_WALL, BOTTOM_WALL, gz), new Vector3(RIGHT_WALL, TOP_WALL,    gz), cGrid);
        }
    }

    static void MakeLine(Vector3 a, Vector3 b, Color color, float width = 0.04f)
    {
        var go = new GameObject("TunnelLine");
        var lr = go.AddComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.SetPosition(0, a); lr.SetPosition(1, b);
        lr.startWidth = lr.endWidth = width;
        lr.material   = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = lr.endColor = color;
        lr.useWorldSpace = true;
    }

    static void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
