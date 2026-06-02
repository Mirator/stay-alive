# SPEC-003: Runtime Systems

Status: Implemented

## Purpose

Define the runtime scripts and editor systems that support the proof-of-concept loop.

## Requirements

### SYS-001: Scene Generation

The project MUST generate the playable scene from editor code.

Acceptance criteria:

- `StayAlivePrototypeBuilder` creates `Assets/Scenes/SampleScene.unity`.
- The builder creates player, cave, mineables, door, reward goal, crafting station, enemy, UI, lights, camera, and core loop objects.
- Regeneration is deterministic enough for tests to inspect named objects.

### SYS-002: Core Loop Controller

The project MUST have a single controller for objective progression.

Acceptance criteria:

- `GameLoopController` owns `CoreLoopStage`.
- It updates objective text through `UIController`.
- It can complete the run through `CompleteRun`.

### SYS-003: Reward Goal

The project MUST have a trigger-based reward-room completion object.

Acceptance criteria:

- `RewardRoomGoal` requires a trigger collider.
- It completes the loop when the player enters.

### SYS-004: Inventory

The inventory MUST store prototype resource counts.

Resources:

- `Dirt`
- `Stone`
- `GlowCrystal`
- `RootFiber`

Acceptance criteria:

- Resource additions clamp to non-negative values.
- `Changed` fires when resource counts change.
- Door and UI can read resource state.

### SYS-005: Mining

Mineable objects MUST define durability, resource type, and reward amount.

Acceptance criteria:

- A hit reduces remaining durability.
- Reaching zero durability disables the object.
- Reward resources are added directly to inventory.

### SYS-006: Door Interaction

The Ancient Door MUST implement `IInteractable`.

Acceptance criteria:

- It checks inventory for the required resource.
- It opens by disabling its blocking collider.
- It exposes `IsOpen`.

### SYS-007: UI

The UI MUST display resource counts, crafted item counts, objective text, prompts, and temporary messages.

Acceptance criteria:

- `UIController` binds to `ResourceInventory`.
- `UIController` binds to `CraftedInventory`.
- Counters refresh when inventory changes.
- Objective text can be set by systems.

### SYS-008: Crafting

The runtime MUST support a minimal Workbench crafting system.

Acceptance criteria:

- `CraftingStation` implements `IInteractable`.
- `CraftedInventory` stores crafted Torches, Stone Markers, and Crystal Key Shards.
- Crafting checks all costs before spending any resource.

### SYS-009: Enemies

The runtime MUST support the first Cave Crawler enemy.

Acceptance criteria:

- `CaveCrawlerEnemy` can idle, chase, and retreat.
- `PlayerKnockback` implements the single first-pass contact consequence.
- Enemy colliders are triggers so the critical path is not physically blocked.

### SYS-010: Editor Automation

The project SHOULD support automated scene rebuild and edit-mode verification.

Acceptance criteria:

- `CodexUnityAutomation` watches for `Temp/CodexStayAliveAutomation.request`.
- It exits Play Mode before rebuilding.
- It writes JSON and XML reports under `Logs`.

## Non-Goals

- No runtime scene generator.
- No persistent save system.
- No dependency injection framework.
- No procedural map system.

## Verification

Covered by `StayAlivePrototypeTests` and `Logs/codex-unity-automation.json`.
