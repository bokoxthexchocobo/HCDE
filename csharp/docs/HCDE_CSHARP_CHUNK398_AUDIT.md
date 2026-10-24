# Chunk 398 — LoadBehavior, GetPolySpots, CalcIndices, LoadLevel translator preamble

## Scope

LoadBehavior, GetPolySpots, CalcIndices, LoadLevel translator preamble

## Native ranges

- `src/maploader/maploader.cpp` **MapLoader::LoadBehavior** 2858–2869 (12 lines)
- `src/maploader/maploader.cpp` **MapLoader::GetPolySpots** 2877–2901 (25 lines)
- `src/maploader/maploader.cpp` **MapLoader::CalcIndices** 2909–2936 (28 lines)
- `src/maploader/maploader.cpp` **MapLoader::LoadLevel translator** 2944–2998 (55 lines)
- `src/maploader/maploader.cpp` **MapLoader::LoadLevel geometry dispatch** 3026–3055 (30 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmMapLoaderLoadBehaviorPolyTests.cs`
Managed LOC (tally): 0.
Conversion commit: `e451074c88881917074057696bde5feb9565ea1c`.

## Tests

- `VmMapLoaderLoadBehaviorTests.ShouldLoadBehaviorModule_WhenLumpPresent`
- `VmMapLoaderLoadBehaviorTests.ShouldUnloadAcsOnCheckFailure_WhenNotGood`
- `VmMapLoaderGetPolySpotsTests.CollectPolyStart_SplitsAnchorAndSpot`
- `VmMapLoaderCalcIndicesTests.ApplyCalcIndices_FillsSequential`
- `VmMapLoaderLoadLevelPreambleTests.ResolveTranslator_PrefersMapInfo`
- `VmMapLoaderLoadLevelPreambleTests.ApplyDummySwitchesFlag_SetsForDoomFormat`
- `VmMapLoaderLoadLevelPreambleTests.ShouldLoadVertexesForBinaryMap`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 398 --to 398` (0 hard / 0 soft).

## Seams

`MapData` IO, `P_LoadTranslator`, and node build orchestration are not hosted.

## Credit rationale

Credits `MapLoader::LoadBehavior`, `GetPolySpots`, `CalcIndices`, and the behavior/translator/maptype preamble of `LoadLevel` from `maploader.cpp`.
