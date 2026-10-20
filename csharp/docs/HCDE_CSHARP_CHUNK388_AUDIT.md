# Chunk 388 — Map load format flags, node signature selection, seg sidedef fixups, spawn ordering

## Scope

Map load format flags, node signature selection, seg sidedef fixups, spawn ordering

## Native ranges

- `src/maploader/maploader.cpp` **MapLoader::LoadMap** 2999–3048 (50 lines)
- `src/maploader/maploader.cpp` **MapLoader::LoadMap** 3056–3115 (60 lines)
- `src/maploader/maploader.cpp` **MapLoader::LoadMap** 3189–3245 (57 lines)

**Credited native LOC:** 167 (tally `nativeLoc` 167).

## Managed

`VmMapLoaderNodeLoadOrchestration.cs`, `VmMapLoaderNodeLoadOrchestrationTests.cs`
Managed LOC (tally): 49.
Conversion commit: `8c5cb0eaa4d2e44be05ca56795096b05b787ec2d`.

## Tests

- `VmMapLoaderNodeLoadOrchestrationTests.CombineNodeLoadResults_RequiresAllThree`
- `VmMapLoaderNodeLoadOrchestrationTests.ResolveHeadGameNodeIndex_PrefersGameNodes`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 388 --to 388` (0 hard / 0 soft).

## Seams

`LoadExtendedNodes`, `FNodeBuilder`, and `SpawnSlopeMakers` are not hosted.

## Credit rationale

Credits map format activation, node-load orchestration, and post-node seg fixups from `maploader.cpp`.
