// LevelCompletePopup.cs — Animated level completion popup
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Bloomline.Core;
using Bloomline.Services;

namespace Bloomline.UI
{
    /// <summary>
    /// Popup shown on level completion displaying star rating (animated),
    /// move count, best moves, and navigation buttons.
    /// Features an animated entrance (scale from 0 with overshoot)
    /// and stars that appear one by one with delay.
    /// </summary>
    public class LevelCompletePopup : MonoBehaviour
    {
        /// <summary>Fired when Next Level is tapped.</summary>
        public event Action OnNextLevel;
        /// <summary>Fired when Retry is tapped.</summary>
        public event Action OnRetry;
        /// <summary>Fired when Level Select is tapped.</summary>
        public event Action OnLevelSelect;

        private Canvas _canvas;
        private GameObject _panelGo;
        private Text[] _starTexts;
        private Text _titleText;
        private Text _moveText;
        private Text _bestText;

        /// <summary>
        /// Show the level complete popup with animated entrance.
        /// </summary>
        /// <param name="stars">Star count earned (1-3).</param>
        /// <param name="moves">Moves taken this attempt.</param>
        /// <param name="bestMoves">Best moves recorded for this level.</param>
        /// <param name="isLastLevel">If true, disable Next Level button.</param>
        public void Show(int stars, int moves, int bestMoves, bool isLastLevel = false)
        {
            CreateUI(stars, moves, bestMoves, isLastLevel);
            StartCoroutine(AnimateEntrance(stars));
        }

        /// <summary>
        /// Build the popup UI.
        /// </summary>
        private void CreateUI(int stars, int moves, int bestMoves, bool isLastLevel)
        {
            // Destroy any previous popup canvas
            if (_canvas != null) Destroy(_canvas.gameObject);

            _canvas = UIHelper.CreateCanvas("LevelCompleteCanvas", sortingOrder: 100);
            Transform root = _canvas.transform;

            // ── Semi-transparent dark overlay ──
            UIHelper.CreateFullScreenBackground(root, UIHelper.ColorOverlay);

            // ── Main panel ──
            _panelGo = new GameObject("Panel");
            _panelGo.transform.SetParent(root, false);

            RectTransform panelRt = _panelGo.AddComponent<RectTransform>();
            panelRt.anchoredPosition = Vector2.zero;
            panelRt.sizeDelta = new Vector2(800, 900);

            Image panelBg = _panelGo.AddComponent<Image>();
            panelBg.color = UIHelper.ColorDeepGreen;

            Transform panel = _panelGo.transform;

            // ── Border accent ──
            Image border = UIHelper.CreatePanel(panel, UIHelper.ColorSoftGold,
                Vector2.zero, new Vector2(810, 910));
            border.transform.SetAsFirstSibling();

            // ── Title ──
            _titleText = UIHelper.CreateText(panel, "LEVEL COMPLETE!",
                new Vector2(0, 330), 60, UIHelper.ColorSoftGold);
            _titleText.fontStyle = FontStyle.Bold;

            // ── Decorative divider ──
            UIHelper.CreateImage(panel, UIHelper.ColorSage,
                new Vector2(0, 280), new Vector2(600, 3));

            // ── Stars ──
            _starTexts = new Text[3];
            float starSpacing = 120f;
            for (int i = 0; i < 3; i++)
            {
                float x = (i - 1) * starSpacing;
                _starTexts[i] = UIHelper.CreateText(panel, "☆",
                    new Vector2(x, 180), 100, UIHelper.ColorStarEmpty,
                    TextAnchor.MiddleCenter, new Vector2(120, 120));
                // Start invisible for animation
                _starTexts[i].transform.localScale = Vector3.zero;
            }

            // ── Move count ──
            _moveText = UIHelper.CreateText(panel, $"Moves: {moves}",
                new Vector2(0, 50), 44, UIHelper.ColorCream);

            // ── Best moves ──
            string bestDisplay = bestMoves > 0 ? bestMoves.ToString() : moves.ToString();
            _bestText = UIHelper.CreateText(panel, $"Best: {bestDisplay}",
                new Vector2(0, -10), 34, UIHelper.ColorSage);

            // ── Divider ──
            UIHelper.CreateImage(panel, UIHelper.ColorSage,
                new Vector2(0, -60), new Vector2(600, 3));

            // ── Buttons ──
            float buttonY = -140f;
            float buttonGap = 110f;

            // Next Level button (prominent)
            if (!isLastLevel)
            {
                Button nextBtn = UIHelper.CreateButton(panel, "NEXT LEVEL",
                    new Vector2(0, buttonY), new Vector2(500, 100),
                    UIHelper.ColorSoftGold, () => OnNextLevel?.Invoke());
                // Dark text on gold
                Text nextText = nextBtn.GetComponentInChildren<Text>();
                if (nextText != null)
                {
                    nextText.color = UIHelper.ColorDeepGreen;
                    nextText.fontSize = 42;
                }
                buttonY -= buttonGap;
            }

            // Retry button
            UIHelper.CreateButton(panel, "RETRY",
                new Vector2(0, buttonY), new Vector2(500, 90),
                UIHelper.ColorGardenGreen, () => OnRetry?.Invoke());
            buttonY -= buttonGap;

            // Level Select button
            UIHelper.CreateButton(panel, "LEVEL SELECT",
                new Vector2(0, buttonY), new Vector2(500, 90),
                UIHelper.ColorSage, () => OnLevelSelect?.Invoke());

            // Start with panel at scale 0 for entrance animation
            _panelGo.transform.localScale = Vector3.zero;
        }

