# Chunk 798 — `DFloor::Tick`, `SetFloorChangeType`, floor/elevator sounds, and `EV_DoElevator` (`mapthinkers/a_floor.cpp` 114–225, 233–251, 259–262, 992–995, 1010–1095). Retired (2026-10-23): out-of-body credit removed; honest **139** < **150**.

## Scope

`DFloor::Tick`, `SetFloorChangeType`, floor/elevator sounds, and `EV_DoElevator` (`mapthinkers/a_floor.cpp` 114–225, 233–251, 259–262, 992–995, 1010–1095). Retired (2026-10-23): out-of-body credit removed; honest **139** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `18fdb015092957e749bf724c34412d9f87642099`.

## Padding review (2026-10-21)

| Managed | Native (`a_floor.cpp`) |
|---------|----------------------|
| `Tick` | `DFloor::Tick` **114–225** |
| `SetFloorChangeType` | `DFloor::SetFloorChangeType` **233–251** |
| `StartFloorSound` | `DFloor::StartFloorSound` **259–262** |
| `StartElevatorFloorSound` | `DElevator::StartFloorSound` **992–995** |
| `EvDoElevator` | `FLevelLocals::EV_DoElevator` **1010–1095** |

### Out-of-body uncredit (798)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **225** |
| **Out-of-body removed** | **86** |
| **Honest** | **139** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 798 --to 798` (0 hard / 0 soft).

## Seams

`MoveFloor`, stair sector chain, tag iterators, height finders, and `CreateThinker` are injected.

## Credit rationale

Floor mover tick/texture tail and parallel elevator EV after donut/waggle/elevator tick (chunk6).
