using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ShapeKit.Editor
{
    static class ShapeKitMenu
    {
        [MenuItem("GameObject/UI/ShapeKit/Shape Image", false, 2100)]
        static void CreateShape(MenuCommand command)
        {
            var go = CreateUIObject("Shape", command, new Vector2(200f, 120f));
            var shape = go.AddComponent<ShapeImage>();
            ShapePresets.All[0].Apply(shape);
        }

        [MenuItem("GameObject/UI/ShapeKit/Pill Button", false, 2101)]
        static void CreatePillButton(MenuCommand command)
        {
            var go = CreateUIObject("Pill Button", command, new Vector2(240f, 72f));
            var shape = go.AddComponent<ShapeImage>();
            ShapePresets.All[1].Apply(shape);
            var button = go.AddComponent<Button>();
            button.targetGraphic = shape;
            go.AddComponent<ShapePressFeedback>();

            var labelGo = new GameObject("Label", typeof(RectTransform));
            Undo.RegisterCreatedObjectUndo(labelGo, "Create Label");
            labelGo.transform.SetParent(go.transform, false);
            var labelRect = (RectTransform)labelGo.transform;
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;
            var text = labelGo.AddComponent<Text>();
#if UNITY_2022_2_OR_NEWER
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
#else
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
#endif
            text.text = "Button";
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.fontSize = 28;
            text.raycastTarget = false;
        }

        [MenuItem("GameObject/UI/ShapeKit/Safe Area", false, 2110)]
        static void CreateSafeArea(MenuCommand command)
        {
            var go = CreateUIObject("Safe Area", command, Vector2.zero);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            go.AddComponent<SafeAreaFitter>();
        }

        static GameObject CreateUIObject(string name, MenuCommand command, Vector2 size)
        {
            var parent = command.context as GameObject;
            if (parent == null || parent.GetComponentInParent<Canvas>() == null)
            {
                parent = FindOrCreateCanvas().gameObject;
            }

            var go = new GameObject(name, typeof(RectTransform));
            Undo.RegisterCreatedObjectUndo(go, "Create " + name);
            GameObjectUtility.SetParentAndAlign(go, parent);
            ((RectTransform)go.transform).sizeDelta = size;
            Selection.activeGameObject = go;
            return go;
        }

        static Canvas FindOrCreateCanvas()
        {
            var selected = Selection.activeGameObject;
            var canvas = selected != null ? selected.GetComponentInParent<Canvas>() : null;
#if UNITY_2023_1_OR_NEWER
            if (canvas == null) canvas = Object.FindFirstObjectByType<Canvas>();
#else
            if (canvas == null) canvas = Object.FindObjectOfType<Canvas>();
#endif
            if (canvas != null) return canvas;

            var canvasGo = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Undo.RegisterCreatedObjectUndo(canvasGo, "Create Canvas");
            canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0.5f;

#if UNITY_2023_1_OR_NEWER
            bool hasEventSystem = Object.FindFirstObjectByType<EventSystem>() != null;
#else
            bool hasEventSystem = Object.FindObjectOfType<EventSystem>() != null;
#endif
            if (!hasEventSystem)
            {
                var es = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
                Undo.RegisterCreatedObjectUndo(es, "Create EventSystem");
            }
            return canvas;
        }
    }
}
