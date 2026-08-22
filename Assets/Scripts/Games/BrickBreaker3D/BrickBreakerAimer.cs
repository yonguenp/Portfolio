using System.Collections.Generic;
using UnityEngine;

public class BrickBreakerAimer : MonoBehaviour
{
    const int   MaxBounces = 12;
    const float DotSpacing = 0.35f;
    const int   DotsPerSeg = 30;
    /// <summary>공 반지름은 파워업으로 변한다. 예측선이 옛 값을 쓰면 거짓말이 된다.</summary>
    static float BALL_R => BrickBreakerManager.Instance != null
        ? BrickBreakerManager.Instance.BallRadius
        : BrickBreakerManager.BALL_BASE_RADIUS;

    float Lx, Rx, By, Ty, Bz;

    // ── 공유 머티리얼 ────────────────────────────────────
    // 조준선은 매 프레임 다시 그려진다. 여기서 new Material을 하면
    // GameObject를 Destroy해도 머티리얼은 남아(=Destroy는 머티리얼을 안 지운다)
    // UnloadUnusedAssets가 도는 **씬 로드 시점까지** 쌓인다.
    // 초당 수백 개가 누적돼 재시작 로딩이 길어지던 원인.
    // 색은 LineRenderer의 정점 색(startColor/endColor)으로 주므로 하나면 충분하다.
    static Shader   spriteShader;
    static Material lineMat;     // 일반 큐
    static Material shadowMat;   // 바닥/벽 그림자 큐(2950)

    static Shader SpriteShader =>
        spriteShader != null ? spriteShader : (spriteShader = Shader.Find("Sprites/Default"));

    static Material LineMat
    {
        get
        {
            if (lineMat == null)
                lineMat = new Material(SpriteShader) { hideFlags = HideFlags.HideAndDontSave };
            return lineMat;
        }
    }

    static Material ShadowMat
    {
        get
        {
            if (shadowMat == null)
                shadowMat = new Material(SpriteShader)
                { renderQueue = 2950, hideFlags = HideFlags.HideAndDontSave };
            return shadowMat;
        }
    }
    // 라인도 풀링한다. 점선으로 바꾸면 매 프레임 수십 개가 생겼다 사라지는데
    // Destroy로 돌리면 GC와 UnloadUnusedAssets 부담이 그대로 커진다.
    readonly List<LineRenderer> linePool = new();
    int lineUsed;

    readonly List<GameObject> dots = new();

    // 마커는 색이 제각각이라 머티리얼을 공유할 수 없다 → 풀링해서 재사용한다.
    // (매 프레임 CreatePrimitive + new Material 하던 것을 없앤다)
    readonly List<Renderer> markerPool = new();
    readonly List<Material> markerMats = new();
    int markerUsed;
    readonly List<BrickBreakerBrick> aimedTargets = new();

    public void SetBounds(float lx, float rx, float by, float ty, float bz)
    { Lx = lx; Rx = rx; By = by; Ty = ty; Bz = bz; }

