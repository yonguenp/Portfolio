using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 터널 표면 + 바닥 블롭 그림자.
///
/// 이 게임의 오브젝트는 전부 Sprites/Default — 언릿이고 ZWrite도 꺼져 있어서
/// 라이트를 켜도 실제 섀도맵은 나오지 않는다. 게다가 터널은 LineRenderer
/// 와이어프레임뿐이라 그림자가 떨어질 표면 자체가 없었다. 배경이 거의 검정
/// (0.04, 0.04, 0.10)이라 검은 그림자를 깔아도 아무 변화가 없다.
///
/// 그래서 두 가지를 같이 만든다.
///   1. 터널 표면 — 바닥/천장/좌우 벽 쿼드. 거리에 따라 배경으로 페이드해서
///      공기원근(aerial perspective)까지 얻는다. 그림자가 떨어질 곳이기도 하다.
///   2. 바닥 블롭 — "위에서 빛이 내리쬔다"고 가정하고 오브젝트 바로 아래
///      바닥에 소프트 원을 깐다. 위치가 x·z를, 크기와 농도가 높이(y)를 읽어준다.
///
/// 표면과 그림자가 같은 거리 페이드 곡선(<see cref="FadeAt"/>)을 써야
/// 표면이 사라진 먼 곳에 그림자만 둥둥 뜨지 않는다. 그래서 한 파일에 둔다.
///
/// 파이프라인·셰이더 변경 없이 동작하고, 쿼드는 풀링되므로 비용은
/// 드로우콜 몇 개뿐이다.
/// </summary>
public class BrickBreakerShadows : MonoBehaviour
{
    public static BrickBreakerShadows Instance { get; private set; }

    /// <summary>바닥 와이어라인(폭 0.04)과 겹쳐 깜빡이지 않도록 살짝 띄운다.</summary>
    const float LIFT = 0.03f;

    // 렌더 순서: 표면 → 그림자 → 나머지 전부(기본 Transparent 3000)
    const int QUEUE_SURFACE = 2900;
    const int QUEUE_SHADOW  = 2950;

    // 높이에 따른 그림자 변화. 높이 올라갈수록 크고 옅어진다.
    const float SCALE_LOW  = 1.15f;
    const float SCALE_HIGH = 1.80f;
    const float ALPHA_LOW  = 0.75f;
    const float ALPHA_HIGH = 0.18f;

    // 오브젝트별 블롭 기준 크기(월드 단위)
    const float SIZE_BRICK = 1.75f;
    const float SIZE_ITEM  = 0.90f;
    const float SIZE_BALL  = 0.55f;
    const float SIZE_CURSOR = 0.75f;

    // 거리 페이드. v=0(카메라 쪽) FADE_NEAR → v=1(터널 끝) FADE_FAR
    //
    // 페이드를 일찍 시작하면 브릭이 스폰되는 z=5S 부근에서 바닥이 이미
    // 배경에 묻혀 그림자가 안 보인다. 플레이 구간(z 0~5S)은 살려두고
    // 뒷벽 근처만 흐려지도록 늦게 시작한다.
    const float FADE_NEAR  = 0.95f;
    const float FADE_FAR   = 0.25f;
    const float FADE_START = 0.70f;

    // 카메라 쪽 끝도 같이 지운다. 안 그러면 카메라를 뒤로 빼거나 위에서 내려다볼 때
    // 표면 쿼드의 각진 앞 모서리가 그대로 보인다.
    // 화면 하단이 균일한 밝은 판때기로 차지 않도록 넉넉히 잡는다.
    // 발사 지점(v≈0.42)은 이 구간 밖이라 대기 볼 그림자는 영향받지 않는다.
    const float FADE_IN_END = 0.28f;

    static readonly Quaternion FloorRot = Quaternion.LookRotation(Vector3.up,    Vector3.forward);
    static readonly Quaternion SideRot  = Quaternion.LookRotation(Vector3.right, Vector3.forward);

    float floorY, ceilY, leftX, rightX;
    float tunnelH, tunnelW, zNear, zFar;

    Texture2D blobTex;
    Texture2D gradTex;
    Shader    spriteShader;

    readonly List<Transform> quads = new();
    readonly List<Material>  mats  = new();
    readonly List<Material>  surfaceMats = new();
    int used;

    void Awake()
    {
        Instance     = this;
        spriteShader = Shader.Find("Sprites/Default");
        blobTex      = MakeBlobTex(64);
        gradTex      = MakeGradTex(64);
    }

