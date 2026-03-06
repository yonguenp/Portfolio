using DG.Tweening;
using Newtonsoft.Json.Linq;
using Spine;
using Spine.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NecoCatSpotContainerFishingRod : NecoCatSpotContainer
{
    public GameObject normalGilmak;
    public GameObject specialGilmak;

    public override void RefreshCat(neco_spot spot)
    {
        base.RefreshCat(spot);

        neco_data.PrologueSeq seq = neco_data.GetPrologueSeq();
        if(seq == neco_data.PrologueSeq.길막이낚시장난감배치)
        {
            normalGilmak.SetActive(false);
            specialGilmak.SetActive(true);

            Spine.Unity.SkeletonGraphic spine = specialGilmak.GetComponent<Spine.Unity.SkeletonGraphic>();
            Spine.Animation animationObject = spine.skeletonDataAsset.GetSkeletonData(false).FindAnimation("animation");
            
            CancelInvoke("길막이특수애니끝");
            Invoke("길막이특수애니끝", animationObject.Duration - 3.0f);
        }
        else
        {
            normalGilmak.SetActive(true);
            specialGilmak.SetActive(false);
        }
    }

    public void 길막이특수애니끝()
    {
        if (neco_data.GetPrologueSeq() == neco_data.PrologueSeq.길막이낚시장난감배치)
        {
            MapObjectController mapController = NecoCanvas.GetGameCanvas().GetCurMapController();
            if (mapController != null)
            {
                mapController.SendMessage("길막이낚시돌발등장", SendMessageOptions.DontRequireReceiver);
            }
        }
    }
}
