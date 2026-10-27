# Chunk 585 — `SetAnimation*` / `SetAnimationFrameRate*` natives, model-flag overrides, `ChangeModelNative`, and `AnimLayerToAnimInfo` (`p_actionfunctions.cpp` **6677–7079**). Follows **517** `SetAnimationInternal` (**6676**); stops before **560** `AnimInfoToAnimLayer` (**7080+**).

## Scope

`SetAnimation*` / `SetAnimationFrameRate*` natives, model-flag overrides, `ChangeModelNative`, and `AnimLayerToAnimInfo` (`p_actionfunctions.cpp` **6677–7079**). Follows **517** `SetAnimationInternal` (**6676**); stops before **560** `AnimInfoToAnimLayer` (**7080+**).

## Native ranges

- `src/playsim/p_actionfunctions.cpp` **SetAnimationNative** 6677–7079 (403 lines)

**Credited native LOC:** 403 (tally `nativeLoc` 403).

## Managed

`VmActionSetAnimationNatives.cs`, `VmActionChangeModel.cs`, `VmActionAnimLayerConvert.cs`
Managed LOC (tally): 306.
Conversion commit: `64ace32eaab0a1c67451d0101310a7dba99d0f8e`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Managed | Native (`p_actionfunctions.cpp`) |
|---------|----------------------------------|
| `VmActionSetAnimationNatives` | `SetAnimationNative`/`UI` **6678–6686**, DEFINE `SetAnimation*` **6991–7041**, `SetAnimationFrameRate*` **6730–6738** |
| `SetAnimationFrameRateInternalNative` | `SetAnimationFrameRateInternal` **6688–6728** |
| `VmActionChangeModel` | `SetModelFlag`/`ClearModelFlag`/`ResetModelFlags` **6740–6795**, `ChangeModelNative` **6804–6968**, `A_ChangeModel` **6970–6989** |
| `VmActionAnimLayerConvert` | `AnimLayerToAnimInfo` **7043–7078** |

### vs chunk **517** / **560**

| | Lines |
|--|------:|
| **517** (`SetAnimationInternal`) | ends **6676** |
| **585** (this row) | **6677–7079** (**403**) |
| **560** (`AnimInfoToAnimLayer`) | **7080+** |

### Gross vs net

| | Value |
|--|------:|
| Gross `nativeLoc` | **403** |
| Net behavior-ported | **403** |


**Post-review ratio:** 306/403 managed/native ≈ 76% (src file LOC at HEAD; see tally).

## Tests

- `VmActionSetAnimationNativesTests.SetAnimationNative_UsesTicFracOne`
- `VmActionSetAnimationNativesTests.SetAnimationFrameRateInternalNative_RejectsNegativeRate`
- `VmActionChangeModelTests.ShouldSetOverrideFlags_WhenFlagNonZero`
- `VmActionChangeModelTests.QueryModelWhenVisible_HideModel`
- `VmActionChangeModelTests.ShouldResetCurAnimOnAnimationChange_WhenIdDiffers`
- `VmActionSetAnimationNativesTests.AnimLayerToAnimInfoNative_NoneWhenNoLayer`
- `VmActionAnimLayerConvertTests.TryBuildPrecalculatedPrev_RequiresTypes`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 585 --to 585` (0 hard / 0 soft).

## Seams

`SetAnimationInternal`, `FindModel`, `LoadSkin`, `EnsureModelData`, `CleanupModelData`, and `CalcBones` are not hosted.

## Credit rationale

Re-lands retired **587** hull: animation DEFINE wrappers, frame-rate internal policy, CMDL change-model routing, and layer→`AnimInfo` conversion (**403** native LOC).
