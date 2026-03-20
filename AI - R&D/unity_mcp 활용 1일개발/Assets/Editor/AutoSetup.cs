using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// 프로젝트 열기 / 스크립트 재컴파일 시 자동으로 에셋 임포트 설정 적용.
/// 이미 설정된 경우 건너뜀 (빠른 체크).
/// </summary>
[InitializeOnLoad]
public static class AutoSetup
{
    const string PREFS_KEY = "AutoSetup_Done_v3";

    static AutoSetup()
    {
        // 도메인 리로드마다 한 번, 에디터가 완전히 로드된 후 실행
        EditorApplication.delayCall += OnEditorReady;
    }

    static void OnEditorReady()
    {
        EditorApplication.delayCall -= OnEditorReady;

        // 이미 설정 완료된 경우 건너뜀
        if (EditorPrefs.GetBool(PREFS_KEY, false)) return;

        // 코인 시트 확인 — 아직 Multiple 슬라이싱 안 됐으면 Setup 실행
        bool needSetup = false;

        const string coinPath = "Assets/Resources/Items/coin_sheet.png";
        if (File.Exists(coinPath))
        {
            var imp = AssetImporter.GetAtPath(coinPath) as TextureImporter;
            if (imp != null && imp.spriteImportMode != SpriteImportMode.Multiple)
                needSetup = true;
        }

        // 캐릭터 스프라이트 확인
        const string advPath = "Assets/Resources/Characters/Adventurer/adventurer_idle.png";
        if (File.Exists(advPath))
        {
            var imp = AssetImporter.GetAtPath(advPath) as TextureImporter;
            if (imp != null && imp.textureType != TextureImporterType.Sprite)
                needSetup = true;
        }

        if (needSetup)
        {
            Debug.Log("[AutoSetup] 에셋 임포트 설정 자동 적용 중...");
            SetupSpriteAssets.Run();
            EditorPrefs.SetBool(PREFS_KEY, true);
        }
        else
        {
            EditorPrefs.SetBool(PREFS_KEY, true);
        }
    }

    /// <summary>설정 캐시 초기화 (강제 재실행용).</summary>
    [MenuItem("Tools/Reset Auto Setup Cache")]
    public static void ResetCache()
    {
        EditorPrefs.DeleteKey(PREFS_KEY);
        Debug.Log("[AutoSetup] 캐시 초기화 완료. 다음 컴파일 시 자동 재실행됩니다.");
    }
}
