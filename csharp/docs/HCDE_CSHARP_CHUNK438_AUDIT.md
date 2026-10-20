# Chunk 438 — P_TestActivateLine monster/lax paths and P_PredictLine teleports

## Scope

P_TestActivateLine monster/lax paths and P_PredictLine teleports

## Native ranges

- `src/playsim/p_spec.cpp` **P_TestActivateLine** 243–356 (114 lines)
- `src/playsim/p_spec.cpp` **P_PredictLine** 364–399 (36 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmPSpecTestActivateLine.cs`, `VmPSpecTestActivateLineTests.cs`
Managed LOC (tally): 87.
Conversion commit: `349061a5a335e2ae8b6fc02f02d3c3d200a8f278`.

## Tests

- `VmPSpecTestActivateLineTests.PromoteMissileTeleport_WhenCrossTeleport`
- `VmPSpecTestActivateLineTests.DoorRaiseMonsterPath_SlowSpeed`
- `VmPSpecTestActivateLineTests.PredictSpecial_AllowsTeleportLine`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 438 --to 438` (0 hard / 0 soft).

## Seams

`P_CheckSwitchRange`, `P_ExecuteSpecial`, and live mobj flags are not hosted.

## Credit rationale

Credits activation expansion, monster-use gates, secret-door blocks, lax-monster switch paths, and client predict teleport filter from `p_spec.cpp`.
