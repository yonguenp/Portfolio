using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// GoStopGame(2인 맞고)의 UI 구성 부분 — 화면 배치(BuildStaticUI), 팝업 빌더
/// (흔들기/필드선택/9월열끗/점수상세), 카드 렌더링(RebuildUI/BuildCapturedRows/
/// DrawCardZone), 애니메이션(SlamIn/FlyAndPunch/ActionPopup) 전부 여기 있다.
/// 턴 진행·규칙 판정 등 게임 로직은 GoStopGame.cs(Core)에 있다 — 같은 타입을
/// partial class로 역할별로 나눴을 뿐, 멤버는 두 파일 사이에서 자유롭게 보인다.
/// </summary>
public partial class GoStopGame
{
    // ── UI 구성 ──────────────────────────────────────────
    // 세로 배치(ContentArea 상단 기준, y는 아래로 갈수록 음수):
    //   상대 정보 → 상대 세트뱃지 → 상대 손패(뒷면) → 상대 획득패 4줄
    //   → 필드 → 내 세트뱃지 → 내 획득패 4줄 → [HUD 토스트가 뜨는 구간] → 내 손패
    // 공용 토스트(GameUIManager)가 화면 하단에서 300~384px(세로 964 기준) 구간을
    // 고정으로 차지한다 — 손패는 반드시 그 아래로 내려서 토스트가 떠도 탭이
    // 막히지 않게 한다. 그 앞 요소들은 최대한 압축해서 겹침을 줄인다.
    /// <summary>
    /// 각 블록을 "이전 블록 하단 + 4px 여백"으로 순서대로 쌓는다 — 하나라도
    /// 손으로 어림하면 다음 블록과 겹친다(이 프로젝트에서 여러 번 겪은 함정).
    /// 손패(Hand)만 예외로 16px 여백을 더 준다 — 토스트 구간(약 -580~-664)이
    /// PlayerCap과는 겹쳐도 무방하지만(캡처 카드는 토스트 중에도 안 눌러도
    /// 그만) Hand는 항상 눌려야 하므로 여유를 더 둔다.
    /// <br/>
    /// ContentArea는 실측(`GetWorldCorners`) 964px 전체를 쓸 수 있다 — v4에서
    /// 손패 아래 138px을 그냥 남겨뒀던 게 "AiCap/PlayerCap이 안 보인다"는
    /// 재신고의 원인이었다(짐작만 하고 실측을 안 했다). 그 여유를 전부
    /// 획득패 카드(CAP_*)를 키우는 데 썼다.
    ///
    /// aiInfoText(-6,h24) → aiSetText(-30,h22) → AiBacks(-56,h44) →
    /// AiCap(-104,h208=4행×52) → Field(-316,h238=2행×114+10) →
    /// PlayerSetText(-558,h22) → PlayerCap(-584,h208) → Hand(-796,h136,바닥여유32px)
    /// </summary>
    void BuildStaticUI()
    {
        var root = ui.ContentArea;

        aiInfoText = HwatuUI.MakeLabel(root, new Vector2(0f, -6f), new Vector2(1000f, 24f), 20f, new Color(1, 1, 1, 0.85f));
        // 세트 뱃지(고도리/홍단/…) 줄은 배경 없이 풀밭 위에 글자만 떠 있으면
        // "텍스트로만 채워져 있어 후지다"는 인상을 준다 — 얇은 반투명 바를
        // 뒤에 깔아 하나의 상태 패널처럼 보이게 한다.
        HwatuUI.MakeRowBg(root, new Vector2(0f, -30f), new Vector2(1000f, 24f));
        aiSetText  = HwatuUI.MakeLabel(root, new Vector2(0f, -30f), new Vector2(1000f, 22f), 18f, Color.white);
        aiBackArea = HwatuUI.MakeRect("AiBacks", root, new Vector2(1000f, BACK_H), new Vector2(0f, -56f));
        aiCapArea  = HwatuUI.MakeRect("AiCap", root, new Vector2(1000f, CAP_ROW_PITCH * 4f), new Vector2(0f, -104f));

        fieldArea    = HwatuUI.MakeRect("Field", root, new Vector2(780f, FIELD_H * 2f + 10f), new Vector2(0f, -316f));
        drawPileArea = HwatuUI.MakeRect("DrawPile", root, new Vector2(160f, FIELD_H * 2f + 10f), new Vector2(430f, -316f));

        HwatuUI.MakeRowBg(root, new Vector2(0f, -558f), new Vector2(1000f, 24f));
        playerSetText = HwatuUI.MakeLabel(root, new Vector2(0f, -558f), new Vector2(1000f, 22f), 18f, Color.white);
        playerCapArea = HwatuUI.MakeRect("PlayerCap", root, new Vector2(1000f, CAP_ROW_PITCH * 4f), new Vector2(0f, -584f));

        // 판돈 — 동전 아이콘 + 숫자. 텍스트 줄 안에 섞어 넣으면 "몇 번째
        // 숫자가 돈인지" 안 읽혀서, 같은 y줄의 남는 여백(중앙정렬 텍스트가
        // 안 쓰는 좌우 가장자리)에 별도 칩으로 뺐다 — 새 세로 줄을 안 늘려도
        // 되니 이미 빡빡한 레이아웃에 부담이 없다.
        aiMoneyText     = HwatuUI.BuildMoneyChip(root, new Vector2(420f, -6f));
        playerMoneyText = HwatuUI.BuildMoneyChip(root, new Vector2(-420f, -558f));

        // 손패는 토스트 구간(약 -580~-664) 아래로 내려서 배치한다.
        handArea = HwatuUI.MakeRect("Hand", root, new Vector2(1000f, HAND_H), new Vector2(0f, -796f));

        // 팝업(딤+패널)은 전부 ContentArea가 아니라 Canvas 바로 밑(Overlay와
        // 같은 층)에 붙인다 — ContentArea 밑에 두면 게임오버 Overlay(Canvas
        // 자식 중 나중 순번이라 항상 위에 그려진다)가 팝업을 덮어버릴 수
        // 있다("점수 상세가 오버레이보다 뒤에 있어서 안 보인다"는 신고로
        // 발견). 지금 뜨는 시점이 게임 중이라 안 겹치는 팝업도 미리 통일해
        // 둔다 — 규칙이 하나여야 나중에 또 걸리지 않는다.
        var canvasRoot = root.parent.parent as RectTransform;
        BuildShakeConfirmUI(canvasRoot);
        BuildFieldChoiceUI(canvasRoot);
        BuildDualPiChoiceUI(canvasRoot);
        BuildScoreDetailUI(canvasRoot);
    }

