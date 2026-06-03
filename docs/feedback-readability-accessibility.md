# 18. SPEC-018: Feedback, Readability, And Accessibility

Status: Implemented

## Purpose

Define UX feedback, readability, and basic accessibility expectations for the wilderness sandbox.

## Design Goal

Important game state changes should be visible, understandable, and readable without relying on a single color cue.

## Requirements

### 18.1 FRA-001: Action Feedback

The game SHOULD provide clear feedback for important player actions and state changes.

Required feedback cases:

- Gather.
- Craft.
- Build.
- Save.
- Damage.
- Healing.
- Hunger warning.
- Enemy alert.
- Invalid action.
- Death.

Acceptance criteria:

- Feedback appears close to the action, HUD, or message area.
- Feedback is short enough to read during play.
- Repeated messages do not permanently obscure the HUD.
- Death feedback connects to SPEC-014 recovery options.

### 18.2 FRA-002: Placement Feedback Beyond Color

Build placement validity MUST NOT rely on color alone.

Acceptance criteria:

- Invalid placement has text, icon, outline pattern, or another non-color cue.
- Invalid placement explains the reason when possible.
- Successful placement has a distinct confirmation cue.
- Failed placement still does not consume resources or buildable counts.

### 18.3 FRA-003: Text Readability

Player-facing text SHOULD be readable at the default resolution.

Acceptance criteria:

- Text has sufficient contrast against its background.
- Text does not overlap other HUD elements at the default resolution.
- Important messages stay on screen long enough to read.
- Long labels wrap or shorten instead of overflowing.

### 18.4 FRA-004: Pause And Help Overlay

The pause state SHOULD include a compact help overlay.

Acceptance criteria:

- The overlay lists current wilderness controls.
- The overlay lists survival basics: gather, craft, build, save, eat, heal, and fight or avoid wildlife.
- The overlay can be dismissed by resuming the game.
- The overlay does not imply a win condition.

## Non-Goals

- No full accessibility settings screen.
- No localization pass.
- No screen reader or voiceover requirement.
- No gamepad, touch, or control rebinding requirement.

## Public Interfaces

This spec does not require a new runtime type, but it depends on the view-data contracts implemented by SPEC-017.

## Dependencies

- SPEC-010 Health, Hunger, Day/Night, And Death.
- SPEC-011 Melee Combat And Wildlife.
- SPEC-013 Grid Building.
- SPEC-014 Save Slots And Menus.
- SPEC-017 HUD, Interaction, Crafting, And Building UX.

## Verification

Automated tests verify:

- Required feedback cases can be triggered.
- Invalid build placement exposes a non-color reason.
- Default HUD text does not overlap in manual smoke testing.
- Pause/help overlay lists current controls.
