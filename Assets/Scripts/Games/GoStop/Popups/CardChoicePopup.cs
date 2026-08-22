using UnityEngine;

/// <summary>
/// 딤 + Kenney 헤더바 패널 + 빈 카드 컨테이너로 구성된 팝업. 필드에 같은 달이
/// 2장 있을 때 어느 걸 가져올지 고르는 화면에서 쓴다(2인/4인 공유 — 규칙이
/// 같아 구조도 완전히 같다). 후보 카드는 상황마다 개수·종류가 달라 프리팹에
/// 못 구워두고 <see cref="cardContainer"/> 밑에 게임 스크립트가 매번
/// <c>HwatuUI.MakeCard</c>로 채워 넣는다.
/// </summary>
public class CardChoicePopup : MonoBehaviour
{
    public RectTransform dim;
    public RectTransform cardContainer;

    public void Show() => dim.gameObject.SetActive(true);
    public void Hide() => dim.gameObject.SetActive(false);
}
