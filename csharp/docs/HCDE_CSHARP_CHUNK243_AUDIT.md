# Chunk 243 — STAT_StartNewGame and StoreLevelStats.

## Scope

STAT_StartNewGame and StoreLevelStats.

## Native ranges

- `src/gamedata/statistics.cpp` **STAT_StartNewGame** 352–367 (16 lines)
- `src/gamedata/statistics.cpp` **compare_level_names** 375–381 (7 lines)
- `src/gamedata/statistics.cpp` **StoreLevelStats** 384–420 (37 lines)

**Credited native LOC:** 60 (tally `nativeLoc` 60).

## Managed

`VmActorKeyCheck.cs`, `VmActorKeyCheckTests.cs`, `VmDecalLibCore.cs`, `VmGameInfoParseGameInfoTests.cs`, `VmStatisticsModels.cs`, `VmStatisticsModelsTests.cs`, `VmStatisticsParser.cs`, `VmStatisticsParserTests.cs`, `VmStatisticsTimeFormat.cs`, `VmStatisticsTimeFormatTests.cs`
Managed LOC (tally): 75.
Conversion commit: `e0dff6e4e1d2444fa60803b02ab92e491a153096`.

## Tests

- `VmStatisticsModelsTests.StatFileDefault_MatchesNativeCvar`
- `VmStatisticsParserTests.CompareEpisodeNames_IsCaseInsensitive`
- `VmStatisticsTimeFormatTests.Seconds_ExtractsFromTics`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 243 --to 243` (0 hard / 0 soft).

## Seams

Native engine globals and VM dispatch are not re-hosted; only credited control flow is modeled.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
