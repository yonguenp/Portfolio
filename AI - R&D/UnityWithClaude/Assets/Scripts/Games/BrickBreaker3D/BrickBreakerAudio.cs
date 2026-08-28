using UnityEngine;

/// <summary>
/// 절차적 효과음. 프로젝트에 오디오 에셋이 하나도 없으므로
/// (기존 머티리얼·메쉬와 같은 방식으로) 파형을 코드로 합성해 쓴다.
///
/// 클립은 Awake에서 한 번만 만들고, 재생은 2D AudioSource 풀을 돌려쓴다.
/// 카메라가 궤도로 도는 게임이라 3D 사운드를 쓰면 좌우가 뒤집히므로
/// spatialBlend는 전부 0(2D)이다.
/// </summary>
public class BrickBreakerAudio : MonoBehaviour
{
    public static BrickBreakerAudio Instance { get; private set; }

    const int SR     = 44100;
    const int VOICES = 14;

    enum Wave { Sine, Square, Tri }

    AudioSource[] voices;
    int           voiceIdx;

    // 클립은 씬을 다시 로드해도 그대로 쓴다. 매 재시작마다 19개를 다시
    // 합성하면 그만큼 로딩이 길어진다. HideAndDontSave라 UnloadUnusedAssets에도
    // 안 쓸려나간다.
    static AudioClip   fireClip, hitClip, breakClip, itemClip, overClip, bestClip;
    static AudioClip   advanceClip, spawnClip, itemSpawnClip;
    static AudioClip[] clearFanfare;
    static AudioClip[] wallClips;   // 브릭을 때린 횟수만큼 굵어진다
    static AudioClip[] comboClips;
    // 층별 스템. 전부 같은 길이·템포·조성이라 동시에 틀어도 어긋나지 않는다.
    // 브릭이 있는 층의 스템만 볼륨을 열어 곡이 층수에 따라 두꺼워진다.
    public const int BGM_LAYERS = 5;
    static AudioClip[] bgmStems;
    static bool        clipsBuilt;

    AudioSource[] bgmSources;
    readonly float[] stemTarget = new float[BGM_LAYERS];
    readonly float[] stemNow    = new float[BGM_LAYERS];
    const float STEM_FADE = 1.4f;   // 초당 볼륨 변화량 — 급하게 켜지면 튄다

    // ── 배경음 ───────────────────────────────────────────
    const int   BGM_SR     = 32000;   // 유로비트는 밝아야 해서 22k보다 올린다
    const float BGM_BPM    = 155f;    // 유로비트 표준 템포대
    const float BGM_VOLUME = 0.34f;   // 효과음(0.16~0.28) 아래에 깔린다

    // 벽 반사는 볼이 많으면 초당 수십 번 일어난다. 그대로 재생하면 소음이 된다.
    const float WALL_MIN_GAP = 0.045f;
    float lastWallTime = -1f;
    int   lastWallTier;

    void Awake()
    {
        Instance = this;

        voices = new AudioSource[VOICES];
        for (int i = 0; i < VOICES; i++)
        {
            var src = gameObject.AddComponent<AudioSource>();
            src.playOnAwake   = false;
            src.spatialBlend  = 0f;   // 2D
            src.dopplerLevel  = 0f;
            voices[i] = src;
        }

        BuildClips();

        // 스템을 같은 dsp 시각에 동시 시작해야 위상이 맞는다.
        // 각각 Play()를 부르면 프레임 사이 오차만큼 밀린다.
        bgmSources = new AudioSource[BGM_LAYERS];
        double start = AudioSettings.dspTime + 0.15;
        for (int i = 0; i < BGM_LAYERS; i++)
        {
            var src = gameObject.AddComponent<AudioSource>();
            src.clip         = bgmStems[i];
            src.loop         = true;
            src.volume       = 0f;      // 매니저가 층 상태를 알려주면 열린다
            src.spatialBlend = 0f;
            src.playOnAwake  = false;
            src.PlayScheduled(start);
            bgmSources[i] = src;
        }
    }

