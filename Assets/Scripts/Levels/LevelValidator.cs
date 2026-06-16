// LevelValidator.cs — Level validation and solvability checker
using System.Collections.Generic;
using Bloomline.Gameplay;

namespace Bloomline.Levels
{
    /// <summary>
    /// Result of validating a level definition.
    /// </summary>
    public class ValidationResult
    {
        /// <summary>Whether the level passed all validation checks.</summary>
        public bool IsValid { get; set; }

        /// <summary>List of error messages describing what failed.</summary>
        public List<string> Errors { get; set; } = new List<string>();

        /// <summary>List of non-fatal warnings.</summary>
        public List<string> Warnings { get; set; } = new List<string>();

        /// <summary>
        /// Adds an error and marks the result as invalid.
        /// </summary>
        public void AddError(string message)
        {
            Errors.Add(message);
            IsValid = false;
        }

        /// <summary>
        /// Adds a warning (does not affect validity).
        /// </summary>
        public void AddWarning(string message)
        {
            Warnings.Add(message);
        }

        public override string ToString()
        {
            if (IsValid && Warnings.Count == 0)
                return "Level is valid.";

            string msg = IsValid ? "Level is valid with warnings:\n" : "Level is INVALID:\n";
            foreach (string error in Errors)
                msg += "  ERROR: " + error + "\n";
            foreach (string warning in Warnings)
                msg += "  WARNING: " + warning + "\n";
            return msg;
        }
    }

    /// <summary>
    /// Static utility for validating level data and optionally brute-force checking solvability.
    /// </summary>
    public static class LevelValidator
    {
        /// <summary>
        /// Maximum number of rotatable tiles for which brute-force solve is attempted.
        /// Beyond this count the combinatorial explosion is too large.
        /// </summary>
        private const int MAX_BRUTE_FORCE_TILES = 12;

        /// <summary>
        /// Validates a LevelData for structural correctness.
        /// Checks: grid dimensions, presence of source and flower, all tiles in bounds.
        /// </summary>
        /// <param name="level">The level data to validate.</param>
        /// <returns>A ValidationResult with errors and warnings.</returns>
        public static ValidationResult ValidateLevel(LevelData level)
        {
            ValidationResult result = new ValidationResult { IsValid = true };

            if (level == null)
            {
                result.AddError("LevelData is null.");
                return result;
            }

            // Check grid dimensions
            if (level.gridWidth <= 0 || level.gridHeight <= 0)
            {
                result.AddError($"Invalid grid dimensions: {level.gridWidth}x{level.gridHeight}. Both must be > 0.");
            }

            if (level.gridWidth > 20 || level.gridHeight > 20)
            {
                result.AddWarning($"Grid dimensions {level.gridWidth}x{level.gridHeight} are very large. Performance may suffer.");
            }

            // Check tile list
            if (level.tiles == null || level.tiles.Count == 0)
            {
                result.AddError("Level has no tiles defined.");
                return result;
            }

            bool hasSource = false;
            bool hasFlower = false;
            HashSet<string> positionSet = new HashSet<string>();

            foreach (TileData tileData in level.tiles)
            {
                // Check bounds
                if (tileData.x < 0 || tileData.x >= level.gridWidth ||
                    tileData.y < 0 || tileData.y >= level.gridHeight)
                {
                    result.AddError($"Tile at ({tileData.x}, {tileData.y}) is out of bounds for grid {level.gridWidth}x{level.gridHeight}.");
                }

                // Check for duplicate positions
                string posKey = $"{tileData.x},{tileData.y}";
                if (positionSet.Contains(posKey))
                {
                    result.AddError($"Duplicate tile at position ({tileData.x}, {tileData.y}).");
                }
                else
                {
                    positionSet.Add(posKey);
                }

                // Check tile type
                TileType type = LevelLoader.ParseTileType(tileData.type);
                if (type == TileType.Source) hasSource = true;
                if (type == TileType.Flower) hasFlower = true;

                // Check rotation is valid
                if (tileData.rotation % 90 != 0)
                {
                    result.AddError($"Tile at ({tileData.x}, {tileData.y}) has invalid rotation {tileData.rotation}. Must be a multiple of 90.");
                }
            }

            if (!hasSource)
            {
                result.AddError("Level has no Source tile. At least one is required.");
            }

            if (!hasFlower)
            {
                result.AddError("Level has no Flower tile. At least one is required.");
            }

            // Check move targets
            if (level.moveTargetThreeStars <= 0)
            {
                result.AddWarning("moveTargetThreeStars is not set or is zero.");
            }

            if (level.moveTargetTwoStars > 0 && level.moveTargetThreeStars > 0 &&
                level.moveTargetTwoStars < level.moveTargetThreeStars)
            {
                result.AddWarning("moveTargetTwoStars is less than moveTargetThreeStars — this seems inverted.");
            }

            return result;
        }

        /// <summary>
        /// Attempts a brute-force solve of the level by trying all rotation combinations
        /// of rotatable tiles. Only feasible for small grids (≤12 rotatable tiles).
        /// </summary>
        /// <param name="level">The level data to solve.</param>
        /// <returns>
        /// A ValidationResult indicating whether the level is solvable.
        /// If the grid is too large for brute-force, a warning is added instead.
        /// </returns>
        public static ValidationResult SolveLevel(LevelData level)
        {
            ValidationResult result = new ValidationResult { IsValid = true };

            if (level == null)
            {
                result.AddError("LevelData is null.");
                return result;
            }

            // Build the grid
            int width = level.gridWidth;
            int height = level.gridHeight;
            TileModel[,] grid = new TileModel[width, height];
            List<TileModel> rotatableTiles = new List<TileModel>();

            foreach (TileData td in level.tiles)
            {
                TileType type = LevelLoader.ParseTileType(td.type);
                TileColor color = LevelLoader.ParseTileColor(td.color);
                GridPosition pos = new GridPosition(td.x, td.y);

                if (pos.x >= 0 && pos.x < width && pos.y >= 0 && pos.y < height)
                {
                    TileModel model = new TileModel(pos, type, td.rotation, color, td.locked);
                    grid[pos.x, pos.y] = model;

                    if (model.CanRotate)
                    {
                        rotatableTiles.Add(model);
                    }
                }
            }

            if (rotatableTiles.Count > MAX_BRUTE_FORCE_TILES)
            {
                result.AddWarning($"Level has {rotatableTiles.Count} rotatable tiles, exceeding brute-force limit of {MAX_BRUTE_FORCE_TILES}. Solvability not checked.");
                return result;
            }

            // Try all combinations (4^N where N = rotatable tile count)
            int totalCombinations = 1;
            for (int i = 0; i < rotatableTiles.Count; i++)
            {
                totalCombinations *= 4;
            }

            bool isSolvable = false;

            for (int combo = 0; combo < totalCombinations; combo++)
            {
                // Set each rotatable tile to the rotation defined by this combination index
                int temp = combo;
                for (int i = 0; i < rotatableTiles.Count; i++)
                {
                    int rotation = temp % 4;
                    temp /= 4;
                    rotatableTiles[i].SetRotation(rotation);
                }

                // Run the solver
                LightSolveResult solveResult = LightSolver.Solve(grid, width, height);
                if (solveResult.IsComplete)
                {
                    isSolvable = true;
                    break;
                }
            }

            // Restore original rotations
            foreach (TileModel tile in rotatableTiles)
            {
                tile.ResetRotation();
            }

            if (!isSolvable)
            {
                result.AddError("Level is NOT solvable — no combination of rotations connects all flowers to sources.");
            }

            return result;
        }
    }
}
