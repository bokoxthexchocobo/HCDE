# Chunk 111 — PNG interlaced / sub-8-bit completion

Adam7 interlaced `M_ReadIDAT` pass loop and `UnpackPixels` for 1/2/4-bit gray and palette PNGs; wired through `VmPngIdatReader.TryReadGrayOrPalette`.

Seam: 16-bit PNG / STB fallback, RGB/RGBA `CreatePalettedPixels`, `CopyPixels`.

Tests: `VmPngTextureImageTests.CreatePalettedPixels_DecodesInterlaced2x2`.
