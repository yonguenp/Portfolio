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
        // 2026-09-13 버그 수정 — 예전엔 항상 AddComponent만 했는데, 이제
        // 이 함수를 카드 "Art" 자식(CardFront.prefab에 SetArtShadow용
        // UIEffect가 이미 심어져 있다)에도 쓰게 되면서(점수 상세의 광패
        // 하이라이트, 아이템10) UIEffect의 [DisallowMultipleComponent]에
        // 걸려 null을 돌려받는 경우가 생겼다 — 기존 컴포넌트를 재사용한다.
        // 하이라이트 링(원래 용도, 컴포넌트가 없는 대상)은 이 변경으로
        // 동작이 전혀 안 바뀐다(GetComponent가 null → 그대로 AddComponent).
        var fx = g.GetComponent<UIEffect>();
        if (fx == null) fx = g.gameObject.AddComponent<UIEffect>();
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
        // 2026-09-06 — "돈 변화 연출이 잘 안 보인다" 요청으로 30→44px 확대.
        rt.sizeDelta = new Vector2(44f, 44f);
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

    // ══════════════════════════════════════════════════════════════
    // 2026-09-13 — "UIEffect·UIParticle로 그래픽컬한 부분을 더 발전시켜
    // 달라"는 요청으로 브레인스토밍했던 12개 아이디어를 전부 적용한다.
    // 전부 기존 안정 경로(SpawnBurst 등)는 안 건드리고 additive로만 얹었다.
    // ══════════════════════════════════════════════════════════════

    /// <summary>아이템1 — 덱카드가 뒤집혀 드러나는 순간, 잠깐 디졸브(흩어진
    /// 입자가 모여 응집되는) 느낌을 얹는다. <see cref="SlamDown"/>이
    /// <c>FlipRevealBack</c>으로 뒷면을 걷어낸 직후(=앞면이 막 보이기
    /// 시작하는 시점) 호출한다 — 그 시점 기준으로 "역방향"(완전히 흩어진
    /// 상태 rate=1 → 정상 rate=0)으로 재생해야 "나타난다"는 인상이 된다.
    /// 대상(Art 이미지)을 못 찾으면 조용히 무시. 재생이 끝나면 UIEffect
    /// 자체를 지워서 Art를 다시 평범한 Image로 되돌린다(이 효과는 그
    /// 카드가 필드에 머무는 동안 계속 켜둘 이유가 없다).</summary>
    public static void PlayCardRevealDissolve(RectTransform cardRT)
    {
        if (cardRT == null) return;
        var art = cardRT.Find("Art");
        var g = art != null ? art.GetComponent<Graphic>() : null;
        if (g == null) return;
        // 2026-09-13 버그 수정("fx가 null" 런타임 에러) — CardFront.prefab의
        // Art 자식엔 SetArtShadow(2026-09-03)가 쓰는 UIEffect가 이미 심어져
        // 있다(기본 비활성, 그림자 토글용). UIEffect는 [DisallowMultipleComponent]
        // 라 여기서 또 AddComponent하면 Unity가 조용히 null을 돌려준다 — 그
        // null에 바로 필드를 대입해서 NRE가 났다. 기존 컴포넌트를 찾아
        // 재사용해야 한다 — transitionFilter는 shadowMode와 완전히 독립된
        // 채널이라 한 컴포넌트에 같이 둬도 서로 안 건드린다.
        var fx = g.GetComponent<UIEffect>();
        if (fx == null) fx = g.gameObject.AddComponent<UIEffect>();
        fx.transitionFilter = TransitionFilter.Dissolve;
        fx.transitionColor = new Color(1f, 0.85f, 0.35f, 1f); // 디졸브 경계 골드 글로우
        fx.transitionWidth = 0.25f;
        fx.transitionSoftness = 0.4f;
        fx.transitionRate = 1f; // 완전히 흩어진 상태에서 시작
        var runner = g.gameObject.AddComponent<GoStopTransitionRunner>();
        // destroyEffectWhenDone: false — 이 UIEffect는 SetArtShadow와 공유하는
        // 컴포넌트라 재생이 끝났다고 통째로 Destroy하면 안 된다(그러면 나중에
        // SetArtShadow가 못 찾아 그림자 토글이 조용히 무시된다). transition
        // 채널만 None으로 되돌리고 컴포넌트 자체는 살려둔다.
        runner.Play(fx, 1f, 0f, 0.30f, destroyEffectWhenDone: false);
    }

    /// <summary>아이템2 — 판이 끝난 뒤 필드에 남은 카드들을 흑백으로
    /// 죽여서 "이제 의미 없는 패"라는 신호를 준다. <paramref name="dead"/>
    /// =false로 다시 부르면 원상복구(다음 판을 대비한 안전장치, 지금은
    /// 호출부가 항상 딜링 전에 필드를 통째로 갈아엎으므로 실제로 쓸 일은
    /// 없지만 대칭을 위해 남겨둔다).</summary>
    public static void ApplyDeadTone(Graphic g, bool dead)
    {
        if (g == null) return;
        var fx = g.GetComponent<UIEffect>();
        if (fx == null)
        {
            if (!dead) return;
            fx = g.gameObject.AddComponent<UIEffect>();
        }
        fx.toneFilter = dead ? ToneFilter.Grayscale : ToneFilter.None;
        fx.toneIntensity = dead ? 0.85f : 0f;
    }

    /// <summary>아이템3 — 광박/멍박/피박처럼 "위험" 배지가 켜지면 은은한
    /// 샤이니 스윕을 반복시켜 눈길을 끈다. <c>edgeShinyAutoPlaySpeed</c>와
    /// 같은 원리로 <c>transitionAutoPlaySpeed</c>를 쓰면 코루틴 없이 계속
    /// 반복된다 — Shiny 트랜지션은 엣지뿐 아니라 그래픽 전체를 가로지르는
    /// 빛줄기라 배지 배경 전체가 "지금 위험하다"는 신호를 낸다.</summary>
    public static void SetRiskShinyPulse(Graphic g, bool active)
    {
        if (g == null) return;
        var fx = g.GetComponent<UIEffect>();
        if (fx == null)
        {
            if (!active) return;
            fx = g.gameObject.AddComponent<UIEffect>();
        }
        fx.transitionFilter = active ? TransitionFilter.Shiny : TransitionFilter.None;
        if (active)
        {
            fx.transitionColor = new Color(1f, 1f, 1f, 0.9f);
            fx.transitionWidth = 0.3f;
            fx.transitionSoftness = 0.5f;
            fx.transitionAutoPlaySpeed = 0.6f;
        }
    }

    /// <summary>아이템4 — 팝업이 뜰 때 즉시 나타나는 대신 짧게 페이드인.
    /// Fade 트랜지션을 1(완전히 가려짐)→0(정상)으로 애니메이션한다. 팝업은
    /// 열고 닫히길 반복하므로 UIEffect/러너를 매번 새로 만들지 않고
    /// 재사용한다.</summary>
    public static void PlayPopupFadeIn(Graphic g)
    {
        if (g == null) return;
        var fx = g.GetComponent<UIEffect>();
        if (fx == null) fx = g.gameObject.AddComponent<UIEffect>();
        fx.transitionFilter = TransitionFilter.Fade;
        fx.transitionRate = 1f;
        var runner = g.GetComponent<GoStopTransitionRunner>();
        if (runner == null) runner = g.gameObject.AddComponent<GoStopTransitionRunner>();
        runner.Play(fx, 1f, 0f, 0.18f, destroyEffectWhenDone: false);
    }

    /// <summary>아이템5 — 총통·5광처럼 극적인 순간에 화면 전체를 아주
    /// 짧게 스타일화된 톤으로 물들였다 원복하는 "필름 컷" 플래시.
    /// <see cref="GoStopVectorEffect"/>(UI Toolkit)의 카드 슬램인과 동시에
    /// 트리거해 무게감을 더한다 — UI Toolkit 엘리먼트엔 UIEffect(UGUI
    /// 전용)를 못 붙이므로, 별도의 전체화면 UGUI 오버레이를 새로 만들어
    /// 그 위에만 건다(아이템11 폭탄 타격감에도 재사용).</summary>
    public static void PlayDramaticFlash(RectTransform canvasRoot, Color tint)
    {
        if (canvasRoot == null) return;
        var go = new GameObject("DramaticFlash", typeof(RectTransform));
        go.transform.SetParent(canvasRoot, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        go.transform.SetAsLastSibling(); // Overlay/다른 이펙트보다 항상 위

        var img = go.AddComponent<Image>();
        img.color = new Color(tint.r, tint.g, tint.b, 0f);
        img.raycastTarget = false;

        var fx = go.AddComponent<UIEffect>();
        fx.toneFilter = ToneFilter.Posterize;
        fx.toneIntensity = 1f;

        var runner = go.AddComponent<GoStopFlashRunner>();
        runner.Play(img);
    }

    /// <summary>아이템6 — 필드→Cap 캡처 비행이 정확히 착지하는 순간에만
    /// 터지는 작은 골드 스파클. 기존 <see cref="GoStopIcons.SpawnBurst"/>
    /// (절차적 버스트)와 별개로, 진짜 UIParticle로 "먹었다"는 손맛을 한 겹
    /// 더 얹는다. <see cref="GoStop3PGame"/>의 <c>SlamToCap</c>이 정상
    /// 완주했을 때만(=실제로 캡처됐을 때만) 부른다.</summary>
    public static void PlayCaptureSparkle(RectTransform stableParent, Vector2 localPos)
    {
        if (stableParent == null) return;
        var host = NewParticleHost(stableParent, localPos, "CaptureSparkle", out var ps, out var uip);

        var main = ps.main;
        main.duration = 0.5f;
        main.startLifetime = 0.4f;
        main.startSpeed = new ParticleSystem.MinMaxCurve(18f, 40f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.25f, 0.5f);
        main.gravityModifier = 0.15f;
        main.maxParticles = 20;
        main.startColor = new ParticleSystem.MinMaxGradient(new Color(1f, 0.85f, 0.3f), Color.white);

        var emission = ps.emission;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 14) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 45f;
        shape.radius = 0.04f;

        uip.RefreshParticles();
        ps.Play();
        host.gameObject.AddComponent<GoStopFXCleanup>().ScheduleDestroy(host.gameObject, 0.5f + 0.4f + 0.3f);
    }

    /// <summary>아이템7 — 판돈이 도착하는 순간, 금액 규모가 일정 이상이면
    /// (<see cref="GoStopMoneyFly"/>가 문턱값을 판단) 동전들이 흩뿌려지는
    /// "동전 비"를 추가로 얹는다. 기존 <see cref="GoStopIcons.SpawnBurst"/>
    /// 스파클은 그대로 두고 위에 additive로 겹친다.</summary>
    public static void PlayCoinRain(RectTransform stableParent, Vector2 localPos, int amount)
    {
        if (stableParent == null) return;
        int count = Mathf.Clamp(amount / 2000, 6, 30);
        var host = NewParticleHost(stableParent, localPos, "CoinRain", out var ps, out var uip);

        var main = ps.main;
        main.duration = 0.8f;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.6f, 0.9f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(25f, 55f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.35f, 0.65f);
        main.gravityModifier = 1.2f;
        main.maxParticles = 40;
        main.startColor = new ParticleSystem.MinMaxGradient(new Color(1f, 0.82f, 0.25f), new Color(1f, 0.95f, 0.6f));

        var emission = ps.emission;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)count) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 30f;
        shape.radius = 0.05f;

        uip.RefreshParticles();
        ps.Play();
        host.gameObject.AddComponent<GoStopFXCleanup>().ScheduleDestroy(host.gameObject, 0.8f + 0.9f + 0.3f);
    }

    /// <summary>아이템8 — AI 캐릭터가 파산해서 은퇴 처리될 때, 그 좌석의
    /// 상태박스 위로 회색 재가 천천히 흩날리는 연출. 플레이어 자신의
    /// 파산(잔액 리셋으로 다시 시작 가능)과는 구분되는, "이 캐릭터는
    /// 이제 이 로스터에서 빠진다"는 신호다.</summary>
    public static void PlayRetirementAsh(RectTransform stableParent, Vector2 localPos)
    {
        if (stableParent == null) return;
        var host = NewParticleHost(stableParent, localPos, "RetirementAsh", out var ps, out var uip);

        var main = ps.main;
        main.duration = 1.2f;
        main.startLifetime = new ParticleSystem.MinMaxCurve(1.0f, 1.6f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(8f, 20f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.3f, 0.55f);
        main.gravityModifier = 0.25f; // 재는 천천히 가라앉는다
        main.maxParticles = 26;
        main.startColor = new ParticleSystem.MinMaxGradient(new Color(0.55f, 0.55f, 0.58f), new Color(0.78f, 0.78f, 0.8f));

        var emission = ps.emission;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 22) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 60f;
        shape.radius = 0.08f;

        var rot = ps.rotationOverLifetime;
        rot.enabled = true;
        rot.z = new ParticleSystem.MinMaxCurve(-90f, 90f);

        uip.RefreshParticles();
        ps.Play();
        host.gameObject.AddComponent<GoStopFXCleanup>().ScheduleDestroy(host.gameObject, 1.2f + 1.6f + 0.3f);
    }

    /// <summary>아이템9 — 연속 뻑(streak≥2)일 때 <see cref="GoStopIcons.SpawnBurst"/>
    /// 위에 추가로 얹는 진짜 UIParticle 버스트. streak가 클수록(연뻑=2,
    /// 쓰리뻑 직전=3…) 개수·속도가 커져서 "스트릭이 유지되고 있다"는
    /// 압박감이 누적된다.</summary>
    public static void PlayStreakBurst(RectTransform stableParent, Vector2 localPos, int streak)
    {
        if (stableParent == null) return;
        int count = Mathf.Clamp(10 + (streak - 1) * 8, 10, 50);
        float speed = Mathf.Clamp(50f + (streak - 1) * 15f, 50f, 110f);
        var host = NewParticleHost(stableParent, localPos, "StreakBurst", out var ps, out var uip);

        var main = ps.main;
        main.duration = 0.7f;
        main.startLifetime = 0.6f;
        main.startSpeed = speed;
        main.startSize = new ParticleSystem.MinMaxCurve(0.4f, 0.9f);
        main.gravityModifier = 0.3f;
        main.maxParticles = 60;
        main.startColor = new ParticleSystem.MinMaxGradient(new Color(1f, 0.55f, 0.15f), new Color(1f, 0.85f, 0.3f));

        var emission = ps.emission;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)count) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 50f;
        shape.radius = 0.05f;

        var rot = ps.rotationOverLifetime;
        rot.enabled = true;
        rot.z = new ParticleSystem.MinMaxCurve(-200f, 200f);

        uip.RefreshParticles();
        ps.Play();
        host.gameObject.AddComponent<GoStopFXCleanup>().ScheduleDestroy(host.gameObject, 0.7f + 0.6f + 0.3f);
    }

    /// <summary>아이템12 — 흔들기/폭탄 선언 순간(PlayShake와 동시에) 그
    /// 좌석 자리에서 부채처럼 넓게 퍼지는 골드 스파크. GoStopVectorEffect의
    /// 카드 부채 자체는 UI Toolkit이라 UIEffect를 못 붙이므로, 같은 자리에
    /// 겹쳐 뜨는 별도 UIParticle로 "짜잔" 하는 느낌을 보탠다.</summary>
    public static void PlayShakeSparkle(RectTransform stableParent, Vector2 localPos)
    {
        if (stableParent == null) return;
        var host = NewParticleHost(stableParent, localPos, "ShakeSparkle", out var ps, out var uip);

        var main = ps.main;
        main.duration = 0.6f;
        main.startLifetime = 0.5f;
        main.startSpeed = new ParticleSystem.MinMaxCurve(30f, 60f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.3f, 0.6f);
        main.gravityModifier = 0f;
        main.maxParticles = 24;
        main.startColor = new ParticleSystem.MinMaxGradient(HwatuTheme.Gold, Color.white);

        var emission = ps.emission;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 20) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 70f; // 부채처럼 넓게 퍼지도록
        shape.radius = 0.05f;

        uip.RefreshParticles();
        ps.Play();
        host.gameObject.AddComponent<GoStopFXCleanup>().ScheduleDestroy(host.gameObject, 0.6f + 0.5f + 0.3f);
    }

    /// <summary>위 6~9,12번 헬퍼가 전부 공유하는 UIParticle 호스트 생성
    /// 보일러플레이트 — <see cref="PlayWinConfetti"/>에서 검증된 것과 같은
    /// 뼈대(anchoredPosition 배치 → UIParticle → 자식 ParticleSystem →
    /// playOnAwake 함정 방지용 Stop)만 뽑아낸 것. 호출자는 main/emission/
    /// shape 등 나머지 튜닝만 채우면 된다.</summary>
    static RectTransform NewParticleHost(RectTransform stableParent, Vector2 localPos, string name, out ParticleSystem ps, out UIParticle uip)
    {
        var host = new GameObject(name, typeof(RectTransform));
        host.transform.SetParent(stableParent, false);
        var hostRT = host.GetComponent<RectTransform>();
        hostRT.anchorMin = hostRT.anchorMax = new Vector2(0.5f, 0.5f);
        hostRT.pivot = new Vector2(0.5f, 0.5f);
        hostRT.anchoredPosition = localPos;
        hostRT.sizeDelta = new Vector2(10f, 10f);

        uip = host.AddComponent<UIParticle>();
        var psGo = new GameObject("PS", typeof(ParticleSystem));
        psGo.transform.SetParent(host.transform, false);
        ps = psGo.GetComponent<ParticleSystem>();
        // ParticleSystem은 playOnAwake 기본값이 true라 컴포넌트가 붙는 순간
        // 이미 재생 중이다 — main.duration 등을 만지기 전에 반드시 먼저
        // 멈춰야 한다(PlayWinConfetti에서 실측으로 확인한 함정과 동일).
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        var main = ps.main;
        main.playOnAwake = false;
        main.loop = false;
        main.simulationSpace = ParticleSystemSimulationSpace.Local;

        var emission = ps.emission;
        emission.rateOverTime = 0f;

        return hostRT;
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

/// <summary>UIEffect의 <c>transitionRate</c>를 짧게 보간하는 공용 러너 —
/// <see cref="GoStopFX.PlayCardRevealDissolve"/>(카드 디졸브 리빌)와
/// <see cref="GoStopFX.PlayPopupFadeIn"/>(팝업 페이드인)이 공유한다.
/// <paramref name="destroyEffectWhenDone"/>이 참이면(카드처럼 일회성 대상)
/// 재생이 끝난 뒤 UIEffect 자체를 지워서 그 그래픽을 원래의 평범한
/// Image로 되돌린다 — 팝업(재사용 대상)은 거짓으로 둬서 다음에 다시 열 때
/// 컴포넌트를 새로 안 만들고 재사용한다.</summary>
public class GoStopTransitionRunner : MonoBehaviour
{
    Coroutine running;

    public void Play(UIEffect fx, float from, float to, float dur, bool destroyEffectWhenDone)
    {
        if (running != null) StopCoroutine(running);
        running = StartCoroutine(Run(fx, from, to, dur, destroyEffectWhenDone));
    }

    IEnumerator Run(UIEffect fx, float from, float to, float dur, bool destroyEffectWhenDone)
    {
        float t = 0f;
        while (t < dur)
        {
            if (fx == null) yield break;
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / dur);
            float smooth = p * p * (3f - 2f * p); // smoothstep
            fx.transitionRate = Mathf.Lerp(from, to, smooth);
            yield return null;
        }
        if (fx == null) yield break;
        fx.transitionRate = to;
        if (destroyEffectWhenDone) Destroy(fx);
        Destroy(this);
    }
}

