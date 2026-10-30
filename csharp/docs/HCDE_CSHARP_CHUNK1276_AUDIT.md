# Chunk 1276 — Hardware 2D command walk, scissor clip, and the shape-buffer reset from hw_draw2d.cpp (1-255).

## Scope

Hardware 2D command walk, scissor clip, and the shape-buffer reset from hw_draw2d.cpp (1-255).

## Native ranges

- `src/common/rendering/hwrenderer/hw_draw2d.cpp` **Draw2D** 1–255 (255 lines)

**Credited native LOC:** 255 (tally `nativeLoc` 255).

## Managed

`VmDraw2d1276Slice.cs`
Managed LOC (tally): 369.
Conversion commit: `12dc11d1095e35492615b0c77fa2c44943c828ea`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Viewport wrapper (**44–48**) | yes |
| Clock and setup (**52–64**) | yes |
| Empty return (**70–74**) | yes |
| First-pass swap and upload (**76–87**) | yes |
| Special commands (**91–105**) | yes |
| Style, fog, and scissor (**108–132**) | yes |
| Colormap and light (**134–145**) | yes |
| Model matrix (**147–168**) | yes |
| Texture, canvas, and burn (**170–194**) | yes |
| Shape buffer and draw type (**196–231**) | yes |
| Command tail (**232–237**) | yes |
| Pass reset (**240–254**) | yes |
| Band hull (**1–255**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| Vertex upload | host buffer |
| Set2D viewpoint | host screen |

The viewport wrapper forwards the screen rectangle. Setup clears the stencil, turns depth and multisampling off, and applies line smoothing from the cvar. The mip cache is the 2D mip cvar, and a missing anisotropic callback leaves that cvar in place. A present callback that reports filtering disabled forces the cache off, and a false cvar never calls the callback. An empty command list unclocks before the channel swap and leaves the first pass set. The first pass swaps red and blue on every vertex and later passes leave those bytes. Special stencil, set, and clear commands skip the per-command reset. Any other special value does the same. A scissor command maps the rectangle with half-away rounding, then adds a negative origin back into the size and clamps that origin to zero. A zero alpha on the first colormap keeps the draw mode. A nonzero alpha switches the mode to the fixed colormap and sets both object colors. Fog distance zero stores a negative zero factor. Indexed commands set the soft light before the shared light parameters. The model matrix writes the 2x2 cells in column order and the translation into the fourth column. A null use type, and a null texture, disable texturing before the canvas flip and the burn effect. Use types at or above Special take no upscale flag, a font character takes the font flag, and every other valid use takes the texture flag. Wrapping clamps to none. Without wrapping, the cached mip selects the mip clamp or the no-mip clamp. A hardware canvas loads identity, scales Y by negative one, then translates Y by one. The burn effect is set only for a valid texture. A shape buffer draws triangles from index zero, restores the uploaded buffer, and clears the buffer list only when the counter matches the last command and the buffer index is greater than zero. A matching counter with index zero only stores last command as negative one. Uploaded-once is cleared on every shape command. Lines and points use their draw types. Every other type, including the rotate-sprite value, draws indexed triangles. Each normal command then restores white, clears the second object color and the add color, and disables both matrices and the effect. The pass then clears the scissor, selects translucent style, binds the screen buffer, and clears the first-pass flag before the unclock.

| Managed (`VmDraw2d1276Slice.cs`) | Native (`hw_draw2d.cpp`) |
|----------------------------------|--------------------------|
| Command walk, clip, and shape reset | **1–255** |
| Band hull (exclusive slice) | **1–255** |

### Chunk **1276** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **255** |
| **Net headline** | **+255** vs post-**1275** |


**Post-review ratio:** 369/255 managed/native ≈ 145% (src file LOC at HEAD; see tally).

## Tests

- `VmDraw2d1276SliceTests.Empty_LeavesTheFirstPassAndSkipsTheSwap`
- `VmDraw2d1276SliceTests.Commands_ClipTheScissorAndResetTheShapeBuffer`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1276 --to 1276` (0 hard / 0 soft).

## Seams

Vertex upload, the 2D viewpoint, and the screen buffer are host inputs. The slice keeps the first-pass channel swap, the mip-cache short-circuit, the scissor clip, and the shape reset.

## Credit rationale

UNCredited hw_draw2d.cpp 1-255 (+255). A full tally line scan shows no overlap. The file ends at line 255.
