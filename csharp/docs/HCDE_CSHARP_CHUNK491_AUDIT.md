# Chunk 491 — P_GetThingFloorType and P_HitWater splash selection

## Scope

P_GetThingFloorType and P_HitWater splash selection

## Native ranges

- `src/playsim/p_mobj.cpp` **P_GetThingFloorType** 7475–7485 (11 lines)
- `src/playsim/p_mobj.cpp` **P_HitWater** 7500–7656 (157 lines)

**Credited native LOC:** 168 (tally `nativeLoc` 168).

## Managed

`VmPMobjHitWater.cs`
Managed LOC (tally): 241.
Conversion commit: `9a0a7b3c1e69eb05c8069e24acab5c22db0e8114`.

## Padding review (2026-10-21)

| Managed | Native (`p_mobj.cpp`) |
|---------|-------------------|
| `ResolveThingFloorType` / `UseActorFloorTerrain` / `Valid3dFloor` / `SkipUnderwaterFloor` / `WalkDownThroughFloorPortal` | `P_GetThingFloorType` 7475–7485 (7477: `if (thing->floorterrain >= 0)`; 7479: `return thing->floorterrain;`; 7483: `return thing->Sector->GetTerrain(sector_t::floor);`) |
| `HitWaterFlags` | `HitWaterFlags` 7494–7498 |
| `CompareZForCheckAbove` / `SelectFloorTerrain` / `ReturnLiquidOnHeightSecPlane` / `SkipWhenPredicting` / `CheckAboveCenter` / `Splashable3dTop` / `ZNearPlane` / `BlockBelowBottomPlane` / `UseHeightSecTerrain` / `NoSplashDefinition` / `DealDamageOnLand` / `SkipNovelLowVelocity` / `SmallSplashMass` / `AllowSplash` / `UseSmallSplashDef` / `ClientSideSmallSplash` / `LiquidWithoutSplash` / `WalkUpThroughCeilingPortal` / `TrySplashAt3dTop` / `RejectBelowBottom` / `SpawnSmallSplash` / `SpawnRegularSplash` / `ReturnLiquidOnly` | `P_HitWater` 7500–7656 (7502: `if (thing->player && (thing->player->cheats & CF_PREDICTING))`; 7503: `return false;`; 7511: `if (checkabove)`) |

### Out-of-body uncredit (491)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **173** |
| **Out-of-body removed** | **5** |
| **Honest** | **168** |


**Post-review ratio:** 241/168 managed/native ≈ 143% (src file LOC at HEAD; see tally).

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

Credits floor terrain selection, HitWater flag constants, predicting-player early out, 3D-floor splash probes, terrain splash choice, and small-splash policy from `p_mobj.cpp` (7475–7656).
