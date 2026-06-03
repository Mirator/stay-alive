# 14. SPEC-014: Save Slots And Menus

Status: Implemented

## Purpose

Define the wilderness build menu and manual save/load system.

## Design Goal

The player should be able to keep sandbox progress across sessions using a small, predictable save-slot model.

## Requirements

### 14.1 SAVE-001: Three Manual Slots

The wilderness build MUST support exactly 3 manual save slots.

Acceptance criteria:

- The main menu displays 3 save slots.
- Each slot displays either empty state or save metadata.
- Each slot exposes New and Load actions.
- Load is disabled or unavailable for empty slots.
- Empty slots can start a new game.
- Occupied slots can be loaded.
- Starting a new game in an occupied slot requires overwrite confirmation.
- Starting or overwriting a slot resets the playable scene to the initial Day 1 wilderness state.
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

- Death UI offers Load Save.
- Death UI offers Return To Menu.
- Loading from death uses the selected existing slot.
- Death does not delete or overwrite save data.

### 14.6 SAVE-006: In-Game Pause Menu

The wilderness build MUST include a simple in-game menu during play.

Acceptance criteria:

- `Esc` opens the pause menu during gameplay.
- The pause menu pauses simulation.
- The pause menu offers Resume.
- The pause menu offers Return To Main Menu.
- Returning to main menu shows the 3 save slots without deleting save data.

## Non-Goals

- No autosave.
- No cloud saves.
- No unlimited save browser.
- No cross-version migration requirement in wilderness build.

## Public Interfaces

The implementation SHOULD expose this conceptual persistence contract:

- `SaveSlotData`: slot metadata plus player position, vitals, inventory, day/night state, placed buildings, harvested node state, defeated enemy state, and map state.
- `MainMenuController`: main menu visibility, pause menu visibility, overwrite confirmation state, slot view refresh, new game, load game, resume, and return-to-menu actions.

## Dependencies

- SPEC-010 Health, Hunger, Day/Night, And Death.
- SPEC-013 Grid Building.
- SPEC-019 Game Menu Flow And States.

## Verification

Automated wilderness build tests verify:

- The menu exposes exactly 3 slots.
- The menu displays slot metadata and empty slot state.
- Occupied-slot New requires overwrite confirmation.
- Starting a New Game resets player position, vitals, day/time, inventory, placed buildings, harvested nodes, wildlife, and onboarding progress.
- The pause menu can resume or return to main menu.
- Saving writes slot metadata and full game state.
- Loading restores player, inventory, vitals, day/night, buildings, harvested nodes, and defeated enemies.
- Death UI can load a save or return to menu.
