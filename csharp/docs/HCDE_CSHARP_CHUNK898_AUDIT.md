# Chunk 898 — `ChangeMap`/crush helpers, `MODtoDamageType`, `LS_NOP`, and polyobj movers (`p_lnspec.cpp` **60–239**). Front slice of **1–1550** conversion gap; stops before `LS_Door_Close` (**241+**).

## Scope

`ChangeMap`/crush helpers, `MODtoDamageType`, `LS_NOP`, and polyobj movers (`p_lnspec.cpp` **60–239**). Front slice of **1–1550** conversion gap; stops before `LS_Door_Close` (**241+**).

## Native ranges

- `src/playsim/p_lnspec.cpp` **ChangeMap** 60–239 (180 lines)

**Credited native LOC:** 180 (tally `nativeLoc` 180).

## Managed

`VmPLnspecArgMacros.cs`, `VmPLnspecPolyobjLineSpecials.cs`
Managed LOC (tally): 137.
Conversion commit: `dd7b41a6fe86ae30eaa6a48a0d3addb42e4cc1c8`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Managed | Native (`p_lnspec.cpp`) |
|---------|-------------------------|
| `VmPLnspecArgMacros` | `ChangeMap` **60**, `CRUSHTYPE` **72–83**, macro helpers **65–70** |
| `VmPLnspecPolyobjLineSpecials` | `NamedACSToNormalACS` **89–98**, `MODtoDamageType` **100–122**, `LS_NOP` + polyobj **126–239** |

### Gross vs net

| | Value |
|--|------:|
| Gross `nativeLoc` | **180** |
| Net behavior-ported | **180** |


**Post-review ratio:** 137/180 managed/native ≈ 76% (src file LOC at HEAD; see tally).

## Tests

- `VmPLnspecPolyobjLineSpecialsTests.ModToDamageType_Telefrag`
- `VmPLnspecPolyobjLineSpecialsTests.MoveTimes8_MultipliesDistance`
- `VmPLnspecPolyobjLineSpecialsTests.DoorSlide_UsesSlideKind`
- `VmPLnspecPolyobjLineSpecialsTests.MoveToSpot_FailsWithoutActor`
- `VmPLnspecFloorCeilingTests.Speed_DividesByEight`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 898 --to 898` (0 hard / 0 soft).

## Seams

`EV_RotatePoly`, `EV_MovePoly`, `EV_OpenPolyDoor`, `EV_StopPoly`, and `SN_StopSequence` are not hosted.

## Credit rationale

Credits EE change remap, damage-type name mapping, ACS opcode table, and polyobj argument packing via `VmPLnspecArgMacros` + `VmPLnspecPolyobjLineSpecials`.
