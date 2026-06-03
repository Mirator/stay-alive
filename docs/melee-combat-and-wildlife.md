# 11. SPEC-011: Melee Combat And Wildlife

Status: Implemented

## Purpose

Define MVP combat as simple melee survival against wild animals.

## Design Goal

Combat should add danger and recovery pressure while staying readable and lightweight.

## Requirements

### 11.1 MCW-001: Melee Survival Combat

The MVP MUST use melee combat as the primary combat model.

Acceptance criteria:

- The player can attack in a short range.
- A melee attack uses an animation or visible feedback.
- A hit animal loses health.
- Defeated animals stop attacking and remain defeated in save data.

### 11.2 MCW-002: Stone Spear

The MVP MUST include one reliable craftable melee weapon: `Stone Spear`.

Acceptance criteria:

- `Stone Spear` can be crafted from the SPEC-012 recipe.
- `Stone Spear` increases attack range or damage compared to bare hands.
- The player can fight the first wildlife enemy with it.

### 11.3 MCW-003: Wild Animal Enemy Contract

The first MVP enemies MUST be wild animals.

Required enemy behaviors:

- Idle or wander near a home area.
- Notice the player within a detection radius.
- Chase or charge the player.
- Attack when in range.
- Disengage or return home when the player escapes.

### 11.4 MCW-004: Wildlife Types

The MVP SHOULD include two wildlife patterns.

Required patterns:

- Wolf: more aggressive at night.
- Boar: charges when approached.

Acceptance criteria:

- Wolves use a larger detection radius or shorter attack cooldown at night.
- Boars have a clear charge tell before dealing damage.

### 11.5 MCW-005: Combat Fairness

Wildlife pressure MUST avoid unavoidable spawn deaths.

Acceptance criteria:

- Hostile animals do not start in the spawn clearing.
- Animal danger zones are visible or learnable.
- The player can retreat from early wildlife.
- Wildlife cannot physically block all paths out of the starting area.

## Non-Goals

- No ranged combat in MVP.
- No bosses.
- No bandits or human enemies.
- No enemy loot table requirement.

## Public Interfaces

The implementation SHOULD expose these conceptual runtime contracts:

- `MeleeWeapon` or equivalent weapon data for `Stone Spear` damage, range, and attack timing.
- `WildAnimalEnemy`: outdoor enemy behavior contract for wandering, detection, chase/charge, attack, disengage, defeat, and save identity.

## Dependencies

- SPEC-009 Wilderness World And Content.
- SPEC-010 Health, Hunger, Day/Night, And Death.
- SPEC-012 Gathering, Crafting, And Inventory.

## Verification

Automated MVP tests verify:

- `Stone Spear` can damage wildlife.
- Wildlife notices and chases the player.
- Wildlife attacks reduce player health.
- Wolves are more aggressive at night.
- Boars charge after a tell.
- Defeated wildlife remains defeated after save/load.
