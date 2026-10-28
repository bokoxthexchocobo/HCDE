# Chunk 920 — `DElevator::StartFloorSound`, `EV_DoElevator`, and `EV_DoChange` from `mapthinkers/a_floor.cpp` (**996–1154**). Post–chunk **6** island (**941–984** skipped); donut / VM glue (**516–529**, **796–863**) on **921**.

## Scope

`DElevator::StartFloorSound`, `EV_DoElevator`, and `EV_DoChange` from `mapthinkers/a_floor.cpp` (**996–1154**). Post–chunk **6** island (**941–984** skipped); donut / VM glue (**516–529**, **796–863**) on **921**.

## Native ranges

- `src/playsim/mapthinkers/a_floor.cpp` **DElevator::StartFloorSound** 992–995 (4 lines)
- `src/playsim/mapthinkers/a_floor.cpp` **FLevelLocals::EV_DoElevator** 996–1095 (100 lines)
- `src/playsim/mapthinkers/a_floor.cpp` **FLevelLocals::EV_DoChange** 1098–1154 (57 lines)

**Credited native LOC:** 161 (tally `nativeLoc` 161).

## Managed

`VmAFloorEvDonutChange920Slice.cs`, `VmAFloorEvElevator920Slice.cs`
Managed LOC (tally): 189.
Conversion commit: `a61715d076bd4fe02cf8a6973075fa66dde92223`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| `DElevator::StartFloorSound` (**992–995**) | yes |
| `EV_DoElevator` (**996–1095**) | yes |
| `EV_DoChange` (**1098–1154**) | yes |

| Managed | Native (`a_floor.cpp`) |
|---------|----------------------|
| `VmAFloorEvElevator920Slice.ElevatorStartFloorSound` | **992–995** |
| `VmAFloorEvElevator920Slice.EvDoElevator` | **996–1095** |
| `VmAFloorEvDonutChange920Slice.EvDoChange` | **1098–1154** |

### Chunk **920** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **161** |
| **Net headline** | **+161** vs post-**919** |


**Post-review ratio:** 189/161 managed/native ≈ 117% (src file LOC at HEAD; see tally).

## Tests

- `VmAFloorTickElevatorTests.EvDoElevator_Raise_SetsDestinations`
- `VmAFloorTickElevatorTests.EvDoElevator_CurrentWithoutLine_ReturnsFalse`
- `VmAFloorTickElevatorTests.EvDoElevator_SkipsSectorWhenFloorAlreadyMoving`
- `VmAFloorEvTests.EvDoChange_TrigChange_CopiesFrontTexture`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 920 --to 920` (0 hard / 0 soft).

## Seams

Tag iterators, thinker spawn, height finders, texture/special hooks, and static `StartFloorSound` are injected.

## Credit rationale

`--band` hull **992–1154** avoids chunk **919** (**≤788**) and chunk **6** tick (**941–984**).
