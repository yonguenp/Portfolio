using UnityEngine;

public class GroundManager : MonoBehaviour
{
    [Header("References")]
    public GameObject groundPrefab;
    public Transform playerTransform;

    [Header("Lengths & Gaps")]
    public float minLength = 3f;
    public float maxLengthStart = 15f;
    public float maxLengthEnd = 5f;
    public float maxGapSizeStart = 1f;
    public float maxGapSizeEnd = 6f;

    [Header("Heights")]
    public float startY = -2f;
    public float maxHeightVariationStart = 0f;
    public float maxHeightVariationEnd = 3f;

    [Header("Items & Enemies")]
    [Range(0f, 1f)] public float itemSpawnChance = 0.65f;
    [Range(0f, 1f)] public float enemyChance = 0.25f;
    [Range(0f, 1f)] public float powerUpChance = 0.10f;

    private float _nextSpawnX;
    private float _lastY;

    void Start()
    {
        _nextSpawnX = -10f;
        _lastY = startY;
        SpawnInitialGroundChunk();
    }

    void Update()
    {
        if (RunnerGameManager.Instance != null && !RunnerGameManager.Instance.isPlaying) return;

        float speed = RunnerGameManager.Instance != null ? RunnerGameManager.Instance.currentScrollSpeed : 5f;
        _nextSpawnX -= speed * Time.deltaTime;

        if (_nextSpawnX < 15f)
            SpawnNextPlatform();
    }

    private void SpawnInitialGroundChunk()
    {
        GameObject go = Instantiate(groundPrefab, new Vector3(0, startY, 0), Quaternion.identity);
        GroundChunk gc = go.GetComponent<GroundChunk>();
        gc.size = new Vector2(30f, 2f);
        _nextSpawnX = gc.GetAABB().maxX;
    }

    private void SpawnNextPlatform()
    {
        float difficulty = RunnerGameManager.Instance != null ? RunnerGameManager.Instance.GetDifficultyProgress() : 0f;

        float gap = Random.Range(1f, Mathf.Lerp(maxGapSizeStart, maxGapSizeEnd, difficulty));
        float spawnXStart = _nextSpawnX + gap;

        float length = Random.Range(minLength, Mathf.Lerp(maxLengthStart, maxLengthEnd, difficulty));

        float heightVar = Mathf.Lerp(maxHeightVariationStart, maxHeightVariationEnd, difficulty);
        float newY = Mathf.Clamp(_lastY + Random.Range(-heightVar, heightVar), -5f, 2f);
        _lastY = newY;

        float centerX = spawnXStart + length * 0.5f;

        GameObject go = Instantiate(groundPrefab, new Vector3(centerX, newY, 0), Quaternion.identity);
        GroundChunk gc = go.GetComponent<GroundChunk>();
        gc.size = new Vector2(length, 4f);

        _nextSpawnX = gc.GetAABB().maxX;

        SpawnItemsOnPlatform(centerX, length, newY + 2f, difficulty);
    }

    private void SpawnItemsOnPlatform(float centerX, float length, float topY, float difficulty)
    {
        if (Random.value > itemSpawnChance) return;

        int count = Random.Range(1, 4);
        float usableWidth = length * 0.65f;
        float step = count > 1 ? usableWidth / (count - 1) : 0f;
        float startX = centerX - usableWidth * 0.5f;

        for (int i = 0; i < count; i++)
        {
            float x = count > 1 ? startX + step * i : centerX;
            bool isEnemy = Random.value < Mathf.Lerp(0.1f, enemyChance, difficulty);

            GameObject itemGO = new GameObject(isEnemy ? "Enemy" : "Coin");
            itemGO.transform.position = new Vector3(x, topY + 0.6f, 0f);

            RunnerItem item = itemGO.AddComponent<RunnerItem>();
            item.isEnemy = isEnemy;
            item.size    = isEnemy ? new Vector2(0.9f, 1.5f) : new Vector2(0.7f, 0.7f);
        }

        // 파워업 스폰 (일반 아이템과 독립적으로 추가)
        if (Random.value < powerUpChance)
        {
            float pux    = centerX + Random.Range(-length * 0.28f, length * 0.28f);
            string puType = Random.value < 0.5f ? "shield" : "magnet";
            var puGO  = new GameObject("PowerUp_" + puType);
            puGO.transform.position = new Vector3(pux, topY + 0.9f, 0f);
            var puItem = puGO.AddComponent<RunnerItem>();
            puItem.powerUpType = puType;
            puItem.size        = new Vector2(1.0f, 1.0f);
        }
    }
}
