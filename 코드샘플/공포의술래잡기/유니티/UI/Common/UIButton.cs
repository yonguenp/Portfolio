using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIButton : Button
{
    Dictionary<MaskableGraphic, Color> originColors = null;

    protected override void OnEnable()
    {
        Clear();
    }
    protected override void OnDisable()
    {
        Clear();
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.pointerId != 0)
            return;

        if (interactable)
        {
            if (!eventData.dragging && eventData.pointerCurrentRaycast.gameObject != null && CheckTarget(transform, eventData.pointerCurrentRaycast.gameObject.transform))
            {
                if (transition == Transition.ColorTint)
                {
                    originColors = new Dictionary<MaskableGraphic, Color>();
                    foreach (MaskableGraphic graphic in GetComponentsInChildren<MaskableGraphic>())
                    {
                        Color ogirin = graphic.color;
                        originColors.Add(graphic, ogirin);
                        ogirin = ogirin - (colors.normalColor - colors.pressedColor);
                        graphic.color = ogirin;
                    }
                }
            }
        }
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.pointerId != 0)
            return;

        base.OnPointerEnter(eventData);

        Clear();
    }

    public void Clear()
    {
        if(originColors != null)
        {
            foreach(var child in originColors)
            {
                child.Key.color = child.Value;
            }
        }

        originColors = null;
    }

    protected bool CheckTarget(Transform check, Transform target)
    {
        foreach (Transform child in check.transform)
        {
            if (child == target)
            {
                return true;
            }

            if (CheckTarget(child, target))
            {
                return true;
            }
        }

        return false;

    }
}
