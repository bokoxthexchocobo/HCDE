# Chunk 1282 — Particle projection, square sprite edges, and the masked segment test from r_particle.cpp (1-293).

## Scope

Particle projection, square sprite edges, and the masked segment test from r_particle.cpp (1-293).

## Native ranges

- `src/rendering/swrenderer/things/r_particle.cpp` **RenderParticle::Project** 1–293 (293 lines)

**Credited native LOC:** 293 (tally `nativeLoc` 293).

## Managed

`VmParticle1282Slice.cs`
Managed LOC (tally): 326.
Conversion commit: `3ebe450048d9b460e224a7e1948a5f5453c4ce09`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Time and portal (**81–93**) | yes |
| View reject (**95–127**) | yes |
| Edge clip (**129–148**) | yes |
| Height sector (**150–200**) | yes |
| Sprite record (**202–228**) | yes |
| Column walk (**230–265**) | yes |
| Masked segs (**267–292**) | yes |
| Band hull (**1–293**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| Column shade | host light table |
| Colormap bytes | host palette |

A paused or frozen view forces the time fraction to zero before the velocity is applied. The portal side test reads the particle position, not the position after velocity, and any nonzero side rejects the particle. A depth below 1/128 is behind the view. An X mirror rewrites the horizontal view coordinate to the view width minus that coordinate minus one before the side test. The side test rejects when depth is less than or equal to the absolute horizontal coordinate. The stored vertical scale is the horizontal scale, while the aspect multiplier applies only to the height delta. Ceiling clips raise the top in two steps, and floor clips lower the bottom in two steps. A top that is past the bottom rejects. A solid floor rejects when the moved height is below the plane sampled at the unmoved position. A solid ceiling rejects when that height is greater than or equal to the plane. Sky pictures skip those tests. The above-ceiling and below-floor choices take the height-sector colors, and the center choice takes the sector colors. The fullbright flag is only the low bit. A column walk whose height or width is not positive returns before masked segments. A blocked column still advances the texture coordinate. A masked segment is drawn only when its interpolated depth is strictly nearer than the particle and the portal id matches, using the integer midpoint of the particle edges.

| Managed (`VmParticle1282Slice.cs`) | Native (`r_particle.cpp`) |
|------------------------------------|---------------------------|
| Projection and masked test | **1–293** |
| Band hull (exclusive slice) | **1–293** |

### Chunk **1282** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **293** |
| **Net headline** | **+293** vs post-**1281** |


**Post-review ratio:** 326/293 managed/native ≈ 111% (src file LOC at HEAD; see tally).

## Tests

- `VmParticle1282SliceTests.Project_KeepsTheParticleSquareAndUsesTheUnmovedPosition`
- `VmParticle1282SliceTests.Render_StepsTheTextureAcrossABlockedColumn`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1282 --to 1282` (0 hard / 0 soft).

## Seams

The portal side, plane height, colormap table, and column shade are host inputs. The slice keeps the unmoved portal test, the square scale, the clip order, and the texture step across a blocked column.

## Credit rationale

UNCredited r_particle.cpp 1-293 (+293). A full tally line scan shows no overlap. The file ends at line 293.
