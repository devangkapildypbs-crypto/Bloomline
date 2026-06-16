// GameSceneController.cs — Orchestrates the GameScene
using UnityEngine;
using System.Collections;
using Bloomline.Core;
using Bloomline.Gameplay;
using Bloomline.Levels;
using Bloomline.Services;
using Bloomline.Data;

namespace Bloomline.UI
{
    /// <summary>
    /// Main controller for the GameScene.
    /// Orchestrates BoardManager, InputController, HUD, and popups.
    /// </summary>
    public class GameSceneController : MonoBehaviour
    {
        /// <summary>
        /// Static level number set by LevelSelectUI before loading GameScene.
        /// </summary>
        public static int CurrentLevelNumber = 1;

        private BoardManager _boardManager;
        private InputController _inputController;
        private GameHUD _hud;
        private LevelCompletePopup _completePopup;
        private TutorialBubble _tutorialBubble;
        private LevelData _currentLevel;
        private HintSystem _hintSystem;

        private void Awake()
        {
            SetupCamera();
        }

        private void Start()
        {
            // Create game systems
            SetupBoard();
            SetupInput();
            SetupHUD();

            // Load and start level
            LoadLevel(CurrentLevelNumber);
        }

        private void SetupCamera()
        {
            Camera cam = Camera.main;
            if (cam != null)
            {
                cam.orthographic = true;
                cam.orthographicSize = 6f; // Good for portrait puzzle view
                cam.backgroundColor = new Color(0.10f, 0.22f, 0.16f, 1f); // Deep forest green
                cam.transform.position = new Vector3(0, 0.5f, -10);
            }
        }

        private void SetupBoard()
        {
            GameObject boardObj = new GameObject("BoardManager");
            _boardManager = boardObj.AddComponent<BoardManager>();

            _boardManager.OnLevelComplete += HandleLevelComplete;
            _boardManager.OnMoveCountChanged += HandleMoveCountChanged;
        }

        private void SetupInput()
        {
            GameObject inputObj = new GameObject("InputController");
            _inputController = inputObj.AddComponent<InputController>();
            _inputController.Initialize(_boardManager);

            _inputController.OnTileRotated += HandleTileRotated;
            _inputController.OnLockedTileTapped += HandleLockedTileTapped;
        }

        private void SetupHUD()
        {
            GameObject hudObj = new GameObject("GameHUD");
            _hud = hudObj.AddComponent<GameHUD>();

            // Connect HUD buttons
            _hud.OnUndoPressed += HandleUndo;
            _hud.OnRestartPressed += HandleRestart;
            _hud.OnHintPressed += HandleHint;
            _hud.OnMenuPressed += HandleBackToMenu;
        }

        /// <summary>
        /// Load and start a level by number.
        /// </summary>
        public void LoadLevel(int levelNumber)
        {
            _currentLevel = LevelLoader.LoadLevel(levelNumber);

            if (_currentLevel == null)
            {
                Debug.LogError($"[GameSceneController] Failed to load level {levelNumber}!");
                return;
            }

            Debug.Log($"[GameSceneController] Loading level {levelNumber}: {_currentLevel.levelId}");

            // Initialize board
            _boardManager.InitializeBoard(_currentLevel);

            // Initialize hint system
            _hintSystem = new HintSystem(_boardManager, _currentLevel);

            // Update HUD
            _hud.SetLevelInfo(levelNumber, _currentLevel.moveTargetThreeStars, _currentLevel.moveTargetTwoStars);
            _hud.UpdateMoveCount(0);

            // Show tutorial text if available
            if (!string.IsNullOrEmpty(_currentLevel.tutorialText))
            {
                ShowTutorial(_currentLevel.tutorialText);
            }

            // Enable input
            _inputController.InputEnabled = true;

            // Log analytics
            var analytics = ServiceLocator.Get<IAnalyticsService>();
            analytics?.LogLevelStarted(levelNumber);
        }

        private void HandleTileRotated(GridPosition pos)
        {
            // Play sound
            var audio = ServiceLocator.Get<IAudioService>();
            audio?.PlayTileRotate();

            // Log analytics
            var analytics = ServiceLocator.Get<IAnalyticsService>();
            analytics?.LogMoveMade(CurrentLevelNumber);
        }

        private void HandleLockedTileTapped(GridPosition pos)
        {
            var audio = ServiceLocator.Get<IAudioService>();
            audio?.PlayLockedTile();
        }

        private void HandleMoveCountChanged(int moves)
        {
            _hud?.UpdateMoveCount(moves);
        }

