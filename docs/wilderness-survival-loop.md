# 8. SPEC-008: Wilderness Survival Loop

Status: Implemented

## Purpose

Define the wilderness build survival loop after the cave baseline. The wilderness build moves Stay Alive outdoors into a wilderness sandbox with no winning condition.

## Design Goal

The wilderness build should answer:

Can a player survive multiple days in the wild by gathering, crafting, building shelter, fighting or avoiding animals, and saving progress?

## Requirements

### 8.1 WSL-001: Outdoor Sandbox Start

The wilderness build MUST start the player outdoors in a wilderness clearing.

Acceptance criteria:

- The player starts in an outdoor scene, not a cave.
- The starting area is safe enough for orientation.
- The player can immediately gather at least one nearby resource.
- The first objective text explains survival actions without implying a win condition.

### 8.2 WSL-002: No Winning Condition

The wilderness build MUST be a sandbox without a final victory state.

Acceptance criteria:

- The game does not end after reaching a location or completing a quest.
- Surviving Day 3 is an wilderness acceptance milestone, not a win state.
- After Day 3, the game continues with repeated or scaled survival pressure.

### 8.3 WSL-003: First 3 Day Target

The wilderness build MUST support a playable first 3 in-game days.

Acceptance criteria:

- Day 1 teaches gathering and basic crafting.
- Day 2 pressures shelter building and hunger management.
- Day 3 introduces stronger wildlife pressure.
- The player can continue playing after Day 3.

### 8.4 WSL-004: Core Repeating Loop

The wilderness loop MUST repeat without requiring authored route completion.

Loop stages:

1. Gather resources.
2. Craft tools or supplies.
3. Build or improve shelter.
4. Manage health and hunger.
5. Fight or avoid wildlife.
6. Save at a safe object.
7. Continue into the next day.

Acceptance criteria:

- Each loop action has at least one implemented player-facing interaction.
- The player can recover from low resources through gathering.
- The player can be threatened by wildlife without being trapped in an unavoidable fail state.

## Non-Goals

- No authored ending.
- No cave progression route requirement.
- No Ancient Door, Glow Crystal gate, or reward room requirement for wilderness build.
- No procedural world requirement in the first wilderness build pass.

## Dependencies

- SPEC-009 Wilderness World And Content.
- SPEC-010 Health, Hunger, Day/Night, And Death.
- SPEC-011 Melee Combat And Wildlife.
- SPEC-012 Gathering, Crafting, And Inventory.
- SPEC-013 Grid Building.
- SPEC-014 Save Slots And Menus.

## Verification

Automated wilderness build tests verify:

- A new game starts outdoors.
- The player can gather at least one resource from spawn-adjacent wilderness content.
- The day counter can reach Day 2 and Day 3.
- The game continues after Day 3.
- No completion state is triggered by Day 3 survival.
