// TutorialBubble.cs — Speech-bubble style tutorial text display
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Bloomline.UI
{
    /// <summary>
    /// Displays tutorial/hint messages in a speech-bubble style panel
    /// at the top of the game screen. Auto-hides on tap or after a timeout.
    /// </summary>
    public class TutorialBubble : MonoBehaviour
    {
        private Canvas _canvas;
        private GameObject _bubblePanel;
        private Text _messageText;
        private Button _tapToDismiss;
        private Coroutine _hideCoroutine;

        /// <summary>Default auto-hide timeout in seconds.</summary>
        public float AutoHideDelay { get; set; } = 5f;

        /// <summary>Whether the bubble is currently visible.</summary>
        public bool IsVisible => _bubblePanel != null && _bubblePanel.activeSelf;

        private void Awake()
        {
            CreateUI();
            _bubblePanel.SetActive(false);
        }

        /// <summary>
        /// Build the bubble UI elements.
        /// </summary>
        private void CreateUI()
        {
            _canvas = UIHelper.CreateCanvas("TutorialBubbleCanvas", sortingOrder: 50);
            Transform root = _canvas.transform;

            // ── Bubble panel ──
            _bubblePanel = new GameObject("BubblePanel");
            _bubblePanel.transform.SetParent(root, false);

            RectTransform panelRt = _bubblePanel.AddComponent<RectTransform>();
            panelRt.anchoredPosition = new Vector2(0, 700);
            panelRt.sizeDelta = new Vector2(900, 160);

            // Bubble background
            Image bg = _bubblePanel.AddComponent<Image>();
            Color bgColor = UIHelper.ColorDeepGreen;
            bgColor.a = 0.95f;
            bg.color = bgColor;

            // ── Border ──
            GameObject borderGo = new GameObject("Border");
            borderGo.transform.SetParent(_bubblePanel.transform, false);
            borderGo.transform.SetAsFirstSibling();

            RectTransform borderRt = borderGo.AddComponent<RectTransform>();
            borderRt.anchoredPosition = Vector2.zero;
            borderRt.sizeDelta = new Vector2(906, 166);

            Image borderImg = borderGo.AddComponent<Image>();
            borderImg.color = UIHelper.ColorSoftGold;
            borderGo.transform.SetAsFirstSibling();

            // ── Message text ──
            _messageText = UIHelper.CreateText(_bubblePanel.transform, "",
                new Vector2(0, 5), 32, UIHelper.ColorCream,
                TextAnchor.MiddleCenter, new Vector2(860, 120));
            _messageText.horizontalOverflow = HorizontalWrapMode.Wrap;
            _messageText.verticalOverflow = VerticalWrapMode.Truncate;

            // ── "Tap to dismiss" hint ──
            UIHelper.CreateText(_bubblePanel.transform, "tap to dismiss",
                new Vector2(0, -55), 22, UIHelper.ColorSage,
                TextAnchor.MiddleCenter, new Vector2(300, 30));

            // ── Invisible full-screen button to dismiss on tap ──
            GameObject dismissGo = new GameObject("DismissArea");
            dismissGo.transform.SetParent(_bubblePanel.transform, false);

            RectTransform dismissRt = dismissGo.AddComponent<RectTransform>();
            dismissRt.anchoredPosition = Vector2.zero;
            dismissRt.sizeDelta = new Vector2(900, 160);

            Image dismissImg = dismissGo.AddComponent<Image>();
            dismissImg.color = new Color(0, 0, 0, 0); // Invisible

            _tapToDismiss = dismissGo.AddComponent<Button>();
            _tapToDismiss.onClick.AddListener(Hide);
        }

        /// <summary>
        /// Show a tutorial message. Auto-hides after AutoHideDelay seconds or on tap.
        /// </summary>
        public void ShowMessage(string text)
        {
            if (_bubblePanel == null) return;

            // Cancel any pending auto-hide
            if (_hideCoroutine != null)
            {
                StopCoroutine(_hideCoroutine);
                _hideCoroutine = null;
            }

            _messageText.text = text;
            _bubblePanel.SetActive(true);
            _bubblePanel.transform.localScale = Vector3.one;

            // Start entrance animation
            StartCoroutine(AnimateShow());

            // Auto-hide after delay
            _hideCoroutine = StartCoroutine(AutoHide());
        }

        /// <summary>
        /// Hide the tutorial bubble immediately.
        /// </summary>
        public void Hide()
        {
            if (_hideCoroutine != null)
            {
                StopCoroutine(_hideCoroutine);
                _hideCoroutine = null;
            }

            if (_bubblePanel != null)
            {
                _bubblePanel.SetActive(false);
            }
        }

        /// <summary>
        /// Animate the bubble sliding/fading in.
        /// </summary>
        private IEnumerator AnimateShow()
        {
            if (_bubblePanel == null) yield break;

            RectTransform rt = _bubblePanel.GetComponent<RectTransform>();
            Vector2 targetPos = new Vector2(0, 700);
            Vector2 startPos = new Vector2(0, 800); // Start higher, slide down

            float duration = 0.3f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                // Smooth ease-out
                float smooth = 1f - (1f - t) * (1f - t);
                rt.anchoredPosition = Vector2.Lerp(startPos, targetPos, smooth);
                yield return null;
            }

            rt.anchoredPosition = targetPos;
        }

        /// <summary>
        /// Wait for the auto-hide delay, then hide.
        /// </summary>
        private IEnumerator AutoHide()
        {
            yield return new WaitForSeconds(AutoHideDelay);
            Hide();
        }

        private void OnDestroy()
        {
            if (_canvas != null)
                Destroy(_canvas.gameObject);
        }
    }
}
