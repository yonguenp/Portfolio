using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerController2D : MonoBehaviour
{
    [Header("Size")]
    public Vector2 size = new Vector2(1f, 1f);

    [Header("Movement")]
    public float jumpVelocity = 15f;
    public float gravity = 40f;
    public int maxJumps = 2;

    private float _verticalVelocity = 0f;
    private bool  _isGrounded = false;
    private int   _jumpCount  = 0;

    private PlayerSpineAnimator _spineAnim;
    private SpriteAnimator      _spriteAnim;

    // 터치 스와이프
    private Vector2 _touchStart;
    private bool    _touchBegan = false;
    private const float SWIPE_THRESHOLD = 60f;

    void Start()
    {
        _spineAnim  = GetComponent<PlayerSpineAnimator>()
                   ?? GetComponentInChildren<PlayerSpineAnimator>();
        _spriteAnim = GetComponent<SpriteAnimator>()
                   ?? GetComponentInChildren<SpriteAnimator>();
        transform.localScale = new Vector3(size.x, size.y, 1f);
    }

    // ── 애니메이션 헬퍼 (Spine + Sprite 동시 구동) ───────────
    public void SetCharacter(string folder)
    {
        _spriteAnim?.SetCharacter(folder);
        _spriteAnim?.SetRun();
    }

    public void PlayRun()
    {
        _spineAnim?.PlayMove();
        _spriteAnim?.SetRun();
    }

    public void PlayDie()
    {
        _spineAnim?.PlayLose();
        _spriteAnim?.SetDie();
    }

    public void PlayWin()
    {
        _spineAnim?.PlayWin();
        _spriteAnim?.SetIdle();
    }

    public void PlayAttack()
    {
        _spineAnim?.PlayAttack();
        _spriteAnim?.SetHurt();
    }

    public void PlaySkill()
    {
        _spineAnim?.PlaySkill();
        _spriteAnim?.SetHurt();
    }

    public void PlayRevive()
    {
        _spineAnim?.PlayMove();
        _spriteAnim?.ResetForRevive();
    }

    void Update()
    {
        if (RunnerGameManager.Instance != null && !RunnerGameManager.Instance.isPlaying) return;

        if (!_isGrounded)
            _verticalVelocity -= gravity * Time.deltaTime;

        bool jumpInput = Input.GetKeyDown(KeyCode.Space);
        if (Input.GetMouseButtonDown(0) && !IsPointerOverUI())
            jumpInput = true;

        // 모바일 스와이프 위 → 점프
        if (Input.touchCount > 0)
        {
            Touch t = Input.GetTouch(0);
            if (t.phase == TouchPhase.Began)
            {
                _touchStart = t.position;
                _touchBegan = true;
            }
            else if (_touchBegan && t.phase == TouchPhase.Ended)
            {
                Vector2 delta = t.position - _touchStart;
                if (delta.y > SWIPE_THRESHOLD && Mathf.Abs(delta.y) > Mathf.Abs(delta.x) * 0.8f)
                    if (!IsPointerOverUI()) jumpInput = true;
                _touchBegan = false;
            }
        }

        if (jumpInput)
        {
            if (_isGrounded || _jumpCount < maxJumps)
            {
                _verticalVelocity = jumpVelocity;
                _isGrounded = false;
                _jumpCount++;
                _spineAnim?.PlayJump();
                _spriteAnim?.SetJump();
            }
        }

        transform.position += Vector3.up * (_verticalVelocity * Time.deltaTime);
    }

    private bool IsPointerOverUI()
        => EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();

    public AABB GetAABB() => new AABB(transform.position, size);

    public void SetGrounded(bool grounded, float floorY)
    {
        if (_verticalVelocity > 0f) { _isGrounded = false; return; }

        bool wasAirborne = !_isGrounded;
        _isGrounded = grounded;

        if (grounded)
        {
            _verticalVelocity = 0f;
            _jumpCount = 0;
            Vector3 pos = transform.position;
            pos.y = floorY + size.y * 0.5f;
            transform.position = pos;

            if (wasAirborne) // 착지 순간 move로 복귀
            {
                _spineAnim?.PlayMove();
                _spriteAnim?.SetRun();
                EffectManager.Instance?.SpawnJumpDust(
                    new Vector3(transform.position.x, transform.position.y - size.y * 0.5f, 0));
            }
        }
    }

    public void ResetVelocity()
    {
        _verticalVelocity = 0f;
        _jumpCount        = 0;
        _isGrounded       = false;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, size);
    }
}
