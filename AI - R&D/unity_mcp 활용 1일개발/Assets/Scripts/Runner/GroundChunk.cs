using UnityEngine;

public class GroundChunk : MonoBehaviour
{
    public Vector2 size = new Vector2(10f, 2f);
    public SpriteRenderer spriteRenderer;

    private static Sprite _cachedSprite;

    void Start()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>()
                          ?? gameObject.AddComponent<SpriteRenderer>();

        if (_cachedSprite == null)
            _cachedSprite = BuildSprite();

        spriteRenderer.sprite       = _cachedSprite;
        spriteRenderer.drawMode     = SpriteDrawMode.Tiled;
        spriteRenderer.tileMode     = SpriteTileMode.Continuous;
        spriteRenderer.size         = size;
        spriteRenderer.sortingOrder = 0;

        transform.localScale = Vector3.one;

        RunnerGameManager.Instance?.RegisterChunk(this);
    }

    void Update()
    {
        if (RunnerGameManager.Instance != null && !RunnerGameManager.Instance.isPlaying) return;
        float speed = RunnerGameManager.Instance?.currentScrollSpeed ?? 5f;
        transform.position += Vector3.left * (speed * Time.deltaTime);
        if (GetAABB().maxX < -15f) DestroyChunk();
    }

    public AABB GetAABB() => new AABB(transform.position, size);

    void DestroyChunk()
    {
        RunnerGameManager.Instance?.UnregisterChunk(this);
        Destroy(gameObject);
    }

    void OnDestroy() => RunnerGameManager.Instance?.UnregisterChunk(this);

    void OnDrawGizmos()
    {
        Gizmos.color = Color.gray;
        Gizmos.DrawWireCube(transform.position, size);
    }

    static Sprite BuildSprite()
    {
        // 실제 이미지 우선, 없으면 절차적 폴백
        var tex = Resources.Load<Texture2D>("Backgrounds/groundTile");
        if (tex == null) tex = BackgroundManager.MakeGroundTex();
        // PPU=32 → 64px = 2 world units (청크 두께와 맞춤)
        float ppu = tex.width / 2f;
        return Sprite.Create(tex,
            new Rect(0, 0, tex.width, tex.height),
            new Vector2(0.5f, 0.5f),
            ppu, 0, SpriteMeshType.FullRect, Vector4.zero, true);
    }
}
