# Chunk 1218 — Splitter event tree, spare-node recycle, and the distance print from nodebuild_events.cpp (1-210).

## Scope

Splitter event tree, spare-node recycle, and the distance print from nodebuild_events.cpp (1-210).

## Native ranges

- `src/utility/nodebuilder/nodebuild_events.cpp` **FEventTree::Insert** 1–210 (210 lines)

**Credited native LOC:** 210 (tally `nativeLoc` 210).

## Managed

`VmEventTree1218Slice.cs`
Managed LOC (tally): 201.
Conversion commit: `0f5bafd764f1c4737cfcdb2082575f98542ecf25`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Construct and destroy (**28–46**) | yes |
| DeleteAll (**48–63**) | yes |
| GetNewNode (**65–79**) | yes |
| Insert (**81–113**) | yes |
| Successor and predecessor (**115–159**) | yes |
| FindEvent and GetMinimum (**161–196**) | yes |
| PrintTree (**198–210**) | yes |
| Band hull (**1–210**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| none | the band is the whole file |

The tree is a binary search with no rebalance. DeleteAll walks post-order and pushes each node onto the spare list through its left link. GetNewNode pops that list. The in-order print uses six-digit glibc %g, including -0, nan, and inf.

| Managed (`VmEventTree1218Slice.cs`) | Native (`nodebuild_events.cpp`) |
|-------------------------------------|-------------------------------|
| Event tree | **1–210** |
| Band hull (exclusive slice) | **1–210** |

### Chunk **1218** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **210** |
| **Net headline** | **+210** vs post-**1217** |


**Post-review ratio:** 201/210 managed/native ≈ 96% (src file LOC at HEAD; see tally).

## Tests

- `VmEventTree1218SliceTests.Insert_PlacesEqualKeysRightAndPrintsTheSqrtDistance`
- `VmEventTree1218SliceTests.DeleteAll_RecyclesNodesInPostOrder`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1218 --to 1218` (0 hard / 0 soft).

## Seams

Equal distances go right. Signed zero compares equal, and a negative zero inserted after zero hangs on the right. NaN never compares equal, so FindEvent misses it. A fresh node from new is indeterminate in C++ and zero-filled here. The printer is glibc %g of sqrt(distance / 2^32), capped at 99 characters.

## Credit rationale

UNCredited nodebuild_events.cpp 1-210 (+210). The file ends at 210. A full tally line scan shows no overlap.
