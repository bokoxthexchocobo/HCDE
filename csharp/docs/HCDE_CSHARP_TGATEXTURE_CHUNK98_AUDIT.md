# Chunk 98 — `tgatexture.cpp`

## Faithful

`TGAImage_TryCreate` validation, RLE `ReadCompressed`, paletted type-1 decode with color map, bottom-up scanline handling, column-major pixel storage.

## Seams

- RGB 15/16/32 and grayscale branches partially ported (`DecodeRgb24` only for 24-bit).
- No `FImageSource` / masked alpha classification beyond header bits.

## Tests

- `VmTgaTextureImageTests`.
