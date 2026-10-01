using UnityEngine;
using UnityEngine.UI;

namespace ShapeKit
{
    /// <summary>
    /// A sprite-free UI shape: rounded rectangle, pill or circle with outline, gradient and drop shadow.
    /// All parameters are packed into vertex data so every ShapeImage batches with one shared material.
    /// </summary>
    [AddComponentMenu("UI/ShapeKit/Shape Image")]
    [RequireComponent(typeof(CanvasRenderer))]
    public class ShapeImage : MaskableGraphic, ICanvasRaycastFilter
    {
        public enum CornerMode
        {
            Uniform,
            PerCorner,
            Pill,
        }

        public enum FillMode
        {
            Solid,
            LinearGradient,
        }

        // Extra pixels around the quad so anti-aliased edges are never cut off.
        const float k_AntiAliasPadding = 2f;

        const AdditionalCanvasShaderChannels k_RequiredChannels =
            AdditionalCanvasShaderChannels.TexCoord1 |
            AdditionalCanvasShaderChannels.TexCoord2 |
            AdditionalCanvasShaderChannels.TexCoord3 |
            AdditionalCanvasShaderChannels.Normal |
            AdditionalCanvasShaderChannels.Tangent;

        [SerializeField] Sprite m_Sprite;

        [SerializeField] CornerMode m_CornerMode = CornerMode.Uniform;
        [SerializeField, Min(0f)] float m_Radius = 16f;
        [Tooltip("x = top-left, y = top-right, z = bottom-right, w = bottom-left")]
        [SerializeField] Vector4 m_CornerRadii = new Vector4(16f, 16f, 16f, 16f);

        [SerializeField] FillMode m_FillMode = FillMode.Solid;
        [SerializeField] Color m_GradientStart = Color.white;
        [SerializeField] Color m_GradientEnd = new Color(0.75f, 0.75f, 0.75f, 1f);
        [SerializeField, Range(0f, 360f)] float m_GradientAngle = 90f;

        [SerializeField, Min(0f)] float m_OutlineWidth;
        [SerializeField] Color m_OutlineColor = Color.black;

        [SerializeField, Min(0f)] float m_EdgeSoftness;

        [SerializeField] bool m_ShadowEnabled;
        [SerializeField] Color m_ShadowColor = new Color(0f, 0f, 0f, 0.35f);
        [SerializeField] Vector2 m_ShadowOffset = new Vector2(0f, -4f);
        [SerializeField, Min(0f)] float m_ShadowBlur = 8f;
        [SerializeField] float m_ShadowSpread;

        [Tooltip("Ignore clicks on the transparent area outside the rounded shape.")]
        [SerializeField] bool m_PreciseRaycast = true;

        static Material s_DefaultMaterial;

        public Sprite sprite
        {
            get => m_Sprite;
            set
            {
                if (m_Sprite == value) return;
                m_Sprite = value;
                SetAllDirty();
            }
        }

        public CornerMode cornerMode { get => m_CornerMode; set => SetField(ref m_CornerMode, value); }
        public float radius { get => m_Radius; set => SetField(ref m_Radius, Mathf.Max(0f, value)); }
        /// <summary>Per-corner radii: x = top-left, y = top-right, z = bottom-right, w = bottom-left.</summary>
        public Vector4 cornerRadii { get => m_CornerRadii; set => SetField(ref m_CornerRadii, value); }
        public FillMode fillMode { get => m_FillMode; set => SetField(ref m_FillMode, value); }
        public Color gradientStart { get => m_GradientStart; set => SetField(ref m_GradientStart, value); }
        public Color gradientEnd { get => m_GradientEnd; set => SetField(ref m_GradientEnd, value); }
        public float gradientAngle { get => m_GradientAngle; set => SetField(ref m_GradientAngle, Mathf.Repeat(value, 360f)); }
        public float outlineWidth { get => m_OutlineWidth; set => SetField(ref m_OutlineWidth, Mathf.Max(0f, value)); }
        public Color outlineColor { get => m_OutlineColor; set => SetField(ref m_OutlineColor, value); }
        public float edgeSoftness { get => m_EdgeSoftness; set => SetField(ref m_EdgeSoftness, Mathf.Max(0f, value)); }
        public bool shadowEnabled { get => m_ShadowEnabled; set => SetField(ref m_ShadowEnabled, value); }
        public Color shadowColor { get => m_ShadowColor; set => SetField(ref m_ShadowColor, value); }
        public Vector2 shadowOffset { get => m_ShadowOffset; set => SetField(ref m_ShadowOffset, value); }
        public float shadowBlur { get => m_ShadowBlur; set => SetField(ref m_ShadowBlur, Mathf.Max(0f, value)); }
        public float shadowSpread { get => m_ShadowSpread; set => SetField(ref m_ShadowSpread, value); }
        public bool preciseRaycast { get => m_PreciseRaycast; set => m_PreciseRaycast = value; }

