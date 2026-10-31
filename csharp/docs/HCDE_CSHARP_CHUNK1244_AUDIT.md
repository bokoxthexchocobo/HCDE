# Chunk 1244 — RGB and gray-alpha palettize, CopyPixels, and the savegame file texture from pngtexture.cpp (466-786). The gray and palette IDAT read stays chunk 106. M_ReadIDAT is not reimplemented; the slice consumes the expanded bytes.

## Scope

RGB and gray-alpha palettize, CopyPixels, and the savegame file texture from pngtexture.cpp (466-786). The gray and palette IDAT read stays chunk 106. M_ReadIDAT is not reimplemented; the slice consumes the expanded bytes.

## Native ranges

- `src/common/textures/formats/pngtexture.cpp` **FPNGTexture::CreatePalettedPixels** 466–786 (321 lines)

**Credited native LOC:** 321 (tally `nativeLoc` 321).

## Managed

`VmPngTexture1244Slice.cs`
Managed LOC (tally): 553.
Conversion commit: `c1f91edec66df1fd2d7eb4e87f0a0703f9b78ad8`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| RGB and gray-alpha palettize (**467–536**) | yes |
| CopyPixels (**544–654**) | yes |
| Savegame texture type (**669–680**) | yes |
| PNGTexture_CreateFromFile (**689–709**) | yes |
| FPNGFileTexture constructor (**717–725**) | yes |
| GetBgraBitmap (**733–786**) | yes |
| Band hull (**466–786**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| Gray and palette IDAT decode | already chunk 106 |
| Pre-IDAT constructor walk | already chunk 1243 |

Column-major RGB writes 0 when the pixel matches the transparency key and otherwise indexes the 64-cube, or luminance times alpha over 255. Gray plus alpha calls palette setup when the map is missing, then writes luminance times alpha or a one-bit alpha cut at 128. CopyPixels walks PLTE and paletted tRNS, clears one gray index when that sample is below 256, and chooses paletted, keyed RGB, gray-alpha, or RGBA. The savegame loader accepts only 8-bit paletted or 24-bit RGB with no filter, compression, or interlace, then expands through the same bitmap copier.

| Managed (`VmPngTexture1244Slice.cs`) | Native (`pngtexture.cpp`) |
|--------------------------------------|---------------------------|
| RGB palettize, CopyPixels, savegame file texture | **466–786** |
| Band hull (exclusive slice) | **466–786** |

### Chunk **1244** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **321** |
| **Net headline** | **+321** vs post-**1243** |


**Post-review ratio:** 553/321 managed/native ≈ 172% (src file LOC at HEAD; see tally).

## Tests

- `VmPngTexture1244SliceTests.Palettize_WalksColumnsAndGrayAlpha`
- `VmPngTexture1244SliceTests.Copy_BuildsBitmapAndSavegameTexture`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1244 --to 1244` (0 hard / 0 soft).

## Seams

M_ReadIDAT stays the chunk 106 decode. This slice copies the caller-supplied expanded bytes and logs a short buffer. RGBToPalette indexes the 64-cube through a callback because RGB256k is not in this file. A null cube writes 0. Bitmap expansion uses the chunk 1241 copier. A type-2 transparency key above 255 cannot be a byte key, so the copy uses CF_RGB and logs trns wide. Type 4 with a null palette map calls the chunk 1243 SetupPalette on the source lump. The memory-file reader rejects a seek that would leave the buffer. A tRNS longer than 256 entries stores 256 and still consumes the chunk. GetBgraBitmap starts the palette at 0 and writes only the PLTE entries it reads. Remap and the translucent pointer on GetBgraBitmap are unused parameters.

## Credit rationale

UNCredited pngtexture.cpp 466-786 (+321). Chunk 106 already owns 74-117, 164-176, and the gray and palette IDAT read. Chunk 1243 owns 177-423. A full tally line scan shows no overlap with 466-786.
