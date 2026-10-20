# Chunk 416 — LS_Stairs_* / Generic_Stairs and Thing_Raise/Stop/SetGoal

## Scope

LS_Stairs_* / Generic_Stairs and Thing_Raise/Stop/SetGoal

## Native ranges

- `src/playsim/p_lnspec.cpp` **LS_Floor_Stop / LS_Stairs_*** 569–650 (82 lines)
- `src/playsim/p_lnspec.cpp` **LS_Thing_Raise** 1759–1847 (89 lines)

**Credited native LOC:** 171 (tally `nativeLoc` 171).

## Managed

`VmPLnspecArgMacros.cs`, `VmPLnspecStairsLineSpecials.cs`, `VmPLnspecStairsThingTests.cs`, `VmPLnspecThingRaiseStop.cs`
Managed LOC (tally): 104.
Conversion commit: `22f4102be58edbf4cb7c785a5157f29ec25f7227`.

## Tests

- `VmPLnspecStairsThingTests.StairsBuildUp_UsesTicsDelay`
- `VmPLnspecStairsThingTests.GenericStairs_TogglesDirectionBit`
- `VmPLnspecStairsThingTests.ThingStop_ZeroesVelocityWhenActorPresent`
- `VmPLnspecStairsThingTests.ThingSetGoal_ClearsChaseGoalWhenArgZero`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 416 --to 416` (0 hard / 0 soft).

## Seams

`EV_BuildStairs`, `P_Thing_Raise`, and patrol iterators are not hosted.

## Credit rationale

Credits stair builder and thing raise/stop/goal argument wiring from `p_lnspec.cpp`.
