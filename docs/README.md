# Stay Alive Specs

This folder is the spec source of truth for the Stay Alive Unity proof of concept.

## Spec Driven Development Rules

- Specs are normative. Use `MUST`, `SHOULD`, and `MAY` deliberately.
- Implemented requirements MUST have verification evidence in [testing.md](testing.md).
- Proposed requirements MUST be marked as proposed and must not be treated as implemented.
- Code, scene generation, and tests should be changed only after the relevant spec is updated.
- If implementation and docs disagree, update the spec first, then update implementation or tests.

## Spec Index

| ID | Spec | Status | File |
| --- | --- | --- | --- |
| SPEC-001 | Core Game Loop | Implemented | [core-loop.md](core-loop.md) |
| SPEC-002 | Controls | Implemented | [controls.md](controls.md) |
| SPEC-003 | Runtime Systems | Implemented | [systems.md](systems.md) |
| SPEC-004 | Content And Assets | Implemented | [content-and-assets.md](content-and-assets.md) |
| SPEC-005 | Testing And Verification | Implemented | [testing.md](testing.md) |
| SPEC-006 | Crafting | Implemented | [crafting.md](crafting.md) |
| SPEC-007 | Enemies | Implemented | [enemies.md](enemies.md) |

## Current Authoritative Scene

- Scene: `Assets/Scenes/SampleScene.unity`
- Scene generator: `Assets/Editor/StayAlivePrototypeBuilder.cs`
- Test fixture: `Assets/Editor/StayAlivePrototypeTests.cs`
- Automation report: `Logs/codex-unity-automation.json`

## Current Implemented Loop

The implemented proof of concept is:

`Spawn -> Mine weak gate -> Mine 3 Glow Crystals -> Open Ancient Door -> Enter reward room -> Complete`

The current build also includes Workbench crafting, crafted torch placement, and one light-sensitive Cave Crawler placed after the crystal chamber.
