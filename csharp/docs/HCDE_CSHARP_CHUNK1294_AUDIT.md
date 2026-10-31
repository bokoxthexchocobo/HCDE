# Chunk 1294 — Upload padding, resize copy, and the buffer lock from vk_hwbuffer.cpp (1-253).

## Scope

Upload padding, resize copy, and the buffer lock from vk_hwbuffer.cpp (1-253).

## Native ranges

- `src/common/rendering/vulkan/system/vk_hwbuffer.cpp` **VkHardwareBuffer::SetData** 1–253 (253 lines)

**Credited native LOC:** 253 (tally `nativeLoc` 253).

## Managed

`VmVkHardwareBuffer1294Slice.cs`
Managed LOC (tally): 281.
Conversion commit: `88ae3b4f72d0be93758d5b1dcd0c6abe30cf5265`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Construct and reset (**32–57**) | yes |
| SetData (**59–139**) | yes |
| SetSubData (**141–159**) | yes |
| Resize (**161–190**) | yes |
| Map and lock (**192–238**) | yes |
| Format and bind (**242–253**) | yes |
| Band hull (**1–253**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| Device | host registration |
| Delete lists | host command buffers |
| Vertex format | host id |

Every allocation is at least 16 bytes. SetData stores the requested size. Static and stream buffers keep a staging copy and copy the whole allocation. Persistent buffers stay mapped. A mappable subdata write is padded to 16 bytes and does not queue a copy. Resize stores the padded size, copies only the previous stored size, waits without finishing, and updates descriptors. The retired resize buffer goes to the transfer list. Lock with no buffer uploads through a static SetData. Reset leaves the stored size and does nothing when the device is gone. Destroy removes the buffer only while the device is attached. Bind stores the start and ignores the length. A storage buffer that needs resize also sets both transfer usage bits.

| Managed (`VmVkHardwareBuffer1294Slice.cs`) | Native (`vk_hwbuffer.cpp`) |
|--------------------------------------------|---------------------------|
| Upload, resize, and lock | **1–253** |
| Band hull (exclusive slice) | **1–253** |

### Chunk **1294** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **253** |
| **Net headline** | **+253** vs post-**1293** |


**Post-review ratio:** 281/253 managed/native ≈ 111% (src file LOC at HEAD; see tally).

## Tests

- `VmVkHardwareBuffer1294SliceTests.SetData_PadsSixteenAndResizeCopiesTheStoredSize`
- `VmVkHardwareBuffer1294SliceTests.Lock_UploadsAStaticBufferAndSubDataPadsTheWrite`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1294 --to 1294` (0 hard / 0 soft).

## Seams

The device, delete lists, descriptor update, and vertex-format id are host services. A detached device skips reset and destroy.

## Credit rationale

UNCredited vk_hwbuffer.cpp 1-253 (+253). A full tally line scan shows no overlap. The file ends at line 253.
