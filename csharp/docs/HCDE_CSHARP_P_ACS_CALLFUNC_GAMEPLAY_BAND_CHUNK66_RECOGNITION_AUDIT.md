# ACS CallFunction gameplay band — chunk 66 recognition re-audit

**Native:** `p_acs.cpp` `CallFunction` cases with existing `TryInvoke` coverage.

| Native case | Managed handler | Test |
|-------------|-----------------|------|
| `ACSF_SetActorVelocity` | `TrySetActorVelocity` | `AcsCallFunctionTests.CallFunc_SetActorVelocity_ThenGetVelXMatches` |
| `ACSF_SetUserVariable` | `TrySetUserVariable` → `AcsScriptUserVariableService` | `AcsScriptUserVariableServiceTests.CallFunc_SetAndGetUserVariable_RoundTripsIndexedSlot` |
| `ACSF_GetUserVariable` | `TryGetUserVariable` | same |
| `ACSF_SetUserArray` / `GetUserArray` | `TrySetUserArray` / `TryGetUserArray` | `AcsHotCallFuncBehaviorTests.UserArrayIndexRoundTrip` |
| `ACSF_CheckActorClass` | `TryCheckActorClass` | `AcsCallFunctionTests.CallFunc_CheckActorClass_MatchesImpByTid` |
| `ACSF_GetActorClass` | `TryGetActorClass` | `AcsCallFunctionTests.CallFunc_GetActorClass_ReturnsDoomImpForTid` |
| `ACSF_SoundSequenceOnActor/Sector/Polyobj` | `TrySoundSequenceOn*` → `AcsSoundSequenceActions` | `AcsSoundSequenceCallFuncTests.*` |
| `ACSF_GetPolyobjX/Y` | `TryGetPolyobjX/Y` | `AcsCallFunctionTests.CallFunc_GetPolyobjX_ReturnsFixedMaxWithoutPolyobjects` |
| `ACSF_CheckSight` | `TryCheckSight` | `AcsCallFunctionTests.CallFunc_CheckSight_ActivatorSeesSelf` |
| `ACSF_SpawnForced` | wire `SpawnForced` | `AcsHotCallFuncBehaviorTests.SpawnForcedCreatesActorAtFixedCoordinates` |
| `ACSF_ACS_NamedExecute` | `TryAcsNamedExecute` | `AcsAbsentCallFuncChunk11Tests.CallFunc_AcsNamedExecute_StartsScriptByName` |
| `ACSF_UniqueTID` / `IsTIDUsed` | `TryUniqueTid` / `TryIsTidUsed` | `AcsCallFunctionTests.CallFunc_UniqueTid_*` / `CallFunc_IsTidUsed_DetectsCoopPlayerTid` |
| `ACSF_Sqrt` / `FixedSqrt` / `VectorLength` | `TrySqrt` / `TryFixedSqrt` / `TryVectorLength` | `AcsCallFunctionTests.CallFunc_Sqrt_*` / `AcsFixedRoundingTests.VectorLengthRoundsFractionalFixedResults` |
| `ACSF_SetHUDClipRect` | `TrySetHudClipRect` | `AcsAbsentCallFuncChunk11Tests.CallFunc_SetHudClipRect_StoresOnSimulation` |
| `ACSF_SetCVar` / `SetCVarString` | `AcsModCvarService` via extended dispatch | `AcsModCvarServiceTests.CallFunc_SetCVar_*` |
| `ACSF_SetUserCVar` | `TrySetUserCVar` | `AcsModCvarServiceTests.CallFunc_SetUserCVar_MirrorsConsolePlayerGlobal` |

Dropped (no CallFunc-named test): `Radius_Quake2`, `SetHUDWrapWidth`, `GetCVarString`, `GetUserCVar*`, `SetUserCVarString`, `LineAttack` (separate conversion backlog).

Verified: `dotnet build -warnaserror`; `dotnet test csharp/HCDE.sln`.
