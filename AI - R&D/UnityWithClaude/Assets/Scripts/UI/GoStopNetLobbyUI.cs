using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// 타이틀 → 고스톱 인원수 선택 팝업(<see cref="GoStopModeChoiceUI"/>)의
/// "네트워크 대전" 항목에서 열리는 로비 화면. 같은 와이파이 안에서
/// 호스트가 방을 만들고(자동 UDP 광고) 게스트가 자동으로 찾아 들어오는
/// 구조 — IP를 직접 입력하지 않는다(사용자 확인 요청). 실제 턴 진행
/// 로직은 전혀 안 건드리고, <see cref="GoStopNetLobby"/> API만 감싼
/// 순수 화면이다.
///
/// 2인(맞고)도 네트워크 대전이 지원되어 2명부터 시작할 수 있다.
///
/// 2026-08-28: 정적인 틀(딤·카드·제목/부제·본문 컨테이너·닫기 버튼)만
/// <c>Assets/Resources/Prefabs/GoStop/UI/GoStopNetLobbyUI.prefab</c>으로
/// 뺐다 — 화면마다(Home/Hosting/Scanning/Connecting/Waiting/Error) 내용
/// 자체의 "구조"가 달라지는(방 목록 개수, 좌석 4개 등) <see cref="body"/>
/// 안쪽은 매 판 카드가 달라지는 GoStop 보드와 같은 이유로 여전히 코드가
/// 그린다(이 프로젝트가 이미 채택한 "정적 틀=프리팹, 가변 콘텐츠=코드"
/// 경계). 닫기 버튼 onClick만 베이킹 시점에 <c>Close()</c>로 persistent
/// 연결해뒀다.
/// </summary>
public class GoStopNetLobbyUI : MonoBehaviour
{
    enum Screen { NicknameSetup, Home, Hosting, Scanning, Connecting, Waiting, Error }
    Screen screen = Screen.Home;
    string errorMsg = "";
    string myName = "";
    GoStopRoomScanner.DiscoveredRoom connectingRoom;
    TMP_InputField pendingNicknameField;

    [SerializeField] RectTransform panelRT;
    [SerializeField] RectTransform body;
    [SerializeField] RectTransform closeBtnRT;
    [SerializeField] TextMeshProUGUI subtitleLbl;

    static readonly Color Surface  = new Color(0.137f, 0.169f, 0.322f, 1f); // #232B52
    static readonly Color Surface2 = new Color(0.169f, 0.208f, 0.376f, 1f); // #2B3560
    static readonly Color Accent   = new Color(0.929f, 0.729f, 0.180f, 1f); // #EDBA2E
    static readonly Color T95 = new Color(1f, 1f, 1f, 0.95f);
    static readonly Color T70 = new Color(1f, 1f, 1f, 0.70f);
    static readonly Color T40 = new Color(1f, 1f, 1f, 0.40f);

    const int MIN_NETWORK_PLAYERS = 2; // GoStopNetLobby.HostStartGame과 동일 — 2인부터 시작 가능
    const float BODY_W = 700f; // 프리팹의 Body 폭(사용자 확인 값)과 반드시 맞출 것 — 아래 각 화면의 sizeDelta 계산이 이 값을 그대로 쓴다

    float cursor; // body 로컬 커서 — 0=맨 위, 내려갈수록(화면 아래쪽) 더 음수

    void ResetCursor() => cursor = -20f; // 본문 맨 위 여백

    /// <summary>다음 요소의 중심 Y를 돌려주고 커서를 그만큼 내린다.
    /// <paramref name="gapBefore"/>는 "바로 앞 요소와의 간격"이다.</summary>
    float NextY(float height, float gapBefore)
    {
        cursor -= gapBefore;
        float center = cursor - height * 0.5f;
        cursor -= height;
        return center;
    }

    public static GoStopNetLobbyUI Create(RectTransform canvasRT)
    {
        var ui = HwatuUI.InstantiateUIPrefab<GoStopNetLobbyUI>("GoStopNetLobbyUI", canvasRT);

        // 로비 이벤트는 이 오브젝트가 살아있는 내내(비활성 상태에서도) 구독해
        // 둔다 — 팝업이 닫혀 있어도 게임 시작 신호(OnGameStarting)는 놓치면
        // 안 된다. Redraw는 활성 상태일 때만 의미가 있으므로 각 핸들러 안에서
        // activeSelf를 확인한다.
        var lobby = EnsureLobby();
        lobby.OnLobbyChanged += ui.HandleLobbyChanged;
        lobby.OnGameStarting += ui.HandleGameStarting;
        lobby.OnDisconnected += ui.HandleDisconnected;
        return ui;
    }

