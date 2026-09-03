using UnityEngine;
using Coffee.UIExtensions;

/// <summary>
/// 배경 바람 파티클 — 화투 12개월 모티프(<see cref="GoStopMotifAtlas"/>)가
/// 랜덤으로 섞여 화면 위에서 천천히 떠내려온다. "밋밋한 화면을 방해하지
/// 않는 선에서 채우고, 바람에 날리듯 여유롭게 움직인다"는 요청.
///
/// <b>레이어링</b> — <c>ambientPS</c>는 ContentArea의 두 번째 자식(첫 자식은
/// 이미 있던 BackgroundPattern 격자무늬 — <see cref="GoStop3PGame"/>의
/// BuildStaticUI 참고)으로 붙인다. 필드/손패/획득패 등 실제 게임 콘텐츠는
/// 전부 그 뒤에 추가되는 자식이라 항상 파티클 위에 그려진다 — 게임 진행에
/// 절대 방해가 안 된다. UIParticle은 UGUI 레이캐스트 대상이 아니라
/// raycastTarget을 따로 끌 필요도 없이 클릭을 절대 가로채지 않는다.
///
/// <b>버스트 연동</b> — 필드 이펙트(뻑/쪽/싹쓸이/폭탄/족보완성/광완성/
/// 총통/나가리)가 터질 때 파티클도 같이 터지는 연출은 <c>burstPS</c>가
/// 담당한다. 8개 호출부 전부를 일일이 고치는 대신, 그 8곳이 이미 공유하는
/// <see cref="GoStopIcons.SpawnBurst"/> 안에서 <see cref="Burst"/>도 같이
/// 부르도록 걸어뒀다.
///
/// <b>ParticleEffectForUGUI(UIParticle) 사용 원칙</b>은 <see cref="GoStopFX.PlayWinConfetti"/>
/// 에서 이미 확립한 것과 동일 — scale3D 기본값(10)이 시뮬레이션 유닛을
/// 캔버스 픽셀로 환산하는 배율이라, 위치·크기 값은 전부 그 배율을 먼저
/// 나눠서 넣는다. ParticleSystem은 playOnAwake 기본값이 true라 컴포넌트가
/// 붙는 순간 이미 재생 중이므로, 설정을 만지기 전에 항상 먼저 Stop한다.
/// </summary>
public class GoStopWindParticles : MonoBehaviour
{
    public static GoStopWindParticles Instance;

    ParticleSystem ambientPS;
    UIParticle burstUip;
    ParticleSystem burstPS;

    // UIParticle 기본 scale3D=(10,10,10) — 캔버스 px ÷ 이 값 = 시뮬레이션 유닛.
    const float SimScale = 10f;

    static Material sharedMat;
    static Material Mat()
    {
        if (sharedMat != null) return sharedMat;
        sharedMat = new Material(Shader.Find("Sprites/Default"));
        sharedMat.mainTexture = GoStopMotifAtlas.Texture;
        return sharedMat;
    }

    /// <param name="contentArea">배경 파티클을 깔 곳 — 그 게임 씬의
    /// ContentArea(항상 존재하고 안 지워지는 컨테이너). 두 번째 자식으로
    /// 붙는다(첫 자식은 BackgroundPattern).</param>
    /// <param name="canvasRoot">버스트 파티클을 붙일 곳 — <see cref="GoStopIcons.SpawnBurst"/>
    /// 가 쓰는 것과 같은 Canvas 레벨 RectTransform.</param>
    public static GoStopWindParticles Ensure(RectTransform contentArea, RectTransform canvasRoot)
    {
        if (Instance != null) return Instance;
        if (contentArea == null || canvasRoot == null) return null;
        var go = new GameObject("GoStopWindParticles");
        Instance = go.AddComponent<GoStopWindParticles>();
        Instance.Setup(contentArea, canvasRoot);
        return Instance;
    }

    void Setup(RectTransform contentArea, RectTransform canvasRoot)
    {
        var ambHost = new GameObject("WindAmbient", typeof(RectTransform));
        ambHost.transform.SetParent(contentArea, false);
        var ambRT = (RectTransform)ambHost.transform;
        ambRT.anchorMin = Vector2.zero; ambRT.anchorMax = Vector2.one;
        ambRT.offsetMin = Vector2.zero; ambRT.offsetMax = Vector2.zero;
        ambHost.transform.SetSiblingIndex(1); // BackgroundPattern(0) 바로 위, 게임 콘텐츠보다는 항상 아래
        var ambUip = ambHost.AddComponent<UIParticle>();

        var ambPsGo = new GameObject("PS", typeof(ParticleSystem));
        ambPsGo.transform.SetParent(ambHost.transform, false);
        ambientPS = ambPsGo.GetComponent<ParticleSystem>();
        ambientPS.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        ambPsGo.GetComponent<ParticleSystemRenderer>().sharedMaterial = Mat();
        ConfigureShared(ambientPS);
        ConfigureAmbient(ambientPS, contentArea.rect.width, contentArea.rect.height);
        ambUip.RefreshParticles();
        ambientPS.Play();

        var burstHost = new GameObject("WindBurst", typeof(RectTransform));
        burstHost.transform.SetParent(canvasRoot, false);
        var burstRT = (RectTransform)burstHost.transform;
        burstRT.anchorMin = burstRT.anchorMax = new Vector2(0.5f, 0.5f);
        burstRT.sizeDelta = Vector2.zero;
        burstUip = burstHost.AddComponent<UIParticle>();

        var burstPsGo = new GameObject("PS", typeof(ParticleSystem));
        burstPsGo.transform.SetParent(burstHost.transform, false);
        burstPS = burstPsGo.GetComponent<ParticleSystem>();
        burstPS.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        burstPsGo.GetComponent<ParticleSystemRenderer>().sharedMaterial = Mat();
        ConfigureShared(burstPS);
        ConfigureBurst(burstPS);
        burstUip.RefreshParticles();
    }

