using SandboxNetwork;
using Spine;
using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BossBubble : MonoBehaviour
{
    [SerializeField] private Image bossImg = null;
    [SerializeField] private Image bossBg = null;
    [SerializeField] private SkeletonGraphic spine = null;
    [SerializeField] private Material normal = null;
    [SerializeField] private Material grayscale = null;
    public void SetData(Sprite sprite, Color colr)
    {
        bossImg.sprite = sprite;
        bossBg.color = colr;
    }

    public void SetData(int world, string prefabPath, bool isFinal, bool isBoss, bool cleared)
    {
        var monsterPrefab = ResourceManager.GetResource<GameObject>(eResourcePath.MonsterClonePath, prefabPath);
        SkeletonAnimation anim = monsterPrefab.GetComponentInChildren<SkeletonAnimation>();
        spine.skeletonDataAsset = anim.skeletonDataAsset;
        spine.initialSkinName = anim.initialSkinName;
        spine.startingAnimation = "monster_idle";
        spine.startingLoop = !cleared;
        if (cleared == true)
        {
            spine.material = grayscale;
        }
        else
        {
            spine.material = normal;
        }

        spine.Initialize(true);

        spine.MatchRectTransformWithBounds();
        
        float scale = 1.0f;
        if(isFinal)
        {
            if(isBoss)                    
                scale = (world == 8 ? 800f : 600f) / (spine.transform as RectTransform).sizeDelta.y;
            else
                scale = Random.Range(100,150) / (spine.transform as RectTransform).sizeDelta.y;
        }
        else
        {
            if (isBoss)
                scale = 300f / (spine.transform as RectTransform).sizeDelta.y;
            else
                scale = Random.Range(100, 150) / (spine.transform as RectTransform).sizeDelta.y;
        }

        spine.transform.localScale = Vector3.one * scale;

        if (cleared == false)
        {
            StartCoroutine(Animation());
        }
        else
        {
            spine.AnimationState.GetCurrent(0).TimeScale = 0.0f;
        }
    }

    IEnumerator Animation()
    {
        yield return new WaitForEndOfFrame();

        while (true)
        {
            TrackEntry track = null;
            switch (Random.Range(0,50))
            {
                case 0:                
                case 4:
                case 5:
                    track = spine.AnimationState.SetAnimation(0, "monster_attack", false);
                    break;    
                case 8:
                    track = spine.AnimationState.SetAnimation(0, "monster_casting", false);
                    yield return new Spine.Unity.WaitForSpineAnimationComplete(track);
                    track = spine.AnimationState.SetAnimation(0, "monster_skill1", false);
                    break;
                default:
                    track = spine.AnimationState.SetAnimation(0, "monster_idle", false);
                    track.TimeScale = 0.5f + Random.value;
                    break;
            }

            
            yield return new Spine.Unity.WaitForSpineAnimationComplete(track);
        }
    }
}
