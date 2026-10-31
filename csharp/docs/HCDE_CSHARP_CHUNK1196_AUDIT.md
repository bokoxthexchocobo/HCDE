# Chunk 1196 — stb_image probe, BGRA copy, and column-major palette from stbtexture.cpp (1–155).

## Scope

stb_image probe, BGRA copy, and column-major palette from stbtexture.cpp (1–155).

## Native ranges

- `src/common/textures/formats/stbtexture.cpp` **FStbTexture::CreatePalettedPixels** 1–155 (155 lines)

**Credited native LOC:** 155 (tally `nativeLoc` 155).

## Managed

`VmStbTexture1196Slice.cs`
Managed LOC (tally): 222.
Conversion commit: `0375aedc0129afad3cecf2d8e082a5b40bbe6e50`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Callbacks (**62–66**) | yes |
| StbImage_TryCreate (**73–84**) | yes |
| FStbTexture constructor (**92–99**) | yes |
| FStbTexture::CreatePalettedPixels (**107–138**) | yes |
| FStbTexture::CopyPixels (**146–155**) | yes |
| Band hull (**1–155**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| none | the band is the whole file |

TryCreate rewinds the reader and keeps a texture only when the probe result is 1. The constructor stores the lump and clears both offsets. CopyPixels opens that lump, and on a decode copies RGBA into BGRA with the source row stride. Alpha 0 stays untouched. The copy is clipped to the bitmap. The call returns -1 whether or not a decode exists. CreatePalettedPixels builds a zeroed bitmap at the texture size, copies into it, then walks BGRA in row order and writes palette indices in column order. Alpha below 128 becomes 0. Otherwise the color uses the 256k cube, or luminance when the conversion requests it. That luminance multiplies by a full alpha of 255, so a visible pixel keeps its luminance.

| Managed (`VmStbTexture1196Slice.cs`) | Native (`stbtexture.cpp`) |
|--------------------------------------|---------------------------|
| stb texture | **1–155** |
| Band hull (exclusive slice) | **1–155** |

### Chunk **1196** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **155** |
| **Net headline** | **+155** vs post-**1195** |


**Post-review ratio:** 222/155 managed/native ≈ 143% (src file LOC at HEAD; see tally).

## Tests

- `VmStbTexture1196SliceTests.TryCreate_SeeksAndKeepsOnlyASuccessfulProbe`
- `VmStbTexture1196SliceTests.CreatePalettedPixels_TransposesClippedRgbaAndZerosLowAlpha`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1196 --to 1196` (0 hard / 0 soft).

## Seams

stbi_info and stbi_load are host results. The slice seeks to the start, copies RGBA into a BGRA bitmap, and frees nothing itself. A missing decode leaves the bitmap unchanged and still returns -1. Alpha 0 is not written. The palette step then treats alpha below 128 as index 0 and does not pass that alpha into the color match.

## Credit rationale

UNCredited stbtexture.cpp 1–155 (+155). The file ends at 155. A full tally line scan shows no overlap.
