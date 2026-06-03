# 14. SPEC-014: Save Slots And Menus

Status: Implemented

## Purpose

Define the wilderness build save-slot and manual save/load system. Menu navigation MUST follow the title-first flow in SPEC-019.

## Design Goal

The player should be able to keep sandbox progress across sessions using a small, predictable save-slot model without showing save slots immediately on the title menu.

## Requirements

### 14.1 SAVE-001: Three Manual Slots

The wilderness build MUST support exactly 3 manual save slots. Slot selection MUST be reached through the title menu flow defined by SPEC-019.

Required flow:

```text
Title Menu
Continue
New Game -> Slot Selection -> New / Overwrite
Load Game -> Slot Selection -> Load
```

Acceptance criteria:

- The first visible menu state is the title menu.
- The title menu displays Continue, New Game, and Load Game actions.
- Save slots are not shown on the initial title menu.
- New Game opens save-slot selection in New Game mode.
- Load Game opens save-slot selection in Load Game mode.
- Save-slot selection displays 3 save slots.
- Each slot displays either empty state or save metadata.
- In New Game mode, each slot exposes a New action.
- In Load Game mode, each slot exposes a Load action.
- Load is disabled or unavailable for empty slots.
- Empty slots can start a new game.
- Occupied slots can be loaded.
- Starting a new game in an occupied slot requires overwrite confirmation.
- Starting or overwriting a slot resets the playable scene to the initial Day 1 wilderness state.
- Back from save-slot selection returns to the title menu.
- There is no autosave requirement.

### 14.2 SAVE-002: Save Access

Saving MUST be manual and available only at safe objects.

Safe save objects:

- Placed Campfire.
- Placed Bedroll.

Acceptance criteria:

- The player can save when near a valid safe save object.
- The player cannot save from arbitrary wilderness positions.
- Saving while dead is not allowed.

### 14.3 SAVE-003: Save Data Contents

Save data MUST include enough state to restore a playable sandbox.

Required save data:

- Slot metadata: slot number, display name, day, time, timestamp.
- Player position.
- Health and hunger.
- Inventory resources, tools, consumables, and buildables.
- Day/night state.
- Placed buildings.
- Harvested resource node state.
- Defeated enemy state.
- Current map state required for replay continuity.

### 14.4 SAVE-004: Load Behavior

Loading MUST restore the selected slot into a playable scene.

Acceptance criteria:

- Player position restores.
- Health and hunger restore.
- Inventory restores.
- Day/night state restores.
- Placed buildings restore.
- Harvested nodes stay harvested.
- Defeated enemies stay defeated.

### 14.5 SAVE-005: Death Menu Integration

Death UI MUST support save-slot recovery.

Acceptance criteria:

- Death UI offers Load Current Slot.
- Death UI offers Return To Main Menu.
- Loading from death uses the current active slot.
- Failed death-menu load attempts keep the death menu visible and show feedback.
- Death does not delete or overwrite save data.

### 14.6 SAVE-006: In-Game Pause Menu

The wilderness build MUST include a simple in-game menu during play.

Acceptance criteria:

- `Esc` opens the pause menu during gameplay.
- The pause menu pauses simulation.
- The pause menu offers Resume.
- The pause menu offers Return To Main Menu.
- Returning to main menu shows the title menu without deleting save data.

## Non-Goals

- No autosave.
- No cloud saves.
- No unlimited save browser.
- No cross-version migration requirement in wilderness build.

## Public Interfaces

The implementation SHOULD expose this conceptual persistence contract:

- `SaveSlotData`: slot metadata plus player position, vitals, inventory, day/night state, placed buildings, harvested node state, defeated enemy state, and map state.
- `MainMenuController`: title menu visibility, slot-selection visibility, slot-selection mode, pause menu visibility, overwrite confirmation state, slot view refresh, continue, new game, load game, back-to-title, resume, and return-to-menu actions.

## Dependencies

- SPEC-010 Health, Hunger, Day/Night, And Death.
- SPEC-013 Grid Building.
- SPEC-019 Game Menu Flow And States.

## Verification

Automated wilderness build tests verify:

- The first visible menu state is the title menu, not save-slot selection.
- New Game opens New Game slot selection.
- Load Game opens Load Game slot selection.
- Back returns from save-slot selection to the title menu.
- The slot-selection view exposes exactly 3 slots.
- The menu displays slot metadata and empty slot state.
- Empty-slot Load is disabled.
- Occupied-slot New requires overwrite confirmation.
- Starting a New Game resets player position, vitals, day/time, inventory, placed buildings, harvested nodes, wildlife, and onboarding progress.
- The pause menu can resume or return to the title menu.
- Saving writes slot metadata and full game state.
- Loading restores player, inventory, vitals, day/night, buildings, harvested nodes, and defeated enemies.
- Death UI can load a save or return to the title menu.
