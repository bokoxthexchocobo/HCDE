# Chunk 1298 — Sample count, pipeline images, and the output framebuffer from vk_renderbuffers.cpp (1-305).

## Scope

Sample count, pipeline images, and the output framebuffer from vk_renderbuffers.cpp (1-305).

## Native ranges

- `src/common/rendering/vulkan/textures/vk_renderbuffers.cpp` **VkRenderBuffers::BeginFrame** 1–305 (305 lines)

**Credited native LOC:** 305 (tally `nativeLoc` 305).

## Managed

`VmVkRenderBuffers1298Slice.cs`
Managed LOC (tally): 304.
Conversion commit: `53c86c79c133f6e418e4fe301bdb49992b3ab97a`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Sample count and frame (**42–85**) | yes |
| Pipeline images (**87–147**) | yes |
| Scene images (**149–253**) | yes |
| Output framebuffer (**255–305**) | yes |
| Band hull (**1–305**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| Sample mask | host device limits |
| Format support | host device |
| Swapchain size | host framebuffer |

A non-positive sample request returns the one-sample bit without reading the device mask. The walk keeps the highest supported bit at or below the clamped request. The stored sample count starts at one. A frame whose size and sample bit are unchanged does not wait, and a scene-size-only change stores the new scene size without recreating images. A size change waits, resets, and rebuilds both pipeline and scene. A sample-only change waits and rebuilds the scene, and leaves the pipeline images in place. Pipeline color and depth images are one sample. Scene images use the chosen sample bit. Pipeline color includes both transfer directions. Scene color omits the transfer destination. Fog is RGBA8. Depth starts at the 24-bit format and falls back to the 32-bit format, and both failures are fatal after the format member has already changed. Normals start at the packed format, fall back to RGBA8 without a second support check, and do not fatal. Output transitions the texture to color attachment. The undefined flag is set only for the next pipeline image. Scene or pipeline stencil adds that depth image with a defined layout. An existing framebuffer is kept, including its pass and attachment count. A null texture uses the swapchain slot for the present index and does not transition. Destroy does not release the images.

| Managed (`VmVkRenderBuffers1298Slice.cs`) | Native (`vk_renderbuffers.cpp`) |
|------------------------------------------|-------------------------------|
| Samples, images, and output | **1–305** |
| Band hull (exclusive slice) | **1–305** |

### Chunk **1298** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **305** |
| **Net headline** | **+305** vs post-**1297** |


**Post-review ratio:** 304/305 managed/native ≈ 100% (src file LOC at HEAD; see tally).

## Tests

- `VmVkRenderBuffers1298SliceTests.Samples_LeaveThePipelineWhenOnlyTheCountChanges`
- `VmVkRenderBuffers1298SliceTests.Output_CachesTheFramebufferAndDepthFallbackFatals`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1298 --to 1298` (0 hard / 0 soft).

## Seams

The sample mask, format support, and swapchain size are host services. Command waits are counters.

## Credit rationale

UNCredited vk_renderbuffers.cpp 1-305 (+305). A full tally line scan shows no overlap. The file ends at line 305.