    /// <summary>
    /// 필드에 같은 달이 2장 있을 때 어느 걸 가져올지 고르는 팝업. 후보 카드는
    /// 매번 다르므로(달·개수 고정 2장) <see cref="ShowFieldChoicePopup"/>에서
    /// 그때그때 만들어 붙인다 — 프리팹은 틀(딤+패널+안내문)만 담고 있다.
    /// </summary>
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

    /// <summary>후보 카드 2장을 팝업에 그리고, 눌린 카드를 <see cref="pendingFieldChoice"/>에 담는다.</summary>
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

    /// <summary>게스트는 결정을 호스트로 보내고 팝업을 직접 닫는다(호스트
    /// 쪽 ContinueChoice의 WaitForRemoteMessage가 대신 기다리고 있다) —
    /// 로컬(싱글플레이·호스트 자신)이면 예전처럼 pendingFieldChoice를 세워
    /// WaitUntil을 풀어준다(GoStop3PGame.UI.cs의 같은 패턴 참고).</summary>
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

    /// <summary>
    /// <see cref="GoStopRules.Resolve"/>가 2장 매칭이라 선택을 미룬 경우(choiceCandidates
    /// != null) 실제로 고르게 해서 마무리한다. 아니면 그냥 그대로 통과시킨다 —
    /// 호출부가 "선택이 필요할 수도 있다" 여부를 미리 안 갈라도 되게 한다.
    /// 플레이어는 팝업으로 직접 고르고, AI는 <see cref="GoStopAI.ChooseFieldMatch"/>로
    /// 즉시 정한다 — 단 네트워크 호스트라면 "ai" 역할은 실제로는 접속한
    /// 게스트라 팝업을 보내고 응답을 기다린다.
    /// </summary>
    IEnumerator ContinueChoice(HwatuCard played, GoStopRules.CaptureResult initial, bool isPlayerSide,
                               System.Action<GoStopRules.CaptureResult> onResolved)
    {
        if (initial.choiceCandidates == null) { onResolved(initial); yield break; }

        HwatuCard chosen;
        if (isPlayerSide)
        {
            pendingFieldChoice = null;
            ShowFieldChoicePopup(initial.choiceCandidates);
            yield return new WaitUntil(() => pendingFieldChoice != null);
            chosen = pendingFieldChoice;
            HideFieldChoicePopup();
        }
        else if (isNetworkHost)
        {
            SendTargetedPrompt(s => s.fieldChoiceCandidates = GoStopDeck.EncodeAll(initial.choiceCandidates));
            GoStopNetMessage msg = null;
            yield return StartCoroutine(WaitForRemoteMessage(m => m.type == GoStopNetMessage.Type.FieldChoice, m => msg = m));
            // 게스트가 보낸 건 스냅샷에서 새로 디코딩한 별개의 HwatuCard
            // 객체다 — 진짜 후보 인스턴스를 찾아 써야 한다(참조 동일성 함정,
            // GoStop3PGame.ContinueChoice와 같은 이유).
            var decoded = GoStopDeck.Decode(msg.cardId);
            chosen = decoded != null ? initial.choiceCandidates.FirstOrDefault(c => c.spriteName == decoded.spriteName) : null;
            if (chosen == null) chosen = GoStopAI.ChooseFieldMatch(initial.choiceCandidates); // 방어
        }
        else chosen = GoStopAI.ChooseFieldMatch(initial.choiceCandidates);

        onResolved(GoStopRules.ResolveChoice(played, chosen, field));
    }

    /// <summary>
    /// 9월 열끗(국화, <see cref="HwatuCard.dualPi"/>)이 내 획득패로 갓 들어온
    /// 순간 열끗/쌍피 역할을 한 번만 묻는다. AI는 팝업 없이
    /// <see cref="GoStopAI.OptimizeDualPi"/>가 캡처 직후 곧바로 유리한 쪽으로
    /// 정하므로 <paramref name="isPlayerSide"/>=false이면서 네트워크 호스트가
    /// 아닐 때만 이 코루틴을 건너뛴다 — 네트워크 호스트라면 "ai" 역할은
    /// 실제로는 접속한 게스트라 마찬가지로 팝업을 보내고 응답을 기다린다.
    /// </summary>
    IEnumerator PromptDualPiChoice(HwatuCard card, bool isPlayerSide)
    {
        if (!isPlayerSide && isNetworkHost)
        {
            SendTargetedPrompt(s => s.dualPiChoicePending = true);
            GoStopNetMessage msg = null;
            yield return StartCoroutine(WaitForRemoteMessage(m => m.type == GoStopNetMessage.Type.DualPiChoice, m => msg = m));
            card.useAsPi = msg.boolValue;
            yield break;
        }
        pendingDualPiChoice = null;
        dualPiPopup.Show();
        yield return new WaitUntil(() => pendingDualPiChoice != null);
        card.useAsPi = pendingDualPiChoice.Value;
        dualPiPopup.Hide();
    }

    void BuildDualPiChoiceUI(RectTransform canvasRoot)
    {
        dualPiPopup = HwatuUI.InstantiatePopup<ModalTwoButtonPopup>("DualPiPopup", canvasRoot);
        dualPiPopup.SetPrimary(() => OnDualPiChoiceClicked(false));
        dualPiPopup.SetSecondary(() => OnDualPiChoiceClicked(true));
    }

    /// <summary>참가 선언과 같은 이유(OnFieldChoiceClicked 문서 참고) —
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

    void BuildShakeConfirmUI(RectTransform canvasRoot)
    {
        shakePopup = HwatuUI.InstantiatePopup<ModalTwoButtonPopup>("ShakeConfirmPopup", canvasRoot);
        shakePopup.SetPrimary(() => OnShakeChoice(true));
        shakePopup.SetSecondary(() => OnShakeChoice(false));
    }

