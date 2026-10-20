# Chunk 264 — MAPINFO level scalar options after line 1560 (teamdamage, fog, lightmode).

## Scope

MAPINFO level scalar options after line 1560 (teamdamage, fog, lightmode).

## Native ranges

- `src/gamedata/g_mapinfo.cpp` **DEFINE_MAP_OPTION teamdamage** 1561–1566 (6 lines)
- `src/gamedata/g_mapinfo.cpp` **DEFINE_MAP_OPTION fogdensity** 1654–1659 (6 lines)
- `src/gamedata/g_mapinfo.cpp` **DEFINE_MAP_OPTION outsidefogdensity** 1661–1666 (6 lines)
- `src/gamedata/g_mapinfo.cpp` **DEFINE_MAP_OPTION skyfog** 1668–1673 (6 lines)
- `src/gamedata/g_mapinfo.cpp` **DEFINE_MAP_OPTION thickfogdistance** 1675–1680 (6 lines)
- `src/gamedata/g_mapinfo.cpp` **DEFINE_MAP_OPTION thickfogmultiplier** 1682–1689 (8 lines)
- `src/gamedata/g_mapinfo.cpp` **DEFINE_MAP_OPTION pixelratio** 1691–1696 (6 lines)
- `src/gamedata/g_mapinfo.cpp` **DEFINE_MAP_OPTION brightfog** 1699–1704 (6 lines)
- `src/gamedata/g_mapinfo.cpp` **DEFINE_MAP_OPTION lightmode** 1706–1720 (15 lines)

**Credited native LOC:** 65 (tally `nativeLoc` 65).

## Managed

`VmMapInfoLevelScalars.cs`, `VmMapInfoLevelScalarsTests.cs`
Managed LOC (tally): 72.
Conversion commit: `af3be89fa7f9f7edfeccc6566bdd146332a0e601`.

## Tests

- `VmMapInfoLevelScalarsTests.ApplyTeamDamage_StoresValue`
- `VmMapInfoLevelScalarsTests.ParseFogDensity_ClampsAndHalves`
- `VmMapInfoLevelScalarsTests.ParseThickFogMultiplier_IgnoresNonPositive`
- `VmMapInfoLevelScalarsTests.ParseLightMode_TreatsEightAsNotSet`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 264 --to 264` (0 hard / 0 soft).

## Seams

MAPINFO handlers omit live `FMapInfoParser` scanner state; managed code models option semantics only.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
