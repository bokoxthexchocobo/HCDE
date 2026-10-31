# Chunk 1243 — Pre-IDAT chunk walk, palette setup, and alpha remap from pngtexture.cpp (177-423). The gray and palette IDAT read at 424-465 stays chunk 106.

## Scope

Pre-IDAT chunk walk, palette setup, and alpha remap from pngtexture.cpp (177-423). The gray and palette IDAT read at 424-465 stays chunk 106.

## Native ranges

- `src/common/textures/formats/pngtexture.cpp` **FPNGTexture::FPNGTexture** 177–423 (247 lines)

**Credited native LOC:** 247 (tally `nativeLoc` 247).

## Managed

`VmPngTexture1243Slice.cs`
Managed LOC (tally): 615.
Conversion commit: `c3ac982ab3ed0d4356cc1af36c73693a501ea1c1`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Constructor call (**177–184**) | yes |
| FPNGTexture constructor (**185–293**) | yes |
| SetupPalette (**295–374**) | yes |
| ReadAlphaRemap (**382–395**) | yes |
| CreatePalettedPixels prologue (**403–423**) | yes |
| Band hull (**177–423**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| Gray and palette IDAT decode | already chunk 106 |

Grayscale with a transparency sample below 256 defers the gray map and masks the image. Every other grayscale and gray-alpha image shares the gray map. Paletted transparency of 0 masks that index and forces the remap entry to 0. A grab offset outside the signed 16-bit range becomes 0. The IDAT prologue fills with 0x99 when the IDAT offset is 0 and otherwise logs the raw header without decoding.

| Managed (`VmPngTexture1243Slice.cs`) | Native (`pngtexture.cpp`) |
|--------------------------------------|---------------------------|
| Pre-IDAT walk, palette setup, alpha remap, pixel prologue | **177–423** |
| Band hull (exclusive slice) | **177–423** |

### Chunk **1243** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **247** |
| **Net headline** | **+247** vs post-**1242** |


**Post-review ratio:** 615/247 managed/native ≈ 249% (src file LOC at HEAD; see tally).

## Tests

- `VmPngTexture1243SliceTests.Walk_DefersGrayAndRemapsPalette`
- `VmPngTexture1243SliceTests.Edge_CapsShortReadsAndLeavesIdatUndecoded`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1243 --to 1243` (0 hard / 0 soft).

## Seams

MemoryReader rejects a seek that would leave the buffer and leaves the position unchanged. The first length and id are zeroed when the native locals are uninitialized, and a short read is logged. Continuation reads overlay the previous length and the IEND preset, so a zero-byte id read stops the walk. A tRNS longer than 256 bytes stores 256 and still consumes the chunk. A missing PLTE on a paletted image uses zeros. SetupPalette copies GrayMap and clears one index without writing the shared table; when the palette size is not 256 the copy is still 256 bytes because the native memcpy is. MakeRemap sorts equal colors with foreign entries first and keeps the original order when the comparator ties, then matches only the foreign entry sitting immediately before a local color. BestColor searches indices 1 through 254 and returns on a zero distance, so an exact local at index 255 is reachable only through that neighbor match. Luminance uses weights 77, 143, and 37. CreatePalettedPixels reads the raw length and id and does not decode IDAT. A missing lump is an empty reader. The pixel buffer is capped at 1048576 bytes.

## Credit rationale

UNCredited pngtexture.cpp 177-423 (+247). Chunk 106 already owns 74-117, 164-176, and 424-465. A full tally line scan shows no overlap with 177-423.
