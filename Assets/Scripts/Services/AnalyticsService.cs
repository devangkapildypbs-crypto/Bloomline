// AnalyticsService.cs — Analytics interface and debug logger implementation
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Bloomline.Services
{
    /// <summary>
    /// Interface for analytics event tracking.
    /// </summary>
    public interface IAnalyticsService
    {
        /// <summary>Log a generic named event with optional parameters.</summary>
        void LogEvent(string eventName, Dictionary<string, string> parameters = null);

        /// <summary>Log when the game is opened.</summary>
        void LogGameOpened();

        /// <summary>Log when a level is started.</summary>
        void LogLevelStarted(int levelNumber);

        /// <summary>Log when a level is completed with results.</summary>
        void LogLevelCompleted(int levelNumber, int stars, int moves);

        /// <summary>Log when a level is restarted.</summary>
        void LogLevelRestarted(int levelNumber);

        /// <summary>Log a move in a level.</summary>
        void LogMoveMade(int levelNumber);

        /// <summary>Log when undo is used.</summary>
        void LogUndoUsed(int levelNumber);

        /// <summary>Log when a hint is used.</summary>
        void LogHintUsed(int levelNumber);

        /// <summary>Log when settings are changed.</summary>
        void LogSettingsChanged();
    }

    /// <summary>
    /// Debug implementation of IAnalyticsService.
    /// All events are logged to the Unity console with an [Analytics] prefix.
    /// Replace with a real analytics SDK (Firebase, Unity Analytics, etc.) for production.
    /// </summary>
    public class DebugAnalyticsService : IAnalyticsService
    {
        /// <inheritdoc/>
        public void LogEvent(string eventName, Dictionary<string, string> parameters = null)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"[Analytics] {eventName}");

            if (parameters != null && parameters.Count > 0)
            {
                sb.Append(" { ");
                foreach (var kvp in parameters)
                {
                    sb.Append($"{kvp.Key}={kvp.Value}, ");
                }
                sb.Length -= 2; // Remove trailing ", "
                sb.Append(" }");
            }

            Debug.Log(sb.ToString());
        }

        /// <inheritdoc/>
        public void LogGameOpened()
        {
            LogEvent("game_opened");
        }

        /// <inheritdoc/>
        public void LogLevelStarted(int levelNumber)
        {
            LogEvent("level_started", new Dictionary<string, string>
            {
                { "level", levelNumber.ToString() }
            });
        }

        /// <inheritdoc/>
        public void LogLevelCompleted(int levelNumber, int stars, int moves)
        {
            LogEvent("level_completed", new Dictionary<string, string>
            {
                { "level", levelNumber.ToString() },
                { "stars", stars.ToString() },
                { "moves", moves.ToString() }
            });
        }

        /// <inheritdoc/>
        public void LogLevelRestarted(int levelNumber)
        {
            LogEvent("level_restarted", new Dictionary<string, string>
            {
                { "level", levelNumber.ToString() }
            });
        }

        /// <inheritdoc/>
        public void LogMoveMade(int levelNumber)
        {
            LogEvent("move_made", new Dictionary<string, string>
            {
                { "level", levelNumber.ToString() }
            });
        }

        /// <inheritdoc/>
        public void LogUndoUsed(int levelNumber)
        {
            LogEvent("undo_used", new Dictionary<string, string>
            {
                { "level", levelNumber.ToString() }
            });
        }

        /// <inheritdoc/>
        public void LogHintUsed(int levelNumber)
        {
            LogEvent("hint_used", new Dictionary<string, string>
            {
                { "level", levelNumber.ToString() }
            });
        }

        /// <inheritdoc/>
        public void LogSettingsChanged()
        {
            LogEvent("settings_changed");
        }
    }
}
