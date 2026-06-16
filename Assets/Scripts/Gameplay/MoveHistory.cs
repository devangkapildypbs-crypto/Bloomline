// MoveHistory.cs — Undo stack for tile rotations
using System.Collections.Generic;

namespace Bloomline.Gameplay
{
    /// <summary>
    /// Entry representing a single rotation move that can be undone.
    /// </summary>
    public class MoveEntry
    {
        /// <summary>Position of the tile that was rotated.</summary>
        public GridPosition Position { get; private set; }

        /// <summary>The rotation step the tile was at before this move.</summary>
        public int PreviousRotation { get; private set; }

        public MoveEntry(GridPosition position, int previousRotation)
        {
            Position = position;
            PreviousRotation = previousRotation;
        }
    }

    /// <summary>
    /// Tracks the history of tile rotations as a stack, supporting undo.
    /// Also tracks the total number of moves made (not reduced by undo).
    /// </summary>
    public class MoveHistory
    {
        private readonly Stack<MoveEntry> _undoStack = new Stack<MoveEntry>();
        private int _totalMoveCount;

        /// <summary>
        /// Whether there are moves that can be undone.
        /// </summary>
        public bool CanUndo
        {
            get { return _undoStack.Count > 0; }
        }

        /// <summary>
        /// Total number of moves made since the last Clear (not reduced by undo).
        /// </summary>
        public int MoveCount
        {
            get { return _totalMoveCount; }
        }

        /// <summary>
        /// The number of entries currently on the undo stack.
        /// </summary>
        public int UndoStackCount
        {
            get { return _undoStack.Count; }
        }

        /// <summary>
        /// Records a new move. Increments the total move counter.
        /// </summary>
        /// <param name="position">The grid position of the rotated tile.</param>
        /// <param name="previousRotation">The rotation step the tile was at before rotating.</param>
        public void Push(GridPosition position, int previousRotation)
        {
            _undoStack.Push(new MoveEntry(position, previousRotation));
            _totalMoveCount++;
        }

        /// <summary>
        /// Pops the most recent move from the undo stack.
        /// Returns null if the stack is empty.
        /// Does NOT decrement the total move count.
        /// </summary>
        /// <returns>The most recent MoveEntry, or null if no moves to undo.</returns>
        public MoveEntry Pop()
        {
            if (_undoStack.Count == 0) return null;
            return _undoStack.Pop();
        }

        /// <summary>
        /// Peeks at the most recent move without removing it.
        /// Returns null if the stack is empty.
        /// </summary>
        public MoveEntry Peek()
        {
            if (_undoStack.Count == 0) return null;
            return _undoStack.Peek();
        }

        /// <summary>
        /// Clears all history and resets the total move count.
        /// </summary>
        public void Clear()
        {
            _undoStack.Clear();
            _totalMoveCount = 0;
        }
    }
}
