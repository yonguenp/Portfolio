using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Coffee.UIEffects;
using Coffee.UIExtensions;

/// <summary>
/// UIEffect(mob-sakai/UIEffect)·ParticleEffectForUGUI(mob-sakai/ParticleEffectForUGUI)
/// 두 라이브러리를 쓰는 공용 그래픽 폴리시 헬퍼. <see cref="GoStop3PGame"/>
/// (2~4인 전부)이 쓴다 — <see cref="HwatuUI"/>와 같은 원칙: 카드 그리기처럼
/// 여러 곳에서 반복되는 시각 처리를 한 곳에 모아두면, 나중에 톤을 바꿀 때
/// 여기 한 곳만 고치면 된다.
///
/// <b>왜 UIEffect인가:</b> 이 프로젝트는 UGUI Image/TMP를 전부 코드로 직접
/// 만든다 — 셰이더를 새로 작성하지 않고 "컴포넌트 하나 붙이고 프로퍼티만
/// 설정"으로 드롭섀도·샤이니 엣지 같은 효과를 낼 수 있는 UIEffect가 이
/// 방식과 정확히 맞는다. <b>왜 ParticleEffectForUGUI(UIParticle)인가:</b>
/// 기존 <see cref="GoStopIcons.SpawnBurst"/>는 개별 Image+코루틴으로 만든
/// 절차적 버스트라 이미 실전 검증된 안정적인 경로다 — 그 경로를 건드리는
/// 대신, UIParticle은 <b>새로운</b> 이펙트(판돈 이동 스파클)에만 additive로
/// 얹어서 회귀 위험을 최소화했다.
/// </summary>
public static class GoStopFX
{
    /// <summary>카드 이미지에 은은한 드롭섀도를 상시 건다 — 평평한 카드
    /// 이미지가 살짝 뜬 것처럼 보이게 하는 효과. 매 프레임 색을 덮어쓰는
    /// 로직이 없는 정적 Image라 한 번만 설정해두면 계속 유지된다.</summary>
    public static void ApplyCardShadow(Graphic g)
    {
        if (g == null) return;
        var fx = g.gameObject.AddComponent<UIEffect>();
        fx.shadowMode = ShadowMode.Shadow;
        fx.shadowDistance = new Vector2(3f, -5f);
        fx.shadowColor = new Color(0f, 0f, 0f, 0.55f);
        fx.shadowBlurIntensity = 0.4f;
    }

    /// <summary>2026-09-03 — 카드 "Art" 자식에 사용자가 미리 심어둔 그림자
    /// UIEffect(<c>CardFront.prefab</c>, 기본 비활성)를 켜고 끈다. "패가
    /// 놓여있다"는 표현이 목적이라, 날아다니는 도중(SlamIn/SlamDown 진행
    /// 중)이 아니라 <b>착지 애니메이션이 완전히 끝난 뒤</b>에만 켠다 —
    /// <see cref="GoStop3PGame.DrawField"/>/<c>FillCapZone</c>의 정적
    /// 카드(이번 리빌드에서 안 움직이는 카드)는 즉시, 움직이는 카드는
    /// <c>FlyAndPunch</c> 코루틴이 끝나는 시점에 호출한다.</summary>
    public static void SetArtShadow(GameObject cardGo, bool on)
    {
        if (cardGo == null) return;
        var art = cardGo.transform.Find("Art");
        if (art == null) return;
        var fx = art.GetComponent<UIEffect>();
        if (fx != null) fx.enabled = on;
    }

    /// <summary>하이라이트 링(낼 수 있는 패, 조준 타겟, 필드 선택 후보 등)에
    /// 자동 반복 샤이니 스윕을 건다 — <c>edgeShinyAutoPlaySpeed</c> 하나면
    /// 코루틴 없이 계속 훑고 지나간다. 정적 금색 링보다 "지금 여기 주목"이라는
    /// 신호가 훨씬 강해진다.</summary>
    public static void ApplyShinyEdge(Graphic g)
    {
        if (g == null) return;
        var fx = g.gameObject.AddComponent<UIEffect>();
        fx.edgeMode = EdgeMode.Shiny;
        fx.edgeColor = new Color(1f, 1f, 1f, 0.95f);
        fx.edgeShinyWidth = 0.55f;
        fx.edgeShinyAutoPlaySpeed = 1.0f;
    }

