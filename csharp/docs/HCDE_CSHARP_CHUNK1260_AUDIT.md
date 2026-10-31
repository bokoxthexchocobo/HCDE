# Chunk 1260 — Image-layout barriers and the mip chain from vk_imagetransition.cpp (1-152).

## Scope

Image-layout barriers and the mip chain from vk_imagetransition.cpp (1-152).

## Native ranges

- `src/common/rendering/vulkan/textures/vk_imagetransition.cpp` **VkImageTransition** 1–152 (152 lines)

**Credited native LOC:** 152 (tally `nativeLoc` 152).

## Managed

`VmImageTransition1260Slice.cs`
Managed LOC (tally): 195.
Conversion commit: `bf6276073b01f257dea2fee8f0fd79c2b0218de9`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| AddImage (**25–98**) | yes |
| Execute (**100–104**) | yes |
| GenerateMipmaps (**108–152**) | yes |
| Band hull (**1–152**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| Barrier record and blit | host steps |
| Layout enumeration headers | not in this file |

A matching layout returns before either stage mask changes. Source and destination masks accumulate with a bitwise or across later images. An unknown source aborts with the stage masks left as they were. An unknown destination aborts after the source stage bit is set, and it does not record a barrier or change the layout. Execute runs only after a barrier was recorded, and it passes the accumulated masks. Asking for an undefined old layout changes the recorded old layout only. The access bits still come from the layout the image currently has. Mip generation halves each side and keeps a floor of one. Each step records a transfer source and an undefined destination, blits with a linear filter, then moves the finished level to shader read. A one-by-one image skips the loop and still transitions mip zero from a transfer destination to shader read. The texture layout ends as shader read. The mip barriers always use the color aspect, not the texture aspect mask.

| Managed (`VmImageTransition1260Slice.cs`) | Native (`vk_imagetransition.cpp`) |
|------------------------------------------|-----------------------------------|
| Layout table, execute, mip chain | **1–152** |
| Band hull (exclusive slice) | **1–152** |

### Chunk **1260** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **152** |
| **Net headline** | **+152** vs post-**1259** |


**Post-review ratio:** 195/152 managed/native ≈ 128% (src file LOC at HEAD; see tally).

## Tests

- `VmImageTransition1260SliceTests.Add_AccumulatesStageMasksAndSkipsAMatchingLayout`
- `VmImageTransition1260SliceTests.Mipmaps_HalvesUntilOneAndLeavesTheLastLevelReadable`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1260 --to 1260` (0 hard / 0 soft).

## Seams

Recording the barrier and issuing the blit are host steps. The slice keeps the access and stage bits this file selects, including the builder defaults of one array layer at index 0. A missing layout aborts before the barrier and before the layout store. Mip generation passes transfer and shader access directly and does not consult the layout table.

## Credit rationale

UNCredited vk_imagetransition.cpp 1-152 (+152). A full tally line scan shows no overlap. The file ends at line 152.
