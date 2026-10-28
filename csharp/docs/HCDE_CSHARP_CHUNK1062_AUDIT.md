# Chunk 1062 — `CalcDefaultTranslation` tail, `V_LogColorFromColorRange`, `V_ParseFontColor`, and `V_InitFonts` / `V_LoadTranslations` head from `v_font.cpp` (**751–900**); follows chunk **1061** (**601–750**). Remaining **901–957** (~57) deferred without padding.

## Scope

`CalcDefaultTranslation` tail, `V_LogColorFromColorRange`, `V_ParseFontColor`, and `V_InitFonts` / `V_LoadTranslations` head from `v_font.cpp` (**751–900**); follows chunk **1061** (**601–750**). Remaining **901–957** (~57) deferred without padding.

## Native ranges

- `src/common/fonts/v_font.cpp` **V_ParseFontColor / V_InitFonts** 751–900 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmVFontParseInit1062Slice.cs`
Managed LOC (tally): 86.
Conversion commit: `eefd5b5c85fdc4fd98b5ae8ba4ca90a5e6515727`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Gap-fill cursor advance (**760–761**) | yes |
| `V_LogColorFromColorRange` bounds (**776–780**) | yes |
| `V_ParseFontColor` escape dispatch (**798–842**) | yes |
| `V_InitFonts` hex console requirement (**863–865**) | yes (predicate) |
| `V_LoadTranslations` load gate (**876–877**) | yes |
| Big/small default translation indices (**881–900**) | yes |
| Luminosity translation index patch (**885–900**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **751–758** | RGB gap blend — **1061** `InterpolateDefaultRemapGap` seam |
| **764** | closing brace host |
| **826** | `V_FindFontColor` host |
| **856–870** | font ctor / `V_GetFont` host |
| **901–957** | deferred tail (<150) |

| Managed (`VmVFontParseInit1062Slice.cs`) | Native (`v_font.cpp`) |
|------------------------------------------|----------------------|
| `AdvanceGapFillCursor` | **760–761** |
| `ShouldUseDefaultLogColor` | **776–778** |
| `ClassifyFontColorEscape` / `ResolveStandardColorLetter` / named token | **798–842** |
| `RequiresConsoleHexFont` | **863–864** |
| `ShouldLoadFontTranslations` | **876** |
| `BigFontDefaultTranslationTableIndex` / `SmallFontDefaultTranslationTableIndex` | **881–895** |
| `PatchLuminosityTranslationIndex` | **885–900** |
| Band hull (exclusive slice) | **751–900** |

### Chunk **1062** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1061** |


**Post-review ratio:** 86/150 managed/native ≈ 57% (src file LOC at HEAD; see tally).

## Tests

- `VmVFontParseInit1062SliceTests.AdvanceGapFillCursor_MatchesNativeIncrement`
- `VmVFontParseInit1062SliceTests.ShouldUseDefaultLogColor_WhenOutOfRange`
- `VmVFontParseInit1062SliceTests.ResolveStandardColorLetter_AcceptsCase`
- `VmVFontParseInit1062SliceTests.ClassifyFontColorEscape_RecognizesNativeLeaders`
- `VmVFontParseInit1062SliceTests.TryParseNamedColorToken_ExtractsBracketName`
- `VmVFontParseInit1062SliceTests.PatchLuminosityTranslationIndex_ReplacesHighBits`
- `VmVFontParseInit1062SliceTests.RequiresConsoleHexFont_WhenLumpMissing`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1062 --to 1062` (0 hard / 0 soft).

## Seams

Gap RGB interpolation (**751–758**) delegates to **1061** `InterpolateDefaultRemapGap`; `V_FindFontColor`, `CreateHexLumpFont`, `fileSystem`, and `FFont` mutation remain host-injected.

## Credit rationale

UNCredited **`v_font.cpp`** **751–900** (+150); finishes `CalcDefaultTranslation` control tail and font color/init policy; **901–957** sub-150 deferred.