    /// <summary>
    /// 승리 순간에 터뜨리는 색종이 폭죽 — ParticleEffectForUGUI(UIParticle)로
    /// 만든 첫 실제 파티클 이펙트다. 색은 "강조색은 하나" 원칙(UI 디자인
    /// 시스템 문서 참고)에 맞춰 골드·화이트 두 톤만 섞는다.
    /// <br/>
    /// <b>스케일 값은 감으로 잡지 않았다.</b> UIParticle의 기본
    /// <c>scale3D=(10,10,10)</c>이 파티클 시뮬레이션 좌표(월드 단위)를
    /// 캔버스 픽셀로 환산하는 배율이라, startSpeed 같은 값을 감으로
    /// 넣으면 화면에 아예 안 보이거나(너무 작음) 순식간에 튕겨나가
    /// (너무 큼) 버릴 수 있다 — 스크린샷이 이 환경에서 신뢰할 수 없다는
    /// 이 프로젝트의 기존 제약 때문에 육안 확인도 불가능하다. 그래서
    /// Play 모드에서 <c>ParticleSystem.GetParticles()</c>로 시뮬레이션
    /// 0.5초 뒤 실제 로컬 좌표 범위를 재고(±20 유닛), 거기에 스케일(10)을
    /// 곱한 값(±200px 안팎)이 1080px 폭 캔버스에서 적당히 넓게 퍼지는
    /// 것을 확인한 뒤에 아래 수치(startSpeed=70, gravityModifier=0.7)를
    /// 확정했다 — 이 프로젝트가 스크린샷 대신 좌표 실측으로 검증해온
    /// 방식을 파티클에도 그대로 적용한 것.
    /// </summary>
    public static void PlayWinConfetti(RectTransform stableParent, Vector2 localPos)
    {
        if (stableParent == null) return;

        var host = new GameObject("Confetti", typeof(RectTransform));
        host.transform.SetParent(stableParent, false);
        var hostRT = host.GetComponent<RectTransform>();
        hostRT.anchorMin = hostRT.anchorMax = new Vector2(0.5f, 1f);
        hostRT.pivot = new Vector2(0.5f, 1f);
        hostRT.anchoredPosition = localPos;
        hostRT.sizeDelta = new Vector2(10f, 10f);

        var uip = host.AddComponent<UIParticle>();

        var psGo = new GameObject("PS", typeof(ParticleSystem));
        psGo.transform.SetParent(host.transform, false);
        var ps = psGo.GetComponent<ParticleSystem>();
        // ParticleSystem은 playOnAwake 기본값이 true라 컴포넌트가 붙는 순간
        // 이미 재생 중인 상태다 — 그 상태에서 duration을 바꾸려 하면
        // "Setting the duration while system is still playing is not
        // supported" 경고가 뜬다(실측으로 확인). 설정을 만지기 전에 먼저
        // 멈춰야 한다.
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var main = ps.main;
        main.playOnAwake = false;
        main.loop = false;
        main.duration = 1f;
        main.startLifetime = 1.1f;
        main.startSpeed = 70f;
        main.startSize = new ParticleSystem.MinMaxCurve(0.6f, 1.1f);
        main.gravityModifier = 0.7f;
        main.simulationSpace = ParticleSystemSimulationSpace.Local;
        main.maxParticles = 60;
        main.startColor = new ParticleSystem.MinMaxGradient(new Color(1f, 0.82f, 0.25f), Color.white);

        var emission = ps.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 44) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 45f;
        shape.radius = 0.05f;

        var rot = ps.rotationOverLifetime;
        rot.enabled = true;
        rot.z = new ParticleSystem.MinMaxCurve(-180f, 180f);

        uip.RefreshParticles();
        ps.Play();

