using UnityEngine;

/// <summary>
/// 고스톱(2인 맞고 + 4인 고스톱 공용) 절차적 효과음. 프로젝트에 오디오
/// 에셋이 없어 파형을 코드로 합성한다(BrickBreakerAudio와 같은 방식 —
/// AudioClip.Create로 Awake에서 한 번만 만들고, 2D AudioSource 풀을 돌려쓴다).
///
/// 2인/4인이 이벤트 종류가 거의 같아서(뻑/쪽/싹쓸이/폭탄/흔들기/고·스톱/
/// 나가리/승패 등) 파일 하나를 공유한다 — GoStopGame.cs/GoStop3PGame.cs
/// 둘 다 이 컴포넌트를 자기 GameObject에 붙여 쓰면 된다.
///
/// 대부분의 이벤트는 <see cref="PlayForLabel"/> 하나로 처리한다 — 두 게임
/// 모두 이미 Toast(label)로 "무슨 일이 일어났는지"를 문자열로 넘기고
/// 있어서, 그 라벨 문자열에 사운드를 매핑하면 호출부를 거의 안 건드리고
/// 붙일 수 있다. Toast를 안 거치는 이벤트(카드 내기, 턴 전환, 고/스톱,
/// 승패, 나가리)만 전용 메서드를 따로 부른다.
/// </summary>
public class GoStopAudio : MonoBehaviour
{
    public static GoStopAudio Instance { get; private set; }

    const int SR = 44100;
    const int VOICES = 10;

    enum Wave { Sine, Square, Tri }

    AudioSource[] voices;
    int voiceIdx;

    static AudioClip cardPlayClip, captureClip, ppeokClip, jjokClip, sweepClip, bombClip,
                      shakeClip, goClip, stopClip, nagariClip, bonusClip, turnClip,
                      winClip, loseClip, moneyClip, gwangPaliClip, chongtongClip;
    static bool clipsBuilt;

    void Awake()
    {
        Instance = this;
        voices = new AudioSource[VOICES];
        for (int i = 0; i < VOICES; i++)
        {
            var src = gameObject.AddComponent<AudioSource>();
            src.playOnAwake  = false;
            src.spatialBlend = 0f; // 2D — 카드가 화면 이곳저곳으로 날아다녀도 좌우가 안 뒤집힌다
            voices[i] = src;
        }
        BuildClips();
    }

    static void BuildClips()
    {
        if (clipsBuilt && cardPlayClip != null) return;
        clipsBuilt = true;

        // 카드 내기 — 짧고 마른 틱. 매 턴 가장 자주 나므로 오래 끌면 거슬린다.
        cardPlayClip = Tone("gs_play", 520f, 380f, 0.06f, Wave.Square, 0.05f, 0.005f, 3.0f, 0.15f);
        // 일반 매칭 캡처 — 상승하는 맑은 톤. "먹었다"는 만족감.
        captureClip  = Tone("gs_capture", 700f, 1100f, 0.10f, Wave.Sine, 0f, 0.006f, 2.2f, 0.18f);
        // 뻑/첫뻑/연뻑/자뻑/뻑 먹기 — 낮고 둔탁하게. 노이즈를 섞어 "철퍽" 소리에 가깝게.
        ppeokClip    = Tone("gs_ppeok", 220f, 110f, 0.30f, Wave.Tri, 0.35f, 0.01f, 1.6f, 0.24f);
        // 쪽 — 짧고 높은 딩.
        jjokClip     = Tone("gs_jjok", 900f, 1300f, 0.12f, Wave.Sine, 0f, 0.004f, 2.6f, 0.20f);
        // 싹쓸이 — 넓게 퍼지는 상승음. 판이 크게 정리되는 느낌.
        sweepClip    = Tone("gs_sweep", 660f, 1600f, 0.30f, Wave.Sine, 0f, 0.006f, 1.4f, 0.24f);
        // 폭탄 — 낮고 굵게 터지는 소리.
        bombClip     = Tone("gs_bomb", 160f, 55f, 0.45f, Wave.Tri, 0.6f, 0.005f, 1.2f, 0.30f);
        // 흔들기 — 달그락거리는 느낌의 사각파.
        shakeClip    = Tone("gs_shake", 380f, 470f, 0.14f, Wave.Square, 0.15f, 0.01f, 2.4f, 0.16f);
        // 고 — 밝게 올라가는 선언음.
        goClip       = Tone("gs_go", 523.25f, 783.99f, 0.20f, Wave.Sine, 0f, 0.006f, 1.8f, 0.20f);
        // 스톱 — 차분하게 내려가는 마무리음.
        stopClip     = Tone("gs_stop", 440f, 220f, 0.28f, Wave.Tri, 0.1f, 0.008f, 1.6f, 0.20f);
        // 나가리 — 무효판. 밋밋하게 흐려지는 톤.
        nagariClip   = Tone("gs_nagari", 300f, 220f, 0.35f, Wave.Sine, 0.2f, 0.01f, 1.4f, 0.16f);
        // 보너스(조커) — 반짝이는 차임.
        bonusClip    = Tone("gs_bonus", 1046.5f, 1568f, 0.22f, Wave.Sine, 0f, 0.006f, 1.6f, 0.20f);
        // 턴 전환 — 아주 조용한 저역 틱. 자주 울리므로 볼륨을 낮게 잡는다.
        turnClip     = Tone("gs_turn", 260f, 200f, 0.12f, Wave.Sine, 0f, 0.008f, 2.4f, 0.10f);
        // 승리 — 상승 팡파레.
        winClip      = Tone("gs_win", 523.25f, 1046.5f, 0.5f, Wave.Sine, 0f, 0.01f, 1.2f, 0.26f);
        // 패배 — 하강 톤.
        loseClip     = Tone("gs_lose", 330f, 110f, 0.6f, Wave.Tri, 0.15f, 0.01f, 1.1f, 0.24f);
        // 돈 이동 — 동전 짤랑거리는 느낌의 짧은 사각파.
        moneyClip    = Tone("gs_money", 1400f, 1800f, 0.08f, Wave.Square, 0.1f, 0.003f, 3.4f, 0.14f);
        // 광팔이 — 특별 공지 느낌의 잔잔한 하강음.
        gwangPaliClip= Tone("gs_gwangpali", 700f, 500f, 0.30f, Wave.Sine, 0.05f, 0.008f, 1.8f, 0.18f);
        // 총통 — 딜 직후 즉시 승리라는 희귀·극적인 이벤트라 승리(Win)보다도
        // 화려하게. 3옥타브를 아르페지오처럼 훑고 올라가는 인상을 주려고
        // 시작 주파수를 낮게 잡고 끝을 훨씬 높게(2.5옥타브 상승) 벌렸다.
        chongtongClip= Tone("gs_chongtong", 392f, 1568f, 0.55f, Wave.Sine, 0f, 0.008f, 1.1f, 0.28f);
    }

