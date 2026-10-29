# Chunk 1197 — Build ANM probe, frame-1 copy, and palette expand from anmtexture.cpp (1–165).

## Scope

Build ANM probe, frame-1 copy, and palette expand from anmtexture.cpp (1–165).

## Native ranges

- `src/common/textures/formats/anmtexture.cpp` **FAnmTexture::CreatePalettedPixels** 1–165 (165 lines)

**Credited native LOC:** 165 (tally `nativeLoc` 165).

## Managed

`VmAnmTexture1197Slice.cs`
Managed LOC (tally): 184.
Conversion commit: `d9ba6dbbeccab5db5e8e2c8f89b2689d34168b9a`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| AnmImage_TryCreate (**55–78**) | yes |
| FAnmTexture constructor (**86–93**) | yes |
| FAnmTexture::ReadFrame (**95–113**) | yes |
| FAnmTexture::CreatePalettedPixels (**127–140**) | yes |
| FAnmTexture::CopyPixels (**148–165**) | yes |
| Band hull (**1–165**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| none | the band is the whole file |

TryCreate rewinds, requires the four characters LPF and a space, rewinds again, and loads the padded file. The load length drops the extra zero. A short read, a failed load, or zero frames returns nothing. One or more frames creates a 320 by 200 texture with both offsets cleared. ReadFrame loads the lump at its real size. It copies 768 palette bytes and frame 1 when that load succeeds and has a frame. Otherwise it clears the pixel and palette buffers. CreatePalettedPixels matches all 256 palette colors and stores them in column order. CopyPixels writes those indices as opaque BGRA in the frame's original order and returns -1.

| Managed (`VmAnmTexture1197Slice.cs`) | Native (`anmtexture.cpp`) |
|--------------------------------------|---------------------------|
| ANM texture | **1–165** |
| Band hull (exclusive slice) | **1–165** |

### Chunk **1197** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **165** |
| **Net headline** | **+165** vs post-**1196** |


**Post-review ratio:** 184/165 managed/native ≈ 112% (src file LOC at HEAD; see tally).

## Tests

- `VmAnmTexture1197SliceTests.TryCreate_RequiresLpfAndAtLeastOneFrame`
- `VmAnmTexture1197SliceTests.ReadFrame_CopiesFrameOneAndPaintsColumnMajorPalette`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1197 --to 1197` (0 hard / 0 soft).

## Seams

ANIM_LoadAnim, ANIM_DrawFrame, and ColorMatcher.Pick are host results. TryCreate passes the file length, which is the padded buffer size minus the extra zero. ReadFrame passes the raw lump size and draws frame 1. A missing lump throws. CopyPixels writes Width times Height pixels from the start of the bitmap and does not clip.

## Credit rationale

UNCredited anmtexture.cpp 1–165 (+165). The file ends at 165. A full tally line scan shows no overlap.