        public override Texture mainTexture => m_Sprite != null ? m_Sprite.texture : s_WhiteTexture;

        public override Material defaultMaterial
        {
            get
            {
                if (s_DefaultMaterial == null)
                {
                    var shader = Resources.Load<Shader>("ShapeKit/ShapeImage");
                    if (shader == null)
                    {
                        Debug.LogError("[ShapeKit] Shader 'ShapeKit/ShapeImage' is missing from Resources.");
                        return base.defaultMaterial;
                    }
                    s_DefaultMaterial = new Material(shader)
                    {
                        name = "ShapeKit Default",
                        hideFlags = HideFlags.HideAndDontSave,
                    };
                }
                return s_DefaultMaterial;
            }
        }

        void SetField<T>(ref T field, T value)
        {
            if (Equals(field, value)) return;
            field = value;
            SetVerticesDirty();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            EnsureCanvasChannels();
        }

        protected override void OnCanvasHierarchyChanged()
        {
            base.OnCanvasHierarchyChanged();
            EnsureCanvasChannels();
        }

        // The canvas strips vertex channels it was not asked for, which would erase the shape data.
        void EnsureCanvasChannels()
        {
            var c = canvas;
            if (c == null) return;
            if ((c.additionalShaderChannels & k_RequiredChannels) != k_RequiredChannels)
            {
                c.additionalShaderChannels |= k_RequiredChannels;
            }
            var root = c.rootCanvas;
            if (root != null && root != c && (root.additionalShaderChannels & k_RequiredChannels) != k_RequiredChannels)
            {
                root.additionalShaderChannels |= k_RequiredChannels;
            }
        }

        /// <summary>Corner radii actually used for drawing, after mode and size clamping.</summary>
        public Vector4 GetResolvedRadii(Vector2 halfSize)
        {
            Vector4 radii;
            switch (m_CornerMode)
            {
                case CornerMode.Pill:
                    float r = Mathf.Min(halfSize.x, halfSize.y);
                    radii = new Vector4(r, r, r, r);
                    break;
                case CornerMode.PerCorner:
                    radii = m_CornerRadii;
                    break;
                default:
                    radii = new Vector4(m_Radius, m_Radius, m_Radius, m_Radius);
                    break;
            }
            return ShapeMath.ClampRadii(radii, halfSize);
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            Rect rect = GetPixelAdjustedRect();
            if (rect.width <= 0f || rect.height <= 0f) return;

            Vector2 half = rect.size * 0.5f;
            Vector2 center = rect.center;
            Vector4 radii = GetResolvedRadii(half);

            if (m_ShadowEnabled && m_ShadowColor.a > 0f)
            {
                AddShadow(vh, center, half, radii);
            }

            float outline = Mathf.Min(m_OutlineWidth, Mathf.Min(half.x, half.y));
            float padding = k_AntiAliasPadding + m_EdgeSoftness;
            Vector4 uvRect = m_Sprite != null
                ? UnityEngine.Sprites.DataUtility.GetOuterUV(m_Sprite)
                : new Vector4(0f, 0f, 1f, 1f);

            int start = vh.currentVertCount;
            for (int i = 0; i < 4; i++)
            {
                Vector2 local = Corner(i, half + new Vector2(padding, padding));
                Vector2 uv01 = new Vector2(local.x / (2f * half.x) + 0.5f, local.y / (2f * half.y) + 0.5f);

                var vert = UIVertex.simpleVert;
                vert.position = center + local;
                vert.color = Color.white;
                vert.uv0 = new Vector4(
                    Mathf.LerpUnclamped(uvRect.x, uvRect.z, uv01.x),
                    Mathf.LerpUnclamped(uvRect.y, uvRect.w, uv01.y),
                    local.x,
                    local.y);
                vert.uv1 = radii;
                vert.uv2 = new Vector4(half.x, half.y, outline, m_EdgeSoftness);
                vert.uv3 = FillColorAt(local, half);
                vert.normal = Vector3.zero;
                vert.tangent = m_OutlineColor;
                vh.AddVert(vert);
            }
            AddQuadTriangles(vh, start);
        }

