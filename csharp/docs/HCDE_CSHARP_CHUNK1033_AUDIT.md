# Chunk 1033 — `MapFlagHandlers[]` continuation and `ParseMapDefinition` flag-dispatch head from `g_mapinfo.cpp` (**1977–2126**). Follows chunk **1032** (**1827–1976**).

## Scope

`MapFlagHandlers[]` continuation and `ParseMapDefinition` flag-dispatch head from `g_mapinfo.cpp` (**1977–2126**). Follows chunk **1032** (**1827–1976**).

## Native ranges

- `src/gamedata/g_mapinfo.cpp` **MapFlagHandlers table / ParseMapDefinition head** 1977–2126 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmMapInfoMapFlagHandlers1033Slice.cs`
Managed LOC (tally): 89.
Conversion commit: `ea1e65d2772806a70ffb4b1ea92aefb27baa74c0`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Freelook / jump toggles (**1976–1979**) | yes |
| Falling-damage `MITYPE_SCFLAGS` variants (**1980–1984**) | yes |
| Infighting / teamplay `SCFLAGS2` (**2007–2014**) | yes |
| Shadow map on/off pair (**2035–2036**) | yes |
| `compat_passover` / `passover` (**2045–2046**) | yes |
| Handler table terminator (**2105–2106**) | yes |
| `ParseMapDefinition` match + `MITYPE_EATNEXT` switch head (**2121–2126**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **1827–1976** | chunk **1032** |
| **2055–2106** | off-tally `VmMapInfoFlagHandlersCompat` (Vm* line-range collision) |
| **2044**, **2069** | `MITYPE_IGNORE` legacy rows (policy only; no extra credit) |
| **2107–2120** | comment banners / function prologue host |
| **2127+** | outside hull (**2126** ends at `switch`) |
| **3050–3151** | deferred tail (**102** lines) |
| Retired **329** / catalog `VmMapInfoFlagHandlers` | off-tally duplicate keywords — seam tests only |

| Managed (`VmMapInfoMapFlagHandlers1033Slice.cs`) | Native (`g_mapinfo.cpp`) |
|--------------------------------------------------|---------------------------|
| `ApplyFreelookScFlags` / `ApplyJumpToggle` | **1976–1979** |
| Falling-damage helpers | **1980–1984** |
| Infighting / teamplay helpers | **2007–2014** |
| Shadow map toggles | **2035–2036** |
| `ApplyCompatPassover` | **2045–2046** |
| `IsHandlersTableTerminator` | **2105–2106** |
| `ShouldDispatchFlagHandler` / `RequiresEatNextAssignment` / `ShouldIgnoreLegacyCompatRow` | **2123–2126**, **2044**, **2069** |
| Band hull (exclusive slice) | **1977–2126** |

### Chunk **1033** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1032** |


**Post-review ratio:** 89/150 managed/native ≈ 59% (src file LOC at HEAD; see tally).

## Tests

- `VmMapInfoMapFlagHandlers1033SliceTests.ApplyFreelookScFlags_TogglesMutually`
- `VmMapInfoMapFlagHandlers1033SliceTests.ApplyFallingDamageModes_ExclusiveHxAndZd`
- `VmMapInfoMapFlagHandlers1033SliceTests.ApplyInfightingNormal_ClearsBothBits`
- `VmMapInfoMapFlagHandlers1033SliceTests.ApplyTeamplayOn_SetsOnMask`
- `VmMapInfoMapFlagHandlers1033SliceTests.ApplyShadowMapToggle_FlipNoShadowBit`
- `VmMapInfoMapFlagHandlers1033SliceTests.ShouldDispatchFlagHandler_RequiresNonNegativeIndex`
- `VmMapInfoMapFlagHandlers1033SliceTests.TryResolveCompatHandlerSeam_DelegatesOffTallyCatalog`
- `VmMapInfoMapFlagHandlers1033SliceTests.RequiresEatNextAssignment_OnlyForEatNextType`
- `VmMapInfoMapFlagHandlers1033SliceTests.ShouldIgnoreLegacyCompatRow_ForNobotnodesAndCorpseGibs`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1033 --to 1033` (0 hard / 0 soft).

## Seams

`sc.MatchString`, `ParseAssign`, `ParseOpenBrace`, and native `MapFlagHandlers[]` storage remain host-injected; compat catalog tail delegates to off-tally `VmMapInfoFlagHandlersCompat`.

## Credit rationale

UNCredited **`g_mapinfo.cpp`** **1977–2126** with zero conversion overlap; excludes off-tally compat line band **2055–2106** from managed port map (no duplicate credit).
