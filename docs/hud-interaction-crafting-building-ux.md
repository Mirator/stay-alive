# 17. SPEC-017: HUD, Interaction, Crafting, And Building UX

Status: Implemented

## Purpose

Define the UX layer for player-facing survival state, prompts, crafting, and building.

## Design Goal

The player should understand what they can do, what they have, what they need, and why an action succeeds or fails.

## Requirements

### 17.1 UXHUD-001: Readable Survival HUD

The HUD SHOULD replace dense text-only survival state with readable, compact presentation.

Acceptance criteria:

- Health and hunger are shown as bars or equivalent glanceable indicators.
- Current day and phase are visible.
- Wood, Stone, Food, and Herbs are shown as compact resource counts.
- Current objective guidance from SPEC-016 is visible when active.
- HUD text and counters do not overlap at the default resolution.

### 17.2 UXHUD-002: Contextual Interaction Prompts

Interaction prompts SHOULD describe the input, action, and target.

Examples:

- `E Gather Wood`
- `E Save at Campfire`
- `E Craft at Workbench`
- `Right Click Place Wall`

Acceptance criteria:

- Prompts update when the nearest interactable changes.
- Disabled actions can show a short reason.
- Prompts disappear when no relevant action is available.
- Prompt wording uses the current wilderness controls.

### 17.3 UXHUD-003: Crafting Readability

Crafting UI SHOULD show enough information to make recipes understandable before crafting.

Acceptance criteria:

- Each recipe shows result, cost, owned counts, and craftability.
- Missing resources are visible before the player confirms crafting.
- Successful crafting shows clear feedback.
- Failed crafting does not consume resources and explains why it failed.

### 17.4 UXHUD-004: Building Mode Readability

Building mode SHOULD make selection and placement state visible.

Acceptance criteria:

- The selected build piece and cost are visible.
- The snapped grid position is previewed.
- Valid and invalid placement states are visibly distinct.
- Invalid placement shows a reason.
- Door rotation and cancel controls are visible while building.

## Non-Goals

- No full inventory grid.
- No controller, touch, or rebinding UI.
- No large crafting tree UI.
- No decoration catalog beyond current wilderness buildables.

## Public Interfaces

The implementation SHOULD expose these conceptual view-data contracts:

- `InteractionPromptData`: input label, action label, target name, and enabled or disabled reason.
- `RecipeViewData`: recipe id, result, costs, owned counts, `canCraft` flag, and missing resources.
- `BuildPreviewState`: selected buildable, snapped position, validity, invalid reason, cost, and rotate state.
- `SaveSlotViewData`: slot number, empty or occupied state, day, phase or time, timestamp, and player summary.

## Dependencies

- SPEC-012 Gathering, Crafting, And Inventory.
- SPEC-013 Grid Building.
- SPEC-014 Save Slots And Menus.
- SPEC-016 First-Session Onboarding And Guidance.
- SPEC-020 Wilderness Controls.

## Verification

Automated tests verify:

- HUD state updates when vitals, resources, and day phase change.
- Context prompts include input, action, and target.
- Crafting UI reports affordability and missing resources.
- Build preview reports valid and invalid placement states.
- Save slot UI summarizes empty and occupied slots.