        void AddShadow(VertexHelper vh, Vector2 center, Vector2 half, Vector4 radii)
        {
            float minHalf = Mathf.Min(half.x, half.y);
            float spread = Mathf.Max(m_ShadowSpread, -minHalf + 0.5f);
            Vector2 shadowHalf = half + new Vector2(spread, spread);
            Vector4 shadowRadii = ShapeMath.ClampRadii(radii + new Vector4(spread, spread, spread, spread), shadowHalf);
            float padding = k_AntiAliasPadding + m_ShadowBlur;

            // Follows the fill's alpha so fading the shape out also fades its shadow.
            Color shadow = m_ShadowColor;
            shadow.a *= color.a;

            int start = vh.currentVertCount;
            for (int i = 0; i < 4; i++)
            {
                Vector2 local = Corner(i, shadowHalf + new Vector2(padding, padding));
                var vert = UIVertex.simpleVert;
                vert.position = center + m_ShadowOffset + local;
                vert.color = Color.white;
                vert.uv0 = new Vector4(0.5f, 0.5f, local.x, local.y);
                vert.uv1 = shadowRadii;
                vert.uv2 = new Vector4(shadowHalf.x, shadowHalf.y, 0f, m_ShadowBlur);
                vert.uv3 = shadow;
                vert.normal = new Vector3(1f, 0f, 0f);
                vert.tangent = Vector4.zero;
                vh.AddVert(vert);
            }
            AddQuadTriangles(vh, start);
        }

        // Graphic.color tints the fill (like Image tints its sprite); the gradient multiplies on top.
        Vector4 FillColorAt(Vector2 local, Vector2 half)
        {
            Color c = color;
            if (m_FillMode == FillMode.LinearGradient)
            {
                // A linear gradient is linear across each triangle, so per-vertex colors reproduce it exactly.
                float t = ShapeMath.GradientT(local, half, m_GradientAngle);
                Color g = Color.LerpUnclamped(m_GradientStart, m_GradientEnd, t);
                c *= new Color(Mathf.Clamp01(g.r), Mathf.Clamp01(g.g), Mathf.Clamp01(g.b), Mathf.Clamp01(g.a));
            }
            return c;
        }

        // 0 = bottom-left, 1 = top-left, 2 = top-right, 3 = bottom-right
        static Vector2 Corner(int index, Vector2 extent)
        {
            switch (index)
            {
                case 0: return new Vector2(-extent.x, -extent.y);
                case 1: return new Vector2(-extent.x, extent.y);
                case 2: return new Vector2(extent.x, extent.y);
                default: return new Vector2(extent.x, -extent.y);
            }
        }

        static void AddQuadTriangles(VertexHelper vh, int start)
        {
            vh.AddTriangle(start, start + 1, start + 2);
            vh.AddTriangle(start + 2, start + 3, start);
        }

        public virtual bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
        {
            if (!m_PreciseRaycast) return true;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screenPoint, eventCamera, out Vector2 local))
            {
                return false;
            }
            Rect rect = GetPixelAdjustedRect();
            Vector2 half = rect.size * 0.5f;
            return ShapeMath.RoundedBoxSdf(local - rect.center, half, GetResolvedRadii(half)) <= 0f;
        }

        public override void SetNativeSize()
        {
            if (m_Sprite == null) return;
            rectTransform.anchorMax = rectTransform.anchorMin;
            float referencePixelsPerUnit = canvas != null ? canvas.referencePixelsPerUnit : 100f;
            rectTransform.sizeDelta = m_Sprite.rect.size * (referencePixelsPerUnit / m_Sprite.pixelsPerUnit);
            SetAllDirty();
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            EnsureCanvasChannels();
        }

        protected override void Reset()
        {
            base.Reset();
            EnsureCanvasChannels();
        }
#endif
    }
}
