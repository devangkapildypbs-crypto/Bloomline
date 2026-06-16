// LevelProgressionService.cs — Level unlock and progression logic
using UnityEngine;
using Bloomline.Core;
using Bloomline.Data;
using Bloomline.Services;

namespace Bloomline.Levels
{
    /// <summary>
    /// Manages level progression: unlocking, completing, and querying star counts.
    /// Reads from and writes to the player profile via ISaveService.
    /// </summary>
    public class LevelProgressionService
    {
        private readonly ISaveService _saveService;
        private PlayerProfile _profile;

        /// <summary>
        /// Create a LevelProgressionService with the given save service.
        /// Loads the player profile immediately.
        /// </summary>
        public LevelProgressionService(ISaveService saveService)
        {
            _saveService = saveService;
            _profile = _saveService.LoadProfile();
        }

        /// <summary>
        /// Create with an explicit profile (useful when profile is already loaded).
        /// </summary>
        public LevelProgressionService(ISaveService saveService, PlayerProfile profile)
        {
            _saveService = saveService;
            _profile = profile ?? new PlayerProfile();
        }

        /// <summary>
        /// The current player profile.
        /// </summary>
        public PlayerProfile Profile => _profile;

        /// <summary>
        /// Check whether a level is unlocked.
        /// Level 1 is always unlocked.
        /// </summary>
        public bool IsLevelUnlocked(int levelNumber)
        {
            if (levelNumber <= 0) return false;
            if (levelNumber == 1) return true;
            return levelNumber <= _profile.highestUnlockedLevel;
        }

        /// <summary>
        /// Record a level completion. Updates profile, recalculates stars,
        /// unlocks the next level if needed, and saves to disk.
        /// Returns true if this was a new best (improved star rating).
        /// </summary>
        public bool CompleteLevel(int levelNumber, int stars, int moves)
        {
            bool isNewBest = _profile.SetLevelProgress(levelNumber, stars, moves);
            _profile.RecalculateTotalStars();

            // Unlock next level
            if (levelNumber >= _profile.highestUnlockedLevel &&
                levelNumber < GameConstants.TOTAL_LEVELS)
            {
                _profile.highestUnlockedLevel = levelNumber + 1;
            }

            // Persist
            _saveService.SaveProfile(_profile);

            Debug.Log($"[Progression] Level {levelNumber} completed — " +
                      $"{stars} stars, {moves} moves, newBest={isNewBest}");
            return isNewBest;
        }

        /// <summary>
        /// Get the best star count for a level (0 if not completed).
        /// </summary>
        public int GetStarsForLevel(int levelNumber)
        {
            LevelProgress progress = _profile.GetLevelProgress(levelNumber);
            return progress != null ? progress.bestStars : 0;
        }

        /// <summary>
        /// Get the best move count for a level (0 if not completed).
        /// </summary>
        public int GetBestMovesForLevel(int levelNumber)
        {
            LevelProgress progress = _profile.GetLevelProgress(levelNumber);
            return progress != null ? progress.bestMoves : 0;
        }

        /// <summary>
        /// Get the total stars earned across all levels.
        /// </summary>
        public int GetTotalStars()
        {
            _profile.RecalculateTotalStars();
            return _profile.totalStarsEarned;
        }

        /// <summary>
        /// Highest unlocked level number.
        /// </summary>
        public int HighestUnlockedLevel => _profile.highestUnlockedLevel;

        /// <summary>
        /// Reload the profile from disk.
        /// </summary>
        public void ReloadProfile()
        {
            _profile = _saveService.LoadProfile();
        }
    }
}
