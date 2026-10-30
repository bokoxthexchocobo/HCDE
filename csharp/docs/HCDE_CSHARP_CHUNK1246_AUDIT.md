# Chunk 1246 — PNG signature, IHDR, gAMA, PLTE, chunk and text append, chunk cursor, and verify from m_png.cpp (117-460). M_ReadIDAT stays on the earlier PNG chunks. M_SaveBitmap is not in this band.

## Scope

PNG signature, IHDR, gAMA, PLTE, chunk and text append, chunk cursor, and verify from m_png.cpp (117-460). M_ReadIDAT stays on the earlier PNG chunks. M_SaveBitmap is not in this band.

## Native ranges

- `src/common/textures/m_png.cpp` **M_CreatePNG** 117–460 (344 lines)

**Credited native LOC:** 344 (tally `nativeLoc` 344).

## Managed

`VmPngFile1246Slice.cs`
Managed LOC (tally): 518.
Conversion commit: `454530c0a9955c7f86adca869faafc4006a1c904`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| M_CreatePNG (**126–170**) | yes |
| M_CreateDummyPNG (**180–191**) | yes |
| M_FinishPNG (**202–206**) | yes |
| M_AppendPNGChunk (**216–233**) | yes |
| M_AppendPNGText (**243–268**) | yes |
| M_FindPNGChunk and M_NextPNGChunk (**284–309**) | yes |
| M_GetPNGText (**320–358**) | yes |
| M_VerifyPNG (**369–447**) | yes |
| M_FreePNG (**457–460**) | yes |
| Band hull (**117–460**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| IDAT inflate | earlier PNG chunks |
| Bitmap save and deflate | later in this file |

The header is the PNG signature, an 8-bit IHDR, a gAMA of 45454.5 times the gamma, and a 256-color PLTE when the picture is paletted. Chunk append writes a big-endian length, the type, the bytes, and the CRC. Text is a tEXt chunk with the keyword cut at 79 characters. Verify requires the signature, an IHDR first chunk, one IDAT, and an empty IEND. A text chunk is stored and then skipped so the next seek covers only its CRC. Find returns a zero-length chunk as 0.

| Managed (`VmPngFile1246Slice.cs`) | Native (`m_png.cpp`) |
|-----------------------------------|----------------------|
| Header, chunk append, text, cursor, verify | **117–460** |
| Band hull (exclusive slice) | **117–460** |

### Chunk **1246** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **344** |
| **Net headline** | **+344** vs post-**1245** |


**Post-review ratio:** 518/344 managed/native ≈ 151% (src file LOC at HEAD; see tally).

## Tests

- `VmPngFile1246SliceTests.Create_WritesHeaderPaletteAndSaveCall`
- `VmPngFile1246SliceTests.Verify_WalksChunksAndRejectsABadSignature`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1246 --to 1246` (0 hard / 0 soft).

## Seams

M_CreatePNG calls M_SaveBitmap, which is later in the file. This slice writes the signature and the IHDR, gAMA, and PLTE chunks, then records the save call and does not deflate. MakeChunk and StuffPalette are also later in the file. The CRC is the zlib CRC used by CalcCRC32 and AddCRC32. A missing palette writes zeros. A text chunk longer than 1048576 bytes is rejected. png_gamma replaces the passed gamma when it is not 0. A short write returns false and leaves the bytes already stored.

## Credit rationale

UNCredited m_png.cpp 117-460 (+344). The IDAT reader at 470-654 and UnpackPixels at 1202-1288 are already credited. A full tally line scan shows no overlap with 117-460.
