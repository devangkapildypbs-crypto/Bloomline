// PlayerProfile.cs — Player save data
using System;
using System.Collections.Generic;

namespace Bloomline.Data
{
    /// <summary>
    /// Persistent player profile data, serialized to JSON.
    /// </summary>
    [Serializable]
    public class PlayerProfile
    {
        public int highestUnlockedLevel = 1;
        public int totalStarsEarned = 0;
        public int hintsRemaining = 999;
        public List<LevelProgress> levelProgressList = new List<LevelProgress>();

        /// <summary>
        /// Get progress for a specific level. Returns null if not played.
        /// </summary>
        public LevelProgress GetLevelProgress(int levelNumber)
        {
            for (int i = 0; i < levelProgressList.Count; i++)
            {
                if (levelProgressList[i].levelNumber == levelNumber)
                    return levelProgressList[i];
            }
            return null;
        }

        /// <summary>
        /// Set or update progress for a level. Returns true if this is a new best.
        /// </summary>
        public bool SetLevelProgress(int levelNumber, int stars, int moves)
        {
            LevelProgress existing = GetLevelProgress(levelNumber);
            if (existing != null)
            {
                bool isNewBest = stars > existing.bestStars;
                if (stars > existing.bestStars)
                    existing.bestStars = stars;
                if (moves < existing.bestMoves || existing.bestMoves == 0)
                    existing.bestMoves = moves;
                existing.completionCount++;
                return isNewBest;
            }
            else
            {
                levelProgressList.Add(new LevelProgress
                {
                    levelNumber = levelNumber,
                    bestStars = stars,
                    bestMoves = moves,
                    completionCount = 1
                });

                // Unlock next level
                if (levelNumber >= highestUnlockedLevel)
                {
                    highestUnlockedLevel = levelNumber + 1;
                }

                // Recalculate total stars
                RecalculateTotalStars();

                return true;
            }
        }

        public void RecalculateTotalStars()
        {
            int total = 0;
            for (int i = 0; i < levelProgressList.Count; i++)
            {
                total += levelProgressList[i].bestStars;
            }
            totalStarsEarned = total;
        }
    }

    /// <summary>
    /// Progress data for a single level.
    /// </summary>
    [Serializable]
    public class LevelProgress
    {
        public int levelNumber;
        public int bestStars;
        public int bestMoves;
        public int completionCount;
    }

    /// <summary>
    /// Player settings data.
    /// </summary>
    [Serializable]
    public class GameSettings
    {
        public bool soundEnabled = true;
        public bool musicEnabled = true;
        public bool hapticsEnabled = true;
        public bool reducedMotionEnabled = false;
        public bool colorblindModeEnabled = false;
        public float soundVolume = 1.0f;
        public float musicVolume = 0.7f;
    }
}