    void Play(AudioClip clip, float volume = 1f, float pitch = 1f)
    {
        if (clip == null || voices == null) return;
        var src = voices[voiceIdx];
        voiceIdx = (voiceIdx + 1) % voices.Length;
        src.Stop();
        src.clip   = clip;
        src.volume = volume * GameAudioSettings.Sfx;
        src.pitch  = pitch;
        src.Play();
    }

    /// <summary>Toast(label)이 이미 부르고 있는 문자열에 사운드를 매핑한다 —
    /// 호출부를 거의 안 건드리고 붙이기 위한 진입점. 순서가 중요하다:
    /// "보너스+뻑"처럼 두 키워드가 겹치는 라벨은 더 구체적인(보너스) 쪽을
    /// 먼저 확인해야 그쪽으로 간다.</summary>
    public void PlayForLabel(string label)
    {
        if (string.IsNullOrEmpty(label)) return;
        if (label.Contains("총통")) Chongtong();
        else if (label.Contains("흔들기")) Shake();
        else if (label.Contains("쪽")) Jjok();
        else if (label.Contains("싹쓸이")) Sweep();
        else if (label.Contains("폭탄")) Bomb();
        else if (label.Contains("보너스")) Bonus();
        else if (label.Contains("광팔이")) GwangPali();
        else if (label.Contains("뻑")) Ppeok();
        else if (label.Contains("따닥")) Capture();
    }

    public void CardPlay()   => Play(cardPlayClip, 1f, Random.Range(0.96f, 1.05f));
    public void Capture()    => Play(captureClip, 1f, Random.Range(0.97f, 1.05f));
    public void Ppeok()      => Play(ppeokClip);
    public void Jjok()       => Play(jjokClip);
    public void Sweep()      => Play(sweepClip);
    public void Bomb()       => Play(bombClip);
    public void Shake()      => Play(shakeClip);
    public void Go()         => Play(goClip);
    public void Stop()       => Play(stopClip);
    public void Nagari()     => Play(nagariClip);
    public void Bonus()      => Play(bonusClip);
    public void TurnChange() => Play(turnClip, 0.6f);
    public void Win()        => Play(winClip);
    public void Lose()       => Play(loseClip);
    public void Money()      => Play(moneyClip, 0.7f);
    public void GwangPali()  => Play(gwangPaliClip);
    public void Chongtong()  => Play(chongtongClip);

    // ── 파형 합성 (BrickBreakerAudio.Tone과 같은 방식) ──────
    static AudioClip Tone(string name, float f0, float f1, float dur,
                          Wave wave, float noiseMix, float attack, float release, float vol)
    {
        int n = Mathf.Max(1, Mathf.RoundToInt(SR * dur));
        var data = new float[n];

        double phase = 0.0;
        for (int i = 0; i < n; i++)
        {
            float k = i / (float)n;
            float f = Mathf.Lerp(f0, f1, k);

            phase += f / SR;
            if (phase >= 1.0) phase -= 1.0;
            float p = (float)phase;

            float w = wave switch
            {
                Wave.Sine   => Mathf.Sin(p * 2f * Mathf.PI),
                Wave.Square => p < 0.5f ? 1f : -1f,
                _           => 4f * Mathf.Abs(p - 0.5f) - 1f, // Tri
            };
            if (noiseMix > 0f) w = Mathf.Lerp(w, Random.Range(-1f, 1f), noiseMix);

            float env = k < attack
                ? k / Mathf.Max(1e-4f, attack)
                : Mathf.Pow(1f - Mathf.InverseLerp(attack, 1f, k), release);

            data[i] = w * env * vol;
        }

        var clip = AudioClip.Create(name, n, 1, SR, false);
        clip.SetData(data, 0);
        clip.hideFlags = HideFlags.HideAndDontSave;
        return clip;
    }

    void OnDestroy() { if (Instance == this) Instance = null; }
}
