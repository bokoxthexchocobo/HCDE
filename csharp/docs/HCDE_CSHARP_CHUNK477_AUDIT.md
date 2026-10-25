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

`VmPMapAimTraverseMain.cs`
Managed LOC (tally): 299.
Conversion commit: `53fffe0f4600ac3e76a96aebf763c8472c0e6324`.

## Padding review (2026-10-21)

| Managed | Native (`p_map.cpp`) |
|---------|-------------------|
| `AdjustBottomOpening` / `AdjustTopOpening` / `AimTargetMismatch` | `aim_t::AimTraverse` 4294–4370 |
| `Check3dRangeExceeded` | `aim_t::AimTraverse` 4494–4565 |
| `ClampThingBottomPitch` / `ClampThingTopPitch` | `aim_t::AimTraverse` 4294–4370 |
| `EvaluateFfSightNarrow` | `aim_t::AimTraverse` 4410–4492 |
| `EvaluateLineIntercept` | `aim_t::AimTraverse` 4294–4370 |
| `EvaluateSmartAim` | `aim_t::AimTraverse` 4494–4565 |
| `ExitPointClearsBottomShot` | `aim_t::AimTraverse` 4294–4370 |
| `ExitPointClearsTopShot` | `aim_t::AimTraverse` 4410–4492 |
| `LineBlocksAim` / `LinePortalEntry` / `LowerBottomPitchFromFloorPlane` | `aim_t::AimTraverse` 4294–4370 |
| `MapSmartAimToResult` / `MidThingPitch` | `aim_t::AimTraverse` 4494–4565 |
| `OpeningBlocks` / `PitchRangeClosed` / `RaiseTopPitchFromCeilingPlane` / `SectorPortalOnEnter` / `ShotOverTop` / `ShotUnderBottom` | `aim_t::AimTraverse` 4294–4370 |
| `ShouldPassThroughPlayer` | `aim_t::AimTraverse` 4380 |
| `SkipSelf` | `aim_t::AimTraverse` 4294–4370 |
| `SkipThingForAim` | `aim_t::AimTraverse` 4372–4409 |
| `StopWhenBetterFound` | `aim_t::AimTraverse` 4294–4370 |
| `VecToAngle` | `aim_t::AimTraverse` 4337–4467 |

### Out-of-body uncredit (477)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **270** |
| **Out-of-body removed** | **0** |
| **Honest** | **270** |


**Post-review ratio:** 299/270 managed/native ≈ 111% (src file LOC at HEAD; see tally).

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
