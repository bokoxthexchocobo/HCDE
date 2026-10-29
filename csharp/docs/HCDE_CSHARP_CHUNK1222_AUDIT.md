# Chunk 1222 — Console line editing, yank buffer, and the scaled prompt draw from c_commandbuffer.cpp (1-337).

## Scope

Console line editing, yank buffer, and the scaled prompt draw from c_commandbuffer.cpp (1-337).

## Native ranges

- `src/common/console/c_commandbuffer.cpp` **FCommandBuffer::MakeStartPosGood** 1–337 (337 lines)

**Credited native LOC:** 337 (tally `nativeLoc` 337).

## Managed

`VmCommandBuffer1222Slice.cs`
Managed LOC (tally): 442.
Conversion commit: `04a621cbdf5a0d2a10b49a71cb008e5d44e38086`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Copy and GetText (**33–45**) | yes |
| Draw (**47–85**) | yes |
| CalcCellSize (**87–96**) | yes |
| CharsForCells (**99–110**) | yes |
| MakeStartPosGood (**113–147**) | yes |
| Cursor motion (**149–204**) | yes |
| Deletes and yank (**206–269**) | yes |
| AddChar, AddString, SetString, AddYankBuffer (**271–336**) | yes |
| Band hull (**1–337**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| none | the band is the whole file |

Word-left stops when the character before the cursor is a space, so a cursor already at a word start continues through that space. A visible start that lands inside a double-width character moves to the next character. A paste stops at the first CR, LF, or backspace.

| Managed (`VmCommandBuffer1222Slice.cs`) | Native (`c_commandbuffer.cpp`) |
|------------------------------------------|--------------------------------|
| Command line and yank buffer | **1–337** |
| Band hull (exclusive slice) | **1–337** |

### Chunk **1222** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **337** |
| **Net headline** | **+337** vs post-**1221** |


**Post-review ratio:** 442/337 managed/native ≈ 131% (src file LOC at HEAD; see tally).

## Tests

- `VmCommandBuffer1222SliceTests.Edit_YanksWordsAndDecodesThePaste`
- `VmCommandBuffer1222SliceTests.Draw_SkipsAWideCellAndTheCopyLeavesColumnsZero`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1222 --to 1222` (0 hard / 0 soft).

## Seams

Cell size divides the new-console font width by 9. The prompt and cursor use the current console font widths of 0x1c and 0x0b. Column count is ConCols divided by the active console scale. Pasted bytes are read by GetCharFromString, which maps Windows-1252 0x80-0x9F and otherwise UTF-8. GetText encodes each code point with utf8_encode and drops a code point that encoder rejects. The copy constructor copies the text and the two cursors and leaves the column count, yank buffer, and cell counters at zero.

## Credit rationale

UNCredited c_commandbuffer.cpp 1-337 (+337). The file ends at 337. A full tally line scan shows no overlap.
