// LightSolver.cs — BFS-based light propagation solver
using System.Collections.Generic;

namespace Bloomline.Gameplay
{
    /// <summary>
    /// Pure logic class that solves light propagation across the puzzle grid.
    /// Uses BFS from all Source tiles, following connected openings and
    /// respecting tile color constraints. No MonoBehaviour or UnityEngine dependency.
    /// </summary>
    public static class LightSolver
    {
        /// <summary>
        /// Solves the light propagation for the entire grid.
        /// BFS starts from every Source tile and follows connected openings.
        /// </summary>
        /// <param name="grid">2D array of TileModel (may contain nulls for empty cells).</param>
        /// <param name="width">Grid width.</param>
        /// <param name="height">Grid height.</param>
        /// <returns>A LightSolveResult with powered tiles, flower states, and path segments.</returns>
        public static LightSolveResult Solve(TileModel[,] grid, int width, int height)
        {
            LightSolveResult result = new LightSolveResult();

            // Collect all flower positions so we can classify them later
            List<GridPosition> allFlowerPositions = new List<GridPosition>();

            // Track which positions are powered and the color flowing through them
            // A tile can be powered by multiple colors (from different sources)
            HashSet<GridPosition> poweredSet = new HashSet<GridPosition>();
            // Track per-position which colors have reached it (for color conflict detection)
            Dictionary<GridPosition, TileColor> positionColorMap = new Dictionary<GridPosition, TileColor>();

            // Collect sources and flowers
            List<TileModel> sources = new List<TileModel>();
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    TileModel tile = grid[x, y];
                    if (tile == null) continue;

                    if (tile.Type == TileType.Source)
                    {
                        sources.Add(tile);
                    }
                    else if (tile.Type == TileType.Flower)
                    {
                        allFlowerPositions.Add(tile.Position);
                    }
                }
            }

            // BFS from each source independently, carrying source color
            foreach (TileModel source in sources)
            {
                BFSFromSource(source, grid, width, height, poweredSet, positionColorMap, result.PathSegments);
            }

            // Populate powered positions
            result.PoweredPositions.AddRange(poweredSet);

            // Classify flowers
            foreach (GridPosition flowerPos in allFlowerPositions)
            {
                if (poweredSet.Contains(flowerPos))
                {
                    result.PoweredFlowerPositions.Add(flowerPos);
                }
                else
                {
                    result.UnpoweredFlowerPositions.Add(flowerPos);
                }
            }

            // Level is complete when there are flowers and all are powered
            result.IsComplete = allFlowerPositions.Count > 0 && result.UnpoweredFlowerPositions.Count == 0;

            // Update IsPowered flag on all tile models
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    TileModel tile = grid[x, y];
                    if (tile != null)
                    {
                        tile.IsPowered = poweredSet.Contains(tile.Position);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Runs BFS from a single source tile, propagating light through connected openings.
        /// </summary>
        private static void BFSFromSource(
            TileModel source,
            TileModel[,] grid,
            int width,
            int height,
            HashSet<GridPosition> poweredSet,
            Dictionary<GridPosition, TileColor> positionColorMap,
            List<LightPathSegment> pathSegments)
        {
            TileColor lightColor = source.Color;

            // BFS queue: each entry is a tile position to process
            Queue<GridPosition> queue = new Queue<GridPosition>();

            // Track visited positions for THIS BFS pass to avoid cycles
            HashSet<GridPosition> visited = new HashSet<GridPosition>();

            // Start from the source
            if (CanAcceptColor(source, lightColor))
            {
                queue.Enqueue(source.Position);
                visited.Add(source.Position);
                poweredSet.Add(source.Position);
                TrackColor(positionColorMap, source.Position, lightColor);
            }

            while (queue.Count > 0)
            {
                GridPosition current = queue.Dequeue();
                TileModel currentTile = GetTile(grid, current, width, height);
                if (currentTile == null) continue;

                // Check all openings of the current tile
                Direction[] directions = DirectionHelper.AllDirections();
                for (int i = 0; i < directions.Length; i++)
                {
                    Direction dir = directions[i];

                    // Current tile must have an opening in this direction
                    if (!currentTile.HasOpening(dir)) continue;

                    // Get the neighbor in that direction
                    GridPosition neighborPos = current.GetNeighbor(dir);

                    // Skip if out of bounds
                    if (!IsInBounds(neighborPos, width, height)) continue;

                    // Skip if already visited in this BFS pass
                    if (visited.Contains(neighborPos)) continue;

                    TileModel neighborTile = GetTile(grid, neighborPos, width, height);
                    if (neighborTile == null) continue;

                    // Neighbor must have an opening facing back toward current tile
                    Direction oppositeDir = DirectionHelper.GetOpposite(dir);
                    if (!neighborTile.HasOpening(oppositeDir)) continue;

                    // Check color compatibility
                    if (!CanAcceptColor(neighborTile, lightColor)) continue;

                    // Connection is valid — record path segment
                    LightPathSegment segment = new LightPathSegment
                    {
                        From = current,
                        To = neighborPos,
                        FromDirection = dir,
                        ToDirection = oppositeDir,
                        Color = lightColor
                    };
                    pathSegments.Add(segment);

                    // Mark as visited and powered
                    visited.Add(neighborPos);
                    poweredSet.Add(neighborPos);
                    TrackColor(positionColorMap, neighborPos, lightColor);

                    // Continue BFS from this neighbor (unless it's a Flower, which is a terminus)
                    // Flowers accept light but don't propagate further
                    if (neighborTile.Type != TileType.Flower)
                    {
                        queue.Enqueue(neighborPos);
                    }
                }
            }
        }

        /// <summary>
        /// Determines whether a tile can accept light of the given color.
        /// White light is accepted by any tile. Colored flowers only accept matching light.
        /// White tiles accept any color light.
        /// </summary>
        private static bool CanAcceptColor(TileModel tile, TileColor lightColor)
        {
            // Empty tiles and blockers cannot carry light
            if (tile.Type == TileType.Empty || tile.Type == TileType.Blocker)
            {
                return false;
            }

            // White tiles accept any color
            if (tile.Color == TileColor.White)
            {
                return true;
            }

            // White light is accepted by any colored tile
            if (lightColor == TileColor.White)
            {
                return true;
            }

            // Colored tile must match colored light
            return tile.Color == lightColor;
        }

        /// <summary>
        /// Records the color that reached a position. First color wins for conflict tracking.
        /// </summary>
        private static void TrackColor(Dictionary<GridPosition, TileColor> map, GridPosition pos, TileColor color)
        {
            if (!map.ContainsKey(pos))
            {
                map[pos] = color;
            }
            // If already tracked, keep the existing color (first source wins)
        }

        /// <summary>
        /// Safely retrieves a tile from the grid, returning null if out of bounds.
        /// </summary>
        private static TileModel GetTile(TileModel[,] grid, GridPosition pos, int width, int height)
        {
            if (!IsInBounds(pos, width, height)) return null;
            return grid[pos.x, pos.y];
        }

        /// <summary>
        /// Checks whether a grid position is within valid bounds.
        /// </summary>
        private static bool IsInBounds(GridPosition pos, int width, int height)
        {
            return pos.x >= 0 && pos.x < width && pos.y >= 0 && pos.y < height;
        }
    }
}
