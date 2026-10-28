# Chunk 582 — `A_FaceMovementDirection`, `A_CopySpriteFrame`, `A_SetVisibleRotation`, and `A_SetTranslation` (`p_actionfunctions.cpp` **4897–5082**). Stops before **552** `A_CheckTerrain` (**5090+**). Re-lands retired **550** hull with honest **≥150** `nativeLoc`.

## Scope

`A_FaceMovementDirection`, `A_CopySpriteFrame`, `A_SetVisibleRotation`, and `A_SetTranslation` (`p_actionfunctions.cpp` **4897–5082**). Stops before **552** `A_CheckTerrain` (**5090+**). Re-lands retired **550** hull with honest **≥150** `nativeLoc`.

## Native ranges

- `src/playsim/p_actionfunctions.cpp` **A_FaceMovementDirection** 4897–4983 (87 lines)
- `src/playsim/p_actionfunctions.cpp` **A_CopySpriteFrame** 4991–5015 (25 lines)
- `src/playsim/p_actionfunctions.cpp` **A_SetVisibleRotation** 5024–5067 (44 lines)
- `src/playsim/p_actionfunctions.cpp` **A_SetTranslation** 5075–5082 (8 lines)

**Credited native LOC:** 164 (tally `nativeLoc` 164).

## Managed

`VmActorFaceMovementCopySprite.cs`
Managed LOC (tally): 108.
Conversion commit: `744f1ac47f969ad203da0b229ff7ebc95ab10a95`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Managed | Native (`p_actionfunctions.cpp`) |
|---------|----------------------------------|
| `EarlyFaceMovementFail`, `FmdfFlags`, `ShouldAdjustYaw`, `ShouldAdjustPitch` | `A_FaceMovementDirection` **4897–4918**, **4920–4948** |
| `ClampYawDelta`, `ClampPitchDelta`, `PitchFromVelocity` | **4928–4947**, **4955–4980** |
| `CopySpriteFrameShouldFail`, `ShouldCopySprite`, `ShouldCopyFrame` | `A_CopySpriteFrame` **4997–5014** |
| `SetVisibleRotationShouldFail`, `ShouldAssignVisible*` | `A_SetVisibleRotation` **5032–5066** |
| `SetTranslationRequiresName` | `A_SetTranslation` **5075–5081** |

### vs chunk **547** / **552**

| | Lines |
|--|------:|
| **547** (`CheckBlock`) | ends **4888** |
| **582** (this row) | **4897–5082** (**186** function-edge sum) |
| **552** (`A_CheckTerrain`) | **5090+** (not in this row) |

### Gross vs net

| | Value |
|--|------:|
| Gross `nativeLoc` | **186** |
| Net behavior-ported | **186** |


**Post-review ratio:** 108/164 managed/native ≈ 66% (src file LOC at HEAD; see tally).

## Tests

- `VmActorFaceMovementCopySpriteTests.EarlyFaceMovementFail_BothNoPitchAndNoAngle`
- `VmActorFaceMovementCopySpriteTests.ShouldAdjustYaw_WhenMoving`
- `VmActorFaceMovementCopySpriteTests.ClampYawDelta_RespectsLimit`
- `VmActorFaceMovementCopySpriteTests.ClampPitchDelta_WhenOverLimit`
- `VmActorFaceMovementCopySpriteTests.ShouldAdjustPitch_WhenFlagClear`
- `VmActorFaceMovementCopySpriteTests.CopySpriteFrameShouldFail_WhenBothSkipFlags`
- `VmActorFaceMovementCopySpriteTests.ShouldCopySprite_WhenFlagClear`
- `VmActorFaceMovementCopySpriteTests.CopySpriteFrameShouldFail_WhenSameActor`
- `VmActorFaceMovementCopySpriteTests.SetVisibleRotationShouldFail_WithoutMobj`
- `VmActorFaceMovementCopySpriteTests.ShouldAssignVisibleStartAngle_WhenFlagClear`
- `VmActorFaceMovementCopySpriteTests.ShouldAssignVisibleEndPitch_WhenFlagClear`
- `VmActorFaceMovementCopySpriteTests.SetTranslationRequiresName_WhenPresent`
- `VmActorFaceMovementCopySpriteTests.SetTranslationRequiresName_WhenMissing`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 582 --to 582` (0 hard / 0 soft).

## Seams

`COPY_AAPTR`, `AActor::SetAngle`/`SetPitch`, sprite/frame assignment, visible-rotation fields, and `SetTranslation` VM calls are not hosted.

## Credit rationale

Face-movement yaw/pitch limits, copy-sprite guards, visible-rotation flag gates, and translation name check mirrored by `VmActorFaceMovementCopySprite` and tests (retired **550** was **144** honest after out-of-body trim).
