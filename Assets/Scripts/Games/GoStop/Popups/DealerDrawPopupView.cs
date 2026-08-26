using UnityEngine;
using TMPro;

/// <summary>선 뽑기 팝업(2~4인 공용) — 2026-08-26 재설계: 좌석마다 카드를
/// 미리 배정해두는 대신, 8장을 뒷면으로 깐 공용 풀(<see cref="pool"/>)에서
/// 좌석마다 한 장씩 순서대로 고르게 한다. 실제 카드 GameObject는 매판
/// 코드가 <see cref="pool"/> 밑에 직접 만들었다 지운다(장수·배치가 매번
/// 같아 프리팹에 고정 슬롯을 둬도 되지만, 뒤집힌 뒤 카드 자리에 그대로
/// 얼굴 있는 카드로 바꿔치기하는 게 더 간단해서 필드/손패처럼 동적으로
/// 그린다).</summary>
public class DealerDrawPopupView : MonoBehaviour
{
    public RectTransform dim;
    public RectTransform pool;
    public TextMeshProUGUI promptText;
    public TextMeshProUGUI resultText;

    public void Show() => dim.gameObject.SetActive(true);
    public void Hide() => dim.gameObject.SetActive(false);
}
