# Chunk 890 — `clearnodecache`, `PointOnLine`, and `MapLoader::SetRenderSector` (`glnodes.cpp` 1196–1396). LoadGLVertexes+DoLoadGLNodes cannot share one `--band` hull (credited 241–643 between them); deferred to chunk 891+.

## Scope

`clearnodecache`, `PointOnLine`, and `MapLoader::SetRenderSector` (`glnodes.cpp` 1196–1396). LoadGLVertexes+DoLoadGLNodes cannot share one `--band` hull (credited 241–643 between them); deferred to chunk 891+.

## Native ranges

- `src/maploader/glnodes.cpp` **clearnodecache** 1196–1223 (28 lines)
- `src/maploader/glnodes.cpp` **PointOnLine** 1247–1274 (28 lines)
- `src/maploader/glnodes.cpp` **MapLoader::SetRenderSector** 1286–1396 (111 lines)

**Credited native LOC:** 167 (tally `nativeLoc` 167).

## Managed

`VmMapLoaderGlRenderSector.cs`
Managed LOC (tally): 264.
Conversion commit: `fa247ab0a3757790c7558a5bf553fac934052c14`.

## Padding review (2026-10-21)

**Behavior ported (yes/no)**

| Native | Behavior ported |
|--------|-----------------|
| `clearnodecache` **1196–1223** | **yes** |
| `PointOnLine` **1247–1274** | **yes** |
| `MapLoader::SetRenderSector` **1286–1396** | **yes** |

| Managed | Native (`glnodes.cpp`) |
|---------|----------------------|
| `ClearNodeCache` (clearnodecache) | **1196–1223** |
| `PointOnLine` | **1247–1274** |
| `SetRenderSector` | **1286–1396** |

### Padding map (hull **1196–1396**)

| Native lines | Reason |
|--------------|--------|
| **1224–1246**, **1275–1285** | comment/separator gaps between functions (in hull, no extra symbols) |

### Gross vs net

| | Value |
|--|------:|
| Gross native (`nativeLoc` sum of ranges) | **167** |
| Net behavior-ported | **167** |

### Out-of-body uncredit (890)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **167** |
| **Out-of-body removed** | **0** |
| **Honest** | **167** |


**Post-review ratio:** 264/167 managed/native ≈ 158% (src file LOC at HEAD; see tally).

## Tests

- `VmMapLoaderGlRenderSectorTests.ClearNodeCache_DeletesTopLevelCacheEntries`
- `VmMapLoaderGlRenderSectorTests.ClearNodeCache_NoThrowWhenCacheDirectoryMissing`
- `VmMapLoaderGlRenderSectorTests.PointOnLine_TrueWhenOnPartition`
- `VmMapLoaderGlRenderSectorTests.PointOnLine_FalseWhenFarFromLine`
- `VmMapLoaderGlRenderSectorTests.SetRenderSector_PicksSectorFromBoundarySeg`
- `VmMapLoaderGlRenderSectorTests.SetRenderSector_FallsBackToRealSectorWhenUndetermined`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 890 --to 890` (0 hard / 0 soft).

## Seams

**Representation notes:** `RenderSeg` / `RenderSubsector` use seg indices and fixed-point vertex ints instead of native `seg_t*` / `subsector_t*`. `ClearNodeCache` uses `System.IO` and `XDG_CACHE_HOME` / `$HOME/.cache` like native `M_GetCachePath` + `FileSys::ScanDirectory` on Unix.

## Credit rationale

Faithful GL render-sector assignment and partner-seg validation after GL BSP load; `PointOnLine` matches node-builder epsilon test; `clearnodecache` backward delete order matches native.
