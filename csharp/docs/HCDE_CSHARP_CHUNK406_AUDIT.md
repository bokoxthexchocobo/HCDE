# Chunk 406 — Classic node fallback and LoadLevel finalize/subsector bounds

## Scope

Classic node fallback and LoadLevel finalize/subsector bounds

## Native ranges

- `src/maploader/maploader.cpp` **MapLoader::LoadLevel classic nodes** 3091–3114 (24 lines)
- `src/maploader/maploader.cpp` **MapLoader::LoadLevel force node build** 3123–3183 (61 lines)
- `src/maploader/maploader.cpp` **MapLoader::LoadLevel finalize** 3225–3335 (111 lines)

**Credited native LOC:** 196 (tally `nativeLoc` 196).

## Managed

`VmMapLoaderClassicNodeFallback.cs`, `VmMapLoaderForceNodeBuild.cs`, `VmMapLoaderLoadLevelFinalize.cs`, `VmMapLoaderLoadLevelFinalizeTests.cs`
Managed LOC (tally): 134.
Conversion commit: `1c0f8bbbd2b3ff4eb226038d7b95eda9cdc450c3`.

## Tests

- `VmMapLoaderLoadLevelFinalizeTests.CombineClassicNodeLoadResults_RequiresAllThree`
- `VmMapLoaderLoadLevelFinalizeTests.ShouldFallbackToZdbsp_WhenNanoFails`
- `VmMapLoaderLoadLevelFinalizeTests.ResolveHeadGameNodeIndex_PrefersGameNodes`
- `VmMapLoaderLoadLevelFinalizeTests.ShouldLoadLightmapAfterSpawn_WhenNotForceRebuild`
- `VmMapLoaderLoadLevelFinalizeTests.ComputeNodeLength_UsesHypotenuse`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 406 --to 406` (0 hard / 0 soft).

## Seams

`LoadGLNodes`, `CreateVBO`, and `SpawnThings` are not hosted.

## Credit rationale

Credits classic node-load fallback in `LoadLevel` and the post-spawn finalize/subsector bbox loop from `maploader.cpp`.
