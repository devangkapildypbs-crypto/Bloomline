// RemoteConfigService.cs — Remote config interface with local defaults
using System.Collections.Generic;
using UnityEngine;
using Bloomline.Core;

namespace Bloomline.Services
{
    /// <summary>
    /// Interface for remote configuration values.
    /// Allows feature flags and tuning parameters to be changed without an app update.
    /// </summary>
    public interface IRemoteConfigService
    {
        /// <summary>Get a boolean config value.</summary>
        bool GetBool(string key, bool defaultValue);

        /// <summary>Get an integer config value.</summary>
        int GetInt(string key, int defaultValue);

        /// <summary>Get a float config value.</summary>
        float GetFloat(string key, float defaultValue);

        /// <summary>Get a string config value.</summary>
        string GetString(string key, string defaultValue);

        /// <summary>Whether hints are enabled.</summary>
        bool HintEnabled { get; }

        /// <summary>Maximum free hints before requiring ads.</summary>
        int MaxFreeHints { get; }

        /// <summary>How often interstitial ads appear (every N levels, 0 = disabled).</summary>
        int InterstitialFrequency { get; }

        /// <summary>Whether rewarded ads for hints are enabled.</summary>
        bool RewardedHintEnabled { get; }

        /// <summary>Whether the daily puzzle feature is enabled.</summary>
        bool DailyPuzzleEnabled { get; }

        /// <summary>Multiplier for difficulty progression.</summary>
        float DifficultyCurveModifier { get; }
    }

    /// <summary>
    /// Local implementation of IRemoteConfigService.
    /// Returns hardcoded defaults from GameConstants.
    /// Replace with Firebase Remote Config or similar for production.
    /// </summary>
    public class LocalRemoteConfigService : IRemoteConfigService
    {
        private readonly Dictionary<string, object> _overrides = new Dictionary<string, object>();

        /// <inheritdoc/>
        public bool GetBool(string key, bool defaultValue)
        {
            if (_overrides.TryGetValue(key, out object val) && val is bool b) return b;
            return defaultValue;
        }

        /// <inheritdoc/>
        public int GetInt(string key, int defaultValue)
        {
            if (_overrides.TryGetValue(key, out object val) && val is int i) return i;
            return defaultValue;
        }

        /// <inheritdoc/>
        public float GetFloat(string key, float defaultValue)
        {
            if (_overrides.TryGetValue(key, out object val) && val is float f) return f;
            return defaultValue;
        }

        /// <inheritdoc/>
        public string GetString(string key, string defaultValue)
        {
            if (_overrides.TryGetValue(key, out object val) && val is string s) return s;
            return defaultValue;
        }

        /// <inheritdoc/>
        public bool HintEnabled =>
            GetBool("hint_enabled", GameConstants.DEFAULT_HINT_ENABLED);

        /// <inheritdoc/>
        public int MaxFreeHints =>
            GetInt("max_free_hints", GameConstants.DEFAULT_MAX_FREE_HINTS);

        /// <inheritdoc/>
        public int InterstitialFrequency =>
            GetInt("interstitial_frequency", GameConstants.DEFAULT_INTERSTITIAL_FREQUENCY);

        /// <inheritdoc/>
        public bool RewardedHintEnabled =>
            GetBool("rewarded_hint_enabled", GameConstants.DEFAULT_REWARDED_HINT_ENABLED);

        /// <inheritdoc/>
        public bool DailyPuzzleEnabled =>
            GetBool("daily_puzzle_enabled", GameConstants.DEFAULT_DAILY_PUZZLE_ENABLED);

        /// <inheritdoc/>
        public float DifficultyCurveModifier =>
            GetFloat("difficulty_curve_modifier", 1.0f);

        /// <summary>
        /// Set an override value (useful for testing or local tuning).
        /// </summary>
        public void SetOverride(string key, object value)
        {
            _overrides[key] = value;
            Debug.Log($"[RemoteConfig] Override set: {key} = {value}");
        }
    }
}
