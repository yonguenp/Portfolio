using System.Collections.Generic;
using UnityEngine;

public class SBFovHide : SBFovUtil
{
    public enum SBFovHideEnum
    {
        None,
        Enter,
        Exit
    }
    
    [SerializeField]
    protected float hideRadius = 0.5f;
    [SerializeField]
    protected float hideTime = 0.5f;

    [SerializeField]
    protected List<GameObject> hideObject = null;

    private SBFovHideEnum utilState = SBFovHideEnum.None;

    public override void OnEnter()
    {
        utilState = SBFovHideEnum.Enter;

        for (int i = 0; i < hideObject.Count; i++)
        {
            hideObject[i]?.SetActive(false);
        }
    }

    public override void OnExit()
    {
        utilState = SBFovHideEnum.Exit;
    }

    public override void UtilUpdate(SBFieldOfView view, float time)
    {
        switch (utilState)
        {
            case SBFovHideEnum.Enter:
                if (view.CurRadius <= hideRadius)
                {
                    view.CurRadius = hideRadius;
                    utilState = SBFovHideEnum.None;
                    return;
                }
                else
                {
                    var diffRadius = view.ViewRadius - hideRadius;
                    var timeDelta = time / hideTime;
                    var addRadius = -diffRadius * timeDelta;
                    view.CurRadius += addRadius;
                }
                break;
            case SBFovHideEnum.Exit:
                if (view.CurRadius >= view.ViewRadius)
                {
                    view.CurRadius = view.ViewRadius;
                    utilState = SBFovHideEnum.None;

                    for (int i = 0; i < hideObject.Count; i++)
                    {
                        hideObject[i]?.SetActive(true);
                    }
                    return;
                }
                else
                {
                    var diffRadius = view.ViewRadius - hideRadius;
                    var timeDelta = time / hideTime;
                    var addRadius = diffRadius * timeDelta;
                    view.CurRadius += addRadius;
                }
                break;
        }
    }
}