    /// <summary>씬 어디에도 <see cref="GoStopNetLobby"/>를 미리 심어두지
    /// 않았다 — <c>DontDestroyOnLoad</c> 싱글톤이라 처음 필요해지는 시점에
    /// (이 로비 UI가 만들어질 때) 한 번만 생성해두면 씬 전환을 넘어 계속
    /// 산다. 이미 있으면(예: 이전에 네트워크 판을 한 번 했다가 타이틀로
    /// 돌아온 경우) 그대로 재사용한다.</summary>
    static GoStopNetLobby EnsureLobby()
    {
        if (GoStopNetLobby.Instance == null)
            new GameObject("GoStopNetLobby").AddComponent<GoStopNetLobby>();
        return GoStopNetLobby.Instance;
    }

    void OnDestroy()
    {
        var lobby = GoStopNetLobby.Instance;
        if (lobby != null)
        {
            lobby.OnLobbyChanged -= HandleLobbyChanged;
            lobby.OnGameStarting -= HandleGameStarting;
            lobby.OnDisconnected -= HandleDisconnected;
        }
    }

    public void Open()
    {
        // 방금 전 게임(네트워크 판)에서 타이틀로 돌아온 직후일 수 있으므로
        // 항상 깨끗한 상태에서 시작한다 — GoToTitle()이 이미 StopAll을
        // 부르지만, 이 화면에 직접 진입하는 다른 경로가 생겨도 안전하도록
        // 방어적으로 한 번 더 정리한다.
        GoStopNetLobby.Instance?.StopAll();
        errorMsg = "";
        // 2026-09-05(사용자 확인) — 네트워크를 맨 처음 쓸 때만 닉네임을
        // 정하게 한다. 한 번 정하면 기기에 영구 저장돼(같은 닉네임에
        // 이어지는 보유머니 — GoStopNetLobby.LoadNetworkMoney 참고) 다음
        // 접속부터는 이 화면을 건너뛰고 곧장 Home으로 간다.
        string saved = PlayerPrefs.GetString(GoStopNetLobby.NicknameKey, "");
        if (string.IsNullOrEmpty(saved))
        {
            myName = GenerateName(); // 입력창의 기본 추천값일 뿐, 확정은 사용자가 눌러야 된다
            screen = Screen.NicknameSetup;
        }
        else
        {
            myName = saved;
            screen = Screen.Home;
        }
        panelRT.gameObject.SetActive(true);
        panelRT.SetAsLastSibling();
        Redraw();
    }

    // 프리팹의 닫기 버튼 onClick에 persistent listener로 이미 연결돼 있다
    // (베이킹 시점) — public이어야 그 연결이 성립한다.
    public void Close()
    {
        // 대기실/스캔 중이었으면 세션을 접고 나간다 — "닫기"를 눌렀는데
        // 방이 계속 열려 있거나 TCP가 연결된 채로 남으면 다음에 이 화면을
        // 다시 열었을 때 옛 세션과 뒤엉킨다.
        if (screen == Screen.Hosting || screen == Screen.Scanning || screen == Screen.Connecting || screen == Screen.Waiting)
            GoStopNetLobby.Instance?.StopAll();
        panelRT.gameObject.SetActive(false);
    }

    static string GenerateName()
    {
        string baseName = string.IsNullOrEmpty(SystemInfo.deviceName) || SystemInfo.deviceName == "<unknown>"
            ? "Player" : SystemInfo.deviceName;
        return baseName + "#" + Random.Range(1000, 9999);
    }

    // ── 로비 이벤트 ───────────────────────────────────────
    void HandleLobbyChanged()
    {
        if (!panelRT.gameObject.activeSelf) return;
        // 게스트가 접속을 시도한 뒤 처음 받는 LobbyUpdate가 곧 "연결됐다"는
        // 신호다(Connect 자체는 TCP 핸드셰이크일 뿐이고, 로비 인원 정보가
        // 와야 실제로 방에 들어간 것이다) — Connecting에서 Waiting으로
        // 화면을 넘긴다.
        if (screen == Screen.Connecting) screen = Screen.Waiting;
        if (screen == Screen.Hosting || screen == Screen.Waiting) Redraw();
    }