        var cleanup = host.AddComponent<GoStopFXCleanup>();
        cleanup.ScheduleDestroy(host, 1f /*main.duration*/ + 1.1f /*startLifetime*/ + 0.3f);
    }

    /// <summary>
    /// 판돈이 오갈 때 동전 하나가 낸 쪽에서 받는 쪽으로 포물선을 그리며
    /// 날아가는 연출 — "돈이 그냥 숫자만 바뀌고 빠져나가는 느낌이 없다"는
    /// 요청으로 추가했다. 도착하면 스파클 버스트 + "+N원" 플로터가 뜬다.
    /// <br/>
    /// <see cref="GoStopParticle"/>과 같은 원칙으로 자기 완결형 컴포넌트
    /// (<see cref="GoStopMoneyFly"/>)를 만들어 코루틴을 걸어둔다 — 호출한
    /// MonoBehaviour(게임 턴 코루틴)의 생명주기와 무관하게 안전하게 끝까지
    /// 돈다. <paramref name="stableParent"/>는 RebuildUI가 절대 지우지 않는
    /// 컨테이너(양쪽 게임 다 <c>ui.ContentArea</c>)여야 한다 — 그렇지 않으면
    /// 애니메이션 도중 필드/캡처 컨테이너가 통째로 갈아엎이면서 대상이
    /// 파괴될 수 있다(이 프로젝트가 겪은 DOTween 충돌 버그와 같은 계열의
    /// 함정 — 그래서 여기서도 코루틴 기반 + 매 프레임 null 체크를 쓴다).
    /// </summary>
    public static void FlyMoney(RectTransform stableParent, Vector3 fromWorld, Vector3 toWorld, int amount)
    {
        if (stableParent == null || amount <= 0) return;

        var go = new GameObject("MoneyFly", typeof(RectTransform));
        go.transform.SetParent(stableParent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(30f, 30f);
        rt.position = fromWorld;

        var img = go.AddComponent<Image>();
        var dollarSprite = Resources.Load<Sprite>("UI/KenneyBoard/dollar");
        img.sprite = dollarSprite != null ? dollarSprite : HwatuShapes.CoinIcon();
        img.color = new Color(1f, 1f, 1f, 0f); // 첫 프레임은 투명 — 코루틴이 페이드인
        img.raycastTarget = false;
        ApplyShinyEdge(img); // 날아가는 동안 계속 반짝이게

        var runner = go.AddComponent<GoStopMoneyFly>();
        runner.Animate(rt, img, fromWorld, toWorld, amount, stableParent);
    }

    /// <summary>게임 시작 딜링 연출 한 장 — 더미 자리에서 목적지(손패/필드)로
    /// 짧게 날아가 살짝 튕기고 줄어들며 사라진다. 실제 게임 상태는 이미
    /// 다 채워진 뒤라(<see cref="GoStop3PGame.NewGameSeq"/> 참고) 이 카드는
    /// 순수하게 시각적인 뒷면 카드일 뿐이다 — 자리를 잡는 순간 사라지고,
    /// 뒤이은 RebuildUI가 진짜 카드를 그 자리에 그린다.</summary>
    public static void FlyDealCard(RectTransform parent, Vector3 fromWorld, Vector3 toWorld, float w, float h)
    {
        if (parent == null) return;
        var rt = HwatuUI.MakeCardBack(parent, Vector2.zero, w, h);
        rt.position = fromWorld;
        var runner = rt.gameObject.AddComponent<GoStopDealingCard>();
        runner.Animate(rt, fromWorld, toWorld);
    }
}

/// <summary><see cref="GoStopFX.FlyDealCard"/> 한 장을 움직이는 자기 완결형
/// 컴포넌트 — 위 컴포넌트들과 같은 안전 패턴.</summary>
public class GoStopDealingCard : MonoBehaviour
{
    public void Animate(RectTransform rt, Vector3 from, Vector3 to) => StartCoroutine(Run(rt, from, to));

    IEnumerator Run(RectTransform rt, Vector3 from, Vector3 to)
    {
        const float flyDur = 0.22f;
        float t = 0f;
        while (t < flyDur)
        {
            if (rt == null) yield break;
            t += Time.deltaTime;
            float p = 1f - Mathf.Pow(1f - Mathf.Clamp01(t / flyDur), 3f); // ease-out
            rt.position = Vector3.Lerp(from, to, p);
            yield return null;
        }
        if (rt == null) yield break;
        rt.position = to;

        // 도착하면 살짝 튕겼다 줄어들며 사라진다 — 실제 카드가 그 자리에
        // 바로 이어서 나타나므로(RebuildUI) 길게 끌 필요가 없다.
        const float settleDur = 0.10f;
        Vector3 baseScale = rt.localScale;
        t = 0f;
        while (t < settleDur)
        {
            if (rt == null) yield break;
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / settleDur);
            float s = p < 0.4f ? Mathf.Lerp(1f, 1.15f, p / 0.4f) : Mathf.Lerp(1.15f, 0f, (p - 0.4f) / 0.6f);
            rt.localScale = baseScale * s;
            yield return null;
        }
        if (rt != null) Destroy(rt.gameObject);
    }
}

