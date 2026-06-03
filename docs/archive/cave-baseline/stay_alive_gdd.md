# Stay Alive — Game Design Document

## 1. High Concept

**Stay Alive** is a small top-down 2D cave exploration early build where the player controls a lone underground explorer who wakes up in a mysterious cavern, moves through a stylized environment, collects glowing resources, mines simple obstacles, and interacts with basic objects.

The first version focuses on **movement, atmosphere, exploration, and basic interaction**, not complex crafting or procedural generation.

---

## 2. Genre

**Top-down 2D exploration / light survival early build**

Primary player actions:

- Move
- Explore
- Mine
- Collect
- Interact
- Place simple objects

---

## 3. Visual Style

The game should use a **stylized illustrated top-down look**.

### Art direction

- Hand-painted or vector-painted 2D sprites
- Strong silhouettes
- Dramatic shadows
- Warm light against dark cave backgrounds
- Slightly exaggerated shapes
- Rich but controlled color palette
- Clean readable objects
- High contrast between interactable and non-interactable elements

### Mood

The world should feel:

- Mysterious
- Underground
- Warm near light sources
- Dangerous in the dark
- Cozy around the player’s base area

### Environment look

The cave should not look like a flat tile grid. Even if implemented with Unity Tilemaps, tiles should visually blend together.

Use:

- Irregular cave floors
- Rounded rock formations
- Glowing crystals
- Large roots
- Mushrooms
- Broken stone ruins
- Wooden planks
- Pools of darkness
- Warm torchlight

---

## 4. Camera

### View

- Top-down or slightly angled top-down
- Orthographic camera
- Camera follows the player smoothly

### Recommended Unity setup

- `Universal 2D` template
- Orthographic camera
- 2D Renderer
- Sprite-based environment
- Optional Cinemachine camera follow

---

## 5. Player Character

### Concept

The player controls a small cave explorer.

Possible character options:

- Mole explorer
- Small miner
- Underground druid
- Lantern keeper
- Tiny armored wanderer

### Visual features

- Clear silhouette
- Small body, larger head or helmet
- Backpack or pouch
- Pickaxe
- Lantern or glowing crystal attached to belt
- Short readable animations

### Required animations for early build

| Animation | Purpose |
|---|---|
| Idle | Character stands still |
| Walk Up | Movement upward |
| Walk Down | Movement downward |
| Walk Side | Movement left/right |
| Mine / Swing | Basic action animation |
| Interact | Optional, can reuse mining animation |

---

## 6. Core Early Build Gameplay

### Early Build goal

The first early build should answer one question:

> Is it satisfying to walk around a small cave, interact with objects, and mine simple obstacles?

### Minimum playable loop

1. Player spawns in a small cave room.
2. Player moves with keyboard input.
3. Camera follows the player.
4. Player approaches mineable rocks or crystals.
5. Player presses a button or clicks to mine.
6. Object breaks after one or more hits.
7. Resource counter increases.
8. Player can interact with a chest, torch, workbench, or ancient door.

---

## 7. Controls

### Keyboard and mouse

| Input | Action |
|---|---|
| `WASD` | Move |
| Mouse position | Aim / choose interaction direction |
| Left mouse button | Mine / use tool |
| `E` | Interact |
| `Q` | Place torch |
| `Esc` | Pause |

### Gamepad

Not required for the first early build.

---

## 8. Player Actions

## 8.1 Movement

The player can move freely in four directions.

### Requirements

- Smooth 2D movement
- Collision with walls, rocks, and props
- No jumping
- No physics-heavy movement
- Player should stop when touching solid objects

### Recommended implementation

- `Rigidbody2D`
- `Collider2D`
- Movement via velocity or `MovePosition`
- Keep movement simple and responsive

---

## 8.2 Mining

The player can mine specific objects.

### Mineable objects

| Object | Hits to break | Reward |
|---|---:|---|
| Dirt mound | 1 | Dirt |
| Stone rock | 2 | Stone |
| Crystal node | 3 | Glow Crystal |
| Root blockage | 2 | Root Fiber |

### Mining rules

- Player must be close enough to target.
- Player clicks or presses the action button.
- Tool swing animation plays.
- Object flashes or shakes when hit.
- When durability reaches zero, object disappears.
- Resource counter updates.

### First implementation

Use simple GameObjects, not destructible terrain.

Example objects:

- `MineableRock`
- `MineableCrystal`
- `MineableRoot`

Each mineable object has:

- `health`
- `resourceType`
- `resourceAmount`

---

## 8.3 Collecting

When a mineable object breaks, it can either:

1. Add resources directly to inventory.
2. Spawn a small pickup item.

For the first early build, use **direct collection**.

### Resource UI

Display simple counters:

```text
Stone: 0
Glow Crystal: 0
Root Fiber: 0
```

No full inventory is needed yet.

---

## 8.4 Interacting

The player can press `E` near objects.

### Interactive objects

| Object | Interaction |
|---|---|
| Chest | Shows simple message: “Storage not implemented” |
| Workbench | Shows simple message: “Crafting not implemented” |
| Ancient Door | Opens if player has 3 Glow Crystals |
| Campfire | Acts as spawn point or safe area marker |

For the first version, only one interaction is necessary.