    /// <param name="zBack">터널 뒷벽 z</param>
    /// <param name="zFront">터널 표면이 시작하는 z (DrawTunnel의 z0와 맞춘다)</param>
    public void Configure(float leftWall, float rightWall, float bottomWall, float topWall,
                          float zFront, float zBack)
    {
        floorY  = bottomWall;
        ceilY   = topWall;
        leftX   = leftWall;
        rightX  = rightWall;
        tunnelH = Mathf.Max(0.001f, topWall  - bottomWall);
        tunnelW = Mathf.Max(0.001f, rightWall - leftWall);
        zNear   = zFront;
        zFar    = zBack;

        BuildSurfaces(leftWall, rightWall, bottomWall, topWall);
    }

    // ── 터널 표면 ────────────────────────────────────────
    void BuildSurfaces(float left, float right, float bottom, float top)
    {
        float depth = zFar - zNear;
        float cz    = (zFar + zNear) * 0.5f;
        float w     = right - left;
        float h     = top - bottom;

        Surface("Floor",     new Vector3(0f, bottom, cz), FloorRot, new Vector3(w, depth, 1f), new Color(0.17f, 0.21f, 0.38f));
        // 천장·벽이 배경(0.04,0.04,0.10)에 가까우면 그 위의 그림자가 안 보인다.
        // 그림자를 4면에 깔려면 면 자체를 배경보다 띄워야 하는데, 천장은
        // 화면 상단을 크게 차지하면서 정보량은 적어 과하게 밝히면 터널이
        // 복도가 아니라 액자처럼 보인다. 좌우 벽은 면적이 작고 x를 알려주므로 더 밝게.
        Surface("Ceiling",   new Vector3(0f, top,    cz), FloorRot, new Vector3(w, depth, 1f), new Color(0.090f, 0.110f, 0.225f));
        Surface("WallLeft",  new Vector3(left,  0f,  cz), SideRot,  new Vector3(h, depth, 1f), new Color(0.130f, 0.155f, 0.300f));
        Surface("WallRight", new Vector3(right, 0f,  cz), SideRot,  new Vector3(h, depth, 1f), new Color(0.130f, 0.155f, 0.300f));
    }

    void Surface(string name, Vector3 pos, Quaternion rot, Vector3 scale, Color tint)
    {
        var mat = new Material(spriteShader)
        {
            mainTexture = gradTex,
            color       = tint,
            renderQueue = QUEUE_SURFACE,
        };
        var go = BrickBreakerMeshes.Make(name, QuadMesh, mat);
        go.transform.SetParent(transform, false);
        go.transform.SetPositionAndRotation(pos, rot);
        go.transform.localScale = scale;
        surfaceMats.Add(mat);
    }

    static Mesh quadMesh;
    static Mesh QuadMesh
    {
        get
        {
            if (quadMesh) return quadMesh;
            var probe = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quadMesh  = probe.GetComponent<MeshFilter>().sharedMesh;
            if (Application.isPlaying) Destroy(probe); else DestroyImmediate(probe);
            return quadMesh;
        }
    }

    // ── 블롭 그림자 ──────────────────────────────────────
    void LateUpdate()
    {
        var mgr = BrickBreakerManager.Instance;
        if (mgr == null) return;

        used = 0;

        var bricks = mgr.Bricks;
        if (bricks != null)
        {
            for (int i = 0; i < bricks.Count; i++)
            {
                var b = bricks[i];
                // Bricks 리스트는 AdvanceLayers에서만 정리되므로
                // 파괴된 항목(null/Dead)이 섞여 있다.
                if (b == null || b.Dead) continue;
                Place(b.transform.position, b.IsBallItem ? SIZE_ITEM : SIZE_BRICK);
            }
        }

        var balls = mgr.BallPool;
        if (balls != null)
        {
            for (int i = 0; i < balls.Count; i++)
            {
                var ball = balls[i];
                if (ball == null || !ball.gameObject.activeSelf) continue;
                Place(ball.transform.position, SIZE_BALL);
            }
        }

        // 조준 중 발사 지점의 대기 볼
        var ready = mgr.ReadyBall;
        if (ready != null) Place(ready.position, SIZE_BALL);

        // 조준 커서 — 네 벽의 그림자가 커서의 x·y·z를 그대로 읽어준다.
        // 커서 깊이를 눈으로 확인할 수 있는 유일한 수단이라 반드시 포함한다.
        var cursor = mgr.AimCursorTransform;
        if (cursor != null) Place(cursor.position, SIZE_CURSOR);

        for (int i = used; i < quads.Count; i++)
            if (quads[i] && quads[i].gameObject.activeSelf)
                quads[i].gameObject.SetActive(false);
    }

