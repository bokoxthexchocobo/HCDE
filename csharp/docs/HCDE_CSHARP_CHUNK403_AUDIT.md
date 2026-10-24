# Chunk 403 — BlockHash, BlockCompare, CreatePackedBlockmap, CreateBlockMap span walks

## Scope

BlockHash, BlockCompare, CreatePackedBlockmap, CreateBlockMap span walks

## Native ranges

- `src/maploader/maploader.cpp` **BlockHash** 2237–2246 (10 lines)
- `src/maploader/maploader.cpp` **BlockCompare** 2248–2270 (23 lines)
- `src/maploader/maploader.cpp` **CreatePackedBlockmap** 2272–2321 (50 lines)
- `src/maploader/maploader.cpp` **MapLoader::CreateBlockMap spans** 2398–2496 (99 lines)

**Credited native LOC:** 182 (tally `nativeLoc` 182).

## Managed

`VmMapLoaderCreateBlockMapSpan.cs`, `VmMapLoaderPackedBlockmap.cs`, `VmMapLoaderPackedBlockmapTests.cs`
Managed LOC (tally): 176.
Conversion commit: `69de734449e504ffc61b6d7b5636d1d2a62a8b74`.

## Tests

- `VmMapLoaderPackedBlockmapTests.ComputeBlockHash_MatchesLinearCongruence`
- `VmMapLoaderPackedBlockmapTests.PackBlocks_DeduplicatesIdenticalLists`
- `VmMapLoaderPackedBlockmapTests.PushHorizontalSpan_FillsInclusiveRange`
- `VmMapLoaderPackedBlockmapTests.ShouldInvokePackedBuilder_RejectsZeroWidth`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 403 --to 403` (0 hard / 0 soft).

## Seams

`TArray`, `memset`, and `Scale` fixed-point walks are not hosted.

## Credit rationale

Credits `BlockHash`, `BlockCompare`, `CreatePackedBlockmap`, and the line-span tail of `MapLoader::CreateBlockMap` from `maploader.cpp`.
