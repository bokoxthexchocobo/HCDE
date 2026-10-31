# Chunk 1262 — Filter and clamp tables, hardware samplers, and the postprocess cache from vk_samplers.cpp (1-190).

## Scope

Filter and clamp tables, hardware samplers, and the postprocess cache from vk_samplers.cpp (1-190).

## Native ranges

- `src/common/rendering/vulkan/textures/vk_samplers.cpp` **VkSamplerManager** 1–190 (190 lines)

**Credited native LOC:** 190 (tally `nativeLoc` 190).

## Managed

`VmSampler1262Slice.cs`
Managed LOC (tally): 238.
Conversion commit: `71013e2a8cae24e84b5d3e8475fa08795124d0ad`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Filter and clamp tables (**36–67**) | yes |
| VkSamplerManager (**69–74**) | yes |
| ~VkSamplerManager (**76–78**) | yes |
| ResetHWSamplers (**80–84**) | yes |
| CreateHWSamplers (**86–141**) | yes |
| DeleteHWSamplers (**143–150**) | yes |
| Get (**152–168**) | yes |
| CreateShadowmapSampler (**170–179**) | yes |
| CreateLightmapSampler (**181–190**) | yes |
| Band hull (**1–190**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| Sampler object creation | host step |
| Filter and anisotropy cvars | host inputs |

Table row six stores a linear minification filter, a nearest magnification filter, and a linear mip mode. A callback that disables filtering forces row zero instead. Anisotropy and a max lod of 100 are set only when that row wants mipmaps and anisotropy is allowed. Otherwise the max lod is one quarter and anisotropy stays off. The no-mip clamp and the camera sampler copy the magnification filter into the minification filter, force nearest mip mode, and use a max lod of one quarter. The unfiltered slots always use nearest filters and the clamp table. Postprocess lookup packs the filter and wrap into an index and returns the existing sampler. A new postprocess sampler uses nearest mip mode and does not set lod. Shadow uses nearest filters and clamp on every axis. Light uses linear filters, linear mip mode, and clamp on every axis. Reset drops only the hardware slots and builds them again. The shadow, light, and postprocess samplers stay. The destructor body does not drop them.

| Managed (`VmSampler1262Slice.cs`) | Native (`vk_samplers.cpp`) |
|-----------------------------------|--------------------------|
| Tables, hardware samplers, postprocess cache | **1–190** |
| Band hull (exclusive slice) | **1–190** |

### Chunk **1262** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **190** |
| **Net headline** | **+190** vs post-**1261** |


**Post-review ratio:** 238/190 managed/native ≈ 125% (src file LOC at HEAD; see tally).

## Tests

- `VmSampler1262SliceTests.Build_UsesFilterRowSixAndCachesPostprocess`
- `VmSampler1262SliceTests.Reset_DropsHardwareSlotsAndKeepsShadow`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1262 --to 1262` (0 hard / 0 soft).

## Seams

Creating the Vulkan sampler object is a host step. The texture-filter cvar, the anisotropy cvar, and the two disable callbacks are host inputs. The slice keeps the table row, the address modes, and whether anisotropy and the max lod were set by this file. The destructor body does not hand the samplers to the delete list.

## Credit rationale

UNCredited vk_samplers.cpp 1-190 (+190). A full tally line scan shows no overlap. The file ends at line 190.
