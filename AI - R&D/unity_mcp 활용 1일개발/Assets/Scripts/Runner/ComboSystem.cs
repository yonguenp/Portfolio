using UnityEngine;

/// <summary>
/// 연속 코인 획득 콤보 시스템.
/// 2.5초 내에 다음 코인 없으면 리셋.
/// 배수: 5개 단위로 x1 → x2 → x3 → x4 (최대)
/// </summary>
public class ComboSystem : MonoBehaviour
{
    public static ComboSystem Instance;

    public int  Count      { get; private set; } = 0;
    public int  Multiplier => Mathf.Clamp(1 + Count / 5, 1, 4);

    const float WINDOW = 2.5f;
    float _timer = 0f;

    void Awake() { Instance = this; }

    void Update()
    {
        if (Count == 0) return;
        _timer += Time.deltaTime;
        if (_timer >= WINDOW) ResetCombo();
    }

    public void OnCoinCollected()
    {
        Count++;
        _timer = 0f;

        int mult = Multiplier;
        if (mult > 1)
        {
            AudioManager.Instance?.PlayCombo(mult);
            RunnerGameManager.Instance?.uiManager?.ShowCombo(mult);
        }
    }

    public void ResetCombo()
    {
        if (Count > 0)
            RunnerGameManager.Instance?.uiManager?.HideCombo();
        Count  = 0;
        _timer = 0f;
    }

    public int Apply(int baseScore) => baseScore * Multiplier;
}
