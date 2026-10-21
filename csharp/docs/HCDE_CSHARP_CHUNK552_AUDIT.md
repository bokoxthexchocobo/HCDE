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

`VmActorTerrainSetSizeModel.cs`, `VmActorTerrainSetSizeModelTests.cs`
Managed LOC (tally): 84.
Conversion commit: `64d0da3f90c4f56757ab5092dd0e7891c9964729`.

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
