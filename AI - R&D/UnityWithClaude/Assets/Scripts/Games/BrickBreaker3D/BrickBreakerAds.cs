using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.LevelPlay;
using UnityEngine;

/// <summary>
/// 광고 래퍼. <b>광고 SDK를 아는 유일한 파일이다.</b>
/// 게임 코드는 <see cref="ShowRewarded"/> / <see cref="ShowInterstitial"/> 만 부르고
/// 결과를 콜백으로 받는다.
///
/// <b>레거시 Unity Ads(<c>com.unity.ads</c>)에서 LevelPlay로 갈아탔다.</b>
/// Unity Ads Network는 이제 입찰(bidding) 전용이라 대시보드의 광고 유닛이 전부
/// Bidding으로만 만들어지고, 레거시 <c>Advertisement.Load()</c>로는 그걸 못 부른다
/// (실기기에서 전부 <c>INVALID_ARGUMENT</c>). 대시보드 툴팁도 같은 말을 한다 —
/// "The Unity Ads Network is only available for in-app bidding in Unity LevelPlay or Max".
///
/// <b>콜백은 어떤 경로로든 반드시 한 번 불린다.</b> 앱 키가 비었든, 초기화가
/// 실패했든, 광고가 없든 마찬가지다. 안 그러면 "광고 보고 이어하기"를 누른
/// 게임오버 화면이 영영 안 닫힌다.
/// </summary>
public class BrickBreakerAds : MonoBehaviour
{
    public static BrickBreakerAds Instance { get; private set; }

    // ── 대시보드 발급값 ──────────────────────────────────
    // LevelPlay는 Unity Ads의 Game ID(6174769/6174768)를 쓰지 않는다.
    // cloud.unity.com → Unity LevelPlay 에서 받는 **App Key**와,
    // 거기서 만든 **Ad Unit ID**가 따로 필요하다.
    const string APP_KEY_IOS     = "279dd5cdd";
    const string APP_KEY_ANDROID = "279dd9665";

    const string REWARDED_ID_IOS      = "ocq1q427bqi8j1c1";
    const string REWARDED_ID_ANDROID  = "byjzzx583dyeiqgk";
    const string INTERSTIT_ID_IOS     = "bpf2rluy4ebw0xxm";
    const string INTERSTIT_ID_ANDROID = "sv5hqh23l926z8fy";

    /// <summary>
    /// 개발 중에는 true. <b>출시 빌드 전에 false로 바꿀 것.</b>
    /// 화면에 진단 줄을 띄울지도 이 값으로 정한다.
    /// </summary>
    const bool TEST_MODE = true;

    public static bool TestMode => TEST_MODE;

    // 에디터에는 iOS 런타임이 없으므로 Android 쪽 값으로 붙는다.
    static bool IsIOS => Application.platform == RuntimePlatform.IPhonePlayer;

    static string AppKey      => IsIOS ? APP_KEY_IOS      : APP_KEY_ANDROID;
    static string RewardedId  => IsIOS ? REWARDED_ID_IOS  : REWARDED_ID_ANDROID;
    static string InterstitId => IsIOS ? INTERSTIT_ID_IOS : INTERSTIT_ID_ANDROID;

    // ── 상태 ─────────────────────────────────────────────
    bool   initialized;
    bool   initFailed;
    string lastError;

    LevelPlayRewardedAd     rewarded;
    LevelPlayInterstitialAd interstitial;

    Action<bool> pendingRewarded;
    Action       pendingInterstitial;

    // 이번에 띄운 리워드 광고에서 보상 조건을 채웠는가.
    // OnAdRewarded로 켜고, 닫힐 때(OnAdClosed) 이 값으로 콜백한다.
    bool rewardEarned;

    // 아직 안 실린 광고를 "누른 뒤에" 받아오는 중인가.
    bool waitingRewarded;
    const float LOAD_WAIT = 8f;

    public bool IsRewardedReady => initialized && rewarded != null && rewarded.IsAdReady();

