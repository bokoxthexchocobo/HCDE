# Chunk 99 — `bitmap.cpp` ice copy path

## Faithful

`IcePalette` table and `iCopyColors` branches `BLEND_NONE` and `BLEND_ICEMAP` for RGB sources to BGRA output.

## Seams

- Special colormap, desaturate, modulate, and overlay blend modes not ported (`NotSupportedException`).
- No `FBitmap::CopyPixelDataRGB` clip rect / op table dispatch.

## Tests

- `VmBitmapColorCopyTests`.
