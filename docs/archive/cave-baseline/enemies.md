# 7. SPEC-007: Enemies

Status: Implemented

## Purpose

Define the first enemy system for Stay Alive. Enemies should add tension in dark areas without turning the early build into a combat-heavy game.

## Design Goal

Enemies should answer:

Does light, movement, and spatial pressure make cave exploration more exciting without overwhelming the mining loop?

## Requirements

### 7.1 ENEMY-001: First Enemy Type

The first enemy MUST be a simple cave crawler.

Behavior:

- Idle near a patrol point.
- Notice the player within a short radius.
- Move toward the player slowly.
- Stop or retreat near strong light.

### 7.2 ENEMY-002: Spawn Placement

Enemies MUST be placed outside the initial safe room.

Acceptance criteria:

- No enemy can attack the player at spawn.
- The first enemy appears after the player reaches or passes the crystal chamber.
- Enemy placement must not block the required critical path.

### 7.3 ENEMY-003: Player Consequence

Enemy contact SHOULD create a light survival consequence.

Allowed first-pass consequences:

- Reduce player health.
- Knock player back.
- Force a short respawn at campfire.

The first implementation MUST choose only one consequence.

Implemented consequence: knock player back.

### 7.4 ENEMY-004: Light Interaction

Enemies SHOULD react to player-placed torches or campfire light.

Acceptance criteria:

- Enemy aggression decreases near warm light.
- Torches create a useful defensive choice.

### 7.5 ENEMY-005: Combat Scope

The first enemy pass MAY allow mining swing damage, but combat depth is optional.

Acceptance criteria if enabled:

- Mining swing can damage enemies in range.
- Enemy death is clear.
- Enemy rewards are optional.

Combat damage is not enabled in the first implemented pass.

## Non-Goals

- No boss enemies.
- No ranged enemies.
- No enemy loot table.
- No large combat system.
- No procedural spawning.
- No enemy in the first safe room.

## Dependencies

- SPEC-001 Core Game Loop.
- SPEC-002 Controls.
- SPEC-003 Runtime Systems.
- SPEC-004 Content And Assets.
- SPEC-006 Crafting, if torches become resource-gated before enemies are implemented.

## Verification

Covered by `StayAliveEditModeTests`:

- Enemy spawns outside the safe room.
- Enemy does not block the critical path.
- Enemy detects and moves toward the player.
- Enemy response changes near torch or campfire light.
- Chosen player consequence triggers on contact.
