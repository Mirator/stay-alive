# SPEC-005: Testing And Verification

Status: Implemented

## Purpose

Define the evidence required to prove the current prototype works.

## Requirements

### TEST-001: Edit-Mode Test Fixture

The project MUST keep verification tests in `Assets/Editor/StayAlivePrototypeTests.cs`.

Acceptance criteria:

- Tests are discoverable by Unity Edit Mode Test Runner.
- Tests run without requiring Play Mode.

### TEST-002: Scene Content Verification

Tests MUST verify generated scene content.

Acceptance criteria:

- Generated assets exist.
- The scene contains player, camera, UI, game loop, and reward goal.
- Mineables, crystals, door, and lights exist.
- Crafting and enemy systems exist in the generated scene.

### TEST-003: Loop Behavior Verification

Tests MUST verify the core loop behavior.

Acceptance criteria:

- Mineables add resources and break.
- Ancient Door requires 3 Glow Crystals.
- Core loop progresses through mining, crystal collection, door opening, reward-room entry, and completion.

### TEST-004: Path Regression Verification

Tests MUST verify that the critical path is walkable after required blockers are cleared.

Acceptance criteria:

- The route from spawn to reward room is sampled with player collision radius.
- Blocking colliders on the sampled route fail the test.

### TEST-005: Automation Report

Automation SHOULD write machine-readable reports.

Acceptance criteria:

- `Logs/codex-unity-automation.json` is written.
- `Logs/codex-unity-test-results.xml` is written.
- JSON `success` is true only if the required test count passes with zero failures.

### TEST-006: Crafting And Enemy Verification

Tests MUST verify the implemented SPEC-006 and SPEC-007 behavior.

Acceptance criteria:

- Workbench crafting succeeds and fails according to resource costs.
- Crafted torch placement requires and consumes a crafted Torch.
- Cave Crawler placement is outside the safe room and cannot block the path.
- Cave Crawler state changes near the player and near warm light.
- Enemy contact triggers the selected knockback consequence.

## Current Pass Gate

The automation MUST report at least 12 passing tests with:

- 0 failures.
- 0 inconclusive results.
- 0 skipped tests counted as pass evidence.

## Manual Verification

In Unity:

1. Select `Stay Alive > Build Proof of Concept`.
2. Open Test Runner.
3. Run `StayAlivePrototypeTests` in Edit Mode.

## Non-Goals

- No full Play Mode input simulation yet.
- No player build automation yet.
- No performance benchmark gate yet.
