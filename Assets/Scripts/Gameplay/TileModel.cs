// TileModel.cs — Pure data model for a single tile
using System;

namespace Bloomline.Gameplay
{
    /// <summary>
    /// Pure data model for a tile on the grid. No Unity dependencies.
    /// Stores type, position, color, rotation state, and computed openings.
    /// </summary>
    [Serializable]
    public class TileModel
    {
        public GridPosition Position { get; private set; }
        public TileType Type { get; private set; }
        public TileColor Color { get; private set; }
        public bool IsLocked { get; private set; }
        public int CurrentRotation { get; private set; } // 0, 1, 2, 3 (number of 90° CW steps)
        public int InitialRotation { get; private set; } // For restart
        public bool IsPowered { get; set; } // Set by LightSolver

        /// <summary>
        /// Current openings based on tile type and rotation.
        /// </summary>
        public Direction CurrentOpenings
        {
            get
            {
                Direction baseOpenings = TileTypeHelper.GetBaseOpenings(Type);
                return DirectionHelper.ApplyRotation(baseOpenings, CurrentRotation);
            }
        }

        /// <summary>
        /// Whether the player can rotate this tile.
        /// </summary>
        public bool CanRotate
        {
            get { return !IsLocked && TileTypeHelper.IsRotatable(Type); }
        }

        public TileModel(GridPosition position, TileType type, int rotationDegrees, TileColor color, bool locked)
        {
            Position = position;
            Type = type;
            Color = color;
            int steps = DirectionHelper.DegreesToSteps(rotationDegrees);
            CurrentRotation = steps;
            InitialRotation = steps;

            // Locked types are always locked; explicit locked flag overrides for rotatable types
            IsLocked = locked || TileTypeHelper.IsLocked(type);
        }

        /// <summary>
        /// Rotate this tile 90° clockwise. Returns true if rotation occurred.
        /// </summary>
        public bool Rotate()
        {
            if (!CanRotate) return false;
            CurrentRotation = (CurrentRotation + 1) % 4;
            return true;
        }

        /// <summary>
        /// Set rotation to a specific step count (0–3).
        /// Used by undo system.
        /// </summary>
        public void SetRotation(int steps)
        {
            CurrentRotation = ((steps % 4) + 4) % 4;
        }

        /// <summary>
        /// Reset to initial rotation. Used by restart.
        /// </summary>
        public void ResetRotation()
        {
            CurrentRotation = InitialRotation;
            IsPowered = false;
        }

        /// <summary>
        /// Returns true if this tile has an opening in the given direction.
        /// </summary>
        public bool HasOpening(Direction dir)
        {
            return (CurrentOpenings & dir) != 0;
        }

        /// <summary>
        /// Returns the rotation in degrees (0, 90, 180, 270).
        /// </summary>
        public int RotationDegrees
        {
            get { return CurrentRotation * 90; }
        }

        public override string ToString()
        {
            return $"Tile[{Type} at {Position}, rot={RotationDegrees}°, color={Color}, locked={IsLocked}, powered={IsPowered}]";
        }
    }
}
