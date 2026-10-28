# Chunk 1031 — CMPGNINF apply tail and `G_ParseMapInfo` COMPLVL / basemapinfo / lump-scan head from `g_mapinfo.cpp` (**2900–3049**). Follows chunk **1030** (**2750–2899**). Hull stops before off-tally `VmMapInfoParseMapInfoLoop` (**3067–3114**).

## Scope

CMPGNINF apply tail and `G_ParseMapInfo` COMPLVL / basemapinfo / lump-scan head from `g_mapinfo.cpp` (**2900–3049**). Follows chunk **1030** (**2750–2899**). Hull stops before off-tally `VmMapInfoParseMapInfoLoop` (**3067–3114**).

## Native ranges

- `src/gamedata/g_mapinfo.cpp` **HCDE_ParseCMPGNINF apply / G_ParseMapInfo head** 2900–3049 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmMapInfoParseMapInfo1031Slice.cs`
Managed LOC (tally): 100.
Conversion commit: `d0eef60cb809c26f3fd64b89ff0068a86d6270a8`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| CMPGNINF `mapname` assign + limit properties (**2900–2917**) | yes (`TryApplyCmpgninfBlockProperty` + `VmCmpgninfParser`) |
| CMPGNINF block skip + limit write (**2920–2940**) | yes |
| `G_ParseMapInfo` Doom COMPLVL gate (**2961–2964**) | yes (predicate) |
| COMPLVL preset bytes (**2969–3006**) | yes (delegate `VmMapInfoComplvlParser.TryParseComplvl`) |
| Basemapinfo core override fatal (**3016–3019**) | yes |
| `gamedefaults` compat OR merge (**3023–3026**) | yes |
| MAPINFO/ZMAPINFO/UMAPINFO skip head (**3034–3051**) | yes (delegate `VmMapInfoLumpPrecedence`) |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **2750–2899** | chunk **1030** |
| **2923–2931** | `FindLevelInfo` / `Reserve` / `Printf` host |
| **2963–2968** | filesystem read host |
| **3011–3021** | `ParseMapInfo` nested parse host |
| **3032–3033** | `FindLumpMulti` host |
| **3053–3066** | EMAPINFO skip tail (**3050+** outside **3049** hull) |
| **3067–3114** | off-tally `VmMapInfoParseMapInfoLoop` — no duplicate credit |
| Retired **306**/**308**/**318** | prior sub-150 rows; off-tally Vm helpers only |

| Managed (`VmMapInfoParseMapInfo1031Slice.cs`) | Native (`g_mapinfo.cpp`) |
|--------------------------------------------------|---------------------------|
| `TryApplyCmpgninfBlockProperty` | **2902–2917** |
| `ShouldSkipCmpgninfBlock` / `ApplyCmpgninfLimits` / `NeedsSyntheticLevelForCmpgninf` | **2920–2940** |
| `ShouldReadComplvlPresets` / `TryLoadComplvlPresets` | **2961–3006** |
| `MergeComplvlIntoDefaults` | **3023–3026** |
| `ShouldFatalOnBasemapinfoAddonOverride` | **3016–3019** |
| `ShouldSkipNamedMapinfoLump` + probe helpers | **3034–3051** |
| Band hull (exclusive slice) | **2900–3049** |

### Chunk **1031** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1030** |


**Post-review ratio:** 100/150 managed/native ≈ 67% (src file LOC at HEAD; see tally).

## Tests

- `VmMapInfoParseMapInfo1031SliceTests.ShouldSkipCmpgninfBlock_WhenMapEmptyOrNoLimits`
- `VmMapInfoParseMapInfo1031SliceTests.ApplyCmpgninfLimits_WritesPresentLimitsOnly`
- `VmMapInfoParseMapInfo1031SliceTests.MergeComplvlIntoDefaults_OrMasksPresets`
- `VmMapInfoParseMapInfo1031SliceTests.TryLoadComplvlPresets_DelegatesToExistingParser`
- `VmMapInfoParseMapInfo1031SliceTests.ShouldFatalOnBasemapinfoAddonOverride_WhenContainerPositive`
- `VmMapInfoParseMapInfo1031SliceTests.ShouldSkipNamedMapinfoLump_SkipsMapinfoWhenZmapinfoInWad`
- `VmMapInfoParseMapInfo1031SliceTests.ShouldProbeZmapinfoWhenClassicMapinfo_OnlyForIndexZero`
- `VmMapInfoParseMapInfo1031SliceTests.TryApplyCmpgninfBlockProperty_ClampsWaveLimit`
- `VmMapInfoParseMapInfo1031SliceTests.NeedsSyntheticLevelForCmpgninf_WhenMissing`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1031 --to 1031` (0 hard / 0 soft).

## Seams

`FindLevelInfo`, `wadlevelinfos.Reserve`, `fileSystem` lump I/O, `FMapInfoParser::ParseMapInfo`, and `I_FatalError` formatting remain native.

## Credit rationale

UNCredited **`g_mapinfo.cpp`** band **2900–3049** with zero conversion/recognition overlap; lump loop body **3050+** and `VmMapInfoParseMapInfoLoop` **3067–3114** outside hull.
