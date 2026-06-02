# SPEC-002: Controls

Status: Implemented

## Purpose

Define the keyboard and mouse controls for the proof of concept.

## Requirements

### CTL-001: Movement

The player MUST move with `WASD`.

Acceptance criteria:

- `W` moves up.
- `A` moves left.
- `S` moves down.
- `D` moves right.
- Diagonal movement is normalized.

### CTL-002: Mining

The player MUST mine with left mouse button.

Acceptance criteria:

- Left click triggers the mining animation frame.
- A reachable mineable can receive a hit.
- Target selection is forgiving and biased by mouse aim.

### CTL-003: Interaction

The player MUST interact with nearby objects using `E`.

Acceptance criteria:

- Nearby interactables show an interaction prompt.
- Pressing `E` invokes the nearest interactable.
- Ancient Door interaction uses the inventory requirement from SPEC-001.

### CTL-004: Torch Placement

The player SHOULD be able to place crafted torches with `Q`.

Acceptance criteria:

- Pressing `Q` without a crafted Torch fails with a message.
- Pressing `Q` with a crafted Torch instantiates `Assets/Prefabs/Torch.prefab`.
- Placing a Torch consumes one crafted Torch.
- Placed torches emit warm light.
- Torches do not block movement.

### CTL-005: Pause

The player MUST toggle pause with `Esc`.

Acceptance criteria:

- `Esc` pauses by setting `Time.timeScale` to `0`.
- Pressing `Esc` again resumes play.
- UI displays a short pause/resume message.

## Non-Goals

- Gamepad support is not required.
- Rebindable controls are not required.
- Touch controls are not required.

## Verification

Covered by `TorchPlacementRequiresCraftedTorchAndConsumesOne` and runtime script inspection. Future play-mode tests SHOULD simulate input for `PlayerController2D` and `PlayerInteraction`.
