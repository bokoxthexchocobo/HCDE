# Chunk 1239 — Startup picture, notch, and Strife background pixels from startuptexture.cpp (1-368).

## Scope

Startup picture, notch, and Strife background pixels from startuptexture.cpp (1-368).

## Native ranges

- `src/common/textures/formats/startuptexture.cpp` **FStartupTexture::CreatePalettedPixels** 1–368 (368 lines)

**Credited native LOC:** 368 (tally `nativeLoc` 368).

## Managed

`VmStartupTexture1239Slice.cs`
Managed LOC (tally): 402.
Conversion commit: `83f9fdf75762ba370b99ef0b140409552e9878d3`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Sizes, Strife table, shared palette (**1–55**) | yes |
| Image classes (**63–91**) | yes |
| Raw probe declaration (**99**) | yes |
| Name gate (**107–138**) | yes |
| Startup palette (**147–172**) | yes |
| Planar decode (**182–213**) | yes |
| Startup paletted pixels and copy (**221–246**) | yes |
| Notch pixels and copy (**255–303**) | yes |
| Strife picture pixels (**313–333**) | yes |
| Background window (**342–367**) | yes |
| Band hull (**1–368**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| none | the band is the whole file |

A notch whose header looks like a patch with every column at width*4+8 is rejected. A zero header of the right length is accepted. The notch paletted loop does not paint, so the flipped result is remap[0] even after a startup palette load. Startup copy returns the unflipped pal32 expansion. Strife and the background have no CopyPixels in this file.

| Managed (`VmStartupTexture1239Slice.cs`) | Native (`startuptexture.cpp`) |
|------------------------------------------|----------------------------|
| Probe, palette, planar, notch, strife, background | **1–368** |
| Band hull (exclusive slice) | **1–368** |

### Chunk **1239** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **368** |
| **Net headline** | **+368** vs post-**1238** |


**Post-review ratio:** 402/368 managed/native ≈ 109% (src file LOC at HEAD; see tally).

## Tests

- `VmStartupTexture1239SliceTests.Probe_RejectsAPatchAndDecodesStrifeAndTheBackgroundWindow`
- `VmStartupTexture1239SliceTests.Startup_SharesThePaletteWithNotchesAndSkipsTheFlipOnCopy`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1239 --to 1239` (0 hard / 0 soft).

## Seams

CheckIfRaw is the host probe this file calls. A buffer whose length is not the desired size is rejected. Dimensions that look like a patch reject the buffer as raw only when every column offset equals width*4+8. Any other column accepts it: an offset at or past the last byte, a topdelta of 255, or a post walk that runs off the end. The startup constructor expands 16 colors from 6-bit components (alpha 63 becomes 255 via (d<<2)|((d>>4)&0x03030303)) into the shared palette. Pal8 is the Pick seam (r*3+g*5+b*7)&255. Pal32 is the expanded dword. Game palette is always false. PlanarToChunky keeps the outer >>4 outside the OR. Startup paletted pixels decode from source+48 through pal8, then FlipNonSquareBlockRemap. Startup CopyPixels writes pal32 into the bitmap with no flip and returns 0. The frame argument is logged and unused. The notch paletted loop condition is i * width * height / 2 with no less-than, so it never runs. The work buffer native leaves uninitialized is zero here, and the log says so. After the flip, every byte is remap[0], which is 7. Notch CopyPixels uses the correct pair count, writes pal32 nibbles, and does not flip. Strife pictures and the background do not override CopyPixels in this file, so the slice logs copy none and returns -1. Strife pixels flip the raw source through GetRemap. Luminance is Gray i^0x22. Other conversions are Remap with [0]=7. The background fills 64000 bytes with 0xF0, seeks to 57*320, and reads 95*320 bytes onto offset 41*320. Rows 0-40 and 136-199 stay 0xF0.

## Credit rationale

UNCredited startuptexture.cpp 1-368 (+368). The file ends at 368. A full tally line scan shows no overlap.
