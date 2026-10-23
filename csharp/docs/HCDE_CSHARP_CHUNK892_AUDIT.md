# Chunk 892 — `SlopeLineToPoint`, `CopyPlane`, `SetSlope`, `VavoomSlope`, `SetSlopesFromVertexHeights`, and `SpawnSlopeMakers` (`slopes.cpp` 41–421). `AlignPlane` / `SetSlopes` / `CopySlopes` deferred (next band).

## Scope

`SlopeLineToPoint`, `CopyPlane`, `SetSlope`, `VavoomSlope`, `SetSlopesFromVertexHeights`, and `SpawnSlopeMakers` (`slopes.cpp` 41–421). `AlignPlane` / `SetSlopes` / `CopySlopes` deferred (next band).

## Native ranges

- `src/maploader/slopes.cpp` **MapLoader::SlopeLineToPoint** 41–102 (62 lines)
- `src/maploader/slopes.cpp` **MapLoader::CopyPlane** 110–137 (28 lines)
- `src/maploader/slopes.cpp` **MapLoader::SetSlope** 145–186 (42 lines)
- `src/maploader/slopes.cpp` **MapLoader::VavoomSlope** 195–233 (39 lines)
- `src/maploader/slopes.cpp` **MapLoader::SetSlopesFromVertexHeights** 241–359 (119 lines)
- `src/maploader/slopes.cpp` **MapLoader::SpawnSlopeMakers** 367–421 (55 lines)

**Credited native LOC:** 345 (tally `nativeLoc` 345).

## Managed

`VmMapLoaderSlopes.cs`
Managed LOC (tally): 557.
Conversion commit: `136806b24feb21f83c70401e452dda1707d2e8af`.

## Padding review (2026-10-21)

**Behavior ported (yes/no)**

| Native | Behavior ported |
|--------|-----------------|
| `MapLoader::SlopeLineToPoint` **41–102** | **yes** |
| `MapLoader::CopyPlane` (both) **110–137** | **yes** |
| `MapLoader::SetSlope` **145–186** | **yes** |
| `MapLoader::VavoomSlope` **195–233** | **yes** |
| `MapLoader::SetSlopesFromVertexHeights` **241–359** | **yes** |
| `MapLoader::SpawnSlopeMakers` **367–421** | **yes** |

| Managed | Native (`slopes.cpp`) |
|---------|----------------------|
| `SlopeLineToPoint` | **41–102** |
| `CopyPlane` | **110–137** |
| `SetSlope` | **145–186** |
| `VavoomSlope` | **195–233** |
| `SetSlopesFromVertexHeights` | **241–359** |
| `SpawnSlopeMakers` | **367–421** |

### Padding map (hull **41–421**)

| Native lines | Reason |
|--------------|--------|
| **103–109**, **138–144**, **187–194**, **234–240**, **360–366** | comment/separator gaps between functions (in hull, no extra symbols) |

### Gross vs net

| | Value |
|--|------:|
| Gross native (`nativeLoc` sum of ranges) | **345** |
| Net behavior-ported | **345** |
| Gross CLI hull (**41–421**) | **381** |


**Post-review ratio:** 557/345 managed/native ≈ 161% (src file LOC at HEAD; see tally).

## Tests

- `VmMapLoaderSlopesTests.SlopeLineToPoint_TiltsFloorTowardPoint`
- `VmMapLoaderSlopesTests.SlopeLineToPoint_LogsWhenPointOnLine`
- `VmMapLoaderSlopesTests.CopyPlane_CopiesFloorBySectorTag`
- `VmMapLoaderSlopesTests.SetSlope_SetsFloorNormalFromAngles`
- `VmMapLoaderSlopesTests.VavoomSlope_UsesLineIdInSector`
- `VmMapLoaderSlopesTests.SetSlopesFromVertexHeights_SlopesTriangleSectorFloor`
- `VmMapLoaderSlopesTests.SetSlopesFromVertexHeights_ReadsVertexThingHeights`
- `VmMapLoaderSlopesTests.SpawnSlopeMakers_AppliesSetFloorSlopeThing`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 892 --to 892` (0 hard / 0 soft).

## Seams

**Representation notes:** `SlopeLevel` holds sectors/lines/vertices for map-load slope setup; `SlopeTagManager` mirrors `FTagManager` line/sector tag hashing for `GetLineIdIterator` / sector-tag walks. `PointInSector` uses polygon winding for tests. `GetStrifeType` / `ParseUSDF` are outside this hull.

## Credit rationale

Faithful slope plane construction from thing specials, vertex heights, and line-ID slope lines before line-based align/copy passes.
