# Chunk 1301 — Automatic uniforms, the scene blit, present gamma, and the shadow-map transition from vk_postprocess.cpp (1-360).

## Scope

Automatic uniforms, the scene blit, present gamma, and the shadow-map transition from vk_postprocess.cpp (1-360).

## Native ranges

- `src/common/rendering/vulkan/renderer/vk_postprocess.cpp` **VkPostprocess::PostProcessScene** 1–360 (360 lines)

**Credited native LOC:** 360 (tally `nativeLoc` 360).

## Managed

`VmVkPostprocess1301Slice.cs`
Managed LOC (tally): 477.
Conversion commit: `ba7770ed2bc45edc9c4c78fc0e8fcc653d9fe0b1`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Automatic uniforms (**50–62**) | yes |
| Active target and scene pass (**64–108**) | yes |
| Scene blit and resolve (**110–165**) | yes |
| Scene image transition (**167–177**) | yes |
| Current-image blit and copy (**179–247**) | yes |
| Present gamma (**249–309**) | yes |
| Ambient, blur, tonemap, shadow, and the eye call (**311–360**) | yes |
| Band hull (**1–360**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| GetDeltaTime | host clock, default current argument false |
| ViewportLinearScale | host bool |
| Shadow-map collect flag | host bool |

The constructor creates a 16-byte uniform buffer, usage uniform, memory CPU-to-GPU, named AutomaticUniformsBuffer. The destructor body adds nothing. Destroying the object releases that buffer once. Post-process maps offset 0 for 16 bytes: delta time, frame time divided by 1000, world timer divided by the tic rate, and a zero pad. Pass 1 runs, then the active target, then the callback, then pass 2. The pipeline index stays put. The active target transitions the current pipeline image to color and the pipeline depth to depth-stencil, both with a defined source, on the draw queue. The render target uses the buffer width and height, RGBA16F, and one sample. The scene blit ends the pass, forces pipeline image 0, and transitions scene color to transfer-source (defined) and that pipeline image to transfer-dest (undefined). Any sample count other than one, including zero, resolves with the scene-color extent on both ends. One sample blits nearest, and both extents are still the scene-color size. The scene transition writes color, fog, and normal to color-attachment and depth to depth-stencil, all with the caller’s undefined flag, and it does not end the pass. Blit-current ends the pass, uses each image’s own size, nearest, then moves only the destination to the final layout with a defined source. The default final layout is shader-read. Copy-current ends the pass first. A width or height mismatch returns before any transition. A match copies the source extent and then returns the source to color-attachment and the destination to the final layout, both defined. Present runs the screen shader only when this is not a screenshot, and that run is before the uniforms. Gamma off writes inverse gamma, contrast, saturation, and white point as 1 and black point as 0, and it leaves the gray formula unset. Gamma on clamps gamma to 0.1–4 and stores its reciprocal, contrast to 0.1–3, saturation to -15–15, black point to 0–1, and white point to 0–5, then stores the integer saturation formula. Color scale is 255 when dither bits are -1, otherwise one shifted by that count, minus one. A screenshot keeps both scale axes positive and the offset at zero and targets the next image. Otherwise the Y scale is negated, the Y offset is 1, and the target is the swapchain. HDR mode is 1 only when gamma is applied, the color space is extended sRGB linear, and this is not a screenshot. The current input is linear when the host scale flag is set and nearest otherwise. The dither input is nearest and repeating. Window-position parity is never written. Ambient occlusion records the scene size and then transitions the scene with a defined source. Blur runs once per eye. It records an eye advance with the eye count when more than one eye remains, so one eye and zero eyes record none, and three eyes record two advances of 3. The pipeline image is unchanged. The tonemap clear only bumps that counter. The shadow update does nothing when the collect flag is off. When it is on, the shadow image goes to shader-read with a defined source and the finish call follows the transition.

| Managed (`VmVkPostprocess1301Slice.cs`) | Native (`vk_postprocess.cpp`) |
|----------------------------------------|------------------------------|
| Uniforms, blit, present, and shadow | **1–360** |
| Band hull (exclusive slice) | **1–360** |

### Chunk **1301** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **360** |
| **Net headline** | **+360** vs post-**1300** |


**Post-review ratio:** 477/360 managed/native ≈ 132% (src file LOC at HEAD; see tally).

## Tests

- `VmVkPostprocess1301SliceTests.Scene_ResolvesAboveOneSampleAndSkipsAMismatchedCopy`
- `VmVkPostprocess1301SliceTests.Present_ClampsGammaAndAdvancesEveryEyeButTheLast`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1301 --to 1301` (0 hard / 0 soft).

## Seams

GetDeltaTime, ViewportLinearScale, and the shadow-map collect flag are host inputs. The uniform buffer release is the member destructor. NextEye records the call and does not change the pipeline image.

## Credit rationale

UNCredited vk_postprocess.cpp 1-360 (+360). A full tally line scan shows no overlap. The file ends at line 360.
