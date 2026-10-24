# Chunk 414 — LS_Ceiling_* crush and move specials

## Scope

LS_Ceiling_* crush and move specials

## Native ranges

- `src/playsim/p_lnspec.cpp` **LS_Ceiling_*** 670–835 (166 lines)

**Credited native LOC:** 166 (tally `nativeLoc` 166).

## Managed

`VmPLnspecCeilingLineSpecials.cs`
Managed LOC (tally): 87.
Conversion commit: `2c86343b9c01ce279f79e3f82f1ccb477ed426c2`.

## Tests

- `VmPLnspecFloorCeilingTests.CeilingCrushStop_RemoveDefaultsHexen`
- `VmPLnspecFloorCeilingTests.CeilingLowerInstant_ScalesHeight`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 414 --to 414` (0 hard / 0 soft).

## Seams

`EV_DoCeiling` thinkers are not hosted in these bands.

## Credit rationale

Credits native `LS_Ceiling_*` argument wiring including crush-stop gametype policy from `p_lnspec.cpp`.
