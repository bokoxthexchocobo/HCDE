# Chunk 384 — SetMapThingUserData, SpawnThings, AllocateSideDefs, LoopSidedefs vertex linking

## Scope

SetMapThingUserData, SpawnThings, AllocateSideDefs, LoopSidedefs vertex linking

## Native ranges

- `src/maploader/maploader.cpp` **MapLoader::SetMapThingUserData** 1238–1273 (36 lines)
- `src/maploader/maploader.cpp` **MapLoader::SpawnThings** 1437–1450 (14 lines)
- `src/maploader/maploader.cpp` **MapLoader::AllocateSideDefs** 1888–1909 (22 lines)
- `src/maploader/maploader.cpp` **MapLoader::LoopSidedefs** 1918–2045 (128 lines)

**Credited native LOC:** 200 (tally `nativeLoc` 200).

## Managed

`VmMapLoaderAllocateSideDefs.cs`, `VmMapLoaderSpawnThings.cs`, `VmMapLoaderSpawnThingsTests.cs`
Managed LOC (tally): 157.
Conversion commit: `6b87c82e0578c2fe542d3b84d90fe3f0b0d6e385`.

## Tests

- `VmMapLoaderSpawnThingsTests.IsWritableUserField_RejectsNativeOrStatic`
- `VmMapLoaderSpawnThingsTests.EnumerateUserDataChain_StopsAtEmptyKey`
- `VmMapLoaderAllocateSideDefsTests.ShouldReportUnusedSidedefs_WhenLumpHasExtra`
- `VmMapLoaderAllocateSideDefsTests.ResolveSideLineAttachment_PicksVertexBySide`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 384 --to 384` (0 hard / 0 soft).

## Seams

`PField` reflection, `SpawnMapThing`, and full angle-based right-side search are not hosted.

## Credit rationale

Credits `SetMapThingUserData`, `SpawnThings`, `AllocateSideDefs`, and the opening of `LoopSidedefs` from `maploader.cpp`.