    /// <summary>
    /// 네 벽 모두에 그림자를 내린다. 각 벽까지의 거리로 농도·크기가 정해지므로
    /// (가까울수록 진하고 작다) 네 개를 같이 보면 x·y·z가 한 번에 읽힌다.
    /// </summary>
    void Place(Vector3 p, float size)
    {
        PlaceOne(new Vector3(p.x, floorY + LIFT, p.z), FloorRot, size, (p.y - floorY) / tunnelH, p.z);
        PlaceOne(new Vector3(p.x, ceilY  - LIFT, p.z), FloorRot, size, (ceilY - p.y) / tunnelH, p.z);
        PlaceOne(new Vector3(leftX  + LIFT, p.y, p.z), SideRot,  size, (p.x - leftX) / tunnelW, p.z);
        PlaceOne(new Vector3(rightX - LIFT, p.y, p.z), SideRot,  size, (rightX - p.x) / tunnelW, p.z);
    }

    /// <param name="k">그 벽까지의 거리 (0 = 붙어 있음, 1 = 반대편 벽)</param>
    void PlaceOne(Vector3 pos, Quaternion rot, float size, float k, float z)
    {
        k = Mathf.Clamp01(k);

        int idx = used++;
        EnsureQuad(idx);

        var t = quads[idx];
        if (t == null) return;
        if (!t.gameObject.activeSelf) t.gameObject.SetActive(true);

        t.SetPositionAndRotation(pos, rot);

        float s = size * Mathf.Lerp(SCALE_LOW, SCALE_HIGH, k);
        t.localScale = new Vector3(s, s, 1f);

        // 면이 배경으로 사라진 먼 곳에서는 그림자도 같이 사라져야 한다
        float depthFade = FadeAt(Mathf.InverseLerp(zNear, zFar, z)) / FADE_NEAR;
        mats[idx].color = new Color(0f, 0f, 0f, Mathf.Lerp(ALPHA_LOW, ALPHA_HIGH, k) * depthFade);
    }

    void EnsureQuad(int idx)
    {
        while (quads.Count <= idx)
        {
            var mat = new Material(spriteShader)
            {
                mainTexture = blobTex,
                renderQueue = QUEUE_SHADOW,
            };
            var go = BrickBreakerMeshes.Make("BlobShadow", QuadMesh, mat);
            go.transform.SetParent(transform, false);
            go.SetActive(false);

            quads.Add(go.transform);
            mats.Add(mat);
        }
    }

    // ── 텍스처 ───────────────────────────────────────────
    /// <summary>
    /// 주어진 월드 z에서 바닥이 얼마나 진한지 (0~1).
    /// 바닥에 얹는 다른 표시(에임 그림자 등)도 이 값을 곱해야
    /// 바닥이 배경으로 사라진 먼 곳에 표시만 둥둥 뜨지 않는다.
    /// </summary>
    public static float GroundFadeAt(float worldZ)
    {
        var inst = Instance;
        if (inst == null) return 1f;
        return FadeAt(Mathf.InverseLerp(inst.zNear, inst.zFar, worldZ)) / FADE_NEAR;
    }

    /// <summary>표면·그림자가 공유하는 거리 페이드 곡선. 양 끝에서 배경으로 사라진다.</summary>
    static float FadeAt(float v)
    {
        float far  = Mathf.Lerp(FADE_NEAR, FADE_FAR,
                                Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(FADE_START, 1f, v)));
        float near = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0f, FADE_IN_END, v));
        return far * near;
    }

    /// <summary>중심이 진하고 가장자리로 부드럽게 사라지는 원형 알파 텍스처.</summary>
    static Texture2D MakeBlobTex(int size)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
        {
            wrapMode   = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear,
        };

        float half = size * 0.5f;
        var px = new Color32[size * size];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = (x + 0.5f - half) / half;
                float dy = (y + 0.5f - half) / half;
                float d  = Mathf.Sqrt(dx * dx + dy * dy);
                // 반지름 0.30까지는 꽉 찬 코어, 그 밖은 1.0에서 0으로 부드럽게
                float a  = 1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.30f, 1f, d));
                px[y * size + x] = new Color32(255, 255, 255, (byte)(a * 255f));
            }
        }
        tex.SetPixels32(px);
        tex.Apply();
        return tex;
    }

    /// <summary>표면용 세로 그라디언트. v=0(가까움) 진함 → v=1(멀리) 배경으로 사라짐.</summary>
    static Texture2D MakeGradTex(int h)
    {
        var tex = new Texture2D(1, h, TextureFormat.RGBA32, false)
        {
            wrapMode   = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear,
        };

        var px = new Color32[h];
        for (int i = 0; i < h; i++)
        {
            float a = FadeAt(i / (float)(h - 1));
            px[i] = new Color32(255, 255, 255, (byte)(a * 255f));
        }
        tex.SetPixels32(px);
        tex.Apply();
        return tex;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
        if (blobTex) Destroy(blobTex);
        if (gradTex) Destroy(gradTex);
        for (int i = 0; i < mats.Count; i++)        if (mats[i])        Destroy(mats[i]);
        for (int i = 0; i < surfaceMats.Count; i++) if (surfaceMats[i]) Destroy(surfaceMats[i]);
    }
}
