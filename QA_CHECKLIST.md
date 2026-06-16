# Bloomline — QA Checklist

## Test Environment
- **Unity Version**: 2022.3 LTS
- **Platform**: Editor (Windows/Mac) + Android
- **Orientation**: Portrait (1080×1920 reference)

---

## Core Flow Tests

### 1. App Launch
- [x] Open Unity project without compiler errors
- [x] Run `Bloomline > Setup All Scenes` from menu bar
- [x] Open BootScene and press Play
- [x] BootScene initializes services (check console logs)
- [x] Automatically transitions to MainMenuScene

### 2. Main Menu
- [x] Game title "BLOOMLINE" is displayed
- [x] "Play" button is visible and tappable
- [x] "Level Select" button is visible and tappable
- [x] "Settings" button is visible and tappable
- [x] Tapping Play navigates to LevelSelectScene
- [x] Tapping Level Select navigates to LevelSelectScene
- [x] Background has deep forest green color

### 3. Level Select
- [x] 20 level buttons are displayed in a grid
- [x] Level 1 is unlocked (green/colored)
- [x] Levels 2–20 start locked (gray)
- [x] Locked levels show lock indicator
- [x] Tapping an unlocked level loads GameScene
- [x] Tapping a locked level does nothing / shows feedback
- [x] Back button returns to MainMenu
- [x] Star count displays correctly for completed levels

### 4. Gameplay — Basic
- [x] Level 1 loads with correct grid layout
- [x] Source tile is visible (cyan crystal)
- [x] Flower tile is visible (purple bud)
- [x] Straight tile is visible and looks different
- [x] Tutorial text appears for Level 1
- [x] Tap on rotatable tile rotates it 90° clockwise
- [x] Rotation animates smoothly
- [x] Move counter increments on each rotation
- [x] Star indicator updates based on move count

### 5. Light Solver
- [x] Light flows from Source through connected tiles
- [x] Connected tiles glow when powered
- [x] Disconnected tiles do NOT glow
- [x] Flower blooms (color change + scale) when powered
- [x] All flowers powered = level complete
- [x] Partial flowers powered = NOT level complete

### 6. Level Completion
- [x] Level complete popup appears after all flowers powered
- [x] Stars are displayed correctly (1–3 based on moves)
- [x] Move count is shown
- [x] "Next Level" button works
- [x] "Retry" button restarts the level
- [x] "Level Select" button returns to level select

### 7. Tile Types
- [x] Source: visible, locked, emits light in correct direction
- [x] Flower: visible, locked, receives light, blooms when powered
- [x] Straight: rotatable, connects opposite sides
- [x] Corner: rotatable, connects two adjacent sides
- [x] Blocker: no openings, cannot be rotated, dark appearance
- [x] LockedStraight: connects like Straight but cannot rotate, shows lock indicator
- [x] LockedCorner: connects like Corner but cannot rotate, shows lock indicator

### 8. Locked Tile Feedback
- [x] Tapping a locked tile shows shake animation
- [x] Tapping a blocker does nothing
- [x] Tapping Source/Flower does nothing

### 9. Undo
- [x] Undo button reverts the last rotation
- [x] Tile visually returns to previous rotation
- [x] Light paths update after undo
- [x] Multiple undos work in sequence
- [x] Undo on empty history does nothing

### 10. Restart
- [x] Restart button resets all tiles to initial rotation
- [x] Move counter resets to 0
- [x] Light paths recalculate
- [x] Level is playable again

### 11. Hint
- [x] Hint button highlights a tile that needs rotation
- [x] Highlighted tile pulses with golden glow
- [x] Hint does NOT auto-solve the level

### 12. Scoring
- [x] Level with ≤ moveTargetThreeStars moves → 3 stars
- [x] Level with ≤ moveTargetTwoStars moves → 2 stars
- [x] Level with > moveTargetTwoStars moves → 1 star
- [x] Star count saves correctly

### 13. Progression & Save
- [x] Completing Level 1 unlocks Level 2
- [x] Star count saved after completion
- [x] Close and reopen app (stop/start Play mode) → progress persisted
- [x] Level Select shows correct stars and unlock state

---

## Level-Specific Tests

### Tutorial Levels (1–3)
- [x] Level 1: 1 straight tile, solvable in 1 move
- [x] Level 2: 2 straight tiles, solvable in 2 moves
- [x] Level 3: Mix of straights, solvable as designed

### Corner Levels (4–6)
- [x] Level 4: First corner tile introduced
- [x] Corner tiles connect two adjacent sides correctly
- [x] Paths can turn around corners

### Blocker Levels (10–12)
- [x] Blocker tiles block light completely
- [x] Player must route around blockers
- [x] Blockers cannot be rotated

### Locked Tile Levels (13–15)
- [x] LockedStraight/LockedCorner tiles visible with lock indicator
- [x] Locked tiles carry light but cannot be rotated
- [x] Player works around locked constraints

### Multiple Flower Levels (16–18)
- [x] All flowers on the level must be powered
- [x] Level is NOT complete until ALL flowers bloom

### Colored Levels (19–20)
- [x] Red source emits red light
- [x] Blue source emits blue light
- [x] Red flower only accepts red light
- [x] Blue flower only accepts blue light
- [x] White tiles pass any color light through
- [x] Color-coded visual difference in powered glow

---

## UI Polish Tests
- [x] All text is readable at mobile size
- [x] Buttons are large enough for touch input
- [x] No UI elements overlap or go off screen
- [x] Canvas scales correctly at different resolutions
- [x] Popup animations play smoothly
- [x] Star animations play one-by-one in completion popup

---

## Analytics Tests (Console Logs)
- [x] `[Analytics] game_opened` on launch
- [x] `[Analytics] level_started` when entering a level
- [x] `[Analytics] move_made` on each rotation
- [x] `[Analytics] undo_used` on undo
- [x] `[Analytics] hint_used` on hint
- [x] `[Analytics] level_completed` with stars and moves
- [x] `[Analytics] level_restarted` on restart

---

## Edge Cases
- [x] Rapidly tapping a tile during rotation animation
- [x] Tapping empty grid spaces (no crash)
- [x] Completing the last level (Level 20)
- [x] Replaying an already-completed level
- [x] Getting 1 star then replaying for 3 stars (best is saved)
