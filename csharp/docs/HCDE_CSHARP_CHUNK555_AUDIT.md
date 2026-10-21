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
Managed LOC (tally): 659.
Conversion commit: `3b1e00b824c4f74282728aafb951be7323eab6da`.

## Padding review (2026-10-21)

Re-home of 5326–5464 credits removed from chunk 552 (2026-10-21). Each credited native is ported as: `SetBoneOffsetShared` gate → early return when null → `modelBoneOverrides[..].rotation|translation|scaling.Set(..., GetModelTimer()+ticFrac, ...)` (seam) → `CalcBones(true)`.

| Managed | Native (`p_actionfunctions.cpp`) |
|---------|----------------------------------|
| `SetModelBoneRotationNative` | `SetModelBoneRotationNative` 5326–5335 |
| `SetBoneRotationNative` | `SetBoneRotationNative` 5337–5340 |
| `SetModelNamedBoneRotationNative` | `SetModelNamedBoneRotationNative` 5342–5355 |
| `SetNamedBoneRotationNative` | `SetNamedBoneRotationNative` 5357–5360 |
| `SetBoneRotationActionWrapper` | `DEFINE_ACTION_FUNCTION_NATIVE(AActor, SetBoneRotation` 5362–5376 (`ticFrac` 1.0) |
| `SetNamedBoneRotationActionWrapper` | `DEFINE_ACTION_FUNCTION_NATIVE(AActor, SetNamedBoneRotation` 5378–5392 |
| `SetModelBoneTranslationNative` | `SetModelBoneTranslationNative` 5400–5409 |
| `SetBoneTranslationNative` | `SetBoneTranslationNative` 5411–5414 |
| `SetModelNamedBoneTranslationNative` | `SetModelNamedBoneTranslationNative` 5416–5429 |
| `SetNamedBoneTranslationNative` | `SetNamedBoneTranslationNative` 5431–5434 |
| `SetBoneTranslationActionWrapper` | `DEFINE_ACTION_FUNCTION_NATIVE(AActor, SetBoneTranslation` 5436–5449 |
| `SetNamedBoneTranslationActionWrapper` | `DEFINE_ACTION_FUNCTION_NATIVE(AActor, SetNamedBoneTranslation` 5451–5464 |
| `SetModelBoneScalingNative` | `SetModelBoneScalingNative` 5472–5481 |
| `SetBoneScalingNative` | `SetBoneScalingNative` 5483–5486 |
| `SetModelNamedBoneScalingNative` | `SetModelNamedBoneScalingNative` 5488–5501 |
| `SetNamedBoneScalingNative` | `SetNamedBoneScalingNative` 5503–5506 |
| `SetBoneScalingActionWrapper` | `DEFINE_ACTION_FUNCTION_NATIVE(AActor, SetBoneScaling` 5508–5521 |
| `SetNamedBoneScalingActionWrapper` | `DEFINE_ACTION_FUNCTION_NATIVE(AActor, SetNamedBoneScaling` 5523–5536 |
| `ClearBoneOffsetsNative`, `ClearBoneOffsetsActionWrapper` | `ClearBoneOffsetsNative` 5545–5548; DEFINE 5550–5557 |
| `GetBoneOffsetNative`, `FinalizeGetBoneOffsetNumret` | `GetBoneOffset` 5565–5604 |
| `GetNamedBoneOffsetNative` | `GetNamedBoneOffset` 5606–5647 |

**Post-review ratio:** 659/276 managed/native ≈ 239% (src file LOC at HEAD; see tally).

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