    /// <summary>
    /// newPlayerCapturedFrom/newAiCapturedFrom — 이번 갱신으로 새로 추가된
    /// 획득 카드의 시작 인덱스. null이면(초기 딜) 애니메이션 없이 그린다.
    /// </summary>
    /// <summary>더미 — 뒷면을 겹쳐 쌓은 스택 + 장수 배지. 장수가 줄어들수록
    /// 겹친 층도 줄어서 눈으로도 줄어드는 게 보인다. RebuildUI에서 갈아
    /// 그릴 때 쓰지만, 딜링 애니메이션(<see cref="DealingAnimationSeq"/>)
    /// 이 시작되기 전에 "더미가 꽉 찬 모습"부터 보여주기 위해 단독으로도
    /// 부를 수 있게 뽑아냈다(2026-08-20, 3인판의 UpdatePileVisual과 같은
    /// 이유 — 다만 3인판처럼 레이어 개수 차이만큼만 증분 갱신하지는
    /// 않는다, 여기는 원래도 매번 통째로 다시 그리는 더 단순한 구조라
    /// 그대로 뒀다).</summary>
    /// <summary>딜링 애니메이션 시작 전에 지난 판 카드들을 화면에서 지운다
    /// — 2026-08-20 정정(사용자 신고, 4인판과 같은 이유). RebuildUI가
    /// 매턴 지우는 것과 똑같은 목록(더미만 빼고)을 그대로 지운다.</summary>
    void ClearBoardForDealing()
    {
        HwatuUI.ClearChildren(fieldArea);
        HwatuUI.ClearChildren(handArea);
        HwatuUI.ClearChildren(playerCapArea);
        HwatuUI.ClearChildren(aiCapArea);
        HwatuUI.ClearChildren(aiBackArea);
    }

    void RedrawDrawPile()
    {
        HwatuUI.ClearChildren(drawPileArea);
        if (drawPile.Count == 0) return;

        int layers = Mathf.Clamp((drawPile.Count + 4) / 5, 1, 4);   // 1~5장=1층 ... 16~20장=4층
        for (int i = 0; i < layers; i++)
            HwatuUI.MakeCardBack(drawPileArea, new Vector2(0f, -i * 4f), PILE_W, PILE_H);

        var badge = HwatuUI.MakeRect("PileBadge", drawPileArea, new Vector2(40f, 28f), new Vector2(PILE_W * 0.5f + 12f, -6f));
        var badgeImg = badge.gameObject.AddComponent<Image>();
        badgeImg.sprite = HwatuShapes.RoundedRect(64, 13);
        badgeImg.type = Image.Type.Sliced;
        badgeImg.color = new Color(0.13f, 0.16f, 0.30f, 0.95f);
        var badgeLabel = HwatuUI.MakeLabel(badge, Vector2.zero, new Vector2(40f, 28f), 16f, Color.white);
        badgeLabel.text = drawPile.Count.ToString();
        badgeLabel.fontStyle = FontStyles.Bold;
    }

