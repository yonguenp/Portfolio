using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

/// <summary>2026-09-06(사용자 확인) — "겹쳐있는 패는 유저가 눌렀을때 겹친패를
/// 확인할 수 있게, 마우스오버나 터치가되면 배경이 dim되있는 툴팁으로 패들이
/// 보이게 해줘. 마우스포인터나 터치 위치 상단에 리스팅되어야할듯." 필드에
/// 같은 달 카드가 2장 이상 겹쳐 쌓인 슬롯(<see cref="GoStop3PGame.FieldStackStep"/>
/// 참고 — 겹쳐서 몇 장인지 어렴풋이는 보이지만 정확히 뭐가 깔려있는지는
/// 맨 위 카드에 가려 안 보인다) 위를 누르고 있는 동안, 화면을 어둡게 깔고
/// 그 슬롯의 카드 전부를 실물 크기로 나열해 보여준다. 손을 떼거나(또는
/// 포인터/손가락이 트리거 영역을 벗어나면) 즉시 닫힌다 — 별도의 닫기
/// 버튼이 필요 없는 "누르고 있는 동안만" 방식(마우스/터치를 굳이 구분할
/// 필요가 없다는 장점도 있다).
/// <br/>
/// 다른 GoStop 싱글턴 UI(<see cref="GoStopVectorEffect"/>/<see
/// cref="GoStopWindParticles"/>)와 같은 <c>Ensure()</c> 패턴 — 씬에 하나만
/// 만들고 재사용한다.</summary>
public class GoStopStackTooltip : MonoBehaviour
{
    public static GoStopStackTooltip Instance;

    RectTransform canvasRoot;
    GameObject dim;
    RectTransform panel;

    const float CARD_W = 76f, CARD_H = 124f;

    public static GoStopStackTooltip Ensure(RectTransform canvasRoot)
    {
        if (Instance != null) return Instance;
        var go = new GameObject("GoStopStackTooltip", typeof(RectTransform));
        go.transform.SetParent(canvasRoot, false);
        var rt = go.GetComponent<RectTransform>();
        // 캔버스 전체를 덮는 컨테이너 — 딤은 이 안에서 stretch로 꽉 채우고,
        // 패널은 이 rt의 중심(=canvasRoot의 중심, GoEffectSeq 등에서 이미
        // 확립된 "루트 Canvas의 로컬 원점 = 화면 중심" 전제와 동일)을
        // 기준으로 좌표를 계산한다.
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        rt.pivot = new Vector2(0.5f, 0.5f);

        Instance = go.AddComponent<GoStopStackTooltip>();
        Instance.canvasRoot = canvasRoot;
        Instance.BuildTree(rt);
        go.transform.SetAsLastSibling(); // Overlay·다른 팝업보다도 항상 위
        return Instance;
    }

    void BuildTree(RectTransform root)
    {
        dim = new GameObject("Dim", typeof(RectTransform));
        dim.transform.SetParent(root, false);
        var dimRt = dim.GetComponent<RectTransform>();
        dimRt.anchorMin = Vector2.zero; dimRt.anchorMax = Vector2.one;
        dimRt.offsetMin = dimRt.offsetMax = Vector2.zero;
        var dimImg = dim.AddComponent<Image>();
        dimImg.color = new Color(0f, 0f, 0f, 0.72f);
        // 손을 떼는 판정은 카드를 누르고 있는 트리거 오브젝트(GoStopStackHoverTrigger)가
        // 담당한다 — 딤 자신은 입력을 가로챌 필요가 없다(raycastTarget=false로
        // 두면 뒤에 있는 필드 카드가 계속 보이면서도 어두워진 느낌만 준다).
        dimImg.raycastTarget = false;

        var panelGo = new GameObject("Panel", typeof(RectTransform));
        panelGo.transform.SetParent(root, false);
        panel = panelGo.GetComponent<RectTransform>();
        panel.anchorMin = panel.anchorMax = new Vector2(0.5f, 0.5f);
        // pivot을 패널 "아래쪽 중앙"으로 둬서, anchoredPosition을 터치
        // 지점으로 잡으면 패널이 자연히 그 지점 위로 솟아오른다.
        panel.pivot = new Vector2(0.5f, 0f);
        var panelBg = panelGo.AddComponent<Image>();
        UISkin.Apply(panelBg, UISkin.PanelBody);
        panelBg.raycastTarget = false;
        var hlg = panelGo.AddComponent<HorizontalLayoutGroup>();
        hlg.childAlignment = TextAnchor.MiddleCenter;
        hlg.spacing = 6f;
        hlg.padding = new RectOffset(12, 12, 12, 12);
        hlg.childControlWidth = false;
        hlg.childControlHeight = false;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = false;

        dim.SetActive(false);
    }

