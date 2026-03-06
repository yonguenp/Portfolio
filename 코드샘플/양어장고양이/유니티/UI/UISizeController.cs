using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UISizeController : UIBehaviour
{
    public Vector2 MaxSize;
    public Vector2 MinSize;


    public Vector2 CurSize;


    protected override void Awake()
    {
        base.Awake();

        RefreshSize();
    }

    protected override void OnRectTransformDimensionsChange()
    {
        RefreshSize();
    }

    [ContextMenu("RefreshSize")]
    public void RefreshSize()
    {
        base.OnRectTransformDimensionsChange();

        RectTransform curTransform = transform as RectTransform;
        Vector2 AnchoredPos = curTransform.anchoredPosition;
        CurSize = GetRectSize(curTransform);

        float y = Mathf.Min(CurSize.y, MaxSize.y);
        y = Mathf.Max(y, MinSize.y);

        float x = Mathf.Min(CurSize.x, MaxSize.x);
        x = Mathf.Max(x, MinSize.x);

        Vector2 controllSize = new Vector2(x, y);
        if(controllSize == CurSize)
        {
            return;
        }

        CurSize = controllSize;

        bool isStrechedVertical = false;
        bool isStrechedHorizontal = false;
        if (curTransform.anchorMin.y == 0.0f && curTransform.anchorMax.y == 1.0f)
        {
            isStrechedVertical = true;
        }

        if (curTransform.anchorMin.x == 0.0f && curTransform.anchorMax.x == 1.0f)
        {
            isStrechedHorizontal = true;
        }

        if (isStrechedVertical)
        {
            float height = GetHeight(curTransform);
            float diff = CurSize.y - height;
            Vector2 min = curTransform.offsetMin;
            Vector2 max = curTransform.offsetMax;
            min.y -= diff * 0.5f;
            max.y += diff * 0.5f;
            curTransform.offsetMin = min;
            curTransform.offsetMax = max;
        }
        else
        {
            Vector2 size = curTransform.sizeDelta;
            size.y = CurSize.y;
            curTransform.sizeDelta = size;
        }

        if (isStrechedHorizontal)
        {
            float width = GetWidth(curTransform);
            float diff = CurSize.x - width;
            Vector2 min = curTransform.offsetMin;
            Vector2 max = curTransform.offsetMax;
            min.x -= diff * 0.5f;
            max.x += diff * 0.5f;
            curTransform.offsetMin = min;
            curTransform.offsetMax = max;
        }
        else
        {
            Vector2 size = curTransform.sizeDelta;
            size.x = CurSize.x;
            curTransform.sizeDelta = size;
        }

        curTransform.anchoredPosition = AnchoredPos;
    }

    public Vector2 GetRectSize(RectTransform rt)
    {
        return new Vector2(GetWidth(rt), GetHeight(rt));
    }

    public float GetWidth(RectTransform rt)
    {
        float ret = 0.0f;
        bool isStrechedHorizontal = false;
        if (rt.anchorMin.x == 0.0f && rt.anchorMax.x == 1.0f)
        {
            isStrechedHorizontal = true;
        }

        if (isStrechedHorizontal)
        {
            float parentValue = GetWidth(rt.parent as RectTransform);
            ret = parentValue - rt.offsetMin.x + rt.offsetMax.x;
        }
        else
        {
            ret = rt.sizeDelta.x;
        }

        return ret;
    }

    public float GetHeight(RectTransform rt)
    {
        float ret = 0.0f;
        bool isStrechedVertical = false;
        if (rt.anchorMin.y == 0.0f && rt.anchorMax.y == 1.0f)
        {
            isStrechedVertical = true;
        }

        if (isStrechedVertical)
        {
            float parentValue = GetHeight(rt.parent as RectTransform);
            ret = parentValue - rt.offsetMin.y + rt.offsetMax.y;
        }
        else
        {
            ret = rt.sizeDelta.y;
        }

        return ret;
    }
}
