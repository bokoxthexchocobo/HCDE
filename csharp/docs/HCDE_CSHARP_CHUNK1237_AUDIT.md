# Chunk 1237 — Doom patch probe, column posts, and the bad-patch scan from patchtexture.cpp (1-311).

## Scope

Doom patch probe, column posts, and the bad-patch scan from patchtexture.cpp (1-311).

## Native ranges

- `src/common/textures/formats/patchtexture.cpp` **FPatchTexture::CreatePalettedPixels** 1–311 (311 lines)

**Credited native LOC:** 311 (tally `nativeLoc` 311).

## Managed

`VmPatchTexture1237Slice.cs`
Managed LOC (tally): 436.
Conversion commit: `90971624eb22ecbfd2b51ac2ae0b9835c5ee42a8`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Patch and post headers (**32–48**) | yes |
| Patch class (**58–68**) | yes |
| Column-directory probe (**76–118**) | yes |
| Try-create gate and construct (**126–161**) | yes |
| Paletted posts and the bad draw (**169–258**) | yes |
| Alpha versus base copy (**266–270**) | yes |
| Bad-patch scan (**278–311**) | yes |
| Band hull (**1–311**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| none | the band is the whole file |

A column that starts exactly at the end of the directory makes the patch eligible. A later column at or past the lump still rejects it. Empty posts update top and do not paint. A deep post overwrites an earlier pixel in the same column. The sky remap clears only index 0 of the game remap. Luminance asked of a non-alpha copy is drawn as normal. The bad scan uses the header width, while the draw uses the image width and height. A 300-tall image whose header is a 256-tall bad patch paints the pad and the 0xFF before the fault stops the column.

| Managed (`VmPatchTexture1237Slice.cs`) | Native (`patchtexture.cpp`) |
|----------------------------------------|----------------------------|
| Probe, posts, copy, and the bad scan | **1–311** |
| Band hull (exclusive slice) | **1–311** |

### Chunk **1237** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **311** |
| **Net headline** | **+311** vs post-**1236** |


**Post-review ratio:** 436/311 managed/native ≈ 140% (src file LOC at HEAD; see tally).

## Tests

- `VmPatchTexture1237SliceTests.Probe_RejectsTheGapAndDrawsClippedPosts`
- `VmPatchTexture1237SliceTests.BadScan_ZerosOffsetsAndPaintsRawColumns`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1237 --to 1237` (0 hard / 0 soft).

## Seams

CheckIfPatch rejects a lump shorter than 13 bytes. Width and height in that probe are signed. TryCreate reads width and height as unsigned and rejects them unless both are in 1..2048, width is less than the length divided by 4, and both offsets have absolute value under 4096. A column offset equal to width*4+8 clears the leading gap even when that offset is past the lump; any other offset at or past the lump rejects the patch. The alpha bit is VmMd5Check1214Slice.CheckPatchForAlpha and is stored only after the gap check passes. A column directory that runs past the lump is a logged fault. CreatePalettedPixels stops a column at length-3, so a post that begins on the last three bytes is not read. topdelta 0xFF ends the column. A later post whose topdelta is less than or equal to the running top adds that delta instead of replacing it. Posts are clipped to the image height, and a clipped length of 0 is not drawn. The source is column-major. Index 0 stays 0 in the buffer until a post writes it. GetRemap selects GrayscaleMap for luminance, GrayRamp for luminance alpha, GrayMap for alpha, and Remap otherwise. noremap0 copies Remap and forces index 0 to 0 only when that table was selected. CopyPixels on a non-alpha image forces luminance back to normal and blits with the base palette; an alpha image ignores the conversion, draws with normal, and blits with the grayscale palette. Palette alpha 0 leaves the destination byte, which starts at 0. A pixel read past the lump is a logged fault and the rest of that draw stops. DetectBadPatches returns before scanning when the lump is shorter than Width*Height/2. It marks the image bad only when the header height is 256 and every header column is a single post of topdelta 0 and length 0 followed by 0xFF at column+260. That path zeros the offsets, clears the mask, and the bad draw copies Height bytes from column+3. A short header or a column that cannot hold that terminator is a logged fault and is not bad. SupportRemap0 is the inverse of the bad flag.

## Credit rationale

UNCredited patchtexture.cpp 1-311 (+311). The file ends at 311. A full tally line scan shows no overlap.
