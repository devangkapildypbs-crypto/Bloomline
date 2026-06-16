// UIHelper.cs — Static utility for creating UI elements programmatically
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace Bloomline.UI
{
    /// <summary>
    /// Static utility class for creating Unity UI elements entirely in code.
    /// All methods produce ready-to-use GameObjects with the required components.
    /// Designed for 1080×1920 portrait layout via CanvasScaler.
    /// </summary>
    public static class UIHelper
    {
        // ──────────────────── Garden Theme Colors ────────────────────
        /// <summary>Deep forest green — primary background.</summary>
        public static readonly Color ColorDeepGreen = HexColor("#1a3a2a");
        /// <summary>Soft gold — accent, buttons, highlights.</summary>
        public static readonly Color ColorSoftGold = HexColor("#d4a574");
        /// <summary>Cream — primary text, light surfaces.</summary>
        public static readonly Color ColorCream = HexColor("#f5f0e8");
        /// <summary>Muted sage — secondary surfaces, locked items.</summary>
        public static readonly Color ColorSage = HexColor("#4a6a52");
        /// <summary>Dark overlay — popups, modals.</summary>
        public static readonly Color ColorOverlay = new Color(0f, 0f, 0f, 0.7f);
        /// <summary>Locked/disabled gray.</summary>
        public static readonly Color ColorLockedGray = HexColor("#5a5a5a");
        /// <summary>Star gold for level-complete stars.</summary>
        public static readonly Color ColorStarGold = HexColor("#FFD700");
        /// <summary>Empty star gray.</summary>
        public static readonly Color ColorStarEmpty = HexColor("#555555");
        /// <summary>Garden green for unlocked items.</summary>
        public static readonly Color ColorGardenGreen = HexColor("#2d6a3f");
        /// <summary>Bright white for glow effects.</summary>
        public static readonly Color ColorGlow = new Color(0.9f, 1f, 1f, 1f);
        /// <summary>Soft red for warnings/errors.</summary>
        public static readonly Color ColorWarning = HexColor("#c0392b");
        /// <summary>Medium green for gradient midpoint.</summary>
        public static readonly Color ColorMidGreen = HexColor("#234a34");

        // ──────────────────── Canvas Creation ────────────────────

        /// <summary>
        /// Create a full-screen Canvas with CanvasScaler configured for 1080×1920
        /// portrait (Scale With Screen Size). Includes a GraphicRaycaster.
        /// </summary>
        public static Canvas CreateCanvas(string name, int sortingOrder = 0)
        {
            GameObject go = new GameObject(name);
            Canvas canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortingOrder;

            CanvasScaler scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            go.AddComponent<GraphicRaycaster>();

            return canvas;
        }

        // ──────────────────── Button Creation ────────────────────

        /// <summary>
        /// Create a button with text label, position, size, and color.
        /// Optional onClick callback is wired automatically.
        /// Features visual feedback via ColorBlock transitions.
        /// </summary>
        public static Button CreateButton(Transform parent, string text,
            Vector2 position, Vector2 size, Color color, Action onClick = null)
        {
            GameObject go = new GameObject($"Btn_{text}");
            go.transform.SetParent(parent, false);

            RectTransform rt = go.AddComponent<RectTransform>();
            rt.anchoredPosition = position;
            rt.sizeDelta = size;

            // Background image
            Image img = go.AddComponent<Image>();
            img.color = color;

            // Button component with visual feedback
            Button btn = go.AddComponent<Button>();
            ColorBlock colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(0.9f, 0.9f, 0.9f, 1f);
            colors.pressedColor = new Color(0.7f, 0.7f, 0.7f, 1f);
            colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            colors.fadeDuration = 0.1f;
            btn.colors = colors;

            // Text label
            GameObject textGo = new GameObject("Text");
            textGo.transform.SetParent(go.transform, false);

            Text textComp = textGo.AddComponent<Text>();
            textComp.text = text;
            textComp.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            textComp.fontSize = Mathf.RoundToInt(size.y * 0.4f);
            textComp.alignment = TextAnchor.MiddleCenter;
            textComp.color = ColorCream;
            textComp.fontStyle = FontStyle.Bold;

            RectTransform textRt = textGo.GetComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = Vector2.zero;
            textRt.offsetMax = Vector2.zero;

            // Wire click handler
            if (onClick != null)
            {
                btn.onClick.AddListener(new UnityAction(() => onClick()));
            }

            return btn;
        }

        // ──────────────────── Text Creation ────────────────────

        /// <summary>
        /// Create a Text element with specified content, position, font size, and color.
        /// </summary>
        public static Text CreateText(Transform parent, string text,
            Vector2 position, int fontSize, Color color,
            TextAnchor alignment = TextAnchor.MiddleCenter,
            Vector2? size = null)
        {
            GameObject go = new GameObject($"Text_{text.Substring(0, Mathf.Min(text.Length, 12))}");
            go.transform.SetParent(parent, false);

            Text textComp = go.AddComponent<Text>();
            textComp.text = text;
            textComp.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            textComp.fontSize = fontSize;
            textComp.alignment = alignment;
            textComp.color = color;
            textComp.horizontalOverflow = HorizontalWrapMode.Overflow;
            textComp.verticalOverflow = VerticalWrapMode.Overflow;

            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchoredPosition = position;
            rt.sizeDelta = size ?? new Vector2(800, fontSize + 20);

            return textComp;
        }

        // ──────────────────── Panel Creation ────────────────────

        /// <summary>
        /// Create a solid-color panel (Image) at the given position and size.
        /// </summary>
        public static Image CreatePanel(Transform parent, Color color,
            Vector2 position, Vector2 size)
        {
            GameObject go = new GameObject("Panel");
            go.transform.SetParent(parent, false);

            RectTransform rt = go.AddComponent<RectTransform>();
            rt.anchoredPosition = position;
            rt.sizeDelta = size;

            Image img = go.AddComponent<Image>();
            img.color = color;

            return img;
        }

        // ──────────────────── Image Creation ────────────────────

        /// <summary>
        /// Create a colored Image element at the given position and size.
        /// </summary>
        public static Image CreateImage(Transform parent, Color color,
            Vector2 position, Vector2 size)
        {
            GameObject go = new GameObject("Image");
            go.transform.SetParent(parent, false);

            RectTransform rt = go.AddComponent<RectTransform>();
            rt.anchoredPosition = position;
            rt.sizeDelta = size;

            Image img = go.AddComponent<Image>();
            img.color = color;

            return img;
        }

        // ──────────────────── Full-Screen Background ────────────────────

        /// <summary>
        /// Create a full-screen background panel that stretches to fill the canvas.
        /// </summary>
        public static Image CreateFullScreenBackground(Transform parent, Color color)
        {
            GameObject go = new GameObject("Background");
            go.transform.SetParent(parent, false);

            RectTransform rt = go.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            Image img = go.AddComponent<Image>();
            img.color = color;

            return img;
        }

        // ──────────────────── Toggle Creation ────────────────────

        /// <summary>
        /// Create a labeled toggle with specified state.
        /// </summary>
        public static Toggle CreateToggle(Transform parent, string label,
            bool isOn, Vector2 position, Action<bool> onValueChanged = null)
        {
            GameObject go = new GameObject($"Toggle_{label}");
            go.transform.SetParent(parent, false);

            RectTransform rt = go.AddComponent<RectTransform>();
            rt.anchoredPosition = position;
            rt.sizeDelta = new Vector2(600, 80);

            // Label
            Text labelText = CreateText(go.transform, label,
                new Vector2(-60, 0), 40, ColorCream,
                TextAnchor.MiddleLeft, new Vector2(400, 80));

            // Toggle background
            GameObject bgGo = new GameObject("Background");
            bgGo.transform.SetParent(go.transform, false);
            RectTransform bgRt = bgGo.AddComponent<RectTransform>();
            bgRt.anchoredPosition = new Vector2(240, 0);
            bgRt.sizeDelta = new Vector2(80, 50);
            Image bgImg = bgGo.AddComponent<Image>();
            bgImg.color = ColorSage;

            // Checkmark
            GameObject checkGo = new GameObject("Checkmark");
            checkGo.transform.SetParent(bgGo.transform, false);
            RectTransform checkRt = checkGo.AddComponent<RectTransform>();
            checkRt.anchorMin = new Vector2(0.15f, 0.15f);
            checkRt.anchorMax = new Vector2(0.85f, 0.85f);
            checkRt.offsetMin = Vector2.zero;
            checkRt.offsetMax = Vector2.zero;
            Image checkImg = checkGo.AddComponent<Image>();
            checkImg.color = ColorSoftGold;

            // Toggle component
            Toggle toggle = go.AddComponent<Toggle>();
            toggle.isOn = isOn;
            toggle.targetGraphic = bgImg;
            toggle.graphic = checkImg;

            if (onValueChanged != null)
            {
                toggle.onValueChanged.AddListener(new UnityAction<bool>(val => onValueChanged(val)));
            }

            return toggle;
        }

        // ──────────────────── Scroll View Creation ────────────────────

        /// <summary>
        /// Create a basic vertical ScrollRect with a content container.
        /// Returns the content RectTransform where items should be added.
        /// </summary>
        public static RectTransform CreateVerticalScrollView(Transform parent,
            Vector2 position, Vector2 size)
        {
            // Viewport
            GameObject viewportGo = new GameObject("ScrollView");
            viewportGo.transform.SetParent(parent, false);

            RectTransform viewportRt = viewportGo.AddComponent<RectTransform>();
            viewportRt.anchoredPosition = position;
            viewportRt.sizeDelta = size;

            Image viewportImg = viewportGo.AddComponent<Image>();
            viewportImg.color = new Color(0, 0, 0, 0.01f); // Nearly invisible, needed for masking
            Mask mask = viewportGo.AddComponent<Mask>();
            mask.showMaskGraphic = false;

            // Content container
            GameObject contentGo = new GameObject("Content");
            contentGo.transform.SetParent(viewportGo.transform, false);

            RectTransform contentRt = contentGo.AddComponent<RectTransform>();
            contentRt.anchorMin = new Vector2(0, 1);
            contentRt.anchorMax = new Vector2(1, 1);
            contentRt.pivot = new Vector2(0.5f, 1);
            contentRt.anchoredPosition = Vector2.zero;
            contentRt.sizeDelta = new Vector2(0, 0); // Will be set by content

            // ScrollRect
            ScrollRect scrollRect = viewportGo.AddComponent<ScrollRect>();
            scrollRect.content = contentRt;
            scrollRect.viewport = viewportRt;
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Elastic;
            scrollRect.elasticity = 0.1f;
            scrollRect.scrollSensitivity = 30f;

            return contentRt;
        }

        // ──────────────────── Utility ────────────────────

        /// <summary>
        /// Parse a hex color string (e.g. "#FF0000" or "FF0000") to a Unity Color.
        /// </summary>
        public static Color HexColor(string hex)
        {
            if (hex.StartsWith("#")) hex = hex.Substring(1);
            if (hex.Length == 6) hex += "FF"; // Add full alpha

            ColorUtility.TryParseHtmlString("#" + hex, out Color color);
            return color;
        }

        /// <summary>
        /// Set a RectTransform to stretch and fill its parent.
        /// </summary>
        public static void StretchToFill(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }
    }
}
