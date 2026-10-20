# Chunk 404 — CheckCmdLine warp/skill/map and CheckEpisodeCmd

## Scope

CheckCmdLine warp/skill/map and CheckEpisodeCmd

## Native ranges

- `src/d_main.cpp` **CheckCmdLine map defaults** 2682–2726 (45 lines)
- `src/d_main.cpp` **CheckCmdLine +map** 2731–2754 (24 lines)
- `src/d_main.cpp` **CheckCmdLine status tail** 2761–2800 (40 lines)
- `src/d_main.cpp` **CheckEpisodeCmd** 2846–2891 (46 lines)

**Credited native LOC:** 155 (tally `nativeLoc` 155).

## Managed

`VmDMainCheckCmdlineMap.cs`, `VmDMainCheckCmdlineMapTests.cs`, `VmDMainCheckEpisodeCmd.cs`
Managed LOC (tally): 89.
Conversion commit: `397241ced8d31ffffaab490211ee88a28febfb85`.

## Tests

- `VmDMainCheckCmdlineMapTests.DefaultStartMap_Mapxx_UsesMap01`
- `VmDMainCheckCmdlineMapTests.TryParseSkillDigit_SubtractsOne`
- `VmDMainCheckCmdlineMapTests.IsLauncherDefaultMap_AcceptsE1M1`
- `VmDMainCheckCmdlineMapTests.ParseWarpEpisode_SwapsWhenMapOutOfRange`
- `VmDMainCheckCmdlineMapTests.ShouldReplaceLauncherDefault_WhenEpisodeDiffers`
- `VmDMainCheckCmdlineMapTests.TryParseEpisodeArg_RejectsHighIndex`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 404 --to 404` (0 hard / 0 soft).

## Seams

`Args`, `P_CheckMapData`, and `StartScreen` are not hosted.

## Credit rationale

Credits the `CheckCmdLine` map/skill/`+map` tail and `CheckEpisodeCmd` from `d_main.cpp`.
