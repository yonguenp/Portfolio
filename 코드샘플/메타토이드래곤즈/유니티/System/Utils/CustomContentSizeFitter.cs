using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class CustomContentSizeFitter : ContentSizeFitter
{

    // Define the min and max width and height
    [SerializeField]
    float minWidth = 0f;
    [SerializeField]
    float maxWidth = 500f;
    [SerializeField]
    float minHeight = 0f;
    [SerializeField]
    float maxHeight = 500f;


    public override void SetLayoutHorizontal()
    { // Override for width
        base.SetLayoutHorizontal();
        // get the rectTransform
        var rectTransform = transform as RectTransform;
        // set the anchors to avoid problems.
        //rectTransform.anchorMin = rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        var sizeDelta = rectTransform.sizeDelta; // get the size delta
        // Clamp the x value based on the min and max width
        sizeDelta.x = Mathf.Clamp(sizeDelta.x, minWidth, maxWidth);
        rectTransform.sizeDelta = sizeDelta; // set the new sizeDelta
    }


    public override void SetLayoutVertical()
    { // Override for height
        base.SetLayoutVertical();
        // get the rectTransform
        var rectTransform = transform as RectTransform;
        // set the anchors to avoid problems.
        //  rectTransform.anchorMin = rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        var sizeDelta = rectTransform.sizeDelta; // get the size delta
        // Clamp the y value based on the min and max height
        sizeDelta.y = Mathf.Clamp(sizeDelta.y, minHeight, maxHeight);
        rectTransform.sizeDelta = sizeDelta; // set the new sizeDelta
    }
}


