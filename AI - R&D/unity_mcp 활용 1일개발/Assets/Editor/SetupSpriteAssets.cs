using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// Kenney 에셋 임포트 설정 + 플레이어/적 스프라이트 교체 자동화.
/// Tools/Setup Sprite Assets 메뉴에서 실행.
/// </summary>
public static class SetupSpriteAssets
{
    [MenuItem("Tools/Setup Sprite Assets (Kenney)", priority = 50)]
    public static void Run()
    {
        EditorUtility.DisplayProgressBar("스프라이트 에셋 설정", "텍스처 임포트 설정 중...", 0.1f);

        // 1) 코인 시트 — 11프레임 스프라이트 시트 (242×42, 22px×frame)
        ConfigureCoinSheet();

        // 2) 캐릭터 스프라이트 — 개별 PNG, Point 필터, 64 PPU
        ConfigureFolder("Assets/Resources/Characters/Adventurer", FilterMode.Point, 64f);
        ConfigureFolder("Assets/Resources/Characters/Female",    FilterMode.Point, 64f);
        ConfigureFolder("Assets/Resources/Characters/Player",    FilterMode.Point, 64f);
        ConfigureFolder("Assets/Resources/Characters/Soldier",   FilterMode.Point, 64f);
        ConfigureFolder("Assets/Resources/Characters/Zombie",    FilterMode.Point, 64f);

        // 3) 파티클 스프라이트 — Bilinear
        ConfigureFolder("Assets/Resources/VFX/Particles", FilterMode.Bilinear, 100f);

        // 4) 배경 타일 — Point 픽셀아트
        ConfigureFolder("Assets/Resources/Tiles", FilterMode.Point, 16f);

        EditorUtility.DisplayProgressBar("스프라이트 에셋 설정", "플레이어 업그레이드 중...", 0.6f);

        // 5) 플레이어 → Spine 제거, SpriteAnimator 추가
        UpgradePlayer();

        // 6) EffectManager 씬에 추가
        EnsureEffectManager();

        EditorUtility.ClearProgressBar();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("[SetupSpriteAssets] ✅ 완료!");
        EditorUtility.DisplayDialog("설정 완료 ✅",
            "Kenney 스프라이트 에셋 설정 완료!\n\n" +
            "• 코인 시트 → 11프레임 (22px×frame)\n" +
            "• 캐릭터 4종 → Point 픽셀아트\n" +
            "• 플레이어 → SpriteAnimator (Spine 제거)\n" +
            "• EffectManager → 씬에 배치\n\n" +
            "Play 버튼을 눌러 확인하세요!", "확인");
    }

    // ─────────────────────────────────────────────────────────

    static void ConfigureCoinSheet()
    {
        const string path = "Assets/Resources/Items/coin_sheet.png";
        if (!File.Exists(path))
        {
            Debug.LogWarning("[SetupSpriteAssets] coin_sheet.png 없음 — 건너뜀");
            return;
        }

        var imp = AssetImporter.GetAtPath(path) as TextureImporter;
        if (imp == null) return;

        imp.textureType         = TextureImporterType.Sprite;
        imp.spriteImportMode    = SpriteImportMode.Multiple;
        imp.filterMode          = FilterMode.Point;
        imp.alphaIsTransparency = true;
        imp.mipmapEnabled       = false;
        imp.isReadable          = true;   // Sprite.Create 런타임 사용 허용
        imp.spritePixelsPerUnit = 42f;  // 높이(42px) = 1 Unity unit

        // 11프레임 슬라이스 — 각 22px 폭 (242 / 22 = 11)
        const int FRAME_W = 22;
        const int FRAMES  = 11;
        int frameH = 42;

        // 실제 텍스처 높이 확인 (import 전이라 AssetDatabase에서 읽기)
        var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        if (tex != null) frameH = tex.height;

        var rects = new SpriteMetaData[FRAMES];
        for (int i = 0; i < FRAMES; i++)
        {
            rects[i] = new SpriteMetaData
            {
                name      = "coin_" + i,
                rect      = new Rect(i * FRAME_W, 0, FRAME_W, frameH),
                pivot     = new Vector2(0.5f, 0.5f),
                alignment = (int)SpriteAlignment.Center
            };
        }
        imp.spritesheet = rects;
        imp.SaveAndReimport();
        Debug.Log("[SetupSpriteAssets] 코인 시트 슬라이싱 완료 (11프레임 × 22px)");
    }

    static void ConfigureFolder(string folder, FilterMode filter, float ppu)
    {
        if (!Directory.Exists(folder)) return;
        int count = 0;
        foreach (var file in Directory.GetFiles(folder, "*.png"))
        {
            var assetPath = file.Replace("\\", "/");
            var imp       = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (imp == null) continue;

            imp.textureType         = TextureImporterType.Sprite;
            imp.spriteImportMode    = SpriteImportMode.Single;
            imp.filterMode          = filter;
            imp.alphaIsTransparency = true;
            imp.mipmapEnabled       = false;
            imp.isReadable          = true;   // 런타임 Sprite.Create 폴백용
            imp.spritePixelsPerUnit = ppu;
            imp.SaveAndReimport();
            count++;
        }
        if (count > 0)
            Debug.Log($"[SetupSpriteAssets] {folder} — {count}개 설정 완료");
    }

    static void UpgradePlayer()
    {
        var player = Object.FindFirstObjectByType<PlayerController2D>();
        if (player == null)
        {
            Debug.LogWarning("[SetupSpriteAssets] Player 없음 — 씬에서 수동 추가 필요");
            return;
        }

        // ── Spine 컴포넌트 제거 ────────────────────────────────
        // PlayerSpineAnimator 제거
        var psa = player.GetComponent<PlayerSpineAnimator>();
        if (psa != null)
        {
            Object.DestroyImmediate(psa);
            Debug.Log("[SetupSpriteAssets] PlayerSpineAnimator 제거");
        }

        // SkeletonAnimation / SkeletonGraphic 제거 (Spine 있을 경우)
        // 리플렉션으로 처리하여 Spine 패키지 없어도 컴파일 가능
        foreach (var comp in player.GetComponents<Component>())
        {
            if (comp == null) continue;
            string typeName = comp.GetType().Name;
            if (typeName == "SkeletonAnimation" || typeName == "SkeletonMecanim")
            {
                Object.DestroyImmediate(comp);
                Debug.Log($"[SetupSpriteAssets] {typeName} 제거");
            }
        }

        // ── SpriteRenderer 추가 ───────────────────────────────
        var sr = player.GetComponent<SpriteRenderer>()
              ?? player.gameObject.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 5;

        // ── SpriteAnimator 추가 ───────────────────────────────
        var sa = player.GetComponent<SpriteAnimator>();
        if (sa == null)
        {
            sa = player.gameObject.AddComponent<SpriteAnimator>();
            sa.characterFolder = "Adventurer";   // 기본 캐릭터
        }

        EditorUtility.SetDirty(player.gameObject);
        Debug.Log("[SetupSpriteAssets] Player → SpriteAnimator(Adventurer) 교체 완료");
    }

    static void EnsureEffectManager()
    {
        if (Object.FindFirstObjectByType<EffectManager>() != null) return;

        var go = new GameObject("EffectManager");
        go.AddComponent<EffectManager>();
        EditorUtility.SetDirty(go);
        Debug.Log("[SetupSpriteAssets] EffectManager 씬에 배치 완료");
    }
}
