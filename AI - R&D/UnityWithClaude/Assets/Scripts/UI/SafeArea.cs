using UnityEngine;

/// <summary>
/// RectTransform을 Screen.safeArea에 맞춰 앵커링한다.
/// 노치/다이나믹 아일랜드/홈 인디케이터 영역을 피해야 하는 컨테이너에 붙인다.
///
/// 대상 RectTransform은 이 컴포넌트가 앵커를 전부 덮어쓰므로
/// 자식 배치만 신경 쓰면 된다. 배경처럼 화면 전체를 덮어야 하는 요소에는 붙이지 말 것.
/// </summary>
[RequireComponent(typeof(RectTransform))]
[DisallowMultipleComponent]
public class SafeArea : MonoBehaviour
{
    [SerializeField] bool applyLeft   = true;
    [SerializeField] bool applyRight  = true;
    [SerializeField] bool applyTop    = true;
    [SerializeField] bool applyBottom = true;

    RectTransform rt;
    Rect          lastSafe;
    Vector2Int    lastRes;

    void Awake()  { rt = GetComponent<RectTransform>(); Apply(); }
    void OnEnable() => Apply();

    void Update()
    {
        // 회전·분할화면·해상도 변경 시 갱신
        if (Screen.safeArea != lastSafe ||
            Screen.width    != lastRes.x ||
            Screen.height   != lastRes.y)
            Apply();
    }

    void Apply()
    {
        if (!rt) rt = GetComponent<RectTransform>();
        if (Screen.width <= 0 || Screen.height <= 0) return;

        Rect safe = Screen.safeArea;
        lastSafe  = safe;
        lastRes   = new Vector2Int(Screen.width, Screen.height);

        Vector2 min = new Vector2(safe.xMin / Screen.width, safe.yMin / Screen.height);
        Vector2 max = new Vector2(safe.xMax / Screen.width, safe.yMax / Screen.height);

        if (!applyLeft)   min.x = 0f;
        if (!applyBottom) min.y = 0f;
        if (!applyRight)  max.x = 1f;
        if (!applyTop)    max.y = 1f;

        // 일부 기기·에디터에서 초기 프레임에 safeArea가 0으로 오는 경우 방어
        if (min.x >= max.x || min.y >= max.y) return;

        rt.anchorMin = min;
        rt.anchorMax = max;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }
}
