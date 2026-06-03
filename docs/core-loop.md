# 1. SPEC-001: Core Game Loop

Status: Implemented

## Purpose

Define the playable vertical slice that proves Stay Alive can support exploration, mining, resource collection, interaction, and a clear completion moment.

## Requirements

### 1.1 CL-001: Spawn

The player MUST spawn in a safe cave room near a campfire.

Acceptance criteria:

- The scene contains `Player Explorer`.
- The scene contains `Campfire`.
- The camera follows the player at scene start.

### 1.2 CL-002: Objective Stages

The loop MUST progress through these stages:

1. `MineBlockedTunnel`
2. `CollectGlowCrystals`
3. `OpenAncientDoor`
4. `ReachRewardRoom`
5. `Complete`

Acceptance criteria:

- `GameLoopController.CurrentStage` exposes the active stage.
- Objective UI changes when stage transitions occur.

### 1.3 CL-003: First Mining Gate

The first progression blocker MUST be a clearly named weak mineable object.

Acceptance criteria:

- The scene contains `Weak Stone Gate`.
- The gate breaks in one hit.
- Breaking or disabling the gate advances the loop to `CollectGlowCrystals`.

### 1.4 CL-004: Crystal Collection

The player MUST collect 3 Glow Crystals before opening the Ancient Door.

Acceptance criteria:

- The scene contains at least 3 `Crystal Node` mineables.
- Each crystal node rewards `GlowCrystal`.
- The objective UI shows crystal collection progress.

### 1.5 CL-005: Ancient Door

The Ancient Door MUST require 3 Glow Crystals and open through interaction.

Acceptance criteria:

- The door does not open without enough Glow Crystals.
- The door opens once the player has 3 Glow Crystals and interacts with it.
- Opening the door disables its blocking collider.

### 1.6 CL-006: Reward Room Completion

The run MUST complete when the player enters the reward room.

Acceptance criteria:

- The scene contains `Reward Room Goal`.
- Entering the trigger calls `GameLoopController.CompleteRun`.
- Completion updates objective text and displays a completion message.

### 1.7 CL-007: Critical Path Passability

The critical route MUST remain physically walkable after required blockers are cleared.

Acceptance criteria:

- A player-sized path exists from spawn to reward room.
- The regression test samples this route with player collision radius.

## Non-Goals

- No procedural generation.
- No save/load.
- No full inventory UI.
- No combat requirement in this implemented loop.

## Verification

Covered by `StayAlivePrototypeTests`:

- `SceneContainsPlayableVerticalSlice`
- `SceneHasMiningCrystalsDoorUiAndLights`
- `CoreLoopProgressesThroughMiningCrystalsDoorAndReward`
- `ClearedCriticalPathIsWalkableFromSpawnToRewardRoom`
