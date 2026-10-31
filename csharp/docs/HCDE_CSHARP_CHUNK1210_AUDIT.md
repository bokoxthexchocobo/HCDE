# Chunk 1210 — Frame size, YUV/RGB/palette copies, and the two-frame swap from animtexture.cpp (1–241).

## Scope

Frame size, YUV/RGB/palette copies, and the two-frame swap from animtexture.cpp (1–241).

## Native ranges

- `src/common/textures/animtexture.cpp` **AnimTexture::SetFrame** 1–241 (241 lines)

**Credited native LOC:** 241 (tally `nativeLoc` 241).

## Managed

`VmAnimTexture1210Slice.cs`
Managed LOC (tally): 330.
Conversion commit: `8c37ba24693ebe2b24d586d8a3a85d6e52746fc3`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| AnimTexture::SetFrameSize (**37–43**) | yes |
| YUVtoRGB (**46–59**) | yes |
| AnimTexture::SetFrame (**61–188**) | yes |
| AnimTexture::GetBgraBitmap (**196–199**) | yes |
| AnimTextures ctor, dtor, Clean, SetSize, SetFrame (**207–241**) | yes |
| Band hull (**1–241**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| none | the band is the whole file |

SetFrameSize stores the format, assigns width and height, and zeroes width*height*4 bytes. A null frame pointer leaves that buffer alone. Packed YUV consumes 4 bytes per pixel and ignores the fourth. VPX I420 samples chroma at (y>>1, x>>1), I444 at (y, x), I422 at (y, x>>1), and I440 at (y>>1, x). Any other VPX format writes nothing. RGB swaps the first three source bytes into every pixel. Paletted indexes RGB triples into BGRA and rejects a null palette. GetBgraBitmap returns pitch width*4 and does not read remap or trans. The pair starts with active 1, toggles before the write, and cleans only the texture it wrote. SetSize sizes both frames, then both game textures, then cleans both. Clean passes full=true and then drops both pointers. The destructor is that clean.

| Managed (`VmAnimTexture1210Slice.cs`) | Native (`animtexture.cpp`) |
|----------------------------------------|-----------------------------|
| AnimTexture and AnimTextures | **1–241** |
| Band hull (exclusive slice) | **1–241** |

### Chunk **1210** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **241** |
| **Net headline** | **+241** vs post-**1209** |


**Post-review ratio:** 330/241 managed/native ≈ 137% (src file LOC at HEAD; see tally).

## Tests

- `VmAnimTexture1210SliceTests.SetFrame_CopiesYuvRgbPaletteAndVpxPlanes`
- `VmAnimTexture1210SliceTests.SetFrame_SwapsTheTwoTexturesAndCleansHardware`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1210 --to 1210` (0 hard / 0 soft).

## Seams

FindGameTexture is the host lookup for AnimTextureFrame1 and AnimTextureFrame2 with ETextureType::Override (10). RGB rereads the first pixel because the source pointer never advances. YUV uses the native float constants and a truncating byte cast. VPX I420/I422/I444/I440 are the vpx format values. GetBgraBitmap ignores remap and trans and returns the live buffer. CleanHardwareData records the full flag, a base clean, and non-null material descriptor deletes.

## Credit rationale

UNCredited animtexture.cpp 1–241 (+241). The file ends at 241. A full tally line scan shows no overlap.
