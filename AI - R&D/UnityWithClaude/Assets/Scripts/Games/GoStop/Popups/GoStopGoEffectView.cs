using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>2026-09-06(사용자 확인) — "GoEffect,StopEffect 코드에서 생성하는
/// 부분 없애고 다른 이펙트처럼 프리팹으로 만들어줘." 예전엔 `GoEffectSeq`가
/// 매번 `new GameObject(...)`로 라벨·링을 직접 조립했다 — 이 컴포넌트는
/// 그 구조(메인 라벨/서브 라벨/확장 링 2개)만 프리팹(<c>GoEffect.prefab</c>)
/// 안에 미리 담아 두고, 코드는 티어에 따라 텍스트·색·크기·활성 여부만
/// 채워 넣는다(<see cref="GoStop3PGame.GoEffectSeq"/> 참고) — 구조는 프리팹이,
/// 애니메이션 타이밍은 코드가 담당하는 이 프로젝트의 확립된 분담 원칙
/// (<see cref="GoStopEffectPopup"/>과 같은 원칙).
/// <br/>서브 라벨/링은 항상 프리팹에 존재하되(사용자가 Project 창에서
/// 언제든 색·폰트를 미리 볼 수 있도록) 담백한 티어(1~2고)에서는 코드가
/// <c>SetActive(false)</c>로 꺼둔다.</summary>
public class GoStopGoEffectView : MonoBehaviour
{
    public RectTransform root;
    public CanvasGroup group;
    public TextMeshProUGUI mainLabel;
    public TextMeshProUGUI subLabel;
    public RectTransform ring1;
    public RectTransform ring2;
    public Image ring1Image;
    public Image ring2Image;
    public CanvasGroup ring1Group;
    public CanvasGroup ring2Group;
}