    public void ShowAim(Vector3 origin, Vector3 direction)
    {
        ClearDots();
        if (direction.z < 0.01f) return;

        // 시작점 마커는 두지 않는다 — 같은 자리에 대기 볼(ReadyBall)이 항상 있어서
        // 노란 구슬이 흰 공 앞에 겹쳐 보인다.

        Vector3 pos = origin;
        Vector3 vel = direction.normalized;
        float used  = 0f;
        float limit = DotSpacing * DotsPerSeg * MaxBounces;

        for (int bounce = 0; bounce < MaxBounces && used < limit; bounce++)
        {
            if (vel.z <= 0f) break;

            float dLx = vel.x < 0 ? (Lx - pos.x) / vel.x : float.MaxValue;
            float dRx = vel.x > 0 ? (Rx - pos.x) / vel.x : float.MaxValue;
            float dBy = vel.y < 0 ? (By - pos.y) / vel.y : float.MaxValue;
            float dTy = vel.y > 0 ? (Ty - pos.y) / vel.y : float.MaxValue;
            float dBz = vel.z > 0 ? (Bz - pos.z) / vel.z : float.MaxValue;
            float wallDist = Mathf.Min(dLx, dRx, dBy, dTy, dBz);

            var (brickDist, brickNorm, brickHit) = FindBrickHit(pos, vel);

            bool  hitsBrick = brickDist < wallDist - 0.001f && brickDist < (limit - used) - 0.001f;
            bool  hitsWall  = !hitsBrick && wallDist < (limit - used) - 0.001f;
            bool  hitsZWall = hitsWall && Mathf.Approximately(wallDist, dBz);
            float segLen    = hitsBrick ? brickDist : Mathf.Min(wallDist, limit - used);

            Vector3 endPos = pos + vel * segLen;
            float   alpha  = Mathf.Lerp(0.80f, 0.12f, used / limit);

            // ── Air trajectory line ────────────────────────
            SpawnSegLine(pos, endPos,
                new Color(1f, 1f, 1f, alpha), 0.065f, 10);
            SpawnAimShadows(pos, endPos, alpha);

            MarkItemsOnSegment(pos, vel, segLen);
            if (hitsBrick) MarkTarget(brickHit);

            pos   = endPos;
            used += segLen;

            // ── Bounce marker ──────────────────────────────
            if ((hitsBrick || hitsWall) && used < limit)
            {
                if (hitsBrick)
                    SpawnCrosshair(pos, brickNorm, new Color(1f, 0.25f, 0.25f, 0.95f), 0.75f);
                else
                {
                    Color mc = hitsZWall
                        ? new Color(0.3f, 0.65f, 1f,  0.95f)
                        : new Color(1f,   0.45f, 0.1f, 0.95f);
                    SpawnMarker(pos, DepthTint(mc, pos.z), 0.13f);
                }
            }

            // ── Reflect ────────────────────────────────────
            if (hitsBrick)
            {
                // 법선 기준 반사로 통일한다. 박스는 법선이 축 정렬이라
                // 예전의 "해당 축만 뒤집기"와 결과가 같고, 구형까지 함께 처리된다.
                vel = Vector3.Reflect(vel, brickNorm);
            }
            else
            {
                float hit = Mathf.Min(dLx, dRx, dBy, dTy, dBz);
                if      (Mathf.Approximately(hit, dLx) || Mathf.Approximately(hit, dRx)) vel.x = -vel.x;
                else if (Mathf.Approximately(hit, dBy) || Mathf.Approximately(hit, dTy)) vel.y = -vel.y;
                else vel.z = -vel.z;
            }
        }
    }

    public void HideAim() => ClearDots();

    // ── Brick ray-cast (expanded AABB slab test) ─────────
    (float dist, Vector3 normal, BrickBreakerBrick brick) FindBrickHit(Vector3 pos, Vector3 vel)
    {
        float             minT      = float.MaxValue;
        Vector3           hitNormal = Vector3.zero;
        BrickBreakerBrick hitBrick  = null;

        var bricks = BrickBreakerManager.Instance?.Bricks;
        if (bricks == null) return (float.MaxValue, Vector3.zero, null);

        foreach (var br in bricks)
        {
            if (br == null || br.Dead || br.IsBallItem) continue;

            Vector3 bp  = br.transform.position;

            // 구형 브릭 — 레이·구 교차. 공 물리와 같은 규칙을 써야 예측이 안 틀린다.
            if (br.IsSphere)
            {
                float   rSum = BrickBreakerBrick.SphereR + BALL_R;
                Vector3 oc   = pos - bp;
                float   b2   = Vector3.Dot(oc, vel);
                float   c2   = Vector3.Dot(oc, oc) - rSum * rSum;
                float   disc = b2 * b2 - c2;
                if (disc < 0f) continue;

                float tHit = -b2 - Mathf.Sqrt(disc);
                if (tHit < 0.001f || tHit >= minT) continue;

                minT      = tHit;
                hitBrick  = br;
                hitNormal = ((pos + vel * tHit) - bp).normalized;
                continue;
            }

            // 박스·정사면체 — 브릭이 제공하는 공용 볼록 레이캐스트.
            // 회전한 브릭도 로컬 공간에서 처리되므로 그대로 맞는다.
            if (br.RaycastConvex(pos, vel, BALL_R, out float tc, out Vector3 nc))
            {
                if (tc < 0.001f || tc >= minT) continue;
                minT      = tc;
                hitBrick  = br;
                hitNormal = nc;
            }
        }

        return (minT, hitNormal, hitBrick);
    }

