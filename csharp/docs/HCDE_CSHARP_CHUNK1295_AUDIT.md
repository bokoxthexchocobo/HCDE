# Chunk 1295 — Null texture, shadowmap size, and the lightmap alpha from vk_texture.cpp (1-253).

## Scope

Null texture, shadowmap size, and the lightmap alpha from vk_texture.cpp (1-253).

## Native ranges

- `src/common/rendering/vulkan/textures/vk_texture.cpp` **VkTextureManager::SetLightmap** 1–253 (253 lines)

**Credited native LOC:** 253 (tally `nativeLoc` 253).

## Managed

`VmVkTexture1295Slice.cs`
Managed LOC (tally): 309.
Conversion commit: `1bbe551d1c579aedddd7a18d921a5beb0f5e2fec`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Construct and frame (**30–60**) | yes |
| Texture lists (**62–84**) | yes |
| Texture lookup (**86–142**) | yes |
| Null and shadow (**144–180**) | yes |
| Lightmap (**182–253**) | yes |
| Band hull (**1–253**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| Pipeline images | host slots |
| Scene images | host slots |
| Shadow quality | host cvar |

The null image is 1 by 1. The shadow map is as wide as the quality value and 1024 tall, and the frame recreates it when that width changes. The next pipeline image wraps at two. Swap chain has no image. An unknown type is fatal. Destroy and deinit remove hardware textures from the back, then post textures from the back. The lightmap reads three channels per pixel and writes a fixed alpha. The initial pixel therefore leaves its fourth source value unread.

| Managed (`VmVkTexture1295Slice.cs`) | Native (`vk_texture.cpp`) |
|-------------------------------------|---------------------------|
| Images, lookup, and lightmap | **1–253** |
| Band hull (exclusive slice) | **1–253** |

### Chunk **1295** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **253** |
| **Net headline** | **+253** vs post-**1294** |


**Post-review ratio:** 309/253 managed/native ≈ 122% (src file LOC at HEAD; see tally).

## Tests

- `VmVkTexture1295SliceTests.Lightmap_ForcesAlphaAndShadowRecreatesOnWidth`
- `VmVkTexture1295SliceTests.Lookup_WrapsThePipelineAndDrainsFromTheBack`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1295 --to 1295` (0 hard / 0 soft).

## Seams

Pipeline images and scene images are host slots. The shadow quality is the host cvar.

## Credit rationale

UNCredited vk_texture.cpp 1-253 (+253). A full tally line scan shows no overlap. The file ends at line 253.
