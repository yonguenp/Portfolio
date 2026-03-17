using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FarmUIPanel : MonoBehaviour
{
    public enum UI_STATE 
    { 
        UI_NORMAL,
        UI_HIDE,
        UI_POPUP_OPENING,
    };

    public enum UI_GROUP
    {
        UI_TOP,
        UI_MID,
        UI_BOT,
    };

    public FarmCanvas FarmCanvas;
    public UIGroup[] UIGroup;

    struct DragControlledObject {
        public MaskableGraphic controlledObject;
        public Color color;
    };

    private List<DragControlledObject> DragControlledObjects = null;

    public void SetUI(UI_STATE state)
    {
        switch (state)
        {
            case UI_STATE.UI_NORMAL:
                foreach(UIGroup ui in UIGroup)
                {
                    ui.SetUI(true);
                }
                break;
            case UI_STATE.UI_HIDE:
                foreach (UIGroup ui in UIGroup)
                {
                    ui.SetUI(false);
                }
                break;
            case UI_STATE.UI_POPUP_OPENING:
                UIGroup[(int)UI_GROUP.UI_TOP].SetUI(true);
                UIGroup[(int)UI_GROUP.UI_TOP].UI[(uint)TopUIGroup.TOP_UI.CONFIG].SetActive(false);
                UIGroup[(int)UI_GROUP.UI_MID].SetUI(false);
                UIGroup[(int)UI_GROUP.UI_BOT].SetUI(false);
                break;
            default:
                Debug.LogError("missing state!");
                break;
        }
    }

    public void Show()
    {
        SetUI(UI_STATE.UI_NORMAL);
    }

    public void Hide()
    {
        SetUI(UI_STATE.UI_HIDE);
    }

    public void Refresh()
    {
        foreach (UIGroup ui in UIGroup)
        {
            ui.Refresh();
        }
    }

    public void OnDragStartMap()
    {
        OnDragEndMap();

        DragControlledObjects = new List<DragControlledObject>();
        MaskableGraphic[] child = GetComponentsInChildren<MaskableGraphic>();
        foreach(MaskableGraphic c in child)
        {
            DragControlledObject obj = new DragControlledObject();
            obj.controlledObject = c;            
            obj.color = c.color;
            Color color = c.color;
            color.a *= 0.3f;
            c.color = color;
            DragControlledObjects.Add(obj);
        }
    }

    public void OnDragEndMap()
    {
        if (DragControlledObjects != null)
        {
            foreach (DragControlledObject controlledObject in DragControlledObjects)
            {
                controlledObject.controlledObject.color = controlledObject.color;
            }

            DragControlledObjects.Clear();
        }
        DragControlledObjects = null;
    }
}
