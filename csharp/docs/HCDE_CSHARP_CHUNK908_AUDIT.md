# Chunk 908 — `FParser::SF_SetCamera` … `SF_MoveFloor` from `fragglescript/t_func.cpp` (**1379–1593**). Re-credits retired **868** band at honest **215** LOC; `SF_CeilingHeight` deferred at **1595** (**869**).

## Scope

`FParser::SF_SetCamera` … `SF_MoveFloor` from `fragglescript/t_func.cpp` (**1379–1593**). Re-credits retired **868** band at honest **215** LOC; `SF_CeilingHeight` deferred at **1595** (**869**).

## Native ranges

- `src/playsim/fragglescript/t_func.cpp` **FParser::SF_SetCamera** 1379–1593 (215 lines)

**Credited native LOC:** 215 (tally `nativeLoc` 215).

## Managed

`VmFScriptParserSfCamera908Slice.cs`, `VmFScriptParserSfCamFloor.cs`, `VmFScriptInitFunctionsReg868.cs`, `VmFScriptInitFunctionsReg908.cs`
Managed LOC (tally): 194.
Conversion commit: `dcd7e3c73c0786cf5de90021bef26eabdfffcc3e`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native `SF_*` | Behavior ported |
|---------------|-----------------|
| `SF_SetCamera` | yes |
| `SF_ClearCamera` | yes |
| `SF_StartSound` | yes |
| `SF_StartSectorSound` | yes |
| `SF_FloorHeight` | yes |
| `SF_MoveFloor` | yes |

| Managed | Native (`t_func.cpp`) |
|---------|----------------------|
| `SfSetCamera` | **1391–1421** |
| `SfClearCamera` | **1429–1443** |
| `SfStartSound` | **1456–1469** |
| `SfStartSectorSound` | **1478–1495** |
| `SfFloorHeight` | **1504–1556** |
| `SfMoveFloor` | **1565–1586** |
| (separator) | **1379–1390** |
| (separator) | **1422–1428** |
| (separator) | **1444–1455** |
| (separator) | **1470–1477** |
| (separator) | **1496–1503** |
| (separator) | **1557–1564** |
| (separator) | **1587–1593** |

### Chunk **908** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **215** |
| **Net headline** | **+215** vs post-**907** |


**Post-review ratio:** 194/215 managed/native ≈ 90% (src file LOC at HEAD; see tally).

## Tests

- `VmFScriptParserSfCamFloorTests.SfSetCamera_SetsCameraThingId`
- `VmFScriptParserSfCamFloorTests.SfClearCamera_RestoresSavedAngleAndZ`
- `VmFScriptParserSfCamFloorTests.SfStartSound_OnActor_DoesNotThrow`
- `VmFScriptParserSfCamFloorTests.SfStartSectorSound_TaggedSector_QueuesSound`
- `VmFScriptParserSfCamFloorTests.SfFloorHeight_ReadsSectorFloor`
- `VmFScriptParserSfCamFloorTests.SfFloorHeight_WriteUpdatesPlaysimFloor`
- `VmFScriptParserSfCamFloorTests.SfMoveFloor_StartsSectorMotion`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 908 --to 908` (0 hard / 0 soft).

## Seams

Camera uses `CameraThingId` + actor save fields; sounds use `ActorSoundActions`; sector sound iterates tagged sectors via `StartSoundAt`; floors use `Floors[]` / `LineSpecials.ExecuteFloorSpecial` (279).

## Credit rationale

Full UNCredited **1379–1593** row with camera on exclusive `VmFScriptParserSfCamera908Slice.cs`; `SF_StartSectorSound` ported (retired **868** deferral cleared).
