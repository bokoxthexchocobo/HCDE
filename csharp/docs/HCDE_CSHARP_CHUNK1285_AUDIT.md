# Chunk 1285 — Sky offset, cylinder width, and the column stripe from r_skyplane.cpp (1-307).

## Scope

Sky offset, cylinder width, and the column stripe from r_skyplane.cpp (1-307).

## Native ranges

- `src/rendering/swrenderer/plane/r_skyplane.cpp` **RenderSkyPlane::RenderSkyPlane** 1–307 (307 lines)

**Credited native LOC:** 307 (tally `nativeLoc` 307).

## Managed

`VmSkyPlane1285Slice.cs`
Managed LOC (tally): 296.
Conversion commit: `1be67491c0624a279880d3469f17b6517b354294`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Offset and cylinders (**66–111**) | yes |
| Sky pick (**113–215**) | yes |
| Column stripe (**217–267**) | yes |
| Tall and power columns (**269–288**) | yes |
| Draw loop (**290–307**) | yes |
| Band hull (**1–307**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| Cap color pair | host seam |
| Short draw height | computed, never passed to the stripe |

A null sky texture returns before the offset is chosen. Height 256 stores offset 166. Old freelook stores offset 0. The second cylinder uses scale Y, and a front position is always reduced modulo the first cylinder, including when the front texture is the second sky. A mirror column flips the angle index by subtracting the start column from the view width and leaves the destination column unchanged. A single sky still multiplies the back cylinder into the unused second angle. Swap without a double sky draws the second texture from the first sky position. An exact sky-flat flag draws the second texture from the second position and still uses the first cylinder as the modulus. A linedef sky with swap and a valid bottom texture uses the bottom offsets, and a missing bottom texture falls through to the swapped sky while the scroll stays on the first sky position. A power-of-two height repeats by scale Y. A taller texture wraps the fractional top and stores the truncated short height. Stretch scales the linedef mid by the texture height over 228 plus the sky offset.

| Managed (`VmSkyPlane1285Slice.cs`) | Native (`r_skyplane.cpp`) |
|------------------------------------|---------------------------|
| Offset, pick, and the stripe | **1–307** |
| Band hull (exclusive slice) | **1–307** |

### Chunk **1285** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **307** |
| **Net headline** | **+307** vs post-**1284** |


**Post-review ratio:** 296/307 managed/native ≈ 96% (src file LOC at HEAD; see tally).

## Tests

- `VmSkyPlane1285SliceTests.Setup_PicksTheOffsetAndTheSecondCylinderFromScaleY`
- `VmSkyPlane1285SliceTests.Render_KeepsTheSky1ModulusAndFlipsOnlyTheAngleX`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1285 --to 1285` (0 hard / 0 soft).

## Seams

Cap colors are a host pair. The short draw height on the tall path is stored and not passed to the stripe. A missing software texture is the Missing flag.

## Credit rationale

UNCredited r_skyplane.cpp 1-307 (+307). A full tally line scan shows no overlap. The file ends at line 307.
