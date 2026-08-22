using System;
using System.Collections;
using UnityEngine;

public class BrickBreakerBall : MonoBehaviour
{
    /// <summary>복귀 위치와 '이 공이 리더인가'를 함께 알린다.</summary>
    public event Action<Vector3, bool> OnReturned;

    /// <summary>다음 턴 발사 지점을 정하는 공. 눈에 띄게 금색으로 칠한다.</summary>
    public bool IsLeader { get; private set; }

    static readonly Color LeaderTint = new Color(1.00f, 0.82f, 0.25f);
    Color tint = Color.white;

    public void SetLeader(bool on)
    {
        IsLeader = on;
        tint     = on ? LeaderTint : Color.white;
    }

    // 반지름은 파워업으로 커진다. 충돌 판정과 겉보기 크기가 같이 움직여야 하고,
    // 조준 예측선도 같은 값을 써야 예측이 거짓말이 안 된다.
    float       R     = BrickBreakerManager.BALL_BASE_RADIUS;
    const float Speed = 22f;

    // 브릭(0.55)보다 덜 어두워지게 한다 — 공은 가장 오래 쫓는 대상이라
    // 터널 안쪽에서도 배경에 묻히면 안 된다.
    const float BALL_FAR_SHADE = 0.72f;

    // 얕게 쏜 공은 |v.z|가 작아 왕복이 4초를 넘는다(80° 기준). 브릭 위치와
    // 무관하게 이 시간이 지나면 무조건 가속해 긴 꼬리를 자른다.
    const float BOOST_AFTER = 2.0f;

    bool  speedBoosted;
    int   brickHits;      // 이번 발사에서 브릭을 때린 횟수 → 벽 소리가 굵어진다
    float flightTime;
    bool  rushRequested;   // 브릭이 다 사라졌다 → 지체 없이 빠져나온다
    float Lx, Rx, By, Ty, Bz, Rz;

    Coroutine     physRoutine;
    Renderer      rend;
    TrailRenderer trail;

    public void Fire(Vector3 origin, Vector3 dir,
        float lx, float rx, float by, float ty, float bz, float rz)
    {
        SetBounds(lx, rx, by, ty, bz, rz);
        speedBoosted = false;
        brickHits    = 0;
        flightTime   = 0f;
        rushRequested = false;

        var mgr = BrickBreakerManager.Instance;
        R = mgr != null ? mgr.BallRadius : BrickBreakerManager.BALL_BASE_RADIUS;
        // 기본 반지름 0.22 ↔ 겉보기 스케일 0.55
        transform.localScale = Vector3.one * (R * 2.5f);
        if (!rend)  rend  = GetComponent<Renderer>();
        if (!trail) trail = GetComponent<TrailRenderer>();
        gameObject.SetActive(true);
        transform.position = origin;
        // 풀에서 재사용하므로 지우지 않으면 이전 볼이 끝난 자리에서
        // 새 발사 지점까지 줄이 그어진다.
        if (trail) trail.Clear();
        ApplyDepthShade(origin.z);
        StopAll();
        physRoutine = StartCoroutine(PhysicsLoop(origin, dir.normalized * Speed));
    }

    /// <summary>브릭과 같은 곡선으로 거리에 따라 어두워진다 — 공의 z를 색으로 읽는다.</summary>
    void ApplyDepthShade(float z)
    {
        if (!rend) return;
        float b = BrickBreakerBrick.DepthShade(z, BALL_FAR_SHADE);
        rend.material.color = new Color(tint.r * b, tint.g * b, tint.b * b, 1f);
    }

    /// <summary>남은 브릭이 없을 때 매니저가 호출. 다음 프레임에 복귀 가속이 걸린다.</summary>
    public void RushHome() => rushRequested = true;

    public void ForceStop()
    {
        StopAll();
        if (trail) trail.Clear();
        gameObject.SetActive(false);
    }

    IEnumerator PhysicsLoop(Vector3 pos, Vector3 vel)
    {
        while (true)
        {
            float dt     = Time.deltaTime;
            float dist   = vel.magnitude * dt;
            int   steps  = Mathf.Max(1, Mathf.CeilToInt(dist / (R * 0.4f)));
            float stepDt = dt / steps;

            for (int s = 0; s < steps; s++)
            {
                pos += vel * stepDt;
                WallBounce(ref pos, ref vel);
                BrickResolve(ref pos, ref vel);
                if (pos.z < Rz)
                {
                    Return(pos);
                    yield break;
                }
            }

            // 브릭이 전부 사라지면 왕복할 이유가 없다 — 즉시 복귀 방향으로 튼다
            if (rushRequested && !speedBoosted)
            {
                vel = new Vector3(vel.x * 0.35f, vel.y * 0.35f, -Mathf.Abs(vel.z));
                vel = vel.normalized * Speed * 5f;
                speedBoosted = true;
            }

            // 가속 조건 두 가지
            //  · 복귀 중 남은 브릭을 모두 지나쳤다 (원래 조건)
            //  · 너무 오래 날고 있다 (얕게 쏜 공의 긴 꼬리를 자른다)
            flightTime += dt;
            if (!speedBoosted)
            {
                bool pastBricks = vel.z < 0f
                    && pos.z < (BrickBreakerManager.Instance?.GetFrontBrickZ() ?? float.MaxValue);

                if (pastBricks || flightTime > BOOST_AFTER)
                {
                    vel *= 3f;
                    speedBoosted = true;
                }
            }

            transform.position = pos;
            ApplyDepthShade(pos.z);
            yield return null;
        }
    }

