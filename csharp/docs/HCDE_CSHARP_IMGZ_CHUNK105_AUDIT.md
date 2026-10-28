# Chunk 105 — `imgztexture.cpp`

`IMGZImage_TryCreate`, uncompressed and ILBM-style RLE column-major decode with remap.

Seam: `checkIMGZPalette` MD5 table via `VmImgzPaletteCatalog` (empty unless extended); `CopyPixels` alpha path not ported.

Tests: `VmImgzTextureImageTests`.
