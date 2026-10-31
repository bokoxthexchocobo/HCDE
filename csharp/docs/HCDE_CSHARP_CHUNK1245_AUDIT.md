# Chunk 1245 — TGA CopyPixels from tgatexture.cpp (368-518). The header check, the RLE reader, and CreatePalettedPixels stay on earlier TGA chunks. Bitmap expansion uses the chunk 1241 copier.

## Scope

TGA CopyPixels from tgatexture.cpp (368-518). The header check, the RLE reader, and CreatePalettedPixels stay on earlier TGA chunks. Bitmap expansion uses the chunk 1241 copier.

## Native ranges

- `src/common/textures/formats/tgatexture.cpp` **FTGATexture::CopyPixels** 368–518 (151 lines)

**Credited native LOC:** 151 (tally `nativeLoc` 151).

## Managed

`VmTgaTexture1245Slice.cs`
Managed LOC (tally): 333.
Conversion commit: `7e1761e0d19878990706635fe5b577b8dd72fff2`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| CopyPixels signature and locals (**376–384**) | yes |
| Header, id skip, and colormap (**385–431**) | yes |
| Raw or compressed pixels and the vertical flip (**433–460**) | yes |
| Paletted, RGB, and gray expansion (**462–517**) | yes |
| Band hull (**368–518**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| Header check and RLE reader | earlier TGA chunks |
| CreatePalettedPixels | earlier TGA chunks |

A colormap of 15, 16, 24, or 32 bits fills palette entries from the first index. A 32-bit entry keeps its alpha only when the descriptor low nibble is 8, and a value other than 0 or 255 marks the texture translucent. Uncompressed pixels are a raw read. Compressed pixels are the RLE loop. The image is bottom-up unless descriptor bit 5 is set. Paletted and 8-bit gray use the palette copier. 15-bit and 16-bit RGB use RGB555, 24-bit and 32-bit without alpha use BGR, 32-bit with alpha uses BGRA and returns -1, and 16-bit gray uses the high byte.

| Managed (`VmTgaTexture1245Slice.cs`) | Native (`tgatexture.cpp`) |
|--------------------------------------|---------------------------|
| CopyPixels colormap, flip, and format switch | **368–518** |
| Band hull (exclusive slice) | **368–518** |

### Chunk **1245** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **151** |
| **Net headline** | **+151** vs post-**1244** |


**Post-review ratio:** 333/151 managed/native ≈ 221% (src file LOC at HEAD; see tally).

## Tests

- `VmTgaTexture1245SliceTests.Copy_ExpandsPaletteRgbAndGray`
- `VmTgaTexture1245SliceTests.Copy_ReadsRleAndRejectsShortFiles`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1245 --to 1245` (0 hard / 0 soft).

## Seams

CopyPixels calls ReadCompressed. That function is already credited, and its managed copy is private, so this slice repeats the packet loop. A repeat packet clears the high bit before the remaining-pixel subtract. The memory-file reader rejects a seek that would leave the buffer. A short header is zeros. The palette starts at 0 when the file has no colormap; native leaves that table uninitialized. A 15-bit image uses one byte per pixel because the width shift is bpp>>3, and the copier may read one byte into a four-byte tail of zeros. Remap and conversion are unused parameters. The destination bitmap starts as the caller fill, which is 0 unless a test sets it.

## Credit rationale

UNCredited tgatexture.cpp 368-518 (+151). Chunk 98 owns 84-110, 133-161, and 169-266. Chunk 101 owns 267-367. A full tally line scan shows no overlap with 368-518.
