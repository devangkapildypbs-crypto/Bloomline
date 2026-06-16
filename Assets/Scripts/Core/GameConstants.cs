// GameConstants.cs — Central constants for Bloomline
namespace Bloomline.Core
{
    public static class GameConstants
    {
        // Scene names
        public const string SCENE_BOOT = "BootScene";
        public const string SCENE_MAIN_MENU = "MainMenuScene";
        public const string SCENE_LEVEL_SELECT = "LevelSelectScene";
        public const string SCENE_GAME = "GameScene";

        // Game settings
        public const int TOTAL_LEVELS = 20;
        public const int MAX_STARS = 3;
        public const float TILE_SIZE = 1.0f;
        public const float TILE_SPACING = 0.05f;
        public const float ROTATION_DURATION = 0.15f;
        public const float LIGHT_FLOW_SPEED = 8.0f;
        public const float BLOOM_DURATION = 0.5f;

        // Save keys
        public const string SAVE_KEY_PROFILE = "bloomline_player_profile";
        public const string SAVE_KEY_SETTINGS = "bloomline_settings";

        // Layers
        public const string LAYER_TILES = "Tiles";
        public const string LAYER_EFFECTS = "Effects";
        public const string LAYER_UI = "UI";

        // Animation
        public const float SHAKE_DURATION = 0.3f;
        public const float SHAKE_MAGNITUDE = 0.05f;
        public const float HINT_PULSE_DURATION = 0.8f;
        public const float CELEBRATION_DURATION = 1.5f;

        // Remote config defaults
        public const bool DEFAULT_HINT_ENABLED = true;
        public const int DEFAULT_MAX_FREE_HINTS = 999;
        public const int DEFAULT_INTERSTITIAL_FREQUENCY = 0;
        public const bool DEFAULT_REWARDED_HINT_ENABLED = false;
        public const bool DEFAULT_DAILY_PUZZLE_ENABLED = false;

        // Level paths
        public const string LEVEL_RESOURCE_PATH = "Levels/";
    }
}