    // ── 메인 스레드 디스패치 ─────────────────────────────
    // LevelPlay 콜백은 메인 스레드가 아닌 곳에서 온다(SDK 로그에 UITHREAD: false).
    // 거기서 Unity API를 건드리면 예외가 나고, 그게 관리 코드 밖으로 빠져나가
    // 앱이 통째로 죽는다(Il2CppExceptionWrapper). 그래서 SDK 콜백은
    // **아무것도 하지 않고 큐에 넣기만** 하고, 실제 처리는 Update에서 한다.
    readonly Queue<Action> mainQueue = new Queue<Action>();

    void Post(Action a)
    {
        if (a == null) return;
        lock (mainQueue) mainQueue.Enqueue(a);
    }

    void Update()
    {
        while (true)
        {
            Action a = null;
            lock (mainQueue) { if (mainQueue.Count > 0) a = mainQueue.Dequeue(); }
            if (a == null) break;
            // 하나가 터져도 나머지 큐는 계속 돌아야 한다.
            try { a(); }
            catch (Exception e) { Debug.LogWarning("[광고] 콜백 처리 실패: " + e.Message); }
        }
    }

    // ── 생성 ─────────────────────────────────────────────
    /// <summary>
    /// 앱이 뜨자마자 초기화한다. 광고를 쓰는 씬에 들어간 뒤 초기화하면
    /// 첫 판에서는 광고가 실릴 시간이 없다.
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
    static void Bootstrap() => Create();

    public static BrickBreakerAds Create()
    {
        if (Instance != null) return Instance;
        var go = new GameObject("BrickBreakerAds");
        DontDestroyOnLoad(go);
        Instance = go.AddComponent<BrickBreakerAds>();
        Instance.Init();
        return Instance;
    }

    void Init()
    {
        if (string.IsNullOrEmpty(AppKey))
        {
            // 앱 키가 없으면 SDK를 부르지 않는다 — 게임은 광고 없이 그대로 돈다.
            initFailed = true;
            lastError  = "앱키 미설정";
            Debug.LogWarning("[광고] LevelPlay 앱 키가 비어 있다 — 광고 없이 진행한다.");
            return;
        }

        LevelPlay.OnInitSuccess += OnInitSuccess;
        LevelPlay.OnInitFailed  += OnInitFailed;
        LevelPlay.Init(AppKey);
    }

    void OnInitSuccess(LevelPlayConfiguration config) => Post(BuildAds);

    void BuildAds()
    {
        initialized = true;
        Debug.Log("[광고] LevelPlay 초기화 완료");

        if (!string.IsNullOrEmpty(RewardedId))
        {
            rewarded = new LevelPlayRewardedAd(RewardedId);
            rewarded.OnAdLoaded        += _ => Post(OnRewardedLoaded);
            rewarded.OnAdLoadFailed    += e => Post(() => OnRewardedFailed(e));
            rewarded.OnAdRewarded      += (_, __) => rewardEarned = true;   // 플래그만 — 안전
            rewarded.OnAdClosed        += _ => Post(() => FinishRewarded(rewardEarned));
            rewarded.OnAdDisplayFailed += (_, e) => Post(() => { lastError = "R show " + e.ErrorMessage; FinishRewarded(false); });
            rewarded.LoadAd();
        }

        if (!string.IsNullOrEmpty(InterstitId))
        {
            interstitial = new LevelPlayInterstitialAd(InterstitId);
            interstitial.OnAdLoadFailed    += e => Post(() => { lastError = "I " + e.ErrorMessage; FinishInterstitial(); });
            interstitial.OnAdClosed        += _ => Post(() => { FinishInterstitial(); interstitial.LoadAd(); });
            interstitial.OnAdDisplayFailed += (_, e) => Post(() => { lastError = "I show " + e.ErrorMessage; FinishInterstitial(); });
            interstitial.LoadAd();
        }
    }

    void OnInitFailed(LevelPlayInitError error) => Post(() => InitFailed(error));

    void InitFailed(LevelPlayInitError error)
    {
        initFailed = true;
        lastError  = error.ErrorMessage;
        Debug.LogWarning($"[광고] LevelPlay 초기화 실패: {error.ErrorCode} {error.ErrorMessage}");
        FinishRewarded(false);
        FinishInterstitial();
    }

