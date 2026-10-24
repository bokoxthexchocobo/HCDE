# Chunk 400 — SetSubsectorLightmap, SetSideLightmap, LoadLightmap ingest

## Scope

SetSubsectorLightmap, SetSideLightmap, LoadLightmap ingest

## Native ranges

- `src/maploader/maploader.cpp` **MapLoader::SetSubsectorLightmap** 3343–3364 (22 lines)
- `src/maploader/maploader.cpp` **MapLoader::SetSideLightmap** 3366–3395 (30 lines)
- `src/maploader/maploader.cpp` **MapLoader::LoadLightmap** 3397–3589 (193 lines)

**Credited native LOC:** 245 (tally `nativeLoc` 245).

## Managed

`VmMapLoaderLightmap.cs`, `VmMapLoaderLightmapSurfaceAssign.cs`, `VmMapLoaderLightmapTests.cs`
Managed LOC (tally): 204.
Conversion commit: `3082f0cbf99a67927f3c1a148bc9f15b1daf002d`.

## Tests

- `VmMapLoaderLightmapTests.ShouldApplySubsectorSurface_FloorOrCeiling`
- `VmMapLoaderLightmapTests.ResolveSideLightmapSlot_MiddleDuplicates`
- `VmMapLoaderLightmapTests.ShouldSkipLoad_WhenNolightmaps`
- `VmMapLoaderLightmapTests.ComputeTextureByteCount_MatchesFormula`
- `VmMapLoaderLightmapTests.CompareLightProbesByCell_SortsYThenX`
- `VmMapLoaderLightmapTests.WriteMainSubsectorSlot_UsesCeilingIndex`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 400 --to 400` (0 hard / 0 soft).

## Seams

`OpenDecompressor`, sidedef/subsector mutation, and texture upload are not hosted.

## Credit rationale

Credits `MapLoader::SetSubsectorLightmap`, `SetSideLightmap`, and `LoadLightmap` from `maploader.cpp`.