    /// <summary>두 시스템이 공유하는 설정 — 아틀라스에서 랜덤으로 한 칸을
    /// 골라 그 파티클이 사는 동안 계속 유지한다(frameOverTime을 상수 0으로
    /// 고정 = 애니메이션 안 함, startFrame만 랜덤). 살짝 회전도 같이 준다.</summary>
    void ConfigureShared(ParticleSystem ps)
    {
        var tsa = ps.textureSheetAnimation;
        tsa.enabled = true;
        tsa.mode = ParticleSystemAnimationMode.Grid;
        tsa.numTilesX = GoStopMotifAtlas.Cols;
        tsa.numTilesY = GoStopMotifAtlas.Rows;
        tsa.animation = ParticleSystemAnimationType.WholeSheet;
        tsa.frameOverTime = new ParticleSystem.MinMaxCurve(0f);
        tsa.startFrame = new ParticleSystem.MinMaxCurve(0f, GoStopMotifAtlas.Cols * GoStopMotifAtlas.Rows - 0.01f);
        tsa.cycleCount = 1;

        var rot = ps.rotationOverLifetime;
        rot.enabled = true;
        rot.z = new ParticleSystem.MinMaxCurve(-0.5f, 0.5f);
    }

    /// <summary>상시 루프 — 화면 상단 가장자리에서 낮은 밀도로 계속
    /// 태어나 아주 약한 중력 + 노이즈(바람 결)로 느긋하게 떨어진다.
    /// colorOverLifetime으로 태어날 때/사라질 때 페이드해서 팝인·팝아웃이
    /// 안 보이게 한다.</summary>
    void ConfigureAmbient(ParticleSystem ps, float canvasW, float canvasH)
    {
        float halfW = canvasW * 0.5f / SimScale;
        float halfH = canvasH * 0.5f / SimScale;

        var main = ps.main;
        main.playOnAwake = false;
        main.loop = true;
        main.startLifetime = new ParticleSystem.MinMaxCurve(7f, 13f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0f, 0.6f);
        main.startSize = new ParticleSystem.MinMaxCurve(1.0f, 2.2f);
        main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
        main.gravityModifier = new ParticleSystem.MinMaxCurve(0.05f, 0.12f);
        main.simulationSpace = ParticleSystemSimulationSpace.Local;
        main.maxParticles = 90;
        main.startColor = Color.white; // 색은 아틀라스에 이미 구워져 있다

        var emission = ps.emission;
        emission.rateOverTime = 2.5f;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(halfW * 2f, 0.05f, 0.05f);
        shape.position = new Vector3(0f, halfH, 0f); // 캔버스 맨 위 가장자리 전체

        var noise = ps.noise;
        noise.enabled = true;
        noise.strength = 0.35f;
        noise.frequency = 0.15f;
        noise.scrollSpeed = 0.25f; // "바람에 날리는 것과 같이" — 좌우로 느리게 흔들리는 결

        var col = ps.colorOverLifetime;
        col.enabled = true;
        var grad = new Gradient();
        grad.SetKeys(
            new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(0.5f, 0.12f), new GradientAlphaKey(0.5f, 0.82f), new GradientAlphaKey(0f, 1f) });
        col.color = grad;
    }

    /// <summary>필드 이펙트 트리거 전용 — 평소엔 안 뿜다가(rateOverTime=0)
    /// <see cref="Burst"/>가 수동으로 <c>Emit</c>할 때만 원뿔형으로 확
    /// 퍼진다. 짧고 빠르게(0.6~1.0초) 날아가며 페이드아웃한다.</summary>
    void ConfigureBurst(ParticleSystem ps)
    {
        var main = ps.main;
        main.playOnAwake = false;
        main.loop = false;
        main.duration = 1f;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.6f, 1.0f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(35f, 80f);
        main.startSize = new ParticleSystem.MinMaxCurve(1.0f, 2.0f);
        main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
        main.gravityModifier = 0.5f;
        main.simulationSpace = ParticleSystemSimulationSpace.Local;
        main.maxParticles = 60;
        main.startColor = Color.white;

        var emission = ps.emission;
        emission.rateOverTime = 0f;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 55f;
        shape.radius = 0.05f;

        var col = ps.colorOverLifetime;
        col.enabled = true;
        var grad = new Gradient();
        grad.SetKeys(
            new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            new[] { new GradientAlphaKey(0.85f, 0f), new GradientAlphaKey(0.85f, 0.55f), new GradientAlphaKey(0f, 1f) });
        col.color = grad;
    }

    /// <summary>필드 이펙트가 터질 때 같이 부른다 — <see cref="GoStopIcons.SpawnBurst"/>
    /// 가 이미 8개 호출부 전부에서 대신 불러주므로 개별 호출부를 안 고쳐도
    /// 된다. <paramref name="canvasLocalPos"/>는 SpawnBurst와 같은 좌표계
    /// (Canvas 기준 anchoredPosition)를 그대로 받는다.</summary>
    public void Burst(Vector2 canvasLocalPos, int count = 10)
    {
        if (burstPS == null) return;
        var pos = new Vector3(canvasLocalPos.x / SimScale, canvasLocalPos.y / SimScale, 0f);
        burstPS.Emit(new ParticleSystem.EmitParams { position = pos }, count);
    }
}
