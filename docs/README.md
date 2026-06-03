# Stay Alive Specs

This folder is the spec source of truth for Stay Alive. The first seven specs describe the implemented cave proof of concept baseline. Specs 008 and later describe the implemented outdoor wilderness sandbox MVP.

## Spec Driven Development Rules

- Specs are normative. Use `MUST`, `SHOULD`, and `MAY` deliberately.
- Implemented requirements MUST have verification evidence in [testing.md](testing.md) or [mvp-testing-and-acceptance.md](mvp-testing-and-acceptance.md).
- Proposed requirements MUST be marked as proposed and must not be treated as implemented.
- Code, scene generation, and tests should be changed only after the relevant spec is updated.
- If implementation and docs disagree, update the spec first, then update implementation or tests.

## PoC Baseline Specs

| No. | ID | Spec | Status | File |
| --- | --- | --- | --- | --- |
| 1 | SPEC-001 | Core Game Loop | Implemented | [core-loop.md](core-loop.md) |
| 2 | SPEC-002 | Controls | Implemented | [controls.md](controls.md) |
| 3 | SPEC-003 | Runtime Systems | Implemented | [systems.md](systems.md) |
| 4 | SPEC-004 | Content And Assets | Implemented | [content-and-assets.md](content-and-assets.md) |
| 5 | SPEC-005 | Testing And Verification | Implemented | [testing.md](testing.md) |
| 6 | SPEC-006 | Crafting | Implemented | [crafting.md](crafting.md) |
| 7 | SPEC-007 | Enemies | Implemented | [enemies.md](enemies.md) |

## MVP Implemented Specs

| No. | ID | Spec | Status | File |
| --- | --- | --- | --- | --- |
| 8 | SPEC-008 | Wilderness Survival Loop | Implemented | [wilderness-survival-loop.md](wilderness-survival-loop.md) |
| 9 | SPEC-009 | Wilderness World And Content | Implemented | [wilderness-world-and-content.md](wilderness-world-and-content.md) |
| 10 | SPEC-010 | Health, Hunger, Day/Night, And Death | Implemented | [vitals-day-night-death.md](vitals-day-night-death.md) |
| 11 | SPEC-011 | Melee Combat And Wildlife | Implemented | [melee-combat-and-wildlife.md](melee-combat-and-wildlife.md) |
| 12 | SPEC-012 | Gathering, Crafting, And Inventory | Implemented | [gathering-crafting-inventory.md](gathering-crafting-inventory.md) |
| 13 | SPEC-013 | Grid Building | Implemented | [grid-building.md](grid-building.md) |
| 14 | SPEC-014 | Save Slots And Menus | Implemented | [save-slots-and-menus.md](save-slots-and-menus.md) |
| 15 | SPEC-015 | MVP Testing And Acceptance | Implemented | [mvp-testing-and-acceptance.md](mvp-testing-and-acceptance.md) |

## Current Authoritative Scene

- Scene: `Assets/Scenes/SampleScene.unity`
- Scene generator: `Assets/Editor/StayAliveMvpBuilder.cs`
- Test fixture: `Assets/Editor/StayAlivePrototypeTests.cs`
- Automation report: `Logs/codex-unity-automation.json`

## Current Implemented Loop

The implemented proof of concept is:

`Spawn -> Mine weak gate -> Mine 3 Glow Crystals -> Open Ancient Door -> Enter reward room -> Complete`

The current build also includes Workbench crafting, crafted torch placement, and one light-sensitive Cave Crawler placed after the crystal chamber.

## Current MVP Loop

The implemented MVP is an outdoor wilderness sandbox:

`Start in the wild -> gather minimal resources -> craft survival tools -> build a grid shelter -> fight or avoid wild animals -> survive day/night cycles -> save manually in one of 3 slots -> continue indefinitely`

There is no winning condition in the MVP. The first acceptance target is a stable, playable first 3 in-game days, after which the sandbox continues with repeated survival pressure.
