# Chunk 587 — ChangeModel* (6740–6989) plus CalculateAnimation/FindAnimation/SetBones policy (7246–7567); supersedes retired chunks 588–590.

## Scope

ChangeModel* (6740–6989) plus CalculateAnimation/FindAnimation/SetBones policy (7246–7567); supersedes retired chunks 588–590.

## Native ranges

- `src/playsim/p_actionfunctions.cpp` **SetModelFlag** 6740–6757 (18 lines)
- `src/playsim/p_actionfunctions.cpp` **ClearModelFlag** 6759–6777 (19 lines)
- `src/playsim/p_actionfunctions.cpp` **ResetModelFlags** 6779–6795 (17 lines)
- `src/playsim/p_actionfunctions.cpp` **ChangeModelFlags** 6797–6802 (6 lines)
- `src/playsim/p_actionfunctions.cpp` **ChangeModelNative** 6804–6889 (86 lines)
- `src/playsim/p_actionfunctions.cpp` **ChangeModelNative** 6890–6968 (79 lines)
- `src/playsim/p_actionfunctions.cpp` **DEFINE_ACTION_FUNCTION_NATIVE(AActor, A_ChangeModel** 6970–6989 (20 lines)
- `src/playsim/p_actionfunctions.cpp` **CalculateAnimationInternal** 7250–7253 (4 lines)
- `src/playsim/p_actionfunctions.cpp` **DEFINE_ACTION_FUNCTION(AActor, CalculateAnimation** 7273–7276 (4 lines)
- `src/playsim/p_actionfunctions.cpp` **DEFINE_ACTION_FUNCTION(AActor, CalculateAnimationUI** 7293–7299 (7 lines)
- `src/playsim/p_actionfunctions.cpp` **DEFINE_ACTION_FUNCTION(AActor, CalculateAnimationFrame** 7311–7316 (6 lines)
- `src/playsim/p_actionfunctions.cpp` **FindAnimationFrameInternal** 7334–7335 (2 lines)
- `src/playsim/p_actionfunctions.cpp` **DEFINE_ACTION_FUNCTION(AActor, FindAnimationFrameAt** 7369–7374 (6 lines)
- `src/playsim/p_actionfunctions.cpp` **DEFINE_ACTION_FUNCTION(AActor, FindAnimationFrame** 7398–7410 (13 lines)
- `src/playsim/p_actionfunctions.cpp` **DEFINE_ACTION_FUNCTION(AActor, FindAnimationFrameUI** 7425–7436 (12 lines)
- `src/playsim/p_actionfunctions.cpp` **DEFINE_ACTION_FUNCTION(AActor, BlendAnimationFrames** 7461–7480 (20 lines)
- `src/playsim/p_actionfunctions.cpp` **DEFINE_ACTION_FUNCTION(AActor, OffsetAnimationFrame** 7489–7511 (23 lines)
- `src/playsim/p_actionfunctions.cpp` **DEFINE_ACTION_FUNCTION(AActor, SetBones** 7514–7531 (18 lines)
- `src/playsim/p_actionfunctions.cpp` **DEFINE_ACTION_FUNCTION(AActor, SetBonesUI** 7541–7558 (18 lines)

**Credited native LOC:** 378 (tally `nativeLoc` 378).

## Managed

`VmActionChangeModel.cs`, `VmActionCalculateAnimation.cs`
Managed LOC (tally): 189.
Conversion commit: `2b08f420a0d50c21a07e550be171b94ebf6978a0`.

## Padding review (2026-10-21)

**Superseded chunk numbers:** 588, 589, 590 (retired; honest credit lives only in chunk587.json).

**ChangeModel fidelity:** Whole `ChangeModelNative` body is banded; seam calls excluded.

| Managed | Native (`p_actionfunctions.cpp`) |
|---------|----------------------------------|
| `MaskIqmFlags` / `ShouldSetOverrideFlags` | `SetModelFlag` / `ClearModelFlag` 6740–6777 |
| `ShouldResetModelOverrides` / `ClearOverrideFlagBits` | `ResetModelFlags` 6779–6795 |
| `Cmdl*` / `Normalize*Index` | `ChangeModelFlags` + indices 6797–6838 |
| `EnsureTrailingSlash` / `QueryModel*` / `SelectInvokerAsMobj` | 6840–6853 |
| `NeedsGrow*` / `AppendNewModelSlot` / surface skin | 6858–6896 |
| `ShouldResetCurAnimOnAnimationChange` / skin arrays | 6928–6958 |
| `ShouldCalcBonesAfterChange` | 6962–6965 |
| (DEFINE seam) | `A_ChangeModel` 6970–6989 |

**Animation/bone policy (no duplicate helper padding):**

| Managed | Native (`p_actionfunctions.cpp`) |
|---------|----------------------------------|
| `CalculateAnimationInternalNullModel` | 7250–7253 |
| `CalculateAnimationLayerNull` / `CalculateAnimationTic` | 7273–7281 |
| `UiModelTimer` / `ReturnNullWhenLayerMissing` | 7293–7299 |
| `CalculateAnimationFrameNull*` | 7311–7320 |
| `FindAnimationFrameNegativeTic` / `ShouldCreateInterpFrame` | 7334–7335 |
| `FindAnimationFrameAtSets*` | 7369–7377, 7396–7410 |
| `UiFindTic` / `SetRetInterWhenThird` | 7425–7446 |
| `BlendSizesMatch` / `BlendOneMinusT` / `ThrowWhenBlendSizesDiffer` / `ShouldInterpolateBone` | 7454–7480 |
| `OffsetSizesMatch` / `ThrowOffsetSizeMismatch` | 7483–7511 |
| `NeedsBoneOverrideSlot` / `SetBonesTic` / `MinBoneLoopCount` / `ShouldCalcBonesAfterSet` | 7514–7537 |
| `SetBonesUiTic` / `UiSkipsCalcBonesComment` | 7541–7564 |

**Post-review ratio:** 189/378 managed/native ≈ 50% (src file LOC at HEAD; see tally).

## Tests

- `VmActionChangeModelTests.MaskIqmFlags_AndsMask`
- `VmActionChangeModelTests.ShouldSetOverrideFlags_WhenFlagNonZero`
- `VmActionChangeModelTests.ShouldResetModelOverrides_WhenResetModel`
- `VmActionChangeModelTests.UseWeaponToPlayer_WhenCmdlFlag`
- `VmActionChangeModelTests.NormalizeModelIndex_NegativeBecomesZero`
- `VmActionChangeModelTests.EnsureTrailingSlash_AppendsSlash`
- `VmActionChangeModelTests.QueryModelWhenVisible_HideModel`
- `VmActionChangeModelTests.SelectInvokerAsMobj_FromInventoryUsesSelf`
- `VmActionChangeModelTests.ShouldPushSurfaceSkin_FlagAndIndex`
- `VmActionChangeModelTests.AppendNewModelSlot_WhenCountsMatch`
- `VmActionChangeModelTests.ShouldResetCurAnimOnAnimationChange_WhenIdDiffers`
- `VmActionChangeModelTests.ShouldGrowSurfaceSkins_WhenIndexPastEnd`
- `VmActionChangeModelTests.ShouldAssignSkinAtEnd_WhenCountsMatch`
- `VmActionChangeModelTests.InvalidModelDefWhenNamed_MissingClass`
- `VmActionChangeModelTests.ShouldCalcBonesAfterChange_ModelDefOnly`
- `VmActionCalculateAnimationTests.CalculateAnimationInternalNullModel_WhenMissing`
- `VmActionCalculateAnimationTests.CalculateAnimationLayerNull_ReturnsNull`
- `VmActionCalculateAnimationTests.UiModelTimer_AddsFracWhenUnpaused`
- `VmActionCalculateAnimationTests.CalculateAnimationFrameNullIframe_Aborts`
- `VmActionCalculateAnimationTests.ReturnNullWhenModelMissing`
- `VmActionCalculateAnimationTests.FindAnimationFrameNegativeTic_SkipsWork`
- `VmActionCalculateAnimationTests.ShouldCreateInterpFrame_WhenLayerAndTicOk`
- `VmActionCalculateAnimationTests.FindAnimationFrameAtSetsFloat_WhenThirdRet`
- `VmActionCalculateAnimationTests.CalcUsesModelTimerPlusOne`
- `VmActionCalculateAnimationTests.FindAnimationFrameAtSetsObject_WhenSecondRet`
- `VmActionCalculateAnimationTests.UiFindTic_AddsFracWhenRunning`
- `VmActionCalculateAnimationTests.SetRetInterWhenThird`
- `VmActionCalculateAnimationTests.BlendSizesMatch_RequiresEqual`
- `VmActionCalculateAnimationTests.BlendOneMinusT_Complements`
- `VmActionCalculateAnimationTests.ThrowWhenBlendSizesDiffer_FlagsMismatch`
- `VmActionCalculateAnimationTests.ShouldInterpolateBone_InRange`
- `VmActionCalculateAnimationTests.OffsetSizesMatch`
- `VmActionCalculateAnimationTests.ThrowOffsetSizeMismatch`
- `VmActionCalculateAnimationTests.NeedsBoneOverrideSlot_WhenEmpty`
- `VmActionCalculateAnimationTests.MinBoneLoopCount_TakesMinimum`
- `VmActionCalculateAnimationTests.ShouldCalcBonesAfterSet_WhenAnyBone`
- `VmActionCalculateAnimationTests.SetBonesUiTic_UsesFrac`
- `VmActionCalculateAnimationTests.UiSkipsCalcBonesComment`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 587 --to 587` (0 hard / 0 soft).

## Seams

`EnsureModelData`, `FindModel`, `LoadSkin`, `calcFrames`, `PrecalculateFrame`, `InterpolateBone`, `SetGetBoneShared`, `BoneOverride::Set`, and `CalcBones` are not hosted.

## Credit rationale

Credits ChangeModelNative policy and slim animation/bone DEFINE guards without ratio padding; former 588–590 bands merged here after padding strip.
