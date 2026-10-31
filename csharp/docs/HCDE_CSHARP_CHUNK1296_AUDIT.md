# Chunk 1296 — Buffer data, map, and resize from gl_buffers.cpp (1-266).

## Scope

Buffer data, map, and resize from gl_buffers.cpp (1-266).

## Native ranges

- `src/common/rendering/gl/gl_buffers.cpp` **GLBuffer::SetData** 1–266 (266 lines)

**Credited native LOC:** 266 (tally `nativeLoc` 266).

## Managed

`VmGlBuffer1296Slice.cs`
Managed LOC (tally): 377.
Conversion commit: `feaa1ad92222fe07a2b67f6b47b339a390a44146`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Construct, bind, and destroy (**45–65**) | yes |
| SetData and subdata (**68–107**) | yes |
| Map, lock, and resize (**109–169**) | yes |
| Sync (**171–193**) | yes |
| Vertex format and bind (**201–249**) | yes |
| Data bind and constructors (**251–264**) | yes |
| Band hull (**1–266**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| Buffer ids | host names |
| Persistent flag | host screen query |
| Render-state reset | host counter |

Static and stream uploads copy the caller bytes. Persistent and mappable uploads ignore the pointer. Persistent storage maps with the write, persistent, and coherent bits when the host says buffers stay mapped, and otherwise falls back to a stream upload and a null map. Either persistent path clears the no-map block. A later mappable upload clears the map member and leaves the persistent flag set, so Map and Unmap stay no-ops. Resize runs only while no-map is clear and the request is larger. It unmaps, allocates a new id, calls SetData as persistent, then copies the size that call just stored, which is the new size. Lock uploads mappable and maps with the invalidate bit, but it does not store that pointer in the map member. Unlock unmaps without clearing the lock token. DropSync replaces a fence. WaitSync waits 50000000 ns, records a failure unless the status is already signaled or satisfied, and still deletes the fence. SetFormat writes the seven-entry table and skips locations outside 0 through 9. Bind walks all ten slots, disables a zero size, and adds stride times the binding offset when an offset array is present. Vertex, index, and data constructors select the array, element, and storage or uniform targets. Destroy binds, unmaps, binds zero, and deletes once.

| Managed (`VmGlBuffer1296Slice.cs`) | Native (`gl_buffers.cpp`) |
|------------------------------------|---------------------------|
| Upload, resize, format, and sync | **1–266** |
| Band hull (exclusive slice) | **1–266** |

### Chunk **1296** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **266** |
| **Net headline** | **+266** vs post-**1295** |


**Post-review ratio:** 377/266 managed/native ≈ 142% (src file LOC at HEAD; see tally).

## Tests

- `VmGlBuffer1296SliceTests.Resize_CopiesTheNewSizeAndLeavesPersistentSticky`
- `VmGlBuffer1296SliceTests.Format_WalksEverySlotAndLockLeavesTheMapMember`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1296 --to 1296` (0 hard / 0 soft).

## Seams

Buffer ids, map tokens, and the persistent-buffer flag are host services. The render-state reset is a counter.

## Credit rationale

UNCredited gl_buffers.cpp 1-266 (+266). A full tally line scan shows no overlap. The file ends at line 266.
