using UnityEngine;
using TMPro;

/// <summary>선 뽑기 연출 팝업(4인 전용) — 좌석 4개 자리에 카드를 한 장씩 순서대로
/// 공개하고 결과 문구를 보여준다. 좌석 수(SEATS)가 이 프로젝트에서 4로 고정돼
/// 있어(3인 확장은 4인 딜에서 한 명이 쉬는 방식으로 흡수) 슬롯도 4개 고정
/// 배열이다.</summary>
public class DealerDrawPopupView : MonoBehaviour
{
    public RectTransform dim;
    public RectTransform[] cardSlots = new RectTransform[4];
    public TextMeshProUGUI[] seatLabels = new TextMeshProUGUI[4];
    public TextMeshProUGUI resultText;

    public void Show() => dim.gameObject.SetActive(true);
    public void Hide() => dim.gameObject.SetActive(false);
}
