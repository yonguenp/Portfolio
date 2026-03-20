using UnityEngine;
using UnityEditor;
using Spine.Unity;

public static class SetupPlayerSpine
{
    [MenuItem("Tools/Setup Player Spine")]
    public static void Run()
    {
        var player = GameObject.Find("Player");
        if (player == null) { Debug.LogError("[SetupPlayerSpine] Player not found!"); return; }

        var skeletonDataAsset = AssetDatabase.LoadAssetAtPath<SkeletonDataAsset>(
            "Assets/Resources/Spine/Dragon/metatoy_1_SkeletonData.asset");
        if (skeletonDataAsset == null) { Debug.LogError("[SetupPlayerSpine] SkeletonDataAsset not found!"); return; }

        // SpriteRenderer 제거
        var sr = player.GetComponent<SpriteRenderer>();
        if (sr != null) Object.DestroyImmediate(sr);

        // SkeletonAnimation 추가 및 에셋 할당
        var skAnim = player.GetComponent<SkeletonAnimation>();
        if (skAnim == null) skAnim = player.AddComponent<SkeletonAnimation>();
        skAnim.skeletonDataAsset = skeletonDataAsset;
        skAnim.Initialize(true);

        // PlayerSpineAnimator 추가
        var psa = player.GetComponent<PlayerSpineAnimator>();
        if (psa == null) psa = player.AddComponent<PlayerSpineAnimator>();

        EditorUtility.SetDirty(player);
        Debug.Log("[SetupPlayerSpine] 완료! SkeletonAnimation + PlayerSpineAnimator 적용됨.");
    }
}