    static bool SlabTest(float rPos, float rDir, float center, float halfExt,
                         out float tEnter, out float tExit)
    {
        if (Mathf.Abs(rDir) < 1e-6f)
        {
            if (Mathf.Abs(rPos - center) >= halfExt) { tEnter = tExit = 0; return false; }
            tEnter = -1e9f; tExit = 1e9f;
            return true;
        }
        float t1 = (center - halfExt - rPos) / rDir;
        float t2 = (center + halfExt - rPos) / rDir;
        tEnter = Mathf.Min(t1, t2);
        tExit  = Mathf.Max(t1, t2);
        return true;
    }

    LineRenderer GetLine(Material mat, float width, int order)
    {
        while (lineUsed >= linePool.Count)
        {
            var go = new GameObject("AimSeg");
            go.transform.SetParent(transform);
            var l = go.AddComponent<LineRenderer>();
            l.useWorldSpace  = true;
            l.numCapVertices = 2;
            linePool.Add(l);
        }

        var lr = linePool[lineUsed++];
        if (!lr.gameObject.activeSelf) lr.gameObject.SetActive(true);
        lr.positionCount  = 2;
        lr.sharedMaterial = mat;
        lr.startWidth     = lr.endWidth = width;
        lr.sortingOrder   = order;
        return lr;
    }

    // ── Segment line ─────────────────────────────────────
    /// <summary>
    /// 조준선도 브릭·공과 같은 공기원근을 탄다. 흰색 단일 밝기면
    /// 선이 터널 어느 깊이를 지나는지 읽을 수가 없다.
    /// </summary>
    static Color DepthTint(Color c, float z)
    {
        float k = Mathf.Clamp01(BrickBreakerBrick.DepthShade(z, 0.30f));
        return new Color(c.r * k, c.g * k, c.b * k, c.a);
    }

    const float DASH_LEN = 0.42f;
    const float DASH_GAP = 0.30f;
    const int   MAX_DASH = 26;

    /// <summary>
    /// 넓고 옅은 글로우 한 줄 + 그 위에 밝은 점선. 단색 실선 하나보다
    /// 진행 방향과 거리감이 잘 읽힌다.
    /// </summary>
    void SpawnSegLine(Vector3 a, Vector3 b, Color color, float width, int sortOrder)
    {
        Vector3 seg = b - a;
        float   len = seg.magnitude;
        if (len < 1e-4f) return;
        Vector3 dir = seg / len;

        var glow = GetLine(LineMat, width * 3.2f, sortOrder - 1);
        glow.SetPosition(0, a);
        glow.SetPosition(1, b);
        var gc = color; gc.a *= 0.16f;
        glow.startColor = DepthTint(gc, a.z);
        glow.endColor   = DepthTint(gc, b.z);

        float step = DASH_LEN + DASH_GAP;
        int   n    = Mathf.Min(MAX_DASH, Mathf.CeilToInt(len / step));
        for (int i = 0; i < n; i++)
        {
            float s0 = i * step;
            if (s0 >= len) break;
            float s1 = Mathf.Min(s0 + DASH_LEN, len);

            var d = GetLine(LineMat, width, sortOrder);
            d.SetPosition(0, a + dir * s0);
            d.SetPosition(1, a + dir * s1);

            // 진행 방향으로 옅어지고(알파), 멀수록 어두워진다(깊이)
            Vector3 p0 = a + dir * s0, p1 = a + dir * s1;
            var c0 = color; c0.a *= 1f - 0.45f * (s0 / len);
            var c1 = color; c1.a *= 1f - 0.45f * (s1 / len);
            d.startColor = DepthTint(c0, p0.z);
            d.endColor   = DepthTint(c1, p1.z);
        }
    }

