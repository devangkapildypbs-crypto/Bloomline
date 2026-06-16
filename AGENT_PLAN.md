# Bloomline — Agent Plan

## Project Overview
**Bloomline** is an Android-first 2D grid-based puzzle game built in Unity. Players rotate tiles to guide glowing light from source crystals to sleeping flowers. When the correct path connects, light travels through and flowers bloom.

## Architecture Summary

### Tech Stack
- **Engine**: Unity 2D (2022.3 LTS recommended)
- **Language**: C#
- **Target**: Android (Portrait, 1080×1920 reference)
- **Input**: Touch-first, mouse-compatible
- **Data**: JSON level files, local save via PlayerPrefs wrapper
- **Backend**: Interface-ready for Firebase (not required for MVP)

### Scene Flow
```
BootScene → MainMenuScene → LevelSelectScene → GameScene
                                                    ↓
                                              LevelCompletePopup → Next Level / LevelSelect
```

### Core Systems
1. **Grid & Tiles** — TileModel (data) + TileView (visual) on a BoardManager grid
2. **Light Solver** — BFS-based graph traversal from source tiles through connected openings
3. **Level Loader** — JSON deserialization into LevelData objects
4. **Input Controller** — Tap detection, rotation dispatch, undo stack
5. **Progression** — Save/load stars, unlock levels, track moves
6. **UI System** — Menu, level select, HUD, popups, tutorial bubbles
7. **Services Layer** — Analytics, RemoteConfig, Audio, Save, Ads (all interface-based)

## Folder Structure
```
Assets/
├── Scripts/
│   ├── Core/           — GameBootstrap, GameConstants, SceneLoader
│   ├── Gameplay/       — BoardManager, TileModel, TileView, LightSolver, InputController, etc.
│   ├── Levels/         — LevelData, LevelLoader, LevelValidator, LevelProgressionService
│   ├── UI/             — MainMenuUI, LevelSelectUI, GameHUD, LevelCompletePopup, etc.
│   ├── Services/       — SaveService, AnalyticsService, AudioService, etc.
│   ├── Data/           — PlayerProfile, LevelProgress, GameSettings
│   └── Utilities/      — AnimationHelper, ColorHelper
├── Art/
│   ├── Sprites/
│   ├── UI/
│   └── Effects/
├── Audio/
│   ├── SFX/
│   └── Music/
├── Resources/
│   └── Levels/         — 20 JSON level files
├── Prefabs/
│   ├── Tiles/
│   └── UI/
├── Scenes/             — Boot, MainMenu, LevelSelect, Game
└── Tests/
    └── EditMode/       — Unit tests for solver, rotation, loading
```

## Phased Execution Plan

### Phase 1: Planning & Architecture ✅
- Create AGENT_PLAN.md
- Define architecture and folder structure
- Create core enums, data models, interfaces

### Phase 2: Core Gameplay
- Implement TileModel with rotation and direction logic
- Implement BoardManager grid creation
- Implement LightSolver (BFS)
- Implement LevelData JSON deserialization
- Implement LevelLoader
- Implement InputController (tap-to-rotate)
- Implement MoveHistory (undo stack)
- Implement win detection

### Phase 3: UI & Progression
- Implement MainMenuUI
- Implement LevelSelectUI with lock/unlock and stars
- Implement GameHUD (moves, stars, buttons)
- Implement LevelCompletePopup
- Implement SaveService (PlayerPrefs wrapper)
- Implement LevelProgressionService
- Implement SceneLoader

### Phase 4: Level Design
- Create 20 handcrafted JSON levels
- Levels 1–3: basic straight connections
- Levels 4–6: corner paths
- Levels 7–9: move efficiency challenges
- Levels 10–12: blockers
- Levels 13–15: locked tiles
- Levels 16–18: multiple flowers
- Levels 19–20: colored mechanics
- Validate all levels with LevelValidator

### Phase 5: Art & Polish
- Create placeholder sprite assets (programmatic)
- Add tile rotation animation
- Add light flow glow effect
- Add flower bloom animation
- Add locked tile shake feedback
- Add UI transitions and polish

### Phase 6: Testing & Documentation
- Write unit tests for LightSolver, rotation, JSON loading
- Complete QA_CHECKLIST.md
- Complete BUILD_LOG.md
- Document known limitations and next steps

## Assumptions & Recommendations

1. **Unity Version**: Targeting Unity 2022.3 LTS. Scripts are compatible with 2021.3+.
2. **No External Packages**: Using only built-in Unity features. No DOTween — using coroutine-based tweening.
3. **Placeholder Art**: Creating colored geometric sprites programmatically (quads/circles with materials). Easy to replace with final art.
4. **Scene Setup**: Providing scene setup instructions since Unity scenes require editor interaction for full configuration. Scripts include auto-setup functionality.
5. **JSON Levels**: Stored in Resources/Levels/ for easy loading via Resources.Load.
6. **Light Solver**: Pure logic class with no MonoBehaviour dependency — fully testable.
7. **Color Support**: White is default/neutral. Red, Blue, Yellow introduced in levels 19–20. System supports arbitrary colors.
8. **Hint System**: MVP uses solution rotation data embedded in level JSON. Fallback highlights any incorrectly rotated tile.
9. **Sound**: AudioService with method stubs. Actual audio clips can be assigned in Unity Inspector.
10. **Android Build**: Standard Unity Android build pipeline. Instructions provided in BUILD_LOG.md.

## Risk Mitigations
- **Light Solver Correctness**: Unit tests validate basic, blocked, and colored paths
- **Level Solvability**: LevelValidator performs automated solve check on all 20 levels
- **Save Corruption**: SaveService uses JSON with try/catch and fallback to defaults
- **Performance**: Grid sizes are small (max 6×8 for MVP), no performance concerns expected
