# Chunk 1267 — Camera colormap, visibility distances, and the sprite shade from r_light.cpp (1-218).

## Scope

Camera colormap, visibility distances, and the sprite shade from r_light.cpp (1-218).

## Native ranges

- `src/rendering/swrenderer/scene/r_light.cpp` **CameraLight** 1–218 (218 lines)

**Credited native LOC:** 218 (tally `nativeLoc` 218).

## Managed

`VmLight1267Slice.cs`
Managed LOC (tally): 313.
Conversion commit: `4d18aa27d2a045eca148c087427ce515f7a69314`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Instance (**50–54**) | yes |
| SetCamera (**56–93**) | yes |
| SetVisibility (**98–144**) | yes |
| LightLevelToShadeImpl (**146–161**) | yes |
| SetColormap (**165–217**) | yes |
| Band hull (**1–218**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| Colormap tables | host palette |
| Viewport size | host screen |

A missing actor faults before the player is read. The camera player replaces the actor player. A null camera matches a null body. A colormap index in range selects that special map and the real colormap, and a minimum extralight is left unchanged. Otherwise a light level in range is stored as the level times 256, and the fullbright colormap is selected only when sector color is ignored. When the colormap is still empty and extralight is at its minimum, the inverse map is selected and extralight is cleared. Visibility stores the clamped value and returns before any divide when either focal term is zero, leaving the previous distances in place. Wall and floor distances clamp on each sign against their own maxima, then the wall distance is rescaled. The shade uses the +12 term. A level that disables light fade and is not foggy uses the remaining-light shift instead, including a zero result when the rescaled level is above 255. Particles halve sprite visibility after the distance read. Weapon sprites then force visibility to zero and subtract 24 from the shade. Fade to black with the invert flag writes a white fade and clears the flag so the color is not inverted again. A fixed colormap still wins after that replacement.

| Managed (`VmLight1267Slice.cs`) | Native (`r_light.cpp`) |
|----------------------------------|------------------------|
| Camera, distances, and shade | **1–218** |
| Band hull (exclusive slice) | **1–218** |

### Chunk **1267** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **218** |
| **Net headline** | **+218** vs post-**1266** |


**Post-review ratio:** 313/218 managed/native ≈ 144% (src file LOC at HEAD; see tally).

## Tests

- `VmLight1267SliceTests.Camera_UsesTheCameraPlayerAndClearsExtralightOnlyWhenUnmapped`
- `VmLight1267SliceTests.Distances_ClampEachPlaneAndHalveParticleVisibility`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1267 --to 1267` (0 hard / 0 soft).

## Seams

Colormap tables, the viewport, and the palette index are host seams. The slice keeps the camera selection, the focal early return, the visibility clamps, the +12 shade, and the particle and weapon-sprite adjustments.

## Credit rationale

UNCredited r_light.cpp 1-218 (+218). A full tally line scan shows no overlap. The file ends at line 218.
