using UnityEngine;

namespace ShapeKit
{
    /// <summary>
    /// CPU-side copies of the shader's shape math, used for raycasting and kept identical to ShapeImage.shader.
    /// </summary>
    public static class ShapeMath
    {
        /// <summary>
        /// Signed distance from <paramref name="p"/> to a rounded box centered at the origin. Negative inside.
        /// </summary>
        /// <param name="radii">Corner radii: x = top-left, y = top-right, z = bottom-right, w = bottom-left.</param>
        public static float RoundedBoxSdf(Vector2 p, Vector2 halfSize, Vector4 radii)
        {
            float r = p.x > 0f
                ? (p.y > 0f ? radii.y : radii.z)
                : (p.y > 0f ? radii.x : radii.w);
            float qx = Mathf.Abs(p.x) - halfSize.x + r;
            float qy = Mathf.Abs(p.y) - halfSize.y + r;
            float outside = new Vector2(Mathf.Max(qx, 0f), Mathf.Max(qy, 0f)).magnitude;
            return Mathf.Min(Mathf.Max(qx, qy), 0f) + outside - r;
        }

        /// <summary>Clamps every radius so no corner can exceed half of the shorter side.</summary>
        public static Vector4 ClampRadii(Vector4 radii, Vector2 halfSize)
        {
            float max = Mathf.Max(0f, Mathf.Min(halfSize.x, halfSize.y));
            return new Vector4(
                Mathf.Clamp(radii.x, 0f, max),
                Mathf.Clamp(radii.y, 0f, max),
                Mathf.Clamp(radii.z, 0f, max),
                Mathf.Clamp(radii.w, 0f, max));
        }

        /// <summary>
        /// Position of <paramref name="p"/> along a linear gradient, 0 at the start edge and 1 at the end edge.
        /// Angle 0 runs left to right, 90 bottom to top.
        /// </summary>
        public static float GradientT(Vector2 p, Vector2 halfSize, float angleDegrees)
        {
            float rad = angleDegrees * Mathf.Deg2Rad;
            var dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
            float extent = Mathf.Abs(halfSize.x * dir.x) + Mathf.Abs(halfSize.y * dir.y);
            if (extent <= 0f)
            {
                return 0.5f;
            }
            return Vector2.Dot(p, dir) / (2f * extent) + 0.5f;
        }
    }
}
