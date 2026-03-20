using UnityEngine;

/// <summary>
/// 5분 후 스폰되어 좌측으로 스크롤. 플레이어가 통과하면 완주 처리.
/// </summary>
public class GoalLine : MonoBehaviour
{
    private SpriteRenderer _sr;

    void Awake()
    {
        // 골라인 시각 표시 (세로 노란 선)
        _sr = gameObject.AddComponent<SpriteRenderer>();
        Texture2D tex = new Texture2D(4, 64);
        for (int x = 0; x < 4; x++)
            for (int y = 0; y < 64; y++)
                tex.SetPixel(x, y, new Color(1f, 0.9f, 0f));
        tex.Apply();
        _sr.sprite = Sprite.Create(tex, new Rect(0, 0, 4, 64), new Vector2(0.5f, 0.5f), 32f);
        _sr.color = new Color(1f, 0.9f, 0f, 0.9f);
        _sr.sortingOrder = 5;
        transform.localScale = new Vector3(0.5f, 10f, 1f);
    }

    void Update()
    {
        if (RunnerGameManager.Instance == null || !RunnerGameManager.Instance.isPlaying)
            return;

        // 월드와 함께 좌측 스크롤
        transform.position += Vector3.left * RunnerGameManager.Instance.currentScrollSpeed * Time.deltaTime;

        // 플레이어 X 위치보다 왼쪽으로 넘어가면 완주
        if (RunnerGameManager.Instance.player != null)
        {
            float playerX = RunnerGameManager.Instance.player.transform.position.x;
            if (transform.position.x < playerX)
            {
                RunnerGameManager.Instance.TriggerWin();
                Destroy(gameObject);
            }
        }
    }
}
