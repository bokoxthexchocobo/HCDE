# Chunk 1302 — Texture upload, the wipe image, and material descriptor sets from vk_hwtexture.cpp (1-386).

## Scope

Texture upload, the wipe image, and material descriptor sets from vk_hwtexture.cpp (1-386).

## Native ranges

- `src/common/rendering/vulkan/textures/vk_hwtexture.cpp` **VkHardwareTexture::CreateTexture** 1–386 (386 lines)

**Credited native LOC:** 386 (tally `nativeLoc` 386).

## Managed

`VmVkHwTexture1302Slice.cs`
Managed LOC (tally): 544.
Conversion commit: `76c3bafb68dbced079f5e56b7ddbe915690392b0`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Construct, destroy, and reset (**40–65**) | yes |
| Image and depth-stencil getters (**67–104**) | yes |
| Canvas and texel upload (**106–199**) | yes |
| Software buffer and wipe (**201–301**) | yes |
| Material descriptors (**305–386**) | yes |
| Band hull (**1–386**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| Palette translation token | host table |
| Scene depth format and allocation byte count | host device |
| Mipmap blit chain | already credited in vk_imagetransition.cpp |

The constructor stores the channel count and registers the texture. Destroy removes it only while the device is still attached, and the image members are released immediately rather than through the draw list. Reset does nothing when the device is gone. Otherwise it unmaps, then retires the post framebuffer, the render-pass framebuffers in key order, the depth-only view, the view, and the image onto the draw list, and it puts the aspect back to color and the layout back to undefined. GetImage creates once. A non-canvas image requests a buffer with the process-data flag added. Indexed data is one byte, R8, and has no mip chain, so the layout stays transfer-dest. Other data is four-byte BGRA and runs mip generation, which leaves shader-read. A hardware canvas skips the upload. HDR uses RGBA32 float and the rest uses RGBA8, with color-attachment and sampled usage, then a transfer-queue transition to shader-read with an undefined source. A zero or negative size throws before any staging buffer exists. The staging buffer is transfer-source and CPU-only. The copy uses the color aspect, one layer, and the image extent. The staging buffer is queued, and a transfer total strictly above 64 megabytes waits with finish false and upload true, which drops the transfer list. Depth-stencil is created once: one sample, depth-stencil usage, depth and stencil aspects, and a transfer-queue transition to the depth layout with an undefined source. AllocateBuffer resets when the size or the texel size differs, then creates a linear sampled image. Four bytes select BGRA and every other size selects R8. Memory is unknown, dedicated and mapped, host-visible and host-coherent, preferring device-local as well. The pitch is the allocated byte count divided by height and then by the texel size. The layout is general. Map returns the same pointer on the second call. The software CreateTexture ignores a null buffer and still returns zero. A buffer uses the stored texel size and ignores the texture unit and the name. Wipe stores four texels, BGRA, sampled and transfer-dest, GPU-only, under the caller’s name. A positive framebuffer size blits to shader-read. Otherwise the image is cleared to black with alpha one while it is transfer-dest, then moved to shader-read with a defined source. The material registers on construct and removes on destroy only while attached. Member descriptors that were not already queued die with the object. DeleteDescriptors queues them on the draw list and clears the cache, and it does nothing when detached. The cache key is the clamp mode after the canvas and warp rules, plus the remap. A luminosity translation (positive with bit 30 set) is its own remap. Anything else asks the palette for the type in the high byte and the low 16 bits. A software canvas forces no-filter even when the texture is also a hardware canvas. A hardware canvas forces the camera clamp. A warped texture, or a user shader at index 15 or above, forces no clamp when the requested mode is XY or below, and it leaves XY-no-mip alone. Layer 0 is sampled with the state translation. Extra indexed layers are 1 and 2, fetched with that same translation and sampled with translation 0, and the layer count becomes 3. Other materials sample every layer, with extra layers at translation 0. Slots are the max of the layer count and 11. Dummy bindings then fill through binding 10 with the null view and shader-read, so an indexed material with 12 layers allocates 12 slots and leaves binding 11 unwritten.

| Managed (`VmVkHwTexture1302Slice.cs`) | Native (`vk_hwtexture.cpp`) |
|--------------------------------------|----------------------------|
| Upload, wipe, and descriptors | **1–386** |
| Band hull (exclusive slice) | **1–386** |

### Chunk **1302** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **386** |
| **Net headline** | **+386** vs post-**1301** |


**Post-review ratio:** 544/386 managed/native ≈ 141% (src file LOC at HEAD; see tally).

## Tests

- `VmVkHwTexture1302SliceTests.Upload_WaitsAboveSixtyFourMegabytesAndSkipsANullBuffer`
- `VmVkHwTexture1302SliceTests.Descriptors_CacheTheClampAndLeaveTheTwelfthBindingEmpty`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1302 --to 1302` (0 hard / 0 soft).

## Seams

Palette tokens, the scene depth format, and the linear allocation size are host services. Mip generation is the existing image transition and leaves the shader-read layout.

## Credit rationale

UNCredited vk_hwtexture.cpp 1-386 (+386). A full tally line scan shows no overlap. The file ends at line 386.
