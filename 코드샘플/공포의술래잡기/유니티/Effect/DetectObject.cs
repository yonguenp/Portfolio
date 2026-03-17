using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetectObject : MonoBehaviour
{
    public SkeletonAnimation SkeletonAnimation;

    void Start()
    {
        CancelInvoke("PlayIdle");

        var isState = SkeletonAnimation.AnimationState.Data.SkeletonData.Animations.Find(_ => _.Name == "f_play_1");
        Spine.TrackEntry track = null;

        if (isState != null)
            track = SkeletonAnimation.AnimationState.SetAnimation(0, "f_play_1", false);
        else
            track = SkeletonAnimation.AnimationState.SetAnimation(0, "f_play_0", false);

        Invoke("PlayIdle", track.TrackTime);
    }

    void PlayIdle()
    {
        SkeletonAnimation.AnimationState.SetAnimation(0, "f_idle_0", false);
    }
}
