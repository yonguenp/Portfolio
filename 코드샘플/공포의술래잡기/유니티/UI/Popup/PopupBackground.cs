using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopupBackground : Image
{
    //Color ShowColor = new Color(0.0f, 0.0f, 0.0f, 0.6f);
    Coroutine animationCoroutine = null;
    public void SetActive(bool enable)
    {
        gameObject.SetActive(true);

        if (animationCoroutine != null)
            StopCoroutine(animationCoroutine);

        animationCoroutine = StartCoroutine(AnimationCoroutine(enable));
    }

    IEnumerator AnimationCoroutine(bool enable)
    {
        float goalAlpha = enable ? 0.6f : 0.0f;
        
        
        if (enable)
        {
            while (goalAlpha > color.a)
            {
                color = new Color(color.r, color.g, color.b, color.a + Time.deltaTime * 10.0f);
                yield return new WaitForEndOfFrame();
            }
        }
        else
        {
            while (goalAlpha > color.a)
            {
                color = new Color(color.r, color.g, color.b, color.a - Time.deltaTime * 10.0f);
                yield return new WaitForEndOfFrame();
            }
        }

        color = new Color(color.r, color.g, color.b, goalAlpha);
        yield return new WaitForEndOfFrame();

        gameObject.SetActive(enable);
    }
}
