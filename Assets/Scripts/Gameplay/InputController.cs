// InputController.cs — Touch/mouse input handler for tile interaction
using UnityEngine;
using System;
using Bloomline.Core;

namespace Bloomline.Gameplay
{
    /// <summary>
    /// Handles touch and mouse input for tile rotation.
    /// Raycasts to find tapped tiles and dispatches rotation.
    /// </summary>
    public class InputController : MonoBehaviour
    {
        /// <summary>Fired when a tile is successfully rotated.</summary>
        public event Action<GridPosition> OnTileRotated;

        /// <summary>Fired when a locked tile is tapped.</summary>
        public event Action<GridPosition> OnLockedTileTapped;

        private BoardManager _boardManager;
        private Camera _mainCamera;
        private bool _inputEnabled = true;
        private bool _isTouching;

        /// <summary>Enable or disable input processing.</summary>
        public bool InputEnabled
        {
            get => _inputEnabled;
            set => _inputEnabled = value;
        }

        /// <summary>
        /// Initialize with board manager reference.
        /// </summary>
        public void Initialize(BoardManager boardManager)
        {
            _boardManager = boardManager;
            _mainCamera = Camera.main;
        }

        private void Update()
        {
            if (!_inputEnabled || _boardManager == null) return;

            // Handle touch input
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began)
                {
                    HandleTap(touch.position);
                }
            }
            // Handle mouse input (for editor testing)
            else if (Input.GetMouseButtonDown(0))
            {
                HandleTap(Input.mousePosition);
            }
        }

        private void HandleTap(Vector2 screenPosition)
        {
            if (_mainCamera == null) _mainCamera = Camera.main;
            if (_mainCamera == null) return;

            Vector3 worldPos = _mainCamera.ScreenToWorldPoint(screenPosition);
            Vector2 worldPos2D = new Vector2(worldPos.x, worldPos.y);

            // Raycast for tile colliders
            RaycastHit2D hit = Physics2D.Raycast(worldPos2D, Vector2.zero);
            if (hit.collider != null)
            {
                TileView tileView = hit.collider.GetComponent<TileView>();
                if (tileView == null)
                {
                    tileView = hit.collider.GetComponentInParent<TileView>();
                }

                if (tileView != null && tileView.Model != null)
                {
                    if (tileView.IsAnimating) return; // Don't process during animation

                    GridPosition pos = tileView.Model.Position;

                    if (tileView.Model.CanRotate)
                    {
                        bool rotated = _boardManager.RotateTile(pos);
                        if (rotated)
                        {
                            OnTileRotated?.Invoke(pos);
                        }
                    }
                    else if (!tileView.Model.CanRotate && tileView.Model.Type != TileType.Empty)
                    {
                        // Show locked feedback
                        if (tileView.Model.IsLocked)
                        {
                            tileView.AnimateShake();
                            OnLockedTileTapped?.Invoke(pos);
                        }
                    }
                }
            }
        }
    }
}
