// Direction.cs — Cardinal direction enum and utilities
using System;

namespace Bloomline.Gameplay
{
    /// <summary>
    /// Cardinal directions for tile openings and light flow.
    /// Values are ordered clockwise for easy rotation math.
    /// </summary>
    [Flags]
    public enum Direction
    {
        None  = 0,
        North = 1 << 0, // 1
        East  = 1 << 1, // 2
        South = 1 << 2, // 4
        West  = 1 << 3  // 8
    }

    public static class DirectionHelper
    {
        /// <summary>
        /// Returns the opposite direction.
        /// </summary>
        public static Direction GetOpposite(Direction dir)
        {
            switch (dir)
            {
                case Direction.North: return Direction.South;
                case Direction.South: return Direction.North;
                case Direction.East:  return Direction.West;
                case Direction.West:  return Direction.East;
                default: return Direction.None;
            }
        }

        /// <summary>
        /// Rotates a single direction 90 degrees clockwise.
        /// </summary>
        public static Direction RotateCW(Direction dir)
        {
            switch (dir)
            {
                case Direction.North: return Direction.East;
                case Direction.East:  return Direction.South;
                case Direction.South: return Direction.West;
                case Direction.West:  return Direction.North;
                default: return Direction.None;
            }
        }

        /// <summary>
        /// Rotates a set of direction flags 90 degrees clockwise.
        /// </summary>
        public static Direction RotateOpeningsCW(Direction openings)
        {
            Direction result = Direction.None;
            if ((openings & Direction.North) != 0) result |= Direction.East;
            if ((openings & Direction.East)  != 0) result |= Direction.South;
            if ((openings & Direction.South) != 0) result |= Direction.West;
            if ((openings & Direction.West)  != 0) result |= Direction.North;
            return result;
        }

        /// <summary>
        /// Gets the grid offset for a direction.
        /// Note: In Unity 2D, +Y is up (North).
        /// </summary>
        public static GridPosition GetOffset(Direction dir)
        {
            switch (dir)
            {
                case Direction.North: return new GridPosition(0, 1);
                case Direction.South: return new GridPosition(0, -1);
                case Direction.East:  return new GridPosition(1, 0);
                case Direction.West:  return new GridPosition(-1, 0);
                default: return new GridPosition(0, 0);
            }
        }

        /// <summary>
        /// Returns all four cardinal directions.
        /// </summary>
        public static Direction[] AllDirections()
        {
            return new[] { Direction.North, Direction.East, Direction.South, Direction.West };
        }

        /// <summary>
        /// Converts rotation degrees (0, 90, 180, 270) to number of 90° CW steps.
        /// </summary>
        public static int DegreesToSteps(int degrees)
        {
            return ((degrees % 360) + 360) % 360 / 90;
        }

        /// <summary>
        /// Apply N clockwise rotation steps to a set of openings.
        /// </summary>
        public static Direction ApplyRotation(Direction baseOpenings, int rotationSteps)
        {
            Direction result = baseOpenings;
            int steps = ((rotationSteps % 4) + 4) % 4;
            for (int i = 0; i < steps; i++)
            {
                result = RotateOpeningsCW(result);
            }
            return result;
        }
    }
}
