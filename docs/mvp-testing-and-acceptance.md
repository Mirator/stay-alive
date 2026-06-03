# 15. SPEC-015: MVP Testing And Acceptance

Status: Implemented

## Purpose

Define the verification bar for the outdoor wilderness sandbox MVP.

## Design Goal

The MVP should be considered ready when automated tests and manual smoke checks prove that the first 3 in-game days are playable without relying on the old cave progression.

## Requirements

### 15.1 MVPTEST-001: Spec Index Verification

Tests or documentation checks SHOULD verify that MVP specs are discoverable.

Acceptance criteria:

- README lists PoC baseline specs separately from MVP implemented specs.
- `SPEC-008` through `SPEC-015` exist.
- MVP specs are marked `Status: Implemented` once the implementation and acceptance tests exist.

### 15.2 MVPTEST-002: Scene Generation Verification

Tests MUST verify the MVP outdoor scene once implementation begins.

Acceptance criteria:

- New game starts outdoors.
- Required landmarks exist.
- Required sprites exist.
- Spawn clearing is safe.
- Buildable ground exists.

### 15.3 MVPTEST-003: Survival Verification

Tests MUST verify the survival systems.

Acceptance criteria:

- Hunger decreases over time.
- Food restores hunger.
- Health decreases from animal attacks.
- Bandage restores health.
- Health reaching `0` enters death state.
- Day/night reaches Day 2 and Day 3.

### 15.4 MVPTEST-004: Crafting And Building Verification

Tests MUST verify gathering, crafting, and grid building.

Acceptance criteria:

- Player gathers Wood, Stone, Food, and Herbs.
- Required recipes consume exact costs.
- Required recipes fail safely without enough resources.
- Valid grid placement succeeds.
- Invalid grid placement fails without spending resources.

### 15.5 MVPTEST-005: Wildlife Combat Verification

Tests MUST verify melee combat and wildlife behavior.

Acceptance criteria:

- `Stone Spear` damages wildlife.
- Wildlife detects and attacks the player.
- Wolves are more aggressive at night.
- Boars charge after a tell.
- Defeated wildlife stops attacking.

### 15.6 MVPTEST-006: Save/Load Verification

Tests MUST verify the 3-slot save system.

Acceptance criteria:

- Saving to one of 3 slots writes slot metadata.
- Loading restores player position, health, hunger, inventory, day/time, buildings, harvested nodes, and defeated enemies.
- Death UI can load an existing slot.

## Current MVP Pass Gate

The MVP automation pass gate is active. The MVP test suite MUST report:

- 0 failures.
- 0 inconclusive results.
- 0 skipped tests counted as pass evidence.
- At least 13 passing edit-mode tests in `StayAlivePrototypeTests`.
- At least 4 passing play-mode tests in `StayAliveMvpPlayModeTests`.
- Coverage for all `MVPTEST` requirements listed above.

## Non-Goals

- No performance benchmark gate in the first MVP spec pass.
- No platform certification testing.
- No save migration testing until post-MVP changes require it.

## Dependencies

- SPEC-008 Wilderness Survival Loop.
- SPEC-009 Wilderness World And Content.
- SPEC-010 Health, Hunger, Day/Night, And Death.
- SPEC-011 Melee Combat And Wildlife.
- SPEC-012 Gathering, Crafting, And Inventory.
- SPEC-013 Grid Building.
- SPEC-014 Save Slots And Menus.
