# Chunk 389 — ProcessSideTextures specials and LoadBlockMap lump ingest

## Scope

ProcessSideTextures specials and LoadBlockMap lump ingest

## Native ranges

- `src/maploader/maploader.cpp` **MapLoader::ProcessSideTextures** 2053–2158 (106 lines)
- `src/maploader/maploader.cpp` **MapLoader::LoadBlockMap** 2594–2649 (56 lines)

**Credited native LOC:** 162 (tally `nativeLoc` 162).

## Managed

`VmMapLoaderLoadBlockMap.cs`, `VmMapLoaderProcessSideTextures.cs`, `VmMapLoaderProcessSideTexturesTests.cs`
Managed LOC (tally): 93.
Conversion commit: `e1ecebce0b2a9e22c947e76c8619e17bdfb73096`.

## Tests

- `VmMapLoaderProcessSideTexturesTests.ResolveSpecialDispatch_TransferHeights`
- `VmMapLoaderProcessSideTexturesTests.IsTranMapLumpName_MatchesPrefix`
- `VmMapLoaderLoadBlockMapTests.ShouldGenerateBlockMap_WhenLumpEmpty`
- `VmMapLoaderLoadBlockMapTests.ExpandWadOffset_NegativeOneBecomesMaxUInt`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 389 --to 389` (0 hard / 0 soft).

## Seams

`SetTexture`, `CreateBlockMap`, and `GPalette::DetermineTranslucency` are not hosted.

## Credit rationale

Credits `MapLoader::ProcessSideTextures` and `MapLoader::LoadBlockMap` from `maploader.cpp`.
