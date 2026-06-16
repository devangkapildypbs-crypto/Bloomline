// SettingsPopup.cs — Settings popup with toggles for sound, music, haptics
using UnityEngine;
using UnityEngine.UI;
using Bloomline.Core;
using Bloomline.Data;
using Bloomline.Services;

namespace Bloomline.UI
{
    /// <summary>
    /// Modal settings popup with toggles for Sound, Music, and Haptics.
    /// Saves changes immediately via ISaveService and updates AudioService/HapticsService.
    /// Creates UI programmatically on a high-priority canvas overlay.
    /// </summary>
    public class SettingsPopup : MonoBehaviour
    {
        private Canvas _canvas;
        private GameSettings _settings;
        private Toggle _soundToggle;
        private Toggle _musicToggle;
        private Toggle _hapticsToggle;

        private void Awake()
        {
            // Load current settings
            if (ServiceLocator.Has<ISaveService>())
            {
                _settings = ServiceLocator.Get<ISaveService>().LoadSettings();
            }
            else
            {
                _settings = new GameSettings();
            }

            CreateUI();
        }

        /// <summary>
        /// Build the settings popup UI.
        /// </summary>
        private void CreateUI()
        {
            _canvas = UIHelper.CreateCanvas("SettingsCanvas", sortingOrder: 200);
            Transform root = _canvas.transform;

            // ── Dark overlay background ──
            Image overlay = UIHelper.CreateFullScreenBackground(root, UIHelper.ColorOverlay);
            // Make overlay clickable to close
            Button overlayBtn = overlay.gameObject.AddComponent<Button>();
            overlayBtn.onClick.AddListener(Close);

            // ── Settings panel ──
            Image panel = UIHelper.CreatePanel(root, UIHelper.ColorDeepGreen,
                new Vector2(0, 100), new Vector2(700, 650));

            Transform panelRoot = panel.transform;

            // ── Border accent ──
            Image border = UIHelper.CreatePanel(panelRoot, UIHelper.ColorSoftGold,
                Vector2.zero, new Vector2(710, 660));
            border.transform.SetAsFirstSibling();

            // ── Title ──
            Text title = UIHelper.CreateText(panelRoot, "SETTINGS",
                new Vector2(0, 250), 54, UIHelper.ColorSoftGold);
            title.fontStyle = FontStyle.Bold;

            // ── Divider ──
            UIHelper.CreateImage(panelRoot, UIHelper.ColorSage,
                new Vector2(0, 205), new Vector2(550, 3));

            // ── Sound Toggle ──
            _soundToggle = UIHelper.CreateToggle(panelRoot, "Sound",
                _settings.soundEnabled, new Vector2(0, 130),
                OnSoundChanged);

            // ── Music Toggle ──
            _musicToggle = UIHelper.CreateToggle(panelRoot, "Music",
                _settings.musicEnabled, new Vector2(0, 30),
                OnMusicChanged);

            // ── Haptics Toggle ──
            _hapticsToggle = UIHelper.CreateToggle(panelRoot, "Haptics",
                _settings.hapticsEnabled, new Vector2(0, -70),
                OnHapticsChanged);

            // ── Divider ──
            UIHelper.CreateImage(panelRoot, UIHelper.ColorSage,
                new Vector2(0, -140), new Vector2(550, 3));

            // ── Close button ──
            Button closeBtn = UIHelper.CreateButton(panelRoot, "CLOSE",
                new Vector2(0, -215), new Vector2(300, 80),
                UIHelper.ColorSage, Close);
        }

        // ──────────────────── Toggle Handlers ────────────────────

        private void OnSoundChanged(bool enabled)
        {
            _settings.soundEnabled = enabled;
            SaveAndApplySettings();
        }

        private void OnMusicChanged(bool enabled)
        {
            _settings.musicEnabled = enabled;
            SaveAndApplySettings();
        }

        private void OnHapticsChanged(bool enabled)
        {
            _settings.hapticsEnabled = enabled;

            // Update haptics service
            if (ServiceLocator.Has<IHapticsService>())
            {
                ServiceLocator.Get<IHapticsService>().Enabled = enabled;
            }

            SaveAndApplySettings();
        }

        /// <summary>
        /// Persist settings and apply them to the audio service.
        /// </summary>
        private void SaveAndApplySettings()
        {
            // Save
            if (ServiceLocator.Has<ISaveService>())
            {
                ServiceLocator.Get<ISaveService>().SaveSettings(_settings);
            }

            // Apply to audio
            if (ServiceLocator.Has<IAudioService>())
            {
                ServiceLocator.Get<IAudioService>().ApplySettings(_settings);
            }

            // Log analytics
            if (ServiceLocator.Has<IAnalyticsService>())
            {
                ServiceLocator.Get<IAnalyticsService>().LogSettingsChanged();
            }

            Debug.Log($"[Settings] Updated — Sound={_settings.soundEnabled}, " +
                      $"Music={_settings.musicEnabled}, Haptics={_settings.hapticsEnabled}");
        }

        /// <summary>
        /// Close and destroy the settings popup.
        /// </summary>
        public void Close()
        {
            if (_canvas != null)
            {
                Destroy(_canvas.gameObject);
                _canvas = null;
            }
            Destroy(gameObject);
        }
    }
}
