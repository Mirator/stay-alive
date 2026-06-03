# 4. SPEC-004: Content And Assets

Status: Implemented

## Purpose

Define the content, generated assets, and map readability rules for the proof of concept.

## Requirements

### 4.1 ART-001: Generated Asset Location

Generated art MUST live under `Assets/Art/Generated`.

Acceptance criteria:

- Concept image exists at `Assets/Art/Generated/stay_alive_concept.png`.
- Sprite PNGs exist under `Assets/Art/Generated/Sprites`.
- Generated sprite material exists at `Assets/Art/Generated/SpriteLitGenerated.mat`.

### 4.2 ART-002: Required Sprite Set

The builder MUST generate sprites for the current prototype objects.

Required sprite categories:

- Cave floor.
- Rock wall.
- Player idle, walk, and mine frames.
- Mineable stone, dirt, crystal, and root blockage.
- Ancient door open and closed.
- Torch.
- Campfire.
- Mushrooms.
- Chest.
- Workbench.
- Wooden planks.
- Cave crawler.

### 4.3 ART-003: Torch Prefab

The project MUST include a torch prefab.

Acceptance criteria:

- `Assets/Prefabs/Torch.prefab` exists.
- The prefab has a sprite renderer.
- The prefab has a warm `Light2D`.

### 4.4 MAP-001: Handcrafted Map

The prototype map MUST be handcrafted, not procedural.

Required route:

`Spawn Room -> Blocked Tunnel -> Crystal Chamber -> Ancient Door -> Reward Room`

Acceptance criteria:

- The scene contains named progression objects along this route.
- The route can be validated by edit-mode tests.

### 4.5 MAP-002: Readability

The critical path MUST be readable and not look like an accidental collision trap.

Acceptance criteria:

- `Weak Stone Gate` blocks early progression but breaks quickly.
- Guide torches mark the intended route.
- Crystal nodes emit cool light.
- Door and reward room use distinct visual cues.
- The first Cave Crawler is placed after the crystal chamber and uses a trigger collider.

## Non-Goals

- No external art pipeline requirement beyond the generated concept image.
- No Tilemap requirement.
- No final production art requirement.

## Verification

Covered by:

- `GeneratedAssetsExist`
- `SceneContainsPlayableVerticalSlice`
- `SceneHasMiningCrystalsDoorUiAndLights`
- `SceneContainsCraftingAndEnemySystems`
- `ClearedCriticalPathIsWalkableFromSpawnToRewardRoom`
