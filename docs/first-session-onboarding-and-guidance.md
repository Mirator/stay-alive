# 16. SPEC-016: First-Session Onboarding And Guidance

Status: Implemented

## Purpose

Define the UX onboarding layer for the first session of the outdoor wilderness sandbox.

## Design Goal

The first 10 minutes should teach survival basics through contextual guidance without adding a win condition, quest chain, or new core mechanics.

## Requirements

### 16.1 ONB-001: Contextual Onboarding

The game SHOULD guide a new player through the first survival loop with lightweight objectives.

Acceptance criteria:

- Guidance appears in the HUD or equivalent objective area.
- Guidance updates when the player completes the current step.
- Guidance never implies a final victory condition.
- Guidance can be ignored without blocking sandbox play.

### 16.2 ONB-002: Day 1 Learning Path

Day 1 SHOULD teach the wilderness loop in a clear order.

Recommended objective chain:

1. Gather Wood and Stone.
2. Craft a Stone Spear.
3. Gather Food and Herbs.
4. Craft a Bandage.
5. Craft and place a Campfire.
6. Save at the Campfire.
7. Build a minimal shelter.
8. Survive the first night.

Acceptance criteria:

- Each step maps to an existing wilderness build action.
- The player can complete the chain without leaving the starting wilderness area.
- The chain does not require defeating wildlife.
- Surviving the first night continues the sandbox instead of ending the game.

### 16.3 ONB-003: Danger Introduction

The onboarding layer SHOULD introduce wildlife danger without forcing a death spiral.

Acceptance criteria:

- The player receives a warning before entering a dangerous animal zone.
- The first danger message suggests avoiding or preparing, not only fighting.
- Night pressure is communicated before or during the first night.
- Death recovery remains handled by SPEC-014.

## Non-Goals

- No authored campaign.
- No final objective or win condition.
- No mandatory tutorial lockout.
- No new survival resources or crafting recipes.

## Public Interfaces

The implementation SHOULD expose this conceptual guidance contract:

- `ObjectiveGuide`: current onboarding step, completion state, next-step text, and optional target marker.

## Dependencies

- SPEC-008 Wilderness Survival Loop.
- SPEC-010 Health, Hunger, Day/Night, And Death.
- SPEC-012 Gathering, Crafting, And Inventory.
- SPEC-013 Grid Building.
- SPEC-014 Save Slots And Menus.

## Verification

Automated tests verify:

- The onboarding objective starts on Day 1.
- Completing each recommended step advances the guide.
- The guide does not create a win state.
- The first 10 minutes are covered by a manual smoke-test expectation: the player should understand gathering, crafting, building, saving, and danger without external docs.
