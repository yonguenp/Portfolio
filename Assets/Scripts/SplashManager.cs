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
        // 2026-08-28: 프로젝트 기본 화면 방향을 AutoRotation으로 바꿔서
        // GoStop3PGame이 실제로 가로 고정을 걸 수 있게 했다(위 orientation
        // 함정 참고 — 단일 orientation이 기본값이면 iOS Info.plist 자체가
        // 그 방향만 지원해서 런타임 Screen.orientation 강제가 무의미해진다).
        // 그 대가로 나머지 7개 게임은 각자 세로를 명시적으로 고정해야 한다 —
        // 앱이 켜지는 가장 이른 지점인 여기서부터 잠가서 첫 프레임에
        // 가로로 잠깐 보이는 걸 최소화한다.
        Screen.orientation = ScreenOrientation.Portrait;
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
