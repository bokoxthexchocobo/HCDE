# Chunk 1319 — Flat plane steps, the span light, and the colored plane from r_flatplane.cpp (1-288).

## Scope

Flat plane steps, the span light, and the colored plane from r_flatplane.cpp (1-288).

## Native ranges

- `src/rendering/swrenderer/plane/r_flatplane.cpp` **RenderFlatPlane::Render** 1–288 (288 lines)

**Credited native LOC:** 288 (tally `nativeLoc` 288).

## Managed

`VmFlatPlane1319Slice.cs`
Managed LOC (tally): 271.
Conversion commit: `23a888cf6d3db83bdd91182ea244ec4b51411a86`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Flat setup, the yaw step, and the mirror flip (**60–147**) | yes |
| Span U/V, the LOD, and the distance light (**149–187**) | yes |
| Row lights and the span destination (**189–270**) | yes |
| Colored plane (**274–287**) | yes |
| Band hull (**1–288**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| The span pixel drawer | the host records the draw arguments |
| The top and bottom closure walk | that walk lives in `r_planerenderer.cpp` |

A non-positive alpha returns before the steps are stored. A zero plane angle keeps the view offset unrotated and does not add the base Y offset. A live angle rotates the view by the plane sine and cosine, then the scales apply. The yaw minus 90 degrees feeds the steps, a mirror negates both steps, and another quarter turn feeds the edge fractions. Equal left and right edges store a zero step scale. Fog follows a fade target, a colormap fade, or the fade-table flag. Shade is on only when the fixed light is negative and there is no fixed colormap. A BGRA target stores the LOD from the larger step magnitude. An inactive light is skipped. A point light whose normal constant is negative is skipped. A light inside its radius stores 256 divided by that radius and the packed color. The colored plane records one colored span per closed row.

| Managed (`VmFlatPlane1319Slice.cs`) | Native (`r_flatplane.cpp`) |
|--------------------------------------|----------------------------|
| Flat steps, span light, and the colored plane | **1–288** |
| Band hull (exclusive slice) | **1–288** |

### Chunk **1319** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **288** |
| **Net headline** | **+288** vs post-**1318** |


**Post-review ratio:** 271/288 managed/native ≈ 94% (src file LOC at HEAD; see tally).

## Tests

- `VmFlatPlane1319SliceTests.Render_SkipsZeroAlphaAndBuildsTheSteps`
- `VmFlatPlane1319SliceTests.RenderLine_LightsTheSpanAndRejectsABadRange`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1319 --to 1319` (0 hard / 0 soft).

## Seams

Closed span rows come from the plane. Plane depth, the screen-to-view Y, and the flat visibility are the viewport and light host. The span drawer records the arguments.

## Credit rationale

UNCredited r_flatplane.cpp 1-288 (+288). A full tally line scan shows no overlap. The file ends at line 288.
