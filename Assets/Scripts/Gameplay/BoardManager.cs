// BoardManager.cs — Manages the puzzle grid, tiles, and game state
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bloomline.Core;
using Bloomline.Levels;

namespace Bloomline.Gameplay
{
    /// <summary>
    /// Core grid manager. Creates tiles from LevelData, manages the board state,
    /// runs the light solver, and detects win conditions.
    /// </summary>
    public class BoardManager : MonoBehaviour
    {
        // Events
        public event Action OnLevelComplete;
        public event Action<int> OnMoveCountChanged;
        public event Action<LightSolveResult> OnLightSolveUpdated;

        // Grid data
        private TileModel[,] _grid;
        private TileView[,] _tileViews;
        private int _gridWidth;
        private int _gridHeight;
        private LevelData _currentLevel;

        private MoveHistory _moveHistory;
        private bool _isLevelComplete;
        private int _moveCount;

        // Board positioning
        private Vector3 _boardOrigin;
        private Transform _boardParent;

        /// <summary>Current move count.</summary>
        public int MoveCount => _moveCount;
        /// <summary>Whether the current level has been completed.</summary>
        public bool IsLevelComplete => _isLevelComplete;
        /// <summary>Grid width.</summary>
        public int GridWidth => _gridWidth;
        /// <summary>Grid height.</summary>
        public int GridHeight => _gridHeight;
        /// <summary>Access to the move history for undo.</summary>
        public MoveHistory MoveHistory => _moveHistory;
        /// <summary>The current level data.</summary>
        public LevelData CurrentLevel => _currentLevel;

        private void Awake()
        {
            _moveHistory = new MoveHistory();
        }

        /// <summary>
        /// Initialize the board with level data. Creates all tiles and runs initial solve.
        /// </summary>
        public void InitializeBoard(LevelData levelData)
        {
            ClearBoard();

            _currentLevel = levelData;
            _gridWidth = levelData.gridWidth;
            _gridHeight = levelData.gridHeight;
            _grid = new TileModel[_gridWidth, _gridHeight];
            _tileViews = new TileView[_gridWidth, _gridHeight];
            _isLevelComplete = false;
            _moveCount = 0;
            _moveHistory.Clear();

            // Create board parent
            _boardParent = new GameObject("Board").transform;
            _boardParent.SetParent(transform);

            // Calculate board origin to center it on screen
            float totalWidth = _gridWidth * (GameConstants.TILE_SIZE + GameConstants.TILE_SPACING) - GameConstants.TILE_SPACING;
            float totalHeight = _gridHeight * (GameConstants.TILE_SIZE + GameConstants.TILE_SPACING) - GameConstants.TILE_SPACING;
            _boardOrigin = new Vector3(-totalWidth / 2f + GameConstants.TILE_SIZE / 2f,
                                       -totalHeight / 2f + GameConstants.TILE_SIZE / 2f + 0.5f, 0);

            // Create tile models from level data
            foreach (var tileData in levelData.tiles)
            {
                if (tileData.x < 0 || tileData.x >= _gridWidth || tileData.y < 0 || tileData.y >= _gridHeight)
                    continue;

                TileType type = LevelLoader.ParseTileType(tileData.type);
                TileColor color = LevelLoader.ParseTileColor(tileData.color);
                var pos = new GridPosition(tileData.x, tileData.y);
                var model = new TileModel(pos, type, tileData.rotation, color, tileData.locked);
                _grid[tileData.x, tileData.y] = model;
            }

            // Fill remaining positions with empty tiles
            for (int x = 0; x < _gridWidth; x++)
            {
                for (int y = 0; y < _gridHeight; y++)
                {
                    if (_grid[x, y] == null)
                    {
                        var pos = new GridPosition(x, y);
                        _grid[x, y] = new TileModel(pos, TileType.Empty, 0, TileColor.White, true);
                    }
                }
            }

            // Create tile views
            for (int x = 0; x < _gridWidth; x++)
            {
                for (int y = 0; y < _gridHeight; y++)
                {
                    CreateTileView(x, y);
                }
            }

            // Run initial light solve
            RunLightSolver();
        }

        /// <summary>
        /// Get the tile model at a grid position.
        /// </summary>
        public TileModel GetTile(GridPosition pos)
        {
            return GetTile(pos.x, pos.y);
        }

        /// <summary>
        /// Get the tile model at grid coordinates.
        /// </summary>
        public TileModel GetTile(int x, int y)
        {
            if (!IsInBounds(x, y)) return null;
            return _grid[x, y];
        }

        /// <summary>
        /// Get the tile view at grid coordinates.
        /// </summary>
        public TileView GetTileView(int x, int y)
        {
            if (!IsInBounds(x, y)) return null;
            return _tileViews[x, y];
        }

        /// <summary>
        /// Get the tile view at a grid position.
        /// </summary>
        public TileView GetTileView(GridPosition pos)
        {
            return GetTileView(pos.x, pos.y);
        }

        /// <summary>
        /// Check if coordinates are within the grid.
        /// </summary>
        public bool IsInBounds(GridPosition pos)
        {
            return IsInBounds(pos.x, pos.y);
        }

        /// <summary>
        /// Check if coordinates are within the grid.
        /// </summary>
        public bool IsInBounds(int x, int y)
        {
            return x >= 0 && x < _gridWidth && y >= 0 && y < _gridHeight;
        }

