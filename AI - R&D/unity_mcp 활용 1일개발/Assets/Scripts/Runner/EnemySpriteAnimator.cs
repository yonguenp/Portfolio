using UnityEngine;

/// <summary>
/// 적 스프라이트 애니메이터 (Kenney Zombie).
/// RunnerItem isEnemy=true 인 오브젝트에 AddComponent로 붙임.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class EnemySpriteAnimator : MonoBehaviour
{
    public float walkFPS = 8f;

    private SpriteRenderer _sr;
    private Sprite[] _walk, _hurt, _die;
    private float _timer;
    private int   _frame;
    private bool  _walking = true;

    void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
        _sr.sortingOrder = 3;
        LoadSprites();

        // 왼쪽을 향해 (flipX)
        _sr.flipX = true;
    }

    /// <summary>왼쪽 방향 고정 (Awake에서 이미 처리, 호환용 no-op).</summary>
    public void FaceLeft() { if (_sr) _sr.flipX = true; }

    void LoadSprites()
    {
        const string P = "Characters/Zombie/zombie_";
        _walk = Load(P+"walk1", P+"walk2", P+"walk1", P+"walk2");
        _hurt = Load(P+"hurt");
        _die  = Load(P+"duck", P+"hurt");

        if (_walk != null && _walk.Length > 0) _sr.sprite = _walk[0];
    }

    void Update()
    {
        if (!_walking) return;
        var frames = _walk;
        if (frames == null || frames.Length == 0) return;

        _timer += Time.deltaTime * walkFPS;
        if (_timer >= 1f)
        {
            _timer -= 1f;
            _frame = (_frame + 1) % frames.Length;
            _sr.sprite = frames[_frame];
        }
    }

    public void PlayHurt()
    {
        if (_hurt != null && _hurt.Length > 0) _sr.sprite = _hurt[0];
        Invoke(nameof(BackToWalk), 0.3f);
    }

    public void PlayDie()
    {
        _walking = false;
        if (_die != null && _die.Length > 0) _sr.sprite = _die[0];
    }

    void BackToWalk()
    {
        if (_walking && _walk != null && _walk.Length > 0)
            _sr.sprite = _walk[_frame % _walk.Length];
    }

    static Sprite[] Load(params string[] paths)
    {
        var list = new System.Collections.Generic.List<Sprite>();
        foreach (var p in paths)
        {
            // 1) Sprite 임포트 모드
            var s = Resources.Load<Sprite>(p);

            // 2) Texture2D 기본 임포트 폴백
            if (s == null)
            {
                var tex = Resources.Load<Texture2D>(p);
                if (tex != null)
                    s = Sprite.Create(tex,
                        new Rect(0, 0, tex.width, tex.height),
                        new Vector2(0.5f, 0.5f), 64f);
            }

            if (s != null) list.Add(s);
        }
        return list.Count > 0 ? list.ToArray() : null;
    }
}
