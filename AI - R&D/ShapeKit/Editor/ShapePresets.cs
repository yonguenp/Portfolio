using System;
using UnityEngine;

namespace ShapeKit.Editor
{
    /// <summary>One-click starting points shown at the top of the ShapeImage inspector.</summary>
    public static class ShapePresets
    {
        public readonly struct Preset
        {
            public readonly string Name;
            public readonly string Description;
            public readonly Action<ShapeImage> Apply;

            public Preset(string name, string description, Action<ShapeImage> apply)
            {
                Name = name;
                Description = description;
                Apply = apply;
            }
        }

        public static readonly Preset[] All =
        {
            new Preset("Card", "White card with soft drop shadow", s =>
            {
                Reset(s);
                s.color = Color.white;
                s.radius = 20f;
                s.shadowEnabled = true;
                s.shadowColor = new Color(0f, 0f, 0f, 0.18f);
                s.shadowOffset = new Vector2(0f, -6f);
                s.shadowBlur = 14f;
            }),
            new Preset("Pill", "Gradient pill button with press-ready shadow", s =>
            {
                Reset(s);
                s.cornerMode = ShapeImage.CornerMode.Pill;
                s.fillMode = ShapeImage.FillMode.LinearGradient;
                s.gradientStart = new Color32(0x4F, 0x8B, 0xFF, 0xFF);
                s.gradientEnd = new Color32(0x7B, 0x5C, 0xFF, 0xFF);
                s.gradientAngle = 0f;
                s.shadowEnabled = true;
                s.shadowColor = new Color32(0x3A, 0x3F, 0xB8, 0x66);
                s.shadowOffset = new Vector2(0f, -5f);
                s.shadowBlur = 10f;
            }),
            new Preset("Outline", "Hollow button with a solid border", s =>
            {
                Reset(s);
                s.radius = 14f;
                s.color = new Color(1f, 1f, 1f, 0f);
                s.outlineWidth = 3f;
                s.outlineColor = new Color32(0x4F, 0x8B, 0xFF, 0xFF);
            }),
            new Preset("Avatar", "Circular crop with a white ring", s =>
            {
                Reset(s);
                s.cornerMode = ShapeImage.CornerMode.Pill;
                s.outlineWidth = 4f;
                s.outlineColor = Color.white;
                s.shadowEnabled = true;
                s.shadowColor = new Color(0f, 0f, 0f, 0.25f);
                s.shadowOffset = new Vector2(0f, -3f);
                s.shadowBlur = 6f;
            }),
            new Preset("Glow", "Soft glowing blob", s =>
            {
                Reset(s);
                s.cornerMode = ShapeImage.CornerMode.Pill;
                s.color = new Color32(0xFF, 0xD2, 0x50, 0xCC);
                s.edgeSoftness = 24f;
            }),
        };

        // Keeps the sprite, size and raycast settings; resets only the look.
        static void Reset(ShapeImage s)
        {
            s.color = Color.white;
            s.cornerMode = ShapeImage.CornerMode.Uniform;
            s.radius = 16f;
            s.fillMode = ShapeImage.FillMode.Solid;
            s.outlineWidth = 0f;
            s.edgeSoftness = 0f;
            s.shadowEnabled = false;
            s.shadowSpread = 0f;
        }
    }
}
