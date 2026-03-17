using DG.Tweening;
using SandboxPlatform.SAMANDA;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SAMANDA_UI_forCMM : SAMANDA_UI
{
    private void OnEnable()
    {
        ResetSize();
    }

    protected override void OnRectTransformDimensionsChange()
    {
        ResetSize();
    }

    void ResetSize()
    {
        (transform as RectTransform).sizeDelta = (SAMANDA.Instance.transform as RectTransform).sizeDelta;
    }
}
