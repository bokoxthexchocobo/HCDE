# Chunk 470 — FSlide::BounceTraverse and FSlide::BounceWall

## Scope

FSlide::BounceTraverse and FSlide::BounceWall

## Native ranges

- `src/playsim/p_map.cpp` **FSlide::BounceTraverse** 3511–3564 (54 lines)
- `src/playsim/p_map.cpp` **FSlide::BounceWall** 3572–3707 (136 lines)

**Credited native LOC:** 190 (tally `nativeLoc` 190).

## Managed

`VmPMapBounceTraverse.cs`
Managed LOC (tally): 127.
Conversion commit: `97cdcfae6f3ca80f1e42a5ed90160689b023a919`.

## Padding review (2026-10-21)

| Managed | Native (`p_map.cpp`) |
|---------|-------------------|
| `BlockEverythingLine` / `BounceCountDepleted` / `BounceWallsEnabled` | `FSlide::BounceTraverse` 3511–3564 |
| `BoxOnWrongSide` | `FSlide::BounceWall` 3607–3691 |
| `CloserSlideFrac` / `EvaluateBounceWallComplete` / `EvaluateBounceWallDestroy` / `EvaluateBounceWallReflect` / `EvaluateTraverseContinue` | `FSlide::BounceTraverse` 3511–3564 |
| `ExplodeMissileOnHit` | `FSlide::BounceWall` 3674 |
| `HorizonDestroys` | `FSlide::BounceWall` 3572–3707 |
| `KeepAngle` | `FSlide::BounceWall` 3678 |
| `MinMoveLen` / `MissileSpecialHitBlock` / `MissileSpecialHitStop` / `OneSidedBlocks` / `OpeningTooLow` | `FSlide::BounceTraverse` 3511–3564 |
| `ProjectileHitLinedef` | `FSlide::BounceWall` 3649 |
| `ReflectAngle` | `FSlide::BounceWall` 3674 |
| `TooHigh` / `TooLow` | `FSlide::BounceTraverse` 3511–3564 |
| `UseBounceState` | `FSlide::BounceWall` 3698 |

### Out-of-body uncredit (470)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **190** |
| **Out-of-body removed** | **0** |
| **Honest** | **190** |


**Post-review ratio:** 127/190 managed/native ≈ 67% (src file LOC at HEAD; see tally).

## Tests

- `VmPMapBounceTraverseTests.BlockEverything_Stops`
- `VmPMapBounceTraverseTests.OpeningTooLow_Blocks`
- `VmPMapBounceTraverseTests.HorizonDestroys`
- `VmPMapBounceTraverseTests.BounceWallsRequired`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 470 --to 470` (0 hard / 0 soft).

## Seams

`FPathTraverse`, `P_ExplodeMissile`, `FloorBounceMissile`, and `SetState` are not hosted.

## Credit rationale

Credits wall bounce trace blocking, opening height checks, horizon/sky destroy, and reflect velocity setup from `p_map.cpp` (3511–3707).
