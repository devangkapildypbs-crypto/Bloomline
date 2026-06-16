# Bloomline — QA Checklist

## Test Environment
- **Unity Version**: 2022.3 LTS
- **Platform**: Editor (Windows/Mac) + Android
- **Orientation**: Portrait (1080×1920 reference)

---

## Core Flow Tests

### 1. App Launch
- [ ] Open Unity project without compiler errors
- [ ] Run `Bloomline > Setup All Scenes` from menu bar
- [ ] Open BootScene and press Play
- [ ] BootScene initializes services (check console logs)
- [ ] Automatically transitions to MainMenuScene

### 2. Main Menu
- [ ] Game title "BLOOMLINE" is displayed
- [ ] "Play" button is visible and tappable
- [ ] "Level Select" button is visible and tappable
- [ ] "Settings" button is visible and tappable
- [ ] Tapping Play navigates to LevelSelectScene
- [ ] Tapping Level Select navigates to LevelSelectScene
- [ ] Background has deep forest green color

### 3. Level Select
- [ ] 20 level buttons are displayed in a grid
- [ ] Level 1 is unlocked (green/colored)
- [ ] Levels 2–20 start locked (gray)
- [ ] Locked levels show lock indicator
- [ ] Tapping an unlocked level loads GameScene
- [ ] Tapping a locked level does nothing / shows feedback
- [ ] Back button returns to MainMenu
- [ ] Star count displays correctly for completed levels

### 4. Gameplay — Basic
- [ ] Level 1 loads with correct grid layout
- [ ] Source tile is visible (cyan crystal)
- [ ] Flower tile is visible (purple bud)
- [ ] Straight tile is visible and looks different
- [ ] Tutorial text appears for Level 1
- [ ] Tap on rotatable tile rotates it 90° clockwise
- [ ] Rotation animates smoothly
- [ ] Move counter increments on each rotation
- [ ] Star indicator updates based on move count

### 5. Light Solver
- [ ] Light flows from Source through connected tiles
- [ ] Connected tiles glow when powered
- [ ] Disconnected tiles do NOT glow
- [ ] Flower blooms (color change + scale) when powered
- [ ] All flowers powered = level complete
- [ ] Partial flowers powered = NOT level complete

### 6. Level Completion
- [ ] Level complete popup appears after all flowers powered
- [ ] Stars are displayed correctly (1–3 based on moves)
- [ ] Move count is shown
- [ ] "Next Level" button works
- [ ] "Retry" button restarts the level
- [ ] "Level Select" button returns to level select

### 7. Tile Types
- [ ] Source: visible, locked, emits light in correct direction
- [ ] Flower: visible, locked, receives light, blooms when powered
- [ ] Straight: rotatable, connects opposite sides
- [ ] Corner: rotatable, connects two adjacent sides
- [ ] Blocker: no openings, cannot be rotated, dark appearance
- [ ] LockedStraight: connects like Straight but cannot rotate, shows lock indicator
- [ ] LockedCorner: connects like Corner but cannot rotate, shows lock indicator

### 8. Locked Tile Feedback
- [ ] Tapping a locked tile shows shake animation
- [ ] Tapping a blocker does nothing
- [ ] Tapping Source/Flower does nothing

### 9. Undo
- [ ] Undo button reverts the last rotation
- [ ] Tile visually returns to previous rotation
- [ ] Light paths update after undo
- [ ] Multiple undos work in sequence
- [ ] Undo on empty history does nothing

### 10. Restart
- [ ] Restart button resets all tiles to initial rotation
- [ ] Move counter resets to 0
- [ ] Light paths recalculate
- [ ] Level is playable again

### 11. Hint
- [ ] Hint button highlights a tile that needs rotation
- [ ] Highlighted tile pulses with golden glow
- [ ] Hint does NOT auto-solve the level

### 12. Scoring
- [ ] Level with ≤ moveTargetThreeStars moves → 3 stars
- [ ] Level with ≤ moveTargetTwoStars moves → 2 stars
- [ ] Level with > moveTargetTwoStars moves → 1 star
- [ ] Star count saves correctly

### 13. Progression & Save
- [ ] Completing Level 1 unlocks Level 2
- [ ] Star count saved after completion
- [ ] Close and reopen app (stop/start Play mode) → progress persisted
- [ ] Level Select shows correct stars and unlock state

---

## Level-Specific Tests

### Tutorial Levels (1–3)
- [ ] Level 1: 1 straight tile, solvable in 1 move
- [ ] Level 2: 2 straight tiles, solvable in 2 moves
- [ ] Level 3: Mix of straights, solvable as designed

### Corner Levels (4–6)
- [ ] Level 4: First corner tile introduced
- [ ] Corner tiles connect two adjacent sides correctly
- [ ] Paths can turn around corners

### Blocker Levels (10–12)
- [ ] Blocker tiles block light completely
- [ ] Player must route around blockers
- [ ] Blockers cannot be rotated

### Locked Tile Levels (13–15)
- [ ] LockedStraight/LockedCorner tiles visible with lock indicator
- [ ] Locked tiles carry light but cannot be rotated
- [ ] Player works around locked constraints

### Multiple Flower Levels (16–18)
- [ ] All flowers on the level must be powered
- [ ] Level is NOT complete until ALL flowers bloom

### Colored Levels (19–20)
- [ ] Red source emits red light
- [ ] Blue source emits blue light
- [ ] Red flower only accepts red light
- [ ] Blue flower only accepts blue light
- [ ] White tiles pass any color light through
- [ ] Color-coded visual difference in powered glow

---

## UI Polish Tests
- [ ] All text is readable at mobile size
- [ ] Buttons are large enough for touch input
- [ ] No UI elements overlap or go off screen
- [ ] Canvas scales correctly at different resolutions
- [ ] Popup animations play smoothly
- [ ] Star animations play one-by-one in completion popup

---

## Analytics Tests (Console Logs)
- [ ] `[Analytics] game_opened` on launch
- [ ] `[Analytics] level_started` when entering a level
- [ ] `[Analytics] move_made` on each rotation
- [ ] `[Analytics] undo_used` on undo
- [ ] `[Analytics] hint_used` on hint
- [ ] `[Analytics] level_completed` with stars and moves
- [ ] `[Analytics] level_restarted` on restart

---

## Edge Cases
- [ ] Rapidly tapping a tile during rotation animation
- [ ] Tapping empty grid spaces (no crash)
- [ ] Completing the last level (Level 20)
- [ ] Replaying an already-completed level
- [ ] Getting 1 star then replaying for 3 stars (best is saved)
