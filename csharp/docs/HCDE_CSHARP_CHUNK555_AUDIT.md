# Chunk 555 — SetBoneRotation/Translation/Scaling, ClearBoneOffsets, GetBoneOffset natives (5326–5647)

## Scope

SetBoneRotation/Translation/Scaling, ClearBoneOffsets, GetBoneOffset natives (5326–5647)

## Native ranges

- `src/playsim/p_actionfunctions.cpp` **SetModelBoneRotationNative** 5326–5335 (10 lines)
- `src/playsim/p_actionfunctions.cpp` **SetBoneRotationNative** 5337–5340 (4 lines)
- `src/playsim/p_actionfunctions.cpp` **SetModelNamedBoneRotationNative** 5342–5355 (14 lines)
- `src/playsim/p_actionfunctions.cpp` **SetNamedBoneRotationNative** 5357–5360 (4 lines)
- `src/playsim/p_actionfunctions.cpp` **DEFINE_ACTION_FUNCTION_NATIVE(AActor, SetBoneRotation** 5362–5376 (15 lines)
- `src/playsim/p_actionfunctions.cpp` **DEFINE_ACTION_FUNCTION_NATIVE(AActor, SetNamedBoneRotation** 5378–5392 (15 lines)
- `src/playsim/p_actionfunctions.cpp` **SetModelBoneTranslationNative** 5400–5409 (10 lines)
- `src/playsim/p_actionfunctions.cpp` **SetBoneTranslationNative** 5411–5414 (4 lines)
- `src/playsim/p_actionfunctions.cpp` **SetModelNamedBoneTranslationNative** 5416–5429 (14 lines)
- `src/playsim/p_actionfunctions.cpp` **SetNamedBoneTranslationNative** 5431–5434 (4 lines)
- `src/playsim/p_actionfunctions.cpp` **DEFINE_ACTION_FUNCTION_NATIVE(AActor, SetBoneTranslation** 5436–5449 (14 lines)
- `src/playsim/p_actionfunctions.cpp` **DEFINE_ACTION_FUNCTION_NATIVE(AActor, SetNamedBoneTranslation** 5451–5464 (14 lines)
- `src/playsim/p_actionfunctions.cpp` **SetModelBoneScalingNative** 5472–5481 (10 lines)
- `src/playsim/p_actionfunctions.cpp` **SetBoneScalingNative** 5483–5486 (4 lines)
- `src/playsim/p_actionfunctions.cpp` **SetModelNamedBoneScalingNative** 5488–5501 (14 lines)
- `src/playsim/p_actionfunctions.cpp` **SetNamedBoneScalingNative** 5503–5506 (4 lines)
- `src/playsim/p_actionfunctions.cpp` **DEFINE_ACTION_FUNCTION_NATIVE(AActor, SetBoneScaling** 5508–5521 (14 lines)
- `src/playsim/p_actionfunctions.cpp` **DEFINE_ACTION_FUNCTION_NATIVE(AActor, SetNamedBoneScaling** 5523–5536 (14 lines)
- `src/playsim/p_actionfunctions.cpp` **ClearBoneOffsetsNative** 5545–5548 (4 lines)
- `src/playsim/p_actionfunctions.cpp` **DEFINE_ACTION_FUNCTION_NATIVE(AActor, ClearBoneOffsets** 5550–5557 (8 lines)
- `src/playsim/p_actionfunctions.cpp` **DEFINE_ACTION_FUNCTION(AActor, GetBoneOffset** 5565–5604 (40 lines)
- `src/playsim/p_actionfunctions.cpp` **DEFINE_ACTION_FUNCTION(AActor, GetNamedBoneOffset** 5606–5647 (42 lines)

**Credited native LOC:** 276 (tally `nativeLoc` 276).

## Managed

`VmActorBoneTransformNatives.cs`
Managed LOC (tally): 458.
Conversion commit: `72bad8ca58e0fa1a5fa1304f8b1223571f9449e2`.

## Padding review (2026-10-21)

Re-home of 5326–5464 credits removed from chunk 552. `TryApplyBoneOverride` is the shared SetBoneOffsetShared early-return + `GetModelTimer()+ticFrac` path. DEFINE wrappers (SetBoneRotation/Translation/Scaling and named variants) are ported by calling the matching `*Native` with `ticFrac: ActionWrapperTicFrac` (1.0), not separate wrapper methods.

| Managed | Native (`p_actionfunctions.cpp`) |
|---------|----------------------------------|
| `SetModelBoneRotationNative` | `SetModelBoneRotationNative` 5326–5335 |
| `SetBoneRotationNative` | `SetBoneRotationNative` 5337–5340 |
| `SetModelNamedBoneRotationNative` | `SetModelNamedBoneRotationNative` 5342–5355 |
| `SetNamedBoneRotationNative` | `SetNamedBoneRotationNative` 5357–5360 |
| `SetBoneRotationNative` + `ActionWrapperTicFrac` | DEFINE SetBoneRotation 5362–5376 |
| `SetNamedBoneRotationNative` + `ActionWrapperTicFrac` | DEFINE SetNamedBoneRotation 5378–5392 |
| `SetModelBoneTranslationNative` | 5400–5409 |
| `SetBoneTranslationNative` | 5411–5414 |
| `SetModelNamedBoneTranslationNative` | 5416–5429 |
| `SetNamedBoneTranslationNative` | 5431–5434 |
| `SetBoneTranslationNative` + `ActionWrapperTicFrac` | DEFINE 5436–5449 |
| `SetNamedBoneTranslationNative` + `ActionWrapperTicFrac` | DEFINE 5451–5464 |
| `SetModelBoneScalingNative` | 5472–5481 |
| `SetBoneScalingNative` | 5483–5486 |
| `SetModelNamedBoneScalingNative` | 5488–5501 |
| `SetNamedBoneScalingNative` | 5503–5506 |
| `SetBoneScalingNative` + `ActionWrapperTicFrac` | DEFINE 5508–5521 |
| `SetNamedBoneScalingNative` + `ActionWrapperTicFrac` | DEFINE 5523–5536 |
| `ClearBoneOffsetsNative` | 5545–5548; DEFINE 5550–5557 |
| `GetBoneOffsetNative`, `FinalizeGetBoneOffsetNumret` | `GetBoneOffset` 5565–5604 |
| `GetNamedBoneOffsetNative` | `GetNamedBoneOffset` 5606–5647 |

**Post-review ratio:** 458/276 managed/native ≈ 166% (src file LOC at HEAD; see tally).

## Tests

- `VmActorBoneTransformNativesTests.SetModelBoneRotationNative_SkipsWithoutModel`
- `VmActorBoneTransformNativesTests.SetBoneRotationNative_UsesDefaultModelIndex`
- `VmActorBoneTransformNativesTests.SetModelBoneTranslationNative_CalcBonesWhenModelOk`
- `VmActorBoneTransformNativesTests.SetModelBoneScalingNative_SampleTime`
- `VmActorBoneTransformNativesTests.ClearBoneOffsetsNative_RequiresModelData`
- `VmActorBoneTransformNativesTests.GetBoneOffsetNative_DefaultsWhenNoModel`
- `VmActorBoneTransformNativesTests.FinalizeGetBoneOffsetNumret_CapsAtThree`
- `VmActorBoneTransformNativesTests.GetNamedBoneOffsetNative_MatchesIndexedPath`
- `VmActorBoneTransformNativesTests.GetBoneOffsetAssignsScaling_WhenNumretAboveTwo`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 555 --to 555` (0 hard / 0 soft).

## Seams

`SetBoneOffsetShared`, `GetBoneOffsetShared`, `FQuaternion`/`FVector3` override `Set`, and `CalcBones` are not hosted.

## Credit rationale

One managed entry point per credited whole native function; credits match ported control flow only.
