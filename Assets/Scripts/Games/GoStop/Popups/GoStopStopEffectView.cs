using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>2026-09-06(사용자 확인) — GoEffect와 같은 이유로 프리팹화된
/// 스톱 이펙트(<c>StopEffect.prefab</c>) 뼈대. 손 아이콘(<see
/// cref="GoStopComboIcons.StopHand"/>)과 "OO 스톱!" 라벨을 담고, 코드는
/// 텍스트·애니메이션만 채운다(<see cref="GoStop3PGame.StopEffectSeq"/>).</summary>
public class GoStopStopEffectView : MonoBehaviour
{
    public RectTransform root;
    public CanvasGroup group;
    public TextMeshProUGUI label;
    public RectTransform icon;
    public Image iconImage;
}
