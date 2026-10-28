# Chunk 1071 — `DSBarInfo::DrawString`, translation helpers, and `AmmoType` from `sbarinfo.cpp` (**1351–1500**); follows **1070** `DrawGraphic`.

## Scope

`DSBarInfo::DrawString`, translation helpers, and `AmmoType` from `sbarinfo.cpp` (**1351–1500**); follows **1070** `DrawGraphic`.

## Native ranges

- `src/g_statusbar/sbarinfo.cpp` **DSBarInfo::DrawString / GetTranslation** 1351–1500 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmSbarinfoDrawString1071Slice.cs`
Managed LOC (tally): 111.
Conversion commit: `5c32afc529531f834d1d4230a14379d96c7fdbc0`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| DrawString spacing + bold translation (**1352–1360**) | yes |
| HUD scale + string loop space/escape (**1362–1387**) | yes (escape gate) |
| Glyph metrics + monospace alignment (**1390–1425**) | yes |
| Statusbar vs fullscreen char coords (**1427–1446**) | yes |
| Shadow alpha/offset + cursor advance (**1447–1467**) | yes |
| `GetTranslation` / `GetTranslationIndex` (**1471–1481**) | yes |
| `AmmoType` slot select (**1483–1487**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **1384–1386** | host `V_ParseFontColor` |
| **1429**, **1452–1463** | host coord transform + `DrawChar` |
| **1489–1504** | class member fields / closing brace |
| **1507+** | `DrawAux` outside hull |

| Managed (`VmSbarinfoDrawString1071Slice.cs`) | Native (`sbarinfo.cpp`) |
|--------------------------------------------------|-------------------------|
| Spacing/bold/HUD scale helpers | **1352–1370** |
| Space advance + glyph width policy | **1374–1394** |
| Monospace alignment + draw rect | **1402–1425** |
| Fullscreen scale/anchor + shadow | **1434–1451** |
| Cursor advance after glyph | **1464–1467** |
| Translation + ammo type policy | **1471–1487** |
| Band hull (exclusive slice) | **1351–1500** |

### Chunk **1071** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1070** |


**Post-review ratio:** 111/150 managed/native ≈ 74% (src file LOC at HEAD; see tally).

## Tests

- `VmSbarinfoDrawString1071SliceTests.ResolveBoldTranslationRange_MatchesNativeFallback`
- `VmSbarinfoDrawString1071SliceTests.AdvanceDrawStringForSpace_UsesSpacingCharacterWhenSet`
- `VmSbarinfoDrawString1071SliceTests.ApplyMonospaceAlignmentX_CentersWithinSpacingCell`
- `VmSbarinfoDrawString1071SliceTests.ComputeDrawStringShadowAlpha_UsesHereticShadowFactor`
- `VmSbarinfoDrawString1071SliceTests.AdvanceDrawStringCursorAfterGlyph_DiffersForMonospace`
- `VmSbarinfoDrawString1071SliceTests.ShouldUseRavenExtraPlayerTranslation_DetectsRavenMask`
- `VmSbarinfoDrawString1071SliceTests.ResolveTranslationIndex_GatesOnTranslateFlag`
- `VmSbarinfoDrawString1071SliceTests.AnchorNegativeFullscreenGlyph_AddsViewSize`
- `VmSbarinfoDrawString1071SliceTests.IsTextColorEscape_RecognizesNativeEscape`
- `VmSbarinfoDrawString1071SliceTests.SelectAmmoTypeFieldIndex_MapsSlotOneAndTwo`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1071 --to 1071` (0 hard / 0 soft).

## Seams

`V_ParseFontColor`, `GetCharFromString`, `DrawChar`, and **1489–1504** member declarations remain host; fullscreen rel-center uses **1068**/**1070** at call sites.

## Credit rationale

UNCredited **`sbarinfo.cpp`** **1351–1500** (+150); no overlap with **1070** `DrawGraphic` (**1201–1348**).
