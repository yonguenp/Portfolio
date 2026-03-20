using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Kenney Particle Pack 스프라이트를 사용한 팡팡 이펙트 매니저.
/// EffectManager.Instance.SpawnXxx(pos) 로 어디서든 호출.
/// </summary>
public class EffectManager : MonoBehaviour
{
    public static EffectManager Instance { get; private set; }

    // ── 파티클 풀 ─────────────────────────────────────────────
    [Header("Pool")]
    public int poolSize = 20;

    private Queue<ParticleSystem> _pool = new Queue<ParticleSystem>();

    // ── 스프라이트 캐시 ──────────────────────────────────────
    private Sprite _star1, _star2, _sparkle1, _sparkle2;
    private Sprite _magic1, _magic2, _magic3;
    private Sprite _dirt1,  _dirt2;
    private Sprite _smoke1, _smoke2;
    private Sprite _flame1, _flame2;
    private Sprite _light1;
    private Sprite _scorch1;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadSprites();
        BuildPool();
    }

    void LoadSprites()
    {
        const string P = "VFX/Particles/";
        _star1    = Load(P + "star_01");
        _star2    = Load(P + "star_02");
        _sparkle1 = Load(P + "spark_01");
        _sparkle2 = Load(P + "spark_02");
        _magic1   = Load(P + "magic_01");
        _magic2   = Load(P + "magic_02");
        _magic3   = Load(P + "magic_03");
        _dirt1    = Load(P + "dirt_01");
        _dirt2    = Load(P + "dirt_02");
        _smoke1   = Load(P + "smoke_01");
        _smoke2   = Load(P + "smoke_02");
        _flame1   = Load(P + "flame_01");
        _flame2   = Load(P + "flame_02");
        _light1   = Load(P + "light_01");
        _scorch1  = Load(P + "scorch_01");
    }

    static Sprite Load(string path) => Resources.Load<Sprite>(path);

    void BuildPool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            var ps = CreatePS("PooledEffect");
            ps.gameObject.SetActive(false);
            _pool.Enqueue(ps);
        }
    }

    ParticleSystem GetPS(Vector3 pos)
    {
        ParticleSystem ps;
        if (_pool.Count > 0)
        {
            ps = _pool.Dequeue();
            ps.transform.position = pos;
            ps.gameObject.SetActive(true);
        }
        else
        {
            ps = CreatePS("DynEffect");
            ps.transform.position = pos;
        }
        return ps;
    }

    void ReturnPS(ParticleSystem ps)
    {
        ps.gameObject.SetActive(false);
        _pool.Enqueue(ps);
    }

    // ── 공개 이펙트 API ──────────────────────────────────────

    /// 코인 수집 — 골드 별 + 스파클
    public void SpawnCoinCollect(Vector3 pos)
    {
        SpawnBurst(pos,
            sprites:   new[] { _star1, _star2, _sparkle1, _sparkle2 },
            count:     12,
            speed:     4f,
            size:      0.35f,
            lifeMin:   0.35f,
            lifeMax:   0.65f,
            startCol:  new Color(1f, 0.9f, 0.2f),
            endCol:    new Color(1f, 0.6f, 0f, 0f));
    }

    /// 적 처치 — 불꽃 + 스모크
    public void SpawnEnemyDefeat(Vector3 pos)
    {
        SpawnBurst(pos,
            sprites:   new[] { _flame1, _flame2, _smoke1, _scorch1 },
            count:     14,
            speed:     5f,
            size:      0.5f,
            lifeMin:   0.3f,
            lifeMax:   0.7f,
            startCol:  new Color(1f, 0.5f, 0.1f),
            endCol:    new Color(0.3f, 0.3f, 0.3f, 0f));
    }

    /// 점프 착지 — 먼지
    public void SpawnJumpDust(Vector3 pos)
    {
        SpawnBurst(pos,
            sprites:   new[] { _dirt1, _dirt2, _smoke1 },
            count:     6,
            speed:     2.5f,
            size:      0.28f,
            lifeMin:   0.2f,
            lifeMax:   0.4f,
            startCol:  new Color(0.8f, 0.65f, 0.4f),
            endCol:    new Color(0.8f, 0.65f, 0.4f, 0f));
    }

    /// 파워업 획득 — 마법 + 빛
    public void SpawnPowerUp(Vector3 pos, Color tint)
    {
        SpawnBurst(pos,
            sprites:   new[] { _magic1, _magic2, _magic3, _light1 },
            count:     16,
            speed:     4.5f,
            size:      0.4f,
            lifeMin:   0.4f,
            lifeMax:   0.8f,
            startCol:  tint,
            endCol:    new Color(tint.r, tint.g, tint.b, 0f));
    }

    /// 부활 — 큰 마법 버스트
    public void SpawnRevive(Vector3 pos)
    {
        SpawnBurst(pos,
            sprites:   new[] { _magic1, _magic2, _magic3, _star1, _star2, _light1 },
            count:     24,
            speed:     6f,
            size:      0.55f,
            lifeMin:   0.5f,
            lifeMax:   1.0f,
            startCol:  new Color(0.4f, 0.8f, 1f),
            endCol:    new Color(0.4f, 0.8f, 1f, 0f));
    }

    // ── 내부: 스프라이트 버스트 파티클 생성 ──────────────────
    void SpawnBurst(Vector3 pos, Sprite[] sprites, int count,
                    float speed, float size,
                    float lifeMin, float lifeMax,
                    Color startCol, Color endCol)
    {
        if (sprites == null || sprites.Length == 0) return;
        StartCoroutine(BurstCo(pos, sprites, count, speed, size,
                               lifeMin, lifeMax, startCol, endCol));
    }

    IEnumerator BurstCo(Vector3 pos, Sprite[] sprites, int count,
                         float speed, float size,
                         float lifeMin, float lifeMax,
                         Color startCol, Color endCol)
    {
        var ps   = GetPS(pos);
        var main = ps.main;
        var em   = ps.emission;
        var sh   = ps.shape;
        var col  = ps.colorOverLifetime;
        var renderer = ps.GetComponent<ParticleSystemRenderer>();

        // 스프라이트 중 하나 랜덤 선택해서 머티리얼에 적용
        Sprite spr = sprites[Random.Range(0, sprites.Length)];
        if (spr != null && renderer != null)
        {
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            if (renderer.material == null || renderer.material.name == "Default-Particle")
            {
                var mat = new Material(Shader.Find("Sprites/Default"));
                mat.mainTexture = spr.texture;
                renderer.material = mat;
            }
            else
            {
                renderer.material.mainTexture = spr.texture;
            }
        }

        main.startLifetime    = new ParticleSystem.MinMaxCurve(lifeMin, lifeMax);
        main.startSpeed       = new ParticleSystem.MinMaxCurve(speed * 0.6f, speed);
        main.startSize        = new ParticleSystem.MinMaxCurve(size * 0.7f, size * 1.3f);
        main.startColor       = startCol;
        main.gravityModifier  = 0.4f;
        main.simulationSpace  = ParticleSystemSimulationSpace.World;
        main.maxParticles     = count * 2;

        col.enabled = true;
        var grad = new Gradient();
        grad.SetKeys(
            new[] { new GradientColorKey(startCol, 0f), new GradientColorKey(endCol, 1f) },
            new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) });
        col.color = new ParticleSystem.MinMaxGradient(grad);

        sh.shapeType = ParticleSystemShapeType.Circle;
        sh.radius    = 0.15f;

        em.enabled = false;
        ps.Clear();
        ps.Emit(count);

        // 가장 긴 수명만큼 대기 후 반환
        yield return new WaitForSeconds(lifeMax + 0.1f);
        ReturnPS(ps);
    }

    // ── 파티클 시스템 생성 헬퍼 ──────────────────────────────
    ParticleSystem CreatePS(string name)
    {
        var go = new GameObject(name);
        go.transform.SetParent(transform);
        var ps   = go.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.playOnAwake = false;
        main.loop        = false;

        var em = ps.emission;
        em.enabled = false;
        return ps;
    }
}
