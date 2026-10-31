# Chunk 1308 — Renderer setup, the offscreen framebuffer, and the save-picture buffers from gl_renderer.cpp (1-172).

## Scope

Renderer setup, the offscreen framebuffer, and the save-picture buffers from gl_renderer.cpp (1-172).

## Native ranges

- `src/common/rendering/gl/gl_renderer.cpp` **FGLRenderer::Initialize** 1–172 (172 lines)

**Credited native LOC:** 172 (tally `nativeLoc` 172).

## Managed

`VmGlRenderer1308Slice.cs`
Managed LOC (tally): 232.
Conversion commit: `a2e4ff6722ace90613cb23908bb0eb877acc545a`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Constructor (**66–69**) | yes |
| Initialize buffers, shaders, and the VAO (**71–92**) | yes |
| Destructor flush and deletes (**94–113**) | yes |
| StartOffscreen (**121–131**) | yes |
| EndOffscreen (**139–142**) | yes |
| BindToFrameBuffer (**150–158**) | yes |
| BeginFrame (**166–170**) | yes |
| Band hull (**1–172**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| OpenGL entry points | host records the call; this band decides when and with which arguments |
| Hardware texture bodies | callee on the texture; this band passes zeros and the texture size |

The constructor stores the framebuffer host. Initialize allocates the screen and save buffer objects, points the current buffers at the screen object, and creates the present, checker, column, row, and shadow shaders. It generates a vertex array, binds it, and labels it FGLRenderer.mVAOID. It then stores framebuffer ids of 0 and creates the shader and sampler managers. The width and height arguments are unused. Destroy flushes models and textures first. It deletes a live shader manager, a live sampler manager, a nonzero framebuffer, and a nonzero vertex array. The vertex array is unbound before it is deleted. It deletes the current buffer object and the save buffer object, then each live present shader. StartOffscreen generates the framebuffer only while its id is 0, queries the current binding, binds the new id, and labels OffscreenFB only on that first bind. Later calls still query and bind, and the method returns true. EndOffscreen binds the queried id. BindToFrameBuffer requires the hardware texture from slot 0 and flags 0, binds or creates it with zeros, unbinds unit 0, clears the last material, and binds the texture to the framebuffer at its width and height. BeginFrame sets up the screen buffers from the screen and scene viewports and the save buffers at 216 by 162.

| Managed (`VmGlRenderer1308Slice.cs`) | Native (`gl_renderer.cpp`) |
|--------------------------------------|----------------------------|
| Setup, offscreen, and save buffers | **1–172** |
| Band hull (exclusive slice) | **1–172** |

### Chunk **1308** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **172** |
| **Net headline** | **+172** vs post-**1307** |


**Post-review ratio:** 232/172 managed/native ≈ 135% (src file LOC at HEAD; see tally).

## Tests

- `VmGlRenderer1308SliceTests.Initialize_LabelsTheVaoAndBindsOffscreenOnce`
- `VmGlRenderer1308SliceTests.Bind_UsesTheBaseLayerAndDestroyBeforeInitOnlyFlushes`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1308 --to 1308` (0 hard / 0 soft).

## Seams

OpenGL object ids come from the host. FlushModels and TexMan.FlushAll are host calls at the start of destroy. The hardware texture bind, the static unbind, and ClearLastMaterial are host calls. Initialize accepts width and height and does not size the buffers.

## Credit rationale

UNCredited gl_renderer.cpp 1-172 (+172). A full tally line scan shows no overlap. The file ends at line 172.
