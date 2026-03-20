using UnityEngine;
using System.Collections;

/// <summary>
/// 메인 카메라에 붙이는 화면 진동 효과.
/// </summary>
public class ScreenShake : MonoBehaviour
{
    public static ScreenShake Instance;
    private Vector3 _origin;

    void Awake()
    {
        Instance = this;
        _origin  = transform.localPosition;
    }

    public void Shake(float duration = 0.35f, float magnitude = 0.18f)
        => StartCoroutine(DoShake(duration, magnitude));

    IEnumerator DoShake(float dur, float mag)
    {
        float t = 0f;
        while (t < dur)
        {
            float ratio = 1f - t / dur; // 점점 약해짐
            transform.localPosition = _origin + (Vector3)Random.insideUnitCircle * mag * ratio;
            t += Time.unscaledDeltaTime;
            yield return null;
        }
        transform.localPosition = _origin;
    }
}
