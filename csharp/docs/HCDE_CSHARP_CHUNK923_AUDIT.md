# Chunk 923 — `DLevelScript::CallFunction` ACSF cases from `p_acs.cpp` (**6614–6764**): `GetMaxInventory` through `SetActorFlag`. Head of uncredited **6614–7509** corridor; `SetTranslation` at **6766** deferred.

## Scope

`DLevelScript::CallFunction` ACSF cases from `p_acs.cpp` (**6614–6764**): `GetMaxInventory` through `SetActorFlag`. Head of uncredited **6614–7509** corridor; `SetTranslation` at **6766** deferred.

## Native ranges

- `src/playsim/p_acs.cpp` **DLevelScript::CallFunction** 6614–6764 (151 lines)

**Credited native LOC:** 151 (tally `nativeLoc` 151).

## Managed

`VmPAcsCallfunc923Slice.cs`
Managed LOC (tally): 174.
Conversion commit: `f4df2e62595928cd86f2521883213e5253f99078`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native ACSF region | Behavior ported |
|--------------------|-----------------|
| `ACSF_GetMaxInventory` (**6614–6621**) | yes |
| `ACSF_SetSectorDamage` (**6623–6638**) | yes |
| `ACSF_SetSectorTerrain` (**6640–6654**) | yes |
| `ACSF_SpawnParticle` (**6656–6687**) | yes (stack consume) |
| `ACSF_SetMusicVolume` (**6689–6692**) | yes |
| `ACSF_CheckProximity` (**6694–6705**) | yes |
| `ACSF_CheckActorState` (**6707–6718**) | yes |
| `ACSF_CheckClass` (**6720–6725**) | yes |
| `ACSF_DamageActor` (**6727–6735**) | yes |
| `ACSF_SetActorFlag` (**6737–6764**) | yes |

| Managed (`VmPAcsCallfunc923Slice.cs`) | Native (`p_acs.cpp`) |
|--------------------------------------|---------------------|
| `TryGetMaxInventory` | **6614–6621** |
| `TrySetSectorDamage` | **6623–6638** |
| `TrySetSectorTerrain` | **6640–6654** |
| `TrySpawnParticle` | **6656–6687** |
| `TrySetMusicVolume` | **6689–6692** |
| `TryCheckProximity` | **6694–6705** |
| `TryCheckActorState` | **6707–6718** |
| `TryCheckClass` | **6720–6725** |
| `TryDamageActor` | **6727–6735** |
| `TrySetActorFlag` | **6737–6764** |

### Chunk **923** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **151** |
| **Net headline** | **+151** vs post-**922** |


**Post-review ratio:** 174/151 managed/native ≈ 115% (src file LOC at HEAD; see tally).

## Tests

- `AcsPlayerInventoryTests.CallFunc_GetMaxInventory_ReadsActivatorClipMax`
- `SetSectorDamageTests.MapAndScriptRoutesChangeTaggedSectorsAndGameplay`
- `AcsCallFunctionTests.CallFunc_SetSectorTerrain_ThenGetActorFloorTerrainReadsName`
- `AcsCallFunctionTests.CallFunc_SpawnParticle_AcceptsArguments`
- `AcsCallFunctionTests.CallFunc_SetMusicVolume_StoresScale`
- `AcsCallFunctionTests.CallFunc_CheckProximity_FindsNearbyImp`
- `AcsCallFunctionTests.CallFunc_CheckActorState_FindsSpawnLabelOnImp`
- `AcsCallFunctionTests.CallFunc_CheckClass_RecognizesKnownSpawnClass`
- `AcsCallFunctionTests.CallFunc_DamageActor_ReducesTidTargetHealth`
- `AcsCallFunctionTests.CallFunc_SetActorFlag_AndCheckFlag_Invulnerable`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 923 --to 923` (0 hard / 0 soft).

## Seams

Full `CallFunction` dispatch, `P_SpawnParticle`, `P_Thing_CheckProximity`, `ModActorFlag`, and `CheckInventory` max path remain host seams.

## Credit rationale

Honest **≥150** UNCredited head slice after chunk **11** (**≤6613**); exclusive `VmPAcsCallfunc923Slice.cs`.
