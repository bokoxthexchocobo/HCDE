# Chunk 661 — Whole `P_GeometryRadiusAttack` (`p_destructible.cpp` 442–688).

## Scope

Whole `P_GeometryRadiusAttack` (`p_destructible.cpp` 442–688).

## Native ranges

- `src/playsim/p_destructible.cpp` **P_GeometryRadiusAttack** 442–688 (247 lines)

**Credited native LOC:** 247 (tally `nativeLoc` 247).

## Managed

`VmPDestructibleGeometryRadiusAttack.cs`
Managed LOC (tally): 268.
Conversion commit: `f1ed46fe7070b35aa08c40497d6f75c56daaa665`.

## Padding review (2026-10-21)

| Managed | Native (`p_destructible.cpp`) |
|---------|-------------------------------|
| `RunGeometryRadiusAttackPipeline` / `GeometryRadiusAttack` | `P_GeometryRadiusAttack` **442–688** |
| `RunSourceSectorCeilingInsert` / `RunSourceSectorFloorInsert` / `ProjectToPlane` | source sector **458–476** |
| `RunSourceFfloorsPipeline` / `ComputeFfSpot` / `FfloorEligible` | source ffloors **478–512** |
| `RunNearbyLinesPipeline` / `LineHasInteractiveGeometry` / `ComputeScaledLineDamage` | line scan **522–590** (simplified) |
| `RunDamageGroupsApplyPipeline` / `ComputeGroupFalloffDamage` | group apply **646–687** (hooked) |
| `ResolveCeilingGroupId` / `ResolveFloorGroupId` / `ResolveFfGroupId` | group id masks **463–492** |
| `ClosestPointOnLine` | `PGRA_ClosestPointOnLine2D` |

### Out-of-body uncredit (661)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **247** |
| **Out-of-body removed** | **0** |
| **Honest** | **247** |


**Post-review ratio:** 268/247 managed/native ≈ 109% (src file LOC at HEAD; see tally).

## Tests

- `VmPDestructibleGeometryRadiusAttackTests.GeometryRadiusAttack_InsertsCeilingGroup`
- `VmPDestructibleGeometryRadiusAttackTests.GeometryRadiusAttack_SkipsNullSpot`
- `VmPDestructibleGeometryRadiusAttackTests.GeometryRadiusAttack_InsertsFloorGroup`
- `VmPDestructibleGeometryRadiusAttackTests.ComputeGroupFalloffDamage_ZeroBeyondRadius`
- `VmPDestructibleGeometryRadiusAttackTests.FfloorEligible_RejectsInvertedGeometry`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 661 --to 661` (0 hard / 0 soft).

## Seams

`PGRA_InsertIfCloser`, `FBlockLinesIterator`, and `P_TraceLine` remain native.

## Credit rationale

Fresh playsim TU; sector/ffloor/line damage grouping hosted.
