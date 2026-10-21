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

`VmActorFaceMovementCopySprite.cs`, `VmActorFaceMovementCopySpriteTests.cs`
Managed LOC (tally): 108.
Conversion commit: `d96727a9d2016da7920a81c0c7ed81d271672a55`.

## Tests

- `VmActorFaceMovementCopySpriteTests.EarlyFaceMovementFail_BothNoPitchAndNoAngle`
- `VmActorFaceMovementCopySpriteTests.ClampYawDelta_RespectsLimit`
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
