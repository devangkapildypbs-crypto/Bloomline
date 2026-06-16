// HintSystem.cs — Hint logic for suggesting next move
using Bloomline.Levels;

namespace Bloomline.Gameplay
{
    /// <summary>
    /// Provides hint functionality for the puzzle.
    /// When solution data is available, finds the first tile whose rotation
    /// doesn't match the solution. Otherwise, uses a heuristic: find a
    /// rotatable unpowered tile adjacent to a powered tile.
    /// </summary>
    public class HintSystem
    {
        private readonly BoardManager _boardManager;
        private readonly LevelData _levelData;

        /// <summary>
        /// Creates a new HintSystem instance.
        /// </summary>
        /// <param name="boardManager">Reference to the active BoardManager.</param>
        /// <param name="levelData">The current level data, which may contain solution entries.</param>
        public HintSystem(BoardManager boardManager, LevelData levelData)
        {
            _boardManager = boardManager;
            _levelData = levelData;
        }

        /// <summary>
        /// Gets the position of the next tile the player should rotate.
        /// Returns null if no hint is available (puzzle is solved or no valid hint target).
        /// </summary>
        /// <returns>The GridPosition of the hint tile, or null if none found.</returns>
        public GridPosition? GetHintPosition()
        {
            // Strategy 1: Use solution data if available
            if (_levelData.solution != null && _levelData.solution.Count > 0)
            {
                return GetHintFromSolution();
            }

            // Strategy 2: Heuristic — find rotatable unpowered tile adjacent to powered tile
            return GetHintFromHeuristic();
        }

        /// <summary>
        /// Finds the first tile whose current rotation doesn't match the solution.
        /// </summary>
        private GridPosition? GetHintFromSolution()
        {
            foreach (SolutionEntry entry in _levelData.solution)
            {
                GridPosition pos = new GridPosition(entry.x, entry.y);
                TileModel tile = _boardManager.GetTile(pos);
                if (tile == null) continue;
                if (!tile.CanRotate) continue;

                int correctSteps = DirectionHelper.DegreesToSteps(entry.correctRotation);
                if (tile.CurrentRotation != correctSteps)
                {
                    return pos;
                }
            }

            // All solution tiles already match — no hint needed
            return null;
        }

        /// <summary>
        /// Finds a rotatable unpowered tile that is adjacent to at least one powered tile.
        /// This heuristic guides the player to extend the light path outward from sources.
        /// </summary>
        private GridPosition? GetHintFromHeuristic()
        {
            // First pass: find unpowered rotatable tiles adjacent to powered tiles
            for (int x = 0; x < _levelData.gridWidth; x++)
            {
                for (int y = 0; y < _levelData.gridHeight; y++)
                {
                    GridPosition pos = new GridPosition(x, y);
                    TileModel tile = _boardManager.GetTile(pos);
                    if (tile == null) continue;
                    if (!tile.CanRotate) continue;
                    if (tile.IsPowered) continue;

                    // Check if any neighbor is powered
                    Direction[] directions = DirectionHelper.AllDirections();
                    for (int i = 0; i < directions.Length; i++)
                    {
                        GridPosition neighborPos = pos.GetNeighbor(directions[i]);
                        if (!_boardManager.IsInBounds(neighborPos)) continue;

                        TileModel neighbor = _boardManager.GetTile(neighborPos);
                        if (neighbor != null && neighbor.IsPowered)
                        {
                            return pos;
                        }
                    }
                }
            }

            // Second pass: any rotatable unpowered tile
            for (int x = 0; x < _levelData.gridWidth; x++)
            {
                for (int y = 0; y < _levelData.gridHeight; y++)
                {
                    GridPosition pos = new GridPosition(x, y);
                    TileModel tile = _boardManager.GetTile(pos);
                    if (tile == null) continue;
                    if (!tile.CanRotate) continue;
                    if (tile.IsPowered) continue;

                    return pos;
                }
            }

            return null;
        }
    }
}
