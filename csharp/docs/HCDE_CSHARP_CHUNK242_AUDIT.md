# Chunk 242 — SaveStatistics and time format helpers.

## Scope

SaveStatistics and time format helpers.

## Native ranges

- `src/gamedata/statistics.cpp` **compare_episode_names and compare_dates** 188–213 (26 lines)
- `src/gamedata/statistics.cpp` **hours minutes seconds helpers** 222–224 (3 lines)
- `src/gamedata/statistics.cpp` **SaveStatistics** 226–270 (45 lines)
- `src/gamedata/statistics.cpp` **GetStatisticsList and entries** 278–342 (65 lines)

**Credited native LOC:** 139 (tally `nativeLoc` 139).

## Managed

`VmActorKeyCheck.cs`, `VmActorKeyCheckTests.cs`, `VmDecalLibCore.cs`, `VmGameInfoParseGameInfoTests.cs`, `VmStatisticsModels.cs`, `VmStatisticsModelsTests.cs`, `VmStatisticsParser.cs`, `VmStatisticsParserTests.cs`, `VmStatisticsTimeFormat.cs`, `VmStatisticsTimeFormatTests.cs`
Managed LOC (tally): 55.
Conversion commit: `e0dff6e4e1d2444fa60803b02ab92e491a153096`.

## Tests

- `VmStatisticsParserTests.CompareEpisodeNames_IsCaseInsensitive`
- `VmStatisticsTimeFormatTests.Seconds_ExtractsFromTics`
- `VmStatisticsParserTests.ParseTimeToTics_ConvertsHms`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 242 --to 242` (0 hard / 0 soft).

## Seams

Native engine globals and VM dispatch are not re-hosted; only credited control flow is modeled.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
