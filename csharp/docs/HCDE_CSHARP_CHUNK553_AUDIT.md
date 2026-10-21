# Chunk 553 — SetGetBoneSharedIndex, Set/Get bone offset/shared, SetBoneRotation/Translation action natives

## Scope

SetGetBoneSharedIndex, Set/Get bone offset/shared, SetBoneRotation/Translation action natives

## Native ranges

- `src/playsim/p_actionfunctions.cpp` **SetGetBoneSharedIndex** 5272–5296 (25 lines)
- `src/playsim/p_actionfunctions.cpp` **SetBoneOffsetShared** 5298–5308 (11 lines)
- `src/playsim/p_actionfunctions.cpp` **GetBoneOffsetShared** 5310–5313 (4 lines)
- `src/playsim/p_actionfunctions.cpp` **GetBoneShared** 5315–5318 (4 lines)
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

**Credited native LOC:** 166 (tally `nativeLoc` 166).

## Managed

`VmActorBoneSetGetShared.cs`, `VmActorBoneSetGetSharedTests.cs`
Managed LOC (tally): 154.
Conversion commit: `cd2985ae55ebb5a74322c2e6af1452d7d78846c7`.

## Tests

- `VmActorBoneSetGetSharedTests.PlanSetGetBoneSharedIndex_NamedBoneMissing_ReturnsMissing`
- `VmActorBoneSetGetSharedTests.PlanSetBoneOffsetShared_InvalidMode_Aborts`
- `VmActorBoneSetGetSharedTests.PlanGetBoneOffsetShared_ValidIndex_Ok`
- `VmActorBoneSetGetSharedTests.BoneRotationNativeShouldCalcBones_WhenOk`
- `VmActorBoneSetGetSharedTests.DefaultModelIndexForActorBone_IsZero`
- `VmActorBoneSetGetSharedTests.PackBoneRotation_ForwardsComponents`
- `VmActorBoneSetGetSharedTests.NormalizeInterpolationDuration_ClampsNegative`
- `VmActorBoneSetGetSharedTests.IsValidSetBoneMode_RejectsAboveTwo`
- `VmActorBoneSetGetSharedTests.PackBoneTranslation_ForwardsComponents`
- `VmActorBoneSetGetSharedTests.TryResolveNamedBoneIndex_FailsWhenMissing`
- `VmActorBoneSetGetSharedTests.TryValidateBoneIndex_RejectsOutOfRange`
- `VmActorBoneSetGetSharedTests.PlanBoneOverrideResize_GrowsModelSlotWhenNeeded`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 553 --to 553` (0 hard / 0 soft).

## Seams

`FModel::FindJoint`, `modelBoneOverrides` resize, `CalcBones`, and `FQuaternion`/`FVector3` mutation are not hosted.

## Credit rationale

Credits bone gate, validation, and native wrapper bands mirrored by `VmActorBoneSetGetShared` and tests.
