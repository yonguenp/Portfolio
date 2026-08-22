using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SplashManager : MonoBehaviour
{
    [SerializeField] CanvasGroup splashGroup;
    [SerializeField] float fadeIn = 1f;
    [SerializeField] float hold = 2f;
    [SerializeField] float fadeOut = 1f;

    // 앱 세션당 한 번만 표시
    static bool shown;

    void Start()
    {
        if (shown)
        {
            SceneManager.LoadScene("TitleScene");
            return;
        }
        shown = true;
        StartCoroutine(RunSplash());
    }

    IEnumerator RunSplash()
    {
        splashGroup.alpha = 0f;
        yield return Fade(0f, 1f, fadeIn);
        yield return new WaitForSeconds(hold);
        yield return Fade(1f, 0f, fadeOut);
        SceneManager.LoadScene("TitleScene");
    }

    IEnumerator Fade(float from, float to, float dur)
    {
        for (float t = 0f; t < dur; t += Time.deltaTime)
        {
            splashGroup.alpha = Mathf.Lerp(from, to, t / dur);
            yield return null;
        }
        splashGroup.alpha = to;
    }
}
