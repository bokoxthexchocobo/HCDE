# Chunk 401 — SetTexture gaps, SetTextureNoErr, LoadSegs preamble and split-vertex snap

## Scope

SetTexture gaps, SetTextureNoErr, LoadSegs preamble and split-vertex snap

## Native ranges

- `src/maploader/maploader.cpp` **MapLoader::SetTexture side header** 143–150 (8 lines)
- `src/maploader/maploader.cpp` **MapLoader::SetTexture side default** 170–174 (5 lines)
- `src/maploader/maploader.cpp` **MapLoader::SetTexture sector missing** 193–196 (4 lines)
- `src/maploader/maploader.cpp` **MapLoader::SetTexture sector tail** 210–219 (10 lines)
- `src/maploader/maploader.cpp` **SummarizeMissingTextures tail** 236–242 (7 lines)
- `src/maploader/maploader.cpp` **MapLoader::SetTextureNoErr** 270–341 (72 lines)
- `src/maploader/maploader.cpp` **badseg and LoadSegs header** 783–813 (31 lines)
- `src/maploader/maploader.cpp` **MapLoader::LoadSegs alloc** 823–856 (34 lines)
- `src/maploader/maploader.cpp` **MapLoader::LoadSegs split vertex** 861–896 (36 lines)

**Credited native LOC:** 207 (tally `nativeLoc` 193).

## Managed

`VmMapLoaderLoadSegsBadseg.cs`, `VmMapLoaderSetTextureVariants.cs`, `VmMapLoaderSetTextureVariantsTests.cs`
Managed LOC (tally): 141.
Conversion commit: `0ec58da571011f8ba6d3921a486c225de78c768c`.

## Tests

- `VmMapLoaderSetTextureVariantsTests.ShouldPrintMissingTextureWarning_UntilLimit`
- `VmMapLoaderSetTextureVariantsTests.ShouldScanLinedefs_WhenTextureMissing`
- `VmMapLoaderSetTextureVariantsTests.TruncateFlatName_EightChars`
- `VmMapLoaderSetTextureVariantsTests.ShouldSummarizeExtraUsage_AfterLimit`
- `VmMapLoaderSetTextureVariantsTests.TryParseLegacyColorHash_ValidSixChar`
- `VmMapLoaderSetTextureVariantsTests.ResolveDeepWaterBlend_PrefersColormap`
- `VmMapLoaderSetTextureVariantsTests.FormatBadSegMessage_Vertex_IncludesIndices`
- `VmMapLoaderSetTextureVariantsTests.CountSegs_DividesLumpByRecordSize`
- `VmMapLoaderSetTextureVariantsTests.ShouldSnapSplitVertex_WhenAngleDeltaOneDegree`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 401 --to 401` (0 hard / 0 soft).

## Seams

`TexMan`, seg iteration, and vertex mutation are not hosted.

## Credit rationale

Credits non-overlapping `SetTexture`/`SetTextureNoErr` bands plus `LoadSegs` preamble and split-vertex angle correction from `maploader.cpp`.
