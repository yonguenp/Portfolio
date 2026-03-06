using DG.Tweening;
using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TitleAnimation : MonoBehaviour
{
    [Header("Title 연출을 위한 애들")]
    
    [SerializeField]
    RectTransform[] ResolutionScaleTarget;
    
    [SerializeField]
    GameObject[] SideGroup;
    [SerializeField]
    RectTransform TitleSpine;
    [SerializeField]
    bool autoPlay = true;

    public bool titleAnimDone { get; private set; }

    private void Start()
    {
        Vector2 CanvasSize = (GetComponentInParent<Canvas>().transform as RectTransform).sizeDelta;
        (transform as RectTransform).sizeDelta = CanvasSize;

        if (autoPlay)
        {
            SetTitle();
            OnTitleAnimation();
        }
    }
    
    public void SetTitle()
    {
        titleAnimDone = false;

        Vector2 CanvasSize = (GetComponentInParent<Canvas>().transform as RectTransform).sizeDelta;
        float pivotRatio = GetComponentInParent<Canvas>().GetComponent<CanvasScaler>().referenceResolution.x / GetComponentInParent<Canvas>().GetComponent<CanvasScaler>().referenceResolution.y;
        float curRatio = CanvasSize.x / CanvasSize.y;
        float diff = 1.0f - (pivotRatio - curRatio);
        if (diff > 1.0f)
        {
            diff = diff - ((diff - 1.0f) * 0.9f);
        }

        foreach (RectTransform rt in ResolutionScaleTarget)
        {
            rt.GetComponent<Image>().SetNativeSize();
            rt.sizeDelta = rt.sizeDelta * 0.73f * diff;
        }

        foreach (GameObject sideObj in SideGroup)
        {
            sideObj.transform.localScale = Vector3.one * 0.5f;
            sideObj.transform.DOScale(Vector3.zero, 1.5f).From().OnComplete(() =>
            {
                sideObj.transform.DOShakeScale(10.0f, 0.1f, 1, 0.1f).SetLoops(-1, LoopType.Yoyo);
            });
        }
    }

    public void OnTitleAnimation()
    {
        Vector2 CanvasSize = (GetComponentInParent<Canvas>().transform as RectTransform).sizeDelta;
        float curRatio = CanvasSize.x / CanvasSize.y;
        TitleSpine.gameObject.SetActive(true);
        TitleSpine.localScale = curRatio > 1.7f ? Vector3.one : Vector3.one * (1.0f - (0.4f * ((1.7f - curRatio) / 0.7f)));

        //foreach (SkeletonGraphic graphic in GetComponentsInChildren<SkeletonGraphic>())
        //{
        //    graphic.AnimationState.AddAnimation(0, "f_play_1", true, 0.0f);
        //}
        Invoke("TitleAnimDone", 1.0f);
    }

    void TitleAnimDone()
    {
        CancelInvoke("TitleAnimDone");

        titleAnimDone = true;
        //if(Managers.Scene.CurrentScene.name != "Lobby")
        //    DOTween.KillAll();
    }
}

