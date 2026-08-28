using UnityEngine;
using TMPro;
using DG.Tweening;

/// <summary>
/// 쪽/쓸/뻑/감사합니다/더 감사합니다처럼 "지금 뭐가 일어났는지"를 1초 정도
/// 큼직한 텍스트로 알려주는 이펙트 — 프리팹 5종(EffectJjok/EffectSweep/
/// EffectPpeok/EffectThanks/EffectThanksMore)이 전부 이 컴포넌트 하나를
/// 공유한다. 프리팹마다 <see cref="label"/>의 기본 텍스트·색을 다르게
/// 구워 두면 사용자가 Project 창에서 각각 열어 디자인만 따로 바꿀 수 있다
/// (구조/애니메이션은 코드가, 문구·색·배경 등은 프리팹이 담당).
///
/// 2026-08-18: "DOTween 적극 활용" 요청 — 예전 코루틴 기반 팝인/유지/페이드
/// (ActionPopupAnim)를 DOTween Sequence로 교체했다. 코루틴과 달리 씬 전환
/// 등으로 GameObject가 파괴돼도 트윈이 자동으로 정리된다(SetLink 불필요 —
/// DOTween은 대상 Transform이 없어지면 트윈을 자동으로 kill한다).
/// </summary>
public class GoStopEffectPopup : MonoBehaviour
{
    public RectTransform root;
    public TextMeshProUGUI label;
    public CanvasGroup group;

    /// <summary>팝인(0.4→1.15→1.0) → 유지 → 페이드아웃 후 자기 자신을 파괴한다.
    /// <paramref name="overrideText"/>/<paramref name="overrideColor"/>를
    /// 넘기면 프리팹 기본값 대신 그 값을 쓴다(호출부가 "뻑 먹기"처럼 상황별
    /// 문구를 넣어야 할 때 사용).</summary>
    public void Play(string overrideText = null, Color? overrideColor = null)
    {
        if (overrideText != null) label.text = overrideText;
        if (overrideColor.HasValue) label.color = overrideColor.Value;

        root.localScale = Vector3.one * 0.4f;
        group.alpha = 1f;

        const float popDur = 0.18f, holdDur = 0.35f, fadeDur = 0.35f;
        var seq = DOTween.Sequence();
        seq.Append(root.DOScale(1.15f, popDur).SetEase(Ease.OutBack));
        seq.Append(root.DOScale(1f, 0.08f).SetEase(Ease.InOutSine));
        seq.AppendInterval(holdDur);
        seq.Append(group.DOFade(0f, fadeDur));
        seq.Join(root.DOScale(1.3f, fadeDur).SetEase(Ease.InSine)); // 사라지며 살짝 더 커진다
        seq.OnComplete(() => { if (this != null) Destroy(gameObject); });
    }
}
