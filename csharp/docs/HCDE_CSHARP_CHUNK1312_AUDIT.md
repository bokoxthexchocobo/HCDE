# Chunk 1312 — Viewport letterbox, frame cap, and the screen palette color from v_framebuffer.cpp (1-311).

## Scope

Viewport letterbox, frame cap, and the screen palette color from v_framebuffer.cpp (1-311).

## Native ranges

- `src/common/rendering/v_framebuffer.cpp` **DFrameBuffer::SetViewportRects** 1–311 (311 lines)

**Credited native LOC:** 311 (tally `nativeLoc` 311).

## Managed

`VmFrameBuffer1312Slice.cs`
Managed LOC (tally): 324.
Conversion commit: `55a6bb5fb65c2293556f2231a64f926921bff382`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Size and palette clear (**63–107**) | yes |
| Viewport, window map, and frame cap (**156–280**) | yes |
| Screen actions (**289–311**) | yes |
| Band hull (**1–311**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| Scale-factor math | host returns the scaled size and the pixel aspect |
| Scene rectangle | host returns the callback rect when one is installed |

SetSize stores the scaled size with no minimum. Update scales the client, then raises a side below 320 or 200, and writes that size directly when it differs. The resize notification runs before the vertex buffer, so a null vertex buffer still leaves the new size in place. SetClearColor divides the palette red, green, and blue by 255 and forces alpha to 1. The clear color starts at alpha 255 until that call. SetVSync, both wipe grabs, and the destructor leave the size unchanged. A bounds rectangle is copied into the scene, the screen, and the letterbox, and it becomes the game size. A zero client side is treated as 160 by 120. The letterbox scale is the minimum of the two client ratios, and the box is centered with integer division. Rounding is half away from zero. The unscaled test compares the screen width with the scaled width and also with the scaled height, then requires a pixel aspect of 1. The windowed scale path runs only when that test passes and scaling is enabled. The game size stays the virtual size. Window Y is measured up from the bottom of the screen viewport. Window coordinates are scaled by the virtual size over the letterbox and stored in 16 bits. The frame cap returns immediately when the maximum is not positive or the cap cvar is on. Otherwise it samples the clock, breaks when the wait is non-positive or above one second, yields on a wait of 2000 microseconds or less, and sleeps the remainder minus 2000. The stored time is the last sample. PaletteColor turns an index outside 0 through 255 into the integer 0 without reading the palette. An in-range index returns the palette dword as a signed int. A non-positive return count still evaluates the read.

| Managed (`VmFrameBuffer1312Slice.cs`) | Native (`v_framebuffer.cpp`) |
|----------------------------------------|-----------------------------|
| Viewport, frame cap, and palette | **1–311** |
| Band hull (exclusive slice) | **1–311** |

### Chunk **1312** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **311** |
| **Net headline** | **+311** vs post-**1311** |


**Post-review ratio:** 324/311 managed/native ≈ 104% (src file LOC at HEAD; see tally).

## Tests

- `VmFrameBuffer1312SliceTests.Update_ClampsTheClientAndResizesBeforeTheVertex`
- `VmFrameBuffer1312SliceTests.Viewport_LetterboxesHalfPixelsAndCapsTheFrame`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1312 --to 1312` (0 hard / 0 soft).

## Seams

ViewportScaledWidth, ViewportScaledHeight, and ViewportPixelAspect are host scale calls. GetSceneRect is optional. V_OutputResized and the vertex buffer are callees. The clock and the sleep are host calls. Palette entries are host colors. twod supplies the screen action sizes.

## Credit rationale

UNCredited v_framebuffer.cpp 1-311 (+311). A full tally line scan shows no overlap. The file ends at line 311.
