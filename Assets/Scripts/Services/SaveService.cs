// SaveService.cs — Persistent save/load via PlayerPrefs + JSON
using System;
using UnityEngine;
using Bloomline.Core;
using Bloomline.Data;

namespace Bloomline.Services
{
    /// <summary>
    /// Interface for saving and loading player data.
    /// </summary>
    public interface ISaveService
    {
        /// <summary>Save the player profile to persistent storage.</summary>
        void SaveProfile(PlayerProfile profile);

        /// <summary>Load the player profile. Returns a new default profile on failure.</summary>
        PlayerProfile LoadProfile();

        /// <summary>Save game settings to persistent storage.</summary>
        void SaveSettings(GameSettings settings);

        /// <summary>Load game settings. Returns defaults on failure.</summary>
        GameSettings LoadSettings();

        /// <summary>Delete all saved data.</summary>
        void DeleteAll();
    }

    /// <summary>
    /// PlayerPrefs + JsonUtility implementation of ISaveService.
    /// Wraps all deserialization in try/catch to gracefully handle data corruption.
    /// </summary>
    public class SaveService : ISaveService
    {
        /// <summary>
        /// Save the player profile as JSON to PlayerPrefs.
        /// </summary>
        public void SaveProfile(PlayerProfile profile)
        {
            try
            {
                string json = JsonUtility.ToJson(profile, false);
                PlayerPrefs.SetString(GameConstants.SAVE_KEY_PROFILE, json);
                PlayerPrefs.Save();
                Debug.Log("[SaveService] Profile saved successfully");
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveService] Failed to save profile: {e.Message}");
            }
        }

        /// <summary>
        /// Load the player profile from PlayerPrefs.
        /// Returns a fresh default profile if data is missing or corrupted.
        /// </summary>
        public PlayerProfile LoadProfile()
        {
            try
            {
                if (PlayerPrefs.HasKey(GameConstants.SAVE_KEY_PROFILE))
                {
                    string json = PlayerPrefs.GetString(GameConstants.SAVE_KEY_PROFILE);
                    PlayerProfile profile = JsonUtility.FromJson<PlayerProfile>(json);
                    if (profile != null)
                    {
                        Debug.Log("[SaveService] Profile loaded successfully");
                        return profile;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveService] Corrupted profile data, using defaults: {e.Message}");
            }

            Debug.Log("[SaveService] No saved profile found, creating default");
            return new PlayerProfile();
        }

        /// <summary>
        /// Save game settings as JSON to PlayerPrefs.
        /// </summary>
        public void SaveSettings(GameSettings settings)
        {
            try
            {
                string json = JsonUtility.ToJson(settings, false);
                PlayerPrefs.SetString(GameConstants.SAVE_KEY_SETTINGS, json);
                PlayerPrefs.Save();
                Debug.Log("[SaveService] Settings saved successfully");
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveService] Failed to save settings: {e.Message}");
            }
        }

        /// <summary>
        /// Load game settings from PlayerPrefs.
        /// Returns fresh defaults if data is missing or corrupted.
        /// </summary>
        public GameSettings LoadSettings()
        {
            try
            {
                if (PlayerPrefs.HasKey(GameConstants.SAVE_KEY_SETTINGS))
                {
                    string json = PlayerPrefs.GetString(GameConstants.SAVE_KEY_SETTINGS);
                    GameSettings settings = JsonUtility.FromJson<GameSettings>(json);
                    if (settings != null)
                    {
                        Debug.Log("[SaveService] Settings loaded successfully");
                        return settings;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveService] Corrupted settings data, using defaults: {e.Message}");
            }

            Debug.Log("[SaveService] No saved settings found, creating defaults");
            return new GameSettings();
        }

        /// <summary>
        /// Delete all saved data from PlayerPrefs.
        /// </summary>
        public void DeleteAll()
        {
            PlayerPrefs.DeleteKey(GameConstants.SAVE_KEY_PROFILE);
            PlayerPrefs.DeleteKey(GameConstants.SAVE_KEY_SETTINGS);
            PlayerPrefs.Save();
            Debug.Log("[SaveService] All saved data deleted");
        }
    }
}
