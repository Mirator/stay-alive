# 6. SPEC-006: Crafting

Status: Implemented

## Purpose

Define the first crafting system for Stay Alive. Crafting should extend mining and exploration without becoming a full crafting tree.

## Design Goal

Crafting should answer:

Can gathered cave resources become useful survival tools while keeping the prototype readable and fast?

## Requirements

### 6.1 CRAFT-001: Crafting Entry Point

Crafting MUST happen at a Workbench.

Acceptance criteria:

- The player stands near a `Workbench`.
- Pressing `E` opens or cycles a simple crafting interaction.
- Crafting is unavailable when the player is out of range.

### 6.2 CRAFT-002: Initial Recipes

The first implementation MUST include a small recipe set.

Required recipes:

| Output | Cost | Purpose |
| --- | --- | --- |
| Torch | 1 Root Fiber | Adds light to dark spaces. |
| Stone Marker | 2 Stone | Marks explored routes. |
| Crystal Key Shard | 1 Glow Crystal + 1 Stone | Door/key progression test item. |

### 6.3 CRAFT-003: Resource Costs

Crafting MUST consume resources only when the craft succeeds.

Acceptance criteria:

- Missing resources show a message.
- Successful craft subtracts cost.
- Successful craft adds the crafted item or places it immediately, depending on recipe.

### 6.4 CRAFT-004: UI Scope

The first crafting UI SHOULD be minimal.

Acceptance criteria:

- No full inventory grid.
- No drag-and-drop.
- A simple list, cycling selection, or direct interaction menu is acceptable.

### 6.5 CRAFT-005: Torch Integration

Torch crafting SHOULD replace unlimited torch placement.

Acceptance criteria:

- `Q` placement fails without a torch item or required resource.
- Crafting a torch enables at least one placement.

## Non-Goals

- No large recipe tree.
- No item rarity.
- No crafting stations beyond Workbench.
- No timed crafting.
- No save/load of crafted inventory in the first crafting pass.

## Dependencies

- SPEC-001 Core Game Loop.
- SPEC-003 Runtime Systems.
- SPEC-004 Content And Assets.

## Verification

Covered by `StayAlivePrototypeTests`:

- Crafting succeeds with enough resources.
- Crafting fails without enough resources.
- Costs are consumed exactly once.
- Crafted torch placement works and emits light.
