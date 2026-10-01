using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ShapeKit
{
    /// <summary>
    /// Mobile-style press feedback: the button sinks (scales down, shadow tightens) while held.
    /// Works with any Graphic; shadow animation is applied when a ShapeImage is on the same object.
    /// </summary>
    [AddComponentMenu("UI/ShapeKit/Shape Press Feedback")]
    [DisallowMultipleComponent]
    public class ShapePressFeedback : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        [SerializeField, Range(0.5f, 1f)] float m_PressedScale = 0.95f;
        [Tooltip("Shadow offset and blur are multiplied by this while pressed.")]
        [SerializeField, Range(0f, 1f)] float m_PressedShadowFactor = 0.4f;
        [SerializeField, Min(0.01f)] float m_Duration = 0.08f;
        [Tooltip("Keeps animating while Time.timeScale is 0, e.g. in pause menus.")]
        [SerializeField] bool m_UnscaledTime = true;

        ShapeImage m_Shape;
        Selectable m_Selectable;
        Vector3 m_BaseScale;
        Vector2 m_BaseShadowOffset;
        float m_BaseShadowBlur;
        float m_Current; // 0 = released, 1 = fully pressed
        float m_Target;
        bool m_Captured;

        void Awake()
        {
            m_Shape = GetComponent<ShapeImage>();
            m_Selectable = GetComponent<Selectable>();
        }

        void OnEnable()
        {
            Capture();
        }

        void OnDisable()
        {
            m_Target = 0f;
            m_Current = 0f;
            ApplyState();
            m_Captured = false;
        }

        void Capture()
        {
            m_BaseScale = transform.localScale;
            if (m_Shape != null)
            {
                m_BaseShadowOffset = m_Shape.shadowOffset;
                m_BaseShadowBlur = m_Shape.shadowBlur;
            }
            m_Captured = true;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) return;
            if (m_Selectable != null && !m_Selectable.IsInteractable()) return;
            if (m_Current <= 0f) Capture();
            m_Target = 1f;
        }

        public void OnPointerUp(PointerEventData eventData) => m_Target = 0f;

        public void OnPointerExit(PointerEventData eventData) => m_Target = 0f;

        void Update()
        {
            if (!m_Captured || Mathf.Approximately(m_Current, m_Target)) return;
            float dt = m_UnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            m_Current = Mathf.MoveTowards(m_Current, m_Target, dt / m_Duration);
            ApplyState();
        }

        void ApplyState()
        {
            if (!m_Captured) return;
            float eased = m_Current * m_Current * (3f - 2f * m_Current);
            transform.localScale = m_BaseScale * Mathf.Lerp(1f, m_PressedScale, eased);
            if (m_Shape != null)
            {
                float factor = Mathf.Lerp(1f, m_PressedShadowFactor, eased);
                m_Shape.shadowOffset = m_BaseShadowOffset * factor;
                m_Shape.shadowBlur = m_BaseShadowBlur * factor;
            }
        }
    }
}
