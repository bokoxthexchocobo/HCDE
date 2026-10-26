# Chunk 868 — `FParser::SF_SetCamera` … `SF_MoveFloor` from `fragglescript/t_func.cpp` (1381–1586); `SF_StartSectorSound` deferred. Retired (2026-10-23): out-of-body credit removed; honest **137** < **150**.

## Scope

`FParser::SF_SetCamera` … `SF_MoveFloor` from `fragglescript/t_func.cpp` (1381–1586); `SF_StartSectorSound` deferred. Retired (2026-10-23): out-of-body credit removed; honest **137** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `017bd16cb8e22236b73115b7f3620230c213385a`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native `SF_*` | Behavior ported | Managed playsim callee |
|---------------|-----------------|----------------------|
| `SF_SetCamera` | yes | `CameraThingId`, actor angle/Z/pitch |
| `SF_ClearCamera` | yes | restore saved actor fields, player camera |
| `SF_StartSound` | yes | `ActorSoundActions.StartSound` |
| `SF_FloorHeight` read/set | yes | `FloorOf` / `Floors[]` + crush helpers |
| `SF_MoveFloor` | yes | `LineSpecials.ExecuteFloorSpecial` (279) → `SectorMotion` |
| `SF_StartSectorSound` | **no** | deferred — no synchronous sector `S_Sound` path |

**Deferred (zero credit in this chunk)**

| Native | Why |
|--------|-----|
| `SF_StartSectorSound` (**1478–1495**) | ACS-style sector queue ≠ native immediate sector sound |
| Later-band `SF_*` (ceilings, lights, doors, …) | see chunk **869+** |

| Managed | Native (`t_func.cpp`) |
|---------|----------------------|
| `SfSetCamera` | **1391–1421** |
| `SfClearCamera` | **1429–1443** |
| `SfStartSound` | **1456–1469** |
| `SfFloorHeight` | **1504–1556** |
| `SfMoveFloor` | **1565–1586** |
| `VmFScriptInitFunctionsReg868.Register` | `NewFunction` rows for ported names only |
| (separators) | **1381–1390**, **1422–1428**, **1444–1455**, **1496–1503**, **1557–1564** |
| (deferred, uncredited here) | **1470–1495** |

### Chunk **868** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` (ported symbols + separators only) | **180** |
| **Net headline** | **+180** vs post-**866** |

### Chunk **851** before/after **868** land

| | `nativeLoc` | `managedLoc` (`VmFScriptInitFunctions.cs`) |
|--|------------:|---------------------------------------------:|
| Before | **215** | **283** |
| After | **215** | **283** (one `Register` call; registration bodies in **868** file) |

### Out-of-body uncredit (868)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **180** |
| **Out-of-body removed** | **43** |
| **Honest** | **137** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 868 --to 868` (0 hard / 0 soft).

## Seams

Camera save uses actor `specialf1`/`specialf2` fields; floor/move call `AuthoritySimulation.Floors` / `LineSpecials.ExecuteFloorSpecial` (`SectorMotion`); actor sounds use `ActorSoundActions.StartSound` (immediate `QueueSoundRequest`). **`SF_StartSectorSound`** deferred: `QueueSectorAcsSound` only buffers requests (even with `SoundRequestsReady`), not synchronous `S_Sound` at sector.

## Credit rationale

First credited `t_func` deferred-band slice after **851**; registrations in `VmFScriptInitFunctionsReg868.Register` (not **851** `managedLoc`).
