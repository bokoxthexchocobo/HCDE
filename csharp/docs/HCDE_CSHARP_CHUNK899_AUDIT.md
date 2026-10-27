# Chunk 899 — `LS_Door_*` through `LS_Floor_LowerToLowestCeiling` (`p_lnspec.cpp` **241–470**). Follows **898** polyobj band (**≤239**); stops before `LS_Floor_RaiseByTexture` (**472+**). No `VmPLnspecDoorsPillars` (padding audit reject).

## Scope

`LS_Door_*` through `LS_Floor_LowerToLowestCeiling` (`p_lnspec.cpp` **241–470**). Follows **898** polyobj band (**≤239**); stops before `LS_Floor_RaiseByTexture` (**472+**). No `VmPLnspecDoorsPillars` (padding audit reject).

## Native ranges

- `src/playsim/p_lnspec.cpp` **LS_Door_Close** 241–470 (230 lines)

**Credited native LOC:** 230 (tally `nativeLoc` 230).

## Managed

`VmPLnspecDoorFloorLineSpecials.cs`
Managed LOC (tally): 252.
Conversion commit: `c0803458234bd07c01e191c0903a107b128bc453`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Managed | Native (`p_lnspec.cpp`) |
|---------|-------------------------|
| `DoorClose` … `GenericDoor` | `LS_Door_*` **241–334** |
| `FloorLowerByValue` … `FloorLowerToLowestCeiling` | `LS_Floor_*` **336–470** |

### vs **898**

| | Lines |
|--|------:|
| **898** (polyobj / macros) | **60–239** |
| **899** (this row) | **241–470** (**231**) |
| `LS_Floor_RaiseByTexture` | **472+** |

### Gross vs net

| | Value |
|--|------:|
| Gross `nativeLoc` | **231** |
| Net behavior-ported | **231** |


**Post-review ratio:** 252/230 managed/native ≈ 110% (src file LOC at HEAD; see tally).

## Tests

- `VmPLnspecDoorFloorLineSpecialsTests.DoorLockedRaise_OpensWhenDelayZero`
- `VmPLnspecDoorFloorLineSpecialsTests.GenericDoor_InvalidKindReturnsFalse`
- `VmPLnspecDoorFloorLineSpecialsTests.GenericDoor_LocalLightTagSwapsTag`
- `VmPLnspecDoorFloorLineSpecialsTests.DoorAnimated_BlocksWithoutKeys`
- `VmPLnspecDoorFloorLineSpecialsTests.FloorMoveToValue_NegativeHeight`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 899 --to 899` (0 hard / 0 soft).

## Seams

`EV_DoDoor`, `EV_DoFloor`, `EV_SlidingDoor`, `P_CheckKeys`, and `EV_FloorCrushStop` are not hosted.

## Credit rationale

Door speed/light packing, locked-raise delay branch, generic door BOOM bits, sliding-door key gate, and floor height/crush/change argument wiring via `VmPLnspecDoorFloorLineSpecials`.