        /// <summary>
        /// Animate the popup: panel scales in with overshoot, then stars appear one by one.
        /// </summary>
        private IEnumerator AnimateEntrance(int earnedStars)
        {
            // ── Phase 1: Scale panel in with overshoot ──
            float duration = 0.5f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                // Overshoot ease: goes to ~1.08 then settles to 1.0
                float scale;
                if (t < 0.6f)
                {
                    scale = Mathf.Lerp(0f, 1.08f, t / 0.6f);
                }
                else
                {
                    float settleT = (t - 0.6f) / 0.4f;
                    scale = Mathf.Lerp(1.08f, 1f, settleT);
                }

                _panelGo.transform.localScale = Vector3.one * scale;
                yield return null;
            }
            _panelGo.transform.localScale = Vector3.one;

            // ── Phase 2: Stars appear one by one ──
            for (int i = 0; i < 3; i++)
            {
                yield return new WaitForSecondsRealtime(0.3f);

                if (i < earnedStars)
                {
                    // Earned star — animate pop in
                    _starTexts[i].text = "★";
                    _starTexts[i].color = UIHelper.ColorStarGold;
                    yield return StartCoroutine(AnimateStarPop(_starTexts[i].transform));

                    // Play haptic
                    if (ServiceLocator.Has<IHapticsService>())
                    {
                        ServiceLocator.Get<IHapticsService>().MediumHaptic();
                    }
                }
                else
                {
                    // Empty star — simple fade in
                    _starTexts[i].text = "☆";
                    _starTexts[i].color = UIHelper.ColorStarEmpty;
                    _starTexts[i].transform.localScale = Vector3.one;
                }
            }
        }

        /// <summary>
        /// Pop-in animation for a single star (scale 0 → 1.3 → 1.0).
        /// </summary>
        private IEnumerator AnimateStarPop(Transform starTransform)
        {
            float duration = 0.35f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                float scale;
                if (t < 0.5f)
                {
                    scale = Mathf.Lerp(0f, 1.3f, t / 0.5f);
                }
                else
                {
                    float bounceT = (t - 0.5f) / 0.5f;
                    scale = Mathf.Lerp(1.3f, 1f, bounceT * bounceT);
                }

                starTransform.localScale = Vector3.one * scale;
                yield return null;
            }

            starTransform.localScale = Vector3.one;
        }

        /// <summary>
        /// Hide and destroy the popup.
        /// </summary>
        public void Hide()
        {
            if (_canvas != null)
            {
                Destroy(_canvas.gameObject);
                _canvas = null;
            }
        }

        private void OnDestroy()
        {
            Hide();
        }
    }
}
