# Chunk 1297 — Command submit, timestamp groups, and the frame wait from vk_commandbuffer.cpp (1-266).

## Scope

Command submit, timestamp groups, and the frame wait from vk_commandbuffer.cpp (1-266).

## Native ranges

- `src/common/rendering/vulkan/system/vk_commandbuffer.cpp` **VkCommandBufferManager::FlushCommands** 1–266 (266 lines)

**Credited native LOC:** 266 (tally `nativeLoc` 266).

## Managed

`VmVkCommandBuffer1297Slice.cs`
Managed LOC (tally): 338.
Conversion commit: `bf8644766dbb1437d5567cdabf454cd5ce53e732`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Construct and commands (**40–99**) | yes |
| Flush (**101–161**) | yes |
| Wait and delete (**163–206**) | yes |
| Timestamp groups (**208–238**) | yes |
| Gpu stats (**240–266**) | yes |
| Band hull (**1–266**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| Device queue | host device |
| Swapchain image | host framebuffer |
| Timestamp period | host limit |

The constructor builds eight semaphores and eight fences. Time queries reset the full pool through the draw buffer. Getters create and begin a buffer once. BeginFrame resets only the used query count and then clears the cursor. An upload-only flush skips the render-pass end, ends the transfer buffer, and leaves the draw buffer open. Transfer is submitted before draw. The first submit does not wait. A later submit waits on the previous semaphore at the top of the pipe. Finish with a present image waits and signals that image. The last submit does not signal the submit semaphore. A submit at the ring size waits the wrapped fence first. Wait always flushes with last-submit set. It limits the frame only when finish is set and vsync is off, then waits the smaller of the submit count and eight fences. Delete replaces the transfer list, and replaces the draw list only when the flush is not upload-only. A finish wait copies the running command count into the published count and then clears the running count, so a later finish with no new buffers publishes zero. Push records a start query and the stack index before the query is stored. Pop with time queries off leaves the end index at zero, and the stat line is skipped when the end is not past the start. A backwards timestamp clamps to zero milliseconds. The stat text uses two digits after the decimal. Update copies the keep flag onto the active flag and then clears the keep flag. It does not clear the query cursor. Destroy does not release the pool.

| Managed (`VmVkCommandBuffer1297Slice.cs`) | Native (`vk_commandbuffer.cpp`) |
|------------------------------------------|-------------------------------|
| Submit, groups, and the frame wait | **1–266** |
| Band hull (exclusive slice) | **1–266** |

### Chunk **1297** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **266** |
| **Net headline** | **+266** vs post-**1296** |


**Post-review ratio:** 338/266 managed/native ≈ 127% (src file LOC at HEAD; see tally).

## Tests

- `VmVkCommandBuffer1297SliceTests.Flush_SubmitsTransferFirstAndFinishPublishesZero`
- `VmVkCommandBuffer1297SliceTests.Stats_SkipsAnOpenGroupAndBeginFrameResetsTheCursor`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1297 --to 1297` (0 hard / 0 soft).

## Seams

The device, swapchain image, and timestamp period are host services. Fence waits are counters.

## Credit rationale

UNCredited vk_commandbuffer.cpp 1-266 (+266). A full tally line scan shows no overlap. The file ends at line 266.
