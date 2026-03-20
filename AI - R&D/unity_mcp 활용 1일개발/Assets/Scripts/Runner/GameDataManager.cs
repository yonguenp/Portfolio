using UnityEngine;

/// <summary>
/// 게임 데이터 영속 저장 (PlayerPrefs).
/// 최고 기록, 총 플레이 횟수, 총 획득 코인.
/// </summary>
public class GameDataManager : MonoBehaviour
{
    public static GameDataManager Instance;

    const string K_BEST  = "BestScore";
    const string K_RUNS  = "TotalRuns";
    const string K_COINS = "TotalCoins";

    public int BestScore  { get; private set; }
    public int TotalRuns  { get; private set; }
    public int TotalCoins { get; private set; }

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Load();
    }

    void Load()
    {
        BestScore  = PlayerPrefs.GetInt(K_BEST,  0);
        TotalRuns  = PlayerPrefs.GetInt(K_RUNS,  0);
        TotalCoins = PlayerPrefs.GetInt(K_COINS, 0);
    }

    /// <returns>true면 신기록</returns>
    public bool SubmitScore(int score)
    {
        TotalRuns++;
        PlayerPrefs.SetInt(K_RUNS, TotalRuns);

        bool newRecord = score > BestScore;
        if (newRecord)
        {
            BestScore = score;
            PlayerPrefs.SetInt(K_BEST, BestScore);
        }
        PlayerPrefs.Save();
        return newRecord;
    }

    public void AddCoins(int amount)
    {
        TotalCoins += amount;
        PlayerPrefs.SetInt(K_COINS, TotalCoins);
        PlayerPrefs.Save();
    }

    public void ResetAll()
    {
        PlayerPrefs.DeleteAll();
        Load();
    }
}
