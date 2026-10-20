# Chunk 244 — statistics.cpp remainder through file tail.

## Scope

statistics.cpp remainder through file tail.

## Native ranges

- `src/gamedata/statistics.cpp` **statistics tail** 421–619 (199 lines)

**Credited native LOC:** 199 (tally `nativeLoc` 199).

## Managed

`VmActorKeyCheck.cs`, `VmActorKeyCheckTests.cs`, `VmDecalLibCore.cs`, `VmGameInfoParseGameInfoTests.cs`, `VmStatisticsModels.cs`, `VmStatisticsModelsTests.cs`, `VmStatisticsParser.cs`, `VmStatisticsParserTests.cs`, `VmStatisticsTimeFormat.cs`, `VmStatisticsTimeFormatTests.cs`
Managed LOC (tally): 60.
Conversion commit: `e0dff6e4e1d2444fa60803b02ab92e491a153096`.

## Tests

- `VmStatisticsTimeFormatTests.Seconds_ExtractsFromTics`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 244 --to 244` (0 hard / 0 soft).

## Seams

Native engine globals and VM dispatch are not re-hosted; only credited control flow is modeled.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
