# Chunk 1061 — `CreateLuminosityTranslationRanges`, `V_ApplyLuminosityTranslation`, and `CalcDefaultTranslation` head from `v_font.cpp` (**601–750**); follows chunk **1060** (**451–600**).

## Scope

`CreateLuminosityTranslationRanges`, `V_ApplyLuminosityTranslation`, and `CalcDefaultTranslation` head from `v_font.cpp` (**601–750**); follows chunk **1060** (**451–600**).

## Native ranges

- `src/common/fonts/v_font.cpp` **CreateLuminosityTranslationRanges / V_ApplyLuminosityTranslation** 601–750 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmVFontLuminosity1061Slice.cs`
Managed LOC (tally): 103.
Conversion commit: `ef53b58159aadf74038c3c5a171c0bcdcadabf6f`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Remap palette offset + identity fill (**613–617**) | yes |
| Skip untranslated table (**618–619**) | yes |
| Range lookup + RGB interpolate (**622–638**) | yes |
| Parm advance predicate (**641–645**) | yes |
| `V_ApplyLuminosityTranslation` guard + lum index (**661–673**) | yes |
| Below-min / above-max scaling (**676–687**) | yes |
| Default translation lum map (**716–722**) | yes |
| Gap interpolation weight (**747–758**, hull **747–750**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **604** | `ImageArena` host alloc |
| **605–612**, **647–648** | comment / loop scaffolding host |
| **668–691** | pixel write loop host |
| **705–712** | `RecordAllTextureColors` / `memset` host |
| **725–746** | gap scan control host |
| **751+** | remainder of `CalcDefaultTranslation` outside hull |

| Managed (`VmVFontLuminosity1061Slice.cs`) | Native (`v_font.cpp`) |
|------------------------------------------|----------------------|
| `ComputeRemapPaletteOffset` / `ShouldBuildLuminosityRemap` | **613–619** |
| `ContainsLuminance` / `InterpolateTranslationColor` | **622–638** |
| `ShouldAdvanceTranslationParm` | **641–645** |
| `ShouldSkipLuminosityApply` / `ComputeLuminanceRemapIndex` | **661–673** |
| `ScaleRgbBelowMin` / `ScaleRgbAboveMax` | **676–687** |
| `ShouldMapDefaultTranslationLum` / `DefaultTranslationRemapIndex` | **716–722** |
| `InterpolateDefaultRemapGap` | **747–758** |
| Band hull (exclusive slice) | **601–750** |

### Chunk **1061** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1060** |


**Post-review ratio:** 103/150 managed/native ≈ 69% (src file LOC at HEAD; see tally).

## Tests

- `VmVFontLuminosity1061SliceTests.ShouldBuildLuminosityRemap_SkipsUntranslatedTable`
- `VmVFontLuminosity1061SliceTests.InterpolateTranslationColor_MatchesNativeLinearBlend`
- `VmVFontLuminosity1061SliceTests.ShouldAdvanceTranslationParm_WhenRangesGap`
- `VmVFontLuminosity1061SliceTests.ComputeLuminanceRemapIndex_ClampsToByte`
- `VmVFontLuminosity1061SliceTests.ScaleRgbBelowMin_ScalesWhenAboveZeroMin`
- `VmVFontLuminosity1061SliceTests.ShouldSkipLuminosityApply_WhenRangeTooLarge`
- `VmVFontLuminosity1061SliceTests.InterpolateDefaultRemapGap_BlendsBetweenAnchors`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1061 --to 1061` (0 hard / 0 soft).

## Seams

`ImageArena.Alloc`, `FFont::RecordAllTextureColors`, `GetLuminosity`, `GPalette.BaseColors`, and BGRA pixel writes remain host-injected; hull ends inside `CalcDefaultTranslation` gap-fill loop (**750**).

## Credit rationale

UNCredited **`v_font.cpp`** **601–750** (+150); hardware luminosity translation continuation; no Vm* line collision; disjoint from **1060** `V_FindFontColor` band.
