# Chunk 1280 — Scene post process, stereo flush, gamma present, and letterbox clear from gl_postprocess.cpp (1-277).

## Scope

Scene post process, stereo flush, gamma present, and letterbox clear from gl_postprocess.cpp (1-277).

## Native ranges

- `src/common/rendering/gl/gl_postprocess.cpp` **FGLRenderer::PostProcessScene** 1–277 (277 lines)

**Credited native LOC:** 277 (tally `nativeLoc` 277).

## Managed

`VmPostProcess1280Slice.cs`
Managed LOC (tally): 231.
Conversion commit: `213297b4fc6a185e3247085c3f16b5f814717f0a`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Screen quad (**52–57**) | yes |
| Scene passes (**59–74**) | yes |
| Ambient occlusion (**82–89**) | yes |
| Eye blur (**91–105**) | yes |
| Tonemap clear (**107–110**) | yes |
| Flush (**118–144**) | yes |
| Copy to backbuffer (**152–179**) | yes |
| Present texture (**181–234**) | yes |
| Letterbox clear (**242–275**) | yes |
| Band hull (**1–277**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| Draw arrays and texture binds | host GL |
| Post-process shader objects | host pipeline |

The scene pass stores the delta as a float, the frame time divided by one thousand as a float, and the world timer divided by thirty-five as a float. It runs the first pass, binds the current framebuffer, calls the callback only when one was passed, then runs the second pass. Ambient occlusion logs the passed factor with the scene size. Blur walks every eye and advances to the next eye only while more than one eye remains. A single eye copies to the backbuffer with gamma applied. Any other eye count, including zero, draws two-dimensional content per eye, advances under the same remaining-eye test, clears the two-dimensional queue, and presents stereo. Copy draws two-dimensional content, clears that queue, and runs the screen shader. A non-null bounds rect is used as the viewport and skips the border clear. A null bounds rect clears the borders and then uses the letterbox. Present sets the viewport, binds dither, and chooses a linear filter only when linear scale is on. Gamma, contrast, saturation, black, and white stay at the identity values, and the gray formula is left unchanged, when gamma is not applied or hardware gamma is active. Otherwise inverse gamma is one over the gamma clamped from one tenth to four, and contrast, saturation, black, and white are clamped to their own ranges before the gray formula is stored. High dynamic range color scale is used only when that mode is active and the framebuffer is fullscreen. A bits-per-channel value of minus one selects 1023 in that mode and 255 otherwise. Any other bits-per-channel value selects one shifted by that count, minus one. Scale is the screen size divided by the buffer size as floats, and the offset stays zero. A zero client width or height returns before any border scissor. Otherwise the letterbox top, bottom, left, and right bars are scissored only when that bar's size is greater than zero.

| Managed (`VmPostProcess1280Slice.cs`) | Native (`gl_postprocess.cpp`) |
|---------------------------------------|--------------------------------|
| Passes, flush, and present | **1–277** |
| Band hull (exclusive slice) | **1–277** |

### Chunk **1280** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **277** |
| **Net headline** | **+277** vs post-**1279** |


**Post-review ratio:** 231/277 managed/native ≈ 83% (src file LOC at HEAD; see tally).

## Tests

- `VmPostProcess1280SliceTests.Scene_PassesTheCallbackBetweenTheTwoPasses`
- `VmPostProcess1280SliceTests.Present_ClampsGammaAndSkipsBordersWhenTheClientIsEmpty`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1280 --to 1280` (0 hard / 0 soft).

## Seams

The scene buffers, bloom callback, eye count, and present shader are host inputs. The slice keeps the pass order, the stereo flush, the gamma identity path, and the letterbox scissor tests.

## Credit rationale

UNCredited gl_postprocess.cpp 1-277 (+277). A full tally line scan shows no overlap. The file ends at line 277.
