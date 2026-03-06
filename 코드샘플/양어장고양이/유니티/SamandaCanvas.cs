using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SamandaCanvas : CanvasControl
{
    // Start is called before the first frame update
    void Start()
    {        
        
    }

    // Update is called once per frame
    void Update()
    {        
        switch (SamandaLauncher.GetState())
        {
            case SamandaLauncher.Samanda_State.HIDE:
            case SamandaLauncher.Samanda_State.CHAT_CUSTOM:
            case SamandaLauncher.Samanda_State.CHAT_LEFT:
            case SamandaLauncher.Samanda_State.CHAT_RIGHT:                
                //GameManager.SetState(GameMain.HahahaState.HAHAHA_GAME);
                break;
            default:
                break;
        }
    }

    public override void SetCanvasState(GameMain.HahahaState state)
    {
        bool visible = false;
        switch (state)
        {
            case GameMain.HahahaState.HAHAHA_SAMANDA:
                SamandaLauncher.OnSamandaShortcut(SamandaLauncher.Samanda_Shorcut.PAGE_HOME);                
                visible = true;
                break;
            case GameMain.HahahaState.HAHAHA_CHAT:
                SamandaLauncher.OnSamandaShortcut(SamandaLauncher.Samanda_Shorcut.PAGE_CHAT);                
                visible = true;
                break;
            default:
                visible = false;
                break;
        }

        if (visible == gameObject.activeSelf)
            return;

        gameObject.SetActive(true);
        if (visible)
        {
            foreach (DOTweenAnimation dotween in gameObject.GetComponentsInChildren<DOTweenAnimation>())
            {
                dotween.DOPlayForward();
            }
            CancelInvoke("OnCompleteTweenAnimation");
        }
        else
        {
            foreach (DOTweenAnimation dotween in gameObject.GetComponentsInChildren<DOTweenAnimation>())
            {
                dotween.DOPlayBackwards();
            }
            Invoke("OnCompleteTweenAnimation", 0.5f);
        }
    }

    public void OnCompleteTweenAnimation()
    {
        gameObject.SetActive(false);
    }

    void OnEnable()
    {
        
    }

    private void OnDisable()
    {
        SamandaLauncher.OnHideScreen();
    }
}
