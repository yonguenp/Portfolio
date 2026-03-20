using UnityEngine;

/// <summary>
/// 프로시저럴 SFX + BGM 합성 - 외부 오디오 파일 없이 모든 사운드 생성.
/// BGM: 6.4초 루프 치픈 (Square wave 아르페지오 + 베이스 + 킥/하이햇)
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Range(0f, 1f)] public float sfxVolume   = 0.75f;
    [Range(0f, 1f)] public float musicVolume = 0.28f;

    private AudioSource _sfx;
    private AudioSource _music;

    private const int SR = 22050;

    // SFX 캐시
    private AudioClip _jump, _coin, _kill, _gameOver, _win;
    private AudioClip _attack, _skill, _combo, _revive, _button;
    private AudioClip _shield, _magnet;

    // BGM
    private AudioClip _bgm;

    // 뮤트 상태
    private bool _sfxMuted;
    private bool _bgmMuted;
    public bool SFXMuted => _sfxMuted;
    public bool BGMMuted => _bgmMuted;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        _sfx   = gameObject.AddComponent<AudioSource>();
        _music = gameObject.AddComponent<AudioSource>();
        _music.loop   = true;
        _music.volume = musicVolume;

        _sfxMuted = PlayerPrefs.GetInt("SFXMuted", 0) == 1;
        _bgmMuted = PlayerPrefs.GetInt("BGMMuted", 0) == 1;

        Build();
    }

    void Build()
    {
        _jump     = Clip("jump",     Sweep(260f, 560f, 0.14f));
        _coin     = Clip("coin",     Mix(Sine(1047f, 0.10f, 0.003f, 0.08f),
                                        Offset(Sine(1319f, 0.08f, 0.003f, 0.06f), 0.04f)));
        _kill     = Clip("kill",     Mix(Sweep(200f, 60f, 0.22f), Noise(0.18f, 700f)));
        _gameOver = Clip("gameover", Mix(Sine(440f, 0.18f), Offset(Sine(330f, 0.18f), 0.20f),
                                        Offset(Sine(220f, 0.40f), 0.40f)));
        _win      = Clip("win",      Mix(Sine(523f, 0.14f), Offset(Sine(659f, 0.14f), 0.14f),
                                        Offset(Sine(784f, 0.28f), 0.28f),
                                        Offset(Sine(1047f, 0.40f), 0.48f)));
        _attack   = Clip("attack",   Mix(Sweep(420f, 180f, 0.10f), Noise(0.07f, 3500f)));
        _skill    = Clip("skill",    Mix(Sweep(300f, 950f, 0.26f),
                                        Offset(Sine(1200f, 0.14f, 0.005f, 0.12f), 0.12f)));
        _combo    = Clip("combo",    Sweep(580f, 920f, 0.11f, 0.004f));
        _revive   = Clip("revive",   Mix(Sweep(180f, 820f, 0.42f, 0.015f),
                                        Offset(Sine(1047f, 0.09f), 0.15f),
                                        Offset(Sine(1319f, 0.09f), 0.28f)));
        _button   = Clip("button",   Sweep(400f, 600f, 0.07f, 0.003f));

        // Power-up SFX
        _shield = Clip("shield", Mix(
            Sweep(440f, 1047f, 0.20f, 0.004f),
            Offset(Sine(1319f, 0.12f, 0.005f, 0.09f), 0.10f),
            Offset(Sine(1760f, 0.08f, 0.005f, 0.06f), 0.17f)));

        _magnet = Clip("magnet", Mix(
            Sweep(700f, 1400f, 0.12f, 0.003f),
            Offset(Sweep(350f, 700f, 0.18f, 0.005f), 0.05f),
            Offset(Sine(880f,  0.10f, 0.005f, 0.07f), 0.12f)));

        // BGM (6.4초 루프)
        _bgm = Clip("bgm", BuildBGM());
    }

    // ── Public API ────────────────────────────────────────────
    public void PlayJump()          => Play(_jump);
    public void PlayCoin()          => Play(_coin, 1f + Random.Range(-0.05f, 0.1f));
    public void PlayKill()          => Play(_kill);
    public void PlayGameOver()      => Play(_gameOver);
    public void PlayWin()           => Play(_win);
    public void PlayAttack()        => Play(_attack);
    public void PlaySkill()         => Play(_skill);
    public void PlayCombo(int lvl)  => Play(_combo, 0.85f + lvl * 0.1f);
    public void PlayRevive()        => Play(_revive);
    public void PlayButton()        => Play(_button);
    public void PlayShield()        => Play(_shield);
    public void PlayMagnet()        => Play(_magnet);

    public void StartBGM()
    {
        if (_bgm == null || _bgmMuted) return;
        _music.clip   = _bgm;
        _music.volume = musicVolume;
        _music.Play();
    }

    public void StopBGM() => _music.Stop();

    public void SetSFXMuted(bool muted)
    {
        _sfxMuted = muted;
        PlayerPrefs.SetInt("SFXMuted", muted ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void SetBGMMuted(bool muted)
    {
        _bgmMuted = muted;
        PlayerPrefs.SetInt("BGMMuted", muted ? 1 : 0);
        PlayerPrefs.Save();
        if (muted) _music.Stop();
        else if (RunnerGameManager.Instance?.isPlaying == true) StartBGM();
    }

    void Play(AudioClip clip, float pitch = 1f)
    {
        if (clip == null || _sfxMuted) return;
        _sfx.pitch = pitch;
        _sfx.PlayOneShot(clip, sfxVolume);
    }

    // ══════════════════════════════════════════════════════════
    //  BGM 생성 (150 BPM, 4 bars × 4 beats = 6.4초 루프)
    // ══════════════════════════════════════════════════════════
    static float[] BuildBGM()
    {
        const float BPM  = 150f;
        float beat    = 60f / BPM;          // 0.4s per beat
        float loopLen = beat * 16f;          // 6.4s (4 bars)
        int   n       = Mathf.RoundToInt(SR * loopLen);
        var   buf     = new float[n];

        // ── 멜로디 (8분음표 = beat/2 간격) ──────────────
        float[] mf = {
            523f, 659f, 784f, 880f,   1047f, 880f,  784f, 659f,   // bar1
            784f, 659f, 523f, 392f,   523f,  659f,  784f, 1047f,  // bar2
            440f, 523f, 659f, 784f,   880f,  784f,  659f, 523f,   // bar3
            392f, 523f, 659f, 784f,   523f,  0f,    523f, 0f,     // bar4
        };
        float nd = beat * 0.5f * 0.86f;
        for (int i = 0; i < mf.Length; i++)
            if (mf[i] > 0f)
                AddSquare(buf, mf[i], i * beat * 0.5f, nd, 0.12f);

        // ── 베이스 (4분음표) ──────────────────────────────
        float[] bf = {
            131f, 131f, 196f, 196f,   // bar1: C3 C3 G3 G3
            131f, 131f, 165f, 165f,   // bar2: C3 C3 E3 E3
            110f, 110f, 131f, 131f,   // bar3: A2 A2 C3 C3
            196f, 196f, 131f, 131f,   // bar4: G3 G3 C3 C3
        };
        for (int i = 0; i < bf.Length; i++)
            AddSquare(buf, bf[i], i * beat, beat * 0.48f, 0.09f);

        // ── 킥 드럼 (각 마디 1박, 3박) ────────────────────
        for (int bar = 0; bar < 4; bar++)
        {
            float bs = bar * beat * 4f;
            AddKick(buf, bs,              beat * 0.14f, 0.14f);
            AddKick(buf, bs + beat * 2f,  beat * 0.14f, 0.11f);
        }

        // ── 하이햇 (8분음표마다) ──────────────────────────
        for (int i = 0; i < 32; i++)
            AddNoise(buf, i * beat * 0.5f, 0.035f, i % 2 == 0 ? 0.022f : 0.014f);

        // Normalize
        float peak = 0f;
        foreach (var v in buf) peak = Mathf.Max(peak, Mathf.Abs(v));
        if (peak > 0.001f) for (int i = 0; i < buf.Length; i++) buf[i] /= peak * 1.25f;

        return buf;
    }

    static void AddSquare(float[] buf, float freq, float startSec, float dur, float vol)
    {
        int   start = Mathf.RoundToInt(SR * startSec);
        int   len   = Mathf.RoundToInt(SR * dur);
        float phase = 0f, step = freq / SR;
        for (int i = 0; i < len; i++)
        {
            int idx = start + i;
            if (idx >= buf.Length) break;
            float t   = (float)i / len;
            float env = Mathf.Clamp01(t / 0.008f) * Mathf.Clamp01((1f - t) / 0.07f);
            buf[idx] += (phase < 0.5f ? 1f : -1f) * vol * env;
            phase    += step;
            if (phase >= 1f) phase -= 1f;
        }
    }

    static void AddNoise(float[] buf, float startSec, float dur, float vol)
    {
        int start = Mathf.RoundToInt(SR * startSec);
        int len   = Mathf.RoundToInt(SR * dur);
        var rng   = new System.Random(start * 7 + 13);
        for (int i = 0; i < len; i++)
        {
            int idx = start + i;
            if (idx >= buf.Length) break;
            float t   = (float)i / len;
            buf[idx] += (float)(rng.NextDouble() * 2 - 1) * vol * (1f - t);
        }
    }

    static void AddKick(float[] buf, float startSec, float dur, float vol)
    {
        int   start = Mathf.RoundToInt(SR * startSec);
        int   len   = Mathf.RoundToInt(SR * dur);
        float phase = 0f;
        for (int i = 0; i < len; i++)
        {
            int   idx  = start + i;
            if (idx >= buf.Length) break;
            float t    = (float)i / len;
            float freq = Mathf.Lerp(160f, 35f, t);
            float env  = 1f - t;
            phase += freq / SR;
            if (phase >= 1f) phase -= 1f;
            buf[idx] += (phase < 0.5f ? 1f : -1f) * vol * env;
        }
    }

    // ══════════════════════════════════════════════════════════
    //  SFX 합성 헬퍼
    // ══════════════════════════════════════════════════════════
    static float[] Sine(float freq, float dur, float atk = 0.01f, float rel = 0.12f)
    {
        int n = Mathf.RoundToInt(SR * dur);
        var s = new float[n];
        for (int i = 0; i < n; i++)
        {
            float t   = (float)i / SR;
            float env = Mathf.Clamp01(t / atk) * Mathf.Clamp01((dur - t) / rel);
            s[i]      = Mathf.Sin(2 * Mathf.PI * freq * t) * env;
        }
        return s;
    }

    static float[] Sweep(float f0, float f1, float dur, float atk = 0.008f)
    {
        int n = Mathf.RoundToInt(SR * dur);
        var s = new float[n];
        for (int i = 0; i < n; i++)
        {
            float t    = (float)i / SR;
            float freq = Mathf.Lerp(f0, f1, t / dur);
            float env  = Mathf.Clamp01(t / atk) * (1f - t / dur);
            s[i]       = Mathf.Sin(2 * Mathf.PI * freq * t) * env;
        }
        return s;
    }

    static float[] Noise(float dur, float cutHz = 2000f)
    {
        int n = Mathf.RoundToInt(SR * dur);
        var s = new float[n];
        var rng = new System.Random(7);
        float prev = 0f, alpha = cutHz / (cutHz + SR / (2 * Mathf.PI));
        for (int i = 0; i < n; i++)
        {
            float raw = (float)(rng.NextDouble() * 2 - 1);
            prev = prev + alpha * (raw - prev);
            s[i] = prev * (1f - (float)i / n) * 0.6f;
        }
        return s;
    }

    static float[] Mix(params float[][] tracks)
    {
        int len = 0;
        foreach (var t in tracks) len = Mathf.Max(len, t.Length);
        var out_ = new float[len];
        foreach (var t in tracks)
            for (int i = 0; i < t.Length; i++) out_[i] += t[i];
        float peak = 0f;
        foreach (var v in out_) peak = Mathf.Max(peak, Mathf.Abs(v));
        if (peak > 0.001f) for (int i = 0; i < out_.Length; i++) out_[i] /= peak * 1.1f;
        return out_;
    }

    static float[] Offset(float[] s, float delaySec)
    {
        int d    = Mathf.RoundToInt(SR * delaySec);
        var out_ = new float[s.Length + d];
        for (int i = 0; i < s.Length; i++) out_[i + d] = s[i];
        return out_;
    }

    static AudioClip Clip(string name, float[] samples)
    {
        var c = AudioClip.Create(name, samples.Length, 1, SR, false);
        c.SetData(samples, 0);
        return c;
    }
}
