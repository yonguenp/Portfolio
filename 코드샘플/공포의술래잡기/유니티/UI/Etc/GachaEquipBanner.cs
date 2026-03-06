using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GachaEquipBanner : MonoBehaviour
{
    [SerializeField] SkeletonGraphic skeleton;
    Coroutine animCoroutine = null;
    private void Awake()
    {
        skeleton.initialSkinName = "default";
        skeleton.Initialize(true);
    }
    void Start()
    {
        PlayIdle();
    }

    // Update is called once per frame
    public void PlayIdle()
    {
        StopIdle();

        animCoroutine = StartCoroutine(Play());
    }
    public IEnumerator Play()
    {
        while (true)
        {
            Spine.TrackEntry track = null;
            if (Random.value <= 0.3f)
                track = skeleton.AnimationState.SetAnimation(0, "f_idle_1", false);
            else
                track = skeleton.AnimationState.SetAnimation(0, "f_idle_0", false);

            yield return new WaitForSpineAnimationComplete(track);
        }
    }

    public void StopIdle()
    {
        if(animCoroutine != null)
        {
            StopCoroutine(animCoroutine);
            animCoroutine = null;
        }
    }
}
