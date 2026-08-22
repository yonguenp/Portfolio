using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class iOSBuilder
{
    const string OUTPUT = "/Users/yonguen/UnityWithClaude/Build/iOS";

    // 2026-08-20: GoStopScene(2인 맞고)·GoStop3PScene(3~4인 고스톱)이
    // 빠져 있었다 — 이 배열이 BuildPlayerOptions.scenes로 그대로
    // 넘어가서 Build Profiles/EditorBuildSettings의 씬 목록을 완전히
    // 무시하고 이 목록만 쓴다. 그래서 그쪽을 아무리 고쳐도(File → Build
    // Profiles에 9개 씬이 다 보여도) 이 커스텀 빌드 메뉴로 뽑은 iOS
    // 빌드에는 반영이 안 됐다 — "Scene 'GoStopScene' couldn't be loaded"
    // 런타임 에러의 진짜 원인이었다. 새 씬을 추가할 때는 EditorBuildSettings
    // 뿐 아니라 이 배열도 같이 업데이트할 것.
    static readonly string[] Scenes =
    {
        "Assets/Scenes/SplashScene.unity",
        "Assets/Scenes/TitleScene.unity",
        "Assets/Scenes/GameScene.unity",
        "Assets/Scenes/Game1to50Scene.unity",
        "Assets/Scenes/Game2048Scene.unity",
        "Assets/Scenes/Game1010Scene.unity",
        "Assets/Scenes/GameBrickBreakerScene.unity",
        "Assets/Scenes/GoStopScene.unity",
        "Assets/Scenes/GoStop3PScene.unity",
    };

    [MenuItem("Build/Build iOS")]
    public static void Build()
    {
        // Player settings
        PlayerSettings.companyName              = "Yonguen";
        PlayerSettings.productName              = "UnityWithClaude";
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.iOS, "com.yonguen.unitywithclaude");
        PlayerSettings.iOS.targetOSVersionString = "14.0";
        PlayerSettings.iOS.appleEnableAutomaticSigning = true;
        PlayerSettings.iOS.appleDeveloperTeamID = ""; // Xcode에서 자동 선택

        PlayerSettings.SetScriptingBackend(BuildTargetGroup.iOS, ScriptingImplementation.IL2CPP);

        // 2026-08-20: 이전엔 여기서 "Landscape only"로 강제하고 있었다 —
        // CLAUDE.md "화면 방향(가로 고정) 설정 버그" 섹션에서 이미 한 번
        // 정리한 프로젝트 전역 기본값(세로 기본, GoStop3PGame·BrickBreaker3D만
        // 자기 씬에 들어올 때 Screen.orientation으로 직접 가로를 강제)과
        // 정반대다. 이 빌드 스크립트가 매번 PlayerSettings를 가로로 덮어써서,
        // 에디터에서 아무리 세로로 맞춰놔도 이 메뉴로 빌드할 때마다 도로
        // 가로 전용으로 되돌아가고 있었다 — 세로로 설계된 나머지 7개
        // 게임이 iOS에서 전부 가로로 눌린 채 나왔을 가능성이 높다.
        // 프로젝트 기본값(세로)에 맞춘다.
        PlayerSettings.defaultInterfaceOrientation              = UIOrientation.Portrait;
        PlayerSettings.allowedAutorotateToPortrait              = true;
        PlayerSettings.allowedAutorotateToPortraitUpsideDown    = false;
        PlayerSettings.allowedAutorotateToLandscapeLeft         = false;
        PlayerSettings.allowedAutorotateToLandscapeRight        = false;

        System.IO.Directory.CreateDirectory(OUTPUT);

        var options = new BuildPlayerOptions
        {
            scenes      = Scenes,
            locationPathName = OUTPUT,
            target      = BuildTarget.iOS,
            options     = BuildOptions.None,
        };

        var report = BuildPipeline.BuildPlayer(options);
        var summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
            Debug.Log($"iOS build succeeded: {summary.totalSize / 1024 / 1024} MB");
        else
            Debug.LogError($"iOS build FAILED: {summary.totalErrors} errors");
    }
}