/// <summary><see cref="GoStopFX.PlayDramaticFlash"/> 전용 — 전체화면 오버레이의
/// 알파를 확 켰다가 부드럽게 꺼서 "필름 컷" 느낌을 낸 뒤 스스로 정리한다.</summary>
public class GoStopFlashRunner : MonoBehaviour
{
    public void Play(Image img) => StartCoroutine(Run(img));

    IEnumerator Run(Image img)
    {
        const float inDur = 0.06f, holdDur = 0.05f, outDur = 0.18f;
        float t = 0f;
        while (t < inDur)
        {
            if (img == null) yield break;
            t += Time.deltaTime;
            var c = img.color;
            c.a = Mathf.Lerp(0f, 0.35f, t / inDur);
            img.color = c;
            yield return null;
        }
        t = 0f;
        while (t < holdDur) { t += Time.deltaTime; yield return null; }
        t = 0f;
        while (t < outDur)
        {
            if (img == null) yield break;
            t += Time.deltaTime;
            var c = img.color;
            c.a = Mathf.Lerp(0.35f, 0f, t / outDur);
            img.color = c;
            yield return null;
        }
        if (img != null) Destroy(img.gameObject);
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
        // 2026-09-06 — "돈 변화 연출이 잘 안 보인다" 요청으로 0.55→0.85초로
        // 늘려서 눈이 따라갈 시간을 더 준다(위 크기 확대와 함께 적용).
        const float dur = 0.85f;
        // 포물선처럼 보이게 중간 지점을 위로 띄운다 — 지속시간이 늘어난
        // 만큼 궤적도 더 크게 띄워서 밋밋해 보이지 않게 한다.
        Vector3 mid = Vector3.Lerp(from, to, 0.5f) + new Vector3(0f, 100f, 0f);

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
            // 2026-09-06 — "돈 변화 연출이 잘 안 보인다" 요청으로 파티클
            // 6→12개로 늘려 도착 순간을 더 화려하게 했다.
            GoStopIcons.SpawnBurst(stableParent, localPos, new Color(1f, 0.85f, 0.3f), count: 12);
            SpawnFloatText(stableParent, localPos, amount);
            // 2026-09-13(아이템7) — 금액이 일정 규모 이상이면(광팔이·최종정산처럼
            // 큰 정산) 동전 비를 추가로 얹는다. 작은 이체(1점=100~1000원대)는
            // 매번 우려지는 걸 막기 위해 문턱값을 둔다.
            if (amount >= 3000) GoStopFX.PlayCoinRain(stableParent, localPos, amount);
        }
        Destroy(gameObject);
    }

    static void SpawnFloatText(RectTransform parent, Vector2 localPos, int amount)
    {
        // 2026-09-06 — 같은 요청으로 폰트 24→32로 확대.
        var lbl = HwatuUI.MakeLabel(parent, localPos + new Vector2(0f, 6f), new Vector2(280f, 52f), 32f,
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
