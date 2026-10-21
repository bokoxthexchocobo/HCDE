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

`VmActorFaceMovementCopySprite.cs`
Managed LOC (tally): 106.
Conversion commit: `5fc43544209cee523aaf8815d4d61ae4bf374bf6`.

## Padding review (2026-10-21)

Removed `PlanFaceMovement` / `VelocityYawDegrees` orchestration (not a single native function). Remaining methods map 1:1 to credited bands.

| Managed | Native (`p_actionfunctions.cpp`) |
|---------|----------------------------------|
| `EarlyFaceMovementFail`, `FmdfFlags` | 4897–4902, 4903 guards |
| `ShouldAdjustYaw`, `ClampYawDelta`, `ClampPitchDelta`, `PitchFromVelocity` | `A_FaceMovementDirection` 4903–4983 |
| `CopySpriteFrameShouldFail`, `ShouldCopySprite`, `ShouldCopyFrame` | `A_CopySpriteFrame` 4997–5015 |
| `SetVisibleRotationShouldFail`, `ShouldAssignVisible*` | `A_SetVisibleRotation` 5032–5067 |
| `SetTranslationRequiresName` | `A_SetTranslation` 5075–5082 |

**Post-review ratio:** 106/162 managed/native ≈ 65% (src file LOC at HEAD; see tally).

## Tests

- `VmActorFaceMovementCopySpriteTests.EarlyFaceMovementFail_BothNoPitchAndNoAngle`
- `VmActorFaceMovementCopySpriteTests.ClampYawDelta_RespectsLimit`
- `VmActorFaceMovementCopySpriteTests.ClampYawDelta_WithinLimitUsesTarget`
- `VmActorFaceMovementCopySpriteTests.ClampPitchDelta_WhenOverLimit`
- `VmActorFaceMovementCopySpriteTests.FaceMovementPitch_AdjustsViaClamp`
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
