# Chunk 1072 — `V_GetFont` lookup policy and `DEFAULT_LOG_COLOR` from `v_font.cpp` (**1–150**); precedes chunk **1058** FONTDEFS (**151–300**).

## Scope

`V_GetFont` lookup policy and `DEFAULT_LOG_COLOR` from `v_font.cpp` (**1–150**); precedes chunk **1058** FONTDEFS (**151–300**).

## Native ranges

- `src/common/fonts/v_font.cpp` **V_GetFont** 1–150 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmVFontGetFont1072Slice.cs`
Managed LOC (tally): 83.
Conversion commit: `724d129960bf6cb00c5e7c98d332853c89330482`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| `DEFAULT_LOG_COLOR` macro (**50**) | yes |
| `V_GetFont` null/alias names (**85–88**) | yes |
| BIGUPPER → BIGFONT fallback gate (**92–96**) | yes |
| Folder vs lump container ordering (**105–114**) | yes |
| Single-lump FON header detect (**116–122**) | yes |
| Translation load gate (**126–127**, **144–145**) | yes |
| Texture pic-font path (**130–139**) | yes |
| Folder font instantiate gate (**141–146**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **1–49** | license banner / includes / extern decls |
| **68–73** | global pointer defs |
| **89–90**, **94–95** | host `FindFont` |
| **117–125**, **136–137** | host lump/texture create |
| **143** | host `new FFont` |
| **151+** | `V_InitCustomFonts` (**1058** seam) |

| Managed (`VmVFontGetFont1072Slice.cs`) | Native (`v_font.cpp`) |
|------------------------------------------|----------------------|
| `DefaultLogPaletteEntry` | **50** |
| `ResolveCanonicalFontLookupName` | **86–88** |
| `ShouldTryBigUpperFallback` | **92–96** |
| Folder/lump ordering predicates | **105–114** |
| `IsRecognizedSingleLumpFontHeader` | **121–122** |
| Pic/folder/translation gates | **126–145** |
| Band hull (exclusive slice) | **1–150** |

### Chunk **1072** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1071** |


**Post-review ratio:** 83/150 managed/native ≈ 55% (src file LOC at HEAD; see tally).

## Tests

- `VmVFontGetFont1072SliceTests.BuildFolderFontPath_MatchesNativePattern`
- `VmVFontGetFont1072SliceTests.ResolveFontLumpLookupName_FallsBackToCanonicalName`
- `VmVFontGetFont1072SliceTests.ResolveCanonicalFontLookupName_MapsLegacyAliases`
- `VmVFontGetFont1072SliceTests.ShouldPreferSingleLumpOverFolder_RequiresContainerOrdering`
- `VmVFontGetFont1072SliceTests.IsRecognizedSingleLumpFontHeader_AcceptsFonMagic`
- `VmVFontGetFont1072SliceTests.ShouldCreatePicFontFromTexture_RequiresSourceLumpOrdering`
- `VmVFontGetFont1072SliceTests.ShouldInstantiateFolderFont_WhenEntriesExist`
- `VmVFontGetFont1072SliceTests.ResolveFolderContainerWhenPresent_ReturnsSentinelWhenMissing`
- `VmVFontGetFont1072SliceTests.DefaultLogPaletteEntry_MatchesMacro`
- `VmVFontGetFont1072SliceTests.ShouldTryBigUpperFallback_OnlyForAlias`
- `VmVFontGetFont1072SliceTests.ShouldAbortFontLookup_WhenNameNull`
- `VmVFontGetFont1072SliceTests.ShouldLoadTranslationsAfterFontCreate_WhenAlreadyLoaded`
- `VmVFontGetFont1072SliceTests.ClassifyLegacyFontAlias_RecognizesConfont`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1072 --to 1072` (0 hard / 0 soft).

## Seams

`FFont::FindFont`, lump readers, `CreateSingleLumpFont` / `CreateSinglePicFont`, and folder `FFont` ctor remain host-injected; **1–49** license/includes are zero-behavior.

## Credit rationale

UNCredited **`src/common/fonts/v_font.cpp`** **1–150** (+150); no overlap with **1058–1062** mid-file bands; **901–957** still deferred.
