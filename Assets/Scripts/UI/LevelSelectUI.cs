// LevelSelectUI.cs — Level selection grid with scrollable level buttons
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Bloomline.Core;
using Bloomline.Data;
using Bloomline.Levels;
using Bloomline.Services;

namespace Bloomline.UI
{
    /// <summary>
    /// Level selection screen showing 20 level buttons in a scrollable grid.
    /// Each button displays level number, lock/unlock state, and star rating.
    /// Color coding: locked=gray, unlocked=garden-green, completed=golden.
    /// All UI is created programmatically.
    /// </summary>
    public class LevelSelectUI : MonoBehaviour
    {
        private Canvas _canvas;
        private LevelProgressionService _progression;
        private readonly List<Button> _levelButtons = new List<Button>();

        // Grid layout constants (4 columns × 5 rows)
        private const int COLUMNS = 4;
        private const float BUTTON_SIZE = 200f;
        private const float BUTTON_SPACING = 30f;
        private const float GRID_TOP_OFFSET = -100f;

        private void Awake()
        {
            // Get or create progression service
            ISaveService saveService = ServiceLocator.Has<ISaveService>()
                ? ServiceLocator.Get<ISaveService>()
                : new SaveService();

            _progression = new LevelProgressionService(saveService);

            CreateUI();
        }

        /// <summary>
        /// Build the entire level select UI.
        /// </summary>
        private void CreateUI()
        {
            _canvas = UIHelper.CreateCanvas("LevelSelectCanvas");
            Transform root = _canvas.transform;

            // ── Background ──
            UIHelper.CreateFullScreenBackground(root, UIHelper.ColorDeepGreen);

            // ── Header ──
            // Back button
            UIHelper.CreateButton(root, "<",
                new Vector2(-440, 860), new Vector2(80, 80),
                UIHelper.ColorSage, OnBackPressed);

            // Title
            Text title = UIHelper.CreateText(root, "SELECT LEVEL",
                new Vector2(0, 860), 64, UIHelper.ColorCream);
            title.fontStyle = FontStyle.Bold;

            // Total stars display
            int totalStars = _progression.GetTotalStars();
            int maxStars = GameConstants.TOTAL_LEVELS * GameConstants.MAX_STARS;
            UIHelper.CreateText(root, $"★ {totalStars}/{maxStars}",
                new Vector2(400, 860), 36, UIHelper.ColorStarGold,
                TextAnchor.MiddleRight, new Vector2(200, 60));

            // Divider line
            UIHelper.CreateImage(root, UIHelper.ColorSage,
                new Vector2(0, 810), new Vector2(900, 3));

            // ── Scrollable Grid ──
            RectTransform content = UIHelper.CreateVerticalScrollView(root,
                new Vector2(0, -60), new Vector2(1000, 1500));

            // Calculate total content height
            int rows = Mathf.CeilToInt((float)GameConstants.TOTAL_LEVELS / COLUMNS);
            float totalHeight = rows * (BUTTON_SIZE + BUTTON_SPACING) + BUTTON_SPACING;
            content.sizeDelta = new Vector2(0, totalHeight);

            // ── Create Level Buttons ──
            for (int i = 1; i <= GameConstants.TOTAL_LEVELS; i++)
            {
                CreateLevelButton(content, i);
            }
        }