    void RebuildUI(int? newPlayerCapturedFrom = null, int? newAiCapturedFrom = null)
    {
        HwatuUI.ClearChildren(fieldArea);
        HwatuUI.ClearChildren(handArea);
        HwatuUI.ClearChildren(playerCapArea);
        HwatuUI.ClearChildren(aiCapArea);
        HwatuUI.ClearChildren(aiBackArea);
        RedrawDrawPile();

        // 필드 — 같은 달끼리 뭉쳐서 놓는다(1~12월 순). 예전엔 뭉친 카드들도
        // 각자 자기 그리드 칸을 온전히 차지해서 나란히 놓였는데, 그러면 "이
        // 두 장이 한 세트"라는 게 안 보이고 그냥 우연히 옆에 온 카드처럼
        // 보였다("겹쳐 보이는 게 버그 같다"는 신고 — 실은 안 겹쳐서 생긴
        // 문제였다). 지금은 한 달의 카드들을 부채처럼 살짝 겹쳐 쌓아서
        // 한 덩어리로 읽히게 한다. 나중에 추가된(종류 순서상 뒤) 카드가
        // sibling index도 나중이라 자동으로 위에 그려진다.
        const float STACK_OFFSET = 26f; // 같은 달 카드끼리 겹칠 때 가로 오프셋
        const float GROUP_GAP = 16f;    // 서로 다른 달 그룹 사이 간격
        const float ROW_WIDTH = 780f;   // fieldArea 폭과 맞춤

        var fieldGroups = field.GroupBy(c => c.month)
                               .OrderBy(g => g.Key)
                               .Select(g => g.OrderBy(c => (int)c.kind).ToList())
                               .ToList();

        float GroupWidth(int n) => FIELD_W + (n - 1) * STACK_OFFSET;

        var rows = new List<List<List<HwatuCard>>>();
        var curRow = new List<List<HwatuCard>>();
        float curW = 0f;
        foreach (var g in fieldGroups)
        {
            float addW = GroupWidth(g.Count) + (curRow.Count > 0 ? GROUP_GAP : 0f);
            if (curRow.Count > 0 && curW + addW > ROW_WIDTH)
            {
                rows.Add(curRow);
                curRow = new List<List<HwatuCard>>();
                curW = 0f;
                addW = GroupWidth(g.Count);
            }
            curRow.Add(g);
            curW += addW;
        }
        if (curRow.Count > 0) rows.Add(curRow);

        for (int r = 0; r < rows.Count; r++)
        {
            var row = rows[r];
            float totalW = row.Sum(g => GroupWidth(g.Count)) + GROUP_GAP * (row.Count - 1);
            float cx = -totalW * 0.5f;
            float y = -r * (FIELD_H + 10f);
            foreach (var g in row)
            {
                for (int i = 0; i < g.Count; i++)
                {
                    float x = cx + FIELD_W * 0.5f + i * STACK_OFFSET;
                    var fgo = HwatuUI.MakeCard(g[i], fieldArea, new Vector2(x, y), FIELD_W, FIELD_H, null, false);
                    if (flyFrom.TryGetValue(g[i], out var ffrom))
                        StartCoroutine(SlamIn(fgo.transform as RectTransform, ffrom));
                }
                cx += GroupWidth(g.Count) + GROUP_GAP;
            }
        }

        // 상대 손패 — 실제 패는 안 보여주고 뒷면 장수만. 마주 앉아 치는 느낌을 준다.
        int aiN = aiHand.Count;
        float aiTotal = aiN * (BACK_W + 4f) - 4f;
        for (int i = 0; i < aiN; i++)
        {
            float x = -aiTotal * 0.5f + BACK_W * 0.5f + i * (BACK_W + 4f);
            HwatuUI.MakeCardBack(aiBackArea, new Vector2(x, 0f), BACK_W, BACK_H);
        }

        // 손패 — 정렬돼 있고, 필드와 달이 맞아 낼 수 있는 카드는 금색으로 강조한다.
        // 폭탄 크레딧이 있으면 손패 맨 끝에 "빈 카드"(뒷면 + 개수)를 하나 더
        // 붙인다 — 예전엔 손패 위에 따로 뜨는 버튼이었는데, 그러면 "내 패 중
        // 하나를 고른다"는 느낌이 안 나서 놓치기 쉬웠다("마지막에 손패가
        // 떨어졌을 때만 자동으로 넘어가더라"는 신고). 손 안의 카드처럼 직접
        // 눌러서 고르게 한다.
        bool showBombSkip = playerBombCredits > 0 && state == State.PlayerTurn;
        int n = playerHand.Count + (showBombSkip ? 1 : 0);
        float total = n * (HAND_W + 8f) - 8f;
        for (int i = 0; i < playerHand.Count; i++)
        {
            var card = playerHand[i];
            float x = -total * 0.5f + HAND_W * 0.5f + i * (HAND_W + 8f);
            bool playable = field.Any(f => f.month == card.month);
            // 2026-08-20: "하이라이트 영역 어긋남 — 사이즈 95,145, posY 5로
            // 하면 딱맞음"(사용자 확인 값). 기본 공식(카드+16, 오프셋 0)이
            // HAND_W/H(88×136)에는 안 맞았다.
            var go = HwatuUI.MakeCard(card, handArea, new Vector2(x, 0f), HAND_W, HAND_H, () => OnPlayerPlay(card), playable,
                highlightSize: new Vector2(95f, 145f), highlightOffset: new Vector2(0f, 5f));

            // 2026-08-19: 4인판에 이미 있던 폭탄/흔들기/굳은자 가능 표시를
            // 2인판에도 포팅했다 — 조건·위치(카드 우측 상단 한 자리에 모아
            // 여러 개면 아래로 쌓기)는 4인판(GoStop3PGame.UI.cs)과 동일.
            int sameMonthHand = playerHand.Count(c => c.month == card.month);
            int sameMonthField = field.Count(f => f.month == card.month);
            bool bombable = sameMonthHand == 3 && sameMonthField == 1;
            bool shakeable = sameMonthHand == 3 && !playerShook.Contains(card.month);
            int capsCount = playerCaptured.Count(c => c.month == card.month) + aiCaptured.Count(c => c.month == card.month);
            bool stuckPair = (sameMonthHand == 1 && capsCount == 2 && sameMonthField >= 1)
                           || (sameMonthHand == 2 && capsCount >= 1);
            // 2026-08-20: 4인판(GoStop3PGame.UI.cs)과 같은 함정 — 굳은자만
            // (40,5)로 옮겨졌고 폭탄/흔들기는 낡은 공식을 그대로 쓰고 있어
            // 위치가 안 맞았다("굳은자와 위치 통일시켜줘" 신고). 셋 다 같은
            // 시작점으로 통일한다.
            const float ICON_S = 26f;
            float iconX = 40f, iconY = 5f;
            if (bombable) { GoStopIcons.MakeShapeIcon(go.transform, new Vector2(iconX, iconY), ICON_S, GoStopIcons.Bomb(), new Color(0.1f, 0.1f, 0.12f, 0.92f)); iconY -= ICON_S + 4f; }
            if (shakeable) { GoStopIcons.MakeShapeIcon(go.transform, new Vector2(iconX, iconY), ICON_S, GoStopIcons.Bell(), new Color(0.29f, 0.64f, 0.91f, 0.95f)); iconY -= ICON_S + 4f; }
            if (stuckPair) GoStopIcons.MakeTextIcon(go.transform, new Vector2(iconX, iconY), ICON_S, "!", new Color(0.85f, 0.2f, 0.2f), Color.white);
        }
        if (showBombSkip)
        {
            float x = -total * 0.5f + HAND_W * 0.5f + playerHand.Count * (HAND_W + 8f);
            MakeBombSkipSlot(handArea, new Vector2(x, 0f));
        }

        BuildCapturedRows(playerCapArea, playerCaptured, newPlayerCapturedFrom, interactive: true);
        BuildCapturedRows(aiCapArea, aiCaptured, newAiCapturedFrom, interactive: false);

        var p = GoStopRules.CalcScore(playerCaptured, playerSweeps);
        var a = GoStopRules.CalcScore(aiCaptured, aiSweeps);
        // 판돈은 기존 상태 줄에 얹는다 — 안 그러면 안 그래도 빡빡한 세로 레이아웃에
        // 또 새 줄을 끼워야 해서 토스트 겹침 문제가 재발한다.
        aiInfoText.text = $"상대 패 {aiHand.Count}장 · 더미 {drawPile.Count}장 · {a.Total}점 · 고 {aiGoCount}";
        // isSeon — 호스트는 항상 선(=매판 자기 쪽부터 시작)이다. 어느
        // 쪽 role 슬롯이 "호스트의 데이터"인지는 isNetworkGuest로 갈린다
        // (게스트 화면에서는 스왑 때문에 호스트 데이터가 ai 슬롯에 있다) —
        // 자세한 근거는 BuildSetBadges 문서 참고.
        aiSetText.text = BuildSetBadges(aiCaptured, playerCaptured, isPlayerSide: false, isSeon: isNetworkGuest);
        // 상대쪽엔 "N점 · 고 N"이 aiInfoText에 있는데 내쪽엔 대응하는 줄이
        // 없었다 — 화면 우상단 SCORE에 점수는 나가지만 고 횟수는 어디에도
        // 안 뜨고 있었다("고를 몇 번 했는지 표시가 안 된다"는 지적). 새 줄을
        // 늘리는 대신 이미 있는 배지 줄 앞에 붙였다.
        playerSetText.text = $"{p.Total}점 · 고 {playerGoCount} · " + BuildSetBadges(playerCaptured, aiCaptured, isPlayerSide: true, isSeon: isNetworkHost);
        if (aiMoneyText) aiMoneyText.text = aiMoney.ToString("N0");
        if (playerMoneyText) playerMoneyText.text = playerMoney.ToString("N0");

        // 상단 HUD의 SCORE는 판점이 아니라 내 보유 머니를 보여준다(사용자 요청) —
        // 판점 자체는 바로 위 playerSetText에 이미 나온다.
        ui?.SetScore(playerMoney);

        // 2026-08-20: "선 플레이어를 모르겠다·내 턴 표시 필요·상대가
        // 선택 중인 걸 표시해달라"는 신고 — 네트워크 대전에서만 타이틀
        // 텍스트를 턴 안내로 바꿔치기한다(싱글플레이는 기존 "고스톱"
        // 고정 타이틀 그대로 — BuildTurnIndicator가 null을 돌려준다).
        if (isNetworkHost || isNetworkGuest)
            ui?.SetTitle(BuildTurnIndicator() ?? "맞고 (네트워크)");

        // 이번 갱신에서 쓸 만큼 다 썼다 — 다음 RebuildUI까지는 다시 비워 둔다.
        flyFrom.Clear();
        flyViaField.Clear();

        CheckEmergencies();

        // 호스트만 — 로컬 화면이 갱신되는 매 순간 접속한 게스트도 같이
        // 갱신시킨다(GoStop3PGame.UI.cs의 같은 훅과 동일한 이유).
        if (isNetworkHost) BroadcastNetworkState();
    }

