# Chunk 552 — A_CheckTerrain, A_SetSize, SetCamera, EnsureModelData, CleanupModelData, SetGetBoneShared

## Scope

A_CheckTerrain, A_SetSize, SetCamera, EnsureModelData, CleanupModelData, SetGetBoneShared

## Native ranges

- `src/playsim/p_actionfunctions.cpp` **A_CheckTerrain** 5090–5111 (22 lines)
- `src/playsim/p_actionfunctions.cpp` **A_SetSize** 5113–5146 (34 lines)
- `src/playsim/p_actionfunctions.cpp` **SetCamera** 5148–5170 (23 lines)
- `src/playsim/p_actionfunctions.cpp` **A_SprayDecal** 5172–5188 (17 lines)
- `src/playsim/p_actionfunctions.cpp` **A_SetMugshotState** 5190–5197 (8 lines)
- `src/playsim/p_actionfunctions.cpp` **EnsureModelData** 5206–5220 (15 lines)
- `src/playsim/p_actionfunctions.cpp` **CleanupModelData** 5222–5237 (16 lines)
- `src/playsim/p_actionfunctions.cpp` **SetGetBoneShared** 5242–5270 (29 lines)

**Credited native LOC:** 164 (tally `nativeLoc` 164).

## Managed

`VmActorTerrainSetSizeModel.cs`
Managed LOC (tally): 94.
Conversion commit: `a8c6a5b6b28e707a8c2d575a0ad1a19554466c5d`.

## Padding review (2026-10-21)

Removed `PlanSetSizeDimensions`, `PlanSetCameraTarget`, and `SprayDecalParameters` record (orchestration / padding). Remaining methods mirror native branches only.

| Managed | Native (`p_actionfunctions.cpp`) |
|---------|----------------------------------|
| `CheckTerrainOnFloor`, `ShouldInstantDeathTerrain`, `ScrollStrifeCurrentThrust` | `A_CheckTerrain` 5090–5111 |
| `ResolveNewRadius`, `ResolveNewHeight`, `ShouldRevertSize` | `A_SetSize` 5113–5146 |
| `SetCameraRequiresPlayer`, `SetCameraNullSelectsSelf`, `ShouldClearPastViewer`, `SetCameraRevertSetsCheat` | `SetCamera` 5148–5170 |
| `SprayDecalRequiresSelf` | `A_SprayDecal` PARAM_SELF 5172–5188 |
| `SetMugshotState_RequiresLocalView` | `A_SetMugshotState` 5190–5197 |
| `EnsureModelDataFlags`, `ShouldCleanupModelData` | `EnsureModelData` / `CleanupModelData` 5206–5237 |
| `ModelIndexInRuntimeRange`, `ModelIndexInBaseFrames`, `TryResolveBoneModelIndex` | `SetGetBoneShared` 5242–5270 |

**Post-review ratio:** 94/164 managed/native ≈ 57% (src file LOC at HEAD; see tally).

## Tests

- `VmActorTerrainSetSizeModelTests.ShouldInstantDeathTerrain_AtTelefragThreshold`
- `VmActorTerrainSetSizeModelTests.ShouldRevertSize_WhenTestFails`
- `VmActorTerrainSetSizeModelTests.SetCameraRequiresPlayer_WhenLocalMo`
- `VmActorTerrainSetSizeModelTests.SprayDecal_ForwardsParameters`
- `VmActorTerrainSetSizeModelTests.SetMugshotState_RequiresLocalView`
- `VmActorTerrainSetSizeModelTests.EnsureModelDataFlags_WhenHadModel`
- `VmActorTerrainSetSizeModelTests.ShouldCleanupModelData_WhenEmpty`
- `VmActorTerrainSetSizeModelTests.ModelIndexInRuntimeRange_MatchesNativeGate`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 552 --to 552` (0 hard / 0 soft).

## Seams

`P_DamageMobj`, `P_TestMobjLocation`, `R_ClearPastViewer`, and live `modelData` allocation are not hosted.

## Credit rationale

Credits whole-function and helper bands mirrored by `VmActorTerrainSetSizeModel` and tests.
