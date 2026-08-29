using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 우측 하단 채팅/이벤트 로그 — 2026-08-28 신규,
/// <c>Assets/Resources/Prefabs/GoStop/UI/ChatPanel.prefab</c>(뷰:
/// <see cref="GoStopChatView"/>)로 프리팹화했다. 두 가지 역할을 한 창에서
/// 겸한다: (1) 게임에서 이미 일어나는 사건(선 결정·카드 플레이·뻑/따닥/쪽/
/// 싹쓸이/폭탄/흔들기/보너스/총통/나가리/승리/판돈 이동)을 자동으로 한 줄씩
/// 적어주는 로그, (2) 네트워크 대전일 때 실제로 타이핑해서 주고받는 채팅.
///
/// 2026-08-28 탭 분리 — "전체/채팅/로그" 탭으로 나눠서 필터링한다. 모든
/// 항목은 <see cref="ChatEntry.isChat"/>로 분류된다 — 사람이 직접 친 채팅
/// (<see cref="SendChatMessage"/>/<see cref="HandleIncomingGuestChat"/>)만
/// true, 나머지(자동 이벤트 로그)는 전부 false. 네트워크로 릴레이할 때도
/// 이 분류를 <c>GoStopNetMessage.boolValue</c>에 실어 보내서, 게스트 화면의
/// 탭 필터링도 호스트와 항상 일치한다.
///
/// <b>왜 대부분 새 코드 없이 나왔는가</b> — 이 게임은 이미 <see cref="Toast"/>
/// (seat,label) 하나로 뻑/따닥/쪽/싹쓸이/폭탄/흔들기/보너스/총통/나가리
/// (필드 4장) 등 거의 모든 판정 이벤트를 좁은 진입점에 몰아 넣고 있었다
/// (토스트+사운드+파티클까지 이 함수 하나가 담당) — 그 함수 안에 로그 한
/// 줄만 얹으면 이 사건들을 전부 공짜로 받는다. 마찬가지로
/// <see cref="FlyMoneyFX"/>(fromSeat,toSeat,amount)가 판돈이 움직이는 모든
/// 경로(광팔이 정산·뻑/따닥 보너스·최종 정산)의 유일한 통로였다. 선 결정
/// (<c>DetermineDealerSeq</c>)·카드 플레이(<c>PlaySeq</c>)·승리/나가리
/// (<c>EndGame</c>)만은 이런 공용 진입점이 없어서 그 지점에 직접 한 줄씩
/// 추가했다.
///
/// <b>네트워크 전파</b> — <see cref="Toast"/>가 겪는 이벤트는 이미
/// <c>GoStopNetMessage.Type.Event</c>로 게스트에게 릴레이돼 게스트 화면에서도
/// 같은 <see cref="Toast"/>가 그대로 재실행되므로(토스트/사운드/파티클까지
/// 게스트도 똑같이 봄), 그 안에서는 <see cref="LogLocalLine"/>(브로드캐스트
/// 안 함)만 부른다 — 여기서 또 브로드캐스트하면 게스트가 Event 재생 1번
/// + 이 채팅 로그 릴레이 1번으로 같은 줄이 두 번 찍힌다. 반대로 선 결정·
/// 카드 플레이·승리/나가리·판돈 이동은 기존에 게스트로 전파해줄 메시지가
/// 없어서 <see cref="AppendChatLine"/>(항상 새 <c>Type.ChatLog</c> 메시지로
/// 브로드캐스트)을 쓴다. 실제 타이핑 채팅은 게스트→호스트는 원문 그대로,
/// 호스트→게스트(전원 릴레이, 보낸 사람 포함)는 이름까지 붙인 완성된 한
/// 줄이다 — 그래서 보낸 사람도 자기 메시지를 "낙관적 로컬 에코" 없이 이
/// 릴레이를 통해 받아서 표시한다(중복 표시를 피하는 가장 단순한 방법).
/// </summary>
public partial class GoStop3PGame
{
    enum ChatFilter { All, Chat, Log }

    struct ChatEntry
    {
        public string text;
        public bool isChat;
        public ChatEntry(string text, bool isChat) { this.text = text; this.isChat = isChat; }
    }

    const int CHAT_MAX_LINES = 80;
    readonly List<ChatEntry> chatEntries = new List<ChatEntry>();
    ChatFilter chatFilter = ChatFilter.All;

    GoStopChatView chatView;
    TextMeshProUGUI chatLogText;
    ScrollRect chatScroll;
    TMP_InputField chatInputField;

    static readonly Color ChatTabOn  = new Color(0.106f, 0.133f, 0.267f, 1f); // #1B2244 — 선택된 탭
    static readonly Color ChatTabOff = new Color(0.85f, 0.85f, 0.88f, 1f);    // 선택 안 된 탭

