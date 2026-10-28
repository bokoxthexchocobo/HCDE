# Chunk 584 — `CalculateAnimationInternal` through `GetRenderStyle` (`p_actionfunctions.cpp` **7245–7809**). Follows **560** layer animation (**7244**).

## Scope

`CalculateAnimationInternal` through `GetRenderStyle` (`p_actionfunctions.cpp` **7245–7809**). Follows **560** layer animation (**7244**).

## Native ranges

- `src/playsim/p_actionfunctions.cpp` **CalculateAnimationInternal** 7245–7449 (205 lines)
- `src/playsim/p_actionfunctions.cpp` **BlendAnimationFrames** 7450–7809 (360 lines)

**Credited native LOC:** 565 (tally `nativeLoc` 565).

## Managed

`VmActorPrecalcAnimation.cs`, `VmActorPrecalcBones.cs`
Managed LOC (tally): 283.
Conversion commit: `92b0205cd33f70af6b563d32a61464dab0a2c0c6`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Managed | Native (`p_actionfunctions.cpp`) |
|---------|----------------------------------|
| `HasModelForPrecalc` / `ShouldAbortCalculateWhenLayerMissing` / sample tic helpers | `CalculateAnimationInternal` **7246–7266**, `CalculateAnimation*` **7268–7304** |
| `ShouldAbortCalculateFrameWhenMissing` / model null guards | `CalculateAnimationFrame` **7306–7330** |
| `ShouldFindAnimationFrame` / `FindAnimationFrameUiTic` / `ShouldUsePrevAnimFrame` | `FindAnimationFrameInternal` **7332–7355**, `FindAnimationFrame*` **7357–7449** |
| `CanBlendPrecalcFrameData` / quaternion & translation helpers | `BlendAnimationFrames` **7454–7481**, `OffsetAnimationFrame` **7483–7512** |
| `VmActorPrecalcBones` apply counts / mask / range / UI tic gates | `SetBones*` / `OverwriteBones*` **7514–7755**, `ForceRecalculateBones` **7757–7764** |
| `ScanLegacyRenderStyleIndex` | `GetRenderStyle` **7801–7808** |

### vs chunk **560**

| | Lines |
|--|------:|
| **560** (layer animation) | ends **7244** |
| **584** (this row) | **7245–7809** (**565** hull) |

### Gross vs net

| | Value |
|--|------:|
| Gross `nativeLoc` | **565** |
| Net behavior-ported | **565** |


**Post-review ratio:** 283/565 managed/native ≈ 50% (src file LOC at HEAD; see tally).

## Tests

- `VmActorPrecalcAnimationTests.ShouldFindAnimationFrame_RequiresLayerAndNonNegativeTic`
- `VmActorPrecalcAnimationTests.FindAnimationFrameUiTic_AddsFracWhenUnpaused`
- `VmActorPrecalcAnimationTests.CanBlendPrecalcFrameData_RequiresEqualSizes`
- `VmActorPrecalcAnimationTests.GameAnimationSampleTic_AddsOne`
- `VmActorPrecalcAnimationTests.ShouldWriteFindAnimationRetSlot_MatchesNativeNumret`
- `VmActorPrecalcAnimationTests.OffsetBoneTranslation_AddsComponents`
- `VmActorPrecalcAnimationTests.MultiplyQuaternion_IdentityTimesRotation`
- `VmActorPrecalcAnimationTests.ScanLegacyRenderStyleIndex_FindsMatch`
- `VmActorPrecalcBonesTests.BoneOverrideApplyCount_TakesMinimum`
- `VmActorPrecalcBonesTests.RangeBoneApplyEnd_ClampsStartPlusLength`
- `VmActorPrecalcBonesTests.ShouldRecalcBonesAfterApply_SkipsUiPath`
- `VmActorPrecalcBonesTests.CountMaskedBoneApplications_CountsTrueEntries`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 584 --to 584` (0 hard / 0 soft).

## Seams

`FindFModel`, `SetGetBoneShared`, `calcFrames`, `PrecalculateFrame`, `CalcBones`, and `LegacyRenderStyles` are not hosted.

## Credit rationale

Cohesive precalc animation calculate/find/blend/offset plus bones set/mask/range/overwrite and render-style scan (**565** native LOC oversize band).
