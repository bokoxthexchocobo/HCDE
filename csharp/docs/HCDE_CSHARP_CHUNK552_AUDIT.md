# Chunk 552 — A_CheckTerrain through SetGetBoneSharedIndex / SetBoneOffsetShared / GetBone* (5090–5318); merged ex-chunk553 bone-index credits

## Scope

A_CheckTerrain through SetGetBoneSharedIndex / SetBoneOffsetShared / GetBone* (5090–5318); merged ex-chunk553 bone-index credits

## Native ranges

- `src/playsim/p_actionfunctions.cpp` **A_CheckTerrain** 5090–5111 (22 lines)
- `src/playsim/p_actionfunctions.cpp` **A_SetSize** 5113–5146 (34 lines)
- `src/playsim/p_actionfunctions.cpp` **SetCamera** 5148–5170 (23 lines)
- `src/playsim/p_actionfunctions.cpp` **EnsureModelData** 5206–5220 (15 lines)
- `src/playsim/p_actionfunctions.cpp` **CleanupModelData** 5222–5237 (16 lines)
- `src/playsim/p_actionfunctions.cpp` **SetGetBoneShared** 5242–5270 (29 lines)
- `src/playsim/p_actionfunctions.cpp` **SetGetBoneSharedIndex** 5272–5296 (25 lines)
- `src/playsim/p_actionfunctions.cpp` **SetBoneOffsetShared** 5298–5308 (11 lines)
- `src/playsim/p_actionfunctions.cpp` **GetBoneOffsetShared** 5310–5313 (4 lines)
- `src/playsim/p_actionfunctions.cpp` **GetBoneShared** 5315–5318 (4 lines)

**Credited native LOC:** 183 (tally `nativeLoc` 183).

## Managed

`VmActorTerrainSetSizeModel.cs`, `VmActorBoneSetGetShared.cs`
Managed LOC (tally): 173.
Conversion commit: `5fc43544209cee523aaf8815d4d61ae4bf374bf6`.

## Padding review (2026-10-21)

Credit fix (2026-10-21): dropped `A_SprayDecal` / `A_SetMugshotState` (no real port). Merged former chunk 553 after removing 5326–5464 credits (not whole-function ports). No filler methods for ratio.

| Managed | Native (`p_actionfunctions.cpp`) |
|---------|----------------------------------|
| `CheckTerrainOnFloor`, `ShouldInstantDeathTerrain`, `ScrollStrifeCurrentThrust` | `A_CheckTerrain` 5090–5111 |
| `ResolveNewRadius`, `ResolveNewHeight`, `ShouldRevertSize` | `A_SetSize` 5113–5146 |
| `SetCameraRequiresPlayer`, `SetCameraNullSelectsSelf`, `ShouldClearPastViewer`, `SetCameraRevertSetsCheat` | `SetCamera` 5148–5170 |
| `EnsureModelDataFlags`, `ShouldCleanupModelData` | `EnsureModelData` 5206–5237 |
| `ModelIndexInRuntimeRange`, `ModelIndexInBaseFrames`, `TryResolveBoneModelIndex` | `SetGetBoneShared` 5242–5270 |
| `SetGetBoneSharedIndexGate`, `TryResolveNamedBoneIndex`, `TryValidateBoneIndex`, `NeedsBoneOverrideModelSlotGrow`, `BoneOverrideModelSlotCount` | `SetGetBoneSharedIndex` 5272–5296 |
| `NormalizeInterpolationDuration`, `IsValidSetBoneMode`, `TryPrepareSetBoneOffset` | `SetBoneOffsetShared` 5298–5308 |
| `SetGetBoneSharedIndexGate` | `GetBoneOffsetShared` 5310–5313, `GetBoneShared` 5315–5318 |

**Post-review ratio:** 173/183 managed/native ≈ 95% (src file LOC at HEAD; see tally).

## Tests

- `VmActorTerrainSetSizeModelTests.ShouldInstantDeathTerrain_AtTelefragThreshold`
- `VmActorTerrainSetSizeModelTests.ShouldRevertSize_WhenTestFails`
- `VmActorTerrainSetSizeModelTests.SetCameraRequiresPlayer_WhenLocalMo`
- `VmActorTerrainSetSizeModelTests.EnsureModelDataFlags_WhenHadModel`
- `VmActorTerrainSetSizeModelTests.ShouldCleanupModelData_WhenEmpty`
- `VmActorTerrainSetSizeModelTests.ModelIndexInRuntimeRange_MatchesNativeGate`
- `VmActorBoneSetGetSharedTests.SetGetBoneSharedIndexGate_NamedBoneMissing`
- `VmActorBoneSetGetSharedTests.TryPrepareSetBoneOffset_InvalidMode_Fails`
- `VmActorBoneSetGetSharedTests.TryPrepareSetBoneOffset_ValidIndex_Ok`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 552 --to 552` (0 hard / 0 soft).

## Seams

`P_DamageMobj`, `P_TestMobjLocation`, `R_ClearPastViewer`, `FModel::FindJoint`, and live `modelData` allocation are not hosted.

## Credit rationale

Credits only whole native functions with matching managed gates; rotation/translation DEFINE wrappers and thin VM forwards are not credited.
