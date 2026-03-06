using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIBackgroundBlurController : Coffee.UIEffects.UIEffect
{
    [SerializeField]
    RawImage blurBackground;
    [SerializeField]
    float Effect;
    [SerializeField]
    float Color;
    [SerializeField]
    float Blur;
    [SerializeField]
    float AnimationTime = 1.0f;

    Coroutine animationCoroutine = null;
    RenderTexture blurTexture = null;


    protected override void OnEnable()
    {
        base.OnEnable();

        if (animationCoroutine != null)
            StopCoroutine(animationCoroutine);

        animationCoroutine = StartCoroutine(BlurAnimation());
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        if (animationCoroutine != null)
            StopCoroutine(animationCoroutine);

        animationCoroutine = null;
    }

    IEnumerator BlurAnimation()
    {
        blurTexture?.Release();
        blurTexture = new RenderTexture((int)(Screen.width * 0.25f), (int)(Screen.height * 0.25f), 0);        
        Camera.main.targetTexture = blurTexture;
        blurBackground.texture = blurTexture;


        float effect = 0.0f;
        float color = 0.0f;
        float blur = 0.0f;

        effectFactor = effect;
        colorFactor = color;
        blurFactor = blur;
        SetEffectParamsDirty();
        yield return new WaitForEndOfFrame();

        float curTime = 0.0f;
        while (AnimationTime > curTime)
        {
            curTime += Time.deltaTime;
            float rate = (curTime / AnimationTime);

            effectFactor = ((Effect - effect) * rate);
            colorFactor = ((Color - color) * rate);
            blurFactor = ((Blur - blur) * rate);
            SetEffectParamsDirty();
            yield return new WaitForEndOfFrame();
        }

        effectFactor = Effect;
        colorFactor = Color;
        blurFactor = Blur;
        SetEffectParamsDirty();
    }

    public void Clear()
    {
        if (animationCoroutine != null)
            StopCoroutine(animationCoroutine);

        effectFactor = 0.0f;
        colorFactor = 0.0f;
        blurFactor = 0.0f;
        SetEffectParamsDirty();


        blurBackground.texture = null;
        blurTexture?.Release();
        blurTexture = null;
        if(Camera.main != null)
            Camera.main.targetTexture = null;

        gameObject.SetActive(false);
    }
}
