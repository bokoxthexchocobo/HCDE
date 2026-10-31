# Chunk 1283 — Static and dynamic AABB build, longest-axis split, and the moved-line refresh from doom_aabbtree.cpp (1-265).

## Scope

Static and dynamic AABB build, longest-axis split, and the moved-line refresh from doom_aabbtree.cpp (1-265).

## Native ranges

- `src/rendering/hwrenderer/doom_aabbtree.cpp` **DoomLevelAABBTree::DoomLevelAABBTree** 1–265 (265 lines)

**Credited native LOC:** 265 (tally `nativeLoc` 265).

## Managed

`VmAabbTree1283Slice.cs`
Managed LOC (tally): 330.
Conversion commit: `84cba42023c537e5105e97def64eda3862d8c6e1`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Centroids and the static miss (**23–37**) | yes |
| Dynamic root (**39–71**) | yes |
| One-sided split (**74–107**) | yes |
| Moved line (**109–155**) | yes |
| Longest axis (**158–265**) | yes |
| Band hull (**1–265**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| Path walk | companion lookup, not this file |

A level with no one-sided static line returns before the dynamic subtree is built, so a polyobject-only level stays empty. One-sided lines with the polyobject flag and a front sidedef go to the dynamic tree. The same flag without a front sidedef stays static. Two-sided lines are ignored. The dynamic line cursor is taken while the tree-line list is still empty, so it stays zero and a later refresh walks every stored line. Equal axis lengths try X before Y. A centroid on the positive side of the median, including zero, goes left. When both axes leave one side empty, the original order is split in half and the sort buffer is discarded. Children are stored before the parent. A shared root is the union of the static and dynamic roots. A refresh compares the four floats by their bits, so negative zero differs from positive zero. The leaf box becomes the new endpoints, then each parent is the union of its children. An empty path leaves the stored line and the boxes unchanged.

| Managed (`VmAabbTree1283Slice.cs`) | Native (`doom_aabbtree.cpp`) |
|------------------------------------|------------------------------|
| Build and refresh | **1–265** |
| Band hull (exclusive slice) | **1–265** |

### Chunk **1283** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **265** |
| **Net headline** | **+265** vs post-**1282** |


**Post-review ratio:** 330/265 managed/native ≈ 125% (src file LOC at HEAD; see tally).

## Tests

- `VmAabbTree1283SliceTests.Build_SplitsStaticFromPolyAndHalvesATiedAxis`
- `VmAabbTree1283SliceTests.Update_RefreshesParentsAndLeavesAPointOutsideTheBox`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1283 --to 1283` (0 hard / 0 soft).

## Seams

Level lines are host records of the float vertices, the back sector, and the polyobject flag. FindNodePath is included so Update can walk from the stored line point. That walk is not credited.

## Credit rationale

UNCredited doom_aabbtree.cpp 1-265 (+265). A full tally line scan shows no overlap. The file ends at line 265.
