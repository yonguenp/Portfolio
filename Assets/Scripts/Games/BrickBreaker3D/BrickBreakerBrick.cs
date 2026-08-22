using System.Collections;
using UnityEngine;
using TMPro;

public class BrickBreakerBrick : MonoBehaviour
{
    public enum ItemType { None, BallAdd, DamageUp, BallSize, LuckUp }

    /// <summary>파워업 종류별 색과 라벨. 한눈에 구분돼야 조준할 값어치가 생긴다.</summary>
    public static (Color color, string label) ItemLook(ItemType t) => t switch
    {
        ItemType.BallAdd  => (new Color(0.10f, 0.90f, 0.45f), "+1"),
        ItemType.DamageUp => (new Color(1.00f, 0.35f, 0.25f), "공격"),
        ItemType.BallSize => (new Color(0.30f, 0.70f, 1.00f), "크게"),
        ItemType.LuckUp   => (new Color(0.85f, 0.45f, 1.00f), "행운"),
        _                 => (Color.white, ""),
    };

    public int      Col       { get; private set; }
    public int      Row       { get; private set; }
    // HP는 내부적으로 실수다 — 데미지가 정수 단위면 1→2가 곧 100% 상승이라
    // 파워업을 잘게 쪼갤 수가 없다. 표시·색상은 올림한 정수를 쓴다.
    float hpF;
    public int      HP        => Mathf.CeilToInt(hpF);
    public ItemType Item      { get; private set; }
    public bool     IsBallItem => Item != ItemType.None;
    public bool     Dead      { get; private set; }

    /// <summary>브릭 모양. Sphere 외에는 **로컬 평면 집합**으로 다룬다.</summary>
    public enum Shape { Box, Sphere, Tetra }

    public Shape Form { get; private set; } = Shape.Box;
    public bool  IsSphere => Form == Shape.Sphere;

    // ── 볼록 다면체 기하 ─────────────────────────────────
    // 공 물리와 조준 예측선이 **같은 함수**를 호출해야 예측이 어긋나지 않는다.
    // 그래서 계산을 브릭에 모아두고 양쪽이 불러 쓴다.
    static readonly (Vector3 n, float d)[] BoxPlanes =
    {
        (Vector3.right, HalfX), (Vector3.left, HalfX),
        (Vector3.up,    HalfY), (Vector3.down, HalfY),
        (Vector3.forward, HalfZ), (Vector3.back, HalfZ),
    };

    static (Vector3 n, float d)[] tetraPlanes;
    static (Vector3 n, float d)[] TetraPlanes
    {
        get
        {
            if (tetraPlanes != null) return tetraPlanes;
            float a = HalfX;
            Vector3[] v =
            {
                new Vector3( a,  a,  a), new Vector3( a, -a, -a),
                new Vector3(-a,  a, -a), new Vector3(-a, -a,  a),
            };
            // 꼭짓점 vi 반대편 면의 바깥 법선은 -vi 방향, 중심까지 거리는 |vi|/3
            tetraPlanes = new (Vector3, float)[4];
            for (int i = 0; i < 4; i++)
                tetraPlanes[i] = (-v[i].normalized, v[i].magnitude / 3f);
            return tetraPlanes;
        }
    }

    (Vector3 n, float d)[] Planes => Form == Shape.Tetra ? TetraPlanes : BoxPlanes;

    /// <summary>
    /// 레이 vs 이 브릭(볼록). 공 반지름만큼 각 평면을 바깥으로 밀어 근사한다.
    /// </summary>
    public bool RaycastConvex(Vector3 originW, Vector3 dirW, float expand,
                              out float tHit, out Vector3 normalW)
    {
        tHit = 0f; normalW = Vector3.zero;

        // 회전만 되돌린다. InverseTransformPoint는 스케일까지 나눠버리는데
        // 박스는 localScale=1.75인 단위 큐브라 로컬 반칸이 0.5가 되고,
        // 평면 거리(월드 기준 0.875)와 안 맞아 충돌 부피가 1.75배로 부푼다.
        Quaternion inv = Quaternion.Inverse(transform.rotation);
        Vector3 o = inv * (originW - transform.position);
        Vector3 d = inv * dirW;

        float tEnter = -1e9f, tExit = 1e9f;
        Vector3 nEnter = Vector3.zero;

        foreach (var (n, dist) in Planes)
        {
            float denom = Vector3.Dot(d, n);
            float distToPlane = Vector3.Dot(o, n) - (dist + expand);

            if (Mathf.Abs(denom) < 1e-6f)
            {
                if (distToPlane > 0f) return false;   // 평면 바깥에서 평행 → 못 맞음
                continue;
            }

            float t = -distToPlane / denom;
            if (denom < 0f) { if (t > tEnter) { tEnter = t; nEnter = n; } }   // 들어가는 면
            else            { if (t < tExit)  tExit = t; }                    // 나가는 면

            if (tEnter > tExit) return false;
        }

        if (tExit < 0f) return false;
        tHit    = Mathf.Max(tEnter, 0f);
        normalW = transform.rotation * nEnter;
        return true;
    }

