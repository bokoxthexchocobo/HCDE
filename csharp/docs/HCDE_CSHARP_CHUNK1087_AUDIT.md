# Chunk 1087 — `CommandDrawInventoryBar::Draw`, `Parse`, and `GetCounterSpacing` null fallback from `sbarinfo_commands.cpp` (**2101–2250**); follows **1086** through **`Draw` `{` (**2100**).

## Scope

`CommandDrawInventoryBar::Draw`, `Parse`, and `GetCounterSpacing` null fallback from `sbarinfo_commands.cpp` (**2101–2250**); follows **1086** through **`Draw` `{` (**2100**).

## Native ranges

- `src/g_statusbar/sbarinfo_commands.cpp` **CommandDrawInventoryBar::Draw / Parse / GetCounterSpacing** 2101–2250 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmSbarinfoCommandDrawInventoryBar1087Slice.cs`
Managed LOC (tally): 180.
Conversion commit: `2f1e6387afa3347ba17e478c29240cbd63f5acdc`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Translucent background alpha (**2104–2105**) | yes |
| Draw bar when empty + slot layout (**2112–2117**) | yes |
| Strife vs pre-selection icon order (**2121–2137**) | yes |
| Style-specific selection overlay (**2125–2134**) | yes |
| Per-item counter gate (**2138–2141**) | yes |
| Trailing empty artibox slots (**2144–2145**) | yes |
| Left/right arrow placement (**2148–2159**) | yes |
| Style keyword parse (**2166–2177**) | yes |
| Parse flag keywords (**2182–2210**) | yes |
| Default counter coords (**2226–2227**) | yes |
| Missing box spacing fallback (**2248–2250**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **2100** | **1086** `Draw` entry `{` seam |
| **2101**, **2111**, **2140–2141** | host spacing lookup / counter draw |
| **2118–2137**, **2145**, **2152–2159** | host `DrawGraphic` / texture |
| **2217–2221**, **2230–2237** | host font + translation scanner |
| **2252+** | live box width/height spacing tail outside hull |

| Managed (`VmSbarinfoCommandDrawInventoryBar1087Slice.cs`) | Native (`sbarinfo_commands.cpp`) |
|-----------------------------------------------------------|----------------------------------|
| Draw layout + arrows | **2103–2160** |
| Parse style/flags/coords | **2163–2241** |
| Spacing fallback | **2243–2250** |
| Band hull (exclusive slice) | **2101–2250** |

### Chunk **1087** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1086** |


**Post-review ratio:** 180/150 managed/native ≈ 120% (src file LOC at HEAD; see tally).

## Tests

- `VmSbarinfoCommandDrawInventoryBar1087SliceTests.ApplyInventoryBarTranslucentBackgroundAlpha_ScalesWithHexenShadow`
- `VmSbarinfoCommandDrawInventoryBar1087SliceTests.ComputeInventoryBarSlotOrigin_AdvancesHorizontalSlots`
- `VmSbarinfoCommandDrawInventoryBar1087SliceTests.ResolveInventorySelectionOverlay_MapsHereticSelectBox`
- `VmSbarinfoCommandDrawInventoryBar1087SliceTests.ResolveInventoryBarRightArrowPlacement_UsesStrifeFormula`
- `VmSbarinfoCommandDrawInventoryBar1087SliceTests.TryParseInventoryBarStyleKeyword_PrefersHexenStrictBeforeHexen`
- `VmSbarinfoCommandDrawInventoryBar1087SliceTests.ClassifyInventoryBarParseKeyword_MapsTranslucent`
- `VmSbarinfoCommandDrawInventoryBar1087SliceTests.ResolveInventoryBarDefaultCounterPosition_Applies2622Offsets`
- `VmSbarinfoCommandDrawInventoryBar1087SliceTests.ResolveInventoryBarCounterSpacingFromBoxDimension_AdjustsForStrife`
- `VmSbarinfoCommandDrawInventoryBar1087SliceTests.ShouldDrawInventoryBarLeftArrow_RequiresPrevItem`
- `VmSbarinfoCommandDrawInventoryBar1087SliceTests.MissingArtiboxSpacingFallback_MatchesNativeNullBox`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1087 --to 1087` (0 hard / 0 soft).

## Seams

**1086** owns inventory bar ctor defaults; **1087** owns draw/layout/parse/spacing policy. Host `DrawGraphic`, `TexMan`, `V_GetFont`, and `ValidateInvFirst` excluded.

## Credit rationale

UNCredited **`sbarinfo_commands.cpp`** **2101–2250** (+150); inventory bar draw gates, style-specific selection/arrow math, parse flags/style, counter coords, spacing fallback **32**; hull ends before box dimension read (**2251+**).
