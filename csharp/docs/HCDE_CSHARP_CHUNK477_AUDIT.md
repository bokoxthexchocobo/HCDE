# Chunk 477 — aim_t AimTraverse intercept loop (lines, things, smart aim)

## Scope

aim_t AimTraverse intercept loop (lines, things, smart aim)

## Native ranges

- `src/playsim/p_map.cpp` **aim_t::AimTraverse** 4294–4370 (77 lines)
- `src/playsim/p_map.cpp` **aim_t::AimTraverse** 4372–4409 (38 lines)
- `src/playsim/p_map.cpp` **aim_t::AimTraverse** 4410–4492 (83 lines)
- `src/playsim/p_map.cpp` **aim_t::AimTraverse** 4494–4565 (72 lines)

**Credited native LOC:** 270 (tally `nativeLoc` 270).

## Managed

`VmPMapAimTraverseMain.cs`, `VmPMapAimTraverseMainTests.cs`
Managed LOC (tally): 301.
Conversion commit: `b7e803711d3ac7b5e156ba12f087dfacd17285b6`.

## Tests

- `VmPMapAimTraverseMainTests.StopWhenBetterFound_SkipsFartherFrac`
- `VmPMapAimTraverseMainTests.LinePortalEntry_FrontOnly`
- `VmPMapAimTraverseMainTests.LineIntercept_StopsOnBlock`
- `VmPMapAimTraverseMainTests.LineIntercept_EntersPortal`
- `VmPMapAimTraverseMainTests.SkipThingForAim_NonShootable`
- `VmPMapAimTraverseMainTests.SkipThingForAim_GhostThruWeapon`
- `VmPMapAimTraverseMainTests.ExitPointClearsTopShot`
- `VmPMapAimTraverseMainTests.FfSightNarrow_StopWhenRangeCloses`
- `VmPMapAimTraverseMainTests.Check3dRangeExceeded_WhenBeyondAttackRange`
- `VmPMapAimTraverseMainTests.SmartAim_RecordsFriend`
- `VmPMapAimTraverseMainTests.SmartAim_HitsMonster`
- `VmPMapAimTraverseMainTests.MidThingPitch_Averages`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 477 --to 477` (0 hard / 0 soft).

## Seams

`FPathTraverse`, `P_LineOpening`, `P_CheckSight`, and `SetResult` are not hosted.

## Credit rationale

Credits aim traverse early-out, two-sided line pitch narrowing, thing visibility filters, 3D-floor pitch clamps, sight narrowing, CHECK3D range, and smart-aim friend/barrel deferral from `p_map.cpp` (4294–4565).