    void HandleGameStarting(int seat, int total)
    {
        // GoStop3PGame이 SEATS=2(맞고)~4 전부 처리하므로 인원수와 무관하게
        // 항상 같은 씬을 연다 — GoStop3PGame.Awake()가 lobby.PlayerCount를
        // 읽어 SetSeatCount로 알아서 모드를 맞춘다.
        SceneManager.LoadScene("GoStop3PScene");
    }

    void HandleDisconnected(string reason)
    {
        if (!panelRT.gameObject.activeSelf) return;
        errorMsg = reason;
        screen = Screen.Error;
        Redraw();
    }

    // ── 화면 전환 ─────────────────────────────────────────
    void Redraw()
    {
        HwatuUI.ClearChildren(body);
        ResetCursor();
        // 2026-08-20: 호스트 대기실은 이미 "방 닫기" 전용 버튼이 있어서
        // 카드 상단의 범용 "닫기" X가 중복이었다(사용자 확인) — 이 화면일
        // 때만 숨긴다. 다른 화면(뒤로/취소/확인 버튼이 있는 화면 포함)은
        // 아직 요청받지 않아 그대로 둔다.
        closeBtnRT.gameObject.SetActive(screen != Screen.Hosting);
        switch (screen)
        {
            case Screen.NicknameSetup: subtitleLbl.text = "다른 유저에게 이 이름으로 보입니다"; ShowNicknameSetup(); break;
            case Screen.Home: subtitleLbl.text = "같은 와이파이 안에서 방을 만들거나 찾습니다"; ShowHome(); break;
            case Screen.Hosting: subtitleLbl.text = $"방 열림 · {myName}"; ShowRoster(true); break;
            case Screen.Scanning: subtitleLbl.text = "주변 방을 찾는 중..."; ShowScanning(); break;
            case Screen.Connecting: subtitleLbl.text = $"{connectingRoom?.hostName} 방에 접속 중..."; ShowConnecting(); break;
            case Screen.Waiting: subtitleLbl.text = "호스트가 시작하기를 기다리는 중"; ShowRoster(false); break;
            case Screen.Error: subtitleLbl.text = "연결 실패"; ShowError(); break;
        }
    }

    const int NicknameMaxLen = 12;

    void ShowNicknameSetup()
    {
        var hint = AddLabel(body, "닉네임을 정해주세요 — 방에 같이 있는\n다른 플레이어에게 이 이름으로 보입니다", 18f, T70);
        hint.rectTransform.sizeDelta = new Vector2(BODY_W, 70f);
        hint.rectTransform.anchoredPosition = new Vector2(0f, 92f);

        pendingNicknameField = MakeInputField(body, -10f, "닉네임", myName);

        MakeBigButton(body, -120f, "확인", "보유머니는 이 닉네임 하나로 계속 이어집니다", OnNicknameConfirmClicked, height: 90f);
    }

    void OnNicknameConfirmClicked()
    {
        string name = pendingNicknameField != null ? pendingNicknameField.text.Trim() : "";
        if (string.IsNullOrEmpty(name)) name = GenerateName(); // 빈 입력은 그냥 자동 추천값으로 확정한다(재입력을 강요하지 않는다)
        if (name.Length > NicknameMaxLen) name = name.Substring(0, NicknameMaxLen);
        myName = name;
        PlayerPrefs.SetString(GoStopNetLobby.NicknameKey, myName);
        PlayerPrefs.Save();
        pendingNicknameField = null;
        screen = Screen.Home;
        Redraw();
    }

