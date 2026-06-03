# 15. SPEC-015: Testing And Acceptance

Status: Implemented

## Purpose

Define the verification bar for the outdoor wilderness sandbox.

## Design Goal

The wilderness build should be considered ready when automated tests and manual smoke checks prove that the first 3 in-game days are playable without relying on the old cave progression.

## Requirements

### 15.1 TEST-001: Spec Index Verification

Tests or documentation checks SHOULD verify that wilderness specs are discoverable.

Acceptance criteria:

- README lists cave baseline specs separately from implemented wilderness specs.
- `SPEC-008` through `SPEC-015` exist.
- Wilderness specs are marked `Status: Implemented` once the implementation and acceptance tests exist.

### 15.2 TEST-002: Scene Generation Verification

Tests MUST verify the wilderness outdoor scene once implementation begins.

Acceptance criteria:

- New game starts outdoors.
- Required landmarks exist.
- Required sprites exist.
- Spawn clearing is safe.
- Buildable ground exists.

### 15.3 TEST-003: Survival Verification

Tests MUST verify the survival systems.

Acceptance criteria:

- Hunger decreases over time.
- Food restores hunger.
- Health decreases from animal attacks.
- Bandage restores health.
- Health reaching `0` enters death state.
- Day/night reaches Day 2 and Day 3.

### 15.4 TEST-004: Crafting And Building Verification

Tests MUST verify gathering, crafting, and grid building.

Acceptance criteria:

- Player gathers Wood, Stone, Food, and Herbs.
- Required recipes consume exact costs.
- Required recipes fail safely without enough resources.
- Valid grid placement succeeds.
- Invalid grid placement fails without spending resources.

### 15.5 TEST-005: Wildlife Combat Verification

Tests MUST verify melee combat and wildlife behavior.

Acceptance criteria:

- `Stone Spear` damages wildlife.
- Wildlife detects and attacks the player.
- Wolves are more aggressive at night.
- Boars charge after a tell.
- Defeated wildlife stops attacking.

### 15.6 TEST-006: Save/Load Verification

Tests MUST verify the 3-slot save system.

Acceptance criteria:

- Saving to one of 3 slots writes slot metadata.
- Loading restores player position, health, hunger, inventory, day/time, buildings, harvested nodes, and defeated enemies.
- Death UI can load an existing slot.

## Current Pass Gate

The wilderness automation pass gate is active. The wilderness test suite MUST report:

- 0 failures.
- 0 inconclusive results.
- 0 skipped tests counted as pass evidence.
- At least 18 passing edit-mode tests in `StayAliveEditModeTests`.
- At least 5 passing play-mode tests in `StayAlivePlayModeTests`.
- Coverage for all `TEST` requirements listed above.
- Coverage for implemented UX specs `SPEC-016` through `SPEC-018`.

## Non-Goals

- No performance benchmark gate in the first wilderness spec pass.
- No platform certification testing.
- No save migration testing until UX changes require it.

## Dependencies

- SPEC-008 Wilderness Survival Loop.
- SPEC-009 Wilderness World And Content.
- SPEC-010 Health, Hunger, Day/Night, And Death.
- SPEC-011 Melee Combat And Wildlife.
- SPEC-012 Gathering, Crafting, And Inventory.
- SPEC-013 Grid Building.
- SPEC-014 Save Slots And Menus.
- SPEC-016 First-Session Onboarding And Guidance.
- SPEC-017 HUD, Interaction, Crafting, And Building UX.
- SPEC-018 Feedback, Readability, And Accessibility.
