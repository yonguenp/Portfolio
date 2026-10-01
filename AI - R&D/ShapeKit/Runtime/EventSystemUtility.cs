using UnityEngine;
using UnityEngine.EventSystems;

namespace ShapeKit
{
    /// <summary>Creates an EventSystem whose input module matches the project's Active Input Handling setting.</summary>
    public static class EventSystemUtility
    {
        public static GameObject CreateEventSystem()
        {
            var go = new GameObject("EventSystem", typeof(EventSystem));
#if SHAPEKIT_INPUT_SYSTEM && ENABLE_INPUT_SYSTEM
            // StandaloneInputModule throws when only the Input System package is active.
            go.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
#else
            go.AddComponent<StandaloneInputModule>();
#endif
            return go;
        }
    }
}
