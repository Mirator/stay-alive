# 19. SPEC-019: Game Menu Flow And States

Status: Implemented

## Purpose

Define the runtime game menu contract for Stay Alive, including title-first navigation, save-slot selection, pause, overwrite confirmation, and death recovery.

## Design Goal

The player should first land on a clear title menu, then choose whether to continue, start a new game, or load an existing save. Save-slot selection should appear only after the player chooses a slot-driven action.

## Requirements

### 19.1 MENU-001: Menu State Ownership

The game MUST use one menu controller as the authority for menu visibility and pause state.

Acceptance criteria:

- `MainMenuController` owns title menu, save-slot selection, pause menu, overwrite confirmation, and death menu state.
- Gameplay simulation is paused when the main menu, pause menu, overwrite confirmation, or death menu is visible.
- Gameplay simulation resumes only after a successful New Game, successful Load, or Resume action.
- Menu state changes hide stale panels from earlier states.
- `Esc` must not dismiss the main menu or death menu into uncontrolled gameplay.

### 19.2 MENU-002: Title Menu

The title menu MUST be the first visible UI state when the scene starts.

Acceptance criteria:

- The title menu displays the game title.
- The title menu displays Continue, New Game, and Load Game actions.
- Continue loads the most recent occupied save slot and enters gameplay.
- Continue is disabled or fails with feedback when no occupied slot exists.
- New Game opens save-slot selection in New Game mode.
- Load Game opens save-slot selection in Load Game mode.
- Save slots are not shown on the initial title menu.

### 19.3 MENU-003: Save-Slot Selection

Save-slot selection MUST be shown only after choosing New Game or Load Game from the title menu.

Acceptance criteria:

- Save-slot selection displays exactly 3 manual save slots.
- Each slot displays either empty state or save metadata.
- New Game mode exposes a New action for each slot.
- New on an empty slot starts a fresh Day 1 wilderness game in that slot.
- New on an occupied slot opens overwrite confirmation instead of immediately deleting data.
- Load Game mode exposes a Load action for each slot.
- Load is disabled for empty slots.
- Load on an occupied slot restores that slot and enters gameplay.
- Save-slot selection includes Back to return to the title menu.

### 19.4 MENU-004: Overwrite Confirmation

Starting a new game in an occupied slot MUST require explicit confirmation.

Acceptance criteria:

- The confirmation panel names the pending slot.
- The confirmation panel explains that starting a new game deletes that slot.
- Confirm overwrites the slot, resets the world to Day 1, and enters gameplay.
- Cancel closes confirmation without changing save data.
- The pending overwrite slot is cleared after confirm or cancel.
- Cancel returns to New Game save-slot selection.

### 19.5 MENU-005: Pause Menu

The in-game pause menu MUST be available during active gameplay.

Acceptance criteria:

- `Esc` opens the pause menu during gameplay.
- `Esc` closes the pause menu and resumes gameplay.
- The pause menu includes Resume.
- The pause menu includes Return To Main Menu.
- Return To Main Menu shows the title menu without deleting save data.
- The pause menu includes the current keyboard/mouse controls.

### 19.6 MENU-006: Death Menu

Death recovery MUST be a menu state, not just a text message.

Acceptance criteria:

- Health reaching zero shows the death menu.
- The death menu pauses gameplay.
- The death menu includes Load Current Slot.
- The death menu includes Return To Main Menu.
- Loading the current slot restores a valid save and resumes gameplay.
- Failed load attempts keep the death menu visible and show feedback.
- Returning to main menu from death shows the title menu and does not delete or overwrite save data.

### 19.7 MENU-007: Menu Feedback And Slot Refresh

Menu actions MUST keep visible state and slot metadata current.

Acceptance criteria:

- Starting a new game refreshes slot summaries.
- Saving refreshes slot summaries.
- Loading refreshes slot summaries.
- Empty-slot Load buttons are disabled.
- Continue availability is refreshed when save slots change.
- Messages are shown for Start, Continue, Load, failed Load, Pause, Resume, Back, and overwrite actions.
- Death UI is hidden after New Game, successful Load, Resume, or Return To Main Menu.

## Non-Goals

- No controller, touch, or localization support yet.
- No animated menu transitions.
- No settings screen.
- No autosave or cloud save behavior.
- No multi-scene campaign menu.

## Public Interfaces

The implementation MUST expose these runtime interfaces:

- `MainMenuController`: authoritative menu state and actions.
- `MainMenuController.MainMenuVisible`: main menu state.
- `MainMenuController.TitleMenuVisible`: title menu sub-state.
- `MainMenuController.SlotSelectionVisible`: save-slot selection sub-state.
- `MainMenuController.SlotSelectionMode`: current save-slot action mode.
- `MainMenuController.PauseMenuVisible`: pause menu state.
- `MainMenuController.OverwriteConfirmationVisible`: overwrite confirmation state.
- `MainMenuController.DeathMenuVisible`: death menu state.
- `MainMenuController.ContinueGame()`: loads the most recent occupied save slot.
- `MainMenuController.OpenNewGameSlots()`: opens New Game slot selection.
- `MainMenuController.OpenLoadGameSlots()`: opens Load Game slot selection.
- `MainMenuController.BackToTitleMenu()`: returns from slot selection to the title menu.
- `MainMenuController.RequestNewGame(slot)`: starts empty slots or requests overwrite.
- `MainMenuController.ConfirmOverwrite()`: confirms pending overwrite.
- `MainMenuController.CancelOverwrite()`: cancels pending overwrite.
- `MainMenuController.LoadSlot(slot)`: loads a specific occupied slot.
- `MainMenuController.LoadActiveSlot()`: loads the current active slot from death recovery.
- `MainMenuController.ShowPauseMenu()`: pauses gameplay with controls/help.
- `MainMenuController.ShowDeathMenu()`: pauses gameplay with death recovery actions.
- `GameMenuButtonAction`: maps generated UI buttons to menu actions.
- `SaveSlotViewData`: supplies slot summaries for menu display.

## Dependencies

- SPEC-014 Save Slots And Menus.
- SPEC-016 First-Session Onboarding And Guidance.
- SPEC-017 HUD, Interaction, Crafting, And Building UX.
- SPEC-018 Feedback, Readability, And Accessibility.

## Verification

Automated tests verify:

- The spec index lists `SPEC-019` as implemented.
- The generated scene contains title menu, save-slot selection, pause menu, overwrite confirmation, and death menu buttons.
- The menu exposes exactly 3 slots.
- The first visible menu state is the title menu, not save-slot selection.
- Continue loads the most recent occupied slot when one exists.
- New Game opens New Game save-slot selection.
- Load Game opens Load Game save-slot selection.
- Back returns from save-slot selection to the title menu.
- Empty-slot Load is disabled.
- New Game starts from an empty slot and hides menus.
- Occupied-slot New requires confirmation.
- Confirm overwrite resets the world and resumes gameplay.
- Cancel overwrite keeps the main menu and save data intact.
- Pause/Resume toggles pause state.
- Return To Main Menu shows the title menu.
- Death shows a recoverable menu state.
- Load Current Slot restores a valid save and resumes gameplay.
