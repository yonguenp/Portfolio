using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;
using System.IO;

/// <summary>
/// Google Play Store 배포용 Android 빌드 자동화.
/// Tools/Android 메뉴에서 실행.
/// </summary>
public static class AndroidBuild
{
    // ── 앱 정보 (여기만 수정하세요) ────────────────────────────
    const string COMPANY_NAME    = "DefaultCompany";
    const string PRODUCT_NAME    = "InfiniteRunner";
    const string PACKAGE_NAME    = "com.defaultcompany.infiniterunner";
    const string VERSION_NAME    = "1.0.0";          // 사용자 표시 버전
    const int    VERSION_CODE    = 1;                 // 스토어 업로드마다 +1

    // ── 키스토어 경로 (서명 필수) ─────────────────────────────
    // 없으면 메뉴에서 "키스토어 생성" 먼저 실행
    const string KEYSTORE_PATH   = "keystore/infiniterunner.keystore";
    const string KEYSTORE_PASS   = "your_keystore_password";   // 실제 비번으로 교체
    const string KEY_ALIAS       = "infiniterunner";
    const string KEY_ALIAS_PASS  = "your_key_password";        // 실제 비번으로 교체

    // ── 출력 경로 ─────────────────────────────────────────────
    const string BUILD_DIR       = "Builds/Android";

    // ─────────────────────────────────────────────────────────
    //  메뉴 항목
    // ─────────────────────────────────────────────────────────

    [MenuItem("Tools/Android/① 앱 설정 적용 (패키지명·버전)", priority = 100)]
    public static void ApplyPlayerSettings()
    {
        PlayerSettings.companyName            = COMPANY_NAME;
        PlayerSettings.productName            = PRODUCT_NAME;
        PlayerSettings.applicationIdentifier  = PACKAGE_NAME; // 전체 플랫폼
        PlayerSettings.SetApplicationIdentifier(
            BuildTargetGroup.Android, PACKAGE_NAME);
        PlayerSettings.bundleVersion          = VERSION_NAME;
        PlayerSettings.Android.bundleVersionCode = VERSION_CODE;

        // 세로 고정
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
        PlayerSettings.allowedAutorotateToPortrait  = true;
        PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
        PlayerSettings.allowedAutorotateToLandscapeLeft  = false;
        PlayerSettings.allowedAutorotateToLandscapeRight = false;

        // 최소 API 레벨 (Android 7.0 = 24)
        PlayerSettings.Android.minSdkVersion    = AndroidSdkVersions.AndroidApiLevel24;
        PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;

        // IL2CPP (스토어 권장)
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android,
            ScriptingImplementation.IL2CPP);
        PlayerSettings.Android.targetArchitectures =
            AndroidArchitecture.ARM64 | AndroidArchitecture.ARMv7;

        // Internet 권한 (광고 필요)
        PlayerSettings.Android.forceInternetPermission = true;

