// LevelData.cs — JSON-serializable level definition
using System;
using System.Collections.Generic;

namespace Bloomline.Levels
{
    /// <summary>
    /// Serializable level data loaded from JSON.
    /// </summary>
    [Serializable]
    public class LevelData
    {
        public string levelId;
        public int chapter;
        public int levelNumber;
        public int gridWidth;
        public int gridHeight;
        public int moveTargetTwoStars;
        public int moveTargetThreeStars;
        public string tutorialText;
        public List<TileData> tiles;
        public List<SolutionEntry> solution; // Optional: correct rotations for hint system

        /// <summary>
        /// Calculate star rating based on moves taken.
        /// </summary>
        public int CalculateStars(int movesTaken)
        {
            if (movesTaken <= moveTargetThreeStars) return 3;
            if (movesTaken <= moveTargetTwoStars) return 2;
            return 1;
        }
    }

    /// <summary>
    /// Serializable tile entry within a level.
    /// </summary>
    [Serializable]
    public class TileData
    {
        public int x;
        public int y;
        public string type;      // "Source", "Flower", "Straight", "Corner", "Blocker", "LockedStraight", "LockedCorner", "Empty"
        public int rotation;     // Degrees: 0, 90, 180, 270
        public string color;     // "White", "Red", "Blue", "Yellow"
        public bool locked;
    }

    /// <summary>
    /// Optional solution data for hint system.
    /// Stores the correct rotation for each rotatable tile.
    /// </summary>
    [Serializable]
    public class SolutionEntry
    {
        public int x;
        public int y;
        public int correctRotation; // Degrees
    }
}
