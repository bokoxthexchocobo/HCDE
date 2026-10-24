# Chunk 102 (complete) — `bitmap.cpp` `iCopyColors`

`BLEND_NONE`, `BLEND_ICEMAP`, desaturate 1–31, modulate, overlay, and special-colormap via `VmBitmapCopyInfo.SpecialColormap`.

Seam: `FBitmap` clip rect and `copyfuncs` op table (blend/add/subtract) not ported.

Tests: `VmBitmapColorCopyTests`.
