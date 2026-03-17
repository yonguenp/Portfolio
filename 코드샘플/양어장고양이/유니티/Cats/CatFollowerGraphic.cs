using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class CatFollowerGraphic : BoneFollowerGraphic
{
    public Button catSelectButton;
    
    public Button InteractionButton;
    public Image InteractionIcon;


    public Sprite[] InteractionSprite;

    public GameObject ProgressBarPanel;
    public Image ProgressBar;

    private WorldCatManager catManager;
    private CatSkeletonGraphic curCatSkeleton;

    private cat_action_def curAction = null;
    private float disableInteractionTime = 0.0f;
    public void Awake()
    {
        InteractionButton.onClick.AddListener(OnSelectAction);
        catSelectButton.onClick.AddListener(OnSelectCat);
    }

    public void Init(CatSkeletonGraphic target, WorldCatManager controller)
    {
        curCatSkeleton = target;
        skeletonGraphic = target;
        catManager = controller;
        curAction = null;

        ProgressBarPanel.SetActive(false);
    }

    public void SetTarget(string bone, cat_action_def interactionData)
    {
        gameObject.SetActive(SetBone(bone));
        InteractionButton.gameObject.SetActive(false);

        curAction = null;

        if (interactionData != null && interactionData.IsEnabled())
        {
            if (disableInteractionTime <= 0.0f)
                InteractionButton.gameObject.SetActive(true);
            curAction = interactionData;
            SetInteractionIcon();
        }
    }

    private void Update()
    {
        if(disableInteractionTime >= 0.0f)
        {
            disableInteractionTime -= Time.deltaTime;

            if (disableInteractionTime < 0)
            {
                disableInteractionTime = 0.0f;
                if(curAction != null)
                {
                    InteractionButton.gameObject.SetActive(true);
                }
            }
        }
    }

    public void SetInteractionIcon()
    {
        cat_action_def.INTERACTION_TYPE type = curAction.GetActionType();
        Sprite icon = InteractionSprite[(int)type];
        if(icon == null)
        {
            InteractionButton.gameObject.SetActive(false);
        }
        InteractionIcon.sprite = icon;
    }

    public void ShowFeedProgressBar(float time)
    {
        InteractionButton.gameObject.SetActive(false);
        catSelectButton.interactable = false;

        ProgressBarPanel.SetActive(true);
        ProgressBar.fillAmount = 1.0f;
        ProgressBar.DOFillAmount(0.0f, time).OnComplete(()=> {
            catSelectButton.interactable = true;
            ProgressBarPanel.SetActive(false);
        });
    }

    public void OnSelectCat()
    {
        catManager.OnSelectCat(curCatSkeleton);
    }

    public void OnSelectAction()
    {
        if (curAction != null)
        {
            user_cats userCat = curAction.GetTargetCat().GetUserCatInfo();
            if (userCat.Getfullness() <= 0)
            {
                catManager.FarmCanvas.CatInfoUI.OnUIOpen(curCatSkeleton, CatInfoUI.CAT_SUB_UI.CAT_FEEDING);
                return;
            }
        }

        catManager.OnSelectAction(curCatSkeleton);
        InteractionButton.gameObject.SetActive(false);
        disableInteractionTime = 10.0f;
    }

    public cat_action_def GetActionData()
    {
        return curAction;
    }
}
