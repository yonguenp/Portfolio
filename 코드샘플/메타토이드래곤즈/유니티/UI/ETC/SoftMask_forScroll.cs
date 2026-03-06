using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Coffee.UISoftMask;
using UnityEngine.UI;
using Coffee.UISoftMaskInternal;

public class SoftMask_forScroll : SoftMask
{
    ScrollRect parentScroll = null;

    protected override void OnEnable()
    {
        base.OnEnable();

        if (parentScroll == null)
        {
            parentScroll = GetComponentInParent<ScrollRect>();

            if (parentScroll != null)
            {
                parentScroll.onValueChanged.AddListener(OnUpdateScroll);
            }
        }
    }

    public void OnUpdateScroll(Vector2 pos)
    {
        if (parentScroll == null)
            return;
                
        if (parentScroll.horizontal)
        {
            if (parentScroll.horizontalNormalizedPosition <= 0.0f)
            {
                if(maskingMode != MaskingMode.Normal)
                {
                    maskingMode = MaskingMode.Normal;
                }
            }
            else
            {
                if (maskingMode == MaskingMode.Normal)
                {
                    maskingMode = MaskingMode.SoftMasking;
                }
            }
        }
        else
        {
            if (parentScroll.verticalNormalizedPosition <= 0.0f)
            {
                if (maskingMode != MaskingMode.Normal)
                {
                    maskingMode = MaskingMode.Normal;
                }
            }
            else
            {
                if (maskingMode == MaskingMode.Normal)
                {
                    maskingMode = MaskingMode.SoftMasking;
                }
            }
        }
    }
}
