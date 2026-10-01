using UnityEditor;
using UnityEditor.UI;
using UnityEngine;

namespace ShapeKit.Editor
{
    [CustomEditor(typeof(ShapeImage), true)]
    [CanEditMultipleObjects]
    public class ShapeImageEditor : GraphicEditor
    {
        SerializedProperty m_Sprite;
        SerializedProperty m_CornerMode;
        SerializedProperty m_Radius;
        SerializedProperty m_CornerRadii;
        SerializedProperty m_FillMode;
        SerializedProperty m_GradientStart;
        SerializedProperty m_GradientEnd;
        SerializedProperty m_GradientAngle;
        SerializedProperty m_OutlineWidth;
        SerializedProperty m_OutlineColor;
        SerializedProperty m_EdgeSoftness;
        SerializedProperty m_ShadowEnabled;
        SerializedProperty m_ShadowColor;
        SerializedProperty m_ShadowOffset;
        SerializedProperty m_ShadowBlur;
        SerializedProperty m_ShadowSpread;
        SerializedProperty m_PreciseRaycast;

        static bool s_ShowShape = true;
        static bool s_ShowFill = true;
        static bool s_ShowOutline = true;
        static bool s_ShowShadow = true;

        protected override void OnEnable()
        {
            base.OnEnable();
            m_Sprite = serializedObject.FindProperty("m_Sprite");
            m_CornerMode = serializedObject.FindProperty("m_CornerMode");
            m_Radius = serializedObject.FindProperty("m_Radius");
            m_CornerRadii = serializedObject.FindProperty("m_CornerRadii");
            m_FillMode = serializedObject.FindProperty("m_FillMode");
            m_GradientStart = serializedObject.FindProperty("m_GradientStart");
            m_GradientEnd = serializedObject.FindProperty("m_GradientEnd");
            m_GradientAngle = serializedObject.FindProperty("m_GradientAngle");
            m_OutlineWidth = serializedObject.FindProperty("m_OutlineWidth");
            m_OutlineColor = serializedObject.FindProperty("m_OutlineColor");
            m_EdgeSoftness = serializedObject.FindProperty("m_EdgeSoftness");
            m_ShadowEnabled = serializedObject.FindProperty("m_ShadowEnabled");
            m_ShadowColor = serializedObject.FindProperty("m_ShadowColor");
            m_ShadowOffset = serializedObject.FindProperty("m_ShadowOffset");
            m_ShadowBlur = serializedObject.FindProperty("m_ShadowBlur");
            m_ShadowSpread = serializedObject.FindProperty("m_ShadowSpread");
            m_PreciseRaycast = serializedObject.FindProperty("m_PreciseRaycast");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawPresets();
            EditorGUILayout.Space(4);

            EditorGUILayout.PropertyField(m_Sprite, new GUIContent("Sprite (optional)", "Drawn inside the shape, e.g. a circular avatar."));
            AppearanceControlsGUI();
            RaycastControlsGUI();
            EditorGUILayout.PropertyField(m_PreciseRaycast);
            MaskableControlsGUI();

            s_ShowShape = Section("Shape", s_ShowShape);
            if (s_ShowShape)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(m_CornerMode);
                if (!m_CornerMode.hasMultipleDifferentValues)
                {
                    var mode = (ShapeImage.CornerMode)m_CornerMode.enumValueIndex;
                    if (mode == ShapeImage.CornerMode.Uniform)
                    {
                        EditorGUILayout.PropertyField(m_Radius);
                    }
                    else if (mode == ShapeImage.CornerMode.PerCorner)
                    {
                        DrawCornerGrid();
                    }
                    else
                    {
                        EditorGUILayout.HelpBox("Radius follows the shorter side: pill on rectangles, circle on squares.", MessageType.None);
                    }
                }
                EditorGUILayout.PropertyField(m_EdgeSoftness, new GUIContent("Edge Softness", "Feathers the edge for glows and soft blobs."));
                EditorGUI.indentLevel--;
            }

            s_ShowFill = Section("Fill", s_ShowFill);
            if (s_ShowFill)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(m_FillMode);
                if (!m_FillMode.hasMultipleDifferentValues && m_FillMode.enumValueIndex == (int)ShapeImage.FillMode.LinearGradient)
                {
                    EditorGUILayout.PropertyField(m_GradientStart, new GUIContent("Start"));
                    EditorGUILayout.PropertyField(m_GradientEnd, new GUIContent("End"));
                    EditorGUILayout.PropertyField(m_GradientAngle, new GUIContent("Angle", "0 = left to right, 90 = bottom to top."));
                    DrawAngleButtons();
                }
                EditorGUI.indentLevel--;
            }

