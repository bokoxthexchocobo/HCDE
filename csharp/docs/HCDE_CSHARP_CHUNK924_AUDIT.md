# Chunk 924 — `DLevelScript::CallFunction` ACSF cases from `p_acs.cpp` (**6766–6915**): `SetTranslation` through `GetLineHealth`. Continues **923** (**≤6764**); `GetLineX` at **6919** deferred.

## Scope

`DLevelScript::CallFunction` ACSF cases from `p_acs.cpp` (**6766–6915**): `SetTranslation` through `GetLineHealth`. Continues **923** (**≤6764**); `GetLineX` at **6919** deferred.

## Native ranges

- `src/playsim/p_acs.cpp` **DLevelScript::CallFunction** 6766–6915 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmPAcsCallfunc924Slice.cs`
Managed LOC (tally): 170.
Conversion commit: `e790957c0f68ebce102c105c8a18250c5391aea8`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native ACSF region | Behavior ported |
|--------------------|-----------------|
| `ACSF_SetTranslation` (**6766–6785**) | yes |
| `ACSF_SetSectorGlow` / `SetFogDensity` (**6788–6817**) | yes |
| `ACSF_GetActorFloorTexture` / `Terrain` (**6819–6847**) | yes |
| `ACSF_StrArg` / `Floor` / `Ceil` / `Round` (**6849–6863**) | yes |
| `ACSF_ScriptCall` / `StartSlideshow` (**6865–6872**) | yes |
| `ACSF_GetSectorHealth` / `GetLineHealth` (**6874–6915**) | yes |

| Managed (`VmPAcsCallfunc924Slice.cs`) | Native (`p_acs.cpp`) |
|--------------------------------------|---------------------|
| `TrySetTranslation` | **6766–6785** |
| `TrySetSectorGlow` | **6788–6804** |
| `TrySetFogDensity` | **6806–6817** |
| `TryGetActorFloorTexture` | **6819–6832** |
| `TryGetActorFloorTerrain` | **6834–6847** |
| `TryStrArg` | **6849–6851** |
| `TryFixedQuantize` | **6853–6863** |
| `TryScriptCall` | **6865–6867** |
| `TryStartSlideshow` | **6869–6872** |
| `TryGetDestructibleHealth` | **6874–6915** |

### Chunk **924** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**923** |


**Post-review ratio:** 170/150 managed/native ≈ 113% (src file LOC at HEAD; see tally).

## Tests

- `AcsAbsentCallFuncChunk11Tests.CallFunc_SetSectorGlow_WritesSectorFields`
- `AcsAbsentCallFuncChunk11Tests.CallFunc_SetFogDensity_ClampedPerSector`
- `AcsCallFunctionTests.CallFunc_GetActorFloorTexture_ReadsSectorFlat`
- `AcsCallFunctionTests.CallFunc_SetSectorTerrain_ThenGetActorFloorTerrainReadsName`
- `AcsCallFunctionTests.CallFunc_StrArg_ReturnsSpawnOptionStringInGlobalPool`
- `AcsCallFunctionTests.CallFunc_FloorRoundCeil_QuantizeFixedValues`
- `AcsScriptCallServiceTests.CallFunc_ScriptCall_InvokesRegisteredStaticIntFunction`
- `AcsAbsentCallFuncChunk11Tests.CallFunc_StartSlideshowAndSubtitle_StorePresentationState`
- `AcsCallFunctionTests.CallFunc_GetSectorHealth_ReturnsFloorHealthForTag`
- `AcsCallFunctionTests.CallFunc_GetLineHealth_ReturnsFirstTaggedLineHealth`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 924 --to 924` (0 hard / 0 soft).

## Seams

`ScriptCall` / `G_StartSlideshow`, `TexMan`, terrain tables, and health groups remain host seams.

## Credit rationale

Honest **150**-line UNCredited band in **6614–7509** corridor; exclusive `VmPAcsCallfunc924Slice.cs`.
