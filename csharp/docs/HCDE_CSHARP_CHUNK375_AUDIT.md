# Chunk 375 — LoadSegs vertex validation and LoadSubsectors lump sizing gates

## Scope

LoadSegs vertex validation and LoadSubsectors lump sizing gates

## Native ranges

- `src/maploader/maploader.cpp` **MapLoader::LoadSegs** 802–975 (174 lines)
- `src/maploader/maploader.cpp` **MapLoader::LoadSubsectors** 985–1047 (63 lines)

**Credited native LOC:** 237 (tally `nativeLoc` 237).

## Managed

`VmMapLoaderLoadSegs.cs`
Managed LOC (tally): 50.
Conversion commit: `02b8ffca5d2d5c9097e752921b71fb083e7c8746`.

## Tests

- `VmMapLoaderLoadSegsTests.ShouldAbortEmptySegs_WhenZero`
- `VmMapLoaderLoadSegsTests.ValidateVertexIndex_ThrowsWhenOutOfRange`
- `VmMapLoaderLoadSegsTests.CountSegs_DividesLumpByRecordSize`
- `VmMapLoaderLoadSegsTests.ShouldSnapSplitVertex_AtOneDegree`
- `VmMapLoaderLoadSegsTests.FormatBadSegMessage_Vertex_IncludesIndices`
- `VmMapLoaderLoadSegsTests.ShouldRejectZeroSubsectors`
- `VmMapLoaderLoadSegsTests.IsSubsectorSegRangeInvalid_DetectsOverflow`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 375 --to 375` (0 hard / 0 soft).

## Seams

Seg/subsector allocation, `badseg` exceptions, and linedef vertex marking loops are not fully hosted.

## Credit rationale

Credits the opening `MapLoader::LoadSegs` validation path and `MapLoader::LoadSubsectors` sizing from `maploader.cpp`.
