# Chunk 491 — P_GetThingFloorType and P_HitWater splash selection

## Scope

P_GetThingFloorType and P_HitWater splash selection

## Native ranges

- `src/playsim/p_mobj.cpp` **P_GetThingFloorType** 7475–7485 (11 lines)
- `src/playsim/p_mobj.cpp` **HitWaterFlags** 7494–7498 (5 lines)
- `src/playsim/p_mobj.cpp` **P_HitWater** 7500–7656 (157 lines)

**Credited native LOC:** 173 (tally `nativeLoc` 211).

## Managed

(see conversion commit file list)
Managed LOC (tally): 120.
Conversion commit: `6b43ad9c14f0cd711802d58a7e0ee180eb8d207e`.

## Tests

- `VmPMobjHitWaterTests.FloorType_UsesActorTerrain`
- `VmPMobjHitWaterTests.HitWater_SmallMassSplash`
- `VmPMobjHitWaterTests.HitWater_SkipsPredictingPlayer`
- `VmPMobjHitWaterTests.HitWater_3dFloorSplashable`
- `VmPMobjHitWaterTests.HitWater_NoSplashWhenUndefined`
- `VmPMobjHitWaterTests.HitWater_AllowsWhenNotDontSplash`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 491 --to 491` (0 hard / 0 soft).

## Seams

`Terrains`, `Spawn`, `P_DamageMobj`, and 3D-floor iteration are not hosted.

## Credit rationale

Credits floor terrain selection, HitWater flag constants, predicting-player early out, 3D-floor splash probes, terrain splash choice, and small-splash policy from `p_mobj.cpp` (7475–7656).
