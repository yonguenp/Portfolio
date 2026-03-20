using UnityEngine;

/// <summary>
/// 자식 타일들을 parallaxFactor 배율로 좌측 스크롤.
/// 타일이 화면 왼쪽을 벗어나면 오른쪽 끝에 재배치해 무한 루프.
/// Playing 상태에서만 움직임 (GameOver/Win 시 정지).
/// </summary>
public class ParallaxLayer : MonoBehaviour
{
    [Tooltip("0 = 정지, 1 = 월드와 동일 속도")]
    public float parallaxFactor = 0.3f;
    [Tooltip("타일 1장의 월드 너비")]
    public float tileWidth = 24f;

    private Transform[] _tiles;

    void Start()
    {
        _tiles = new Transform[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
            _tiles[i] = transform.GetChild(i);
    }

    void Update()
    {
        var gm = RunnerGameManager.Instance;
        // Playing 상태에서만 스크롤 (GameOver / Win 시 완전 정지)
        if (gm == null || !gm.isPlaying) return;

        float move = gm.currentScrollSpeed * parallaxFactor * Time.deltaTime;

        foreach (var t in _tiles)
            t.position += Vector3.left * move;

        float rightmostX = float.MinValue;
        foreach (var t in _tiles)
            if (t.position.x > rightmostX) rightmostX = t.position.x;

        foreach (var tile in _tiles)
        {
            if (tile.position.x + tileWidth * 0.5f < -16f)
            {
                tile.position = new Vector3(rightmostX + tileWidth,
                                            tile.position.y,
                                            tile.position.z);
                rightmostX += tileWidth;
            }
        }
    }
}
