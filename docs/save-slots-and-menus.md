# 14. SPEC-014: Save Slots And Menus

Status: Implemented

## Purpose

Define the MVP menu and manual save/load system.

## Design Goal

The player should be able to keep sandbox progress across sessions using a small, predictable save-slot model.

## Requirements

### 14.1 SAVE-001: Three Manual Slots

The MVP MUST support exactly 3 manual save slots.

Acceptance criteria:

- The main menu displays 3 save slots.
- Empty slots can start a new game.
- Occupied slots can be loaded.
- Starting a new game in an occupied slot requires overwrite confirmation.
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

## Non-Goals

- No autosave.
- No cloud saves.
- No unlimited save browser.
- No cross-version migration requirement in MVP.

## Public Interfaces

The implementation SHOULD expose this conceptual persistence contract:

- `SaveSlotData`: slot metadata plus player position, vitals, inventory, day/night state, placed buildings, harvested node state, defeated enemy state, and map state.

## Dependencies

- SPEC-010 Health, Hunger, Day/Night, And Death.
- SPEC-013 Grid Building.

## Verification

Automated MVP tests verify:

- The menu exposes exactly 3 slots.
- Saving writes slot metadata and full game state.
- Loading restores player, inventory, vitals, day/night, buildings, harvested nodes, and defeated enemies.
- Death UI can load a save or return to menu.
