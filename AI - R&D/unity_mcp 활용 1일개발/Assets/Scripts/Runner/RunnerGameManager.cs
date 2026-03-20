using System.Collections.Generic;
using UnityEngine;

public class RunnerGameManager : MonoBehaviour
{
    public static RunnerGameManager Instance;

    public enum GameState { CharSelect, Playing, GameOver, Win }
    public static string SelectedSkin = "Adventurer";  // Kenney 캐릭터 폴더명

    [Header("References")]
    public PlayerController2D player;
    public GroundManager       groundManager;
    public UIManager           uiManager;
    public GoalLine            goalLinePrefab;

    [Header("Difficulty (0~300s)")]
    public float maxTime = 300f;

    [Header("Scroll Speed")]
    public float startSpeed = 9f;
    public float maxSpeed   = 22f;

    [Header("Combat")]
    public float attackRange = 4f;

    [Header("State")]
    public GameState state           = GameState.CharSelect;
    public float     currentTime     = 0f;
    public float     currentScrollSpeed = 0f;
    public int       score           = 0;
    public bool      IsInvincible    = false;

    // 내부 점수
    private int   _timeScore  = 0;
    private int   _itemBonus  = 0;
    public  int   CoinCount   { get; private set; } = 0;

    private bool _goalSpawned = false;

    public bool isGameOver => state == GameState.GameOver;
    public bool isPlaying  => state == GameState.Playing;
    public bool isWin      => state == GameState.Win;

    public List<GroundChunk> activeChunks = new List<GroundChunk>();
    public List<RunnerItem>  activeItems  = new List<RunnerItem>();

    void Awake() { Instance = this; }

    void Start()
    {
        EnsureSystems();
        uiManager?.ShowCharSelect();
    }

    // 필수 시스템 오브젝트 자동 생성
    void EnsureSystems()
    {
        if (!FindFirstObjectByType<AudioManager>())
            new GameObject("AudioManager").AddComponent<AudioManager>();
        if (!FindFirstObjectByType<GameDataManager>())
            new GameObject("GameDataManager").AddComponent<GameDataManager>();
        if (!FindFirstObjectByType<ComboSystem>())
            new GameObject("ComboSystem").AddComponent<ComboSystem>();
        if (!FindFirstObjectByType<ReviveSystem>())
            new GameObject("ReviveSystem").AddComponent<ReviveSystem>();
        if (!FindFirstObjectByType<PauseManager>())
            new GameObject("PauseManager").AddComponent<PauseManager>();
        if (!FindFirstObjectByType<PowerUpSystem>())
            new GameObject("PowerUpSystem").AddComponent<PowerUpSystem>();
        if (Camera.main && !Camera.main.GetComponent<ScreenShake>())
            Camera.main.gameObject.AddComponent<ScreenShake>();
    }

    void Update()
    {
        if (state != GameState.Playing) return;

        currentTime += Time.deltaTime;
        float t = Mathf.Clamp01(currentTime / maxTime);
        currentScrollSpeed = Mathf.Lerp(startSpeed, maxSpeed, t);

        _timeScore = Mathf.FloorToInt(currentTime * 10f);
        score      = _timeScore + _itemBonus;
        uiManager?.UpdateScore(score);

        if (!_goalSpawned && currentTime >= maxTime)
        {
            _goalSpawned = true;
            SpawnGoalLine();
        }

        CheckCollisions();
    }

    void SpawnGoalLine()
    {
        if (goalLinePrefab != null)
            Instantiate(goalLinePrefab, new Vector3(30f, 0f, 0f), Quaternion.identity);
        else
        {
            var go = new GameObject("GoalLine");
            go.transform.position = new Vector3(30f, 0f, 0f);
            go.AddComponent<GoalLine>();
        }
    }

    // ── 게임 시작 ─────────────────────────────────────────
    public static void StartGameWithSkin(string skin)
    {
        SelectedSkin = skin;
        Instance?.BeginGame();
    }

    void BeginGame()
    {
        // 이전 게임 잔여 아이템 모두 제거 (반복 플레이 시 중복 방지)
        for (int i = activeItems.Count - 1; i >= 0; i--)
            if (activeItems[i] != null) Destroy(activeItems[i].gameObject);
        activeItems.Clear();

        CoinCount   = 0;
        _itemBonus  = 0;
        currentTime = 0f;
        _goalSpawned = false;

        // 플레이어 위치 초기화
        if (player != null)
        {
            player.transform.position = new Vector3(0f, 1f, 0f);
            player.ResetVelocity();
        }

        // 스프라이트 캐릭터 전환 (Spine 있으면 동시 적용)
        player?.SetCharacter(SelectedSkin);
        var spineAnim = player?.GetComponent<PlayerSpineAnimator>()
                     ?? player?.GetComponentInChildren<PlayerSpineAnimator>();
        spineAnim?.SetSkin(SelectedSkin);
        player?.PlayRun();

        ComboSystem.Instance?.ResetCombo();
        PowerUpSystem.Instance?.ResetAll();

        state              = GameState.Playing;
        currentScrollSpeed = startSpeed;
        uiManager?.ShowHUD();
        uiManager?.UpdateBestScore(GameDataManager.Instance?.BestScore ?? 0);
        AudioManager.Instance?.StartBGM();
    }

