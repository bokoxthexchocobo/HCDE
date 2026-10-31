# Chunk 1198 — FON1 and FON2 character pixels and the paletted copy from fontchars.cpp (1–167).

## Scope

FON1 and FON2 character pixels and the paletted copy from fontchars.cpp (1–167).

## Native ranges

- `src/common/textures/formats/fontchars.cpp` **FFontChar2::CreatePalettedPixels** 1–167 (167 lines)

**Credited native LOC:** 167 (tally `nativeLoc` 167).

## Managed

`VmFontChars1198Slice.cs`
Managed LOC (tally): 248.
Conversion commit: `74fa42ca09c329547b51c566435a55c66a5e5127`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| FFontChar2 constructor (**40–47**) | yes |
| FFontChar2::CreatePalettedPixels (**57–159**) | yes |
| FFontChar2::CopyPixels (**161–167**) | yes |
| Band hull (**1–167**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| none | the band is the whole file |

The constructor stores the lump, the source position, and both offsets. CreatePalettedPixels reads four header bytes. A FON2 mark reads seven more and uses the last of those as the maximum color, then seeks to the source position. A FON1 mark reads thirteen more, subtracts one from the last of those for the maximum, turns RLE off, and seeks to the source position. Any other header keeps RLE and a maximum of 255. RLE codes of zero or more start a literal run. Negative codes other than -128 start a repeated color. -128 is skipped. Colors above the maximum are clamped. Raw FON1 bytes are clamped the same way. Both walks store the glyph column-major. CopyPixels forces luminance back to normal, expands the palette to BGRA, and returns 0.

| Managed (`VmFontChars1198Slice.cs`) | Native (`fontchars.cpp`) |
|-------------------------------------|--------------------------|
| Font character | **1–167** |
| Band hull (exclusive slice) | **1–167** |

### Chunk **1198** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **167** |
| **Net headline** | **+167** vs post-**1197** |


**Post-review ratio:** 248/167 managed/native ≈ 149% (src file LOC at HEAD; see tally).

## Tests

- `VmFontChars1198SliceTests.CreatePalettedPixels_DecodesFon2RunsAndFon1Raw`
- `VmFontChars1198SliceTests.CopyPixels_ExpandsRemapAndSkipsTransparentEntries`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1198 --to 1198` (0 hard / 0 soft).

## Seams

The lump bytes and the font's short name are host results. A missing lump throws. A wrapped width times height aborts with the corrupt-font error before the pixel walk. CopyPixels clips to the bitmap and skips a remap entry whose alpha is 0. A null remap writes nothing and still returns 0.

## Credit rationale

UNCredited fontchars.cpp 1–167 (+167). The file ends at 167. A full tally line scan shows no overlap.
