# Chunk 391 — LoadSideDefs2 sector fixups, VerifyBlockMap validation, GroupLines subsector hookup

## Scope

LoadSideDefs2 sector fixups, VerifyBlockMap validation, GroupLines subsector hookup

## Native ranges

- `src/maploader/maploader.cpp` **MapLoader::LoadSideDefs2** 2169–2218 (50 lines)
- `src/maploader/maploader.cpp` **FBlockmap::VerifyBlockMap** 2508–2581 (74 lines)
- `src/maploader/maploader.cpp` **MapLoader::GroupLines** 2659–2705 (47 lines)

**Credited native LOC:** 171 (tally `nativeLoc` 171).

## Managed

`VmMapLoaderGroupLines.cs`, `VmMapLoaderLoadSideDefs2.cs`, `VmMapLoaderLoopSidedefsRightEdge.cs`
Managed LOC (tally): 57.
Conversion commit: `9f9cecb05cdeb144d1aa9aee51dd4d8ec8bd00f6`.

## Tests

- `VmMapLoaderLoadSideDefs2Tests.ShouldPatchStrifeLaserRow_ForStrifeGame`
- `VmMapLoaderLoadBlockMapTests.IsLineIndexOutOfRange_RejectsHighIndex`
- `VmMapLoaderGroupLinesTests.ShouldFlagMissingFrontSector`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 391 --to 391` (0 hard / 0 soft).

## Seams

`mapsidedef_t` IO and blockmap lump memory are not hosted.

## Credit rationale

Credits `MapLoader::LoadSideDefs2`, `FBlockmap::VerifyBlockMap`, and the opening of `GroupLines` from `maploader.cpp`.
