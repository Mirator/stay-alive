# 9. SPEC-009: Wilderness World And Content

Status: Implemented

## Purpose

Define the wilderness outdoor world, content set, landmarks, and visual replacement for cave-specific content.

## Design Goal

The wilderness should feel readable, buildable, and dangerous without requiring procedural generation.

## Requirements

### 9.1 WWC-001: Handcrafted Outdoor Map

The wilderness build MUST use a handcrafted outdoor map.

Acceptance criteria:

- The scene is outdoors and uses wilderness ground, not cave floor.
- The map supports free exploration around the spawn area.
- The map has natural boundaries such as water, cliffs, dense trees, or rocks.
- The map has enough open terrain for grid building.

### 9.2 WWC-002: Required Landmarks

The wilderness map MUST include the core survival landmarks.

Required landmarks:

- Spawn clearing.
- Forest resource area.
- Stone outcrop.
- Food and herb area.
- Water or impassable boundary.
- Open building area.
- Animal danger zones.

Acceptance criteria:

- Each landmark has a clear visual identity.
- At least one safe route connects the spawn clearing to each early resource landmark.
- Animal danger zones are not inside the spawn clearing.

### 9.3 WWC-003: Required Visual Set

The wilderness build MUST generate or include sprites for the outdoor content set.

Required visual categories:

- Grass and soil ground.
- Trees and logs.
- Rocks and stone nodes.
- Bushes.
- Herb plants.
- Food nodes.
- Shelter floor, wall, and door pieces.
- Campfire.
- Bedroll.
- Storage box.
- Workbench.
- Wild animal enemy.

### 9.4 WWC-004: Cave Content Replacement

wilderness specs MUST NOT require cave-specific progression content.

Acceptance criteria:

- wilderness progression does not depend on `GlowCrystal`.
- wilderness progression does not depend on `AncientDoor`.
- wilderness enemy requirements do not depend on `CaveCrawlerEnemy`.
- Cave baseline content may remain in old specs or unused implementation paths.

## Non-Goals

- No final production art requirement.
- No procedural terrain requirement.
- No large biome system.
- No town, NPC settlement, or quest hub requirement.

## Dependencies

- SPEC-008 Wilderness Survival Loop.
- SPEC-013 Grid Building.

## Verification

Automated wilderness build tests verify:

- The generated wilderness build scene contains all required landmarks.
- Outdoor sprites exist under `Assets/Art/Generated/Sprites`.
- The spawn clearing has no hostile animal in immediate attack range.
- The buildable area accepts basic grid placement.
