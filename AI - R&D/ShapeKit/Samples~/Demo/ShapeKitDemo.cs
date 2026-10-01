using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ShapeKit.Samples
{
    /// <summary>
    /// Add this to an empty GameObject in an empty scene and press Play.
    /// Builds a showcase screen plus a batching stress test (open Window > Analysis > Frame Debugger to count draw calls).
    /// </summary>
    public class ShapeKitDemo : MonoBehaviour
    {
        [SerializeField, Range(0, 2000)] int m_StressCount = 300;

        Font m_Font;

        void Start()
        {
#if UNITY_2022_2_OR_NEWER
            m_Font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
#else
            m_Font = Resources.GetBuiltinResource<Font>("Arial.ttf");
#endif
            var canvas = CreateCanvas();
            var root = MakeRect(canvas.transform, "Safe Area", Vector2.zero, Vector2.zero);
            Stretch(root);
            root.gameObject.AddComponent<SafeAreaFitter>();

            var background = Shape(root, "Background", Vector2.zero, Vector2.zero);
            Stretch(background.rectTransform);
            background.radius = 0f;
            background.fillMode = ShapeImage.FillMode.LinearGradient;
            background.gradientStart = new Color32(0xF4, 0xF6, 0xFB, 0xFF);
            background.gradientEnd = new Color32(0xDD, 0xE4, 0xF5, 0xFF);
            background.preciseRaycast = false;

            BuildShowcase(root);
            BuildStressTest(root);
        }

        void BuildShowcase(RectTransform root)
        {
            // Card
            var card = Shape(root, "Card", new Vector2(0f, 620f), new Vector2(900f, 360f));
            card.radius = 32f;
            card.shadowEnabled = true;
            card.shadowColor = new Color(0f, 0f, 0f, 0.15f);
            card.shadowOffset = new Vector2(0f, -10f);
            card.shadowBlur = 24f;
            Label(card.rectTransform, "Card · radius 32 · soft shadow", new Vector2(0f, 120f), 34, new Color32(0x22, 0x26, 0x33, 0xFF));

            // Avatar inside the card: circular crop of a generated texture.
            var avatar = Shape(card.rectTransform, "Avatar", new Vector2(-300f, -30f), new Vector2(180f, 180f));
            avatar.cornerMode = ShapeImage.CornerMode.Pill;
            avatar.sprite = CreateAvatarSprite();
            avatar.outlineWidth = 6f;
            avatar.outlineColor = Color.white;
            avatar.shadowEnabled = true;
            avatar.shadowColor = new Color(0f, 0f, 0f, 0.3f);
            avatar.shadowBlur = 8f;
            avatar.shadowOffset = new Vector2(0f, -4f);

            // Per-corner chat bubble.
            var bubble = Shape(card.rectTransform, "Bubble", new Vector2(120f, -30f), new Vector2(480f, 140f));
            bubble.cornerMode = ShapeImage.CornerMode.PerCorner;
            bubble.cornerRadii = new Vector4(36f, 36f, 36f, 4f);
            bubble.color = new Color32(0x4F, 0x8B, 0xFF, 0xFF);
            Label(bubble.rectTransform, "Per-corner radius", Vector2.zero, 30, Color.white);

            // Buttons
            var pill = MakeButton(root, "Gradient Pill", new Vector2(-230f, 280f), new Vector2(400f, 110f));
            pill.cornerMode = ShapeImage.CornerMode.Pill;
            pill.fillMode = ShapeImage.FillMode.LinearGradient;
            pill.gradientStart = new Color32(0x4F, 0x8B, 0xFF, 0xFF);
            pill.gradientEnd = new Color32(0x7B, 0x5C, 0xFF, 0xFF);
            pill.gradientAngle = 0f;
            pill.shadowEnabled = true;
            pill.shadowColor = new Color32(0x3A, 0x3F, 0xB8, 0x66);
            pill.shadowOffset = new Vector2(0f, -8f);
            pill.shadowBlur = 14f;

            var outline = MakeButton(root, "Outline", new Vector2(230f, 280f), new Vector2(400f, 110f));
            outline.radius = 24f;
            outline.color = new Color(1f, 1f, 1f, 0f);
            outline.outlineWidth = 4f;
            outline.outlineColor = new Color32(0x4F, 0x8B, 0xFF, 0xFF);
            outline.GetComponentInChildren<Text>().color = new Color32(0x4F, 0x8B, 0xFF, 0xFF);

            // Gradient angles and glow
            for (int i = 0; i < 4; i++)
            {
                var g = Shape(root, "Gradient " + (i * 90), new Vector2(-330f + i * 220f, 60f), new Vector2(180f, 180f));
                g.radius = 40f;
                g.fillMode = ShapeImage.FillMode.LinearGradient;
                g.gradientStart = new Color32(0xFF, 0x7A, 0x59, 0xFF);
                g.gradientEnd = new Color32(0xFF, 0xD2, 0x50, 0xFF);
                g.gradientAngle = i * 45f;
            }

            var glow = Shape(root, "Glow", new Vector2(0f, -150f), new Vector2(600f, 80f));
            glow.cornerMode = ShapeImage.CornerMode.Pill;
            glow.color = new Color32(0x7B, 0x5C, 0xFF, 0x99);
            glow.edgeSoftness = 30f;
            glow.preciseRaycast = false;
        }

        void BuildStressTest(RectTransform root)
        {
            if (m_StressCount <= 0) return;
            var area = MakeRect(root, "Stress Test", new Vector2(0f, -560f), new Vector2(1000f, 640f));
            Label(root, m_StressCount + " shapes, different radius/color/shadow. Check draw calls in Frame Debugger.",
                new Vector2(0f, -220f), 26, new Color32(0x55, 0x5B, 0x6E, 0xFF));

            var random = new System.Random(7);
            for (int i = 0; i < m_StressCount; i++)
            {
                float size = 20f + (float)random.NextDouble() * 50f;
                var pos = new Vector2(
                    ((float)random.NextDouble() - 0.5f) * (1000f - size),
                    ((float)random.NextDouble() - 0.5f) * (560f - size));
                var s = Shape(area, "Shape " + i, pos, new Vector2(size * (0.8f + (float)random.NextDouble()), size));
                s.radius = (float)random.NextDouble() * size * 0.5f;
                s.color = Color.HSVToRGB((float)random.NextDouble(), 0.6f, 0.95f);
                s.outlineWidth = random.Next(3) == 0 ? 2f : 0f;
                s.outlineColor = Color.white;
                s.shadowEnabled = random.Next(2) == 0;
                s.shadowBlur = 4f;
                s.shadowOffset = new Vector2(0f, -2f);
                s.raycastTarget = false;
            }
        }

        Canvas CreateCanvas()
        {
            var go = new GameObject("ShapeKit Demo Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0.5f;

            if (FindAnyEventSystem() == null)
            {
                EventSystemUtility.CreateEventSystem();
            }
            return canvas;
        }

        static EventSystem FindAnyEventSystem()
        {
#if UNITY_2023_1_OR_NEWER
            return FindFirstObjectByType<EventSystem>();
#else
            return FindObjectOfType<EventSystem>();
#endif
        }

        static RectTransform MakeRect(Transform parent, string name, Vector2 position, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            rt.anchoredPosition = position;
            rt.sizeDelta = size;
            return rt;
        }

        static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        static ShapeImage Shape(Transform parent, string name, Vector2 position, Vector2 size)
        {
            return MakeRect(parent, name, position, size).gameObject.AddComponent<ShapeImage>();
        }

        ShapeImage MakeButton(Transform parent, string name, Vector2 position, Vector2 size)
        {
            var shape = Shape(parent, name, position, size);
            var button = shape.gameObject.AddComponent<Button>();
            button.targetGraphic = shape;
            button.onClick.AddListener(() => Debug.Log("[ShapeKit Demo] Clicked " + name));
            shape.gameObject.AddComponent<ShapePressFeedback>();
            Label(shape.rectTransform, name, Vector2.zero, 34, Color.white);
            return shape;
        }

        void Label(Transform parent, string text, Vector2 position, int fontSize, Color color)
        {
            var rt = MakeRect(parent, "Label", position, new Vector2(960f, fontSize * 1.6f));
            var label = rt.gameObject.AddComponent<Text>();
            label.font = m_Font;
            label.text = text;
            label.fontSize = fontSize;
            label.color = color;
            label.alignment = TextAnchor.MiddleCenter;
            label.raycastTarget = false;
            label.horizontalOverflow = HorizontalWrapMode.Overflow;
        }

        static Sprite CreateAvatarSprite()
        {
            const int size = 128;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
            var pixels = new Color32[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float u = x / (float)size, v = y / (float)size;
                    // Simple face: warm background, head and shoulders.
                    Color c = Color.Lerp(new Color32(0xFF, 0xC8, 0x8A, 0xFF), new Color32(0xFF, 0x8F, 0x70, 0xFF), v);
                    float head = new Vector2(u - 0.5f, v - 0.58f).magnitude;
                    float body = new Vector2((u - 0.5f) * 0.8f, v + 0.05f).magnitude;
                    if (head < 0.2f || body < 0.36f) c = new Color32(0x3B, 0x2F, 0x4A, 0xFF);
                    pixels[y * size + x] = c;
                }
            }
            tex.SetPixels32(pixels);
            tex.Apply();
            return Sprite.Create(tex, new UnityEngine.Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        }
    }
}
