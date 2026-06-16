// HapticsService.cs — Haptic feedback interface and stub implementation
using UnityEngine;

namespace Bloomline.Services
{
    /// <summary>
    /// Interface for haptic feedback on supported devices.
    /// </summary>
    public interface IHapticsService
    {
        /// <summary>Trigger light haptic feedback (e.g. tile tap).</summary>
        void LightHaptic();

        /// <summary>Trigger medium haptic feedback (e.g. connection made).</summary>
        void MediumHaptic();

        /// <summary>Trigger heavy haptic feedback (e.g. level complete).</summary>
        void HeavyHaptic();

        /// <summary>Enable or disable haptics globally.</summary>
        bool Enabled { get; set; }
    }

    /// <summary>
    /// Stub implementation of IHapticsService.
    /// Logs haptic events to the console. Replace with platform-specific
    /// haptics (iOS Taptic Engine, Android vibration) for production.
    /// </summary>
    public class HapticsService : IHapticsService
    {
        /// <inheritdoc/>
        public bool Enabled { get; set; } = true;

        /// <inheritdoc/>
        public void LightHaptic()
        {
            if (!Enabled) return;
            Debug.Log("[Haptics] Light haptic triggered");
        }

        /// <inheritdoc/>
        public void MediumHaptic()
        {
            if (!Enabled) return;
            Debug.Log("[Haptics] Medium haptic triggered");
        }

        /// <inheritdoc/>
        public void HeavyHaptic()
        {
            if (!Enabled) return;
            Debug.Log("[Haptics] Heavy haptic triggered");
        }
    }
}
