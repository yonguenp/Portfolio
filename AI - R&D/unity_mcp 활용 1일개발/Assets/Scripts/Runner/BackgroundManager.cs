using UnityEngine;

/// <summary>
/// Kenney Background Elements Redux 실제 이미지 기반 패럴랙스 배경.
/// LoadSprite는 항상 Sprite.Create 경유 → worldW 크기 보장 (타일 간격 0).
/// SpawnBgObjects는 타일 컨테이너 방식 → 오브젝트 그룹 단위 랩.
/// </summary>
public class BackgroundManager : MonoBehaviour
{
    void Awake()
    {
        SetCamera();

        // ── 레이어 순서 (sort 낮을수록 뒤) ─────────────────────────
        // 카메라 배경색(하늘색)이 투명 레이어 사이로 보임
        // sort -11  원경 산          parallax 0.02
        // sort -9   원경 언덕        parallax 0.05
        // sort -8   원경 구름        parallax 0.09
        // sort -7   중경 언덕        parallax 0.18
        // sort -6   근경 구름        parallax 0.28
        // sort -5   근경 나무/덤불   parallax 0.44
        // sort -3   전경 땅 스트립   parallax 0.62

        CreateLayer("BG_Mountains", "Backgrounds/Elements/mountains",
                    sort:-11, y:-2.0f, parallax:0.02f, worldW:28f, count:4);

        CreateLayer("BG_HillsFar",  "Backgrounds/Elements/hillsLarge",
                    sort:-9,  y:-1.5f, parallax:0.05f, worldW:26f, count:4);

        CreateLayer("BG_CloudFar",  "Backgrounds/Elements/cloudLayerB1",
                    sort:-8,  y:-5.0f, parallax:0.09f, worldW:28f, count:4);

        CreateLayer("BG_HillsMid",  "Backgrounds/Elements/hills",
                    sort:-7,  y:-1.0f, parallax:0.18f, worldW:22f, count:4);

        CreateLayer("BG_CloudNear", "Backgrounds/Elements/cloudLayerB2",
                    sort:-6,  y:-4.0f, parallax:0.28f, worldW:22f, count:4);

        SpawnBgObjects();

        CreateLayer("BG_Ground",    "Backgrounds/Elements/groundLayer1",
                    sort:-3,  y:-2.0f, parallax:0.62f, worldW:18f, count:5);
    }

