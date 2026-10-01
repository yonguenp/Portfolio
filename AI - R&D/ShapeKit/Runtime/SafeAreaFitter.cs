using UnityEngine;

namespace ShapeKit
{
    /// <summary>
    /// Fits this RectTransform to the device safe area (notches, rounded screen corners, home indicator).
    /// Put it on a full-screen child of the canvas and place the UI that must stay visible inside it.
    /// </summary>
    [AddComponentMenu("UI/ShapeKit/Safe Area Fitter")]
    [ExecuteAlways]
    [RequireComponent(typeof(RectTransform))]
    [DisallowMultipleComponent]
    public class SafeAreaFitter : MonoBehaviour
    {
        [SerializeField] bool m_Top = true;
        [SerializeField] bool m_Bottom = true;
        [SerializeField] bool m_Left = true;
        [SerializeField] bool m_Right = true;

        Rect m_AppliedSafeArea;
        Vector2Int m_AppliedScreenSize;
        int m_AppliedEdges = -1;

        public bool top { get => m_Top; set { m_Top = value; Apply(); } }
        public bool bottom { get => m_Bottom; set { m_Bottom = value; Apply(); } }
        public bool left { get => m_Left; set { m_Left = value; Apply(); } }
        public bool right { get => m_Right; set { m_Right = value; Apply(); } }

        void OnEnable()
        {
            Apply();
        }

        void Update()
        {
            if (Screen.safeArea != m_AppliedSafeArea
                || Screen.width != m_AppliedScreenSize.x
                || Screen.height != m_AppliedScreenSize.y
                || EdgeMask() != m_AppliedEdges)
            {
                Apply();
            }
        }

        int EdgeMask()
        {
            return (m_Top ? 1 : 0) | (m_Bottom ? 2 : 0) | (m_Left ? 4 : 0) | (m_Right ? 8 : 0);
        }

        /// <summary>Re-applies the safe area immediately.</summary>
        public void Apply()
        {
            Rect safe = Screen.safeArea;
            m_AppliedSafeArea = safe;
            m_AppliedScreenSize = new Vector2Int(Screen.width, Screen.height);
            m_AppliedEdges = EdgeMask();

            if (Screen.width <= 0 || Screen.height <= 0) return;

            var min = new Vector2(safe.xMin / Screen.width, safe.yMin / Screen.height);
            var max = new Vector2(safe.xMax / Screen.width, safe.yMax / Screen.height);
            if (!m_Left) min.x = 0f;
            if (!m_Bottom) min.y = 0f;
            if (!m_Right) max.x = 1f;
            if (!m_Top) max.y = 1f;

            var rt = (RectTransform)transform;
            rt.anchorMin = min;
            rt.anchorMax = max;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }
    }
}