    /// <summary>2026-08-20: "고스톱(GoStop3PGame)에서 쓰는 유저 상태정보
    /// 표기를 맞고에도 똑같이 맞춰달라"는 요청 — 3~4인판은 흔들기/뻑 횟수와
    /// 광박/멍박/피박 실시간 위험을 원형 아이콘·카운트 배지로 보여준다
    /// (DrawBadgeStrip). 2인판은 이미 세로 공간이 극도로 빠듯하다고 여러
    /// 세션에 걸쳐 확인된 파일이라(레이아웃 상단 주석 참고) 새 UI 요소·
    /// 새 줄을 추가하는 대신, 이미 있는 이 배지 줄(고도리/홍단/초단/청단)에
    /// 같은 정보를 <b>색깔 텍스트로</b> 이어 붙이는 가장 낮은 위험의 방법을
    /// 택했다 — 순수 아이콘 위젯 이식이 아니라는 점에서 3~4인판과 겉모습은
    /// 다르지만, 표시되는 정보 자체(흔들기·뻑 횟수, 광박·피박 위험)는 동일하다.
    /// 멍박은 뺐다 — 2인판 문서가 "멍따는 의도적으로 안 넣었다"고 이미
    /// 밝혀뒀고, 3~4인판의 멍박 배지도 그 위에 얹은 실시간 안내일 뿐이라
    /// 굳이 새로 들여올 실익이 적다고 판단했다.
    /// 피박 기준은 2인 맞고 고유값 7(3~4인의 PI_BAK_THRESHOLD_3P=5와
    /// 다르다 — IsLivePiBakRisk 문서 참고).</summary>
    /// <summary><paramref name="isSeon"/> — 이 줄이 "선"(먼저 시작하는
    /// 쪽)을 나타내는지. 2인판은 매판 항상 호스트(player 역할)가 먼저
    /// 시작하므로(3~4인판처럼 승자가 다음 판 선을 잇는 로테이션이 없다),
    /// "어느 role 슬롯이 호스트의 데이터를 담고 있는지"만 알면 된다 —
    /// 호스트 자신의 화면에서는 player 슬롯이 곧 자기 자신(호스트)이라
    /// <c>isNetworkHost</c>를 그대로 쓰고, 게스트 화면에서는 스왑
    /// 때문에 호스트 데이터가 ai 슬롯에 있으므로 <c>isNetworkGuest</c>를
    /// 쓴다(호출부 참고).</summary>
    string BuildSetBadges(List<HwatuCard> mine, List<HwatuCard> theirs, bool isPlayerSide, bool isSeon)
    {
        string One(string label, System.Func<HwatuCard, bool> pred)
        {
            var (st, have) = GoStopRules.CheckSet(mine, theirs, pred);
            string color = st switch
            {
                GoStopRules.SetState.Achieved => "#7CE38B",
                GoStopRules.SetState.Blocked  => "#FF7A6E",
                _ => "#FFFFFFCC",
            };
            string mark = st == GoStopRules.SetState.Blocked ? " 막힘" : "";
            return $"<color={color}>{label} {have}/3{mark}</color>";
        }
        string sets = string.Join("   ",
            One("고도리", GoStopRules.IsGodori),
            One("홍단", GoStopRules.IsHongdan),
            One("초단", GoStopRules.IsChodan),
            One("청단", GoStopRules.IsCheongdan));

        var extra = new List<string>();
        if (isSeon) extra.Add("<color=#EDBA2E>선</color>");
        int shakeCount = (isPlayerSide ? playerShook : aiShook).Count;
        int ppeokCount = isPlayerSide ? playerPpeokTotal : aiPpeokTotal;
        if (shakeCount > 0) extra.Add($"<color=#EDC94E>흔들기 {shakeCount}</color>");
        if (ppeokCount > 0) extra.Add($"<color=#FF8A5C>뻑 {ppeokCount}</color>");
        if (GoStopRules.IsLiveGwangBakRisk(mine, new[] { theirs })) extra.Add("<color=#FF6E6E>광박 위험</color>");
        if (GoStopRules.IsLivePiBakRisk(mine, new[] { theirs }, 7)) extra.Add("<color=#FF6E6E>피박 위험</color>");

        return extra.Count > 0 ? sets + "   " + string.Join("   ", extra) : sets;
    }

    const int CAP_MAX_PER_ROW = 5; // 한 존에 5장까지, 6장째부터 위로 새 줄

