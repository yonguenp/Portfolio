using System;

namespace TransitCity
{
    /// <summary>격자 좌표. 값 타입 — GameObject/MonoBehaviour가 아니라 순수 데이터.</summary>
    public readonly struct GridCoord : IEquatable<GridCoord>
    {
        public readonly int X;
        public readonly int Y;

        public GridCoord(int x, int y)
        {
            X = x;
            Y = y;
        }

        public static GridCoord[] Neighbors4(GridCoord c) => new[]
        {
            new GridCoord(c.X + 1, c.Y),
            new GridCoord(c.X - 1, c.Y),
            new GridCoord(c.X, c.Y + 1),
            new GridCoord(c.X, c.Y - 1),
        };

        public bool Equals(GridCoord other) => X == other.X && Y == other.Y;
        public override bool Equals(object obj) => obj is GridCoord other && Equals(other);
        public override int GetHashCode() => (X * 397) ^ Y;
        public override string ToString() => $"({X},{Y})";
    }
}
