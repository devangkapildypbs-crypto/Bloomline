# 🌸 Bloomline — Puzzle Game

A beautiful 2D grid-based puzzle game where you rotate tiles to guide glowing light from source crystals to sleeping flowers. When the correct path connects, light flows through and flowers bloom!

## 🎮 Game Features
- **Tap-to-rotate** tile-based puzzle mechanics
- **20 handcrafted levels** with progressive difficulty
- **3-star scoring** system based on move efficiency
- **7 tile types**: Source, Flower, Straight, Corner, Blocker, LockedStraight, LockedCorner
- **Colored light** mechanics (Red, Blue, Yellow, White)
- **Undo, restart, and hint** systems
- **Local save** with progress tracking
- **Garden-themed** visual design

---

## 🚀 Quick Start (Unity Editor)

### Prerequisites
- **Unity 2022.3 LTS** (any 2022.3.x patch)
- Unity modules: Android Build Support (for Android builds)

### Setup Steps

1. **Open the project** in Unity Hub → Add → select this folder
2. **Wait for import** — Unity will compile all scripts
3. **Setup scenes** → Menu bar → `Bloomline > Setup All Scenes`
4. **Open BootScene** → Menu bar → `Bloomline > Open Boot Scene`
5. **Press Play** ▶️

The game will boot, show the main menu, and you can start playing!

### Quick Test
- From Main Menu → tap **Play** → tap **Level 1**
- Tap the middle tile to rotate it → light connects → flower blooms! 🌸

---

## 📱 Building for Android

1. **File > Build Settings** → select **Android** → **Switch Platform**
2. Verify all 4 scenes in the build list (BootScene must be index 0)
3. **Player Settings**:
   - Company: BloomlineStudios
   - Product: Bloomline
   - Package: com.bloomlinestudios.bloomline
   - Orientation: Portrait
   - Min API Level: Android 7.0 (API 24)
   - Scripting Backend: IL2CPP
4. Click **Build** → choose output .apk location

---

## 📁 Project Structure

```
Assets/
├── Scripts/
│   ├── Core/           GameBootstrap, GameConstants, SceneLoader, ServiceLocator
│   ├── Gameplay/       BoardManager, TileModel, TileView, LightSolver, InputController, etc.
│   ├── Levels/         LevelData, LevelLoader, LevelValidator, LevelProgressionService
│   ├── UI/             MainMenuUI, LevelSelectUI, GameHUD, LevelCompletePopup, etc.
│   ├── Services/       SaveService, AnalyticsService, AudioService, etc.
│   ├── Data/           PlayerProfile, LevelProgress, GameSettings
│   ├── Utilities/      AnimationHelper (coroutine-based tweening)
│   └── Editor/         BloomlineSceneSetup (editor utility)
├── Resources/
│   └── Levels/         20 JSON level files (level_001.json — level_020.json)
├── Tests/
│   └── EditMode/       Unit tests for solver, tiles, parsing, scoring
└── Scenes/             BootScene, MainMenuScene, LevelSelectScene, GameScene
```

---

## 🧩 Adding New Levels

1. Create a new JSON file in `Assets/Resources/Levels/` named `level_XXX.json`
2. Follow this structure:

```json
{
  "levelId": "level_021",
  "chapter": 4,
  "levelNumber": 21,
  "gridWidth": 5,
  "gridHeight": 5,
  "moveTargetTwoStars": 8,
  "moveTargetThreeStars": 5,
  "tutorialText": "",
  "tiles": [
    { "x": 0, "y": 2, "type": "Source", "rotation": 0, "color": "White", "locked": true },
    { "x": 1, "y": 2, "type": "Straight", "rotation": 0, "color": "White", "locked": false },
    { "x": 2, "y": 2, "type": "Flower", "rotation": 0, "color": "White", "locked": true }
  ],
  "solution": [
    { "x": 1, "y": 2, "correctRotation": 90 }
  ]
}
```

3. Update `TOTAL_LEVELS` in `GameConstants.cs` to include the new level count

### Tile Types
| Type | Rotation 0° Openings | Rotatable? |
|------|---------------------|------------|
| Source | East | No |
| Flower | West | No |
| Straight | North + South | Yes |
| Corner | North + East | Yes |
| Blocker | None | No |
| LockedStraight | North + South | No |
| LockedCorner | North + East | No |

### Colors
`White` (neutral), `Red`, `Blue`, `Yellow`

### Coordinate System
- `(0,0)` = bottom-left
- `x` increases rightward
- `y` increases upward

---

## 🔧 Architecture Notes

- **LightSolver** is a pure static class with no Unity dependencies — fully unit testable
- **TileModel** is a pure data class — view is separated in TileView
- **Services** use interface + implementation pattern for easy swapping (e.g., ISaveService → Firebase)
- **UI** is built programmatically — no prefab dependencies
- **Level data** is JSON-driven — new levels require zero code changes

---

## 📝 License
Placeholder — add your license here.
