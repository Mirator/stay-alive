# Stay Alive Specs

This folder is the active spec source of truth for Stay Alive. Specs 008 through 015 describe the implemented outdoor wilderness sandbox. Specs 016 and later describe implemented UX, menu, and control improvements.

The earlier cave baseline documents are archived in [archive/cave-baseline/README.md](archive/cave-baseline/README.md). Archived documents are historical reference only and are not current scope.

## Spec Driven Development Rules

- Specs are normative. Use `MUST`, `SHOULD`, and `MAY` deliberately.
- Implemented requirements MUST have verification evidence in [testing-and-acceptance.md](testing-and-acceptance.md).
- Proposed requirements MUST be marked as proposed and must not be treated as implemented.
- Code, scene generation, and tests should be changed only after the relevant spec is updated.
- If implementation and docs disagree, update the spec first, then update implementation or tests.

## Wilderness Implemented Specs

| No. | ID | Spec | Status | File |
| --- | --- | --- | --- | --- |
| 8 | SPEC-008 | Wilderness Survival Loop | Implemented | [wilderness-survival-loop.md](wilderness-survival-loop.md) |
| 9 | SPEC-009 | Wilderness World And Content | Implemented | [wilderness-world-and-content.md](wilderness-world-and-content.md) |
| 10 | SPEC-010 | Health, Hunger, Day/Night, And Death | Implemented | [vitals-day-night-death.md](vitals-day-night-death.md) |
| 11 | SPEC-011 | Melee Combat And Wildlife | Implemented | [melee-combat-and-wildlife.md](melee-combat-and-wildlife.md) |
| 12 | SPEC-012 | Gathering, Crafting, And Inventory | Implemented | [gathering-crafting-inventory.md](gathering-crafting-inventory.md) |
| 13 | SPEC-013 | Grid Building | Implemented | [grid-building.md](grid-building.md) |
| 14 | SPEC-014 | Save Slots And Menus | Implemented | [save-slots-and-menus.md](save-slots-and-menus.md) |
| 15 | SPEC-015 | Testing And Acceptance | Implemented | [testing-and-acceptance.md](testing-and-acceptance.md) |

## UX, Menu, And Controls Implemented Specs

| No. | ID | Spec | Status | File |
| --- | --- | --- | --- | --- |
| 16 | SPEC-016 | First-Session Onboarding And Guidance | Implemented | [first-session-onboarding-and-guidance.md](first-session-onboarding-and-guidance.md) |
| 17 | SPEC-017 | HUD, Interaction, Crafting, And Building UX | Implemented | [hud-interaction-crafting-building-ux.md](hud-interaction-crafting-building-ux.md) |
| 18 | SPEC-018 | Feedback, Readability, And Accessibility | Implemented | [feedback-readability-accessibility.md](feedback-readability-accessibility.md) |
| 19 | SPEC-019 | Game Menu Flow And States | Implemented | [game-menu-flow-and-states.md](game-menu-flow-and-states.md) |
| 20 | SPEC-020 | Wilderness Controls | Implemented | [controls.md](controls.md) |

## Current Authoritative Scene

- Scene: `Assets/Scenes/SampleScene.unity`
- Scene generator: `Assets/Editor/StayAliveWildernessSceneBuilder.cs`
- Test fixture: `Assets/Editor/StayAliveEditModeTests.cs`
- Automation report: `Logs/codex-unity-automation.json`

## Current Implemented Loop

The implemented wilderness build is an outdoor wilderness sandbox:

`Start in the wild -> gather minimal resources -> craft survival tools -> build a grid shelter -> fight or avoid wild animals -> survive day/night cycles -> save manually in one of 3 slots -> continue indefinitely`

There is no winning condition in the wilderness build. The first acceptance target is a stable, playable first 3 in-game days, after which the sandbox continues with repeated survival pressure.