        /// <summary>
        /// Create a single level button within the grid.
        /// </summary>
        private void CreateLevelButton(Transform parent, int levelNumber)
        {
            bool isUnlocked = _progression.IsLevelUnlocked(levelNumber);
            int stars = _progression.GetStarsForLevel(levelNumber);
            bool isCompleted = stars > 0;

            // Calculate grid position
            int col = (levelNumber - 1) % COLUMNS;
            int row = (levelNumber - 1) / COLUMNS;

            float gridWidth = COLUMNS * (BUTTON_SIZE + BUTTON_SPACING) - BUTTON_SPACING;
            float startX = -gridWidth / 2f + BUTTON_SIZE / 2f;

            float x = startX + col * (BUTTON_SIZE + BUTTON_SPACING);
            float y = GRID_TOP_OFFSET - row * (BUTTON_SIZE + BUTTON_SPACING);

            // Determine button color
            Color btnColor;
            if (!isUnlocked) btnColor = UIHelper.ColorLockedGray;
            else if (isCompleted) btnColor = UIHelper.ColorSoftGold;
            else btnColor = UIHelper.ColorGardenGreen;

            // Create button container
            GameObject go = new GameObject($"Level_{levelNumber}");
            go.transform.SetParent(parent, false);

            RectTransform rt = go.AddComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(x, y);
            rt.sizeDelta = new Vector2(BUTTON_SIZE, BUTTON_SIZE);

            // Background
            Image bgImg = go.AddComponent<Image>();
            bgImg.color = btnColor;

            // Button component
            Button btn = go.AddComponent<Button>();
            btn.targetGraphic = bgImg;
            btn.interactable = isUnlocked;

            // Apply visual feedback colors
            ColorBlock colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(0.9f, 0.9f, 0.9f, 1f);
            colors.pressedColor = new Color(0.7f, 0.7f, 0.7f, 1f);
            colors.disabledColor = new Color(0.6f, 0.6f, 0.6f, 0.8f);
            btn.colors = colors;

            // Level number or lock icon
            string displayText = isUnlocked ? levelNumber.ToString() : "🔒";
            Color textColor = isCompleted ? UIHelper.ColorDeepGreen : UIHelper.ColorCream;

            Text levelText = UIHelper.CreateText(go.transform, displayText,
                new Vector2(0, isUnlocked && isCompleted ? 15 : 0),
                isUnlocked ? 56 : 40, textColor,
                TextAnchor.MiddleCenter, new Vector2(BUTTON_SIZE, BUTTON_SIZE * 0.6f));

            // Star display for completed levels
            if (isUnlocked && isCompleted)
            {
                string starText = "";
                for (int s = 0; s < GameConstants.MAX_STARS; s++)
                {
                    starText += s < stars ? "★" : "☆";
                }

                Text starsDisplay = UIHelper.CreateText(go.transform, starText,
                    new Vector2(0, -55), 32,
                    s: stars == 3 ? UIHelper.ColorStarGold : UIHelper.ColorCream,
                    alignment: TextAnchor.MiddleCenter,
                    size: new Vector2(BUTTON_SIZE, 40));
            }
            else if (isUnlocked && !isCompleted)
            {
                // Show empty stars for unlocked but not completed
                UIHelper.CreateText(go.transform, "☆☆☆",
                    new Vector2(0, -55), 28, UIHelper.ColorSage,
                    TextAnchor.MiddleCenter, new Vector2(BUTTON_SIZE, 40));
            }

            // Wire click
            int capturedLevel = levelNumber; // Capture for closure
            btn.onClick.AddListener(() => OnLevelPressed(capturedLevel));

            _levelButtons.Add(btn);
        }

        // ──────────────────── Handlers ────────────────────

        private void OnLevelPressed(int levelNumber)
        {
            PlayButtonSound();

            Debug.Log($"[LevelSelect] Loading level {levelNumber}");

            // Store selected level for GameSceneController to read
            GameSceneController.CurrentLevelNumber = levelNumber;

            // Log analytics
            if (ServiceLocator.Has<IAnalyticsService>())
            {
                ServiceLocator.Get<IAnalyticsService>().LogLevelStarted(levelNumber);
            }

            SceneLoader.LoadScene(GameConstants.SCENE_GAME);
        }

        private void OnBackPressed()
        {
            PlayButtonSound();
            SceneLoader.LoadScene(GameConstants.SCENE_MAIN_MENU);
        }

        private void PlayButtonSound()
        {
            if (ServiceLocator.Has<IAudioService>())
            {
                IAudioService audio = ServiceLocator.Get<IAudioService>();
                if (audio is AudioService audioImpl)
                {
                    audio.PlaySFX(audioImpl.ButtonTapClip);
                }
            }
        }
    }
}
