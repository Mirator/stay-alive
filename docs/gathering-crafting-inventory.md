# 12. SPEC-012: Gathering, Crafting, And Inventory

Status: Implemented

## Purpose

Define the wilderness resource, gathering, crafting, and inventory model for outdoor wilderness survival.

## Design Goal

The wilderness build should use a compact resource set that supports survival, combat, and shelter building without a large crafting tree.

## Requirements

### 12.1 GCI-001: wilderness build Resource Set

The wilderness build MUST use the minimal wilderness resource set.

Required resources:

- `Wood`
- `Stone`
- `Food`
- `Herbs`

Acceptance criteria:

- `ResourceType` supports all required wilderness resource values.
- Cave-only resources are not required for wilderness progression.
- Resource counters are visible in the UI or inventory view.

### 12.2 GCI-002: Gathering Sources

The wilderness build MUST provide gatherable wilderness nodes.

Required sources:

- Trees or logs reward `Wood`.
- Rocks reward `Stone`.
- Berry bushes or food nodes reward `Food`.
- Herb plants reward `Herbs`.

Acceptance criteria:

- Gathering requires player proximity.
- Gathered nodes give resources exactly once until reset by implementation-defined rules.
- Gathered node state is included in save data.

### 12.3 GCI-003: Required Recipes

The wilderness build MUST implement the core survival recipe set.

Required recipes:

| Output | Cost | Purpose |
| --- | --- | --- |
| Stone Spear | 2 Wood + 1 Stone | Basic melee weapon. |
| Bandage | 2 Herbs | Restores health. |
| Campfire | 3 Wood + 1 Stone | Safe save object and light source. |
| Bedroll | 4 Wood + 2 Herbs | Safe save object and shelter anchor. |
| Storage Box | 6 Wood | Stores items or resources. |
| Workbench | 5 Wood + 2 Stone | Crafting station. |

### 12.4 GCI-004: Crafting Rules

Crafting MUST consume resources only when the craft succeeds.

Acceptance criteria:

- Missing resources show a message.
- Successful craft subtracts exact costs.
- Successful craft adds an item, equips a tool, or creates a buildable count.
- Recipes do not depend on `GlowCrystal`, `RootFiber`, or Ancient Door keys.

### 12.5 GCI-005: Inventory Scope

The wilderness build inventory MUST remain simple but support resources, tools, and buildables.

Acceptance criteria:

- Resources are count-based.
- `Stone Spear` can be owned or equipped.
- `Bandage` can be consumed.
- Buildables can be selected for grid placement.

## Non-Goals

- No large crafting tree.
- No item rarity.
- No weight or encumbrance system.
- No cave key or crystal progression requirement.

## Dependencies

- SPEC-010 Health, Hunger, Day/Night, And Death.
- SPEC-011 Melee Combat And Wildlife.
- SPEC-013 Grid Building.
- SPEC-014 Save Slots And Menus.

## Verification

Automated wilderness build tests verify:

- The player can gather Wood, Stone, Food, and Herbs.
- Each required recipe succeeds with exact resources.
- Each required recipe fails without enough resources.
- Food restores hunger.
- Bandage restores health.
- Crafted buildables can be selected for placement.
