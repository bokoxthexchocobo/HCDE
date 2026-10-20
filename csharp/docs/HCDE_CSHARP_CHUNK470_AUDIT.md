# Chunk 470 — FSlide::BounceTraverse and FSlide::BounceWall

## Scope

FSlide::BounceTraverse and FSlide::BounceWall

## Native ranges

- `src/playsim/p_map.cpp` **FSlide::BounceTraverse** 3511–3564 (54 lines)
- `src/playsim/p_map.cpp` **FSlide::BounceWall** 3572–3707 (136 lines)

**Credited native LOC:** 190 (tally `nativeLoc` 190).

## Managed

`VmPEnemyLookEx.cs`, `VmPEnemyLookExTests.cs`, `VmPInteractionDamageEarly.cs`, `VmPMapBounceTraverse.cs`, `VmPMapBounceTraverseTests.cs`, `VmPMapPortalMissile.cs`
Managed LOC (tally): 27.
Conversion commit: `53421b2029c5785355f2a52a139216e761f78ae5`.

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
