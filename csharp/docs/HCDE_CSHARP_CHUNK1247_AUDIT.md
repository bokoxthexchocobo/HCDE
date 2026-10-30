# Chunk 1247 — M_SaveBitmap and WriteIDAT from m_png.cpp (887-1104). Filter 0 rows, deflate flush, and the IDAT chunk. The header writer at 117-460 is chunk 1246. M_ReadIDAT stays on the earlier PNG chunks.

## Scope

M_SaveBitmap and WriteIDAT from m_png.cpp (887-1104). Filter 0 rows, deflate flush, and the IDAT chunk. The header writer at 117-460 is chunk 1246. M_ReadIDAT stays on the earlier PNG chunks.

## Native ranges

- `src/common/textures/m_png.cpp` **M_SaveBitmap** 887–1104 (218 lines)

**Credited native LOC:** 218 (tally `nativeLoc` 218).

## Managed

`VmPngSave1247Slice.cs`
Managed LOC (tally): 298.
Conversion commit: `3f689d2712b26e20fe262d5624c0eac71d19b45e`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| M_SaveBitmap (**896–1078**) | yes |
| WriteIDAT (**1088–1104**) | yes |
| Band hull (**887–1104**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| Header writer | chunk 1246 |
| IDAT inflate | earlier PNG chunks |
| Filter heuristic body | compiled out by USE_FILTER_HEURISTIC 0 |

Paletted rows copy width bytes after a 0 filter byte. RGB rows copy width times 3. BGRA rows reorder to RGB and drop alpha. The last row finishes the zlib stream. A full output buffer is written as one IDAT and the stream continues. WriteIDAT stores a big-endian length, the IDAT type, the bytes, and the zlib CRC. A short write returns false. Init failure and a stream that does not end return false. The fixed 32768-byte buffer never trips the size errors.

| Managed (`VmPngSave1247Slice.cs`) | Native (`m_png.cpp`) |
|-----------------------------------|----------------------|
| Bitmap save and IDAT chunk | **887–1104** |
| Band hull (exclusive slice) | **887–1104** |

### Chunk **1247** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **218** |
| **Net headline** | **+218** vs post-**1246** |


**Post-review ratio:** 298/218 managed/native ≈ 137% (src file LOC at HEAD; see tally).

## Tests

- `VmPngSave1247SliceTests.Save_PacksPaletteRgbAndBgraRows`
- `VmPngSave1247SliceTests.Save_FlushesFullBuffersAndRejectsABadStream`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1247 --to 1247` (0 hard / 0 soft).

## Seams

deflateInit, deflate, and deflateEnd are miniz. This slice calls the host zlib wrapper instead, so the compressed bytes are not the miniz level-5 stream. The row bytes, the filter-0 marker, the avail_out flush, and the IDAT length, type, and CRC match the native loop. ChunkBytes defaults to PNG_WRITE_SIZE (32768) and tests may shrink it to force a full buffer. A null pixel pointer returns false. png_level outside -1..9 fails init the way zlib rejects a level past 9.

## Credit rationale

UNCredited m_png.cpp 887-1104 (+218). The header writer, the IDAT reader, and UnpackPixels are already credited. A full tally line scan shows no overlap with 887-1104. USE_FILTER_HEURISTIC is 0, so the five-row filter block in this function is compiled out and every row stays filter 0.
