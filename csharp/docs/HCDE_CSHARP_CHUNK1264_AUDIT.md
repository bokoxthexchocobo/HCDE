# Chunk 1264 — Stereo mode table, viewport scale, HUD ortho, and the eye frustum from hw_vrmodes.cpp (1-202).

## Scope

Stereo mode table, viewport scale, HUD ortho, and the eye frustum from hw_vrmodes.cpp (1-202).

## Native ranges

- `src/common/rendering/hwrenderer/data/hw_vrmodes.cpp` **VRMode** 1–202 (202 lines)

**Credited native LOC:** 202 (tally `nativeLoc` 202).

## Managed

`VmVrMode1264Slice.cs`
Managed LOC (tally): 243.
Conversion commit: `5bee5fe994e4c4eda18810201f36a8cdeb53d55b`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Mode records (**47–56**) | yes |
| Degree helpers (**58–66**) | yes |
| GetVRMode (**68–105**) | yes |
| AdjustViewport (**107–118**) | yes |
| GetHUDSpriteProjection (**120–129**) | yes |
| getShift (**131–135**) | yes |
| GetProjection (**137–184**) | yes |
| GetViewShift (**189–202**) | yes |
| Band hull (**1–202**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| Mode and eye cvars | host inputs |
| Screen size and clip planes | framebuffer inputs |

A request that is not going to the screen, or a filter callback that returns true, selects mono. A null callback leaves the cvar in force. Unknown mode numbers use the mono record. Green-magenta, red-cyan, quad, amber-blue, and side-by-side letterbox share the stereo record. Squished side-by-side and column interleave share the half-width record. Full side-by-side halves the width and doubles the weapon scale. Top-bottom and row interleave halve the height. The checker scales are the literal constant in this file. Viewport scaling truncates each edge toward zero, including a negative top. The weapon matrix is an ortho whose bottom is the screen height and whose top is zero. Swapping eyes negates the shift. A zero shift factor returns the origin and a symmetric perspective matrix. Any other shift builds an asymmetric frustum from the near plane times that shift over the screen distance. Isometric ortho uses a near plane of negative three over the fov ratio and the far plane from the screen.

| Managed (`VmVrMode1264Slice.cs`) | Native (`hw_vrmodes.cpp`) |
|----------------------------------|--------------------------|
| Mode table, viewport, and eye frustum | **1–202** |
| Band hull (exclusive slice) | **1–202** |

### Chunk **1264** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **202** |
| **Net headline** | **+202** vs post-**1263** |


**Post-review ratio:** 243/202 managed/native ≈ 120% (src file LOC at HEAD; see tally).

## Tests

- `VmVrMode1264SliceTests.Modes_ForceMonoAndScaleTheFullFrame`
- `VmVrMode1264SliceTests.Eyes_ShiftTheFrustumAndLeaveMonoAtTheOrigin`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1264 --to 1264` (0 hard / 0 soft).

## Seams

The screen width, height, near plane, and far plane are the framebuffer values this file reads. The interpupillary distance, screen distance, units per meter, eye swap, and mode cvar are host inputs. A null filter callback is not called. Ortho, perspective, and frustum are the matrix writes this file performs on a fresh identity matrix.

## Credit rationale

UNCredited hw_vrmodes.cpp 1-202 (+202). A full tally line scan shows no overlap. The file ends at line 202.