        /// <summary>
        /// Attempt to rotate a tile at the given position.
        /// Returns true if rotation occurred.
        /// </summary>
        public bool RotateTile(GridPosition pos)
        {
            if (_isLevelComplete) return false;

            TileModel tile = GetTile(pos);
            if (tile == null) return false;

            if (!tile.CanRotate)
            {
                // Show locked feedback
                TileView view = GetTileView(pos);
                if (view != null)
                {
                    view.AnimateShake();
                }
                return false;
            }

            // Record for undo
            int previousRotation = tile.CurrentRotation;
            _moveHistory.Push(pos, previousRotation);

            // Rotate the tile
            tile.Rotate();
            _moveCount++;
            OnMoveCountChanged?.Invoke(_moveCount);

            // Animate rotation
            TileView tileView = GetTileView(pos);
            if (tileView != null)
            {
                tileView.AnimateRotation();
            }

            // Re-solve light paths
            RunLightSolver();

            return true;
        }

        /// <summary>
        /// Undo the last rotation.
        /// </summary>
        public bool Undo()
        {
            if (_isLevelComplete) return false;

            var entry = _moveHistory.Pop();
            if (entry == null) return false;

            TileModel tile = GetTile(entry.Position);
            if (tile != null)
            {
                tile.SetRotation(entry.PreviousRotation);
                TileView view = GetTileView(entry.Position);
                if (view != null)
                {
                    view.UpdateVisuals();
                }
            }

            // Note: undo doesn't decrement move count (design decision: moves still count)
            RunLightSolver();
            return true;
        }

        /// <summary>
        /// Restart the level to initial state.
        /// </summary>
        public void Restart()
        {
            _isLevelComplete = false;
            _moveCount = 0;
            _moveHistory.Clear();
            OnMoveCountChanged?.Invoke(0);

            for (int x = 0; x < _gridWidth; x++)
            {
                for (int y = 0; y < _gridHeight; y++)
                {
                    if (_grid[x, y] != null)
                    {
                        _grid[x, y].ResetRotation();
                        if (_tileViews[x, y] != null)
                        {
                            _tileViews[x, y].UpdateVisuals();
                        }
                    }
                }
            }

            RunLightSolver();
        }

        /// <summary>
        /// Run the light solver and update all visuals.
        /// </summary>
        public void RunLightSolver()
        {
            LightSolveResult result = LightSolver.Solve(_grid, _gridWidth, _gridHeight);

            // Update all tile powered states
            for (int x = 0; x < _gridWidth; x++)
            {
                for (int y = 0; y < _gridHeight; y++)
                {
                    if (_grid[x, y] != null)
                    {
                        _grid[x, y].IsPowered = false;
                    }
                }
            }

            foreach (var pos in result.PoweredPositions)
            {
                TileModel tile = GetTile(pos);
                if (tile != null) tile.IsPowered = true;
            }

            // Update visuals
            for (int x = 0; x < _gridWidth; x++)
            {
                for (int y = 0; y < _gridHeight; y++)
                {
                    if (_tileViews[x, y] != null)
                    {
                        _tileViews[x, y].UpdatePoweredState(_grid[x, y].IsPowered);
                    }
                }
            }

            // Animate flowers that just bloomed
            foreach (var flowerPos in result.PoweredFlowerPositions)
            {
                TileView flowerView = GetTileView(flowerPos);
                if (flowerView != null)
                {
                    flowerView.AnimateBloom();
                }
            }

            OnLightSolveUpdated?.Invoke(result);

            // Check win condition
            if (result.IsComplete && !_isLevelComplete)
            {
                _isLevelComplete = true;
                StartCoroutine(LevelCompleteSequence());
            }
        }

        /// <summary>
        /// Get the grid as a 2D array (for external use like HintSystem).
        /// </summary>
        public TileModel[,] GetGrid()
        {
            return _grid;
        }

        /// <summary>
        /// Convert grid position to world position.
        /// </summary>
        public Vector3 GridToWorldPosition(int x, int y)
        {
            return _boardOrigin + new Vector3(
                x * (GameConstants.TILE_SIZE + GameConstants.TILE_SPACING),
                y * (GameConstants.TILE_SIZE + GameConstants.TILE_SPACING),
                0);
        }

        /// <summary>
        /// Convert grid position to world position.
        /// </summary>
        public Vector3 GridToWorldPosition(GridPosition pos)
        {
            return GridToWorldPosition(pos.x, pos.y);
        }

        private void CreateTileView(int x, int y)
        {
            TileModel model = _grid[x, y];
            if (model == null || model.Type == TileType.Empty) return;

            GameObject tileObj = new GameObject($"Tile_{x}_{y}");
            tileObj.transform.SetParent(_boardParent);
            tileObj.transform.position = GridToWorldPosition(x, y);

            TileView view = tileObj.AddComponent<TileView>();
            view.Initialize(model);
            _tileViews[x, y] = view;
        }

        private IEnumerator LevelCompleteSequence()
        {
            // Small delay for the last animation to play
            yield return new WaitForSeconds(0.5f);
            OnLevelComplete?.Invoke();
        }

        /// <summary>
        /// Clear all tiles and reset the board.
        /// </summary>
        public void ClearBoard()
        {
            if (_boardParent != null)
            {
                Destroy(_boardParent.gameObject);
                _boardParent = null;
            }

            _grid = null;
            _tileViews = null;
            _gridWidth = 0;
            _gridHeight = 0;
            _isLevelComplete = false;
            _moveCount = 0;
        }
    }
}
