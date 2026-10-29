# Chunk 1225 — Text line breaking, broken-line queries, and the generic UI font switch from v_text.cpp (1-316).

## Scope

Text line breaking, broken-line queries, and the generic UI font switch from v_text.cpp (1-316).

## Native ranges

- `src/common/fonts/v_text.cpp` **V_BreakLines** 1–316 (316 lines)

**Credited native LOC:** 316 (tally `nativeLoc` 316).

## Managed

`VmText1225Slice.cs`
Managed LOC (tally): 466.
Conversion commit: `bfb426af33cd96853d17854a9bfdb9ad747a1d57`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| breakit (**40–49**) | yes |
| V_BreakLines (**51–181**) | yes |
| Serialize (**183–192**) | yes |
| DBrokenLines (**196–215**) | yes |
| IMPLEMENT_CLASS (**217–217**) | yes |
| Count (**219–223**) | yes |
| StringWidth (**225–230**) | yes |
| StringAt (**232–238**) | yes |
| FFont::BreakLines (**240–248**) | yes |
| CheckFontComplete (**254–259**) | yes |
| UpdateGenericUI (**261–311**) | yes |
| ui_generic cvar (**313–316**) | yes |
| Band hull (**1–316**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| none | the band is the whole file |

The color letter is both a glyph and a code. Two color escapes in one line keep the first code. A newline in an open whitespace run splits at the first space of that run.

| Managed (`VmText1225Slice.cs`) | Native (`v_text.cpp`) |
|---------------------------------|----------------------|
| Line break, queries, and generic UI | **1–316** |
| Band hull (exclusive slice) | **1–316** |

### Chunk **1225** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **316** |
| **Net headline** | **+316** vs post-**1224** |


**Post-review ratio:** 466/316 managed/native ≈ 147% (src file LOC at HEAD; see tally).

## Tests

- `VmText1225SliceTests.Break_WrapsColorUtf8AndRejectsABadIndex`
- `VmText1225SliceTests.Ui_ParsesStrtollAndFallsBackToTheStockFont`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1225 --to 1225` (0 hard / 0 soft).

## Seams

A single-character color code is not consumed, so the break gives it glyph width, while StringWidth of the stored line skips that same byte. A bracket color is consumed and adds no glyph width. The word-break pointer stays on the first byte of a whitespace run, so a later newline in that run splits there and the following skip eats the newline. A newline clears the carried color unless preservecolor is set. A hard break walks back over UTF-8 continuation bytes. The first glyph is kept even when it is wider than the line. Kerning is added after a glyph is kept and is not part of the overflow test. An out-of-range string query returns a one-byte 0xFF string because the -1 converts through FString(char). UpdateGenericUI uses strtoll base 0, falls the big font back to NewSmallFont, and rewrites only the i and I case-map slots when REQUIRED_CHARACTERS contains U+0130.

## Credit rationale

UNCredited v_text.cpp 1-316 (+316). The file ends at 316. A full tally line scan shows no overlap.
