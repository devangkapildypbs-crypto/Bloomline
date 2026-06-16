// GameHUD.cs — In-game heads-up display overlay
using System;
using UnityEngine;
using UnityEngine.UI;
using Bloomline.Core;
using Bloomline.Services;

namespace Bloomline.UI
{
    /// <summary>
    /// In-game HUD showing level info at the top and action buttons at the bottom.
    /// Top bar: level number, move counter, star targets/indicator.
    /// Bottom bar: undo, restart, hint, menu/back buttons.
    /// All UI created programmatically — no prefab dependencies.
    /// </summary>
    public class GameHUD : MonoBehaviour
    {
        // ── Public events for GameSceneController to subscribe to ──
        /// <summary>Fired when the undo button is tapped.</summary>
        public event Action OnUndoPressed;
        /// <summary>Fired when the restart button is tapped.</summary>
        public event Action OnRestartPressed;
        /// <summary>Fired when the hint button is tapped.</summary>
        public event Action OnHintPressed;
        /// <summary>Fired when the menu/back button is tapped.</summary>
        public event Action OnMenuPressed;

        // ── Internal UI references ──
        private Canvas _canvas;
        private Text _levelText;
        private Text _moveCountText;
        private Text _starIndicatorText;
        private Text _starTargetText;
        private Text _tutorialText;
        private GameObject _tutorialPanel;

        private int _currentMoves;
        private int _twoStarTarget;
        private int _threeStarTarget;

        /// <summary>
        /// Initialize the HUD with level information.
        /// </summary>
        public void Initialize(int levelNumber, int twoStarTarget, int threeStarTarget)
        {
            _twoStarTarget = twoStarTarget;
            _threeStarTarget = threeStarTarget;

            CreateUI(levelNumber);
            UpdateMoveCount(0);
        }

        /// <summary>
        /// Alias for Initialize, used by GameSceneController.
        /// </summary>
        public void SetLevelInfo(int levelNumber, int threeStarTarget, int twoStarTarget)
        {
            Initialize(levelNumber, twoStarTarget, threeStarTarget);
        }

