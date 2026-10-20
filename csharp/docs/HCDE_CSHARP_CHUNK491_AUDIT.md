# Chunk 491 — P_GetThingFloorType and P_HitWater splash selection

## Scope

P_GetThingFloorType and P_HitWater splash selection

## Native ranges

- `src/playsim/p_mobj.cpp` **P_GetThingFloorType** 7475–7485 (11 lines)
- `src/playsim/p_mobj.cpp` **P_HitWater** 7500–7699 (200 lines)

**Credited native LOC:** 211 (tally `nativeLoc` 211).

## Managed

`VmPMobjHitWater.cs`, `VmPMobjHitWaterTests.cs`
Managed LOC (tally): 61.
Conversion commit: `df98aebd49b4815c04bfb40e17291e30ec41a03d`.

## Tests

- `VmPMobjHitWaterTests.FloorType_UsesActorTerrain`
- `VmPMobjHitWaterTests.HitWater_SkipsPredictingPlayer`
- `VmPMobjHitWaterTests.HitWater_3dFloorSplashable`
- `VmPMobjHitWaterTests.HitWater_NoSplashWhenUndefined`
- `VmPMobjHitWaterTests.HitWater_SmallMassSplash`
- `VmPMobjHitWaterTests.HitWater_AllowsWhenNotDontSplash`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 491 --to 491` (0 hard / 0 soft).

## Seams

`Terrains`, `Spawn`, `P_DamageMobj`, and 3D-floor iteration are not hosted.

## Credit rationale

Credits floor terrain selection, predicting-player early out, 3D-floor splash probes, terrain splash choice, and small-splash policy from `p_mobj.cpp` (7475–7699).