            s_ShowOutline = Section("Outline", s_ShowOutline);
            if (s_ShowOutline)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(m_OutlineWidth, new GUIContent("Width"));
                if (m_OutlineWidth.hasMultipleDifferentValues || m_OutlineWidth.floatValue > 0f)
                {
                    EditorGUILayout.PropertyField(m_OutlineColor, new GUIContent("Color"));
                }
                EditorGUI.indentLevel--;
            }

            s_ShowShadow = Section("Shadow", s_ShowShadow, m_ShadowEnabled);
            if (s_ShowShadow && (m_ShadowEnabled.hasMultipleDifferentValues || m_ShadowEnabled.boolValue))
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(m_ShadowColor, new GUIContent("Color"));
                EditorGUILayout.PropertyField(m_ShadowOffset, new GUIContent("Offset"));
                EditorGUILayout.PropertyField(m_ShadowBlur, new GUIContent("Blur"));
                EditorGUILayout.PropertyField(m_ShadowSpread, new GUIContent("Spread"));
                EditorGUI.indentLevel--;
            }

            SetShowNativeSize(m_Sprite.hasMultipleDifferentValues || m_Sprite.objectReferenceValue != null, false);
            NativeSizeButtonGUI();
            serializedObject.ApplyModifiedProperties();
        }

        static bool Section(string title, bool expanded, SerializedProperty toggle = null)
        {
            EditorGUILayout.Space(2);
            Rect rect = EditorGUILayout.GetControlRect(false, 20f);
            EditorGUI.DrawRect(rect, EditorGUIUtility.isProSkin ? new Color(1f, 1f, 1f, 0.05f) : new Color(0f, 0f, 0f, 0.06f));
            var foldRect = new Rect(rect.x + 2f, rect.y, rect.width - 24f, rect.height);
            expanded = EditorGUI.Foldout(foldRect, expanded, title, true, EditorStyles.foldoutHeader);
            if (toggle != null)
            {
                var toggleRect = new Rect(rect.xMax - 20f, rect.y + 1f, 18f, rect.height);
                EditorGUI.showMixedValue = toggle.hasMultipleDifferentValues;
                EditorGUI.BeginChangeCheck();
                bool value = EditorGUI.Toggle(toggleRect, toggle.boolValue);
                if (EditorGUI.EndChangeCheck())
                {
                    toggle.boolValue = value;
                }
                EditorGUI.showMixedValue = false;
            }
            return expanded;
        }

        // Laid out like the shape itself: top corners on the first row, bottom corners on the second.
        void DrawCornerGrid()
        {
            Vector4 r = m_CornerRadii.vector4Value;
            EditorGUI.showMixedValue = m_CornerRadii.hasMultipleDifferentValues;
            EditorGUI.BeginChangeCheck();
            using (new EditorGUILayout.HorizontalScope())
            {
                r.x = LabeledFloat("TL", r.x);
                r.y = LabeledFloat("TR", r.y);
            }
            using (new EditorGUILayout.HorizontalScope())
            {
                r.w = LabeledFloat("BL", r.w);
                r.z = LabeledFloat("BR", r.z);
            }
            if (EditorGUI.EndChangeCheck())
            {
                m_CornerRadii.vector4Value = Vector4.Max(r, Vector4.zero);
            }
            EditorGUI.showMixedValue = false;
        }

        static float LabeledFloat(string label, float value)
        {
            float oldWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 40f;
            value = EditorGUILayout.FloatField(label, value);
            EditorGUIUtility.labelWidth = oldWidth;
            return value;
        }

        void DrawAngleButtons()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(EditorGUIUtility.labelWidth);
                foreach (var (label, angle) in new[] { ("0°", 0f), ("45°", 45f), ("90°", 90f), ("180°", 180f), ("270°", 270f) })
                {
                    if (GUILayout.Button(label, EditorStyles.miniButton))
                    {
                        m_GradientAngle.floatValue = angle;
                    }
                }
            }
        }

        void DrawPresets()
        {
            EditorGUILayout.LabelField("Presets", EditorStyles.miniBoldLabel);
            using (new EditorGUILayout.HorizontalScope())
            {
                foreach (var preset in ShapePresets.All)
                {
                    if (GUILayout.Button(new GUIContent(preset.Name, preset.Description), EditorStyles.miniButton))
                    {
                        foreach (var t in targets)
                        {
                            var shape = (ShapeImage)t;
                            Undo.RecordObject(shape, "Apply ShapeKit Preset");
                            preset.Apply(shape);
                            EditorUtility.SetDirty(shape);
                        }
                        serializedObject.Update();
                    }
                }
            }
        }
    }
}
