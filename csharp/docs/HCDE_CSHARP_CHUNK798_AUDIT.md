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

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 798 --to 798` (0 hard / 0 soft).

## Seams

`MoveFloor`, stair sector chain, tag iterators, height finders, and `CreateThinker` are injected.

## Credit rationale

Floor mover tick/texture tail and parallel elevator EV after donut/waggle/elevator tick (chunk6).