/// <summary>일회성 파티클(<see cref="GoStopFX.PlayWinConfetti"/>)이 재생을
/// 끝낸 뒤 스스로 사라지게 하는 타이머 — 대상이 이미 사라졌으면 조용히
/// 넘어간다(다른 GoStopFX 컴포넌트와 같은 안전 패턴).</summary>
public class GoStopFXCleanup : MonoBehaviour
{
    public void ScheduleDestroy(GameObject target, float delay) => StartCoroutine(Run(target, delay));

    IEnumerator Run(GameObject target, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (target != null) Destroy(target);
    }
}

/// <summary><see cref="GoStopFX.FlyMoney"/> 동전 한 개를 움직이는 자기 완결형
/// 컴포넌트 — <see cref="GoStopParticle"/>과 동일한 안전 패턴(자기 코루틴,
/// 매 프레임 null 체크, 대상이 사라지면 예외 없이 조용히 멈춤).</summary>
public class GoStopMoneyFly : MonoBehaviour
{
    public void Animate(RectTransform rt, Image img, Vector3 from, Vector3 to, int amount, RectTransform stableParent)
        => StartCoroutine(Run(rt, img, from, to, amount, stableParent));

    IEnumerator Run(RectTransform rt, Image img, Vector3 from, Vector3 to, int amount, RectTransform stableParent)
    {
        const float dur = 0.55f;
        // 포물선처럼 보이게 중간 지점을 위로 띄운다.
        Vector3 mid = Vector3.Lerp(from, to, 0.5f) + new Vector3(0f, 70f, 0f);

        float t = 0f;
        while (t < dur)
        {
            if (rt == null || img == null) yield break; // 도중에 부모가 지워졌으면 조용히 멈춘다
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / dur);
            float ease = p * p * (3f - 2f * p); // smoothstep
            Vector3 a = Vector3.Lerp(from, mid, ease);
            Vector3 b = Vector3.Lerp(mid, to, ease);
            rt.position = Vector3.Lerp(a, b, ease);
            rt.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(0f, 540f, p));

            float fadeIn = Mathf.Clamp01(t / 0.12f);
            float fadeOut = 1f - Mathf.Clamp01((p - 0.82f) / 0.18f);
            var c = img.color;
            c.a = Mathf.Min(fadeIn, fadeOut);
            img.color = c;
            yield return null;
        }
        if (rt == null) yield break;

        if (stableParent != null)
        {
            Vector2 localPos = stableParent.InverseTransformPoint(to);
            GoStopIcons.SpawnBurst(stableParent, localPos, new Color(1f, 0.85f, 0.3f), count: 6);
            SpawnFloatText(stableParent, localPos, amount);
        }
        Destroy(gameObject);
    }

    static void SpawnFloatText(RectTransform parent, Vector2 localPos, int amount)
    {
        var lbl = HwatuUI.MakeLabel(parent, localPos + new Vector2(0f, 6f), new Vector2(240f, 44f), 24f,
                                     new Color(1f, 0.85f, 0.3f));
        lbl.text = $"+{amount:N0}원";
        lbl.font = HwatuTheme.FontBold;
        lbl.raycastTarget = false;
        var runner = lbl.gameObject.AddComponent<GoStopFloatText>();
        runner.Animate(lbl.rectTransform, lbl);
    }
}

/// <summary>"+N원" 같은 플로터 텍스트가 위로 떠오르며 페이드아웃하는 짧은
/// 연출 — 위와 같은 자기 완결형 안전 패턴.</summary>
public class GoStopFloatText : MonoBehaviour
{
    public void Animate(RectTransform rt, TextMeshProUGUI tmp) => StartCoroutine(Run(rt, tmp));

    IEnumerator Run(RectTransform rt, TextMeshProUGUI tmp)
    {
        const float dur = 0.7f;
        Vector2 start = rt.anchoredPosition;
        Vector2 end = start + new Vector2(0f, 46f);
        float t = 0f;
        while (t < dur)
        {
            if (rt == null || tmp == null) yield break;
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / dur);
            rt.anchoredPosition = Vector2.Lerp(start, end, 1f - (1f - p) * (1f - p));
            var c = tmp.color;
            c.a = 1f - Mathf.Clamp01((p - 0.4f) / 0.6f);
            tmp.color = c;
            yield return null;
        }
        if (rt != null) Destroy(rt.gameObject);
    }
}
