# Chunk 1216 — AABB node path, ray overlap, and ray-line hit from hw_aabbtree.cpp (1-161).

## Scope

AABB node path, ray overlap, and ray-line hit from hw_aabbtree.cpp (1-161).

## Native ranges

- `src/common/rendering/hwrenderer/data/hw_aabbtree.cpp` **LevelAABBTree::RayTest** 1–161 (161 lines)

**Credited native LOC:** 161 (tally `nativeLoc` 161).

## Managed

`VmAabbTree1216Slice.cs`
Managed LOC (tally): 167.
Conversion commit: `db45c605d79b257f6fafeec8437138019e99adea`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| FindNodePath (**24-49**) | yes |
| RayTest (**51-96**) | yes |
| OverlapRayAABB (**98-127**) | yes |
| IntersectRayLine (**129-158**) | yes |
| Band hull (**1-161**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| none | the band is the whole file |

A line point on the box edge is inside. The path is the matching leaf, then each parent back to the start node, and the left child is used only when the right child misses. A ray shorter than length 1 returns 1 before the walk. A parallel line, a hit at or behind the origin, and a parent box that misses the ray all stay at 1. The nearest positive fraction replaces the running hit.

| Managed (`VmAabbTree1216Slice.cs`) | Native (`hw_aabbtree.cpp`) |
|------------------------------------|----------------------------|
| Ray tree | **1-161** |
| Band hull (exclusive slice) | **1-161** |

### Chunk **1216** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **161** |
| **Net headline** | **+161** vs post-**1215** |


**Post-review ratio:** 167/161 managed/native ≈ 104% (src file LOC at HEAD; see tally).

## Tests

- `VmAabbTree1216SliceTests.FindNodePath_ReturnsTheLeafThenItsParents`
- `VmAabbTree1216SliceTests.RayTest_KeepsTheNearestHitAndSkipsATooDeepTree`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1216 --to 1216` (0 hard / 0 soft).

## Seams

Node and line coordinates are float widened to double. The ray Z is ignored. std::min keeps the current fraction when the new one is not smaller, including NaN. A 32-slot stack skips an internal node instead of descending.

## Credit rationale

UNCredited hw_aabbtree.cpp 1-161 (+161). The file ends at 161. A full tally line scan shows no overlap.
