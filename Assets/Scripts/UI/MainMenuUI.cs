// MainMenuUI.cs — Main menu screen with garden-themed programmatic UI
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Bloomline.Core;
using Bloomline.Services;

namespace Bloomline.UI
{
    /// <summary>
    /// Main menu controller for the Bloomline title screen.
    /// Creates all UI elements programmatically — no prefab dependencies.
    /// Features an animated title entrance and garden-themed color palette.
    /// </summary>
    public class MainMenuUI : MonoBehaviour
    {
        [Header("Optional References (auto-created if null)")]
        [SerializeField] private Button playButton;
        [SerializeField] private Button levelSelectButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Image logoImage;
        [SerializeField] private Text titleText;

        private Canvas _canvas;
        private SettingsPopup _settingsPopup;

        private void Awake()
        {
            if (playButton == null || titleText == null)
            {
                CreateUI();
            }
        }

        private void Start()
        {
            // Wire button handlers
            if (playButton != null)
                playButton.onClick.AddListener(OnPlayPressed);
            if (levelSelectButton != null)
                levelSelectButton.onClick.AddListener(OnLevelSelectPressed);
            if (settingsButton != null)
                settingsButton.onClick.AddListener(OnSettingsPressed);

            // Animate title entrance
            StartCoroutine(AnimateTitleEntrance());

            // Log analytics
            if (ServiceLocator.Has<IAnalyticsService>())
            {
                ServiceLocator.Get<IAnalyticsService>().LogGameOpened();
            }
        }

        /// <summary>
        /// Build the entire main menu UI from scratch.
        /// </summary>
        private void CreateUI()
        {
            _canvas = UIHelper.CreateCanvas("MainMenuCanvas");

            Transform root = _canvas.transform;

            // ── Background gradient (two overlapping panels) ──
            UIHelper.CreateFullScreenBackground(root, UIHelper.ColorDeepGreen);

            // Lighter top gradient overlay
            Image topGradient = UIHelper.CreatePanel(root, UIHelper.ColorMidGreen,
                new Vector2(0, 400), new Vector2(1080, 960));
            Color topColor = topGradient.color;
            topColor.a = 0.5f;
            topGradient.color = topColor;

            // ── Decorative garden elements ──
            // Top leaf accent
            Image leafTop = UIHelper.CreateImage(root, UIHelper.ColorSage,
                new Vector2(0, 700), new Vector2(300, 8));
            Color leafColor = leafTop.color;
            leafColor.a = 0.4f;
            leafTop.color = leafColor;

            // Bottom leaf accent
            Image leafBottom = UIHelper.CreateImage(root, UIHelper.ColorSage,
                new Vector2(0, -700), new Vector2(300, 8));
            leafColor = leafBottom.color;
            leafColor.a = 0.4f;
            leafBottom.color = leafColor;

            // ── Logo / flower icon ──
            logoImage = UIHelper.CreateImage(root, UIHelper.ColorSoftGold,
                new Vector2(0, 420), new Vector2(120, 120));

            // ── Title ──
            titleText = UIHelper.CreateText(root, "BLOOMLINE",
                new Vector2(0, 300), 96, UIHelper.ColorCream);
            titleText.fontStyle = FontStyle.Bold;

            // ── Subtitle ──
            Text subtitle = UIHelper.CreateText(root, "A Garden Puzzle",
                new Vector2(0, 220), 40, UIHelper.ColorSoftGold);
            subtitle.fontStyle = FontStyle.Italic;

            // ── Play Button (prominent, centered) ──
            playButton = UIHelper.CreateButton(root, "PLAY",
                new Vector2(0, 20), new Vector2(400, 120),
                UIHelper.ColorSoftGold, null);
            // Override text color for play button
            Text playText = playButton.GetComponentInChildren<Text>();
            if (playText != null)
            {
                playText.color = UIHelper.ColorDeepGreen;
                playText.fontSize = 52;
            }

            // ── Level Select Button ──
            levelSelectButton = UIHelper.CreateButton(root, "LEVELS",
                new Vector2(0, -130), new Vector2(350, 90),
                UIHelper.ColorGardenGreen, null);

            // ── Settings Button ──
            settingsButton = UIHelper.CreateButton(root, "SETTINGS",
                new Vector2(0, -250), new Vector2(350, 90),
                UIHelper.ColorSage, null);

            // ── Version text ──
            UIHelper.CreateText(root, "v1.0",
                new Vector2(0, -850), 28, UIHelper.ColorSage);
        }

        /// <summary>
        /// Animate the title scaling from 0 with a slight overshoot.
        /// </summary>
        private IEnumerator AnimateTitleEntrance()
        {
            if (titleText == null) yield break;

            Transform titleTransform = titleText.transform;
            titleTransform.localScale = Vector3.zero;

            // Also animate logo
            Transform logoTransform = logoImage != null ? logoImage.transform : null;
            if (logoTransform != null) logoTransform.localScale = Vector3.zero;

            float duration = 0.8f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                // Overshoot ease-out: goes past 1.0 then settles
                float overshoot = 1.0f + 0.15f * Mathf.Sin(t * Mathf.PI);
                float scale = Mathf.Lerp(0f, 1f, t) * (t < 0.7f ? overshoot : 1f);

                // Smooth final approach
                if (t > 0.7f)
                {
                    float finalT = (t - 0.7f) / 0.3f;
                    scale = Mathf.Lerp(overshoot, 1f, finalT * finalT);
                }

                titleTransform.localScale = Vector3.one * scale;
                if (logoTransform != null)
                    logoTransform.localScale = Vector3.one * Mathf.Clamp01(scale);

                yield return null;
            }

            titleTransform.localScale = Vector3.one;
            if (logoTransform != null)
                logoTransform.localScale = Vector3.one;

            // Gentle idle pulse on the play button
            if (playButton != null)
            {
                StartCoroutine(PulsePlayButton());
            }
        }

        /// <summary>
        /// Gentle scale pulse on the Play button to draw attention.
        /// </summary>
        private IEnumerator PulsePlayButton()
        {
            Transform btnTransform = playButton.transform;
            while (true)
            {
                float pulse = 1f + 0.03f * Mathf.Sin(Time.time * 2f);
                btnTransform.localScale = Vector3.one * pulse;
                yield return null;
            }
        }

        // ──────────────────── Button Handlers ────────────────────

        private void OnPlayPressed()
        {
            PlayButtonSound();
            // Go directly to LevelSelect (or could go to last-played level)
            SceneLoader.LoadScene(GameConstants.SCENE_LEVEL_SELECT);
        }

        private void OnLevelSelectPressed()
        {
            PlayButtonSound();
            SceneLoader.LoadScene(GameConstants.SCENE_LEVEL_SELECT);
        }

        private void OnSettingsPressed()
        {
            PlayButtonSound();

            // Create and show settings popup if not already active
            if (_settingsPopup == null)
            {
                GameObject popupGo = new GameObject("SettingsPopup");
                _settingsPopup = popupGo.AddComponent<SettingsPopup>();
            }
            else
            {
                _settingsPopup.gameObject.SetActive(true);
            }
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
