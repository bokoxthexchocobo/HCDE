# Chunk 271 — MAPINFO lightblend, HCDE shadow, skyrotate, toggles, edname/acs. (re-audit 265-286: tested ranges only)

## Scope

MAPINFO lightblend, HCDE shadow, skyrotate, toggles, edname/acs. (re-audit 265-286: tested ranges only)

## Native ranges

- `src/gamedata/g_mapinfo.cpp` **lightblendmode** 1722–1771 (50 lines)
- `src/gamedata/g_mapinfo.cpp` **hcde_shadowprofile** 1773–1783 (11 lines)
- `src/gamedata/g_mapinfo.cpp` **hcde_shadowqualitycap** 1785–1801 (17 lines)
- `src/gamedata/g_mapinfo.cpp` **hcde_shadowmaxlightscap** 1803–1823 (21 lines)
- `src/gamedata/g_mapinfo.cpp` **notexturefill** 1825–1836 (12 lines)
- `src/gamedata/g_mapinfo.cpp` **lightadditivesurfaces** 1838–1849 (12 lines)
- `src/gamedata/g_mapinfo.cpp` **skyrotate** 1851–1863 (13 lines)
- `src/gamedata/g_mapinfo.cpp` **skyrotate2** 1865–1877 (13 lines)
- `src/gamedata/g_mapinfo.cpp` **fs_nocheckposition** 1879–1890 (12 lines)
- `src/gamedata/g_mapinfo.cpp` **edata** 1892–1897 (6 lines)
- `src/gamedata/g_mapinfo.cpp` **loadacs** 1899–1904 (6 lines)

**Credited native LOC:** 173 (tally `nativeLoc` 53).

## Managed

`VmMapInfoHcdeShadowOptions.cs`, `VmMapInfoHcdeShadowOptionsTests.cs`, `VmMapInfoLevelScalars.cs`, `VmMapInfoLightBlendTonemap.cs`, `VmMapInfoLightBlendTonemapTests.cs`, `VmMapInfoSkyRotateParser.cs`, `VmMapInfoSkyRotateParserTests.cs`, `VmMapInfoToggleOptions.cs`, `VmMapInfoToggleOptionsTests.cs`
Managed LOC (tally): 268.
Conversion commit: `35824186f6e906f477df7cba1eb67aa9a22bc189`.

## Tests

- `VmMapInfoLightBlendTonemapTests.TryParseLightBlendMode_Unclamped`
- `VmMapInfoHcdeShadowOptionsTests.ClampShadowProfile_Bounds`
- `VmMapInfoHcdeShadowOptionsTests.NormalizeShadowQualityCap_PicksNearestTier`
- `VmMapInfoToggleOptionsTests.ParseOptionalBool_FlagWhenOmitted`
- `VmMapInfoSkyRotateParserTests.TryParseVector_Normalizes`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 271 --to 271` (0 hard / 0 soft).

## Seams

MAPINFO handlers omit live `FMapInfoParser` scanner state; managed code models option semantics only.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
