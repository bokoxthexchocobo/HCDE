# Chunk 1306 — Texture collection, sprite translations, and the model vertex buffers from hw_precache.cpp (1-330).

## Scope

Texture collection, sprite translations, and the model vertex buffers from hw_precache.cpp (1-330).

## Native ranges

- `src/rendering/hwrenderer/hw_precache.cpp` **hw_PrecacheTexture** 1–330 (330 lines)

**Credited native LOC:** 330 (tally `nativeLoc` 330).

## Managed

`VmHwPrecache1306Slice.cs`
Managed LOC (tally): 323.
Conversion commit: `4de6b2a70c2b1e95236bcbed03bbf1d14c6995a3`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| PrecacheTexture and PrecacheSprite (**43–80**) | yes |
| Texture collection and skybox faces (**89–130**) | yes |
| Actor states, model skins, and sprite frames (**132–195**) | yes |
| Unused models, material marks, and CleanUnused (**199–258**) | yes |
| Image register, precache, and model buffers (**260–329**) | yes |
| Band hull (**1–330**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| AddSkins body | virtual callee on the model; this band passes the surface-skin tail or null |
| shouldUpscale | one-line mask and flag check used by this band |

The collect loop starts at texture 1. A texture is kept when it is valid, has an image, is not a font character, and its use type is below Special. Each of its layers is marked and inserted once. A sky hit expands six faces onto the flat bit even for a font character, and a missing game texture throws. Index 0 is not expanded there. Actor states insert the remap index, or 0 when the remap is missing, then look up a model frame with the dropped flag clear. A valid skin sets the flat bit and skips AddSkins. An invalid skin with a model id calls AddSkins with the tail at slot times 32, or null when that index is past the array, and then marks the model. Sprite frames are walked from the last sprite toward the first, so a shared picture keeps the lower sprite. Pictures at index 0 or below are skipped. Unused models lose their vertex buffer before precaching starts. The mark walk includes index 0 and runs even when the precache cvar is off. Wall marks pass the upscale bit only. Sprite marks pass 1 and true, ignoring the upscale bit computed beside that call. CleanUnused follows insertion order. When the cvar is on, a wall image is registered only when the hardware texture for the upscale flag (0 or 1, not the expand bit) is missing, and a sprite image is registered when translation 0 is present. PrecacheSprite does use the expand bit and the upscale bit. A null material does not precache. Used models are built on one renderer created with -1, then that renderer is deleted. The notify line prints the elapsed time to three decimals.

| Managed (`VmHwPrecache1306Slice.cs`) | Native (`hw_precache.cpp`) |
|--------------------------------------|----------------------------|
| Collect, sprites, and model buffers | **1–330** |
| Band hull (exclusive slice) | **1–330** |

### Chunk **1306** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **330** |
| **Net headline** | **+330** vs post-**1305** |


**Post-review ratio:** 323/330 managed/native ≈ 98% (src file LOC at HEAD; see tally).

## Tests

- `VmHwPrecache1306SliceTests.Collect_ExpandsSkyFacesAndPrecachesTranslatedSprites`
- `VmHwPrecache1306SliceTests.Precache_StopsBeforeImagesWhenTheCvarIsOff`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1306 --to 1306` (0 hard / 0 soft).

## Seams

AddSkins is the model callee. This band chooses a null surface-skin pointer when i times 32 is past the array, and otherwise passes the tail. ValidateTexture, hardware-texture lookup, and image registration are host calls. The upscale helper returns the texture flag only when the mask bit is set.

## Credit rationale

UNCredited hw_precache.cpp 1-330 (+330). A full tally line scan shows no overlap. The file ends at line 330.