    // ── 이미지 패럴랙스 레이어 생성 ──────────────────────────────
    void CreateLayer(string layerName, string resPath,
                     int sort, float y, float parallax, float worldW, int count)
    {
        var sp = LoadSprite(resPath, worldW);
        if (sp == null) return;

        // 실제 스프라이트 너비 = worldW (LoadSprite가 보장)
        float actualW = sp.bounds.size.x;

        var root = new GameObject(layerName);
        root.transform.SetParent(transform, false);
        var pl = root.AddComponent<ParallaxLayer>();
        pl.parallaxFactor = parallax;
        pl.tileWidth      = actualW;

        for (int i = 0; i < count; i++)
        {
            var go = new GameObject("T" + i);
            go.transform.SetParent(root.transform, false);
            go.transform.position = new Vector3(actualW * (i - count / 2f), y, 0f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite       = sp;
            sr.sortingOrder = sort;
        }
    }

    // ── 근경 나무/덤불 오브젝트 (타일 컨테이너 방식) ────────────
    void SpawnBgObjects()
    {
        var objs = new (string path, float w, float y, int sort)[]
        {
            ("Backgrounds/Objects/treePine",          1.5f, -1.2f, -5),
            ("Backgrounds/Objects/tree",              1.8f, -1.0f, -5),
            ("Backgrounds/Objects/treeSmall_green1",  1.0f, -1.4f, -4),
            ("Backgrounds/Objects/treeSmall_green2",  1.0f, -1.4f, -4),
            ("Backgrounds/Objects/bush1",             0.9f, -1.7f, -4),
            ("Backgrounds/Objects/bush2",             1.1f, -1.7f, -4),
            ("Backgrounds/Objects/bush3",             1.2f, -1.7f, -4),
        };

        float tileW    = 28f;
        int   tileCnt  = 4;
        // 타일 내 오브젝트 x 오프셋 (tileW 범위 내 균등 배치)
        float[] xs = { -12f, -7f, -2f, 3f, 8f, 13f };

        var root = new GameObject("BG_Objects");
        root.transform.SetParent(transform, false);
        var pl = root.AddComponent<ParallaxLayer>();
        pl.parallaxFactor = 0.44f;
        pl.tileWidth      = tileW;

        int oi = 0;
        for (int tile = 0; tile < tileCnt; tile++)
        {
            float tileX = tileW * (tile - tileCnt / 2f);

            // 컨테이너 — ParallaxLayer가 이 단위로 재배치
            var tileGo = new GameObject("ObjTile_" + tile);
            tileGo.transform.SetParent(root.transform, false);
            tileGo.transform.position = new Vector3(tileX, 0f, 0f);

            foreach (float bx in xs)
            {
                var def = objs[oi % objs.Length];
                oi++;
                var sp = LoadSprite(def.path, def.w);
                if (sp == null) continue;

                var go = new GameObject("Obj");
                go.transform.SetParent(tileGo.transform, false);
                go.transform.localPosition = new Vector3(bx, def.y, 0f);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite       = sp;
                sr.sortingOrder = def.sort;
            }
        }
    }

    // ── 카메라 배경색 (하늘) ──────────────────────────────────────
    void SetCamera()
    {
        if (Camera.main == null) return;
        Camera.main.clearFlags      = CameraClearFlags.SolidColor;
        Camera.main.backgroundColor = new Color(0.69f, 0.87f, 0.99f);
    }

    // ── 헬퍼: Texture2D 경유 Sprite.Create → 항상 worldW 크기 보장 ──
    // Resources.Load<Sprite> 는 import PPU(=100) 기준 스프라이트를 반환해
    // tileWidth 불일치를 유발하므로 사용하지 않음.
    static Sprite LoadSprite(string path, float worldW)
    {
        // Sprite 타입으로 임포트된 경우에도 texture 속성으로 원본 텍스처 확보
        Texture2D tex = null;
        var sp = Resources.Load<Sprite>(path);
        if (sp != null) tex = sp.texture;
        if (tex == null) tex = Resources.Load<Texture2D>(path);
        if (tex == null) { Debug.LogWarning("[BG] 없음: " + path); return null; }

        float ppu = tex.width / worldW;
        return Sprite.Create(tex,
            new Rect(0, 0, tex.width, tex.height),
            new Vector2(0.5f, 0f), ppu);
    }

    // ══════════════════════════════════════════════════════════════
    //  땅 텍스처 (GroundChunk 공유 — 절차적 폴백)
    // ══════════════════════════════════════════════════════════════
    public static Texture2D MakeGroundTex()
    {
        int w = 32, h = 32;
        var tex = new Texture2D(w, h, TextureFormat.RGB24, false)
            { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Repeat };
        var p   = new Color[w * h];
        var rng = new System.Random(99);

        Color Px(int r, int g, int b) => new Color(r/255f, g/255f, b/255f);
        Color GND_G1 = Px(105,235, 70), GND_G2 = Px( 65,165, 38), GND_G3 = Px( 40,105, 20);
        Color GND_D1 = Px(175,105, 42), GND_D2 = Px(120, 66, 22), GND_D3 = Px( 72, 36,  8);
        Color GND_ST = Px( 98, 90, 82);

        for (int y = 0; y < h; y++)
        for (int x = 0; x < w; x++)
        {
            float fy = (float)y / h;
            bool checker = ((x + y * 3) % 4 == 0);
            Color c;
            if      (fy >= 0.88f) c = checker ? GND_G1 : GND_G2;
            else if (fy >= 0.72f) c = checker ? GND_G2 : GND_G3;
            else if (fy >= 0.55f) c = checker ? GND_G3 : GND_D1;
            else if (fy >= 0.30f) { c = checker ? GND_D1 : GND_D2; if (rng.NextDouble() < 0.06) c = GND_D3; }
            else                  { c = checker ? GND_D2 : GND_ST;  if (x % 6 == 0 || y == 0) c = GND_D3; }
            p[y * w + x] = c;
        }
        for (int x = 0; x < w; x++)
        {
            p[(h-1)*w+x] = GND_G1;
            if (x % 5 == 2) p[(h-1)*w+x] = new Color(0.55f, 0.95f, 0.30f);
        }
        tex.SetPixels(p); tex.Apply(); return tex;
    }
}
