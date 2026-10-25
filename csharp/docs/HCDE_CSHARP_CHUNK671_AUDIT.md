# Chunk 671 — Whole `DPolyDoor::Tick`, `EV_OpenPolyDoor`, `EV_StopPoly` (`po_man.cpp` 527–682, 690–712).

## Scope

Whole `DPolyDoor::Tick`, `EV_OpenPolyDoor`, `EV_StopPoly` (`po_man.cpp` 527–682, 690–712).

## Native ranges

- `src/playsim/po_man.cpp` **DPolyDoor::Tick** 527–625 (99 lines)
- `src/playsim/po_man.cpp` **EV_OpenPolyDoor** 633–682 (50 lines)
- `src/playsim/po_man.cpp` **EV_StopPoly** 690–703 (14 lines)

**Credited native LOC:** 163 (tally `nativeLoc` 163).

## Managed

`VmPoManPolyDoor.cs`
Managed LOC (tally): 186.
Conversion commit: `663ed7e70343f568ffd0f1508f7f6e86e1a033ef`.

## Padding review (2026-10-21)

| Managed | Native (`po_man.cpp`) |
|---------|-------------------------|
| `PolyDoorTick` | `DPolyDoor::Tick` 527–625 |
| `OpenPolyDoor` | `EV_OpenPolyDoor` 633–682 |
| `StopPoly` | `EV_StopPoly` 690–712 |

### Out-of-body uncredit (671)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **172** |
| **Out-of-body removed** | **9** |
| **Honest** | **163** |


**Post-review ratio:** 186/163 managed/native ≈ 114% (src file LOC at HEAD; see tally).

## Tests

- `VmPoManPolyDoorTests.PolyDoorTick_SlideClosesAndDestroys`
- `VmPoManPolyDoorTests.PolyDoorTick_WaitsOnTics`
- `VmPoManPolyDoorTests.OpenPolyDoor_StartsOnFirstFreeMirror`
- `VmPoManPolyDoorTests.OpenPolyDoor_WalksMirrorChainWithoutCycling`
- `VmPoManPolyDoorTests.OpenPolyDoor_AbortsWhenPolyBlocked`
- `VmPoManPolyDoorTests.StopPoly_InvokesStopWhenSpecialData`
- `VmPoManPolyDoorTests.StopPoly_ReturnsFalseWhenPolyMissing`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 671 --to 671` (0 hard / 0 soft).

## Seams

Sound sequences and thinker creation remain native; mirror chain walk matches `FPolyMirrorIterator`.

## Credit rationale

Poly door thinker and line special movers.