    // ── 바닥 투영 그림자 ──────────────────────────────────
    /// <summary>
    /// 조준 궤적을 터널 바닥에 내려 그린다. 축 방향으로 보는 게임이라
    /// 공중의 선만으로는 그 선이 터널 어느 깊이를 지나는지 읽을 수 없다.
    /// 바닥에 붙은 그림자는 z를 직접 보여준다.
    /// </summary>
    void SpawnAimShadows(Vector3 a, Vector3 b, float alpha)
    {
        const float lift = 0.04f;   // 면 살짝 위 (z-fighting 방지)
        float spanY = Ty - By;
        float spanX = Rx - Lx;

        // 바닥 / 천장 — y 거리로 농도가 정해진다
        Proj(new Vector3(a.x, By + lift, a.z), new Vector3(b.x, By + lift, b.z),
             alpha, (a.y - By) / spanY, (b.y - By) / spanY, a.z, b.z);
        Proj(new Vector3(a.x, Ty - lift, a.z), new Vector3(b.x, Ty - lift, b.z),
             alpha, (Ty - a.y) / spanY, (Ty - b.y) / spanY, a.z, b.z);

        // 좌 / 우 벽 — x 거리로 농도가 정해진다
        Proj(new Vector3(Lx + lift, a.y, a.z), new Vector3(Lx + lift, b.y, b.z),
             alpha, (a.x - Lx) / spanX, (b.x - Lx) / spanX, a.z, b.z);
        Proj(new Vector3(Rx - lift, a.y, a.z), new Vector3(Rx - lift, b.y, b.z),
             alpha, (Rx - a.x) / spanX, (Rx - b.x) / spanX, a.z, b.z);
    }

    // 그 면에 붙어 있을수록 진하다 (블롭 그림자와 같은 규칙)
    const float SHADOW_NEAR = 0.60f;
    const float SHADOW_FAR  = 0.12f;

    /// <param name="ka">시작점의 해당 벽까지 거리 (0=붙음, 1=반대편)</param>
    /// <param name="kb">끝점의 해당 벽까지 거리</param>
    void Proj(Vector3 a, Vector3 b, float alpha, float ka, float kb, float za, float zb)
    {
        var lr = GetLine(ShadowMat, 0.075f, 0);   // 블롭 그림자와 같은 큐
        lr.SetPosition(0, a);
        lr.SetPosition(1, b);

        // 벽까지 거리 + 면 자체의 거리 페이드를 둘 다 곱한다
        float aa = alpha * Mathf.Lerp(SHADOW_NEAR, SHADOW_FAR, Mathf.Clamp01(ka))
                         * BrickBreakerShadows.GroundFadeAt(za);
        float ab = alpha * Mathf.Lerp(SHADOW_NEAR, SHADOW_FAR, Mathf.Clamp01(kb))
                         * BrickBreakerShadows.GroundFadeAt(zb);
        lr.startColor = new Color(0f, 0f, 0f, aa);
        lr.endColor   = new Color(0f, 0f, 0f, ab);
    }

    // ── Crosshair (brick impact) ──────────────────────────
    /// <summary>
    /// 충돌 지점 표시. 법선에 수직인 정규직교 축을 만들어 **면에 딱 붙은 링**을 그린다.
    /// 예전엔 normal.z!=0 같은 축 정렬 가정이라 회전 브릭·구형에서 축이 엉뚱했다.
    /// </summary>
    void SpawnCrosshair(Vector3 worldPos, Vector3 normal, Color color, float size)
    {
        Vector3 n = normal.sqrMagnitude > 1e-6f ? normal.normalized : Vector3.back;

        // 법선과 안 나란한 아무 벡터로 직교 기저를 만든다
        Vector3 seed = Mathf.Abs(n.y) < 0.9f ? Vector3.up : Vector3.right;
        Vector3 a = Vector3.Cross(n, seed).normalized;
        Vector3 b = Vector3.Cross(n, a);

        // 충돌 지점은 **공 반지름만큼 확장한** 도형으로 구한 값이라 공 중심 위치다.
        // 그대로 그리면 브릭에서 반지름만큼 떠 보인다 → 표면으로 끌어당긴다.
        Vector3 front = worldPos - n * (BALL_R - 0.04f);

        var rc = DepthTint(color, front.z);

        const int SEG = 16;
        float r = size * 0.34f;                 // 예전 0.62 — 과녁이 컸다
        var ring = GetLine(LineMat, 0.045f, 11);
        ring.positionCount = SEG + 1;
        for (int i = 0; i <= SEG; i++)
        {
            float t = i / (float)SEG * Mathf.PI * 2f;
            ring.SetPosition(i, front + (a * Mathf.Cos(t) + b * Mathf.Sin(t)) * r);
        }
        ring.startColor = ring.endColor = rc;

        // 짧은 조준 틱 4개
        for (int i = 0; i < 4; i++)
        {
            float t = i * Mathf.PI * 0.5f;
            Vector3 dir = a * Mathf.Cos(t) + b * Mathf.Sin(t);
            SpawnLine(front + dir * r * 1.25f, front + dir * r * 1.85f, rc);
        }
    }

