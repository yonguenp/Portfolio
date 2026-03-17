using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapObjectControlMeetMother : MapObjectController
{
    public SkeletonGraphic motherAnimation;
    public GameObject ButtonObject;
    public override void OnInitMap(neco_map mapData)
    {
        curMapData = mapData;

        InitSpots();

        NecoCanvas.GetUICanvas().gameObject.SetActive(false);
        //NecoCanvas.GetPopupCanvas().gameObject.SetActive(false);
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        ButtonObject.SetActive(false);
        motherAnimation.Initialize(false);
        motherAnimation.AnimationState.SetAnimation(0, "tail", false);
        
        Spine.Animation animationObject = motherAnimation.skeletonDataAsset.GetSkeletonData(false).FindAnimation("tail");
        Invoke("OnButtonShow", animationObject.Duration);
    }

    public void OnButtonShow()
    {
        motherAnimation.AnimationState.SetAnimation(0, "tail_replay", true);
        ButtonObject.SetActive(true);
    }
}
