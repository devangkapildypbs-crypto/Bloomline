// LightSolveResult.cs — Result object from the light solver
using System.Collections.Generic;

namespace Bloomline.Gameplay
{
    /// <summary>
    /// Result of a light solver pass across the grid.
    /// </summary>
    public class LightSolveResult
    {
        /// <summary>
        /// True if all flowers on the board are powered by matching sources.
        /// </summary>
        public bool IsComplete { get; set; }

        /// <summary>
        /// All grid positions that have light flowing through them.
        /// </summary>
        public List<GridPosition> PoweredPositions { get; set; } = new List<GridPosition>();

        /// <summary>
        /// Flower positions that are correctly powered.
        /// </summary>
        public List<GridPosition> PoweredFlowerPositions { get; set; } = new List<GridPosition>();

        /// <summary>
        /// Flower positions that are NOT yet powered.
        /// </summary>
        public List<GridPosition> UnpoweredFlowerPositions { get; set; } = new List<GridPosition>();

        /// <summary>
        /// Path segments for animation: each segment is a list of positions from source to destination.
        /// </summary>
        public List<LightPathSegment> PathSegments { get; set; } = new List<LightPathSegment>();
    }

    /// <summary>
    /// A segment of light flowing from one tile to the next.
    /// Used for animation purposes.
    /// </summary>
    public class LightPathSegment
    {
        public GridPosition From { get; set; }
        public GridPosition To { get; set; }
        public Direction FromDirection { get; set; }
        public Direction ToDirection { get; set; }
        public TileColor Color { get; set; }
    }
}