    static void BuildClips()
    {
        if (clipsBuilt && fireClip != null && bgmStems != null) return;
        clipsBuilt = true;

        fireClip  = Tone("bb_fire",  700f,  300f, 0.11f, Wave.Square, 0.05f, 0.004f, 2.6f, 0.20f);
        hitClip   = Tone("bb_hit",   980f,  680f, 0.05f, Wave.Square, 0.28f, 0.002f, 3.2f, 0.16f);
        breakClip = Tone("bb_break", 520f,  110f, 0.20f, Wave.Tri,    0.55f, 0.002f, 2.2f, 0.28f);
        // 벽 반사 — 그 공이 브릭을 때린 횟수에 따라 단계별로 굵어진다.
        // 피치만 내리면 싸구려로 들려서 파형·노이즈·길이를 같이 바꾼다.
        wallClips = new[]
        {
            Tone("bb_wall0", 1420f, 1050f, 0.055f, Wave.Sine, 0.14f, 0.002f, 3.2f, 0.19f),
            Tone("bb_wall1",  900f,  620f, 0.075f, Wave.Sine, 0.22f, 0.002f, 2.8f, 0.24f),
            Tone("bb_wall2",  620f,  380f, 0.100f, Wave.Tri,  0.30f, 0.002f, 2.4f, 0.28f),
            Tone("bb_wall3",  430f,  220f, 0.130f, Wave.Tri,  0.38f, 0.002f, 2.0f, 0.32f),
        };
        itemClip  = Tone("bb_item",  660f, 1560f, 0.22f, Wave.Sine,   0f,    0.008f, 1.6f, 0.22f);
        overClip  = Tone("bb_over",  420f,   85f, 0.80f, Wave.Tri,    0.08f, 0.010f, 1.2f, 0.28f);
        bestClip  = Tone("bb_best",  780f, 1900f, 0.55f, Wave.Sine,   0f,    0.010f, 1.4f, 0.26f);

        // 콤보는 올라갈수록 음이 높아진다. 펜타토닉이라 아무 순서로 겹쳐도 안 어긋난다.
        int[] penta = { 0, 2, 4, 7, 9, 12, 14, 16, 19, 21, 24, 26 };
        comboClips = new AudioClip[penta.Length];
        for (int i = 0; i < penta.Length; i++)
        {
            float f = 523.25f * Mathf.Pow(2f, penta[i] / 12f);
            comboClips[i] = Tone($"bb_combo{i}", f, f * 1.005f, 0.13f, Wave.Sine, 0f, 0.004f, 2.0f, 0.17f);
        }

        // 턴 전환 / 브릭 생성 / 추가볼 생성.
        // 같은 프레임에 겹쳐 나므로 **음역대를 갈라** 서로 마스킹하지 않게 한다.
        // 음정은 다른 효과음·BGM과 같은 C 펜타토닉에서 고른다.
        advanceClip   = Tone("bb_advance",    220.00f,  164.81f, 0.30f, Wave.Tri,  0.28f, 0.010f, 1.8f, 0.22f); // A2→E2 저역
        spawnClip     = Tone("bb_spawn",      659.26f,  880.00f, 0.16f, Wave.Sine, 0.06f, 0.008f, 2.4f, 0.14f); // E5→A5 중고역
        itemSpawnClip = Tone("bb_itemspawn", 1046.50f, 1318.50f, 0.20f, Wave.Sine, 0f,    0.006f, 2.0f, 0.13f); // C6→E6 차임

        // 올클리어 팡파레 — 펜타토닉 상행 4음. 다른 효과음과 같은 음계라 안 부딪힌다.
        float[] fan = { 523.25f, 659.26f, 783.99f, 1046.50f };   // C E G C
        clearFanfare = new AudioClip[fan.Length];
        for (int i = 0; i < fan.Length; i++)
            clearFanfare[i] = Tone($"bb_clear{i}", fan[i], fan[i] * 1.01f, 0.30f,
                                   Wave.Sine, 0f, 0.005f, 1.6f, 0.26f);

        bgmStems = BuildBgmStems();
    }

