# Chunk 1281 — Software world matrix, clip frustum, aspect setup, and the sky angle table from r_viewport.cpp (1-290).

## Scope

Software world matrix, clip frustum, aspect setup, and the sky angle table from r_viewport.cpp (1-290).

## Native ranges

- `src/rendering/swrenderer/viewport/r_viewport.cpp` **RenderViewport::SetViewport** 1–290 (290 lines)

**Credited native LOC:** 290 (tally `nativeLoc` 290).

## Managed

`VmViewport1281Slice.cs`
Managed LOC (tally): 348.
Conversion commit: `7ff85e5175a870fa5edb98666a55c4aba6b69e45`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Poly viewport (**61–72**) | yes |
| World matrix (**74–89**) | yes |
| Clip frustum (**91–122**) | yes |
| Viewport setup (**124–185**) | yes |
| Freelook (**187–204**) | yes |
| Buffer seams (**206–210**) | yes |
| Destination offset (**212–220**) | yes |
| Sky angles (**222–247**) | yes |
| View points (**249–269**) | yes |
| Screen scale (**271–289**) | yes |
| Band hull (**1–290**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| Fuzz table init | host drawer |
| Particle texture init | host drawer |

A ratio below 1.333 is taller than wide. The viewport multiplies the chosen axis by the aspect multiplier and then divides by 48 with integer division. A null pixel stretch uses 1.2. The wall scale is taken from the inverse aspect before that inverse aspect is multiplied by 1.2 over the stretch. The view-size text is written only when the target is not a canvas, and it keeps at most 15 characters. The fuzz row is the view height minus two. Sky angles start at the center with slope zero and step by the focal tangent over the center. The last written column is the view width, and the left side is the two's complement of the column at view width minus one minus the index. A missing camera leaves the vertical look at half the view height. A camera adds the focal length times the tangent of the negated pitch, and the stored center truncates toward zero. The destination offset adds the window origin and then multiplies by four only for a BGRA target. World-to-view Y is the height delta, and Z is the yaw-rotated horizontal. The screen Y subtracts the view Y over Z times the inverse Z scale. A disabled pixel stretch divides the vertical screen scale by the aspect multiplier. The world matrix translates by the negated float position and does not write the bottom translation element. An X flip scales the first column by negative one, including a zero that becomes negative zero. The clip matrix is loaded from the frustum coefficients, and the world-to-clip slot is multiplied by that matrix instead of being replaced.

| Managed (`VmViewport1281Slice.cs`) | Native (`r_viewport.cpp`) |
|------------------------------------|---------------------------|
| Aspect, angles, and clip | **1–290** |
| Band hull (exclusive slice) | **1–290** |

### Chunk **1281** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **290** |
| **Net headline** | **+290** vs post-**1280** |


**Post-review ratio:** 348/290 managed/native ≈ 120% (src file LOC at HEAD; see tally).

## Tests

- `VmViewport1281SliceTests.Viewport_ScalesTheWideRatioAndMirrorsTheSkyAngles`
- `VmViewport1281SliceTests.Freelook_MultipliesTheClipMatrixAlreadyStored`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1281 --to 1281` (0 hard / 0 soft).

## Seams

The fuzz table and particle texture are host calls. The slice keeps the aspect integer division, the angle mirror, the freelook truncation, and the clip multiply onto the matrix already stored.

## Credit rationale

UNCredited r_viewport.cpp 1-290 (+290). A full tally line scan shows no overlap. The file ends at line 290.
