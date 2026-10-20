# Chunk 241 — Statistics structs and ParseStatistics.

## Scope

Statistics structs and ParseStatistics.

## Native ranges

- `src/gamedata/statistics.cpp` **statistics header and CVARs** 1–52 (52 lines)
- `src/gamedata/statistics.cpp` **OneLevel and FLevelStatistics structs** 59–98 (40 lines)
- `src/gamedata/statistics.cpp` **ParseStatistics** 108–167 (60 lines)
- `src/gamedata/statistics.cpp` **ReadStatistics** 176–179 (4 lines)

**Credited native LOC:** 156 (tally `nativeLoc` 156).

## Managed

`VmActorKeyCheck.cs`, `VmActorKeyCheckTests.cs`, `VmDecalLibCore.cs`, `VmGameInfoParseGameInfoTests.cs`, `VmStatisticsModels.cs`, `VmStatisticsModelsTests.cs`, `VmStatisticsParser.cs`, `VmStatisticsParserTests.cs`, `VmStatisticsTimeFormat.cs`, `VmStatisticsTimeFormatTests.cs`
Managed LOC (tally): 110.
Conversion commit: `e0dff6e4e1d2444fa60803b02ab92e491a153096`.

## Tests

- `VmStatisticsModelsTests.StatFileDefault_MatchesNativeCvar`
- `VmStatisticsParserTests.ParseTimeToTics_ConvertsHms`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 241 --to 241` (0 hard / 0 soft).

## Seams

Native engine globals and VM dispatch are not re-hosted; only credited control flow is modeled.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
