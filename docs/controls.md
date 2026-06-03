# 20. SPEC-020: Wilderness Controls

Status: Implemented

## Purpose

Define the current keyboard and mouse controls for the wilderness build.

## Design Goal

The player should be able to move, fight, gather, craft, consume survival items, build shelter, save, and use menus with a small readable control set.

## Requirements

### 20.1 CTRL-001: Movement And Aim

The wilderness build MUST support keyboard movement and mouse aim.

Acceptance criteria:

- `WASD` moves the player.
- Mouse position sets the aim direction for attacks and tool actions.
- Movement remains responsive during normal gameplay.
- Movement and actions stop while title, save-slot, pause, overwrite, or death menus are active.

### 20.2 CTRL-002: Primary Action And Combat

The primary mouse action MUST support melee survival combat.

Acceptance criteria:

- `Left Click` performs the primary attack or tool action toward the mouse aim direction.
- If the player has a `Stone Spear`, the primary attack uses spear damage and range.
- If the player has no spear, the primary attack still performs a weak close-range swing.
- Primary action feedback is shown when the player swings or hits wildlife.

### 20.3 CTRL-003: Context Interaction, Gathering, Crafting, Saving, And Storage

The wilderness build MUST use one contextual interaction key for nearby world objects.

Acceptance criteria:

- `E` interacts with the nearest valid interactable in range.
- `E` gathers wilderness resource nodes such as logs, rocks, food bushes, and herbs.
- `E` crafts or advances recipes at a Workbench.
- `E` saves at a placed Campfire or Bedroll.
- `E` inspects Storage Box objects.
- Context prompts show the input, action, target, and disabled reason when relevant.

### 20.4 CTRL-004: Survival Item Consumption

The wilderness build MUST expose direct controls for urgent survival item use.

Acceptance criteria:

- `F` consumes one `Food` item when available.
- `H` consumes one `Bandage` when available.
- Food restores hunger according to SPEC-010.
- Bandage restores health according to SPEC-010.
- Missing item attempts show feedback and do not change state.

### 20.5 CTRL-005: Building Selection And Placement

The wilderness build MUST expose keyboard selection and mouse placement for grid building.

Required bindings:

| Input | Build Piece |
| --- | --- |
| `1` | Floor |
| `2` | Wall |
| `3` | Door |
| `4` | Campfire |
| `5` | Bedroll |
| `6` | Storage Box |
| `7` | Workbench |

Acceptance criteria:

- Number keys `1` through `7` select the corresponding build piece.
- Selecting a build piece shows build preview, cost, and validity.
- `Right Click` places the selected build piece at the snapped grid position.
- Placement only consumes resources or crafted buildable counts on success.
- Invalid placement shows feedback and does not consume resources.

### 20.6 CTRL-006: Building Rotation And Cancel

The wilderness build MUST support explicit rotation and cancel actions while building.

Acceptance criteria:

- `R` rotates the selected Door preview.
- `R` has no required effect for non-door build pieces.
- `X` cancels the active build selection.
- Cancel hides the build preview and does not consume resources.

### 20.7 CTRL-007: Pause And Menu Controls

The wilderness build MUST support pause and menu access without exposing save slots immediately on title.

Acceptance criteria:

- `Esc` opens the pause menu during active gameplay.
- `Esc` closes the pause menu and resumes gameplay.
- `Esc` does not dismiss the title menu or death menu into uncontrolled gameplay.
- Title menu, save-slot selection, overwrite confirmation, pause menu, and death menu behavior follows SPEC-019.

## Non-Goals

- No controller support.
- No touch controls.
- No key rebinding UI.
- No hotbar inventory.
- No required torch-placement binding in the current wilderness loop.
- No dedicated storage inventory UI beyond Storage Box inspection.

## Public Interfaces

The implementation SHOULD expose or use these control-facing systems:

- `PlayerController2D`: `WASD` movement state and last move direction.
- `PlayerInteraction`: primary action, contextual `E` interaction, food use, bandage use, and pause toggle.
- `MeleeWeapon`: primary melee attack contract.
- `GridBuildingSystem`: build selection, placement preview, right-click placement, door rotation, and cancel.
- `InteractionPromptData`: visible input/action/target prompt contract.
- `MainMenuController`: pause and menu state ownership.

## Dependencies

- SPEC-010 Health, Hunger, Day/Night, And Death.
- SPEC-011 Melee Combat And Wildlife.
- SPEC-012 Gathering, Crafting, And Inventory.
- SPEC-013 Grid Building.
- SPEC-014 Save Slots And Menus.
- SPEC-017 HUD, Interaction, Crafting, And Building UX.
- SPEC-019 Game Menu Flow And States.

## Verification

Automated tests verify:

- The active spec index lists `SPEC-020` as implemented.
- The controls spec documents movement, attack, context interaction, food, bandage, building, placement, rotation, cancel, save, and pause controls.
- The pause/help overlay lists the current wilderness control set.
- Context prompts use `E` for gather, craft, and save actions.
- Build preview uses `Right Click`, `R`, and `X` guidance.
