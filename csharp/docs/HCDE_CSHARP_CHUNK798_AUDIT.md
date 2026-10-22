# Chunk 798 — `DFloor::Tick`, `SetFloorChangeType`, floor/elevator sounds, and `EV_DoElevator` (`mapthinkers/a_floor.cpp` 114–225, 233–251, 259–262, 992–995, 1010–1095).

## Scope

`DFloor::Tick`, `SetFloorChangeType`, floor/elevator sounds, and `EV_DoElevator` (`mapthinkers/a_floor.cpp` 114–225, 233–251, 259–262, 992–995, 1010–1095).

## Native ranges

- `src/playsim/mapthinkers/a_floor.cpp` **DFloor::Tick** 114–225 (112 lines)
- `src/playsim/mapthinkers/a_floor.cpp` **DFloor::SetFloorChangeType** 233–251 (19 lines)
- `src/playsim/mapthinkers/a_floor.cpp` **DFloor::StartFloorSound** 259–262 (4 lines)
- `src/playsim/mapthinkers/a_floor.cpp` **DElevator::StartFloorSound** 992–995 (4 lines)
- `src/playsim/mapthinkers/a_floor.cpp` **FLevelLocals::EV_DoElevator** 1010–1095 (86 lines)

**Credited native LOC:** 225 (tally `nativeLoc` 225).

## Managed

`VmAFloorTickElevator.cs`
Managed LOC (tally): 363.
Conversion commit: `55b4f49b26a7477a0a1d77a6153c9f3001a5068f`.

## Padding review (2026-10-21)

| Managed | Native (`a_floor.cpp`) |
|---------|----------------------|
| `Tick` | `DFloor::Tick` **114–225** |
| `SetFloorChangeType` | `DFloor::SetFloorChangeType` **233–251** |
| `StartFloorSound` | `DFloor::StartFloorSound` **259–262** |
| `StartElevatorFloorSound` | `DElevator::StartFloorSound` **992–995** |
| `EvDoElevator` | `FLevelLocals::EV_DoElevator` **1010–1095** |


**Post-review ratio:** 363/225 managed/native ≈ 161% (src file LOC at HEAD; see tally).

## Tests

- `VmAFloorTickElevatorTests.Tick_PastDest_ClearsFloorDataAndDestroys`
- `VmAFloorTickElevatorTests.Tick_BuildStairPastDest_BecomesWaitStair`
- `VmAFloorTickElevatorTests.SetFloorChangeType_ModelZero_SetsGenFloorChg0`
- `VmAFloorTickElevatorTests.EvDoElevator_Raise_SetsDestinations`
- `VmAFloorTickElevatorTests.EvDoElevator_CurrentWithoutLine_ReturnsFalse`

## Verified

`bash csharp/tools/verify_full_solution_tests.sh` (full solution build + all 14 test projects); `python3 tools/reaudit_chunks.py audit --from 798 --to 798` (0 hard / 0 soft).

## Test gate incident (2026-10-22)

Tally commit `b2c9e283` added `Tick_BuildStairPastDest_BecomesWaitStair` using the nonexistent type `VmAFloorTickElevator.TickContext` (correct name: `FloorTickContext`). `land_chunk_fast.sh` ran `verify_full_solution_tests.sh` **before** that second commit, so the pushed tree never passed a full build at HEAD. The verify script itself did not ignore build errors; the land ordering gap did. **Remediation:** verify now runs after the tally commit (immediately before push); verify fails on nonzero `dotnet build`, on any `Failed!` assembly line, or when fewer than 14 expected `Passed!` project summaries are present. Helper-only chunk **799** (`VmAFloorCreate` / `chunk799.json`) never appeared on `origin/cursor/native-managed-matrix-3df6` (local-only commit `291c36c9`, reverted locally, not pushed).

## Seams

`MoveFloor`, stair sector chain, tag iterators, height finders, and `CreateThinker` are injected.

## Credit rationale

Floor mover tick/texture tail and parallel elevator EV after donut/waggle/elevator tick (chunk6).
