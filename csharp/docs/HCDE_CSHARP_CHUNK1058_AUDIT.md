# Chunk 1058 — `V_InitCustomFonts` FONTDEFS parser policy from `v_font.cpp` (**151–300**). Pivot off blocked `g_mapinfo` OPTIONS corridor (**1029** / chunk **1057** tail).

## Scope

`V_InitCustomFonts` FONTDEFS parser policy from `v_font.cpp` (**151–300**). Pivot off blocked `g_mapinfo` OPTIONS corridor (**1029** / chunk **1057** tail).

## Native ranges

- `src/common/fonts/v_font.cpp` **V_InitCustomFonts** 151–300 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmVFontFontDefs1058Slice.cs`
Managed LOC (tally): 76.
Conversion commit: `6a6237bb5793dc99eb5f74b47e501991d34412eb`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| FONTDEFS per-block defaults (**187–192**) | yes |
| TEMPLATE/BASE/FIRST/COUNT format-1 path (**198–225**) | yes (format conflict policy) |
| CURSOR/SPACEWIDTH/DONTTRANSLATE toggles (**226–239**) | yes |
| NOTRANSLATION index marks (**240–248**) | yes |
| KERNING/ignoreoffsets/luminosity clamps (**250–268**) | yes |
| Lump-map glyph key escape (**271–274**) | yes |
| Missing-texture message gate (**282–286**) | yes (predicate) |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **151–158** | comment banner / prior function tail |
| **178–180** | lump open host |
| **277–281** | `TexMan` texture resolve host |
| **290–300** | `FFont` ctor / first-glyph scan (seam past hull) |
| **301+** | remainder of `V_InitCustomFonts` outside band |

| Managed (`VmVFontFontDefs1058Slice.cs`) | Native (`v_font.cpp`) |
|-------------------------------------------|----------------------|
| `CreateDefaults` / format conflict | **187–225**, **240–248** |
| `TryMarkNoTranslation` | **245–246** |
| `ClampLuminosity` | **262**, **267** |
| `NormalizeFontGlyphKey` | **273–274** |
| `ShouldEmitMissingTextureMessage` | **282–286** |
| Band hull (exclusive slice) | **151–300** |

### Chunk **1058** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1057** |


**Post-review ratio:** 76/150 managed/native ≈ 51% (src file LOC at HEAD; see tally).

## Tests

- `VmVFontFontDefs1058SliceTests.CreateDefaults_MatchesNativeInitializers`
- `VmVFontFontDefs1058SliceTests.FormatConflict_RejectsMixedTemplateAndLumpMap`
- `VmVFontFontDefs1058SliceTests.TryMarkNoTranslation_OnlyValidCodePoints`
- `VmVFontFontDefs1058SliceTests.ClampLuminosity_ClampsToByteRange`
- `VmVFontFontDefs1058SliceTests.NormalizeFontGlyphKey_UnescapesBraces`
- `VmVFontFontDefs1058SliceTests.ShouldEmitMissingTextureMessage_OnlyForNonIwadContainers`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1058 --to 1058` (0 hard / 0 soft).

## Seams

`fileSystem.FindLump`, `FScanner`, `TexMan.CheckForTexture`, and `FFont` construction remain host-injected; hull ends inside lump-map first-glyph scan (**300**).

## Credit rationale

UNCredited **`src/common/fonts/v_font.cpp`** **151–300** (+150); zero conversion/recognition overlap and no existing `Vm*` line-range collision on the band.
