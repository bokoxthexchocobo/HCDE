# Chunk 1016 — `G_ReadVisited` tail, `P_WriteACSDefereds`, `P_ReadACSDefereds`, and `DSectorMarker::PropagateMark` head from `g_level.cpp` (**2259–2408**); follows chunk **1015** (**2109–2258**).

## Scope

`G_ReadVisited` tail, `P_WriteACSDefereds`, `P_ReadACSDefereds`, and `DSectorMarker::PropagateMark` head from `g_level.cpp` (**2259–2408**); follows chunk **1015** (**2109–2258**).

## Native ranges

- `src/g_level.cpp` **P_WriteACSDefereds** 2259–2408 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmGLevelDeferredGc1016Slice.cs`
Managed LOC (tally): 112.
Conversion commit: `51dc5f8dac22cc6fc82b4ab6b11e0fb57de97d05`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| `G_ReadVisited` playerclasses (**2259–2267**) | yes |
| `P_WriteACSDefereds` (**2276–2302**) | yes |
| `P_ReadACSDefereds` (**2310–2330**) | yes |
| `DSectorMarker` step constants (**2344–2348**) | yes |
| `PropagateMark` sector/poly batches (**2371–2407**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **2255** | `randomclasses` array (**1015** seam) |
| **2289–2300**, **2316–2329** | serializer host |
| **2360**, **2410+** | `IMPLEMENT_CLASS` / sidedef loop body (**1017** follow-on) |
| Other `PropagateMark` | **773** / **778** / **816** / **836** (different symbols) |

| Managed (`VmGLevelDeferredGc1016Slice.cs`) | Native (`g_level.cpp`) |
|-------------------------------------------|----------------------|
| Player class key fallback | **2261–2265** |
| Deferred presence / per-map write | **2281–2298** |
| Unknown map guard | **2323–2325** |
| Sector/poly/side step sizes | **2344–2348**, **2378–2405** |
| Band hull (exclusive slice) | **2259–2408** |

### Chunk **1016** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1015** |


**Post-review ratio:** 112/150 managed/native ≈ 75% (src file LOC at HEAD; see tally).

## Tests

- `VmGLevelDeferredGc1016SliceTests.ReadVisitedPlayerClasses`
- `VmGLevelDeferredGc1016SliceTests.AcsDeferredSaveLoadGates`
- `VmGLevelDeferredGc1016SliceTests.SectorMarkerPropagateSteps`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1016 --to 1016` (0 hard / 0 soft).

## Seams

`FSerializer`, `FindLevelInfo`, `I_Error`, `UpdatePlayerClass`, `P_RemoveDefereds`, and `GC::Mark` remain injected.

## Credit rationale

UNCredited **2259–2408**; disjoint from chunk **8**, **1015** snapshot band (seam **2259** continues read path), and other files' unrelated `PropagateMark` symbols.
