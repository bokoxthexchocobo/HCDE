# Chunk 1368 — IDAT chunk tail, MakeChunk, StuffPalette, CalcSum, and SelectFilter from m_png.cpp (655-886).

## Scope

IDAT chunk tail, MakeChunk, StuffPalette, CalcSum, and SelectFilter from m_png.cpp (655-886).

## Native ranges

- `src/common/textures/m_png.cpp` **PngTail** 655–886 (232 lines)

**Credited native LOC:** 232 (tally `nativeLoc` 232).

## Managed

`VmPngTail1368Slice.cs`
Managed LOC (tally): 160.
Conversion commit: `1cd7fb2c7b457932e47291d66f0d8c081ba12d4c`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Next-chunk read (**655–672**) | yes |
| `inflateEnd` and the low-bit unpack (**673–688**) | yes |
| `MakeChunk` banner and body (**690–709**) | yes |
| `StuffPalette` (**711–729**) | yes |
| `CalcSum` (**731–747**) | yes |
| `SelectFilter` and the compiled-out macro (**749–885**) | yes |
| Blank line before `M_SaveBitmap` (**886**) | yes |
| Band hull (**655–886**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| m_png.cpp **470–654** | `M_ReadIDAT` inflate loop, earlier PNG chunks |
| m_png.cpp **887–1104** | `M_SaveBitmap` and `WriteIDAT`, chunk 1247 |
| `UnpackPixels` and `CalcCRC32` | helpers outside this band stay injected |

A zero `chunklen` with `lastIDAT` still false reads 12 bytes. Any count other than 12 sets `lastIDAT` and leaves `chunklen` at 0. A type other than `IDAT` does the same. A matching type stores `BigLong` of the length word and leaves `lastIDAT` false. `x[0]` is not used. A nonzero `chunklen` or `lastIDAT` does not read.

`inflateEnd` runs before the unpack. Bit depth 8 and above skips `UnpackPixels` and still returns true. Below 8, the step is `pitch << interlace` and the first offset is `pitch * interlace`. The walk continues while the offset is less than or equal to `prev`, and both pointers passed to `UnpackPixels` are that same offset. The gray flag is `colortype == 0`. A zero step that would not advance throws `passpitch`.

`MakeChunk` writes the big-endian length eight bytes before the payload and the type word, unswapped, four bytes before it. The CRC covers that type word plus the payload. The CRC word is stored big-endian at the end of the payload. A throw from `CalcCRC32` leaves the length and type already written.

`StuffPalette` copies 256 red, green, and blue bytes and skips each alpha byte. `CalcSum` adds each byte as a signed char into a 32-bit sum. A zero length adds nothing. `SelectFilter` returns 0. `USE_FILTER_HEURISTIC` is 0, and the function form returns 0 before the row sums, so those sums do not run and the row is left unchanged.

| Managed (`VmPngTail1368Slice.cs`) | Native (`m_png.cpp`) |
|-----------------------------------|----------------------|
| IDAT tail, chunk, palette, sum, and filter | **655–886** |
| Band hull (exclusive slice) | **655–886** |

### Chunk **1368** gross vs net

| | Value |
|--|------:|
| Row nativeLoc | **232** |
| **Net headline** | **+232** vs the prior chunk |


**Post-review ratio:** 160/232 managed/native ≈ 69% (src file LOC at HEAD; see tally).

## Tests

- `VmPngTail1368SliceTests.IdatTailAndUnpack`
- `VmPngTail1368SliceTests.ChunkPaletteSumAndFilter`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1368 --to 1368` (0 hard / 0 soft).

## Seams

Read, inflateEnd, UnpackPixels, and CalcCRC32 stay injected. The inflate loop through line 654 stays on the earlier PNG chunks. M_SaveBitmap at line 887 stays chunk 1247.

## Credit rationale

UNCredited m_png.cpp 655-886 (+232). A full tally line scan, including recognition ranges, shows no overlap. This is the largest remaining hole of at most 400 lines. It is not a tie. findfile.cpp 165-391 is 227. Gaps longer than 400 lines stay whole and are not sliced. USE_FILTER_HEURISTIC is 0, and the function form also returns 0 before the sums, so SelectFilter does not run the heuristic.
