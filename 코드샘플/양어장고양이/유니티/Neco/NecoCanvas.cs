using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NecoCanvas : MonoBehaviour
{
    public enum NECO_CANVAS { 
        NONE = -1,
        GAME_CANVAS = 0,
        UI_CANVAS = 1,
        POPUP_CANVAS = 2,
        VIDEO_CANVAS = 3,
        NECO_CANVAS_MAX,
    };

    private static NecoCanvas[] Instance = new NecoCanvas[(int)NECO_CANVAS.NECO_CANVAS_MAX];
    public NECO_CANVAS CurCanvasType = NECO_CANVAS.NONE;

    protected virtual void OnDestroy()
    {
        Instance[(int)CurCanvasType] = null;
    }
    protected virtual void Awake()
    {
        Instance[(int)CurCanvasType] = this;
    }
    public static NecoGameCanvas GetGameCanvas()
    {
        return (NecoGameCanvas)Instance[(int)NECO_CANVAS.GAME_CANVAS];
    }

    public static NecoPopupCanvas GetPopupCanvas()
    {
        return (NecoPopupCanvas)Instance[(int)NECO_CANVAS.POPUP_CANVAS];
    }

    public static NecoUICanvas GetUICanvas()
    {
        return (NecoUICanvas)Instance[(int)NECO_CANVAS.UI_CANVAS];
    }

    public static NecoVideoCanvas GetVideoCanvas()
    {
        return (NecoVideoCanvas)Instance[(int)NECO_CANVAS.VIDEO_CANVAS];
    }

    public static uint GetCurTime()
    {
        if (GetGameCanvas() != null)
        {
            return (uint)GetGameCanvas().GetCurTime();
        }

        return (uint)NetworkManager.GetInstance().ServerTime;
    }
}