    /// <summary>프리팹(정적 틀) 인스턴스화 + 런타임 전용 배선만 한다 —
    /// 구조 자체(배경·헤더·탭·스크롤 로그·입력 행)는 전부
    /// <c>ChatPanel.prefab</c>/<see cref="GoStopChatView"/>가 담당한다.</summary>
    void BuildChatUI(RectTransform canvasRoot)
    {
        chatView = HwatuUI.InstantiateUIPrefab<GoStopChatView>("ChatPanel", canvasRoot);
        if (chatView == null) return; // 프리팹 로드 실패 — 로그를 쌓을 곳이 없을 뿐, 게임 자체는 계속 진행돼야 한다

        // 프리팹을 독립 에셋으로 저장하는 과정에서 Canvas.overrideSorting이
        // false로 초기화돼 저장된다(루트 자체엔 부모 Canvas가 없어서 저장
        // 시점에 "의미 없는 값"으로 리셋되는 것으로 보인다) — 인스턴스화
        // 직후 여기서 다시 켜줘야 "팝업이 몇 개 열려도 항상 최상단" 요구가
        // 실제로 성립한다. 베이킹 스크립트로 프리팹을 다시 구울 때마다 이걸
        // 깜빡하기 쉬우므로 반드시 런타임에서 재보증한다.
        var overrideCanvas = chatView.GetComponent<Canvas>();
        if (overrideCanvas != null) { overrideCanvas.overrideSorting = true; overrideCanvas.sortingOrder = 500; }

        chatLogText = chatView.logText;
        chatScroll = chatView.logScroll;
        chatInputField = chatView.inputField;

        bool interactive = isNetworkHost || isNetworkGuest;
        chatView.inputRow.SetActive(interactive);
        if (interactive)
        {
            chatInputField.onSubmit.AddListener(_ => SendChatMessage());
            chatView.sendButton.onClick.AddListener(SendChatMessage);
        }

        for (int i = 0; i < chatView.tabButtons.Length; i++)
        {
            var filter = (ChatFilter)i; // 프리팹의 탭 순서(전체/채팅/로그)가 이 enum 순서와 반드시 같아야 한다
            chatView.tabButtons[i].onClick.AddListener(() => SetChatFilter(filter));
        }
        RefreshTabVisuals();

        RedrawChatLog();
    }

    void SetChatFilter(ChatFilter filter)
    {
        if (chatFilter == filter) return;
        chatFilter = filter;
        RefreshTabVisuals();
        RedrawChatLog();
    }

    void RefreshTabVisuals()
    {
        if (chatView == null) return;
        for (int i = 0; i < chatView.tabImages.Length; i++)
        {
            bool on = (int)chatFilter == i;
            chatView.tabImages[i].color = on ? ChatTabOn : ChatTabOff;
            chatView.tabLabels[i].color = on ? Color.white : ChatTabOn;
        }
    }

    /// <summary>내가 입력창에 쳐서 "전송"을 눌렀을 때. 게스트는 원문 그대로
    /// 호스트에게 보내고(호스트가 이름을 붙여 완성한다), 호스트/오프라인은
    /// 직접 이름을 붙여 바로 기록+브로드캐스트한다.</summary>
    void SendChatMessage()
    {
        if (chatInputField == null) return;
        string text = chatInputField.text.Trim();
        chatInputField.text = "";
        chatInputField.ActivateInputField();
        if (string.IsNullOrEmpty(text)) return;

        if (isNetworkGuest)
            GoStopNetLobby.Instance?.SendToHost(GoStopNetMessage.ChatLogMsg(text));
        else if (isNetworkHost)
            AppendChatLine($"{SeatNameFor(PLAYER_SEAT, -1)}: {text}", isChat: true);
    }

    /// <summary>호스트 전용 — 게스트가 보낸 원문 채팅을 받았다. 보낸 좌석의
    /// 실제 이름을 붙여 완성한 뒤 <see cref="AppendChatLine"/>으로 내 로그에
    /// 남기고 전원(보낸 사람 포함)에게 릴레이한다 — 보낸 사람도 이 릴레이를
    /// 통해 자기 메시지를 받아 표시하므로 별도 로컬 에코가 필요 없다.</summary>
    void HandleIncomingGuestChat(int fromSeat, string rawText)
    {
        if (string.IsNullOrEmpty(rawText)) return;
        AppendChatLine($"{SeatNameFor(fromSeat, -1)}: {rawText}", isChat: true);
    }

    /// <summary>브로드캐스트가 필요한 이벤트용 — 선 결정·카드 플레이·승리/
    /// 나가리·판돈 이동·(호스트가 직접 친) 채팅처럼, 게스트에게 따로 전해줄
    /// 기존 경로가 없는 것들이 쓴다. 호스트가 아니면(오프라인이거나, 이미
    /// 다른 경로로 게스트에게 전달된 이벤트를 재생하는 게스트 자신이면)
    /// 그냥 로컬에만 남긴다. <paramref name="isChat"/>은 탭 분류용 —
    /// 사람이 직접 친 채팅만 true.</summary>
    void AppendChatLine(string line, bool isChat = false)
    {
        LogLocalLine(line, isChat);
        if (isNetworkHost) GoStopNetLobby.Instance?.BroadcastToGuests(GoStopNetMessage.ChatLogMsg(line, isChat));
    }

    /// <summary>내 화면에만 적는다(브로드캐스트 없음) — <see cref="Toast"/>처럼
    /// 이미 다른 경로(Event 메시지 재생)로 양쪽 화면에서 각자 실행되는
    /// 이벤트, 그리고 게스트가 호스트로부터 릴레이받은 채팅/이벤트 줄이
    /// 쓴다. 여기서 또 브로드캐스트하면 중복 표시가 난다.</summary>
    void LogLocalLine(string line, bool isChat = false)
    {
        chatEntries.Add(new ChatEntry(line, isChat));
        if (chatEntries.Count > CHAT_MAX_LINES) chatEntries.RemoveAt(0);
        RedrawChatLog();
    }

    void RedrawChatLog()
    {
        if (chatLogText == null) return;
        IEnumerable<ChatEntry> visible = chatFilter switch
        {
            ChatFilter.Chat => chatEntries.Where(e => e.isChat),
            ChatFilter.Log => chatEntries.Where(e => !e.isChat),
            _ => chatEntries,
        };
        chatLogText.text = string.Join("\n", visible.Select(e => e.text));
        Canvas.ForceUpdateCanvases(); // 레이아웃을 먼저 갱신해야 아래 스크롤 위치가 새 높이 기준으로 맞는다
        if (chatScroll != null) chatScroll.verticalNormalizedPosition = 0f; // 0 = 맨 아래(최신 줄)
    }
}