    /// <summary>
    /// 점(공 중심)이 이 브릭 안에 파고들었는가. 가장 얕게 박힌 면을 밀어내는 면으로 고른다.
    /// </summary>
    public bool OverlapConvex(Vector3 pointW, float expand, out Vector3 normalW, out float depth)
    {
        normalW = Vector3.zero; depth = 0f;

        Vector3 p = Quaternion.Inverse(transform.rotation) * (pointW - transform.position);
        float   best = float.MaxValue;
        Vector3 bestN = Vector3.zero;

        foreach (var (n, dist) in Planes)
        {
            float outside = Vector3.Dot(p, n) - (dist + expand);
            if (outside > 0f) return false;          // 한 면이라도 바깥이면 충돌 아님
            float pen = -outside;                     // 파고든 깊이
            if (pen < best) { best = pen; bestN = n; }
        }

        normalW = transform.rotation * bestN;
        depth   = best;
        return true;
    }

    /// <summary>구형 브릭 반지름. 박스와 같은 외곽(셀 반칸)이라 배치 규칙이 안 바뀐다.</summary>
    public const float SphereR = 0.875f;

    public const float HalfX = 0.875f;
    public const float HalfY = 0.875f;
    public const float HalfZ = 0.875f;

    // ── 지속 회전 ────────────────────────────────────────
    // 타점이 매 순간 달라져서 같은 방향으로 쏜 뒤따르는 공들이 사방으로 흩어진다.
    // 충돌은 브릭 로컬 공간에서 계산하므로(RaycastConvex/OverlapConvex)
    // 공 물리와 조준 예측선 모두 회전을 자동으로 반영한다.
    Vector3 spinAxis;
    float   spinSpeed;          // deg/sec, 0이면 정지
    const float LabelFront = 1.35f;

    public bool IsSpinning => spinSpeed != 0f;

    public void SetSpin(Vector3 axis, float degPerSec)
    {
        spinAxis  = axis.normalized;
        spinSpeed = degPerSec;
    }

    void Update()
    {
        if (spinSpeed == 0f) return;

        transform.rotation = Quaternion.AngleAxis(spinSpeed * Time.deltaTime, spinAxis) * transform.rotation;

        // 라벨이 같이 돌면 숫자를 못 읽는다 → 월드 기준으로 앞쪽에 세워둔다
        if (label != null)
        {
            label.transform.position = transform.position + Vector3.back * LabelFront;
            label.transform.rotation = Quaternion.identity;
        }
    }

    int  hpMax = 1;
    bool aimTargeted;
    TextMeshPro label;
    Renderer    rend;
    Coroutine   blinkRoutine;
    Coroutine   hitRoutine;
    Vector3     baseScaleCached = Vector3.one;

    static readonly Color[] BrickColors =
    {
        new Color(0.20f, 0.55f, 1.00f),  // blue      (1-2 HP)
        new Color(0.10f, 0.85f, 0.35f),  // green     (3-5 HP)
        new Color(1.00f, 0.85f, 0.00f),  // yellow    (6-10 HP)
        new Color(1.00f, 0.50f, 0.00f),  // orange    (11-20 HP)
        new Color(1.00f, 0.18f, 0.12f),  // red       (21-40 HP)
        new Color(0.80f, 0.10f, 1.00f),  // purple    (41-80 HP)
        new Color(0.00f, 0.90f, 1.00f),  // cyan      (81+ HP)
    };

    static Color HpToColor(int hp)
    {
        if (hp <= 2)  return BrickColors[0];
        if (hp <= 5)  return BrickColors[1];
        if (hp <= 10) return BrickColors[2];
        if (hp <= 20) return BrickColors[3];
        if (hp <= 40) return BrickColors[4];
        if (hp <= 80) return BrickColors[5];
        return BrickColors[6];
    }

    public void Init(int col, int row, int hp, ItemType item = ItemType.None, Shape form = Shape.Box)
    {
        Col = col; Row = row; hpF = hp; hpMax = Mathf.Max(1, hp); Item = item; Dead = false; Form = form;
        baseScaleCached = transform.localScale;
        rend  = GetComponentInChildren<Renderer>();
        label = GetComponentInChildren<TextMeshPro>();
        if (rend) rend.material = new Material(Shader.Find("Sprites/Default"));
        UpdateVisual();
    }

