# Chunk 1032 — MAPINFO toggles, skyrotate, script lumps, and `MapFlagHandlers` table head from `g_mapinfo.cpp` (**1827–1976**). **Pivot:** primary **3050–3199** aborted — only **102** uncredited lines remain (**3050–3151** EOF); **3067–3114** off-tally `VmMapInfoParseMapInfoLoop`; alternate **3115–3264** past EOF (**3151**).

## Scope

MAPINFO toggles, skyrotate, script lumps, and `MapFlagHandlers` table head from `g_mapinfo.cpp` (**1827–1976**). **Pivot:** primary **3050–3199** aborted — only **102** uncredited lines remain (**3050–3151** EOF); **3067–3114** off-tally `VmMapInfoParseMapInfoLoop`; alternate **3115–3264** past EOF (**3151**).

## Native ranges

- `src/gamedata/g_mapinfo.cpp` **DEFINE_MAP_OPTION post-shadow / MapFlagHandlers head** 1827–1976 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmMapInfoMapOptionsPostShadow1032Slice.cs`
Managed LOC (tally): 94.
Conversion commit: `b457f594750eece869ac6cb375277bbcb57f381a`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| `notexturefill` optional bool (**1827–1835**) | yes |
| `lightadditivesurfaces` optional bool (**1838–1848**) | yes |
| `skyrotate` / `skyrotate2` vectors (**1851–1876**) | yes (delegate `VmMapInfoSkyRotateParser`) |
| `fs_nocheckposition` toggle (**1879–1889**) | yes |
| `edata` / `loadacs` / `intro` / `outro` (**1892–1914**) | yes (delegate `VmMapInfoMapOptionsScript`) |
| `EMIType` / `MapFlagHandlers` head (**1923–1976**) | yes (kind + keyword policy) |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **3050–3199** (primary target) | EOF **3151** + Loop **3067–3114** — deferred without padding |
| **1738–1826** | off-tally `VmMapInfoHcdeShadowMapOptions` |
| **1567–1716** | off-tally `VmMapInfoScalarOptionsBlock` |
| **1825–1826** | closing brace seam before hull **1827** |
| **1977+** | outside hull |
| Retired **329** script-flags | overlap duplicate — not re-listed |

| Managed (`VmMapInfoMapOptionsPostShadow1032Slice.cs`) | Native (`g_mapinfo.cpp`) |
|------------------------------------------------------|---------------------------|
| `ParseOptionalBoolFlag` + toggle apply | **1827–1889** |
| `TryParseSkyRotateVector` | **1851–1876** |
| `ApplyScriptLumps` | **1892–1914** |
| `ResolveHandlerKind` / `MatchesFlagHandlerKeyword` | **1923–1976** |
| Band hull (exclusive slice) | **1827–1976** |

### Chunk **1032** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1031** |


**Post-review ratio:** 94/150 managed/native ≈ 63% (src file LOC at HEAD; see tally).

## Tests

- `VmMapInfoMapOptionsPostShadow1032SliceTests.ParseOptionalBoolFlag_MatchesNativeToggleShape`
- `VmMapInfoMapOptionsPostShadow1032SliceTests.TryParseSkyRotateVector_Normalizes`
- `VmMapInfoMapOptionsPostShadow1032SliceTests.ApplyScriptLumps_DelegatesToScriptSlice`
- `VmMapInfoMapOptionsPostShadow1032SliceTests.ResolveHandlerKind_MapsNativeMitTypes`
- `VmMapInfoMapOptionsPostShadow1032SliceTests.ResolveSetFlagMask_ReturnsDataForSetFlag`
- `VmMapInfoMapOptionsPostShadow1032SliceTests.ShouldIgnoreFlagHandler_ForMitIgnore`
- `VmMapInfoMapOptionsPostShadow1032SliceTests.MatchesFlagHandlerKeyword_IsCaseInsensitive`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1032 --to 1032` (0 hard / 0 soft).

## Seams

`DEFINE_MAP_OPTION` macro expansion, `parse.ParseCutscene`, `FMapInfoParser` format commas, and native `MapFlagHandlers[]` storage remain host-injected.

## Credit rationale

UNCredited **`g_mapinfo.cpp`** **1827–1976** with zero conversion overlap; avoids **1341–1640** deferred bands and tail Loop collision.
