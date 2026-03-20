using UnityEngine;

/// <summary>
/// Kenney 스프라이트 기반 플레이어 애니메이터.
/// characterFolder 를 바꾸면 다른 캐릭터(Adventurer, Female, Player, Soldier)로 전환.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class SpriteAnimator : MonoBehaviour
{
    public enum State { Idle, Run, Jump, Fall, Hurt, Die }

    [Header("Character")]
    [Tooltip("Resources/Characters/ 아래 폴더명. e.g. Adventurer / Female / Player / Soldier")]
    public string characterFolder = "Adventurer";

    [Header("FPS")]
    public float runFPS  = 10f;
    public float idleFPS = 5f;
    public float hurtFPS = 8f;

    private SpriteRenderer _sr;
    private State  _state  = State.Run;
    private float  _timer;
    private int    _frame;
    private bool   _locked;

    // 스프라이트 캐시
    private Sprite[] _run, _idle, _jump, _fall, _hurt, _die;

    public State CurrentState => _state;

    void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
        _sr.sortingOrder = 5;
        LoadSprites();
    }

    // ── 공개 API ──────────────────────────────────────────────
    public void SetRun()  => Transition(State.Run);
    public void SetIdle() => Transition(State.Idle);
    public void SetJump() => Transition(State.Jump);
    public void SetFall() => Transition(State.Fall);

    public void SetHurt()
    {
        if (_locked) return;
        Transition(State.Hurt);
        _locked = true;
        Invoke(nameof(UnlockRun), 0.4f);
    }

    public void SetDie()
    {
        CancelInvoke();
        _locked = true;
        Transition(State.Die);
    }

    public void ResetForRevive()
    {
        CancelInvoke();
        _locked = false;
        Transition(State.Run);
    }

    public void FaceRight(bool right) => _sr.flipX = !right;

    // 캐릭터 폴더를 바꿔 즉시 스프라이트 세트 교체
    public void SetCharacter(string folder)
    {
        characterFolder = folder;
        LoadSprites();
        Transition(State.Idle);
    }

    // ── 업데이트 ─────────────────────────────────────────────
    void Update()
    {
        if (_locked && _state != State.Hurt) return;

        var frames = GetFrames(_state);
        if (frames == null || frames.Length == 0) return;

        float fps = _state == State.Run  ? runFPS
                  : _state == State.Hurt ? hurtFPS
                  : idleFPS;

        _timer += Time.deltaTime * fps;
        if (_timer >= 1f)
        {
            _timer -= 1f;
            _frame = (_frame + 1) % frames.Length;
            _sr.sprite = frames[_frame];
        }
    }

    // ── 내부 ─────────────────────────────────────────────────
    void Transition(State s)
    {
        if (_state == s) return;
        _state = s; _timer = 0f; _frame = 0;
        var f = GetFrames(s);
        if (f != null && f.Length > 0) _sr.sprite = f[0];
    }

    void UnlockRun()
    {
        _locked = false;
        Transition(State.Run);
    }

    Sprite[] GetFrames(State s) => s switch
    {
        State.Run  => _run,
        State.Idle => _idle,
        State.Jump => _jump,
        State.Fall => _fall,
        State.Hurt => _hurt,
        State.Die  => _die,
        _          => _idle
    };

    void LoadSprites()
    {
        string p = "Characters/" + characterFolder + "/";
        string n = characterFolder.ToLower(); // e.g. "adventurer"

        _run  = Load(p, n+"_walk1", n+"_walk2", n+"_walk1", n+"_walk2");
        _idle = Load(p, n+"_idle",  n+"_stand", n+"_idle");
        _jump = Load(p, n+"_jump");
        _fall = Load(p, n+"_fall");
        _hurt = Load(p, n+"_hurt");
        _die  = Load(p, n+"_duck", n+"_hurt", n+"_duck");

        // stand 없는 캐릭터 폴백
        if ((_idle == null || _idle.Length == 0) && _run != null)
            _idle = new[] { _run[0] };

        if (_sr != null)
        {
            var show = _idle ?? _run;
            if (show != null && show.Length > 0) _sr.sprite = show[0];
        }
    }

    static Sprite[] Load(string folder, params string[] names)
    {
        var list = new System.Collections.Generic.List<Sprite>();
        foreach (var n in names)
        {
            var path = folder + n;

            // 1) Sprite 모드로 임포트된 경우 (Setup 실행 후)
            var s = Resources.Load<Sprite>(path);

            // 2) Texture2D 기본 임포트인 경우 → Sprite 생성
            if (s == null)
            {
                var tex = Resources.Load<Texture2D>(path);
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
