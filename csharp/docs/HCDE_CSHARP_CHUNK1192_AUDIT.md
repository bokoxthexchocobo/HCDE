# Chunk 1192 — Explicit character-patch font from specialfont.cpp (1–195).

## Scope

Explicit character-patch font from specialfont.cpp (1–195).

## Native ranges

- `src/common/fonts/specialfont.cpp` **FSpecialFont::FSpecialFont** 1–195 (195 lines)

**Credited native LOC:** 195 (tally `nativeLoc` 195).

## Managed

`VmSpecialFont1192Slice.cs`
Managed LOC (tally): 559.
Conversion commit: `8a769aa21f915f2e2cbf5c61d820b24e81906951`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| FSpecialFont::FSpecialFont (**53–119**) | yes |
| FSpecialFont::LoadTranslations (**127–190**) | yes |
| CreateSpecialFont (**192–195**) | yes |
| Band hull (**1–195**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| none | the band is the whole file |

A missing patch keeps a null picture and then takes the space width, or the uppercase or unaccented advance when that character is in range. N supplies the space width when its picture exists; otherwise the space width is 4. Height is the tallest display height plus the absolute display top offset. Font characters use texture type FontChar, copy the source display size, and keep image offsets when the source sets OffsetsNotForFont. The created callback sees the source and the font character. An empty character list leaves translations empty. A font whose pictures are all null still builds luminosity ids, including the untranslated slot at 0, and then returns before the locked-color pass. With no locked colors the luminosity ids stay in place. Locked colors keep the base palette and an identity remap; every other index is the luminosity color and the nearest palette index. Duplicate palettes reuse the stored id. The untranslated slot is not a luminosity id and is left alone.

| Managed (`VmSpecialFont1192Slice.cs`) | Native (`specialfont.cpp`) |
|----------------------------------------|---------------------------|
| Character-patch font | **1–195** |
| Band hull (exclusive slice) | **1–195** |

### Chunk **1192** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **195** |
| **Net headline** | **+195** vs post-**1191** |


**Post-review ratio:** 559/195 managed/native ≈ 287% (src file LOC at HEAD; see tally).

## Tests

- `VmSpecialFont1192SliceTests.Create_CopiesPatchesAndMeasuresFromTheTallestGlyph`
- `VmSpecialFont1192SliceTests.FixXMoves_FillsMissingWidthsFromCaseAccentAndSpace`
- `VmSpecialFont1192SliceTests.LoadTranslations_RebuildsLockedLuminosityColors`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1192 --to 1192` (0 hard / 0 soft).

## Seams

Case maps, accent folding, and the nearest-color pick are host results. Luminosity ramps are host tables indexed by color range. A character list shorter than count, or a notranslate mask shorter than 256, aborts. A character without an image contributes no luminosity samples. The palette container starts with the identity translation in slot 0.

## Credit rationale

UNCredited specialfont.cpp 1–195 (+195). The file ends at 195. A full tally line scan shows no overlap.
