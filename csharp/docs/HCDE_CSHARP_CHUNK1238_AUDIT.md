# Chunk 1238 — TEXTUREx composite, blend map, and true-color downconvert from multipatchtexture.cpp (1-345).

## Scope

TEXTUREx composite, blend map, and true-color downconvert from multipatchtexture.cpp (1-345).

## Native ranges

- `src/common/textures/formats/multipatchtexture.cpp` **FMultiPatchTexture::CreatePalettedPixels** 1–345 (345 lines)

**Credited native LOC:** 345 (tally `nativeLoc` 345).

## Managed

`VmMultiPatch1238Slice.cs`
Managed LOC (tally): 750.
Conversion commit: `2ca13b89550691c44d0753b2471f58b45b461462`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Constructor and palette vote (**37–60**) | yes |
| Sky remap gate (**73–81**) | yes |
| Blend map (**89–135**) | yes |
| Paletted block copy (**143–184**) | yes |
| Paletted composite and downconvert (**192–259**) | yes |
| True-color copy and return (**269–320**) | yes |
| Precache forwarding (**328–345**) | yes |
| Band hull (**1–345**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| none | the band is the whole file |

A luminance request on a game-palette texture stays paletted even when a part op is not copy. The same request on a texture that misses the game palette builds true color even when every op is copy. noremap0 survives only for a non-textual game-palette texture, and a non-zero style then drops the blend map. The copy return starts at -1 and a positive part result does not replace it. A transparent source is skipped, while overwrite still stores the color and an alpha of 0. The downconvert walks the finished bitmap in column order and will not replace a non-zero index.

| Managed (`VmMultiPatch1238Slice.cs`) | Native (`multipatchtexture.cpp`) |
|--------------------------------------|-------------------------------|
| Composite, blend, copy, and precache | **1–345** |
| Band hull (exclusive slice) | **1–345** |

### Chunk **1238** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **345** |
| **Net headline** | **+345** vs post-**1237** |


**Post-review ratio:** 750/345 managed/native ≈ 217% (src file LOC at HEAD; see tally).

## Tests

- `VmMultiPatch1238SliceTests.PaletteVote_CompositesPostsAndClearsStyleMaps`
- `VmMultiPatch1238SliceTests.TrueColor_BlendsDownconvertsAndKeepsUnknownReturn`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1238 --to 1238` (0 hard / 0 soft).

## Seams

The constructor copies every part, then sets the game-palette flag only when the texture is not complex and every part uses the game palette. An early miss leaves the flag clear. SupportRemap0 is false for a textual definition or a game-palette texture, and otherwise requires every part to support it. An empty non-palette texture therefore supports it. GetBlendMap returns the ice remap when the blend dword is 1. Dwords 33 through 36 select the four special-map seams, and 2 through 32 select the desaturate seams (index xor 0x20+slot). Any other non-zero alpha fills a work table through the slice Pick seam, (r*3+g*5+b*7)&255, using base channels b=i, g=(i+1)&255, r=(i+2)&255. Alpha 0 that is not one of those indexes returns null and the part is copied raw. CopyToBlock reads column-major pixels with step height,1 and uses the bitmap clip. A non-zero style drops the translation after the blend map is chosen. Index 0 is not written, so a translation can store 0 and a later part can paint that pixel. CreatePalettedPixels forces the paletted path for luminance when the texture uses the game palette, even if a part op is not copy, and forces the true-color path for luminance when it does not. Other conversions scan ops only when the texture is not complex. noremap0 becomes normal when the texture is textual or not a game-palette texture. The true-color path calls CopyPixels with normal and frame 0, then writes a column-major index from RGBToPalette only where the paletted byte is still 0 and the BGRA alpha is non-zero. Luminance uses (r*77+g*143+b*37)>>8. Other conversions use the RGB256 seam ((r>>2)*3+(g>>2)*5+(b>>2)*7)&255. CopyPixels leaves the running return at -1 for every part result, including a positive one. Alpha 255 sets BLEND_MODULATE and blendcolor channel*65536/255. Any other non-zero alpha sets BLEND_OVERLAY. A zero-alpha blend dword other than 0 is passed through as the blend enum. Child bitmaps are the caller RGBA, or the part translation palette expanded with index 0 skipped when its alpha is 0. The true-color ice table is the 16-entry bitmap.cpp palette. True-color desaturate uses gray (r*77+g*143+b*36)>>8. A special blend past the four-entry seam is a logged fault and does not write. CollectForPrecache records this image with the incoming flag, then upgrades the flag for children when the texture is complex or any op is not copy.

## Credit rationale

UNCredited multipatchtexture.cpp 1-345 (+345). The file ends at 345. A full tally line scan shows no overlap.
