# Chunk 375 — LoadSegs empty-lump gate, vertex validation, split-vertex angle threshold, badseg messages; LoadSubsectors BSP sizing gates

## Scope

LoadSegs empty-lump gate, vertex validation, split-vertex angle threshold, badseg messages; LoadSubsectors BSP sizing gates

## Native ranges

- `src/maploader/maploader.cpp` **MapLoader::LoadSegs** 814–822 (9 lines)
- `src/maploader/maploader.cpp` **MapLoader::LoadSegs** 857–860 (4 lines)
- `src/maploader/maploader.cpp` **MapLoader::LoadSegs** 897–901 (5 lines)
- `src/maploader/maploader.cpp` **MapLoader::LoadSegs** 952–968 (17 lines)
- `src/maploader/maploader.cpp` **MapLoader::LoadSubsectors** 991–996 (6 lines)
- `src/maploader/maploader.cpp` **MapLoader::LoadSubsectors** 1026–1044 (19 lines)

**Credited native LOC:** 60 (tally `nativeLoc` 60).

## Managed

`VmMapLoaderLoadSegs.cs`
Managed LOC (tally): 118.
Conversion commit: `02b8ffca5d2d5c9097e752921b71fb083e7c8746`.

## Tests

- `VmMapLoaderLoadSegsTests.ShouldAbortEmptySegs_WhenZero`
- `VmMapLoaderLoadSegsTests.CountSegs_DividesLumpByRecordSize`
- `VmMapLoaderLoadSegsTests.ValidateVertexIndex_ThrowsWhenOutOfRange`
- `VmMapLoaderLoadSegsTests.ShouldSnapSplitVertex_AtOneDegree`
- `VmMapLoaderLoadSegsTests.FormatBadSegMessage_Vertex_IncludesIndices`
- `VmMapLoaderLoadSegsTests.DescribeBspFailure_AppendsRebuildBanner`
- `VmMapLoaderLoadSegsTests.ShouldRejectZeroSubsectors`
- `VmMapLoaderLoadSegsTests.IsSubsectorSegRangeInvalid_DetectsOverflow`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 375 --to 375` (0 hard / 0 soft).

## Seams

Seg iteration, vertex snap geometry, linedef/sidedef binding, allocation, and subsector read loops are not hosted.

## Credit rationale

Credits only native bands exercised by named tests — not the full LoadSegs/LoadSubsectors function bodies.
