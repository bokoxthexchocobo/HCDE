# Chunk 1320 — Slope normal, the fixed-point steps, and the span light from r_slopeplane.cpp (1-283).

## Scope

Slope normal, the fixed-point steps, and the span light from r_slopeplane.cpp (1-283).

## Native ranges

- `src/rendering/swrenderer/plane/r_slopeplane.cpp` **RenderSlopePlane::Render** 1–283 (283 lines)

**Credited native LOC:** 283 (tally `nativeLoc` 283).

## Managed

`VmSlopePlane1320Slice.cs`
Managed LOC (tally): 295.
Conversion commit: `20b6e0b7ee82d39a37b1b55a0d3b247b8e0b4b38`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Slope normal and the scale table (**64–110**) | yes |
| Fixed-point origin and the U/V axes (**112–166**) | yes |
| Mirror, fog, shade, and the 1-wide zero (**168–206**) | yes |
| Row lights and the tilted span (**208–282**) | yes |
| Band hull (**1–283**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| The tilted pixel drawer | the host records the draw arguments |
| The top and bottom closure walk | that walk lives in `r_planerenderer.cpp` |

A non-positive alpha returns before the normal is stored. The view-space normal is the camera transform of the plane normal, and a mirror negates its X. Texture scales divide by the physical scale, then by the inverse power-of-two table. The fixed origin uses the round-half-even shift `32` minus the texture bits. The U and V axes stay in the XZ plane until the slope heights fill Y. Only U and V are multiplied by 2^32. A mirror then negates all three X components. Fog follows a fade target, a colormap fade, or the fade-table flag. A positive plane C negates the light. The drawn style is opaque even when the caller passes additive or masked. Shade is on only when the fixed light is negative and there is no fixed colormap. A texture that is one unit wide or tall zeros that axis after the mirror. An inactive light is skipped. An attenuated light stores a negative radius. The span step is the screen-space delta from the first column to the last.

| Managed (`VmSlopePlane1320Slice.cs`) | Native (`r_slopeplane.cpp`) |
|--------------------------------------|-----------------------------|
| Slope normal, fixed steps, and the span light | **1–283** |
| Band hull (exclusive slice) | **1–283** |

### Chunk **1320** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **283** |
| **Net headline** | **+283** vs post-**1319** |


**Post-review ratio:** 295/283 managed/native ≈ 104% (src file LOC at HEAD; see tally).

## Tests

- `VmSlopePlane1320SliceTests.Render_SkipsZeroAlphaAndFixesTheOpaqueSteps`
- `VmSlopePlane1320SliceTests.RenderLine_PacksActiveLightsAndSkipsTheRest`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1320 --to 1320` (0 hard / 0 soft).

## Seams

Closed span rows come from the plane. World-to-view and the screen position are the viewport host. The tilted span drawer records the arguments.

## Credit rationale

UNCredited r_slopeplane.cpp 1-283 (+283). A full tally line scan shows no overlap. The file ends at line 283.