    // ── 리워드 ───────────────────────────────────────────
    void OnRewardedLoaded()
    {
        // 버튼을 누른 뒤 기다리고 있었다면 도착하는 즉시 띄운다.
        if (!waitingRewarded) return;
        waitingRewarded = false;
        rewardEarned    = false;
        rewarded.ShowAd();
    }

    void OnRewardedFailed(LevelPlayAdError e)
    {
        lastError = "R " + e.ErrorMessage;
        Debug.LogWarning($"[광고] 리워드 로드 실패: {e.ErrorCode} {e.ErrorMessage}");
        if (waitingRewarded) { waitingRewarded = false; FinishRewarded(false); }
    }

    /// <summary>
    /// 콜백의 bool은 <b>보상을 줘야 하는가</b>다 — 끝까지 본 경우에만 true.
    /// 광고가 아직 안 실렸으면 여기서 받아서 오는 즉시 띄운다. 로드 완료를
    /// 버튼 노출 조건으로 걸면 로드가 늦을 때 버튼 자체가 안 보인다.
    /// </summary>
    public void ShowRewarded(Action<bool> onDone)
    {
        if (initFailed || !initialized || rewarded == null) { onDone?.Invoke(false); return; }

        pendingRewarded = onDone;
        rewardEarned    = false;

        if (rewarded.IsAdReady()) { rewarded.ShowAd(); return; }

        waitingRewarded = true;
        rewarded.LoadAd();
        StartCoroutine(RewardedTimeout());
    }

    IEnumerator RewardedTimeout()
    {
        yield return new WaitForSecondsRealtime(LOAD_WAIT);
        if (!waitingRewarded) yield break;
        waitingRewarded = false;
        lastError = "rewarded timeout";
        FinishRewarded(false);
    }

    void FinishRewarded(bool ok)
    {
        var cb = pendingRewarded; pendingRewarded = null;
        cb?.Invoke(ok);
        // 다음 판을 위해 미리 받아둔다.
        if (initialized && rewarded != null && !rewarded.IsAdReady()) rewarded.LoadAd();
    }

    // ── 전면 ─────────────────────────────────────────────
    /// <summary>보상이 없으므로 결과와 무관하게 콜백만 한 번 부른다.</summary>
    public void ShowInterstitial(Action onDone)
    {
        if (initFailed || !initialized || interstitial == null || !interstitial.IsAdReady())
        {
            onDone?.Invoke();
            if (initialized && interstitial != null) interstitial.LoadAd();
            return;
        }

        pendingInterstitial = onDone;
        interstitial.ShowAd();
    }

    void FinishInterstitial()
    {
        var cb = pendingInterstitial; pendingInterstitial = null;
        cb?.Invoke();
    }

    // ── 배너 ─────────────────────────────────────────────
    // 플레이 중에는 띄우지 않는다 — 터널이 화면을 꽉 채우는 게임이라 하단 배너가
    // 발사 지점과 바닥 그림자를 가린다. 아직 어디에도 붙이지 않았다.
    public void ShowBanner() { }
    public void HideBanner() { }

    // ── 진단 ─────────────────────────────────────────────
    /// <summary>
    /// 실기기에는 콘솔을 붙일 수 없어서, 광고가 왜 안 나오는지 볼 방법이
    /// 화면에 띄우는 것뿐이다. <see cref="TEST_MODE"/>일 때만 표시된다.
    /// </summary>
    public string Status()
    {
        if (string.IsNullOrEmpty(AppKey)) return "광고: 앱키 미설정";
        if (initFailed)   return "광고실패: " + (lastError ?? "?");
        if (!initialized) return "광고 초기화중…";
        return $"광고 R={(rewarded != null && rewarded.IsAdReady())} "
             + $"I={(interstitial != null && interstitial.IsAdReady())}"
             + (lastError != null ? "\n" + lastError : "");
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
        LevelPlay.OnInitSuccess -= OnInitSuccess;
        LevelPlay.OnInitFailed  -= OnInitFailed;
        rewarded?.DestroyAd();
        interstitial?.DestroyAd();
    }
}