        private void HandleLevelComplete()
        {
            // Disable input
            _inputController.InputEnabled = false;

            int moves = _boardManager.MoveCount;
            int stars = _currentLevel.CalculateStars(moves);

            // Save progress
            var progression = ServiceLocator.Get<LevelProgressionService>();
            progression?.CompleteLevel(CurrentLevelNumber, stars, moves);

            // Play sounds
            var audio = ServiceLocator.Get<IAudioService>();
            audio?.PlayLevelComplete();

            // Log analytics
            var analytics = ServiceLocator.Get<IAnalyticsService>();
            analytics?.LogLevelCompleted(CurrentLevelNumber, stars, moves);

            // Show completion popup
            StartCoroutine(ShowCompletePopup(stars, moves));
        }

        private IEnumerator ShowCompletePopup(int stars, int moves)
        {
            yield return new WaitForSeconds(0.8f);

            // Create popup if not exists
            if (_completePopup == null)
            {
                GameObject popupObj = new GameObject("LevelCompletePopup");
                _completePopup = popupObj.AddComponent<LevelCompletePopup>();
            }

            int bestMoves = moves;
            var profile = ServiceLocator.Get<PlayerProfile>();
            if (profile != null)
            {
                var progress = profile.GetLevelProgress(CurrentLevelNumber);
                if (progress != null)
                {
                    bestMoves = progress.bestMoves;
                }
            }

            bool isLastLevel = CurrentLevelNumber >= GameConstants.TOTAL_LEVELS;
            _completePopup.Show(stars, moves, bestMoves, isLastLevel);
            _completePopup.OnNextLevel += HandleNextLevel;
            _completePopup.OnRetry += HandleRetry;
            _completePopup.OnLevelSelect += HandleBackToLevelSelect;
        }

        private void HandleUndo()
        {
            bool undone = _boardManager.Undo();
            if (undone)
            {
                var analytics = ServiceLocator.Get<IAnalyticsService>();
                analytics?.LogUndoUsed(CurrentLevelNumber);
            }
        }

        private void HandleRestart()
        {
            _boardManager.Restart();
            _hud.UpdateMoveCount(0);
            _inputController.InputEnabled = true;

            if (_completePopup != null)
            {
                _completePopup.Hide();
            }

            var analytics = ServiceLocator.Get<IAnalyticsService>();
            analytics?.LogLevelRestarted(CurrentLevelNumber);
        }

        private void HandleHint()
        {
            if (_hintSystem == null) return;

            GridPosition? hintPos = _hintSystem.GetHintPosition();
            if (hintPos.HasValue)
            {
                TileView view = _boardManager.GetTileView(hintPos.Value);
                if (view != null)
                {
                    view.AnimateHintPulse();
                }

                var analytics = ServiceLocator.Get<IAnalyticsService>();
                analytics?.LogHintUsed(CurrentLevelNumber);
            }
        }

        private void HandleNextLevel()
        {
            if (_completePopup != null) _completePopup.Hide();

            int nextLevel = CurrentLevelNumber + 1;
            if (nextLevel > GameConstants.TOTAL_LEVELS)
            {
                // Last level — go to level select
                HandleBackToLevelSelect();
                return;
            }

            CurrentLevelNumber = nextLevel;
            _boardManager.ClearBoard();
            LoadLevel(nextLevel);
            _inputController.InputEnabled = true;
        }

        private void HandleRetry()
        {
            if (_completePopup != null) _completePopup.Hide();
            HandleRestart();
        }

        private void HandleBackToMenu()
        {
            SceneLoader.LoadScene(GameConstants.SCENE_MAIN_MENU);
        }

        private void HandleBackToLevelSelect()
        {
            SceneLoader.LoadScene(GameConstants.SCENE_LEVEL_SELECT);
        }

        private void ShowTutorial(string text)
        {
            if (_tutorialBubble == null)
            {
                GameObject tutObj = new GameObject("TutorialBubble");
                _tutorialBubble = tutObj.AddComponent<TutorialBubble>();
            }
            _tutorialBubble.ShowMessage(text);
        }

        private void OnDestroy()
        {
            if (_boardManager != null)
            {
                _boardManager.OnLevelComplete -= HandleLevelComplete;
                _boardManager.OnMoveCountChanged -= HandleMoveCountChanged;
            }

            if (_inputController != null)
            {
                _inputController.OnTileRotated -= HandleTileRotated;
                _inputController.OnLockedTileTapped -= HandleLockedTileTapped;
            }

            if (_completePopup != null)
            {
                _completePopup.OnNextLevel -= HandleNextLevel;
                _completePopup.OnRetry -= HandleRetry;
                _completePopup.OnLevelSelect -= HandleBackToLevelSelect;
            }
        }
    }
}
