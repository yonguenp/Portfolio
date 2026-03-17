using SandboxNetwork;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResizeAnchorSizeAndPos : MonoBehaviour
{

    [ContextMenu("SetDoublePosAndSize")]

    public void SetDouble()
    {
        
        var rects= gameObject.GetComponentsInChildren<RectTransform>(true);
        var imgs = gameObject.GetComponentsInChildren<Image>(true);

        var texts = gameObject.GetComponentsInChildren<Text>(true);

        var tableViews = gameObject.GetComponentsInChildren<TableView>(true);

        var horizonLayouts = gameObject.GetComponentsInChildren<HorizontalLayoutGroup>(true);

        var verticalLayouts = gameObject.GetComponentsInChildren<VerticalLayoutGroup>(true);

        var gridLayouts = gameObject.GetComponentsInChildren<GridLayoutGroup>(true);

        foreach ( var img in imgs)
        {
            if(img.type == Image.Type.Sliced)
            {
                if(img.pixelsPerUnitMultiplier != 1)
                {
                    img.pixelsPerUnitMultiplier = 1f;
                }
            }
        }

        foreach (var rect in rects)
        {

            if(rect.sizeDelta == Vector2.zero)
            {
                rect.offsetMax *= 2f;
                rect.offsetMin *= 2f;
            }
            else
            {
                rect.sizeDelta *= 2f;
                rect.anchoredPosition *= 2f;
            }
            
        }

        foreach(var horizontalLayout in horizonLayouts)
        {
            horizontalLayout.padding.left *= 2;
            horizontalLayout.padding.right *= 2;
            horizontalLayout.padding.top *= 2;
            horizontalLayout.padding.bottom *= 2;
            horizontalLayout.spacing *= 2f;
        }
        foreach(var verticalLayout in verticalLayouts)
        {
            verticalLayout.padding.left *= 2;
            verticalLayout.padding.right *= 2;
            verticalLayout.padding.top *= 2;
            verticalLayout.padding.bottom *= 2;
            verticalLayout.spacing *= 2f;
        }
        foreach (var gridLayout in gridLayouts)
        {
            gridLayout.padding.left *= 2;
            gridLayout.padding.right *= 2;
            gridLayout.padding.top *= 2;
            gridLayout.padding.bottom *= 2;
            gridLayout.spacing *= 2f;
            gridLayout.cellSize *= 2f;
        }

        foreach (var tableview in tableViews)
        {
            tableview.SetPaddingMulti(2f);
            tableview.SetSpaceingMulti(2f);
        }

        foreach (var text in texts)
        {
            text.resizeTextMaxSize *= 2;
            text.resizeTextMinSize *= 2;
            
            text.fontSize *= 2;
            
        }
            
    }



    [ContextMenu("SetRoundValue")]
    public void SetRound()
    {
        var rects = gameObject.GetComponentsInChildren<RectTransform>(true);

        foreach (var rect in rects)
        {

            if (rect.sizeDelta == Vector2.zero)
            {
                rect.offsetMax =TruncatedFloat2Vector2(rect.offsetMax);
                rect.offsetMin = TruncatedFloat2Vector2(rect.offsetMin);
            }
            else
            {
                rect.sizeDelta = TruncatedFloat2Vector2(rect.sizeDelta);
                rect.anchoredPosition = TruncatedFloat2Vector2(rect.anchoredPosition);
            }

        }

    }

    Vector2 TruncatedFloat2Vector2(Vector2 floatVector)
    {
        float x = Mathf.Round(floatVector.x * 100) / 100f;
        float y = Mathf.Round(floatVector.y * 100) / 100f;
        return new Vector2 (x, y);
    }


    [ContextMenu("SetHalfPosAndSize")]

    public void SetHalf()
    {

        var rects = gameObject.GetComponentsInChildren<RectTransform>(true);
        var imgs = gameObject.GetComponentsInChildren<Image>(true);

        var texts = gameObject.GetComponentsInChildren<Text>(true);

        var tableViews = gameObject.GetComponentsInChildren<TableView>(true);

        var horizonLayouts = gameObject.GetComponentsInChildren<HorizontalLayoutGroup>(true);

        var verticalLayouts = gameObject.GetComponentsInChildren<VerticalLayoutGroup>(true);

        var gridLayouts = gameObject.GetComponentsInChildren<GridLayoutGroup>(true);

        foreach (var img in imgs)
        {
            if (img.type == Image.Type.Sliced)
            {
                if (img.pixelsPerUnitMultiplier != 1)
                {
                    img.pixelsPerUnitMultiplier = 1f;
                }
            }
        }

        foreach (var rect in rects)
        {

            if (rect.sizeDelta == Vector2.zero)
            {
                rect.offsetMax /= 2f;
                rect.offsetMin /= 2f;
            }
            else
            {
                rect.sizeDelta /= 2f;
                rect.anchoredPosition /= 2f;
            }

        }

        foreach (var horizontalLayout in horizonLayouts)
        {
            horizontalLayout.padding.left /= 2;
            horizontalLayout.padding.right /= 2;
            horizontalLayout.padding.top /= 2;
            horizontalLayout.padding.bottom /= 2;
            horizontalLayout.spacing /= 2f;
        }
        foreach (var verticalLayout in verticalLayouts)
        {
            verticalLayout.padding.left /= 2;
            verticalLayout.padding.right /= 2;
            verticalLayout.padding.top /= 2;
            verticalLayout.padding.bottom /= 2;
            verticalLayout.spacing /= 2f;
        }
        foreach (var gridLayout in gridLayouts)
        {
            gridLayout.padding.left /= 2;
            gridLayout.padding.right /= 2;
            gridLayout.padding.top /= 2;
            gridLayout.padding.bottom /= 2;
            gridLayout.spacing /= 2f;
            gridLayout.cellSize /= 2f;
        }

        foreach (var tableview in tableViews)
        {
            tableview.SetPaddingMulti(0.5f);
            tableview.SetSpaceingMulti(0.5f);
        }

        foreach (var text in texts)
        {
            text.resizeTextMaxSize/= 2;
            text.resizeTextMinSize /= 2;

            text.fontSize /= 2;

        }

    }

}
