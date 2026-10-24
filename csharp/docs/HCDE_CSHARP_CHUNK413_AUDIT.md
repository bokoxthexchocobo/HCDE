# Chunk 413 — LS_Floor_* movers and Generic_Floor dispatch

## Scope

LS_Floor_* movers and Generic_Floor dispatch

## Native ranges

- `src/playsim/p_lnspec.cpp` **LS_Floor_* core** 336–471 (136 lines)
- `src/playsim/p_lnspec.cpp` **LS_Floor_RaiseToCeiling** 484–501 (18 lines)

**Credited native LOC:** 154 (tally `nativeLoc` 154).

## Managed

`VmPLnspecArgMacros.cs`, `VmPLnspecFloorCeilingTests.cs`, `VmPLnspecFloorLineSpecials.cs`
Managed LOC (tally): 173.
Conversion commit: `48ec1a79971be88a3be75aef1eb4c202d4002586`.

## Tests

- `VmPLnspecFloorCeilingTests.FloorLowerByValueTimes8_ScalesHeight`
- `VmPLnspecFloorCeilingTests.GenericFloor_SelectsRaiseToHighest`
- `VmPLnspecFloorCeilingTests.Speed_DividesByEight`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 413 --to 413` (0 hard / 0 soft).

## Seams

`EV_DoFloor` and live sector motion are not hosted in these bands.

## Credit rationale

Credits native `LS_Floor_*` argument wiring and `LS_Generic_Floor` target selection from `p_lnspec.cpp`.
