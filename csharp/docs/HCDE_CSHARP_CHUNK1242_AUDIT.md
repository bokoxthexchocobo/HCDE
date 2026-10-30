# Chunk 1242 — Material shader index, texture layers, and validate from hw_material.cpp (1-218).

## Scope

Material shader index, texture layers, and validate from hw_material.cpp (1-218).

## Native ranges

- `src/common/textures/hw_material.cpp` **FMaterial::FMaterial** 1–218 (218 lines)

**Credited native LOC:** 218 (tally `nativeLoc` 218).

## Managed

`VmHwMaterial1242Slice.cs`
Managed LOC (tally): 409.
Conversion commit: `8b0e3ced6aa355ff0e7a49a2d500a521f0343dfa`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Custom-shader cvar and layer callback (**28–37**) | yes |
| Constructor (**45–150**) | yes |
| Destructor (**158–160**) | yes |
| GetLayer (**169–189**) | yes |
| ValidateTexture (**198–213**) | yes |
| DeleteMaterial (**215–218**) | yes |
| Band hull (**1–218**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| none | the band is the whole file |

The destructor body is empty, so the slice has no method for it. Delete leaves the material pointer in the texture slot. Indexed layers 1 and 2 use the callback. A hardware canvas stores a user shader index without the custom-shader cvar. A null placeholder is logged and stored as texture 0. A shader index past the user table is logged and does not change the shader.

| Managed (`VmHwMaterial1242Slice.cs`) | Native (`hw_material.cpp`) |
|--------------------------------------|-----------------------------|
| Shader index, layers, and validate | **1–218** |
| Band hull (exclusive slice) | **1–218** |

### Chunk **1242** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **218** |
| **Net headline** | **+218** vs post-**1241** |


**Post-review ratio:** 409/218 managed/native ≈ 188% (src file LOC at HEAD; see tally).

## Tests

- `VmHwMaterial1242SliceTests.Build_ChoosesShaderLayersAndBrightmap`
- `VmHwMaterial1242SliceTests.Query_ValidatesFlagsAndRoutesIndexedLayers`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1242 --to 1242` (0 hard / 0 soft).

## Seams

The constructor pushes the base texture, then stops after a software canvas whose color format is 0, an indexed scale flag, or a hardware canvas. A hardware canvas copies a user shader index without consulting gl_customshader and sets the clamp to the camera sampler. The other path uses a nonzero warp value as the shader, otherwise a normal plus specular pair, otherwise the four PBR maps. It then calls the brightmap scan, and fills missing bright, detail, and glow slots from game texture 1. Detail uses clamp none. A matching user shader appends non-null custom textures with clamp 0 and replaces the shader index. A mismatched type or a missing table entry leaves the shader alone. Validate forces indexed flags to that one bit, clears expand when sprites do not expand, and creates only when the slot is empty. GetLayer sends indexed layers 1 and 2 to the callback and returns nothing for any later indexed layer. An indexed base layer forces the translation to -1. DeleteMaterial does not clear the texture slot. The destructor body is empty.

## Credit rationale

UNCredited hw_material.cpp 1-218 (+218). The file ends at 218. A full tally line scan shows no overlap.
