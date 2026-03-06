using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;
using Spine;

public class IdleAnimObject : MapObjectBase
{

    protected SkeletonAnimation skeletonAnimation = null;
    protected Spine.AnimationState animState = null;
    Coroutine IdleAnimationCoritine = null;

    [SerializeField]
    float minAniTime = 2;
    [SerializeField]
    float maxAniTime = 5;

    // Start is called before the first frame update
    void Start()
    {
        base.Init();

        if (model)
        {
            skeletonAnimation = model.GetComponent<SkeletonAnimation>();
            if (skeletonAnimation == null) return;
            animState = skeletonAnimation.AnimationState;
        }

        animState.SetAnimation(0, "f_play_0", true);

        IdleAnimationCoritine = StartCoroutine(UpdateOneSecCO());

        animState.Complete += OnCompleteEvent;
    }

    IEnumerator UpdateOneSecCO()
    {
        var playTime = Random.Range(minAniTime, maxAniTime);
        float time = 0;

        while (true)
        {
            if (time > playTime)
            {
                animState.SetAnimation(0, "f_play_1", false);
                yield break;
            }
            time += Time.deltaTime;
            yield return null;
        }
    }

    void OnCompleteEvent(TrackEntry trackEntry)
    {
        if (trackEntry.Animation.Name.Equals("f_play_1"))
        {
            animState.SetAnimation(0, "f_play_0", true);
            StopCoroutine(IdleAnimationCoritine);
            IdleAnimationCoritine = StartCoroutine(UpdateOneSecCO());
        }
    }
}