    /// <summary>이 프로젝트에서 두 번째로 만드는 TMP_InputField(첫 번째는
    /// GoStopChatView.prefab에 에디터에서 미리 구워둔 것) — 이번엔 화면
    /// 자체가 매 Redraw()마다 코드로 새로 그려지는 구조라 프리팹으로 뺄 수
    /// 없어서 런타임에 직접 조립한다. Viewport(RectMask2D)+Placeholder+
    /// Text 세 조각을 TMP_InputField가 요구하는 최소 구성 그대로 만든다.</summary>
    TMP_InputField MakeInputField(Transform parent, float y, string placeholder, string initialText)
    {
        var root = MakeRect("Input", parent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        root.sizeDelta = new Vector2(560f, 80f);
        root.anchoredPosition = new Vector2(0f, y);
        var bg = AddImg(root, UISkin.Input, Color.white, true);
        bg.raycastTarget = true; // AddImg 기본 false — 입력창 클릭/캐럿 배치엔 반드시 켜야 한다(이 프로젝트 공통 함정)

        var viewport = MakeRect("TextArea", root, Vector2.zero, Vector2.one);
        viewport.offsetMin = new Vector2(24f, 8f);
        viewport.offsetMax = new Vector2(-24f, -8f);
        viewport.gameObject.AddComponent<RectMask2D>();

        var placeholderTmp = MakeFieldText(viewport, placeholder, new Color(0f, 0f, 0f, 0.35f));
        placeholderTmp.fontStyle = FontStyles.Italic;
        var textTmp = MakeFieldText(viewport, "", new Color(0.05f, 0.05f, 0.08f, 1f));

        var field = root.gameObject.AddComponent<TMP_InputField>();
        field.targetGraphic = bg;
        field.textViewport = viewport;
        field.textComponent = textTmp;
        field.placeholder = placeholderTmp;
        field.lineType = TMP_InputField.LineType.SingleLine;
        field.characterLimit = NicknameMaxLen;
        field.text = initialText ?? "";
        return field;
    }

    static TextMeshProUGUI MakeFieldText(Transform parent, string text, Color col)
    {
        var go = new GameObject("Text", typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.font = Resources.Load<TMP_FontAsset>("TextMesh Pro/Fonts/ONE Mobile POP SDF");
        tmp.text = text; tmp.fontSize = 26f; tmp.color = col;
        tmp.alignment = TextAlignmentOptions.MidlineLeft;
        tmp.textWrappingMode = TextWrappingModes.NoWrap;
        tmp.raycastTarget = false;
        return tmp;
    }

    void ShowHome()
    {
        // 사용자 확인 값.
        MakeBigButton(body, 92f, "방 만들기", "내가 호스트가 되어 방을 엽니다", OnHostClicked);
        MakeBigButton(body, -48f, "방 찾기", "주변에 열린 방을 자동으로 찾습니다", OnJoinClicked);
        var hint = AddLabel(body, "2명이 모이면 맞고, 3명 이상이면 고스톱으로 시작합니다", 16f, T40);
        hint.rectTransform.sizeDelta = new Vector2(BODY_W, 60f);
        hint.rectTransform.anchoredPosition = new Vector2(0f, -176f);

        // 2026-08-23(design.md §49.2): 1점 가격은 방 만들기 전(Home)에
        // 정해야 한다 — 게임 진행 중에는 안 바뀐다. hint(-176, 높이60,
        // 하단 -206) 바로 아래, body 하단(-BODY_H=-300)까지 남는 94px
        // 안에 들어가야 해서(이 화면은 CARD_H/BODY_H를 다른 화면
        // (Hosting/Waiting 로스터)에 맞춰 역산해둔 걸 그대로 쓰므로 Home
        // 자신에게 남는 여유만큼만 써야 한다) 새 줄 하나(56px)로 압축했다.
        MakePointPriceStepper(body, -248f);
    }

    /// <summary>1점 가격 선택 — 임의 숫자 입력 대신 프리셋 사이를 +/-로
    /// 오가는 스텝퍼다(GoStopNetLobby.PointPriceSteps). 이 프로젝트에
    /// TMP_InputField를 쓴 전례가 없어 새로 들여오는 리스크를 피했고,
    /// 스텝퍼는 애초에 [10원, 10만원] 범위를 벗어날 수 없어 별도
    /// 유효성 검증 UI(실패 메시지 등)도 필요 없다.</summary>
    void MakePointPriceStepper(Transform parent, float y)
    {
        var row = MakeRect("PointPrice", parent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        row.sizeDelta = new Vector2(660f, 56f);
        row.anchoredPosition = new Vector2(0f, y);

        var titleLbl2 = AddLabel(row, "1점 가격", 18f, T70);
        titleLbl2.rectTransform.sizeDelta = new Vector2(180f, 56f);
        titleLbl2.rectTransform.anchoredPosition = new Vector2(-230f, 0f);

        MakeStepBtn(row, 60f, "−", () => { GoStopNetLobby.Instance.StepPointPrice(-1); Redraw(); });
        var valueLbl = AddLabel(row, FormatPointPrice(GoStopNetLobby.Instance.PointPrice), 20f, T95);
        valueLbl.rectTransform.sizeDelta = new Vector2(140f, 56f);
        valueLbl.rectTransform.anchoredPosition = new Vector2(160f, 0f);
        MakeStepBtn(row, 260f, "+", () => { GoStopNetLobby.Instance.StepPointPrice(1); Redraw(); });
    }

    static string FormatPointPrice(int price) => price.ToString("N0") + "원";

    RectTransform MakeStepBtn(Transform parent, float x, string label, UnityEngine.Events.UnityAction onClick)
    {
        var btn = MakeRect("Step", parent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        btn.sizeDelta = new Vector2(56f, 56f);
        btn.anchoredPosition = new Vector2(x, 0f);
        var img = AddImg(btn, UISkin.Button, Surface2, true);
        img.raycastTarget = true; // AddImg 기본 false — 버튼 직접 클릭엔 반드시 켜야 한다(이 프로젝트 공통 함정)
        var lbl = AddLabel(btn, label, 30f, T95);
        lbl.fontStyle = FontStyles.Bold;
        btn.gameObject.AddComponent<Button>().onClick.AddListener(onClick);
        return btn;
    }

    void OnHostClicked()
    {
        GoStopNetLobby.Instance.HostRoom(myName);
        screen = Screen.Hosting;
        Redraw();
    }

    void OnJoinClicked()
    {
        GoStopNetLobby.Instance.StartScanningForRooms();
        screen = Screen.Scanning;
        Redraw();
    }

    void ShowScanning()
    {
        var rooms = GoStopNetLobby.Instance.DiscoveredRooms.OrderBy(r => r.hostName).ToList();
        if (rooms.Count == 0)
        {
            var lbl = AddLabel(body, "아직 찾은 방이 없습니다...\n같은 와이파이에서 호스트가 방을 열면\n자동으로 나타납니다", 18f, T70);
            lbl.rectTransform.sizeDelta = new Vector2(BODY_W, 100f);
            lbl.rectTransform.anchoredPosition = new Vector2(0f, NextY(100f, 60f));
        }
        else
        {
            // 발견되는 방이 많아도 카드가 넘치지 않게 최대 4개까지만
            // 보여준다(그 이상은 이 기능 규모(같은 와이파이 소수 인원
            // LAN 파티)에서 실질적으로 안 나오는 상황이라 스크롤까지는
            // 안 만들었다).
            foreach (var room in rooms.Take(4))
            {
                var r = room; // 클로저 캡처 — foreach 변수를 그대로 캡처하면 마지막 값만 남는다
                MakeRoomRow(body, NextY(84f, 12f), r, () => OnRoomTapped(r));
            }
        }

        MakeSmallButton(body, NextY(72f, 28f), "뒤로", OnScanBackClicked);
    }

    void OnRoomTapped(GoStopRoomScanner.DiscoveredRoom room)
    {
        connectingRoom = room;
        GoStopNetLobby.Instance.JoinRoom(room, myName);
        screen = Screen.Connecting;
        Redraw();
    }

    void OnScanBackClicked()
    {
        GoStopNetLobby.Instance.StopAll();
        screen = Screen.Home;
        Redraw();
    }

    void ShowConnecting()
    {
        AddLabel(body, "연결 중입니다...", 22f, T70).rectTransform.anchoredPosition = new Vector2(0f, NextY(50f, 60f));
        MakeSmallButton(body, NextY(72f, 40f), "취소", OnScanBackClicked);
    }

    /// <summary>호스트("Hosting")·게스트("Waiting") 대기실이 거의 같은
    /// 모양이라(좌석 4개 나열) 하나로 합쳤다 — 다른 건 "시작" 버튼 유무뿐.
    /// 이 화면이 4개 화면 중 세로로 가장 깊다(로우 4개+힌트+버튼 최대 2개) —
    /// 프리팹의 Body 높이는 이 화면 기준으로 역산돼 있다.</summary>
    void ShowRoster(bool isHostView)
    {
        var lobby = GoStopNetLobby.Instance;
        var names = lobby.PlayerNames;
        int total = names.Count(n => !string.IsNullOrEmpty(n) && n != "(입장 중...)");

        // 사용자 확인 값 — 안내 라벨(168)이 좌석 4개(117/51/-15/-81)보다
        // 위에 온다.
        string modeHint = total switch
        {
            2 => "2명 · 맞고",
            3 => "3명 · 고스톱 (3인)",
            4 => "4명 · 고스톱 (4인, 광팔이)",
            _ => $"{total}명 · {MIN_NETWORK_PLAYERS}명 이상 모이면 시작할 수 있어요",
        };
        // 2026-08-23: Home에서 정한 1점 가격을 대기실에서도 계속 보여준다 —
        // 호스트 자신도 방금 뭘로 정했는지 잊기 쉽고, 게스트는 이 화면
        // 전까지는 알 방법이 없다(그냥 텍스트만 덧붙이는 거라 레이아웃
        // 위험이 없다).
        modeHint += $" · 1점={FormatPointPrice(GoStopNetLobby.Instance.PointPrice)}";
        AddLabel(body, modeHint, 18f, T70).rectTransform.anchoredPosition = new Vector2(0f, 168f);

        float[] seatY = { 117f, 51f, -15f, -81f };
        for (int s = 0; s < 4; s++)
        {
            string label = string.IsNullOrEmpty(names[s]) ? "빈 자리" : names[s];
            bool filled = !string.IsNullOrEmpty(names[s]);
            MakeSeatRow(body, seatY[s], s, label, filled);
        }

        if (isHostView)
        {
            bool canStart = total >= MIN_NETWORK_PLAYERS;
            var startBtn = MakeBigButton(body, -186f, canStart ? "시작" : $"시작 ({MIN_NETWORK_PLAYERS}명부터)",
                canStart ? "모든 참가자에게 게임 화면을 띄웁니다" : $"{MIN_NETWORK_PLAYERS}명 이상 모여야 시작할 수 있습니다",
                canStart ? OnStartClicked : (UnityEngine.Events.UnityAction)null, height: 90f);
            if (!canStart)
            {
                var btn = startBtn.GetComponent<Button>();
                if (btn) btn.interactable = false;
            }
            MakeSmallButton(body, -253f, "방 닫기", OnCancelHostClicked);
        }
        else
        {
            MakeSmallButton(body, -253f, "나가기", OnLeaveWaitingClicked);
        }
    }

    void OnStartClicked() => GoStopNetLobby.Instance.HostStartGame();

    void OnCancelHostClicked()
    {
        GoStopNetLobby.Instance.StopAll();
        screen = Screen.Home;
        Redraw();
    }

    void OnLeaveWaitingClicked()
    {
        GoStopNetLobby.Instance.StopAll();
        screen = Screen.Home;
        Redraw();
    }

    void ShowError()
    {
        var lbl = AddLabel(body, string.IsNullOrEmpty(errorMsg) ? "연결에 실패했습니다." : errorMsg, 20f, T70);
        lbl.rectTransform.sizeDelta = new Vector2(BODY_W, 100f);
        lbl.rectTransform.anchoredPosition = new Vector2(0f, NextY(100f, 60f));
        MakeSmallButton(body, NextY(72f, 40f), "확인", () => { screen = Screen.Home; Redraw(); });
    }

    // ── 로우/버튼 헬퍼 ────────────────────────────────────
    // 전부 (0, y) 형태의 중심좌표 하나만 받는다 — 가로는 항상 카드
    // 중앙(x=0) 고정이라 Vector2를 통째로 넘길 이유가 없어 y만 받게
    // 정리했다(NextY()가 y만 돌려주므로 호출부에서 Vector2로 감싸는
    // 실수를 원천 차단).
    RectTransform MakeBigButton(Transform parent, float y, string title, string sub, UnityEngine.Events.UnityAction onClick, float height = 116f)
    {
        var btn = MakeRect("Choice", parent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        btn.sizeDelta = new Vector2(660f, height);
        btn.anchoredPosition = new Vector2(0f, y);
        var btnImg = AddImg(btn, UISkin.ButtonLine, Surface2, true);
        btnImg.raycastTarget = true; // AddImg 기본 false — 버튼 직접 클릭엔 반드시 켜야 한다(이 프로젝트 공통 함정)

        var titleLbl2 = AddLabel(btn, title, 26f, T95);
        titleLbl2.rectTransform.anchoredPosition = new Vector2(0f, 20f);
        var subLbl = AddLabel(btn, sub, 15f, T70);
        subLbl.rectTransform.anchoredPosition = new Vector2(0f, -22f);
        subLbl.rectTransform.sizeDelta = new Vector2(600f, 40f);

        var button = btn.gameObject.AddComponent<Button>();
        if (onClick != null) button.onClick.AddListener(onClick);
        return btn;
    }

    void MakeSmallButton(Transform parent, float y, string label, UnityEngine.Events.UnityAction onClick)
    {
        var btn = MakeRect("Small", parent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        btn.sizeDelta = new Vector2(300f, 72f);
        btn.anchoredPosition = new Vector2(0f, y);
        var img = AddImg(btn, UISkin.Button, Color.white, true);
        img.raycastTarget = true;
        AddLabel(btn, label, 22f, T95);
        btn.gameObject.AddComponent<Button>().onClick.AddListener(onClick);
    }

    void MakeSeatRow(Transform parent, float y, int seat, string label, bool filled)
    {
        var row = MakeRect("Seat", parent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        row.sizeDelta = new Vector2(660f, 70f); // 사용자 확인 값
        row.anchoredPosition = new Vector2(0f, y);
        var img = AddImg(row, UISkin.PanelLine, filled ? Surface2 : Surface, true);
        img.color = filled ? Surface2 : new Color(Surface.r, Surface.g, Surface.b, 0.5f);

        string seatTag = seat == 0 ? "호스트" : $"게스트{seat}";
        AddLabel(row, seatTag, 16f, filled ? T70 : T40).rectTransform.anchoredPosition = new Vector2(-220f, 0f);
        AddLabel(row, label, 22f, filled ? T95 : T40).rectTransform.anchoredPosition = new Vector2(40f, 0f);
    }

    void MakeRoomRow(Transform parent, float y, GoStopRoomScanner.DiscoveredRoom room, UnityEngine.Events.UnityAction onClick)
    {
        var row = MakeRect("Room", parent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        row.sizeDelta = new Vector2(660f, 84f);
        row.anchoredPosition = new Vector2(0f, y);
        var img = AddImg(row, UISkin.ButtonLine, Surface2, true);
        img.raycastTarget = true;

        AddLabel(row, $"{room.hostName}의 방", 24f, T95).rectTransform.anchoredPosition = new Vector2(0f, 16f);
        AddLabel(row, $"{room.playerCount}/{room.maxPlayers}명 참가 중", 16f, T70).rectTransform.anchoredPosition = new Vector2(0f, -18f);

        row.gameObject.AddComponent<Button>().onClick.AddListener(onClick);
    }

    // ── 공용 헬퍼 — body 안쪽 가변 콘텐츠(로우/버튼)를 여전히 코드로
    // 그리는 데 쓴다. 정적 틀(Panel/Card/제목/부제/닫기)은 이제 프리팹이
    // 담당하므로 그쪽 전용 헬퍼(Stretch 등)는 제거했다.
    static RectTransform MakeRect(string name, Transform parent, Vector2 aMin, Vector2 aMax)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = aMin; rt.anchorMax = aMax;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        return rt;
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

    // ── 실시간 갱신 ───────────────────────────────────────
    // 방 목록(Scanning)은 로비 이벤트가 없다 — 호스트가 몇 초마다 광고를
    // 새로 쏘고, 스캐너 쪽은 타임아웃으로 방이 조용히 사라지는 구조라
    // "방금 바뀌었다"는 신호 자체가 없다. 그래서 이 화면만 주기적으로
    // 폴링해서 다시 그린다.
    float nextScanRedraw;
    void Update()
    {
        if (screen != Screen.Scanning || !panelRT.gameObject.activeSelf) return;
        if (Time.unscaledTime < nextScanRedraw) return;
        nextScanRedraw = Time.unscaledTime + 0.5f;
        Redraw();
    }
}
