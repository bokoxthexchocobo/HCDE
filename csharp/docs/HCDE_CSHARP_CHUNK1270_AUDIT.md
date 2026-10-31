# Chunk 1270 — Portal segment stacks, sprite clip groups, and the drawn-range backup from r_drawsegment.cpp (1-223).

## Scope

Portal segment stacks, sprite clip groups, and the drawn-range backup from r_drawsegment.cpp (1-223).

## Native ranges

- `src/rendering/swrenderer/segments/r_drawsegment.cpp` **DrawSegmentList** 1–223 (223 lines)

**Credited native LOC:** 223 (tally `nativeLoc` 223).

## Managed

`VmDrawSeg1270Slice.cs`
Managed LOC (tally): 309.
Conversion commit: `d4d946dc9d98ce5ca4b7a5a2958dbe2728c0bdf5`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| DrawSegmentList (**53–56**) | yes |
| Clear (**58–67**) | yes |
| PushPortal (**69–73**) | yes |
| PopPortal (**75–82**) | yes |
| Push (**84–87**) | yes |
| PushTranslucent (**89–92**) | yes |
| BuildSegmentGroups (**94–168**) | yes |
| SetTopClip (**172–185**) | yes |
| SetBottomClip (**187–200**) | yes |
| SetBackupClip (**202–207**) | yes |
| SetRangeDrawn (**209–213**) | yes |
| SetRangeUndrawn (**215–222**) | yes |
| Band hull (**1–223**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| Frame allocator | host memory |
| View height | host viewport |

Clear plants a zero start on both stacks. Segment zero is the newest push, and a count stops at the current portal. Pop resizes both stacks back to the saved start and then drops that start. A pop of the baseline start leaves the stacks empty, and another pop faults. Groups walk the visible segments in bands of one hundred. The first segment in a band seeds the screen edges and the near and far depth, and each later segment widens them. Columns in that span start at a zero top and a bottom of the view height, stored in sixteen bits. A bottom silhouette keeps the lesser column and a top silhouette keeps the greater column, so a negative top does not replace zero. The group copies those columns into new buffers. A missing segment or a missing allocator faults. Clip storage is the allocated buffer minus x1, so a later column index is absolute. A null source faults after the allocation and does not replace the previous buffer. Marking a range drawn fills the top with the view height. Marking it undrawn does nothing unless it was drawn. The drawn flag drops before the backup copy, so a missing backup faults with the flag already clear and leaves the filled columns in place. A later undrawn call then does nothing.

| Managed (`VmDrawSeg1270Slice.cs`) | Native (`r_drawsegment.cpp`) |
|-----------------------------------|------------------------------|
| Stacks, groups, and clip backup | **1–223** |
| Band hull (exclusive slice) | **1–223** |

### Chunk **1270** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **223** |
| **Net headline** | **+223** vs post-**1269** |


**Post-review ratio:** 309/223 managed/native ≈ 139% (src file LOC at HEAD; see tally).

## Tests

- `VmDrawSeg1270SliceTests.Portals_HideTheOuterSegmentsAndSplitGroupsOfOneHundred`
- `VmDrawSeg1270SliceTests.Clips_MergeSilhouetteAndRestoreOnlyTheDrawnRange`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1270 --to 1270` (0 hard / 0 soft).

## Seams

The frame allocator and the view height are host inputs. The slice keeps the reverse segment index, the portal resize, the group of 100, the silhouette min and max, and the backup gate.

## Credit rationale

UNCredited r_drawsegment.cpp 1-223 (+223). A full tally line scan shows no overlap. The file ends at line 223.