    public Color CurrentColor => rend ? rend.material.color : Color.white;

    public bool TakeDamage(float dmg = 1f)
    {
        if (Dead || IsBallItem) return false;
        hpF -= dmg;
        if (hpF <= 1e-4f) { hpF = 0f; Die(); return true; }
        UpdateVisual();
        PlayHitReaction();
        return false;
    }

    // ── 피격 반응: 흰 플래시 + 스케일 펀치 ────────────────
    void PlayHitReaction()
    {
        BrickBreakerFX.Instance?.HitSpark(transform.position + Vector3.back * HalfZ, CurrentColor);
        BrickBreakerAudio.Instance?.BrickHit(HP, hpMax);
        if (hitRoutine != null) StopCoroutine(hitRoutine);
        hitRoutine = StartCoroutine(HitReaction());
    }

    IEnumerator HitReaction()
    {
        Vector3 baseScale = baseScaleCached;
        float t = 0f, dur = 0.16f;
        while (t < dur)
        {
            t += Time.deltaTime;
            float k = t / dur;
            // 눌렸다가 되돌아온다
            float punch = Mathf.Sin(k * Mathf.PI) * 0.22f;
            transform.localScale = baseScale * (1f + punch);
            if (rend) rend.material.color = Color.Lerp(Color.white, HpToColor(HP), k);
            yield return null;
        }
        transform.localScale = baseScale;
        hitRoutine = null;
        if (blinkRoutine == null) UpdateVisual();
    }

    public void Collect() { if (!Dead) Die(); }

    void Die()
    {
        Dead = true;
        if (!IsBallItem) BrickBreakerAudio.Instance?.BrickBreak();
        var fx = BrickBreakerFX.Instance;
        if (fx != null)
        {
            if (IsBallItem) fx.ItemSparkle(transform.position);
            else            fx.Explode(transform.position, CurrentColor);
        }
        else SpawnFX(); // FX 매니저가 없을 때의 폴백
        Destroy(gameObject);
    }

    // 브릭이 실제로 존재하는 최대 z (LAYER_START + HalfZ = 5.5S).
    // 예전엔 9S(=15.75)라 밝기 범위의 앞 55%만 써서 원근 페이드가 거의 안 보였다.
    const float ZMax = 5.5f * 1.75f;

    /// <summary>
    /// z 거리에 따른 밝기 배수. 공기원근 — 멀수록 어두워져 배경에 묻힌다.
    /// 브릭과 공이 같은 곡선을 써야 서로 거리 비교가 된다.
    /// </summary>
    /// <param name="far">가장 먼 곳의 밝기. 공은 항상 눈에 띄어야 하므로 더 높게 준다.</param>
    public static float DepthShade(float z, float far = 0.55f) =>
        Mathf.Lerp(1.2f, far, Mathf.Clamp01(z / ZMax));

    void UpdateVisual()
    {
        switch (Item)
        {
            case ItemType.None:
                break;   // 아래 일반 브릭 처리로

            default:
            {
                // 조준선이 실제로 관통할 때만 노랗게. 화면상 겹치는 것과
                // 3D에서 맞는 건 다르므로 이게 유일한 확실한 신호다.
                var look = ItemLook(Item);
                SetColor(aimTargeted ? AimTargetColor : look.color);
                if (label) { label.text = look.label; label.color = aimTargeted ? Color.black : Color.white; }
                return;
            }
        }

        {
                Color hpColor    = HpToColor(HP);
                float brightness = DepthShade(transform.position.z);
                Color c = new Color(
                    Mathf.Clamp01(hpColor.r * brightness),
                    Mathf.Clamp01(hpColor.g * brightness),
                    Mathf.Clamp01(hpColor.b * brightness));
                if (aimTargeted) c = Color.Lerp(c, Color.white, AimTargetBrighten);
                SetColor(c);
            if (label) { label.text = HP.ToString(); label.color = aimTargeted ? Color.black : Color.white; }
        }
    }

    void SetColor(Color c)
    {
        if (rend) rend.material.color = c;
    }

