# 13. SPEC-013: Grid Building

Status: Implemented

## Purpose

Define MVP grid-based shelter building.

## Design Goal

Building should let the player create a small functional shelter without becoming a full construction game.

## Requirements

### 13.1 BLD-001: Grid Placement

The MVP MUST use grid-based placement for shelter objects.

Acceptance criteria:

- Build previews snap to a visible or implied grid.
- Placement can be confirmed.
- Placement can be canceled without spending resources.
- Door placement can rotate between horizontal and vertical orientation.

### 13.2 BLD-002: Required Build Pieces

The MVP MUST include shelter basics.

Required build pieces:

- Floor.
- Wall.
- Door.
- Campfire.
- Bedroll.
- Storage Box.
- Workbench.

### 13.3 BLD-003: Placement Validity

Placement MUST only succeed on valid outdoor ground.

Acceptance criteria:

- Placement fails on water or impassable terrain.
- Placement fails when overlapping blockers, resources, animals, or the player.
- Placement fails outside the buildable map area.
- Failed placement does not consume resources or buildable counts.

### 13.4 BLD-004: Building Cost Integration

Build placement MUST integrate with crafting and inventory.

Acceptance criteria:

- Build pieces come from crafted buildable counts or direct recipe costs.
- Successful placement consumes the required buildable count or resources.
- Placed buildings are included in save data.

### 13.5 BLD-005: Shelter Function

Placed shelter objects SHOULD provide survival utility.

Acceptance criteria:

- Campfire provides light and save access.
- Bedroll provides save access.
- Storage Box provides item or resource storage.
- Workbench provides crafting access.
- Walls and doors block movement.

## Non-Goals

- No multi-floor buildings.
- No structural stability simulation.
- No furniture decoration set beyond required shelter pieces.
- No enemy base raiding requirement in MVP.

## Public Interfaces

The implementation SHOULD expose this conceptual runtime contract:

- `BuildableType`: floor, wall, door, campfire, bedroll, storage box, and workbench.

## Dependencies

- SPEC-009 Wilderness World And Content.
- SPEC-012 Gathering, Crafting, And Inventory.
- SPEC-014 Save Slots And Menus.

## Verification

Automated MVP tests verify:

- Valid placement succeeds and consumes cost.
- Invalid placement fails and does not consume cost.
- Door rotation changes orientation.
- Campfire and bedroll enable saving.
- Placed building state restores after save/load.