    /// <summary>
    /// 획득패를 실제 화투판처럼 3열로 나눠 그린다 — 광 | (위)열끗·(아래)띠 | 피.
    /// 예전엔 광/열끗/띠/피를 완전히 다른 4줄로 세로로 늘어놨는데, 실물
    /// 배치와 다르고 세로 공간도 많이 먹었다. 각 존은 5장까지 한 줄, 6장째부터
    /// 그 존 안에서 위로 새 줄이 붙는다(<see cref="CAP_MAX_PER_ROW"/>).
    /// </summary>
    /// <summary>
    /// <paramref name="interactive"/>를 플레이어/상대 구분으로도 겸해 쓴다 —
    /// 내 획득패(interactive=true)는 손패와 붙어 있는 <b>아래쪽</b>이 기준이라
    /// 바닥부터 위로 쌓고, 상대 획득패(false)는 상대 손패 뒷면과 붙어 있는
    /// <b>위쪽</b>이 기준이라 꼭대기부터 아래로 쌓는다. 예전엔 둘 다 바닥
    /// 기준으로 그려서, 카드가 적을 때(제일 흔한 경우) 상대 획득패가 자기
    /// 존의 빈 위쪽 공간 아래 깔려 Field 줄과 시각적으로 붙어버렸다
    /// ("AiCap이 Field랑 헷갈린다"는 신고).
    /// </summary>
    void BuildCapturedRows(RectTransform area, List<HwatuCard> captured, int? animateFrom, bool interactive)
    {
        int newCount = animateFrom.HasValue ? captured.Count - animateFrom.Value : 0;
        var newCards = newCount > 0 ? captured.Skip(captured.Count - newCount).ToList() : null;

        // EffectiveKind로 묶는다 — 9월 열끗을 쌍피로 선택하면 열끗 존에서
        // 피 존으로 카드가 실제로 옮겨가야 "지금 뭘로 쓰는지" 한눈에 보인다.
        // 위치 자체가 지금 역할을 보여주므로 카드 위 별도 글자 태그는 안 붙인다.
        var gwang = captured.Where(c => c.EffectiveKind == HwatuKind.Gwang).OrderBy(c => c.month).ToList();
        var yeol  = captured.Where(c => c.EffectiveKind == HwatuKind.Yeolkkeut).OrderBy(c => c.month).ToList();
        var ddi   = captured.Where(c => c.EffectiveKind == HwatuKind.Ddi).OrderBy(c => c.month).ToList();
        var pi    = captured.Where(c => c.EffectiveKind == HwatuKind.Pi).OrderBy(c => c.month).ToList();

        float step = CAP_H + 4f;
        if (interactive)
        {
            // 내 획득패 — 존의 맨 아래 줄이 놓일 y를 존 전체 예산의 바닥에
            // 맞추고, 거기서부터 위(양의 y 방향)로 줄이 쌓인다.
            float baseline = -(CAP_ROW_PITCH * 4f - CAP_H);
            // 띠 위에 열끗을 놓아야 하므로, 띠가 몇 줄을 쓰는지부터 세서
            // 열끗의 시작 높이를 정한다 — 띠가 5장 넘게 쌓이는 흔치 않은
            // 경우에도 겹치지 않는다.
            int ddiRows = Mathf.Max(1, Mathf.CeilToInt(ddi.Count / (float)CAP_MAX_PER_ROW));
            float yeolBaseline = baseline + ddiRows * step;

            DrawCardZone(area, gwang, -260f, baseline,     step, interactive, newCards);
            DrawCardZone(area, ddi,      0f, baseline,     step, interactive, newCards);
            DrawCardZone(area, yeol,     0f, yeolBaseline, step, interactive, newCards);
            DrawCardZone(area, pi,     260f, baseline,     step, interactive, newCards, weighted: true); // 5장이 아니라 5피(쌍피=2) 기준
        }
        else
        {
            // 상대 획득패 — 존의 맨 위 줄이 존 꼭대기(y=0)에 놓이고, 거기서부터
            // 아래(음의 y 방향)로 줄이 쌓인다. 열끗이 띠보다 위에 있어야
            // 하므로 열끗을 꼭대기에 두고, 띠는 열끗이 쓰는 줄 수만큼 그 아래에서 시작한다.
            int yeolRows = Mathf.Max(1, Mathf.CeilToInt(yeol.Count / (float)CAP_MAX_PER_ROW));
            float ddiBaseline = -yeolRows * step;

            DrawCardZone(area, gwang, -260f, 0f,           -step, interactive, newCards);
            DrawCardZone(area, yeol,     0f, 0f,           -step, interactive, newCards);
            DrawCardZone(area, ddi,      0f, ddiBaseline,  -step, interactive, newCards);
            DrawCardZone(area, pi,     260f, 0f,           -step, interactive, newCards, weighted: true);
        }
    }

    /// <summary>한 존(광/열끗/띠/피 중 하나)을 줄을 갈라가며 그린다.
    /// <paramref name="baselineY"/>가 0번째 줄의 y, <paramref name="rowStep"/>이
    /// 한 줄마다 더할 오프셋이다(양수면 위로, 음수면 아래로 쌓인다).
    /// <paramref name="weighted"/>가 true면 장수가 아니라 피 값(쌍피=2) 합으로
    /// 줄을 나눈다(<see cref="HwatuUI.GroupIntoRows"/> — "5장씩"이 아니라
    /// "5피씩" 쌓여야 한다는 사용자 확인 규칙, 피 존에만 적용한다).</summary>
    void DrawCardZone(RectTransform area, List<HwatuCard> cards, float centerX, float baselineY, float rowStep,
                      bool interactive, List<HwatuCard> newCards, bool weighted = false)
    {
        var rows = HwatuUI.GroupIntoRows(cards, CAP_MAX_PER_ROW, weighted);
        for (int row = 0; row < rows.Count; row++)
        {
            var rowCards = rows[row];
            float rowWidth = rowCards.Count * (CAP_W + 3f) - 3f;
            float y = baselineY + row * rowStep;

            for (int i = 0; i < rowCards.Count; i++)
            {
                float x = centerX - rowWidth * 0.5f + CAP_W * 0.5f + i * (CAP_W + 3f);
                var card = rowCards[i];
                // 예전엔 획득패에서 9월 열끗을 아무 때나 눌러서 역할을 바꿀 수
                // 있었는데, "가져올 때 한 번만 정하고 그 뒤엔 못 바꾸게 하자"는
                // 피드백으로 상시 토글은 없앴다 — 선택은 캡처 순간 팝업
                // (PromptDualPiChoice)에서 한 번만 이뤄진다.
                var go = HwatuUI.MakeCard(card, area, new Vector2(x, y), CAP_W, CAP_H, null, false);
                if (flyFrom.TryGetValue(card, out var cfrom))
                {
                    if (flyViaField.TryGetValue(card, out var hitPoint))
                        StartCoroutine(SlamInViaField(go.transform as RectTransform, cfrom, hitPoint));
                    else
                        StartCoroutine(SlamIn(go.transform as RectTransform, cfrom));
                }
                else if (newCards != null && newCards.Contains(card))
                    StartCoroutine(PunchScale(go.transform));
            }
        }
    }

    IEnumerator PunchScale(Transform t)
    {
        const float dur = 0.22f;
        float elapsed = 0f;
        var rt = t as RectTransform;
        while (elapsed < dur && rt != null)
        {
            elapsed += Time.deltaTime;
            float s = Mathf.Lerp(1.4f, 1f, elapsed / dur);
            rt.localScale = new Vector3(s, s, 1f);
            yield return null;
        }
        if (rt != null) rt.localScale = Vector3.one;
    }

