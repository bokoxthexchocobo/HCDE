# Chunk 1318 — Sprite billboard, the water clip, and the portal column clip from r_sprite.cpp (1-298).

## Scope

Sprite billboard, the water clip, and the portal column clip from r_sprite.cpp (1-298).

## Native ranges

- `src/rendering/swrenderer/things/r_sprite.cpp` **RenderSprite::Project** 1–298 (298 lines)

**Credited native LOC:** 298 (tally `nativeLoc` 298).

## Managed

`VmSpriteProject1318Slice.cs`
Managed LOC (tally): 436.
Conversion commit: `9c82598d87a8d5c98000ca852cd2438d1c3d59af`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Billboard endpoints and the wall reject (**81–93**) | yes |
| Water and fake-floor clip (**95–126**) | yes |
| Mirror, sprite flip, and the HQ scale (**128–164**) | yes |
| Vanilla trans, invert, fullbright, and the shadow alpha (**166–208**) | yes |
| Dynamic light accumulation (**210–268**) | yes |
| Colormap arguments and the sprite push (**270–272**) | yes |
| Masked render and the portal column clip (**275–297**) | yes |
| Band hull (**1–298**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| The masked pixel drawer | the host records the draw arguments |

The billboard walks the sprite offset along the view right, then the wall clip. A mirror swaps the rotated edges and xors the x-flip bit. Sprite flip xors that bit after the endpoints are built. HQ resize divides the vertical scale only when the mode is live, the multiple is at least one, and sprite targets are set. Vanilla trans forces the normal style when the actor is marked and the class is listed. Invert source flips the overlay invert before the shadow replacement. An additive style with a nonzero fade copies the colormap and clears the fade. Fullbright is off in fog. A bright actor still sets the stored fullbright bit. A texture that disables fullbright blocks the render flag and a forced texture, and a bright actor still passes. The shadow style is the translucent stencil, the fill is zero, and the alpha uses the clamp that takes zero as its value. A light inside its radius adds attenuated color. A null node, a light that declines the actor, and a light outside the radius add nothing. Render copies the ceiling clip on a portal column and the floor clip otherwise, then draws at the top minus the floor clip.

| Managed (`VmSpriteProject1318Slice.cs`) | Native (`r_sprite.cpp`) |
|------------------------------------------|--------------------------|
| Billboard, water clip, lights, and the column clip | **1–298** |
| Band hull (exclusive slice) | **1–298** |

### Chunk **1318** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **298** |
| **Net headline** | **+298** vs post-**1317** |


**Post-review ratio:** 436/298 managed/native ≈ 146% (src file LOC at HEAD; see tally).

## Tests

- `VmSpriteProject1318SliceTests.Project_PlacesTheBillboardAndRejectsWater`
- `VmSpriteProject1318SliceTests.Project_FlipsScalesStylesAndFadesTheShadow`
- `VmSpriteProject1318SliceTests.Project_AccumulatesLightsAndRenderClipsColumns`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1318 --to 1318` (0 hard / 0 soft).

## Seams

Wall coordinates, the height-sector sample, and the dynamic-light list are host inputs. Project stores the vissprite and Render records the masked draw.

## Credit rationale

UNCredited r_sprite.cpp 1-298 (+298). A full tally line scan shows no overlap. The file ends at line 298.