    void BrickResolve(ref Vector3 pos, ref Vector3 vel)
    {
        var bricks = BrickBreakerManager.Instance?.Bricks;
        if (bricks == null) return;

        for (int i = bricks.Count - 1; i >= 0; i--)
        {
            var br = bricks[i];
            if (br == null || br.Dead) continue;

            Vector3 bp = br.transform.position;

            // ── 구형 브릭: 법선 기준 반사 ──────────────────
            // 축 정렬 반사와 달리 맞은 지점에 따라 튀는 방향이 연속적으로 달라진다.
            if (br.IsSphere)
            {
                Vector3 dv   = pos - bp;
                float   rSum = BrickBreakerBrick.SphereR + R;
                float   dist = dv.magnitude;
                if (dist >= rSum) continue;

                if (br.IsBallItem)
                {
                    br.Collect();
                    BrickBreakerManager.Instance.OnItemCollected(br);
                    continue;
                }

                brickHits++;
                float dmgS = BrickBreakerManager.Instance != null ? BrickBreakerManager.Instance.BallDamage : 1f;
                bool destroyedS = br.TakeDamage(dmgS);
                BrickBreakerManager.Instance.OnBrickHit(destroyedS, bp, br.CurrentColor);

                // 중심이 정확히 겹치면 법선이 없다 → 진행 반대 방향으로 밀어낸다
                Vector3 n = dist > 1e-4f ? dv / dist : -vel.normalized;
                vel = Vector3.Reflect(vel, n);
                pos = bp + n * (rSum + 1e-3f);   // 표면 밖으로 빼서 재충돌 방지
                break;
            }

            // ── 박스·정사면체: 공용 볼록 판정 ────────────
            // 공 물리와 예측선이 같은 함수를 쓰므로 어긋날 수 없다.
            if (!br.OverlapConvex(pos, R, out Vector3 nrm, out float depth)) continue;

            if (br.IsBallItem)
            {
                br.Collect();
                BrickBreakerManager.Instance.OnItemCollected(br);
                continue;
            }

            brickHits++;
            float dmg = BrickBreakerManager.Instance != null ? BrickBreakerManager.Instance.BallDamage : 1f;
            bool destroyed = br.TakeDamage(dmg);
            BrickBreakerManager.Instance.OnBrickHit(destroyed, bp, br.CurrentColor);

            vel  = Vector3.Reflect(vel, nrm);
            pos += nrm * (depth + 1e-3f);      // 표면 밖으로 빼서 재충돌 방지
            break;
        }
    }

    void WallBounce(ref Vector3 p, ref Vector3 v)
    {
        bool bounced = false;
        if (p.x - R < Lx) { p.x = Lx + R; v.x =  Mathf.Abs(v.x); bounced = true; }
        if (p.x + R > Rx) { p.x = Rx - R; v.x = -Mathf.Abs(v.x); bounced = true; }
        if (p.y - R < By) { p.y = By + R; v.y =  Mathf.Abs(v.y); bounced = true; }
        if (p.y + R > Ty) { p.y = Ty - R; v.y = -Mathf.Abs(v.y); bounced = true; }
        if (p.z + R > Bz) { p.z = Bz - R; v.z = -Mathf.Abs(v.z); bounced = true; }

        // 벽 반사는 자주 일어나므로 아주 가벼운 링만
        if (bounced)
        {
            BrickBreakerFX.Instance?.WallPing(p);
            BrickBreakerAudio.Instance?.WallBounce(brickHits);
        }
    }

    static void Reflect1D(ref float pos, ref float vel, float bCenter, float overlap)
    {
        bool positive = pos >= bCenter;
        vel = positive ?  Mathf.Abs(vel) : -Mathf.Abs(vel);
        pos += positive ? overlap : -overlap;
    }

    void Return(Vector3 pos) { gameObject.SetActive(false); OnReturned?.Invoke(pos, IsLeader); }

    void StopAll()
    {
        if (physRoutine != null) { StopCoroutine(physRoutine); physRoutine = null; }
    }

    void SetBounds(float lx, float rx, float by, float ty, float bz, float rz)
    { Lx = lx; Rx = rx; By = by; Ty = ty; Bz = bz; Rz = rz; }
}
