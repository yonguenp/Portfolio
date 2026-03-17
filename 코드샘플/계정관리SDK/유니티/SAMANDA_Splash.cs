using SandboxPlatform.SAMANDA;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SAMANDA_Splash : MonoBehaviour
{
    [SerializeField]
    Image Background;
    [SerializeField]
    Image SplashImage;
    [SerializeField]
    AudioSource SplashAudio;

    const int SPLASH_SPRITE_COUNT = 42;

    public void OnSplash()
    {
        gameObject.SetActive(true);

        StopAllCoroutines();
        StartCoroutine(RunSplash());
    }

    IEnumerator RunSplash()
    {
        Sprite[] sprites = new Sprite[SPLASH_SPRITE_COUNT];

        for (int i = 0; i < SPLASH_SPRITE_COUNT; i++)
        {
            sprites[i] = (Resources.Load<Sprite>("Sprite/Splash/" + i.ToString("00")));
        }

        bool isSFXOn = true;
        if (isSFXOn)
        {
            SplashAudio.Play();
        }
        yield return new WaitForSecondsRealtime(0.1f);

        int seq = 0;
        while (seq < SPLASH_SPRITE_COUNT)
        {
            SplashImage.sprite = sprites[seq++];
            yield return new WaitForSecondsRealtime(0.0333333333333333f);
        }

        yield return new WaitForSecondsRealtime(0.5f);

        Color logocolor = SplashImage.color;

        float time = 1.5f;
        while (time > 0)
        {
            float delta = Time.deltaTime;
            time -= delta;

            logocolor.a -= delta;
            SplashImage.color = logocolor;

            yield return new WaitForEndOfFrame();
        }

        for (int i = 0; i < SPLASH_SPRITE_COUNT; i++)
        {
            sprites[i] = null;
        }

        gameObject.SetActive(false);

        SAMANDA.Instance.OnSplashDone();
    }
}
