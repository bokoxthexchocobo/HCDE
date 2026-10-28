# Chunk 470 — FSlide::BounceTraverse and FSlide::BounceWall

## Scope

FSlide::BounceTraverse and FSlide::BounceWall

## Native ranges

- `src/playsim/p_map.cpp` **FSlide::BounceTraverse** 3511–3564 (54 lines)
- `src/playsim/p_map.cpp` **FSlide::BounceWall** 3572–3707 (136 lines)

**Credited native LOC:** 190 (tally `nativeLoc` 190).

## Managed

`VmPMapBounceTraverse.cs`
Managed LOC (tally): 396.
Conversion commit: `b3bd1d6e9a1e71ed2045198a1ca1c9b6d2b3f5cc`.

## Padding review (2026-10-21)

| Managed | Native (`p_map.cpp`) |
|---------|----------------------|
| `RunBounceTraverseLoop` / `RunBounceTraverseInterceptStep` / `TryUpdateBlockingSlide` | `BounceTraverse` **3511–3564** |
| `EvaluateTraverseContinue` / `LineBlocksMovement` | opening + height checks **3533–3550** |
| `RunBounceWallPipeline` / `BounceWall` | `BounceWall` **3572–3707** |
| `ComputeLeadingCorner` | leading corner **3591–3606** |
| `RunFloorCeilingBounceFallback` / `PreferFloorBounce` | no-wall floor/ceiling **3609–3623** |
| `RunMissileSpecialBounceGate` | missile `SpecialBounceHit` **3631–3638** |
| `RunBounceWallReflectPipeline` / `ReflectAngle` / `MinMoveLen` | reflect velocity **3670–3697** |
| `EvaluateBounceWallDestroy` / `HorizonDestroys` / `ProjectileHitLinedef` | destroy paths **3641–3667** |
| prior helpers (`BlockEverythingLine`, `OpeningTooLow`, …) | shared predicates **3529–3605** |

### Out-of-body uncredit (470)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **190** |
| **Out-of-body removed** | **0** |
| **Honest** | **190** |


**Post-review ratio:** 396/190 managed/native ≈ 208% (src file LOC at HEAD; see tally).

## Tests

- `VmPMapBounceTraverseTests.BlockEverything_Stops`
- `VmPMapBounceTraverseTests.OpeningTooLow_Blocks`
- `VmPMapBounceTraverseTests.RunBounceTraverseLoop_UpdatesBestSlideOnBlock`
- `VmPMapBounceTraverseTests.HorizonDestroys`
- `VmPMapBounceTraverseTests.BounceWallsRequired`
- `VmPMapBounceTraverseTests.RunBounceWallPipeline_DestroyOnHorizon`
- `VmPMapBounceTraverseTests.ComputeLeadingCorner_FollowsVelocitySign`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 470 --to 470` (0 hard / 0 soft).

## Seams

`FPathTraverse`, `P_ExplodeMissile`, `FloorBounceMissile`, and `SetState` are not hosted.

## Credit rationale

Credits wall bounce trace blocking, opening height checks, horizon/sky destroy, and reflect velocity setup from `p_map.cpp` (3511–3707).
