using Spine.Unity;
using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class PickupBanner : MonoBehaviour
{
    [SerializeField]
    Transform Container;
    [SerializeField]
    SkeletonGraphic target;

    List<int> targetCharacters = new List<int>();

    public void SetTarget(List<int> type)
    {
        foreach (Transform child in Container)
        {
            if (child == target.transform)
            {
                continue;
            }

            Destroy(child.gameObject);
        }

        targetCharacters = type;
        if (targetCharacters.Count <= 0)
            return;

        int idx = 0;
        foreach (int targetChar in targetCharacters)
        {
            var charObj = GameObject.Instantiate(target, Container);
            charObj.gameObject.SetActive(true);

            SkeletonGraphic skel = charObj.GetComponent<SkeletonGraphic>();
            var info = CharacterGameData.GetCharacterData(targetChar).spine_resource;
            skel.skeletonDataAsset = info;

            skel.startingAnimation = "f_idle_0";

            skel.initialFlipX = true;
            //준형 :: 마리아때 서로 등돌리고 있게 해달라는 요청때문에 넣었습니다.
            /*
            if (idx == 0)
                skel.initialFlipX = true;
            else
                skel.initialFlipX = false;  
            */
            skel.startingLoop = true;
            skel.Initialize(true);
            skel.gameObject.SetActive(true);

            skel.DOColor(new Color(0.3333333333333333f, 0.3333333333333333f, 0.3333333333333333f), 0.4f).SetDelay(0.2f).From();
            idx++;
        }

        target.gameObject.SetActive(false);
    }
}
