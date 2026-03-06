using UnityEngine.UI;
using System;
using DG.Tweening;

public static class DOTweenEx
{
    public static Tweener DOTextInt(this Text text, int initialValue, int finalValue, float duration, Func<int, string> convertor)
    {
        return DOTween.To(
             () => initialValue,
             it => text.text = convertor(it),
             finalValue,
             duration
         );
    }

    public static Tweener DOTextInt(this Text text, int initialValue, int finalValue, float duration)
    {
        return DOTweenEx.DOTextInt(text, initialValue, finalValue, duration, it => it.ToString());
    }

    public static Tweener DOTextFloat(this Text text, float initialValue, float finalValue, float duration, Func<float, string> convertor)
    {
        return DOTween.To(
             () => initialValue,
             it => text.text = convertor(it),
             finalValue,
             duration
         );
    }

    public static Tweener DOTextFloat(this Text text, float initialValue, float finalValue, float duration)
    {
        return DOTweenEx.DOTextFloat(text, initialValue, finalValue, duration, it => it.ToString());
    }

    public static Tweener DOTextLong(this Text text, long initialValue, long finalValue, float duration, Func<long, string> convertor)
    {
        return DOTween.To(
             () => initialValue,
             it => text.text = convertor(it),
             finalValue,
             duration
         );
    }

    public static Tweener DOTextLong(this Text text, long initialValue, long finalValue, float duration)
    {
        return DOTweenEx.DOTextLong(text, initialValue, finalValue, duration, it => it.ToString());
    }

    public static Tweener DOTextDouble(this Text text, double initialValue, double finalValue, float duration, Func<double, string> convertor)
    {
        return DOTween.To(
             () => initialValue,
             it => text.text = convertor(it),
             finalValue,
             duration
         );
    }

    public static Tweener DOTextDouble(this Text text, double initialValue, double finalValue, float duration)
    {
        return DOTweenEx.DOTextDouble(text, initialValue, finalValue, duration, it => it.ToString());
    }
}