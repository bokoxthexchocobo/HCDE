# Chunk 366 — LoadVertexes, LoadZSegs, LoadGLZSegs, LoadZNodes binary layout and validation

## Scope

LoadVertexes, LoadZSegs, LoadGLZSegs, LoadZNodes binary layout and validation

## Native ranges

- `src/maploader/maploader.cpp` **MapLoader::LoadVertexes** 403–427 (25 lines)
- `src/maploader/maploader.cpp` **MapLoader::LoadZSegs** 435–460 (26 lines)
- `src/maploader/maploader.cpp` **MapLoader::LoadGLZSegs** 470–535 (66 lines)
- `src/maploader/maploader.cpp` **MapLoader::LoadZNodes** 543–664 (122 lines)

**Credited native LOC:** 239 (tally `nativeLoc` 239).

## Managed

`VmMapLoaderZNodeLoad.cs`, `VmMapLoaderZNodeLoadTests.cs`
Managed LOC (tally): 25.
Conversion commit: `41f2642817c7f2513ee5733c195d7a41d055f318`.

## Tests

- `VmMapLoaderZNodeLoadTests.EnsureHasVertexes_Zero_Throws`
- `VmMapLoaderZNodeLoadTests.ReadVertexRecords_ParsesLittleEndianPairs`
- `VmMapLoaderZNodeLoadTests.ReadZSegRecord_AdvancesOffset`
- `VmMapLoaderZNodeLoadTests.ValidateGlPartner_OutOfRange_Throws`
- `VmMapLoaderZNodeLoadTests.ReadGlLineIndex_Type1_MapsFFFFToInvalid`
- `VmMapLoaderZNodeLoadTests.EnsureSegCountMatches_Mismatch_Throws`
- `VmMapLoaderZNodeLoadTests.EnsureVertexNodeCounts_TooManyOrgVerts_Throws`
- `VmMapLoaderZNodeLoadTests.ComputeNewVertexReserve_WhenCountsMatch_ReturnsZeroReserve`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 366 --to 366` (0 hard / 0 soft).

## Seams

`Level` allocation, `FileReader`, and seg pointer wiring are not hosted; managed code mirrors lump sizing, record parsing, and GL/Z node validation gates.

## Credit rationale

Credits `MapLoader::LoadVertexes`, `LoadZSegs`, `LoadGLZSegs`, and `LoadZNodes` through the classic node child read loop in `maploader.cpp`.
