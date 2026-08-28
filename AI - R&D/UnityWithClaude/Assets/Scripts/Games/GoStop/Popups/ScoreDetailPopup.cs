using UnityEngine;
using TMPro;

/// <summary>
/// 게임오버 오버레이의 "점수 상세" 버튼에서 여는 팝업 — 2인/4인 공유(레이아웃이
/// 완전히 같다). 요약 줄 → 항목별 점수+관여 카드 스크롤 목록 → 배수·최종
/// 점수 각주 3단으로 구성된다. 닫기 버튼(헤더 X + 하단 "닫기")은 항상 이
/// 팝업 스스로를 <see cref="Hide"/>하는 동작뿐이라 프리팹 저장 시점에 이미
/// persistent listener로 구워둔다 — 게임 스크립트가 매번 다시 연결할 필요가
/// 없다(닫기 동작은 게임 상태에 의존하지 않으므로).
/// </summary>
public class ScoreDetailPopup : MonoBehaviour
{
    public RectTransform dim;
    public TextMeshProUGUI summaryText;
    public RectTransform rowsContent;   // HwatuUI.MakeScrollBody가 만든 스크롤 콘텐츠
    public TextMeshProUGUI footerText;
    // 2026-08-18: 4인판 패자별 광박/멍박/피박 아이콘 줄 — footerText는 여러
    // 줄 자동 텍스트라 그 옆에 아이콘을 정확히 맞추기 어려워서 별도
    // 컨테이너를 둔다(footerText 바로 아래, 4인판 ShowScoreDetail 전용 —
    // 2인판은 패자가 하나뿐이라 이 필드를 안 쓴다, null이어도 무방).
    public RectTransform badgeStripArea;

    public void Show()
    {
        dim.gameObject.SetActive(true);
        dim.SetAsLastSibling(); // Overlay보다 항상 위에 뜨도록 방어적으로 보장
    }

    public void Hide() => dim.gameObject.SetActive(false);
}
