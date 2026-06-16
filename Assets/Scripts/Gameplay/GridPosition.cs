// GridPosition.cs — Immutable 2D grid coordinate
using System;

namespace Bloomline.Gameplay
{
    /// <summary>
    /// Immutable integer 2D position on the puzzle grid.
    /// </summary>
    [Serializable]
    public struct GridPosition : IEquatable<GridPosition>
    {
        public int x;
        public int y;

        public GridPosition(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public GridPosition Offset(int dx, int dy)
        {
            return new GridPosition(x + dx, y + dy);
        }

        public GridPosition Offset(GridPosition delta)
        {
            return new GridPosition(x + delta.x, y + delta.y);
        }

        public GridPosition GetNeighbor(Direction dir)
        {
            GridPosition offset = DirectionHelper.GetOffset(dir);
            return new GridPosition(x + offset.x, y + offset.y);
        }

        public bool Equals(GridPosition other)
        {
            return x == other.x && y == other.y;
        }

        public override bool Equals(object obj)
        {
            return obj is GridPosition other && Equals(other);
        }

        public override int GetHashCode()
        {
            return x * 397 ^ y;
        }

        public static bool operator ==(GridPosition a, GridPosition b)
        {
            return a.x == b.x && a.y == b.y;
        }

        public static bool operator !=(GridPosition a, GridPosition b)
        {
            return !(a == b);
        }

        public override string ToString()
        {
            return $"({x}, {y})";
        }
    }
}
