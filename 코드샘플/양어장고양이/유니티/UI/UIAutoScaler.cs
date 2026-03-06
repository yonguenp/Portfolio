using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIAutoScaler : UIBehaviour
{
    public void OnEnable()
    {
        ResetBackgroundSize();
    }

    protected override void OnRectTransformDimensionsChange()
    {
        base.OnRectTransformDimensionsChange();

        if (gameObject.activeInHierarchy)
            ResetBackgroundSize();
    }

    public void ResetBackgroundSize()
    {
        Canvas parentCanvas = GetComponentInParent<Canvas>();
        if (parentCanvas == null)
            return;

        Vector2 size = (parentCanvas.transform as RectTransform).sizeDelta;
        float curRatio = size.x / size.y;
        if(curRatio < 0.45f)
        {
            float diff = 0.45f - curRatio;
            diff = 1.0f - (diff * 2.5f);
            transform.localScale = Vector3.one * diff;
        }
        else
        {
            transform.localScale = Vector3.one;
        }
    }
}
