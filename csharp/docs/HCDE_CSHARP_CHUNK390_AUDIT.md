# Chunk 390 — Generated blockmap extents and line bucketing

## Scope

Generated blockmap extents and line bucketing

## Native ranges

- `src/maploader/maploader.cpp` **MapLoader::CreateBlockMap** 2324–2496 (173 lines)

**Credited native LOC:** 173 (tally `nativeLoc` 173).

## Managed

`VmMapLoaderCreateBlockMap.cs`
Managed LOC (tally): 37.
Conversion commit: `ce2e1325f9641c558cad7fcee86dc9a7a73c9258`.

## Tests

- `VmMapLoaderCreateBlockMapTests.IsSingleBlock_WhenSameCell`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 390 --to 390` (0 hard / 0 soft).

## Seams

`CreatePackedBlockmap`, `TArray`, and `Scale` fixed-point walks are not hosted.

## Credit rationale

Credits `MapLoader::CreateBlockMap` from `maploader.cpp`.
