// AdsServicePlaceholder.cs — Placeholder ads service for development
using System;
using UnityEngine;

namespace Bloomline.Services
{
    /// <summary>
    /// Interface for ad display (interstitials and rewarded ads).
    /// </summary>
    public interface IAdsService
    {
        /// <summary>
        /// Show an interstitial ad. Calls onComplete when the ad is closed.
        /// </summary>
        void ShowInterstitial(Action onComplete);

        /// <summary>
        /// Show a rewarded ad. Calls onResult with true if the reward was earned,
        /// false if the ad was skipped or failed.
        /// </summary>
        void ShowRewardedAd(Action<bool> onResult);

        /// <summary>
        /// Returns true if a rewarded ad is loaded and ready to show.
        /// </summary>
        bool IsRewardedAdReady { get; }

        /// <summary>
        /// Initialize the ads SDK.
        /// </summary>
        void Initialize();
    }

    /// <summary>
    /// Placeholder implementation of IAdsService for development.
    /// Logs ad requests and immediately invokes callbacks (no real ads).
    /// Replace with AdMob, Unity Ads, or IronSource for production.
    /// </summary>
    public class PlaceholderAdsService : IAdsService
    {
        /// <inheritdoc/>
        public bool IsRewardedAdReady => true;

        /// <inheritdoc/>
        public void Initialize()
        {
            Debug.Log("[Ads] Ads SDK initialized (placeholder)");
        }

        /// <inheritdoc/>
        public void ShowInterstitial(Action onComplete)
        {
            Debug.Log("[Ads] Interstitial ad requested (placeholder — skipping)");
            onComplete?.Invoke();
        }

        /// <inheritdoc/>
        public void ShowRewardedAd(Action<bool> onResult)
        {
            Debug.Log("[Ads] Rewarded ad requested (placeholder — granting reward)");
            onResult?.Invoke(true);
        }
    }
}
