# Chunk 1194 — Console text, line breaks, and old-line deletion from c_consolebuffer.cpp (1–166).

## Scope

Console text, line breaks, and old-line deletion from c_consolebuffer.cpp (1–166).

## Native ranges

- `src/common/console/c_consolebuffer.cpp` **FConsoleBuffer::AddText** 1–166 (166 lines)

**Credited native LOC:** 166 (tally `nativeLoc` 166).

## Managed

`VmConsoleBuffer1194Slice.cs`
Managed LOC (tally): 170.
Conversion commit: `d4281a58add3060e731ba0acafa2119732df2a4c`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| FConsoleBuffer::FConsoleBuffer (**35–45**) | yes |
| FConsoleBuffer::AddText (**60–103**) | yes |
| FConsoleBuffer::FormatText (**111–150**) | yes |
| FConsoleBuffer::ResizeBuffer (**158–166**) | yes |
| Band hull (**1–166**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| none | the band is the whole file |

A critical line keeps the tan prefix. Other levels below the print-level count use the color table. Bold uses green. A log line and a level at or above the count stay tan. A carriage return makes the next add replace the line. A missing terminator makes the next add continue it, and the continued line keeps its original color. Format rebuilds every paragraph when the font, width, or cleared flag changes. Otherwise it rebuilds only the last paragraph when that paragraph was continued. Resize deletes the oldest lines and marks the buffer cleared when the text is longer than the new size.

| Managed (`VmConsoleBuffer1194Slice.cs`) | Native (`c_consolebuffer.cpp`) |
|------------------------------------------|------------------------------|
| Console text buffer | **1–166** |
| Band hull (exclusive slice) | **1–166** |

### Chunk **1194** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **166** |
| **Net headline** | **+166** vs post-**1193** |


**Post-review ratio:** 170/166 managed/native ≈ 102% (src file LOC at HEAD; see tally).

## Tests

- `VmConsoleBuffer1194SliceTests.AddText_ColorsContinuesAndReplacesLines`
- `VmConsoleBuffer1194SliceTests.FormatText_RebuildsTheDirtyTailAndDropsOldLines`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1194 --to 1194` (0 hard / 0 soft).

## Seams

V_BreakLines is a host call and always receives preservecolor true. PrintColors is the console color table. An empty string has no terminator, so it does not read a character before the start of the text. An append or replace on an empty buffer stores the new line only.

## Credit rationale

UNCredited c_consolebuffer.cpp 1–166 (+166). The file ends at 166. A full tally line scan shows no overlap.
