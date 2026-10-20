# Chunk 106 — `pngtexture.cpp` / `m_png.cpp`

`PNGImage_TryCreate` signature and IHDR validation; `FPNGTexture::CreatePalettedPixels` for 8-bit grayscale/palette; `M_ReadIDAT` non-interlaced subset in `VmPngIdatReader`.

Seams: 16-bit PNG / `StbImage_TryCreate` path (118–162), interlaced Adam7, RGB/RGBA `CreatePalettedPixels`, `CopyPixels`, full `M_ReadIDAT` chunk chaining.

Tests: `VmPngTextureImageTests`.
