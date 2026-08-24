using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 좌석 정보 박스(닉네임/고+점수/금액/상태아이콘) 프리팹 뷰 —
/// <c>Assets/Resources/Prefabs/GoStop/UI/StatusBoxView.prefab</c>.
/// 2026-08-24 이전엔 <c>GoStop3PGame.BuildInfoBlock</c>이 매번 코드로
/// (<see cref="HwatuUI.MakeStatusBox"/> 등) 새로 조립했는데, 이제 이
/// 프리팹을 Instantiate해서 재사용한다 — 사용자가 프리팹을 열어 배경
/// 스프라이트·색·폰트 크기 등을 직접 바꿀 수 있다(팝업·이펙트 프리팹화와
/// 같은 이유).
///
/// 좌석마다 박스 폭이 다르다(내 정보=700, 상단=520, 좌우=400) — 프리팹
/// 자체는 폭을 고정하지 않고, <see cref="Configure"/>가 인스턴스마다
/// 자식들을 그 폭에 맞춰 다시 배치한다. 세로 배치(칸 높이·간격)는 폭과
/// 무관하게 고정이라 Configure가 안 건드린다 — 그 값을 바꾸고 싶으면
/// 프리팹을 직접 열어 <see cref="nameRect"/> 등의 sizeDelta.y를 조정하면
/// 된다(다만 그 경우 이 스크립트의 계산과 어긋나지 않도록 <see cref="NAME_H"/>
/// 등 상수도 같이 맞춰야 한다).
///
/// 2026-08-24(2차) — 배지(선/광박/멍박/피박/흔들기/뻑)도 이 프리팹 안에
/// 고정 슬롯으로 넣었다. 예전엔 <c>GoStop3PGame.DrawBadgeStrip</c>이 매턴
/// <c>HwatuUI.ClearChildren</c>으로 배지 영역을 통째로 지우고 <see
/// cref="GoStopIcons"/>로 새로 그렸다 — 카드처럼 "이번 판에 뭐가 나올지
/// 예측 불가능한" 콘텐츠가 아니라 "항상 정해진 6개 중 색/숫자만 바뀌는"
/// 콘텐츠라, 매턴 파괴·재생성할 이유가 없었다(디자인 편집도 안 됨 —
/// 프리팹을 열면 텅 빈 BadgeArea만 보였다). 지금은 6개 슬롯을 프리팹에
/// 미리 구워두고, <see cref="SetDealer"/>/<see cref="SetRisk"/>/
/// <see cref="SetCountBadge"/>로 상태만 갱신한다.
/// <br/><br/>
/// **레이아웃 단순화 — 선(先) 아이콘은 숨겨져도 자리를 계속 차지한다.**
/// 예전 동적 배치는 선이 없으면(딜러가 아니면) 광이 그 자리로 당겨져
/// 왔다("가변 개수를 순서대로 흘려 넣는" 방식) — 고정 슬롯에서는 이걸
/// 그대로 재현하려면 매턴 위치를 다시 계산해야 해서 "고정 슬롯" 취지와
/// 어긋난다. 대신 선 슬롯은 항상 같은 자리에 있고 <c>SetActive</c>로만
/// 껐다 켠다 — 딜러가 아닐 때 그 자리가 비어 보이지만(광이 안 당겨짐),
/// 아이콘이 매턴 위치를 옮겨 다니지 않아 오히려 더 안정적으로 읽힌다.
/// </summary>
public class GoStopStatusBoxView : MonoBehaviour
{
    [SerializeField] Image background;
    [SerializeField] RectTransform nameRect;
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] RectTransform goScoreRect;
    [SerializeField] TextMeshProUGUI goScoreText;
    [SerializeField] RectTransform moneyChipRect;
    [SerializeField] RectTransform moneyIconRect;
    [SerializeField] TextMeshProUGUI moneyText;
    [SerializeField] RectTransform badgeArea;

    [Header("배지 — 매턴 상태만 갱신(재생성 안 함)")]
    [SerializeField] RectTransform dealerIcon;          // "선"(항상 같은 자리, SetActive로만 표시)
    [SerializeField] Image[] riskIconBg = new Image[3];       // [0]=광 [1]=멍 [2]=피
    [SerializeField] TextMeshProUGUI[] riskIconFg = new TextMeshProUGUI[3];
    [SerializeField] RectTransform shakeBadge;
    [SerializeField] TextMeshProUGUI shakeBadgeLabel;
    [SerializeField] Image[] shakeDots = new Image[2];
    [SerializeField] RectTransform ppeokBadge;
    [SerializeField] TextMeshProUGUI ppeokBadgeLabel;
    [SerializeField] Image[] ppeokDots = new Image[2];

    public Image Background => background;
    public TextMeshProUGUI NameText => nameText;
    public TextMeshProUGUI GoScoreText => goScoreText;
    public TextMeshProUGUI MoneyText => moneyText;
    public RectTransform BadgeArea => badgeArea;

    // GoStop3PGame.BuildInfoBlock이 예전에 쓰던 것과 동일한 상수 —
    // 세로 배치(칸 높이·간격)는 폭과 무관하게 고정이다.
    const float NAME_H = 32f, GOSCORE_H = 28f, MONEY_H = 32f, GAP = 5f;
    public const float TotalHeight = NAME_H + GOSCORE_H + MONEY_H + GAP * 2f;

    // 배지 배치 — GoStop3PGame.DrawBadgeStrip이 예전에 쓰던 것과 동일한
    // 상수(BADGE_SIZE=34, STEP=BADGE_SIZE+6). 흔듬/뻑 카운트 배지 폭은
    // GoStopIcons.MakeCountBadge의 고정 공식(labelW=52, pad=4, dotSize=13,
    // dotGap=4, maxCount=2)에서 그대로 가져왔다.
    const float BADGE_SIZE = 34f;
    const float BADGE_STEP = BADGE_SIZE + 6f;
    const float COUNT_BADGE_W = 52f + 4f + 2 * 13f + 1 * 4f; // 86

    /// <summary>이 박스의 폭만 다시 설정하고 내부 요소(이름/고점수/금액/
    /// 배지 6종)를 전부 그 폭에 맞춰 재배치한다. 박스 자신의 화면 위치
    /// (anchoredPosition)는 호출자가 별도로 설정한다 — 이 메서드는 오직
    /// "내용물이 담기는 폭"만 다룬다.</summary>
    public void Configure(float width)
    {
        var rt = (RectTransform)transform;
        rt.sizeDelta = new Vector2(width, TotalHeight);

        float halfW = width * 0.5f;
        float leftCenterX = -halfW * 0.5f - 4f;
        float rightCenterX = halfW * 0.5f + 4f;
        float leftWidth = halfW - 20f;

        SetRect(nameRect, leftCenterX, -7f, leftWidth, NAME_H);
        SetRect(goScoreRect, leftCenterX, -7f - (NAME_H + GAP), leftWidth, GOSCORE_H);

        float moneyY = -7f - (NAME_H + GAP) - (GOSCORE_H + GAP);
        var chipSize = moneyChipRect.sizeDelta;
        SetRect(moneyChipRect, leftCenterX, moneyY, leftWidth, chipSize.y);
        // 칩 안쪽 아이콘 크기는 고정이고(HwatuUI.BuildMoneyChip과 동일하게
        // 프리팹에 이미 구워둔 값), 라벨 폭만 칩 폭에 맞춰 다시 늘린다 —
        // 안 그러면 좁은 좌/우 슬롯에서 금액 텍스트가 잘리거나, 넓은 내
        // 정보 슬롯에서 라벨 폭이 낭비된다.
        float iconSize = moneyIconRect.sizeDelta.x;
        var labelRect = moneyText.rectTransform;
        labelRect.sizeDelta = new Vector2(leftWidth - iconSize - 8f, labelRect.sizeDelta.y);

        float badgeW = halfW - 12f;
        SetRect(badgeArea, rightCenterX, -7f, badgeW, TotalHeight);

        // 배지 6종 — badgeArea 로컬 좌표계 기준(badgeArea 자신이 top-center
        // pivot이라 로컬 (0,0)이 그 상단 중앙). 예전 DrawBadgeStrip의
        // Place() 누적 좌표 공식을 그대로 고정 슬롯 위치로 옮겼다.
        float startX = -badgeW * 0.5f + BADGE_SIZE * 0.5f;
        dealerIcon.anchoredPosition = new Vector2(startX, 0f);
        for (int i = 0; i < 3; i++)
            riskIconBg[i].rectTransform.anchoredPosition = new Vector2(startX + BADGE_STEP * (i + 1), 0f);
        float row2Y = -BADGE_STEP;
        shakeBadge.anchoredPosition = new Vector2(startX, row2Y);
        ppeokBadge.anchoredPosition = new Vector2(startX + COUNT_BADGE_W + 8f, row2Y);
    }

    /// <summary>선(딜러) 여부 — 슬롯 자체는 항상 같은 자리, 표시만 껐다 켠다.</summary>
    public void SetDealer(bool isDealer) => dealerIcon.gameObject.SetActive(isDealer);

    /// <summary>이 좌석이 이번 판 배지 표시 대상이 아닐 때(쉬는 좌석, 빈
    /// 슬롯) 전부 꺼진 상태로 되돌린다. 예전엔 배지 영역 자체를
    /// ClearChildren으로 지워서 자동으로 해결됐는데, 지금은 슬롯이
    /// 영구적이라 명시적으로 리셋해야 지난 좌석의 상태가 남아있지 않는다.</summary>
    public void HideAllBadges()
    {
        SetDealer(false);
        // active=false면 SetRisk가 DimBg/DimFg를 쓰므로 나머지 인자는 안 쓰인다.
        SetRisk(0, false, Color.white, Color.white);
        SetRisk(1, false, Color.white, Color.white);
        SetRisk(2, false, Color.white, Color.white);
        SetCountBadge(true, 0, Color.white);
        SetCountBadge(false, 0, Color.white);
    }

    /// <summary>광박/멍박/피박(index 0/1/2) 위험 표시 — 위험하면 진한 색+흰
    /// 글자, 아니면 표면색 배경+반투명 글자(<see cref="DimBg"/>/<see
    /// cref="DimFg"/>).</summary>
    public void SetRisk(int index, bool active, Color activeBg, Color activeFg)
    {
        riskIconBg[index].color = active ? activeBg : DimBg;
        riskIconFg[index].color = active ? activeFg : DimFg;
    }

    /// <summary>흔들기/뻑 카운트 배지 — 점 <paramref name="count"/>개를
    /// <paramref name="dotColor"/>로 채우고 나머지는 흐리게 남긴다.</summary>
    public void SetCountBadge(bool isShake, int count, Color dotColor)
    {
        var dots = isShake ? shakeDots : ppeokDots;
        for (int i = 0; i < dots.Length; i++)
            dots[i].color = i < count ? dotColor : new Color(1f, 1f, 1f, 0.25f);
    }

    // 꺼진 상태 공통 색 — GoStop3PGame.BadgeDimBg/BadgeDimFg와 동일한 값
    // (그 상수들은 이제 이 컴포넌트가 대신 들고 있다).
    public static readonly Color DimBg = new Color(0.106f, 0.133f, 0.267f, 0.95f); // #1B2244 계열 — B안 표면색
    public static readonly Color DimFg = new Color(1f, 1f, 1f, 0.62f);

    static void SetRect(RectTransform r, float x, float y, float w, float h)
    {
        r.anchoredPosition = new Vector2(x, y);
        r.sizeDelta = new Vector2(w, h);
    }
}