    /// <summary>
    /// 유로비트풍 배경음을 **층별 스템 5개**로 나눠 만든다.
    /// 전부 같은 진행(Am→C→Gsus2→Dsus4)·같은 155BPM·같은 길이라 동시에 틀면
    /// 하나의 곡이 되고, 볼륨만 여닫아 층수에 따라 두께가 변한다.
    ///
    /// 층 0이 플레이어에 가장 가까운 줄이다. 가까울수록 리듬(킥·베이스)이,
    /// 멀수록 장식음(아르페지오·스파클)이 붙어 브릭이 다가올수록 곡이 거칠어진다.
    /// </summary>
    static AudioClip[] BuildBgmStems()
    {
        float beat = 60f / BGM_BPM;
        float bar  = beat * 4f;
        float six  = beat * 0.25f;

        float[] roots = { 110.00f, 130.81f, 98.00f, 73.42f };   // A2 C3 G2 D2
        float[][] arps =
        {
            new[] { 220.00f, 261.63f, 329.63f, 440.00f },
            new[] { 261.63f, 329.63f, 392.00f, 523.25f },
            new[] { 196.00f, 220.00f, 293.66f, 392.00f },
            new[] { 146.83f, 196.00f, 220.00f, 293.66f },
        };

        int bars     = roots.Length;
        int n        = Mathf.RoundToInt(BGM_SR * bar * bars);
        var data     = new float[BGM_LAYERS][];
        for (int L = 0; L < BGM_LAYERS; L++) data[L] = new float[n];

        for (int i = 0; i < n; i++)
        {
            float t     = i / (float)BGM_SR;
            int   barIx = Mathf.Min(bars - 1, (int)(t / bar));
            float inBar = t - barIx * bar;
            float root  = roots[barIx];
            var   arp   = arps[barIx];

            float kb = inBar % beat;                       // 온비트 위상
            float ob = (inBar + beat * 0.5f) % beat;       // 오프비트 위상

            // ── 층 0: 킥 + 하이햇 (가장 가까움 = 가장 급박) ──
            if (kb < 0.12f)
            {
                float f = Mathf.Lerp(145f, 45f, Mathf.Clamp01(kb / 0.09f));
                data[0][i] += Mathf.Sin(2f * Mathf.PI * f * kb) * Mathf.Exp(-32f * kb) * 0.55f;
            }
            if (ob < 0.03f)
                data[0][i] += (Random.value * 2f - 1f) * Mathf.Exp(-90f * ob) * 0.06f;

            // ── 층 1: 오프비트 베이스 ──
            if (ob < beat * 0.42f)
            {
                float v = 0f;
                for (int h = 1; h <= 6; h++) v += Mathf.Sin(2f * Mathf.PI * root * h * ob) / h;
                data[1][i] += v * Mathf.Exp(-14f * ob) * 0.16f;
            }

            // ── 층 2: 16분 아르페지오 ──
            int   si = (int)(inBar / six);
            float sb = inBar - si * six;
            if (sb < six * 0.8f)
                data[2][i] += Mathf.Sin(2f * Mathf.PI * arp[si % arp.Length] * sb)
                            * Mathf.Exp(-26f * sb) * 0.13f;

            // ── 층 3: 코드 패드 (마디 전체를 채우는 지속음) ──
            {
                float env = Mathf.Sin(inBar / bar * Mathf.PI);
                env *= env;
                float v = 0f;
                for (int j = 1; j < arp.Length; j++)
                    v += Mathf.Sin(2f * Mathf.PI * arp[j] * 0.5f * t);
                data[3][i] += v * env * 0.055f;
            }

            // ── 층 4: 하이 스파클 (가장 멂 = 가장 가벼움) ──
            if (si % 2 == 0 && sb < six * 0.5f)
                data[4][i] += Mathf.Sin(2f * Mathf.PI * arp[si % arp.Length] * 2f * sb)
                            * Mathf.Exp(-40f * sb) * 0.07f;
        }

        var clips = new AudioClip[BGM_LAYERS];
        for (int L = 0; L < BGM_LAYERS; L++)
        {
            for (int i = 0; i < n; i++) data[L][i] *= 0.85f;
            clips[L] = AudioClip.Create($"bb_bgm{L}", n, 1, BGM_SR, false);
            clips[L].SetData(data[L], 0);
            clips[L].hideFlags = HideFlags.HideAndDontSave;
        }
        return clips;
    }

    /// <summary>
    /// 브릭이 존재하는 층 비트마스크를 매니저가 매 프레임 알려준다.
    /// 볼륨은 즉시 바꾸지 않고 서서히 열고 닫는다 — 층이 비는 순간 뚝 끊기면 튄다.
    /// </summary>
    public void SetActiveLayers(int mask)
    {
        for (int i = 0; i < BGM_LAYERS; i++)
            stemTarget[i] = (mask & (1 << i)) != 0 ? BGM_VOLUME : 0f;
    }