        /// <summary>
        /// Build the HUD UI elements.
        /// </summary>
        private void CreateUI(int levelNumber)
        {
            _canvas = UIHelper.CreateCanvas("GameHUD", sortingOrder: 10);
            Transform root = _canvas.transform;

            // ═══════════════════ TOP BAR ═══════════════════
            Image topBar = UIHelper.CreatePanel(root, UIHelper.ColorDeepGreen,
                new Vector2(0, 900), new Vector2(1080, 140));
            // Make slightly transparent
            Color topColor = topBar.color;
            topColor.a = 0.92f;
            topBar.color = topColor;

            Transform topRoot = topBar.transform;

            // Level label
            _levelText = UIHelper.CreateText(topRoot, $"LEVEL {levelNumber}",
                new Vector2(-350, 0), 44, UIHelper.ColorCream,
                TextAnchor.MiddleLeft, new Vector2(300, 60));
            _levelText.fontStyle = FontStyle.Bold;

            // Move counter
            _moveCountText = UIHelper.CreateText(topRoot, "Moves: 0",
                new Vector2(0, 0), 38, UIHelper.ColorSoftGold,
                TextAnchor.MiddleCenter, new Vector2(250, 60));

            // Star indicator (current star prediction)
            _starIndicatorText = UIHelper.CreateText(topRoot, "★★★",
                new Vector2(300, 15), 36, UIHelper.ColorStarGold,
                TextAnchor.MiddleCenter, new Vector2(200, 40));

            // Star targets (small text)
            _starTargetText = UIHelper.CreateText(topRoot, $"★★★≤{_threeStarTarget}  ★★≤{_twoStarTarget}",
                new Vector2(300, -20), 22, UIHelper.ColorSage,
                TextAnchor.MiddleCenter, new Vector2(300, 30));

            // ═══════════════════ BOTTOM BAR ═══════════════════
            Image bottomBar = UIHelper.CreatePanel(root, UIHelper.ColorDeepGreen,
                new Vector2(0, -880), new Vector2(1080, 140));
            Color bottomColor = bottomBar.color;
            bottomColor.a = 0.92f;
            bottomBar.color = bottomColor;

            Transform bottomRoot = bottomBar.transform;

            float btnSpacing = 200f;
            float btnY = 0f;
            Vector2 btnSize = new Vector2(140, 80);

            // Menu/Back button
            UIHelper.CreateButton(bottomRoot, "☰",
                new Vector2(-btnSpacing * 1.5f, btnY), btnSize,
                UIHelper.ColorSage, () => OnMenuPressed?.Invoke());

            // Undo button
            UIHelper.CreateButton(bottomRoot, "↩ Undo",
                new Vector2(-btnSpacing * 0.5f, btnY), new Vector2(170, 80),
                UIHelper.ColorGardenGreen, () => OnUndoPressed?.Invoke());

            // Restart button
            UIHelper.CreateButton(bottomRoot, "↻",
                new Vector2(btnSpacing * 0.5f, btnY), btnSize,
                UIHelper.ColorSage, () => OnRestartPressed?.Invoke());

            // Hint button
            UIHelper.CreateButton(bottomRoot, "💡",
                new Vector2(btnSpacing * 1.5f, btnY), btnSize,
                UIHelper.ColorSoftGold, () => OnHintPressed?.Invoke());

            // ═══════════════════ TUTORIAL OVERLAY ═══════════════════
            _tutorialPanel = new GameObject("TutorialPanel");
            _tutorialPanel.transform.SetParent(root, false);
            _tutorialPanel.SetActive(false);

            Image tutBg = _tutorialPanel.AddComponent<Image>();
            Color tutColor = UIHelper.ColorDeepGreen;
            tutColor.a = 0.9f;
            tutBg.color = tutColor;

            RectTransform tutRt = _tutorialPanel.GetComponent<RectTransform>();
            tutRt.anchoredPosition = new Vector2(0, 700);
            tutRt.sizeDelta = new Vector2(900, 100);

            _tutorialText = UIHelper.CreateText(_tutorialPanel.transform, "",
                Vector2.zero, 32, UIHelper.ColorCream,
                TextAnchor.MiddleCenter, new Vector2(860, 90));
        }

        /// <summary>
        /// Update the displayed move count and recalculate the star indicator.
        /// </summary>
        public void UpdateMoveCount(int moves)
        {
            _currentMoves = moves;

            if (_moveCountText != null)
                _moveCountText.text = $"Moves: {moves}";

            UpdateStarIndicator();
        }

        /// <summary>
        /// Force update the star indicator to show a specific star count.
        /// </summary>
        public void UpdateStars(int stars)
        {
            if (_starIndicatorText == null) return;

            string display = "";
            for (int i = 0; i < GameConstants.MAX_STARS; i++)
            {
                display += i < stars ? "★" : "☆";
            }
            _starIndicatorText.text = display;
            _starIndicatorText.color = stars == 3 ? UIHelper.ColorStarGold : UIHelper.ColorCream;
        }

        /// <summary>
        /// Show tutorial text at the top of the screen.
        /// </summary>
        public void ShowTutorialText(string text)
        {
            if (_tutorialPanel == null || _tutorialText == null) return;

            _tutorialText.text = text;
            _tutorialPanel.SetActive(true);
        }

        /// <summary>
        /// Hide the tutorial text.
        /// </summary>
        public void HideTutorialText()
        {
            if (_tutorialPanel != null)
                _tutorialPanel.SetActive(false);
        }

        /// <summary>
        /// Recalculate which star rating the player will get based on current moves.
        /// </summary>
        private void UpdateStarIndicator()
        {
            if (_starIndicatorText == null) return;

            int predictedStars;
            if (_currentMoves <= _threeStarTarget)
                predictedStars = 3;
            else if (_currentMoves <= _twoStarTarget)
                predictedStars = 2;
            else
                predictedStars = 1;

            UpdateStars(predictedStars);
        }

        private void OnDestroy()
        {
            if (_canvas != null)
                Destroy(_canvas.gameObject);
        }
    }
}
