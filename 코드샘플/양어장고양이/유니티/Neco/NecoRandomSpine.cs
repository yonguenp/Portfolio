using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;

public class NecoRandomSpine : MonoBehaviour
{
    public SkeletonGraphic skeletonGraphic;
    public float min = 30;
    public float max = 90;

    float duration;
    private void OnEnable()
    {
        if (skeletonGraphic != null)
        {
            skeletonGraphic.enabled = false;
            Invoke("OnSpinePlay", Random.Range(min, max));
        }
    }

    public void OnSpinePlay()
    {
        CancelInvoke("OnSpinePlay");

        if (skeletonGraphic != null && skeletonGraphic.skeletonDataAsset != null)
        {
            Spine.Animation animationObject = skeletonGraphic.skeletonDataAsset.GetSkeletonData(false).FindAnimation("animation");
            if (animationObject != null)
            {
                Invoke("OnSpinePlay", Mathf.Max(Random.Range(min, max), animationObject.Duration * Random.Range(1.0f, 3.0f)));
            }

            MapObjectController controller = GetComponentInParent<MapObjectController>();
            if (controller != null)
            {
                neco_map mapData = controller.GetMapData();
                if (mapData != null)
                {
                    foreach (neco_spot spot in mapData.GetSpots())
                    {
                        for (uint i = 0; i < 3; i++)
                        {
                            if (spot.GetCurSpotCat(i) != null)
                            {
                                return;
                            }
                        }
                    }
                }
            }

            skeletonGraphic.enabled = true;
            skeletonGraphic.AnimationState.ClearTracks();
            skeletonGraphic.AnimationState.SetAnimation(0, "animation", false);
        }
    }
}