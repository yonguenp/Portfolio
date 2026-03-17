using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ColorTween : Image
{
    Tweener tween;

    protected override void Start()
    {
        ColorAction();
    }

    public void ColorAction()
    {
        tween = ((Image)this).DOColor(new Color(Random.value, Random.value, Random.value), 1.0f + (Random.value * 1.0f)).OnComplete(ColorAction);
    }

    protected override void OnDestroy()
    {
        if (tween != null)
        {
            tween.Kill();
        }
        base.OnDestroy();
    }
}