        AssetDatabase.SaveAssets();
        Debug.Log($"[AndroidBuild] 앱 설정 적용 완료\n" +
                  $"패키지: {PACKAGE_NAME}\n버전: {VERSION_NAME} ({VERSION_CODE})");
        EditorUtility.DisplayDialog("앱 설정 완료",
            $"패키지명: {PACKAGE_NAME}\n버전: {VERSION_NAME} (코드 {VERSION_CODE})\n\n" +
            "다음: ② 씬 등록 → ③ 키스토어 설정 → ④ AAB 빌드",
            "확인");
    }

    [MenuItem("Tools/Android/② 빌드 씬 등록 (InfiniteRunner)", priority = 101)]
    public static void AddSceneToBuild()
    {
        const string SCENE_PATH = "Assets/Scenes/InfiniteRunner.unity/InfiniteRunner.unity";
        const string SCENE_PATH2 = "Assets/Scenes/InfiniteRunner.unity";

        string actualPath = File.Exists(SCENE_PATH) ? SCENE_PATH :
                            File.Exists(SCENE_PATH2 + "/InfiniteRunner.unity") ? SCENE_PATH :
                            FindScene("InfiniteRunner");

        if (actualPath == null)
        {
            EditorUtility.DisplayDialog("씬 없음",
                "InfiniteRunner 씬을 찾을 수 없어요!\n" +
                "Assets/Scenes/InfiniteRunner.unity 경로 확인 필요.", "확인");
            return;
        }

        var scenes = new EditorBuildSettingsScene[]
        {
            new EditorBuildSettingsScene(actualPath, true)
        };
        EditorBuildSettings.scenes = scenes;
        AssetDatabase.SaveAssets();

        Debug.Log("[AndroidBuild] 씬 등록 완료: " + actualPath);
        EditorUtility.DisplayDialog("씬 등록 완료",
            "빌드 씬 등록:\n" + actualPath + "\n\n다음: ③ 키스토어 설정", "확인");
    }

    [MenuItem("Tools/Android/③ 키스토어 설정 확인", priority = 102)]
    public static void CheckKeystore()
    {
        string fullPath = Path.GetFullPath(KEYSTORE_PATH);
        bool exists = File.Exists(fullPath);

        if (!exists)
        {
            bool create = EditorUtility.DisplayDialog("키스토어 없음",
                $"키스토어 파일이 없습니다:\n{fullPath}\n\n" +
                "Unity에서 직접 생성하려면 확인을 누르세요.\n" +
                "(Project Settings → Player → Android → Publishing Settings)",
                "Project Settings 열기", "취소");
            if (create)
            {
                EditorApplication.ExecuteMenuItem(
                    "Edit/Project Settings...");
            }
            return;
        }

        // 키스토어 정보 PlayerSettings에 적용
        PlayerSettings.Android.useCustomKeystore   = true;
        PlayerSettings.Android.keystoreName        = fullPath;
        PlayerSettings.Android.keystorePass        = KEYSTORE_PASS;
        PlayerSettings.Android.keyaliasName        = KEY_ALIAS;
        PlayerSettings.Android.keyaliasPass        = KEY_ALIAS_PASS;
        AssetDatabase.SaveAssets();

        Debug.Log("[AndroidBuild] 키스토어 설정 완료: " + fullPath);
        EditorUtility.DisplayDialog("키스토어 설정 완료",
            "서명 설정 완료!\n다음: ④ AAB 빌드 실행", "확인");
    }

    [MenuItem("Tools/Android/④ AAB 빌드 (Play Store용)", priority = 103)]
    public static void BuildAAB()
    {
        if (!ValidatePreBuild()) return;

        Directory.CreateDirectory(BUILD_DIR);
        string outputPath = Path.Combine(BUILD_DIR,
            $"{PRODUCT_NAME}_v{VERSION_NAME}_{VERSION_CODE}.aab");

        // AAB 모드 설정
        EditorUserBuildSettings.buildAppBundle = true;
        EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;

        var options = new BuildPlayerOptions
        {
            scenes       = GetScenePaths(),
            locationPathName = outputPath,
            target       = BuildTarget.Android,
            options      = BuildOptions.None
        };

        Debug.Log($"[AndroidBuild] AAB 빌드 시작...\n출력: {Path.GetFullPath(outputPath)}");

        var report = BuildPipeline.BuildPlayer(options);
        HandleBuildResult(report, outputPath);
    }

    [MenuItem("Tools/Android/④-b APK 빌드 (테스트용)", priority = 104)]
    public static void BuildAPK()
    {
        if (!ValidatePreBuild()) return;

        Directory.CreateDirectory(BUILD_DIR);
        string outputPath = Path.Combine(BUILD_DIR,
            $"{PRODUCT_NAME}_v{VERSION_NAME}_{VERSION_CODE}_debug.apk");

        EditorUserBuildSettings.buildAppBundle = false;
        EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;

        var options = new BuildPlayerOptions
        {
            scenes       = GetScenePaths(),
            locationPathName = outputPath,
            target       = BuildTarget.Android,
            options      = BuildOptions.Development | BuildOptions.AllowDebugging
        };

        Debug.Log($"[AndroidBuild] APK 빌드 시작...\n출력: {Path.GetFullPath(outputPath)}");

        var report = BuildPipeline.BuildPlayer(options);
        HandleBuildResult(report, outputPath);
    }

    [MenuItem("Tools/Android/⑤ 빌드 폴더 열기", priority = 200)]
    public static void OpenBuildFolder()
    {
        string full = Path.GetFullPath(BUILD_DIR);
        Directory.CreateDirectory(full);
        EditorUtility.RevealInFinder(full);
    }

    // ─────────────────────────────────────────────────────────
    //  내부 헬퍼
    // ─────────────────────────────────────────────────────────

    static bool ValidatePreBuild()
    {
        // Android 모듈 설치 확인
        if (!BuildPipeline.IsBuildTargetSupported(
            BuildTargetGroup.Android, BuildTarget.Android))
        {
            EditorUtility.DisplayDialog("Android 모듈 없음",
                "Unity Hub에서 Android Build Support 모듈을 먼저 설치하세요!\n\n" +
                "Unity Hub → 설치 → 해당 버전 → 모듈 추가 → Android Build Support 체크",
                "확인");
            return false;
        }

        // 씬 등록 확인
        if (EditorBuildSettings.scenes.Length == 0)
        {
            bool fix = EditorUtility.DisplayDialog("씬 미등록",
                "빌드 씬이 없습니다. 지금 등록할까요?", "등록", "취소");
            if (fix) AddSceneToBuild();
            return false;
        }

        // 플랫폼 전환
        if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
        {
            bool sw = EditorUtility.DisplayDialog("플랫폼 전환 필요",
                "현재 플랫폼이 Android가 아닙니다.\nAndroid로 전환할까요?\n(수 분 소요)", "전환", "취소");
            if (sw)
            {
                EditorUserBuildSettings.SwitchActiveBuildTarget(
                    BuildTargetGroup.Android, BuildTarget.Android);
            }
            return false;
        }

        return true;
    }

    static string[] GetScenePaths()
    {
        var paths = new System.Collections.Generic.List<string>();
        foreach (var s in EditorBuildSettings.scenes)
            if (s.enabled) paths.Add(s.path);
        return paths.ToArray();
    }

    static void HandleBuildResult(BuildReport report, string outputPath)
    {
        if (report.summary.result == BuildResult.Succeeded)
        {
            long size = new FileInfo(outputPath).Length / (1024 * 1024);
            Debug.Log($"[AndroidBuild] ✅ 빌드 성공! {size}MB\n{Path.GetFullPath(outputPath)}");

            bool open = EditorUtility.DisplayDialog("빌드 성공! 🎉",
                $"✅ 빌드 완료!\n크기: {size}MB\n경로: {Path.GetFullPath(outputPath)}\n\n" +
                "Google Play Console에 업로드하려면:\n" +
                "play.google.com/console → 앱 선택 → 프로덕션/내부 테스트 → 새 출시 만들기 → AAB 업로드",
                "폴더 열기", "닫기");
            if (open) EditorUtility.RevealInFinder(outputPath);
        }
        else
        {
            var errors = "";
            foreach (var step in report.steps)
                foreach (var msg in step.messages)
                    if (msg.type == LogType.Error)
                        errors += "\n• " + msg.content;

            Debug.LogError("[AndroidBuild] ❌ 빌드 실패!\n" + errors);
            EditorUtility.DisplayDialog("빌드 실패 ❌",
                "빌드 중 오류 발생!\nUnity Console에서 에러 확인 필요.\n\n" +
                "자주 발생하는 원인:\n" +
                "• Android SDK/JDK 경로 오류\n" +
                "• 키스토어 비밀번호 틀림\n" +
                "• Gradle 빌드 오류 (Console 확인)", "확인");
        }
    }

    static string FindScene(string name)
    {
        string[] guids = AssetDatabase.FindAssets($"t:Scene {name}");
        if (guids.Length > 0)
            return AssetDatabase.GUIDToAssetPath(guids[0]);
        return null;
    }
}
