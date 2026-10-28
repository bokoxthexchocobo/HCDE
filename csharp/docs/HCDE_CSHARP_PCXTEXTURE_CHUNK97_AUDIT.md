# Chunk 97 — `pcxtexture.cpp` + `imagehelpers.h` helpers

## Faithful

`PCXImage_TryCreate` header gates, 8-bit RLE decode, VGA palette tail (`0x0C`), column-major flip via `FlipSquareBlockRemap` / `FlipNonSquareBlockRemap`, `RGBToPalettePrecise` / `Luminance`.

## Seams

- 1/4/24-bit PCX readers and `FImageSource` lump IO not ported.
- `VmPaletteColorMatcher` / `VmRgb256kTable` stand in for `ColorMatcher` / `RGB256k`.

## Tests

- `VmPcxTextureImageTests`, `VmFlatTextureLayoutTests.FlipSquareBlockRemap_TransposesTwoByTwo`.
