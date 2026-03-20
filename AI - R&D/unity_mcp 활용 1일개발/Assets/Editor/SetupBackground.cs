using UnityEngine;
using UnityEditor;

public static class SetupBackground
{
    [MenuItem("Tools/Setup Background")]
    public static void Run()
    {
        // 기존 배경 오브젝트 제거 후 재생성
        var old = GameObject.Find("BackgroundManager");
        if (old != null) Object.DestroyImmediate(old);

        var go = new GameObject("BackgroundManager");
        go.AddComponent<BackgroundManager>();

        // 씬 최상위에 배치
        EditorUtility.SetDirty(go);
        Debug.Log("[SetupBackground] BackgroundManager 생성 완료! Play 시 배경 레이어가 자동 생성됩니다.");
    }
}