    void Update()
    {
        if (bgmSources == null) return;
        float step = STEM_FADE * Time.unscaledDeltaTime;
        float master = GameAudioSettings.Bgm;
        for (int i = 0; i < BGM_LAYERS; i++)
        {
            if (!Mathf.Approximately(stemNow[i], stemTarget[i]))
                stemNow[i] = Mathf.MoveTowards(stemNow[i], stemTarget[i], step);
            // stemNow가 수렴해 있어도 매 프레임 다시 곱한다 — 안 그러면 층 페이드가
            // 끝난 뒤에는 옵션 화면에서 슬라이더를 움직여도 반영되지 않는다.
            if (bgmSources[i]) bgmSources[i].volume = stemNow[i] * master;
        }
    }

    public void SetBgmVolume(float v)
    {
        if (bgmSources == null) return;
        for (int i = 0; i < BGM_LAYERS; i++)
            if (bgmSources[i]) bgmSources[i].volume = stemNow[i] * Mathf.Clamp01(v);
    }

    // ── 재생 ─────────────────────────────────────────────
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

    public void Fire()      => Play(fireClip,  1f, Random.Range(0.96f, 1.05f));
    public void BrickBreak()=> Play(breakClip, 1f, Random.Range(0.92f, 1.10f));
    public void Item()      => Play(itemClip);
    public void GameOver()  => Play(overClip);
    public void NewBest()   => Play(bestClip);

    /// <summary>남은 HP가 적을수록 음이 높아져 "곧 깨진다"가 들린다.</summary>
    public void BrickHit(int hpRemaining, int hpMax)
    {
        float t = hpMax > 1 ? 1f - Mathf.Clamp01(hpRemaining / (float)hpMax) : 0.5f;
        Play(hitClip, 1f, Mathf.Lerp(0.85f, 1.45f, t));
    }

    /// <param name="brickHits">이 공이 이번 발사에서 브릭을 때린 누적 횟수.</param>
    public void WallBounce(int brickHits = 0)
    {
        if (wallClips == null) return;

        int tier = brickHits <= 0 ? 0
                 : brickHits <= 2 ? 1
                 : brickHits <= 5 ? 2
                 : 3;

        // 스로틀은 유지하되 **더 굵은 소리는 뚫고 나온다**.
        // 안 그러면 아직 아무것도 못 때린 공들이 무거운 공의 타격감을 계속 가로챈다.
        if (Time.unscaledTime - lastWallTime < WALL_MIN_GAP && tier <= lastWallTier) return;
        lastWallTime = Time.unscaledTime;
        lastWallTier = tier;

        Play(wallClips[tier], 1f, Random.Range(0.94f, 1.08f));
    }

    /// <summary>브릭이 한 칸 다가올 때. 저역이라 생성음과 겹쳐도 안 뭉친다.</summary>
    public void TurnAdvance() => Play(advanceClip);

    /// <summary>새 브릭이 나타날 때. 덩어리 단위로 한 번만 울린다.</summary>
    public void BrickSpawn() => Play(spawnClip, 1f, Random.Range(0.97f, 1.05f));

    /// <summary>추가볼 아이템이 나타날 때. 획득음(Item)보다 조용하고 짧아 구분된다.</summary>
    public void ItemSpawn() => Play(itemSpawnClip);

    /// <summary>올클리어 팡파레. 네 음을 시간차로 쏘아 상행 아르페지오가 된다.</summary>
    public void AllClear(MonoBehaviour host)
    {
        if (clearFanfare == null || host == null) return;
        host.StartCoroutine(FanfareRoutine());
    }

    System.Collections.IEnumerator FanfareRoutine()
    {
        for (int i = 0; i < clearFanfare.Length; i++)
        {
            Play(clearFanfare[i]);
            yield return new WaitForSeconds(0.09f);
        }
    }

    public void Combo(int combo)
    {
        if (comboClips == null || comboClips.Length == 0) return;
        int i = Mathf.Clamp(combo - 1, 0, comboClips.Length - 1);
        Play(comboClips[i]);
    }

    // ── 파형 합성 ────────────────────────────────────────
    /// <param name="f0">시작 주파수</param>
    /// <param name="f1">끝 주파수 (글라이드)</param>
    /// <param name="noiseMix">0=순음, 1=화이트노이즈. 타격음은 섞어야 단단해진다.</param>
    /// <param name="attack">전체 길이 대비 어택 비율</param>
    /// <param name="release">감쇠 지수. 클수록 빨리 사라진다.</param>
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
                _           => 4f * Mathf.Abs(p - 0.5f) - 1f,   // Tri
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
