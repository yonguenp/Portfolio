using Spine;
using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatSkeletonGraphic : SkeletonGraphic
{
    public CatFollowerGraphic cursor;
    public cat_def curCatData;
    public ani_list curAnimationData;
    
    public GameObject EatMotion;

    private Coroutine AnimationCoroutine = null;
    private float animationDuration = 0.0f;
    protected override void Awake()
    {
        base.Awake();
        EatMotion.SetActive(false);
    }

    protected override void OnEnable()
    {
        base.OnEnable();
    }

    public override void Update()
    {
        base.Update();

        //FollowTransform();
    }

    public void InitAnimation(cat_def cat, CatFollowerGraphic target)
    {
        cursor = target;
        curCatData = cat;
        
        skeleton.SetSkin(curCatData.GetCatSkinName());
        curAnimationData = curCatData.GetRandomAnimationInfo();
        
        //개발중 예외처리
        int limitCount = 10;
        while (curAnimationData == null)
        {
            curAnimationData = curCatData.GetRandomAnimationInfo();
            limitCount--;

            if (limitCount < 0)
            {
                Debug.Log("not found Init Animation");
                Destroy(this.gameObject);
                return;
            }
        }

        ResetAnimation();
    }

    public void ResetAnimation()
    {
        string animName = curAnimationData.GetMoveAnimation();

        //개발중 예외처리
        Spine.Animation animationObject = skeletonDataAsset.GetSkeletonData(false).FindAnimation(animName);
        if(animationObject == null)
        {
            InitAnimation(curCatData, cursor);
            return;
        }

        ani_list.SIBLING_TYPE siblingType = curAnimationData.GetSiblingType();
        if(siblingType == ani_list.SIBLING_TYPE.FIRST)
        {
            transform.SetAsFirstSibling();
        }
        else
        {
            transform.SetAsLastSibling();
        }
        //skeletonDataAsset.defaultMix = 0.2f;
        state.SetAnimation(0, animName, false);

        cat_action_def interactionData = curAnimationData.GetRandomCatAction();
        cursor.SetTarget("cat", interactionData);

        //Invoke("ChangeAnimation", animationObject.Duration - 0.2f);
        if(AnimationCoroutine != null)
            StopCoroutine(AnimationCoroutine);

        AnimationCoroutine = StartCoroutine(AnimationRun(animationObject.Duration));
    }

    public void ChangeAnimation()
    {
        location_def endData = curAnimationData.GetEndLocation();
        List<ani_list> list = curCatData.GetLocationAnimationList(endData);
        curAnimationData = list[Random.Range(0, list.Count)];
        ResetAnimation();
    }
    

    protected override void OnDestroy()
    {
        base.OnDestroy();
        DestroyImmediate(cursor.gameObject);
    }

    [ContextMenu("OnStartEatAnimation")]
    public void OnStartEatAnimation(float time = 10.0f)
    {
        state.TimeScale = 0;
        EatMotion.SetActive(true);

        SkeletonGraphic eatMotion = EatMotion.GetComponent<SkeletonGraphic>();
        if (eatMotion)
        {
            eatMotion.Skeleton.SetSkin(curCatData.GetCatSkinName());
        }

        if (AnimationCoroutine != null)
            StopCoroutine(AnimationCoroutine);

        Color c = color;
        c.a = 0.0f;
        color = c;

        animationDuration += 0.2f;

        Invoke("OnDoneEatAnimation", time);
        cursor.ShowFeedProgressBar(time);
    }

    [ContextMenu("OnDoneEatAnimation")]
    public void OnDoneEatAnimation()
    {
        state.TimeScale = 1;
        EatMotion.SetActive(false);

        if (AnimationCoroutine != null)
            StopCoroutine(AnimationCoroutine);

        Color c = color;
        c.a = 1.0f;
        color = c;

        AnimationCoroutine = StartCoroutine(AnimationRun(animationDuration));
    }

    IEnumerator AnimationRun(float duration)
    {
        animationDuration = duration - 0.2f;

        while (animationDuration > 0)
        {
            animationDuration -= 0.1f;
            yield return new WaitForSeconds(0.1f);
        }

        ChangeAnimation();
    }
}
