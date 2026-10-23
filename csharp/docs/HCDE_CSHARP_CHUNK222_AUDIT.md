# Chunk 222 — UMAPINFO schema remainder, InitSpawnablesFromMapinfo and class-map helpers.

## Scope

UMAPINFO schema remainder, InitSpawnablesFromMapinfo and class-map helpers.

## Native ranges

- `src/gamedata/umapinfo.cpp` **UMapEntry layout and helpers** 29–83 (55 lines)
- `src/gamedata/umapinfo.cpp` **ParseMultiString clear branch** 112–118 (7 lines)
- `src/gamedata/umapinfo.cpp` **ParseLumpName validation** 130–136 (7 lines)
- `src/gamedata/umapinfo.cpp` **ParseStandardProperty header** 156–164 (9 lines)
- `src/gamedata/umapinfo.cpp` **ParseStandardProperty nextsecret sky** 223–265 (43 lines)
- `src/gamedata/g_doomedmap.cpp` **InitSpawnablesFromMapinfo and dump** 1–32 (32 lines)
- `src/gamedata/g_doomedmap.cpp` **Spawnable class map** 61–97 (37 lines)
- `src/gamedata/g_doomedmap.cpp` **CCMD spawnables** 306–360 (55 lines)
- `src/gamedata/g_doomedmap.cpp` **Strife type clear** 406–419 (14 lines)
- `src/gamedata/g_doomedmap.cpp` **Spawn CCMD tail** 452–457 (6 lines)

**Credited native LOC:** 265 (tally `nativeLoc` 265).

## Managed

`VmDoomEdSpawnInit.cs`, `VmDoomEdSpawnInitTests.cs`, `VmUMapInfoSchema.cs`, `VmUMapInfoSchemaTests.cs`, `VmUMapInfoTokenParserTests.cs`
Managed LOC (tally): 85.
Conversion commit: `868d46235f564f4c230307d0847b2217d7d6fbdf`.

## Tests

- `VmUMapInfoSchemaTests.LumpNameFields_MatchesNativeLayout`
- `VmUMapInfoTokenParserTests.TryParseMultiString_Clear`
- `VmUMapInfoTokenParserTests.TryParseLumpName_RejectsLongName`
- `VmUMapInfoStandardPropertyParserTests.TryApplyProperty_SetsLevelName`
- `VmDoomEdSpawnInitTests.InitSpawnablesFromMapinfo_ResolvesClasses`
- `VmDoomEdSpawnInitTests.SortClassMapForDump_OrdersByKey`
- `VmDoomEdSpawnInitTests.ClearStrifeTypes_EmptiesMap`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 222 --to 222` (0 hard / 0 soft).

## Seams

Native engine globals and VM dispatch are not re-hosted; only credited control flow is modeled.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
