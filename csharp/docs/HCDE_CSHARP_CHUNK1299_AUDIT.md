# Chunk 1299 — Descriptor pools, the null texture set, and the postprocess input from vk_descriptorset.cpp (1-297).

## Scope

Descriptor pools, the null texture set, and the postprocess input from vk_descriptorset.cpp (1-297).

## Native ranges

- `src/common/rendering/vulkan/renderer/vk_descriptorset.cpp` **VkDescriptorSetManager::GetInput** 1–297 (297 lines)

**Credited native LOC:** 297 (tally `nativeLoc` 297).

## Managed

`VmVkDescriptorSet1299Slice.cs`
Managed LOC (tally): 415.
Conversion commit: `9b362cfb10f773850853f590f5bf108668f913e8`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Constructor, init, deinit, and begin frame (**42–72**) | yes |
| HW buffer set (**74–92**) | yes |
| Fixed set (**94–111**) | yes |
| Texture reset and the null set (**113–145**) | yes |
| Texture allocate and layout (**147–183**) | yes |
| Materials (**185–195**) | yes |
| Postprocess input and pool (**197–253**) | yes |
| Layouts and pools (**255–297**) | yes |
| Band hull (**1–297**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| Device, sampler, and buffer objects | host services |
| Delete-list storage | reclaimed when the slice waits |

A null descriptor is not pushed onto the delete list. The eleventh HW or fixed allocate moves the current set first, try-allocate fails, and `WaitForCommands(false)` reclaims every queued set before the next allocate. Dynamic uniform ranges stay the viewpoint, matrix, and stream struct sizes even when the buffer object has a different size. Storage ranges use the buffer size. Ray tracing is read when the fixed layout and pool are created, and again when the fixed set is written, so a later flag change adds or omits the acceleration structure without rebuilding the layout. Texture pools refill when the set counter is zero or the descriptor remainder is below the request, including a request above 2000 that leaves a negative remainder. Growing the layout vector does not create the missing lower layouts. The null set is allocated once at 11 layers with the clamp-xy-nomip sampler and is not written again. Reset deletes material descriptors in list order, retires the texture pools and the null set, and leaves the materials registered. Deinit and the destructor remove materials from the back. `DeleteDescriptors` runs while the material still has its device pointer, then that pointer is cleared. A material whose device pointer is already null does not retire its sets. Postprocess input prefers a depth-only view, always writes the automatic uniform at binding 15 as a uniform buffer, and adds the three light storage buffers only when requested. The image transition uses shader-read layout and a defined flag, and it runs on the draw commands after the descriptor write. The postprocess pool is retired only when try-allocate fails. A wait that reclaims the queued sets lets the next allocate reuse that pool. The one hundred first allocate retires the full pool before creating the replacement, and the new set is queued after that pool.

| Managed (`VmVkDescriptorSet1299Slice.cs`) | Native (`vk_descriptorset.cpp`) |
|------------------------------------------|-------------------------------|
| Pools, the null set, and postprocess input | **1–297** |
| Band hull (exclusive slice) | **1–297** |

### Chunk **1299** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **297** |
| **Net headline** | **+297** vs post-**1298** |


**Post-review ratio:** 415/297 managed/native ≈ 140% (src file LOC at HEAD; see tally).

## Tests

- `VmVkDescriptorSet1299SliceTests.HwSets_WaitBeforeTheEleventhAllocateAndReadRaytracingLive`
- `VmVkDescriptorSet1299SliceTests.Textures_RefillBelowTheDescriptorFloorAndPostprocessRetiresAFullPool`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1299 --to 1299` (0 hard / 0 soft).

## Seams

Device objects, sampler instances, and buffer storage are host services. The draw delete list is reclaimed by WaitForCommands.

## Credit rationale

UNCredited vk_descriptorset.cpp 1-297 (+297). A full tally line scan shows no overlap. The file ends at line 297.