    /// <summary>게임을 시작하고 패를 나눠주는 연출(사용자 확인 규칙, 2인판) —
    /// 1차 돌리기: 필드 4장 + 나·상대 각 5장씩. 2차 돌리기: 필드 4장 더 +
    /// 나·상대 각 5장씩 더. 최종 손 10장씩·필드 8장(<see cref="GoStopRules.DealNew"/>이
    /// 이미 만들어 둔 실제 딜과 정확히 같은 장수). 4인판(<see cref="GoStop3PGame.DealingAnimationSeq"/>)
    /// 과 같은 원칙 — 순수 시각 연출만 하고 게임 상태는 전혀 안 건드린다.
    /// <see cref="NewGameSeq"/>가 손패/필드/더미를 다 채운 뒤, 실제로 그리는
    /// RebuildUI() 전에 부른다.</summary>
    IEnumerator DealingAnimationSeq()
    {
        yield return StartCoroutine(DealRound(4, 5));
        yield return StartCoroutine(DealRound(4, 5));
    }

    IEnumerator DealRound(int toField, int perSide)
    {
        for (int i = 0; i < toField; i++)
        {
            GoStopFX.FlyDealCard(ui.ContentArea, drawPileArea.position, fieldArea.position, FIELD_W, FIELD_H);
            yield return new WaitForSeconds(0.04f);
        }
        yield return new WaitForSeconds(0.08f);

        for (int i = 0; i < perSide; i++)
        {
            GoStopFX.FlyDealCard(ui.ContentArea, drawPileArea.position, handArea.position, HAND_W, HAND_H);
            GoStopFX.FlyDealCard(ui.ContentArea, drawPileArea.position, aiBackArea.position, BACK_W, BACK_H);
            yield return new WaitForSeconds(0.045f);
        }
        yield return new WaitForSeconds(0.12f); // 라운드 사이 여백
    }

    /// <summary>
    /// 손(또는 더미)에서 지금 자리까지 빠르게 날아와 <b>딱</b> 맞고 튕기는 연출.
    /// 딱지치기 느낌을 내려는 것 — 그냥 순간이동으로 나타나던 예전 방식엔
    /// "친다"는 손맛이 전혀 없었다. 이동은 짧고 빠르게(0.11초, 감속),
    /// 도착 즉시 살짝 커졌다가 튕기듯 줄어들고(충격), 마지막에 흰 원이
    /// 확 퍼졌다 사라지는 임팩트 플래시를 더한다.
    /// </summary>
    IEnumerator SlamIn(RectTransform rt, Vector3 fromWorld)
    {
        if (rt == null) yield break;
        yield return FlyAndPunch(rt, fromWorld, rt.position, 0.11f, 1.55f, 0.16f);
    }

    /// <summary>
    /// 필드의 짝을 실제로 쳐서 맞추는 2단 연출 — 손/더미에서 <b>맞은 필드패
    /// 자리까지</b> 먼저 날아가 딱 맞고 튕긴 다음(1구간), 거기서 다시 최종
    /// 획득패 자리까지 날아간다(2구간). 그냥 손→획득패로 한 방에 날아가면
    /// "필드에서 짝을 맞춰 가져온다"는 느낌이 안 산다는 지적으로 추가했다.
    /// </summary>
    IEnumerator SlamInViaField(RectTransform rt, Vector3 fromWorld, Vector3 hitWorld)
    {
        if (rt == null) yield break;
        Vector3 toWorld = rt.position;

        yield return FlyAndPunch(rt, fromWorld, hitWorld, 0.09f, 1.4f, 0.10f);
        if (rt == null) yield break;

        yield return FlyAndPunch(rt, hitWorld, toWorld, 0.14f, 1.55f, 0.16f);
    }

