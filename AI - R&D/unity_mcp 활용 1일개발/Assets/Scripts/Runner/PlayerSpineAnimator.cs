using UnityEngine;
using Spine;
using Spine.Unity;

/// <summary>
/// 애니메이션 우선순위 관리.
/// 고우선순위(공격/스킬) 종료 감지: TrackEntry 폴링 + 타이머 이중 보장
/// → Spine TrackEntry 풀 재사용 이슈로 인한 상태 고착 원천 차단.
/// </summary>
[RequireComponent(typeof(SkeletonAnimation))]
public class PlayerSpineAnimator : MonoBehaviour
{
    private SkeletonAnimation _sa;

    // 기본 상태
    private enum BaseAnim { Move, Jump }
    private BaseAnim _baseAnim      = BaseAnim.Move;

    // 고우선순위 (공격/스킬)
    private bool       _highActive  = false;
    private TrackEntry _highEntry   = null;
    private float      _highTimer   = 0f;   // 경과 시간
    private float      _highMaxTime = 0f;   // 애니메이션 길이 + 여유

    // 덮어쓰기 (패배/완주)
    private bool _overrideActive = false;

    void Awake() => _sa = GetComponent<SkeletonAnimation>();
    void Start() => PlayMove();

    void Update()
    {
        if (!_highActive) return;

        _highTimer += Time.deltaTime;

        // ① 타이머 초과 (절대적 안전망)
        bool done = _highTimer >= _highMaxTime;

        // ② TrackEntry 직접 폴링 (주 감지)
        if (!done && _highEntry != null && !_highEntry.Loop)
            done = _highEntry.TrackTime >= _highEntry.AnimationEnd;

        if (done)
        {
            _highActive = false;
            _highEntry  = null;
            ReturnToBase();
        }
    }

    // ── 스킨 ─────────────────────────────────────────────────
    public void SetSkin(string skin)
    {
        if (_sa == null) _sa = GetComponent<SkeletonAnimation>();
        _sa.Skeleton.SetSkin(skin);
        _sa.Skeleton.SetSlotsToSetupPose();
        _sa.AnimationState.Apply(_sa.Skeleton);
    }

    // ── 이동 (기본) ───────────────────────────────────────────
    public void PlayMove()
    {
        if (_overrideActive) return;
        _baseAnim = BaseAnim.Move;
        if (!_highActive)
            _sa.AnimationState.SetAnimation(0, "move_ani1", true);
    }

    // ── 점프 중 Idle ──────────────────────────────────────────
    public void PlayJump()
    {
        if (_overrideActive) return;
        _baseAnim = BaseAnim.Jump;
        if (!_highActive)
            _sa.AnimationState.SetAnimation(0, "idle_ani1", true);
    }

    // ── 공격 (1회 재생 후 복귀) ────────────────────────────────
    // 점프 중에는 idle_ani1 유지, 공격 로직만 적용
    public void PlayAttack()
    {
        if (_overrideActive) return;
        if (_baseAnim == BaseAnim.Jump) return;
        BeginHighPriority("atk_ani1");
    }

    // ── 스킬 (1회 재생 후 복귀) ────────────────────────────────
    // 점프 중에는 idle_ani1 유지, 스킬 로직만 적용
    public void PlaySkill()
    {
        if (_overrideActive) return;
        if (_baseAnim == BaseAnim.Jump) return;
        BeginHighPriority("skill_ani1");
    }

    // ── 게임오버 ──────────────────────────────────────────────
    public void PlayLose()
    {
        ResetHighPriority();
        _overrideActive = true;
        _sa.AnimationState.SetAnimation(0, "lose_ani1", false);
    }

    // ── 완주 ──────────────────────────────────────────────────
    public void PlayWin()
    {
        ResetHighPriority();
        _overrideActive = true;
        _sa.AnimationState.SetAnimation(0, "win_ani1", false);
    }

    // ── 내부 ──────────────────────────────────────────────────
    private void BeginHighPriority(string animName)
    {
        _highActive  = true;
        _highTimer   = 0f;
        _highEntry   = _sa.AnimationState.SetAnimation(0, animName, false);
        // AnimationEnd가 0이면 기본 1.5초로 폴백
        _highMaxTime = (_highEntry != null && _highEntry.AnimationEnd > 0f
                        ? _highEntry.AnimationEnd : 1.5f) + 0.2f;
    }

    /// <summary>부활 시 모든 상태 플래그를 초기화하고 move_ani1로 복귀.</summary>
    public void ResetForRevive()
    {
        _overrideActive = false;
        ResetHighPriority();
        _baseAnim = BaseAnim.Move;
        _sa.AnimationState.SetAnimation(0, "move_ani1", true);
    }

    private void ResetHighPriority()
    {
        _highActive = false;
        _highEntry  = null;
        _highTimer  = 0f;
    }

    private void ReturnToBase()
    {
        if (_overrideActive) return;
        string anim = _baseAnim == BaseAnim.Jump ? "idle_ani1" : "move_ani1";
        _sa.AnimationState.SetAnimation(0, anim, true);
    }
}
