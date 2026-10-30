# Chunk 1300 — Null mesh, the level-mesh swap, and the bottom and top acceleration structures from vk_raytrace.cpp (1-293).

## Scope

Null mesh, the level-mesh swap, and the bottom and top acceleration structures from vk_raytrace.cpp (1-293).

## Native ranges

- `src/common/rendering/vulkan/renderer/vk_raytrace.cpp` **VkRaytrace::SetLevelMesh** 1–293 (293 lines)

**Credited native LOC:** 293 (tally `nativeLoc` 293).

## Managed

`VmVkRaytrace1300Slice.cs`
Managed LOC (tally): 351.
Conversion commit: `fd207530fa2f9721fd4c0fb7d6fa06d0e867e7f9`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Null mesh and level-mesh swap (**29–77**) | yes |
| Reset and object creation (**79–100**) | yes |
| Vertex and index buffers (**102–150**) | yes |
| Bottom-level acceleration structure (**152–209**) | yes |
| Top-level acceleration structure (**211–293**) | yes |
| Band hull (**1–293**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| Build-size query results | host device |
| Device addresses | host device |

The constructor seeds a twelve-vertex null mesh and index elements 0 through 11, then sets a null mesh. That first assignment does not idle, because no mesh is current yet. A later call with the same mesh, including null while the null mesh is already current, does not idle or rebuild. A change away from a current mesh idles, retires the eleven buffers and acceleration structures in source order, and stores the new mesh. Ray tracing is read after that store. When it is off, the retired objects stay queued and alive and no new build runs. When it is on, vertex bytes are twelve times the vertex count and index bytes are four times the element count. The vertex copy starts at source offset zero and the index copy starts just after the vertices. Both copies use destination offset zero and the destination buffer size. The transfer barrier is transfer-write to shader-read, from the transfer stage to acceleration-structure build. The bottom level uses one triangle geometry, opaque, a twelve-byte stride, unsigned 32-bit indices, and a max vertex of count minus one. The primitive count is the element count divided by three. Scratch and storage sizes come from that primitive count. The top-level instance is an identity 3×4 matrix with mask 255 and culling disabled, and it references the bottom-level address. Its copy is a sixty-four-byte whole-buffer copy. The top-level build asks for one instance and records a primitive count of one. Its closing barrier is acceleration-structure write to shader-read, from build to the fragment stage. The upload wait is `WaitForCommands(false, true)` and runs only after the objects exist, so the wait reclaims the retired set and leaves the new one alive.

| Managed (`VmVkRaytrace1300Slice.cs`) | Native (`vk_raytrace.cpp`) |
|--------------------------------------|---------------------------|
| Null mesh, swap, and acceleration structures | **1–293** |
| Band hull (exclusive slice) | **1–293** |

### Chunk **1300** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **293** |
| **Net headline** | **+293** vs post-**1299** |


**Post-review ratio:** 351/293 managed/native ≈ 120% (src file LOC at HEAD; see tally).

## Tests

- `VmVkRaytrace1300SliceTests.NullMesh_SkipsTheIdleAndBuildsFourTriangles`
- `VmVkRaytrace1300SliceTests.MeshChange_IdlesBeforeResetAndSkipsTheBuildWhenRaytracingIsOff`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1300 --to 1300` (0 hard / 0 soft).

## Seams

Build sizes and device addresses are host services. The device idle and the upload wait are counters.

## Credit rationale

UNCredited vk_raytrace.cpp 1-293 (+293). A full tally line scan shows no overlap. The file ends at line 293.
