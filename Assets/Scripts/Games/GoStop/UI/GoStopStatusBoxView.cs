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
/// 2026-08-24(2차) — 배지(선/광박/멍박/피박/흔들기/뻑)도 이 프리팹 안에
/// 고정 슬롯으로 넣었다. 예전엔 <c>GoStop3PGame.DrawBadgeStrip</c>이 매턴
/// <c>HwatuUI.ClearChildren</c>으로 배지 영역을 통째로 지우고 <see
/// cref="GoStopIcons"/>로 새로 그렸다 — 카드처럼 "이번 판에 뭐가 나올지
/// 예측 불가능한" 콘텐츠가 아니라 "항상 정해진 6개 중 색/숫자만 바뀌는"
/// 콘텐츠라, 매턴 파괴·재생성할 이유가 없었다(디자인 편집도 안 됨 —
/// 프리팹을 열면 텅 빈 BadgeArea만 보였다). 지금은 6개 슬롯을 프리팹에
/// 미리 구워두고, <see cref="SetDealer"/>/<see cref="SetRisk"/>/
/// <see cref="SetCountBadge"/>로 상태만 갱신한다.
///
/// 2026-08-24(3차) — 사용자가 프리팹 내부를 직접 재설계해서 이름/고점수/
/// 금액/배지를 <c>Top</c>(이름+금액칩, 폭 전체 스트레치)/<c>Body</c>
/// (고점수+BadgeArea, HorizontalLayoutGroup)로 재구성하고, BadgeArea
/// 안의 <c>top</c>(선/광/멍/피)·<c>bot</c>(흔들기/뻑)에도 각각
/// HorizontalLayoutGroup을 얹어 자동 정렬되게 했다 — 이전 버전의
/// <see cref="Configure"/>가 모든 자식의 위치·크기를 코드로 직접
/// 재계산하고 있어서 이 새 레이아웃과 정면으로 충돌했다("코드에서
/// 포지션이나 크기를 다시 잡나봐" 신고로 발견). 지금은 루트 박스의
/// 폭만 바꾸고 나머지는 앵커·LayoutGroup에 맡긴다 — 이 프리팹 안의
/// 정확한 배치(칸 간격·정렬 등)를 바꾸고 싶으면 코드가 아니라 프리팹을
/// 열어 직접 조정할 것.
///
/// 2026-08-24(4차) — 기본/현재턴 배경·글자색도 SerializeField로 뺐다.
/// 예전엔 <c>GoStop3PGame.FillSlot</c>이 이 색들(#1B2244/#EDBA2E 등)을
/// 코드에 직접 박아 넣고 있었다 — 이제 <see cref="ApplyTurnState"/>가
/// 이 프리팹의 필드 값으로 배경·글자색을 정하므로, 프리팹을 열어 색만
/// 바꾸면 4개 좌석(상단/좌/우/하단) 전부에 반영된다.
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

    [Header("색상 — 2026-08-24: 기본/현재턴 배경·글자색을 프리팹에서 직접\n조정할 수 있게 SerializeField로 뺐다. 예전엔 GoStop3PGame.FillSlot이\n이 값들을 코드에 직접 박아 넣고 있었다(#1B2244/#EDBA2E 등) — 이제\n이 프리팹을 열어 색만 바꾸면 4개 좌석(상단/좌/우/하단) 전부에\n반영된다. 기본값은 기존 하드코딩 값과 동일하게 맞춰서 색을 아직\n안 바꾼 기존 씬은 시각적으로 그대로다.")]
    [SerializeField] Color normalBgColor = new Color(0.106f, 0.133f, 0.267f, 0.88f);       // #1B2244 계열 — B안 표면색
    [SerializeField] Color normalTextColor = Color.white;
    [SerializeField] Color highlightBgColor = new Color(0.929f, 0.729f, 0.180f, 0.95f);    // #EDBA2E — 강조색(현재 턴)
    [SerializeField] Color highlightTextColor = new Color(0.106f, 0.133f, 0.267f, 1f);     // 밝은 배경 위라 어두운 글자로 뒤집는다

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

    // GoStop3PGame.BuildInfoBlock이 예전에 쓰던 것과 같은 세로 예산값 —
    // 루트 박스 높이는 여전히 이 값으로 고정한다(폭과 무관).
    public const float TotalHeight = 102f;

    /// <summary>이 박스의 폭만 다시 설정한다. 2026-08-24(3차) — 사용자가
    /// 프리팹 안에 앵커 스트레치(Top/Body)+HorizontalLayoutGroup(Body·
    /// BadgeArea/top·BadgeArea/bot)로 직접 반응형 레이아웃을 구성해 뒀다
    /// (이전엔 이 메서드가 이름/고점수/금액/배지 6종의 위치·크기를 전부
    /// 코드로 다시 계산해서, 프리팹에서 사용자가 손으로 잡아둔 배치를
    /// 매번 덮어쓰는 문제가 있었다 — "코드에서 포지션이나 크기를 다시
    /// 잡나봐" 신고로 발견). 지금은 루트 폭만 바꾸고, 나머지는 앵커·
    /// LayoutGroup이 알아서 재배치하게 맡긴다 — 강제 리빌드만 걸어서
    /// 이번 프레임에 바로 반영되게 한다(안 걸면 다음 레이아웃 패스까지
    /// 옛 배치로 한 프레임 어긋나 보일 수 있다).</summary>
    public void Configure(float width)
    {
        var rt = (RectTransform)transform;
        rt.sizeDelta = new Vector2(width, TotalHeight);
        LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
    }

    /// <summary>선(딜러) 여부 — 슬롯 자체는 항상 같은 자리, 표시만 껐다 켠다.</summary>
    public void SetDealer(bool isDealer) => dealerIcon.gameObject.SetActive(isDealer);

    /// <summary>배경·이름/고점수/금액 글자색을 기본↔현재턴 강조 상태로
    /// 전환한다. 예전엔 <c>GoStop3PGame.FillSlot</c>이 이 네 색을 직접
    /// 골라 각 컴포넌트에 대입했는데, 이제 이 프리팹의 <see
    /// cref="normalBgColor"/> 등 필드로 색 자체를 디자인하고 이 메서드는
    /// "지금 어느 상태냐"만 전달받는다. 이름 라벨만 강조 시 볼드로
    /// 바뀐다(고점수/금액은 굵기 그대로) — 기존 동작과 동일.</summary>
    public void ApplyTurnState(bool highlight)
    {
        if (background) background.color = highlight ? highlightBgColor : normalBgColor;
        var textColor = highlight ? highlightTextColor : normalTextColor;
        if (nameText)
        {
            nameText.color = textColor;
            nameText.fontStyle = highlight ? FontStyles.Bold : FontStyles.Normal;
        }
        if (goScoreText) goScoreText.color = textColor;
        if (moneyText) moneyText.color = textColor;
    }

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
}
