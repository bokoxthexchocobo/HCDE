# Chunk 1202 — Theme fallback colors, theme.txt commands, and layer mix from themedata.cpp (1–166).

## Scope

Theme fallback colors, theme.txt commands, and layer mix from themedata.cpp (1–166).

## Native ranges

- `src/widgets/themedata.cpp` **Theme::initilize** 1–166 (166 lines)

**Credited native LOC:** 166 (tally `nativeLoc` 166).

## Managed

`VmThemeData1202Slice.cs`
Managed LOC (tally): 237.
Conversion commit: `8e48dc3c53036ecc9fadb174d539abc487843db9`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Theme storage and command names (**25–55**) | yes |
| Theme::initilize (**57–149**) | yes |
| Theme::mix (**151–157**) | yes |
| Theme getters (**159–166**) | yes |
| Band hull (**1–166**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| none | the band is the whole file |

initilize returns immediately once a theme is selected. Otherwise it selects the mode, paints the light and dark fallbacks, and sets the accent to 0x7f7f7f. It then reads the pk3 text and the mod text. A three-digit hex color is expanded to six digits. Any other length except six becomes -1, which fromRgb turns into opaque white. Commands match without case. An accent updates even in contrast mode. Layer pairs are skipped while contrast is on, and also while no section has been selected. Those skipped colors stay in the token stream. Theme::mix returns the background at or below 0 and the foreground at or above 1, with alpha 1. The getters read the active theme.

| Managed (`VmThemeData1202Slice.cs`) | Native (`themedata.cpp`) |
|-------------------------------------|--------------------------|
| Theme data | **1–166** |
| Band hull (exclusive slice) | **1–166** |

### Chunk **1202** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **166** |
| **Net headline** | **+166** vs post-**1201** |


**Post-review ratio:** 237/166 managed/native ≈ 143% (src file LOC at HEAD; see tally).

## Tests

- `VmThemeData1202SliceTests.Initialize_PaintsFallbacksThenAppliesThemeText`
- `VmThemeData1202SliceTests.Contrast_AndMix_KeepPairsAndBlendEndpoints`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1202 --to 1202` (0 hard / 0 soft).

## Seams

The pk3 text and the mod text stand in for the two LoadWidgetData calls. GetString follows the Hexen scanner: whitespace, // comments, semicolon comments, block comments, and #region lines are skipped. MatchString is ordinal case-insensitive. fromRgb is the ZWidget pack of the 24-bit value into opaque RGBA. Color::mix endpoints are preserved. A blend strictly between 0 and 1 is linear sRGB because the OKLCH conversion lives in colorspace.cpp.

## Credit rationale

UNCredited themedata.cpp 1–166 (+166). The file ends at 166. A full tally line scan shows no overlap.
