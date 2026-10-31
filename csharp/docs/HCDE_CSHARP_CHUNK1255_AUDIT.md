# Chunk 1255 — Hardware draw-list routing for walls, mirrors, flats, and sprites from hw_drawlistadd.cpp (1-153), through end of file.

## Scope

Hardware draw-list routing for walls, mirrors, flats, and sprites from hw_drawlistadd.cpp (1-153), through end of file.

## Native ranges

- `src/rendering/hwrenderer/scene/hw_drawlistadd.cpp` **AddWall** 1–153 (153 lines)

**Credited native LOC:** 153 (tally `nativeLoc` 153).

## Managed

`VmDrawList1255Slice.cs`
Managed LOC (tally): 228.
Conversion commit: `39b41119ae1a45a3502366a38117a5a6d48e16fb`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| AddWall (**35–58**) | yes |
| AddMirrorSurface (**66–88**) | yes |
| AddFlat (**99–131**) | yes |
| AddSprite (**138–153**) | yes |
| Band hull (**1–153**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| Pass-flag table | other file |
| Light upload | host step |

A translucent wall goes to the translucent list before the sky hack is considered. A sky-hack two-sided wall goes to the offset masked list. A pass flag of 1 ignores a masked texture and uses the plain list. Any other pass flag uses the masked list only when a texture is masked. The mirror path sets the source type, copies the wall, clears the copy vertex count, writes the segment normal into every texture coordinate, builds vertices, runs lights only when decals exist and the scene is not fullbright and dynamic lights are on, runs decals, then stores light index -1. A flat whose style is not translucent, whose alpha is below 1 minus float epsilon, that is fog, or that has no texture, uses the translucent list when it is a 3D plane and the border list otherwise. A translucent texture with a stack, or a sloped 3D plane, does not use that translucent list. A sprite with the translucent flag, no actor, or neither a model frame nor a wall sprite, uses the translucent list. A model frame or a wall sprite uses the model list.

| Managed (`VmDrawList1255Slice.cs`) | Native (`hw_drawlistadd.cpp`) |
|------------------------------------|--------------------------------|
| Wall, mirror, flat, and sprite lists | **1–153** |
| Band hull (exclusive slice) | **1–153** |

### Chunk **1255** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **153** |
| **Net headline** | **+153** vs post-**1254** |


**Post-review ratio:** 228/153 managed/native ≈ 149% (src file LOC at HEAD; see tally).

## Tests

- `VmDrawList1255SliceTests.Wall_RoutesSkyhackAndCopiesTheMirrorNormal`
- `VmDrawList1255SliceTests.Flat_KeepsAFullAlphaAndSendsModelsToTheirOwnList`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1255 --to 1255` (0 hard / 0 soft).

## Seams

The pass-flag table is the static array this file indexes. Segment length is the CRT square root. MakeVertices, dynamic lights, and decals are host steps. The stored wall is a copy.

## Credit rationale

UNCredited hw_drawlistadd.cpp 1-153 (+153). A full tally line scan shows no overlap. The file ends at line 153.
