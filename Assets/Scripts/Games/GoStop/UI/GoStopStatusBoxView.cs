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

    public Image Background => background;
    public TextMeshProUGUI NameText => nameText;
    public TextMeshProUGUI GoScoreText => goScoreText;
    public TextMeshProUGUI MoneyText => moneyText;
    public RectTransform BadgeArea => badgeArea;

    // GoStop3PGame.BuildInfoBlock이 예전에 쓰던 것과 동일한 상수 —
    // 세로 배치(칸 높이·간격)는 폭과 무관하게 고정이다.
    const float NAME_H = 32f, GOSCORE_H = 28f, MONEY_H = 32f, GAP = 5f;
    public const float TotalHeight = NAME_H + GOSCORE_H + MONEY_H + GAP * 2f;

    /// <summary>이 박스의 폭만 다시 설정하고 내부 요소(이름/고점수/금액/
    /// 배지 영역)를 전부 그 폭에 맞춰 재배치한다. 박스 자신의 화면 위치
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

        SetRect(badgeArea, rightCenterX, -7f, halfW - 12f, TotalHeight);
    }

    static void SetRect(RectTransform r, float x, float y, float w, float h)
    {
        r.anchoredPosition = new Vector2(x, y);
        r.sizeDelta = new Vector2(w, h);
    }
}
