using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// 벽돌깨기 연출 모음. 모든 이펙트는 런타임 생성 후 수명이 끝나면 스스로 파괴된다.
/// (프로젝트에 프리팹/머티리얼 에셋이 없으므로 기존 코드와 같은 런타임 생성 방식을 따른다)
/// </summary>
public class BrickBreakerFX : MonoBehaviour
{
    public static BrickBreakerFX Instance { get; private set; }

    Camera cam;
    TMP_FontAsset font;

    void Awake()
    {
        Instance = this;
        cam  = Camera.main;
        font = Resources.Load<TMP_FontAsset>("TextMesh Pro/Fonts/ONE Mobile POP SDF");
    }

    public void SetCamera(Camera c) => cam = c;

    static Material SpriteMat(Color c)
    {
        var m = new Material(Shader.Find("Sprites/Default"));
        m.color = c;
        return m;
    }

    // ── 파티클 버스트 ────────────────────────────────────
    ParticleSystem Burst(Vector3 pos, Color color, int count,
                         float speedMin, float speedMax,
                         float sizeMin,  float sizeMax,
                         float lifeMin,  float lifeMax,
                         float shapeRadius, float destroyAfter)
    {
        var go = new GameObject("FX_Burst");
        go.transform.position = pos;
        var ps  = go.AddComponent<ParticleSystem>();
        var psr = go.GetComponent<ParticleSystemRenderer>();
        psr.material   = SpriteMat(Color.white);
        psr.renderMode = ParticleSystemRenderMode.Billboard;

        var main = ps.main;
        main.startColor    = new ParticleSystem.MinMaxGradient(color);
        main.startSize     = new ParticleSystem.MinMaxCurve(sizeMin, sizeMax);
        main.startSpeed    = new ParticleSystem.MinMaxCurve(speedMin, speedMax);
        main.startLifetime = new ParticleSystem.MinMaxCurve(lifeMin, lifeMax);
        main.maxParticles  = count * 2;
        main.gravityModifier = 0.15f;

        var emit = ps.emission;
        emit.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)count) });
        emit.enabled = true;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius    = shapeRadius;

        var col = ps.colorOverLifetime;
        col.enabled = true;
        var grad = new Gradient();
        grad.SetKeys(
            new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 0.55f), new GradientAlphaKey(0f, 1f) });
        col.color = new ParticleSystem.MinMaxGradient(grad);

        ps.Play();
        Destroy(go, destroyAfter);
        return ps;
    }

    // ── 브릭 피격 (파괴 안 됨) ───────────────────────────
    public void HitSpark(Vector3 pos, Color color)
    {
        Burst(pos, color, 8, 3f, 7f, 0.08f, 0.20f, 0.15f, 0.30f, 0.25f, 1.0f);
        Shockwave(pos, color, 0.5f, 1.6f, 0.18f);
    }

    // ── 브릭 파괴 ────────────────────────────────────────
    public void Explode(Vector3 pos, Color color, float scale = 1f)
    {
        Burst(pos, color, Mathf.RoundToInt(26 * scale), 3f, 9f,
              0.12f * scale, 0.38f * scale, 0.30f, 0.65f, 0.35f, 1.4f);
        Burst(pos, Color.white, 8, 6f, 12f, 0.06f, 0.16f, 0.12f, 0.28f, 0.2f, 0.8f);
        Shockwave(pos, color, 0.6f, 3.2f * scale, 0.28f);
        StartCoroutine(Debris(pos, color, Mathf.RoundToInt(5 * scale)));
    }

    // ── 아이템 획득 ──────────────────────────────────────
    public void ItemSparkle(Vector3 pos)
    {
        var gold = new Color(1f, 0.85f, 0.2f);
        Burst(pos, gold, 30, 4f, 10f, 0.12f, 0.4f, 0.4f, 0.9f, 0.3f, 1.6f);
        Shockwave(pos, gold, 0.5f, 4f, 0.4f);
    }

    // ── 발사 순간 ────────────────────────────────────────
    public void MuzzleFlash(Vector3 pos, Vector3 dir)
    {
        Burst(pos, new Color(1f, 1f, 0.6f), 14, 5f, 11f, 0.1f, 0.3f, 0.12f, 0.3f, 0.2f, 0.7f);
        Shockwave(pos, new Color(1f, 1f, 0.5f), 0.4f, 2.2f, 0.22f);
    }

    // ── 벽 반사 ──────────────────────────────────────────
    public void WallPing(Vector3 pos)
    {
        Shockwave(pos, new Color(0.4f, 0.7f, 1f, 1f), 0.25f, 1.1f, 0.16f);
    }

    // ── 확산 링 ──────────────────────────────────────────
    public void Shockwave(Vector3 pos, Color color, float from, float to, float dur)
    {
        var go = new GameObject("FX_Ring");
        go.transform.position = pos;
        var lr = go.AddComponent<LineRenderer>();
        const int SEG = 28;
        lr.positionCount  = SEG + 1;
        lr.useWorldSpace  = false;
        lr.material       = SpriteMat(Color.white);
        lr.startColor     = lr.endColor = color;
        lr.startWidth     = lr.endWidth = 0.09f;
        lr.sortingOrder   = 12;
        StartCoroutine(RingRoutine(go, lr, from, to, dur, color, SEG));
    }

    IEnumerator RingRoutine(GameObject go, LineRenderer lr,
                            float from, float to, float dur, Color color, int seg)
    {
        float t = 0f;
        while (t < dur && go)
        {
            t += Time.deltaTime;
            float k = t / dur;
            float r = Mathf.Lerp(from, to, 1f - (1f - k) * (1f - k)); // ease-out
            for (int i = 0; i <= seg; i++)
            {
                float a = i / (float)seg * Mathf.PI * 2f;
                lr.SetPosition(i, new Vector3(Mathf.Cos(a) * r, Mathf.Sin(a) * r, 0f));
            }
            var c = color; c.a = 1f - k;
            lr.startColor = lr.endColor = c;
            lr.startWidth = lr.endWidth = Mathf.Lerp(0.10f, 0.02f, k);
            yield return null;
        }
        if (go) Destroy(go);
    }

    // ── 파편 큐브 ────────────────────────────────────────
    IEnumerator Debris(Vector3 pos, Color color, int count)
    {
        var pieces = new Transform[count];
        var vels   = new Vector3[count];
        var spins  = new Vector3[count];

        for (int i = 0; i < count; i++)
        {
            var c = BrickBreakerMeshes.Make("FX_Debris", BrickBreakerMeshes.Cube, SpriteMat(color));
            c.transform.position   = pos + Random.insideUnitSphere * 0.3f;
            c.transform.localScale = Vector3.one * Random.Range(0.14f, 0.30f);
            pieces[i] = c.transform;
            vels[i]   = Random.onUnitSphere * Random.Range(3f, 7f);
            spins[i]  = Random.onUnitSphere * Random.Range(180f, 540f);
        }

        float t = 0f, dur = 0.7f;
        while (t < dur)
        {
            t += Time.deltaTime;
            float k = 1f - t / dur;
            for (int i = 0; i < count; i++)
            {
                if (!pieces[i]) continue;
                vels[i] += Vector3.down * 9f * Time.deltaTime;
                pieces[i].position += vels[i] * Time.deltaTime;
                pieces[i].Rotate(spins[i] * Time.deltaTime, Space.World);
                pieces[i].localScale = Vector3.one * Mathf.Max(0.01f, 0.25f * k);
            }
            yield return null;
        }
        for (int i = 0; i < count; i++) if (pieces[i]) Destroy(pieces[i].gameObject);
    }

    // ── 월드 팝업 텍스트 (점수 등) ───────────────────────
    public void Popup(Vector3 worldPos, string text, Color color, float size = 3.2f)
    {
        var go = new GameObject("FX_Popup");
        go.transform.position = worldPos;
        var tmp = go.AddComponent<TextMeshPro>();
        tmp.font      = font;
        tmp.text      = text;
        tmp.fontSize  = size;
        tmp.color     = color;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = FontStyles.Bold;
        var rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(6f, 2f);
        StartCoroutine(PopupRoutine(go, tmp));
    }

    IEnumerator PopupRoutine(GameObject go, TextMeshPro tmp)
    {
        float t = 0f, dur = 0.85f;
        Vector3 start = go.transform.position;
        Color   c0    = tmp.color;

        while (t < dur && go)
        {
            t += Time.deltaTime;
            float k = t / dur;

            go.transform.position = start + Vector3.up * (1.8f * Mathf.Sqrt(k)) - Vector3.forward * 0.4f;
            if (cam) go.transform.rotation = cam.transform.rotation;

            // 처음에 튀어오르고 끝에 사라진다
            float pop = k < 0.18f ? Mathf.Lerp(0.4f, 1.25f, k / 0.18f)
                                  : Mathf.Lerp(1.25f, 1f, Mathf.InverseLerp(0.18f, 0.4f, k));
            go.transform.localScale = Vector3.one * Mathf.Max(pop, 1f);

            var c = c0; c.a = k > 0.6f ? Mathf.InverseLerp(1f, 0.6f, k) : 1f;
            tmp.color = c;
            yield return null;
        }
        if (go) Destroy(go);
    }
}
