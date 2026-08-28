using UnityEngine;
using TMPro;

/// <summary>광팔이 결과 팝업(4인 전용) — 판 카드 실물 + 총액·지불자 문구.
/// 헤더 제목 자체를 "{좌석} 광팔이!"로 매번 갈아끼우므로 헤더의
/// TextMeshProUGUI도 노출한다(다른 팝업들은 제목이 고정이라 노출할 필요가
/// 없었다).</summary>
public class GwangSalePopupView : MonoBehaviour
{
    public RectTransform dim;
    public TextMeshProUGUI titleText;
    public RectTransform cardRow;
    public TextMeshProUGUI amountText;
    public TextMeshProUGUI payerText;

    public void Show() => dim.gameObject.SetActive(true);
    public void Hide() => dim.gameObject.SetActive(false);
}
