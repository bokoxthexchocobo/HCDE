# Chunk 80 — `emptytexture.cpp`

## Scope

Placeholder 8-byte lump recognition and 1×1 masked flat metadata.

## Faithful reproduction (credited)

| Native lines | Behaviour |
|--------------|-----------|
| 49–58 | `EmptyImage_TryCreate`: length 8, all-zero payload |
| 71–77 | Constructor: `bMasked`, 1×1 dimensions, `bUseGamePalette` |
| 85–89 | Single-byte paletted pixel stub |

## Not ported (not credited)

- File header / license block (lines 1–34)
- `FImageSource` lump binding, `CreateEmptyTexture()`, virtual `CreatePalettedPixels` IO

## Managed

`VmEmptyTextureImage.cs`

## Tests

`VmWholeFilePortTests.EmptyTexture_TryCreateFromEightZeroBytes`

## Addendum (honest LOC)

- **Native credited:** 22 code lines (ranges 49–58, 71–77, 85–89).
- **Managed credited:** 24 LOC implementation lines in `VmEmptyTextureImage.cs`.