    /// <param name="cards">겹쳐 쌓인 슬롯의 카드 전부(2장 이상).</param>
    /// <param name="screenPointerPos">터치/마우스의 현재 화면 좌표
    /// (<see cref="PointerEventData.position"/>) — 이 지점 바로 위에 뜬다.</param>
    public void Show(List<HwatuCard> cards, Vector2 screenPointerPos)
    {
        if (cards == null || cards.Count == 0) return;
        dim.SetActive(true);

        HwatuUI.ClearChildren(panel);
        // 월순 정렬 — 필드/획득패에서 이미 쓰는 관례와 같은 순서라 눈에 익다.
        foreach (var c in cards.OrderBy(c => c.month).ThenBy(c => (int)c.EffectiveKind))
            HwatuUI.MakeCard(c, panel, Vector2.zero, CARD_W, CARD_H, null, false);

        // 2026-09-06 버그 수정 — HorizontalLayoutGroup은 childControlWidth=false
        // 라 자식 배치만 계산할 뿐, 부모(panel) 자신의 sizeDelta는 절대 안
        // 건드린다(ContentSizeFitter 없이는). 그래서 panel.sizeDelta.x가
        // RectTransform 기본값 100에 그대로 멈춰 있어(라이브 테스트로 실제
        // 발견 — 카드 4장인데 폭이 100으로 나온 것을 보고 잡았다) 카드가
        // 몇 장이든 패널 폭이 안 늘어났다. 카드 수 기준으로 직접 계산한다.
        float spacing = 6f, padding = 12f;
        float width = cards.Count * CARD_W + Mathf.Max(0, cards.Count - 1) * spacing + padding * 2f;
        panel.sizeDelta = new Vector2(width, CARD_H + padding * 2f);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRoot, screenPointerPos, null, out var local);
        // 손가락/커서에 안 가리도록 24px 위로 띄운다.
        panel.anchoredPosition = local + new Vector2(0f, 24f);

        // 화면 가장자리를 벗어나지 않게 좌우로만 클램프 — 세로는 항상
        // 포인터 위로만 뜨므로(pivot 아래쪽) 위쪽이 남는 한 벗어날 일이
        // 드물지만, 좌우는 슬롯이 화면 끝에 가까우면 쉽게 넘친다.
        Canvas.ForceUpdateCanvases();
        float halfW = panel.rect.width * 0.5f;
        float canvasHalfW = canvasRoot.rect.width * 0.5f;
        float clampedX = Mathf.Clamp(panel.anchoredPosition.x, -canvasHalfW + halfW, canvasHalfW - halfW);
        panel.anchoredPosition = new Vector2(clampedX, panel.anchoredPosition.y);
    }

    public void Hide()
    {
        if (dim != null) dim.SetActive(false);
    }

    /// <summary>2026-09-06(사용자 확인) — "안 사라진다"는 재신고 조사 —
    /// PointerUp/Exit이 원래 눌렀던 트리거 오브젝트로만 배달되는데, 그
    /// 오브젝트가 눌려 있는 동안 자연 게임 진행(다른 좌석 턴 등)으로
    /// RebuildUI가 돌면 필드가 다시 그려지며 <see cref="GoStopStackHoverTrigger"/>
    /// 가 새 오브젝트로 교체된다 — 옛 오브젝트가 파괴돼 버리면 Up/Exit을
    /// 받을 대상 자체가 사라져 손을 떼도 영원히 안 닫힌다. 특정 오브젝트가
    /// 이벤트를 배달받는 것에 기대지 않고, **떠 있는 동안 매 프레임
    /// "지금 실제로 뭔가 눌려 있는가"를 직접 확인**하는 안전망을 추가한다 —
    /// 마우스도 터치도 아무것도 안 눌려 있으면 무조건 닫는다.</summary>
    void Update()
    {
        if (dim == null || !dim.activeSelf) return;

        bool anyPressed = Mouse.current != null && Mouse.current.leftButton.isPressed;
        if (!anyPressed && Touchscreen.current != null)
        {
            foreach (var t in Touchscreen.current.touches)
            {
                if (t.press.isPressed) { anyPressed = true; break; }
            }
        }
        if (!anyPressed) Hide();
    }
}

/// <summary>필드에서 카드가 여러 장 겹친 슬롯 하나를 통째로 덮는 투명
/// 오버레이에 붙는 트리거. 개별 카드가 아니라 슬롯 전체를 덮으므로, 맨
/// 위에 어떤 카드가 그려져 있든 그 슬롯 안 아무 곳이나 가리키면 똑같이
/// 전체 목록이 뜬다.
/// <br/>2026-09-06 1차 정정(사용자 확인) — "터치나 마우스오버시에 툴팁이
/// 올라왔다가 마우스오버해제나 터치아웃때 사라지는걸 생각했어"라는 요청으로
/// Down/Up→Enter/Exit(호버)로 바꿨었다.
/// <br/>2026-09-06 2차 정정(사용자 확인) — "마우스다운일때 띄우고
/// 마우스업이면 닫아줘, 터치도 다운/업으로"로 다시 방향이 바뀌어 Down/Up
/// (누르고 있는 동안만)으로 되돌렸다 — 마우스/터치 둘 다 Unity UGUI가
/// PointerDown/PointerUp을 동일하게 합성해주므로 별도 분기 없이 한 쌍의
/// 핸들러로 충분하다. Exit도 안전망으로 같이 둔다 — 손가락이 트리거
/// 영역을 벗어난 채로 뗄 때 UGUI가 원래 눌렀던 오브젝트에 Up을 그대로
/// 배달해주긴 하지만(포인터가 다른 곳으로 옮겨가도 "누른 대상"을 계속
/// 추적), 혹시라도 그 경로가 안 걸리는 예외적 상황에서도 확실히 닫히게
/// 이중으로 막아둔다.</summary>
public class GoStopStackHoverTrigger : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    List<HwatuCard> cards;
    RectTransform canvasRoot;

    public void Init(List<HwatuCard> cardsInSlot, RectTransform canvasRoot)
    {
        cards = cardsInSlot;
        this.canvasRoot = canvasRoot;
    }

    public void OnPointerDown(PointerEventData e) => GoStopStackTooltip.Ensure(canvasRoot).Show(cards, e.position);
    public void OnPointerUp(PointerEventData e) => GoStopStackTooltip.Instance?.Hide();
    public void OnPointerExit(PointerEventData e) => GoStopStackTooltip.Instance?.Hide();
}
