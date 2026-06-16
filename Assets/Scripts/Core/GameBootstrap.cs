// GameBootstrap.cs — Initializes all services and transitions to main menu
using UnityEngine;
using Bloomline.Services;
using Bloomline.Levels;

namespace Bloomline.Core
{
    /// <summary>
    /// Entry point for the game. Placed in BootScene.
    /// Initializes all services, loads player data, then moves to main menu.
    /// </summary>
    public class GameBootstrap : MonoBehaviour
    {
        private void Awake()
        {
            // Ensure this is the only bootstrap
            DontDestroyOnLoad(gameObject);
            gameObject.name = "GameBootstrap";

            InitializeServices();
        }

        private void Start()
        {
            // Load player profile
            var saveService = ServiceLocator.Get<ISaveService>();
            if (saveService != null)
            {
                var profile = saveService.LoadProfile();
                ServiceLocator.Register(profile);

                var settings = saveService.LoadSettings();
                ServiceLocator.Register(settings);
            }

            // Log game opened
            var analytics = ServiceLocator.Get<IAnalyticsService>();
            if (analytics != null)
            {
                analytics.LogGameOpened();
            }

            // Go to main menu
            SceneLoader.LoadScene(GameConstants.SCENE_MAIN_MENU);
        }

        private void InitializeServices()
        {
            // Save Service
            var saveService = new SaveService();
            ServiceLocator.Register<ISaveService>(saveService);

            // Analytics Service
            var analyticsService = new DebugAnalyticsService();
            ServiceLocator.Register<IAnalyticsService>(analyticsService);

            // Remote Config Service
            var remoteConfig = new LocalRemoteConfigService();
            ServiceLocator.Register<IRemoteConfigService>(remoteConfig);

            // Haptics Service
            var haptics = new HapticsService();
            ServiceLocator.Register<IHapticsService>(haptics);

            // Ads Service (placeholder)
            var ads = new PlaceholderAdsService();
            ServiceLocator.Register<IAdsService>(ads);

            // Audio Service — needs a MonoBehaviour
            var audioObj = new GameObject("AudioService");
            audioObj.transform.SetParent(transform);
            var audioService = audioObj.AddComponent<AudioService>();
            ServiceLocator.Register<IAudioService>(audioService);

            // Level Progression Service
            var progression = new LevelProgressionService(saveService);
            ServiceLocator.Register<LevelProgressionService>(progression);

            Debug.Log("[GameBootstrap] All services initialized.");
        }
    }
}
