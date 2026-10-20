# Chunk 100 (complete) — `pcxtexture.cpp`

`CreatePalettedPixels` dispatches 1/4/8/24-bit RLE readers and flip/remap paths from `CreatePalettedPixels` / `ReadPCX*`.

Seam: `FBitmap::CopyPixels` true-color path not ported (paletted column-major only).

Tests: `VmPcxTextureImageTests`.