    /// <summary>이동(감속) + 도착 시 임팩트 플래시 + 펀치 스케일. SlamIn 계열이 공유하는 한 구간.</summary>
    IEnumerator FlyAndPunch(RectTransform rt, Vector3 from, Vector3 to, float flyDur, float punchScale, float punchDur)
    {
        float t = 0f;
        while (t < flyDur && rt != null)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / flyDur);
            p = 1f - (1f - p) * (1f - p); // ease-out — 도착 직전에 확 붙는다
            rt.position = Vector3.Lerp(from, to, p);
            yield return null;
        }
        if (rt == null) yield break;
        rt.position = to;
        SpawnImpactFlash(rt);

        t = 0f;
        while (t < punchDur && rt != null)
        {
            t += Time.deltaTime;
            float s = Mathf.Lerp(punchScale, 1f, t / punchDur);
            rt.localScale = new Vector3(s, s, 1f);
            yield return null;
        }
        if (rt != null) rt.localScale = Vector3.one;
    }

    /// <summary>내가 이겼을 때 화면 위에 색종이 폭죽을 터뜨린다. Overlay(승패
    /// 카드)보다 나중에 그려져야 가려지지 않으므로, ContentArea가 아니라
    /// Canvas 바로 밑(Overlay와 같은 층)에 붙이고 맨 위로 올린다 — "점수 상세"
    /// 팝업이 Overlay에 가려지던 버그를 고칠 때 확립한 것과 같은 규칙
    /// (위 "고스톱 UI 구조화" 문서 참고).</summary>
    void PlayWinConfettiFX()
    {
        var canvasRoot = ui.ContentArea.parent.parent as RectTransform;
        if (canvasRoot == null) return;
        // 새로 생성된 GameObject는 자동으로 마지막 sibling이 되므로(=가장
        // 나중에 그려짐=Overlay보다 위) 별도 정렬이 필요 없다.
        GoStopFX.PlayWinConfetti(canvasRoot, Vector2.zero);
    }

    /// <summary>판돈이 오갈 때(첫뻑/연뻑/첫따닥 보너스, 최종 정산) 동전이
    /// 낸 쪽 머니칩에서 받는 쪽 머니칩으로 날아가는 연출. 두 칩(aiMoneyText/
    /// playerMoneyText)은 BuildStaticUI에서 한 번만 만들어지는 안정적인
    /// Transform이라 RebuildUI 타이밍과 무관하게 항상 유효하다.</summary>
    void FlyMoneyFX(bool toPlayer, int amount)
    {
        if (amount <= 0) return;
        Vector3 from = (toPlayer ? aiMoneyText : playerMoneyText).transform.position;
        Vector3 to   = (toPlayer ? playerMoneyText : aiMoneyText).transform.position;
        GoStopFX.FlyMoney(ui.ContentArea, from, to, amount);
    }

    /// <summary>충격 지점에 흰 원이 확 퍼졌다 사라지는 짧은 플래시 — "딱!" 소리가 나는 것 같은 느낌을 시각으로 대신한다.</summary>
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

        // 2026-08-19: "파티클 이펙트로 애니메이션을 좀 더 역동적으로" —
        // 특별 이벤트(쪽/뻑/싹쓸이/폭탄)뿐 아니라 평범한 카드 매칭 한 번
        // 한 번에도 작은 스파크를 곁들인다(개수는 절반 이하로 줄여서
        // 큰 이벤트의 버스트와 시각적으로 구분되게 했다).
        //
        // 함정 — 처음엔 at.parent(필드/획득패/손패 컨테이너)에 그대로
        // 붙였다가 DOTween이 "Image가 파괴됐는데 계속 접근하려 한다"는
        // 예외를 던지며 그 프레임의 코루틴을 통째로 멈춰버렸다(actionBusy가
        // 영원히 true로 남는 버그로 나타났다) — 이 컨테이너들은 매
        // RebuildUI마다 ClearChildren으로 통째로 지워지는데, 파티클의
        // DOTween 트윈(0.4~0.6초)이 끝나기 전에 다음 RebuildUI가 먼저
        // 돌면 트윈 대상 Image가 중간에 파괴돼 버린다. 코루틴 기반
        // FlashAndDestroy는 매 프레임 null 체크를 해서 안전하지만, DOTween
        // 트윈은 그 보호가 없다 — 그래서 파티클은 절대 안 지워지는 안정적인
        // 부모(ContentArea, fieldArea.parent)에 별도로 붙이고 월드 좌표를
        // 그 공간으로 변환해서 위치만 맞춘다.
        if (fieldArea.parent is RectTransform stableParent)
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

    /// <summary>뻑/쪽/싹쓸이/피뺏기처럼 "지금 뭐가 일어났는지" 피드백이 약하다는
    /// 신고를 받아 추가했다 — 작은 토스트 한 줄만으로는 눈에 잘 안 띈다.
    /// 필드 중앙 위에 큼직한 컬러 텍스트를 띄워 순간적으로 확 커졌다 사라지게
    /// 한다. <see cref="Toast"/>의 라벨과 같은 문자열 매칭으로 트리거한다
    /// (사운드의 <see cref="GoStopAudio.PlayForLabel"/>과 같은 접근).</summary>
    void ShowActionPopup(string label)
    {
        Color? color =
            label == "따닥"          ? new Color(0.72f, 0.45f, 0.95f) : // exact — "첫따닥"과는 다른 이벤트
            label.Contains("쪽")     ? new Color(0.35f, 0.85f, 1f) :
            label.Contains("싹쓸이") ? new Color(1f, 0.82f, 0.25f) :
            label.Contains("폭탄")   ? new Color(1f, 0.45f, 0.30f) :
            label.Contains("뻑")     ? new Color(0.95f, 0.55f, 0.20f) :
            (Color?)null;
        if (color == null) return; // 흔들기·보너스 등은 토스트만으로 충분하다고 판단해 팝업은 생략

        var burstPos = fieldArea.anchoredPosition + new Vector2(0f, -60f);
        // 2026-08-19: "파티클 이펙트로 애니메이션을 좀 더 역동적으로" 요청 —
        // 텍스트 팝업과 같은 자리·같은 색으로 원형 파티클 버스트를 같이
        // 터뜨린다(4인판 GoStop3PGame.cs의 ShowActionPopup과 같은 패턴).
        GoStopIcons.SpawnBurst(fieldArea.parent as RectTransform, burstPos, color.Value);

        var lbl = HwatuUI.MakeLabel(fieldArea.parent, burstPos,
                             new Vector2(600f, 100f), 52f, color.Value);
        lbl.text = label;
        lbl.fontStyle = FontStyles.Bold;
        lbl.raycastTarget = false;
        StartCoroutine(ActionPopupAnim(lbl));
    }

    IEnumerator ActionPopupAnim(TextMeshProUGUI lbl)
    {
        var rt = lbl.rectTransform;
        const float popDur = 0.18f, holdDur = 0.35f, fadeDur = 0.35f;
        float t = 0f;
        while (t < popDur && rt != null)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / popDur);
            float s = Mathf.Lerp(0.4f, 1.15f, 1f - (1f - p) * (1f - p));
            rt.localScale = new Vector3(s, s, 1f);
            yield return null;
        }
        if (rt == null) yield break;
        rt.localScale = Vector3.one * 1.15f;

        yield return new WaitForSeconds(holdDur);
        if (rt == null) yield break;

        t = 0f;
        Color c0 = lbl.color;
        while (t < fadeDur && rt != null)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / fadeDur);
            float s = Mathf.Lerp(1.15f, 1.4f, p);
            rt.localScale = new Vector3(s, s, 1f);
            lbl.color = new Color(c0.r, c0.g, c0.b, Mathf.Lerp(c0.a, 0f, p));
            yield return null;
        }
        if (rt != null) Destroy(rt.gameObject);
    }

    /// <summary>
    /// 폭탄 크레딧을 손패 안의 "빈 카드"로 보여준다 — 카드 뒷면과 같은 톤(금테 +
    /// 점무늬)에 남은 횟수를 크게 얹어서, 진짜 손패 한 장을 고르는 것처럼
    /// 자연스럽게 낄 수 있게 한다.
    /// </summary>
    void MakeBombSkipSlot(Transform parent, Vector2 pos)
    {
        var go = new GameObject("BombSkip", typeof(RectTransform));
        go.transform.SetParent(parent, false);
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

        var capLabel = HwatuUI.MakeLabel(go.transform, new Vector2(0f, -16f), new Vector2(HAND_W - 8f, 22f), 13f, new Color(1, 1, 1, 0.9f));
        capLabel.text = "덱만";
        var numLabel = HwatuUI.MakeLabel(go.transform, new Vector2(0f, -HAND_H * 0.5f - 4f), new Vector2(HAND_W - 8f, 64f), 34f, Color.white);
        numLabel.text = playerBombCredits.ToString();
        numLabel.fontStyle = FontStyles.Bold;

        var btn = go.AddComponent<Button>();
        btn.targetGraphic = frame;
        btn.onClick.AddListener(OnPlayerBombSkip);
    }

}
