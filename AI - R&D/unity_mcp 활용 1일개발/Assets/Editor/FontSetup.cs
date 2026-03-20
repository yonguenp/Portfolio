using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using UnityEngine.TextCore.LowLevel;

/// <summary>
/// ONE Mobile POP.ttf → TMP SDF FontAsset 생성.
/// - 4096×4096 아틀라스 (가-힣 전체 수록)
/// - 한글 전체(AC00-D7A3, 11172자) + ASCII 미리 등록
/// Menu: Tools > Apply ONE Mobile Pop Font
/// </summary>
public static class FontSetup
{
    const string TTF_PATH   = "Assets/Resources/Font/ONE Mobile POP.ttf";
    const string ASSET_PATH = "Assets/Resources/Font/ONE Mobile POP SDF.asset";

    // ── 공개 접근용 (SetupRunnerUI에서 호출) ──────────────────
    public static TMP_FontAsset GetOrCreateFont()
    {
        var existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(ASSET_PATH);
        if (existing != null) return existing;
        return BuildFont();
    }

    // ── 메뉴: 강제 재생성 + 씬 전체 적용 ─────────────────────
    [MenuItem("Tools/Apply ONE Mobile Pop Font")]
    public static void ApplyFont()
    {
        // 기존 에셋 삭제 후 재생성 (문자 누락 방지)
        if (AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(ASSET_PATH) != null)
        {
            AssetDatabase.DeleteAsset(ASSET_PATH);
            AssetDatabase.Refresh();
        }

        var font = BuildFont();
        if (font == null) return;

        ApplyToScene(font);
    }

    // ── 폰트 에셋 빌드 ────────────────────────────────────────
    static TMP_FontAsset BuildFont()
    {
        var ttf = AssetDatabase.LoadAssetAtPath<Font>(TTF_PATH);
        if (ttf == null)
        {
            Debug.LogError("[FontSetup] TTF 파일 없음: " + TTF_PATH);
            return null;
        }

        // 4096×4096, SamplingPoint 40 → 한글 전체를 한 페이지에 수록 가능
        var asset = TMP_FontAsset.CreateFontAsset(
            ttf,
            samplingPointSize:       40,
            atlasPadding:            6,
            renderMode:              GlyphRenderMode.SDFAA,
            atlasWidth:              4096,
            atlasHeight:             4096,
            atlasPopulationMode:     AtlasPopulationMode.Dynamic,
            enableMultiAtlasSupport: true
        );
        asset.name = "ONE Mobile POP SDF";

        // 먼저 디스크에 저장
        AssetDatabase.CreateAsset(asset, ASSET_PATH);
        AssetDatabase.SaveAssets();

        // 한글 전체(가-힣) + ASCII 미리 등록
        string chars = BuildCharSet();
        bool ok = asset.TryAddCharacters(chars, out string missing);
        if (!ok && !string.IsNullOrEmpty(missing))
            Debug.LogWarning($"[FontSetup] 미등록 문자 {missing.Length}개: " + missing);

        EditorUtility.SetDirty(asset);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("[FontSetup] 폰트 에셋 생성 완료: " + ASSET_PATH);
        return asset;
    }

    // ── 문자 집합: ASCII + 한글 전체 ─────────────────────────
    static string BuildCharSet()
    {
        var sb = new System.Text.StringBuilder();

        // ASCII 출력 가능 문자 (32~126)
        for (int c = 32; c <= 126; c++)
            sb.Append((char)c);

        // 한글 자모 (U+1100~U+11FF)
        for (int c = 0x1100; c <= 0x11FF; c++)
            sb.Append((char)c);

        // 한글 호환 자모 (U+3130~U+318F)
        for (int c = 0x3130; c <= 0x318F; c++)
            sb.Append((char)c);

        // 한글 음절 전체 가-힣 (U+AC00~U+D7A3, 11172자)
        for (int c = 0xAC00; c <= 0xD7A3; c++)
            sb.Append((char)c);

        return sb.ToString();
    }

    // ── 씬 내 TMP 전체에 폰트 적용 ───────────────────────────
    static void ApplyToScene(TMP_FontAsset font)
    {
        int count = 0;

        var uguis = Object.FindObjectsByType<TextMeshProUGUI>(
            FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var t in uguis) { t.font = font; EditorUtility.SetDirty(t); count++; }

        var tmps = Object.FindObjectsByType<TextMeshPro>(
            FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var t in tmps) { t.font = font; EditorUtility.SetDirty(t); count++; }

        EditorSceneManager.SaveOpenScenes();
        Debug.Log($"[FontSetup] 폰트 적용 완료 → {count}개 TMP (씬 저장됨)");
    }
}
