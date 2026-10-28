# Chunk 107 — `ddstexture.cpp`

`CheckDDS`, `DDSImage_TryCreate`, uncompressed `ReadRGB` paletted output, and `DecompressDXT1` for block textures.

Seams: DXT2–5, `CopyPixels` / ARGB mode, mip chains beyond first surface.

Tests: `VmDdsTextureImageTests`.
