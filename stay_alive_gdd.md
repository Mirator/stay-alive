# Stay Alive Game Design Document

## High Concept

**Stay Alive** is a top-down 2D wilderness survival sandbox. The player starts outdoors, gathers basic resources, crafts survival tools, builds a simple grid shelter, fights or avoids wild animals, manages hunger and health, and saves manually through one of three save slots.

The game has no winning condition. The first tuning target is a readable, playable first 3 in-game days; after that, the sandbox continues with repeated survival pressure.

## Current Player Loop

1. Start in a wilderness clearing.
2. Gather `Wood`, `Stone`, `Food`, and `Herbs`.
3. Craft survival items such as `Stone Spear`, `Bandage`, `Campfire`, `Bedroll`, `Storage Box`, and `Workbench`.
4. Build a basic grid shelter with floors, walls, and a door.
5. Fight or avoid wolves and boars.
6. Manage health, hunger, and day/night pressure.
7. Save manually at a placed campfire or bedroll.
8. Continue into later days.

## Active Specs

The active source of truth is [docs/README.md](docs/README.md).

Current implemented spec groups:

- `SPEC-008` through `SPEC-015`: wilderness survival, world content, vitals, combat, crafting, building, saves, and testing.
- `SPEC-016` through `SPEC-020`: onboarding, HUD/interactions, readability/accessibility, game menu flow, and current wilderness controls.

Archived cave-era documents are kept in [docs/archive/cave-baseline/README.md](docs/archive/cave-baseline/README.md) for reference only.
