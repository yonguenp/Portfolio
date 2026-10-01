using System;
using UnityEngine;
using UnityEngine.EventSystems;
using Object = UnityEngine.Object;

namespace ShapeKit
{
    /// <summary>
    /// Keeps the scene's EventSystem compatible with Player Settings > Active Input Handling.
    /// The Input System module is looked up by name so this works whether or not the package is installed,
    /// without compile-time defines or assembly references.
    /// </summary>
    public static class EventSystemUtility
    {
        static readonly Type s_InputSystemModule =
            Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");

        /// <summary>Returns the active EventSystem, creating one or fixing its input module as needed.</summary>
        public static EventSystem EnsureEventSystem()
        {
            var eventSystem = EventSystem.current;
            if (eventSystem == null)
            {
#if UNITY_2023_1_OR_NEWER
                eventSystem = Object.FindFirstObjectByType<EventSystem>();
#else
                eventSystem = Object.FindObjectOfType<EventSystem>();
#endif
            }
            if (eventSystem == null)
            {
                eventSystem = new GameObject("EventSystem", typeof(EventSystem)).GetComponent<EventSystem>();
            }
            EnsureCompatibleInputModule(eventSystem.gameObject);
            return eventSystem;
        }

        /// <summary>
        /// Replaces StandaloneInputModule when the legacy Input Manager is disabled (it throws every frame),
        /// and adds a module matching the active input handling if none is present.
        /// </summary>
        public static void EnsureCompatibleInputModule(GameObject eventSystemObject)
        {
#if !ENABLE_LEGACY_INPUT_MANAGER
            var legacy = eventSystemObject.GetComponent<StandaloneInputModule>();
            if (legacy != null)
            {
                Object.DestroyImmediate(legacy);
            }
#endif
            if (eventSystemObject.GetComponent<BaseInputModule>() != null)
            {
                return;
            }

#if ENABLE_INPUT_SYSTEM
            if (s_InputSystemModule != null)
            {
                eventSystemObject.AddComponent(s_InputSystemModule);
                return;
            }
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
            eventSystemObject.AddComponent<StandaloneInputModule>();
#else
            Debug.LogWarning("[ShapeKit] No UI input module could be added. Install the Input System package or enable the legacy Input Manager.");
#endif
        }
    }
}
