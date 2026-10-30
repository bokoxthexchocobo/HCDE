# Chunk 1263 — Software canvas pixels, the unrendered fill, and the true-color remap from swcanvastexture.cpp (1-188).

## Scope

Software canvas pixels, the unrendered fill, and the true-color remap from swcanvastexture.cpp (1-188).

## Native ranges

- `src/rendering/swrenderer/textures/swcanvastexture.cpp` **FSWCanvasTexture** 1–188 (188 lines)

**Credited native LOC:** 188 (tally `nativeLoc` 188).

## Managed

`VmSwCanvas1263Slice.cs`
Managed LOC (tally): 277.
Conversion commit: `2dbd5eda31261970cc833463c20d3bef9af14e19`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| canvasMap and GetSWCamTex (**30–36**) | yes |
| FSWCanvasTexture (**38–43**) | yes |
| ~FSWCanvasTexture (**46–59**) | yes |
| GetPixelsLocked (**68–77**) | yes |
| GetPixelsBgraLocked (**85–93**) | yes |
| MakeTexture (**101–109**) | yes |
| MakeTextureBgra (**117–125**) | yes |
| Unload (**133–148**) | yes |
| UpdatePixels (**156–188**) | yes |
| Band hull (**1–188**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| DCanvas allocation | host canvas |
| RGB32k and GPalette.Remap | host tables |

The constructor inserts this software texture under the canvas and replaces an existing entry. Lookup of a missing canvas returns null. The destructor drops the paletted and BGRA canvases when they exist and leaves the map entry in place. The paletted lock calls NeedUpdate, builds the texture only when the canvas is missing, and ignores its style argument. The unrendered paletted bytes are zero through width times height over two, then 255 for that same count, so an odd size leaves the last byte at the value from allocation. The BGRA lock does the same for its canvas. Its second fill starts at byte offset width times height over two and writes 255 for four times width times height over two bytes. Unload drops both canvases and then clears the pixel buffers and the unlock cache. A true-color update copies BGRA canvas pixels by pitch into column-major order, then reads the paletted and BGRA software buffers through the generation cache. A cache miss locks again. A present canvas is not rebuilt, so the copy survives. Each column stores RGB32k at the five-bit channels from RPART, GPART, and BPART. A missing BGRA canvas throws before SetUpdated. A paletted update remaps through the canvas pitch and does not lock. SetUpdated is passed false, which clears the need flag and the first-update flag and stores a false render type.

| Managed (`VmSwCanvas1263Slice.cs`) | Native (`swcanvastexture.cpp`) |
|-----------------------------------|--------------------------|
| Map, unrendered fill, and column remap | **1–188** |
| Band hull (exclusive slice) | **1–188** |

### Chunk **1263** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **188** |
| **Net headline** | **+188** vs post-**1262** |


**Post-review ratio:** 277/188 managed/native ≈ 147% (src file LOC at HEAD; see tally).

## Tests

- `VmSwCanvas1263SliceTests.Truecolor_FlipsColumnsAndKeepsTheFillWhenTheCacheMisses`
- `VmSwCanvas1263SliceTests.Paletted_RemapsOddFillAndKeepsTheMapAfterDestroy`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1263 --to 1263` (0 hard / 0 soft).

## Seams

DCanvas pitch for a width above 640, the RGB32k table, and GPalette.Remap are host inputs. FlipNonSquareBlock is the column copy this file calls. NeedUpdate and SetUpdated are the one-line canvas flags this file calls. Base Unload clears the software pixel buffers and the unlock cache.

## Credit rationale

UNCredited swcanvastexture.cpp 1-188 (+188). A full tally line scan shows no overlap. The file ends at line 188.
