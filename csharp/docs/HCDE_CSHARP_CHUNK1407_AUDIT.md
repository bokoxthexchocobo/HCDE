# Chunk 1407 — Span drawer construction, texture and style setup, and the four span draws from r_spandrawer.cpp (1-150).

## Scope

Span drawer construction, texture and style setup, and the four span draws from r_spandrawer.cpp (1-150).

## Native ranges

- `src/rendering/swrenderer/viewport/r_spandrawer.cpp` **SpanDrawerArgs** 1–150 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmSpanDrawer1407Slice.cs`
Managed LOC (tally): 215.
Conversion commit: `508b63c2f053315b50885fdd122b48d98834b4b4`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| SpanDrawerArgs constructor (**26–29**) | yes |
| SetTexture (**31–48**) | yes |
| SetStyle (**50–120**) | yes |
| DrawSpan (**122–125**) | yes |
| DrawTiltedSpan (**127–133**) | yes |
| DrawFogBoundaryLine (**135–141**) | yes |
| DrawColoredSpan (**143–149**) | yes |
| Band hull (**1–150**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| Col2RGB8 and Col2RGB8_LessPrecision | callee tables; the port stores the table and the index |
| CameraLight and LightLevelToShade | callees |
| DefaultRenderStyle and the pixel getters | callees |

The constructor selects DrawSpan. SetTexture copies the physical size and the bit counts, then decrements a bit count once when one shifted bit is wider than that axis. BGRA reads GetPixelsBgra. Any other target reads GetPixels with DefaultRenderStyle. A texture is mipmapped only when the texture says so and both axes are greater than 1. SetStyle selects the masked or plain span, and the translucent or add-clamp variant when alpha is below 65536 or the blend is additive. Translucent blends use Col2RGB8. Additive blends use the less-precision table, source alpha as given, and destination alpha FRACUNIT. An opaque pick changes the function and leaves the previous blend values in place. A fixed light level of zero or more selects FullNormalLight when r_fullbrightignoresectorcolor is set, otherwise the passed colormap, and sets light 0 with the fixed shade. A fixed colormap otherwise replaces the colormap and sets light 0 shade 0. The remaining path stores the passed colormap and does not change the light. DrawSpan calls the current function with the current viewport. The tilted, fog, and colored draws store the destination first. The tilted draw then asks LightLevelToShade and forwards that shade with the plane arguments.

| Managed (`VmSpanDrawer1407Slice.cs`) | Native (`r_spandrawer.cpp`) |
|--------------------------------------|----------------------------|
| Span drawer args | **1–150** |
| Band hull (exclusive slice) | **1–150** |

### Chunk **1407** gross vs net

| | Value |
|--|------:|
| Row nativeLoc | **150** |
| **Net headline** | **+150** vs the prior chunk |


**Post-review ratio:** 215/150 managed/native ≈ 143% (src file LOC at HEAD; see tally).

## Tests

- `VmSpanDrawer1407SliceTests.StylePicksTheSpanAndTextureShrinksOversizedBits`
- `VmSpanDrawer1407SliceTests.DrawsSetDestBeforeTheShadeAndTheNamedDrawer`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1407 --to 1407` (0 hard / 0 soft).

## Seams

Blend tables, CameraLight, and LightLevelToShade stay callees. The file ends at line 150.

## Credit rationale

UNCredited r_spandrawer.cpp 1-150 (+150). After chunk 1406 this is the last whole-function hole of at least 150 lines. codegen.cpp 3778-3933 and p_user.cpp 2395-2544 stay uncredited because the whole functions left in each gap are under 150 lines. The band is the whole file and is not sliced.