    // ── 재시작 ────────────────────────────────────────────
    public void RestartGame()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    // ── 공격 ──────────────────────────────────────────────
    public void AttackNearestEnemy()
    {
        if (!isPlaying) return;

        player?.PlayAttack();
        AudioManager.Instance?.PlayAttack();

        RunnerItem nearest = null;
        float minDist = float.MaxValue;
        Vector2 pPos  = player.transform.position;

        for (int i = activeItems.Count - 1; i >= 0; i--)
        {
            var item = activeItems[i];
            if (item == null || !item.isEnemy) continue;
            float d = Mathf.Abs(item.transform.position.x - pPos.x);
            if (d < attackRange && d < minDist) { minDist = d; nearest = item; }
        }

        if (nearest != null)
        {
            int bonus = nearest.scoreValue * 2;
            _itemBonus += bonus;
            FloatingText.Show("+" + bonus, nearest.transform.position,
                              new Color(1f, 0.5f, 0.1f));
            AudioManager.Instance?.PlayKill();
            nearest.Die();
        }
    }

    // ── 스킬 ──────────────────────────────────────────────
    public void UseSkill()
    {
        if (!isPlaying) return;

        player?.PlaySkill();
        AudioManager.Instance?.PlaySkill();

        for (int i = activeItems.Count - 1; i >= 0; i--)
        {
            var item = activeItems[i];
            if (item == null || !item.isEnemy) continue;
            if (item.transform.position.x < 20f)
            {
                int bonus = item.scoreValue * 2;
                _itemBonus += bonus;
                FloatingText.Show("+" + bonus, item.transform.position,
                                  new Color(0.8f, 0.4f, 1f));
                AudioManager.Instance?.PlayKill();
                item.Die();
            }
        }
    }

    // ── 충돌 체크 ─────────────────────────────────────────
    void CheckCollisions()
    {
        if (player == null) return;

        bool  isGrounded    = false;
        float highestFloorY = float.MinValue;
        AABB  playerAABB    = player.GetAABB();

        foreach (var chunk in activeChunks)
        {
            AABB ca = chunk.GetAABB();
            if (AABB.TryResolveFloor(playerAABB, ca, out float fy))
            {
                if (fy > highestFloorY) { highestFloorY = fy; isGrounded = true; }
            }
            else if (AABB.Intersect(playerAABB, ca))
            {
                TriggerGameOver(); return;
            }
        }

        player.SetGrounded(isGrounded, highestFloorY);

        for (int i = activeItems.Count - 1; i >= 0; i--)
        {
            var item = activeItems[i];
            if (item == null) { activeItems.RemoveAt(i); continue; }

            if (AABB.Intersect(playerAABB, item.GetAABB()))
            {
                if (item.isEnemy)
                {
                    if (!IsInvincible) { TriggerGameOver(); return; }
                }
                else
                {
                    int gained = ComboSystem.Instance?.Apply(item.scoreValue) ?? item.scoreValue;
                    _itemBonus += gained;
                    CoinCount++;
                    ComboSystem.Instance?.OnCoinCollected();
                    AudioManager.Instance?.PlayCoin();
                    FloatingText.Show("+" + gained, item.transform.position,
                                      new Color(1f, 0.9f, 0.1f));
                    item.Collect();
                }
            }
        }

        if (player.transform.position.y < -7f)
            TriggerGameOver();
    }

    // ── 완주 ──────────────────────────────────────────────
    public void TriggerWin()
    {
        if (state != GameState.Playing) return;
        state = GameState.Win;

        player?.PlayWin();
        AudioManager.Instance?.StopBGM();
        AudioManager.Instance?.PlayWin();

        bool newRecord = GameDataManager.Instance?.SubmitScore(score) ?? false;
        GameDataManager.Instance?.AddCoins(CoinCount);
        uiManager?.ShowWin(score, newRecord, GameDataManager.Instance?.BestScore ?? 0);
    }

    // ── 게임오버 ──────────────────────────────────────────
    void TriggerGameOver()
    {
        if (state == GameState.GameOver || state == GameState.Win) return;
        state = GameState.GameOver;

        player?.PlayDie();
        AudioManager.Instance?.StopBGM();
        AudioManager.Instance?.PlayGameOver();
        ScreenShake.Instance?.Shake();
        ComboSystem.Instance?.ResetCombo();

        ReviveSystem.Instance?.OfferRevive();
    }

    public float GetDifficultyProgress() => Mathf.Clamp01(currentTime / maxTime);

    public void RegisterChunk(GroundChunk c)  { if (!activeChunks.Contains(c)) activeChunks.Add(c); }
    public void UnregisterChunk(GroundChunk c) { activeChunks.Remove(c); }
    public void RegisterItem(RunnerItem i)     { if (!activeItems.Contains(i)) activeItems.Add(i); }
    public void UnregisterItem(RunnerItem i)   { activeItems.Remove(i); }
}