Best first interaction:

> Player collects 3 Glow Crystals, presses `E` near an Ancient Door, and the door opens.

---

## 8.5 Placing Torches

Optional, but useful for atmosphere.

### Basic version

- Press `Q` to place a torch at the player position.
- Torch creates a small warm light.
- Torch count can be unlimited in the early build.

### Better version later

- Torches cost `1 Wood` or `1 Root Fiber`.
- Player can only place torches on valid floor tiles.

---

## 9. World Design

## 9.1 First Map

Do **not** start with procedural generation.

Create one handcrafted test map.

### Map size

Recommended:

```text
40 x 25 tiles
```

or equivalent if using freeform sprites.

### Areas

| Area | Purpose |
|---|---|
| Spawn room | Safe starting area |
| Mining corridor | Teaches mining |
| Crystal chamber | Gives reward |
| Locked door | Teaches interaction |
| Small enemy room | Optional combat test |
| Base corner | Contains chest, workbench, or campfire |

---

## 9.2 Map Layout

Simple structure:

```text
[Spawn Room] → [Blocked Tunnel] → [Crystal Room] → [Ancient Door] → [Reward Room]
```

### Player journey

1. Spawn near campfire.
2. Walk around.
3. See blocked tunnel.
4. Mine rocks.
5. Find crystals.
6. Collect 3 crystals.
7. Open ancient door.
8. Reach small reward room.

This is enough for a first vertical slice.

---

## 10. Lighting

Lighting is important for the atmosphere.

### Required

- Player has a small light radius.
- Torches emit warm light.
- Crystals emit colored glow.
- Areas outside light are darker.

### Unity implementation

Use:

- URP 2D Renderer
- `Light2D`
- Global Light 2D set to a dim value
- Point Light 2D on player, torches, and crystals

### Lighting mood

- Player light: warm, small radius
- Crystals: cool glow
- Campfire / torches: warm and larger radius
- Background: dark blue, dark green, or dark brown

---

## 11. UI

Keep UI minimal.

### Cave UI

Top-left corner:

```text
Stone: 0
Glow Crystal: 0
Root Fiber: 0
```

Optional interaction prompt:

```text
Press E to interact
```

Optional objective text:

```text
Find 3 Glow Crystals and open the sealed door.
```

---

## 12. Basic Game Objects

## 12.1 Player

Components:

- Sprite Renderer
- Animator
- Rigidbody2D
- Collider2D
- Player Movement Script
- Player Interaction Script
- Player Inventory Script
- Light2D child object

---

## 12.2 Mineable Rock

Components:

- Sprite Renderer
- Collider2D
- Mineable Script

Properties:

```text
Health: 2
Resource: Stone
Amount: 1
```

---

## 12.3 Crystal Node

Properties:

```text
Health: 3
Resource: Glow Crystal
Amount: 1
Emits Light: Yes
```

---

## 12.4 Ancient Door

Properties:

```text
Required Resource: Glow Crystal
Required Amount: 3
State: Closed / Open
```

Interaction:

- If player has enough crystals, the door opens.
- If not, show a message.

---

## 12.5 Torch

Properties:

```text
Emits Light: Yes
Blocks Movement: No
Can Be Placed: Yes
```

---

## 13. Early Build Scope

## Must Have

- Player spawn
- Player movement
- Camera follow
- Collision
- Small cave environment
- Mineable rocks
- Mineable crystals
- Resource counter
- One interactable door
- Basic lighting

## Should Have

- Torch placement
- Hit feedback
- Simple objective text
- Basic sound effects

## Could Have

- One simple enemy
- Health
- Basic attack
- Workbench interaction
- Resource pickups
- Main menu

## Not For First Early Build

- Procedural generation
- Full inventory
- Crafting tree
- Multiplayer
- Save/load
- Complex combat
- Bosses
- Large map
- Building system
- Farming system

---

## 14. First Development Milestones

### Milestone 1 — Movement Test

Goal:

> Player can spawn and move around a blank test room.

Includes:

- Player object
- Movement script
- Camera follow
- Basic collision

---

### Milestone 2 — Cave Room

Goal:

> Player can walk around a small cave environment.

Includes:

- Floor
- Walls
- Props
- Collision
- Spawn point

---

### Milestone 3 — Mining

Goal:

> Player can mine rocks and collect resources.

Includes:

- Mineable object script
- Mining range
- Hit detection
- Resource counter

---

### Milestone 4 — Interaction

Goal:

> Player can use collected crystals to open a door.

Includes:

- Crystal resource
- Ancient Door
- `E` interaction
- Simple feedback message

---

### Milestone 5 — Atmosphere

Goal:

> The early build has a clear visual mood.

Includes:

- Player light
- Torch light
- Crystal glow
- Darkened cave areas
- Basic sound effects

---

## 15. Target First Playable Experience

The first build should last **2–3 minutes**.

Player experience:

1. Player appears beside a small campfire.
2. They move around a cave.
3. They mine rocks blocking a path.
4. They discover glowing crystals.
5. They mine crystals.
6. The UI shows collected crystals.
7. They return to a sealed door.
8. They press `E`.
9. The door opens.
10. They enter a small reward chamber.

That is the first playable version.