    void SpawnFX()
    {
        Color fxColor = rend ? rend.material.color : Color.white;
        var go  = new GameObject("BrickFX");
        go.transform.position = transform.position;
        var ps  = go.AddComponent<ParticleSystem>();
        var psr = go.GetComponent<ParticleSystemRenderer>();
        var mat = new Material(Shader.Find("Sprites/Default"));
        mat.color = Color.white;
        psr.material   = mat;
        psr.renderMode = ParticleSystemRenderMode.Billboard;
        var main = ps.main;
        main.startColor    = new ParticleSystem.MinMaxGradient(fxColor);
        main.startSize     = new ParticleSystem.MinMaxCurve(0.1f, 0.35f);
        main.startSpeed    = new ParticleSystem.MinMaxCurve(2f, 6f);
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.3f, 0.6f);
        main.maxParticles  = 30;
        var emit = ps.emission;
        emit.SetBursts(new[] { new ParticleSystem.Burst(0f, 24) });
        emit.enabled = true;
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale     = new Vector3(1.5f, 1.5f, 0.5f);
        ps.Play();
        Destroy(go, 1.2f);
    }

    public void SetDanger(bool danger)
    {
        if (danger)
        {
            if (blinkRoutine == null)
                blinkRoutine = StartCoroutine(BlinkRed());
        }
        else
        {
            if (blinkRoutine != null) { StopCoroutine(blinkRoutine); blinkRoutine = null; }
            UpdateVisual();
        }
    }

    IEnumerator BlinkRed()
    {
        while (true)
        {
            SetColor(new Color(1f, 0.08f, 0.08f));
            yield return new WaitForSeconds(0.25f);
            UpdateVisual();
            yield return new WaitForSeconds(0.25f);
        }
    }

    /// <summary>브릭이 한 칸 다가오는 데 걸리는 시간. 매니저가 이만큼 기다렸다 조준을 연다.</summary>
    public const float AdvanceDuration = 0.28f;

    Coroutine slideRoutine;

    /// <returns>이동이 끝났을 때의 z. 게임오버 판정은 <b>반드시 이 반환값</b>을 쓸 것 —
    /// 슬라이드 중에는 transform.position이 중간값이라 판정이 한 턴 늦어진다.</returns>
    public float MoveTowardPlayer(float step)
    {
        Vector3 from = transform.position;
        Vector3 to   = from + Vector3.back * step;

        transform.position = to;
        UpdateVisual();

        if (slideRoutine != null) StopCoroutine(slideRoutine);
        slideRoutine = StartCoroutine(Slide(from, to));
        return to.z;
    }

    IEnumerator Slide(Vector3 from, Vector3 to)
    {
        // 반드시 먼저 양보한다. StartCoroutine은 첫 yield까지를 그 자리에서
        // 실행하므로, 여기서 위치부터 건드리면 호출한 프레임 안에서 transform이
        // 다시 출발점으로 되돌아간다.
        yield return null;

        float t = 0f;
        while (t < AdvanceDuration)
        {
            t += Time.deltaTime;
            transform.position = Vector3.Lerp(from, to, Mathf.SmoothStep(0f, 1f, t / AdvanceDuration));
            UpdateVisual();   // 다가올수록 밝아진다
            yield return null;
        }
        transform.position = to;
        slideRoutine = null;
        UpdateVisual();
    }

    static readonly Color AimTargetColor    = new Color(1.00f, 0.90f, 0.10f);  // 아이템용
    const           float AimTargetBrighten = 0.60f;                           // 일반 브릭용

    /// <summary>
    /// 조준 예측선이 이 브릭/아이템을 맞히는지 여부. 에이머가 매 프레임 갱신한다.
    /// 아이템은 노랑, 일반 브릭은 HP 색을 유지한 채 흰색 쪽으로 밝아진다
    /// (노랑을 쓰면 HP 6~10 브릭과 구분이 안 된다).
    ///
    /// 색만 바꾼다. 스케일을 키우면 라벨이 메쉬 안쪽에 있는데 ZWrite가 꺼져 있어
    /// 커진 표면이 라벨 위로 정렬돼 글자가 사라진다.
    /// </summary>
    public void SetAimTargeted(bool on)
    {
        if (aimTargeted == on) return;
        aimTargeted = on;
        UpdateVisual();
    }

    /// <summary>스폰 시 튀어나오는 연출. Init 이후에 부를 것 (baseScaleCached 필요).</summary>
    public void PlaySpawnIn() => StartCoroutine(SpawnIn());

    IEnumerator SpawnIn()
    {
        Vector3 target = baseScaleCached;
        float t = 0f, dur = 0.30f;
        while (t < dur)
        {
            t += Time.deltaTime;
            float k = t / dur;
            float s = k < 0.7f ? Mathf.Lerp(0.15f, 1.12f, k / 0.7f)
                               : Mathf.Lerp(1.12f, 1f, Mathf.InverseLerp(0.7f, 1f, k));
            transform.localScale = target * s;
            yield return null;
        }
        transform.localScale = target;
    }
}
