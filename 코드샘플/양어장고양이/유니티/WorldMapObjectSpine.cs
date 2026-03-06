using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldMapObjectSpine : MonoBehaviour
{
    private SkeletonGraphic target;

    public string stopAnimationName = "stop";
    public string IdleAnimationName = "idle";

    public float StopMinTime = 15.0f;
    public float StopMaxTime = 30.0f;
    private void Awake()
    {
        target = GetComponent<SkeletonGraphic>();
        if(target)
        {
            StopAnimation();
        }
    }

    public void StopAnimation()
    {
        Spine.Animation animationObject = target.skeletonDataAsset.GetSkeletonData(false).FindAnimation(stopAnimationName);
        if (animationObject == null)
        {
            return;
        }

        target.AnimationState.SetAnimation(0, stopAnimationName, false);
        Invoke("IdleAnimation", Random.Range(StopMinTime, StopMaxTime));
    }

    public void IdleAnimation()
    {
        Spine.Animation animationObject = target.skeletonDataAsset.GetSkeletonData(false).FindAnimation(IdleAnimationName);
        if (animationObject == null)
        {   
            return;
        }

        target.AnimationState.SetAnimation(0, IdleAnimationName, false);
        Invoke("StopAnimation", animationObject.Duration);
    }
}