    void SpawnLine(Vector3 a, Vector3 b, Color color)
    {
        var lr = GetLine(LineMat, 0.07f, 10);
        lr.SetPosition(0, a); lr.SetPosition(1, b);
        lr.startColor = lr.endColor = color;
    }

    // ── Marker (sphere) ───────────────────────────────────
    void SpawnMarker(Vector3 worldPos, Color color, float size)
    {
        // 예전엔 여기서 z를 0.45 당겼는데, 옆벽·천장 충돌에선 그 방향이
        // 표면과 무관해서 마커만 엉뚱하게 떠 보였다.

        while (markerUsed >= markerPool.Count)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Destroy(go.GetComponent<Collider>());
            go.transform.SetParent(transform);

            var m = new Material(SpriteShader) { hideFlags = HideFlags.HideAndDontSave };
            var r = go.GetComponent<Renderer>();
            r.material     = m;     // 이 풀 항목이 끝까지 재사용한다
            r.sortingOrder = 10;

            markerPool.Add(r);
            markerMats.Add(m);
        }

        var rend = markerPool[markerUsed];
        markerMats[markerUsed].color = color;
        rend.transform.position   = worldPos;
        rend.transform.localScale = Vector3.one * size;
        if (!rend.gameObject.activeSelf) rend.gameObject.SetActive(true);
        markerUsed++;
    }

    void ClearDots()
    {
        foreach (var d in dots) if (d) Destroy(d);
        dots.Clear();

        for (int i = 0; i < linePool.Count; i++)
            if (linePool[i] && linePool[i].gameObject.activeSelf)
                linePool[i].gameObject.SetActive(false);
        lineUsed = 0;

        // 마커는 지우지 않고 꺼둔다 (다음 프레임에 다시 켜서 재사용)
        for (int i = 0; i < markerPool.Count; i++)
            if (markerPool[i] && markerPool[i].gameObject.activeSelf)
                markerPool[i].gameObject.SetActive(false);
        markerUsed = 0;
        foreach (var t in aimedTargets) if (t) t.SetAimTargeted(false);
        aimedTargets.Clear();
    }

    // ── 아이템 관통 판정 ──────────────────────────────────
    /// <summary>
    /// 이 구간이 관통하는 BallAdd 아이템을 노랗게 표시한다.
    /// 아이템은 볼을 튕겨내지 않으므로(그냥 통과) 궤적 계산에는 넣지 않고
    /// 표시만 한다 — <see cref="FindBrickHit"/>가 아이템을 건너뛰는 이유.
    /// </summary>
    void MarkItemsOnSegment(Vector3 origin, Vector3 dir, float segLen)
    {
        var bricks = BrickBreakerManager.Instance?.Bricks;
        if (bricks == null || segLen <= 0f) return;

        foreach (var br in bricks)
        {
            if (br == null || br.Dead || !br.IsBallItem) continue;
            if (!SegmentHitsItem(origin, dir, segLen, br.transform.position)) continue;
            MarkTarget(br);
        }
    }

    /// <summary>같은 대상을 두 번 표시하지 않는다 (해제도 한 번만 하면 되도록).</summary>
    void MarkTarget(BrickBreakerBrick target)
    {
        if (target == null || target.Dead || aimedTargets.Contains(target)) return;
        target.SetAimTargeted(true);
        aimedTargets.Add(target);
    }

    /// <summary>볼이 실제로 아이템을 획득할 때 쓰는 것과 같은 확장 AABB로 검사한다.</summary>
    static bool SegmentHitsItem(Vector3 origin, Vector3 dir, float segLen, Vector3 center)
    {
        float exX = BrickBreakerBrick.HalfX + BALL_R;
        float exY = BrickBreakerBrick.HalfY + BALL_R;
        float exZ = BrickBreakerBrick.HalfZ + BALL_R;

        if (!SlabTest(origin.x, dir.x, center.x, exX, out float txE, out float txX)) return false;
        if (!SlabTest(origin.y, dir.y, center.y, exY, out float tyE, out float tyX)) return false;
        if (!SlabTest(origin.z, dir.z, center.z, exZ, out float tzE, out float tzX)) return false;

        float tEnter = Mathf.Max(txE, tyE, tzE);
        float tExit  = Mathf.Min(txX, tyX, tzX);

        if (tEnter > tExit || tExit < 0f) return false;
        return tEnter <= segLen;   // 이 구간 안에서 만나야 한다
    }
}
