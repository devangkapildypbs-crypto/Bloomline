// TileType.cs — All tile type definitions
namespace Bloomline.Gameplay
{
    /// <summary>
    /// Types of tiles in the Bloomline puzzle grid.
    /// </summary>
    public enum TileType
    {
        // MVP tile types
        Empty = 0,
        Source = 1,
        Flower = 2,
        Straight = 3,
        Corner = 4,
        Blocker = 5,
        LockedStraight = 6,
        LockedCorner = 7,

        // Future tile types (placeholders)
        Splitter = 8,
        OneWay = 9,
        Bridge = 10,
        Switch = 11,
        Gate = 12
    }

    /// <summary>
    /// Light/tile color for colored puzzle mechanics.
    /// </summary>
    public enum TileColor
    {
        White = 0,
        Red = 1,
        Blue = 2,
        Yellow = 3
    }

    public static class TileTypeHelper
    {
        /// <summary>
        /// Returns the base openings for a tile type at rotation 0.
        /// </summary>
        public static Direction GetBaseOpenings(TileType type)
        {
            switch (type)
            {
                case TileType.Source:
                    return Direction.East; // Default: emits East
                case TileType.Flower:
                    return Direction.West; // Default: receives from West
                case TileType.Straight:
                case TileType.LockedStraight:
                    return Direction.North | Direction.South; // Vertical by default
                case TileType.Corner:
                case TileType.LockedCorner:
                    return Direction.North | Direction.East; // Top-right by default
                case TileType.Empty:
                case TileType.Blocker:
                default:
                    return Direction.None;
            }
        }

        /// <summary>
        /// Returns whether a tile type can be rotated by the player.
        /// </summary>
        public static bool IsRotatable(TileType type)
        {
            switch (type)
            {
                case TileType.Straight:
                case TileType.Corner:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Returns whether a tile type is locked (cannot be rotated).
        /// </summary>
        public static bool IsLocked(TileType type)
        {
            switch (type)
            {
                case TileType.LockedStraight:
                case TileType.LockedCorner:
                case TileType.Source:
                case TileType.Flower:
                case TileType.Blocker:
                case TileType.Empty:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Returns whether a tile is a path-carrying tile (has openings).
        /// </summary>
        public static bool IsPathTile(TileType type)
        {
            switch (type)
            {
                case TileType.Source:
                case TileType.Flower:
                case TileType.Straight:
                case TileType.Corner:
                case TileType.LockedStraight:
                case TileType.LockedCorner:
                    return true;
                default:
                    return false;
            }
        }
    }
}
