# 10. SPEC-010: Health, Hunger, Day/Night, And Death

Status: Implemented

## Purpose

Define the wilderness build survival pressure systems: health, hunger, day/night, and death.

## Design Goal

The player should feel pressure from time, hunger, and wildlife without turning the wilderness build into a punishing survival sim.

## Requirements

### 10.1 VIT-001: Player Health

The player MUST have health.

Acceptance criteria:

- Health has a visible current and maximum value.
- Animal attacks reduce health.
- Healing items can restore health.
- Health cannot exceed its maximum.

Default values:

- Maximum health: `100`.
- Death threshold: `0`.

### 10.2 VIT-002: Player Hunger

The player MUST have hunger.

Acceptance criteria:

- Hunger has a visible current and maximum value.
- Hunger decreases over time.
- Food restores hunger.
- Hunger cannot exceed its maximum.

Default values:

- Maximum hunger: `100`.
- Hunger drain: tuned so the player needs food during the first 3 days.

### 10.3 VIT-003: Hunger Consequence

Low hunger SHOULD create survival pressure without instant death.

Acceptance criteria:

- Hunger at `0` causes health to drain slowly.
- Eating food stops hunger-based health loss.
- Hunger loss pauses while the game is paused.

### 10.4 VIT-004: Day/Night Cycle

The wilderness build MUST have a day/night cycle.

Acceptance criteria:

- Day length defaults to 5 real-time minutes.
- Each day has day, dusk, and night phases.
- Current day and phase are visible in the UI.
- Wildlife pressure increases at night.

### 10.5 VIT-005: Death State

The wilderness build MUST have a death state.

Acceptance criteria:

- Health reaching `0` stops player control.
- The death UI offers Load Save and Return To Menu actions.
- Death does not delete save slots.

## Non-Goals

- No thirst meter in wilderness build.
- No stamina meter in wilderness build.
- No diseases, temperature, or weather survival in wilderness build.

## Public Interfaces

The implementation SHOULD expose these conceptual runtime contracts:

- `PlayerVitals`: owns current health, maximum health, current hunger, maximum hunger, and death state.
- `DayNightCycle`: owns current day, current phase, normalized time within the day, and day length.

## Dependencies

- SPEC-011 Melee Combat And Wildlife.
- SPEC-012 Gathering, Crafting, And Inventory.
- SPEC-014 Save Slots And Menus.

## Verification

Automated wilderness build tests verify:

- Hunger decreases over time.
- Food restores hunger.
- Animal attacks reduce health.
- Bandage or healing item restores health.
- Health reaching `0` enters death state.
- Day/night reaches Day 2 and Day 3.
