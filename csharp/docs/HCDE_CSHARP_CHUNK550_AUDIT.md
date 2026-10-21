# Chunk 550 — A_FaceMovementDirection, A_CopySpriteFrame, A_SetVisibleRotation, A_SetTranslation

## Scope

A_FaceMovementDirection, A_CopySpriteFrame, A_SetVisibleRotation, A_SetTranslation

## Native ranges

- `src/playsim/p_actionfunctions.cpp` **FMDFlags** 4897–4902 (6 lines)
- `src/playsim/p_actionfunctions.cpp` **A_FaceMovementDirection** 4903–4983 (81 lines)
- `src/playsim/p_actionfunctions.cpp` **CPSFFlags** 4991–4995 (5 lines)
- `src/playsim/p_actionfunctions.cpp` **A_CopySpriteFrame** 4997–5015 (19 lines)
- `src/playsim/p_actionfunctions.cpp` **VRFFlags** 5024–5030 (7 lines)
- `src/playsim/p_actionfunctions.cpp` **A_SetVisibleRotation** 5032–5067 (36 lines)
- `src/playsim/p_actionfunctions.cpp` **A_SetTranslation** 5075–5082 (8 lines)

**Credited native LOC:** 162 (tally `nativeLoc` 162).

## Managed

`ZccStackVariableEmit.cs`, `ZccStackVariableEmitTests.cs`, `ZccSuperGlobalCvarResolve.cs`
Managed LOC (tally): 139.
Conversion commit: `4b9d8720d150d0775523870e85b5ee1e6b78b0f9`.

## Tests

- `VmActorFaceMovementCopySpriteTests.EarlyFaceMovementFail_BothNoPitchAndNoAngle`
- `VmActorFaceMovementCopySpriteTests.ClampYawDelta_RespectsLimit`
- `VmActorFaceMovementCopySpriteTests.ClampYawDelta_WithinLimitUsesTarget`
- `VmActorFaceMovementCopySpriteTests.ClampPitchDelta_WhenOverLimit`
- `VmActorFaceMovementCopySpriteTests.PlanFaceMovement_AdjustsYawAndPitch`
- `VmActorFaceMovementCopySpriteTests.ShouldAdjustYaw_WhenMoving`
- `VmActorFaceMovementCopySpriteTests.CopySpriteFrameShouldFail_WhenBothSkipFlags`
- `VmActorFaceMovementCopySpriteTests.ShouldAssignVisibleStartAngle_WhenFlagClear`
- `VmActorFaceMovementCopySpriteTests.SetVisibleRotationShouldFail_WithoutMobj`
- `VmActorFaceMovementCopySpriteTests.SetTranslationRequiresName_WhenPresent`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 550 --to 550` (0 hard / 0 soft).

## Seams

`COPY_AAPTR`, `AActor::SetAngle`/`SetPitch`, and `SetTranslation` VM calls are not hosted.

## Credit rationale

Credits native guard and angle/pitch/copy/visible-rotation bands mirrored by `VmActorFaceMovementCopySprite` and tests.
